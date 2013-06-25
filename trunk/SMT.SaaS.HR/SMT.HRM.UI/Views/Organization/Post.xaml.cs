using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.ObjectModel;

using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.Form;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Organization
{
    public partial class Post : BasePage, IClient
    {
        SMTLoading loadbar = new SMTLoading();
        public string Checkstate { get; set; }

        OrganizationServiceClient client;
        public Post()
        {
            InitializeComponent();
            InitEvent();
            //GetEntityLogo("T_HR_POST");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_POST", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);

            client = new OrganizationServiceClient();
            client.PostPagingCompleted += new EventHandler<PostPagingCompletedEventArgs>(client_PostPagingCompleted);
            client.PostDeleteCompleted += new EventHandler<PostDeleteCompletedEventArgs>(client_PostDeleteCompleted);
            client.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(client_GetCompanyActivedCompleted);
            client.GetDepartmentActivedCompleted += new EventHandler<GetDepartmentActivedCompletedEventArgs>(client_GetDepartmentActivedCompleted);
            client.PostCancelCompleted += new EventHandler<PostCancelCompletedEventArgs>(client_PostCancelCompleted);
            this.Loaded += new RoutedEventHandler(Post_Loaded);
            this.DtGrid.SelectionChanged += new SelectionChangedEventHandler(DtGrid_SelectionChanged);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ImageButton btnCancel = new ImageButton();
            btnCancel.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/area/18_import.png", Utility.GetResourceStr("CANCEL")).Click += new RoutedEventHandler(btnCancel_Click);
            ToolBar.stpOtherAction.Children.Add(btnCancel);

        }



        void client_GetCompanyActivedCompleted(object sender, GetCompanyActivedCompletedEventArgs e)
        {
            ObservableCollection<T_HR_COMPANY> ctmp = new ObservableCollection<T_HR_COMPANY>();
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComboBox cbxCpyName = Utility.FindChildControl<ComboBox>(expander, "cbxCpyName");
                if (e.Result != null)
                {
                    ctmp = e.Result;
                    T_HR_COMPANY all = new T_HR_COMPANY();
                    all.COMPANYID = "companyID";
                    all.CNAME = Utility.GetResourceStr("ALL");
                    ctmp.Insert(0, all);
                }
                cbxCpyName.ItemsSource = ctmp;
                cbxCpyName.DisplayMemberPath = "CNAME";
            }
        }


        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_POST");
                //if (dict.DICTIONARYVALUE.Value.ToInt32() == Convert.ToInt32(CheckStates.Approved))
                //{
                //    ToolBar.btnReSubmit.Visibility = Visibility.Visible;
                //}
                dataPager.PageIndex = 1;
                LoadData();
            }
        }

        void Post_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_POST");
            // Utility.DisplayGridToolBarButton(ToolBar, "T_HR_POST", true);
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            //绑定公司
            client.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            string strState = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }
            //根据部门过滤
            ComboBox cbxDepName = Utility.FindChildControl<ComboBox>(expander, "cbxDepName");
            if (cbxDepName != null)
            {
                if (cbxDepName.SelectedIndex > 0)
                {
                    T_HR_DEPARTMENT temp = cbxDepName.SelectedItem as T_HR_DEPARTMENT;
                    filter += "T_HR_DEPARTMENT.DEPARTMENTID==@" + paras.Count().ToString();
                    paras.Add(temp.DEPARTMENTID);
                }
            }
            //根据公司过滤
            ComboBox cbxCpyName = Utility.FindChildControl<ComboBox>(expander, "cbxCpyName");
            if (cbxCpyName != null)
            {
                if (cbxCpyName.SelectedIndex > 0)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    T_HR_COMPANY temp = cbxCpyName.SelectedItem as T_HR_COMPANY;
                    filter += " T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID==@" + paras.Count().ToString();
                    paras.Add(temp.COMPANYID);
                }
            }
            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            if (txtEmpName != null)
            {
                if (!string.IsNullOrEmpty(txtEmpName.Text))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    //filter += "T_HR_POSTDICTIONARY.POSTNAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(T_HR_POSTDICTIONARY.POSTNAME)";
                    paras.Add(txtEmpName.Text.Trim());
                }
            }
            client.PostPagingAsync(dataPager.PageIndex, dataPager.PageSize, "T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME",
                filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);
        }

        void client_PostPagingCompleted(object sender, PostPagingCompletedEventArgs e)
        {
            List<T_HR_POST> list = new List<T_HR_POST>();
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                    //根据部门过滤
                    ComboBox cbxDepName = Utility.FindChildControl<ComboBox>(expander, "cbxDepName");
                    if (cbxDepName.SelectedIndex > 0)
                    {
                        T_HR_DEPARTMENT temp = cbxDepName.SelectedItem as T_HR_DEPARTMENT;
                        var ent = list.Where(s => s.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME == temp.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);
                        list = ent.Count() > 0 ? ent.ToList() : null;
                    }
                    //根据公司过滤
                    ComboBox cbxCpyName = Utility.FindChildControl<ComboBox>(expander, "cbxCpyName");
                    //if (cbxCpyName.SelectedItem != null)
                    if (cbxCpyName.SelectedIndex > 0 && list != null)
                    {
                        T_HR_COMPANY temp = cbxCpyName.SelectedItem as T_HR_COMPANY;
                        var ent = list.Where(s => s.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME == temp.CNAME);
                        list = ent.Count() > 0 ? ent.ToList() : null;
                    }
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            dataPager.PageIndex = 1;
            LoadData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 添加、修改、删除、审核
        public T_HR_POST SelectPost { get; set; }
        void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectPost = grid.SelectedItems[0] as T_HR_POST;
            }
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            PostForm form = new PostForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            //form.MinHeight = 330.0;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
           // dataPager.PageIndex = 1;
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (SelectPost != null)
            {
                PostForm form = new PostForm(FormTypes.Browse, SelectPost.POSTID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinHeight = 260;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectPost != null)
            {
                if (SelectPost.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
                PostForm form = new PostForm(FormTypes.Edit, SelectPost.POSTID);
                //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(SelectPost, "T_HR_POST", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                //form.MinHeight = 330.0;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                //   ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            string strMsg = "";
            if (SelectPost != null)
            {
                if (SelectPost.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                         Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }

                //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(SelectPost, "T_HR_POST", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NODELETEPERMISSION", SelectPost.T_HR_POSTDICTIONARY.POSTNAME),
                //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.PostDeleteAsync(SelectPost.POSTID, strMsg);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                // ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }


        void client_PostDeleteCompleted(object sender, PostDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                         Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg),
                    //     Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "POST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            dataPager.PageIndex = 1;
            LoadData();
        }


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectPost != null)
            {
                PostForm form = new PostForm(FormTypes.Audit, SelectPost.POSTID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }


        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectPost != null)
            {
                PostForm form = new PostForm(FormTypes.Resubmit, SelectPost.POSTID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Resubmit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void client_PostCancelCompleted(object sender, PostCancelCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    SelectPost.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    SelectPost.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
               

                PostForm form = new PostForm(FormTypes.Resubmit, SelectPost.POSTID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Resubmit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
        }

        //撤销岗位
        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            string strMsg = "";
            if (SelectPost != null)
            {

                if (SelectPost.CHECKSTATE != Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("审核通过的才能进行撤销"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                if (SelectPost.EDITSTATE == Convert.ToInt32(EditStates.Canceled).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("已经撤销了"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    SelectPost.EDITSTATE = Convert.ToInt32(EditStates.PendingCanceled).ToString();
                    SelectPost.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    client.PostCancelAsync(SelectPost, strMsg);

                };
                com.SelectionBox(Utility.GetResourceStr("CANCELALTER"), Utility.GetResourceStr("CANCELCONFIRM"), ComfirmWindow.titlename, Result);
            }

            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "CANCEL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "CANCEL"),
         Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_POST");
        }

        private void cbxCpyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_HR_COMPANY tempEnt = ((System.Windows.Controls.Primitives.Selector)(sender)).SelectedItem as T_HR_COMPANY;
            client.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, tempEnt.COMPANYID);
            dataPager.PageIndex = 1;
            LoadData();
        }


        void client_GetDepartmentActivedCompleted(object sender, GetDepartmentActivedCompletedEventArgs e)
        {

            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    List<T_HR_DEPARTMENT> list = e.Result.ToList();
                    ComboBox cbxCpyName = Utility.FindChildControl<ComboBox>(expander, "cbxCpyName");
                    if (cbxCpyName.SelectedIndex > 0)
                    {
                        var ent = from a in list
                                  where a.T_HR_COMPANY.COMPANYID == e.UserState.ToString()
                                  select a;
                        list = ent.Count() > 0 ? ent.ToList() : null;
                    }
                    ComboBox cbxDepName = Utility.FindChildControl<ComboBox>(expander, "cbxDepName");
                    if (list == null)
                    {
                        list = new List<T_HR_DEPARTMENT>();
                    }
                    T_HR_DEPARTMENT deptmp = new T_HR_DEPARTMENT();
                    deptmp.DEPARTMENTID = "departmentID";
                    deptmp.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                    deptmp.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = Utility.GetResourceStr("ALL");
                    list.Insert(0, deptmp);
                    cbxDepName.ItemsSource = list;
                    cbxDepName.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                }
            }
        }


        private void cbxDepName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
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

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }
    }
}
