//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace SMT.FB.UI.Common
{

    public class XLSXReader
    {
        #region 变量

        FileInfo theFile;
        internal static XNamespace excelNamespace = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        internal static XNamespace excelRelationshipsNamepace = XNamespace.Get("http://schemas.openxmlformats.org/officeDocument/2006/relationships");

        const string sharedStringsMarker = @"xl/sharedStrings.xml";
        const string worksheetsMarker = @"/xl/worksheets/";
        const string worksheetMarker = "/xl/worksheets/sheet{0}.xml";
        const string workbooksMarker = @"xl/workbook.xml";

        private int worksheetsCount = 0;
        private XElement sharedStringsElement;

        private Dictionary<int, string> sharedStrings;

        #endregion

        public XLSXReader(FileInfo _file)
        {
            theFile = _file;
        }

        public string GetFilename()
        {
            return theFile.Name;
        }

        public int GetWorksheetIndex(string itemSelected)
        {
            if (string.IsNullOrEmpty(itemSelected) == true)
                return 0;

            List<string> worksheets = GetListSubItems();

            if (worksheets == null)
                return 0;

            int count = 0;
            foreach (string worksheetName in worksheets)
            {
                count += 1;
                if (worksheetName == itemSelected)
                {
                    return count;
                }
            }
            return 0;
        }

        /// <summary>
        /// 返回excel文件中指定Sheet的数据
        /// </summary>
        /// <param name="itemSelected">sheet的名称</param>
        /// <returns></returns>
        public IEnumerable<IDictionary> GetData(string itemSelected)
        {
            int worksheetIdex = GetWorksheetIndex(itemSelected);

            if (worksheetIdex <= 0)
                yield break;

            XElement wsSelectedElement = GetWorksheet(worksheetIdex);
            if (wsSelectedElement == null)
                yield break;

            IEnumerable<XElement> rowsExcel = from row in wsSelectedElement.Descendants(XLSXReader.excelNamespace + "row")
                                              select row;

            if (rowsExcel == null)
                yield break;

            foreach (XElement row in rowsExcel)
            {
                var dict = new Dictionary<string, object>();
                IEnumerable<XElement> cellsRow = row.Elements(XLSXReader.excelNamespace + "c");
                foreach (XElement cell in cellsRow)
                {
                    if (cell.HasElements == true)
                    {
                        string cellValue = cell.Element(XLSXReader.excelNamespace + "v").Value;
                        if (cell.Attribute("t") != null)
                        {
                            if (cell.Attribute("t").Value == "s")
                            {
                                cellValue = sharedStrings[Convert.ToInt32(cellValue)];
                            }
                        }

                        dict[cell.Attribute("r").Value.Substring(0, 1)] = cellValue as Object;
                    }
                }
                yield return dict;
            }
        }
        /// <summary>
        /// 将Excel指定sheet中的数据转换为实体集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sheetName">excel指定的sheet</param>
        /// <param name="nameCorrespend">实体属性名称与excel列名的对应关系,key=实体属性名称,valule=excel列名(excel第一行的数据)</param>
        /// <returns></returns>
        public List<T> GetData<T>(string sheetName, Dictionary<string, string> nameCorrespend) where T : new()
        {
            var data = GetData(sheetName);
            if (data == null)
            {
                return null;
            }
            if (data.Count() <= 1)
            {
                return new List<T>();
            }

            List<DictionaryEntry> columns = new List<DictionaryEntry>();
            IDictionary dic = data.First();
            foreach (DictionaryEntry item in dic)
            {
                columns.Add(item);
            }
            //建立Excel列与实体属性名称的对应关系
            Dictionary<string, string> columnCorrespend = new Dictionary<string, string>();
            foreach (var item in columns)
            {
                var tmp = nameCorrespend.FirstOrDefault(p => p.Value == item.Value.ToString());
                if (tmp.Value == null)
                {
                    continue;
                }
                //建立对应与sheet列的对应
                columnCorrespend[tmp.Key] = item.Key.ToString();
            }

            List<IDictionary> excelData = data.ToList();
            excelData.Remove(excelData.First());//排出第一行,(即列信息)
            //创建实体集合
            List<T> results = new List<T>();
            foreach (IDictionary item in excelData)
            {
                T t = new T();
                foreach (KeyValuePair<string, string> correspend in columnCorrespend)
                {
                    //PropertyInfo prop = propDic[correspend.Key];
                    t.SetObjValue(correspend.Key, item[correspend.Value]);
                }
                results.Add(t);
            }

            return results;
        }

        /// <summary>
        /// 将Excel指定sheet中的数据转换为实体集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sheetName">excel指定的sheet</param>
        /// <returns></returns>
        public List<T> GetData<T>(string sheetName) where T : new()
        {
            //得到实体列信息
            PropertyInfo[] properties = typeof(T).GetProperties();
            Dictionary<string, string> nameCorrespend = new Dictionary<string, string>();
            foreach (var item in properties)
            {
                nameCorrespend[item.Name] = item.Name;
            }

            return GetData<T>(sheetName, nameCorrespend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worksheetID"></param>
        /// <returns></returns>
        public XElement GetWorksheet(int worksheetID)
        {
            string worksheetMarker = String.Format(CultureInfo.CurrentCulture, "xl/worksheets/sheet{0}.xml", worksheetID);

            return GetXLSXPart(worksheetMarker);
        }

        public List<string> GetListSubItems()
        {

            XElement worksheetElement = GetXLSXPart(workbooksMarker);

            IEnumerable<XElement> workSheetItems = from s in worksheetElement.Descendants(XLSXReader.excelNamespace + "sheet")
                                                   select s;

            IEnumerable<XAttribute> workSheetAttrs = workSheetItems.Attributes("name");

            List<string> worksheets = new List<string>();

            foreach (XAttribute xattr in workSheetAttrs)
            {
                worksheets.Add(xattr.Value);
            }

            // a good place to get the sharedStrings included in the xlsx file
            ParseSharedStrings(GetSharedStringPart());

            return worksheets;

        }

        #region 私有方法

        private XElement GetSharedStringPart()
        {
            return GetXLSXPart(sharedStringsMarker);
        }

        /// <summary>
        /// Extracts from the xslx file the part specified with partMarker
        /// </summary>
        /// <param name="partMarker"></param>
        /// <returns></returns>
        private XElement GetXLSXPart(string partMarker)
        {
            UnZipper unzip;
            Stream s = theFile.OpenRead();
            unzip = new UnZipper(s);
            XElement partElement = null;

            foreach (string filename in unzip.GetFileNamesInZip())
            {
                Stream partStream = unzip.GetFileStream(filename);
                if (filename == partMarker)
                {
                    partElement = XElement.Load(XmlReader.Create(partStream));
                    partStream.Close();
                    return partElement;
                }
            }
            return null;
        }

        private void ParseSharedStrings(XElement SharedStringsElement)
        {
            sharedStrings = new Dictionary<int, string>();
            IEnumerable<XElement> sharedStringsElements = from s in SharedStringsElement.Descendants(excelNamespace + "t")
                                                          select s;
            int Counter = 0;
            foreach (XElement sharedString in sharedStringsElements)
            {
                sharedStrings.Add(Counter, sharedString.Value);
                Counter++;
            }
            return;
        }

        #endregion
    }

    /// <summary>
    /// 读取Excel文件
    /// </summary>
    internal class UnZipper
    {
        private Stream stream;

        public UnZipper(Stream zipFileStream)
        {
            this.stream = zipFileStream;
        }

        public Stream GetFileStream(string filename)
        {
            Uri fileUri = new Uri(filename, UriKind.Relative);
            StreamResourceInfo info = new StreamResourceInfo(this.stream, null);
            if (this.stream is System.IO.FileStream)
                this.stream.Seek(0, SeekOrigin.Begin);
            StreamResourceInfo stream = System.Windows.Application.GetResourceStream(info, fileUri);
            if (stream != null)
                return stream.Stream;
            return null;


        }

        public IEnumerable<string> GetFileNamesInZip()
        {
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            string name = null;
            List<string> names = new List<string>();
            while (ParseFileHeader(reader, out name))
            {
                names.Add(name);
            }
            return names;
        }


        private static bool ParseFileHeader(BinaryReader reader, out string filename)
        {
            filename = null;
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int headerSignature = reader.ReadInt32();
                if (headerSignature == 67324752) //ggggggrrrrrrrrrrrrrrrrr
                {
                    reader.BaseStream.Seek(2, SeekOrigin.Current);

                    short genPurposeFlag = reader.ReadInt16();
                    if (((((int)genPurposeFlag) & 0x08) != 0))
                        return false;
                    reader.BaseStream.Seek(10, SeekOrigin.Current);
                    //short method = reader.ReadInt16(); //Unused
                    //short lastModTime = reader.ReadInt16(); //Unused
                    //short lastModDate = reader.ReadInt16(); //Unused
                    //int crc32 = reader.ReadInt32(); //Unused
                    int compressedSize = reader.ReadInt32();
                    int unCompressedSize = reader.ReadInt32();
                    short fileNameLenght = reader.ReadInt16();
                    short extraFieldLenght = reader.ReadInt16();
                    filename = new string(reader.ReadChars(fileNameLenght));
                    if (string.IsNullOrEmpty(filename))
                        return false;

                    reader.BaseStream.Seek(extraFieldLenght + compressedSize, SeekOrigin.Current);
                    if (unCompressedSize == 0)
                        return ParseFileHeader(reader, out filename);
                    else
                        return true;
                }
            }
            return false;
        }
    }

    internal static class DataSourceCreator
    {
        private static readonly Regex PropertNameRegex =
                new Regex(@"^[A-Za-z]+[A-Za-z1-9_]*$", RegexOptions.Singleline);

        public static IEnumerable ToDataSource(this IEnumerable<IDictionary> list)
        {
            IDictionary firstDict = null;
            bool hasData = false;
            foreach (IDictionary currentDict in list)
            {
                hasData = true;
                firstDict = currentDict;
                break;
            }
            if (!hasData)
            {
                return new object[] { };
            }
            if (firstDict == null)
            {
                throw new ArgumentException("IDictionary entry cannot be null");
            }

            Type objectType = null;

            TypeBuilder tb = GetTypeBuilder(list.GetHashCode());

            ConstructorBuilder constructor =
                        tb.DefineDefaultConstructor(
                                    MethodAttributes.Public |
                                    MethodAttributes.SpecialName |
                                    MethodAttributes.RTSpecialName);

            foreach (DictionaryEntry pair in firstDict)
            {
                //if (PropertNameRegex.IsMatch(Convert.ToString(pair.Key), 0))
                // {
                CreateProperty(tb,
                                Convert.ToString(pair.Value),
                                pair.Value == null ?
                                            typeof(object) :
                                            pair.Value.GetType());
            }
            objectType = tb.CreateType();

            return GenerateEnumerable(objectType, list, firstDict);
        }

        private static IEnumerable GenerateEnumerable(
                 Type objectType, IEnumerable<IDictionary> list, IDictionary firstDict)
        {
            var listType = typeof(List<>).MakeGenericType(new[] { objectType });
            var listOfCustom = Activator.CreateInstance(listType);

            foreach (var currentDict in list)
            {
                if (currentDict == null)
                {
                    throw new ArgumentException("IDictionary entry cannot be null");
                }
                var row = Activator.CreateInstance(objectType);
                foreach (DictionaryEntry pair in firstDict)
                {
                    if (currentDict.Contains(pair.Key))
                    {
                        PropertyInfo property =
                            objectType.GetProperty(Convert.ToString(pair.Value));
                        property.SetValue(
                            row,
                            Convert.ChangeType(
                                    currentDict[pair.Key],
                                    property.PropertyType,
                                    null),
                            null);
                    }
                }
                listType.GetMethod("Add").Invoke(listOfCustom, new[] { row });
            }
            return listOfCustom as IEnumerable;
        }

        private static TypeBuilder GetTypeBuilder(int code)
        {
            AssemblyName an = new AssemblyName("TempAssembly" + code);
            AssemblyBuilder assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            TypeBuilder tb = moduleBuilder.DefineType("TempType" + code
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , typeof(object));
            return tb;
        }

        private static void CreateProperty(
                        TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName,
                                                        propertyType,
                                                        FieldAttributes.Private);


            PropertyBuilder propertyBuilder =
                tb.DefineProperty(
                    propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr =
                tb.DefineMethod("get_" + propertyName,
                    MethodAttributes.Public |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig,
                    propertyType, Type.EmptyTypes);

            ILGenerator getIL = getPropMthdBldr.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new Type[] { propertyType });

            ILGenerator setIL = setPropMthdBldr.GetILGenerator();

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }
}