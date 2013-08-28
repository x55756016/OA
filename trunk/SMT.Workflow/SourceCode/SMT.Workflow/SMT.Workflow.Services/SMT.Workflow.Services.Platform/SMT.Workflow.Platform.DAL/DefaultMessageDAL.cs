using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Data;
using SMT.Workflow.Common.Model;

namespace SMT.Workflow.Platform.DAL
{
    public class DefaultMessageDAL : BaseDAL
    {

        public List<T_WF_DEFAULTMESSAGE> GetDefaultMessageList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {

                dao.Open();
                int number = pageIndex <= 1 ? 1 : (pageIndex - 1) * pageSize + 1;
                string countSql = @"SELECT count(1)  from T_WF_DEFAULTMESSAGE where (1=1)";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    countSql += strFilter + "";
                }
                string sql = @"SELECT * FROM (SELECT A.*, ROWNUM Page FROM (select * from T_WF_DEFAULTMESSAGE 
                                   order by " + strOrderBy + " ) A WHERE (1=1) AND ROWNUM<= " + pageSize * pageIndex + "";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    sql += strFilter + ")";
                }
                else
                {
                    sql += ")";
                }
                sql += "  WHERE  Page >= " + number + "";
                DataTable dt = dao.GetDataTable(sql);
                pageCount = Convert.ToInt32(dao.ExecuteScalar(countSql));
                pageCount = pageCount / pageSize + (pageCount % pageSize > 0 ? 1 : 0);
                return ToList<T_WF_DEFAULTMESSAGE>(dt);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }

        }
        public DataTable GetDataTable(string sql)
        {
            try
            {
                dao.Open();
                DataTable dt = dao.GetDataTable(sql);
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }

        }

        public void ExecuteMessageSql(string sql, OracleParameter[] pageparm)
        {
            try
            {
                dao.Open();
                dao.ExecuteNonQuery(sql, CommandType.Text, pageparm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }
        }
    }
}
