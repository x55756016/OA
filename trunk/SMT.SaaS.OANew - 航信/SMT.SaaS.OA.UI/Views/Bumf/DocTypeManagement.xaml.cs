using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
//using SMT.SaaS.OA.UI.BumfManagementWS;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.Bumf
{
    public partial class DocTypeManagement : BasePage,IClient
    {

        #region 页面初始化
        //BumfManagementServiceClient BumfClient = new BumfManagementServiceClient();
        SmtOACommonOfficeClient BumfClient = new SmtOACommonOfficeClient();
        private ObservableCollection<string> DelInfosList ;
        CheckBox SelectBox = new CheckBox();
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_SENDDOCTYPE senddoctype;
        DateTime start = new DateTime();
        DateTime end = new DateTime();
        public T_OA_SENDDOCTYPE Senddoctype
        {
            get { return senddoctype; }
            set { senddoctype = value; }
        }
        public DocTypeManagement()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButtonUI(ToolBar, "OABUMFDOCTYPE", true);
            PARENT.Children.Add(loadbar);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar.stpOtherAction.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            
            BumfClient.GetDocTypeInfosListBySearchCompleted += new EventHandler<GetDocTypeInfosListBySearchCompletedEventArgs>(BumfClient_GetDocTypeInfosListBySearchCompleted);
            BumfClient.DocTypeBatchDelCompleted +=new EventHandler<DocTypeBatchDelCompletedEventArgs>(BumfClient_DocTypeBatchDelCompleted);
            LoadDocTypeInfos();
            this.Loaded += new RoutedEventHandler(DocTypeManagement_Loaded);
            ToolBar.ShowRect();
            //DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Senddoctype = (T_OA_SENDDOCTYPE)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void DocTypeManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_SENDDOCTYPE");
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Senddoctype = null;//刷新将选择对象置空
            LoadDocTypeInfos();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Senddoctype != null)
            {
                DocTypeForm AddWin = new DocTypeForm(Action.Read, Senddoctype);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 400;
                browser.MinWidth = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            //T_OA_SENDDOCTYPE DocTypeInfoT = new T_OA_SENDDOCTYPE();

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {                            
            //                DocTypeInfoT = cb1.Tag as T_OA_SENDDOCTYPE;
            //                break;
            //            }
            //        }
            //    }

            //}

            
            //if (!string.IsNullOrEmpty(DocTypeInfoT.SENDDOCTYPEID))
            //{   
            //    DocTypeForm AddWin = new DocTypeForm(Action.Read, DocTypeInfoT);
            //    EntityBrowser browser = new EntityBrowser(AddWin);
            //    browser.MinHeight = 300;
            //    browser.MinWidth = 320;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            //}
            //else
            //{
            //    //MessageBox.Show("请选择需要修改的公文类型");
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //    return;
            //}
        }

        
        void LoadDocTypeInfos()
        { 
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            string StrName = "";
            string StrRecord = "";
            string StrStart = "";
            string StrEnd = "";
            string StrTypeFlag = ""; //文档类型
            StrStart = dpStart.Text.ToString();
            StrEnd = dpEnd.Text.ToString();
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            StrRecord = this.cbxIsSave.SelectedIndex.ToString();
            StrName = this.txtDocType.Text.Trim().ToString();
            switch (this.cbxIsSave.SelectedIndex)
            { 
                case 0:
                    break;
                case 1:
                    StrTypeFlag ="1";
                    break;
                case 2:
                    StrTypeFlag ="0";
                    break;
            }
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            
            if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
            {
                
                //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ENDTIMENOTNULL"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {

                //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("MEETINGSTARTTIMENOTNULL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTNULL"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                if (DtStart > DtEnd)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "CREATEDATE >=@" + paras.Count().ToString();//开始时间
                    paras.Add(DtStart);
                    filter += " and ";
                    filter += "CREATEDATE <=@" + paras.Count().ToString();//结束时间
                    paras.Add(DtEnd);
                }
            }
            //if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            //{
            //    DtStart = System.Convert.ToDateTime(StrStart);
            //    DtEnd = System.Convert.ToDateTime(StrEnd);
            //    if (DtStart > DtEnd)
            //    {
            //        //MessageBox.Show("开始时间不能大于结束时间");
            //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("MEETINGSTARTTIMENOTNULL"));
            //        return;
            //    }
            //    else
            //    {
                    
            //    }
            //}
            
            if (!string.IsNullOrEmpty(StrName))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "SENDDOCTYPE ^@" + paras.Count().ToString();//类型名称
                paras.Add(StrName);
            }
            if(!string.IsNullOrEmpty(StrTypeFlag))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "OPTFLAG ==@" + paras.Count().ToString();//类型名称
                paras.Add(StrTypeFlag);

            }

            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loadbar.Start();
            //client.GetLendingListByUserIdAsync(dataPager.PageIndex, dataPager.PageSize, "archivesLending.CREATEDATE", filter, paras, pageCount, checkState, loginUserInfo);
            start = DateTime.Now;
            BumfClient.GetDocTypeInfosListBySearchAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);
        }
        void BumfClient_GetDocTypeInfosListBySearchCompleted(object sender,GetDocTypeInfosListBySearchCompletedEventArgs e)
        {
            loadbar.Stop();
            try
            {
                if (e.Result != null)
                {
                    //SelectBox = Utility.FindChildControl<CheckBox>(DaGr, "SelectAll");
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                }
                //end = DateTime.Now;
                //TimeSpan ts = end.Subtract(start);

                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"),ts.Seconds.ToString()+"M"+ ts.Milliseconds.ToString());
                
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        #region  绑定DataGird
        private void BindDataGrid(List<T_OA_SENDDOCTYPE> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }
        #endregion
        void BumfClient_GetDocTypeInfosCompleted(object sender, GetDocTypeInfosCompletedEventArgs e)
        {
            //if (e.Result != null)
            //{
            //    SelectBox = Utility.FindChildControl<CheckBox>(DaGr, "SelectAll");
            //    List<T_OA_SENDDOCTYPE> infos = new List<T_OA_SENDDOCTYPE>(e.Result);
            //    dpGrid.PageSize = 20;
            //    PagedCollectionView pager = new PagedCollectionView(infos);
            //    DaGr.ItemsSource = pager;
            //    //DaGr.ItemsSource = infos;
            //}
        }
        #endregion

        #region 导航

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #endregion

        #region 模板中checkbox单击事件
        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.IsChecked == false)
            {                
                if (SelectBox != null)
                {
                    if (SelectBox.IsChecked == true)
                    {
                        SelectBox.IsChecked = false;
                    }
                }
            }
        }
        #endregion

        #region DaGr_loadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_SENDDOCTYPE");
        }
        #endregion
       
        #region 删除按钮事件
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (DaGr.SelectedItems.Count > 0)
            {

                string Result = "";
                DelInfosList = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    string StrTip = "";
                    for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                    {
                        T_OA_SENDDOCTYPE doctype = new T_OA_SENDDOCTYPE();
                        doctype =DaGr.SelectedItems[i] as T_OA_SENDDOCTYPE;
                        if (!(SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(doctype, "OABUMFDOCTYPE", OperationType.Delete, Common.CurrentLoginUserInfo.EmployeeID)))
                        {
                            StrTip = "您不能删除您选中的第" + (i + 1).ToString() + "条，标题为" + doctype.SENDDOCTYPE + "的信息";
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), StrTip, Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                            return;
                        }
                        string DocTypeID = "";
                        DocTypeID = (DaGr.SelectedItems[i] as T_OA_SENDDOCTYPE).SENDDOCTYPEID;
                        if (!(DelInfosList.IndexOf(DocTypeID) > -1))
                        {
                            DelInfosList.Add(DocTypeID);
                        }
                    }      
                    if(DelInfosList.Count >0)
                        BumfClient.DocTypeBatchDelAsync(DelInfosList);
                    else
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "删除记录不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            } 
        }

        void BumfClient_DocTypeBatchDelCompleted(object sender,DocTypeBatchDelCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                DelInfosList.Clear();//清空列表
                if (e.Result == "")
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "DOCUMENTTYPE"));
                    LoadDocTypeInfos();
                }
                else
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(e.Result.ToString()));
                    return;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", e.Error.ToString()));
                return;
            }
        }

        #endregion

        #region 修改按钮事件
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Senddoctype != null)
            {
                if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Senddoctype, "OABUMFDOCTYPE", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
                {
                    DocTypeForm AddWin = new DocTypeForm(Action.Edit, Senddoctype);
                    EntityBrowser browser = new EntityBrowser(AddWin);
                    browser.MinHeight = 400;
                    browser.MinWidth = 600;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {


                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            //T_OA_SENDDOCTYPE DocTypeInfoT = new T_OA_SENDDOCTYPE();

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {                            
            //                DocTypeInfoT = cb1.Tag as T_OA_SENDDOCTYPE;
            //                break;
            //            }
            //        }
            //    }

            //}

            
            //if (!string.IsNullOrEmpty(DocTypeInfoT.SENDDOCTYPEID))
            //{   
            //    DocTypeForm AddWin = new DocTypeForm(Action.Edit, DocTypeInfoT);
            //    EntityBrowser browser = new EntityBrowser(AddWin);
            //    browser.MinHeight = 300;
            //    browser.MinWidth = 320;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            //}
            //else
            //{
            //    //MessageBox.Show("请选择需要修改的公文类型");
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //    return;
            //}

        }

        #endregion

        #region 添加按钮事件
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            T_OA_SENDDOCTYPE doctype = new T_OA_SENDDOCTYPE();
            DocTypeForm AddWin = new DocTypeForm(Action.Add, doctype);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinHeight = 240;
            browser.MinWidth = 600;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        

        void AddWin_ReloadDataEvent()
        {
            LoadDocTypeInfos();
        }
        #endregion

        #region 查询按钮
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadDocTypeInfos();            
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadDocTypeInfos();
        }

        private void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
            {
                return;
            }
            if (grid.SelectedItems.Count > 0)
            {
                Senddoctype = (T_OA_SENDDOCTYPE)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        //void BumfClient_GetDocTypeInfosListBySearchCompleted(object sender, GetDocTypeInfosListBySearchCompletedEventArgs e)
        //{
        //    //if (e.Result != null)
        //    //{
        //    //    List<T_OA_SENDDOCTYPE> infos = new List<T_OA_SENDDOCTYPE>(e.Result);
        //    //    dpGrid.PageSize = 20;
        //    //    PagedCollectionView pager = new PagedCollectionView(infos);
        //    //    DaGr.ItemsSource = pager;
        //    //    //DaGr.ItemsSource = infos;
        //    //}
        //    //else
        //    //{
        //    //    DaGr.ItemsSource = null;
        //    //    MessageBox.Show("对不起，没找到您需要的数据");
        //    //    return;
        //    //}
        //}

        #endregion


        #region IClient 成员

        public void ClosedWCFClient()
        {
            BumfClient.DoClose();
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
