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
    public partial class SalaryTaxesForm : BaseForm, IEntityEditor,IClient
    {

        SalaryServiceClient client;
        private string savesid;
        public string SAVEID
        {
            get { return savesid; }
            set { savesid = value; }
        }
        public SalaryTaxesForm()
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
                client.SalaryTaxesAddCompleted += new EventHandler<SalaryTaxesAddCompletedEventArgs>(client_SalaryTaxesAddCompleted);
                client.SalaryTaxesDeleteCompleted += new EventHandler<SalaryTaxesDeleteCompletedEventArgs>(client_SalaryTaxesDeleteCompleted);
                client.GetSalaryTaxesWithPagingCompleted += new EventHandler<GetSalaryTaxesWithPagingCompletedEventArgs>(client_GetSalaryTaxesWithPagingCompleted);
                client.CheckSalaryTaxesCompleted += new EventHandler<CheckSalaryTaxesCompletedEventArgs>(client_CheckSalaryTaxesCompleted);
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
            ToolBar.btnRefresh.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Visibility = Visibility.Visible;
            ToolBar.retNew.Visibility = Visibility.Visible;
            //ToolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            //ToolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Collapsed;
            #endregion

        }

        void client_CheckSalaryTaxesCompleted(object sender, CheckSalaryTaxesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result)
                {
                    T_HR_SALARYTAXES taxes = new T_HR_SALARYTAXES();
                    taxes.SALARYTAXESID = Guid.NewGuid().ToString();
                    taxes.TAXESRATE = Convert.ToDecimal(txtTaxesRate.Text);
                    taxes.TAXESSUM = Convert.ToDecimal(txtTaxessum.Value);
                    taxes.MINITAXESSUM = Convert.ToDecimal(txtMinTaxessum.Value);
                    taxes.CALCULATEDEDUCT = Convert.ToDecimal(txtCalculateDeduct.Text);
                    taxes.T_HR_SALARYSOLUTION = new T_HR_SALARYSOLUTION();
                    taxes.T_HR_SALARYSOLUTION.SALARYSOLUTIONID = savesid;
                    client.SalaryTaxesAddAsync(taxes);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CHECKTAXES"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CHECKTAXES"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }

        }

        void client_GetSalaryTaxesWithPagingCompleted(object sender, GetSalaryTaxesWithPagingCompletedEventArgs e)
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

        void client_SalaryTaxesDeleteCompleted(object sender, SalaryTaxesDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
            else
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "TAXESRATE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "TAXESRATE"));
            LoadData(savesid);
        }

        void client_SalaryTaxesAddCompleted(object sender, SalaryTaxesAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result.ToString() != "SAVESUCCESSED")
                {
                    if (e.Result == "NOSOLUTION")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "先新增薪资方案", Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);                         
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);                        
                    }                    
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString()));
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "TAXESRATE"));
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

            //ToolbarItem item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "0",
            //    Title = "保存",
            //    ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            //};

            //items.Add(item);

            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "1",
            //    Title = "保存并关闭",
            //    ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            //};

            //items.Add(item); items.Clear();

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
            //int pageCount = 0;
            //string filter = "";
            //string strState = "";
            //RefreshUI(RefreshedTypes.ProgressBar);
            //System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            //filter = "T_HR_SALARYSOLUTION.SALARYSOLUTIONID==@" + paras.Count().ToString();
            //paras.Add(savesid);
            //client.GetSalaryStandardItemPagingAsync(dataPager.PageIndex, dataPager.PageSize, "TAXESSUM", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
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
                filter = "T_HR_SALARYSOLUTION.SALARYSOLUTIONID==@" + paras.Count().ToString();
                paras.Add(savesid);
                client.GetSalaryTaxesWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "TAXESSUM", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);
            }
        }



        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTaxessum.Value.ToString().Trim()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "TAXESSUM"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "TAXESSUM"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            if (string.IsNullOrEmpty(txtMinTaxessum.Value.ToString().Trim()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "TAXESSUM"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "TAXESSUM"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            if (string.IsNullOrEmpty(txtTaxesRate.Text.Trim()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "TAXESRATE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "TAXESRATE"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            if (string.IsNullOrEmpty(txtCalculateDeduct.Text.Trim()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CALCULATEDEDUCT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CALCULATEDEDUCT"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            if (txtTaxessum.Value < txtMinTaxessum.Value)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("TAXESCOMPARE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("TAXESCOMPARE"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            client.CheckSalaryTaxesAsync(savesid, Convert.ToDecimal(txtMinTaxessum.Value));

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

                    foreach (T_HR_SALARYTAXES tmp in DtGrid.SelectedItems)
                    {
                        ids.Add(tmp.SALARYTAXESID);
                    }

                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        client.SalaryTaxesDeleteAsync(ids);
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
