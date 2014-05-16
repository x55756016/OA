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
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Windows.Controls.Primitives;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.Travelmanagement
{
    public partial class AreaSort : BasePage
    {


        private SmtOAPersonOfficeClient client = new SmtOAPersonOfficeClient();
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient permissionClient = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
        private ObservableCollection<T_SYS_DICTIONARY> PostLevelDicts;
        private IQueryable<T_OA_AREAALLOWANCE> areaAllowance;
        private T_OA_AREAALLOWANCE allowance;
        private ObservableCollection<T_OA_AREAALLOWANCE> allowanceList;
        private List<T_OA_AREADIFFERENCE> areaDifference;
        private T_OA_AREADIFFERENCE currentArea;
        private List<T_OA_TRAVELSOLUTIONS> travelObj = new List<T_OA_TRAVELSOLUTIONS>();
        private T_OA_TRAVELSOLUTIONS solutionsObj = new T_OA_TRAVELSOLUTIONS();
        private string AreaID;

        public AreaSort()
        {
            InitializeComponent();
            InitParas();
            LoadArea();
            GetEntityLogo("T_OA_AREADIFFERENCE");
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_AREADIFFERENCE", true);
            LoadSolutionInfos();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

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
            client.GetQueryPlanCityCompleted += new EventHandler<GetQueryPlanCityCompletedEventArgs>(client_GetQueryPlanCityCompleted);
            client.GetTravelSolutionFlowCompleted += new EventHandler<GetTravelSolutionFlowCompletedEventArgs>(client_GetTravelSolutionFlowCompleted);
            client.GetAreaCityByCategoryCompleted += new EventHandler<GetAreaCityByCategoryCompletedEventArgs>(client_GetAreaCityByCategoryCompleted);

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
            ToolBar.ShowRect();
        }

        void client_GetAreaCityByCategoryCompleted(object sender, GetAreaCityByCategoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    LoadCity();
                }
            }
        }

        void client_GetQueryPlanCityCompleted(object sender, GetQueryPlanCityCompletedEventArgs e)
        {
            try
            {
                if (!e.Cancelled)
                {
                    if (e.Result != null)
                    {
                        if (e.Result.Count() > 0)
                        {
                            e.Result.ToList().ForEach(item =>
                            {
                                T_OA_AREACITY Sport = new T_OA_AREACITY();
                                Sport.AREACITYID = item.AREACITYID;
                                //Sport.T_OA_AREADIFFERENCE.AREADIFFERENCEID = item.T_OA_AREADIFFERENCE.AREADIFFERENCEID;
                                Sport.CITY = item.CITY;
                                Sport.CREATEDATE = item.CREATEDATE;
                                Sport.CREATEUSERID = item.CREATEUSERID;
                                Sport.UPDATEDATE = item.UPDATEDATE;
                                Sport.UPDATEUSERID = item.UPDATEUSERID;
                                Sport.EntityKey = item.EntityKey;
                            });
                            if (e.ListAREADIFFERENCE.Count() > 0)
                            {
                                if(DtGridArea.ItemsSource==null) DtGridArea.ItemsSource = e.ListAREADIFFERENCE;
                                client.GetAreaCityByCategoryAsync(e.ListAREADIFFERENCE.FirstOrDefault().AREADIFFERENCEID);
                            }
                        }
                        else
                        {
                            cmbSolution.ItemsSource = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        void client_GetTravelSolutionFlowCompleted(object sender, GetTravelSolutionFlowCompletedEventArgs e)
        {

            try
            {
                if (e.Result != null)
                {
                    travelObj = e.Result.ToList();
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                    client.GetQueryPlanCityAsync(solutionsObj.TRAVELSOLUTIONSID, null);
                }
                else
                {
                    BindDataGrid(null, 0);
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        private void BindDataGrid(List<T_OA_TRAVELSOLUTIONS> obj, int pageCount)
        {

            if (obj == null || obj.Count < 1)
            {
                this.cmbSolution.ItemsSource = null;
                return;
            }
            cmbSolution.ItemsSource = obj;
            cmbSolution.DisplayMemberPath = "PROGRAMMENAME";
            foreach (T_OA_TRAVELSOLUTIONS Region in cmbSolution.Items)
            {
                if (Region.OWNERCOMPANYID == Common.CurrentLoginUserInfo.UserPosts[0].CompanyID)
                {
                    cmbSolution.SelectedItem = Region;
                }
                else
                {
                    cmbSolution.SelectedIndex = 0;
                }
                solutionsObj = Region;
            }
        }

        void client_AreaCategoryDeleteCompleted(object sender, AreaCategoryDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                return;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "AREADIFFERENCECATEGORY"));
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

                foreach (T_OA_AREADIFFERENCE tmp in DtGridArea.SelectedItems)
                {
                    if (Common.CurrentLoginUserInfo.UserPosts[0].CompanyID != tmp.CREATECOMPANYID)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("PROMPT"), "不能删除修改其他公司创建的的城市分类");
                        return;
                    }
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                return;
            }
            else
            {
                if (e.Result != null)
                {
                    DtGridArea.ItemsSource = e.Result;
                    areaDifference = e.Result.ToList();
                    //dataPagerArea.PageCount = e.pageCount;
                    if (e.Result.Count() > 0)
                    {
                        currentArea = e.Result[0];
                    }
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
            List<T_OA_AREACITY> list = new List<T_OA_AREACITY>();
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                return;
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGridCity.ItemsSource = list;


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

                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CITY"));
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

                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CITY"));
                LoadCity();
            }
        }

        bool isRefresh = false;
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            isRefresh = true;
            LoadArea();
        }


        #region  加载数据
        /// <summary>
        /// 加载地区分类
        /// </summary>
        private void LoadArea()
        {
            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            filter = null;
            //if (currentArea != null && isRefresh == false)
            //{
            //    filter += " && T_OA_AREADIFFERENCE.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID==@" + paras.Count().ToString();
            //    paras.Add(solutionsObj.TRAVELSOLUTIONSID);
            //}
            //else
            //{
            //    isRefresh = false;
            //    currentArea = null;
            //}

            if (cmbSolution.SelectedIndex > 0)
            {
                T_OA_TRAVELSOLUTIONS travelObjs = cmbSolution.SelectedItem as T_OA_TRAVELSOLUTIONS;
                if (travelObjs != null)
                {
                    client.GetAreaWithPagingAsync(1, 100, "AREAINDEX", filter, paras, pageCount, loginUserInfo.companyID, travelObjs.TRAVELSOLUTIONSID);
                }
            }
            else
            {
                client.GetAreaWithPagingAsync(1, 100, "AREAINDEX", filter, paras, pageCount, loginUserInfo.companyID, null);
            }
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
                filter += " && T_OA_AREADIFFERENCE.AREADIFFERENCEID==@" + paras.Count().ToString();
                paras.Add(currentArea.AREADIFFERENCEID);
            }
            client.GetAreaCityWithPagingAsync(1, 100, "AREACITYID", filter, paras, pageCount);

        }

        # endregion

        # region 添加 修改 删除


        private void btnAreaEdit_Click(object sender, RoutedEventArgs e)
        {


        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            AreaSortForm form = new AreaSortForm(FormTypes.New, "", solutionsObj);
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinHeight = 200;
            browser.MinWidth = 600;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }


        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
           
            this.cmbSolution.IsEnabled = true;//修改时启用选择方案cmbox
            if (DtGridArea.SelectedItems.Count > 0)
            {
                var item=DtGridArea.SelectedItems[0] as T_OA_AREADIFFERENCE;
                if ( Common.CurrentLoginUserInfo.UserPosts[0].CompanyID != item.CREATECOMPANYID)
                {
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("PROMPT"), "不能删除修改其他公司创建的的城市分类");
                    return;
                }

                AreaSortForm form = new AreaSortForm(FormTypes.Edit, (DtGridArea.SelectedItems[0] as T_OA_AREADIFFERENCE).AREADIFFERENCEID, solutionsObj);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 200;
                browser.MinWidth = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }

        }

        //private void btnCityDel_Click(object sender, RoutedEventArgs e)
        //{
        //    string Result = "";
        //    if (DtGridCity.SelectedItems.Count > 0)
        //    {
        //        ObservableCollection<string> ids = new ObservableCollection<string>();

        //        foreach (T_OA_AREACITY tmp in DtGridCity.SelectedItems)
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

        public List<T_OA_AREAALLOWANCE> Pager(IQueryable<T_OA_AREAALLOWANCE> ents, int pageIndex, int pageSize, ref int pageCount)
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
                currentArea = DtGridArea.SelectedItems[0] as T_OA_AREADIFFERENCE;
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
            //    T_OA_AREAALLOWANCE temp = DtGrid.SelectedItem as T_OA_AREAALLOWANCE;
            //    temp.T_OA_AREADIFFERENCE = new T_OA_AREADIFFERENCE();
            //    temp.T_OA_AREADIFFERENCE.AREADIFFERENCEID = currentArea.AREADIFFERENCEID;


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
            client.AreaAllowanceAsync(allowanceList, solutionsObj.TRAVELSOLUTIONSID);

        }

        private void btnAllowanceAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DtGridArea.SelectedItems.Count > 0)
            {
                AreaAllowanceForm form = new AreaAllowanceForm(FormTypes.New, "", AreaID, solutionsObj.TRAVELSOLUTIONSID, "");
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 150;
                browser.MinWidth = 410;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTEAERA"));
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
            SetRowLogo(DtGridCity, e.Row, "T_OA_AREAALLOWANCE");
        }

        #region 加载数据
        void LoadSolutionInfos()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件

            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值

            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            client.GetTravelSolutionFlowAsync(0, 100, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);
        }
        #endregion

        #region 出差方案选择
        private void cmbSolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            solutionsObj = cmbSolution.SelectedItem as T_OA_TRAVELSOLUTIONS;
            if (travelObj != null)
            {
                client.GetQueryPlanCityAsync(solutionsObj.TRAVELSOLUTIONSID, null);
            }
        }
        #endregion
    }
}
