using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT.HRM.BLL.Permission;
using TM_SaaS_OA_EFModel;
using System.IO;
using System.Web.Hosting;
using System.Configuration;

namespace SMT.SaaS.Permission.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FileUploadManager
    {
        private FileUploadManagementBll fileUploadBll ;
        #region ---数据库操作
        /// <summary>
        ///  获取附件数据库记录
        /// </summary>
        /// <param name="parentID">父ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<T_SYS_FILEUPLOAD> Get_ParentID(string parentID)
        {
            //using (fileUploadBll = new FileUploadManagementBll())
            //{
            //    if (fileUploadBll.Get_ParentID(parentID) != null)
            //        return fileUploadBll.Get_ParentID(parentID).ToList();
            //    else
            //        return null;
            //}
            fileUploadBll = new FileUploadManagementBll();

            if (fileUploadBll.Get_ParentID(parentID) != null)
            {
                List<T_SYS_FILEUPLOAD> fs=new List<T_SYS_FILEUPLOAD>();
                foreach (var q in fileUploadBll.Get_ParentID(parentID).ToList())
                {
                    string uploadFolder = GetUploadFolder();
                    string fileName= uploadFolder+"\\"+q.FILENAME;
                    string filePath = "http://" + System.Web.HttpContext.Current.Request.Url.Host + System.Web.HttpContext.Current.Request.ApplicationPath +"\\"+ fileName;
                    q.FILENAME = filePath;
                    fs.Add(q);
                }
                return fs;
            }

            else
                return null;
            
        }

        /// <summary>
        /// 添加单条数据库记录
        /// </summary>
        /// <param name="obj"></param>
        [OperationContract]
        public void Add(T_SYS_FILEUPLOAD obj)
        {
            //using (fileUploadBll = new FileUploadManagementBll())
            //{
            //    bool sucess = fileUploadBll.Add(obj);
            //    if (sucess == false)
            //        throw new Exception("添加数据失败");
            //}
            fileUploadBll = new FileUploadManagementBll();
            
                bool sucess = fileUploadBll.Add(obj);
                if (sucess == false)
                    throw new Exception("添加数据失败");
            
        }
        /// <summary>
        ///  删除单条数据库记录
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [OperationContract]
        public int Delete(T_SYS_FILEUPLOAD obj)
        {
            //using (fileUploadBll = new FileUploadManagementBll())
            //{
            //    return fileUploadBll.Del(obj.FILEUPLOADID);
            //}
            fileUploadBll = new FileUploadManagementBll();
            
                return fileUploadBll.Del(obj.FILEUPLOADID);
            
        }

        /// <summary>
        /// 删除单条数据库记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        public int Del(string id)
        {
            //using (fileUploadBll = new FileUploadManagementBll())
            //{
            //    return fileUploadBll.Del(id);
            //}
            fileUploadBll = new FileUploadManagementBll();
            
                return fileUploadBll.Del(id);
            
        }
        /// <summary>
        /// 删除多条数据库记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        public int DelTB(string[] ids)
        {
            //using (fileUploadBll = new FileUploadManagementBll())
            //{
            //    return fileUploadBll.DelTB(ids);
            //}
            fileUploadBll = new FileUploadManagementBll();
            
                return fileUploadBll.DelTB(ids);
            
        }
        /// <summary>
        /// 根据父id 删除 数据库附件记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        public int DelTB_ParentID(string[] parentIDs)
        {
            //using (fileUploadBll = new FileUploadManagementBll())
            //{
            //    return fileUploadBll.DelTB_ParentID(parentIDs);
            //}
            fileUploadBll = new FileUploadManagementBll();
            
                return fileUploadBll.DelTB_ParentID(parentIDs);
            
        }

        [OperationContract]
        public void Upd(T_SYS_FILEUPLOAD obj)
        {
            //using (fileUploadBll = new FileUploadManagementBll())
            //{
            //    fileUploadBll.Update(obj);
            //}
            fileUploadBll = new FileUploadManagementBll();
            
                fileUploadBll.Update(obj);
            
        }
        #endregion ---数据库操作

        //无用
        [OperationContract]
        public int FileUpLoad(string UploadFolder, byte[] FileByte)
        {
            String FileExtention = GetUploadFolder();
            string filePath = String.Format(@"D:\example{0}", FileExtention);//文件存放路径
            FileStream stream = new FileStream(filePath, FileMode.CreateNew);
            stream.Write(FileByte, 0, FileByte.Length);
            stream.Close();
            return FileByte.Length;
        }
        private static string _tempExtension = "_temp";

        #region --上传操作 IUploadService Members


        /// <summary>
        /// 上传附件 
        /// </summary>
        /// <param name="UploadFolder">config key</param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="dataLength"></param>
        /// <param name="parameters"></param>
        /// <param name="firstChunk"></param>
        /// <param name="lastChunk"></param>
        /// <returns>空字符表示上传不成功</returns>
        [OperationContract]
        public string AddFile(string fileName, byte[] data, int dataLength, string parameters, bool firstChunk, bool lastChunk)
        {
            try
            {
                string uploadFolder = GetUploadFolder();
                //创建路径
                int i = fileName.LastIndexOf('\\');
                string path = @HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + fileName.Substring(0, i);
                DirectoryInfo m_DirInfo = new DirectoryInfo(path);
                if (!m_DirInfo.Exists)
                    Directory.CreateDirectory(path);


                string fname = IsExists_FileName(fileName, uploadFolder);
                string tempFileName = IsExists_FileName_temp(fileName, uploadFolder);

                using (FileStream fs = File.Open(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + tempFileName, FileMode.Append))
                {
                    fs.Write(data, 0, dataLength);
                    fs.Close();
                }
                //Finish up if this is the last chunk of the file
                if (lastChunk)
                {
                    //Rename file to original file
                    File.Move(HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + tempFileName, HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + fname);
                    //Finish stuff....
                    FinishedFileUpload(fname, parameters);
                }
                return fname;
            }
            catch { return null; }
        }

        //取消和删除服务器上正在上传的文件
        [OperationContract]
        public bool CancelUpload(string fileName)
        {
            bool b = true;
            try
            {
                string uploadFolder = GetUploadFolder();
                string tempFileName = fileName + _tempExtension;

                if (File.Exists(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + tempFileName))
                {
                    File.Delete(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + tempFileName);
                }
                DeleteUploadedFile(fileName);
            }
            catch { b = false; }
            return b;
        }
        /// <summary>
        /// 删除上传的文件
        /// </summary>
        /// <param name="UploadFolder"></param>
        /// <param name="fileName"></param>
        [OperationContract]
        protected void DeleteUploadedFile(string fileName)
        {
            string uploadFolder = GetUploadFolder();
            if (File.Exists(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + fileName))
                File.Delete(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + fileName);
        }
        /// <summary>
        ///  删除上传的文件和数据库记录
        /// </summary>
        /// <param name="UploadFolder"></param>
        /// <param name="fileName"></param>
        /// <param name="ID"></param>
        [OperationContract]
        public void Del_FileAndID(string fileName, string ID)
        {
            using (fileUploadBll = new FileUploadManagementBll())
            {
                string uploadFolder = GetUploadFolder();
                if (File.Exists(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + fileName))
                    File.Delete(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + fileName);
                fileUploadBll.Del(ID);
            }
        }
        /// <summary>
        /// Do your own stuff here when the file is finished uploading
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="parameters"></param>
        [OperationContract]
        protected virtual void FinishedFileUpload(string fileName, string parameters)
        {  //Thread.Sleep(5000);
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] Download(string fileName)
        {
            string uploadFolder = GetUploadFolder();
            string filePath = "";
            //判断传入的参数是否是完整路径
            
            if (fileName.IndexOf("http://") > -1)
            {
                int k = fileName.IndexOf("UpLoadFiles");

                int start = k + 11;//11是UpLoadFiles的长度
                
                fileName = fileName.Substring(start);
                
            }
            //else
            //{
            filePath = @HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + fileName;
                //filePath = @HostingEnvironment.ApplicationPhysicalPath + "\\FileDownload.aspx?username=" + username + "&password=" + pwd + "&filename=" + uploadFolder + "\\" + fileName;
            //}
            if (File.Exists(filePath))
            {
                FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] by = new byte[Convert.ToInt32(fs.Length)];
                fs.Read(by, 0, by.Length);              
                fs.Close();               
                return by;
            }
            else
                return null;

        }


        [OperationContract]
        public byte[] Download_va(string fileName,string username,string pwd)
        {
            string uploadFolder = GetUploadFolder();
            string filePath = "";
            //判断传入的参数是否是完整路径

            if (fileName.IndexOf("http://") > -1)
            {
                int k = fileName.IndexOf("UpLoadFiles");

                int start = k + 11;//11是UpLoadFiles的长度

                fileName = fileName.Substring(start);

            }
            //else
            //{

            filePath = @HostingEnvironment.ApplicationPhysicalPath + "\\FileDownload.aspx?username=" + username + "&password=" + pwd + "&filename=" + uploadFolder + "\\" + fileName;
            
            //}
            if (File.Exists(filePath))
            {
                FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] by = new byte[Convert.ToInt32(fs.Length)];
                fs.Read(by, 0, by.Length);
                fs.Close();
                return by;
            }
            else
                return null;

        }
        
        /// <summary>
        /// 获取文件夹.默认为  “UpLoadFiles"
        /// </summary>
        /// <returns></returns>
        protected virtual string GetUploadFolder()
        {
            string folder = ConfigurationSettings.AppSettings["UploadFolder"];
            if (string.IsNullOrEmpty(folder))
                folder = "UpLoadFiles";
            return folder;
        }
        //判断重名_tempExtension
        private static string IsExists_FileName(string fname, string uploadFolder)
        {

            if (File.Exists(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + fname))
            {
                int n = 1;
                int i = fname.LastIndexOf('.');
                string pre = fname.Substring(0, i);
                string suff = fname.Substring(i);
                do
                {
                    if (File.Exists(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + pre + "_" + n.ToString() + suff))
                        n++;
                    else
                        break;
                } while (n < 1000);
                fname = pre + "_" + n.ToString() + suff;
            }
            return fname;
        }
        //判断重名 _tempExtension
        private static string IsExists_FileName_temp(string fname, string uploadFolder)
        {
            if (File.Exists(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + fname))
            {
                int n = 1;
                int i = fname.LastIndexOf('.');
                string pre = fname.Substring(0, i);
                string suff = fname.Substring(i);
                do
                {
                    if (File.Exists(@HostingEnvironment.ApplicationPhysicalPath + "\\" + uploadFolder + "\\" + pre + "_" + n.ToString() + suff))
                        n++;
                    else
                        break;
                } while (n < 1000);
                fname = pre + "_" + n.ToString() + suff;
            }
            return fname;
        }
        #endregion
    }
}
