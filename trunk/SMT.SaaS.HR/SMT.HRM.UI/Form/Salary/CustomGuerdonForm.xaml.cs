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
    public partial class CustomGuerdonForm : BaseForm, IEntityEditor ,IClient
    {
        SalaryServiceClient client;
        public V_CUSTOMGUERDON SelectID { get; set; }
        private string savesid;

        private T_HR_CUSTOMGUERDON customGuerdon;

        private List<V_CUSTOMGUERDON> customGuerdonlist;

        private ObservableCollection<string> customSalaryIDs = new ObservableCollection<string>();

        public ObservableCollection<string> CustomSalaryIDs
        {
            get { return customSalaryIDs; }
            set { customSalaryIDs = value; }
        }

        public string SAVEID 
        {
            get { return savesid; }
            set { savesid = value; }
        }
        public CustomGuerdonForm()
        {
            InitializeComponent();
            InitPara();
            //txtSalaryStandardName.IsReadOnly = true;
        }
        public CustomGuerdonForm(string id,string names)
        {
            InitPara();
            this.SAVEID = id;
            InitializeComponent();
            //txtSalaryStandardName.IsReadOnly = true;
            //txtSalaryStandardName.Text = names;
            LoadData(SAVEID);
        }

        public void InitPara()
        {
            customGuerdon = new T_HR_CUSTOMGUERDON();
            customGuerdonlist = new List<V_CUSTOMGUERDON>();
            try
            { 
                client = new SalaryServiceClient();
                client.GetCustomGuerdonSetPagingCompleted += new EventHandler<GetCustomGuerdonSetPagingCompletedEventArgs>(client_GetCustomGuerdonSetPagingCompleted);
                client.CustomGuerdonSetDeleteCompleted += new EventHandler<CustomGuerdonSetDeleteCompletedEventArgs>(client_CustomGuerdonSetDeleteCompleted);
                client.CustomGuerdonAddCompleted += new EventHandler<CustomGuerdonAddCompletedEventArgs>(client_CustomGuerdonAddCompleted);    
                client.GetCustomGuerdonPagingCompleted += new EventHandler<GetCustomGuerdonPagingCompletedEventArgs>(client_GetCustomGuerdonPagingCompleted);
                client.GetCustomGuerdonCompleted += new EventHandler<GetCustomGuerdonCompletedEventArgs>(client_GetCustomGuerdonCompleted);
                client.CustomGuerdonDeleteCompleted += new EventHandler<CustomGuerdonDeleteCompletedEventArgs>(client_CustomGuerdonDeleteCompleted);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            #region 工具栏初试化
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

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
            #endregion

        }

        void client_CustomGuerdonAddCompleted(object sender, CustomGuerdonAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
            }
            if (e.Result == "NOSALARYSTANDARDID")
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOSALARYSTANDARDID"));
            }
            RefreshUI(RefreshedTypes.All);
        }

        void client_GetCustomGuerdonCompleted(object sender, GetCustomGuerdonCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
            }
            customGuerdonlist = e.Result.ToList();
            //DtGrid.DataContext = customGuerdon;
            DtGrid.ItemsSource = customGuerdonlist;
            //dataPager.PageCount = e.pageCount;
        }

        void client_GetCustomGuerdonPagingCompleted(object sender, GetCustomGuerdonPagingCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
            }
            //customGuerdonlist = e.Result.ToList();
            //DtGrid.ItemsSource = customGuerdonlist;
            dataPager.PageCount = e.pageCount;
        }

        void client_GetCustomGuerdonSetPagingCompleted(object sender, GetCustomGuerdonSetPagingCompletedEventArgs e)
        {
            List<T_HR_CUSTOMGUERDONSET> list = new List<T_HR_CUSTOMGUERDONSET>();
            if (e.Result != null)
            {
                list = e.Result.ToList();
            }
            DtGrid.ItemsSource = list;

            dataPager.PageCount = e.pageCount;
        }

        void client_CustomGuerdonSetDeleteCompleted(object sender, CustomGuerdonSetDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED"));
                LoadData(SAVEID);
                RefreshUI(RefreshedTypes.All); 
            }
        }

        void client_CustomGuerdonDeleteCompleted(object sender, CustomGuerdonDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CUSTOMGUERDON"));
                LoadData(SAVEID);
                RefreshUI(RefreshedTypes.All);
            }
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData(SAVEID);
        }

        //// 当用户导航到此页面时执行。
        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    //if (CheckUserPermission())
        //    LoadData(SAVEID);
        //    ViewTitles.TextTitle.Text = "薪资标准名"; //e.Uri.ToString();
        //}


        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData(SAVEID);
        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            customSalaryIDs.Clear();
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectID = grid.SelectedItems[0] as V_CUSTOMGUERDON;
                customSalaryIDs.Add((grid.SelectedItems[0] as V_CUSTOMGUERDON).CUSTOMGUERDONID);
            }
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
            //item = new NavigateItem
            //{
            //    Title = "薪资标准",
            //    Tooltip = "薪资标准",
            //    Url = "/Salary/SalaryStandard.xaml"
            //};
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
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = "保存并关闭",
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
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
//....................................


        void browser_ReloadDataEvent()
        {
            LoadData(SAVEID);
        }


        private void LoadData(string id)
        {
            //int pageCount = 0;
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            client.GetCustomGuerdonAsync(id);
        }

        internal void LoadData(string id, string names)
        {
            this.SAVEID = id;
            //if(names!=null)txtSalaryStandardName.Text = names;
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            client.GetCustomGuerdonAsync(id);
        }
        
        void ButtonOK1_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (V_CUSTOMGUERDON tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.CUSTOMGUERDONID);
                }
                client.CustomGuerdonDeleteAsync(ids);
                LoadData(SAVEID);
            }
        }


        #endregion  

        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            CustomSalaryAddForm CustomAdd = new CustomSalaryAddForm(SAVEID);
            EntityBrowser browser = new EntityBrowser(CustomAdd);
            CustomAdd.MinWidth = 330;
            CustomAdd.MinHeight = 160;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData(SAVEID);
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedIndex != -1)
            {
                //ComfirmBox deleComfir = new ComfirmBox();
                //deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
                //deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
                //deleComfir.ButtonOK.Click += new RoutedEventHandler(ButtonOK1_Click);
                //deleComfir.Show();

                string Result = "";
                if (DtGrid.SelectedItems.Count > 0)
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();

                    foreach (V_CUSTOMGUERDON tmp in DtGrid.SelectedItems)
                    {
                        ids.Add(tmp.CUSTOMGUERDONID);
                    }

                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        client.CustomGuerdonDeleteAsync(ids);
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                    //LoadData(SAVEID);
                }

            }
            else 
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            }

        }


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核            
        }
        #endregion  

        #region ----------
        //private void lkCustomSalary_FindClick(object sender, EventArgs e)
        //{
        //    Dictionary<string, string> cols = new Dictionary<string, string>();
        //    cols.Add("GUERDONNAME", "GUERDONNAME");
        //    cols.Add("GUERDONSUM", "GUERDONSUM");
        //    cols.Add("UPDATEDATE", "UPDATEDATE");

        //    LookupForm lookup = new LookupForm(EntityNames.CustomGuerdonSet,
        //        typeof(List<T_HR_CUSTOMGUERDONSET>), cols);

        //    lookup.SelectedClick += (o, ev) =>
        //    {
        //        T_HR_CUSTOMGUERDONSET ent = lookup.SelectedObj as T_HR_CUSTOMGUERDONSET;
        //        if (!GetExit(ent.GUERDONNAME))
        //        {
        //            if (ent != null)
        //            {
        //                lkCustomSalary.DataContext = ent;
        //                T_HR_SALARYSTANDARD entSALARYSTANDARD = new T_HR_SALARYSTANDARD();
        //                entSALARYSTANDARD.SALARYSTANDARDID = SAVEID;
        //                customGuerdon.CUSTOMGUERDONID = Guid.NewGuid().ToString();
        //                customGuerdon.T_HR_SALARYSTANDARD = entSALARYSTANDARD;
        //                customGuerdon.T_HR_CUSTOMGUERDONSET = ent;
        //                customGuerdon.SUM = ent.GUERDONSUM;
        //                customGuerdon.CREATEDATE = System.DateTime.Now;

        //                customGuerdon.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //                customGuerdon.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //                customGuerdon.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        //                customGuerdon.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //                customGuerdon.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        //                customGuerdon.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //                customGuerdon.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //                customGuerdon.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;

        //                client.CustomGuerdonAddAsync(customGuerdon);
        //                LoadData(SAVEID);
        //            }
        //        }
        //        else
        //        {
        //            ent.GUERDONNAME = "already exists";
        //            lkCustomSalary.DataContext = ent;
        //            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("already exists"));
        //        }
        //    };
        //    lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{}); 

        //}
        private  bool GetExit(string names)
        {
            try
            {
                foreach (V_CUSTOMGUERDON cg in customGuerdonlist)
                {
                    if (cg.GUERDONNAME == names)
                        return true;
                }
            }
            catch 
            {
                return true;
            }
           return false;
        }
        #endregion

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
        //    if (DtGrid.SelectedIndex != -1)
        //    {
        //        //ComfirmBox deleComfir = new ComfirmBox();
        //        //deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
        //        //deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
        //        //deleComfir.ButtonOK.Click += new RoutedEventHandler(ButtonOK1_Click);
        //        //deleComfir.Show();

        //        string Result = "";
        //        if (DtGrid.SelectedItems.Count > 0)
        //        {
        //            ObservableCollection<string> ids = new ObservableCollection<string>();

        //            foreach (V_CUSTOMGUERDON tmp in DtGrid.SelectedItems)
        //            {
        //                ids.Add(tmp.CUSTOMGUERDONID);
        //            }

        //            ComfirmWindow com = new ComfirmWindow();
        //            com.OnSelectionBoxClosed += (obj, result) =>
        //            {
        //                client.CustomGuerdonDeleteAsync(ids);
        //            };
        //            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
        //        }

        //    }
        //    else 
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
        //    }
        }

    }
}
