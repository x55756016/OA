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
using System.Windows.Data;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.SalaryWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalarySystemForm : System.Windows.Controls.Window, IEntityEditor, IClient
    {
        SMTLoading loadbar = new SMTLoading();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private SalaryServiceClient client = new SalaryServiceClient();
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient permissionClient = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();

        //private ObservableCollection<T_SYS_DICTIONARY> salaryLevelDicts;
        //private ObservableCollection<T_HR_SALARYLEVEL> salaryLevels;
        //private ObservableCollection<T_SYS_DICTIONARY> postLevledicts;
        private List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> salaryLevelDicts;
        private List<T_HR_SALARYLEVEL> salaryLevels;
        private List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> postLevledicts;
        private List<T_HR_POSTLEVELDISTINCTION> postLevelList;
        private string systemID { get; set; }
        private string flag = "1";
        public SalarySystemForm(string systemid)
        {
            InitializeComponent();
            systemID = systemid;
            this.TitleContent = Utility.GetResourceStr("SALARYSYSTEMTABEL");
            //this.MinWidth = 800;
            //this.MinHeight = 600;
            InitParas();
        }
        public SalarySystemForm()
        {
            InitializeComponent();
            this.TitleContent = Utility.GetResourceStr("SALARYSYSTEMTABEL");
            this.MinWidth = 800;
            this.MinHeight = 600;
        }
        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            permissionClient.GetSysDictionaryByCategoryCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs>(permissionClient_GetSysDictionaryByCategoryCompleted);
            //client.GetAllSalaryLevelCompleted += new EventHandler<GetAllSalaryLevelCompletedEventArgs>(client_GetAllSalaryLevelCompleted);
            client.GetSalaryLevelBySystemIDCompleted += new EventHandler<GetSalaryLevelBySystemIDCompletedEventArgs>(client_GetSalaryLevelBySystemIDCompleted);
            client.GenerateSalaryLevelCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_GenerateSalaryLevelCompleted);
            //  client.GetAllPostLevelDistinctionCompleted += new EventHandler<GetAllPostLevelDistinctionCompletedEventArgs>(client_GetAllPostLevelDistinctionCompleted);
            client.GetPostLevelDistinctionBySystemIDCompleted += new EventHandler<GetPostLevelDistinctionBySystemIDCompletedEventArgs>(client_GetPostLevelDistinctionBySystemIDCompleted);

            #region 设置当前加载DtGriddy样式
            DtGrid.Style = Application.Current.Resources["DataGridStyle"] as Style;
            //DtGrid.CellStyle = Application.Current.Resources["DataGridCellStyle"] as Style;
            //DtGrid.RowHeaderStyle = Application.Current.Resources["DataGridRowHeaderStyle"] as Style;
            //DtGrid.RowStyle = Application.Current.Resources["DataGridRowStyle"] as Style;
            //DtGrid.ColumnHeaderStyle = Application.Current.Resources["DataGridColumnHeaderStyle"] as Style;
            DtGrid.ColumnHeaderHeight = 50;//\n\r
            #endregion

            //获取字典中的岗位级别
            //   permissionClient.GetSysDictionaryByCategoryAsync("SALARYLEVEL");
            //   client.GetAllSalaryLevelAsync();
            //ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            //ToolBar.btnNew.Visibility = Visibility.Collapsed;


            //ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            //ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            //ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            //ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            //ToolBar.retAudit.Visibility = Visibility.Collapsed;
            //ToolBar.retDelete.Visibility = Visibility.Collapsed;
            //ToolBar.BtnView.Visibility = Visibility.Collapsed;

        }



        void client_GetPostLevelDistinctionBySystemIDCompleted(object sender, GetPostLevelDistinctionBySystemIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    postLevelList = e.Result.ToList();
                    FillData();
                }

            }
        }

        //void client_GetAllPostLevelDistinctionCompleted(object sender, GetAllPostLevelDistinctionCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            postLevelList = e.Result.ToList();
        //            FillData();
        //        }

        //    }
        //}
        # region 完成事件
        /// <summary>
        /// 获取所有的岗位薪资
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetSalaryLevelBySystemIDCompleted(object sender, GetSalaryLevelBySystemIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    salaryLevels = e.Result.ToList();
                    // FillData();
                    client.GetPostLevelDistinctionBySystemIDAsync(systemID);
                }
                else
                {
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.Close();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("你还没有生成薪资体系"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("你还没有生成薪资体系"));
                }
            }
        }
        //void client_GetAllSalaryLevelCompleted(object sender, GetAllSalaryLevelCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        //if (e.Result != null)
        //        //{
        //        salaryLevels = e.Result.ToList();
        //        //    FillData();
        //        //}

        //        client.GetAllPostLevelDistinctionAsync();
        //    }
        //   // loadbar.Stop();
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GenerateSalaryLevelCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYLEVEL"));
                //  LoadData();
            }
            loadbar.Stop();

        }
        /// <summary>
        /// 获取岗位级别和薪资级别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void permissionClient_GetSysDictionaryByCategoryCompleted(object sender, SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {

                    if (flag == "1")
                    {
                        salaryLevelDicts = e.Result.OrderBy(c => c.DICTIONARYVALUE).ToList();
                        DtGrid.ItemsSource = salaryLevelDicts;
                        flag = "2";
                        permissionClient.GetSysDictionaryByCategoryAsync("POSTLEVEL");
                    }
                    else
                    {
                        postLevledicts = e.Result.OrderBy(b => b.DICTIONARYNAME).ToList();
                        client.GetSalaryLevelBySystemIDAsync(systemID);
                    }
                }
            }

        }
        #endregion
        void LoadPost()
        {
            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY AA in postLevledicts)
            {
                var ents = (from c in postLevelList
                            where c.POSTLEVEL == AA.DICTIONARYVALUE
                            select c).ToList().FirstOrDefault();
                try
                {
                    string dist = ents.LEVELBALANCE.ToString();
                    DataGridTextColumn txtCol = new DataGridTextColumn();
                    txtCol.Header = "级差额" + dist + "\n" + AA.DICTIONARYNAME;
                    txtCol.Binding = new Binding("SALARYSUM");


                    //txtCol.Width = DataGridLength.SizeToCells;
                    //txtCol.MinWidth = 100;
                    //txtCol.MaxWidth = 100;
                    DtGrid.Columns.Add(txtCol);
                }
                catch { }
            }
        }
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void DtGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //loadbar.Start();
            //flag = "1";
            //permissionClient.GetSysDictionaryByCategoryAsync("SALARYLEVEL");

        }

        void FillData()
        {


            if (postLevledicts == null)
            {
                loadbar.Stop();
                return;
            }
            else if (salaryLevelDicts == null)
            {
                loadbar.Stop();
                return;
            }
            else if (salaryLevels == null)
            {
                loadbar.Stop();
                return;
            }
            else
            {
                // salaryLevelDicts = salaryLevelDicts.OrderBy(c=>c.DICTIONARYVALUE).ToList();
                DtGrid.ItemsSource = salaryLevelDicts;
                LoadPost();
                foreach (object obj in DtGrid.ItemsSource)
                {
                    string salarylevel = (DtGrid.Columns[0].GetCellContent(obj).DataContext as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
                    for (int i = 1; i < DtGrid.Columns.Count; i++)
                    {
                        int postlevel = postLevledicts[i - 1].DICTIONARYVALUE.ToInt32();
                        var ent = from c in salaryLevels
                                  join b in salaryLevelDicts on c.SALARYLEVEL equals b.DICTIONARYVALUE.ToString()
                                  //     join d in postLevleDistinct on c.T_HR_POSTLEVELDISTINCTION.POSTLEVEL equals  d.POSTLEVELID
                                  where c.T_HR_POSTLEVELDISTINCTION.POSTLEVEL == postlevel && c.SALARYLEVEL == salarylevel
                                  select c;
                        if (ent.Count() > 0)
                        {
                            DtGrid.Columns[i].GetCellContent(obj).DataContext = ent.ToList().First();
                        }
                        //  DtGrid.Columns[i].GetCellContent(obj).MouseLeftButtonDown += new MouseButtonEventHandler(Salarysystem_MouseLeftButtonDown);
                    }
                }
            }
            loadbar.Stop();
        }

        void Salarysystem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr((sender as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString()),
                                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //MessageBox.Show((sender as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString());
        }


        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            loadbar.Start();
            flag = "1";
            permissionClient.GetSysDictionaryByCategoryAsync("SALARYLEVEL");
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYSYSTEMTABEL");
        }
        public string GetStatus()
        {
            return "";// CustomGuerdonSet != null ? CustomGuerdonSet.CREATECOMPANYID : "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    //Save();
                    break;
                case "1":
                    // Cancel();
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
            //items.Add(item); 
            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
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
        #endregion

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
