using System;
using System.Collections.Generic;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI.AuditControl;
using System.Windows;
using System.Linq;
using SMT.SaaS.MobileXml;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.AutoCompleteComboBox;
namespace SMT.HRM.UI.Form.Organization
{
    public partial class PostDictionaryForms : BaseForm, IEntityEditor, IClient, IAudit
    {
        #region 初始化
        private T_HR_POSTDICTIONARY postDictionary;

        public T_HR_POSTDICTIONARY PostDictionary
        {
            get { return postDictionary; }
            set
            {
                postDictionary = value;
                this.DataContext = value;
            }
        }
        private FormTypes FormType { set; get; }
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        OrganizationServiceClient client;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient personClient;
        public bool needsubmit = false;//提交审核
        private bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private string postdictionaryid;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="postID"></param>
        public PostDictionaryForms(FormTypes type, string postID)
        {
            InitializeComponent();
            FormType = type;
            postdictionaryid = postID;
            InitParas(postID);
        }

        private void InitParas(string postID)
        {
            client = new OrganizationServiceClient();
            personClient = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
            client.PostDictionaryAddCompleted += new EventHandler<PostDictionaryAddCompletedEventArgs>(client_PostDictionaryAddCompleted);
            client.PostDictionaryUpdateCompleted += new EventHandler<PostDictionaryUpdateCompletedEventArgs>(client_PostDictionaryUpdateCompleted);
            client.GetPostDictionaryByIdCompleted += new EventHandler<GetPostDictionaryByIdCompletedEventArgs>(client_GetPostDictionaryByIdCompleted);
            client.GetDepartmentDictionaryAllCompleted += new EventHandler<GetDepartmentDictionaryAllCompletedEventArgs>(client_GetDepartmentDictionaryAllCompleted);
            personClient.GetEmployeeToEngineCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeeToEngineCompletedEventArgs>(personClient_GetEmployeeToEngineCompleted);
            this.Loaded += new RoutedEventHandler(PostDictionaryForms_Loaded);
            #region 原来的
            /*
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnableControl();
            }
            */
            #endregion
            ///TODO部门轮换岗位
        }

        void PostDictionaryForms_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnableControl();
            }
            #endregion
            if (FormType == FormTypes.New)
            {
                PostDictionary = new T_HR_POSTDICTIONARY();
                PostDictionary.POSTDICTIONARYID = Guid.NewGuid().ToString();
                PostDictionary.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                client.GetDepartmentDictionaryAllAsync();
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetPostDictionaryByIdAsync(postdictionaryid);
            }

            //Load事件之后，加载完后获取到父控件
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
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
            AddToClose();
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
        void EnableControl()
        {
            txtPosCode.IsReadOnly = true;
            txtPosName.IsReadOnly = true;
            //cbxDepName.IsEnabled = false;
            acbDepName.IsEnabled = false;
            txtPostFunction.IsReadOnly = true;
            txtPrompteDirection.IsReadOnly = true;
        }

        #endregion

