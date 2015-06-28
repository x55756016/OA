using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;

using SMT.SAAS.MP.DAL;
using TM_SaaS_OA_EFModel;
using System.Web;
using System.IO;
using System.Configuration;

namespace SMT.FileUpLoad.Service
{
    public class FileDAL: CommonDAL<T_SYS_FILELIST>
    {
        private string DownloadUrl = string.Concat(ConfigurationManager.AppSettings["DownLoadUrl"]);
        #region ADO 操作
        ///// <summary>
        ///// 增加一条数据
        ///// </summary>
        //public int Add(UserFile model)
        //{
           
        //    string insSql = "INSERT INTO T_SYS_FILELIST (SMTFILELISTID,FILENAME,FILETYPE,FILEURL,COMPANYCODE,COMPANYNAME,SYSTEMCODE,MODELCODE,THUMBNAILURL,INDEXL,REMARK,CREATETIME,CREATENAME,UPDATETIME,UPDATENAME,FILESIZE,APPLICATIONID) VALUES (:SMTFILELISTID,:FILENAME,:FILETYPE,:FILEURL,:COMPANYCODE,:COMPANYNAME,:SYSTEMCODE,:MODELCODE,:THUMBNAILURL,:INDEXL,:REMARK,:CREATETIME,:CREATENAME,:UPDATETIME,:UPDATENAME,:FILESIZE,:APPLICATIONID)";
        //    OracleParameter[] pageparm =
        //        {               
        //             new OracleParameter("SMTFILELISTID",OracleType.NVarChar,100), 
        //             new OracleParameter("FILENAME",OracleType.NVarChar,1000), 
        //             new OracleParameter("FILETYPE",OracleType.NVarChar,100), 
        //             new OracleParameter("FILEURL",OracleType.NVarChar,1000), 
        //             new OracleParameter("COMPANYCODE",OracleType.NVarChar,100), 
        //             new OracleParameter("COMPANYNAME",OracleType.NVarChar,200), 
        //             new OracleParameter("SYSTEMCODE",OracleType.NVarChar,100), 
        //             new OracleParameter("MODELCODE",OracleType.NVarChar,100), 
        //             new OracleParameter("THUMBNAILURL",OracleType.NVarChar,1000), 
        //             new OracleParameter("INDEXL",OracleType.Number,22), 
        //             new OracleParameter("REMARK",OracleType.NVarChar,1000), 
        //             new OracleParameter("CREATETIME",OracleType.DateTime,7), 
        //             new OracleParameter("CREATENAME",OracleType.NVarChar,100), 
        //             new OracleParameter("UPDATETIME",OracleType.DateTime,7), 
        //             new OracleParameter("UPDATENAME",OracleType.NVarChar,100),
        //             new OracleParameter("FILESIZE",OracleType.Number,22),
        //              new OracleParameter("APPLICATIONID",OracleType.NVarChar,100)
        //        };
        //    pageparm[0].Value = model.SmtFileListId;//主键ID
        //    pageparm[1].Value = model.FileName;//文件名
        //    pageparm[2].Value = model.FileType;//文件类型(.doc、.xls、.txt、.pdf......)
        //    pageparm[3].Value = model.FileUrl;//文件地址
        //    pageparm[4].Value = model.CompanyCode;//公司代号
        //    pageparm[5].Value = model.CompanyName;//公司名字
        //    pageparm[6].Value = model.SystemCode;//系统代号
        //    pageparm[7].Value = model.ModelCode;//模块代号
        //    pageparm[8].Value = String.IsNullOrEmpty(model.ThumbnailUrl) ? DBNull.Value.ToString() : model.ThumbnailUrl;// model.ThumbnailUrl;//缩略图地址
        //    pageparm[9].Value = model.INDEXL;//排序
        //    pageparm[10].Value = String.IsNullOrEmpty(model.Remark) ? DBNull.Value.ToString() : model.Remark;// model.Remark;//备注
        //    pageparm[11].Value = model.CreateTime;//创建时间
        //    pageparm[12].Value = String.IsNullOrEmpty(model.CreateName) ? DBNull.Value.ToString() : model.CreateName; ;// model.CreateName;//创建人
        //    pageparm[13].Value = model.UpdateTime;//修改时间
        //    pageparm[14].Value = String.IsNullOrEmpty(model.UpdateName) ? DBNull.Value.ToString() : model.UpdateName; ;// model.UpdateName;//修改人
        //    pageparm[15].Value = model.FileSize;//文件大小
        //    pageparm[16].Value = model.ApplicationID;//业务ID
        //    return SMT.MSOracle.ExecuteSQL(insSql, pageparm);
        //}
        ///// <summary>
        ///// 删除一条数据
        ///// </summary>
        //public int Delete(string smtfilelistid)
        //{
        //    string delSql = "DELETE FROM T_SYS_FILELIST  WHERE   SMTFILELISTID=:SMTFILELISTID";
        //    OracleParameter[] pageparm =
        //        {               
        //             new OracleParameter("SMTFILELISTID",OracleType.NVarChar,100) 

