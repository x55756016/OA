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
using SMT.SaaS.OA.UI;
using SMT.SaaS.FrameworkUI;
//using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.Class;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using SMT.SaaS.FrameworkUI.SelectPostLevel;
using SMT.Saas.Tools.OrganizationWS;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SolutionForms : BaseForm,IEntityEditor
    {

        #region 初始化参数
        private string AAA = "";
        FormTypes action;
        RefreshedTypes saveType;
        
        SmtOAPersonOfficeClient client = new SmtOAPersonOfficeClient();
        PermissionServiceClient permissionclient = new PermissionServiceClient();
        OrganizationServiceClient OrgClient = new OrganizationServiceClient();
        //交通工具标准
        private ObservableCollection<T_OA_TAKETHESTANDARDTRANSPORT> StandardList = new ObservableCollection<T_OA_TAKETHESTANDARDTRANSPORT>();
        private ObservableCollection<T_OA_TAKETHESTANDARDTRANSPORT> AddStandardList = new ObservableCollection<T_OA_TAKETHESTANDARDTRANSPORT>();
        //记录旧的交通工具信息  用来和新的做比较如果不一样 则修改标志为true
        private List<T_OA_TAKETHESTANDARDTRANSPORT> OldStandardList = new List<T_OA_TAKETHESTANDARDTRANSPORT>();
        private T_OA_TAKETHESTANDARDTRANSPORT StandardObj = new T_OA_TAKETHESTANDARDTRANSPORT();
        //飞机路线列表
        
        private bool IsAddStandard = false;//添加交通工具
        private bool IsAddPlaneLine = false;//添加飞机路线
        private T_OA_TRAVELSOLUTIONS travelObj=new T_OA_TRAVELSOLUTIONS();
        private bool EditFlag = false;//修改标志 修改的时候如果做了改动则传列表 否则只修改解决方案

        ObservableCollection<T_OA_CANTAKETHEPLANELINE> RefPlaneList = new ObservableCollection<T_OA_CANTAKETHEPLANELINE>();
        ObservableCollection<T_OA_TAKETHESTANDARDTRANSPORT> RefvechileList = new ObservableCollection<T_OA_TAKETHESTANDARDTRANSPORT>();

        
        /// <summary>
        /// 用来记录交通类型的等级信息
        /// </summary>
        List<T_SYS_DICTIONARY> ListVechileLevel = new List<T_SYS_DICTIONARY>();
        //记录岗位级别
        List<T_SYS_DICTIONARY> ListPost = new List<T_SYS_DICTIONARY>();
        #region 方案设置
        List<T_HR_COMPANY> ListCompany = new List<T_HR_COMPANY>();
        List<T_HR_COMPANY> ListFirstCompany = new List<T_HR_COMPANY>(); List<T_OA_PROGRAMAPPLICATIONS> ListAppSolution = new List<T_OA_PROGRAMAPPLICATIONS>();
        private ObservableCollection<string> companyids = new ObservableCollection<string>();
        List<T_OA_PROGRAMAPPLICATIONS> ListAddAppSolution = new List<T_OA_PROGRAMAPPLICATIONS>();
        #endregion
        #endregion
        
        #region 构造函数
        public SolutionForms(FormTypes Action,T_OA_TRAVELSOLUTIONS SolutionObj)
        {
            action = Action;
            InitEvent();
            //this.Loaded += new RoutedEventHandler(SolutionForms_Loaded);
            if (action == FormTypes.Edit || action == FormTypes.Browse)
            {
                travelObj = SolutionObj;
            }
            else if (action == FormTypes.New)
            {
                //IsAddStandard = true;
            }
            
            InitializeComponent();
            InitToobar();
        }

        public SolutionForms()
        {
            
            InitEvent();
            SetEnabled();
            InitializeComponent();
            InitToobar();
        }
        void client_GetTravelSolutionFlowCompleted(object sender, GetTravelSolutionFlowCompletedEventArgs e)
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void BindDataGrid(List<T_OA_TRAVELSOLUTIONS> obj, int pageCount)
        {
            
            if (obj == null || obj.Count < 1)
            {
                this.cmbSolution.ItemsSource = null;
                return;
            }
            cmbSolution.ItemsSource = obj;
            cmbSolution.DisplayMemberPath = "PROGRAMMENAME";
            cmbSolution.SelectedIndex = 0;
        }
        private void DetailSolutionInfo(T_OA_TRAVELSOLUTIONS obj)
        {
            txtSolutionName.Text = string.IsNullOrEmpty(obj.PROGRAMMENAME) ? "" : obj.PROGRAMMENAME;
            nuHalfDay.Value = System.Convert.ToInt32( obj.CUSTOMHALFDAY);
            
            //区间天数
            nuqujiaomindays.Value = System.Convert.ToInt32(obj.MINIMUMINTERVALDAYS);
            nuqujianmax.Value = System.Convert.ToInt32(obj.MAXIMUMRANGEDAYS);
            nuqujianbili.Value = System.Convert.ToInt32(obj.INTERVALRATIO);
            //最大天数
            //numaxdays.Value = System.Convert.ToInt32(obj.MAXIMUMDAYS);
            nubaoxiaomindays.Value = System.Convert.ToInt32(obj.RANGEDAYS);
            

            if (!string.IsNullOrEmpty(obj.RANGEPOSTLEVEL.ToString()))
            {
                foreach (T_SYS_DICTIONARY Region in cbxpostlevel.Items)
                {
                    if (Region.DICTIONARYVALUE.ToString() == obj.RANGEPOSTLEVEL.ToString())
                    {
                        cbxpostlevel.SelectedItem = Region;
                        break;
                    }
                }
            }
            
        }
        /// <summary>
        /// 禁用控件
        /// </summary>
        private void SetEnabled()
        {
            //this.txtHour.IsEnabled = false;
            this.nuHalfDay.IsEnabled = false;
            this.nuqujianbili.IsEnabled = false;
            this.nuqujianmax.IsEnabled = false;
            this.nuqujiaomindays.IsEnabled = false;
            this.txtSolutionName.IsEnabled = false;            
            
            this.DGVechileStandard.IsEnabled = false;
            
            ToolBar_Vechile.IsEnabled = false;
            this.nubaoxiaomindays.IsEnabled = false;
            this.cbxpostlevel.IsEnabled = false;
        }
        #endregion

        #region 公共调用函数
        /// <summary>
        /// 初始化事件
        /// </summary>
        private void InitEvent()
        {
            client.AddTravleSolutionCompleted += new EventHandler<AddTravleSolutionCompletedEventArgs>(client_AddTravleSolutionCompleted);
            client.GetTravelSolutionFlowCompleted += new EventHandler<GetTravelSolutionFlowCompletedEventArgs>(client_GetTravelSolutionFlowCompleted);
            client.GetVechileStandardAndPlaneLineCompleted += new EventHandler<GetVechileStandardAndPlaneLineCompletedEventArgs>(client_GetVechileStandardAndPlaneLineCompleted);
            client.UpdateTravleSolutionCompleted += new EventHandler<UpdateTravleSolutionCompletedEventArgs>(client_UpdateTravleSolutionCompleted);
            //client.GetTravelSolutionFlowAsync(0,100, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);
            LoadSolutionInfos();
            OrgClient.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(OrgClient_GetCompanyActivedCompleted);
            OrgClient.GetCompanyActivedAsync(Common.CurrentLoginUserInfo.EmployeeID);

            //方案应用
            client.AddTravleSolutionSetCompleted += new EventHandler<AddTravleSolutionSetCompletedEventArgs>(client_AddTravleSolutionSetCompleted);
            //initToolbarSolution();
        }

        
        #region 加载数据
        void LoadSolutionInfos()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件

            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值

            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            
            client.GetTravelSolutionFlowAsync(0, 100, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);

        }
        #endregion
        /// <summary>
        /// 初始化 出差方案toolbar
        /// </summary>
        private void initToolbarSolution()
        {

            ToolBar_Solution.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar_Solution.BtnView.Visibility = Visibility.Collapsed;
            ToolBar_Solution.btnRefresh.Visibility = Visibility.Collapsed;
            ToolBar_Solution.btnEdit.Click +=new RoutedEventHandler(btnEdit_Click);
            ToolBar_Solution.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar_Solution.btnNew.Click += new RoutedEventHandler(ToolBarSolution_btnNew_Click);
            ToolBar_Solution.btnDelete.Click += new RoutedEventHandler(ToolBar_SolutionbtnDelete_Click);
            ToolBar_Solution.ShowRect();
            
        }
        /// <summary>
        /// 添加新的解决方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ToolBarSolution_btnNew_Click(object sender, RoutedEventArgs e)
        {
            action = FormTypes.New;
            travelObj.TRAVELSOLUTIONSID = System.Guid.NewGuid().ToString();
            travelObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            travelObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            travelObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            travelObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            travelObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            travelObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            travelObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            travelObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            this.SetEnabledIsTrue();
        }
        /// <summary>
        /// 删除 出差方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ToolBar_SolutionbtnDelete_Click(object sender, RoutedEventArgs e)
        {
            
        }
        /// <summary>
        /// 修改出差方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            action = FormTypes.Edit;
            travelObj = cmbSolution.SelectedItem as T_OA_TRAVELSOLUTIONS;
            SetEnabledIsTrue();
        }

        /// <summary>
        /// 禁用控件
        /// </summary>
        private void SetEnabledIsTrue()
        {
            this.nuHalfDay.IsEnabled = true;
            this.nuqujianbili.IsEnabled = true;
            this.nuqujianmax.IsEnabled = true;
            this.nuqujiaomindays.IsEnabled = true;
            this.txtSolutionName.IsEnabled = true;
            this.DGVechileStandard.IsEnabled = true;

            ToolBar_Vechile.IsEnabled = true;
            this.nubaoxiaomindays.IsEnabled = true;
            this.cbxpostlevel.IsEnabled = true;


            if (action == FormTypes.New)
            {
                if (!IsAddStandard)
                {
                    StandardList.Clear();
                    DGVechileStandard.ItemsSource = null;
                    this.nuHalfDay.Value = 1;
                    this.nuqujianbili.Value = 1;
                    this.nuqujianmax.Value = 1;
                    this.nuqujiaomindays.Value = 1;
                    this.txtSolutionName.Text = "";
                    this.nubaoxiaomindays.Value = 1;
                    this.cbxpostlevel.SelectedIndex = 0;
                    //IninAddVechile(true);
                }
                IninAddVechile(true);
            }
            else
           {
               //DGVechileStandard.ItemsSource = null;
               //StandardList.Clear();
               client.GetVechileStandardAndPlaneLineAsync(travelObj.TRAVELSOLUTIONSID, RefPlaneList, RefvechileList);
            }

            

            

        }

        void client_UpdateTravleSolutionCompleted(object sender, UpdateTravleSolutionCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Result ==0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    if (GlobalFunction.IsSaveAndClose(saveType))
                    {
                        RefreshUI(saveType);
                    }
                    else
                    {
                        action = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;

                        RefreshUI(RefreshedTypes.All);
                    }
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        void client_GetVechileStandardAndPlaneLineCompleted(object sender, GetVechileStandardAndPlaneLineCompletedEventArgs e)
        {
            try
            {
                if (!e.Cancelled)
                {
                    if (e.Error==null)
                    {
                        if (e.VechileStandardList.Count() > 0)
                        {
                            StandardList = e.VechileStandardList;
                            OldStandardList = e.VechileStandardList.ToList();
                            StandardBindDataGrid(StandardList, false);
                        }
                        
                                                
                    }
                }
            }
            catch(Exception ex)
            { 
                throw(ex);
            }
        }

        

        void client_AddTravleSolutionCompleted(object sender, AddTravleSolutionCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Result != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                    if (GlobalFunction.IsSaveAndClose(saveType))
                    {
                        RefreshUI(saveType);
                    }
                    else
                    {
                        action = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                        
                        RefreshUI(RefreshedTypes.All);
                    }
                }

            }
            catch (Exception ex)
            { 
                throw(ex);
            }

        }


        /// <summary>
        /// 初始化toolbar
        /// </summary>
        private void InitToobar()
        {
            InitVechileStandardToolBar();
            
            this.Loaded += new RoutedEventHandler(SolutionForms_Loaded);
            //GetVechileLevelInfos();
            if (action != FormTypes.New && action != FormTypes.Edit)
            {
                IninAddVechile(true);
                
            }
            //GetVechileLevelInfos();
        }

        void SolutionForms_Loaded(object sender, RoutedEventArgs e)
        {
            GetVechileLevelInfos();
            initToolbarSolution();
            if (action == FormTypes.New)
            {
                IninAddVechile(true);                
            }
            switch (action)
            {
                case FormTypes.New:
                    travelObj.TRAVELSOLUTIONSID = System.Guid.NewGuid().ToString();
                    travelObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    travelObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    travelObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    travelObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    travelObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    travelObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    travelObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    travelObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    this.cbxpostlevel.SelectedIndex = 0;
                    break;
                case FormTypes.Edit:

                    
                    client.GetVechileStandardAndPlaneLineAsync(travelObj.TRAVELSOLUTIONSID,RefPlaneList,RefvechileList);
                    DetailSolutionInfo(travelObj);
                    break;
                case FormTypes.Browse:
                    
                    client.GetVechileStandardAndPlaneLineAsync(travelObj.TRAVELSOLUTIONSID,RefPlaneList,RefvechileList);
                    DetailSolutionInfo(travelObj);
                    SetEnabled();
                    break;
            }
            SecondCompany.DisplayMemberPath = "CNAME";
            FirstComany.DisplayMemberPath = "CNAME";
        }
        /// <summary>
        /// 添加时初始化 交通工具的行信息
        /// </summary>
        private void IninAddVechile(bool IsFirst)
        {
            
            T_OA_TAKETHESTANDARDTRANSPORT transport = new T_OA_TAKETHESTANDARDTRANSPORT();
            transport.TAKETHESTANDARDTRANSPORTID = System.Guid.NewGuid().ToString();
            transport.CREATEDATE = DateTime.Today;
            StandardList.Add(transport);
            DGVechileStandard.ItemsSource = StandardList;
            if(IsFirst)
                DGVechileStandard.SelectedIndex = 0;

        }

        
        /// <summary>
        /// 获取交通工具的级别
        /// </summary>
        void GetVechileLevelInfos()
        {
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var objs = from d in dicts
                       where d.DICTIONCATEGORY == "VICHILELEVEL"
                       orderby d.DICTIONARYVALUE
                       select d;
            var objposts = from ent in dicts
                           where ent.DICTIONCATEGORY == "POSTLEVEL"
                           orderby ent.DICTIONARYVALUE
                           select ent;

            ListVechileLevel = objs.ToList();
            ListPost = objposts.ToList();
        }
        /// <summary>
        /// 初始化交通工具标准toolbar
        /// </summary>
        private void InitVechileStandardToolBar()
        {
            ToolBar_Vechile.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar_Vechile.BtnView.Visibility = Visibility.Collapsed;
            ToolBar_Vechile.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar_Vechile.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar_Vechile.btnNew.Click += new RoutedEventHandler(ToolBar_VechilebtnNew_Click);
            ToolBar_Vechile.btnDelete.Click += new RoutedEventHandler(ToolBar_VechilebtnDelete_Click);
            ToolBar_Vechile.ShowRect();
        }
        /// <summary>
        /// 删除交通工具标准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ToolBar_VechilebtnDelete_Click(object sender, RoutedEventArgs e)
        {
            
            if (DGVechileStandard.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DGVechileStandard.SelectedItems.Count; i++)
                {
                    T_OA_TAKETHESTANDARDTRANSPORT ent = DGVechileStandard.SelectedItems[i] as T_OA_TAKETHESTANDARDTRANSPORT;
                    StandardList.Remove(ent);
                }
                StandardBindDataGrid(StandardList,false);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        /// <summary>
        /// 增加交通工具标准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ToolBar_VechilebtnNew_Click(object sender, RoutedEventArgs e)
        {
            IsAddStandard = true;
            //StandardList.Clear();
            

            //DGVechileStandard.ItemsSource = null;
            //StandardBindDataGrid(StandardList, true);
            
            //this.cbxpostlevel.SelectedIndex = 0;
            SetEnabledIsTrue();

        }
        #endregion

        #region DataGrid 数据加载
        /// <summary>
        /// 绑定乘坐交通工具
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="IsAdd">如果是新建 则添加一行</param>
        private void StandardBindDataGrid(ObservableCollection<T_OA_TAKETHESTANDARDTRANSPORT> obj,bool IsAdd)
        {
            StandardList = obj;
            if (StandardList.Count > 0)
            {
                StandardList.ForEach(item =>
                {
                    item.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Standard_PropertyChanged);
                });
                if(IsAdd)
                    NewStandardDetail();
            }
            else
            {
                if (IsAdd)
                    NewStandardDetail();
            }
            this.DGVechileStandard.ItemsSource = StandardList;
        }

        
        private void NewStandardDetail()
        {
            StandardObj.TAKETHESTANDARDTRANSPORTID = Guid.NewGuid().ToString();
            StandardObj.T_OA_TRAVELSOLUTIONS = travelObj;

            StandardObj.ENDPOSTLEVEL = StandardList[DGVechileStandard.SelectedIndex].ENDPOSTLEVEL;//结束岗位级别

            StandardObj.TYPEOFTRAVELTOOLS = StandardList[DGVechileStandard.SelectedIndex].TYPEOFTRAVELTOOLS;//乘坐类型
            StandardObj.TAKETHETOOLLEVEL = StandardList[DGVechileStandard.SelectedIndex].TAKETHETOOLLEVEL;//乘坐级别
            this.StandardList.Add(StandardObj);
            this.DGVechileStandard.ItemsSource = StandardList;
            StandardObj.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Standard_PropertyChanged);

           
        }
        void Standard_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
            if (DGVechileStandard.ItemsSource != null)
            {
                foreach (Object row in DGVechileStandard.ItemsSource)//判断所选的出发城市是否与目标城市相同
                {
                    
                    SMT.SaaS.OA.UI.Class.DictionaryComboBox ComVechile = ((SMT.SaaS.OA.UI.Class.DictionaryComboBox)((StackPanel)DGVechileStandard.Columns[1].GetCellContent(row)).Children.FirstOrDefault()) as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                    SMT.SaaS.OA.UI.Class.DictionaryComboBox ComLevel = ((SMT.SaaS.OA.UI.Class.DictionaryComboBox)((StackPanel)DGVechileStandard.Columns[2].GetCellContent(row)).Children.FirstOrDefault()) as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                    SelectPost EndComPost = ((SelectPost)((StackPanel)DGVechileStandard.Columns[3].GetCellContent(row)).Children.FirstOrDefault()) as SelectPost;
                    
                    
                }
                
            }
        }
        #endregion
        

        #region 交通工具标准datagrid事件

        private void DgVechileStandard_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (DGVechileStandard.ItemsSource != null)
            {
                if (IsAddStandard)
                {
                    Utility.DataRowAddRowNo(sender, e);
                    SMT.SaaS.OA.UI.Class.DictionaryComboBox ComVechile = DGVechileStandard.Columns[1].GetCellContent(e.Row).FindName("ComVechileType") as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                    SMT.SaaS.OA.UI.Class.DictionaryComboBox ComLevel = DGVechileStandard.Columns[2].GetCellContent(e.Row).FindName("ComVechileTypeLeve") as SMT.SaaS.OA.UI.Class.DictionaryComboBox;

                    SelectPost EndComPost = DGVechileStandard.Columns[3].GetCellContent(e.Row).FindName("txtSelectPost") as SelectPost;

                    ComVechile.SelectedIndex = 0;
                    ComLevel.SelectedIndex = 0;
                    IsAddStandard = false;
                }
                else
                {
                    T_OA_TAKETHESTANDARDTRANSPORT Standard = (T_OA_TAKETHESTANDARDTRANSPORT)e.Row.DataContext;
                    SMT.SaaS.OA.UI.Class.DictionaryComboBox ComVechile = DGVechileStandard.Columns[1].GetCellContent(e.Row).FindName("ComVechileType") as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                    SMT.SaaS.OA.UI.Class.DictionaryComboBox ComLevel = DGVechileStandard.Columns[2].GetCellContent(e.Row).FindName("ComVechileTypeLeve") as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                    SelectPost EndComPost = DGVechileStandard.Columns[3].GetCellContent(e.Row).FindName("txtSelectPost") as SelectPost;


                    DGVechileStandard.SelectedItem = e.Row;

                    if (ComVechile != null)
                    {
                        foreach (T_SYS_DICTIONARY Region in ComVechile.Items)
                        {
                            if (Region.DICTIONARYVALUE.ToString() == Standard.TYPEOFTRAVELTOOLS)
                            {
                                ComVechile.SelectedItem = Region;
                                break;
                            }

                        }
                    }
                    if (ComLevel != null)
                    {
                        foreach (T_SYS_DICTIONARY Region in ComLevel.Items)
                        {
                            if (Region.DICTIONARYVALUE.ToString() == Standard.TAKETHETOOLLEVEL)
                            {
                                ComLevel.SelectedItem = Region;
                                break;
                            }

                        }
                    }
                    if (!string.IsNullOrEmpty(Standard.ENDPOSTLEVEL))
                    {
                        //将  岗位值转换为对应的名称
                        //string PostCode = EndComPost.TxtSelectedPost.Text;
                        string PostCode = "";
                        string[] arrstr = Standard.ENDPOSTLEVEL.Split(',');
                        foreach (var d in arrstr)
                        {
                            int i = d.ToInt32();
                            var ents = from n in ListPost
                                       where n.DICTIONARYVALUE == i
                                       select n;
                            if (ents.Count() > 0)
                                PostCode += ents.FirstOrDefault().DICTIONARYNAME.ToString() + ",";
                        }
                        if (!(string.IsNullOrEmpty(PostCode)))
                        {
                            PostCode = PostCode.Substring(0, PostCode.Length - 1);
                            
                        }
                        EndComPost.TxtSelectedPost.Text = PostCode;
                    }
                }
            }
            

        }


        private void DGVechileStandard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        /// <summary>
        /// 交通工具类型选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComVechileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            SMT.SaaS.OA.UI.Class.DictionaryComboBox vechiletype = sender as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
            if (vechiletype.SelectedIndex > 0)
            {
                T_SYS_DICTIONARY VechileTypeObj = vechiletype.SelectedItem as T_SYS_DICTIONARY;
                if (DGVechileStandard.Columns[1].GetCellContent(DGVechileStandard.SelectedItem) != null)
                {
                    SMT.SaaS.OA.UI.Class.DictionaryComboBox ComLevel = DGVechileStandard.Columns[2].GetCellContent(DGVechileStandard.SelectedItem).FindName("ComVechileTypeLeve") as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                    
                    var ListObj = from ent in ListVechileLevel
                                  where ent.T_SYS_DICTIONARY2Reference.EntityKey.EntityKeyValues[0].Value.ToString() == VechileTypeObj.DICTIONARYID
                                  select ent;
                    if (ListObj.Count() > 0)
                    {
                        
                        if (ListObj != null)
                        {
                            //ListObj.ToList().Insert(0, nuldict);
                            ComLevel.ItemsSource = ListObj.ToList();
                            ComLevel.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 保存
        private void Save()
        {
            try
            {
                if (Check())
                {
                    
                    if (ChxOnday.IsChecked == true)
                    {
                        travelObj.ANDFROMTHATDAY = "1";
                    }
                    else
                    {
                        travelObj.ANDFROMTHATDAY = "0";
                    }
                    if (cbxpostlevel.SelectedIndex == 0)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), "请选择岗位");
                        return;
                    }
                    else
                    {
                         T_SYS_DICTIONARY Dict = cbxpostlevel.SelectedItem as T_SYS_DICTIONARY;
                         travelObj.RANGEPOSTLEVEL = Dict.DICTIONARYVALUE.ToString();
                    }
                    travelObj.PROGRAMMENAME = this.txtSolutionName.Text;                    
                    //最小天数                    
                    travelObj.RANGEDAYS = nubaoxiaomindays.Value.ToString();
                    //区间天数
                    travelObj.MINIMUMINTERVALDAYS = nuqujiaomindays.Value.ToString();
                    travelObj.MAXIMUMRANGEDAYS = nuqujianmax.Value.ToString();
                    travelObj.INTERVALRATIO = nuqujianbili.Value.ToString();
                    //最大天数
                    
                    travelObj.CUSTOMHALFDAY = nuHalfDay.Value.ToString();
                    AddVechileStandard();
                    //添加出差方案设置
                    AddSetSolution();
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    if (action == FormTypes.New)
                    {                        
                        client.AddTravleSolutionAsync(travelObj, AddStandardList, null);
                    }
                    else
                    {
                        if (EditFlag)
                        {
                            client.UpdateTravleSolutionAsync(travelObj, AddStandardList, null, EditFlag);
                        }
                        else
                        {
                            //只对出差方案进行修改  出差路线、飞机路线不做改动
                            client.UpdateTravleSolutionAsync(travelObj, null, null, false);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        /// <summary>
        /// 添加交通工具标准
        /// </summary>
        private void AddVechileStandard()
        {

            try
            {
                //if (action == FormTypes.New)
                //{
                    AddStandardList.Clear();
                    if (DGVechileStandard.ItemsSource != null)
                    {
                        
                        foreach (Object obj in DGVechileStandard.ItemsSource)
                        {
                            T_OA_TAKETHESTANDARDTRANSPORT ent = (T_OA_TAKETHESTANDARDTRANSPORT)obj;
                            //T_OA_TAKETHESTANDARDTRANSPORT ent = new T_OA_TAKETHESTANDARDTRANSPORT();
                            if (ent.TAKETHESTANDARDTRANSPORTID == null)
                            {
                                ent.TAKETHESTANDARDTRANSPORTID = System.Guid.NewGuid().ToString();
                            }
                            ent.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                            SMT.SaaS.OA.UI.Class.DictionaryComboBox ComVechile = DGVechileStandard.Columns[1].GetCellContent(obj).FindName("ComVechileType") as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                            SMT.SaaS.OA.UI.Class.DictionaryComboBox ComLevel = DGVechileStandard.Columns[2].GetCellContent(obj).FindName("ComVechileTypeLeve") as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                            SelectPost EndComPost = DGVechileStandard.Columns[3].GetCellContent(obj).FindName("txtSelectPost") as SelectPost;
                            
                            
                            if (EndComPost != null)
                            {
                                //添加到数据库中的为数字
                                string PostCode = EndComPost.TxtSelectedPost.Text;
                                string PostValue="";
                                string[] arrstr = PostCode.Split(',');
                                foreach (var d in arrstr)
                                {
                                    var ents = from e in ListPost
                                               where e.DICTIONARYNAME == d
                                               select e;
                                    if (ents.Count() > 0)
                                        PostValue += ents.FirstOrDefault().DICTIONARYVALUE.ToString() + ",";
                                }
                                if (!(string.IsNullOrEmpty(PostValue)))
                                {
                                    PostValue = PostValue.Substring(0, PostValue.Length-1);
                                    ent.ENDPOSTLEVEL = PostValue;
                                }
                                //ent.ENDPOSTLEVEL = EndComPost.TxtSelectedPost.Text;
                                
                            }
                            if (ComVechile != null)
                            {
                                T_SYS_DICTIONARY ComVechileObj = ComVechile.SelectedItem as T_SYS_DICTIONARY;//开始岗位
                                ent.TYPEOFTRAVELTOOLS = ComVechileObj.DICTIONARYVALUE.ToString();
                            }
                            if (ComLevel != null)
                            {
                                T_SYS_DICTIONARY ComLevelObj = ComLevel.SelectedItem as T_SYS_DICTIONARY;//开始岗位
                                ent.TAKETHETOOLLEVEL = ComLevelObj.DICTIONARYVALUE.ToString();
                            }
                            
                            ent.T_OA_TRAVELSOLUTIONS = travelObj;
                            AddStandardList.Add(ent);
                            if (action == FormTypes.Edit)
                            {
                                var ents = from standard in OldStandardList
                                           where standard == ent
                                           select standard;
                                if(!(ents.Count() >0))
                                {
                                    EditFlag= true;
                                }
                            }
                            
                        }
                    }
                //}
            }
            catch (Exception ex)
            {
                string StrError = ex.ToString();
                throw(ex);
            }
        }
        

        private bool Check()
        {
            
            //工具标准检查
            foreach (Object obj in DGVechileStandard.ItemsSource)
            {
                SMT.SaaS.OA.UI.Class.DictionaryComboBox ComVechile = DGVechileStandard.Columns[1].GetCellContent(obj).FindName("ComVechileType") as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                SMT.SaaS.OA.UI.Class.DictionaryComboBox ComLevel = DGVechileStandard.Columns[2].GetCellContent(obj).FindName("ComVechileTypeLeve") as SMT.SaaS.OA.UI.Class.DictionaryComboBox;
                SelectPost EndComPost = DGVechileStandard.Columns[3].GetCellContent(obj).FindName("txtSelectPost") as SelectPost;
                
                
                if (EndComPost != null)
                {
                    if (string.IsNullOrEmpty(EndComPost.TxtSelectedPost.Text))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Caution, "CAUTION", "岗位不能为空");
                        return false;
                    }
                }
                if (ComVechile != null)
                {
                    if (ComVechile.SelectedIndex == 0)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Caution, "CAUTION", "交通工具类型不能为空");
                        return false;
                    }
                }
                
            }

            

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            return true;
        }
        #endregion

        
        #region IEntityEditor
        public string GetTitle()
        {

            if (action == FormTypes.New)
            {
                return "添加出差方案";
                //return Utility.GetResourceStr("ADDTITLE", "GRADENAME");
            }
            else if (action == FormTypes.Edit)
            {
                return "修改出差方案";
                //return Utility.GetResourceStr("EDITTITLE", "GRADENAME");
            }
            else
            {
                return "查看出差方案";
                //return Utility.GetResourceStr("VIEWTITLE", "GRADENAME");
            }
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
                    saveType = RefreshedTypes.LeftMenu;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    Save();
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
            if (action != FormTypes.Browse )
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1",
                    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                };
                items.Add(item);
                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("SAVE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"

                };
                items.Add(item);
            }

            return items;
        }

        private void Close()
        {
            RefreshUI(saveType);
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

        

        private void txtSelectPost_SelectClick(object sender, EventArgs e)
        {
            SelectPost txt = (SelectPost)sender;
            string StrOld = txt.TxtSelectedPost.Text.ToString();

            PostList SelectCity = new PostList(StrOld, AAA);
            string citycode = "";
            SelectCity.SelectedClicked += (obj, ea) =>
            {
                AAA = "";
                string StrPost = SelectCity.Result.Keys.FirstOrDefault();
                if (!string.IsNullOrEmpty(StrPost))
                    txt.TxtSelectedPost.Text = StrPost.Substring(0, StrPost.Length - 1);
                citycode = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                if (!string.IsNullOrEmpty(citycode))
                    citycode = citycode.Substring(0, citycode.Length - 1);

                if (txt.Name == "txtSelectPost")
                {
                    StandardList[DGVechileStandard.SelectedIndex].ENDPOSTLEVEL = citycode;
                    AAA = citycode;
                }
            };
            var windows = SMT.SAAS.Controls.Toolkit.Windows.ProgramManager.ShowProgram(Utility.GetResourceStr("CITY"), "", "123", SelectCity, false, false, null);
            if (SelectCity is PostList)
            {
                (SelectCity as PostList).Close += (o, args) =>
                {
                    windows.Close();
                };
            }
        }

        private void cmbSolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            travelObj = cmbSolution.SelectedItem as T_OA_TRAVELSOLUTIONS;

            client.GetVechileStandardAndPlaneLineAsync(travelObj.TRAVELSOLUTIONSID, RefPlaneList, RefvechileList);
            DetailSolutionInfo(travelObj);
            client.GetTravleSolutionSetBySolutionIDCompleted += new EventHandler<GetTravleSolutionSetBySolutionIDCompletedEventArgs>(client_GetTravleSolutionSetBySolutionIDCompleted);
            client.GetTravleSolutionSetBySolutionIDAsync(travelObj.TRAVELSOLUTIONSID);
            SetEnabled();
        }

        #region 出差方案设置
        
        
        /// <summary>
        /// 添加所选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (FirstComany.SelectedItems.Count > 0)
            {
                T_HR_COMPANY selectcompany = new T_HR_COMPANY();
                selectcompany = FirstComany.SelectedItem as T_HR_COMPANY;
                if (SecondCompany.Items.Count > 0)
                {
                    if (!(SecondCompany.Items.Contains(selectcompany)))
                    {
                        SecondCompany.Items.Add(selectcompany);
                    }
                }
                else
                {
                    SecondCompany.Items.Add(selectcompany);
                }
                
                FirstComany.Items.Remove(selectcompany);
                
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, "CAUTION", "请选择公司");
            }
            //FirstComany.ItemsSource = null;
            //FirstComany.ItemsSource = ListFirstCompany;
            SecondCompany.DisplayMemberPath = "CNAME";
        }
        /// <summary>
        /// 添加所有项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddAllBtn_Click(object sender, RoutedEventArgs e)
        {
            //SecondCompany.Items.Clear();
            if (FirstComany.Items.Count > 0)
            {
                foreach (var obj in FirstComany.Items)
                {
                    T_HR_COMPANY selectcompany = new T_HR_COMPANY();
                    selectcompany = obj as T_HR_COMPANY;

                    SecondCompany.Items.Add(selectcompany);

                    //FirstComany.Items.Remove(selectcompany);

                }
                FirstComany.Items.Clear();
            }
        }
        /// <summary>
        /// 删除选中项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SecondCompany.SelectedItems.Count > 0)
            {
                
                T_HR_COMPANY selectcompany = new T_HR_COMPANY();
                selectcompany = SecondCompany.SelectedItem as T_HR_COMPANY;
                
                SecondCompany.Items.Remove(selectcompany);
                FirstComany.Items.Add(selectcompany);
                


            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, "CAUTION", "请选择公司");
            }
            
            SecondCompany.DisplayMemberPath = "CNAME";
        }
        /// <summary>
        /// 删除所有项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var obj in SecondCompany.Items)
            {
                T_HR_COMPANY selectcompany = new T_HR_COMPANY();
                selectcompany = obj as T_HR_COMPANY;

                FirstComany.Items.Add(selectcompany);

            }
            SecondCompany.Items.Clear();
            this.FirstComany.DisplayMemberPath = "CNAME";

        }

        void OrgClient_GetCompanyActivedCompleted(object sender, GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                //this.FirstComany.ItemsSource = e.Result;
                foreach (var obj in e.Result)
                {
                    FirstComany.Items.Add(obj as T_HR_COMPANY);
                }
                ListCompany = e.Result.ToList();
                
            }
        }

        void client_AddTravleSolutionSetCompleted(object sender, AddTravleSolutionSetCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                

            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        void client_GetTravleSolutionSetBySolutionIDCompleted(object sender, GetTravleSolutionSetBySolutionIDCompletedEventArgs e)
        {
            
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {

                    if (e.Result.Count() > 0)
                    {
                        ListAppSolution = e.Result.ToList();

                        foreach (var h in ListAppSolution)
                        {
                            var ents = from ent in ListCompany
                                       where ent.COMPANYID == h.COMPANYID
                                       select ent;
                            SecondCompany.Items.Add(ents.FirstOrDefault());
                            FirstComany.Items.Remove(ents.FirstOrDefault());
                        }
                        
                    }
                }

            }
        }

        private void AddSetSolution()
        {
            if (SecondCompany.Items.Count > 0)
            {
                foreach (var obj in SecondCompany.Items)
                {
                    //ListAddAppSolution.Add(obj as T_HR_COMPANY);
                    companyids.Add((obj as T_HR_COMPANY).COMPANYID);
                    //ListAddAppSolution.Add( obj as );
                }
                
            }
        }
        #endregion

    }
}
