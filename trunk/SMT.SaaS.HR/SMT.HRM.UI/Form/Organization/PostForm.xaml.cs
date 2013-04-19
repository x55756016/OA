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
using SMT.Saas.Tools.OrganizationWS;

using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using System.Xml.Serialization;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;

namespace SMT.HRM.UI.Form
{
    public partial class PostForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        #region 初始化

        #region 全局变量
        public FormTypes FormType { get; set; }
        private T_HR_POST post;
        public string createUserName;
        public T_HR_POST Post
        {
            get { return post; }
            set
            {
                post = value;
                this.DataContext = post;
            }
        }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        OrganizationServiceClient client;
        private bool canSubmit = false;//能否提交审核
        public bool needsubmit = false;//提交审核
        private bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient personClient;
        private string postID = "";
        //List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> deptList;//部门字典
        #endregion

        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建  zwp 2011.10.09
        /// </summary>
        public PostForm()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            postID = "";
            InitControlEvent();
        }

        /// <summary>
        /// 添加岗位信息
        /// </summary>
        /// <param name="type">操作类型</param>
        /// <param name="org">公司信息</param>
        /// <param name="dep">部门信息</param>
        public PostForm(FormTypes type, string strID)
        {
            InitializeComponent();


            FormType = type;
            postID = strID;
            InitControlEvent();
            //this.Loaded += (sender, args) =>
            //{
            //    InitControlEvent();
            //};
            #region 原来的
            /*
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnableControl();
            }
            */
            #endregion
        }
        /// <summary>
        /// 初始化服务
        /// </summary>
        private void InitControlEvent()
        {
            client = new OrganizationServiceClient();
            personClient = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
            client.PostAddCompleted += new EventHandler<PostAddCompletedEventArgs>(client_PostAddCompleted);
            client.PostUpdateCompleted += new EventHandler<PostUpdateCompletedEventArgs>(client_PostUpdateCompleted);
            client.GetPostByIdCompleted += new EventHandler<GetPostByIdCompletedEventArgs>(client_GetPostByIdCompleted);
            client.GetPostDictionaryByDepartmentDictionayIDCompleted += new EventHandler<GetPostDictionaryByDepartmentDictionayIDCompletedEventArgs>(client_GetPostDictionaryByDepartmentDictionayIDCompleted);
            personClient.GetEmployeeToEngineCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeeToEngineCompletedEventArgs>(personClient_GetEmployeeToEngineCompleted);
            this.Loaded += new RoutedEventHandler(PostForm_Loaded);
        }

        void PostForm_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnableControl();
            }
            #endregion
            if (FormType == FormTypes.New)
            {
                Post = new T_HR_POST();
                Post.POSTID = Guid.NewGuid().ToString();
                Post.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetPostByIdAsync(postID, "POST");
            }

            if (FormType != FormTypes.Browse)
            {
                //Load事件之后，加载完后获取到父控件
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
                entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            }
        }

        /// <summary>
        /// 新增的保存事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            isSubmit = true;
            needsubmit = true;
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            Save();
        }

        /// <summary>
        ///     回到提交前的状态
        /// </summary>
        private void BackToSubmit()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            needsubmit = false;


            //隐藏工具栏 不允许二次提交
            //EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            //#region 隐藏entitybrowser中的toolbar按钮
            //entBrowser.BtnSaveSubmit.IsEnabled = false;
            //if (entBrowser.EntityEditor is IEntityEditor)
            //{
            //    List<ToolbarItem> bars = GetToolBarItems();
            //    if (bars != null)
            //    {
            //        ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
            //        if (bar != null)
            //        {
            //            bar.Visibility = Visibility.Collapsed;
            //        }
            //    }
            //}
            //#endregion
            if (refreshType == RefreshedTypes.CloseAndReloadData)
            {
                //refreshType = RefreshedTypes.AuditInfo;
                refreshType = RefreshedTypes.HideProgressBar;
            }

        }

        /// <summary>
        /// 禁用控件
        /// </summary>
        private void EnableControl()
        {
            lkCompany.IsEnabled = false;
            cbPostlevel.IsEnabled = false;
            lkDepart.IsEnabled = false;
            txtManageNmber.IsEnabled = false;
            txtPosCode.IsReadOnly = true;
            txtPostFunction.IsReadOnly = true;
            txtPromote.IsReadOnly = true;
            nuPostNumber.IsEnabled = false;
            lkPost.IsEnabled = false;
            cbxPosition.IsEnabled = false;
            txtRemark.IsReadOnly = true;
            rbtYes.IsEnabled = false;
            RbtNo.IsEnabled = false;
            rbtYesCore.IsEnabled = false;
            rbtNoCore.IsEnabled = false;
        }

        #endregion
        #region 完成事件
        void personClient_GetEmployeeToEngineCompleted(object sender, Saas.Tools.PersonnelWS.GetEmployeeToEngineCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                }
                else
                {
                    createUserName = e.Result[0].EMPLOYEENAME;
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 获取所有部门
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetDepartmentAllCompleted(object sender, GetDepartmentAllCompletedEventArgs e)
        {
            //if (e.Error != null && string.IsNullOrEmpty(e.Error.Message))
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
            //            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            //}
            //else
            //{
            //    if (lkCompany.DataContext != null || Post != null)
            //    {
            //        T_HR_COMPANY ent = null;
            //        if (Post.T_HR_DEPARTMENT != null)
            //        {
            //            ent = Post.T_HR_DEPARTMENT.T_HR_COMPANY;
            //        }
            //        else
            //        {
            //            ent = lkCompany.DataContext as T_HR_COMPANY;
            //        }
            //        if (e.UserState.ToString() == "SetCompany")
            //        {
            //            ent = lkCompany.DataContext as T_HR_COMPANY;

            //        }
            //        if (e.Result != null)
            //        {
            //            string checkState = Convert.ToInt32(CheckStates.Approved).ToString();
            //            string editState = Convert.ToInt32(EditStates.Actived).ToString();
            //            System.Collections.ObjectModel.ObservableCollection<T_HR_DEPARTMENT> tempList = e.Result;
            //            var entity = tempList.Where(s => s.T_HR_COMPANY.COMPANYID == ent.COMPANYID && s.CHECKSTATE == checkState && s.EDITSTATE == editState);
            //            entity = entity.Count() > 0 ? entity.ToList() : null;
            //            cbxDepartment.ItemsSource = entity;
            //            cbxDepartment.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
            //            if (e.UserState.ToString() == "SetCompany")
            //            {
            //                return;
            //            }
            //            if (Post.T_HR_DEPARTMENT != null)
            //            {
            //                foreach (var item in cbxDepartment.Items)
            //                {
            //                    T_HR_DEPARTMENT dict = item as T_HR_DEPARTMENT;
            //                    if (dict != null)
            //                    {
            //                        if (dict.DEPARTMENTID == Post.T_HR_DEPARTMENT.DEPARTMENTID)
            //                        {
            //                            cbxDepartment.SelectedItem = item;
            //                            break;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }
        /// <summary>
        /// 根据id获取岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetPostByIdCompleted(object sender, GetPostByIdCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {

                if (e.UserState.ToString() == "POST")//加载岗位
                {
                    if (e.Result == null)
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
                           Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        return;
                    }
                    Post = e.Result;

                    if (FormType == FormTypes.Resubmit)
                    {
                        lkCompany.IsEnabled=false;
                        lkDepart.IsEnabled = false;
                        cbxPosition.IsEnabled = false;
                        Post.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                        if (Post.EDITSTATE != Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            Post.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                        }
                        //Post.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                        //if (Post.EDITSTATE == Convert.ToInt32(EditStates.Actived).ToString())
                        //{
                        //    Post.EDITSTATE = Convert.ToInt32(EditStates.PendingCanceled).ToString();
                        //}
                        //else
                        //{
                        //    Post.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                        //}

                    }
                    if (Post.PROSCENIUM == "1")
                    {
                        this.RbtNo.IsChecked = false;
                        this.rbtYes.IsChecked = true;
                    }
                    if (Post.BACKSTAGE == "1")
                    {
                        this.RbtNo.IsChecked = true;
                        this.rbtYes.IsChecked = false;
                    }
                    if (Post.ISCOREPERSONNEL.ToInt32() <= 0)
                    {
                        rbtYesCore.IsChecked = true;
                        rbtNoCore.IsChecked = false;
                    }
                    //是否为撤销，2为撤销中，3为已撤销
                    if (Post.EDITSTATE == "3" || Post.EDITSTATE == "2")
                    {
                        this.rbtYesCancel.IsChecked = true;
                    }
                    else
                    {
                        this.rbtNoCancel.IsChecked = true;
                    }
                    //显示岗位的公司
                    lkCompany.DataContext = Post.T_HR_DEPARTMENT.T_HR_COMPANY;
                    lkDepart.DataContext = Post.T_HR_DEPARTMENT;
                    lkDepart.TxtLookUp.Text = GetFullOrgName(post.T_HR_DEPARTMENT);
                    if (Post.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        EnableControl();
                        //绑定岗位的部门 和字典
                        List<T_HR_POSTDICTIONARY> pdslist = new List<T_HR_POSTDICTIONARY>();
                        if (Post.T_HR_POSTDICTIONARY != null)
                        {
                            pdslist.Add(Post.T_HR_POSTDICTIONARY);
                            cbxPosition.ItemsSource = pdslist;
                            cbxPosition.DisplayMemberPath = "POSTNAME";
                            foreach (var item in cbxPosition.Items)
                            {
                                T_HR_POSTDICTIONARY dict = item as T_HR_POSTDICTIONARY;
                                if (dict != null)
                                {
                                    if (dict.POSTDICTIONARYID == Post.T_HR_POSTDICTIONARY.POSTDICTIONARYID)
                                    {
                                        cbxPosition.SelectedItem = item;
                                        break;
                                    }
                                }
                            }

                        }

                    }
                    else
                    {
                        //岗位字典
                        client.GetPostDictionaryByDepartmentDictionayIDAsync(Post.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID, "load");
                    }

                    if (!string.IsNullOrEmpty(post.FATHERPOSTID))
                    {
                        client.GetPostByIdAsync(post.FATHERPOSTID, "FATHERPOST");
                    }
                    if (Post.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                    || Post.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        RefreshUI(RefreshedTypes.AuditInfo);
                        SetToolBar();
                        RefreshUI(RefreshedTypes.HideProgressBar);
                    }
                    else
                    {

                        System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                        CreateUserIDs.Add(Post.CREATEUSERID);
                        personClient.GetEmployeeToEngineAsync(CreateUserIDs);
                    }
                }
                else if (e.UserState.ToString() == "FATHERPOST")//加载岗位的父岗位
                {
                    lkPost.DataContext = e.Result;
                }

            }
        }
        /// <summary>
        /// 薪增岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void client_PostAddCompleted(object sender, PostAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.strMsg == "Repetition")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "DEPARTMENT,POSTNAME"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 修改岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void client_PostUpdateCompleted(object sender, PostUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                needsubmit = false;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.strMsg == "Repetition")
                {
                    needsubmit = false;
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "DEPARTMENT,POSTNAME"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                if (e.UserState.ToString() == "Edit")
                {
                    if (!isSubmit)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                }
                if (needsubmit)
                {
                    try
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.ManualSubmit();
                        BackToSubmit();
                    }
                    catch (Exception ex)
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("提交失败"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

                    }

                }
                else if (e.UserState.ToString() == "Audit")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
       Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                canSubmit = true;
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 根据部门字典获取岗位字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetPostDictionaryByDepartmentDictionayIDCompleted(object sender, GetPostDictionaryByDepartmentDictionayIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                    return;
                cbxPosition.ItemsSource = e.Result;
                cbxPosition.DisplayMemberPath = "POSTNAME";

                if (e.UserState.ToString() == "setDepartment")
                {
                    return;
                }
                if (Post != null)
                {
                    if (Post.T_HR_POSTDICTIONARY != null)
                    {
                        foreach (var item in cbxPosition.Items)
                        {
                            T_HR_POSTDICTIONARY dict = item as T_HR_POSTDICTIONARY;
                            if (dict != null)
                            {
                                if (dict.POSTDICTIONARYID == Post.T_HR_POSTDICTIONARY.POSTDICTIONARYID)
                                {
                                    cbxPosition.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_POST", Post.OWNERID,
            //        Post.OWNERPOSTID, Post.OWNERDEPARTMENTID, Post.OWNERCOMPANYID);

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else if (FormType == FormTypes.Resubmit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                ToolbarItems.RemoveAt(0);
                ToolbarItems.RemoveAt(0);
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_POST", Post.OWNERID,
                    Post.OWNERPOSTID, Post.OWNERDEPARTMENTID, Post.OWNERCOMPANYID);
            }

            RefreshUI(RefreshedTypes.All);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("POSTINFO");
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    closeFormFlag = true;
                    Save();
                    // Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            //NavigateItem item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("DETAILINFO"),
            //    Tooltip = Utility.GetResourceStr("DETAILINFO")
            //};
            //items.Add(item);
            //if (postID != "")
            //{
            //    item = new NavigateItem
            //    {
            //        Title = Utility.GetResourceStr("RELATIONPOST"),
            //        Tooltip = Utility.GetResourceStr("RELATIONPOST"),
            //        Url = "/Organization/RelationPost.xaml?PostID=" + postID
            //    };
            //    items.Add(item);
            //}
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            try
            {
                if (OnUIRefreshed != null)
                {
                    ///读取缓存里面存入的操作，如果为编辑状态那就隐藏审核信息，缓存在待办打开时存入
                    string strCurrentAction = Application.Current.Resources["CurrentActionInfo"] as string;
                    Refresh(type);
                    //if (strCurrentAction == "FormTypes.Edit")
                    //{
                    if (this.post != null && this.post.CHECKSTATE == "0")//未提交的单据
                    {
                        if (strCurrentAction == "FormTypes.Edit")
                        {
                            type = RefreshedTypes.HideAudit;
                            Refresh(type);
                            Application.Current.Resources.Remove("CurrentActionInfo");//清除，受不了了，发了4次更新包了都是这些问题,ps：希望这次能行
                        }
                    }
                    else
                    {
                        type = RefreshedTypes.ShowAudit;
                        Refresh(type);
                        //Application.Current.Resources.Remove("CurrentActionInfo");//清除
                    }
                    // }
                    //else
                    //{
                    //    type = RefreshedTypes.ShowAudit;
                    //    Refresh(type);
                    //    //Application.Current.Resources.Remove("CurrentActionInfo");//清除
                    //}

                }
            }
            catch (Exception ex)//避免错误还能继续执行
            {
                Refresh(type);
            }
        }
        /// <summary>
        /// 负责刷新
        /// </summary>
        /// <param name="type"></param>
        private void Refresh(RefreshedTypes type)
        {
            UIRefreshedEventArgs args = new UIRefreshedEventArgs();
            args.RefreshedType = type;
            OnUIRefreshed(this, args);
        }


        #endregion

        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_POST Info)
        {

            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;

            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ownerCompany = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(s => s.COMPANYID == Info.OWNERCOMPANYID).FirstOrDefault();
            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT ownerDepartment = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(s => s.DEPARTMENTID == Info.OWNERDEPARTMENTID).FirstOrDefault();
            SMT.Saas.Tools.OrganizationWS.T_HR_POST ownerPost = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(s => s.POSTID == Info.OWNERPOSTID).FirstOrDefault();
            string ownerCompanyName = string.Empty;
            string ownerDepartmentName = string.Empty;
            string ownerPostName = string.Empty;
            if (ownerCompany != null)
            {
                ownerCompanyName = ownerCompany.CNAME;
            }
            if (ownerDepartment != null)
            {
                ownerDepartmentName = ownerDepartment.T_HR_DEPARTMENTDICTIONARY == null ? "" : ownerDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            }
            if (ownerPost != null)
            {
                ownerPostName = ownerPost.T_HR_POSTDICTIONARY == null ? "" : ownerPost.T_HR_POSTDICTIONARY.POSTNAME;
            }

            T_HR_POST fatherPost = lkPost.DataContext as T_HR_POST;
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_POST", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_POST", "FATHERPOSTID", Info.FATHERPOSTID, fatherPost == null ? "" : fatherPost.T_HR_POSTDICTIONARY.POSTNAME));
            AutoList.Add(basedata("T_HR_POST", "POSTNAME", Info.T_HR_POSTDICTIONARY.POSTNAME, Info.T_HR_POSTDICTIONARY.POSTNAME));
            AutoList.Add(basedata("T_HR_POST", "POSTDICTIONARYID", Info.T_HR_POSTDICTIONARY.POSTDICTIONARYID, Info.T_HR_POSTDICTIONARY.POSTNAME));
            AutoList.Add(basedata("T_HR_POST", "DEPARTMENTID", Info.T_HR_DEPARTMENT.DEPARTMENTID, lkDepart.TxtLookUp.Text));
            AutoList.Add(basedata("T_HR_POST", "POSTCODE", txtPosCode.Text, txtPosCode.Text));
            AutoList.Add(basedata("T_HR_POST", "UNDERNUMBER", txtManageNmber.Value.ToString(), ""));
            AutoList.Add(basedata("T_HR_POST", "POSTNUMBER", nuPostNumber.Value.ToString(), ""));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            AutoList.Add(basedata("T_HR_POST", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_POST", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_POST", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
            string a = mx.TableToXml(Info, null, StrSource, AutoList);

            return a;
        }

        private AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }

        #endregion
        #region IAudit
        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            //if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID != Post.OWNERID&&Post.CHECKSTATE==Convert.ToInt32(CheckStates.UnSubmit).ToString())
            //{
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
            //        Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return;
            //}
            if (FormType == FormTypes.Resubmit && canSubmit == false)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("请先保存修改的记录"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("POSTNAME", Post.T_HR_POSTDICTIONARY.POSTNAME);
            para.Add("CREATEUSERNAME", createUserName);
            para.Add("OWNER", createUserName);


            //Dictionary<string, string> para2 = new Dictionary<string, string>();
            //para2.Add("T_HR_DEPARTMENTReference", Post.T_HR_DEPARTMENT == null ? "" : Post.T_HR_DEPARTMENT.DEPARTMENTID + "#" + Post.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);
            //para2.Add("T_HR_POSTDICTIONARYReference", Post.T_HR_POSTDICTIONARY == null ? "" : Post.T_HR_POSTDICTIONARY.POSTDICTIONARYID + "#" + Post.T_HR_POSTDICTIONARY.POSTNAME);
            //para2.Add("FATHERPOSTID", (lkPost.DataContext as T_HR_POST) == null ? "" : (lkPost.DataContext as T_HR_POST).POSTID + "#" + (lkPost.DataContext as T_HR_POST).T_HR_POSTDICTIONARY.POSTNAME);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_POST>(Post, para, "HR", para2, null);
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, Post);
            Utility.SetAuditEntity(entity, "T_HR_POST", Post.POSTID, strXmlObjectSource);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            string state = "";
            string strMsg = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    //if (FormType == FormTypes.Audit && post.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    //{
                    //    state = Utility.GetCheckState(CheckStates.UnApproved);
                    //    post.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    //}
                    //else
                    //{
                    //    state = Utility.GetCheckState(CheckStates.Approved);
                    //    post.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    //}
                    state = Utility.GetCheckState(CheckStates.Approved);
                    if (post.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        post.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                    }
                    else
                    {
                        Post.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    }
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    if (Post.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        state = Utility.GetCheckState(CheckStates.Approved);
                        Post.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    }
                    else
                    {
                        state = Utility.GetCheckState(CheckStates.UnApproved);
                        Post.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    }
                    //state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (Post.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            Post.CHECKSTATE = state;
            if (UserState.ToString() == "Audit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else if (UserState.ToString() == "Submit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            // client.PostUpdateAsync(Post, strMsg, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (Post != null)
                state = Post.CHECKSTATE;
            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion
        #region 保存
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        private bool Save()
        {
            // List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            //if (validators.Count > 0)
            //{
            //    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}
            string strMsg = "";
            //部门赋值
            T_HR_DEPARTMENT dep = lkDepart.DataContext as T_HR_DEPARTMENT;
            if (dep != null)
            {
                Post.T_HR_DEPARTMENT = new T_HR_DEPARTMENT();
                Post.T_HR_DEPARTMENT.DEPARTMENTID = dep.DEPARTMENTID;
                Post.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                Post.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID;
                post.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "DEPARTMENTNAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }

            //岗位字典赋值
            T_HR_POSTDICTIONARY pos = cbxPosition.SelectedItem as T_HR_POSTDICTIONARY;
            if (pos == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "POSTNAME"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (this.rbtYes.IsChecked == true)
            {
                Post.PROSCENIUM = "1";
            }
            else
            {
                Post.PROSCENIUM = "0";
            }
            if (this.RbtNo.IsChecked == true)
            {
                Post.BACKSTAGE = "1";
            }
            else
            {
                Post.BACKSTAGE = "0";
            }

            //是否为核心人员
            if (rbtYesCore.IsChecked == true)
            {
                Post.ISCOREPERSONNEL = "0";
            }
            else
            {
                Post.ISCOREPERSONNEL = "1";
            }

            if (FormType == FormTypes.New)
            {
                //所属
                Post.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                Post.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                Post.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                Post.OWNERCOMPANYID = Post.COMPANYID;
                Post.OWNERDEPARTMENTID = Post.T_HR_DEPARTMENT.DEPARTMENTID;
                Post.OWNERPOSTID = Post.POSTID;
                Post.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                Post.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                Post.CREATEDATE = DateTime.Now;
                Post.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                Post.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();

                post.POSTNUMBER = Convert.ToInt32(nuPostNumber.Value);
                //System.Xml.Serialization.XmlSerializer ser = new XmlSerializer(typeof(T_HR_POST));
                //ser.Serialize(System.IO.File.Create("C:\\x.xml"), Post);

                client.PostAddAsync(Post, strMsg);
            }
            else
            {
                //如果状态为审核通过，修改时，则修改状态为审核中
                if (Post.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    Post.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    Post.CHECKSTATE = Convert.ToInt32(CheckStates.Approving).ToString();
                }
                Post.UPDATEDATE = DateTime.Now;
                Post.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.PostUpdateAsync(Post, strMsg, "Edit");
            }

            return true;
        }



        //public void Cancel()
        //{
        //    //if (MessageBox.Show(Utility.GetResourceStr("CANCELCONFIRM"), Utility.GetResourceStr("CANCEL"), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        //    //{
        //    //    //提交撤消时，审核状态改为审核中
        //    //    Post.EDITSTATE = Convert.ToInt32(EditStates.PendingCanceled).ToString();
        //    //    Post.CHECKSTATE = Convert.ToInt32(CheckStates.Approving).ToString();
        //    //    client.PostCancelAsync(Post);
        //    //}
        //}
        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            //EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            //entBrowser.Close();
        }
        #endregion
        #region
        void client_PostDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //if (e.Error != null && e.Error.Message != "")
            //{
            //    MessageBox.Show(e.Error.Message);
            //}
            //else
            //{
            //    MessageBox.Show(Utility.GetResourceStr("DELETESUCCESSED"));
            //}
        }

        void client_PostCancelCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //if (e.Error != null && e.Error.Message != "")
            //{
            //    MessageBox.Show(e.Error.Message);
            //}
            //else
            //{
            //    MessageBox.Show(Utility.GetResourceStr("CANCELLEDSUCCESSED"));
            //}
        }
        public void delete(string strid)
        {
            //if (!string.IsNullOrEmpty(strid))
            //{
            //    client.PostDeleteAsync(strid);
            //}
        }

        public void SubmitAudit()
        {
            //if (Post != null)
            //{
            //    Post.CHECKSTATE = Convert.ToInt32(CheckStates.Approving).ToString();
            //    Post.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
            //    Post.UPDATEDATE = DateTime.Now;
            //    Post.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    client.PostUpdateAsync(Post, "SubmitAudit");
            //}
        }

        public void Audit()
        {
            //T_HR_POSTHISTORY posHis = new T_HR_POSTHISTORY();
            //posHis.RECORDSID = Guid.NewGuid().ToString();
            //posHis.POSTID = Post.POSTID;
            //posHis.T_HR_POSTDICTIONARY = Post.T_HR_POSTDICTIONARY;
            //posHis.DEPARTMENTID = Post.T_HR_DEPARTMENT.DEPARTMENTID;
            //posHis.POSTFUNCTION = Post.POSTFUNCTION;
            //posHis.POSTNUMBER = Post.POSTNUMBER;
            //posHis.POSTGOAL = Post.POSTGOAL;
            ////posHis.CHECKUSER = Post.CHECKUSER;
            //posHis.FATHERPOSTID = Post.FATHERPOSTID;
            //posHis.UNDERNUMBER = Post.UNDERNUMBER;
            //posHis.PROMOTEDIRECTION = Post.PROMOTEDIRECTION;
            //posHis.CHANGEPOST = Post.CHANGEPOST;
            ////posHis.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            //if (Post.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
            //    posHis.CANCELDATE = DateTime.Now;
            //posHis.REUSEDATE = DateTime.Now;
            //posHis.CREATEDATE = Post.CREATEDATE;
            //posHis.CREATEUSERID = Post.CREATEUSERID;
            //posHis.UPDATEDATE = DateTime.Now;
            //posHis.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //client.PostHistoryAddAsync(posHis);
        }

        void client_PostHistoryAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //if (e.Error != null)
            //{
            //    MessageBox.Show(e.Error.Message);
            //}
            //else
            //{
            //    if (Post.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
            //    {
            //        Post.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
            //        Post.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
            //    }
            //    else
            //    {
            //        Post.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
            //        Post.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            //    }
            //    Post.UPDATEDATE = DateTime.Now;
            //    //Post.CHECKUSER = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    Post.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    client.PostUpdateAsync(Post, "Audit");
            //}

        }
        //private void HandleDepartChanged(T_HR_DEPARTMENT ent)
        //{
        //    if (ent != null && ent.T_HR_COMPANY != null)
        //    {
        //        //txtOrgName.Text = ent.T_HR_COMPANY.CNAME;
        //        Post.COMPANYID = ent.T_HR_COMPANY.COMPANYID;
        //        //绑定岗位字典
        //        client.GetPostDictionaryAllAsync();
        //    }
        //}
        //private void LookUp_FindClick(object sender, EventArgs e)
        //{
        //    Dictionary<string, string> cols = new Dictionary<string, string>();
        //    cols.Add("DEPARTMENTCODE", "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE");
        //    cols.Add("DEPARTMENTNAME", "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME");

        //    LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Department,
        //        typeof(T_HR_DEPARTMENT[]), cols);

        //    lookup.SelectedClick += (o, ev) =>
        //    {
        //        T_HR_DEPARTMENT ent = lookup.SelectedObj as T_HR_DEPARTMENT;

        //        if (ent != null)
        //        {
        //            lkDepart.DataContext = ent;
        //            HandleDepartChanged(ent);
        //        }
        //    };

        //    lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        //}
        #endregion

        #region 单选按钮
        private void RbtNo_Click(object sender, RoutedEventArgs e)
        {
            this.RbtNo.IsChecked = true;
            this.rbtYes.IsChecked = false;
        }

        private void rbtYes_Click(object sender, RoutedEventArgs e)
        {
            this.rbtYes.IsChecked = true;
            this.RbtNo.IsChecked = false;
        }
        #endregion


        #region 单选是否为核心按钮
        private void rbtYesCore_Click(object sender, RoutedEventArgs e)
        {
            rbtNoCore.IsChecked = false;
            rbtYesCore.IsChecked = true;
        }
        private void rbtNoCore_Click(object sender, RoutedEventArgs e)
        {
            rbtNoCore.IsChecked = true;
            rbtYesCore.IsChecked = false;
        }
        #endregion
        private void cbxPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxPosition.SelectedItem != null)
            {
                T_HR_POSTDICTIONARY dict = cbxPosition.SelectedItem as T_HR_POSTDICTIONARY;
                if (dict != null)
                {
                    if (dict.EDITSTATE != "1")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("岗位字典无效"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        cbxPosition.SelectedItem = null;
                        return;
                    }
                    Post.T_HR_POSTDICTIONARY = new T_HR_POSTDICTIONARY();
                    Post.T_HR_POSTDICTIONARY.POSTDICTIONARYID = dict.POSTDICTIONARYID;
                    post.T_HR_POSTDICTIONARY.POSTCODE = dict.POSTCODE;
                    Post.T_HR_POSTDICTIONARY.POSTNAME = dict.POSTNAME;
                    txtPosCode.Text = dict.POSTCODE;


                    if (FormType == FormTypes.New)
                    {
                        Post.POSTFUNCTION = dict.POSTFUNCTION;
                        Post.PROMOTEDIRECTION = dict.PROMOTEDIRECTION;
                        Post.CHANGEPOST = dict.CHANGEPOST;
                    }

                }
            }
        }

        /// <summary>
        /// 选择上级岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LookUp_FindClick_1(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.Post;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_POST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                if (ent != null)
                {
                    lkPost.DataContext = ent;
                    post.FATHERPOSTID = ent.POSTID;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void LookCompany_FindClick(object sender, EventArgs e)
        {
        }

        private void lkDepart_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                if (ent != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY com = ent.T_HR_COMPANY;
                    lkCompany.DataContext = com;
                    if (com != null)
                    {
                        Post.COMPANYID = com.COMPANYID;
                    }
                    lkDepart.DataContext = ent;
                    if (ent.T_HR_DEPARTMENTDICTIONARY != null)
                    {
                        client.GetPostDictionaryByDepartmentDictionayIDAsync(ent.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID, "setDepartment");
                    }
                    lkDepart.TxtLookUp.Text = GetFullOrgName(ent);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }
        public string GetFullOrgName(SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT dep)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = dep;
            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
            string orgName = string.Empty;
            string fatherType = "0";
            string fatherID = "";
            bool hasFather = false;

            if (department != null)
            {
                orgName += department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                {
                    fatherType = department.FATHERTYPE;
                    fatherID = department.FATHERID;
                    hasFather = true;
                }
                else
                {
                    hasFather = false;
                }
            }

            while (hasFather)
            {
                if (fatherType == "1" && !string.IsNullOrEmpty(fatherID))
                {
                    department = (from de in allDepartments
                                  where de.DEPARTMENTID == fatherID
                                  select de).FirstOrDefault();
                    if (department != null)
                    {
                        orgName += " - " + department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                        {
                            fatherID = department.FATHERID;
                            fatherType = department.FATHERTYPE;
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else
                    {
                        hasFather = false;
                    }
                }
                else if (fatherType == "0" && !string.IsNullOrEmpty(fatherID))
                {
                    company = (from com in allCompanys
                               where com.COMPANYID == fatherID
                               select com).FirstOrDefault();

                    if (company != null)
                    {
                        orgName += " - " + company.CNAME;
                        hasFather = false;
                        //if (!string.IsNullOrEmpty(company.FATHERTYPE) && !string.IsNullOrEmpty(company.FATHERID))
                        //{
                        //    fatherID = company.FATHERID;
                        //    fatherType = company.FATHERTYPE;
                        //}
                        //else
                        //{
                        //    hasFather = false;
                        //}
                    }
                    else
                    {
                        hasFather = false;
                    }

                }
                else
                {
                    hasFather = false;
                }

            }
            return orgName;
        }
    }
}
