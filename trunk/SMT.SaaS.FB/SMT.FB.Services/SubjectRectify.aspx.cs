using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data.OleDb;
using System.Data;
using System.IO;
using SMT.Foundation.Core;
using System.Configuration;
using SMT_FB_EFModel;
using SMT.FB.BLL;

namespace SMT.FB.Services
{
    public partial class SubjectRectify : System.Web.UI.Page
    {

        //OleDbDataAccess dbc = new OleDbDataAccess();
        
        private static string conn = ConfigurationManager.ConnectionStrings["ConnectiongString"].ToString();
       
        protected void Page_Load(object sender, EventArgs e)
        {
            Label3.Text = "";
            Label4.Text = "";
            if (!IsPostBack)
            {
                int n = int.Parse(DateTime.Now.ToString("yyyy"));
                for (int i = 0; i < 5; i++)
                {
                    ListItem li = new ListItem((n - i).ToString(), (i + 1).ToString());
                    DropDownList2.Items.Insert(0, li);
                    if (DropDownList2.Items[i].Text == n.ToString())
                    {
                        DropDownList2.SelectedValue = DropDownList2.Items[i].Value;
                    }
                }
            }          
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button1_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            string strdate=string .Empty ;
            if(DropDownList2.SelectedItem.Value !="0")
                strdate=" and  to_char((t.BUDGETARYMONTH),'YYYY') ='"+DropDownList2.SelectedItem.Text+"' ";

             string strsql = "";
            if(DropDownList1.SelectedValue=="1")
                strsql = "select t.ORDERTYPENAME as 单据类型,t.budgetarymonth as 时间,t.checkstatesname as 审核状态,t.ordercode as 单号,t.subjectname as 科目名称,t.usablemoney as 可用金额,t.budgetmoney as 实际金额,t.createusername as 创建人,t.ownerdepartmentname as 部门,t.ownercompanyname as 公司,t.OWNERDEPARTMENTID as 部门ID, t.OWNERCOMPANYID as 公司ID,t.subjectid as 科目ID from zzz_comp t where t.OWNERCOMPANYNAME='" + TextBox2.Text + "' and t.OWNERDEPARTMENTNAME='" + TextBox3.Text + "' and t.SUBJECTNAME = '" + TextBox1.Text + "'  " + strdate + "  order by t.budgetarymonth";
            else  if(DropDownList1.SelectedValue=="2")
                strsql = "select t.ORDERTYPENAME as 单据类型,t.budgetarymonth as 时间,t.checkstatesname as 审核状态,t.ordercode as 单号,t.subjectname as 科目名称,t.usablemoney as 可用金额,t.budgetmoney as 实际金额,t.createusername as 创建人,t.ownerdepartmentname as 部门,t.ownercompanyname as 公司,t.OWNERDEPARTMENTID as 部门ID, t.OWNERCOMPANYID as 公司ID,t.subjectid as 科目ID from zb_dept t where t.OWNERDEPARTMENTNAME='" + TextBox3.Text + "' and t.OWNERCOMPANYNAME='" + TextBox2.Text + "' and t.SUBJECTNAME = '" + TextBox1.Text + "' " + strdate + " order by t.budgetarymonth";
            else if (DropDownList1.SelectedValue == "3")
                strsql = "select t.ORDERTYPENAME as 单据类型,t.budgetarymonth as 时间,t.checkstatesname as 审核状态,t.ordercode as 单号,t.subjectname as 科目名称,t.usablemoney as 可用金额,t.budgetmoney as 实际金额,t.ownername as 个人姓名,t.ownerdepartmentname as 部门,t.ownercompanyname as 公司,t.ownerid as 个人ID, t.OWNERCOMPANYID as 公司ID,t.subjectid as 科目ID from zb_person t where t.ownername='" + TextBox4.Text + "' and t.OWNERCOMPANYNAME='" + TextBox2.Text + "' and t.SUBJECTNAME = '" + TextBox1.Text + "'  " + strdate + "  order by t.budgetarymonth";
           
           //OleDbDataAdapter ada= dbc.ExecuteAdapter(strsql);
           DataTable dtOrder = new DataTable();
           dtOrder = this.GetDataTable(strsql, dao);

           order.DataSource = dtOrder;
           order.DataBind();
            
           if (order.Rows.Count < 1)
           {
               Response.Write("<script language=javascript>alert('没有查到你要的数据！');</script>");
               total.DataSource = null;
               total.DataBind();
               return;
           }
             string strtotal = "";
             if (DropDownList1.SelectedValue == "1")
                 strtotal = "select t.subjectname as 科目名称,t.accountobjecttype as 预算类型,t.budgetyear as 年份,t.budgetmonth as 月份,t.budgetmoney as 预算额度,t.usablemoney as 可用结余,t.actualmoney as 实际结余,t.paiedmoney as 费用,t.updatedate as 更新时间  from budgetaccount t where t.ownerdepartmentid='" + dtOrder.Rows[0][10].ToString() + "' and t.ownercompanyid='" + dtOrder.Rows[0][11].ToString() + "' and t.SUBJECTNAME = '" + TextBox1.Text + "' and t.accountobjecttype=" + DropDownList1.SelectedValue;
             else if (DropDownList1.SelectedValue == "2")
                 strtotal = "select t.subjectname as 科目名称,t.accountobjecttype as 预算类型,t.budgetyear as 年份,t.budgetmonth as 月份,t.budgetmoney as 预算额度,t.usablemoney as 可用结余,t.actualmoney as 实际结余,t.paiedmoney as 费用,t.updatedate as 更新时间  from budgetaccount t where t.ownerdepartmentid='" + dtOrder.Rows[0][10].ToString() + "' and t.ownercompanyid='" + dtOrder.Rows[0][11].ToString() + "' and t.SUBJECTNAME = '" + TextBox1.Text + "' and t.accountobjecttype=" + DropDownList1.SelectedValue;
             else if (DropDownList1.SelectedValue == "3")
                 strtotal = "select t.subjectname as 科目名称,t.accountobjecttype as 预算类型,t.budgetyear as 年份,t.budgetmonth as 月份,t.budgetmoney as 预算额度,t.usablemoney as 可用结余,t.actualmoney as 实际结余,t.paiedmoney as 费用,t.updatedate as 更新时间  from budgetaccount t where t.ownerid='" + dtOrder.Rows[0][10].ToString()+"' ";
             strtotal += "and t.ownercompanyid='" + dtOrder.Rows[0][11].ToString() + "' ";
                strtotal     +="and t.SUBJECTNAME = '" + TextBox1.Text + "' and t.accountobjecttype=" + DropDownList1.SelectedValue;
                       
           //ada = dbc.ExecuteAdapter(strtotal);
           DataTable dsTotal = new DataTable();
           dsTotal = this.GetDataTable(strtotal,dao);
           total.DataSource = dsTotal;
           total.DataBind();

           Decimal um = 0;
           Decimal am = 0;
           for (int i = 0; i < order.Rows.Count; i++)
           {
               if (dtOrder.Rows[i][2].ToString() == "审核通过")
               {
                   um += Decimal.Parse(dtOrder.Rows[i][6].ToString());
                   am += Decimal.Parse(dtOrder.Rows[i][6].ToString());
               }
               if (dtOrder.Rows[i][2].ToString() == "审核中" || dtOrder.Rows[i][2].ToString() == "审核中或未汇总")
               {
                   if (dtOrder.Rows[i][0].ToString() == "预算分配(调入)" ||dtOrder.Rows[i][0].ToString() == "月度部门调拨(调入)" || dtOrder.Rows[i][0].ToString() == "个人调拨(调入)" || dtOrder.Rows[i][0].ToString() == "个人月度部门预算" || dtOrder.Rows[i][0].ToString() == "月度部门预算" || dtOrder.Rows[i][0].ToString() == "月度部门增补")
                   {
                   //上面这些单可用的要审核通过后才算
                   }
                   else
                   {
                       um += Decimal.Parse(dtOrder.Rows[i][6].ToString());
                   }
               }
           }
           TextBox6.Text = um.ToString ();
           TextBox7.Text = am.ToString();
           Label1.Text = um.ToString();
           Label2.Text = am.ToString();
        }

