using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model.FlowEngine;
using System.Data.OracleClient;

namespace SMT.Workflow.Platform.BLL
{
    public class DoTaskRuleBLL
    {

        DoTaskRuleDAL dal = new DoTaskRuleDAL();

        /// <summary>
        ///
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="strFilter"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_WF_DOTASKRULE> GetDoTaskList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
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
                return dal.GetDoTaskList(pageSize, pageIndex, strFilter, strOrderBy, ref  pageCount);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public T_WF_DOTASKRULE GetDoTaskRule(string RuleID)
        {
            try
            {
                string sql = "select * from T_WF_DOTASKRULE where DOTASKRULEID='" + RuleID + "' order by CREATEDATETIME desc";
                return dal.GetEntity(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public List<T_WF_DOTASKRULEDETAIL> GetDoTaskRuleDetail(string RuleID)
        {
            try
            {
                string sql = "select * from T_WF_DOTASKRULEDETAIL where DOTASKRULEID='" + RuleID + "' order by CREATEDATETIME desc";
                return dal.GetDoTaskRuleDetail(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void AddDoTaskRule(T_WF_DOTASKRULE rule)
        {
            try
            {              
                string insSql = "INSERT INTO T_WF_DOTASKRULE (DOTASKRULEID,COMPANYID,SYSTEMCODE,SYSTEMNAME,MODELCODE,MODELNAME,TRIGGERORDERSTATUS,CREATEDATETIME) VALUES (:DOTASKRULEID,:COMPANYID,:SYSTEMCODE,:SYSTEMNAME,:MODELCODE,:MODELNAME,:TRIGGERORDERSTATUS,:CREATEDATETIME)";
                OracleParameter[] pageparm =
                {               
                    new OracleParameter(":DOTASKRULEID",OracleType.NVarChar,200), 
                    new OracleParameter(":COMPANYID",OracleType.NVarChar,100), 
                    new OracleParameter(":SYSTEMCODE",OracleType.NVarChar,100), 
                    new OracleParameter(":SYSTEMNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":MODELCODE",OracleType.NVarChar,100), 
                    new OracleParameter(":MODELNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":TRIGGERORDERSTATUS",OracleType.Number,22), 
                    new OracleParameter(":CREATEDATETIME",OracleType.DateTime) 

                };
                pageparm[0].Value = GetValue(rule.DOTASKRULEID);//待办规则主表ID
                pageparm[1].Value = GetValue(rule.COMPANYID);//公司ID
                pageparm[2].Value = GetValue(rule.SYSTEMCODE);//系统代码
                pageparm[3].Value = GetValue(rule.SYSTEMNAME);//系统名称
                pageparm[4].Value = GetValue(rule.MODELCODE);//模块代码
                pageparm[5].Value = GetValue(rule.MODELNAME);//模块名称
                pageparm[6].Value = GetValue(rule.TRIGGERORDERSTATUS);//触发条件的单据状态
                pageparm[7].Value = GetValue(DateTime.Now);//创建日期时间

                dal.ExecuteSql(insSql, pageparm);
              
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public bool GetBool(T_WF_DOTASKRULE rule)
        {
            string sql = "select * from T_WF_DOTASKRULE where COMPANYID='" + rule.COMPANYID + "' and SYSTEMCODE='" + rule.SYSTEMCODE + "' and MODELCODE='" + rule.MODELCODE + "' and TRIGGERORDERSTATUS='" + rule.TRIGGERORDERSTATUS + "'";
            sql += " and  DOTASKRULEID!='" + rule.DOTASKRULEID + "'";
            if (dal.GetDataTable(sql).Rows.Count > 0)
            {
                return false;
            }
            return true;
        }
        public void AddDoTaskRuleDetail(T_WF_DOTASKRULEDETAIL Detail)
        {
            try
            {
              
                string insSql = "INSERT INTO T_WF_DOTASKRULEDETAIL (DOTASKRULEDETAILID,DOTASKRULEID,COMPANYID,SYSTEMCODE,SYSTEMNAME,MODELCODE,MODELNAME,WCFURL,FUNCTIONNAME,FUNCTIONPARAMTER,PAMETERSPLITCHAR,WCFBINDINGCONTRACT,MESSAGEBODY,LASTDAYS,APPLICATIONURL,RECEIVEUSER,RECEIVEUSERNAME,OWNERCOMPANYID,OWNERDEPARTMENTID,OWNERPOSTID,ISDEFAULTMSG,PROCESSFUNCLANGUAGE,ISOTHERSOURCE,OTHERSYSTEMCODE,OTHERMODELCODE,CREATEUSERNAME,CREATEUSERID,REMARK) VALUES (:DOTASKRULEDETAILID,:DOTASKRULEID,:COMPANYID,:SYSTEMCODE,:SYSTEMNAME,:MODELCODE,:MODELNAME,:WCFURL,:FUNCTIONNAME,:FUNCTIONPARAMTER,:PAMETERSPLITCHAR,:WCFBINDINGCONTRACT,:MESSAGEBODY,:LASTDAYS,:APPLICATIONURL,:RECEIVEUSER,:RECEIVEUSERNAME,:OWNERCOMPANYID,:OWNERDEPARTMENTID,:OWNERPOSTID,:ISDEFAULTMSG,:PROCESSFUNCLANGUAGE,:ISOTHERSOURCE,:OTHERSYSTEMCODE,:OTHERMODELCODE,:CREATEUSERNAME,:CREATEUSERID,:REMARK)";
                OracleParameter[] pageparm =
                {               
                    new OracleParameter(":DOTASKRULEDETAILID",OracleType.NVarChar,200), 
                    new OracleParameter(":DOTASKRULEID",OracleType.NVarChar,200), 
                    new OracleParameter(":COMPANYID",OracleType.NVarChar,100), 
                    new OracleParameter(":SYSTEMCODE",OracleType.NVarChar,100), 
                    new OracleParameter(":SYSTEMNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":MODELCODE",OracleType.NVarChar,100), 
                    new OracleParameter(":MODELNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":WCFURL",OracleType.NVarChar,400), 
                    new OracleParameter(":FUNCTIONNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":FUNCTIONPARAMTER",OracleType.NVarChar,4000), 
                    new OracleParameter(":PAMETERSPLITCHAR",OracleType.NVarChar,200), 
                    new OracleParameter(":WCFBINDINGCONTRACT",OracleType.NVarChar,200), 
                    new OracleParameter(":MESSAGEBODY",OracleType.NVarChar,800), 
                    new OracleParameter(":LASTDAYS",OracleType.Number,22), 
                    new OracleParameter(":APPLICATIONURL",OracleType.NClob), 
                    new OracleParameter(":RECEIVEUSER",OracleType.NVarChar,200), 
                    new OracleParameter(":RECEIVEUSERNAME",OracleType.NVarChar,200), 
                    new OracleParameter(":OWNERCOMPANYID",OracleType.NVarChar,200), 
                    new OracleParameter(":OWNERDEPARTMENTID",OracleType.NVarChar,200), 
                    new OracleParameter(":OWNERPOSTID",OracleType.NVarChar,200), 
                    new OracleParameter(":ISDEFAULTMSG",OracleType.Number,22), 
                    new OracleParameter(":PROCESSFUNCLANGUAGE",OracleType.NVarChar,200), 
                    new OracleParameter(":ISOTHERSOURCE",OracleType.NVarChar,200), 
                    new OracleParameter(":OTHERSYSTEMCODE",OracleType.NVarChar,200), 
                    new OracleParameter(":OTHERMODELCODE",OracleType.NVarChar,200), 
                    new OracleParameter(":CREATEUSERNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":CREATEUSERID",OracleType.NVarChar,100), 
                    new OracleParameter(":REMARK",OracleType.NVarChar,400) 

                };
                pageparm[0].Value = GetValue(Detail.DOTASKRULEDETAILID);//待办规则明细ID
                pageparm[1].Value = GetValue(Detail.DOTASKRULEID);//待办规则主表ID
                pageparm[2].Value = GetValue(Detail.COMPANYID);//公司ID
                pageparm[3].Value = GetValue(Detail.SYSTEMCODE);//系统代码
                pageparm[4].Value = GetValue(Detail.SYSTEMNAME);//系统名称
                pageparm[5].Value = GetValue(Detail.MODELCODE);//模块代码
                pageparm[6].Value = GetValue(Detail.MODELNAME);//模块名称
                pageparm[7].Value = GetValue(Detail.WCFURL);//WCF的URL
                pageparm[8].Value = GetValue(Detail.FUNCTIONNAME);//所调方法名称
                pageparm[9].Value = GetValue(Detail.FUNCTIONPARAMTER);//方法参数
                pageparm[10].Value = GetValue(Detail.PAMETERSPLITCHAR);//参数分解符
                pageparm[11].Value = GetValue(Detail.WCFBINDINGCONTRACT);//WCF绑定的契约
                pageparm[12].Value = GetValue(Detail.MESSAGEBODY);//消息体
                pageparm[13].Value = GetValue(Detail.LASTDAYS);//可处理日期（剩余天数）
                pageparm[14].Value = GetValue(Detail.APPLICATIONURL);//应用URL
                pageparm[15].Value = GetValue(Detail.RECEIVEUSER);//接收用户
                pageparm[16].Value = GetValue(Detail.RECEIVEUSERNAME);//接收用户名
                pageparm[17].Value = GetValue(Detail.OWNERCOMPANYID);//所属公司ID
                pageparm[18].Value = GetValue(Detail.OWNERDEPARTMENTID);//所属部门ID
                pageparm[19].Value = GetValue(Detail.OWNERPOSTID);//所属岗位ID
                pageparm[20].Value = GetValue(Detail.ISDEFAULTMSG);//是否缺省消息
                pageparm[21].Value = GetValue(Detail.PROCESSFUNCLANGUAGE);//处理功能语言
                pageparm[22].Value = GetValue(Detail.ISOTHERSOURCE);//是否其它来源
                pageparm[23].Value = GetValue(Detail.OTHERSYSTEMCODE);//其它系统代码
                pageparm[24].Value = GetValue(Detail.OTHERMODELCODE);//其它系统模块
                pageparm[25].Value = GetValue(Detail.CREATEUSERNAME);//创建人
                pageparm[26].Value = GetValue(Detail.CREATEUSERID);//创建人ID
                pageparm[27].Value = GetValue(Detail.REMARK);//备注
                dal.ExecuteSql(insSql, pageparm);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public void EditDoTaskRuleDetail(T_WF_DOTASKRULEDETAIL Detail)
        {
            try
            {
                //string sql = "Update T_WF_DOTASKRULEDETAIL set DOTASKRULEID='" + Detail.DOTASKRULEID + "',COMPANYID='" + Detail.COMPANYID + "',"
                //        + " SYSTEMCODE='" + Detail.SYSTEMCODE + "',SYSTEMNAME='" + Detail.SYSTEMNAME + "',MODELCODE='" + Detail.MODELCODE + "',MODELNAME='" + Detail.MODELNAME + "',"
                //        + " WCFURL='" + Detail.WCFURL + "',FUNCTIONNAME='" + Detail.FUNCTIONNAME + "',FUNCTIONPARAMTER='" + Detail.FUNCTIONPARAMTER + "',PAMETERSPLITCHAR='" + Detail.PAMETERSPLITCHAR + "',"
                //        + " WCFBINDINGCONTRACT='" + Detail.WCFBINDINGCONTRACT + "',MESSAGEBODY='" + Detail.MESSAGEBODY + "',LASTDAYS='" + Detail.LASTDAYS + "',APPLICATIONURL='" + Detail.APPLICATIONURL + "',"
                //        + " RECEIVEUSER='" + Detail.RECEIVEUSER + "',RECEIVEUSERNAME='" + Detail.RECEIVEUSERNAME + "',OWNERCOMPANYID='" + Detail.OWNERCOMPANYID + "',OWNERDEPARTMENTID='" + Detail.OWNERDEPARTMENTID + "',"
                //        + " OWNERPOSTID='" + Detail.OWNERPOSTID + "',ISDEFAULTMSG='" + Detail.ISDEFAULTMSG + "',PROCESSFUNCLANGUAGE='" + Detail.PROCESSFUNCLANGUAGE + "',ISOTHERSOURCE='" + Detail.ISOTHERSOURCE + "',"
                //        + " OTHERSYSTEMCODE='" + Detail.OTHERSYSTEMCODE + "', OTHERMODELCODE='" + Detail.OTHERMODELCODE + "',REMARK='" + Detail.REMARK + "' where DOTASKRULEDETAILID ='" + Detail.DOTASKRULEDETAILID + "'";


                //dal.ExecuteSql(sql);

                string updSql = "UPDATE T_WF_DOTASKRULEDETAIL SET DOTASKRULEID=:DOTASKRULEID,COMPANYID=:COMPANYID,SYSTEMCODE=:SYSTEMCODE,SYSTEMNAME=:SYSTEMNAME,MODELCODE=:MODELCODE,MODELNAME=:MODELNAME,WCFURL=:WCFURL,FUNCTIONNAME=:FUNCTIONNAME,FUNCTIONPARAMTER=:FUNCTIONPARAMTER,PAMETERSPLITCHAR=:PAMETERSPLITCHAR,WCFBINDINGCONTRACT=:WCFBINDINGCONTRACT,MESSAGEBODY=:MESSAGEBODY,LASTDAYS=:LASTDAYS,APPLICATIONURL=:APPLICATIONURL,RECEIVEUSER=:RECEIVEUSER,RECEIVEUSERNAME=:RECEIVEUSERNAME,OWNERCOMPANYID=:OWNERCOMPANYID,OWNERDEPARTMENTID=:OWNERDEPARTMENTID,OWNERPOSTID=:OWNERPOSTID,ISDEFAULTMSG=:ISDEFAULTMSG,PROCESSFUNCLANGUAGE=:PROCESSFUNCLANGUAGE,ISOTHERSOURCE=:ISOTHERSOURCE,OTHERSYSTEMCODE=:OTHERSYSTEMCODE,OTHERMODELCODE=:OTHERMODELCODE,REMARK=:REMARK WHERE   DOTASKRULEDETAILID=:DOTASKRULEDETAILID";
                OracleParameter[] pageparm =
                {               
                    new OracleParameter(":DOTASKRULEDETAILID",OracleType.NVarChar,200), 
                    new OracleParameter(":DOTASKRULEID",OracleType.NVarChar,200), 
                    new OracleParameter(":COMPANYID",OracleType.NVarChar,100), 
                    new OracleParameter(":SYSTEMCODE",OracleType.NVarChar,100), 
                    new OracleParameter(":SYSTEMNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":MODELCODE",OracleType.NVarChar,100), 
                    new OracleParameter(":MODELNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":WCFURL",OracleType.NVarChar,400), 
                    new OracleParameter(":FUNCTIONNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":FUNCTIONPARAMTER",OracleType.NVarChar,4000), 
                    new OracleParameter(":PAMETERSPLITCHAR",OracleType.NVarChar,200), 
                    new OracleParameter(":WCFBINDINGCONTRACT",OracleType.NVarChar,200), 
                    new OracleParameter(":MESSAGEBODY",OracleType.NVarChar,800), 
                    new OracleParameter(":LASTDAYS",OracleType.Number,22), 
                    new OracleParameter(":APPLICATIONURL",OracleType.NClob), 
                    new OracleParameter(":RECEIVEUSER",OracleType.NVarChar,200), 
                    new OracleParameter(":RECEIVEUSERNAME",OracleType.NVarChar,200), 
                    new OracleParameter(":OWNERCOMPANYID",OracleType.NVarChar,200), 
                    new OracleParameter(":OWNERDEPARTMENTID",OracleType.NVarChar,200), 
                    new OracleParameter(":OWNERPOSTID",OracleType.NVarChar,200), 
                    new OracleParameter(":ISDEFAULTMSG",OracleType.Number,22), 
                    new OracleParameter(":PROCESSFUNCLANGUAGE",OracleType.NVarChar,200), 
                    new OracleParameter(":ISOTHERSOURCE",OracleType.NVarChar,200), 
                    new OracleParameter(":OTHERSYSTEMCODE",OracleType.NVarChar,200), 
                    new OracleParameter(":OTHERMODELCODE",OracleType.NVarChar,200), 
                    new OracleParameter(":REMARK",OracleType.NVarChar,400) 

                };
                pageparm[0].Value = GetValue(Detail.DOTASKRULEDETAILID);//待办规则明细ID
                pageparm[1].Value = GetValue(Detail.DOTASKRULEID);//待办规则主表ID
                pageparm[2].Value = GetValue(Detail.COMPANYID);//公司ID
                pageparm[3].Value = GetValue(Detail.SYSTEMCODE);//系统代码
                pageparm[4].Value = GetValue(Detail.SYSTEMNAME);//系统名称
                pageparm[5].Value = GetValue(Detail.MODELCODE);//模块代码
                pageparm[6].Value = GetValue(Detail.MODELNAME);//模块名称
                pageparm[7].Value = GetValue(Detail.WCFURL);//WCF的URL
                pageparm[8].Value = GetValue(Detail.FUNCTIONNAME);//所调方法名称
                pageparm[9].Value = GetValue(Detail.FUNCTIONPARAMTER);//方法参数
                pageparm[10].Value = GetValue(Detail.PAMETERSPLITCHAR);//参数分解符
                pageparm[11].Value = GetValue(Detail.WCFBINDINGCONTRACT);//WCF绑定的契约
                pageparm[12].Value = GetValue(Detail.MESSAGEBODY);//消息体
                pageparm[13].Value = GetValue(Detail.LASTDAYS);//可处理日期（剩余天数）
                pageparm[14].Value = string.IsNullOrEmpty(Detail.APPLICATIONURL) ? " " : GetValue(Detail.APPLICATIONURL);//应用URL
                pageparm[15].Value = GetValue(Detail.RECEIVEUSER);//接收用户
                pageparm[16].Value = GetValue(Detail.RECEIVEUSERNAME);//接收用户名
                pageparm[17].Value = GetValue(Detail.OWNERCOMPANYID);//所属公司ID
                pageparm[18].Value = GetValue(Detail.OWNERDEPARTMENTID);//所属部门ID
                pageparm[19].Value = GetValue(Detail.OWNERPOSTID);//所属岗位ID
                pageparm[20].Value = GetValue(Detail.ISDEFAULTMSG);//是否缺省消息
                pageparm[21].Value = GetValue(Detail.PROCESSFUNCLANGUAGE);//处理功能语言
                pageparm[22].Value = GetValue(Detail.ISOTHERSOURCE);//是否其它来源
                pageparm[23].Value = GetValue(Detail.OTHERSYSTEMCODE);//其它系统代码
                pageparm[24].Value = GetValue(Detail.OTHERMODELCODE);//其它系统模块
                pageparm[25].Value = GetValue(Detail.REMARK);//备注
                dal.ExecuteSql(updSql, pageparm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public void DeleteDoTaskRuleDetail(string DetailID)
        {
            try
            {

                string sql = "delete T_WF_DOTASKRULEDETAIL where DOTASKRULEDETAILID='" + DetailID + "'";
                dal.ExecuteSql(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public void EditDoTaskRule(T_WF_DOTASKRULE rule)
        {
            try
            {

                //string sql = " update  T_WF_DOTASKRULE set COMPANYID='" + rule.COMPANYID + "',SYSTEMCODE='" + rule.SYSTEMCODE + "',SYSTEMNAME='" + rule.SYSTEMNAME + "',";
                //sql += "MODELCODE='" + rule.MODELCODE + "',MODELNAME='" + rule.MODELNAME + "',TRIGGERORDERSTATUS=" + rule.TRIGGERORDERSTATUS + "";
                //sql += " where DOTASKRULEID='" + rule.DOTASKRULEID + "'";
                //dal.ExecuteSql(sql);

                string updSql = "UPDATE T_WF_DOTASKRULE SET COMPANYID=:COMPANYID,SYSTEMCODE=:SYSTEMCODE,SYSTEMNAME=:SYSTEMNAME,MODELCODE=:MODELCODE,MODELNAME=:MODELNAME,TRIGGERORDERSTATUS=:TRIGGERORDERSTATUS WHERE   DOTASKRULEID=:DOTASKRULEID";
                OracleParameter[] pageparm =
                {               
                    new OracleParameter(":DOTASKRULEID",OracleType.NVarChar,200), 
                    new OracleParameter(":COMPANYID",OracleType.NVarChar,100), 
                    new OracleParameter(":SYSTEMCODE",OracleType.NVarChar,100), 
                    new OracleParameter(":SYSTEMNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":MODELCODE",OracleType.NVarChar,100), 
                    new OracleParameter(":MODELNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":TRIGGERORDERSTATUS",OracleType.Number,22) 

                };
                pageparm[0].Value = GetValue(rule.DOTASKRULEID);//待办规则主表ID
                pageparm[1].Value = GetValue(rule.COMPANYID);//公司ID
                pageparm[2].Value = GetValue(rule.SYSTEMCODE);//系统代码
                pageparm[3].Value = GetValue(rule.SYSTEMNAME);//系统名称
                pageparm[4].Value = GetValue(rule.MODELCODE);//模块代码
                pageparm[5].Value = GetValue(rule.MODELNAME);//模块名称
                pageparm[6].Value = GetValue(rule.TRIGGERORDERSTATUS);//触发条件的单据状态
                dal.ExecuteSql(updSql, pageparm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void DeleteDoTaskRule(string ruleID)
        {
            try
            {

                string sqldetail = "delete T_WF_DOTASKRULEDETAIL where DOTASKRULEID='" + ruleID + "'";
                dal.ExecuteSql(sqldetail);
                string sql = "delete T_WF_DOTASKRULE where DOTASKRULEID='" + ruleID + "'";
                dal.ExecuteSql(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        /// <summary>
        /// 如果value值为null则返回""字符串,否则返回value值。
        /// </summary>
        /// <param name="value">value值</param>
        /// <returns></returns>
        private object GetValue(object value)
        {
            return value == null ? DBNull.Value.ToString() : value;
        }
    }
}