        //        };
        //    pageparm[0].Value = smtfilelistid;
        //    return SMT.MSOracle.ExecuteSQL(delSql, pageparm);
        //}
        ///// <summary>
        ///// 删除一条数据
        ///// </summary>
        //public int DeleteByFileUrl(string fileurl)
        //{
        //    string delSql = "DELETE FROM T_SYS_FILELIST  WHERE   FILEURL=:FILEURL";
        //    OracleParameter[] pageparm =
        //        {               
        //             new OracleParameter("FILEURL",OracleType.NVarChar,1000) 

        //        };
        //    pageparm[0].Value = fileurl;
        //    return SMT.MSOracle.ExecuteSQL(delSql, pageparm);
        //}
        #endregion
        #region 实体操作        
      
        #region 返回文件列表 成员

        /// <summary>
        /// 返回文件列表
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <returns></returns>
        public CallBackResult GetFileListByCompanyCode(string companycode)
        {
            CallBackResult result = new CallBackResult();
            result.DownloadUrl = DownloadUrl;
            #region 代码
            try
            {
                //var ents = from ent in base.GetObjects<T_SYS_FILELIST>()
                //           where ent.COMPANYCODE == companycode
                //           orderby ent.CREATETIME descending
                //           select ent;
                var ents = from ent in base.GetObjects<T_SYS_FILELIST>()
                          where ent.COMPANYCODE == companycode
                          orderby ent.INDEXL
                          select ent;
                if (ents.Count() > 0)
                {
                    //foreach (T_SYS_FILELIST file in ents)
                    //{
                    //    string path = file.FILEURL;
                    //    string filename = path.Substring(path.LastIndexOf('\\') + 1);
                    //    string filepath = HttpUtility.UrlEncode(file.FILEURL + "\\" + file.FILENAME);
                    //    file.FILEURL = DownloadUrl + "?filename=" + filepath;//文件地址
                    //}
                    result.FileList= ents.ToList();
                }
                
            }
            catch (Exception ex)
            {
                result.Error=ex.Message;
                throw (ex);
            }
            return result;
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
            CallBackResult result = new CallBackResult();
            result.DownloadUrl = DownloadUrl;
            #region 代码
            try
            {
                var ents = from ent in base.GetObjects<T_SYS_FILELIST>()
                           where ent.COMPANYCODE == companycode && ent.SYSTEMCODE == systemcode
                           orderby ent.INDEXL
                           select ent;
                if (ents.Count() > 0)
                {
                    //foreach (T_SYS_FILELIST file in ents)
                    //{
                    //    string path = file.FILEURL;
                    //    string filename = path.Substring(path.LastIndexOf('\\') + 1);
                    //    string filepath = HttpUtility.UrlEncode(file.FILEURL + "\\" + file.FILENAME);
                    //    file.FILEURL = DownloadUrl + "?filename=" + filepath;//文件地址
                    //}
                    result.FileList= ents.ToList();
                }               
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                throw (ex);
            }
            return result;
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
            CallBackResult result = new CallBackResult();
            result.DownloadUrl = DownloadUrl;
            #region 代码
            try
            {
                var ents = from ent in base.GetObjects<T_SYS_FILELIST>()
                           where ent.COMPANYCODE == companycode && ent.SYSTEMCODE == systemcode && ent.MODELCODE == modelcode
                           orderby ent.INDEXL
                           select ent;
                if (ents.Count() > 0)
                {
                    //foreach (T_SYS_FILELIST file in ents)
                    //{
                    //    string path = file.FILEURL;
                    //    string filename = path.Substring(path.LastIndexOf('\\') + 1);
                    //    string filepath = HttpUtility.UrlEncode(file.FILEURL + "\\" + file.FILENAME);
                    //    file.FILEURL = DownloadUrl + "?filename=" + filepath;//文件地址
                    //}
                    result.FileList= ents.ToList();
                }               
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                throw (ex);
            }
            return result;
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
            CallBackResult result = new CallBackResult();
            result.DownloadUrl = DownloadUrl;
            #region 代码
            try
            {
                var ents = from ent in base.GetObjects<T_SYS_FILELIST>()
                           where ent.MODELCODE == modelcode
                           && ent.APPLICATIONID == applicationid
                           orderby ent.INDEXL
                           select ent;
                int allcount = ents.Count();
                if (allcount > 0)
                {
                    result.FileList = ents.ToList();
                }
               
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                throw (ex);
            }
            return result;
            #endregion
        }
        /// <summary>
        /// 返回文件列表
        /// </summary>
        /// <param name="applicationid">业务ID(一定是唯一的)</param>
        /// <returns></returns>
        public CallBackResult GetFileListByOnlyApplicationID(string applicationid)
        {
            CallBackResult result = new CallBackResult();
            result.DownloadUrl = DownloadUrl;
            #region 代码
            try
            {
                var ents = from ent in base.GetObjects<T_SYS_FILELIST>()
                           where ent.APPLICATIONID == applicationid
                           orderby ent.INDEXL,ent.CREATETIME
                           select ent;
                if (ents.Count() > 0)
                {                  
                    result.FileList = ents.ToList();
                    SMT.Foundation.Log.Tracer.Debug("获取的数量" + result.FileList.ToList().Count());
                }

            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                throw (ex);
            }
            
            return result;
            #endregion
        }

