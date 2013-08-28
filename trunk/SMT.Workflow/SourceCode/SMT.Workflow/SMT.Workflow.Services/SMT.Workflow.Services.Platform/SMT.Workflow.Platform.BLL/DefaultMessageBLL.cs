using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model;
using System.Data.OracleClient;
using System.Transactions;
using System.Data;
using System.Xml.Linq;
using SMT.Workflow.Common.Model.FlowXml;

namespace SMT.Workflow.Platform.BLL
{
    public class DefaultMessageBLL
    {
        DefaultMessageDAL dal = new DefaultMessageDAL();


        /// <summary>
        ///
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="strFilter"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_WF_DEFAULTMESSAGE> GetDefaultMessageList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {
                if (string.IsNullOrEmpty(strOrderBy))
                {
                    strOrderBy = "CREATEDATETIME DESC";
                }
                if (pageSize < 5)
                {
                    pageSize = 15;
                }
                return dal.GetDefaultMessageList(pageSize, pageIndex, strFilter, strOrderBy, ref  pageCount);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void InitDefaultMessage()
        {
            //using (TransactionScope scope = new TransactionScope())
            //{
                try
                {
                    List<AppSystem> ListSystemCode = ListSystem();
                    foreach (var item in ListSystemCode)
                    {
                        List<AppModel> ListModel = AppModel(item.ObjectFolder);
                        foreach (var detail in ListModel)
                        {
                            for (int i = 1; i <= 3; i++)
                            {
                                T_WF_DEFAULTMESSAGE entity = new T_WF_DEFAULTMESSAGE();
                                entity.MODELCODE = detail.Name;
                                entity.SYSTEMCODE = item.Name;
                                entity.SYSTEMNAME = item.Description;
                                entity.MODELNAME = detail.Description;
                                entity.APPLICATIONURL = GetMessageUrl(item.ObjectFolder, detail.Name);
                                entity.AUDITSTATE = i;
                                entity.CREATEUSERID = "系统初始化";
                                entity.CREATEUSERNAME = "系统初始化";
                                switch (i)
                                {
                                    case 1:
                                        entity.MESSAGECONTENT = "您有一条[" + detail.Description + "]的单据需要审核!";
                                        break;
                                    case 2:
                                        entity.MESSAGECONTENT = "您的[" + detail.Description + "]已审核通过!";
                                        break;
                                    case 3:
                                        entity.MESSAGECONTENT = "您的[" + detail.Description + "]审核不通过!";
                                        break;
                                }
                                AddDefaultMessage(entity);
                            }
                        }
                    }
                    //scope.Complete();

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex);
                }
                finally
                {
                    //释放资源
                    //scope.Dispose();
                }
            //}

        }


        public List<AppSystem> ListSystem()
        {
            List<AppSystem> List = new List<AppSystem>();
            try
            {
                string Path = System.Web.Hosting.HostingEnvironment.MapPath("~/BOSystemList.xml");
                XDocument xdoc = XDocument.Load(Path);
                var xmlTree = from c in xdoc.Descendants("System")
                              select c;
                if (xmlTree.Count() > 0)
                {
                    foreach (var v in xmlTree)
                    {
                        AppSystem sys = new AppSystem();
                        sys.Name = v.Attribute("Name").Value;
                        sys.Description = v.Attribute("Description").Value;
                        sys.ObjectFolder = v.Attribute("ObjectFolder").Value;
                        List.Add(sys);
                    }
                }
            }
            catch
            {
            }
            return List;
        }

        public List<AppModel> AppModel(string ObjectFolder)
        {
            List<AppModel> List = new List<AppModel>();
            try
            {
                string Path = System.Web.Hosting.HostingEnvironment.MapPath("~/BusinessObjects/" + ObjectFolder + "/BOList.xml");
                XDocument xdoc = XDocument.Load(Path);
                var xmlTree = from c in xdoc.Descendants("ObjectList").Descendants<XElement>("Object")
                              select c;
                if (xmlTree.Count() > 0)
                {
                    foreach (var v in xmlTree)
                    {
                        AppModel model = new AppModel();
                        model.Name = v.Attribute("Name").Value;
                        model.Description = v.Attribute("Description").Value;
                        model.ObjectFolder = ObjectFolder;
                        List.Add(model);
                    }
                }
            }
            catch
            {
             
            }
            return List;
        }


        public string GetMessageUrl(string ObjectFolder, string strFolderName)
        {
            try
            {
                string Path = System.Web.Hosting.HostingEnvironment.MapPath("~/BusinessObjects/" + ObjectFolder + "/" + strFolderName + ".xml");
                XDocument doc = XDocument.Load(Path);

                string strXml = doc.Document.ToString();
                int Start = strXml.IndexOf("<MsgOpen>") + "<MsgOpen>".Length;
                int End = strXml.IndexOf("</MsgOpen>");
                return strXml.Substring(Start, End - Start);
            }
            catch
            {
                return "";
            }
          
        }

