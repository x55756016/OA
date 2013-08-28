using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model.FlowEngine;
using System.Data.OracleClient;

namespace SMT.Workflow.Platform.BLL
{
    public class TimingTriggerBLL
    {


        TimingTriggerDAL dal = new TimingTriggerDAL();

        /// <summary>
        ///
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="strFilter"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_WF_TIMINGTRIGGERACTIVITY> GetTimingTriggerList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
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
                return dal.GetTimingTriggerList(pageSize, pageIndex, strFilter, strOrderBy, ref  pageCount);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void AddTimingActivity(T_WF_TIMINGTRIGGERACTIVITY model)
        {
            try
            {

                string insSql = "INSERT INTO T_WF_TIMINGTRIGGERACTIVITY (TRIGGERNAME,COMPANYID,SYSTEMCODE,SYSTEMNAME,MODELCODE,MODELNAME,TRIGGERACTIVITYTYPE,TRIGGERTIME,TRIGGERROUND,WCFURL,FUNCTIONNAME,FUNCTIONPARAMTER,PAMETERSPLITCHAR,WCFBINDINGCONTRACT,RECEIVERUSERID,RECEIVEROLE,RECEIVERNAME,MESSAGEBODY,MESSAGEURL,TRIGGERSTATUS,TRIGGERTYPE,TRIGGERDESCRIPTION,CONTRACTTYPE,CREATEDATETIME,CREATEUSERID,CREATEUSERNAME,REMARK,TRIGGERID) VALUES (:TRIGGERNAME,:COMPANYID,:SYSTEMCODE,:SYSTEMNAME,:MODELCODE,:MODELNAME,:TRIGGERACTIVITYTYPE,:TRIGGERTIME,:TRIGGERROUND,:WCFURL,:FUNCTIONNAME,:FUNCTIONPARAMTER,:PAMETERSPLITCHAR,:WCFBINDINGCONTRACT,:RECEIVERUSERID,:RECEIVEROLE,:RECEIVERNAME,:MESSAGEBODY,:MESSAGEURL,:TRIGGERSTATUS,:TRIGGERTYPE,:TRIGGERDESCRIPTION,:CONTRACTTYPE,:CREATEDATETIME,:CREATEUSERID,:CREATEUSERNAME,:REMARK,:TRIGGERID)";
                OracleParameter[] pageparm =
                {  
                    new OracleParameter(":TRIGGERNAME",dal.GetValue(model.TRIGGERNAME)), //定时触发名称 
                    new OracleParameter(":COMPANYID",dal.GetValue(model.COMPANYID)), //公司ID 
                    new OracleParameter(":SYSTEMCODE",dal.GetValue(model.SYSTEMCODE)), //系统代码 
                    new OracleParameter(":SYSTEMNAME",dal.GetValue(model.SYSTEMNAME)), //系统名称 
                    new OracleParameter(":MODELCODE",dal.GetValue(model.MODELCODE)), //模块代码 
                    new OracleParameter(":MODELNAME",dal.GetValue(model.MODELNAME)), //模块名称 
                    new OracleParameter(":TRIGGERACTIVITYTYPE",dal.GetValue(model.TRIGGERACTIVITYTYPE)), //触发活动类型（1、短信活动 2、服务活动） 
                    new OracleParameter(":TRIGGERTIME",dal.GetValue(model.TRIGGERTIME)), //触发时间 
                    new OracleParameter(":TRIGGERROUND",dal.GetValue(model.TRIGGERROUND)), //触发周期 0 只触发一次 1 分钟 2小时 3 天 4 月 5年 6周 7未知 
                    new OracleParameter(":WCFURL",dal.GetValue(model.WCFURL)), //WCF的URL 
                    new OracleParameter(":FUNCTIONNAME",dal.GetValue(model.FUNCTIONNAME)), //所调方法名称 
                    new OracleParameter(":FUNCTIONPARAMTER",dal.GetValue(model.FUNCTIONPARAMTER)), //方法参数 
                    new OracleParameter(":PAMETERSPLITCHAR",dal.GetValue(model.PAMETERSPLITCHAR)), //参数分解符 
                    new OracleParameter(":WCFBINDINGCONTRACT",dal.GetValue(model.WCFBINDINGCONTRACT)), //WCF绑定的契约 
                    new OracleParameter(":RECEIVERUSERID",dal.GetValue(model.RECEIVERUSERID)), //接收人ID 
                    new OracleParameter(":RECEIVEROLE",dal.GetValue(model.RECEIVEROLE)), //接受人角色 
                    new OracleParameter(":RECEIVERNAME",dal.GetValue(model.RECEIVERNAME)), //接收人名称 
                    new OracleParameter(":MESSAGEBODY",dal.GetValue(model.MESSAGEBODY)), //接受消息 
                    new OracleParameter(":MESSAGEURL",dal.GetValue(model.MESSAGEURL)), //消息链接 
                    new OracleParameter(":TRIGGERSTATUS",dal.GetValue(model.TRIGGERSTATUS)), //触发器状态 
                    new OracleParameter(":TRIGGERTYPE",dal.GetValue(model.TRIGGERTYPE)), //触发类型 
                    new OracleParameter(":TRIGGERDESCRIPTION",dal.GetValue(model.TRIGGERDESCRIPTION)), //触发描述 
                    new OracleParameter(":CONTRACTTYPE",dal.GetValue(model.CONTRACTTYPE)), //接口类型（引擎，定时接口） 
                    new OracleParameter(":CREATEDATETIME",dal.GetValue(DateTime.Now)), //创建日期 
                    new OracleParameter(":CREATEUSERID",dal.GetValue(model.CREATEUSERID)), //创建人ID 
                    new OracleParameter(":CREATEUSERNAME",dal.GetValue(model.CREATEUSERNAME)), //创建人 
                    new OracleParameter(":REMARK",dal.GetValue(model.REMARK)), //备注 
                    new OracleParameter(":TRIGGERID",dal.GetValue(model.TRIGGERID)) //触发ID 

                };
                dal.ExecuteSql(insSql, pageparm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public void EditTimingActivity(T_WF_TIMINGTRIGGERACTIVITY model)
        {
            try
            {

                string updSql = "UPDATE T_WF_TIMINGTRIGGERACTIVITY SET TRIGGERNAME=:TRIGGERNAME,COMPANYID=:COMPANYID,SYSTEMCODE=:SYSTEMCODE,SYSTEMNAME=:SYSTEMNAME,MODELCODE=:MODELCODE,MODELNAME=:MODELNAME,TRIGGERACTIVITYTYPE=:TRIGGERACTIVITYTYPE,TRIGGERTIME=:TRIGGERTIME,TRIGGERROUND=:TRIGGERROUND,WCFURL=:WCFURL,FUNCTIONNAME=:FUNCTIONNAME,FUNCTIONPARAMTER=:FUNCTIONPARAMTER,PAMETERSPLITCHAR=:PAMETERSPLITCHAR,WCFBINDINGCONTRACT=:WCFBINDINGCONTRACT,MESSAGEBODY=:MESSAGEBODY,MESSAGEURL=:MESSAGEURL,TRIGGERTYPE=:TRIGGERTYPE,TRIGGERDESCRIPTION=:TRIGGERDESCRIPTION,REMARK=:REMARK WHERE   TRIGGERID=:TRIGGERID";
                OracleParameter[] pageparm =
                { 
                    new OracleParameter(":TRIGGERNAME",dal.GetValue(model.TRIGGERNAME)), //定时触发名称 
                    new OracleParameter(":COMPANYID",dal.GetValue(model.COMPANYID)), //公司ID 
                    new OracleParameter(":SYSTEMCODE",dal.GetValue(model.SYSTEMCODE)), //系统代码 
                    new OracleParameter(":SYSTEMNAME",dal.GetValue(model.SYSTEMNAME)), //系统名称 
                    new OracleParameter(":MODELCODE",dal.GetValue(model.MODELCODE)), //模块代码 
                    new OracleParameter(":MODELNAME",dal.GetValue(model.MODELNAME)), //模块名称 
                    new OracleParameter(":TRIGGERACTIVITYTYPE",dal.GetValue(model.TRIGGERACTIVITYTYPE)), //触发活动类型（1、短信活动 2、服务活动） 
                    new OracleParameter(":TRIGGERTIME",dal.GetValue(model.TRIGGERTIME)), //触发时间 
                    new OracleParameter(":TRIGGERROUND",dal.GetValue(model.TRIGGERROUND)), //触发周期 0 只触发一次 1 分钟 2小时 3 天 4 月 5年 6周 7未知 
                    new OracleParameter(":WCFURL",dal.GetValue(model.WCFURL)), //WCF的URL 
                    new OracleParameter(":FUNCTIONNAME",dal.GetValue(model.FUNCTIONNAME)), //所调方法名称 
                    new OracleParameter(":FUNCTIONPARAMTER",dal.GetValue(model.FUNCTIONPARAMTER)), //方法参数 
                    new OracleParameter(":PAMETERSPLITCHAR",dal.GetValue(model.PAMETERSPLITCHAR)), //参数分解符 
                    new OracleParameter(":WCFBINDINGCONTRACT",dal.GetValue(model.WCFBINDINGCONTRACT)), //WCF绑定的契约 
                    new OracleParameter(":MESSAGEBODY",dal.GetValue(model.MESSAGEBODY)), //接受消息 
                    new OracleParameter(":MESSAGEURL",dal.GetValue(model.MESSAGEURL)), //消息链接 
                    new OracleParameter(":TRIGGERTYPE",dal.GetValue(model.TRIGGERTYPE)), //触发类型 
                    new OracleParameter(":TRIGGERDESCRIPTION",dal.GetValue(model.TRIGGERDESCRIPTION)), //触发描述 
                    new OracleParameter(":REMARK",dal.GetValue(model.REMARK)), //备注 
                    new OracleParameter(":TRIGGERID",dal.GetValue(model.TRIGGERID)) //触发ID 
                };
                dal.ExecuteSql(updSql, pageparm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void DeleteTimingActivity(string TimingID)
        {
            try
            {


                string sql = "delete from T_WF_TIMINGTRIGGERACTIVITY where TRIGGERID='" + TimingID + "'";
                dal.ExecuteSql(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

    }
}
