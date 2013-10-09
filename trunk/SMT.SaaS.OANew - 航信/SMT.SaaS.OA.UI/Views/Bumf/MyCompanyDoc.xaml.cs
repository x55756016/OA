using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Data;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;
namespace SMT.SaaS.OA.UI.Views.Bumf
{
    public partial class MyCompanyDoc : BasePage,IClient
    {

        #region 加载数据
                
        SmtOACommonOfficeClient SendDocClient = new SmtOACommonOfficeClient();
        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        
        private string StrOperationFlag = ""; //用来控制 归档 和 文档发布
        T_OA_SENDDOC tmpSendDoc = new T_OA_SENDDOC();
        string StrIp = "";
        private T_OA_SENDDOCTYPE SelectDocType = new T_OA_SENDDOCTYPE();
        private bool auditflag = false; //提交审核标志
        private bool IsQueryBtn = false; //是否是查询按钮提交
        CheckBox SelectBox = new CheckBox();        
        private SMTLoading loadbar = new SMTLoading();

        private V_BumfCompanySendDoc companysenddoc;

        public V_BumfCompanySendDoc Companysenddoc
        {
            get { return companysenddoc; }
            set { companysenddoc = value; }
        }

        public MyCompanyDoc()
        {
            //Common.Userlogin(Common.CurrentLoginUserInfo);
            InitializeComponent();            
            Utility.DisplayGridToolBarButton(ToolBar, "OAMYCOMPANYDOC", true);
            PARENT.Children.Add(loadbar);
            InitEvent();
           
        }

        private void InitEvent()
        {
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar.stpOtherAction.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            
            ToolBar.BtnView.Click += new RoutedEventHandler(SendDocDetailBtn_Click);
            
            ToolBar.stpOtherAction.Visibility = Visibility.Collapsed;
            SendDocClient.GetMYSendDocInfosListCompleted += new EventHandler<GetMYSendDocInfosListCompletedEventArgs>(SendDocClient_GetMYSendDocInfosListCompleted);            
            SendDocClient.GetDocTypeInfosCompleted += new EventHandler<GetDocTypeInfosCompletedEventArgs>(SendDocClient_GetDocTypeInfosCompleted);
            LoadSendDocInfos();
            //DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
            DaGr.SelectionChanged += new SelectionChangedEventHandler(DaGr_SelectionChanged);
            this.Loaded += new RoutedEventHandler(MyCompanyDoc_Loaded);
            InitComboxSource();
            ToolBar.ShowRect();
        }

        void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
            {
                return;
            }
            if (grid.SelectedItems.Count > 0)
            {
                Companysenddoc = (V_BumfCompanySendDoc)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void MyCompanyDoc_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_SENDDOC");
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Companysenddoc = (V_BumfCompanySendDoc)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void SendDocClient_GetMYSendDocInfosListCompleted(object sender, GetMYSendDocInfosListCompletedEventArgs e)
        {
            loadbar.Stop();
            try
            {
                if (e.Result != null)
                {
                    SelectBox = Utility.FindChildControl<CheckBox>(DaGr, "SelectAll");
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                }
                
            }
            catch (Exception ex)
            {

                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSendDocInfos();
        }
        #region Combox 填充

        private void InitComboxSource()
        {
            //this.cbxGrade.SelectedIndex = 0;
            Combox_ItemSourceDocType();
            //this.cbxProritity.SelectedIndex = 0;
        }
        #endregion

        #region 填充级别信息
        private void Combox_ItemSourceDocType()
        {
            SendDocClient.GetDocTypeInfosAsync();

        }
        void SendDocClient_GetDocTypeInfosCompleted(object sender, GetDocTypeInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    this.cbxdoctype.Items.Clear();
                    T_OA_SENDDOCTYPE doctype = new T_OA_SENDDOCTYPE();
                    doctype.SENDDOCTYPE = "请选择";
                    e.Result.Insert(0, doctype);

                    this.cbxdoctype.ItemsSource = e.Result;
                    this.cbxdoctype.DisplayMemberPath = "SENDDOCTYPE";
                    this.cbxdoctype.SelectedIndex = 0;

                }
            }
        }

