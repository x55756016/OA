using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMT.Foundation.Core;
using System.Configuration;
using System.Data;

namespace SMT.FB.Services
{
    public partial class SubjectRectifyNext : System.Web.UI.Page
    {
        private static string conn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 执行sql方法
        /// </summary>
        /// <param name="Sqlstring"></param>
        /// <returns></returns>
        public object ExecuteCustomerSql(string Sqlstring)
        {
            OracleDAO dao = new OracleDAO(conn);
            object obj = dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text);
            return obj;
        }
        #region "数据库基本操作"

        protected DataTable GetDataTable(string Sqlstring, OracleDAO dao)
        {
            Sqlstring.Replace("\n", "");
            Sqlstring.Replace("\r", "");
            DataTable obj = dao.GetDataTable(Sqlstring, System.Data.CommandType.Text);
            return obj;
        }
        #endregion

        /// <summary>
        /// 查询视图smtfb.accountd得到数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCheck_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            string strSql = string.Empty;
            if (tbSub.Text.Trim() == "" || tbSub.Text.Trim() == null)
            {
                strSql = "select * from smtfb.accountd t where t.OWNERCOMPANYNAME = '" + tbCom.Text + "'  and t.OWNERDEPARTMENTNAME = '" + tbDep.Text + "'";
            }
            else
            {
                 strSql = "select * from smtfb.accountd t where t.OWNERCOMPANYNAME = '" + tbCom.Text + "'  and t.OWNERDEPARTMENTNAME = '" + tbDep.Text + "' and t.subjectname = '" + tbSub.Text + "'";
            }
            ExecuteCustomerSql(strSql);
            DataTable dsTotal = new DataTable();
            dsTotal = this.GetDataTable(strSql, dao);

            accountd.DataSource = dsTotal;
            accountd.DataBind();
            string ownercompanyid = dsTotal.Rows[0]["OWNERCOMPANYID"].ToString();
            string ownerdepartmentid = dsTotal.Rows[0]["OWNERDEPARTMENTID"].ToString();
            string subjectid = dsTotal.Rows[0]["SUBJECTID"].ToString();


