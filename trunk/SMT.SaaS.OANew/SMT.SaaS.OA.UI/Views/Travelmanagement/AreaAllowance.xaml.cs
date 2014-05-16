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

using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.Travelmanagement
{
    public partial class AreaAllowance : BasePage
    {

        private SmtOAPersonOfficeClient client = new SmtOAPersonOfficeClient();
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient permissionClient = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
        private List<T_SYS_DICTIONARY> PostLevelDicts;
        private IQueryable<T_OA_AREAALLOWANCE> areaAllowance;
        private T_OA_AREAALLOWANCE allowance;
        private ObservableCollection<T_OA_AREAALLOWANCE> allowanceList;
        private List<T_OA_AREADIFFERENCE> areaDifference;
        private T_OA_AREADIFFERENCE currentArea;
        private List<T_OA_TRAVELSOLUTIONS> travelObj = new List<T_OA_TRAVELSOLUTIONS>();
        private T_OA_TRAVELSOLUTIONS solutionsObj = new T_OA_TRAVELSOLUTIONS();
        string AreaID;

        public AreaAllowance()
        {
            InitializeComponent();
            this.Loaded += (o, e) =>
            {
                GetEntityLogo("T_OA_AREAALLOWANCE");
                Utility.DisplayGridToolBarButton(ToolBar1, "T_OA_AREAALLOWANCE", true);
                InitParas();
            };
        }

        // 当用户导航到此页面时执行。
        private void InitParas()
        {
            //permissionClient.GetSysDictionaryByCategoryCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs>(permissionClient_GetSysDictionaryByCategoryCompleted);
            //   permissionClientOther.GetSysDictionaryByFatherIDCompleted += new EventHandler<GetSysDictionaryByFatherIDCompletedEventArgs>(permissionClientOther_GetSysDictionaryByFatherIDCompleted);
            //地区差异补贴
            client.GetAreaAllowanceByAreaIDCompleted += new EventHandler<GetAreaAllowanceByAreaIDCompletedEventArgs>(client_GetAreaAllowanceByAreaIDCompleted);
            //client.GetAreaAllowanceByIDCompleted += new EventHandler<GetAreaAllowanceByIDCompletedEventArgs>(client_GetAreaAllowanceByIDCompleted);
            //client.GetQueryProgramSubsidiesCompleted += new EventHandler<GetQueryProgramSubsidiesCompletedEventArgs>(client_GetQueryProgramSubsidiesCompleted);
            client.GetTravelSolutionFlowCompleted += new EventHandler<GetTravelSolutionFlowCompletedEventArgs>(client_GetTravelSolutionFlowCompleted);

            // 地区分类
            client.GetAreaWithPagingCompleted += new EventHandler<GetAreaWithPagingCompletedEventArgs>(client_GetAreaWithPagingCompleted);

            ToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar1.btnNew.Visibility = Visibility.Collapsed;
            ToolBar1.retNew.Visibility = Visibility.Collapsed;
            ToolBar1.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar1.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar1.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar1.BtnView.Visibility = Visibility.Collapsed;
            ToolBar1.retRead.Visibility = Visibility.Collapsed;
            ToolBar1.retAudit.Visibility = Visibility.Collapsed;
            ToolBar1.retDelete.Visibility = Visibility.Collapsed;
            ToolBar1.ShowRect();
            //开始获取数据
            PostLevelDicts = (from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                             where a.DICTIONCATEGORY == "POSTLEVEL"
                             orderby a.DICTIONARYVALUE
                             select a).ToList();
            LoadSolutionInfos();
        }
        
        #region 根据出差方案查询数据

        #region "第1步，获取出差方案"

        void LoadSolutionInfos()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件

            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值

            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            this.RefreshUI(RefreshedTypes.ShowProgressBar);
            client.GetTravelSolutionFlowAsync(0, 100, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);
        }
        void client_GetTravelSolutionFlowCompleted(object sender, GetTravelSolutionFlowCompletedEventArgs e)
        {

            try
            {
                if (e.Result != null)
                {
                    travelObj = e.Result.ToList();
                    BindComboBox(e.Result.ToList(), e.pageCount);
                    //获取城市分类触发绑定BindComboBox事件，这里无需//LoadArea();                    
                }
                else
                {
                    BindComboBox(null, 0);
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            finally
            {
                this.RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        private void BindComboBox(List<T_OA_TRAVELSOLUTIONS> obj, int pageCount)
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
        #endregion

        #region "第2步获取地区分类"
        /// <summary>
        /// 地区分类
        /// </summary>
        private void LoadArea()
        {
            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();//参数值

            //filter += "OWNERCOMPANYID ^@" + paras.Count().ToString();
            //paras.Add(loginUserInfo.companyID); 
            this.RefreshUI(RefreshedTypes.ShowProgressBar);
            if (cmbSolution.SelectedIndex > 0)
            {
                T_OA_TRAVELSOLUTIONS travelObjs = cmbSolution.SelectedItem as T_OA_TRAVELSOLUTIONS;
                if (travelObjs != null)
                {
                    client.GetAreaWithPagingAsync(1, 100, "AREAINDEX ascending", filter, paras, pageCount, loginUserInfo.companyID, travelObjs.TRAVELSOLUTIONSID);
                }
            }
            else
            {
                client.GetAreaWithPagingAsync(1, 100, "AREAINDEX ascending", filter, paras, pageCount, loginUserInfo.companyID, null);
            }
        }

        /// <summary>
        /// 获取地区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetAreaWithPagingCompleted(object sender, GetAreaWithPagingCompletedEventArgs e)
        {
            try
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
                        if (areaDifference != null && areaDifference.Count() > 0)
                        {
                            AreaID = areaDifference.FirstOrDefault().AREADIFFERENCEID;
                            this.RefreshUI(RefreshedTypes.ShowProgressBar);
                            client.GetAreaAllowanceByAreaIDAsync(AreaID, solutionsObj.TRAVELSOLUTIONSID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                this.RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion 

        #region "第3步根据地区分类获取补贴"
        /// <summary>
        /// 根据地区分类获取补贴列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetAreaAllowanceByAreaIDCompleted(object sender, GetAreaAllowanceByAreaIDCompletedEventArgs e)
        {
            try
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
                    BindAreaAllowanceData();
                    //client.GetQueryProgramSubsidiesAsync(travelObj.FirstOrDefault().TRAVELSOLUTIONSID);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                this.RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        /// <summary>
        /// 加载地区差异补贴
        /// </summary>
        private void BindAreaAllowanceData()
        {
            string solutionId = string.Empty;
            if (cmbSolution.SelectedItem != null)
            {
                solutionsObj = cmbSolution.SelectedItem as T_OA_TRAVELSOLUTIONS;
                solutionId = solutionsObj.TRAVELSOLUTIONSID;
            }
            int pageCount = 0;
            allowanceList = new ObservableCollection<T_OA_AREAALLOWANCE>();
            List<T_OA_AREAALLOWANCE> temp = new List<T_OA_AREAALLOWANCE>();
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            if (PostLevelDicts != null)
            {
                if (areaAllowance != null)
                {
                    var ents = from c in PostLevelDicts
                               join b in areaAllowance on c.DICTIONARYVALUE.ToString() equals b.POSTLEVEL into d
                               from b in d.DefaultIfEmpty()
                               //where b.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == solutionId

                               select new T_OA_AREAALLOWANCE
                               {
                                   //ALLOWANCE = b == null ? null : b.ALLOWANCE,
                                   ACCOMMODATION = b == null ? null : b.ACCOMMODATION,
                                   TRANSPORTATIONSUBSIDIES = b == null ? null : b.TRANSPORTATIONSUBSIDIES,
                                   MEALSUBSIDIES = b == null ? null : b.MEALSUBSIDIES,
                                   OVERSEASSUBSIDIES = b == null ? null : b.OVERSEASSUBSIDIES,
                                   POSTLEVEL = c.DICTIONARYNAME,
                                   AREAALLOWANCEID = b == null ? Guid.NewGuid().ToString() : b.AREAALLOWANCEID,
                                   CREATEUSERID = b == null ? null : b.CREATEUSERID
                               };
                    if (ents!=null && ents.Count() > 0)
                    {
                        temp = Pager(ents.AsQueryable(), 0, 100, ref pageCount);
                        DtGrid.ItemsSource = temp;
                    }
                    //dataPager.PageCount = pageCount;
                }
                else
                {
                    var ents = from c in PostLevelDicts
                               //join b in areaAllowance on c.DICTIONARYNAME equals b.POSTLEVEL into d
                               //from b in d.DefaultIfEmpty()

                               select new T_OA_AREAALLOWANCE
                               {
                                   //ALLOWANCE = null,
                                   POSTLEVEL = c.DICTIONARYNAME,
                                   AREAALLOWANCEID = Guid.NewGuid().ToString(),
                                   CREATEUSERID = null
                               };

                    temp = Pager(ents.AsQueryable(), 0, 100, ref pageCount);
                    DtGrid.ItemsSource = temp;

                }
                foreach (T_OA_AREAALLOWANCE tmp in temp)
                {
                    tmp.T_OA_AREADIFFERENCE = new T_OA_AREADIFFERENCE();
                    tmp.T_OA_AREADIFFERENCE.AREADIFFERENCEID = AreaID;
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


        
      
        #endregion
       
        #endregion

        # region 添加 修改 删除 查询

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadArea();
        }
        private void btnAreaEdit_Click(object sender, RoutedEventArgs e)
        {


        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ////Form.Salary.AreaForm form = new SMT.HRM.UI.Form.Salary.AreaForm(FormTypes.New, "");
            //EntityBrowser browser = new EntityBrowser(form);
            ////browser.MinHeight = 120;
            ////browser.MinWidth = 380;
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            this.cmbSolution.IsEnabled = true;//修改时启用选择方案cmbox
            if (DtGridArea.SelectedItems.Count > 0)
            {
                var item = (DtGridArea.SelectedItems[0] as T_OA_AREADIFFERENCE);
                if (Common.CurrentLoginUserInfo.UserPosts[0].CompanyID != item.OWNERCOMPANYID)
                {
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("PROMPT"), "不能修改其他公司的出差补贴");
                    return;
                }
                AreaForm form = new AreaForm(FormTypes.Edit, (DtGridArea.SelectedItems[0] as T_OA_AREADIFFERENCE).AREADIFFERENCEID, solutionsObj.TRAVELSOLUTIONSID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 120;
                browser.MinWidth = 380;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }

        }

        private void btnCityAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DtGridArea.SelectedItems.Count > 0)
            {
                //CityForm form = new CityForm(FormTypes.New, (DtGridArea.SelectedItems[0] as T_OA_AREADIFFERENCE).AREADIFFERENCEID, "");
                //EntityBrowser browser = new EntityBrowser(form);
                //browser.MinHeight = 150;
                //browser.MinWidth = 400;
                //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTEAERA"));
            }

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            client.AreaAllowanceAsync(allowanceList, solutionsObj.TRAVELSOLUTIONSID);

        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindAreaAllowanceData();
        }

        private void GridPagerArea_Click(object sender, RoutedEventArgs e)
        {
            LoadArea();
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
            SetRowLogo(DtGrid, e.Row, "T_OA_AREAALLOWANCE");
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

        void browser_ReloadDataEvent()
        {
            LoadArea();
        }


        #endregion

        #region 页面事件
        //选择出差方案
        private void cmbSolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            solutionsObj = cmbSolution.SelectedItem as T_OA_TRAVELSOLUTIONS;
            if (solutionsObj != null)
            {
                //获取城市分类
                LoadArea();
               // client.GetQueryProgramSubsidiesAsync(solutionsObj.TRAVELSOLUTIONSID);
            }
        }
        //选择城市分类
        private void DtGridArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DtGridArea.SelectedItems.Count > 0)
            {
                currentArea = DtGridArea.SelectedItems[0] as T_OA_AREADIFFERENCE;
                AreaID = currentArea.AREADIFFERENCEID;
                this.RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetAreaAllowanceByAreaIDAsync(AreaID, solutionsObj.TRAVELSOLUTIONSID);
            }
        }

        //grid编辑出差补贴
        private void btEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                var item = (DtGrid.SelectedItems[0] as T_OA_AREAALLOWANCE);
                if (Common.CurrentLoginUserInfo.UserPosts[0].CompanyID != solutionsObj.OWNERCOMPANYID)
                {
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("PROMPT"), "不能修改其他公司的出差补贴");
                    return;
                }
                AreaAllowanceForm form = new AreaAllowanceForm(FormTypes.Edit, (DtGrid.SelectedItems[0] as T_OA_AREAALLOWANCE).AREAALLOWANCEID, AreaID, solutionsObj.TRAVELSOLUTIONSID, (DtGrid.SelectedItems[0] as T_OA_AREAALLOWANCE).POSTLEVEL);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 280;
                browser.MinWidth = 410;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }
        #endregion

        #region IClient
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
    }
}