        #endregion
 
        void LoadSendDocInfos()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            string StrTitle = ""; //标题
            
            string StrStart = "";//添加文档的起始时间
            string StrEnd = "";//添加文档的结束时间
            string StrIsSave = "";//是否归档
            string StrDistrbute = "";//是否发布
            string StrGrade = "";//级别
            string StrProritity = "";//缓急
            string StrDocType = "";//文档类型
            bool IsNull = false;  //用来控制是否有查询条件
            
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            //if (checkState != ((int)CheckStates.Approving).ToString())
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "OACompanySendDoc.OWNERID==@" + paras.Count().ToString();
            //    paras.Add(Common.CurrentLoginUserInfo.EmployeeID);
            //}
            if (IsQueryBtn)
            {
                StrTitle = this.txtSendDocTitle.Text.Trim().ToString();
                T_SYS_DICTIONARY GradeObj = cbxGrade.SelectedItem as T_SYS_DICTIONARY;//级别
                T_SYS_DICTIONARY ProritityObj = cbxProritity.SelectedItem as T_SYS_DICTIONARY;//缓急
                T_OA_SENDDOCTYPE DocType = new T_OA_SENDDOCTYPE();
                StrStart = this.dpStart.Text.Trim().ToString();
                StrEnd = this.dpEnd.Text.Trim().ToString();
                
                if (this.cbxdoctype.SelectedIndex >0)
                {
                    StrDocType = this.cbxdoctype.SelectedItem.ToString();
                    DocType = this.cbxdoctype.SelectedItem as T_OA_SENDDOCTYPE;
                    
                }

                if (this.cbxGrade.SelectedIndex>0)
                {
                    StrGrade = GradeObj.DICTIONARYNAME.ToString();
                }
                if (this.cbxProritity.SelectedIndex > 0)
                {
                    StrProritity = ProritityObj.DICTIONARYNAME.ToString();
                }
                if (!string.IsNullOrEmpty(StrTitle))
                {
                    IsNull = true;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OACompanySendDoc.SENDDOCTITLE ^@" + paras.Count().ToString();//标题名称
                    paras.Add(StrTitle);
                }
                
                if (!string.IsNullOrEmpty(StrProritity))
                {
                    IsNull = true;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OACompanySendDoc.PRIORITIES ==@" + paras.Count().ToString();//缓急名称
                    paras.Add(StrProritity);
                }
                if (!string.IsNullOrEmpty(StrDocType))
                {
                    IsNull = true;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "doctype.SENDDOCTYPEID ==@" + paras.Count().ToString();//类型名称
                    paras.Add(DocType.SENDDOCTYPEID);
                }

                if (!string.IsNullOrEmpty(StrGrade))
                {
                    IsNull = true;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OACompanySendDoc.GRADED ==@" + paras.Count().ToString();//级别名称
                    paras.Add(StrGrade);
                }
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("dtSearch"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    return;
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("dtSearch"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                    return;
                }
                if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    DtStart = System.Convert.ToDateTime(StrStart);
                    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                    if (DtStart > DtEnd)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("dtSearch"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                        return;
                    }
                    else
                    {
                        IsNull = true;
                    }
                }
            }

            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loginUserInfo.postID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            loginUserInfo.departmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            loadbar.Start();            
            //SendDocClient.GetSendDocInfosListByWorkFlowAsync(dataPager.PageIndex, dataPager.PageSize, "OACompanySendDoc.CREATEDATE", filter, paras, pageCount, checkState, loginUserInfo);
            SendDocClient.GetMYSendDocInfosListAsync(dataPager.PageIndex, dataPager.PageSize, "OACompanySendDoc.CREATEDATE", filter, paras, pageCount, "2", loginUserInfo);
            
        }
        
