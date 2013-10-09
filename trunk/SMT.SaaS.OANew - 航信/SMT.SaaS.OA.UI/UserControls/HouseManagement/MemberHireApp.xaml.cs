using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MemberHireApp : BaseForm,IClient, IEntityEditor
    {
        private T_OA_HOUSEINFO houseInfoObj;
        private SmtOACommonAdminClient client;
        private SMTLoading loadbar = new SMTLoading();
        //public T_OA_HOUSEINFO selectHouseInfoObj;
        public V_HouseHireList selectHouseInfoObj;

        #region 初始化
        public MemberHireApp()
        {
            InitializeComponent();
            InitEvent();
        }

        private void InitEvent()
        {
            selectHouseInfoObj = new V_HouseHireList();
            client = new SmtOACommonAdminClient();
            //client.GetHireAppHouseListPagingCompleted += new EventHandler<GetHireAppHouseListPagingCompletedEventArgs>(client_GetHireAppHouseListPagingCompleted);
            client.GetMemberHireHouseAppListPagingCompleted += new EventHandler<GetMemberHireHouseAppListPagingCompletedEventArgs>(client_GetMemberHireHouseAppListPagingCompleted);
            InitData();
        }

        private void InitData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (Common.CurrentLoginUserInfo != null)
            {
                if (!string.IsNullOrEmpty(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID))
                {
                    filter += "(distrbuteObj.VIEWER =@" + paras.Count().ToString();
                    paras.Add(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
                }
                if (!string.IsNullOrEmpty(Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " or ";
                    }
                    filter += "distrbuteObj.VIEWER =@" + paras.Count().ToString();
                    paras.Add(Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
                }
                if (!string.IsNullOrEmpty(Common.CurrentLoginUserInfo.UserPosts[0].PostID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " or ";
                    }
                    filter += "distrbuteObj.VIEWER =@" + paras.Count().ToString();
                    paras.Add(Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                }
                if (!string.IsNullOrEmpty(Common.CurrentLoginUserInfo.EmployeeID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " or ";
                    }
                    filter += "distrbuteObj.VIEWER =@" + paras.Count().ToString() + ")";
                    paras.Add(Common.CurrentLoginUserInfo.EmployeeID);
                }
            }
            if (!string.IsNullOrEmpty(txtUptown.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "houseInfoObj.COMMUNITY ^@" + paras.Count().ToString();
                paras.Add(txtUptown.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtHouseName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "houseInfoObj.HOUSENAME ^@" + paras.Count().ToString();
                paras.Add(txtHouseName.Text.Trim());
            }
            loadbar.Start();
            SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loginUserInfo.departmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            //if (string.IsNullOrEmpty(loginUserInfo.companyID))
            //{
            //    Utility.GetLoginUserInfo(loginUserInfo);
            //}

            client.GetMemberHireHouseAppListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "houselistObj.CREATEDATE", filter, paras, pageCount, loginUserInfo);
        }

        private void BindData(List<V_HouseHireList> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }
        #endregion

        #region 完成事件

        void client_GetMemberHireHouseAppListPagingCompleted(object sender, GetMemberHireHouseAppListPagingCompletedEventArgs e)
        {
            try
            {
                loadbar.Stop();
                if (e.Error != null)
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != null)
                    {
                        BindData(e.Result.ToList(), e.pageCount);
                    }
                    else
                    {
                        BindData(null, 0);
                    }
                    dataPager.PageCount = e.pageCount;
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        } 


        private void client_GetHireAppHouseListPagingCompleted(object sender, GetHireAppHouseListPagingCompletedEventArgs e)
        {
            
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("COMPANY");
            return Utility.GetResourceStr("SELECTTITLE", "HOUSEINFOISSUANCE");
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
                    Save();
                    break;
                case "1":
                    Cancel();
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
                Key = "0",
                Title = Utility.GetResourceStr("CONFIRM"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CANCEL"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png"
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

        #region 自定义事件
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            //InitData();
        }

        private void Save()
        {
                        
            if (DaGr.SelectedItem != null)
            {
                V_HouseHireList tmp = DaGr.SelectedItem as V_HouseHireList;
                
                selectHouseInfoObj = tmp;

            }
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        private void Cancel()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
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