        public bool GetBool(T_WF_DEFAULTMESSAGE message)
        {
            string sql = "select * from T_WF_DEFAULTMESSAGE where SYSTEMCODE='" + message.SYSTEMCODE + "' and MODELCODE='" + message.MODELCODE + "' and AUDITSTATE='" + message.AUDITSTATE + "'";
            sql += " and  MESSAGEID!='" + message.MESSAGEID + "'";
            if (dal.GetDataTable(sql).Rows.Count > 0)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 新增默认消息
        /// </summary>
        /// <param name="entity"></param>
        public void AddDefaultMessage(T_WF_DEFAULTMESSAGE entity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entity.MESSAGEID))
                {
                    entity.MESSAGEID = Guid.NewGuid().ToString();
                }
                string insSql = @"INSERT INTO T_WF_DEFAULTMESSAGE (MESSAGEID,SYSTEMCODE,SYSTEMNAME,MODELCODE,MODELNAME,AUDITSTATE,
                                  MESSAGECONTENT,APPLICATIONURL,CREATEUSERID,CREATEUSERNAME) VALUES (:MESSAGEID,:SYSTEMCODE,:SYSTEMNAME,:MODELCODE,
                                  :MODELNAME,:AUDITSTATE,:MESSAGECONTENT,:APPLICATIONURL,:CREATEUSERID,:CREATEUSERNAME)";

                OracleParameter[] pageparm =
                {  
                    new OracleParameter(":MESSAGEID",dal.GetValue(entity.MESSAGEID)), 
                    new OracleParameter(":SYSTEMCODE",dal.GetValue(entity.SYSTEMCODE)),
                    new OracleParameter(":SYSTEMNAME",dal.GetValue(entity.SYSTEMNAME)), 
                    new OracleParameter(":MODELCODE",dal.GetValue(entity.MODELCODE)), 
                    new OracleParameter(":MODELNAME",dal.GetValue(entity.MODELNAME)),
                    new OracleParameter(":AUDITSTATE",dal.GetValue(entity.AUDITSTATE)), 
                    new OracleParameter(":MESSAGECONTENT",dal.GetValue(entity.MESSAGECONTENT)),    
                     new OracleParameter(":APPLICATIONURL",dal.GetValue(entity.APPLICATIONURL)),    
                    new OracleParameter(":CREATEUSERID",dal.GetValue(entity.CREATEUSERID)), 
                    new OracleParameter(":CREATEUSERNAME",dal.GetValue(entity.CREATEUSERNAME))

                };
                dal.ExecuteSql(insSql, pageparm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void EditDefaultMessage(T_WF_DEFAULTMESSAGE entity)
        {
            try
            {

                string updSql = @"UPDATE T_WF_DEFAULTMESSAGE SET SYSTEMCODE=:SYSTEMCODE,SYSTEMNAME=:SYSTEMNAME,MODELCODE=:
                                 MODELCODE,MODELNAME=:MODELNAME,AUDITSTATE=:AUDITSTATE,MESSAGECONTENT=:MESSAGECONTENT,APPLICATIONURL=:APPLICATIONURL,UPDATEUSERID=:UPDATEUSERID,
                                 UPDATEUSERNAME=:UPDATEUSERNAME,UPDATEDATE=:UPDATEDATE WHERE   MESSAGEID=:MESSAGEID";
                OracleParameter[] pageparm =
                { 
                    new OracleParameter(":SYSTEMCODE",dal.GetValue(entity.SYSTEMCODE)), 
                    new OracleParameter(":SYSTEMNAME",dal.GetValue(entity.SYSTEMNAME)), 
                    new OracleParameter(":MODELCODE",dal.GetValue(entity.MODELCODE)), 
                    new OracleParameter(":SYSTEMCODE",dal.GetValue(entity.SYSTEMCODE)),
                    new OracleParameter(":MODELNAME",dal.GetValue(entity.MODELNAME)), 
                    new OracleParameter(":AUDITSTATE",dal.GetValue(entity.AUDITSTATE)), 
                    new OracleParameter(":MESSAGECONTENT",dal.GetValue(entity.MESSAGECONTENT)), 
                    new OracleParameter(":APPLICATIONURL",dal.GetValue(entity.APPLICATIONURL)),    
                    new OracleParameter(":UPDATEUSERID",dal.GetValue(entity.CREATEUSERID)),
                    new OracleParameter(":UPDATEUSERNAME",dal.GetValue(entity.CREATEUSERNAME)),
                    new OracleParameter(":UPDATEDATE",dal.GetValue(DateTime.Now)),
                    new OracleParameter(":MESSAGEID",dal.GetValue(entity.MESSAGEID))

                };
                dal.ExecuteSql(updSql, pageparm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        public void DeleteDefaultMessage(string DeleteID)
        {
            try
            {

                string Sql = "DELETE FROM T_WF_DEFAULTMESSAGE WHERE  MESSAGEID =:MESSAGEID";
                OracleParameter[] pageparm =
                { 
                    new OracleParameter(":MESSAGEID",dal.GetValue(DeleteID)), 
                };
                dal.ExecuteSql(Sql, pageparm);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

    }
}