        #region  绑定DataGird
        private void BindDataGrid(List<V_BumfCompanySendDoc> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {                
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
            this.Loaded += new RoutedEventHandler(MyCompanyDoc_Loaded);
        }
        #endregion



        //归档文档信息
        void SendDocClient_GetSavedSendDocInfosCompleted(object sender, GetSavedSendDocInfosCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<T_OA_SENDDOC> infos = new List<T_OA_SENDDOC>(e.Result);
                //dpGrid.PageSize = 10;
                PagedCollectionView pager = new PagedCollectionView(infos);
                DaGr.ItemsSource = pager;
                //DaGr.ItemsSource = infos;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
        }
        //已发布文档信息
        void SendDocClient_GetDistrbutedSendDocInfosCompleted(object sender, GetDistrbutedSendDocInfosCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<T_OA_SENDDOC> infos = new List<T_OA_SENDDOC>(e.Result);
                //dpGrid.PageSize = 10;
                PagedCollectionView pager = new PagedCollectionView(infos);
                DaGr.ItemsSource = pager;
                //DaGr.ItemsSource = infos;
            }
            else
            {
                //MessageBox.Show("暂无数据");
                Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("CAUTION"),Utility.GetResourceStr("NODATA"));
                return;
            }
        }

        

        void SendDocClient_GetSendDocInfosCompleted(object sender, GetSendDocInfosCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<T_OA_SENDDOC> infos = new List<T_OA_SENDDOC>(e.Result);
                //dpGrid.PageSize = 3;
                PagedCollectionView pager = new PagedCollectionView(infos);
                DaGr.ItemsSource = pager;                
            }
        }
        #endregion
        
        #region 页面导航时代码
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

        #region 全选事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        #region DaGr_loadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_SENDDOC");    
        }
        #endregion


        #region 删除按钮事件

        void AddWin_ReloadDataEvent()
        {
            LoadSendDocInfos();
        }
        

        #endregion

        #region 查询按钮
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQueryBtn = true;
            LoadSendDocInfos();
        }
        #endregion
        
        #region 公司发文详情
        private void SendDocDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Companysenddoc != null)
            {
                //MySendDocForm DetailWin = new MySendDocForm(Companysenddoc);
                CompanyDocWebPart AddWin = new CompanyDocWebPart(Companysenddoc.OACompanySendDoc.SENDDOCID);
                System.Windows.Controls.Window wd = new System.Windows.Controls.Window();
                wd.MinWidth = 1050;
                wd.MinHeight = 500;
                wd.Content = AddWin;
                wd.TitleContent = "查看公文"+ Companysenddoc.OACompanySendDoc.SENDDOCTITLE +"信息";
                wd.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true, false, Companysenddoc.OACompanySendDoc.SENDDOCID);

                //CompanyDocWebPart DetailWin = new CompanyDocWebPart(Companysenddoc.OACompanySendDoc.SENDDOCID);
                //EntityBrowser browser = new EntityBrowser(DetailWin);
                //browser.FormType = FormTypes.Browse;
                //browser.MinWidth = 850;
                //browser.MinHeight = 520;
                //browser.Content = "查看公文信息";
                //browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            //V_BumfCompanySendDoc SendDocInfoT = new V_BumfCompanySendDoc();

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                SendDocInfoT = cb1.Tag as V_BumfCompanySendDoc;
            //                break;
            //            }
            //        }
            //    }

            //}

            
            //if (SendDocInfoT.OACompanySendDoc != null)
            //{
            //    CompanyDocForm AddWin = new CompanyDocForm(FormTypes.Edit, SendDocInfoT);
            //    SendDocInfoForm DetailWin = new SendDocInfoForm(SendDocInfoT);
            //    EntityBrowser browser = new EntityBrowser(DetailWin);
            //    browser.Width = 500;
            //    browser.Height = 520;                
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            //}
            //else
            //{
            //    //MessageBox.Show("请选择需要修改的公文类型");
            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTWARNING", "VIEW"));
            //    return;
            //}
            

        }
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadSendDocInfos();
        }

        #region IClient 成员

        public void ClosedWCFClient()
        {
            SendDocClient.DoClose();
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

