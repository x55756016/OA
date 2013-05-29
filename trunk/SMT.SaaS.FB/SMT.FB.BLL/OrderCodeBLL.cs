
/*
 * 文件名：OrderCodeBLL.cs
 * 作  用：单据编号重复问题
 * 创建人：向寒咏
 * 创建时间：2012-04-09 11:47:04
 * 修改人：
 * 修改时间：
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using SMT_FB_EFModel;
using SMT.FB.BLL;
using SMT.FB.DAL;
using SMT.Foundation.Log;

namespace SMT.FBEntityBLL.BLL
{
    public class OrderCodeBLL:BaseBLL
    {
        public OrderCodeBLL()
        { }

        #region 生成单据编号
        /// <summary>
        /// 获取单据编号
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string GetAutoOrderCode(string tablename)
        {
            string strclear = string.Empty;
            try
            {
                string strRes = string.Empty;
                if (string.IsNullOrWhiteSpace(tablename))
                {
                    return strRes;
                }
                var ents = from n in GetTable<T_FB_ORDERCODE>()
                           where n.TABLENAME == tablename
                           select n;
                T_FB_ORDERCODE orderCode = ents.FirstOrDefault();
                if (orderCode == null)
                {
                    return strRes;
                }
                string shortName = orderCode.PRENAME;
                DateTime CurrentDate = orderCode.CURRENTDATE.Value;

                using (BaseDAL dal = new BaseDAL())
                {
                    Tracer.Debug("模块代码：" + tablename);
                    switch (tablename)
                    {
                        case "T_FB_COMPANYBUDGETAPPLYMASTER":
                            tablename = "T_FB_COMPANYBUDGETAPPLY";//名称太长导致无法创建sequence
                            break;
                        case "T_FB_COMPANYBUDGETMODMASTER":
                            tablename = "T_FB_COMPANYBUDGETMOD";//名称太长导致无法创建sequence
                            break;
                        case "T_FB_COMPANYBUDGETSUMMASTER":
                            tablename = "T_FB_COMPANYBUDGETSUM";//名称太长导致无法创建sequence
                            break;
                        case "T_FB_PERSONMONEYASSIGNMASTER":
                            tablename = "T_FB_PERSONMONEYASSIGN";//名称太长导致无法创建sequence
                            break;
                        default:
                            break;
                    }
                    if (CurrentDate.Date != System.DateTime.Now.Date)
                    {
                        orderCode.CURRENTDATE = System.DateTime.Now.Date;
                        orderCode.RUNNINGNUMBER = 1;
                        strclear = @"declare n number(10); tsql   varchar2(1000); begin select "
                                            + tablename + "_SEQ.nextval   into   n   from   dual; "
                                            + " n:=-(n);"
                                            + " tsql:= 'alter   sequence " + tablename + "_SEQ   increment   by '||   n;"
                                            + " execute   immediate   tsql; "
                                            + "  select " + tablename + "_SEQ.nextval   into   n   from   dual; "
                                            + "  tsql:= 'alter   sequence " + tablename + "_SEQ   increment   by   1 '; "
                                            + "  execute   immediate   tsql; "
                                            + "  end; ";
                        Tracer.Debug("ExecuteCustomerSql：" + strclear);
                        dal.ExecuteCustomerSql(strclear);
                    }
                    decimal curNumber = 0;
                    string strNextSqense = @" select " + tablename + "_SEQ.nextval from dual";
                    Tracer.Debug("调用新生成单号接口：tablename：" + tablename + " ExecuteCustomerSql:" + strNextSqense);
                    curNumber = decimal.Parse(dal.ExecuteCustomerSql(strNextSqense).ToString());

                    string strDate = orderCode.CURRENTDATE.Value.ToString("yyyyMMdd");
                    string code = shortName + "_" + strDate + curNumber.ToString().PadLeft(6, '0');
                    Tracer.Debug("生成单号：" + code);
                    orderCode.RUNNINGNUMBER = curNumber + 1;
                    Update(orderCode);
                    return code;
                }

            }
            catch (Exception ex)
            {
                Tracer.Debug("ExecuteCustomerSql：" + strclear + ex.ToString());
                throw (ex);
            }
        }

        /// <summary>
        /// 获取传入实体的表名/实体类名
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string GetTableName(EntityObject entity)
        {
            string tableName = entity.GetType().Name;
            return tableName;
        }
        #endregion
    }
}

