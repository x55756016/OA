using System;
using SMT.SAAS.Platform.DALFactory;
using SMT.SAAS.Platform.DAL.Interface;
using SMT.SAAS.Platform.Model;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 应用系统业务操作接口，提供了对数据库、第三方服务、文件系统的访问提供支持。
// 完成日期：2011-04-20 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.BLL
{
    /// <summary>
    /// 应用系统业务操作接口，提供了对数据库、第三方服务、文件系统的访问提供支持。
    /// </summary>
    public class ModuleBLL
    {
        private static readonly IModuleInfoDAL dal = DataAccess.CreateModuleInfo();

        /// <summary>
        /// 新增一个ModuleInfo到数据库中
        /// </summary>
        /// <param name="model">ModuleInfo数据</param>
        /// <returns>是否新增成功</returns>
        public bool Add(ModuleInfo model)
        {
            try
            {
                return dal.Add(model);
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }

        /// <summary>
        /// 新增一个ModuleInfo到数据库中,并保存其对应的XAP文件
        /// </summary>
        /// <param name="model">ModuleInfo数据</param>
        /// <returns>是否新增成功</returns>
        public bool AddModuleInfoByFile(Model.ModuleInfo model, string folderName, byte[] xapFileStream)
        {
            try
            {
                string filePath = UploadFile(folderName, model.FileName, xapFileStream, false);
                if (filePath.Length > 0)
                {
                    model.FilePath = filePath;
                    return this.Add(model);
                }
                return false;
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }

        /// <summary>
        /// 根据文件路径获取文件流
        /// </summary>
        /// <param name="xapFolder">所属文件夹</param>
        /// <param name="fileName">文件名称</param>
        /// <returns>文件流</returns>
        public byte[] GetFileStream(string folderName, string fileName)
        {
            try
            {
                if (Directory.Exists(folderName))
                {
                    string filePath = folderName + @"\" + fileName;
                    FileMode fileMode = FileMode.Open;
                    if (File.Exists(filePath))
                    {
                        using (FileStream fileStream = new FileStream(filePath, fileMode, FileAccess.Read, FileShare.ReadWrite))
                        {
                            byte[] fileBytes = new byte[fileStream.Length];
                            fileStream.Read(fileBytes, 0, fileBytes.Length);
                            fileStream.Close();
                            return fileBytes;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion
                return null;
            }
        }

        /// <summary>
        /// 写入一个文件
        /// </summary>
        /// <param name="xapFolder">所属文件夹</param>
        /// <param name="fileName">文件名称</param>
        /// <returns>文件流</returns>
        public string UploadFile(string folderName, string fileName, byte[] fileData, bool isAppend)
        {
            try
            {
                //文件上传所在目录,目录不存在则新建一个
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                string path = folderName + @"\" + fileName;
                if (File.Exists(path))
                    File.Delete(path);

                //文件读写模式，如果参数为True则表示续传，否则以附加模式写到现有文件中
                FileMode fileMode = isAppend ? FileMode.Append : FileMode.Create;
                using (System.IO.FileStream fs = new System.IO.FileStream(path, fileMode, System.IO.FileAccess.Write))
                {
                    fs.Write(fileData, 0, fileData.Length);
                    fs.Close();
                    fs.Dispose();
                }
                return path;
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion
                return string.Empty;
            }
        }

        /// <summary>
        /// 根据给定的子应用编号集合获取其详细信息，一个编号代办一个系统，
        /// 单个系统可以包含多个子系统 
        /// </summary>
        /// <param name="appCodes">系统编号</param>
        /// <returns>结果，详细信息</returns>
        public List<ModuleInfo> GetModuleByCodes(string[] appCodes)
        {
            try
            {
                return dal.GetModuleListByCodes(appCodes);
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion
                return null;
            }
        }

        /// <summary>
        /// 根据用户系统ID获取用户所拥有的系统模块目录信息。
        /// </summary>
        /// <param name="userSysID">用户系统ID</param>
        /// <returns>生成后的模块集合</returns>
        public List<ModuleInfo> GetModuleCatalogByUser(string userSysID)
        {
            try
            {
                return dal.GetModuleCatalogByUser(userSysID);
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion
                return null;
            }
        }


        public List<ModuleInfo> GetTaskConfigInfoByUser(string userSysID, string configFilePath)
        {
            try
            {
                return dal.GetTaskConfigInfoByUser(userSysID, configFilePath);
            }
            catch (Exception ex)
            {
                #region 将异常写入日志文件
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion
                return null;
            }
            
        }
    }
}
