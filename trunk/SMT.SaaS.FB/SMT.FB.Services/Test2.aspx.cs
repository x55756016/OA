using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMT_FB_EFModel;
using SMT.FB.DAL;
using SMT.FB.BLL;



namespace SMT.FB.Services
{
    public partial class Test2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //FBService FBClient = new FBService();
            //string strMsg = "";
            ////// s.UpdateExtensionOrder("Travel", "e8577fe8-7fe6-4e5e-8697-2d00504069b1", "1", ref ms);

            //var fbResult = FBClient.UpdateExtensionOrder("Travel", "4bd60448-588b-415d-bf38-bb03e3614f19", "1", ref strMsg);
           // BaseBLL bll = new BaseBLL();
            //var sub = bll.InnerGetEntities<T_FB_SUBJECT>(new QueryExpression()).FirstOrDefault();

            //T_FB_SUBJECT.CreateT_FB_SUBJECT(
            //    sub.SUBJECTID,
            //    sub.SUBJECTNAME,
            //    sub.SUBJECTCODE,
            //    sub.ACTIVED,
            //    sub.CREATEUSERID,
            //    sub.CREATEDATE,
            //    sub.UPDATEUSERID,
            //    sub.UPDATEDATE,

            //    sub.EDITSTATES);
            //T_FB_SUBJECT aa = new T_FB_SUBJECT()
            //{
            //    EntityKey = sub.EntityKey,
            //    SUBJECTID = sub.SUBJECTID,
            //    SUBJECTCODE = sub.SUBJECTCODE,
            //    SUBJECTNAME = sub.SUBJECTNAME
            //};
            //var  ab = aa.EntityKey;

       
        }

        protected void btnTest_Click(object sender, EventArgs e)
        {
            FBCommonService s = new FBCommonService();
            QueryExpression qe = new QueryExpression() { QueryType = typeof(T_FB_COMPANYBUDGETAPPLYMASTER).Name };
            var data = s.QueryFBEntities(qe);
            this.GridView1.DataSource = data;
            DataBind();
        }

        protected void btnContenList_Click(object sender, EventArgs e)
        {
            string count = "0";// DBContext.contenxtCache.Count.ToString();
            this.listDetail.Text = string.Format("一共有: {0} 个", count);
        }

        protected void btnUpdateOrder_Click(object sender, EventArgs e)
        {
            FBService FBClient = new FBService();
            string strMsg = "成功!";
            // s.UpdateExtensionOrder("Travel", "e8577fe8-7fe6-4e5e-8697-2d00504069b1", "1", ref ms);
            var fbResult = FBClient.UpdateExtensionOrder("Travel", this.txtOrderID.Text,txtCheckState.Text, ref strMsg);
            this.lbMsg.Text = strMsg;
        }

        protected void btnDeptDetail_Click(object sender, EventArgs e)
        {
            // 预算月份
            //DateTime dtBudgetDate = DateTime.Parse();
            //string dataValue = dtBudgetDate.ToString("yyyy-MM-dd");
            QueryExpression qeMonth = QueryExpression.Equal("BUDGETARYMONTH", this.txtMonth.Text);
            qeMonth.QueryType = typeof(T_FB_DEPTBUDGETAPPLYDETAIL).Name;

            // 预算部门
            QueryExpression qeDept = QueryExpression.Equal("OWNERDEPARTMENTID", this.txtDeptID.Text);
            qeDept.QueryType = typeof(T_FB_DEPTBUDGETAPPLYDETAIL).Name;
            qeDept.RelatedExpression = qeMonth;

            FBCommonService fbService = new FBCommonService();

            var resultList = fbService.QueryFBEntities(qeDept);

            this.GridView1.DataSource = resultList.ToEntityList<T_FB_DEPTBUDGETAPPLYDETAIL>();
            DataBind();
        }
        
    }
}