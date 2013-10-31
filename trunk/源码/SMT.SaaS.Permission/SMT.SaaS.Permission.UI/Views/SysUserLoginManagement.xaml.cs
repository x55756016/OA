using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using System.Globalization;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysUserLoginManagement : BasePage
    {
        #region 初始化数据
        private static PermissionServiceClient SysLoginClient = new PermissionServiceClient();//龙康才新增
       // private PermissionServiceClient SysLoginClient = new PermissionServiceClient();
        ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
        public T_SYS_USERLOGINRECORD SelectLoginReCordID { get; set; }
        private bool QueryBtn = false;
        private SMTLoading loadbar = new SMTLoading(); 
        private void InitControlEvent()
        {            
            this.Loaded += new RoutedEventHandler(SysUserLoginRecordManagement_Loaded);            
            SysLoginClient.GetSysUserLoginRecordInfosPagingCompleted += new EventHandler<GetSysUserLoginRecordInfosPagingCompletedEventArgs>(SysLoginClient_GetSysUserLoginRecordInfosPagingCompleted);            
            GetEntityLogo("T_SYS_USERLOGINRECORD");
        }

        void SysLoginClient_GetSysUserLoginRecordInfosPagingCompleted(object sender, GetSysUserLoginRecordInfosPagingCompletedEventArgs e)
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
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            loadbar.Stop();
        }

        //  绑定DataGird
        private void BindDataGrid(List<V_UserLoginRecord> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                DtGrid.ItemsSource = null;
                return;
            }
            DtGrid.ItemsSource = obj;
        }

        void SysLoginClient_GetSysUserLoginRecordInfosCompleted(object sender, GetSysUserLoginRecordInfosCompletedEventArgs e)
        {
            PagedCollectionView pcv = null;            
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    List<V_UserLoginRecord> LoginList = e.Result.ToList();
                    var q = from ent in LoginList
                            select ent;
                    pcv = new PagedCollectionView(q);
                    pcv.PageSize = 25;
                    dataPager.DataContext = pcv;
                    DtGrid.ItemsSource = pcv;
                }
            }
            loadbar.Stop();
        }

        void SysLoginClient_GetSysUserLoginRecordInfosBySearchCompleted(object sender, GetSysUserLoginRecordInfosBySearchCompletedEventArgs e)
        {
            PagedCollectionView pcv = null;
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    List<V_UserLoginRecord> LoginList = e.Result.ToList();
                    var q = from ent in LoginList
                            select ent;
                    pcv = new PagedCollectionView(q);
                    pcv.PageSize = 25;
                    dataPager.DataContext = pcv;
                    DtGrid.ItemsSource = pcv;
                    //this.DtGrid.ItemsSource = e.Result.ToList();
                }
                else
                {
                    //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "没有找到数据", Utility.GetResourceStr("CONFIRMBUTTON"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "没有找到数据", Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
            }
            loadbar.Stop();
        }

        private void LoadData()
        { 
            string filter = "";    //查询过滤条件
            int pageCount = 0;
            if (QueryBtn)
            {
                string StrState = ""; //状态

                string StrStart = "";//添加文档的起始时间
                string StrEnd = "";//添加文档的结束时间

                ComboBox dpState = Utility.FindChildControl<ComboBox>(expander, "dpState");
                switch (dpState.SelectedIndex)
                {
                    case 0:
                        StrState = "";
                        break;
                    case 1:
                        StrState = "1";
                        break;
                    case 2:
                        StrState = "0";
                        break;
                }

                DatePicker DtpStart = Utility.FindChildControl<DatePicker>(expander, "DtStart");
                DatePicker DtpEnd = Utility.FindChildControl<DatePicker>(expander, "DtEnd");

                StrStart = DtpStart.Text.Trim().ToString();
                StrEnd = DtpEnd.Text.Trim().ToString();

                if (StrState == "" && StrStart == "" && StrEnd == "")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("QUERYINFONOTNULL"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("QUERYINFONOTNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //return;
                    DtStart = System.Convert.ToDateTime(StrStart);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "userloginrecord.LOGINDATE >@" + paras.Count().ToString();
                    paras.Add(DtStart);
                   
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ENDTIMENOTNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //return;
                    DtEnd = System.Convert.ToDateTime(StrEnd);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    
                    filter += "userloginrecord.LOGINDATE <@" + paras.Count().ToString();
                    paras.Add(DtEnd);
                }
                if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    DtStart = System.Convert.ToDateTime(StrStart);
                    //DtEnd = System.Convert.ToDateTime(StrEnd);
                    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                    if (DtStart > DtEnd)
                    {

                        //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("SEARCH"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME")); Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("SEARCH"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "userloginrecord.LOGINDATE >@" + paras.Count().ToString();
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "userloginrecord.LOGINDATE <@" + paras.Count().ToString();
                        paras.Add(DtEnd);
                    }
                }
            }
            
            loadbar.Start();
            SMT.Saas.Tools.PermissionWS.LoginUserInfo loginUserInfo = new SMT.Saas.Tools.PermissionWS.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            SysLoginClient.GetSysUserLoginRecordInfosPagingAsync(dataPager.PageIndex, dataPager.PageSize, "userloginrecord.LOGINDATE", filter, paras, pageCount, loginUserInfo);
            //SysLoginClient.GetSysUserLoginRecordInfosAsync();
        }
        
        public SysUserLoginManagement()
        {
            InitializeComponent();
            //Utility.DisplayGridToolBarButton(FormToolBar1, "USERLOGINRECORD", true);
            PARENT.Children.Add(loadbar);
            InitControlEvent();
            LoadData();
        }
        #endregion

        #region 查询按钮事件

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            QueryBtn = true;
            LoadData();            
        }
        #endregion

        #region 设置样式

        void SysUserLoginRecordManagement_Loaded(object sender, RoutedEventArgs e)
        {

            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            
            //隐藏未使用按钮
            //FormToolBar1.btnRead.Visibility = Visibility.Collapsed;
            FormToolBar1.btnNew.Visibility = Visibility.Collapsed;
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnimport.Visibility = Visibility.Collapsed;
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;

            FormToolBar1.BtnView.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            FormToolBar1.btnEdit.Visibility = Visibility.Collapsed;
            FormToolBar1.btnDelete.Visibility = Visibility.Collapsed;


            FormToolBar1.ShowRect();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ///TOADD:添加权限

            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_USERLOGINRECORD");
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }

    #region 登录状态
    public class ConverterNumberToWayString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            if(value == null)
                return "不在线";
            switch (value.ToString())
            {
                case "0":
                    StrReturn = "不在线";
                    break;
                case "1":
                    StrReturn = "在线";
                    break;                
            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            if (value == null)
                return "0";
            switch (value.ToString())
            {
                case "不在线":
                    StrReturn = "0";
                    break;
                case "在线":
                    StrReturn = "1";
                    break;                
            }
            return StrReturn;

        }
    }
    #endregion

    #region 系统用户状态
    public class ConverterSysUserState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            if (value != null)
                return StrReturn;
            switch (value.ToString())
            {
                case "0":
                    StrReturn = "禁止";
                    break;
                case "1":
                    StrReturn = "活动";
                    break;
            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            if (value != null)
                return StrReturn;
            switch (value.ToString())
            {
                case "禁止":
                    StrReturn = "0";
                    break;
                case "活动":
                    StrReturn = "1";
                    break;
            }
            return StrReturn;

        }
    }
    #endregion

    #region 日期时间格式化
    public class ConverterDateToFormatString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime Dt = new DateTime();
            if (value != null && value != "")
            {
                Dt = (DateTime)value;
                return Dt.ToShortDateString() ;
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDateTime(value).ToString();
        }
    }

    #endregion
}
