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
    public partial class StandardItemForm : BaseForm, IEntityEditor ,IClient
    {
        SalaryServiceClient client;
        private string savesid;
        public string SAVEID
        {
            get { return savesid; }
            set { savesid = value; }
        }
        public StandardItemForm()
        {
            InitializeComponent();
            InitPara();
            //LoadData();
            LoadData(savesid);
        }

        public void InitPara()
        {
            try
            {
                client = new SalaryServiceClient();
                client.SalaryStandardItemAddCompleted += new EventHandler<SalaryStandardItemAddCompletedEventArgs>(client_SalaryStandardItemAddCompleted);
                client.SalaryStandardItemDeleteCompleted += new EventHandler<SalaryStandardItemDeleteCompletedEventArgs>(client_SalaryStandardItemDeleteCompleted);
                client.GetSalaryStandardItemPagingCompleted += new EventHandler<GetSalaryStandardItemPagingCompletedEventArgs>(client_GetSalaryStandardItemPagingCompleted);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
               // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            #region 工具栏初试化
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            ToolBar.btnNew.Content = "选择薪资项";
            ToolBar.btnNew.Width = 70;
            //ToolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            //ToolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Collapsed;
            #endregion

        }

        void client_SalaryStandardItemAddCompleted(object sender, SalaryStandardItemAddCompletedEventArgs e)
        {
            if (e.Error != null) ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            else 
            {
                if (e.Result == 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOSALARYSTANDARDID"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOSALARYSTANDARDID"));
                }
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            browser_ReloadDataEvent();
        }

        void client_GetSalaryStandardItemPagingCompleted(object sender, GetSalaryStandardItemPagingCompletedEventArgs e)
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

        void client_SalaryStandardItemDeleteCompleted(object sender, SalaryStandardItemDeleteCompletedEventArgs e)
        {
            if(e.Error!=null)
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
            else
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSTANDARDITEM"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSTANDARDITEM"));
             LoadData(savesid);
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

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            string strState = "";
            RefreshUI(RefreshedTypes.ProgressBar);
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            client.GetSalaryStandardItemPagingAsync(dataPager.PageIndex, dataPager.PageSize, "STANDRECORDITEMID", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        public void LoadData(string id)
        {
            int pageCount = 0;
            string filter = "";
            string strState = "";
            RefreshUI(RefreshedTypes.ProgressBar);
            savesid = id;
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            if (!string.IsNullOrEmpty(savesid))
            {
                filter += "SALARYSTANDARDID==@" + paras.Count().ToString();
                paras.Add(savesid);
                client.GetSalaryStandardItemPagingAsync(dataPager.PageIndex, dataPager.PageSize, "STANDRECORDITEMID", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }



        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string filter = "";
            ObservableCollection<object> paras = new ObservableCollection<object>();
                T_HR_SALARYSTANDARDITEM salarystandarditem = new T_HR_SALARYSTANDARDITEM();
                T_HR_SALARYSTANDARD salarystandard = new T_HR_SALARYSTANDARD();
                salarystandard.SALARYSTANDARDID = savesid;
                Dictionary<string, string> cols = new Dictionary<string, string>();
                cols.Add("SALARYITEMNAME", "SALARYITEMNAME");
                cols.Add("GUERDONSUM", "GUERDONSUM");
                cols.Add("CALCULATEFORMULA", "CALCULATEFORMULA");
                RefreshUI(RefreshedTypes.ProgressBar);
                filter += "CREATECOMPANYID==@" + paras.Count().ToString();
                paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
                LookupForm lookup = new LookupForm(EntityNames.SalaryItemSet,
                    typeof(List<T_HR_SALARYITEM>), cols, filter,paras);
                lookup.TitleContent = Utility.GetResourceStr("SALARYITEM");
                lookup.SelectedClick += (o, ev) =>
                {
                    T_HR_SALARYITEM ent = lookup.SelectedObj as T_HR_SALARYITEM;
                    if (ent != null)
                    {
                        salarystandarditem.STANDRECORDITEMID = Guid.NewGuid().ToString();
                        salarystandarditem.T_HR_SALARYITEM = ent;
                        salarystandarditem.T_HR_SALARYSTANDARD = salarystandard;
                        salarystandarditem.SUM = ent.GUERDONSUM.ToString();
                        salarystandarditem.REMARK = ent.REMARK;
                        salarystandarditem.CREATEDATE = System.DateTime.Now;
                        salarystandarditem.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        client.SalaryStandardItemAddAsync(salarystandarditem);
                    }
                };
                lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

            #region ----
            //System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            ////string filter = "";
            ////filter = "CHECKSTATE==@" + paras.Count;
            //paras.Add(Convert.ToInt16(CheckStates.Approved).ToString());
            //LookupForm lookup = new LookupForm(EntityNames.SalaryStandard,
            //  typeof(List<T_HR_SALARYSTANDARD>), cols, filter, paras);
            #endregion

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

                    foreach (V_SALARYSTANDARDITEM tmp in DtGrid.SelectedItems)
                    {
                        ids.Add(tmp.STANDRECORDITEMID);
                    }

                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        client.SalaryStandardItemDeleteAsync(ids);
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
