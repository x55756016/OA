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

namespace SMT.SaaS.OA.UI.Views.AssetManagement
{
    public partial class AssetPurchaseControl : BaseForm,IClient, IEntityEditor
    {
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private Action actions;
        private SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER order = new SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER();//预算控件

        public AssetPurchaseControl(Action acion)
        {
            actions = acion;
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region 保存函数
        private void Save()
        {
            try
            {
                if (Check())
                {
                    //RefreshUI(RefreshedTypes.ProgressBar);//点击保存后显示进度条
                    return;
                }
                //RefreshUI(RefreshedTypes.ProgressBar);//关闭进度条动画
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {
            System.Text.RegularExpressions.Regex reg
                = new System.Text.RegularExpressions.Regex(@"^[1-9][0-9]*$");//申购数量大于0
            if (!reg.IsMatch(txtPurchaseQuantity.Text)) 
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PurchaseQuantityIsEmptyOrLessThan0", ""));
                //RefreshUI(RefreshedTypes.ProgressBar);//关闭进度条动画
                return false;
            }
            if (txtPurchaseQuantity.Text == string.Empty)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "PURCHASEQUANTITY"));
                return false;
            }

            if (txtFunds.Text == string.Empty)//所需资金
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "FUNDSNEEDED"));
                return false;
            }

            if (txtBenefit.Text == string.Empty)//经济效益
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "BUDGETCOST"));
                return false;
            }

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
                //RefreshUI(RefreshedTypes.ProgressBar);//关闭进度条动画
            }
            return true;
        }
        #endregion

        #region 提交审批
        private void SubmitAuditToClose()
        {
 
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            if (actions == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "ASSETPURCHASE");//新增
            }
            else if (actions == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "ASSETPURCHASE");//修改
            }
            else if (actions == Action.Read)
            {
                return Utility.GetResourceStr("VIEW", "ASSETPURCHASE");//查看
            }
            else
            {
                return Utility.GetResourceStr("AUDIT", "ASSETPURCHASE");//审核
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
                case "2":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    SubmitAuditToClose();
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

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "2",
                Title = Utility.GetResourceStr("SUBMITAUDIT"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            };
            items.Add(item);

            item = new ToolbarItem
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

        #region 申购人
        private void btnLookUpUserName_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    //ctapp.PARTYA = companyInfo.ObjectID;//申购人Id
                    txtUserName.Text = companyInfo.ObjectName;//申购人姓名
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        #endregion

        #region 申请部门
        private void btnLookDepartment_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    //ctapp.PARTYA = companyInfo.ObjectID;//申购部门ID
                    txtDepartment.Text = companyInfo.ObjectName;//申购部门名
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        #endregion

        #region 人是否落实(是)
        private void rbtYes_Click(object sender, RoutedEventArgs e)
        {
            this.rbtYes.IsChecked = true;
            this.RbtNo.IsChecked = false;
        }
        #endregion

        #region 人是否落实(否)
        private void RbtNo_Click(object sender, RoutedEventArgs e)
        {
            this.RbtNo.IsChecked = true;
            this.rbtYes.IsChecked = false;
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            //throw new NotImplementedException();
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
