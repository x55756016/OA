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
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.MobileXml;

namespace SMT.HRM.UI.Form.Organization
{
    public partial class DepartmentDictionaryForms : BaseForm, IEntityEditor, IClient, IAudit
    {
        #region 初始化
        private T_HR_DEPARTMENTDICTIONARY departmentDictionary;

        public T_HR_DEPARTMENTDICTIONARY DepartmentDictionary
        {
            get { return departmentDictionary; }
            set
            {
                departmentDictionary = value;
                this.DataContext = value;
            }
        }
        private FormTypes FormType { set; get; }
        private string createUserName;
        OrganizationServiceClient client;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public bool needsubmit = false;//提交审核
        private bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient personClient;
        private string departmentdictid;
        public DepartmentDictionaryForms(FormTypes type, string depID)
        {
            InitializeComponent();
            FormType = type;
            this.departmentdictid = depID;
            InitParas(depID);
        }

        private void InitParas(string depID)
        {
            client = new OrganizationServiceClient();
            personClient = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
            client.DepartmentDictionaryUpdateCompleted += new EventHandler<DepartmentDictionaryUpdateCompletedEventArgs>(client_DepartmentDictionaryUpdateCompleted);
            client.DepartmentDictionaryAddCompleted += new EventHandler<DepartmentDictionaryAddCompletedEventArgs>(client_DepartmentDictionaryAddCompleted);
            client.GetDepartmentDictionaryByIdCompleted += new EventHandler<GetDepartmentDictionaryByIdCompletedEventArgs>(client_GetDepartmentDictionaryByIdCompleted);
            personClient.GetEmployeeToEngineCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeeToEngineCompletedEventArgs>(personClient_GetEmployeeToEngineCompleted);
            this.Loaded += new RoutedEventHandler(DepartmentDictionaryForms_Loaded);
            #region 原来的
            /*
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnableControl();

            }
            */
            #endregion
        }

        void DepartmentDictionaryForms_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnableControl();

            }
            #endregion
            if (FormType == FormTypes.New)
            {
                DepartmentDictionary = new T_HR_DEPARTMENTDICTIONARY();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                DepartmentDictionary.DEPARTMENTDICTIONARYID = Guid.NewGuid().ToString();
                DepartmentDictionary.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetDepartmentDictionaryByIdAsync(departmentdictid);
            }

