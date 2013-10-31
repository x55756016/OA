using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysMenu : BasePage
    {
        private static PermissionServiceClient client = new PermissionServiceClient();//龙康才新增
        //PermissionServiceClient client = new PermissionServiceClient();
        ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
        private SMTLoading loadbar = new SMTLoading();
        DictionaryManager DictManager = new DictionaryManager();
        List<string> ListDict = new List<string>(); //字典列表
        public SysMenu()
        {
            //if (Application.Current.Resources["SYS_DICTIONARY"] == null)
            //    LoadDicts();
            //Utility.DisplayGridToolBarButton(FormToolBar1, "SYSMENUMANAGEMENT", true);
            InitializeComponent();
            PARENT.Children.Add(loadbar);
            InitControlEvent();
            ListDict.Add("SYSTEMTYPE");//系统类型
            
            this.Loaded += new RoutedEventHandler(SysMenu_Loaded);
        }

        void DictManager_OnDictionaryLoadCompleted(object sender, OnDictionaryLoadArgs e)
        {
            if (e.Error == null && e.Result)
            {
                InitFormToolBarButton();
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "字典加载错误，请联系管理员",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        #region 加载事件显示当前所需按钮
        void SysMenu_Loaded(object sender, RoutedEventArgs e)
        {
            DictManager.LoadDictionary(ListDict);

            
        }
        /// <summary>
        /// formtoolbar控件控制
        /// </summary>
        private void InitFormToolBarButton()
        {
            GetEntityLogo("T_SYS_ENTITYMENU");
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            //隐藏未使用按钮
            //FormToolBar1.btnRead.Visibility = Visibility.Collapsed;
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;
            FormToolBar1.BtnView.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnimport.Visibility = Visibility.Collapsed;
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;

            //FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DataGrid_CurrentCellChanged);
            if (Application.Current.Resources["SYS_DICTIONARY"] == null)
            {
                client.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
            }
            else
            {
                InitSysType();//如果字典加载了，则过滤了再帮定
            }
            FormToolBar1.ShowRect();
            LoadData();
        }
        #endregion

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Menu != null)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.SysMenuDeleteAsync(Menu.ENTITYMENUID);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"),"请选择需要删除的记录",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Menu == null)
            {
                
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

            Form.SysMenuForms editForm = new SMT.SaaS.Permission.UI.Form.SysMenuForms(FormTypes.Edit, Menu.ENTITYMENUID);
            EntityBrowser browser = new EntityBrowser(editForm);
            browser.MinHeight = 480;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        void browser_ReloadDataEvent()
        {
            Menu = null;//释放选中的菜单
            LoadData();
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.SysMenuForms editForm = new SMT.SaaS.Permission.UI.Form.SysMenuForms(FormTypes.New);
            EntityBrowser browser = new EntityBrowser(editForm);
            browser.MinHeight = 480;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        protected void LoadDicts()
        {
            client.GetSysDictionaryByCategoryCompleted += (o, e) =>
            {
                List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
                dicts = e.Result == null ? null : e.Result.ToList();
                Application.Current.Resources.Add("SYS_DICTIONARY", dicts);

                LoadData();
            };
            //TODO: 按需取出字典值
            client.GetSysDictionaryByCategoryAsync("");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString()); 
        } 

        private void InitControlEvent()
        {
            


            client.GetSysMenuByTypeCompleted += new EventHandler<GetSysMenuByTypeCompletedEventArgs>(ServiceClient_GetSysMenuByTypeCompleted);
            client.SysMenuDeleteCompleted += new EventHandler<SysMenuDeleteCompletedEventArgs>(client_SysMenuDeleteCompleted);
            //client.SysMenuDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysMenuDeleteCompleted);
            client.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(ServiceClient_GetSysDictionaryByCategoryCompleted);
            client.GetSysMenuByTypePagingCompleted += new EventHandler<GetSysMenuByTypePagingCompletedEventArgs>(client_GetSysMenuByTypePagingCompleted);
            DictManager.OnDictionaryLoadCompleted += new EventHandler<OnDictionaryLoadArgs>(DictManager_OnDictionaryLoadCompleted);
            //绑定系统类型
            
        }

        void client_SysMenuDeleteCompleted(object sender, SysMenuDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == "")
                {
                    //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETESUCCESSEDCONFIRM"), Utility.GetResourceStr("CONFIRMBUTTON"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DELETESUCCESSEDCONFIRM"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //刷新数据
                    ShowPageStyle();
                    Menu = null;
                    LoadData();
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
        }

        void client_GetSysMenuByTypePagingCompleted(object sender, GetSysMenuByTypePagingCompletedEventArgs e)
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
                        //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            
            loadbar.Stop();
        }

        //  绑定DataGird
        private void BindDataGrid(List<T_SYS_ENTITYMENU> obj, int pageCount)
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

        //void ServiceClient_SysMenuDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
        //    }
        //    else
        //    {
        //        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETESUCCESSEDCONFIRM"), Utility.GetResourceStr("CONFIRMBUTTON"));
        //        //刷新数据
        //        ShowPageStyle();

        //        LoadData();
        //    }
        //}

        void ServiceClient_GetSysMenuByTypeCompleted(object sender, GetSysMenuByTypeCompletedEventArgs e)
        {
            PagedCollectionView pcv = null;
            if (e.Result != null)
            {
                List<T_SYS_ENTITYMENU> menulist = e.Result.ToList();
                var q = from ent in menulist
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 25;
            }

            dataPager.DataContext = pcv;
            DtGrid.ItemsSource = pcv;
            loadbar.Stop();
            HidePageStyle();
        }

        /// <summary>
        /// 加载菜单数据
        /// </summary>
        private void LoadData()
        {
            string filter = " 1=1 ";
            int pageCount = 0;
            //from a in DataContext.T_SYS_ENTITYMENU.Include("T_SYS_ENTITYMENU2")
            //           where (string.IsNullOrEmpty(sysType) || a.SYSTEMTYPE == sysType)
            //           && (string.IsNullOrEmpty(parentID) || ( a.T_SYS_ENTITYMENU2!=null && a.T_SYS_ENTITYMENU2.ENTITYMENUID == parentID) )
            //           orderby a.ORDERNUMBER
            //           select a;
            

            ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(expander, "cbxSystemType");
            T_SYS_DICTIONARY dict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;
            string systype = dict == null ? "" : dict.DICTIONARYVALUE.GetValueOrDefault().ToString();

            string parentid = "";
            //string StrName = "";//菜单名称
            
            
            LookUp lkParentMenu = Utility.FindChildControl<LookUp>(expander, "lkParentMenu");
            T_SYS_ENTITYMENU menu = lkParentMenu.DataContext as T_SYS_ENTITYMENU;
            if(!string.IsNullOrEmpty(systype))
            {
                
                filter += " && SYSTEMTYPE ==@" + paras.Count().ToString();
                paras.Add(systype);
            }
            if (menu != null)
            {
                parentid = menu.ENTITYMENUID;
                if(!string.IsNullOrEmpty(parentid))
                {
                    
                    filter += " && T_SYS_ENTITYMENU2!=null && T_SYS_ENTITYMENU2.ENTITYMENUID ==@" + paras.Count().ToString();
                    paras.Add(parentid);
                }
            }
            SMT.Saas.Tools.PermissionWS.LoginUserInfo loginUserInfo = new SMT.Saas.Tools.PermissionWS.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loadbar.Start();
            client.GetSysMenuByTypePagingAsync(dataPager.PageIndex, dataPager.PageSize, "ORDERNUMBER", filter, paras, pageCount, loginUserInfo);
            //client.GetSysMenuByTypeAsync(systype, parentid);
        }

        void ServiceClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            //绑定系统类型
            if (e.Result != null)
            {
                ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(expander, "cbxSystemType");
                List<T_SYS_DICTIONARY> dicts = e.Result.ToList();

                cbxSystemType.ItemsSource = dicts;
                cbxSystemType.DisplayMemberPath = "DICTIONARYNAME";   
            }
        }

        private void InitSysType()
        {
            if (Application.Current.Resources["SYS_DICTIONARY"] != null)
            {
                ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(expander, "cbxSystemType");
                List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
                var ents = from ent in dicts
                           where ent.DICTIONCATEGORY == "SYSTEMTYPE"
                           select ent;
                if (ents != null)
                {
                    if (ents.Count() > 0)
                    {
                        cbxSystemType.ItemsSource = ents.ToList();
                        cbxSystemType.DisplayMemberPath = "DICTIONARYNAME";
                    }
                }
            }
        }

        #region "添加，修改，删除"
        private T_SYS_ENTITYMENU menu;

        public T_SYS_ENTITYMENU Menu
        {
            get { return menu; }
            set { menu = value; }
        }
        public class ExtObj
        {
            T_SYS_ENTITYMENU menu;
            string systemname;
            string othername;
        }
        void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Menu = (T_SYS_ENTITYMENU)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void ButtonDeleteOK_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (obj, result) =>
            {
                if (Menu != null)
                {
                    client.SysMenuDeleteAsync(Menu.ENTITYMENUID);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NODELETEROWINFOS"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NODELETEROWINFOS"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            };
            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
         
        }

        void AddWin_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion

        private void lkParentMenu_FindClick(object sender, EventArgs e)
        {
            ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(expander, "cbxSystemType");
            T_SYS_DICTIONARY dict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;

            string systype = dict == null ? "" : dict.DICTIONARYVALUE.GetValueOrDefault().ToString();
            MenuLookupForm lookup = new MenuLookupForm(systype);

            LookUp lkParentMenu = Utility.FindChildControl<LookUp>(expander, "lkParentMenu");
            lookup.SelectedClick += (obj, ev) =>
            {
                lkParentMenu.DataContext = lookup.SelectedObj;
                lkParentMenu.DisplayMemberPath = "MENUNAME";
            };
            lookup.Show();
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_ENTITYMENU");
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
