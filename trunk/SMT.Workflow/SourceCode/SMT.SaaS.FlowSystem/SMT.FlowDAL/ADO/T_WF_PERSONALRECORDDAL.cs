using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.EntityFlowSys;
using System.Data.OracleClient;
using System.Data;
using SMT.Workflow.Common.DataAccess;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.FLOWDAL.ADO
{
    /// <summary>
    /// 我的单据
    /// </summary>
    public class T_WF_PERSONALRECORDDAL
    {
        #region 龙康才新增
        /// <summary>
        /// 根据ID获取我的单据实体
        /// </summary>
        /// <param name="PERSONALRECORDID">PERSONALRECORDID</param>
        /// <returns></returns>
        public static T_WF_PERSONALRECORD GetPersonalRecordByPersonalrecordid(OracleConnection con, string personalrecordid)
        {
            if (string.IsNullOrEmpty(con.ConnectionString))
            {
                con.ConnectionString = ADOHelper.ConnectionString;
            }
            T_WF_PERSONALRECORD model = new T_WF_PERSONALRECORD();
            OracleDataReader dr = null;
            try
            {
                #region

                string sql = "select * from T_WF_PERSONALRECORD where PERSONALRECORDID='" + personalrecordid + "'";
                dr = MsOracle.ExecuteReaderByTransaction(con, sql, null);
                while (dr.Read())
                {
                    #region define
                    model.PERSONALRECORDID = dr["PERSONALRECORDID"].ToString();//个人单据ID 
                    model.SYSTYPE = dr["SYSTYPE"].ToString();//系统类型 
                    model.MODELCODE = dr["MODELCODE"].ToString();//所属模块代码 
                    model.MODELID = dr["MODELID"].ToString();//单据ID 
                    model.CHECKSTATE = dr["CHECKSTATE"].ToString() != "" ? Convert.ToDecimal(dr["CHECKSTATE"]) : 0; //单据审核状态  
                    model.OWNERID = dr["OWNERID"].ToString();//所属员工ID 
                    model.OWNERPOSTID = dr["OWNERPOSTID"].ToString();//所属岗位ID 
                    model.OWNERDEPARTMENTID = dr["OWNERDEPARTMENTID"].ToString();//所属部门ID 
                    model.OWNERCOMPANYID = dr["OWNERCOMPANYID"].ToString();//所属公司ID 
                    model.CONFIGINFO = dr["CONFIGINFO"].ToString();//参数配置 
                    model.MODELDESCRIPTION = dr["MODELDESCRIPTION"].ToString();//单据简要描叙 
                    model.ISFORWARD = dr["ISFORWARD"].ToString() != "" ? Convert.ToDecimal(dr["ISFORWARD"]) : 0; //是否转发(0表示非转发，1表示转发)  
                    model.ISVIEW = dr["ISVIEW"].ToString() != "" ? Convert.ToDecimal(dr["ISVIEW"]) : 0; //是否已查看(0表示未查看，1表示已查看)  
                    model.CREATEDATE = dr["CREATEDATE"].ToString() != "" ? Convert.ToDateTime(dr["CREATEDATE"]) : DateTime.Now; //创建时间  
                    model.UPDATEDATE = dr["UPDATEDATE"].ToString() != "" ? Convert.ToDateTime(dr["UPDATEDATE"]) : DateTime.Now; //修改时间  

                    #endregion
                }
                dr.Close();
                #endregion
                return model;
            }
            catch (Exception ex)
            {
                if (dr != null && !dr.IsClosed)
                {
                    dr.Close();
                }
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                throw new Exception("我的单据异常信息-->" + ex.Message);
            }
        }
        #endregion

    }
}
