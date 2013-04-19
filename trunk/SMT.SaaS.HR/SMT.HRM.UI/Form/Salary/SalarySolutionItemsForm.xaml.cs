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

using SMT.Saas.Tools.SalaryWS;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;

using SMT.HRM.UI.Form.Salary;
using System.Collections.ObjectModel;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows.Controls.Primitives;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalarySolutionItemsForm : BaseForm, IEntityEditor, IClient
    {
        SalaryServiceClient client;
        private string savesid;
        public FormTypes FormType;
        public ObservableCollection<T_HR_SALARYSOLUTIONITEM> salarySolutionItemsList { get; set; }
        public string SAVEID
        {
            get { return savesid; }
            set { savesid = value; }
        }
        public SalarySolutionItemsForm()
        {
            InitializeComponent();
            InitPara();
            //LoadData();
            // LoadData(savesid);
        }

        public void InitPara()
        {
            try
            {
                client = new SalaryServiceClient();
                client.SalarySolutionItemAddCompleted += new EventHandler<SalarySolutionItemAddCompletedEventArgs>(client_SalarySolutionItemAddCompleted);
                client.SalarySolutionItemsDeleteCompleted += new EventHandler<SalarySolutionItemsDeleteCompletedEventArgs>(client_SalarySolutionItemsDeleteCompleted);
                client.GetSalarySolutionItemsWithPagingCompleted += new EventHandler<GetSalarySolutionItemsWithPagingCompletedEventArgs>(client_GetSalarySolutionItemsWithPagingCompleted);
                client.GetSalaryItemSetPagingCompleted += new EventHandler<GetSalaryItemSetPagingCompletedEventArgs>(client_GetSalaryItemSetPagingCompleted);
                client.SalarySolutionItemsAddCompleted += new EventHandler<SalarySolutionItemsAddCompletedEventArgs>(client_SalarySolutionItemsAddCompleted);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            #region 工具栏初试化
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            //ToolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
            //ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            //ToolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            //ToolBar.retEdit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            #endregion

        }



        void client_SalarySolutionItemsAddCompleted(object sender, SalarySolutionItemsAddCompletedEventArgs e)
        {
            if (e.Error != null) ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            else
            {
                if (e.Result != "SAVESUCCESSED")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYSOLUTION"));
                }
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            browser_ReloadDataEvent();
        }

        void client_GetSalaryItemSetPagingCompleted(object sender, GetSalaryItemSetPagingCompletedEventArgs e)
        {
            List<V_SALARYSOLUTIONITEM> VsolutionItems = new List<V_SALARYSOLUTIONITEM>();
            V_SALARYSOLUTIONITEM vsolutionItem;
            salarySolutionItemsList = new ObservableCollection<T_HR_SALARYSOLUTIONITEM>();
            T_HR_SALARYSOLUTIONITEM salaryItem;
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    foreach (var item in e.Result)
                    {
                        vsolutionItem = new V_SALARYSOLUTIONITEM();
                        vsolutionItem.SOLUTIONITEMID = Guid.NewGuid().ToString();
                        vsolutionItem.SALARYITEMNAME = item.SALARYITEMNAME;
                        vsolutionItem.SALARYSOLUTIONID = savesid;
                        VsolutionItems.Add(vsolutionItem);
                        salaryItem = new T_HR_SALARYSOLUTIONITEM();
                        salaryItem.SOLUTIONITEMID = vsolutionItem.SOLUTIONITEMID;
                        salaryItem.T_HR_SALARYITEM = new T_HR_SALARYITEM();
                        salaryItem.T_HR_SALARYITEM.SALARYITEMID = item.SALARYITEMID;
                        salaryItem.T_HR_SALARYSOLUTION = new T_HR_SALARYSOLUTION();
                        salaryItem.T_HR_SALARYSOLUTION.SALARYSOLUTIONID = savesid;
                        salarySolutionItemsList.Add(salaryItem);
                    }
                    DtGrid.ItemsSource = VsolutionItems;
                    dataPager.PageCount = e.pageCount;
                }
                else
                {
                    DtGrid.ItemsSource = null;
                }
            }
            else
            {
                DtGrid.ItemsSource = null;
            }
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_GetSalarySolutionItemsWithPagingCompleted(object sender, GetSalarySolutionItemsWithPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    DtGrid.ItemsSource = e.Result.ToList();
                    dataPager.PageCount = e.pageCount;
                }
                else
                {
                    DtGrid.ItemsSource = null;
                }
            }
            else
            {
                DtGrid.ItemsSource = null;
            }
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_SalarySolutionItemsDeleteCompleted(object sender, SalarySolutionItemsDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
            else
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYITEM"));
            LoadData(savesid);
        }

        void client_SalarySolutionItemAddCompleted(object sender, SalarySolutionItemAddCompletedEventArgs e)
        {
            if (e.Error != null) ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            else
            {
                if (e.Result != "SAVESUCCESSED")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYITEM"));
                }
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            browser_ReloadDataEvent();
        }



        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "";// Utility.GetResourceStr("CUSTOMSALARY");
        }
        public string GetStatus()
        {
            return "";//CustomGuerdon != null ? CustomGuerdon.CREATECOMPANYID : "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    //Save();
                    break;
                case "1":
                    //Cancel();
                    break;
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
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

            items.Add(item); items.Clear();

            return items;
        }

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


        void browser_ReloadDataEvent()
        {
            LoadData(savesid);
        }

        #endregion
        void LoadNecessary()
        {
            int pageCount = 0;
            string filter = "";
            string strState = "";
            RefreshUI(RefreshedTypes.ProgressBar);
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            if (!string.IsNullOrEmpty(savesid))
            {
                filter += "MUSTSELECTED==@" + paras.Count().ToString();
                paras.Add("1");
                client.GetSalaryItemSetPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYITEMNAME", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }
        public void LoadData(string id)
        {
            if (FormType == FormTypes.New)
            {
                LoadNecessary();
            }
            else
            {
                int pageCount = 0;
                string filter = "";
                string strState = "";
                RefreshUI(RefreshedTypes.ProgressBar);
                savesid = id;
                System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
                if (!string.IsNullOrEmpty(savesid))
                {
                    filter += "SALARYSOLUTIONID==@" + paras.Count().ToString();
                    paras.Add(savesid);
                    client.GetSalarySolutionItemsWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "ORDERNUMBER", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                }
            }
        }



        #region 添加,修改,删除,查询,审核

        public void Save()
        {
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            filter += "MUSTSELECTED==@" + paras.Count().ToString();
            paras.Add("1");
            filter += " and CREATECOMPANYID==@" + paras.Count().ToString();
            paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            client.SalarySolutionItemsAddAsync(filter, paras, savesid, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            FormType = FormTypes.Edit;
        }
        void btnNew_Click(object sender, RoutedEventArgs e)
        {

            SMT.HRM.UI.Form.Salary.SolutionItemForm form = new SolutionItemForm(FormTypes.New, savesid);
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 400;
            form.MinHeight = 180;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

            #region ----
            //System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            ////string filter = "";
            ////filter = "CHECKSTATE==@" + paras.Count;
            //paras.Add(Convert.ToInt16(CheckStates.Approved).ToString());
            //LookupForm lookup = new LookupForm(EntityNames.SalaryStandard,
            //  typeof(List<T_HR_SALARYSTANDARD>), cols, filter, paras);
            #endregion

        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_SALARYSOLUTIONITEM tmp = DtGrid.SelectedItems[0] as V_SALARYSOLUTIONITEM;
                SMT.HRM.UI.Form.Salary.SolutionItemForm form = new SolutionItemForm(FormTypes.Edit, tmp.SOLUTIONITEMID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 400;
                form.MinHeight = 180;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }

        }
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData(savesid);
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedIndex != -1)
            {
                string Result = "";
                if (DtGrid.SelectedItems.Count > 0)
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();

                    foreach (V_SALARYSOLUTIONITEM tmp in DtGrid.SelectedItems)
                    {
                        if (tmp.MUSTSELECTED == "1")
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MUSTSELECTEDCANNOTDELETEED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MUSTSELECTEDCANNOTDELETEED"));
                            return;
                        }
                        ids.Add(tmp.SOLUTIONITEMID);
                    }

                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        client.SalarySolutionItemsDeleteAsync(ids);
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            }

        }


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核            
        }
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData(savesid);
        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
    }
}
