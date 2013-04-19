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
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
//using SMT.HRM.UI.HrCommonWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.SalaryWS;

namespace SMT.HRM.UI.Form.Setting
{
    public partial class SsytemParamSetModel : BaseForm, IEntityEditor
    {
        SMTLoading loadbar = new SMTLoading();
        //private HrCommonServiceClient  client;
        private SalaryServiceClient client = new SalaryServiceClient();
        private string GETMODELTYPES = "0";
        public T_HR_SYSTEMSETTING SelectID { get; set; }
        private ObservableCollection<string> systemParamSetIDs = new ObservableCollection<string>();

        public ObservableCollection<string> SystemParamSetIDs
        {
            get { return systemParamSetIDs; }
            set { systemParamSetIDs = value; }
        }
        public string GETMODELTYPE 
        {
            get { return GETMODELTYPES; }
            set { GETMODELTYPES = value; }
        }

        public SsytemParamSetModel()
        {
            InitializeComponent();
            InitContent();
        }

        public void InitContent()
        {
            PARENT.Children.Add(loadbar);
            //client = new HrCommonServiceClient();
            client.GetSystemParamSetPagingCompleted += new EventHandler<GetSystemParamSetPagingCompletedEventArgs>(client_GetSystemParamSetPagingCompleted);
            client.SystemParamSetDeleteCompleted += new EventHandler<SystemParamSetDeleteCompletedEventArgs>(client_SystemParamSetDeleteCompleted);
            client.AddSystemParamSetCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AddSystemParamSetCompleted);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            //ToolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            //ToolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData(GETMODELTYPE);
        }

        void client_AddSystemParamSetCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SYSTEMPARAMSET"));
            }
        }

        void client_GetSystemParamSetPagingCompleted(object sender, GetSystemParamSetPagingCompletedEventArgs e)
        {
            List<T_HR_SYSTEMSETTING> list = new List<T_HR_SYSTEMSETTING>();
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;

                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            loadbar.Stop();
        }

        void client_SystemParamSetDeleteCompleted(object sender, SystemParamSetDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED"));
                LoadData(GETMODELTYPE);
            }
        }

        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            OperatorParamSet Oparamset = new OperatorParamSet(FormTypes.New, "", GETMODELTYPE);
            EntityBrowser browser = new EntityBrowser(Oparamset);
            
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        void browser_ReloadDataEvent()
        {
            LoadData(GETMODELTYPE);
        }


        public void LoadData(string modeltype)
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            loadbar.Start();
            if (tbmodetypevalue != null && !string.IsNullOrEmpty(tbmodetypevalue.Text.Trim()))
            {
                filter += "PARAMETERNAME==@" + paras.Count().ToString();
                paras.Add(tbmodetypevalue.Text.Trim());
            }
            if (filter != "") filter += " and ";
            filter += "MODELTYPE==@" + paras.Count().ToString();
            paras.Add(modeltype);
            client.GetSystemParamSetPagingAsync(dataPager.PageIndex, dataPager.PageSize, "PARAMETERNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SYSTEMSETTING tmpSystemset = DtGrid.SelectedItems[0] as T_HR_SYSTEMSETTING;

                //if (tmpSystemsetIDs.Count <= 0)
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                //    return;
                //}
                OperatorParamSet form = new OperatorParamSet(FormTypes.Edit, tmpSystemset.SYSTEMSETTINGID, GETMODELTYPE);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //ComfirmBox deleComfir = new ComfirmBox();
            //deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
            //deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
            //deleComfir.ButtonOK.Click += new RoutedEventHandler(ButtonOK1_Click);
            //deleComfir.Show();


            //string Result = "";
            //if (DtGrid.SelectedItems.Count > 0)
            //{
            //    ObservableCollection<string> ids = new ObservableCollection<string>();

            //    foreach (T_HR_SYSTEMSETTING tmp in DtGrid.SelectedItems)
            //    {
            //        ids.Add(tmp.SYSTEMSETTINGID);
            //    }

            //    ComfirmWindow com = new ComfirmWindow();
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {
            //        client.SystemParamSetDeleteAsync(ids);
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            //}

            if (DtGrid.SelectedItems.Count <= 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                return;
            }
            string Result = "";
            if (SelectID != null)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();
                    ids.Add(SelectID.SYSTEMSETTINGID);
                    client.SystemParamSetDeleteAsync(ids);
                    LoadData(GETMODELTYPE);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }

        }

        void ButtonOK1_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_SYSTEMSETTING tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.SYSTEMSETTINGID);
                }
                client.SystemParamSetDeleteAsync(ids);
            }

        }


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核            
        }
        #endregion  

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "SystemParamSet";
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    SaveAndClose();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息A",
                Tooltip = "详细信息A"
            };
            items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = "保存",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };


            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = "保存并关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            items.Clear();

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        #endregion

        public void Save()
        {

        }
        public void SaveAndClose()
        {

        }


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectID = grid.SelectedItems[0] as T_HR_SYSTEMSETTING;
            }
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadData(GETMODELTYPE);
        }
    }
}
