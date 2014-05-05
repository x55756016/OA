using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

//using SMT.Framework.Core.Utilities;

namespace SMT.Foundation.Log
{
    public class DBLogger : ILogger
    {
        public DBLogger()
        {

        }
        public void Write(ErrorLog message)
        {
            //string pk = Guid.NewGuid().ToString();
            //string baseSql = "INSERT INTO SMT_ErrorMessageBase (SMT_ErrorMessageId, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, StateCode, StatusCode, DeletionStateCode,OrganizationId) "
            //                         + " VALUES  ('{0}', {1},{2},{3}, {4},{5},{6}, {7},{8}) ";
            //string exSql = "INSERT INTO SMT_ErrorMessageExtensionBase (SMT_ErrorMessageId,SMT_ComputerName,SMT_AWBAssemblyVersion,SMT_AWBVersion,"
            //                    + " SMT_ClientBrowser,SMT_ClientBrowserLanguage,SMT_ClientJavaScriptSupport,SMT_ClientNetRuntime,SMT_ClientOS,"
            //                    + " SMT_ClientOSLanguage,SMT_ServerOS,SMT_ServerNetRuntime,SMT_Message,SMT_ErrorStackTrace,SMT_ErrorURL)"
            //                    + " values (@SMT_ErrorMessageId,@SMT_ComputerName,@SMT_AWBAssemblyVersion,@SMT_AWBVersion,"
            //                    + " @SMT_ClientBrowser,@SMT_ClientBrowserLanguage,@SMT_ClientJavaScriptSupport,@SMT_ClientNetRuntime,@SMT_ClientOS,"
            //                    + " @SMT_ClientOSLanguage,@SMT_ServerOS,@SMT_ServerNetRuntime,@SMT_ErrorMessage,@SMT_ErrorStackTrace,@SMT_ErrorURL)";

            //baseSql = string.Format(baseSql, pk, "getutcdate()", "dbo.fn_FindUserGuid()", "getutcdate() ", " dbo.fn_FindUserGuid()", 0, 1, 0, "dbo.fn_FindOrganizationGuid()");
            //ParameterCollection pcoll = new ParameterCollection();
            //pcoll.Add("@SMT_ErrorMessageId", pk);
            //pcoll.Add("@SMT_ComputerName", ConvertHelper.ToSqlParameter(message.ComputerName));
            //pcoll.Add("@SMT_AWBAssemblyVersion", ConvertHelper.ToSqlParameter(message.AWBAssembliesVersion));
            //pcoll.Add("@SMT_AWBVersion", ConvertHelper.ToSqlParameter(message.AWBVersion));
            //pcoll.Add("@SMT_ClientBrowser", ConvertHelper.ToSqlParameter(message.ClientBrowser));
            //pcoll.Add("@SMT_ClientBrowserLanguage", ConvertHelper.ToSqlParameter(message.ClientBrowserLanguage));
            //pcoll.Add("@SMT_ClientJavaScriptSupport", ConvertHelper.ToSqlParameter(message.ClientJavaScriptSupport == true ? 1 : 0));
            //pcoll.Add("@SMT_ClientNetRuntime", ConvertHelper.ToSqlParameter(message.ClientNetRuntime));
            //pcoll.Add("@SMT_ClientOS", ConvertHelper.ToSqlParameter(message.ClientOS));
            //pcoll.Add("@SMT_ClientOSLanguage", ConvertHelper.ToSqlParameter(message.ClientOSLanguage));
            //pcoll.Add("@SMT_ServerOS", ConvertHelper.ToSqlParameter(message.ServerOS));
            //pcoll.Add("@SMT_ServerNetRuntime", ConvertHelper.ToSqlParameter(message.ServerNetRuntime));
            //pcoll.Add("@SMT_ErrorMessage", ConvertHelper.ToSqlParameter(message.ErrorMessage));
            //pcoll.Add("@SMT_ErrorStackTrace", ConvertHelper.ToSqlParameter(message.ErrorStackTrace));
            //pcoll.Add("@SMT_ErrorURL", ConvertHelper.ToSqlParameter(message.ErrorURL));

            //IDAO dao = null;
            //try
            //{
            //    dao = new SqlServerDAO(LogConfig.Instance.ConnectionString);
            //    dao.BeginTransaction();
            //    dao.ExecuteNonQuery(baseSql, CommandType.Text);
            //    dao.ExecuteNonQuery(exSql, CommandType.Text, pcoll);
            //    dao.Commit();
            //}
            //catch (Exception ex)
            //{
            //    if (dao != null)
            //    {
            //        dao.Rollback();
            //    }
            //    throw ex;
            //}
        }

