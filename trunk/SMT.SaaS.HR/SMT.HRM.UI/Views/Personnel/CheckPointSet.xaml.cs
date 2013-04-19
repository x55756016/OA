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

using System.Collections.ObjectModel;
using SMT.Saas.Tools.PersonnelWS;
using SMT.HRM.UI.Form.Personnel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class CheckPointSet : BasePage, IClient
    {
        private T_HR_CHECKPROJECTSET CheckProjectSingle { get; set; }
        private T_HR_CHECKPOINTSET CheckPointSingle { get; set; }
        SMTLoading loadbar = new SMTLoading();
        //项目考核
        private ObservableCollection<T_HR_CHECKPROJECTSET> checkProject = new ObservableCollection<T_HR_CHECKPROJECTSET>();

        public ObservableCollection<T_HR_CHECKPROJECTSET> CheckProject
        {
            get { return checkProject; }
            set { checkProject = value; }
        }

        //项目考核点
        private ObservableCollection<T_HR_CHECKPOINTSET> checkPoint = new ObservableCollection<T_HR_CHECKPOINTSET>();

        public ObservableCollection<T_HR_CHECKPOINTSET> CheckPoint
        {
            get { return checkPoint; }
            set { checkPoint = value; }
        }

        //项目等级
        private ObservableCollection<T_HR_CHECKPOINTLEVELSET> checkPointLevel = new ObservableCollection<T_HR_CHECKPOINTLEVELSET>();

        public ObservableCollection<T_HR_CHECKPOINTLEVELSET> CheckPointLevel
        {
            get { return checkPointLevel; }
            set { checkPointLevel = value; }
        }

        PersonnelServiceClient client;
        public CheckPointSet()
        {
            InitializeComponent();
            InitEvent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_CHECKPOINTSET", false);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 添加判断权限
            LoadData();
        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);

            client = new PersonnelServiceClient();
            client.CheckProjectSetPagingCompleted += new EventHandler<CheckProjectSetPagingCompletedEventArgs>(client_CheckProjectSetPagingCompleted);
            client.CheckProjectSetDeleteCompleted += new EventHandler<CheckProjectSetDeleteCompletedEventArgs>(client_CheckProjectSetDeleteCompleted);

            client.GetCheckPointSetByPrimaryIDCompleted += new EventHandler<GetCheckPointSetByPrimaryIDCompletedEventArgs>(client_GetCheckPointSetByPrimaryIDCompleted);
            client.CheckPointSetDeleteCompleted += new EventHandler<CheckPointSetDeleteCompletedEventArgs>(client_CheckPointSetDeleteCompleted);

            client.GetCheckPointLevelSetByPrimaryIDCompleted += new EventHandler<GetCheckPointLevelSetByPrimaryIDCompletedEventArgs>(client_GetCheckPointLevelSetByPrimaryIDCompleted);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.cbxOtherAction.SelectionChanged += new SelectionChangedEventHandler(cbxOtherAction_SelectionChanged);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/18_workPlace.png", "浏览考核").Click += new RoutedEventHandler(CheckPointSet_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //绑定考核员工类型
            ToolBar.txtOtherName.Visibility = Visibility.Visible;
            ToolBar.cbxOtherAction.Visibility = Visibility.Visible;
            ToolBar.txtOtherName.Text = Utility.GetResourceStr("CHECKEMPLOYEETYPE");
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            EmployeeTypeBinder();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            LoadData();
        }



        void CheckPointSet_Click(object sender, RoutedEventArgs e)
        {
            CheckGrade form = new CheckGrade(FormTypes.Browse, "", "0");
            EntityBrowser browser = new EntityBrowser(form);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        //员工类型绑定
        private void EmployeeTypeBinder()
        {
            var objs = from d in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                       where d.DICTIONCATEGORY == "CHECKEMPLOYEETYPE"
                       select d;
            List<T_SYS_DICTIONARY> tmpDicts = objs.ToList();

            ToolBar.cbxOtherAction.ItemsSource = tmpDicts;
            ToolBar.cbxOtherAction.DisplayMemberPath = "DICTIONARYNAME";
            if (ToolBar.cbxOtherAction.Items.Count() > 0)
            {
                ToolBar.cbxOtherAction.SelectedItem = ToolBar.cbxOtherAction.Items[0];
            }
        }

        void cbxOtherAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CheckProjectSingle != null)
            {
                T_SYS_DICTIONARY dic = ToolBar.cbxOtherAction.SelectedItem as T_SYS_DICTIONARY;
                client.GetCheckPointSetByPrimaryIDAsync(CheckProjectSingle.CHECKPROJECTID, dic.DICTIONARYVALUE.ToString());
            }
        }

        void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            client.CheckProjectSetPagingAsync(dataProjectPager.PageIndex, dataProjectPager.PageSize, "CHECKPROJECT", filter, paras, pageCount);
        }

        void client_CheckProjectSetPagingCompleted(object sender, CheckProjectSetPagingCompletedEventArgs e)
        {
            List<T_HR_CHECKPROJECTSET> list = new List<T_HR_CHECKPROJECTSET>();
            if (e.Error != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                dtgProject.ItemsSource = list;
                dataProjectPager.PageCount = e.pageCount;
                if (dtgProject.ItemsSource != null)
                {
                    if (list.Count > 0)
                    {
                        dtgProject.SelectedItems.Add(list[0]);
                    }
                }
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }

        #region 考核项目
        private void dtgProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                CheckProjectSingle = grid.SelectedItems[0] as T_HR_CHECKPROJECTSET;
            }
            if (ToolBar.cbxOtherAction.SelectedItem != null)
            {
                T_SYS_DICTIONARY dic = ToolBar.cbxOtherAction.SelectedItem as T_SYS_DICTIONARY;
                client.GetCheckPointSetByPrimaryIDAsync(CheckProjectSingle.CHECKPROJECTID, dic.DICTIONARYVALUE.ToString());
            }
        }

        void client_GetCheckPointSetByPrimaryIDCompleted(object sender, GetCheckPointSetByPrimaryIDCompletedEventArgs e)
        {
            List<T_HR_CHECKPOINTSET> list = new List<T_HR_CHECKPOINTSET>();
            if (e.Error != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
            }
            dtgPoint.ItemsSource = list;
            if (list.Count() > 0)
            {
                dtgPoint.SelectedItems.Add(list[0]);
            }
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            CheckProjectSetForm form = new CheckProjectSetForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 400.0;
            form.MinHeight = 300.0;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (CheckProjectSingle == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            CheckProjectSetForm form = new CheckProjectSetForm(FormTypes.Browse, CheckProjectSingle.CHECKPROJECTID);
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 400.0;
            browser.FormType = FormTypes.Browse;
            form.MinHeight = 300.0;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckProjectSingle == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            CheckProjectSetForm form = new CheckProjectSetForm(FormTypes.Edit, CheckProjectSingle.CHECKPROJECTID);
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 400.0;

            form.MinHeight = 300.0;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (CheckProjectSingle != null)
            {
                //ComfirmBox deleComfir = new ComfirmBox();
                //deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
                //deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
                //deleComfir.ButtonOK.Click += new RoutedEventHandler(ProjectDel_Click);
                //deleComfir.Show();

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    //ObservableCollection<string> ids = new ObservableCollection<string>();

                    //foreach (T_HR_EMPLOYEECONTRACT tmp in DtGrid.SelectedItems)
                    //{
                    //    if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    //    {
                    //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                    //        break;
                    //    }
                    //    ids.Add(tmp.EMPLOYEECONTACTID);
                    //}
                    //client.EmployeeContractDeleteAsync(ids);
                    client.CheckProjectSetDeleteAsync(CheckProjectSingle.CHECKPROJECTID);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void ProjectDel_Click(object sender, RoutedEventArgs e)
        {
            client.CheckProjectSetDeleteAsync(CheckProjectSingle.CHECKPROJECTID);
        }

        void client_CheckProjectSetDeleteCompleted(object sender, CheckProjectSetDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CHECKPROJECTSET"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }
        #endregion


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckProjectSingle == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        #region 考核点和考核等级
        private void btnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (CheckProjectSingle != null)
            {
                CheckPointSetForm form = new CheckPointSetForm(FormTypes.New, CheckProjectSingle, "");
                //form.ParentLayoutRoot = SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot;
                form.MinWidth = 600;
                form.MinHeight = 430;
                form.ReloadDataEvent += new BaseFloatable.refreshGridView(from_ReloadData);
                form.Show();
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECT", "CHECKPROJECT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECT", "CHECKPROJECT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

        }

        private void btnEditPoint_Click(object sender, RoutedEventArgs e)
        {
            if (CheckPointSingle == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            CheckPointSetForm form = new CheckPointSetForm(FormTypes.Edit, CheckProjectSingle, CheckPointSingle.CHECKPOINTSETID);
            //form.ParentLayoutRoot = SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot;
            form.MinWidth = 600;
            form.MinHeight = 430;
            form.ReloadDataEvent += new BaseFloatable.refreshGridView(from_ReloadData);
            form.Show();
        }

        void from_ReloadData()
        {
            T_SYS_DICTIONARY dic = ToolBar.cbxOtherAction.SelectedItem as T_SYS_DICTIONARY;
            client.GetCheckPointSetByPrimaryIDAsync(CheckProjectSingle.CHECKPROJECTID, dic.DICTIONARYVALUE.ToString());
        }

        private void dtgPoint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                CheckPointSingle = grid.SelectedItems[0] as T_HR_CHECKPOINTSET;
                client.GetCheckPointLevelSetByPrimaryIDAsync(CheckPointSingle.CHECKPOINTSETID);
            }
            else
            {
                dtgPointLevel.ItemsSource = null;
            }
        }

        void client_GetCheckPointLevelSetByPrimaryIDCompleted(object sender, GetCheckPointLevelSetByPrimaryIDCompletedEventArgs e)
        {
            List<T_HR_CHECKPOINTLEVELSET> list = new List<T_HR_CHECKPOINTLEVELSET>();
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
            }
            dtgPointLevel.ItemsSource = list;
        }

        private void btnDelPoint_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (CheckPointSingle != null)
            {
                //ComfirmBox deleComfir = new ComfirmBox();
                //deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
                //deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
                //deleComfir.ButtonOK.Click += new RoutedEventHandler(ButtonOK_Click);
                //deleComfir.Show();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    //ObservableCollection<string> ids = new ObservableCollection<string>();

                    //foreach (T_HR_EMPLOYEECONTRACT tmp in DtGrid.SelectedItems)
                    //{
                    //    if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    //    {
                    //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                    //        break;
                    //    }
                    //    ids.Add(tmp.EMPLOYEECONTACTID);
                    //}
                    client.CheckPointSetDeleteAsync(CheckPointSingle.CHECKPOINTSETID);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            client.CheckPointSetDeleteAsync(CheckPointSingle.CHECKPOINTSETID);
        }

        void client_CheckPointSetDeleteCompleted(object sender, CheckPointSetDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CHECKPOINT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                from_ReloadData();
            }
        }
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
