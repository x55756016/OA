/// <summary>
/// Log No.： 1
/// Modify Desc： 等待控制，审核状态
/// Modifier： 冉龙军
/// Modify Date： 2010-08-09
/// </summary>
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.Saas.Tools.PerformanceWS;
using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.Form.Performance;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PersonnelWS;

namespace SMT.HRM.UI.Views.Performance
{
    public partial class RandomGroup : BasePage
    {
        private PerformanceServiceClient client = new PerformanceServiceClient();  //绩效考核服务
        SMTLoading loadbar = new SMTLoading();
        public T_HR_RANDOMGROUP SelectedRandomGroup { get; set; }                  //当前所选抽查组
        public string groupPersonIDs = string.Empty;                               //当前所选抽查组的人员ID串
        public ObservableCollection<T_HR_RANDOMGROUP> RandomGroups = new ObservableCollection<T_HR_RANDOMGROUP>();  //抽查组列表

        public RandomGroup()
        {
            InitializeComponent();
            InitPara();
            GetEntityLogo("T_HR_RANDOMGROUP");
        }

        /// <summary>
        /// 读取页面事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RandomGroup_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            // 1s 冉龙军
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_RANDOMGROUP", false);
            // 1e
        }

        /// <summary>
        /// 当用户导航到此页面时执行
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // 1s 冉龙军
            //Utility.DisplayGridToolBarButton(ToolBar, "RandomGroup", false);
            // 1e
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        /// <summary>
        /// 初始化事件
        /// </summary>
        public void InitPara()
        {
            try
            {
                // 1s 冉龙军
                //LayoutRoot.Children.Add(loadbar);
                //loadbar.Start();
                PARENT.Children.Add(loadbar);
                // 1e
                client.GetRandomGroupPagingCompleted += new EventHandler<GetRandomGroupPagingCompletedEventArgs>(client_GetRandomGroupPagingCompleted);
                client.DeleteRandomGroupCompleted += new EventHandler<DeleteRandomGroupCompletedEventArgs>(client_DeleteRandomGroupCompleted);
                client.DeleteRandomPersonsCompleted += new EventHandler<DeleteRandomPersonsCompletedEventArgs>(client_DeleteRandomPersonsCompleted);
                client.DeleteRandomGroupsCompleted += new EventHandler<DeleteRandomGroupsCompletedEventArgs>(client_DeleteRandomGroupsCompleted);
                client.GetRandomGroupPersonPagingCompleted += new EventHandler<GetRandomGroupPersonPagingCompletedEventArgs>(client_GetRandomGroupPersonPagingCompleted);
                this.Loaded += new RoutedEventHandler(RandomGroup_Loaded);
                this.ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
                this.ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                this.ToolBar.btnDelete.Click += new RoutedEventHandler(btnDel_Click);
                this.ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                this.ToolBar.BtnView.Click += new RoutedEventHandler(btnView_Click);
                // 1s 冉龙军
                //this.ToolBar.retNew.Visibility = Visibility.Collapsed;
                // 1e
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }





        #region 抽查组相关

        /// <summary>
        /// 刷新抽查组列表
        /// </summary>
        void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtRandomGroupName = Utility.FindChildControl<TextBox>(expander, "txtRandomGroupName");
            if (!string.IsNullOrEmpty(txtRandomGroupName.Text.Trim()))
            {
                // filter += "RANDOMGROUPNAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(RANDOMGROUPNAME)";
                paras.Add(txtRandomGroupName.Text.Trim());
            }

            client.GetRandomGroupPagingAsync(dataPager.PageIndex, dataPager.PageSize, "RANDOMGROUPID", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            // 1s 冉龙军
            loadbar.Start();
            // 1e
        }

        /// <summary>
        /// 获取抽查组当前页后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetRandomGroupPagingCompleted(object sender, GetRandomGroupPagingCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                RandomGroups = e.Result;
                DtGrid.ItemsSource = RandomGroups;
                dataPager.PageCount = e.pageCount;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }
        /// <summary>
        /// 删除抽查组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DeleteRandomGroupsCompleted(object sender, DeleteRandomGroupsCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }
        /// <summary>
        /// 删除当前抽查组后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DeleteRandomGroupCompleted(object sender, DeleteRandomGroupCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }

        /// <summary>
        /// 添加抽查组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            GroupInfo form = new GroupInfo(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 430;
            form.MinHeight = 180;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

