using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Web.Hosting;
using System.ServiceModel.Activation;
using System.Diagnostics;
using System.Configuration;
using System.Threading;

using System.Web;
using TM_SaaS_OA_EFModel;
using SMT.SAAS.MP.DAL;
using System.ServiceModel;
using System.Security.Cryptography;

namespace SMT.FileUpLoad.Service
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults=true)]
    public class UploadService : IUploadService
    {
        FileDAL dal = new FileDAL();
       
        private string DownloadUrl = string.Concat(ConfigurationManager.AppSettings["DownLoadUrl"]);
        //获取用户身份，由用户名,密码组成
        private string StrAuthUser = string.Concat(ConfigurationManager.AppSettings["FileUploadUser"]);
        #region 获取公司对上传文件的设置信息
        public UserFile GetCompanyFileSetInfo(string companycode)
        {
            UserFile userfile = FileConfig.GetCompanyItem(companycode);
            return userfile;
        }
        /// <summary>
        /// 公司代码和公司名
        /// </summary>
        /// <param name="companycode"></param>
        /// <param name="CName"></param>
        /// <returns></returns>
        public UserFile GetCompanyFileByCompanyCodeAndName(string companycode,string CName)
        {
            UserFile userfile = FileConfig.GetCompanyItem(companycode,CName);
            return userfile;
        }
        #endregion
        #region 上传文件
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="strSystemCode">系统编号</param>
        /// <param name="strModelCode">模块编号</param>
        /// <param name="strFileName">文件名</param>
        /// <param name="strMd5Name">加密文件名</param>
        /// <param name="strID">存储ID(业务ID)</param>
        /// <param name="strGuid">用于真实存储ID(些ID动态产生，用于唯一性)</param>
        /// <param name="data">数据流</param>
        /// <param name="BytesUploaded">已上传量</param>
        /// <param name="dataLength">每次上传文件的大小</param>
        /// <param name="firstChunk">第一块</param>
        /// <param name="lastChunk">最后一块</param>
        /// <param name="strCreateUserID">创建用户ID</param>
        /// <param name="model">model类</param>
        /// <returns></returns>       
        public string SaveUpLoadFile(string strSystemCode, string strModelCode, string strFileName, string strMd5Name, string strID, string strGuid, byte[] data, int BytesUploaded, int dataLength, bool firstChunk, bool lastChunk, string strCreateUserID,UserFile model)
        {
          //  string strPath = string.Format(SavePath, strSystemCode, strModelCode);
            string NewPath = "";
            try
            {
                string strPath = string.Format(model.SavePath, model.CompanyCode, model.SystemCode, model.ModelCode);
                if (!System.IO.Directory.Exists(strPath))
                {
                    if (strPath.IndexOf("\\\\") == 0)
                    {
                        string FirstName = "";
                        string UserName = "";
                        string StrPwd = "";
                        string ComputerName = "";
                        if (StrAuthUser.IndexOf(",") > 0)
                        {
                            FirstName = StrAuthUser.Split(',')[0];
                            ComputerName = FirstName.Split('/')[0];
                            UserName = FirstName.Split('/')[1];
                            StrPwd = StrAuthUser.Split(',')[1];
                        }
                        //LogonImpersonate imper = new LogonImpersonate("user1", "123456");
                        LogonImpersonate imper = new LogonImpersonate(UserName, StrPwd);
                        //WNetHelper.WNetAddConnection(@"liujianxng/user1", "123456", @"//172.16.1.57/fileupload", "Z:");
                        //Directory.CreateDirectory(strPath);
                        string path = @"Z:\" + model.CompanyCode + @"\" + model.SystemCode + @"\" + model.ModelCode;
                        string OldPath = model.SavePath.Split('{')[0].TrimEnd('\\');
                        if (CreateDirectory(path, UserName, StrPwd, OldPath))
                        {
                            SMT.Foundation.Log.Tracer.Debug("创建了驱动器映射");
                        }
                        else
                        {
                            SMT.Foundation.Log.Tracer.Debug("有问题");
                        }
                        //Directory.CreateDirectory(@"Z:\" + model.CompanyCode + @"\" + model.SystemCode + @"\" + model.ModelCode);
                        //file1.SaveAs(@"Z:/newfolder/test.jpg");


                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(strPath);
                    }
                }
                NewPath = Path.Combine(strPath, strMd5Name); //还未上传完成的路径
                string OldFilePath = Path.Combine(strPath, strFileName); //原来上传的文件
                string PassWordKey = "";//加密字符串
                if (strMd5Name.Length > 8)
                {
                    PassWordKey = strMd5Name.Substring(0, 8);
                }
                else
                {
                    PassWordKey = "12345678";
                }
                if (File.Exists(NewPath))
                {//如果存在，追加内容（续传）
                    using (FileStream fs = File.Open(NewPath, FileMode.Append))
                    {
                        //DecryptFile(fs,strPath, "");
                        fs.Write(data, 0, dataLength);//datalength:文件已上传量
                        fs.Close();
                        fs.Dispose();
                    }
                    using (FileStream fsOld = File.Open(OldFilePath, FileMode.Append))
                    {
                        //DecryptFile(fs,strPath, "");
                        fsOld.Write(data, 0, dataLength);//datalength:文件已上传量
                        fsOld.Close();
                        fsOld.Dispose();
                    }
                }
                else
                {//如果不存在，就创建
                    using (FileStream fs = File.Create(NewPath))
                    {

                        fs.Write(data, 0, data.Length);
                        fs.Close();
                        fs.Dispose();

                    }
                    //创建上传过来的文件
                    using (FileStream fsOld = File.Create(OldFilePath))
                    {

                        fsOld.Write(data, 0, data.Length);
                        fsOld.Close();
                        fsOld.Dispose();

                    }
                }
                if (lastChunk)
                {
                    //string[] strExtension = strMd5Name.Split('.');
                    //string fileName = strID + "." + strExtension[strExtension.Length - 1];
                    string strTempName = Path.Combine(strPath, strMd5Name);//已经上传完成的路径

                    File.Move(NewPath, strTempName);
                    NewPath = strTempName;//最近返回已完成的路径      

                    T_SYS_FILELIST entity = new T_SYS_FILELIST();
                    entity.FILENAME = model.FileName;
                    entity.FILESIZE = Convert.ToInt32(model.FileSize);
                    entity.REMARK = model.Remark;
                    entity.SMTFILELISTID = model.SmtFileListId;// uc.SmtFileListId;//主键ID              
                    entity.FILEURL = model.FileUrl;//文件地址
                    entity.COMPANYCODE = model.CompanyCode;//公司代号
                    entity.COMPANYNAME = model.CompanyName;//公司名字
                    entity.PASSWORD = PassWordKey;//加密字符串
                    //entity.COMPANYNAOWNERDEPARTMENTIDME = model.CompanyName;//公司名字
                    entity.SYSTEMCODE = model.SystemCode;//系统代号
                    entity.MODELCODE = model.ModelCode;//模块代号
                    entity.APPLICATIONID = model.ApplicationID;//业务ID
                    entity.THUMBNAILURL = model.ThumbnailUrl;//缩略图地址
                    entity.FILETYPE = model.FileType;
                    entity.INDEXL =  Convert.ToInt32(model.INDEXL);//排序
                    entity.REMARK = model.Remark;//备注
                    entity.CREATETIME = model.CreateTime;//创建时间
                    entity.CREATENAME = model.CreateName;//创建人
                    entity.UPDATETIME = model.UpdateTime;//修改时间
                    entity.UPDATENAME = model.UpdateName;//修改人
                    entity.OWNERCOMPANYID = model.OWNERCOMPANYID;
                    entity.OWNERDEPARTMENTID = model.OWNERDEPARTMENTID;
                    entity.OWNERPOSTID = model.OWNERPOSTID;
                    
                    entity.OWNERID = model.OWNERID;
                    entity.FILEURL = string.Format("\\{0}\\{1}\\{2}", model.CompanyCode, model.SystemCode, model.ModelCode + "\\" + strMd5Name);
                    
                    string imageType = ".JPEG,.JPG,.GIF,.BMP";
                    //判断上传的模块的类型，如果是新闻则生成缩略图

                    if (imageType.IndexOf(entity.FILETYPE) > 0 || imageType.ToLower().IndexOf(entity.FILETYPE) > 0)
                    {
                            
                        string thumbPath = string.Format(model.SavePath, model.CompanyCode, model.SystemCode, model.ModelCode)+"\\thumb\\";
                        if (!System.IO.Directory.Exists(thumbPath))
                        {
                            if (strPath.IndexOf("\\\\") == 0)
                            {
                                string FirstName = "";
                                string UserName = "";
                                string StrPwd = "";
                                string ComputerName = "";
                                if (StrAuthUser.IndexOf(",") > 0)
                                {
                                    FirstName = StrAuthUser.Split(',')[0];
                                    ComputerName = FirstName.Split('/')[0];
                                    UserName = FirstName.Split('/')[1];
                                    StrPwd = StrAuthUser.Split(',')[1];
                                }
                                //LogonImpersonate imper = new LogonImpersonate("user1", "123456");
                                LogonImpersonate imper = new LogonImpersonate(UserName, StrPwd);
                                //WNetHelper.WNetAddConnection(@"liujianxng/user1", "123456", @"//172.16.1.57/fileupload", "Z:");
                                //Directory.CreateDirectory(strPath);
                                string path = @"Z:\" + model.CompanyCode + @"\" + model.SystemCode + @"\" + model.ModelCode;
                                string OldPath = model.SavePath.Split('{')[0].TrimEnd('\\');
                                if (CreateDirectory(path, UserName, StrPwd, OldPath))
                                {
                                    SMT.Foundation.Log.Tracer.Debug("创建了驱动器映射");
                                }
                                else
                                {
                                    SMT.Foundation.Log.Tracer.Debug("有问题");
                                }
                                //Directory.CreateDirectory(@"Z:\" + model.CompanyCode + @"\" + model.SystemCode + @"\" + model.ModelCode);
                                //file1.SaveAs(@"Z:/newfolder/test.jpg");


                            }
                            else
                            {
                                System.IO.Directory.CreateDirectory(thumbPath);
                            }
                        }
                        string thumbFile = thumbPath + strMd5Name;
                        string strType = "JPG"; 
                        strType = entity.FILETYPE.Substring(1, entity.FILETYPE.Length-1).ToUpper();
                        MakeImageThumb.MakeThumbnail(OldFilePath, thumbFile, 250, 200, "DB", strType);
                        //保存到数据库的路径
                        string strSaveThumb = string.Format("\\{0}\\{1}\\{2}", model.CompanyCode, model.SystemCode, model.ModelCode + "\\thumb\\" + strMd5Name);
                        //entity.THUMBNAILURL = string.Format("\\{0}\\{1}\\{2}", model.CompanyCode, model.SystemCode, model.ModelCode + "\\" + strMd5Name);
                        entity.THUMBNAILURL = strSaveThumb;
                    }
                    

                    dal.AddEntity(entity);
                    //DecryptFile(NewPath);
                    //Encrypt(NewPath, PassWordKey);
                    //FileStream filOutStream = new FileStream(NewfilePath, FileMode.OpenOrCreate, FileAccess.Write);
                    //InputFileStream = File.OpenWrite(FileName);
                    CryptoHelp.EncryptFile(OldFilePath, NewPath, PassWordKey);
                    //Decrypt(NewPath);
                    //model.FileUrl = string.Format("\\{0}\\{1}\\{2}", model.CompanyCode, model.SystemCode, model.ModelCode +"\\"+ fileName);
                    //dal.Add(model);

                }
                
            }
            catch (Exception ex)
            {                
                SMT.Foundation.Log.Tracer.Debug("model.SavePath:" +model.SavePath);
                SMT.Foundation.Log.Tracer.Debug("model.CompanyCode:" +model.CompanyCode);
                SMT.Foundation.Log.Tracer.Debug("model.SystemCode:" +model.SystemCode);
                SMT.Foundation.Log.Tracer.Debug("model.ModelCode:" +model.ModelCode);
                SMT.Foundation.Log.Tracer.Debug("调用上传文件出错："+ex.ToString());
            }
            return NewPath;
        }

        public string SaveUploadFileForMvc(string strSystemCode, string strModelCode, string strFileName, string strMd5Name, string strID, string strGuid, string strCreateUserID, UserFile model)
        {
            string NewPath = "";
            try
            {
                SMT.Foundation.Log.Tracer.Debug("mvc2.0开始添加上传文件的记录");
                string strPath = string.Format(model.SavePath, model.CompanyCode, model.SystemCode, model.ModelCode);

                NewPath = Path.Combine(strPath, strMd5Name); //还未上传完成的路径
                string OldFilePath = Path.Combine(strPath, strFileName); //原来上传的文件
                string PassWordKey = "";//加密字符串
                if (strMd5Name.Length > 8)
                {
                    PassWordKey = strMd5Name.Substring(0, 8);
                }
                else
                {
                    PassWordKey = "12345678";
                }
                string strTempName = Path.Combine(strPath, strMd5Name);//已经上传完成的路径
                SMT.Foundation.Log.Tracer.Debug("开始添加上传文件的记录strTempName：" + strTempName);
                
                NewPath = strTempName;//最近返回已完成的路径
                T_SYS_FILELIST entity = new T_SYS_FILELIST();
                entity.FILENAME = model.FileName;
                entity.FILESIZE = Convert.ToInt32(model.FileSize);
                entity.REMARK = model.Remark;
                entity.SMTFILELISTID = model.SmtFileListId;// uc.SmtFileListId;//主键ID              
                entity.FILEURL = model.FileUrl;//文件地址
                entity.COMPANYCODE = model.CompanyCode;//公司代号
                entity.COMPANYNAME = model.CompanyName;//公司名字
                entity.PASSWORD = PassWordKey;//加密字符串
                //entity.COMPANYNAOWNERDEPARTMENTIDME = model.CompanyName;//公司名字
                entity.SYSTEMCODE = model.SystemCode;//系统代号
                entity.MODELCODE = model.ModelCode;//模块代号
                entity.APPLICATIONID = model.ApplicationID;//业务ID
                entity.THUMBNAILURL = model.ThumbnailUrl;//缩略图地址
                entity.FILETYPE = model.FileType;
                entity.INDEXL =  Convert.ToInt32(model.INDEXL);//排序
                entity.REMARK = model.Remark;//备注
                entity.CREATETIME = model.CreateTime;//创建时间
                entity.CREATENAME = model.CreateName;//创建人
                entity.UPDATETIME = model.UpdateTime;//修改时间
                entity.UPDATENAME = model.UpdateName;//修改人
                entity.OWNERCOMPANYID = model.OWNERCOMPANYID;
                entity.OWNERDEPARTMENTID = model.OWNERDEPARTMENTID;
                entity.OWNERPOSTID = model.OWNERPOSTID;

                entity.OWNERID = model.OWNERID;
                entity.FILEURL = string.Format("\\{0}\\{1}\\{2}", model.CompanyCode, model.SystemCode, model.ModelCode + "\\" + strMd5Name);

                string imageType = ".JPEG,.JPG,.GIF,.BMP";
                //判断上传的模块的类型，如果是新闻则生成缩略图
                if (imageType.IndexOf(entity.FILETYPE) > 0 || imageType.ToLower().IndexOf(entity.FILETYPE) > 0)
                {
                    string thumbPath = string.Format(model.SavePath, model.CompanyCode, model.SystemCode, model.ModelCode) + "\\thumb\\";
                    string thumbFile = thumbPath + strMd5Name;
                    string strType = "JPG";
                    strType = entity.FILETYPE.Substring(1, entity.FILETYPE.Length - 1).ToUpper();
                    //MakeImageThumb.MakeThumbnail(OldFilePath, thumbFile, 250, 200, "DB", strType);
                    //保存到数据库的路径
                    string strSaveThumb = string.Format("\\{0}\\{1}\\{2}", model.CompanyCode, model.SystemCode, model.ModelCode + "\\thumb\\" + strMd5Name);
                    //entity.THUMBNAILURL = string.Format("\\{0}\\{1}\\{2}", model.CompanyCode, model.SystemCode, model.ModelCode + "\\" + strMd5Name);
                    entity.THUMBNAILURL = strSaveThumb;
                }
                int intResult = dal.AddEntity(entity);
                if (intResult > 0)
                {
                    SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc添加成功:" + entity.APPLICATIONID);
                    SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc添加FileUrl成功:" + entity.FILEURL);
                    SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc添加FileName成功:" + entity.FILENAME);
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc添加失败:" + entity.APPLICATIONID);
                    SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc添加FileUrl失败:" + entity.FILEURL);
                    SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc添加FileName失败:" + entity.FILENAME);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc操作model.SavePath:" + model.SavePath);
                SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc操作model.CompanyCode:" + model.CompanyCode);
                SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc操作.SystemCode:" + model.SystemCode);
                SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc操作model.ModelCode:" + model.ModelCode);
                SMT.Foundation.Log.Tracer.Debug("SaveUploadFileForMvc操作调用上传文件出错：" + ex.ToString());
                NewPath = "";
            }
            return NewPath;
        }
        #endregion

        #region 如果不是同一表单，则添加一条记录
        public string SaveUpLoadFileIsExist(string strSystemCode, string strModelCode, string strFileName, string strMd5Name, string strID, string strGuid, byte[] data, int BytesUploaded, int dataLength, bool firstChunk, bool lastChunk, string strCreateUserID,UserFile model)
        {
          
            string NewPath = "";
            try
            {
                string strPath = string.Format(model.SavePath, model.CompanyCode, model.SystemCode, model.ModelCode);
                                
                string PassWordKey = "";//加密字符串
                if (strMd5Name.Length > 8)
                {
                    PassWordKey = strMd5Name.Substring(0, 8);
                }
                else
                {
                    PassWordKey = "12345678";
                }
                
                
                    //string[] strExtension = strMd5Name.Split('.');
                    //string fileName = strID + "." + strExtension[strExtension.Length - 1];
                string strTempName = Path.Combine(strPath, strMd5Name);//已经上传完成的路径

                //File.Move(NewPath, strTempName);
                NewPath = strTempName;//最近返回已完成的路径      

                T_SYS_FILELIST entity = new T_SYS_FILELIST();
                entity.FILENAME = model.FileName;
                entity.FILESIZE = Convert.ToInt32(model.FileSize);
                entity.REMARK = model.Remark;
                entity.SMTFILELISTID = model.SmtFileListId;// uc.SmtFileListId;//主键ID              
                entity.FILEURL = strTempName;//model.FileUrl;//文件地址
                entity.COMPANYCODE = model.CompanyCode;//公司代号
                entity.COMPANYNAME = model.CompanyName;//公司名字
                entity.PASSWORD = PassWordKey;//加密字符串
                //entity.COMPANYNAOWNERDEPARTMENTIDME = model.CompanyName;//公司名字
                entity.SYSTEMCODE = model.SystemCode;//系统代号
                entity.MODELCODE = model.ModelCode;//模块代号
                entity.APPLICATIONID = model.ApplicationID;//业务ID
                entity.THUMBNAILURL = model.ThumbnailUrl;//缩略图地址
                entity.FILETYPE = model.FileType;
                entity.INDEXL =  Convert.ToInt32(model.INDEXL);//排序
                entity.REMARK = model.Remark;//备注
                entity.CREATETIME = model.CreateTime;//创建时间
                entity.CREATENAME = model.CreateName;//创建人
                entity.UPDATETIME = model.UpdateTime;//修改时间
                entity.UPDATENAME = model.UpdateName;//修改人
                entity.OWNERCOMPANYID = model.OWNERCOMPANYID;
                entity.OWNERDEPARTMENTID = model.OWNERDEPARTMENTID;
                entity.OWNERPOSTID = model.OWNERPOSTID;
                entity.OWNERID = model.OWNERID;
                entity.FILEURL = string.Format("\\{0}\\{1}\\{2}", model.CompanyCode, model.SystemCode, model.ModelCode + "\\" + strMd5Name);
                if (this.GetFileListByApplicationIDAndFileName(model.ApplicationID,strMd5Name).FileList != null)
                {
                    if (this.GetFileListByApplicationIDAndFileName(model.ApplicationID,strMd5Name).FileList.Count() > 0)
                    {
                        NewPath = "EXIST";
                    }
                    else
                    {
                        dal.AddEntity(entity);
                    }
                }
                else
                {
                    dal.AddEntity(entity);
                }
                    //DecryptFile(NewPath);
                    
                
                
            }
            catch (Exception ex)
            {                
                SMT.Foundation.Log.Tracer.Debug("model.SavePath:" +model.SavePath);
                SMT.Foundation.Log.Tracer.Debug("model.CompanyCode:" +model.CompanyCode);
                SMT.Foundation.Log.Tracer.Debug("model.SystemCode:" +model.SystemCode);
                SMT.Foundation.Log.Tracer.Debug("model.ModelCode:" +model.ModelCode);
                SMT.Foundation.Log.Tracer.Debug("调用上传文件出错："+ex.ToString());
            }
            return NewPath;
        }
        #endregion
      
        #region 取消上传
        public void CancelUpload(string fileName)
        {
            //string uploadFolder = GetUploadFolder();
            //string tempFileName = fileName + _tempExtension;

            //if (File.Exists(@HostingEnvironment.ApplicationPhysicalPath + "/" + uploadFolder + "/" + tempFileName))
            //    File.Delete(@HostingEnvironment.ApplicationPhysicalPath + "/" + uploadFolder + "/" + tempFileName);

        }
        #endregion 
        #region 检查文件是否已存在
        public UserFile CheckFileExists(UserFile model, string strMd5Name)
        {           
            string strPath = string.Format(model.SavePath, model.CompanyCode, model.SystemCode, model.ModelCode);
            string NewPath = Path.Combine(strPath, strMd5Name); //还未上传完成的路径
            if (File.Exists(NewPath))
            {
                FileInfo fi = new FileInfo(NewPath);
                model.BytesUploaded = fi.Length;
                model.FileUrl = NewPath;
                
                //return fi.Length;
            }
            return model;
        }
        #endregion
        private void SaveFile(Stream stream, FileStream fs)
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                fs.Write(buffer, 0, bytesRead);
            }
        }



        #region 返回文件列表 成员

        /// <summary>
        /// 返回文件列表
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <returns></returns>
        public CallBackResult GetFileListByCompanyCode(string companycode)
        {
           return dal.GetFileListByCompanyCode(companycode);
            #region ado代码
            //string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            //List<T_SYS_FILELIST> li = new List<T_SYS_FILELIST>();
            //string sql = "select * from T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "'  order by INDEXL, CREATETIME desc ";
            //System.Data.DataTable dt = SMT.MSOracle.GetDataTable(sql);
            //int n = dt.Rows.Count;
            //for (int i = 0; i < n; i++)
            //{
            //    T_SYS_FILELIST file = new T_SYS_FILELIST();
            //    string path = dt.Rows[i]["FILEURL"].ToString();
            //    string filename = path.Substring(path.LastIndexOf('\\') + 1);
            //    //  string filepath = HttpUtility.UrlEncode(dt.Rows[i]["COMPANYCODE"].ToString() + "|" + dt.Rows[i]["SYSTEMCODE"].ToString() + "|" + dt.Rows[i]["MODELCODE"].ToString() + "|" + filename + "|" + dt.Rows[i]["FILENAME"].ToString());
            //    string filepath = HttpUtility.UrlEncode(dt.Rows[i]["FILEURL"].ToString() + "\\" + dt.Rows[i]["FILENAME"].ToString());
            //    file.SMTFILELISTID = dt.Rows[i]["SMTFILELISTID"].ToString();// uc.SmtFileListId;//主键ID
            //    file.FILENAME = dt.Rows[i]["FILENAME"].ToString();//文件名(用户自定义)         
            //    file.FILEURL = DownloadUrl + "?filename=" + filepath;//文件地址
            //    file.COMPANYCODE = dt.Rows[i]["COMPANYCODE"].ToString();//公司代号
            //    file.COMPANYNAME = dt.Rows[i]["COMPANYNAME"].ToString();//公司名字
            //    file.SYSTEMCODE = dt.Rows[i]["SYSTEMCODE"].ToString();//系统代号
            //    file.MODELCODE = dt.Rows[i]["MODELCODE"].ToString();//模块代号
            //    file.APPLICATIONID = dt.Rows[i]["APPLICATIONID"].ToString();//业务ID
            //    file.THUMBNAILURL = dt.Rows[i]["THUMBNAILURL"].ToString();//缩略图地址
            //    file.INDEXL = dt.Rows[i]["INDEXL"].ToString() == "" ? 0 : Convert.ToDecimal(dt.Rows[i]["INDEXL"]);//排序
            //    file.REMARK = dt.Rows[i]["REMARK"].ToString();//备注
            //    file.CREATETIME = Convert.ToDateTime(dt.Rows[i]["CREATETIME"]);//创建时间
            //    file.CREATENAME = dt.Rows[i]["CREATENAME"].ToString();//创建人
            //    file.UPDATETIME = Convert.ToDateTime(dt.Rows[i]["UPDATETIME"]);//修改时间
            //    file.UPDATENAME = dt.Rows[i]["UPDATENAME"].ToString();//修改人
            //    file.FILESIZE = dt.Rows[i]["FILESIZE"].ToString() == "" ? 0 : Convert.ToDecimal(dt.Rows[i]["FILESIZE"]);//修改人
            //    li.Add(file);
            //}
            //return li;
            #endregion
        }
        /// <summary>
        /// 返回文件列表
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <param name="systemcode">系统代号</param>
        /// <returns></returns>
        public CallBackResult GetFileListBySystemCode(string companycode, string systemcode)
        {
            return dal.GetFileListBySystemCode( companycode,systemcode);
            #region 代码
            //string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            //List<T_SYS_FILELIST> li = new List<T_SYS_FILELIST>();
            //string sql = "select * from T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' and SYSTEMCODE='" + systemcode + "'  order by INDEXL, CREATETIME desc ";
            //System.Data.DataTable dt = SMT.MSOracle.GetDataTable(sql);
            //int n = dt.Rows.Count;
            //for (int i = 0; i < n; i++)
            //{
            //    T_SYS_FILELIST file = new T_SYS_FILELIST();
            //    string path = dt.Rows[i]["FILEURL"].ToString();
            //    string filename = path.Substring(path.LastIndexOf('\\') + 1);
            //    //  string filepath = HttpUtility.UrlEncode(dt.Rows[i]["COMPANYCODE"].ToString() + "|" + dt.Rows[i]["SYSTEMCODE"].ToString() + "|" + dt.Rows[i]["MODELCODE"].ToString() + "|" + filename + "|" + dt.Rows[i]["FILENAME"].ToString());
            //    string filepath = HttpUtility.UrlEncode(dt.Rows[i]["FILEURL"].ToString() + "\\" + dt.Rows[i]["FILENAME"].ToString());
            //    file.SMTFILELISTID = dt.Rows[i]["SMTFILELISTID"].ToString();// uc.SmtFileListId;//主键ID
            //    file.FILENAME = dt.Rows[i]["FILENAME"].ToString();//文件名(用户自定义)         
            //    file.FILEURL = DownloadUrl + "?filename=" + filepath;//文件地址
            //    file.COMPANYCODE = dt.Rows[i]["COMPANYCODE"].ToString();//公司代号
            //    file.COMPANYNAME = dt.Rows[i]["COMPANYNAME"].ToString();//公司名字
            //    file.SYSTEMCODE = dt.Rows[i]["SYSTEMCODE"].ToString();//系统代号
            //    file.MODELCODE = dt.Rows[i]["MODELCODE"].ToString();//模块代号
            //    file.APPLICATIONID = dt.Rows[i]["APPLICATIONID"].ToString();//业务ID
            //    file.THUMBNAILURL = dt.Rows[i]["THUMBNAILURL"].ToString();//缩略图地址
            //    file.INDEXL = dt.Rows[i]["INDEXL"].ToString() == "" ? 0 : Convert.ToDecimal(dt.Rows[i]["INDEXL"]);//排序
            //    file.REMARK = dt.Rows[i]["REMARK"].ToString();//备注
            //    file.CREATETIME = Convert.ToDateTime(dt.Rows[i]["CREATETIME"]);//创建时间
            //    file.CREATENAME = dt.Rows[i]["CREATENAME"].ToString();//创建人
            //    file.UPDATETIME = Convert.ToDateTime(dt.Rows[i]["UPDATETIME"]);//修改时间
            //    file.UPDATENAME = dt.Rows[i]["UPDATENAME"].ToString();//修改人
            //    file.FILESIZE = dt.Rows[i]["FILESIZE"].ToString() == "" ? 0 : Convert.ToDecimal(dt.Rows[i]["FILESIZE"]);//修改人
            //    li.Add(file);
            //}
            //return li;
            #endregion
        }
        /// <summary>
        /// 返回文件列表
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <param name="systemcode">系统代号</param>
        /// <param name="modelcode">模块代号</param>
        /// <returns></returns>
        public CallBackResult GetFileListByModelCode(string companycode, string systemcode, string modelcode)
        {
            return dal.GetFileListByModelCode(companycode,systemcode,modelcode);
            #region ado代码
            ////string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            //List<T_SYS_FILELIST> li = new List<T_SYS_FILELIST>();
            //string sql = "select * from T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' and SYSTEMCODE='" + systemcode + "' and MODELCODE='" + modelcode + "' order by INDEXL, CREATETIME desc ";
            //System.Data.DataTable dt = SMT.MSOracle.GetDataTable(sql);
            //int n = dt.Rows.Count;
            //for (int i = 0; i < n; i++)
            //{
            //    T_SYS_FILELIST file = new T_SYS_FILELIST();
            //    string path = dt.Rows[i]["FILEURL"].ToString();
            //    string filename = path.Substring(path.LastIndexOf('\\') + 1);
            //    //  string filepath = HttpUtility.UrlEncode(dt.Rows[i]["COMPANYCODE"].ToString() + "|" + dt.Rows[i]["SYSTEMCODE"].ToString() + "|" + dt.Rows[i]["MODELCODE"].ToString() + "|" + filename + "|" + dt.Rows[i]["FILENAME"].ToString());
            //    string filepath = HttpUtility.UrlEncode(dt.Rows[i]["FILEURL"].ToString() + "\\" + dt.Rows[i]["FILENAME"].ToString());
            //    file.SMTFILELISTID = dt.Rows[i]["SMTFILELISTID"].ToString();// uc.SmtFileListId;//主键ID
            //    file.FILENAME = dt.Rows[i]["FILENAME"].ToString();//文件名(用户自定义)         
            //    file.FILEURL = DownloadUrl + "?filename=" + filepath;//文件地址
            //    file.COMPANYCODE = dt.Rows[i]["COMPANYCODE"].ToString();//公司代号
            //    file.COMPANYNAME = dt.Rows[i]["COMPANYNAME"].ToString();//公司名字
            //    file.SYSTEMCODE = dt.Rows[i]["SYSTEMCODE"].ToString();//系统代号
            //    file.MODELCODE = dt.Rows[i]["MODELCODE"].ToString();//模块代号
            //    file.APPLICATIONID = dt.Rows[i]["APPLICATIONID"].ToString();//业务ID
            //    file.THUMBNAILURL = dt.Rows[i]["THUMBNAILURL"].ToString();//缩略图地址
            //    file.INDEXL = dt.Rows[i]["INDEXL"].ToString() == "" ? 0 : Convert.ToDecimal(dt.Rows[i]["INDEXL"]);//排序
            //    file.REMARK = dt.Rows[i]["REMARK"].ToString();//备注
            //    file.CREATETIME = Convert.ToDateTime(dt.Rows[i]["CREATETIME"]);//创建时间
            //    file.CREATENAME = dt.Rows[i]["CREATENAME"].ToString();//创建人
            //    file.UPDATETIME = Convert.ToDateTime(dt.Rows[i]["UPDATETIME"]);//修改时间
            //    file.UPDATENAME = dt.Rows[i]["UPDATENAME"].ToString();//修改人
            //    file.FILESIZE = dt.Rows[i]["FILESIZE"].ToString() == "" ? 0 : Convert.ToDecimal(dt.Rows[i]["FILESIZE"]);//修改人
            //    li.Add(file);
            //}
            //return li;
            #endregion
        }
        /// <summary>
        /// 返回文件列表
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <param name="systemcode">系统代号</param>
        /// <param name="modelcode">模块代号</param>
        /// <param name="applicationid">业务ID</param>
        /// <param name="createname">创建人</param>      
        /// <returns></returns>
        public CallBackResult GetFileListByApplicationID(string companycode, string systemcode, string modelcode, string applicationid,string createname)
        {
            return dal.GetFileListByApplicationID(companycode, systemcode, modelcode, applicationid, createname);
            #region 代码 
            //string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            //List<T_SYS_FILELIST> li = new List<T_SYS_FILELIST>();
            //string sql = "select * from T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' and SYSTEMCODE='" + systemcode + "' and MODELCODE='" + modelcode + "' and APPLICATIONID='" + applicationid + "'  order by INDEXL, CREATETIME desc ";
            //System.Data.DataTable dt = SMT.MSOracle.GetDataTable(sql);
            //int n = dt.Rows.Count;
            //for (int i = 0; i < n; i++)
            //{
            //    T_SYS_FILELIST file = new T_SYS_FILELIST();
            //    string path = dt.Rows[i]["FILEURL"].ToString();
            //    string filename = path.Substring(path.LastIndexOf('\\') + 1);
            //    //  string filepath = HttpUtility.UrlEncode(dt.Rows[i]["COMPANYCODE"].ToString() + "|" + dt.Rows[i]["SYSTEMCODE"].ToString() + "|" + dt.Rows[i]["MODELCODE"].ToString() + "|" + filename + "|" + dt.Rows[i]["FILENAME"].ToString());
            //    string filepath = HttpUtility.UrlEncode(dt.Rows[i]["FILEURL"].ToString() + "\\" + dt.Rows[i]["FILENAME"].ToString());
            //    file.SMTFILELISTID = dt.Rows[i]["SMTFILELISTID"].ToString();// uc.SmtFileListId;//主键ID
            //    file.FILENAME = dt.Rows[i]["FILENAME"].ToString();//文件名(用户自定义)         
            //    file.FILEURL = DownloadUrl + "?filename=" + filepath;//文件地址
            //    file.COMPANYCODE = dt.Rows[i]["COMPANYCODE"].ToString();//公司代号
            //    file.COMPANYNAME = dt.Rows[i]["COMPANYNAME"].ToString();//公司名字
            //    file.SYSTEMCODE = dt.Rows[i]["SYSTEMCODE"].ToString();//系统代号
            //    file.MODELCODE = dt.Rows[i]["MODELCODE"].ToString();//模块代号
            //    file.APPLICATIONID = dt.Rows[i]["APPLICATIONID"].ToString();//业务ID
            //    file.THUMBNAILURL = dt.Rows[i]["THUMBNAILURL"].ToString();//缩略图地址
            //    file.INDEXL = dt.Rows[i]["INDEXL"].ToString() == "" ? 0 : Convert.ToDecimal(dt.Rows[i]["INDEXL"]);//排序
            //    file.REMARK = dt.Rows[i]["REMARK"].ToString();//备注
            //    file.CREATETIME = Convert.ToDateTime(dt.Rows[i]["CREATETIME"]);//创建时间
            //    file.CREATENAME = dt.Rows[i]["CREATENAME"].ToString();//创建人
            //    file.UPDATETIME = Convert.ToDateTime(dt.Rows[i]["UPDATETIME"]);//修改时间
            //    file.UPDATENAME = dt.Rows[i]["UPDATENAME"].ToString();//修改人
            //    file.FILESIZE = dt.Rows[i]["FILESIZE"].ToString() == "" ? 0 : Convert.ToDecimal(dt.Rows[i]["FILESIZE"]);//修改人
            //    li.Add(file);
            //}
            //return li;
            #endregion
        }
        public CallBackResult GetFileListByOnlyApplicationID(string applicationid)
        {
            SMT.Foundation.Log.Tracer.Debug("开始获取文件");
            return  dal.GetFileListByOnlyApplicationID(applicationid);
        }

        public CallBackResult GetFileListByApplicationIDAndFileName(string applicationid,string FileName)
        {
            return dal.GetFileListByApplicationIDAndFileName(applicationid, FileName);
        }
        #endregion

        #region 删除文件 成员

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <param name="systemcode">系统代号</param>
        /// <param name="modelcode">模块代号</param>
        /// <param name="applicationid">业务ID</param>
        /// <returns></returns>
        public string DeleteFileByApplicationID(string companycode, string systemcode, string modelcode, string applicationid)
        {
            return dal.DeleteFileByApplicationID( companycode,  systemcode,  modelcode,  applicationid);
            #region
           
            //List<string> li = new List<string>();
            //string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            //string sql = "select * from T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' and SYSTEMCODE='" + systemcode + "' and MODELCODE='" + modelcode + "' and APPLICATIONID='" + applicationid + "' ";
            //string delSql="DELETE T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' and SYSTEMCODE='" + systemcode + "' and MODELCODE='" + modelcode + "' and APPLICATIONID='" + applicationid + "' ";
            //System.Data.DataTable dt = SMT.MSOracle.GetDataTable(sql);
            //int n = dt.Rows.Count;
            //for (int i = 0; i < n; i++)
            //{               
            //   string fileurl = dt.Rows[i]["FILEURL"].ToString();//
            //   string fileaddress = string.Concat(savepath, fileurl);
            //   li.Add(fileaddress);
            //}
            //if (SMT.MSOracle.ExecuteSQL(delSql) > 0)
            //{
            //    foreach (string u in li)
            //    {
            //        if (File.Exists(u))
            //        {
            //            File.Delete(u);
            //        }
            //    }
            //}
            
           // return applicationid;
            #endregion
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <param name="systemcode">系统代号</param>
        /// <param name="modelcode">模块代号</param>
        /// <param name="applicationid">业务ID</param>
        /// <returns></returns>
        public string DeleteFileByApplicationIDAndFileName(string applicationid,string FileName)
        {
            return dal.DeleteFileByApplicationIDAndFileName(applicationid, FileName);
           
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <param name="systemcode">系统代号</param>
        /// <param name="modelcode">模块代号</param>
        /// <returns></returns>
        public string DeleteFileByModelCode(string companycode, string systemcode, string modelcode)
        {
            return dal.DeleteFileByModelCode(companycode, systemcode, modelcode);
            #region
          
            //List<string> li = new List<string>();
            //string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            //string sql = "select * from T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' and SYSTEMCODE='" + systemcode + "' and MODELCODE='" + modelcode + "'  ";
            //string delSql = "DELETE T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' and SYSTEMCODE='" + systemcode + "' and MODELCODE='" + modelcode + "' ";
            //System.Data.DataTable dt = SMT.MSOracle.GetDataTable(sql);
            //int n = dt.Rows.Count;
            //for (int i = 0; i < n; i++)
            //{
            //    string fileurl = dt.Rows[i]["FILEURL"].ToString();//
            //    string fileaddress = string.Concat(savepath, fileurl);
            //    li.Add(fileaddress);
            //}
            //if (SMT.MSOracle.ExecuteSQL(delSql) > 0)
            //{
            //    foreach (string u in li)
            //    {
            //        if (File.Exists(u))
            //        {
            //            File.Delete(u);
            //        }
            //    }
            //}
            
           // return modelcode;
            #endregion
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <param name="systemcode">系统代号</param>
        /// <returns></returns>
        public string DeleteFileBySystemCode(string companycode, string systemcode)
        {
            return dal.DeleteFileBySystemCode(companycode, systemcode);
            #region
            
            //List<string> li = new List<string>();
            //string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            //string sql = "select * from T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' and SYSTEMCODE='" + systemcode + "' ";
            //string delSql = "DELETE T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' and SYSTEMCODE='" + systemcode + "' ";
            //System.Data.DataTable dt = SMT.MSOracle.GetDataTable(sql);
            //int n = dt.Rows.Count;
            //for (int i = 0; i < n; i++)
            //{
            //    string fileurl = dt.Rows[i]["FILEURL"].ToString();//
            //    string fileaddress = string.Concat(savepath, fileurl);
            //    li.Add(fileaddress);
            //}
            //if (SMT.MSOracle.ExecuteSQL(delSql) > 0)
            //{
            //    foreach (string u in li)
            //    {
            //        if (File.Exists(u))
            //        {
            //            File.Delete(u);
            //        }
            //    }
            //}
            //return systemcode;
            #endregion
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <returns></returns>
        public string DeleteFileByCompanycode(string companycode)
        {
            return dal.DeleteFileByCompanycode(companycode);
            #region
            
            //List<string> li = new List<string>();
            //string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            //string sql = "select * from T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "' ";
            //string delSql = "DELETE T_SYS_FILELIST WHERE COMPANYCODE='" + companycode + "'  ";
            //System.Data.DataTable dt = SMT.MSOracle.GetDataTable(sql);
            //int n = dt.Rows.Count;
            //for (int i = 0; i < n; i++)
            //{
            //    string fileurl = dt.Rows[i]["FILEURL"].ToString();//
            //    string fileaddress = string.Concat(savepath, fileurl);
            //    li.Add(fileaddress);
            //}
            //if (SMT.MSOracle.ExecuteSQL(delSql) > 0)
            //{
            //    foreach (string u in li)
            //    {
            //        if (File.Exists(u))
            //        {
            //            File.Delete(u);
            //        }
            //    }
            //}
            //return companycode;
            #endregion
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">业务ID(一定是唯一的)</param>
        /// <returns></returns>
        public string DeleteFileByOnlyApplicationID(string applicationid)
        {
            return dal.DeleteFileByOnlyApplicationID(applicationid);
            #region
            
            //List<string> li = new List<string>();
            //string savepath = string.Format(FileConfig.GetCompanyItem(applicationid).SavePath, "", "", "");
            //string sql = "select * from T_SYS_FILELIST WHERE APPLICATIONID='" + applicationid + "' ";
            //string delSql = "DELETE T_SYS_FILELIST WHERE APPLICATIONID='" + applicationid + "'  ";
            //System.Data.DataTable dt = SMT.MSOracle.GetDataTable(sql);
            //int n = dt.Rows.Count;
            //for (int i = 0; i < n; i++)
            //{
            //    string fileurl = dt.Rows[i]["FILEURL"].ToString();//
            //    string fileaddress = string.Concat(savepath, fileurl);
            //    li.Add(fileaddress);
            //}
            //if (SMT.MSOracle.ExecuteSQL(delSql) > 0)
            //{
            //    foreach (string u in li)
            //    {
            //        if (File.Exists(u))
            //        {
            //            File.Delete(u);
            //        }
            //    }
            //}
            //return applicationid;
            #endregion
        }

        #region 删除文件(真对正在上传，上传完成，取消上 传等)
        public string DeleteFile(string smtfilelistid, string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    //如果存在一个文件被多个记录使用则不删除文件
                    //var ents = from ent in dal.GetObjects<T_SYS_FILELIST>()
                    //           where ent.FILEURL == filePath
                    //           select ent;
                    var ents = from ent in dal.GetObjects<T_SYS_FILELIST>()
                               where ent.SMTFILELISTID == smtfilelistid
                               select ent;
                    if (ents.Count() == 1)
                    {
                        File.Delete(filePath);
                    }
                    dal.DeleteEntityByID(smtfilelistid);
                    return "成功";
                }
                catch (Exception e)
                {
                    return "失败：" + e.Message;
                }
            }
            else
            {
                return "文件不存在";
            }
        }
        //删除文件(真对DATAGRID帮定)
        public string DeleteFileByUrl(string url)
        {
            return dal.DeleteFileByUrl(url);
            #region
          
            ////\SMT\OA\考勤管理\2010121702324784503941.JPG
            //String fileurl = HttpUtility.UrlDecode(url).Split('=')[1];
            ////  string filePath = string.Format(SavePath, fileurl.Split('|')[0], fileurl.Split('|')[1], fileurl.Split('|')[2] + "\\" + fileurl.Split('|')[3]); //context.Server.MapPath("Bubble.jpg");
            //string filePath = string.Format(FileConfig.GetCompanyItem(fileurl.Split('\\')[1]).SavePath, fileurl.Split('\\')[1], fileurl.Split('\\')[2], fileurl.Split('\\')[3] + "\\" + fileurl.Split('\\')[4]);
            //if (File.Exists(filePath))
            //{
            //    try
            //    {
            //        File.Delete(filePath);
            //        dal.DeleteByFileUrl(fileurl.Substring(0, fileurl.LastIndexOf('\\')));
            //        return "1";
            //    }
            //    catch (Exception e)
            //    {
            //        return "0：" + e.Message;
            //    }
            //}
            //else
            //{
            //    dal.DeleteByFileUrl(fileurl.Substring(0, fileurl.LastIndexOf('\\')));
            //    return "-1";
            //}
            #endregion
        }
        #endregion
        #endregion

        #region 加密解密
        private string key = "12345678";
        /// <summary>
        /// 加密函数，将文件内容加密
        /// </summary>
        /// <param name="FileName"></param>
        public void Encrypt(string FileName,string StrPwd)
        {
            try
            {
                string Key = StrPwd;
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                des.Key = ASCIIEncoding.Default.GetBytes(Key);
                des.IV = ASCIIEncoding.Default.GetBytes(Key);
                FileStream InputFileStream = File.OpenRead(FileName);
                byte[] FileStreamArray = new byte[InputFileStream.Length - 1];
                InputFileStream.Read(FileStreamArray, 0, FileStreamArray.Length);
                InputFileStream.Close();
                InputFileStream.Dispose();
                MemoryStream ms = new MemoryStream();
                CryptoStream FileCryptoStream = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                FileCryptoStream.Write(FileStreamArray, 0, FileStreamArray.Length);
                FileCryptoStream.FlushFinalBlock();
                FileStream  InputFileStream2 = File.OpenWrite(FileName);
                
                foreach (byte b in ms.ToArray())
                {
                    InputFileStream2.WriteByte(b);

                }
                ms.Close();
                InputFileStream2.Close();
                FileCryptoStream.Close();
                InputFileStream2.Dispose();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("文件加密出现错误" + ex.ToString());
                SMT.Foundation.Log.Tracer.Debug("文件名："+ FileName +"加密字符串为：" +StrPwd);

            }
        }
        

        /// <summary>
        /// 将文件解密
        /// </summary>
        /// <param name="FileName"></param>
        public void Decrypt(string FileName)  
        {
            string Key = this.key;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = ASCIIEncoding.Default.GetBytes(Key);
            des.IV = ASCIIEncoding.Default.GetBytes(Key);
            FileStream InputFileStream = File.OpenRead(FileName);
            byte[] FileStreamArray = new byte[InputFileStream.Length];
            InputFileStream.Read(FileStreamArray, 0, FileStreamArray.Length);
            InputFileStream.Close();
            MemoryStream ms = new MemoryStream();
            CryptoStream FileCryptoStream = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            FileCryptoStream.Write(FileStreamArray, 0, FileStreamArray.Length);
            FileCryptoStream.FlushFinalBlock();
            InputFileStream = File.OpenWrite(FileName);
            foreach (byte b in ms.ToArray())
            {
                InputFileStream.WriteByte(b);
            }
            ms.Close();
            InputFileStream.Close();
            FileCryptoStream.Close();
            InputFileStream.Dispose();
          
        }

        
        #endregion

        /// <summary>
        /// 创建映射驱动器路径，默认创建的映射驱动器为Z:
        /// </summary>
        /// <param name="path">路径名</param>
        /// <param name="UserName">用户名</param>
        /// <param name="Pwd">用户密码</param>
        /// <param name="WNetPath">虚拟路径</param>
        /// <returns></returns>
        private  bool CreateDirectory(string path,string UserName,string Pwd,string WNetPath)  
        {  
            uint state = 0;
            bool IsReturn = false;
            try
            {
                if (!Directory.Exists("Z:"))
                {
                    //state = WNetHelper.WNetAddConnection("user1", "123456", @"\\172.16.1.57\fileupload", "Z:");  
                    state = WNetHelper.WNetAddConnection(UserName, Pwd, WNetPath, "Z:");
                }
                if (state.Equals(0))
                {
                    Directory.CreateDirectory(path);
                    IsReturn= true;
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("添加网络驱动器错误，错误号：" + state.ToString());
                    throw new Exception("添加网络驱动器错误，错误号：" + state.ToString());
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("添加网络驱动器错误，错误号：" + ex.ToString());

            }
            return IsReturn;
        }

        /// <summary>
        /// 创建公司上传文件的文件夹
        /// </summary>
        /// <param name="companyID">公司ID</param>
        /// <param name="companyName">公司名称(全称)</param>
        /// <param name="companyCode">公司编码</param>
        /// <returns></returns>
        public string CreateCompanyDirectory(string companyID, string companyName, string companyCode)
        {
            SMT.Foundation.Log.Tracer.Debug("公司:"+companyName+"开始创建文件夹："+companyID);
            return dal.CreateCompanyDirectory(companyID,companyName,companyCode);
        }

    }
}
