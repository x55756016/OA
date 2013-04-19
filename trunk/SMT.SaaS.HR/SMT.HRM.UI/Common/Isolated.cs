using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SMT.HRM.UI
{
    /// <summary>
    /// IsolatedStorage辅助类
    /// </summary>
    public class Isolated
    {
        private static string PackageRelationshipType = @"http://schemas.microsoft.com/opc/2006/sample/document";
        public const string FileName = "HRMConfig.xml";
        public const string DirName = "HRM";
        public static void SaveIsolatedFile<T>(T obj)
        {
            SaveIsolatedFile<T>(obj, FileName);
        }
        /// <summary>
        /// 保存对像到IsolatedStorage
        /// </summary>
        /// <typeparam name="T">要保存对像的类型</typeparam>
        /// <param name="obj">要保存的对像</param>
        /// <param name="fileName">要保存到地文件名</param>
        public static void SaveIsolatedFile<T>(T obj, string fileName)
        {

            IsolatedStorageFileStream cfs = null;

            ////判断该目录是否已经存在,不存在就新建一个
            if (IsolatedStorageFile.GetUserStoreForApplication().DirectoryExists(DirName) == false)
            {
                IsolatedStorageFile.GetUserStoreForApplication().CreateDirectory(DirName);
            }

            //判断文件是否存在,不存在就新建一个
            if (IsolatedStorageFile.GetUserStoreForApplication().FileExists(FileName) == false)
            {
                cfs = IsolatedStorageFile.GetUserStoreForApplication().CreateFile(FileName);
                cfs.Close();
            }

            //向本地文件中写入XML信息
            cfs = new IsolatedStorageFileStream(FileName,
                        FileMode.Truncate,
                        IsolatedStorageFile.GetUserStoreForApplication());


            StreamWriter writer = new StreamWriter(cfs);

            var xs = new XmlSerializer(typeof(T));
            xs.Serialize(writer, obj);
            writer.Close();
  
            cfs.Close();
        }
        /// <summary>
        /// 从IsolatedStorageFile中加载对像
        /// </summary>
        /// <typeparam name="T">需要加载对像的类型</typeparam>
        /// <param name="fileName">要加载的文件名称</param>
        /// <returns>加载后的对像</returns>
        public static T LoadIsolatedFile<T>(string fileName) where T : new()
        {
            T obj;
            obj = new T();
            if (IsolatedStorageFile.GetUserStoreForApplication().DirectoryExists(DirName) == false)
            {
                return obj;
            }

            IsolatedStorageFileStream cfs = null;

            //从本地文件中读取登录信息
            cfs = new IsolatedStorageFileStream(FileName,
                FileMode.Open, IsolatedStorageFile.GetUserStoreForApplication());

            StreamReader reader = new StreamReader(cfs);
            XmlReader xmlReader = XmlReader.Create(reader);

            var xs = new XmlSerializer(typeof(T));
            try
            {
                obj = (T)xs.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                cfs.Close();
            }

            return obj;
        }

 
    }
}