        /// <summary>
        /// 修改可用结余
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button3_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
           Decimal um=0;
           if (Decimal.TryParse(TextBox6.Text.Trim(), out um))
           {
               if (total.Rows.Count != 1)
               {
                   if (TextBox1.Text == "活动经费")
                   {
                   
                   }
                   else
                   {
                       Response.Write("<script language=javascript>alert('汇总记录不是一条,不能修改可用结余！');</script>");
                       return;
                   }
               }
                string strupdate = "";
                 if(DropDownList1.SelectedValue=="1")
                    strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + order.Rows[0].Cells[12].Text + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + order.Rows[0].Cells[11].Text + "'";
                 else if (DropDownList1.SelectedValue == "2")
                     strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + order.Rows[0].Cells[12].Text + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + order.Rows[0].Cells[11].Text + "'  and t.ownerdepartmentid='" + order.Rows[0].Cells[10].Text + "'";
                 else if (DropDownList1.SelectedValue == "3")
                     strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + order.Rows[0].Cells[12].Text + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + order.Rows[0].Cells[11].Text + "'  and t.ownerid='" + order.Rows[0].Cells[10].Text + "'";
              
               
               int n =this.ExecuteNonQuery(strupdate,dao);
               if (n > 0)
               {                   
                   Label3.Text = "更新成功！";
               }
               else
               {
                   Label3.Text = "可用结余更新失败！";
               }
           }
           else
           {
               Label3.Text = "可用结余不是有效的数字！！！";
           }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            string strsql = TextBox5.Text .Trim ();          
            //OleDbDataAdapter ada = dbc.ExecuteAdapter(strsql);
            DataTable ds = new DataTable();
            ds = this.GetDataTable(strsql,dao);
            order.DataSource = ds;
            order.DataBind();
        }

