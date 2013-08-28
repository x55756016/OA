using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model.FlowEngine;
using System.Data;

namespace SMT.Workflow.Platform.DAL
{
    public class TimingTriggerDAL : BaseDAL
    {
        public List<T_WF_TIMINGTRIGGERACTIVITY> GetTimingTriggerList(int pageSize, int pageIndex, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {

                dao.Open();
                strFilter += " and length(TRIGGERNAME)>1 and length(nvl(BUSINESSID, 0))<2 ";
                int number = pageIndex <= 1 ? 1 : (pageIndex - 1) * pageSize + 1;
                string countSql = @"SELECT count(1)  from T_WF_TIMINGTRIGGERACTIVITY where (1=1)";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    countSql += strFilter + "";
                }
                string sql = @"SELECT * FROM (SELECT A.*, ROWNUM Page FROM (select * from T_WF_TIMINGTRIGGERACTIVITY 
                                   order by " + strOrderBy + " ) A WHERE (1=1) ";
                if (!string.IsNullOrWhiteSpace(strFilter))
                {
                    sql += strFilter + " AND ROWNUM<= " + pageSize * pageIndex + ")";
                }
                sql += "WHERE  Page >= " + number + "";
                DataTable dt = dao.GetDataTable(sql);
                pageCount = Convert.ToInt32(dao.ExecuteScalar(countSql));
                pageCount = pageCount / pageSize + (pageCount % pageSize > 0 ? 1 : 0);
                return ToList<T_WF_TIMINGTRIGGERACTIVITY>(dt);
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
