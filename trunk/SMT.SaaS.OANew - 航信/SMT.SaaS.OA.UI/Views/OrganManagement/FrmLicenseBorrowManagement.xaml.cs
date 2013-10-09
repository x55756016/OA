using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
//using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.UserControls;

namespace SMT.SaaS.OA.UI.Views.OrganManagement
{
    public partial class FrmLicenseBorrowManagement : BasePage,IClient
    {
        private SmtOADocumentAdminClient client;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_LICENSEUSER licenseuser;

        public T_OA_LICENSEUSER Licenseuser
        {
            get { return licenseuser; }
            set { licenseuser = value; }
        }
        //private V_LicenseBorrow licenseObj;
       
        //public V_LicenseBorrow LicenseObj
        //{
        //    get { return licenseObj; }
        //    set 
        //    {
        //        this.DataContext = value;
        //        licenseObj = value; 
        //    }
        //}
        private ObservableCollection<string> licenseDelID ;

        #region 初始化
        public FrmLicenseBorrowManagement()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                PARENT.Children.Add(loadbar);
                InitEvent();
            };
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }


        private void InitEvent()
        {
            //licenseObj = new V_LicenseBorrow();
            client = new SmtOADocumentAdminClient();
            client.GetLicenseBorrowListCompleted += new EventHandler<GetLicenseBorrowListCompletedEventArgs>(client_GetLicenseBorrowListCompleted);
            client.DeleteLicenseBorrowCompleted += new EventHandler<DeleteLicenseBorrowCompletedEventArgs>(client_DeleteLicenseBorrowCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);            
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "5");
            //SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(int.Parse(checkState), ToolBar, "T_OA_LICENSEUSER");
            //ToolBar.btnRead.Visibility = Visibility.Collapsed;  
            //LoadData();
            this.Loaded += new RoutedEventHandler(FrmLicenseBorrowManagement_Loaded);
            //this.dgLicense.CurrentCellChanged += new EventHandler<EventArgs>(dgLicense_CurrentCellChanged);
            dgLicense.SelectionChanged += new SelectionChangedEventHandler(dgLicense_SelectionChanged);
            ToolBar.ShowRect();
            //SetButtonVisible();       
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Licenseuser != null)
            {
                if (Licenseuser.CHECKSTATE == "0" || Licenseuser.CHECKSTATE == "3")
                {
                    LicenseBorrowForm form = new LicenseBorrowForm(Action.ReSubmit, Licenseuser.LICENSEUSERID, checkState);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Resubmit;
                    browser.HideLeftMenu();
                    browser.MinWidth = 450;
                    browser.MinHeight = 370;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("FRMLICENSEBORROWMANAGEMENTNOTEDIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void dgLicense_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
                return;
            if (grid.SelectedItems.Count > 0 )
            {
                Licenseuser = (T_OA_LICENSEUSER)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Licenseuser != null)
            {
                LicenseBorrowForm form = new LicenseBorrowForm(Action.Read, Licenseuser.LICENSEUSERID, checkState);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.HideLeftMenu();
                browser.MinWidth = 450;
                browser.MinHeight = 370;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void dgLicense_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Licenseuser = (T_OA_LICENSEUSER)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmLicenseBorrowManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_LICENSEUSER");
            Utility.DisplayGridToolBarButton(ToolBar, "OAORGANLICENUSER", true);
        }

        
        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值 
            //filter += "licenseUser.CREATEUSERID==@" + paras.Count().ToString();
            //paras.Add(Common.CurrentLoginUserInfo.EmployeeID);
            if (!string.IsNullOrEmpty(txtLicenseName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "T_OA_LICENSEMASTER.LICENSENAME ^@" + paras.Count().ToString();
                paras.Add(txtLicenseName.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtContent.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CONTENT ^@" + paras.Count().ToString();
                paras.Add(txtContent.Text.Trim());
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loadbar.Start();
            client.GetLicenseBorrowListAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
        }

        #endregion

        #region 完成事件
        private void client_DeleteLicenseBorrowCompleted(object sender, DeleteLicenseBorrowCompletedEventArgs e)
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
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "ARCHIVELENDING"));
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_GetLicenseBorrowListCompleted(object sender, GetLicenseBorrowListCompletedEventArgs e)
        {
            loadbar.Stop();
            try
            {
                if (e.Error == null)
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
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch(Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 绑定DataGird
        private void BindData(List<T_OA_LICENSEUSER> obj,int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                dgLicense.ItemsSource = null;
                return;
            }
            dgLicense.ItemsSource = obj;            
        }
        #endregion
     
        #region 按钮事件
        
          
        //新增
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //CFrmLicenseBorrowAdd addFrm = new CFrmLicenseBorrowAdd(Action.Add, null, checkState);         
            ////addFrm = new CFrmLendingAdd(Action.Add, "");
            //addFrm.Title = Utility.GetResourceStr("ADDTITLE", "LICENSELENDING");
            ////addFrm.Title = "新增外借记录";
            //addFrm.Show();
            //addFrm.ReloadDataEvent += new BaseForm.refreshGridView(addFrm_ReloadDataEvent);

            LicenseBorrowForm form = new LicenseBorrowForm(Action.Add,"",checkState);
            EntityBrowser browser = new EntityBrowser(form);
            browser.HideLeftMenu();
            browser.MinWidth = 450;
            browser.MinHeight = 370;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        private void browser_ReloadDataEvent()
        {
            Licenseuser = null;
            LoadData();
        }

        //修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Licenseuser != null)
            {
                if (Licenseuser.CHECKSTATE == "0" || Licenseuser.CHECKSTATE == "3")
                {
                    LicenseBorrowForm form = new LicenseBorrowForm(Action.Edit, Licenseuser.LICENSEUSERID, checkState);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Edit;
                    browser.HideLeftMenu();
                    browser.MinWidth = 450;
                    browser.MinHeight = 370;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("FRMLICENSEBORROWMANAGEMENTNOTEDIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


            //ObservableCollection<string> licenseID = GridHelper.GetSelectItem(dgLicense, "myChkBox", Action.Edit);
            //if (licenseID == null || licenseID.Count<1)
            //{
            //    //HtmlPage.Window.Alert("请先选择需要修改的外借记录！");

            //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));

            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{
            //    //CFrmLicenseBorrowAdd addFrm = new CFrmLicenseBorrowAdd(Action.Edit, licenseID[0],checkState);
            //    ////addFrm.Title = "修改外借记录";
            //    //addFrm.Title = Utility.GetResourceStr("EDITTITLE", "LICENSELENDING");
            //    //addFrm.Show();
            //    //addFrm.ReloadDataEvent += new BaseForm.refreshGridView(addFrm_ReloadDataEvent);

            //    LicenseBorrowForm form = new LicenseBorrowForm(Action.Edit, licenseID[0], checkState);
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.HideLeftMenu();
            //    browser.MinWidth = 450;
            //    browser.MinHeight = 370;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            //}
        }

        //删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgLicense.SelectedItems.Count > 0)
            {

                string Result = "";                
                licenseDelID = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    for (int i = 0; i < dgLicense.SelectedItems.Count; i++)
                    {
                        string LicenseuserID = "";
                        LicenseuserID = (dgLicense.SelectedItems[i] as T_OA_LICENSEUSER).LICENSEUSERID;
                        if ((dgLicense.SelectedItems[i] as T_OA_LICENSEUSER).CHECKSTATE == "0" || (dgLicense.SelectedItems[i] as T_OA_LICENSEUSER).CHECKSTATE == "3")
                        {
                            if (!(licenseDelID.IndexOf(LicenseuserID) > -1))
                            {
                                licenseDelID.Add(LicenseuserID);
                            }
                        }
                        
                    }
                    if (licenseDelID.Count > 0)
                    {
                        client.DeleteLicenseBorrowAsync(licenseDelID);
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


            //licenseDelID = GridHelper.GetSelectItem(dgLicense, "myChkBox", Action.Delete);
            //if (licenseDelID != null && licenseDelID.Count > 0)
            //{
            //    //if (HtmlPage.Window.Confirm("您确定要删除选中的外借记录？"))
            //    //{
            //    //    client.DeleteLicenseBorrowAsync(licenseDelID);
            //    //}

            //    //ComfirmBox deleComfirm = new ComfirmBox();
            //    //deleComfirm.Title = Utility.GetResourceStr("DELETECONFIRM");
            //    //deleComfirm.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
            //    //deleComfirm.ButtonOK.Click += new RoutedEventHandler(ButtonOK_Click);
            //    //deleComfirm.Show();

            //    string Result = "";
            //    ComfirmWindow com = new ComfirmWindow();
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {
            //        client.DeleteLicenseBorrowAsync(licenseDelID);
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            //}
            //else
            //{
            //    //HtmlPage.Window.Alert("请先选择需要删除的外借记录！");

            //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));

            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}          
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }       


        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            client.DeleteLicenseBorrowAsync(licenseDelID);
        }

        //提交审核
        private void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            //V_LicenseBorrow licenseObj = null; 
            //if (dgLicense.ItemsSource != null)
            //{
            //    ObservableCollection<string> selectedObj = new ObservableCollection<string>();
            //    foreach (object obj in dgLicense.ItemsSource)
            //    {
            //        if (dgLicense.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox ckbSelect = dgLicense.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (ckbSelect.IsChecked == true)
            //            {
            //                licenseObj = ckbSelect.DataContext as V_LicenseBorrow;
            //                break;
            //            }

            //        }
            //    }
            //}
            //if (licenseObj != null)
            //{
                //T_OA_LICENSEUSER license = new T_OA_LICENSEUSER();
                //license = licenseObj.licenseUser;
                //license.CHECKSTATE = "2";
                //license.UPDATEUSERID = "User";
                //license.UPDATEDATE = DateTime.Now;

                //Flow_FlowRecord_T entity = new Flow_FlowRecord_T();
                //entity.CreateCompanyID = "smt";
                //entity.Content = "dfwfw";
                //entity.CreateUserID = "admin"; //创建流程用户ID
                //entity.CreateDate = DateTime.Now;
                //entity.Flag = "0";
                ////entity.EditDate = DateTime.Now;
                //entity.EditUserID = "admin";
                //entity.FlowCode = "gefege";
                //entity.FormID = license.LICENSEUSERID;//保存的模块表ID
                //entity.GUID = Guid.NewGuid().ToString();
                //entity.InstanceID = "";
                //entity.ModelCode = "LicenseBorrowApp";  //模块代码
                //entity.CreatePostID = "Manage"; //岗位ID
                //entity.ParentStateCode = "StartFlow"; //父状态代码
                //entity.StateCode = "StartFlow";  //状态代码
                //client.LicenseBorrowSubmitFlowAsync(license, entity, "admin");
            //}
            //else
            //{
            //    HtmlPage.Window.Alert("请先选择需要提交审批的外借记录！");
            //}       
            
        }


        //审核
        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {

            if (Licenseuser != null)
            {
                LicenseBorrowForm form = new LicenseBorrowForm(Action.AUDIT, Licenseuser.LICENSEUSERID, checkState);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                browser.HideLeftMenu();
                browser.MinWidth = 450;
                browser.MinHeight = 370;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }



            //ObservableCollection<string> licenseID = GridHelper.GetSelectItem(dgLicense, "myChkBox", Action.Edit);
            //if (licenseID == null || licenseID.Count < 1)
            //{
            //    //HtmlPage.Window.Alert("请先选择需要修改的外借记录！");

            //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));

            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{
            //    //CFrmLicenseBorrowAdd addFrm = new CFrmLicenseBorrowAdd(Action.AUDIT, licenseID[0], checkState);
            //    ////addFrm.Title = "修改外借记录";
            //    //addFrm.Title = Utility.GetResourceStr("AUDITTITLE", "LICENSELENDING");
            //    //addFrm.Show();
            //    //addFrm.ReloadDataEvent += new BaseForm.refreshGridView(addFrm_ReloadDataEvent);

            //    LicenseBorrowForm form = new LicenseBorrowForm(Action.AUDIT, licenseID[0], checkState);
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.HideLeftMenu();
            //    browser.MinWidth = 450;
            //    browser.MinHeight = 370;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

            //}
        }    

       

        //模板列事件
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (!chkbox.IsChecked.Value)
            //{
            //    licenseObj = chkbox.DataContext as V_LicenseBorrow;
            //    licenseDelID.Remove(licenseObj.licenseUser.LICENSEUSERID);
            //    licenseObj = null;
            //}
            GridHelper.SetUnCheckAll(dgLicense);
        }

        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (chkbox.IsChecked.Value)
            //{
            //    licenseObj = chkbox.DataContext as V_LicenseBorrow;

            //    licenseDelID.Add(licenseObj.licenseUser.LICENSEUSERID);
            //}
        }

        //查询
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (licenseObj == null)
            //{
            //    HtmlPage.Window.Alert("请先保存外借记录！");
            //}
            //else
            //{ 

            //}
        }

        private void SetButtonVisible()
        {
            //switch (checkState)
            //{
            //    case "0":  //草稿
            //        ToolBar.btnNew.Visibility = Visibility.Visible;         //新增
            //        ToolBar.btnEdit.Visibility = Visibility.Visible;        //修改
            //        ToolBar.btnDelete.Visibility = Visibility.Visible;      //删除
            //        //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
            //        ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
            //        ToolBar.btnSumbitAudit.Visibility = Visibility.Visible; //提交审核
            //        break;
            //    case "1":  //审批通过
            //        ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
            //        ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
            //        ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
            //        //ToolBar.btnRead.Visibility = Visibility.Visible;      //查看
            //        ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
            //         //提交审核
            //        break;
            //    case "2":  //审批中   审核人身份
            //        ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
            //        ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
            //        ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
            //        //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
            //        ToolBar.btnAudit.Visibility = Visibility.Visible;     //审核
            //         //提交审核
            //        break;
            //    case "3":  //审批未通过
            //        ToolBar.btnNew.Visibility = Visibility.Visible;         //新增
            //        ToolBar.btnEdit.Visibility = Visibility.Visible;        //修改
            //        ToolBar.btnDelete.Visibility = Visibility.Visible;      //删除
            //        //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
            //        ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
            //        ToolBar.btnSumbitAudit.Visibility = Visibility.Visible; //提交审核
            //        break;
            //}
        }

        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "OAORGANLICENUSER");
                checkState = dict.DICTIONARYVALUE.ToString();
                LoadData();
            }  

        }        

        #endregion 

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GridHelper.SetUnCheckAll(dgLicense);
            LoadData();
        }

        private void dgLicense_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgLicense, e.Row, "T_OA_LICENSEUSER");
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
