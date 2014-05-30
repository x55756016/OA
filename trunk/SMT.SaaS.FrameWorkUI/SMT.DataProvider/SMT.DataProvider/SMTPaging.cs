using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Client;
//using Oracle.DataAccess.Client;

namespace SMT
{
    /// <summary>
    /// 分页处理类(目前只能实现oracle分页)
    /// </summary>
    public class SMTPaging
    {
       /// <summary>
        /// 分页处理
       /// </summary>
       /// <param name="sql">SQL语句</param>
       /// <param name="index">页索引</param>
       /// <param name="pageSize">每面显示记录数</param>
       /// <param name="pageCount">总页数</param>
       /// <param name="rowCount">总记录数</param>
       /// <returns></returns>
        public static DataResult BindPaging(string sql, int index, int pageSize, out int pageCount, out int rowCount)
        {
            if (index > 0)
            {//索引从0开始
                index = index - 1;
            }
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
            result.Sql = sql + ";";
            #region ORACLE

           // int rowCount = 0;
         //   DataTable dt = new DataTable();
            try
            {
                OracleParameter[] param = new OracleParameter[] {
                    new OracleParameter("pindex", OracleDbType.Int32), 
                    new OracleParameter("psql", OracleDbType.Varchar2,2000),
                    new OracleParameter("psize", OracleDbType.Int32),
                    new OracleParameter("pcount", OracleDbType.Int32),
                    new OracleParameter("prowcount", OracleDbType.Int32),
                    new OracleParameter("v_cur", OracleDbType.RefCursor) };
                param[0].Value = index;
                param[1].Value = sql;
                param[2].Value = pageSize;

                param[0].Direction = ParameterDirection.Input;
                param[1].Direction = ParameterDirection.Input;
                param[2].Direction = ParameterDirection.Input;
                param[3].Direction = ParameterDirection.Output;
                param[4].Direction = ParameterDirection.Output;
                param[5].Direction = ParameterDirection.Output;
                result.DataTable = SMT.OracleDataProvider.ExecuteDataTableByPaging("SMT_P_PAGE.SMT_Paging", param);

                pageCount = int.Parse(param[3].Value.ToString());
                rowCount = int.Parse(param[4].Value.ToString());

            }
            catch (OracleException on)
            {
                pageCount = 0;
                rowCount = 0;
                result.Error = on.Message;
                //throw new Exception(on.Message);
            }
            return result;
            #endregion     
       
        }
        //public static DataTable BindPaging(int index, int pageSize)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("SELECT * FROM  ");
        //    sb.AppendLine("(  SELECT A.*, ROWNUM RN  FROM ");
        //    sb.AppendLine("       (SELECT * FROM ms_student where 1=1 order by userid) A ");
        //    sb.AppendLine("        WHERE ROWNUM <=(" + index + ")*" + pageSize + "  ) ");
        //    sb.AppendLine("              WHERE RN > (" + index + "-1)*" + pageSize + "");
        //    string strSql = sb.ToString();
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        dt = SMT.DataProvider.GetDataTable(strSql).DataTable;
        //    }
        //    catch (Oracle.DataAccess.Client.OracleException on)
        //    {
        //        throw new Exception(on.Message);
        //    }
        //    return dt;
        //}
        //public static DataTable BindPaging(string tableName, int index, int pageSize, string strWhere, string orderbyString)
        //{
        //    string where = "";
        //    string orderby = "";
        //    if (!string.IsNullOrEmpty(strWhere))
        //    {
        //        where = strWhere;
        //    }
        //    if (!string.IsNullOrEmpty(orderbyString))
        //    {
        //        orderby = orderbyString;
        //    }
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("SELECT * FROM  ");
        //    sb.AppendLine("(  SELECT A.*, ROWNUM RN  FROM ");
        //    sb.AppendLine("       (SELECT * FROM " + tableName + " " + where + " " + orderby + ") A ");
        //    sb.AppendLine("        WHERE ROWNUM <=(" + index + ")*" + pageSize + "  ) ");
        //    sb.AppendLine("              WHERE RN > (" + index + "-1)*" + pageSize + "");
        //    string strSql = sb.ToString();
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        dt = SMT.DataProvider.GetDataTable(strSql).DataTable;
        //    }
        //    catch (Oracle.DataAccess.Client.OracleException on)
        //    {
        //        throw new Exception(on.Message);
        //    }
        //    return dt;
        //}
    }
}


