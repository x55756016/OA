/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：PersonalRecordBLL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/26 9:30:04   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services.BLL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Engine.Services.DAL;
using System.Runtime.Serialization;
using System.ServiceModel;
using EngineDataModel;
using SMT.Workflow.Common.DataAccess;
using System.IO;
using System.Net.Mail;

namespace SMT.Workflow.Engine.Services.BLL
{
    public class PersonalRecordBLL
    {

        #region 发邮件
        private  string Templete = string.Empty;
        /// <summary>
        /// 模版
        /// </summary>
        /// <returns>string</returns>
        private  string GetMailBodeTempete()
        {
            if (Templete == string.Empty)
            {
                StreamReader sr = File.OpenText(Config.MailTempletePath);
                string strRead = sr.ReadToEnd();
                sr.Dispose();
                Templete = strRead;
                return strRead;
            }
            else
            {
                return Templete;
            }
        }
        private  string Utf8(string strLine)
        {
            byte[] utf8byte = Encoding.Unicode.GetBytes(strLine);
            string strOutLine = Encoding.UTF8.GetString(utf8byte);
            return strLine;
        }
       /// <summary>
        /// 发送邮件
       /// </summary>
       /// <param name="sendMessage">发送消息</param>
       /// <param name="toEmail">到达Email</param>
        private  void SendEmail(string sendMessage, string toEmail)
        {
            try
            {
                if (!string.IsNullOrEmpty(toEmail))
                {
                    string Title = sendMessage;
                    string PutDate = DateTime.Now.ToString();
                    string BodyTemplete = GetMailBodeTempete();
                    string MsgUrl = Config.MailUrl;
                    PutDate = Convert.ToDateTime(PutDate).ToString("yyyy年MM月dd日");
                    BodyTemplete = string.Format(BodyTemplete, MsgUrl, Title, PutDate);

                    MailAddress sendAddress = new MailAddress(Config.MailAddress);
                    MailAddress receiveAddress = new MailAddress(toEmail);
                    MailMessage message = new MailMessage(sendAddress, receiveAddress);
                    message.Subject = string.Format(Config.MailTitle, sendMessage);
                    message.IsBodyHtml = true;
                    message.Body = Utf8(BodyTemplete);
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                    string smtp = Config.MailServerAddress;
                    SmtpClient sc = new SmtpClient(smtp);
                    sc.Port = Config.MailServerPort;
                    sc.Credentials = new System.Net.NetworkCredential(Config.MailAddress, Config.MailPwd);
                    sc.Timeout = 10000;
                    sc.Send(message);
                }
            }
            catch (Exception ex)
            {
                string cMessage = "发送邮件报错(转化新增):";
                cMessage += "SendMail:" + string.Concat(Config.MailAddress) + "\r\n";
                cMessage += "ReceiveMail:" + string.Concat(toEmail) + "\r\n";
                Log.WriteLog(cMessage + ex.Message);
            }
        }
        #endregion
        /// <summary>
        /// 我的单据新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AddPersonalRecord(T_PF_PERSONALRECORD model)
        {
            try
            {            
                PersonalRecordDAL dal = new PersonalRecordDAL();
                string recordID = dal.GetExistDoTask(model.MODELCODE, model.MODELID, model.OWNERID, model.OWNERCOMPANYID);
                if (model.CHECKSTATE == "0" && string.IsNullOrWhiteSpace(recordID))
                {
                    SMTEngine.EngineWcfGlobalFunctionClient client = new SMTEngine.EngineWcfGlobalFunctionClient();
                    client.TaskCacheReflesh(model.OWNERID);
                    return dal.AddDoTask(model);
                }
                else
                {
                    return true; //修改状态该为流程来做
                }             
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("新增我的代办 CHECKSTATE=" + model.MODELCODE + "Exception:"+ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }
        /// <summary>
        /// 我的单据新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AddPersonalRecordList(T_PF_PERSONALRECORD model)
        {
            try
            {
                PersonalRecordDAL dal = new PersonalRecordDAL();
                string recordID = dal.GetExistRecord(model.SYSTYPE, model.MODELCODE, model.MODELID, model.OWNERID, model.OWNERCOMPANYID, model.ISFORWARD);
                if (recordID != "")
                {
                    return dal.UpdatePersonalRecord(model, recordID);
                }
                else
                {
                    var bol= dal.AddPersonalRecord(model);
                    if (bol)
                    {
                         PersonnelWS.PersonnelServiceClient HRClient = new PersonnelWS.PersonnelServiceClient();
                         var user = HRClient.GetEmployeeByID(model.OWNERID);
                         SendEmail("你有新的单据请登录系统查看!", user.EMAIL);
                         Log.WriteLog("FormID=" + model.MODELID + ";MODELDESCRIPTION=" + model.MODELDESCRIPTION + ";接收EMAIL=" + user.EMAIL+";所属人ID="+user.OWNERID);
                    }
                    return bol;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// 新增我的单据
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public bool AddPersonalRecords(List<T_PF_PERSONALRECORD> models)
        {
            try
            {
                if (models == null||models.Count<=0)
                {
                    return false;
                }
                foreach (T_PF_PERSONALRECORD entity in models)
                { 
                    string CodeName="";
                    SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient clientPerm = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient();
                    SMT.SaaS.BLLCommonServices.PermissionWS.T_SYS_ENTITYMENU entMenu = clientPerm.GetSysMenuByEntityCode(entity.MODELCODE);
                    CodeName = entMenu != null && !string.IsNullOrWhiteSpace(entMenu.MENUNAME) ? entMenu.MENUNAME : entity.MODELCODE;
                    entity.MODELDESCRIPTION = entity.MODELDESCRIPTION.Replace(entity.MODELCODE, CodeName);
                    AddPersonalRecordList(entity);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(this, "AddPersonalRecords()", "BLLCommonServices调用出错", ex);
                return false;
            }
        }

        /// <summary>
        /// 修改的单据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdatePersonalRecord(T_PF_PERSONALRECORD model)
        {
            PersonalRecordDAL dal = new PersonalRecordDAL();
            if (string.IsNullOrWhiteSpace(model.PERSONALRECORDID))
            {
                string recordID = dal.GetExistRecord(model.SYSTYPE, model.MODELCODE, model.MODELID, model.OWNERID, model.OWNERCOMPANYID, model.ISFORWARD);
                return dal.UpdatePersonalRecord(model, recordID);
            }
            else
            {
                return dal.UpdatePersonalRecord(model, model.PERSONALRECORDID);
            }
        }

        /// <summary>
        /// 删除我的单据
        /// </summary>
        /// <param name="_personalrecordID"></param>
        /// <returns></returns>
        public bool DeletePersonalRecord(string _personalrecordID)
        {            
             if (string.IsNullOrWhiteSpace(_personalrecordID))
             {
                 return false;
             }
             else
             {
                 PersonalRecordDAL dal = new PersonalRecordDAL();
                 T_PF_PERSONALRECORD ent = dal.GetEntity("", "", _personalrecordID, "");
                 SMTEngine.EngineWcfGlobalFunctionClient client = new SMTEngine.EngineWcfGlobalFunctionClient();
                 client.TaskCacheReflesh(ent.OWNERID);
                 return dal.DeletePersonalRecord(_personalrecordID);
             }
        }

        /// <summary>
        /// 返回我的单据实体
        /// </summary>
        /// <param name="_personalrecordID"></param>
        /// <returns></returns>
        public T_PF_PERSONALRECORD GetPersonalRecordModelByID(string _personalrecordID)
        {          
            if (string.IsNullOrWhiteSpace(_personalrecordID))
            {
                return null;
            }
            else
            {
                PersonalRecordDAL dal = new PersonalRecordDAL();
                return dal.GetEntity(_personalrecordID);
            }
        }

        /// <summary>
        /// 返回我的单据实体
        /// </summary>
        /// <param name="_sysType"></param>
        /// <param name="_modelCode"></param>
        /// <param name="_modelID"></param>
        /// <param name="_IsForward"></param>
        /// <returns></returns>
        public T_PF_PERSONALRECORD GetPersonalRecordModelByModelID(string _sysType, string _modelCode, string _modelID, string _IsForward)
        {
            if (string.IsNullOrWhiteSpace(_sysType) && string.IsNullOrWhiteSpace(_modelCode) && string.IsNullOrWhiteSpace(_modelID)
                && string.IsNullOrWhiteSpace(_IsForward))
            {
                return null;
            }
            else
            {
                PersonalRecordDAL dal = new PersonalRecordDAL();
                return dal.GetEntity(_sysType, _modelCode, _modelID, _IsForward);
            }
        }

        /// <summary>
        /// 获取我的单据列表（三种状态，审核中，审核通过，审核不通过）
        /// </summary>
        /// <param name="strOrderBy"></param>
        /// <param name="strCreateID"></param>
        /// <param name="pageIndex"></param>
        /// <param name="checkstate"></param>
        /// <param name="strFilter"></param>
        /// <param name="BeginDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_PF_PERSONALRECORD> GetPersonalRecord(int pageIndex, string strOrderBy, string checkstate, string filterString, string strCreateID,
            string BeginDate, string EndDate, ref int pageCount)
        {
            PersonalRecordDAL dal = new PersonalRecordDAL();
            string filter = "";
            if (!string.IsNullOrEmpty(checkstate))
            {
                filter += " AND CHECKSTATE=" + checkstate + "";
            }
            if (!string.IsNullOrEmpty(strCreateID))
            {
                filter += " AND OWNERID='" + strCreateID + "'";
            }          
            filter += " AND ISFORWARD=" + 0 + "";           
            if (!string.IsNullOrEmpty(filterString))
            {
                filter += filterString;
            }
            return dal.GetPersonalRecordList(Config.pageSize, filter, pageIndex, strOrderBy, ref  pageCount);
           
            //PersonalRecordDAL dal = new PersonalRecordDAL();          
            //return dal.GetPersonalRecord(Config.pageSize,strOrderBy, strCreateID, pageIndex, checkstate, filterString, BeginDate, EndDate, ref  pageCount);
        }

        /// <summary>
        /// 返回我的单据实体列表（未提交，转发两种状态的）
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="checkstate"></param>
        /// <param name="filterString"></param>
        /// <param name="strCreateID"></param>
        /// <param name="Isforward"></param>
        /// <param name="BeginDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_PF_PERSONALRECORD> GetPersonalRecordList(int pageIndex, string strOrderBy, string checkstate, string filterString,
             string strCreateID, string Isforward, string BeginDate, string EndDate, ref int pageCount)
        {
             PersonalRecordDAL dal = new PersonalRecordDAL();
             string filter = "";
             if (!string.IsNullOrEmpty(checkstate))
             {
                 filter += " AND CHECKSTATE=" + checkstate + "";
             }
             if (!string.IsNullOrEmpty(strCreateID))
             {
                 filter += " AND OWNERID='" + strCreateID + "'";
             }
             if (!string.IsNullOrEmpty(Isforward))
             {
                 filter += " AND ISFORWARD=" + Isforward + "";
             }
             if (!string.IsNullOrEmpty(filterString))
             {
                 filter += filterString;
             }
             return dal.GetPersonalRecordList(Config.pageSize, filter, pageIndex, strOrderBy, ref  pageCount);
        }
        /// <summary>
        /// 返回我的单据实体列表[longkc]
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="checkstate"></param>
        /// <param name="filterString"></param>
        /// <param name="ownerid"></param>
        /// <param name="Isforward"></param>
        /// <param name="BeginDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_PF_PERSONALRECORD> GetPersonalRecordListNew(int pageIndex, string strOrderBy, string checkstate, string filterString,
             string ownerid, string Isforward, string BeginDate, string EndDate, ref int pageCount)
        {
            PersonalRecordDAL dal = new PersonalRecordDAL();
            string filter = "";
            if (!string.IsNullOrEmpty(checkstate))
            {
                filter += " AND CHECKSTATE=" + checkstate + "";
            }
            if (!string.IsNullOrEmpty(ownerid))
            {
                filter += " AND OWNERID='" + ownerid + "'";
            }
            if (!string.IsNullOrEmpty(Isforward))
            {
                filter += " AND ISFORWARD=" + Isforward + "";
            }  
            if (!string.IsNullOrEmpty(BeginDate))
            {
                filter += " AND createdate>=to_date('" + BeginDate + "','yyyy-MM-dd')";
            }
            if (!string.IsNullOrEmpty(EndDate))
            {
                filter += " AND createdate<=to_date('" + EndDate + "','yyyy-MM-dd')";
            } 
            if (!string.IsNullOrEmpty(filterString)) 
            {
                filter += filterString;
            }
            return dal.GetPersonalRecordList(Config.pageSize, filter, pageIndex, strOrderBy, ref  pageCount);
        }
    }
}
