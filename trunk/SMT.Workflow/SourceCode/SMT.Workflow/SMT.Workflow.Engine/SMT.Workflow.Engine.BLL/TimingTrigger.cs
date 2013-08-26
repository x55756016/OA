/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：TimingTrigger.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/23 14:32:24   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.BLL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Engine.DAL;
using System.Data;
using System.Collections;


namespace SMT.Workflow.Engine.BLL
{
    public static class TimingTrigger
    {

        public static TimingTriggerDAL dal = new TimingTriggerDAL();
        public static SMSService.SMSServiceClient client = new SMSService.SMSServiceClient();
        public static PersonnelService.PersonnelServiceClient Client = new PersonnelService.PersonnelServiceClient();
        private static readonly object lockTimingObject = new object();
        /// <summary>
        /// 定时触发获取主入口
        /// </summary>
        public static void TimingTriggerActivity()
        {

            try
            {
                lock (lockTimingObject)
                {
                    Log.WriteLog("调用开始TimingTriggerActivity()时间：" + DateTime.Now);
                    DataTable dt = dal.GetTimingTriggerList();//获取到时间改触发的数据
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            int multiple = 1;
                            try
                            {
                                multiple = (int)dr["TRIGGERMULTIPLE"];
                            }
                            catch
                            {
                                multiple = 1;
                            }
                            // 0 只触发一次 1 分钟 2小时 3 天 4 月 5年 6周 7未知
                            switch (dr["TRIGGERROUND"].ToString())
                            {
                                case "0"://只触发一次
                                    dal.DeleteTrigger(dr["TRIGGERID"].ToString());//删除只触发一次的数据                                  
                                    break;
                                case "1"://分钟2011/12/4
                                    string EditMinutes = DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddMinutes(1 * multiple).ToString("yyyy/MM/dd HH:mm");
                                    if (DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddMinutes(1 * multiple) < DateTime.Now)
                                    {
                                        EditMinutes = DateTime.Now.AddMinutes(1 * multiple).ToString("yyyy/MM/dd HH:mm");
                                    }
                                    dal.UpdateTriggerDate(dr["TRIGGERID"].ToString(), EditMinutes);
                                    break;
                                case "2"://小时
                                    string EditHours = DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddHours(1 * multiple).ToString("yyyy/MM/dd HH:mm");
                                    if (DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddHours(1 * multiple) < DateTime.Now)
                                    {
                                        EditHours = DateTime.Now.AddHours(1 * multiple).ToString("yyyy/MM/dd HH") + EditHours.Substring(13);
                                    }                                  
                                    dal.UpdateTriggerDate(dr["TRIGGERID"].ToString(), EditHours);                                
                                    break;
                                case "3"://天
                                    string EditDate = DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddDays(1 * multiple).ToString();
                                    if (DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddDays(1 * multiple) < DateTime.Now)//处理N多天服务都没运行的时间，修改成当前周期的下一周期
                                    {
                                        EditDate = DateTime.Now.AddDays(1 * multiple).ToString("yyyy/MM/dd") + " " + DateTime.Parse(dr["TRIGGERTIME"].ToString()).ToString("HH:mm");
                                    }                                  
                                    dal.UpdateTriggerDate(dr["TRIGGERID"].ToString(), EditDate);                                  
                                    break;
                                case "4"://月
                                    string EditMoth = DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddMonths(1 * multiple).ToString("yyyy/MM/dd HH:mm");
                                    if (DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddMonths(1 * multiple) < DateTime.Now)//处理N个月服务都没运行的时间，修改成当前周期的下一周期
                                    {
                                        EditMoth = DateTime.Now.AddMonths(1 * multiple).ToString("yyyy/MM") + EditMoth.Substring(7);
                                    }                                 
                                    dal.UpdateTriggerDate(dr["TRIGGERID"].ToString(), EditMoth);
                                    break;
                                case "5"://年
                                    dal.UpdateTriggerDate(dr["TRIGGERID"].ToString(), DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddYears(1 * multiple).ToString());
                                    break;
                                case "6"://周                                  
                                    dal.UpdateTriggerDate(dr["TRIGGERID"].ToString(), DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddDays(7 * multiple).ToString());
                                    break;
                                case "7"://季度                                
                                    dal.UpdateTriggerDate(dr["TRIGGERID"].ToString(), DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddMonths(3 * multiple).ToString());
                                    break;
                                case "8"://半年                                
                                    dal.UpdateTriggerDate(dr["TRIGGERID"].ToString(), DateTime.Parse(dr["TRIGGERTIME"].ToString()).AddMonths(6 * multiple).ToString());
                                    break;
                            }
                            if (dr["WCFURL"].ToString().IndexOf(':') > 1) //处理垃圾数据直接删除http://portal.smt-online.net/New/Services/HR/EngineTriggerService.svc
                            {
                                dal.DeleteTrigger(dr["TRIGGERID"].ToString());//处理垃圾                           
                                dal.AddTimingRecord(dr, "删除的垃圾数据");
                                continue;
                            }
                            switch (dr["TRIGGERACTIVITYTYPE"].ToString())
                            {
                                case "3"://发送短信提醒
                                    SendSMS(dr);
                                    break;
                                case "1": //发送代办提醒
                                    SendDoTaskSMS();
                                    break;
                                case "2": //触发服务定时
                                    DataTable sourceTable = FieldStringToDataTable(dr);                                 
                                    CallWCFService(dr, sourceTable);
                                    dal.AddTimingRecord(dr, "服务发送");//添加一条服务的发送记录
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("TimingTriggerActivity()方法出现错误" + ex.Message);
            }
        }        
        private static void SendSMS(DataRow dr)
        {
            try
            {
                if (dr["RECEIVERUSERID"] != null&&dr["MESSAGEBODY"]!=null)
                {
                    SMSService.MessageEntity[] entity = new SMSService.MessageEntity[1];
                    entity[0] = new SMSService.MessageEntity();
                    entity[0].ACCOUNTID = Config.Account;
                    entity[0].MOBILE = dr["RECEIVERUSERID"].ToString();
                    entity[0].SENDMESSAGE = dr["MESSAGEBODY"].ToString();
                    entity[0].SENDTIME = DateTime.Now;
                    entity[0].REMARK = "新平台短信提醒";
                    entity[0].OWNERID = "";
                    entity[0].OWNERNAME = dr["RECEIVERNAME"].ToString();
                    entity[0].OWNERCOMPANYID = "";
                    entity[0].OWNERDEPARTMENTID = "";
                    entity[0].OWNERPOSTID = "";
                    entity[0].CREATEUSERID = "";
                    entity[0].CREATEUSERNAME = "系统发送短信提醒";
                    entity[0].CREATECOMPANYID = "bac05c76-0f5b-40ae-b73b-8be541ed35ed";
                    entity[0].CREATEDEPARTMENTID = "";
                    entity[0].CREATEPOSTID = "";
                    entity[0].CREATEDATE = DateTime.Now;
                    entity[0].SMSTYPE = 0;
                    T_FLOW_SMSRECORD ent = new T_FLOW_SMSRECORD();
                    ent.SMSRECORD = Guid.NewGuid().ToString();
                    ent.SENDTIME = DateTime.Now;
                    ent.MOBILE = dr["RECEIVERUSERID"].ToString();
                    ent.COMPANYID = "系统发送短信提醒";
                    ent.BATCHNUMBER = string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddHHmm"), entity.Length);
                    ent.ACCOUNT = Config.Account;
                    ent.OWNERNAME = dr["RECEIVERNAME"].ToString();
                    ent.SENDSTATUS = 1;//已发送
                    ent.OWNERID = "";
                    ent.TASKCOUNT = 1;
                    ent.SENDMESSAGE = dr["MESSAGEBODY"].ToString();
                    dal.AddSMSRecord(ent);//插入发送记录
                    try
                    {                    
                        //调用短信接口服务
                        string strMsg = client.SendMsg(entity);//调试不发送
                        Log.WriteLog("调用短信接口结束,返回信息SendSMS():" + strMsg +  dr["RECEIVERNAME"].ToString());
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog("调用短信接口结束(异常消息),返回信息SendSMS():" + ex.ToString() + DateTime.Now.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("调用短信SendSMS(),返回信息:" + ex.ToString());
            }
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="dr"></param>
        private static void SendDoTaskSMS()
        {
            try
            {
                DataTable dt = dal.GetSMSDoTask();
                if (dt.Rows.Count > 0)
                {
                    Log.WriteLog("查询到得代办数据总数：" + dt.Rows.Count);
                    SMSService.MessageEntity[] entity = null;
                    string[] strUser = new string[dt.Rows.Count];
                    IDictionary<string, string> dict = new Dictionary<string, string>();
                    Hashtable has = new Hashtable();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        strUser[i] = dt.Rows[i]["RECEIVEUSERID"].ToString();
                        has.Add(dt.Rows[i]["RECEIVEUSERID"].ToString(), dt.Rows[i]["TASKCOUNT"].ToString());
                    }
                    if (strUser != null)
                    {
                        //EMPLOYEESTATE 员工状态 0：试用期、1：在职、2：离职、3：离职中 4：未入职
                        PersonnelService.EmployeeContactWays[] List = Client.GetEmployeeToEngine(strUser);
                        var employeeOnDuty = from emp in List where emp.EMPLOYEESTATE == "1" || emp.EMPLOYEESTATE == "3" select emp;
                        if (employeeOnDuty != null && employeeOnDuty.Count() > 0)
                        {
                            Log.WriteLog("真实发送短信数据总数：" + dt.Rows.Count);
                            entity = new SMSService.MessageEntity[employeeOnDuty.Count()];
                            int i = 0;
                            foreach (var employee in employeeOnDuty)
                            {
                                string taskCount = has[employee.EMPLOYEEID].ToString();
                                string sendMsg = string.Format("温馨提醒：截止{0}您还有{1}条待办任务未处理，请及时处理，谢谢！【协同办公系统】", DateTime.Now.ToString("HH时mm分"), taskCount);

                                dict.Add(employee.EMPLOYEEID, taskCount);
                                entity[i] = new SMSService.MessageEntity();
                                entity[i].ACCOUNTID = employee.EMPLOYEEID;
                                entity[i].MOBILE = employee.TELPHONE;
                                entity[i].SENDMESSAGE = sendMsg;
                                entity[i].SENDTIME = DateTime.Now;
                                entity[i].REMARK = "引擎发送待办短信";
                                entity[i].OWNERID = employee.EMPLOYEEID;
                                entity[i].OWNERNAME = employee.EMPLOYEENAME;
                                entity[i].OWNERCOMPANYID = employee.COMPANYID;
                                entity[i].OWNERDEPARTMENTID = employee.DEPARTMENTID;
                                entity[i].OWNERPOSTID = employee.POSTID;
                                entity[i].CREATEUSERID = "";
                                entity[i].CREATEUSERNAME = "系统发送待办短信";
                                entity[i].CREATECOMPANYID = "bac05c76-0f5b-40ae-b73b-8be541ed35ed";
                                entity[i].CREATEDEPARTMENTID = employee.DEPARTMENTID;
                                entity[i].CREATEPOSTID = employee.POSTID;
                                entity[i].CREATEDATE = DateTime.Now;
                                entity[i].SMSTYPE = 0;
                                i++;
                            }
                        }

                        #region 插入短信发送的记录
                        if (entity != null)
                        {
                            for (int i = 0; i < entity.Length; i++)
                            {
                                if (entity[i] != null)
                                {
                                    string account = entity[i].ACCOUNTID;
                                    T_FLOW_SMSRECORD ent = new T_FLOW_SMSRECORD();
                                    ent.SMSRECORD = Guid.NewGuid().ToString();
                                    ent.SENDTIME = entity[i].SENDTIME;
                                    ent.MOBILE = entity[i].MOBILE;
                                    ent.COMPANYID = entity[i].CREATECOMPANYID;
                                    ent.BATCHNUMBER = string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddHHmm"), entity.Length);
                                    ent.ACCOUNT = entity[i].ACCOUNTID;
                                    ent.OWNERNAME = entity[i].OWNERNAME;
                                    ent.SENDSTATUS = 1;//已发送
                                    ent.OWNERID = entity[i].OWNERID;
                                    ent.TASKCOUNT = Convert.ToInt32(dict[account]);
                                    ent.SENDMESSAGE = entity[i].SENDMESSAGE;
                                    dal.AddSMSRecord(ent);//插入发送记录
                                }
                            }
                        }
                        #endregion
                        try
                        {
                            Log.WriteLog("调用短信接口开始：" + DateTime.Now.ToString());
                            //调用短信接口服务
                            string strMsg = client.SendMsg(entity);//调试不发送

                            Log.WriteLog("调用短信接口结束,返回信息:" + strMsg + DateTime.Now.ToString());
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog("调用短信接口结束(异常消息),返回信息:" + ex.ToString() + DateTime.Now.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("调用短信SendDoTaskSMS(),返回信息:" + ex.ToString() + DateTime.Now.ToString());
            }
        }
        /// <summary>
        /// 调用WCF
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="sourceTable"></param>
        private static void CallWCFService(DataRow dr, DataTable sourceTable)
        {
            string WCFBinding = dr["WCFBINDINGCONTRACT"].ToString();
            string WCFUrl = dr["WCFURL"].ToString();
            string WCFFunctionName = dr["FUNCTIONNAME"].ToString();
            string WCFPameter = dr["FUNCTIONPARAMTER"].ToString();
            string WCFSplitChar = dr["PAMETERSPLITCHAR"].ToString();
            WCFPameter = PameterValue(WCFPameter, sourceTable);
            object TmpreceiveString = new object();
            try
            {
                WCFUrl = Config.GetSystemCode(dr["SYSTEMCODE"].ToString()) + WCFUrl;
                if (!string.IsNullOrEmpty(WCFBinding) && !string.IsNullOrEmpty(WCFUrl) && !string.IsNullOrEmpty(WCFFunctionName))
                {                 
                    TmpreceiveString = Utility.CallEventWCFService(dr["CONTRACTTYPE"].ToString(), WCFBinding, WCFUrl, WCFFunctionName, WCFPameter);                 
                    Log.WriteLog("定时触发调用WCF(正常执行)执行结果:" + TmpreceiveString + "主键ID：" + dr["TRIGGERID"]);
                }
            }
            catch (Exception e)
            {
                string cMessage = "执行链接：" + WCFUrl + "\r\n" +
                                     "执行方法:" + WCFFunctionName + "\r\n" +
                                     "绑定契约：" + WCFBinding + "\r\n" +
                                     "执行结果：" + TmpreceiveString + "\r\n" +
                                     "执行参数：" + WCFPameter + "\r\n" +
                                     "----------------------------------------------------------";
                Log.WriteLog("CallWCFService()方法出现错误||" + cMessage + "||" + e.Message + "主键ID：" + dr["TRIGGERID"]);
            }
        }

        /// <summary>
        /// 将WCF参数，根据数据源转换成具体值
        /// </summary>
        /// <param name="PorcessString">PorcessString</param>
        /// <param name="SourceValueDT">SourceValueDT</param>
        /// <returns>string</returns>
        private static string PameterValue(string PameterString, DataTable SourceValueDT)
        {
            PameterString = PameterString.Replace("{", "").Replace("}", "");
            string[] TmpCompareNode = PameterString.Split('Г');

            for (int k = 0; k < TmpCompareNode.Length; k++)
            {
                string TmpValue = TmpCompareNode[k].Trim();
                DataRow[] dr = SourceValueDT.Select("ColumnName='" + TmpValue + "'");
                if (dr != null && dr.Length > 0)
                {
                    string newValue = dr[0]["ColumnValue"].ToString();
                    PameterString = PameterString.Replace(TmpCompareNode[k].Trim(), newValue);
                }
            }
            return PameterString;
        }
        /// <summary>
        /// 将数据源字段转换成数据表
        /// </summary>
        /// <param name="dr">dr</param>
        /// <returns>DataTable</returns>
        private static DataTable FieldStringToDataTable(DataRow dr)
        {
            DataRow[] list;
            DataRow drvalue;
            DataTable valueTable = new DataTable();
            valueTable.Columns.Add("FieldType", typeof(string));
            valueTable.Columns.Add("ColumnName", typeof(string));
            valueTable.Columns.Add("ColumnValue", typeof(string));
            string TmpFieldValueString = Utility.GetColumnString(dr, "APPFIELDVALUE");
            string[] valuerownode = TmpFieldValueString.Split('Ё');
            for (int j = 0; j < valuerownode.Length; j++)
            {
                if (valuerownode[j] != "")
                {
                    string[] valuecolnode = valuerownode[j].Split('|');
                    list = valueTable.Select("ColumnName='" + valuecolnode[0] + "'");
                    if (list.Length > 0)
                    {
                        drvalue = list[0];
                    }
                    else
                    {
                        drvalue = valueTable.NewRow();
                        valueTable.Rows.Add(drvalue);
                    }
                    drvalue["FieldType"] = "sys";
                    drvalue["ColumnName"] = valuecolnode[0];
                    drvalue["ColumnValue"] = valuecolnode[1];
                }
            }
            return valueTable;
        }
    }
}