        #region 完成事件
        /// <summary>
        /// 根据id获取岗位字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetPostDictionaryByIdCompleted(object sender, GetPostDictionaryByIdCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                PostDictionary = e.Result;
                if (FormType == FormTypes.Resubmit)
                {
                    PostDictionary.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    PostDictionary.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    acbDepName.IsEnabled = false;
                }
                if (PostDictionary.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    EnableControl();
                }
                if (PostDictionary.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                   || PostDictionary.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                }
                else
                {
                    System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                    CreateUserIDs.Add(PostDictionary.CREATEUSERID);
                    personClient.GetEmployeeToEngineAsync(CreateUserIDs);
                }
                client.GetDepartmentDictionaryAllAsync();
            }
        }
        /// <summary>
        /// 获取创建人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        }
        /// <summary>
        /// 获取部门字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetDepartmentDictionaryAllCompleted(object sender, GetDepartmentDictionaryAllCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                var entity = from ent in e.Result
                             orderby ent.DEPARTMENTNAME
                                 select ent;
                List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
                dicts = dicts.Where(s => s.DICTIONCATEGORY == "COMPANYTYPE").OrderBy(s => s.DICTIONARYVALUE).ToList();

                foreach (T_HR_DEPARTMENTDICTIONARY diction in entity)
                {
                    decimal dptype = Convert.ToDecimal(diction.DEPARTMENTTYPE);
                    var tmp = dicts.Where(s => s.DICTIONARYVALUE == dptype).FirstOrDefault();
                    if (tmp != null)
                    {
                        diction.DEPARTMENTNAME = diction.DEPARTMENTNAME + "(" + tmp.DICTIONARYNAME + ")";
                    }
                }

                //ComboBox数据源
                //cbxDepName.ItemsSource = entity;
                //cbxDepName.DisplayMemberPath = "DEPARTMENTNAME";

                //AutoCompleteBox数据源
                acbDepName.ItemsSource = entity;
                acbDepName.ValueMemberPath = "DEPARTMENTNAME";
               
                if (PostDictionary.T_HR_DEPARTMENTDICTIONARY != null)
                {
                    foreach (var dep in acbDepName.ItemsSource)
                    {
                        T_HR_DEPARTMENTDICTIONARY ent = dep as T_HR_DEPARTMENTDICTIONARY;
                        if (ent.DEPARTMENTDICTIONARYID == PostDictionary.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID)
                        {
                            //cbxDepName.SelectedItem = dep;
                            acbDepName.SelectedItem = dep;
                            break;
                        }
                    }
                }
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

       
        /// <summary>
        /// 修改岗位字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_PostDictionaryUpdateCompleted(object sender, PostDictionaryUpdateCompletedEventArgs e)
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
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "POSTCODE,POSTNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "POSTCODE,POSTNAME"),
                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                if (e.strMsg == "Repetition")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "COMPANRYCODE,CNAME"),
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
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                }

            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 新增岗位字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void client_PostDictionaryAddCompleted(object sender, PostDictionaryAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.strMsg == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "POSTCODE,POSTNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "POSTCODE,POSTNAME"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
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

        #endregion
        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_COMPANY", Company.OWNERID,
            //        Company.OWNERPOSTID, Company.OWNERDEPARTMENTID, Company.OWNERCOMPANYID);

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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_POSTDICTIONARY", PostDictionary.OWNERID,
                    PostDictionary.OWNERPOSTID, PostDictionary.OWNERDEPARTMENTID, PostDictionary.OWNERCOMPANYID);
            }

            RefreshUI(RefreshedTypes.All);
        }
        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("POSTDICTIONARY");
        }

        public string GetStatus()
        {
            return "编辑中";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    AddToClose();
                    break;
                case "1":
                    closeFormFlag = true;
                    Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);

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
        private string createUserName;
        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            //Dictionary<string, string> para = new Dictionary<string, string>();
            //para.Add("CREATEUSERNAME", createUserName);
            //para.Add("OWNER", createUserName);


            //Dictionary<string, string> para2 = new Dictionary<string, string>();
            //para2.Add("T_HR_DEPARTMENTDICTIONARYReference", PostDictionary.T_HR_DEPARTMENTDICTIONARY == null ? "" : PostDictionary.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID + "#" + PostDictionary.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);

            //string strXmlObjectSource = string.Empty;
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_POSTDICTIONARY>(PostDictionary, para, "HR", para2, null);
            //Utility.SetAuditEntity(entity, "T_HR_POSTDICTIONARY", PostDictionary.POSTDICTIONARYID, strXmlObjectSource);


            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_POST>(Post, para, "HR", para2, null);
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, PostDictionary);
            Utility.SetAuditEntity(entity, "T_HR_POSTDICTIONARY", PostDictionary.POSTDICTIONARYID, strXmlObjectSource);
        }

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_POSTDICTIONARY Info)
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

            //T_HR_POST fatherPost = lkPost.DataContext as T_HR_POST;
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_POSTDICTIONARY", "CHECKSTATE", "1", checkState));
            //AutoList.Add(basedata("T_HR_POST", "FATHERPOSTID", Info.FATHERPOSTID, fatherPost == null ? "" : fatherPost.T_HR_POSTDICTIONARY.POSTNAME));
            //AutoList.Add(basedata("T_HR_POST", "POSTNAME", Info.T_HR_POSTDICTIONARY.POSTNAME, Info.T_HR_POSTDICTIONARY.POSTNAME));
            //AutoList.Add(basedata("T_HR_POST", "POSTDICTIONARYID", Info.T_HR_POSTDICTIONARY.POSTDICTIONARYID, Info.T_HR_POSTDICTIONARY.POSTNAME));
            //AutoList.Add(basedata("T_HR_POST", "DEPARTMENTID", Info.T_HR_DEPARTMENT.DEPARTMENTID, lkDepart.TxtLookUp.Text));
            //AutoList.Add(basedata("T_HR_POST", "POSTCODE", txtPosCode.Text, txtPosCode.Text));
            //AutoList.Add(basedata("T_HR_POST", "UNDERNUMBER", txtManageNmber.Value.ToString(), ""));
            //AutoList.Add(basedata("T_HR_POST", "POSTNUMBER", nuPostNumber.Value.ToString(), ""));
            ////AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
            ////AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
            ////AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            ////AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            ////AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_HR_POST", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            //AutoList.Add(basedata("T_HR_POST", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            //AutoList.Add(basedata("T_HR_POST", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
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

        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            // RefreshUI(RefreshedTypes.ProgressBar);
            string state = "";
            string UserState = "Audit";
            string strMsg = "";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    if (PostDictionary.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        PostDictionary.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                    }
                    else
                    {
                        PostDictionary.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    }
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (PostDictionary.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            PostDictionary.CHECKSTATE = state;

            client.PostDictionaryUpdateAsync(PostDictionary, strMsg, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (PostDictionary != null)
                state = PostDictionary.CHECKSTATE;

            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
            personClient.DoClose();
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
        #region 保存
        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = AddToClose();

            if (!flag)
            {
                return;
            }
            //EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            //entBrowser.Close();
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public bool AddToClose()
        {
            // List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            string strMsg = "";
            RefreshUI(RefreshedTypes.ShowProgressBar);
            //if (validators.Count > 0)
            //{
            //    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                needsubmit = false;
                return false;
            }
            //if (cbxDepName.SelectedItem == null)
            //{
            //    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "DEPARTMENTNAME"));
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "DEPARTMENTNAME"),
            //      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}
            //else
            //{
            //    T_HR_DEPARTMENTDICTIONARY ent = cbxDepName.SelectedItem as T_HR_DEPARTMENTDICTIONARY;
            //    PostDictionary.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
            //    postDictionary.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = ent.DEPARTMENTDICTIONARYID;
            //}
            if (acbDepName.SelectedItem == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "DEPARTMENTNAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "DEPARTMENTNAME"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            else
            {
                T_HR_DEPARTMENTDICTIONARY ent = acbDepName.SelectedItem as T_HR_DEPARTMENTDICTIONARY;
                PostDictionary.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                postDictionary.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = ent.DEPARTMENTDICTIONARYID;
            }


            if (FormTypes.New == this.FormType)
            {
                PostDictionary.CREATEDATE = System.DateTime.Now;
                ///TODO增加操作人
                PostDictionary.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                PostDictionary.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                PostDictionary.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                PostDictionary.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                PostDictionary.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                PostDictionary.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                client.PostDictionaryAddAsync(PostDictionary, strMsg);
            }
            else
            {
                PostDictionary.UPDATEDATE = System.DateTime.Now;
                ///TODO增加修改人
                PostDictionary.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.PostDictionaryUpdateAsync(PostDictionary, strMsg, "Edit");
            }

            return true;
        }
        #endregion

       

       
    }
}