//create or replace package body SMT_P_page is
//  procedure SMT_Paging(Pindex in number, --要显示的页数索引，从0开始
//                       Psql in varchar2, --产生分页数据的查询语句
//                       Psize in number, --每页显示记录数
//                       Pcount out number, --返回的分页数
//                       Prowcount out number, --返回的记录数
//                       v_cur out type_cur --返回分页数据的游标
//                       ) AS
//    v_sql VARCHAR2(1000);
//    v_Pbegin number;
//    v_Pend number;
//  begin
//    v_sql := 'select count(*) from (' || Psql || ')';
//    execute immediate v_sql into Prowcount; --计算记录总数
//    Pcount := ceil(Prowcount / Psize); --计算分页总数
//    --显示任意页内容
//    v_Pend := Pindex * Psize + Psize;
//    v_Pbegin := v_Pend - Psize + 1;
//    -- v_sql :=  'select * from (' || Psql || ') where rownum between ' || v_Pbegin || ' and ' || v_Pend;
//         v_sql :=  'select * from (select a.*,rownum rn from (' || Psql || ') a where rownum <= ' || v_Pend || ') b where b.rn>='  || v_Pbegin ;


//         --v_sql :=  'select * from(select * from tb_exp_yddd) where  rownum between 1 AND 10';
//    open v_cur for v_sql;
//  end SMT_Paging;
//end SMT_P_page;




//create or replace package SMT_P_page is
//type type_cur is ref cursor;                    --定义游标变量用于返回记录集
//procedure SMT_Paging  (Pindex in number,        --要显示的页数索引，从0开始
//                       Psql in varchar2,        --产生分页数据的查询语句
//                       Psize in number,         --每页显示记录数
//                       Pcount out number,       --返回的分页数
//                       Prowcount out number,    --返回的记录数
//                       v_cur out type_cur      --返回分页数据的游标
//                       );
//end SMT_P_page;

//create or replace package SMT_Paging is
//type typeCursor is ref cursor;                    --定义游标变量用于返回记录集
//procedure SMT_Pagination  (Pindex in number,        --要显示的页数索引，从0开始
//                       SqlString in varchar2,        --产生分页数据的查询语句
//                       PageSize in number,         --每页显示记录数
//                       PageCount out number,       --返回的分页数
//                       RecordCount out number,    --返回的记录数
//                       DataCursor out typeCursor      --返回分页数据的游标
//                       );
//end SMT_Paging;
//--定义包主体
//create or replace package body SMT_Paging is
//  procedure SMT_Pagination(Pindex in number, --要显示的页数索引，从0开始
//                       SqlString in varchar2, --产生分页数据的查询语句
//                       PageSize in number, --每页显示记录数
//                       PageCount out number, --返回的分页数
//                       RecordCount out number, --返回的记录数
//                       DataCursor out typeCursor --返回分页数据的游标
//                       ) AS
//    v_sql VARCHAR2(1000);
//    v_Pbegin number;
//    v_Pend number;
//  begin
//    v_sql := 'select count(*) from (' || SqlString || ')';
//    execute immediate v_sql into RecordCount; --计算记录总数
//    PageCount := ceil(RecordCount / PageSize); --计算分页总数
//    --显示任意页内容
//    v_Pend := Pindex * PageSize + PageSize;
//    v_Pbegin := v_Pend - PageSize + 1;     
//    v_sql :=  'select * from (' || SqlString || ') where rn between ' || v_Pbegin || ' and ' || v_Pend;          
//    open DataCursor for v_sql;
//  end SMT_Pagination;  
//end SMT_Paging;
