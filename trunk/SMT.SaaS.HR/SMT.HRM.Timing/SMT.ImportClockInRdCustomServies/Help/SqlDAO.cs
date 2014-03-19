using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;


namespace SMT.HRM.ImportAttRecordWinSV.Help
{
    class SqlDAO
    {
        static string conn = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;

        private SqlConnection Conn = null;



        public SqlDAO()
        {

        }
        private void openConnection()
        {
            if (Conn == null)
            {
                Conn = new SqlConnection(conn);
                Conn.Open();
            }
            else if (Conn.State == System.Data.ConnectionState.Closed)
            {
                Conn.Open();
            }
            else if (Conn.State == System.Data.ConnectionState.Broken)
            {
                Conn.Close();
                Conn.Open();
            }
        }
        private void closeConnection()
        {
            if (Conn != null)
            {
                Conn.Close();
            }

        }

        public string doSearch(string sqls)
        {
            try
            {
                DataSet dataSet = new DataSet();
                openConnection();
                String result = "";
                SqlDataAdapter sqlDA = new SqlDataAdapter(sqls, Conn);
                sqlDA.Fill(dataSet, "Result");
                result = dataSet.GetXml();
                return result;

            }
            finally
            {
                closeConnection();
            }

        }

        public string doSearch(string sqls, IList<DbParameter> sqlparameters)
        {
            try
            {
                openConnection();
                String result = "";
                using (SqlDataAdapter sqlDA = new SqlDataAdapter(sqls, Conn))
                {
                    DataSet dataSet = new DataSet();
                    if (sqlparameters != null)
                    {

                        foreach (SqlParameter parameter in sqlparameters)
                        {
                            if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                            {
                                parameter.Value = DBNull.Value;
                            }
                            sqlDA.SelectCommand.Parameters.Add(parameter);
                        }
                    }

                    sqlDA.Fill(dataSet, "Result");
                    sqlDA.SelectCommand.Parameters.Clear();
                    result = dataSet.GetXml();
                }
                return result;

            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                closeConnection();
            }

        }


        public DataSet doSearchByDataset(string sqls)
        {
            try
            {
                DataSet dataSet = new DataSet();
                openConnection();
                SqlDataAdapter sqlDA = new SqlDataAdapter(sqls, Conn);
                sqlDA.Fill(dataSet, "Result");
                return dataSet;

            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                closeConnection();
            }

        }


        public DataSet doSearchByDataset(string sqls, IList<DbParameter> sqlparameters)
        {

            try
            {
                openConnection();
                DataSet dataSet = new DataSet();
                using (SqlDataAdapter sqlDA = new SqlDataAdapter(sqls, Conn))
                {
                    if (sqlparameters != null)
                    {

                        foreach (SqlParameter parameter in sqlparameters)
                        {
                            if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                            {
                                parameter.Value = DBNull.Value;
                            }
                            sqlDA.SelectCommand.Parameters.Add(parameter);
                        }
                    }

                    sqlDA.Fill(dataSet, "Result");
                    sqlDA.SelectCommand.Parameters.Clear();
                    return dataSet;

                }

            }

            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                closeConnection();
            }

        }

        public bool doExec(string sqls)
        {
            bool result = false;
            openConnection();
            SqlTransaction trans = Conn.BeginTransaction();
            try
            {
                SqlCommand sqlcom = new SqlCommand();
                sqlcom.CommandText = sqls;
                sqlcom.Connection = Conn;
                sqlcom.Transaction = trans;
                sqlcom.ExecuteNonQuery();
                trans.Commit();
                result = true;
            }
            catch (SqlException ex)
            {
                result = false;
                throw new Exception(ex.Message);
            }
            finally
            {
                closeConnection();
            }
            return result;
        }

        public bool doExec(string sqls, IList<DbParameter> sqlparameters)
        {
            bool result = false;
            openConnection();
            SqlTransaction trans = Conn.BeginTransaction();
            try
            {
                using (SqlCommand sqlcom = new SqlCommand())
                {

                    if (sqlparameters != null)
                    {
                        foreach (SqlParameter parameter in sqlparameters)
                        {
                            if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                            {
                                parameter.Value = DBNull.Value;
                            }
                            sqlcom.Parameters.Add(parameter);
                        }
                    }

                    sqlcom.CommandText = sqls;
                    sqlcom.Connection = Conn;
                    sqlcom.Transaction = trans;
                    sqlcom.ExecuteNonQuery();
                    trans.Commit();
                    result = true;

                }
            }
            catch (SqlException ex)
            {
                trans.Rollback();
                result = false;
                throw new Exception(ex.Message);
            }

            finally
            {
                closeConnection();
            }
            return result;
        }
    }
}


