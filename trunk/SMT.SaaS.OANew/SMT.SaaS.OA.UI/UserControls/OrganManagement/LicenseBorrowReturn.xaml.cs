using System;
using System.Collections.Generic;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class LicenseBorrowReturn : BaseForm,IClient,IEntityEditor
    {
        //private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private Action action;
        private SmtOADocumentAdminClient client;
        private T_OA_LICENSEUSER licenseObj;

        public T_OA_LICENSEUSER LicenseObj
        {
            get { return licenseObj; }
            set 
            {
                this.DataContext = value;
                licenseObj = value; 
            }
        }

        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;

        #region 初始化
        public LicenseBorrowReturn(Action action, T_OA_LICENSEUSER licenseObj)
        {
            InitializeComponent();
            this.action = action;
            LicenseObj = licenseObj;
            InitEvent();
        }

        private void InitEvent()
        {
            if (action == Action.Return)
            {
                this.txtStartDate.IsEnabled = false;
            }
            client = new SmtOADocumentAdminClient();
            client.LendOrReturnCompleted += new EventHandler<LendOrReturnCompletedEventArgs>(client_LendOrReturnCompleted);
        }        
        #endregion

        #region 完成事件
        private void client_LendOrReturnCompleted(object sender, LendOrReturnCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result)
                    {
                        if (action == Action.Lend)
                        {
                            //HtmlPage.Window.Alert("外借证照成功！");
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("LENDSUCCESSED"));
                        }
                        else
                        {
                            //HtmlPage.Window.Alert("归还证照成功！");
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("RETURNSUCCESSED"));
                        }
                    }
                    else
                    {
                        if (action == Action.Lend)
                        {
                            //HtmlPage.Window.Alert("外借证照失败,请重试！");
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILURE"), Utility.GetResourceStr("LENDFAILURE"));
                        }
                        else
                        {
                            //HtmlPage.Window.Alert("归还证照失败,请重试！");
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILURE"), Utility.GetResourceStr("RETURNFAILURE"));
                        }
                    }
                    //this.ReloadData();
                    //todo: closeandrefresh

                    RefreshUI(refreshType);
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    //RefreshUI(RefreshedTypes.ProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {            
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "LICENSELENDING");
            }
            else
            {
                return Utility.GetResourceStr("EDITTITLE", "LICENSELENDING");
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
                    //refreshType = RefreshedTypes.All;
                    refreshType = RefreshedTypes.CloseAndReloadData;
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

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                //Title = Utility.GetResourceStr("CLOSE"),
                //ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png"
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

        private List<ToolbarItem> CreateFormNewButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
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

            return items;
        }

        private List<ToolbarItem> CreateFormEditButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png"
            };

            items.Add(item);



            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "5",
            //    Title = "提交审核",
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};

            //items.Add(item);
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "6",
            //    Title = "审核",//"审核"
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};

            //items.Add(item);
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "7",
            //    Title = "审核不通过",//"审核不通过"
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4424.png"
            //};

            //items.Add(item);

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

        #region 自定义函数
        private void Save()
        {
            try
            {
                if (Check())
                {
                    RefreshUI(RefreshedTypes.ProgressBar);
                    licenseObj.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    licenseObj.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    licenseObj.UPDATEDATE = DateTime.Now;
                    if (action == Action.Lend)
                    {
                        licenseObj.HASRETURN = "0";
                        licenseObj.STARTDATE = Convert.ToDateTime(this.txtStartDate.SelectedDate);
                    }
                    else
                    {
                        licenseObj.HASRETURN = "1";
                        licenseObj.ENDDATE = Convert.ToDateTime(this.txtEndDate.SelectedDate);
                    }
                    client.LendOrReturnAsync(licenseObj, action.ToString());
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        private bool Check()
        {
            if (licenseObj != null)
            {
                if (action == Action.Lend)
                {
                    if (Convert.ToDateTime(licenseObj.STARTDATE).Year < 2000)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATENOTNULL", "LENDTIME"));
                        return false;
                    }
                }
                else
                {
                    if (Convert.ToDateTime(licenseObj.ENDDATE).Year < 2000)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATENOTNULL", "RETURNTIME"));
                        return false;
                    }
                }
                if (licenseObj.STARTDATE > licenseObj.ENDDATE)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEGREATERERROR", "RETURNTIME,LENDTIME"));
                    return false;
                }
                return true;
            }
            return false;
        }
        #endregion

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
