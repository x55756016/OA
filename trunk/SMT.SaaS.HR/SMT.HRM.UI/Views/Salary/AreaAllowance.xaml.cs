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
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Views.Salary
{
    public partial class AreaAllowance : BasePage,IClient
    {
        private T_HR_AREADIFFERENCE selectedAreaDifference;//选中的实体
        private SalaryServiceClient client = new SMT.Saas.Tools.SalaryWS.SalaryServiceClient();
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient permissionClient = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
        // private PermissionServiceClient permissionClientOther = new PermissionServiceClient();
        private ObservableCollection<T_SYS_DICTIONARY> PostLevelDicts;
        private IQueryable<T_HR_AREAALLOWANCE> areaAllowance;
        private T_HR_AREAALLOWANCE allowance;
        private ObservableCollection<T_HR_AREAALLOWANCE> allowanceList;
        private List<T_HR_AREADIFFERENCE> areaDifference;
        private T_HR_AREADIFFERENCE currentArea;
        //   private List<T_SYS_DICTIONARY> CityDicts;
        string AreaID;
        //  string DictionaryType;

        public AreaAllowance()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(AreaAllowance_Loaded);
            //InitParas();
            //LoadArea();
            //GetEntityLogo("T_HR_AREAALLOWANCE");
        }

        void AreaAllowance_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            LoadArea();
            GetEntityLogo("T_HR_AREAALLOWANCE");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_AREAALLOWANCE", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitParas()
        {
            permissionClient.GetSysDictionaryByCategoryCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs>(permissionClient_GetSysDictionaryByCategoryCompleted);
            //   permissionClientOther.GetSysDictionaryByFatherIDCompleted += new EventHandler<GetSysDictionaryByFatherIDCompletedEventArgs>(permissionClientOther_GetSysDictionaryByFatherIDCompleted);
            //获取字典中的岗位级别
            //   DictionaryType = "POSTLEVEL";
            permissionClient.GetSysDictionaryByCategoryAsync("POSTLEVEL");

            //地区差异补贴
            client.GetAreaAllowanceByAreaIDCompleted += new EventHandler<GetAreaAllowanceByAreaIDCompletedEventArgs>(client_GetAreaAllowanceByAreaIDCompleted);
            client.GetAreaAllowanceByIDCompleted += new EventHandler<GetAreaAllowanceByIDCompletedEventArgs>(client_GetAreaAllowanceByIDCompleted);

            // 地区分类
            client.GetAreaWithPagingCompleted += new EventHandler<GetAreaWithPagingCompletedEventArgs>(client_GetAreaWithPagingCompleted);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
        }



        #region completed事件
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
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                return;
            }
            else
            {
                if (e.Result != null)
                {
                    DtGridArea.ItemsSource = e.Result;
                    areaDifference = e.Result.ToList();
                    dataPagerArea.PageCount = e.pageCount;
                    if (this.selectedAreaDifference != null)
                    {
                        DtGridArea.SelectedItem = selectedAreaDifference;
                        AreaID = selectedAreaDifference.AREADIFFERENCEID;
                    }
                    else
                    {
                        AreaID = areaDifference.FirstOrDefault().AREADIFFERENCEID;
                    }
                    client.GetAreaAllowanceByAreaIDAsync(AreaID);
                }
            }
        }

        /// <summary>
        ///根据ID获取地区差异补贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetAreaAllowanceByIDCompleted(object sender, GetAreaAllowanceByIDCompletedEventArgs e)
        {

            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                allowance = e.Result;
            }
        }
        /// <summary>
        /// 根据地区分类获取补贴列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetAreaAllowanceByAreaIDCompleted(object sender, GetAreaAllowanceByAreaIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    areaAllowance = e.Result.AsQueryable();
                }
                else
                {
                    areaAllowance = null;
                }

                LoadData();

            }
        }
        /// <summary>
        ///从字典读取岗位级别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void permissionClient_GetSysDictionaryByCategoryCompleted(object sender, SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                PostLevelDicts = e.Result;
            }

        }
        # endregion
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
        /// 加载地区差异补贴
        /// </summary>

        private void LoadData()
        {
            int pageCount = 0;
            //   string filter = "";
            allowanceList = new ObservableCollection<T_HR_AREAALLOWANCE>();
            List<T_HR_AREAALLOWANCE> temp = new List<T_HR_AREAALLOWANCE>();
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            if (PostLevelDicts != null)
            {
                if (areaAllowance != null)
                {
                    var ents = from c in PostLevelDicts
                               join b in areaAllowance on c.DICTIONARYVALUE.ToString() equals b.POSTLEVEL into d
                               from b in d.DefaultIfEmpty()

                               select new T_HR_AREAALLOWANCE
                               {
                                   ALLOWANCE = b == null ? null : b.ALLOWANCE,
                                   POSTLEVEL = c.DICTIONARYNAME,
                                   AREAALLOWANCEID = b == null ? Guid.NewGuid().ToString() : b.AREAALLOWANCEID,
                                   CREATEUSERID = b == null ? null : b.CREATEUSERID
                               };

                    temp = Pager(ents.AsQueryable(), dataPager.PageIndex, dataPager.PageSize, ref pageCount);
                    DtGrid.ItemsSource = temp;
                    dataPager.PageCount = pageCount;
                }
                else
                {
                    var ents = from c in PostLevelDicts
                               //join b in areaAllowance on c.DICTIONARYNAME equals b.POSTLEVEL into d
                               //from b in d.DefaultIfEmpty()

                               select new T_HR_AREAALLOWANCE
                               {
                                   ALLOWANCE = null,
                                   POSTLEVEL = c.DICTIONARYNAME,
                                   AREAALLOWANCEID = Guid.NewGuid().ToString(),
                                   CREATEUSERID = null
                               };

                    temp = Pager(ents.AsQueryable(), dataPager.PageIndex, dataPager.PageSize, ref pageCount);
                    DtGrid.ItemsSource = temp;
                    dataPager.PageCount = pageCount;
                }
                foreach (T_HR_AREAALLOWANCE tmp in temp)
                {
                    tmp.T_HR_AREADIFFERENCE = new T_HR_AREADIFFERENCE();
                    tmp.T_HR_AREADIFFERENCE.AREADIFFERENCEID = AreaID;
                    if (string.IsNullOrEmpty(tmp.CREATEUSERID))
                    {
                        tmp.CREATEUSERID = SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        tmp.CREATEDATE = System.DateTime.Now;

                    }
                    else
                    {
                        tmp.UPDATEUSERID = SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        tmp.UPDATEDATE = System.DateTime.Now;
                    }
                    allowanceList.Add(tmp);
                }
            }

        }

        # endregion

        # region 添加 修改 删除


        private void btnAreaEdit_Click(object sender, RoutedEventArgs e)
        {


        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ////Form.Salary.AreaForm form = new SMT.HRM.UI.Form.Salary.AreaForm(FormTypes.New, "");
            //EntityBrowser browser = new EntityBrowser(form);
            ////form.MinHeight = 120;
            ////form.MinWidth = 380;
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }


        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // var lowLevel = areaAllowanceDicts.Max(s => Convert.ToInt32(s.DICTIONARYVALUE));

            // client.GenerateSalaryLevelAsync(lowLevel, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            if (DtGridArea.SelectedItems.Count > 0)
            {
                Form.Salary.AreaForm form = new SMT.HRM.UI.Form.Salary.AreaForm(FormTypes.Edit, (DtGridArea.SelectedItems[0] as T_HR_AREADIFFERENCE).AREADIFFERENCEID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 120;
                form.MinWidth = 380;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }

        }
        private void btnCityAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DtGridArea.SelectedItems.Count > 0)
            {
                Form.Salary.CityForm form = new SMT.HRM.UI.Form.Salary.CityForm(FormTypes.New,(DtGridArea.SelectedItems[0] as T_HR_AREADIFFERENCE).AREADIFFERENCEID, "");
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 150;
                form.MinWidth = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTEAERA"));
            }

        }


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

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
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
                client.GetAreaAllowanceByAreaIDAsync(AreaID);
            }


        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DtGridArea_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGridArea, e.Row, "T_HR_CUSTOMGUERDONSET");
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_AREAALLOWANCE");
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTEAERA"));
            }

        }

        #region 修改岗位代码
        //private void btnAllowanceEdit_Click(object sender, RoutedEventArgs e)
        //{

        //    if (DtGrid.SelectedItems.Count > 0)
        //    {
        //        Form.Salary.AreaAllowanceForm form = new SMT.HRM.UI.Form.Salary.AreaAllowanceForm(FormTypes.Edit, (DtGrid.SelectedItems[0] as T_HR_AREAALLOWANCE).AREAALLOWANCEID, AreaID);
        //        EntityBrowser browser = new EntityBrowser(form);
        //        form.MinHeight = 150;
        //        form.MinWidth = 410;
        //        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //        browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
        //        return;
        //    }
        //}
        #endregion

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

        void browser_ReloadDataEvent()
        {
            LoadArea();
            //LoadData();
            client.GetAreaAllowanceByAreaIDAsync(AreaID);
        }

        private void btEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                this.selectedAreaDifference = (DtGrid.SelectedItems[0] as T_HR_AREAALLOWANCE).T_HR_AREADIFFERENCE;
                Form.Salary.AreaAllowanceForm form = new SMT.HRM.UI.Form.Salary.AreaAllowanceForm(FormTypes.Edit, (DtGrid.SelectedItems[0] as T_HR_AREAALLOWANCE).AREAALLOWANCEID, AreaID, (DtGrid.SelectedItems[0] as T_HR_AREAALLOWANCE).POSTLEVEL);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 150;
                form.MinWidth = 410;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }

    }
}