        //public DataSet RetrieveErrorLogs(DateTime dtfrom, DateTime dtto, string strUserID)
        //{
        //    string sql = "select * from SMT_ErrorMessage where createdon>='{0}' and createdon<='{1}' ";
        //    sql = string.Format(sql, dtfrom.ToShortDateString(), dtto.ToShortDateString());
        //    if (strUserID != "")
        //        sql += " and createdby='" + strUserID + "'";
        //    IDAO dao = new SqlServerDAO(LogConfig.Instance.ConnectionString);
        //    DataTable ret = dao.GetDataTable(sql);
        //    DataSet ds = new DataSet();
        //    ds.Tables.Add(ret);
        //    return ds;
        //}

        //public ErrorLog RetrieveErrorLogById(System.Guid errorLogID)
        //{
        //    string sql = "select * from SMT_ErrorMessage where SMT_ErrorMessageID='{0}'";
        //    sql = string.Format(sql, errorLogID.ToString());
        //    IDAO dao = new SqlServerDAO(LogConfig.Instance.ConnectionString);
        //    DataTable ret = dao.GetDataTable(sql);
        //    ErrorLog errorLog = new ErrorLog();
        //    if (ret.Rows.Count > 0)
        //    {
        //        errorLog.AWBAssembliesVersion = ret.Rows[0]["SMT_AWBAssemblyVersion"].ToString();
        //        errorLog.AWBVersion = ret.Rows[0]["SMT_AWBVersion"].ToString();
        //        errorLog.ClientBrowser = ret.Rows[0]["SMT_ClientBrowser"].ToString();
        //        errorLog.ClientBrowserLanguage = ret.Rows[0]["SMT_ClientBrowserLanguage"].ToString();
        //        errorLog.ClientJavaScriptSupport = (bool)ret.Rows[0]["SMT_ClientJavaScriptSupport"];
        //        errorLog.ClientNetRuntime = ret.Rows[0]["SMT_ClientNetRuntime"].ToString();
        //        errorLog.ClientOS = ret.Rows[0]["SMT_ClientOS"].ToString();
        //        errorLog.ClientOSLanguage = ret.Rows[0]["SMT_ClientOSLanguage"].ToString();
        //        errorLog.ComputerName = ret.Rows[0]["SMT_ComputerName"].ToString();
        //        // errorLog.ErrorExecutionTrace = ret.Rows[0]["SMT_ErrorExecutionTrace"].ToString();
        //        errorLog.ErrorMessage = ret.Rows[0]["SMT_ErrorMessage"].ToString();
        //        errorLog.ErrorStackTrace = ret.Rows[0]["SMT_ErrorStackTrace"].ToString();
        //        errorLog.ErrorURL = ret.Rows[0]["SMT_ErrorURL"].ToString();
        //        errorLog.ServerNetRuntime = ret.Rows[0]["SMT_ServerNetRuntime"].ToString();
        //        errorLog.ServerOS = ret.Rows[0]["SMT_ServerOS"].ToString();
        //    }
        //    return errorLog;
        //}

        //public void DeleteErrorLog(Guid errorLogID)
        //{
        //    string sql = "update SMT_ErrorMessagebase set DeletionStateCode=1 where SMT_ErrorMessageID='{0}'"; ;
        //    sql = string.Format(sql, errorLogID.ToString());
        //    IDAO dao = null;
        //    try
        //    {
        //         dao = new SqlServerDAO(LogConfig.Instance.ConnectionString);
        //         dao.ExecuteNonQuery(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (dao != null)
        //        {
        //            dao.Rollback();
        //        }
        //        throw ex;
        //    }
        //}
    }
}
