/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-18

** 修改人：刘锦

** 修改时间：2010-06-28

** 描述：

**    主要用于福利发放明细数据信息的修改&查看

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.SaaS.FrameworkUI;
using System.Windows.Browser;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;

namespace SMT.SaaS.OA.UI.Views.BenefitsAdministration
{
    public partial class UpdateWelfarePaymentDetailsChildWindows : BaseForm, IClient, IEntityEditor
    {

        #region 全局变量

        private T_OA_WELFAREDISTRIBUTEDETAIL welfarePaymentDetails = new T_OA_WELFAREDISTRIBUTEDETAIL();
        SmtOADocumentAdminClient wsscs = new SmtOADocumentAdminClient();
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private FormTypes actions; //操作类型
        
        #endregion
       
        #region 构造

        public UpdateWelfarePaymentDetailsChildWindows(FormTypes action, V_WelfareDetails welfarePaymentDetailsObj)
        {
            actions = action;
            InitializeComponent();
            if (action == FormTypes.Edit || action == FormTypes.Browse)
            {
                GetWelfarePaymentDetailsRoom(welfarePaymentDetailsObj);
            }
            if (actions == FormTypes.Browse)
            {
                this.txtWelfareStandard.IsReadOnly = true;
                this.txtNotes.IsReadOnly = true;
            }
        }
        #endregion

        #region GetWelfarePaymentDetailsRoom
        private void GetWelfarePaymentDetailsRoom(V_WelfareDetails WelfareDetails)
        {
            if (WelfareDetails != null)
            {
                welfarePaymentDetails = WelfareDetails.welfareDetailsViews;

                txtWelfareStandard.Text = WelfareDetails.welfareDetailsViews.STANDARD.ToString();//发放金额(福利标准)
                txtNotes.Text = WelfareDetails.welfareDetailsViews.REMARK;//发放项
            }
        }
        #endregion

        #region IEntityEditor 成员
        public string GetTitle()
        {
            if (actions == FormTypes.Browse)
            {
                return Utility.GetResourceStr("VIEWTITLE", "WELFAREPAYMENTDETAILSPAGE");
            }
            else
            {
                return Utility.GetResourceStr("EDITTITLE", "WELFAREPAYMENTDETAILSPAGE");
            }
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (actions != FormTypes.Browse)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1",
                    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                };
                items.Add(item);

                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("SAVE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"

                };
                items.Add(item);
            }
            return items;
        }
        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region 修改
        private void Save()
        {
            T_OA_WELFAREDISTRIBUTEDETAIL welfarePaymentDetailsRoom = welfarePaymentDetails;
            string ContractModified = DateTime.Now.ToShortDateString();//修改时间

            welfarePaymentDetailsRoom.UPDATEDATE = Convert.ToDateTime(ContractModified);//修改时间
            welfarePaymentDetailsRoom.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
            welfarePaymentDetailsRoom.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//修改人姓名
            welfarePaymentDetailsRoom.REMARK = txtNotes.Text;//发放内容
            welfarePaymentDetailsRoom.STANDARD = Convert.ToDecimal(txtWelfareStandard.Text);//发放金额
            welfarePaymentDetailsRoom.USERID = Common.CurrentLoginUserInfo.EmployeeID;
            welfarePaymentDetailsRoom.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            welfarePaymentDetailsRoom.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            welfarePaymentDetailsRoom.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            welfarePaymentDetailsRoom.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            welfarePaymentDetailsRoom.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            try
            {
                wsscs.UpdateWelfarePaymentDetailsCompleted += new EventHandler<UpdateWelfarePaymentDetailsCompletedEventArgs>(wsscs_UpdateWelfarePaymentDetailsCompleted);
                wsscs.UpdateWelfarePaymentDetailsAsync(welfarePaymentDetailsRoom);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        void wsscs_UpdateWelfarePaymentDetailsCompleted(object sender, UpdateWelfarePaymentDetailsCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (GlobalFunction.IsSaveAndClose(refreshType))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "PAYMENTDETAILS"));
                }
                RefreshUI(refreshType);
                RefreshUI(RefreshedTypes.ProgressBar);//数据完成后隐藏进度条
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            throw new NotImplementedException();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
