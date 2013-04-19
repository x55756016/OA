using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Windows.Browser;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.OrganManagement
{
    public partial class FrmOrganRegister : BasePage,IClient
    {
        private SmtOADocumentAdminClient client;
        private ObservableCollection<string> organDelID ;
        private string checkState = ((int)CheckStates.ALL).ToString();
        
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_ORGANIZATION organizationtable;

        public T_OA_ORGANIZATION Organizationtable
        {
            get { return organizationtable; }
            set { organizationtable = value; }
        }
        //public ObservableCollection<string> OrganID
        //{
        //    get { return organDelID; }
        //    set { organDelID = value; }
        //}

        
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #region 初始化
        public FrmOrganRegister()
        {            
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                PARENT.Children.Add(loadbar);
                InitEvent();
            };
        }

        private void InitEvent()
        {
           
            client = new SmtOADocumentAdminClient();
            client.GetOrganListCompleted += new EventHandler<GetOrganListCompletedEventArgs>(client_GetOrganListCompleted);
            client.DeleteOrganCompleted += new EventHandler<DeleteOrganCompletedEventArgs>(client_DeleteOrganCompleted);
            //SetButtonVisible();
            //LoadData();
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            //ToolBar.btnRead.Click += new RoutedEventHandler(btnRead_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            //CheckStateCBX.SetCheckStateCBX(ToolBar.cbxCheckState);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "5");
            
            //SetButtonVisible();
            //SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(int.Parse(checkState), ToolBar, "T_OA_ORGANIZATION");
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_ORGANIZATION", true);
            this.Loaded += new RoutedEventHandler(FrmOrganRegister_Loaded);
            //dgOrgan.CurrentCellChanged += new EventHandler<EventArgs>(dgOrgan_CurrentCellChanged);
            dgOrgan.SelectionChanged += new SelectionChangedEventHandler(dgOrgan_SelectionChanged);
            ToolBar.ShowRect();
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Organizationtable != null)
            {
                if (Organizationtable.CHECKSTATE == "2" || Organizationtable.CHECKSTATE == "3")
                {
                    //OrganAddForm orgFrm = new OrganAddForm(FormTypes.Resubmit, Organizationtable.ORGANIZATIONID, Organizationtable.CHECKSTATE);
                    OrganAddForm orgFrm = new OrganAddForm(FormTypes.Resubmit, Organizationtable.ORGANIZATIONID, Organizationtable.CHECKSTATE);
                    EntityBrowser browser = new EntityBrowser(orgFrm);
                    browser.FormType = FormTypes.Resubmit;
                    browser.MinHeight = 510;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ORGANREGISTERNOTEDIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void dgOrgan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
                return;
            if (grid.SelectedItems.Count > 0 )
            {
                Organizationtable = (T_OA_ORGANIZATION)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void dgOrgan_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Organizationtable = (T_OA_ORGANIZATION)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmOrganRegister_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_ORGANIZATION");
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_ORGANIZATION", true);
        }              

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值            
            if (!string.IsNullOrEmpty(txtOrgCode.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "ORGCODE ^@" + paras.Count().ToString();
                paras.Add(txtOrgCode.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtOrgName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "ORGNAME ^@" + paras.Count().ToString();
                paras.Add(txtOrgName.Text.Trim());
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;            
            loadbar.Start();
            client.GetOrganListAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
        }
        
        #endregion

        #region 事件完成
        private void client_DeleteOrganCompleted(object sender, DeleteOrganCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
            else
            {
                if (!e.Result) //返回值为假
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ORGANIZATIONREGISTERDELETEFAILED"));
                    return;
                }
                if (!e.FBControl)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ORGANIZATIONREGISTERMONEYFAILED"));
                    return;
                }
                if (e.Result)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ORGANIZATIONREGISTERDELETESUCCESSED"));
                }
                LoadData();
            }
        }       

        private void client_GetOrganListCompleted(object sender, GetOrganListCompletedEventArgs e)
        {
            try
            {
                
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
                        BindData(null,0);                        
                    }
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            loadbar.Stop();
        }       
        #endregion

        #region 绑定网格
        private void BindData(List<T_OA_ORGANIZATION> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                dgOrgan.ItemsSource = null;
                return;
            }
            dgOrgan.ItemsSource = obj;            
        }
        #endregion

        #region 按钮事件
        //新增
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            OrganAddForm orgFrm = new OrganAddForm(FormTypes.New, "", checkState);
            EntityBrowser browser = new EntityBrowser(orgFrm);
            browser.MinHeight = 510; 
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }       

        //修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Organizationtable != null)
            {
                if (Organizationtable.CHECKSTATE == "0" || Organizationtable.CHECKSTATE == "3")
                {
                    OrganAddForm orgFrm = new OrganAddForm(FormTypes.Edit, Organizationtable.ORGANIZATIONID, Organizationtable.CHECKSTATE);
                    EntityBrowser browser = new EntityBrowser(orgFrm);
                    browser.FormType = FormTypes.Edit;
                    browser.MinHeight = 510;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ORGANREGISTERNOTEDIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        //删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrgan.SelectedItems.Count > 0)
            {
                string Result = "";
                string errorMsg = "";
                organDelID = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    for (int i = 0; i < dgOrgan.SelectedItems.Count; i++)
                    {
                        string MeetingTypeID = "";
                        MeetingTypeID = (dgOrgan.SelectedItems[i] as T_OA_ORGANIZATION).ORGANIZATIONID;
                        if ((dgOrgan.SelectedItems[i] as T_OA_ORGANIZATION).CHECKSTATE == "0" || (dgOrgan.SelectedItems[i] as T_OA_ORGANIZATION).CHECKSTATE == "3")
                        {
                            if (!(organDelID.IndexOf(MeetingTypeID) > -1))
                            {
                                organDelID.Add(MeetingTypeID);
                            }                            
                        }                      
                    }
                    if (organDelID.Count >0 )
                    {
                        bool FBControl = true;
                        client.DeleteOrganAsync(organDelID, FBControl);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTITEMSNOTDELETE"));
                    }
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


            
        }

        

        //查看
        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            if (Organizationtable != null)
            {
                OrganAddForm orgFrm = new OrganAddForm(FormTypes.Browse, Organizationtable.ORGANIZATIONID, checkState);
                EntityBrowser browser = new EntityBrowser(orgFrm);
                browser.MinHeight = 510;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }

                        
        }

        //提交审核
        private void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {


            T_OA_ORGANIZATION organObj = null;
            if (dgOrgan.ItemsSource != null)
            {
                ObservableCollection<string> selectedObj = new ObservableCollection<string>();
                foreach (object obj in dgOrgan.ItemsSource)
                {
                    if (dgOrgan.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox ckbSelect = dgOrgan.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        if (ckbSelect.IsChecked == true)
                        {
                            organObj = ckbSelect.DataContext as T_OA_ORGANIZATION;                         
                            break;
                        }

                    }
                }
               
            }
            if (organObj != null)
            {
                HtmlPage.Window.Alert("请先选择需要提交审核的机构！");
            }
            else
            {
                //T_OA_ORGANIZATION organ = new T_OA_ORGANIZATION();
                //organ = organObj.organ;
                //organ.CHECKSTATE = "2";
                //organ.UPDATEUSERID = "User";
                //organ.UPDATEDATE = DateTime.Now;
                //Flow_FlowRecord_T entity = new Flow_FlowRecord_T();
                //entity.CreateCompanyID = "smt";
                //entity.Content = "dfwfw";
                //entity.CreateUserID = "admin"; //创建流程用户ID
                //entity.CreateDate = DateTime.Now;
                //entity.Flag = "0";
                ////entity.EditDate = DateTime.Now;
                //entity.EditUserID = "admin";
                //entity.FlowCode = "gefege";
                //entity.FormID = organ.ORGANIZATIONID;//保存的模块表ID
                //entity.GUID = Guid.NewGuid().ToString();
                //entity.InstanceID = "";
                //entity.ModelCode = "OrganApp";  //模块代码
                //entity.CreatePostID = "Manage"; //岗位ID
                //entity.ParentStateCode = "StartFlow"; //父状态代码
                //entity.StateCode = "StartFlow";  //状态代码
                //client.SubmitFlowAsync(organ, entity, "Admin");
            }
        }

        //审核
        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {

            if (Organizationtable != null)
            {
                //OrganAddForm orgFrm = new OrganAddForm(FormTypes.Audit, Organizationtable.ORGANIZATIONID, checkState);
                OrganAddForm orgFrm = new OrganAddForm(FormTypes.Audit, Organizationtable.ORGANIZATIONID);
                EntityBrowser browser = new EntityBrowser(orgFrm);
                browser.FormType = FormTypes.Audit;
                browser.MinHeight = 510;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


            
        }

        //查看
        private void BtnView_Click(object sender, RoutedEventArgs e)
        {

            if (Organizationtable != null)
            {
                OrganAddForm orgFrm = new OrganAddForm(FormTypes.Browse, Organizationtable.ORGANIZATIONID, checkState);
                EntityBrowser browser = new EntityBrowser(orgFrm);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 510;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


            //ObservableCollection<string> organID = GridHelper.GetSelectItem(dgOrgan, "myChkBox", Action.Edit);
            //if (organID != null && organID.Count > 0)
            //{
            //    OrganAddForm orgFrm = new OrganAddForm(Action.Read, organID[0], checkState);
            //    EntityBrowser browser = new EntityBrowser(orgFrm);
            //    browser.MinHeight = 430;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            //}
            //else
            //{
            //    //HtmlPage.Window.Alert("请先选择需要查看的机构！");

            //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "VIEW"));

            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
        }

        //刷新
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }     

        //模板列取消选中
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (!chkbox.IsChecked.Value)
            //{
            //    organObj = chkbox.DataContext as V_OrganRegister;
            //    organDelID.Remove(organObj.organ.ORGANIZATIONID);
            //    organObj = null;
            //}
            GridHelper.SetUnCheckAll(dgOrgan);
        }

        //模板列选中
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (chkbox.IsChecked.Value)
            //{
            //    organObj = chkbox.DataContext as V_OrganRegister;
            //    organDelID.Add(organObj.organ.ORGANIZATIONID);
            //}
        }

        //查询
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {  
            LoadData();
        }

        //刷新
        private void browser_ReloadDataEvent()
        {
            Organizationtable = null;
            LoadData();
        }

        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_ORGANIZATION");
                checkState = dict.DICTIONARYVALUE.ToString();                
                LoadData();
            }  
            
        }
       
        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //草稿
                    ToolBar.btnNew.Visibility = Visibility.Visible;         //新增
                    ToolBar.btnEdit.Visibility = Visibility.Visible;        //修改
                    ToolBar.btnDelete.Visibility = Visibility.Visible;      //删除
                    //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
                    ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
                  //  ToolBar.btnSumbitAudit.Visibility = Visibility.Visible; //提交审核
                    break;
                case "2":  //审批通过
                    ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
                    ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
                    ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
                    //ToolBar.btnRead.Visibility = Visibility.Visible;          //查看
                    ToolBar.btnAudit.Visibility = Visibility.Collapsed;       //审核
                     //提交审核
                    break;
                case "1":  //审批中   审核人身份
                    ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
                    ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
                    ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
                    //ToolBar.btnRead.Visibility = Visibility.Collapsed;        //查看
                    ToolBar.btnAudit.Visibility = Visibility.Collapsed;         //审核
                     //提交审核
                    break;
                case "3":  //审批未通过
                    ToolBar.btnNew.Visibility = Visibility.Visible;         //新增
                    ToolBar.btnEdit.Visibility = Visibility.Visible;        //修改
                    ToolBar.btnDelete.Visibility = Visibility.Visible;      //删除
                    //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
                    ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
                   // ToolBar.btnSumbitAudit.Visibility = Visibility.Visible; //提交审核
                    break;
                case "4":  //审批中   审核人身份
                    ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
                    ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
                    ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
                    //ToolBar.btnRead.Visibility = Visibility.Collapsed;        //查看
                    ToolBar.btnAudit.Visibility = Visibility.Visible;         //审核
                     //提交审核
                    break;
            }
        }

        //private void orgFrm_ReloadDataEvent()
        //{
        //    LoadData();
        //}

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GridHelper.SetUnCheckAll(dgOrgan);
            LoadData();
        }

        #endregion

        private void dgOrgan_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgOrgan, e.Row, "T_OA_ORGANIZATION");
        }

        #region IClient 成员

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
