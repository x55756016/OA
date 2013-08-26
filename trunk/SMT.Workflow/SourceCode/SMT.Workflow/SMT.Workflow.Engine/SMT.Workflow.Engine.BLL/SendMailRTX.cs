/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：SendMailRTX.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/23 15:54:42   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.BLL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using SMT.Workflow.Engine.DAL;
using System.IO;
using System.Net.Mail;

namespace SMT.Workflow.Engine.BLL
{
    public static class SendMailRTX
    {
        private static string Templete = string.Empty;
        //private static RTXWCF.RTXServiceClient client = new RTXWCF.RTXServiceClient();
        private static SMTIM.MessageServiceClient imClient = new SMTIM.MessageServiceClient();
        private static readonly object lockObject = new object();
        /// <summary>
        /// 发送待办任务
        /// </summary>
        public static void SendDoTask()
        {
            try
            {
                lock (lockObject)
                {
                    Log.WriteLog("调用开始SendDoTask()时间：" + DateTime.Now);
                    SendMailRTXDAL dal = new SendMailRTXDAL();
                    DataTable dt = dal.GetDoTaskList();
                    foreach (DataRow dr in dt.Rows)
                    {
                        string strRTXAccount = string.Empty;
                        string strEmail = string.Empty;
                        dal.UpDateDoTaskStatus(dr["DOTASKID"].ToString());//修改待办的邮件RTX的状态改为已经发送 1
                        SendIM(dr);
                        UserInfo.GetUserInfo(dr["RECEIVEUSERID"].ToString(), ref strEmail, ref strRTXAccount);
                        SendEmail(dr, strEmail);//发送邮件
                        //SendRTX(dr, strRTXAccount);//发送RTX
                    }
                    SendDoTaskMessage();//发送待办消息
                }
            }
            catch(Exception ex)
            {
                Log.WriteLog("发送待办任务主入口SendDoTask（）方法出错" + ex.Message);
            }
           
        }

        /// <summary>
        /// 发送待办消息
        /// </summary>
        private static void SendDoTaskMessage()
        {
            try
            {
                SendMailRTXDAL dal = new SendMailRTXDAL();
                DataTable dt = dal.GetDoTaskMessageList();
                foreach (DataRow dr in dt.Rows)
                {
                    string strRTXAccount = string.Empty;
                    string strEmail = string.Empty;
                    dal.UpDateDoTaskMessageStatus(dr["DOTASKMESSAGEID"].ToString());//修改待办消息的邮件RTX的状态改为已经发送 1
                    SendIM(dr);
                    UserInfo.GetUserInfo(dr["RECEIVEUSERID"].ToString(), ref strEmail, ref strRTXAccount);
                    SendEmail(dr, strEmail);//发送邮件
                    //SendRTX(dr, strRTXAccount);//发送RTX                  
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("发送待办消息主入口SendDoTaskMessage（）方法出错" + ex.Message);
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="strEmail"></param>
        private static void SendEmail(DataRow dr, string strEmail)
        {
            try
            {
                if (!string.IsNullOrEmpty(strEmail) && dr["MAILSTATUS"].ToString() == "0")
                {
                    string Title = dr["MESSAGEBODY"].ToString();
                    string PutDate = dr["CREATEDATETIME"].ToString();
                    string BodyTemplete = GetMailBodeTempete();
                    string MsgUrl = Config.MailUrl;
                    PutDate = Convert.ToDateTime(PutDate).ToString("yyyy年MM月dd日");
                    BodyTemplete = string.Format(BodyTemplete, MsgUrl, Title, PutDate);

                    MailAddress sendAddress = new MailAddress(Config.MailAddress);
                    MailAddress receiveAddress = new MailAddress(strEmail);
                    MailMessage message = new MailMessage(sendAddress, receiveAddress);
                    message.Subject = string.Format(Config.MailTitle, dr["MESSAGEBODY"].ToString());
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
                string cMessage = "发送邮件报错:";
                cMessage += "SendMail:" + string.Concat(Config.MailAddress) + "\r\n";
                cMessage += "ReceiveMail:" + string.Concat(strEmail) + "\r\n";
                cMessage += "ReceiveUserID:" + string.Concat((dr["RECEIVEUSERID"]!=null?dr["RECEIVEUSERID"].ToString():"")) + "\r\n";
                
                Log.WriteLog(cMessage + ex.Message);
            }
        }


        private static void SendIM(DataRow dr)
        {
            if (dr["RECEIVEUSERID"] != null && !string.IsNullOrEmpty(dr["RECEIVEUSERID"].ToString()))
            {
                try
                {
                    string strBody = dr["MESSAGEBODY"].ToString();
                    SMTIM.MessageDataObject dataObject = new SMTIM.MessageDataObject();
                    dataObject.Title = dr["SYSTEMCODE"].ToString();
                    dataObject.Msg = strBody;
                    dataObject.Type = 0;
                    dataObject.TargetList = dr["RECEIVEUSERID"].ToString(); //接收人的ID
                    dataObject.TargetType = "2";
                    dataObject.Url = "";
                    dataObject.Remark = strBody;
                    imClient.SendMessage(dataObject);
                }
                catch (Exception ex)
                {
                    string cMessage = "发送即时消息报错:SendIM：" + dr["RECEIVEUSERID"];
                    Log.WriteLog(cMessage + ex.Message);
                }
            }
        }
        /// <summary>
        /// 发送RTX
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="strRTXAccount"></param>
        private static void SendRTX(DataRow dr, string strRTXAccount)
        {
            if (!string.IsNullOrEmpty(strRTXAccount) && dr["RTXSTATUS"].ToString() == "0")
            {
                try
                {
                    string strBody = dr["MESSAGEBODY"].ToString();
                    //string strTitle = dr["APPLICATIONCODE"].ToString();
                    //client.RTXSendMsg(strRTXAccount, Config.RTXTitle, strBody);
                }
                catch (Exception ex)
                {
                    string cMessage = "发送TRX报错:strRTXAccount" + strRTXAccount;
                    Log.WriteLog(cMessage + ex.Message);
                }
            }

        }

        /// <summary>
        /// zhua
        /// </summary>
        /// <param name="strLine">strLine</param>
        /// <returns>string</returns>
        private static string Utf8(string strLine)
        {
            byte[] utf8byte = Encoding.Unicode.GetBytes(strLine);
            string strOutLine = Encoding.UTF8.GetString(utf8byte);
            return strLine;
        }

        /// <summary>
        /// 模版
        /// </summary>
        /// <returns>string</returns>
        private static string GetMailBodeTempete()
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
    }
}