        /// <summary>
        /// 返回文件列表
        /// </summary>
        /// <param name="applicationid">业务ID(一定是唯一的)</param>
        /// <returns></returns>
        public CallBackResult GetFileListByApplicationIDAndFileName(string applicationid,string FileName)
        {
            CallBackResult result = new CallBackResult();
            result.DownloadUrl = DownloadUrl;
            #region 代码
            try
            {
                var ents = from ent in base.GetObjects<T_SYS_FILELIST>()
                           where ent.APPLICATIONID == applicationid
                           //&& ent.FILEURL.Contains(FileName)
                           && FileName.Contains(ent.FILEURL)
                           orderby ent.INDEXL
                           select ent;
                if (ents.Count() > 0)
                {
                    result.FileList = ents.ToList();
                }

            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                throw (ex);
            }
            return result;
            #endregion
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
            string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            // List<string> li = new List<string>();//存放临时文件
            var list = from det in base.GetObjects<T_SYS_FILELIST>()
                       where det.COMPANYCODE == companycode && det.SYSTEMCODE == systemcode && det.MODELCODE == modelcode && det.APPLICATIONID==applicationid 
                       select det;//找出原有的实体
            foreach (T_SYS_FILELIST file in list)
            {
                string fileurl = file.FILEURL;
                string fileaddress = string.Concat(savepath, fileurl);
                if (DeleteEntity(file) > 0)
                {
                    if (File.Exists(fileaddress))
                    {
                        File.Delete(fileaddress);
                    }
                }
                //  li.Add(fileaddress); 
            }
            return applicationid;
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
            try
            {
                var list = from det in base.GetObjects<T_SYS_FILELIST>()
                           where det.APPLICATIONID == applicationid
                           && det.FILEURL == FileName
                           select det;//找出原有的实体
                foreach (T_SYS_FILELIST file in list)
                {
                    string savepath = string.Format(FileConfig.GetCompanyItem(file.COMPANYCODE).SavePath, "", "", "");
                    string fileurl = file.FILEURL;
                    string fileaddress = string.Concat(savepath, fileurl);
                    if (DeleteEntity(file) > 0)
                    {
                        var listExists = from det in base.GetObjects<T_SYS_FILELIST>()
                                         where det.FILEURL == FileName
                                         select det;//找出原有的实体
                        //只有上传的文件不存在被其它文件使用则删除该文件
                        //即一个文件可能会已经上传
                        if (listExists.Count() == 0)
                        {
                            if (File.Exists(fileaddress))
                            {
                                File.Delete(fileaddress);
                            }
                            else
                            {
                                return "-1";
                            }
                        }

                    }

                }
                return "1";
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("删除上传附件出现错误:删除ID为："+ applicationid+",错误信息为："+ex.ToString());
                return "0";
            }
            
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
            string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            // List<string> li = new List<string>();//存放临时文件
            var list = from det in base.GetObjects<T_SYS_FILELIST>()
                       where det.COMPANYCODE == companycode && det.SYSTEMCODE == systemcode && det.MODELCODE==modelcode
                       select det;//找出原有的实体
            foreach (T_SYS_FILELIST file in list)
            {
                string fileurl = file.FILEURL;
                string fileaddress = string.Concat(savepath, fileurl);
                if (DeleteEntity(file) > 0)
                {
                    if (File.Exists(fileaddress))
                    {
                        File.Delete(fileaddress);
                    }
                }
                //  li.Add(fileaddress); 
            }
            return modelcode;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <param name="systemcode">系统代号</param>
        /// <returns></returns>
        public string DeleteFileBySystemCode(string companycode, string systemcode)
        {
            string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            // List<string> li = new List<string>();//存放临时文件
            var list = from det in base.GetObjects<T_SYS_FILELIST>()
                       where det.COMPANYCODE == companycode && det.SYSTEMCODE == systemcode
                       select det;//找出原有的实体
            foreach (T_SYS_FILELIST file in list)
            {
                string fileurl = file.FILEURL;
                string fileaddress = string.Concat(savepath, fileurl);
                if (DeleteEntity(file) > 0)
                {
                    if (File.Exists(fileaddress))
                    {
                        File.Delete(fileaddress);
                    }
                }
                //  li.Add(fileaddress); 
            }
            return systemcode;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <returns></returns>
        public string DeleteFileByCompanycode(string companycode)
        {
            string savepath = string.Format(FileConfig.GetCompanyItem(companycode).SavePath, "", "", "");
            // List<string> li = new List<string>();//存放临时文件
            var list = from det in base.GetObjects<T_SYS_FILELIST>()
                       where det.COMPANYCODE == companycode
                       select det;//找出原有的实体
            foreach (T_SYS_FILELIST file in list)
            {
                string fileurl = file.FILEURL;
                string fileaddress = string.Concat(savepath, fileurl);
                if (DeleteEntity(file) > 0)
                {
                    if (File.Exists(fileaddress))
                    {
                        File.Delete(fileaddress);
                    }
                }
                //  li.Add(fileaddress); 
            }
            return companycode;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">业务ID(一定是唯一的)</param>
        /// <returns></returns>
        public string DeleteFileByOnlyApplicationID(string applicationid)
        {
           
           // List<string> li = new List<string>();//存放临时文件
            var list = from det in base.GetObjects<T_SYS_FILELIST>()
                          where det.APPLICATIONID == applicationid
                          select det;//找出原有的实体
            foreach (T_SYS_FILELIST file in list)
            {
                string savepath = string.Format(FileConfig.GetCompanyItem(file.COMPANYCODE).SavePath, "", "", "");
                string fileurl = file.FILEURL;
                string fileaddress = string.Concat(savepath, fileurl);
                if (DeleteEntity(file) > 0)
                {
                    if (File.Exists(fileaddress))
                    {
                        File.Delete(fileaddress);
                    } 
                }
              //  li.Add(fileaddress); 
            }
            return applicationid;
        }

        #region 删除文件(真对正在上传，上传完成，取消上 传等)
        public string DeleteFile(string smtfilelistid, string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    DeleteEntityByID(smtfilelistid);
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
            String fileurl = HttpUtility.UrlDecode(url).Split('=')[1];
            //  string filePath = string.Format(SavePath, fileurl.Split('|')[0], fileurl.Split('|')[1], fileurl.Split('|')[2] + "\\" + fileurl.Split('|')[3]); //context.Server.MapPath("Bubble.jpg");
            string filePath = string.Format(FileConfig.GetCompanyItem(fileurl.Split('\\')[1]).SavePath, fileurl.Split('\\')[1], fileurl.Split('\\')[2], fileurl.Split('\\')[3] + "\\" + fileurl.Split('\\')[4]);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    DeleteEntityByUrl(fileurl.Substring(0, fileurl.LastIndexOf('\\')));                  
                    return "1";
                }
                catch (Exception e)
                {
                    return "0：" + e.Message;
                }
            }
            else
            {
                DeleteEntityByUrl(fileurl.Substring(0, fileurl.LastIndexOf('\\')));
                return "-1";
            }
        }
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="url">路径如:\SMT\OA\TaskManager\2010122303190771495486.txt</param>
        /// <returns></returns>
        public int DeleteEntityByUrl(string url)
        {
            int result = 0;
            #region 删除从表
            TM_SaaS_OA_EFModelContext edc = new TM_SaaS_OA_EFModelContext();
            var entity = from det in edc.T_SYS_FILELIST
                          where det.FILEURL == url
                          select det;//找出原有的实体
            if (entity.Count() > 0)
            {
                var det = entity.FirstOrDefault();
                //DataContext.DeleteObject(det);
                //result = DataContext.SaveChanges();            
                 edc.DeleteObject(det);
                result = edc.SaveChanges();  
            }
            return result > 0 ? 1 : 0;
            #endregion
        }
        /// <summary>
        /// 增加实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int AddEntity(T_SYS_FILELIST entity)
        {
            int result = 0;
            #region
            try
            {
                if (entity != null)
                {
                    entity.CREATETIME = System.DateTime.Now;
                    TM_SaaS_OA_EFModel.TM_SaaS_OA_EFModelContext edc = new TM_SaaS_OA_EFModelContext();
                    edc.AddToT_SYS_FILELIST(entity);
                    result=edc.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return result > 0 ? 1 : 0;
            #endregion
        }
        public int DeleteEntity(T_SYS_FILELIST entity)
        {
            int result = 0;
            #region 删除从表
            if (entity != null)
            {
                //DataContext.DeleteObject(entity);
                //result = DataContext.SaveChanges();
                base.DeleteFromContext(entity);
                result = base.SaveContextChanges();
                //EngineDataModel.EngineDataModelContext edc = new EngineDataModelContext();
                //edc.DeleteObject(entity);
                //result=edc.SaveChanges();
            }
           return result > 0 ? 1 : 0;
            #endregion
        }
       
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="smtfilelistid">ID</param>
        /// <returns></returns>
        public int DeleteEntityByID(string smtfilelistid)
        {
            int result = 0;
            #region 删除从表
            var details = from det in base.GetObjects<T_SYS_FILELIST>()
                          where det.SMTFILELISTID == smtfilelistid
                          select det;//找出原有的实体
            if (details.Count() > 0)
            {
                var det = details.FirstOrDefault();
                base.DeleteFromContext(det);
                //DataContext.DeleteObject(det);
                //result = DataContext.SaveChanges();
                result = base.SaveContextChanges();
            }
            return result > 0 ? 1 : 0;
            #endregion
        }
        #endregion
        #endregion
        ///// <summary>
        ///// 返回[这是一个树状结构，包括产品名称和产品的内容（菜单）]所有列表
        ///// </summary>
        ///// <returns></returns>
        //public IQueryable<T_SYS_FILELIST> GetTable()
        //{
        //    return base.GetTable<T_SYS_FILELIST>();
        //}
        #endregion

        #region 当刚创建公司时添加文件夹和配置文件
        public string CreateCompanyDirectory(string companyID, string companyName, string companyCode)
        {
            string strReturn = string.Empty;
            try
            {
                strReturn = FileConfig.CreateCompanyDirecttory(companyCode, companyName, companyID);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("创建文件夹失败："+ ex.ToString());
                string bb = ex.ToString();
            }
            return strReturn;
        }
        #endregion
    }
}
