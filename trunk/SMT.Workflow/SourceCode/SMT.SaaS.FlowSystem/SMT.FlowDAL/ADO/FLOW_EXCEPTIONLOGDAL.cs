/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2012     
	 * 文件名：FLOW_EXCEPTIONLOGDAL.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2012/9/7 16:32:15   
	 * NET版本： 4.0.30319.225 
	 * 命名空间：SMT.Workflow.Monitoring.DAL 
	 * 模块名称：异常记录日志
	 * 描　　述： 对流程监控产生的日志进行处理
	 * 修改人员：
	 * 修改日期：
	 * 修改内容：
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using SMT.Workflow.Common.Model;
using SMT.FlowDAL;
using SMT.Workflow.Common.DataAccess;


namespace SMT.FLOWDAL.ADO
{
    /// <summary>
    /// 异常记录日志
    /// </summary>
    public class FLOW_EXCEPTIONLOGDAL : BaseDAL
    {
        /// <summary>
        /// 增加异常记录(以实体传值)
        /// </summary>
        /// <param name="conn">OracleConnection </param>
        /// <param name="model">FLOW_EXCEPTIONLOG</param>
        /// <returns></returns>
        public int Add(OracleConnection conn, FLOW_EXCEPTIONLOG model)
        {
            try
            {
                string insSql = "INSERT INTO FLOW_EXCEPTIONLOG (ID,FORMID,MODELCODE,CREATEDATE,CREATENAME,SUBMITINFO,LOGINFO,MODELNAME,OWNERID,OWNERNAME,OWNERCOMPANYID,OWNERCOMPANYNAME,OWNERDEPARMENTID,OWNERDEPARMENTNAME,OWNERPOSTID,OWNERPOSTNAME,AUDITSTATE) VALUES (:ID,:FORMID,:MODELCODE,:CREATEDATE,:CREATENAME,:SUBMITINFO,:LOGINFO,:MODELNAME,:OWNERID,:OWNERNAME,:OWNERCOMPANYID,:OWNERCOMPANYNAME,:OWNERDEPARMENTID,:OWNERDEPARMENTNAME,:OWNERPOSTID,:OWNERPOSTNAME,:AUDITSTATE)";
                OracleParameter[] pageparm =
                {               
                    new OracleParameter(":ID",OracleType.NVarChar,100), 
                    new OracleParameter(":FORMID",OracleType.NVarChar,100), 
                    new OracleParameter(":MODELCODE",OracleType.NVarChar,100), 
                    new OracleParameter(":CREATEDATE",OracleType.DateTime), 
                    new OracleParameter(":CREATENAME",OracleType.NVarChar,100), 
                    new OracleParameter(":SUBMITINFO",OracleType.NVarChar,4000), 
                    new OracleParameter(":LOGINFO",OracleType.Clob), 
                    new OracleParameter(":MODELNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":OWNERID",OracleType.NVarChar,100), 
                    new OracleParameter(":OWNERNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":OWNERCOMPANYID",OracleType.NVarChar,100), 
                    new OracleParameter(":OWNERCOMPANYNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":OWNERDEPARMENTID",OracleType.NVarChar,100), 
                    new OracleParameter(":OWNERDEPARMENTNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":OWNERPOSTID",OracleType.NVarChar,100), 
                    new OracleParameter(":OWNERPOSTNAME",OracleType.NVarChar,100), 
                    new OracleParameter(":AUDITSTATE",OracleType.NVarChar,100) 

                   

                };
                pageparm[0].Value = OracleDataProvider.GetValue(model.ID);//主键ID
                pageparm[1].Value = OracleDataProvider.GetValue(model.FORMID);//业务ID
                pageparm[2].Value = OracleDataProvider.GetValue(model.MODELCODE);//模块代码
                pageparm[3].Value = OracleDataProvider.GetValue(model.CREATEDATE);//创建日期
                pageparm[4].Value = OracleDataProvider.GetValue(model.CREATENAME);//创建人
                pageparm[5].Value = OracleDataProvider.GetValue(model.SUBMITINFO);//提交信息
                pageparm[6].Value = OracleDataProvider.GetValue(model.LOGINFO);//异常日志信息
                pageparm[7].Value = OracleDataProvider.GetValue(model.MODELNAME);//模块名称
                pageparm[8].Value = OracleDataProvider.GetValue(model.OWNERID);//单据所属人ID
                pageparm[9].Value = OracleDataProvider.GetValue(model.OWNERNAME);//单据所属人姓名
                pageparm[10].Value = OracleDataProvider.GetValue(model.OWNERCOMPANYID);//单据所属人公司ID
                pageparm[11].Value = OracleDataProvider.GetValue(model.OWNERCOMPANYNAME);//单据所属人公司名称
                pageparm[12].Value = OracleDataProvider.GetValue(model.OWNERDEPARMENTID);//单据所属人部门ID
                pageparm[13].Value = OracleDataProvider.GetValue(model.OWNERDEPARMENTNAME);//单据所属人部门名称
                pageparm[14].Value = OracleDataProvider.GetValue(model.OWNERPOSTID);//单据所属人岗位ID
                pageparm[15].Value = OracleDataProvider.GetValue(model.OWNERPOSTNAME);//单据所属人岗位名称
                pageparm[16].Value = OracleDataProvider.GetValue(model.AUDITSTATE);//审核状态;审核通过,审核不通过


                return ExecuteSQL(conn, insSql, pageparm);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 运维人员处理异常记录
        /// </summary>
        /// <param name="conn">OracleConnection</param>
        /// <param name="model">FLOW_EXCEPTIONLOG</param>
        /// <returns></returns>
        public int Update(OracleConnection conn, FLOW_EXCEPTIONLOG model)
        {
            try
            {
                string updSql = "UPDATE FLOW_EXCEPTIONLOG SET STATE=:STATE,UPDATEDATE=:UPDATEDATE,UPDATENAME=:UPDATENAME,REMARK=:REMARK WHERE   ID=:ID";
                OracleParameter[] pageparm =
                {               
                    new OracleParameter(":ID",OracleType.NVarChar,100), 
                    new OracleParameter(":STATE",OracleType.NVarChar,100), 
                    new OracleParameter(":UPDATEDATE",OracleType.DateTime), 
                    new OracleParameter(":UPDATENAME",OracleType.NVarChar,100), 
                    new OracleParameter(":REMARK",OracleType.NVarChar,4000) 
                };
                pageparm[0].Value = OracleDataProvider.GetValue(model.ID);//主键ID
                pageparm[1].Value = OracleDataProvider.GetValue(model.STATE);//状态:未处理;已处理
                pageparm[2].Value = OracleDataProvider.GetValue(DateTime.Now);//处理日期
                pageparm[3].Value = OracleDataProvider.GetValue(model.UPDATENAME);//处理人
                pageparm[4].Value = OracleDataProvider.GetValue(model.REMARK);//备注
                return ExecuteSQL(conn, updSql, pageparm);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }
    }
}
