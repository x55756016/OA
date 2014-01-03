using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Data;
using System.Configuration;
using System.Windows.Forms;
using AttendaceAccount.HRCommonSV;

namespace SmtPortalSetUp
{

    public static class OracleHelp
    {
        private static OracleConnection myConnection;
        public static TextBox MsgBox = null;

        public static void Connect()
        {
            if (myConnection != null )
            {
                if(myConnection.State == ConnectionState.Closed)
                {
                    myConnection.Open();//打开连接 
                }
            }
            else
            {

                string myConnString = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
                myConnection = new OracleConnection(myConnString);
                myConnection.Open();//打开连接 
            }
        }
        public static void close()
        {
            myConnection.Close();//打开连接
        }

        public static DataTable getTable(string strSql)
        {
            if (MsgBox != null) MsgBox.Text = strSql + System.Environment.NewLine + MsgBox.Text;
            DataTable dt = new DataTable();
            try
            {
                Connect();
                OracleCommand orcCmd = myConnection.CreateCommand();//触发条件表
                orcCmd.CommandText = strSql;//根据公司ID查询触发条件表
                OracleDataAdapter orcDap = new OracleDataAdapter(orcCmd);//主表
                
                orcDap.Fill(dt);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                if (MsgBox != null) MsgBox.Text = "执行sql异常：" + ex.ToString() + System.Environment.NewLine + MsgBox.Text;
                throw ex;         
            }
            finally
            {
                close();
            }
            return dt;
        }

        public static int Excute(string strSql)
        {
            



            if (MsgBox != null) MsgBox.Text = strSql + System.Environment.NewLine + MsgBox.Text;
            try
            {
                Connect();
                OracleCommand orcCmd = myConnection.CreateCommand();//触发条件表
                orcCmd.CommandText = strSql;//根据公司ID查询触发条件表
                return orcCmd.ExecuteNonQuery();

                //HrCommonServiceClient sqlclient = new HrCommonServiceClient();
                //string msg = string.Empty;
                //object obj = sqlclient.CustomerQuery(strSql, ref msg);
                //return (int)obj;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                if (MsgBox != null) MsgBox.Text = "执行sql异常：" + ex.ToString() + System.Environment.NewLine + MsgBox.Text;
                return 0;
                throw ex;
            }
            finally
            {
                close();
            }
        }
    }
}
