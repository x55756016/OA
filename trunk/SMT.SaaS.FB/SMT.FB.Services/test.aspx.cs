using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMT_FB_EFModel;
using SMT.FB.BLL;

namespace SMT.FB.Services
{
    public partial class test : System.Web.UI.Page
    {
        protected List<EntityInfo> EntityInfoList { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        { 

            FBCommonService fbCommonService = new FBCommonService();
            EntityInfoList = fbCommonService.GetEntityInfoList();
            if (!IsPostBack)
            {

                this.ddlOrderType.DataTextField = "Type";
                this.ddlOrderType.DataValueField = "Type";
                this.ddlOrderType.DataSource = EntityInfoList;
                this.DataBind();

                BindMobileDpd(dpdMobileEntityList, "Type", "Type", EntityInfoList);
            }

           
         
        }

        /// <summary>
        /// 绑定DropDownList
        /// </summary>
        /// <param name="dpdMobileEntityList"></param>
        /// <param name="strDisplayText"></param>
        /// <param name="strBindValue"></param>
        /// <param name="EntityInfoList"></param>
        private void BindMobileDpd(DropDownList dpd, string strDisplayText, string strBindValue, List<EntityInfo> EntityInfoList)
        {
            if (EntityInfoList == null)
            {
                return;
            }

            if (EntityInfoList.Count() == 0)
            {
                return;
            }

            dpd.Items.Clear();

            dpd.DataTextField = strDisplayText;
            dpd.DataValueField = strBindValue;
            dpd.DataSource = EntityInfoList;
            dpd.DataBind();
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            FBService fs = new FBService();
            string comID = this.TextBox1.Text;
            string deptID = this.TextBox2.Text;
            var data = fs.FetchSalaryBudget(comID, deptID);
            DataList1.DataSource = data;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string xml =  TextBox3.Text;
            xml = "<?xml version='1.0' encoding='utf-8' ?>" + xml;
            FBService fs = new FBService();
            fs.UpdateSalaryBudget(xml);

        }

        protected void btnGetTravelExpApplyMasterCode_Click(object sender, EventArgs e)
        {
            IOaService service = new OaServerce();
            this.lblExpApplyMasterCode.Text = service.GetTravelExpApplyMasterCode(this.txtExtensionalOrderID.Text.Trim());
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            AuditOrder(CheckStates.Approved);
        }

        private void AuditOrder(CheckStates cs)
        {
            string[] orderids = this.txtOrderID.Text.Split(',');
            orderids.ToList().ForEach(item =>
            {
                AuditOrder(item, cs);
            });
        }

        private void AuditOrder(string orderID, CheckStates cs)
        {
            string typeName = "";
            string keyName = "";

            var entityInfo = EntityInfoList.First(item => item.Type == this.ddlOrderType.SelectedValue);
            typeName = entityInfo.Type;
            keyName = entityInfo.KeyName;
            QueryExpression qe = QueryExpression.Equal(keyName, orderID);
            qe.QueryType = typeName;
            using (BudgetAccountBLL bll = new BudgetAccountBLL())
            {
                var data = qe.Query(bll);
                var order = data.FirstOrDefault();
                if (order != null)
                {
                   
                    try
                    {
                        bll.BeginTransaction();
                        bll.UpdateAccount(order.ToFBEntity(), cs);
                        bll.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        bll.RollbackTransaction();
                        SMT.Foundation.Log.Tracer.Error("手动审核异常：" + ex.ToString());
                        Response.Write(ex.ToString());
                    }
                    Response.Write("操作成功");
                }
                else
                {
                    Response.Write("没有可操作的数据");
                }


            }
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            AuditOrder(CheckStates.UnApproved);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            AuditOrder(CheckStates.Approving);
        }

        protected void Button5_Click(object sender, EventArgs e)
        {
            BaseBLL bll = new BaseBLL();
            var data = bll.InnerGetEntities<T_FB_CHARGEAPPLYMASTER>(new QueryExpression());
            var b = from item in data
                    orderby item.UPDATEDATE
                    select item;
            var c = from item in b
                    where item.TOTALMONEY > 0
                    select item;
            var d = c.Take(5).ToList();

            
        }

        protected void btnShow0_Click(object sender, EventArgs e)
        {
            FBService service = new FBService();
            service.CloseBudget();
        }
               
        /// <summary>
        /// 手机提交审核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnMobileApproving_Click(object sender, EventArgs e)
        {
            string strModelCode = string.Empty, orderID = string.Empty, strCheckStates = string.Empty, strMsg = string.Empty;
            int i = 0;
            
            var entityInfo = EntityInfoList.First(item => item.Type == dpdMobileEntityList.SelectedValue);
            strModelCode = entityInfo.Type;

            if (string.IsNullOrWhiteSpace(txtMobileEntityID.Text))
            {
                return;
            }

            orderID = txtMobileEntityID.Text;
            strCheckStates = ((int)CheckStates.Approving).ToString();

            FBService svc = new FBService();
            i = svc.UpdateCheckState(strModelCode, orderID, strCheckStates, ref strMsg);

            if (i < 0)
            {
                Response.Write("手机测试服务操作失败：错误消息为：" + strMsg);
                return;
            }
            Response.Write("手机测试服务操作成功");
        }

        /// <summary>
        /// 手机审核通过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnMobileApproved_Click(object sender, EventArgs e)
        {
            string strModelCode = string.Empty, orderIDs = string.Empty, strCheckStates = string.Empty, strMsg = string.Empty;
            int i = 0;

            var entityInfo = EntityInfoList.First(item => item.Type == dpdMobileEntityList.SelectedValue);
            strModelCode = entityInfo.Type;

            if (string.IsNullOrWhiteSpace(txtMobileEntityID.Text))
            {
                return;
            }

            orderIDs = txtMobileEntityID.Text;
            string[] allid=orderIDs.Split(',');

            foreach (string orderID in allid)
            {                
                strCheckStates = ((int)CheckStates.Approved).ToString();

                FBService svc = new FBService();
                i = svc.UpdateCheckState(strModelCode, orderID, strCheckStates, ref strMsg);

                if (i < 0)
                {
                    Response.Write("手机测试服务操作失败：错误消息为：" + strMsg + "\r\n");
                    return;
                }
                Response.Write(orderID + "手机测试服务操作成功");
            }
        }

        /// <summary>
        /// 手机审核不通过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnMobileUnApproved_Click(object sender, EventArgs e)
        {
            string strModelCode = string.Empty, orderID = string.Empty, strCheckStates = string.Empty, strMsg = string.Empty;
            int i = 0;

            var entityInfo = EntityInfoList.First(item => item.Type == dpdMobileEntityList.SelectedValue);
            strModelCode = entityInfo.Type;

            if (string.IsNullOrWhiteSpace(txtMobileEntityID.Text))
            {
                return;
            }

            orderID = txtMobileEntityID.Text;
            strCheckStates = ((int)CheckStates.UnApproved).ToString();

            FBService svc = new FBService();
            i = svc.UpdateCheckState(strModelCode, orderID, strCheckStates, ref strMsg);

            if (i < 0)
            {
                Response.Write("手机测试服务操作失败：错误消息为：" + strMsg);
                return;
            }
            Response.Write("手机测试服务操作成功");
        }
    }
}