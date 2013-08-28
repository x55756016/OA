using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.EntityFlowSys;
using System.Data.OracleClient;
using System.Data;
using SMT.Workflow.Common.DataAccess;

namespace SMT.FLOWDAL.ADO
{
    /// <summary>
    /// [咨询]
    /// </summary>
    public class FLOW_CONSULTATION_TDAL
    {
        #region 龙康才新增
        /// <summary>
        /// 新增[咨询]
        /// </summary>
        /// <param name="con">OracleConnection</param>
        /// <param name="model">FLOW_CONSULTATION_T</param>
        /// <returns></returns>
        public static int Add(OracleConnection con, FLOW_CONSULTATION_T model)
        {
            try
            {
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                string insSql = "INSERT INTO FLOW_CONSULTATION_T (CONSULTATIONID,FLOWRECORDDETAILID,CONSULTATIONUSERID,CONSULTATIONUSERNAME,CONSULTATIONCONTENT,CONSULTATIONDATE,REPLYUSERID,REPLYUSERNAME,REPLYCONTENT,REPLYDATE,FLAG) VALUES (:CONSULTATIONID,:FLOWRECORDDETAILID,:CONSULTATIONUSERID,:CONSULTATIONUSERNAME,:CONSULTATIONCONTENT,:CONSULTATIONDATE,:REPLYUSERID,:REPLYUSERNAME,:REPLYCONTENT,:REPLYDATE,:FLAG)";
                OracleParameter[] pageparm =
                {               
                    new OracleParameter(":CONSULTATIONID",OracleType.NVarChar), 
                    new OracleParameter(":FLOWRECORDDETAILID",OracleType.NVarChar), 
                    new OracleParameter(":CONSULTATIONUSERID",OracleType.NVarChar), 
                    new OracleParameter(":CONSULTATIONUSERNAME",OracleType.NVarChar), 
                    new OracleParameter(":CONSULTATIONCONTENT",OracleType.NVarChar), 
                    new OracleParameter(":CONSULTATIONDATE",OracleType.DateTime), 
                    new OracleParameter(":REPLYUSERID",OracleType.NVarChar), 
                    new OracleParameter(":REPLYUSERNAME",OracleType.NVarChar), 
                    new OracleParameter(":REPLYCONTENT",OracleType.NVarChar), 
                    new OracleParameter(":REPLYDATE",OracleType.DateTime), 
                    new OracleParameter(":FLAG",OracleType.NVarChar) 

                };
                pageparm[0].Value = MsOracle.GetValue(model.CONSULTATIONID);//
                pageparm[1].Value = MsOracle.GetValue(model.FLOW_FLOWRECORDDETAIL_T.FLOWRECORDDETAILID);//
                pageparm[2].Value = MsOracle.GetValue(model.CONSULTATIONUSERID);//
                pageparm[3].Value = MsOracle.GetValue(model.CONSULTATIONUSERNAME);//
                pageparm[4].Value = MsOracle.GetValue(model.CONSULTATIONCONTENT);//
                pageparm[5].Value = MsOracle.GetValue(model.CONSULTATIONDATE);//
                pageparm[6].Value = MsOracle.GetValue(model.REPLYUSERID);//
                pageparm[7].Value = MsOracle.GetValue(model.REPLYUSERNAME);//
                pageparm[8].Value = MsOracle.GetValue(model.REPLYCONTENT);//
                pageparm[9].Value = MsOracle.GetValue(model.REPLYDATE);//
                pageparm[10].Value = MsOracle.GetValue(model.FLAG);//0未回复，1回复

                return MsOracle.ExecuteSQLByTransaction(con, insSql, pageparm);
            }
            catch (Exception ex)
            {

                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                throw new Exception("FLOW_CONSULTATION_TDAL_Add:" + ex.Message);
            }
        }
        /// <summary>
        ///  更新: [咨询]
        /// </summary>
        /// <param name="model">FLOW_CONSULTATION_T</param>
        /// <returns></returns>
        public static int Update(OracleConnection con, FLOW_CONSULTATION_T model)
        {
            try
            {
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                model.REPLYDATE = DateTime.Now;
                model.FLAG = "1";
                string updSql = "UPDATE FLOW_CONSULTATION_T SET FLOWRECORDDETAILID=:FLOWRECORDDETAILID,CONSULTATIONUSERID=:CONSULTATIONUSERID,CONSULTATIONUSERNAME=:CONSULTATIONUSERNAME,CONSULTATIONCONTENT=:CONSULTATIONCONTENT,CONSULTATIONDATE=:CONSULTATIONDATE,REPLYUSERID=:REPLYUSERID,REPLYUSERNAME=:REPLYUSERNAME,REPLYCONTENT=:REPLYCONTENT,REPLYDATE=:REPLYDATE,FLAG=:FLAG WHERE   CONSULTATIONID=:CONSULTATIONID";
                OracleParameter[] pageparm =
                {               
                    new OracleParameter(":CONSULTATIONID",OracleType.NVarChar), 
                    new OracleParameter(":FLOWRECORDDETAILID",OracleType.NVarChar), 
                    new OracleParameter(":CONSULTATIONUSERID",OracleType.NVarChar), 
                    new OracleParameter(":CONSULTATIONUSERNAME",OracleType.NVarChar), 
                    new OracleParameter(":CONSULTATIONCONTENT",OracleType.NVarChar), 
                    new OracleParameter(":CONSULTATIONDATE",OracleType.DateTime), 
                    new OracleParameter(":REPLYUSERID",OracleType.NVarChar), 
                    new OracleParameter(":REPLYUSERNAME",OracleType.NVarChar), 
                    new OracleParameter(":REPLYCONTENT",OracleType.NVarChar), 
                    new OracleParameter(":REPLYDATE",OracleType.DateTime), 
                    new OracleParameter(":FLAG",OracleType.NVarChar) 

                };
                pageparm[0].Value = MsOracle.GetValue(model.CONSULTATIONID);//
                pageparm[1].Value = MsOracle.GetValue(model.FLOW_FLOWRECORDDETAIL_T.FLOWRECORDDETAILID);//
                pageparm[2].Value = MsOracle.GetValue(model.CONSULTATIONUSERID);//
                pageparm[3].Value = MsOracle.GetValue(model.CONSULTATIONUSERNAME);//
                pageparm[4].Value = MsOracle.GetValue(model.CONSULTATIONCONTENT);//
                pageparm[5].Value = MsOracle.GetValue(model.CONSULTATIONDATE);//
                pageparm[6].Value = MsOracle.GetValue(model.REPLYUSERID);//
                pageparm[7].Value = MsOracle.GetValue(model.REPLYUSERNAME);//
                pageparm[8].Value = MsOracle.GetValue(model.REPLYCONTENT);//
                pageparm[9].Value = MsOracle.GetValue(model.REPLYDATE);//
                pageparm[10].Value = MsOracle.GetValue(model.FLAG);//0未回复，1回复

                return MsOracle.ExecuteSQLByTransaction(con, updSql, pageparm);
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                throw new Exception("FLOW_CONSULTATION_TDAL_Update:" + ex.Message);
            }
        }
        #endregion
        public static void Add(FLOW_CONSULTATION_T flowConsultation)
        {
            using (OracleConnection con = new OracleConnection(ADOHelper.ContextOracleConnection))
            {
                try
                {
                    flowConsultation.CONSULTATIONDATE = DateTime.Now;
                    string sql = @"insert into FLOW_CONSULTATION_T(CONSULTATIONID,FLOWRECORDDETAILID,CONSULTATIONUSERID,CONSULTATIONCONTENT,
                                        CONSULTATIONDATE,REPLYUSERID,REPLYCONTENT,REPLYDATE,FLAG,CONSULTATIONUSERNAME,REPLYUSERNAME) 
                                  values(:pCONSULTATIONID,:pFLOWRECORDDETAILID,:pCONSULTATIONUSERID,:pCONSULTATIONCONTENT,
                                        :pCONSULTATIONDATE,:pREPLYUSERID,:pREPLYCONTENT,:pREPLYDATE,:pFLAG,:pCONSULTATIONUSERNAME,:pREPLYUSERNAME)";

                    #region
                    con.Open();


                    OracleCommand cmd = con.CreateCommand();
                    cmd.CommandText = sql;

                    ADOHelper.AddParameter("CONSULTATIONID", flowConsultation.CONSULTATIONID, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("FLOWRECORDDETAILID", flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOWRECORDDETAILID, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("CONSULTATIONUSERID", flowConsultation.CONSULTATIONUSERID, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("CONSULTATIONCONTENT", flowConsultation.CONSULTATIONCONTENT, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("CONSULTATIONDATE", flowConsultation.CONSULTATIONDATE, OracleType.DateTime, cmd.Parameters);
                    ADOHelper.AddParameter("REPLYUSERID", flowConsultation.REPLYUSERID, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("REPLYCONTENT", flowConsultation.REPLYCONTENT, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("REPLYDATE", flowConsultation.REPLYDATE, OracleType.DateTime, cmd.Parameters);
                    ADOHelper.AddParameter("FLAG", flowConsultation.FLAG, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("CONSULTATIONUSERNAME", flowConsultation.CONSULTATIONUSERNAME, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("REPLYUSERNAME", flowConsultation.REPLYUSERNAME, OracleType.NVarChar, cmd.Parameters);

                    cmd.ExecuteNonQuery();

                    con.Close();
                    #endregion
                }
                catch (Exception ex)
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    throw new Exception("FLOW_CONSULTATION_TDAL_Add:" + ex.Message);
                }

            }
        }

        public static void Update(FLOW_CONSULTATION_T flowConsultation)
        {
            using (OracleConnection con = new OracleConnection(ADOHelper.ContextOracleConnection))
            {
                try
                {
                    flowConsultation.REPLYDATE = DateTime.Now;
                    flowConsultation.FLAG = "1";
                    //flowConsultation.CONSULTATIONDATE = DateTime.Now;
                    string sql = @"update FLOW_CONSULTATION_T set FLOWRECORDDETAILID=:pFLOWRECORDDETAILID,CONSULTATIONUSERID=:pCONSULTATIONUSERID,
                             CONSULTATIONCONTENT=:pCONSULTATIONCONTENT,CONSULTATIONDATE=:pCONSULTATIONDATE,REPLYUSERID=:pREPLYUSERID,
                             REPLYCONTENT=:pREPLYCONTENT,REPLYDATE=:pREPLYDATE,FLAG=:pFLAG,
                             CONSULTATIONUSERNAME=:pCONSULTATIONUSERNAME,REPLYUSERNAME=:pREPLYUSERNAME 
                              where  CONSULTATIONID=:pCONSULTATIONID ";

                    #region
                    con.Open();


                    OracleCommand cmd = con.CreateCommand();
                    cmd.CommandText = sql;


                    ADOHelper.AddParameter("CONSULTATIONID", flowConsultation.CONSULTATIONID, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("FLOWRECORDDETAILID", flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOWRECORDDETAILID, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("CONSULTATIONUSERID", flowConsultation.CONSULTATIONUSERID, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("CONSULTATIONCONTENT", flowConsultation.CONSULTATIONCONTENT, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("CONSULTATIONDATE", flowConsultation.CONSULTATIONDATE, OracleType.DateTime, cmd.Parameters);
                    ADOHelper.AddParameter("REPLYUSERID", flowConsultation.REPLYUSERID, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("REPLYCONTENT", flowConsultation.REPLYCONTENT, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("REPLYDATE", flowConsultation.REPLYDATE, OracleType.DateTime, cmd.Parameters);
                    ADOHelper.AddParameter("FLAG", flowConsultation.FLAG, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("CONSULTATIONUSERNAME", flowConsultation.CONSULTATIONUSERNAME, OracleType.NVarChar, cmd.Parameters);
                    ADOHelper.AddParameter("REPLYUSERNAME", flowConsultation.REPLYUSERNAME, OracleType.NVarChar, cmd.Parameters);

                    cmd.ExecuteNonQuery();

                    con.Close();
                    #endregion
                }
                catch (Exception ex)
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    throw new Exception("FLOW_CONSULTATION_TDAL_Update:" + ex.Message);
                }

            }
        }


    }
}
