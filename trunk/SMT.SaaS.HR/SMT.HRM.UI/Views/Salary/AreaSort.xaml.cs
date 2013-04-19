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
using System.Windows.Navigation;

using System.Windows.Data;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Controls.Primitives;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.HRM.UI.Views.Salary
{
    public partial class AreaSort : BasePage, IClient
    {
        private SalaryServiceClient client = new SMT.Saas.Tools.SalaryWS.SalaryServiceClient();
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient permissionClient = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
        private ObservableCollection<T_SYS_DICTIONARY> PostLevelDicts;
        private IQueryable<T_HR_AREAALLOWANCE> areaAllowance;
        private T_HR_AREAALLOWANCE allowance;
        private ObservableCollection<T_HR_AREAALLOWANCE> allowanceList;
        private List<T_HR_AREADIFFERENCE> areaDifference;
        private T_HR_AREADIFFERENCE currentArea;
        private string AreaID;

        public AreaSort()
        {
            InitializeComponent();
            InitParas();
            LoadArea();
            GetEntityLogo("T_HR_AREADIFFERENCE");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_AREADIFFERENCE", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());

            #region /*测试用*/
            ToolBar.btnDelete.Visibility = Visibility.Visible;
            ToolBar.btnNew.Visibility = Visibility.Visible;
            ToolBar.btnEdit.Visibility = Visibility.Visible;
            #endregion
    
        }

        private void InitParas()
        {
            // 地区分类
            client.GetAreaWithPagingCompleted += new EventHandler<GetAreaWithPagingCompletedEventArgs>(client_GetAreaWithPagingCompleted);
            client.AreaCategoryDeleteCompleted += new EventHandler<AreaCategoryDeleteCompletedEventArgs>(client_AreaCategoryDeleteCompleted);
            
            //城市
            client.AreaCityAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaCityAddCompleted);
            client.AreaCityDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaCityDeleteCompleted);
            client.GetAreaCityWithPagingCompleted += new EventHandler<GetAreaCityWithPagingCompletedEventArgs>(client_GetAreaCityWithPagingCompleted);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnDelete.Visibility = Visibility.Visible;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
        }

        void client_AreaCategoryDeleteCompleted(object sender, AreaCategoryDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "AREADIFFERENCECATEGORY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
               // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "AREADIFFERENCECATEGORY"));
                LoadArea();
                LoadCity();
            }
        }

        /// <summary>
        /// 删除地区分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGridArea.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_AREADIFFERENCE tmp in DtGridArea.SelectedItems)
                {
                    ids.Add(tmp.AREADIFFERENCEID);
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.AreaCategoryDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            }

        }


        /// <summary>
        /// 获取地区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetAreaWithPagingCompleted(object sender, GetAreaWithPagingCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            else
            {
                if (e.Result != null)
                {
                    DtGridArea.ItemsSource = e.Result;
                    areaDifference = e.Result.ToList();
                    dataPagerArea.PageCount = e.pageCount;
                    currentArea = e.Result[0];
                    LoadCity();
                }
            }
        }
        /// <summary>
        /// 获取城市
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetAreaCityWithPagingCompleted(object sender, GetAreaCityWithPagingCompletedEventArgs e)
        {
            List<T_HR_AREACITY> list = new List<T_HR_AREACITY>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGridCity.ItemsSource = list;

                dataPagerCity.PageCount = e.pageCount;
            }

        }
        /// <summary>
        /// 删除城市
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_AreaCityDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CITY"));
                LoadCity();
            }
        }
        /// <summary>
        /// 添加城市
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_AreaCityAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CITY"));
                LoadCity();
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
             LoadArea();
        }


        #region  加载数据
        /// <summary>
        /// 加载地区分类
        /// </summary>
        private void LoadArea()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            client.GetAreaWithPagingAsync(dataPagerArea.PageIndex, dataPagerArea.PageSize, "AREAINDEX", filter, paras, pageCount);
        }

        /// <summary>
        /// 加载地区分类城市
        /// </summary>
        private void LoadCity()
        {
            int pageCount = 0;
            string filter = " 1=1";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            if (currentArea != null)
            {
                filter += " && T_HR_AREADIFFERENCE.AREADIFFERENCEID==@" + paras.Count().ToString();
                paras.Add(currentArea.AREADIFFERENCEID);
            }
            client.GetAreaCityWithPagingAsync(dataPagerCity.PageIndex, dataPagerCity.PageSize, "AREACITYID", filter, paras, pageCount);

        }

        # endregion

        # region 添加 修改 删除


        private void btnAreaEdit_Click(object sender, RoutedEventArgs e)
        {


        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.AreaSortForm form = new Form.Salary.AreaSortForm(FormTypes.New,"");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 200;
            form.MinWidth = 600;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }


        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGridArea.SelectedItems.Count > 0)
            {
                Form.Salary.AreaSortForm form = new Form.Salary.AreaSortForm(FormTypes.Edit, (DtGridArea.SelectedItems[0] as T_HR_AREADIFFERENCE).AREADIFFERENCEID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 200;
                form.MinWidth = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }

        }

        //private void btnCityDel_Click(object sender, RoutedEventArgs e)
        //{
        //    string Result = "";
        //    if (DtGridCity.SelectedItems.Count > 0)
        //    {
        //        ObservableCollection<string> ids = new ObservableCollection<string>();

        //        foreach (T_HR_AREACITY tmp in DtGridCity.SelectedItems)
        //        {
        //            ids.Add(tmp.AREACITYID);
        //        }
        //        ComfirmWindow com = new ComfirmWindow();
        //        com.OnSelectionBoxClosed += (obj, result) =>
        //        {
        //            client.AreaCityDeleteAsync(ids);
        //        };
        //        com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);

        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
        //        return;
        //    }


        //}


        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {

        }

        public List<T_HR_AREAALLOWANCE> Pager(IQueryable<T_HR_AREAALLOWANCE> ents, int pageIndex, int pageSize, ref int pageCount)
        {
            int count = ents.Count();
            pageCount = count / pageSize;
            int tmp = count % pageSize;

            pageCount = pageCount + (tmp > 0 ? 1 : 0);
            if (pageIndex > pageCount)
                pageIndex = 1;

            ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return ents.ToList();
        }

        private void GridPagerCity_Click(object sender, RoutedEventArgs e)
        {
            LoadCity();
        }

        private void GridPagerArea_Click(object sender, RoutedEventArgs e)
        {
            LoadArea();
        }

        private void DtGridArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DtGridArea.SelectedItems.Count > 0)
            {
                currentArea = DtGridArea.SelectedItems[0] as T_HR_AREADIFFERENCE;
                AreaID = currentArea.AREADIFFERENCEID;
                LoadCity();
            }


        }

        private void DtGridArea_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGridArea, e.Row, "T_HR_CUSTOMGUERDONSET");
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            //if (DtGrid.SelectedItems.Count > 0)
            //{
            //    T_HR_AREAALLOWANCE temp = DtGrid.SelectedItem as T_HR_AREAALLOWANCE;
            //    temp.T_HR_AREADIFFERENCE = new T_HR_AREADIFFERENCE();
            //    temp.T_HR_AREADIFFERENCE.AREADIFFERENCEID = currentArea.AREADIFFERENCEID;


            //    client.GetAreaAllowanceByIDAsync(temp.AREAALLOWANCEID);
            //    if (allowance == null)
            //    {
            //        if (temp.AREAALLOWANCEID == null)
            //        {
            //            temp.AREAALLOWANCEID = Guid.NewGuid().ToString();
            //        }
            //        temp.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //        temp.CREATEDATE = System.DateTime.Now;
            //        client.AreaAllowanceADDAsync(temp);
            //    }
            //    else
            //    {
            //        temp.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //        temp.UPDATEDATE = System.DateTime.Now;
            //        client.AreaAllowanceUpdateAsync(temp);
            //    }
            //}
            client.AreaAllowanceAsync(allowanceList);

        }

        private void btnAllowanceAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DtGridArea.SelectedItems.Count > 0)
            {
                Form.Salary.AreaAllowanceForm form = new SMT.HRM.UI.Form.Salary.AreaAllowanceForm(FormTypes.New, "", AreaID,"");
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 150;
                form.MinWidth = 410;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTEAERA"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTEAERA"));
            }

        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        private void DtGridCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        void browser_ReloadDataEvent()
        {
            LoadArea();
            LoadCity();
        }

        private void DtGridCity_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGridCity, e.Row, "T_HR_AREAALLOWANCE");
        }


    }
}
