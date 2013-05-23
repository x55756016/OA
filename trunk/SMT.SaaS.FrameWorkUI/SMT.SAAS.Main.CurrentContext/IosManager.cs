using System;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
namespace SMT.SAAS.Main.CurrentContext
{
    /// <summary>
    /// 独立存储区管理器
    /// 主要用于向独立存储区读写文件、Settings、序列化数据
    /// </summary>
    public class IosManager
    {
        /// <summary>
        /// 存储区集合访问对象
        /// </summary>
        public static IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;

        /// <summary>
        /// 检测存储区剩余空间
        /// </summary>
        /// <returns></returns>
        public static bool CheckeSpace()
        {
            long size = 5242880;//5MB
            return CheckeSpace(size);
        }

        /// <summary>
        /// 检测存储区剩余空间
        /// </summary>
        /// <param name="Size">对比大小</param>
        /// <returns>若小于Size返回False,否则返回True</returns>
        public static bool CheckeSpace(long Size)
        {
            bool isok = false;
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                long curAvail = store.AvailableFreeSpace;
                if (curAvail < Size)
                {
                    isok = false;
                }
                else
                {
                    isok = true;
                }
            }
            return isok;
        }

        /// <summary>
        /// 申请独立存储空间
        /// </summary>
        public static bool AddSpace()
        {
            bool isok = false;
            // Obtain an isolated store for an application.
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                   string[] sts= store.GetDirectoryNames();
                    // Request 100MB more space in bytes.
                    Int64 spaceToAdd = 104857600;
                    Int64 curAvail = store.AvailableFreeSpace;

                    // If available space is less than
                    // what is requested, try to increase.
                    if (store.Quota < spaceToAdd)
                    {

                        // Request more quota space.
                        if (!store.IncreaseQuotaTo(spaceToAdd))
                        {
                            // The user clicked NO to the
                            // host's prompt to approve the quota increase.
                        }
                        else
                        {
                            isok = true;
                            // The user clicked YES to the
                            // host's prompt to approve the quota increase.
                        }
                    }
                }
            }

            catch (IsolatedStorageException)
            {
                // TODO: Handle that store could not be accessed.

            }

            return isok;
        }

        /// <summary>
        /// 获取文件字节
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] GetFileBytes(string path)
        {
            byte[] buffer = null;
            try
            {
                using (IsolatedStorageFileStream stream = GetFileStream(path))
                {
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    //若加密可在此返回前对数据解密
                    //buffer = Security.Encryption.Decrypt(buffer,path);
                }
                return buffer;

            }
            catch (Exception ex)
            {
                throw new Exception("获取客户端版本文件异常!", ex);
            }
        }

        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件流</returns>
        public static IsolatedStorageFileStream GetFileStream(string path)
        {
            IsolatedStorageFileStream stream = null;
            try
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (file.FileExists(path))
                    {
                        stream = file.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }

                    return stream;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("获取本地版本文件发生异常!", ex);
            }
        }

        /// <summary>
        ///  判断文件是否存在
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否存在</returns>
        public static bool ExistsFile(string path)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //file.DeleteFile(path);
                return file.FileExists(path);
            }
        }


        /// <summary>
        ///  创建路径
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件路径</returns>
        public static string CreatePath(string path)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create three directories in the root.
                store.CreateDirectory(path);
            }
            return "";
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="DircetoryName">文件夹</param>
        /// <param name="path">文件路径</param>
        /// <param name="bytes">文件流</param>
        public static void CreateFile(string DircetoryName, string path, byte[] bytes)
        {
            try
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!file.DirectoryExists(DircetoryName))
                    {
                        file.CreateDirectory(DircetoryName);
                    }
                    path = string.Format(@"{0}\{1}", DircetoryName, path);
                    if (file.FileExists(path))
                    {
                        file.DeleteFile(path);
                    }
                    using (IsolatedStorageFileStream stream = file.CreateFile(path))
                    {
                        if (stream.CanWrite)
                        {
                            //可加密后再进行缓存
                            //byte[] secBytes= Security.Encryption.Encrypt(bytes, DircetoryName);
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("创建独立存储文件发生异常!", ex);
            }
        }


        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="DircetoryName">文件夹</param>
        /// <param name="path">文件路径</param>
        /// <param name="bytes">文件流</param>
        public static void DeletFile(string DircetoryName, string path)
        {
            try
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    path = string.Format(@"{0}\{1}", DircetoryName, path);
                    if (file.FileExists(path))
                    {
                        file.DeleteFile(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("独立存储删除文件发生异常!", ex);
            }
        }

        public static void WriteStream(Stream stream, System.IO.IsolatedStorage.IsolatedStorageFileStream fileStream)
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="DircetoryName">文件夹</param>
        /// <param name="path">文件路径</param>
        /// <param name="bytes">文件流</param>
        public static void CreateFile(string DircetoryName, string path, XElement xmlDoc)
        {
            try
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!file.DirectoryExists(DircetoryName))
                    {
                        file.CreateDirectory(DircetoryName);
                    }
                    path = string.Format(@"{0}\{1}", DircetoryName, path);
                    if (file.FileExists(path))
                    {
                        file.DeleteFile(path);
                    }
                    
                    using (IsolatedStorageFileStream stream = file.CreateFile(path))
                    {
                        xmlDoc.Save(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("创建独立存储文件发生异常!", ex);
            }
        }
        /// <summary>
        /// 读取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="Key">存储的键信息</param>
        /// <returns>返回获取的对象</returns>
        public static T GetValue<T>(string Key)
        {
            T var = default(T);
            if (AppSettings.Contains(Key))
            {
                var = (T)AppSettings[Key];
            }
            return var;
        }

        /// <summary>
        /// 存储对象
        /// </summary>
        /// <param name="Key">存储的Key</param>
        /// <param name="objects">存储的Value</param>
        public static void SetValue(string Key, object objects)
        {
            if (AppSettings.Contains(Key))
            {
                AppSettings[Key] = objects;
            }
            else
            {
                AppSettings.Add(Key, objects);
            }
        }

        /// <summary>
        /// 使用DataContractSerializer,根据给定的文件名称,将指定的数据序列化到独立存储
        /// </summary>
        /// <typeparam name="TArgs">数据类型</typeparam>
        /// <param name="fileName">文件名</param>
        /// <param name="type">数据</param>
        public static void SerializerDataContract<TArgs>(string fileName, TArgs type)
        {
            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(TArgs));
                    MemoryStream memoryStream = new MemoryStream();
                    using (FileStream stream = store.CreateFile(fileName))
                    {
                        serializer.WriteObject(memoryStream, type);
                        byte[] json = memoryStream.ToArray();
                        stream.Write(json, 0, json.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("SerializerDataContract:FAIL.", ex);
            }
        }

        /// <summary>
        /// 使用DataContractSerializer将WCF数据从独立存储反序列化
        /// </summary>
        /// <typeparam name="TArgs">数据类型</typeparam>
        /// <param name="fileName">存储文件名</param>
        /// <returns>结果</returns>
        public static TArgs DeserializeDataContract<TArgs>(string fileName)
        {
            try
            {
                TArgs result = default(TArgs);
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    IsolatedStorageFileStream stream = null;
                    if (file.FileExists(fileName))
                    {
                        stream = file.OpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        DataContractSerializer ser2 = new DataContractSerializer(typeof(TArgs));
                        result = (TArgs)ser2.ReadObject(stream);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("DeserializeDataContract:FAIL.", ex);
            }
        }
    }
}