            tbComID.Text = dsTotal.Rows[0]["OWNERCOMPANYID"].ToString();
            tbDepID.Text = dsTotal.Rows[0]["OWNERDEPARTMENTID"].ToString();
            tbSubID.Text = dsTotal.Rows[0]["SUBJECTID"].ToString();
            tbBG.Text = dsTotal.Rows[0]["BUDGETACCOUNTID"].ToString();
        }
        //各种字段
        string BUDGETACCOUNTID = string.Empty, OWNERCOMPANYID = string.Empty, OWNERDEPARTMENTID = string.Empty, 
            OWNERID = string.Empty, OWNERPOSTID = string.Empty, SUBJECTID = string.Empty,
            CREATEUSERID = string.Empty, UPDATEUSERID = string.Empty, REMARK = string.Empty;
        int ACCOUNTOBJECTTYPE = 0, BUDGETYEAR = 0, BUDGETMONTH = 0;
        float BUDGETMONEY = 0, USABLEMONEY = 0, ACTUALMONEY = 0, PAIEDMONEY = 0;
        DateTime CREATEDATE, UPDATEDATE;

        /// <summary>
        /// 根据公司ownercompanyid部门ownerdepartmentid科目subjectid去找总账表t_Fb_Budgetaccount信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnBudget_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            string strSql = "select * from smtfb.t_Fb_Budgetaccount t where t.ownercompanyid = '" + tbComID.Text + "'  and t.ownerdepartmentid = '" + tbDepID.Text + "' and t.subjectid = '" + tbSubID.Text + "'";
            ExecuteCustomerSql(strSql);
            DataTable dsTotal = new DataTable();
            dsTotal = this.GetDataTable(strSql, dao);

            t_Fb_Budgetaccount.DataSource = dsTotal;
            t_Fb_Budgetaccount.DataBind();

            ///有用没用都取出来先
            BUDGETACCOUNTID = Convert.ToString(dsTotal.Rows[0]["BUDGETACCOUNTID"]);
            ACCOUNTOBJECTTYPE = Convert.ToInt32(dsTotal.Rows[0]["ACCOUNTOBJECTTYPE"]);
            BUDGETYEAR = Convert.ToInt32(dsTotal.Rows[0]["BUDGETYEAR"]);
            BUDGETMONTH = Convert.ToInt32(dsTotal.Rows[0]["BUDGETMONTH"]);
            OWNERCOMPANYID = Convert.ToString(dsTotal.Rows[0]["OWNERCOMPANYID"]);
            OWNERDEPARTMENTID = Convert.ToString(dsTotal.Rows[0]["OWNERDEPARTMENTID"]);
            OWNERID = Convert.ToString(dsTotal.Rows[0]["OWNERID"]);
            OWNERPOSTID = Convert.ToString(dsTotal.Rows[0]["OWNERPOSTID"]);
            SUBJECTID = Convert.ToString(dsTotal.Rows[0]["SUBJECTID"]);
            BUDGETMONEY = Convert.ToInt32(dsTotal.Rows[0]["BUDGETMONEY"]);
            USABLEMONEY = Convert.ToInt32(dsTotal.Rows[0]["USABLEMONEY"]);
            ACTUALMONEY = Convert.ToInt32(dsTotal.Rows[0]["ACTUALMONEY"]);
            PAIEDMONEY = Convert.ToInt32(dsTotal.Rows[0]["PAIEDMONEY"]);
            CREATEUSERID = Convert.ToString(dsTotal.Rows[0]["CREATEUSERID"]);
            CREATEDATE = Convert.ToDateTime(dsTotal.Rows[0]["CREATEDATE"]);
            UPDATEUSERID = Convert.ToString(dsTotal.Rows[0]["UPDATEUSERID"]);
            UPDATEDATE = Convert.ToDateTime(dsTotal.Rows[0]["UPDATEDATE"]);
            REMARK = Convert.ToString(dsTotal.Rows[0]["REMARK"]);
            tbcreate.Text = CREATEDATE.ToString();
            tbupdate.Text = UPDATEDATE.ToString();
        }

        
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnInsert_Click(object sender, EventArgs e)
        {
            string strSql = @"insert into smtfb.t_Fb_Budgetaccount (BUDGETACCOUNTID, ACCOUNTOBJECTTYPE, BUDGETYEAR, BUDGETMONTH, 
OWNERCOMPANYID, OWNERDEPARTMENTID, OWNERID, OWNERPOSTID, 
SUBJECTID, BUDGETMONEY, USABLEMONEY, ACTUALMONEY, PAIEDMONEY, 
CREATEUSERID, CREATEDATE, UPDATEUSERID, UPDATEDATE, REMARK) 
            values ('@BUDGETACCOUNTID', 2, 2013, @BUDGETMONTH, 
'@OWNERCOMPANYID', '@OWNERDEPARTMENTID', null, null, '@SUBJECTID', 
@BUDGETMONEY, @USABLEMONEY, @ACTUALMONEY, 0, '001', 
to_date('@CREATEDATE', 'dd-mm-yyyy hh24:mi:ss'), '001', to_date('@UPDATEDATE', 'dd-mm-yyyy hh24:mi:ss'), '@REMARK')";

            strSql = strSql.Replace("@BUDGETACCOUNTID",Guid.NewGuid().ToString());
            strSql = strSql.Replace("@BUDGETMONTH", tbmonth.Text);
            strSql = strSql.Replace("@OWNERCOMPANYID", tbComID.Text);
            strSql = strSql.Replace("@OWNERDEPARTMENTID",tbDepID.Text);
            strSql = strSql.Replace("@SUBJECTID", tbSubID.Text);
            strSql = strSql.Replace("@BUDGETMONEY",tbbm.Text);
            strSql = strSql.Replace("@USABLEMONEY",tbuser.Text);
            strSql = strSql.Replace("@ACTUALMONEY",tbact.Text);
            string strcreate = Convert.ToDateTime(tbcreate.Text).ToString("dd-MM-yyyy HH:mm:ss");//这里奇怪要转换几次才行
            string strupdate = Convert.ToDateTime(tbupdate.Text).ToString("dd-MM-yyyy HH:mm:ss");
            strSql = strSql.Replace("@CREATEDATE", strcreate);
            strSql = strSql.Replace("@UPDATEDATE", strupdate);
            strSql = strSql.Replace("@REMARK", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "手动添加");
            ExecuteCustomerSql(strSql);
        }

        /// <summary>
        /// 查找部门流水以查看该部门是否做月度预算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btCheckzb_dept_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            string strSql = "select * from smtfb.zb_dept t where t.OWNERCOMPANYNAME = '" + tbCom.Text + "'  and t.OWNERDEPARTMENTNAME = '" + tbDep.Text + "' and t.subjectname = '" + tbSub.Text + "'" + "and extract(year from t.BUDGETARYMONTH) = 2013";
            ExecuteCustomerSql(strSql);
            DataTable dsTotal = new DataTable();
            dsTotal = this.GetDataTable(strSql, dao);

            zb_dept.DataSource = dsTotal;
            //if (zb_dept.Rows.Count < 1)
            //{
            //    Response.Write("<script language=javascript>alert('没有流水！');</script>");
            //    zb_dept.DataSource = null;
            //    zb_dept.DataBind();
            //    return;
            //}
            zb_dept.DataBind();
        }

        /// <summary>
        /// 用于跨年年结后科目没有清零的情况，慎用！！！！！！
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClear_Click(object sender, EventArgs e)
        {
            string strSql = "update smtfb.t_Fb_Budgetaccount t set  t.usablemoney = 0, t.actualmoney = 0,t.budgetmoney=0 where t.budgetaccountid = '@budgetaccountid'";
            strSql = strSql.Replace("@budgetaccountid", tbBG.Text);
            ExecuteCustomerSql(strSql);
        }
        /// <summary>
        /// 查找部门流水以查看该部门是否做月度预算（增强版）(只根据公司部门没有预算科目)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btCheckNew_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            string strSql = "select * from smtfb.zb_dept t where t.OWNERCOMPANYNAME = '" + tbCom.Text + "'  and t.OWNERDEPARTMENTNAME = '" + tbDep.Text + "'" + "and extract(year from t.BUDGETARYMONTH) = 2013";
            ExecuteCustomerSql(strSql);
            DataTable dsTotal = new DataTable();
            dsTotal = this.GetDataTable(strSql, dao);

            zb_deptNew.DataSource = dsTotal;
            //if (zb_deptNew.Rows.Count < 1)
            //{
            //    Response.Write("<script language=javascript>alert('没有流水！');</script>");
            //    zb_deptNew.DataSource = null;
            //    zb_deptNew.DataBind();
            //    lbflag.Text = "没有流水";
            //    return;
            //}
            //else
            //{
            //    lbflag.Text = "有流水，不能清零";            
            //}
            zb_deptNew.DataBind();
        }

        /// <summary>
        /// 用于跨年年结后科目没有清零的情况（增强版），更加慎用！！！！！！
        /// 除去活动经费和部门经费
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClearNew_Click(object sender, EventArgs e)
        {
            if (lbflag.Text == "有流水，不能清零")
            {
                Response.Write("<script language=javascript>alert('有流水，不能清零！');</script>");
                return;
            }
            else
            {
                string strSql = "update smtfb.t_Fb_Budgetaccount t set  t.usablemoney = 0, t.actualmoney = 0,t.budgetmoney=0 where t.OWNERCOMPANYID = '@OWNERCOMPANYID' and t.OWNERDEPARTMENTID = '@OWNERDEPARTMENTID' and t.BUDGETYEAR=2013 and t.BUDGETMONTH=1 and t.accountobjecttype <> 1 and t.SUBJECTID not in('08c1d9c6-2396-43c3-99f9-227e4a7eb417','d5134466-c207-44f2-8a36-cf7b96d5851f')";
                strSql = strSql.Replace("@OWNERCOMPANYID", tbComID.Text);
                strSql = strSql.Replace("@OWNERDEPARTMENTID", tbDepID.Text);
                ExecuteCustomerSql(strSql);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btdelete_Click(object sender, EventArgs e)
        {
            string strSql = "delete smtfb.t_Fb_Budgetaccount t where t.budgetaccountid='@budgetaccountid'";
            strSql = strSql.Replace("@budgetaccountid", tbdelete.Text);
            ExecuteCustomerSql(strSql);
        }

       


    }
}