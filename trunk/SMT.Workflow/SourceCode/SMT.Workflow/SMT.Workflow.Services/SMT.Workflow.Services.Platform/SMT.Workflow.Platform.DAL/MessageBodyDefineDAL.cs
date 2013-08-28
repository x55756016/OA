using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model.FlowEngine;
using System.Data;
using SMT.Workflow.Common.DataAccess;

namespace SMT.Workflow.Platform.DAL
{
    public class MessageBodyDefineDAL : BaseDAL
    {

        /// <summary>
        /// 获取默认消息的分页列表
        /// </summary>
        /// <param name="number1"></param>
        /// <param name="number2"></param>
        /// <param name="strFilter"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_WF_MESSAGEBODYDEFINE> GetDefaultMessgeList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {

                dao.Open();
                int number = pageIndex <= 1 ? 1 : (pageIndex - 1) * pageSize + 1;
                string countSql = @"SELECT count(1)  from T_WF_MESSAGEBODYDEFINE where (1=1)";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    countSql += strFilter + "";
                }
                string sql = @"SELECT * FROM (SELECT A.*, ROWNUM Page FROM (select * from T_WF_MESSAGEBODYDEFINE 
                                   order by " + strOrderBy + " ) A WHERE (1=1) AND ROWNUM<= " + pageSize * pageIndex + "";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    sql += strFilter + ")";
                }
                sql += "WHERE  Page >= " + number + "";
                DataTable dt = dao.GetDataTable(sql);              
                pageCount = Convert.ToInt32(dao.ExecuteScalar(countSql));
                pageCount = pageCount / pageSize + (pageCount % pageSize > 0 ? 1 : 0);
                return ToList<T_WF_MESSAGEBODYDEFINE>(dt);
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
    }
}