        /// <summary>
        /// 修改实际结余
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Button4_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            Decimal am = 0;
            if (Decimal.TryParse(TextBox7.Text.Trim(), out am))
            {
                if (total.Rows.Count > 1)
                {
                    Response.Write("<script language=javascript>alert('汇总记录大于一条,不能修改实际额度！');</script>");
                    return;
                }
                string strupdate = "";
                if (DropDownList1.SelectedValue == "1")
                    strupdate = "update t_fb_budgetaccount t set t.actualmoney=" + am + " where t.subjectid='" + order.Rows[0].Cells[12].Text + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + order.Rows[0].Cells[11].Text + "'";
                else if (DropDownList1.SelectedValue == "2")
                    strupdate = "update t_fb_budgetaccount t set t.actualmoney=" + am + " where t.subjectid='" + order.Rows[0].Cells[12].Text + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + order.Rows[0].Cells[11].Text + "'  and t.ownerdepartmentid='" + order.Rows[0].Cells[10].Text + "'";
                else if (DropDownList1.SelectedValue == "3")
                    strupdate = "update t_fb_budgetaccount t set t.actualmoney=" + am + " where t.subjectid='" + order.Rows[0].Cells[12].Text + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + order.Rows[0].Cells[11].Text + "'  and t.ownerid='" + order.Rows[0].Cells[10].Text + "'";


            


                int n = (int)this.ExecuteNonQuery(strupdate,dao);
                if (n > 0)
                {
                    Label4.Text = "更新成功！";
                }
                else
                {
                    Label4.Text = "实际额度更新失败！";
                }
            }
            else
            {
                Label3.Text = "实际额度不是有效的数字！！！";
            }
        }

        protected object ExecuteCustomerSql(string Sqlstring, SMT.Foundation.Core.ParameterCollection prameters)
        {
            Sqlstring.Replace("\n", "");
            Sqlstring.Replace("\r", "");
            OracleDAO dao = new OracleDAO(conn);
            return dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text, prameters);
        }


        protected void Button5_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
         //   Response.Write("<script language='javascript'>if(confirm('是否批量更新?')){alert('确定')}else{alert('取消')};</script>");
           
          //  string strsql = "select t.ORDERTYPENAME as 单据类型,t.budgetarymonth as 时间,t.checkstatesname as 审核状态,t.ordercode as 单号,t.subjectname as 科目名称,t.usablemoney as 可用金额,t.budgetmoney as 实际金额,t.createusername as 创建人,t.ownerdepartmentname as 部门,t.ownercompanyname as 公司,t.OWNERDEPARTMENTID as 部门ID, t.OWNERCOMPANYID as 公司ID,t.subjectid as 科目ID from zzz_dept t where t.usablemoney!=999999 order by t.OWNERCOMPANYNAME,t.ownerdepartmentname ,t.subjectname";
            string strdate = string.Empty;
            if (DropDownList2.SelectedItem.Value != "0")
                strdate = " where  to_char((t.BUDGETARYMONTH),'YYYY') ='" + DropDownList2.SelectedItem.Text + "' ";
            string strsql = "";
            if (DropDownList1.SelectedValue == "1")
                strsql = "select t.ORDERTYPENAME as 单据类型,t.budgetarymonth as 时间,t.checkstatesname as 审核状态,t.ordercode as 单号,t.subjectname as 科目名称,t.usablemoney as 可用金额,t.budgetmoney as 实际金额,t.createusername as 创建人,t.ownerdepartmentname as 部门,t.ownercompanyname as 公司,t.OWNERDEPARTMENTID as 部门ID, t.OWNERCOMPANYID as 公司ID,t.subjectid as 科目ID from zzz_comp t  " + strdate + "  order by t.OWNERCOMPANYNAME,t.ownerdepartmentname ,t.subjectname";
            else if (DropDownList1.SelectedValue == "2")
                strsql = "select t.ORDERTYPENAME as 单据类型,t.budgetarymonth as 时间,t.checkstatesname as 审核状态,t.ordercode as 单号,t.subjectname as 科目名称,t.usablemoney as 可用金额,t.budgetmoney as 实际金额,t.createusername as 创建人,t.ownerdepartmentname as 部门,t.ownercompanyname as 公司,t.OWNERDEPARTMENTID as 部门ID, t.OWNERCOMPANYID as 公司ID,t.subjectid as 科目ID from zzz_dept t  " + strdate + " and t.subjectid!='08c1d9c6-2396-43c3-99f9-227e4a7eb417' and t.subjectid!='d5134466-c207-44f2-8a36-cf7b96d5851f' order by t.OWNERCOMPANYID,t.ownerdepartmentname ,t.OWNERDEPARTMENTID ,t.subjectid";//and t.subjectid='08c1d9c6-2396-43c3-99f9-227e4a7eb417'  部门经费
               // strsql = "select t.ORDERTYPENAME as 单据类型,t.budgetarymonth as 时间,t.checkstatesname as 审核状态,t.ordercode as 单号,t.subjectname as 科目名称,t.usablemoney as 可用金额,t.budgetmoney as 实际金额,t.createusername as 创建人,t.ownerdepartmentname as 部门,t.ownercompanyname as 公司,t.OWNERDEPARTMENTID as 部门ID, t.OWNERCOMPANYID as 公司ID,t.subjectid as 科目ID from zzz_dept t  " + strdate + " where t.subjectid='08c1d9c6-2396-43c3-99f9-227e4a7eb417' order by t.OWNERCOMPANYID,t.ownerdepartmentname ,t.OWNERDEPARTMENTID ,t.subjectid";
            else if (DropDownList1.SelectedValue == "3")
                strsql = "select t.ORDERTYPENAME as 单据类型,t.budgetarymonth as 时间,t.checkstatesname as 审核状态,t.ordercode as 单号,t.subjectname as 科目名称,t.usablemoney as 可用金额,t.budgetmoney as 实际金额,t.ownername as 个人姓名,t.ownerdepartmentname as 部门,t.ownercompanyname as 公司,t.ownerid as 个人ID, t.OWNERCOMPANYID as 公司ID,t.subjectid as 科目ID from zzz_person t  " + strdate + " where t.subjectid in (select distinct(subjectid) from t_fb_subjectcompany t where t.controltype=3) order by t.OWNERCOMPANYNAME,t.subjectname,t.ownername";
              // strsql = "select t.ORDERTYPENAME as 单据类型,t.budgetarymonth as 时间,t.checkstatesname as 审核状态,t.ordercode as 单号,t.subjectname as 科目名称,t.usablemoney as 可用金额,t.budgetmoney as 实际金额,t.ownername as 个人姓名,t.ownerdepartmentname as 部门,t.ownercompanyname as 公司,t.ownerid as 个人ID, t.OWNERCOMPANYID as 公司ID,t.subjectid as 科目ID from zzz_person t  " + strdate + "  order by t.OWNERCOMPANYNAME,t.subjectname,t.ownername";//where t.subjectid='d5134466-c207-44f2-8a36-cf7b96d5851f' 活动经费
            
            //OleDbDataAdapter ada1 = dbc.ExecuteAdapter(strsql);
            DataTable ds1 = new DataTable();
            DataTable ds2 = new DataTable();
            ds1 = GetDataTable(strsql,dao);
            order.DataSource = ds1;
            order.DataBind();
            Decimal um = 0;           
            string oldsubjectname = string.Empty;
            string oldname = string.Empty;
           string oldid= string.Empty;

            StreamWriter sw = File.AppendText("D:\\SMTOA相关文档\\预算维护\\update.txt");
            string w1 = "";
            sw.WriteLine(w1);
            string w2 = "修改表t_fb_budgetaccount中 USABLEMONEY "+ DropDownList1.SelectedItem.Text+" 可用结余字段值的时间是：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",修改的记录如下（空白则没有修改）：";
            sw.WriteLine(w2);
                             
            for (int i = 0; i < ds1.Rows.Count; i++)
            {
                string name = "";
                string id = "";
                if (DropDownList1.SelectedValue == "1")
                    name = ds1.Rows[i][9].ToString();
                else if (DropDownList1.SelectedValue == "2")
                {
                    name = ds1.Rows[i][8].ToString();
                    id = ds1.Rows[i][10].ToString();
                }
                else if (DropDownList1.SelectedValue == "3")
                {
                    name = ds1.Rows[i][7].ToString();
                    id = ds1.Rows[i][10].ToString();
                }

                if ((ds1.Rows[i][4].ToString() == oldsubjectname && oldname == name) || i == 0)//&& oldid ==id
                {
                    if (ds1.Rows[i][2].ToString() == "审核通过" || ds1.Rows[i][2].ToString() == "审核中" || ds1.Rows[i][2].ToString() == "审核中或未汇总")
                    {
                        if (ds1.Rows[i][2].ToString() != "审核通过" && (ds1.Rows[i][0].ToString() == "预算分配(调入)" || ds1.Rows[i][0].ToString() == "月度部门调拨(调入)" || ds1.Rows[i][0].ToString() == "个人调拨(调入)" || ds1.Rows[i][0].ToString() == "个人月度部门预算" || ds1.Rows[i][0].ToString() == "月度部门预算" || ds1.Rows[i][0].ToString() == "月度部门增补"))
                        {
                            //些上面这些单可用的要审核通过后才算
                        }
                        else
                        {
                            if (ds1.Rows[i][2].ToString() != "审核中或未汇总")
                            um += Decimal.Parse(ds1.Rows[i][6].ToString());
                        }
                    }

                    string sql = string.Empty;
                    if (i == ds1.Rows.Count - 1)
                    {
                        if (DropDownList1.SelectedValue == "1")
                            sql = "select * from t_fb_budgetaccount t  where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i][10].ToString() + "'";
                        else if (DropDownList1.SelectedValue == "2")
                            sql = "select * from t_fb_budgetaccount t  where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i][10].ToString() + "'";
                        else if (DropDownList1.SelectedValue == "3")
                            sql = "select * from t_fb_budgetaccount t  where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerid='" + ds1.Rows[i][10].ToString() + "' order by t.UPDATEDATE desc";

                        //OleDbDataAdapter ada4 = dbc.ExecuteAdapter(sql);
                        DataTable ds4 = new DataTable();
                        ds4 = GetDataTable(sql,dao);
                        //ada4.Fill(ds4);

                        #region 最后只有一条科目（处理最后一条记录）
                        //if (ds4.Rows.Count ==1)
                        //{
                        //    if (i == ds1.Rows.Count - 1)//处理最后一条记录
                        //    {
                        //        if (ds1.Rows[i][4].ToString() == oldsubjectname && oldname == ds1.Rows[i][7].ToString())
                        //            um += Decimal.Parse(ds1.Rows[i][6].ToString());
                        //        else
                        //            um = Decimal.Parse(ds1.Rows[i][6].ToString());
                        //    }
                        //    if (Decimal.Parse(ds4.Rows[0][10].ToString()) != um)//(Decimal.Parse(ds4.Rows[0][10].ToString()) < um && Decimal.Parse(ds4.Rows[0][10].ToString()) < Decimal.Parse(ds4.Rows[0][11].ToString()))
                        //    {
                        //        string strupdate = string.Empty;
                        //        if (i == ds1.Rows.Count - 1)
                        //        {
                        //            if (DropDownList1.SelectedValue == "1")
                        //                strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i][10].ToString() + "'";
                        //            else if (DropDownList1.SelectedValue == "2")
                        //                strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i][10].ToString() + "'";
                        //            else if (DropDownList1.SelectedValue == "3")
                        //                strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerid='" + ds1.Rows[i][10].ToString() + "'";

                        //        }
                        //        else
                        //        {
                        //            if (DropDownList1.SelectedValue == "1")
                        //                strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i - 1][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i - 1][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i - 1][10].ToString() + "'";
                        //            else if (DropDownList1.SelectedValue == "2")
                        //                strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i - 1][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i - 1][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i - 1][10].ToString() + "'";
                        //            else if (DropDownList1.SelectedValue == "3")
                        //                strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i - 1][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i - 1][11].ToString() + "'  and t.ownerid='" + ds1.Rows[i - 1][10].ToString() + "'";

                        //        }
                        //        //int n = (int)this.ExecuteCustomerSql(strupdate);
                        //        //if (n > 0)
                        //        //{
                        //        string w = "修改表记录的ID是：" + ds4.Rows[0][0].ToString() + " 修改前可用结余是：" + ds4.Rows[0][10].ToString() + "  修改后的可用结余是：" + um.ToString();
                        //        sw.WriteLine(w);
                        //        //}
                        //    }
                        //    um = Decimal.Parse(ds1.Rows[i][6].ToString());//重新调整可用结余             
                        //}
                        #endregion
                    }
                }
                else
                {
                    string sql = string.Empty;
                    if (i == ds1.Rows.Count - 1)
                    {                      
                        if (DropDownList1.SelectedValue == "1")
                            sql = "select * from t_fb_budgetaccount t  where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i][10].ToString() + "'";
                        else if (DropDownList1.SelectedValue == "2")
                            sql = "select * from t_fb_budgetaccount t  where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i][10].ToString() + "'";
                        else if (DropDownList1.SelectedValue == "3")
                            sql = "select * from t_fb_budgetaccount t  where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerid='" + ds1.Rows[i][10].ToString() + "'";
               
                    }
                    else
                    {
                        if (DropDownList1.SelectedValue == "1")
                            sql = "select * from t_fb_budgetaccount t  where t.subjectid='" + ds1.Rows[i - 1][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i - 1][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i - 1][10].ToString() + "'";
                        else if (DropDownList1.SelectedValue == "2")
                            sql = "select * from t_fb_budgetaccount t  where t.subjectid='" + ds1.Rows[i - 1][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i - 1][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i - 1][10].ToString() + "'";
                        else if (DropDownList1.SelectedValue == "3")
                            sql = "select * from t_fb_budgetaccount t  where t.subjectid='" + ds1.Rows[i - 1][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i - 1][11].ToString() + "'  and t.ownerid='" + ds1.Rows[i - 1][10].ToString() + "'";

                    }
                    //OleDbDataAdapter ada3 = dbc.ExecuteAdapter(sql);
                    DataTable ds3 = new DataTable();
                    ds3 = GetDataTable(sql,dao);
                    //ada3.Fill(ds3);

                    if (ds3.Rows.Count == 1)//多条记录一般有岗位异动或兼职,暂不处理
                    {
                        if (i == ds1.Rows.Count - 1)//处理最后一条记录
                        {
                            if (ds1.Rows[i][4].ToString() == oldsubjectname && oldname == name)
                            {
                                if (ds1.Rows[i][2].ToString() != "审核通过" && (ds1.Rows[i][0].ToString() == "月度部门调拨(调入)" || ds1.Rows[i][0].ToString() == "个人调拨(调入)" || ds1.Rows[i][0].ToString() == "个人月度部门预算" || ds1.Rows[i][0].ToString() == "月度部门预算" || ds1.Rows[i][0].ToString() == "月度部门增补"))
                                {
                                    //些上面这些单可用的要审核通过后才算
                                }
                                else
                                {
                                    if (ds1.Rows[i][2].ToString() != "审核中或未汇总")
                                    um += Decimal.Parse(ds1.Rows[i][6].ToString());
                                }
                            }
                            else
                                um = Decimal.Parse(ds1.Rows[i][6].ToString());
                        }
                        if (Decimal.Parse(ds3.Rows[0][10].ToString()) != um)// && Decimal.Parse(ds3.Rows[0][10].ToString()) != Decimal.Parse(ds3.Rows[0][11].ToString()))
                        {
                            string strupdate = string.Empty;
                            string Dname = "";
                            string Pname = "";
                            string Sname = "";
                            string Cname = "";
                            if (i == ds1.Rows.Count - 1)
                            {
                                if (DropDownList1.SelectedValue == "1")
                                {
                                    strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i][10].ToString() + "'";
                                    Pname = " 部门：";
                                    Dname = ds1.Rows[i][8].ToString();
                                    Sname = ds1.Rows[i][4].ToString();
                                    Cname = ds1.Rows[i][9].ToString();
                                }
                                else if (DropDownList1.SelectedValue == "2")
                                {
                                    strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i][10].ToString() + "'";
                                    Pname = " 部门：";
                                    Dname = ds1.Rows[i][8].ToString();
                                    Sname = ds1.Rows[i][4].ToString();
                                    Cname = ds1.Rows[i][9].ToString();
                                }
                                else if (DropDownList1.SelectedValue == "3")
                                {
                                    strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i][11].ToString() + "'  and t.ownerid='" + ds1.Rows[i][10].ToString() + "'";
                                    Pname = " 个人：";
                                    Dname = ds1.Rows[i][7].ToString();
                                    Sname = ds1.Rows[i][4].ToString();
                                    Cname = ds1.Rows[i][9].ToString();
                                }
                            }
                            else
                            {
                                if (DropDownList1.SelectedValue == "1")
                                {
                                    strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i - 1][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i - 1][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i - 1][10].ToString() + "'";
                                    Pname = " 部门：";
                                    Dname = ds1.Rows[i - 1][8].ToString();
                                    Sname = ds1.Rows[i - 1][4].ToString();
                                    Cname = ds1.Rows[i - 1][9].ToString();
                                }
                                else if (DropDownList1.SelectedValue == "2")
                                {
                                    strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i - 1][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i - 1][11].ToString() + "'  and t.ownerdepartmentid='" + ds1.Rows[i - 1][10].ToString() + "'";
                                    Pname = " 部门：";
                                    Dname = ds1.Rows[i - 1][8].ToString();
                                    Sname = ds1.Rows[i - 1][4].ToString();
                                    Cname = ds1.Rows[i - 1][9].ToString();
                                }
                                else if (DropDownList1.SelectedValue == "3")
                                {
                                    strupdate = "update t_fb_budgetaccount t set t.USABLEMONEY=" + um + " where t.subjectid='" + ds1.Rows[i - 1][12].ToString() + "' and  t.accountobjecttype=" + DropDownList1.SelectedValue + "  and t.ownercompanyid='" + ds1.Rows[i - 1][11].ToString() + "'  and t.ownerid='" + ds1.Rows[i - 1][10].ToString() + "'";
                                    Pname = " 个人：";
                                    Dname = ds1.Rows[i - 1][7].ToString();
                                    Sname = ds1.Rows[i - 1][4].ToString();
                                    Cname = ds1.Rows[i - 1][9].ToString();
                                }
                            }
                            //int n = (int)this.ExecuteCustomerSql(strupdate);
                            //if (n > 0)
                            //{
                            string w = "修改表记录的ID是：" + ds3.Rows[0][0].ToString() + " 修改前实际结余是：" + ds3.Rows[0][11].ToString() + " 修改前可用结余是：" + ds3.Rows[0][10].ToString() + "  修改后的可用结余是：" + um.ToString();
                            sw.WriteLine(w);
                            string c = " 科目：" + Sname + " 公司：" + Cname + Pname + Dname;
                            sw.WriteLine(c);
                            //}
                        }
                        if (ds1.Rows[i][2].ToString() != "审核中或未汇总")
                            um = Decimal.Parse(ds1.Rows[i][6].ToString());//重新调整可用结余
                        else
                            um = 0;     
                    }
                    else
                    {
                        if (ds1.Rows[i][4].ToString() == oldsubjectname && oldname == name)
                        {
                            if (ds1.Rows[i][2].ToString() != "审核通过" && (ds1.Rows[i][0].ToString() == "月度部门调拨(调入)" || ds1.Rows[i][0].ToString() == "个人调拨(调入)" || ds1.Rows[i][0].ToString() == "个人月度部门预算" || ds1.Rows[i][0].ToString() == "月度部门预算" || ds1.Rows[i][0].ToString() == "月度部门增补"))
                            {
                                //些上面这些单可用的要审核通过后才算
                            }
                            else
                            {
                                if (ds1.Rows[i][2].ToString() != "审核中或未汇总")
                                    um += Decimal.Parse(ds1.Rows[i][6].ToString());
                            }
                        }
                        else
                        {
                            if (ds1.Rows[i][2].ToString() != "审核中或未汇总")
                                um = Decimal.Parse(ds1.Rows[i][6].ToString());
                            else
                                um = 0;
                        }
                    }
                }
                oldsubjectname = ds1.Rows[i][4].ToString();
                if (DropDownList1.SelectedValue == "1")
                {
                    oldname = ds1.Rows[i][9].ToString();
                }
                else if (DropDownList1.SelectedValue == "2")
                {
                    oldname = ds1.Rows[i][8].ToString();
                    oldid = ds1.Rows[i][10].ToString();
                }
                else if (DropDownList1.SelectedValue == "3")
                {
                    oldname = ds1.Rows[i][7].ToString();
                    oldid = ds1.Rows[i][10].ToString();
                }
            }
            sw.Close();  
        }

        protected void btnsubject_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            string strSql = @"select t.subjectname from t_fb_subject t
                                where t.subjectname like '%" + TextBox1.Text + "%'";
            DataTable dt = new DataTable();
            dt = GetDataTable(strSql,dao);
            if (dt.Rows.Count > 0)
            {
                string result = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    result =result+Environment.NewLine+ dr[0].ToString();
                }
                txtResult.Text = "科目检查正确：" + result;
            }
            else
            {
                txtResult.Text = "科目不存在";

            }
        }

        protected void btnCompany_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            string strSql = @"select t.cname from smthrm.t_hr_company t
                            where t.cname like '%" + TextBox2.Text + "%'";
            DataTable dt = new DataTable();
            dt = GetDataTable(strSql,dao);
            if (dt.Rows.Count > 0)
            {
                string result = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    result = result + Environment.NewLine + dr[0].ToString();
                }
                txtResult.Text = "公司检查正确：" + result;
            }
            else
            {
                txtResult.Text = "公司不存在";

            }
        }

        protected void btnDepartment_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
            string strSql = @"select t.departmentname from smthrm.t_hr_departmentdictionary t
                                where t.departmentname like '%" + TextBox3.Text + "%'";
            DataTable dt = new DataTable();
            dt = GetDataTable(strSql,dao);
            if (dt.Rows.Count > 0)
            {
                string result = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    result = result + Environment.NewLine + dr[0].ToString();
                }
                txtResult.Text = "部门检查正确：" + result;
            }
            else
            {
                txtResult.Text = "部门不存在";

            }
        }

        protected void btnCheckDep_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);
           string strSql = @"select 
                        c.cname,
                        dd.departmentname
                        from smthrm.t_hr_company c
                        inner join smthrm.t_hr_department d on c.companyid =d.companyid
                        inner join smthrm.t_hr_departmentdictionary dd on d.departmentdictionaryid=dd.departmentdictionaryid
                        where c.cname='" + TextBox2.Text + "'" +
                     "and dd.departmentname='" + TextBox3.Text + "'";
            DataTable dt = new DataTable();
            dt = GetDataTable(strSql,dao);
            if (dt.Rows.Count > 0)
            {
                string result = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    result = result + Environment.NewLine + dr[0].ToString()+" " + dr[1].ToString();
                }
                txtResult.Text = "部门检查正确：" + result;
            }
            else
            {
                txtResult.Text = "部门不存在";

            }
        }
        /// <summary>
        /// 查询借还款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnBrowrry_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 修改借还款余额
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnBowrryMoney_Click(object sender, EventArgs e)
        {

        }

        protected void btnCloseBudget_Click(object sender, EventArgs e)
        {
            //using (SMT_FB_EFModelContext dbcontext = new SMT_FB_EFModelContext())
            //{
            //    
            //}
                OracleDAO dao = new OracleDAO(conn);
                try
                {
                    #region "结算活动经费"
//                    dao.BeginTransaction();
//                    string strAccounttd = @"select * from SMTFB.ZC_ACCOUNT_2012 t
//                                            inner join smtfb.t_fb_subject s on t.subjectid=s.subjectid
//                                            where 
//                                            s.subjectname='活动经费'";
//                    DataTable dtAccounttd = new DataTable();
//                    dtAccounttd = GetDataTable(strAccounttd,dao);

//                    //备份表
//                    string backAccountd = @"create table Huodongjingfei" + DateTime.Now.ToString("yyyy_MM_dd") + @" as select t.* from t_fb_budgetaccount t
//                                            inner join smtfb.t_fb_subject s on t.subjectid=s.subjectid
//                                            where s.subjectname='活动经费'";
//                    int i = this.ExecuteNonQuery(backAccountd, dao);

//                    //清空总账表
//                    string strDelT_fb_budgetAccount = @"delete smtfb.t_fb_budgetaccount t
//                                                        where t.subjectid='d5134466-c207-44f2-8a36-cf7b96d5851f'";
//                    int j = this.ExecuteNonQuery(strDelT_fb_budgetAccount, dao);
                    #endregion 

                    #region "结算部门和个人科目总账"
                    dao.BeginTransaction();
                    string strAccounttd = @"select * from SMTFB.ZC_ACCOUNT_2012 t
                                            inner join smtfb.t_fb_subject s on t.subjectid=s.subjectid
                                            where 
                                            s.subjectname<>'活动经费'
                                            and s.subjectname<>'部门经费'
                                            and t.ACCOUNTOBJECTTYPE=2
                                            or t.ACCOUNTOBJECTTYPE=3";
                    DataTable dtAccounttd = new DataTable();
                    dtAccounttd = GetDataTable(strAccounttd, dao);

                    //备份表
                    string backAccountd = @"create table " + DateTime.Now.ToString("yyyy_MM_dd") + @"BudgetAcount as select t.* from t_fb_budgetaccount t";
                    int i = this.ExecuteNonQuery(backAccountd, dao);

                    //清空总账表
                    string strDelT_fb_budgetAccount = @"delete smtfb.t_fb_budgetaccount t
                                                        where   t.subjectid<>'08c1d9c6-2396-43c3-99f9-227e4a7eb417'
                                                        and t.subjectid<>'d5134466-c207-44f2-8a36-cf7b96d5851f'
                                                        and t.accountobjecttype=2
                                                        or t.accountobjecttype=3";
                    int j = this.ExecuteNonQuery(strDelT_fb_budgetAccount, dao);
                    #endregion

                    string strinsert = "";
                    //插入新记录           
                    foreach (DataRow dr in dtAccounttd.Rows)
                    {
                        //生成新的guid
                        dr[0] = Guid.NewGuid().ToString();
                        strinsert += @"insert into t_fb_budgetaccount
                                    (budgetaccountid, accountobjecttype, budgetyear, budgetmonth, ownercompanyid, ownerdepartmentid, ownerid, ownerpostid, subjectid, budgetmoney, usablemoney, actualmoney, paiedmoney, createuserid, createdate, updateuserid, updatedate, remark)
                                  values
                                    (" + "'" +
                                               Guid.NewGuid().ToString() + "','" +
                                                dr["accountobjecttype"].ToString().ToString() + "','" +
                                                dr["budgetyear"].ToString() + "','" +
                                                 DateTime.Now.Month + "','" +
                                                dr["ownercompanyid"].ToString() + "','" +
                                                dr["ownerdepartmentid"].ToString() + "','" +
                                                dr["ownerid"].ToString() + "','" +
                                                dr["ownerpostid"].ToString() + "','" +
                                                dr["subjectid"].ToString() + "','" +
                                                dr["budgetmoney"].ToString() + "','" +
                                                dr["usablemoney"].ToString() + "','" +
                                                dr["actualmoney"].ToString() + "','" +
                                                dr["paiedmoney"].ToString() + "','" +
                                                dr["createuserid"].ToString() + "'," +
                                                "TO_DATE('" +DateTime.Now.ToString("yyyy-MM-dd") + "','yyyy-mm-dd hh24:mi:ss')" + ",'" +
                                                dr["updateuserid"].ToString() + "'," +
                                                "TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd") + "','yyyy-mm-dd hh24:mi:ss')" + ",'" +
                                                DateTime.Now.ToString("yyyy-MM-dd")+"总账手动结算" + "')" +
                                          " ;" + System.Environment.NewLine;
                    }
                    int k = this.ExecuteNonQuery(strinsert, dao);
                    dao.Commit();
                }
                catch (Exception ex)
                {
                    dao.Rollback();
                }
            
        }


        #region "数据库基本操作"

        protected int ExecuteNonQuery(string Sqlstring,OracleDAO dao)
        {
            Sqlstring.Replace("\n", "");
            Sqlstring.Replace("\r", "");
           
            int i = dao.ExecuteNonQuery(Sqlstring, System.Data.CommandType.Text);
            return i;
        }

        protected DataTable GetDataTable(string Sqlstring,OracleDAO dao)
        {
            Sqlstring.Replace("\n", "");
            Sqlstring.Replace("\r", "");
            DataTable obj = dao.GetDataTable(Sqlstring, System.Data.CommandType.Text);
            return obj;
        }
        #endregion

        protected void btnHuodong_Click(object sender, EventArgs e)
        {
            OracleDAO dao = new OracleDAO(conn);         
            //获取所有有多条记录的员工
            string strAll = @"select count(t.usablemoney),t.ownerid from smtfb.t_fb_budgetaccount t
                                            inner join smtfb.t_fb_subject s on t.subjectid=s.subjectid
                                            where 
                                            s.subjectname='活动经费'
                                            group by t.ownerid
                                            having count(t.ownerid)>1";
            DataTable dsTotal = new DataTable();
            dsTotal = this.GetDataTable(strAll,dao);
            //循环处理查出的员工记录
            foreach (DataRow dr in dsTotal.Rows)
            {
                string ownid = dr["ownerid"].ToString();
                if(ownid=="24ccad45-b49b-4499-9012-40f356e9811a")
                {

                }
                //查出指定员工所有的活动经费记录
                string allDealData = @"select t.budgetmoney,t.usablemoney,t.actualmoney,t.paiedmoney,t.ownerpostid,ep.isagency,ep.editstate,t.ownerid from smtfb.t_fb_budgetaccount t
                                                inner join smtfb.t_fb_subject s on t.subjectid=s.subjectid
                                                inner join smthrm.t_hr_employeepost ep on ep.employeeid=t.ownerid
                                                and ep.postid=t.ownerpostid
                                                where 
                                                t.ownerid='" + ownid + @"'
                                                and 
                                                s.subjectname='活动经费'";
                DataTable dtallDealData = GetDataTable(allDealData,dao);
                //判断主岗位是否多个
                if (dtallDealData.Select("isagency=0 and editstate=1").Count() > 1)
                {
                    txtResult.Text += System.Environment.NewLine + "主岗位存在多个，请及时处理employeid：" + ownid;
                }
                else
                {
                    //查出汇总金额
                    string strSumDealData = @"select sum(t.budgetmoney) sumbudgetmoney
                                                    ,sum(t.usablemoney) sumusablemoney,
                                                    sum(t.actualmoney) sumactualmoney
                                                    ,sum(t.paiedmoney) sumpaiedmoney,t.ownerid from smtfb.t_fb_budgetaccount t
                                                    inner join smtfb.t_fb_subject s on t.subjectid=s.subjectid
                                                    where t.ownerid='" + ownid + @"'
                                                    and s.subjectname='活动经费'
                                                    group by t.ownerid";
                    DataTable dtSumDealData = GetDataTable(strSumDealData,dao);
                    //开始处理
                    //foreach (DataRow dro in dtallDealData.Rows)
                    //{
                    DataRow[] drs = dtallDealData.Select("isagency=0 and editstate=1");
                    if (drs.Count() == 0)
                    {
                        txtResult.Text += System.Environment.NewLine + "通过岗位关联没查到活动经费 ownid：" + ownid;
                        continue;
                    }
                    DataRow drd = dtallDealData.Select("isagency=0 and editstate=1")[0];
                    string budgetmoney = dtSumDealData.Rows[0]["sumbudgetmoney"].ToString();
                    string usablemoney = dtSumDealData.Rows[0]["sumusablemoney"].ToString();
                    string actualmoney = dtSumDealData.Rows[0]["sumactualmoney"].ToString();
                    string paiedmoney = dtSumDealData.Rows[0]["sumpaiedmoney"].ToString();
                    //更新主岗位活动经费数据
                    string updateMainPost = @"update smtfb.t_fb_budgetaccount t
                                                                set t.budgetmoney=" + budgetmoney + @",
                                                                t.usablemoney=" + usablemoney + @",
                                                                t.actualmoney=" + actualmoney + @",
                                                                t.paiedmoney=" + paiedmoney + @"
                                                                where t.ownerid='" + ownid + @"'
                                                                and t.subjectid='d5134466-c207-44f2-8a36-cf7b96d5851f'
                                                                and t.ownerpostid='" + drd["ownerpostid"].ToString() + "'";
                    int i = ExecuteNonQuery(updateMainPost,dao);
                    txtResult.Text += System.Environment.NewLine + "更新额度成功 ownid：" + ownid;
                    if (i == 1)//更新成功
                    {
                        string strDelete = @"delete from smtfb.t_fb_budgetaccount t
                                                    where t.ownerid='" + ownid + @"'
                                                    and t.subjectid='d5134466-c207-44f2-8a36-cf7b96d5851f'
                                                    and t.ownerpostid <> '" + drd["ownerpostid"].ToString() + "'";
                        //删除其他所有无用数据
                        int j = ExecuteNonQuery(strDelete,dao);
                        txtResult.Text += System.Environment.NewLine + "删除其他数据成功 ownid：" + ownid;
                    }
                }
            }
            return;
        }
    }
}