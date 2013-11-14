using System;
using SmtOnlinePortalTools.AttWS;
using SmtOnlinePortalTools.HrCommonSV;

namespace SMT.HRM.Services
{
    /// <summary>
    /// 调用外网服务
    /// </summary>
    public partial class TestDo2 : System.Web.UI.Page
    {
        private HrCommonServiceClient client;
        protected void Page_Load(object sender, EventArgs e)
        {
            client = new HrCommonServiceClient();
            TxtConfigName.Text = "ConnectionString";

        }

        /// <summary>
        /// 生成带薪假
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpdateFreeLeaveRd_Click(object sender, EventArgs e)
        {
            //机构类型 1表示公司 4表示员工
            string txtFLOrgType = this.txtFLOrgType.Text;
            //机构ID
            string txtFLOrgID = this.txtFLOrgID.Text;

            if (string.IsNullOrEmpty(txtFLOrgType) || string.IsNullOrEmpty(txtFLOrgID))
            {
                this.TextBox1.Text = "机构类型/机构ID请正确填写";
                return;
            }

            AttendanceServiceClient client = new AttendanceServiceClient();

            //生成带薪假期
            client.CalculateEmployeeLevelDayCountByOrgID(txtFLOrgType,txtFLOrgID);

            this.TextBox1.Text = "生成带薪假完毕！";
        }

        protected void btnGetConfig_Click(object sender, EventArgs e)
        {
            string str = client.GetAppConfigByName(TxtConfigName.Text);
            txtResult.Text = txtResult.Text + System.Environment.NewLine + str;
        }

        protected void btnExcuteSql_Click(object sender, EventArgs e)
        {
          object obj=  client.CustomerQuery(TxtSql.Text);
          gridViewResult.DataSource = obj;
        }
    }
}