            //Load事件之后，加载完后获取到父控件
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
        }

        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            isSubmit = true;
            needsubmit = true;
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            AddToClose(false);
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
            txtDepart.IsReadOnly = true;
            txtDepID.IsReadOnly = true;
            txtDepName.IsReadOnly = true;
            cbxDepartmentLevel.IsEnabled = false;
            cbxDepartmentType.IsEnabled = false;

        }
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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_DEPARTMENTDICTIONARY", DepartmentDictionary.OWNERID,
                    DepartmentDictionary.OWNERPOSTID, DepartmentDictionary.OWNERDEPARTMENTID, DepartmentDictionary.OWNERCOMPANYID);
            }

            RefreshUI(RefreshedTypes.All);
        }
        #endregion
        #region 完成事件
        /// <summary>
        /// 根据ID获取部门字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetDepartmentDictionaryByIdCompleted(object sender, GetDepartmentDictionaryByIdCompletedEventArgs e)
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
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                DepartmentDictionary = e.Result;
                if (FormType == FormTypes.Resubmit)
                {
                    DepartmentDictionary.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    DepartmentDictionary.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                }
                if (DepartmentDictionary.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    EnableControl();
                }
                if (DepartmentDictionary.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                   || DepartmentDictionary.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                    CreateUserIDs.Add(DepartmentDictionary.CREATEUSERID);
                    personClient.GetEmployeeToEngineAsync(CreateUserIDs);
                }
                foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxDepartmentType.ItemsSource)
                {
                    if (item.DICTIONARYVALUE.ToString() == DepartmentDictionary.DEPARTMENTTYPE)
                    {
                        cbxDepartmentType.SelectedItem = item;
                        break;
                    }
                }
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
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        /// <summary>
        /// 薪增部门字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DepartmentDictionaryAddCompleted(object sender, DepartmentDictionaryAddCompletedEventArgs e)
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
                    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "DEPARTMENTCODE,DEPARTMENTNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "DEPARTMENTCODE,DEPARTMENTNAME"),
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

        /// <summary>
        /// 修改部门字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DepartmentDictionaryUpdateCompleted(object sender, DepartmentDictionaryUpdateCompletedEventArgs e)
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
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "DEPARTMENTCODE,DEPARTMENTNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "DEPARTMENTCODE,DEPARTMENTNAME"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
                //      Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //RefreshUI(RefreshedTypes.All);
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

        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("DEPARTMENTDICTIONARY");
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
                    AddToClose(false);
                    break;
                case "1":
                    closeFormFlag = true;
                    AddToClose(true);
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
        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            //Dictionary<string, string> para = new Dictionary<string, string>();
            //para.Add("CREATEUSERNAME", createUserName);
            //para.Add("OWNER", createUserName);

            //Dictionary<string, string> para2 = new Dictionary<string, string>();
            //para2.Add("COMPANYTYPE", (cbxDepartmentType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY) == null ? "" : (cbxDepartmentType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYNAME);
            //para2.Add("DEPARTMENTLEVEL", (cbxDepartmentLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY) == null ? "" : (cbxDepartmentLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYNAME);

            //string strXmlObjectSource = string.Empty;
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_DEPARTMENTDICTIONARY>(DepartmentDictionary, para, "HR", para2, null);
            //Utility.SetAuditEntity(entity, "T_HR_DEPARTMENTDICTIONARY", DepartmentDictionary.DEPARTMENTDICTIONARYID, strXmlObjectSource);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_POST>(Post, para, "HR", para2, null);
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, DepartmentDictionary);
            Utility.SetAuditEntity(entity, "T_HR_DEPARTMENTDICTIONARY", DepartmentDictionary.DEPARTMENTDICTIONARYID, strXmlObjectSource);
        }
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
                    if (DepartmentDictionary.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        DepartmentDictionary.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                    }
                    else
                    {
                        DepartmentDictionary.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    }
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (DepartmentDictionary.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            DepartmentDictionary.CHECKSTATE = state;

            client.DepartmentDictionaryUpdateAsync(DepartmentDictionary, strMsg, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (DepartmentDictionary != null)
                state = DepartmentDictionary.CHECKSTATE;

            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }

        #endregion

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_DEPARTMENTDICTIONARY Info)
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
            AutoList.Add(basedata("T_HR_DEPARTMENTDICTIONARY", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_DEPARTMENTDICTIONARY", "CREATEUSERNAME", createUserName, createUserName));
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
        #region 保存
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="isCloseForm"></param>
        public void AddToClose(bool isCloseForm)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                needsubmit = false;
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            else if (cbxDepartmentType.SelectedIndex < 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "DEPARTMENTTYPE"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                cbxDepartmentType.Focus();
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            else
            {
                string strMsg = "";
                DepartmentDictionary.DEPARTMENTTYPE = (cbxDepartmentType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
                if (FormTypes.New == this.FormType)
                {
                    DepartmentDictionary.CREATEDATE = System.DateTime.Now;
                    ///TODO增加操作人
                    DepartmentDictionary.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    DepartmentDictionary.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    DepartmentDictionary.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    DepartmentDictionary.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    DepartmentDictionary.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    DepartmentDictionary.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    client.DepartmentDictionaryAddAsync(this.DepartmentDictionary, strMsg);
                }
                else
                {
                    DepartmentDictionary.UPDATEDATE = System.DateTime.Now;
                    ///TODO增加修改人
                    DepartmentDictionary.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    client.DepartmentDictionaryUpdateAsync(this.DepartmentDictionary, strMsg, "Edit");
                }
            }

        }
        #endregion

        private void cbxDepartmentType_Loaded(object sender, RoutedEventArgs e)
        {
            List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
            dicts = dicts.Where(s => s.DICTIONCATEGORY == "COMPANYTYPE").OrderBy(s => s.DICTIONARYVALUE).ToList();
            cbxDepartmentType.ItemsSource = dicts.ToList();
            cbxDepartmentType.DisplayMemberPath = "DICTIONARYNAME";
        }
    }
}
