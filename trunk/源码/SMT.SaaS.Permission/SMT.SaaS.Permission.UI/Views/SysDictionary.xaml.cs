using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.Common;
using System.IO;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysDictionary : BasePage
    {
        private static PermissionServiceClient client = new PermissionServiceClient();//龙康才新增
        //  private PermissionServiceClient client = new PermissionServiceClient();
        List<T_SYS_DICTIONARY> SysDictionaryCategoryList = new List<T_SYS_DICTIONARY>();
        List<T_SYS_DICTIONARY> sysTypeList = new List<T_SYS_DICTIONARY>();
        ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
        private SMTLoading loadbar = new SMTLoading();
        public SysDictionary()
        {
            InitializeComponent();
            //Utility.DisplayGridToolBarButton(FormToolBar1, "OAMEETINGROOMAPP", true);
            this.Loaded += new RoutedEventHandler(SysDictionary_Loaded);
            //client.GetSysDictionaryByCategoryByUpdateDate
            client.GetSysDictionaryByCategoryByUpdateDateAsync("",DateTime.Now);
        }

        void SysDictionary_Loaded(object sender, RoutedEventArgs e)
        {
            PARENT.Children.Add(loadbar);
            InitPara();
            GetEntityLogo("T_SYS_DICTIONARY");
            //LookUp lkDictioanryType = Utility.FindChildControl<LookUp>(expander, "lkDictionaryType");
            //lkDictioanryType.TxtLookUp.IsEnabled = true;
            LoadData();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        void InitPara()
        {
            client.GetSysDictionaryCategoryCompleted += new EventHandler<GetSysDictionaryCategoryCompletedEventArgs>(client_GetSysDictionaryCategoryCompleted);
            client.GetSysDictionaryByFilterPagingCompleted += new EventHandler<GetSysDictionaryByFilterPagingCompletedEventArgs>(client_GetSysDictionaryByFilterPagingCompleted);
            // client.GetSysDictionarySysTypeCompleted += new EventHandler<GetSysDictionarySysTypeCompletedEventArgs>(client_GetSysDictionarySysTypeCompleted);
            client.SysDictionaryDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SysDictionaryDeleteCompleted);
            
            client.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(client_GetSysDictionaryByCategoryCompleted);

            client.ImportCityCSVCompleted +=new EventHandler<ImportCityCSVCompletedEventArgs>(client_ImportCityCSVCompleted);

            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);

            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            FormToolBar1.cbxCheckState.Visibility = Visibility.Collapsed;
            FormToolBar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
            FormToolBar1.retAudit.Visibility = Visibility.Collapsed;
            //   FormToolBar1.BtnView.Visibility = Visibility.Collapsed;
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);

            #region 自定义导入按钮
            ImageButton ChangeMeetingBtn = new ImageButton();
            ChangeMeetingBtn.Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_import.png", UriKind.Relative));
            ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("批量导入城市字典");// 考勤备案
            ChangeMeetingBtn.Image.Width = 16.0;
            ChangeMeetingBtn.Image.Height = 22.0;
            ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            ChangeMeetingBtn.Click += new RoutedEventHandler(btnImport_Click);
            FormToolBar1.stpOtherAction.Children.Add(ChangeMeetingBtn);
            #endregion

            FormToolBar1.ShowRect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_ImportCityCSVCompleted(object sender, ImportCityCSVCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    loadbar.Stop();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), e.Error.Message, Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                //bool flag = e.Result;
               // RefreshUI(RefreshedTypes.HideProgressBar);
                string strTemp = e.strMsg;
                bool flag = e.Result;
                if (flag)
                {
                     ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(strTemp), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                loadbar.Stop();
            }
            catch (Exception ex)
            {
                loadbar.Stop();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        /// <summary>
        /// 导入城市
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "csv Files (*.csv)|*.csv;";
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            Stream Stream = (System.IO.Stream)openFileDialog.File.OpenRead();
            byte[] Buffer = new byte[Stream.Length];
            Stream.Read(Buffer, 0, (int)Stream.Length);
            Stream.Dispose();
            Stream.Close();

            UploadFileModel UploadFile = new UploadFileModel();
            UploadFile.FileName = openFileDialog.File.Name;
            UploadFile.File = Buffer;
            Dictionary<string, string> empInfo = new Dictionary<string, string>();
            empInfo.Add("ownerID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            client.ImportCityCSVAsync(UploadFile, empInfo, string.Empty);
             loadbar.Start();
        }

        void client_SysDictionaryDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DELETESUCCESS"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                LoadData();
            }
        }

        void client_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                sysTypeList = e.Result.ToList();
                T_SYS_DICTIONARY temp = new T_SYS_DICTIONARY();
                // temp.DICTIONARYNAME = "ALL";
                temp.DICTIONARYNAME = Utility.GetResourceStr("ALL");
                sysTypeList.Insert(0, temp);


                ComboBox cbSysType = Utility.FindChildControl<ComboBox>(expander, "cbSysType");
                cbSysType.ItemsSource = sysTypeList;
                cbSysType.SelectedIndex = 0;
            }
        }

        void client_GetSysDictionaryByFilterPagingCompleted(object sender, GetSysDictionaryByFilterPagingCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    try
                    {
                        if (e.Result != null)
                        {

                            BindDataGrid(e.Result.ToList(), e.pageCount);
                        }
                        else
                        {
                            BindDataGrid(null, 0);
                        }
                    }
                    catch (Exception ex)
                    {                        
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }

            loadbar.Stop();
        }

        //void client_GetSysDictionarySysTypeCompleted(object sender, GetSysDictionarySysTypeCompletedEventArgs e)
        //{
        //    if (e.Result != null)
        //    {
        //        sysTypeList = e.Result.ToList();
        //        T_SYS_DICTIONARY temp = new T_SYS_DICTIONARY();
        //        temp.SYSTEMCODE = "ALL";
        //        temp.SYSTEMNAME = Utility.GetResourceStr("ALL");
        //        sysTypeList.Insert(0, temp);


        //        ComboBox cbSysType = Utility.FindChildControl<ComboBox>(expander, "cbSysType");
        //        cbSysType.ItemsSource = sysTypeList;
        //        cbSysType.SelectedIndex = 0;
        //    }
        //}

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void client_GetSysDictionaryByFilterCompleted(object sender, GetSysDictionaryByFilterCompletedEventArgs e)
        {

        }


        //  绑定DataGird
        private void BindDataGrid(List<T_SYS_DICTIONARY> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                DtGrid.ItemsSource = null;
                return;
            }
            DtGrid.ItemsSource = obj;
        }


        void client_GetSysDictionaryCategoryCompleted(object sender, GetSysDictionaryCategoryCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                SysDictionaryCategoryList = e.Result.ToList();
                T_SYS_DICTIONARY temp = new T_SYS_DICTIONARY();
                temp.DICTIONCATEGORY = "ALL";
                temp.DICTIONCATEGORYNAME = Utility.GetResourceStr("ALL");
                SysDictionaryCategoryList.Insert(0, temp);

                ComboBox cbDictionay = Utility.FindChildControl<ComboBox>(expander, "cbDictionay");
                cbDictionay.ItemsSource = SysDictionaryCategoryList;
                cbDictionay.SelectedIndex = 0;
            }
        }

        //void client_GetSysDictionaryByCategoryCompleted(object sender, SMT.SaaS.Permission.UI.PermissionService.GetSysDictionaryByCategoryCompletedEventArgs e)
        //{
        //    List<PermissionService.T_SYS_DICTIONARY> listDictionary = new List<PermissionService.T_SYS_DICTIONARY>();
        //    PagedCollectionView pcv = null;
        //    if (e.Result != null)
        //    {
        //        listDictionary = e.Result.ToList();
        //        var q = from ent in listDictionary
        //                select ent;

        //        pcv = new PagedCollectionView(q);
        //        pcv.PageSize = 25;
        //    }
        //    dataPager.DataContext = pcv;
        //    DtGrid.ItemsSource = pcv;

        //    HidePageStyle();
        //}
        void LoadData()
        {
            string filter = " 1=1 ";
            int pageCount = 0;


            //ComboBox cbDictionay = Utility.FindChildControl<ComboBox>(expander, "cbDictionay");
            //if (cbDictionay.SelectedIndex > 0)
            //{
            //    string catogry = ((T_SYS_DICTIONARY)cbDictionay.SelectedItem).DICTIONCATEGORY;
            //    filter += " && DICTIONCATEGORY==@" + paras.Count().ToString();
            //    paras.Add(catogry);

            //}

            ComboBox cbSysType = Utility.FindChildControl<ComboBox>(expander, "cbSysType");
            if (cbSysType.SelectedIndex > 0)
            {

                //filter += " && SYSTEMCODE==@" + paras.Count().ToString();
                //paras.Add((cbSysType.SelectedItem as T_SYS_DICTIONARY).SYSTEMCODE);
                filter += " && SYSTEMNAME==@" + paras.Count().ToString();
                paras.Add((cbSysType.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME);
            }
            LookUp lkDictioanryType = Utility.FindChildControl<LookUp>(expander, "lkDictionaryType");
            if ((lkDictioanryType.DataContext as T_SYS_DICTIONARY)!=null)
            {
                filter += " && DICTIONCATEGORY==@" + paras.Count().ToString();
                paras.Add((lkDictioanryType.DataContext as  T_SYS_DICTIONARY ).DICTIONCATEGORY);
            }

            TextBox txtSearch = Utility.FindChildControl<TextBox>(expander, "txtSearch");
            if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
               // filter += "  && DICTIONARYNAME==@" + paras.Count().ToString();
              //  filter += "&& @" + paras.Count().ToString() + ".Contains(DICTIONARYNAME)";
                filter += "&& DICTIONARYNAME.Contains(@" + paras.Count().ToString() + ")";
                paras.Add(txtSearch.Text.Trim());
            }
            loadbar.Start();
            //SMT.Saas.Tools.PermissionWS.LoginUserInfo loginUserInfo = new SMT.Saas.Tools.PermissionWS.LoginUserInfo();
            //loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;

            client.GetSysDictionaryByFilterPagingAsync(dataPager.PageIndex, dataPager.PageSize, "DICTIONCATEGORY", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            dataPager.PageIndex = 0;
            LoadData();
        }

        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        //private void cbDictionay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    LoadData();
        //}


        #region 添加 修改 删除

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count != 0)
            {
                Form.SysDictionaryForm form = new Form.SysDictionaryForm(FormTypes.Browse, (DtGrid.SelectedItems[0] as T_SYS_DICTIONARY).DICTIONARYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 310;
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.EntityEditor = form;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请选择查看记录", Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> IDs = new ObservableCollection<string>();
                    foreach (T_SYS_DICTIONARY item in DtGrid.SelectedItems)
                    {
                        IDs.Add(item.DICTIONARYID);
                    }
                    client.SysDictionaryDeleteAsync(IDs);

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请选择需要删除的记录", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count <= 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            Form.SysDictionaryForm editForm = new SMT.SaaS.Permission.UI.Form.SysDictionaryForm(FormTypes.Edit, (DtGrid.SelectedItems[0] as T_SYS_DICTIONARY).DICTIONARYID);
            EntityBrowser browser = new EntityBrowser(editForm);
            browser.MinHeight = 310;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        /// <summary>
        /// 查看
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void BtnView_Click(object sender, RoutedEventArgs e)
        //{
        //    if (DtGrid.SelectedItems.Count <= 0)
        //    {
        //        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
        //        return;
        //    }

        //    Form.SysDictionaryForm editForm = new SMT.SaaS.Permission.UI.Form.SysDictionaryForm(FormTypes.Edit, (DtGrid.SelectedItems[0] as T_SYS_DICTIONARY).DICTIONARYID);
        //    EntityBrowser browser = new EntityBrowser(editForm);
        //    browser.MinHeight = 310;
        //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        //}
        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.SysDictionaryForm editForm = new SMT.SaaS.Permission.UI.Form.SysDictionaryForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(editForm);
            browser.MinHeight = 310;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        # endregion

        private void cbDictionay_Loaded(object sender, RoutedEventArgs e)
        {
            client.GetSysDictionaryCategoryAsync();
        }

        //private void cbSysType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ComboBox cbSysType = Utility.FindChildControl<ComboBox>(expander, "cbSysType");
        //    ComboBox cbDictionay = Utility.FindChildControl<ComboBox>(expander, "cbDictionay");
        //    if (cbSysType.SelectedIndex > 0)
        //    {
        //        string sysCode = (cbSysType.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME;
        //        var ent = from a in SysDictionaryCategoryList
        //                  where a.SYSTEMNAME == sysCode
        //                  select a;

        //        if (ent.Count() > 0)
        //        {
        //            List<T_SYS_DICTIONARY> ListDict = new List<T_SYS_DICTIONARY>();
        //            T_SYS_DICTIONARY temp = new T_SYS_DICTIONARY();
        //            temp.DICTIONCATEGORY = "ALL";
        //            temp.DICTIONCATEGORYNAME = Utility.GetResourceStr("ALL");
        //            ListDict = ent.ToList();
        //            ListDict.Insert(0, temp);
        //            cbDictionay.ItemsSource = ListDict;
        //            cbDictionay.SelectedIndex = 0;
        //        }
        //        else
        //        {
        //            cbDictionay.ItemsSource = null;
        //        }
        //    }
        //    else
        //    {
        //        cbDictionay.ItemsSource = SysDictionaryCategoryList;
        //    }
        //    LoadData();
        //}

        private void cbSysType_Loaded(object sender, RoutedEventArgs e)
        {
            client.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_DICTIONARY");
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void lkDictionaryType_FindClick(object sender, EventArgs e)
        {
            LookUp lkDictionaryType = sender as LookUp;
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("SysDictionary", "2");
            string[] cols = { "DICTIONCATEGORY", "DICTIONCATEGORYNAME" };

            LookupForm lookups = new LookupForm(EntityNames.SysDictionary,
                typeof(List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>), cols, para);
            lookups.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY ent = lookups.SelectedObj as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;

                //if (ent != null)
                //{
                    lkDictionaryType.DataContext = ent;

                //}
            };
            lookups.Show();
        }

    }
}
