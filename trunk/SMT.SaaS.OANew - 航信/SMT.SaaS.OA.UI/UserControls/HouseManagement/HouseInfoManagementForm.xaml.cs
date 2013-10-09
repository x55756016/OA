using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class HouseInfoManagementForm :BaseForm,IClient,IEntityEditor
    {
        private Action action;
        private string houseID;
        private SmtOACommonAdminClient client;
        private T_OA_HOUSEINFO houseObj;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;          //保存方式 0:保存 1:保存并关闭
        //private SMTLoading loadbar = new SMTLoading();
        #region 初始化
        public HouseInfoManagementForm(Action action, string houseID)
        {            
            InitializeComponent();            
            this.action = action;
            this.houseID = houseID;            
            InitEvent();
            InitData();
            if (action == Action.Read)
            {
                txtContent.HideControls();
                this.IsEnabled = false;
            }
        }
        //门户调用审核使用
        public HouseInfoManagementForm(FormTypes typeaction, string houseID)
        {
            InitializeComponent();
            //PARENT.Children.Add(loadbar);
            this.action = Action.AUDIT;
            this.houseID = houseID;
            InitEvent();
            InitData();
        }

        private void InitEvent()
        {
            client = new SmtOACommonAdminClient();
            client.GetHouseInfoByIdCompleted += new EventHandler<GetHouseInfoByIdCompletedEventArgs>(client_GetHouseInfoByIdCompleted);
            client.AddHouseCompleted += new EventHandler<AddHouseCompletedEventArgs>(client_AddHouseCompleted);
            client.UpdateHouseCompleted += new EventHandler<UpdateHouseCompletedEventArgs>(client_UpdateHouseCompleted);          
        }

        private void InitData()
        {
            this.Loaded += new RoutedEventHandler(HouseInfoManagementForm_Loaded);            
            client.GetHouseInfoByIdAsync(houseID);
        }

        void HouseInfoManagementForm_Loaded(object sender, RoutedEventArgs e)
        {           
            RefreshUI(RefreshedTypes.ProgressBar);
        }
        #endregion

        #region 完成事件
        private void client_GetHouseInfoByIdCompleted(object sender, GetHouseInfoByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        houseObj = e.Result.ToList()[0];                        
                        txtContent.RichTextBoxContext = houseObj.CONTENT;
                        txtDeposit.Text = houseObj.DEPOSIT.ToString();
                        txtFloor.Text = houseObj.FLOOR.ToString();
                        txtHouseName.Text = houseObj.HOUSENAME;
                        //txtIsRent.Text = houseObj.ISRENT;

                        txtSharedDeposit.Text = houseObj.SHAREDDEPOSIT.ToString();
                        txtSharedRentCost.Text = houseObj.SHAREDRENTCOST.ToString();
                        
                        if (!string.IsNullOrEmpty(houseObj.REMARK))
                        {
                            txtRemark.Text = houseObj.REMARK.ToString();
                        }
                        txtNumber.Text = houseObj.Number.ToString();
                        txtContent.RichTextBoxContext = houseObj.CONTENT;
                        txtRentCost.Text = houseObj.RENTCOST.ToString();
                        txtManageCost.Text = houseObj.MANAGECOST.ToString();
                        txtNum.Text = houseObj.ROOMCODE;
                        txtRentCost.Text = houseObj.RENTCOST.ToString();
                        txtUptown.Text = houseObj.UPTOWN;
                    }
                }
                else
                {   
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"));
            }
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        private void client_UpdateHouseCompleted(object sender, UpdateHouseCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {             
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        //HtmlPage.Window.Alert("房源记录修改成功！");
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "HOUSEINFO"));
                        }
                        else
                        {
                            InitData();
                        }
                        RefreshUI(refreshType);
                    }
                }
                else
                {            
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }          
        }

        private void client_AddHouseCompleted(object sender, AddHouseCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        //HtmlPage.Window.Alert(e.Result);
                        //RefreshUI(RefreshedTypes.ProgressBar);
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        //HtmlPage.Window.Alert("新增房源记录成功！");
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "HOUSEINFO"));
                            RefreshUI(refreshType);
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "HOUSEINFO"));
                            this.action = Action.Edit;
                            this.houseID = houseObj.HOUSEID;
                            InitData();
                        }
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.ToString());
                    //RefreshUI(RefreshedTypes.ProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
            
        }
        #endregion

        #region 保存
        private void Save()
        {
            try
            {
                if (CheckPriority())
                {

                    if (txtContent.RichTextBoxContext == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REQUIRED", "LEASECONTRACT"));
                        return;
                    }
                    RefreshUI(RefreshedTypes.ProgressBar);
                    if (action == Action.Add)
                    {
                        houseObj = new T_OA_HOUSEINFO();
                        houseObj.HOUSEID = Guid.NewGuid().ToString();
                        houseObj.HOUSENAME = txtHouseName.Text.Trim();
                        houseObj.UPTOWN = txtUptown.Text.Trim();
                        houseObj.FLOOR = Convert.ToDecimal(txtFloor.Text.Trim());
                        houseObj.ROOMCODE = txtNum.Text.Trim();
                        houseObj.ISRENT = "0";                        
                        houseObj.CONTENT = txtContent.RichTextBoxContext;
                        houseObj.DEPOSIT = Convert.ToDecimal(txtDeposit.Text);
                        houseObj.MANAGECOST = Convert.ToDecimal(txtManageCost.Text);
                        houseObj.RENTCOST = Convert.ToDecimal(txtRentCost.Text);
                        //houseObj.CONTENT = txtContent.Text.Trim();
                        houseObj.CONTENT = txtContent.RichTextBoxContext;
                        houseObj.SHAREDDEPOSIT = Convert.ToDecimal(txtSharedDeposit.Text);
                        houseObj.SHAREDRENTCOST = Convert.ToDecimal(txtSharedRentCost.Text);
                        houseObj.Number = Convert.ToDecimal(txtNumber.Text);
                        houseObj.REMARK = txtRemark.Text.ToString();

                        houseObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        houseObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        houseObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        houseObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        houseObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        houseObj.CREATEDATE = DateTime.Now;

                        houseObj.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        houseObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        houseObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        houseObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        houseObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;

                        client.AddHouseAsync(houseObj);
                    }
                    else
                    {
                        houseObj.HOUSENAME = txtHouseName.Text.Trim();
                        houseObj.UPTOWN = txtUptown.Text.Trim();
                        houseObj.FLOOR = Convert.ToDecimal(txtFloor.Text.Trim());
                        houseObj.ROOMCODE = txtNum.Text.Trim();
                        //houseObj.ISRENT = txtIsRent.Text.Trim();                                               
                        houseObj.CONTENT = txtContent.RichTextBoxContext;
                        houseObj.DEPOSIT = Convert.ToDecimal(txtDeposit.Text);
                        houseObj.MANAGECOST = Convert.ToDecimal(txtManageCost.Text);
                        houseObj.RENTCOST = Convert.ToDecimal(txtRentCost.Text);
                        //houseObj.CONTENT = txtContent.Text.Trim();
                        houseObj.CONTENT = txtContent.RichTextBoxContext;
                        houseObj.SHAREDDEPOSIT = Convert.ToDecimal(txtSharedDeposit.Text);
                        houseObj.SHAREDRENTCOST = Convert.ToDecimal(txtSharedRentCost.Text);
                        
                        houseObj.Number = Convert.ToDecimal(txtNumber.Text);
                        houseObj.REMARK = txtRemark.Text.ToString();

                        houseObj.UPDATEDATE = DateTime.Now;
                        houseObj.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        houseObj.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        client.UpdateHouseAsync(houseObj);
                    }
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        private bool CheckPriority()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators2 = Group2.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            if (validators2.Count > 0)
            {
                foreach (var h in validators2)
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            int iResult = 0;
            if (!string.IsNullOrEmpty(txtRentCost.Text.Trim()))
            {
                if (!int.TryParse(txtRentCost.Text.Trim(), out iResult))
                {
                    //HtmlPage.Window.Alert("请输入正确的租金！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "RENTAL"));
                    return false;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "RENTAL"));
                return false;
            }
            if (!string.IsNullOrEmpty(txtDeposit.Text.Trim()))
            {
                if (!int.TryParse(txtDeposit.Text.Trim(), out iResult))
                {
                    //HtmlPage.Window.Alert("请输入正确的押金！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "DEPOSIT"));
                    return false;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "DEPOSIT"));
                return false;

            }
            if (!string.IsNullOrEmpty(txtManageCost.Text.Trim()))
            {
                if (!int.TryParse(txtManageCost.Text.Trim(), out iResult))
                {
                    //HtmlPage.Window.Alert("请输入正确的管理费！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "MANAGEMENTFEE"));
                    return false;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "MANAGEMENTFEE"));
                return false;
            }

            if (!string.IsNullOrEmpty(txtFloor.Text.Trim()))
            {
                if (!int.TryParse(txtFloor.Text.Trim(), out iResult))
                {
                    //HtmlPage.Window.Alert("请输入正确的管理费！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "FLOOR"));
                    return false;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "FLOOR"));
                return false;
            }

            if (!string.IsNullOrEmpty(txtSharedDeposit.Text.Trim()))
            {
                if (!int.TryParse(txtSharedDeposit.Text.Trim(), out iResult))
                {
                    //HtmlPage.Window.Alert("请输入正确的管理费！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "SHAREDDEPOSIT"));
                    return false;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "SHAREDDEPOSIT"));
                return false;
            }

            if (!string.IsNullOrEmpty(txtSharedRentCost.Text.Trim()))
            {
                if (!int.TryParse(txtSharedRentCost.Text.Trim(), out iResult))
                {
                    //HtmlPage.Window.Alert("请输入正确的管理费！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "SHAREDRENTCOST"));
                    return false;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "SHAREDRENTCOST"));
                return false;
            }

            if (!string.IsNullOrEmpty(txtNumber.Text.Trim()))
            {
                if (!int.TryParse(txtNumber.Text.Trim(), out iResult))
                {
                    //HtmlPage.Window.Alert("请输入正确的管理费！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "HIRENUMBER"));
                    return false;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "HIRENUMBER"));
                return false;
            }
            return true;
        }

        private bool Check()
        {
            int iResult = 0;
            if (string.IsNullOrEmpty(txtUptown.Text.Trim()))
            {
                //HtmlPage.Window.Alert("小区名称不能为空！");
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "COMMUNITY"));
                return false;
            }
            if (string.IsNullOrEmpty(txtHouseName.Text.Trim()))
            {
                //HtmlPage.Window.Alert("楼栋名称不能为空！");
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "BUILDING"));
                return false;
            }
            if (string.IsNullOrEmpty(txtFloor.Text.Trim()))
            {
                //HtmlPage.Window.Alert("楼层不能为空！");
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "FLOOR"));
                return false;
            }
            if (!int.TryParse(txtFloor.Text.Trim(), out iResult))
            {
                //HtmlPage.Window.Alert("请输入正确的楼层！");
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "FLOOR"));                
                return false;
            }
            if (string.IsNullOrEmpty(txtNum.Text.Trim()))
            {
                //HtmlPage.Window.Alert("房间号不能为空！");
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ROOMNO"));
                return false;
            }
            if (!string.IsNullOrEmpty(txtRentCost.Text.Trim()))
            {
                if (!int.TryParse(txtRentCost.Text.Trim(), out iResult))
                {
                    //HtmlPage.Window.Alert("请输入正确的租金！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "RENTAL"));
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(txtDeposit.Text.Trim()))
            {
                if (!int.TryParse(txtDeposit.Text.Trim(), out iResult))
                {
                    //HtmlPage.Window.Alert("请输入正确的押金！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "DEPOSIT"));
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(txtManageCost.Text.Trim()))
            {
                if (!int.TryParse(txtManageCost.Text.Trim(), out iResult))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FORMATERROR", "MANAGEMENTFEE"));
                    return false;
                }
            }
            if (txtContent.GetRichTextbox().Xaml.Length ==0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "LEASECONTRACT"));
                return false;
            }
            return true;
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            string StrReturn = "";
            switch (action)
            { 
                case Action.Add:
                    StrReturn= Utility.GetResourceStr("ADDTITLE", "HOUSESOURCEINFO");
                    break;
                case Action.Edit:
                    StrReturn= Utility.GetResourceStr("EDITTITLE", "HOUSESOURCEINFO");
                    break;
                case Action.Read:
                    StrReturn= Utility.GetResourceStr("VIEWTITLE", "HOUSESOURCEINFO");
                    break;
                default:
                    StrReturn= Utility.GetResourceStr("HOUSESOURCEINFO");
                    break;
                        
            }
            return StrReturn;
                  
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
            //item = new NavigateItem
            //{
            //    Title = "员工资料",
            //    Tooltip = "员工详细",
            //    Url = "/Personnel/Employee"
            //};
            //items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (action != Action.Read)
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

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            txtContent.Height = ((Grid)sender).ActualHeight * 0.5;
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
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
