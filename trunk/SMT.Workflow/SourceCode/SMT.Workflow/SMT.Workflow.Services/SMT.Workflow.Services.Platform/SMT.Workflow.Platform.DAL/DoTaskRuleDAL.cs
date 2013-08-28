using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model.FlowEngine;
using System.Data;
using System.Data.OracleClient;
using SMT.Workflow.Common.DataAccess;

namespace SMT.Workflow.Platform.DAL
{
    public class DoTaskRuleDAL : BaseDAL
    {
        public List<T_WF_DOTASKRULE> GetDoTaskList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {

                dao.Open();
                int number = pageIndex <= 1 ? 1 : (pageIndex - 1) * pageSize + 1;
                string countSql = @"SELECT count(1)  from T_WF_DOTASKRULE where (1=1)";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    countSql += strFilter + "";
                }
                string sql = @"SELECT * FROM (SELECT A.* FROM (select t.*,ROWNUM Page from T_WF_DOTASKRULE t  WHERE (1=1)  ";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    sql += strFilter ;
                }
                sql += "  order by " + strOrderBy + " ) A WHERE (1=1) AND Page<= " + pageSize * pageIndex + ") ";
                sql += "WHERE  Page >= " + number + "";              
                DataTable dt = dao.GetDataTable(sql);
                pageCount = Convert.ToInt32(dao.ExecuteScalar(countSql));
                pageCount = pageCount / pageSize + (pageCount % pageSize > 0 ? 1 : 0);
                return ToList<T_WF_DOTASKRULE>(dt);
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

        public T_WF_DOTASKRULE GetEntity(string sql)
        {
            try
            {
                dao.Open();
                DataTable dt = dao.GetDataTable(sql);
                return ToList<T_WF_DOTASKRULE>(dt)[0];
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


        public List<T_WF_DOTASKRULEDETAIL> GetDoTaskRuleDetail(string sql)
        {
            try
            {
                dao.Open();
                DataTable dt = dao.GetDataTable(sql);
                return ToList<T_WF_DOTASKRULEDETAIL>(dt);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        public void ExecuteSql(string sql)
        {
            try
            {
                dao.Open();
                dao.ExecuteNonQuery(sql);
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
        //public void ExecuteSql(string sql, OracleParameter[] pageparm)
        //{
        //    try
        //    {
        //        dao.Open();
        //        dao.ExecuteNonQuery(sql, CommandType.Text, pageparm);               
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message, ex);
        //    }
        //    finally
        //    {
        //        dao.Close();
        //    }
        //}
    }
}
