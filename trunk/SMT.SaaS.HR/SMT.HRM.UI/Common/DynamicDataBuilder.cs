using System;
using System.Net;
using SMT.Saas.Tools.SalaryWS;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;
namespace SMT.HRM.UI
{
    public class DynamicDataBuilder
    {
        public static System.Type BuildDataObjectType(ObservableCollection<DataColumnInfo> Columns, string DataObjectName)
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("AprimoDynamicData"), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DataModule");

            TypeBuilder tb = moduleBuilder.DefineType(DataObjectName,
                                                    TypeAttributes.Public |
                                                    TypeAttributes.Class,
                                                    typeof(CustomerDataObject));

            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            foreach (var col in Columns)
            {
                string propertyName = col.ColumnName.Replace(' ', '_');
                Type dataType = System.Type.GetType(col.DataTypeName, false, true);
                if (dataType != null)
                {
                    FieldBuilder fb = tb.DefineField("_" + propertyName, dataType, FieldAttributes.Private);
                    PropertyBuilder pb = tb.DefineProperty(propertyName, System.Reflection.PropertyAttributes.HasDefault, dataType, null);
                    MethodBuilder getMethod = tb.DefineMethod("get_" + propertyName,
                                                                MethodAttributes.Public |
                                                                MethodAttributes.HideBySig |
                                                                MethodAttributes.SpecialName,
                                                                dataType,
                                                                Type.EmptyTypes);

                    ILGenerator ilgen = getMethod.GetILGenerator();
                    //Emit Get property, return _prop
                    ilgen.Emit(OpCodes.Ldarg_0);
                    ilgen.Emit(OpCodes.Ldfld, fb);
                    ilgen.Emit(OpCodes.Ret);
                    pb.SetGetMethod(getMethod);
                    MethodBuilder setMethod = tb.DefineMethod("set_" + propertyName,
                    MethodAttributes.Public |
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName,
                    null,
                    new Type[] { dataType });
                    ilgen = setMethod.GetILGenerator();
                    LocalBuilder localBuilder = ilgen.DeclareLocal(typeof(String[]));
                    //Emit set property, _Prop = value;
                    ilgen.Emit(OpCodes.Ldarg_0);
                    ilgen.Emit(OpCodes.Ldarg_1);
                    ilgen.Emit(OpCodes.Stfld, fb);

                    //Notify Change:
                    Type[] wlParams = new Type[] { typeof(string[]) };
                    MethodInfo notifyMI = typeof(CustomerDataObject).GetMethod("NotifyChange",
                    BindingFlags.NonPublic |
                    BindingFlags.Instance,
                    null,
                    CallingConventions.HasThis,
                    wlParams,
                    null);

                    //NotifyChange Property change
                    ilgen.Emit(OpCodes.Ldc_I4_1);
                    ilgen.Emit(OpCodes.Newarr, typeof(String));
                    ilgen.Emit(OpCodes.Stloc_0);
                    ilgen.Emit(OpCodes.Ldloc_0);
                    ilgen.Emit(OpCodes.Ldc_I4_0);
                    ilgen.Emit(OpCodes.Ldstr, propertyName);
                    ilgen.Emit(OpCodes.Stelem_Ref);
                    ilgen.Emit(OpCodes.Ldarg_0);
                    ilgen.Emit(OpCodes.Ldloc_0);
                    ilgen.EmitCall(OpCodes.Call, notifyMI, null); // call nodifyChange function

                    ilgen.Emit(OpCodes.Ret);
                    pb.SetSetMethod(setMethod);
                }
            }
            System.Type rowType = tb.CreateType();
            //assemblyBuilder.Save("DynamicData.dll");
            return rowType;
        }
        public static IEnumerable GetDataList(DataSetData data)
        {
            if (data == null)
                return null;
            if (data.Tables.Count == 0)
                return null;

            DataTableInfo tableInfo = data.Tables[0];

            System.Type dataType = BuildDataObjectType(tableInfo.Columns, "MyDataObject");

            //ObservableCollection<DataObject> l = new ObservableCollection<DataObject>();

            var listType = typeof(ObservableCollection<>).MakeGenericType(new[] { dataType });
            var list = Activator.CreateInstance(listType);

            XDocument xd = XDocument.Parse(data.DataXML);
            var table = from row in xd.Descendants(tableInfo.TableName)
                        select row.Elements().ToDictionary(r => r.Name, r => r.Value);

            foreach (var r in table)
            {
                var rowData = Activator.CreateInstance(dataType) as CustomerDataObject;
                if (rowData != null)
                {
                    foreach (DataColumnInfo col in tableInfo.Columns)
                    {
                        if (r.ContainsKey(col.ColumnName) && col.DataTypeName != typeof(System.Byte[]).FullName && col.DataTypeName != typeof(System.Guid).FullName)
                        {
                            if (col.IsEncrypt)
                            {
                                rowData.SetFieldValue(col.ColumnName, SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(r[col.ColumnName]), true);
                            }
                            else
                            {
                                rowData.SetFieldValue(col.ColumnName, r[col.ColumnName], true);
                            }
                        }
                    }
                }
                listType.GetMethod("Add").Invoke(list, new[] { rowData });
            }
            ObservableCollection<CustomerDataObject> l = list as ObservableCollection<CustomerDataObject>;
            return list as IEnumerable;
        }
    }
}
