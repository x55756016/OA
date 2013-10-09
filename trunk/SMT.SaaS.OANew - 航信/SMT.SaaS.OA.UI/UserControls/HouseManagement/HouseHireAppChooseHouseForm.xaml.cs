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
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class HouseHireAppChooseHouseForm : BaseForm,IClient,IEntityEditor
    {
        private T_OA_HOUSEINFO houseInfoObj;
        private SmtOACommonAdminClient client;
        private SMTLoading loadbar = new SMTLoading();
        public T_OA_HOUSEINFO selectHouseInfoObj;


        #region 初始化
        public HouseHireAppChooseHouseForm()
        {
            InitializeComponent();
            InitEvent();
        }

        private void InitEvent()
        {
            selectHouseInfoObj = new T_OA_HOUSEINFO();
            client = new SmtOACommonAdminClient();
            client.GetHireAppHouseListPagingCompleted += new EventHandler<GetHireAppHouseListPagingCompletedEventArgs>(client_GetHireAppHouseListPagingCompleted);
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
                    filter += "(q.VIEWER =@" + paras.Count().ToString();
                    paras.Add(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
                }
                if (!string.IsNullOrEmpty(Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " or ";
                    }
                    filter += "q.VIEWER =@" + paras.Count().ToString();
                    paras.Add(Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
                }
                if (!string.IsNullOrEmpty(Common.CurrentLoginUserInfo.UserPosts[0].PostID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " or ";
                    }
                    filter += "q.VIEWER =@" + paras.Count().ToString();
                    paras.Add(Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                }
                if (!string.IsNullOrEmpty(Common.CurrentLoginUserInfo.EmployeeID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " or ";
                    }
                    filter += "q.VIEWER =@" + paras.Count().ToString() + ")";
                    paras.Add(Common.CurrentLoginUserInfo.EmployeeID);
                }
            }
            if (!string.IsNullOrEmpty(txtUptown.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "l.m.COMMUNITY ^@" + paras.Count().ToString();
                paras.Add(txtUptown.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtHouseName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "l.m.HOUSENAME ^@" + paras.Count().ToString();
                paras.Add(txtHouseName.Text.Trim());
            }
            loadbar.Start();
            SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();           
            if (string.IsNullOrEmpty(loginUserInfo.companyID))
            {
                Utility.GetLoginUserInfo(loginUserInfo);
            }
            client.GetHireAppHouseListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo);
        }

        private void BindData(List<T_OA_HOUSEINFO> obj, int pageCount)
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
        private void client_GetHireAppHouseListPagingCompleted(object sender, GetHireAppHouseListPagingCompletedEventArgs e)
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
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ico_16_delete.png"
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

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox ckbSelect = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (ckbSelect.IsChecked == true)
            //            {
            //                T_OA_HOUSEINFO tmp = ckbSelect.DataContext as T_OA_HOUSEINFO;
            //                selectHouseInfoObj = tmp;
            //                break;
            //            }
            //        }
            //    }
            //}
            if (DaGr.SelectedItem != null)
            {
                T_OA_HOUSEINFO tmp = DaGr.SelectedItem as T_OA_HOUSEINFO;
                
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