            //LoadData();
        }

        /// <summary>
        /// 查看抽查组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRandomGroup != null)
            {
                GroupInfo form = new GroupInfo(FormTypes.Browse, SelectedRandomGroup.RANDOMGROUPID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 430;
                form.MinHeight = 180;
                // 1s 冉龙军
                browser.FormType = FormTypes.Browse;
                // 1e
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 修改抽查组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRandomGroup != null)
            {
                GroupInfo form = new GroupInfo(FormTypes.Edit, SelectedRandomGroup.RANDOMGROUPID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 430;
                form.MinHeight = 180;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 删除抽查组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();

                    foreach (T_HR_RANDOMGROUP tmp in DtGrid.SelectedItems)
                    {
                        if (tmp.T_HR_RAMDONGROUPPERSON.Count > 0)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("M00016"));
                            return;
                        }
                        ids.Add(tmp.RANDOMGROUPID);
                    }
                    client.DeleteRandomGroupsAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 刷新抽查组列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            LoadData();
        }

        /// <summary>
        /// 查询抽查组信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 加载抽查组列表行事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_RANDOMGROUP");
        }

        /// <summary>
        /// 翻页控件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 抽查组列表换行事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DtGrid.SelectedItem != null)
            {
                SelectedRandomGroup = (T_HR_RANDOMGROUP)DtGrid.SelectedItems[0];
            }
            LoadGroupPersonData();
        }

        #endregion 抽查组相关

        #region 抽查组人员相关

        /// <summary>
        /// 刷新抽查组人员
        /// </summary>
        private void RefreshGroupPersonData()
        {
            DtGrid_SelectionChanged(null, null);
        }

        /// <summary>
        /// 读取抽查组人员列表
        /// </summary>
        private void LoadGroupPersonData()
        {
            int pageCount = 0;
            string filter = string.Empty;
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            client.GetRandomGroupPersonPagingAsync(personPager.PageIndex, personPager.PageSize, "EMPLOYEECNAME", filter, paras, pageCount, SelectedRandomGroup.RANDOMGROUPID);
        }

        /// <summary>
        /// 获取所有抽查组人员后事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetRandomGroupPersonPagingCompleted(object sender, GetRandomGroupPersonPagingCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {

                dtgPerson.ItemsSource = e.Result;
                personPager.PageCount = e.pageCount;
            }
        }

        /// <summary>
        /// 删除抽查组人员后事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DeleteRandomPersonsCompleted(object sender, DeleteRandomPersonsCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "RANDOMGROUPPERSON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

                // LoadData();
                LoadGroupPersonData();
            }
        }

        /// <summary>
        /// 翻页控件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void personPager_Click(object sender, RoutedEventArgs e)
        {
            LoadGroupPersonData();
        }

        /// <summary>
        /// 添加抽查组人员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddPerson_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                EditGroupPerson form = new EditGroupPerson(FormTypes.New, SelectedRandomGroup);
                EntityBrowser browser = new EntityBrowser(form);
                form.Height = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);//RefreshGroupPersonData);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                LoadGroupPersonData();
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTFIRST", "RANDOMGROUP"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 删除抽查组人员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelPerson_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string Result = "";
            if (dtgPerson.ItemsSource != null)
            {
                if (dtgPerson.SelectedItems != null && dtgPerson.SelectedItems.Count > 0)
                {
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        ObservableCollection<string> delListId = new ObservableCollection<string>();
                        foreach (SMT.Saas.Tools.PerformanceWS.V_EMPLOYEEVIEW ent in dtgPerson.SelectedItems)
                        {
                            //foreach (T_HR_RAMDONGROUPPERSON person in GroupPersons)
                            //{
                            //    if (person.PERSONID.Equals(ent.EMPLOYEEID))
                            //    {
                            //        delListId.Add(person.GROUPPERSONID);
                            //        GroupPersons.Remove(person);
                            //        break;
                            //    }
                            //}
                            delListId.Add(ent.EMPLOYEEID);
                        }
                        client.DeleteRandomPersonsAsync(delListId, SelectedRandomGroup.RANDOMGROUPID);
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
        }


        #endregion 抽查组人员相关

        private void dtgPerson_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dtgPerson, e.Row, "T_HR_EMPLOYEE");
        }
    }
}
