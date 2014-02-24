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
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;
namespace SMT.HRM.UI.Form
{
    public partial class DepartmentForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        public FormTypes FormType { get; set; }

        private T_HR_DEPARTMENT department;
        public string createUserName;
        public T_HR_DEPARTMENT Department
        {
            get { return department; }
            set
            {
                department = value;
                this.DataContext = department;
            }
        }
        System.Collections.ObjectModel.ObservableCollection<T_HR_DEPARTMENTDICTIONARY> tempList;//部门字典集合
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        private bool canSubmit = false;//能否提交审核
        private string DepartmentName;//保存部门名称
        public bool needsubmit = false;//提交审核
        private bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private bool canSave = true;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        OrganizationServiceClient client;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient pclient = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
        private string departmentid;
        public delegate void refreshGridView();
        public event refreshGridView ReloadDataEvent;
        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建  zwp 2011.10.09
        /// </summary>
        public DepartmentForm()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            departmentid = "";
            InitControlEvent();
        }

        public DepartmentForm(FormTypes formType, string strID)
        {
            InitializeComponent();


            FormType = formType;
            departmentid = strID;
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
        void EnableControl()
        {
            lkFather.IsEnabled = false;
            lkHead.IsEnabled = false;
            txtCompanyName.IsReadOnly = true;
            txtDepartmentCode.IsReadOnly = true;
            txtDepFun.IsReadOnly = true;
            txtRemark.IsReadOnly = true;
            //cbxCheckState.IsEnabled = false;
            //cbxDepartMent.IsEnabled = false;
            acbDepName.IsEnabled = false;
            acbDepName.ToggleButton.IsEnabled = false;
            acbDepName.TxtLookUp.IsReadOnly = true;
            this.rbtYes.IsEnabled = false;
            this.rbtNo.IsEnabled = false;
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue("T_HR_DEPARTMENT", SMT.SaaS.FrameworkUI.Common.Permissions.Edit) < 0)
            {
                btnEditIndex.Visibility = Visibility.Collapsed;
                txtSortNumber.IsEnabled = false;
            }
            txtSortNumber.IsHitTestVisible = false;
            btnEditIndex.IsEnabled = false;
        }
        private void InitControlEvent()
        {
            client = new OrganizationServiceClient();
            client.GetDepartmentDictionaryAllCompleted += new EventHandler<GetDepartmentDictionaryAllCompletedEventArgs>(client_GetDepartmentDictionaryAllCompleted);
            client.DepartmentAddCompleted += new EventHandler<DepartmentAddCompletedEventArgs>(client_DepartmentAddCompleted);
            client.DepartmentUpdateCompleted += new EventHandler<DepartmentUpdateCompletedEventArgs>(client_DepartmentUpdateCompleted);
            client.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(client_GetDepartmentByIdCompleted);
            client.GetCompanyByIdCompleted += new EventHandler<GetCompanyByIdCompletedEventArgs>(client_GetCompanyByIdCompleted);
            client.GetPostByIdCompleted += new EventHandler<GetPostByIdCompletedEventArgs>(client_GetPostByIdCompleted);
            client.DepartmentIndexUpdateCompleted += new EventHandler<DepartmentIndexUpdateCompletedEventArgs>(client_DepartmentIndexUpdateCompleted);
            client.GetDepartmentActivedByCompanyIDCompleted += new EventHandler<GetDepartmentActivedByCompanyIDCompletedEventArgs>(client_GetDepartmentActivedByCompanyIDCompleted);
            pclient.GetEmployeeToEngineCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeeToEngineCompletedEventArgs>(pclient_GetEmployeeToEngineCompleted);
            this.Loaded += new RoutedEventHandler(DepartmentForm_Loaded);
            client.DepartmentDeleteCompleted += new EventHandler<DepartmentDeleteCompletedEventArgs>(client_DepartmentDeleteCompletedEventArgs);
        
        }

        public void client_DepartmentDeleteCompletedEventArgs(object sender, DepartmentDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.strMsg != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    closeForm();
                }
            }
            FormType = FormTypes.Browse;
            RefreshUI(RefreshedTypes.All);
        }


        void DepartmentForm_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新加
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnableControl();
            }
            #endregion
            if (FormType == FormTypes.New)
            {
                Department = new T_HR_DEPARTMENT();
                Department.DEPARTMENTID = Guid.NewGuid().ToString();
                Department.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetDepartmentByIdAsync(departmentid, "");
            }

            if (FormType != FormTypes.Browse)
            {
                //Load事件之后，加载完后获取到父控件
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
                entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            }
        }

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
        /// 获取岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetPostByIdCompleted(object sender, GetPostByIdCompletedEventArgs e)
        {
            T_HR_POST cp = new T_HR_POST();
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }
                cp = e.Result;
                lkHead.DataContext = cp;

            }
        }
        #region
        /// <summary>
        /// 获取员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pclient_GetEmployeeToEngineCompleted(object sender, Saas.Tools.PersonnelWS.GetEmployeeToEngineCompletedEventArgs e)
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
        void pclient_GetEmployeeByIDCompleted(object sender, Saas.Tools.PersonnelWS.GetEmployeeByIDCompletedEventArgs e)
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
                    //  ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
                    //   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //return;
                    createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                }
                else
                {
                    createUserName = e.Result.EMPLOYEECNAME;
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();

            }
        }
        #endregion
        /// <summary>
        /// 获取公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetCompanyByIdCompleted(object sender, GetCompanyByIdCompletedEventArgs e)
        {
            T_HR_COMPANY cp = new T_HR_COMPANY();
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }
                cp = e.Result;

                lkFather.DisplayMemberPath = "CNAME";
                lkFather.DataContext = cp;

            }
        }
        /// <summary>
        /// 修改排序号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DepartmentIndexUpdateCompleted(object sender, DepartmentIndexUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        void client_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
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
                if (e.UserState.ToString() != "FATHER")
                {
                    Department = e.Result;
                    if (FormType == FormTypes.Resubmit)
                    {
                        lkFather.IsEnabled = false;
                        acbDepName.IsEnabled = false;
                        acbDepName.ToggleButton.IsEnabled = false;
                        acbDepName.TxtLookUp.IsReadOnly = true;
                        Department.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                        if (Department.EDITSTATE != Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            Department.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();                      
                        }
                        //Department.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                        //if (Department.EDITSTATE == Convert.ToInt32(EditStates.Actived).ToString())
                        //{
                        //    Department.EDITSTATE = Convert.ToInt32(EditStates.PendingCanceled).ToString();
                        //}
                        //else
                        //{
                        //    Department.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                        //}
                    }

                    //加载是否前台
                    if (Department.ISBACKGROUND == 1)
                    {
                        this.rbtYes.IsChecked = true;
                        this.rbtNo.IsChecked = false;
                    }
                    if (Department.ISBACKGROUND == 0)
                    {
                        this.rbtYes.IsChecked = false;
                        this.rbtNo.IsChecked = true;
                    }

                    if (Department.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        EnableControl();
                    }
                    if (Department.T_HR_COMPANY != null)
                    {
                        txtCompanyName.Text = Department.T_HR_COMPANY.CNAME;
                    }
                    //绑定部门字典
                    client.GetDepartmentDictionaryAllAsync();
                    if (!string.IsNullOrEmpty(Department.FATHERID) && !string.IsNullOrEmpty(Department.FATHERTYPE))
                    {
                        if (Department.FATHERTYPE == "0")
                        {
                            client.GetCompanyByIdAsync(Department.FATHERID);
                        }
                        else
                        {
                            client.GetDepartmentByIdAsync(Department.FATHERID, "FATHER");
                        }
                    }
                    DepartmentName = Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    client.GetPostByIdAsync(Department.DEPARTMENTBOSSHEAD);
                    if (Department.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                   || Department.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        RefreshUI(RefreshedTypes.AuditInfo);
                        SetToolBar();
                        RefreshUI(RefreshedTypes.HideProgressBar);
                    }
                    else
                    {
                        System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                        CreateUserIDs.Add(Department.CREATEUSERID);
                        pclient.GetEmployeeToEngineAsync(CreateUserIDs);
                    }

                }
                else
                {
                    lkFather.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    lkFather.DataContext = e.Result;
                }
                // by luojie
                client.GetDepartmentActivedByCompanyIDAsync(e.Result.FATHERID);
            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                if (Department != null)
                {
                    if (Department.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_DEPARTMENT", Department.OWNERID,
                    Department.OWNERPOSTID, Department.OWNERDEPARTMENTID, Department.OWNERCOMPANYID);
            }

            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        ///     Added by luojie
        ///     获取某公司拥有的可用部门，并判断是否含有目标部门
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetDepartmentActivedByCompanyIDCompleted(object sender,GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    if (e.Error != null || e.Error.Message != "")
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                    if (e.Result != null)
                    {
                        //by luojie
                        //检查此部门是否已经存在
                        var result = from d in e.Result
                                     where d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID == Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID
                                           && d.EDITSTATE == "1"
                                     select d;
                        if (result.Count() > 0)
                        {
                            canSave = false;
                        }
                        else
                        {
                            canSave = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void client_GetDepartmentDictionaryAllCompleted(object sender, GetDepartmentDictionaryAllCompletedEventArgs e)
        {
            if (Department.T_HR_COMPANY != null || Department != null)
            {
                //判断状态是否为修改
                T_HR_COMPANY ent = Department.T_HR_COMPANY;

                tempList = e.Result;
                var entity = tempList.Where(s => s.DEPARTMENTTYPE == "-1" || s.DEPARTMENTTYPE == ent.COMPANYTYPE);
                entity = entity.Count() > 0 ? entity.ToList() : null;

                List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
                dicts = dicts.Where(s => s.DICTIONCATEGORY == "COMPANYTYPE").OrderBy(s => s.DICTIONARYVALUE).ToList();

                foreach (T_HR_DEPARTMENTDICTIONARY diction in entity)
                {
                    decimal dptype = Convert.ToDecimal(diction.DEPARTMENTTYPE);
                    var tmp = dicts.Where(s => s.DICTIONARYVALUE == dptype).FirstOrDefault();
                    if (tmp != null)
                    {
                        diction.OWNERPOSTID = diction.DEPARTMENTNAME;//暂存部门名称
                        diction.DEPARTMENTNAME = diction.DEPARTMENTNAME + "(" + tmp.DICTIONARYNAME + ")";

                    }
                }

                //cbxDepartMent.ItemsSource = entity;
                //cbxDepartMent.DisplayMemberPath = "DEPARTMENTNAME";
                acbDepName.ItemsSource = entity;
                acbDepName.ValueMemberPath = "DEPARTMENTNAME";
                string deptName = string.Empty;
                if (Department.T_HR_DEPARTMENTDICTIONARY != null)
                {
                    foreach (var item in acbDepName.ItemsSource)
                    {
                        T_HR_DEPARTMENTDICTIONARY dict = item as T_HR_DEPARTMENTDICTIONARY;
                        if (dict != null)
                        {
                            if (dict.DEPARTMENTDICTIONARYID == Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID)
                            {
                                //cbxDepartMent.SelectedItem = item;
                                acbDepName.SelectedItem = item;
                                deptName = dict.DEPARTMENTNAME;
                                break;
                            }
                        }
                    }
                }
                acbDepName.IsEnabled = true;
                //acbDepName.ToggleButton.IsEnabled = false;
                //acbDepName.TxtLookUp.IsReadOnly = true;
                ToolTipService.SetToolTip(this.acbDepName.TxtLookUp, deptName);
            }
        }

        //private void cbxDepartMent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (cbxDepartMent.SelectedItem != null)
        //    {
        //        T_HR_DEPARTMENTDICTIONARY dict = cbxDepartMent.SelectedItem as T_HR_DEPARTMENTDICTIONARY;
        //        if (dict != null)
        //        {
        //            Department.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
        //            Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = dict.DEPARTMENTDICTIONARYID;
        //            Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = DepartmentName;
        //            txtDepartmentCode.Text = dict.DEPARTMENTCODE;
        //            if (FormType == FormTypes.New)
        //            {
        //                if (!string.IsNullOrEmpty(dict.DEPARTMENTFUNCTION))
        //                {
        //                    txtDepFun.Text = dict.DEPARTMENTFUNCTION;
        //                }
        //                else
        //                {
        //                    txtDepFun.Text = string.Empty;
        //                }
        //            }
        //            client.GetDepartmentActivedByCompanyIDAsync(Department.FATHERID);
        //        }
        //    }
        //}

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("DEPARTMENTINFO");
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "编辑中";
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
                    Cancel();
                    break;
                case "Delete":
                    delete(department.DEPARTMENTID);
                    break;
                //case "2":
                //    SubmitAduit();
                //    break;
            }
        }

        public void delete(string strid)
        {
            string Result = "";
            string strMsg = string.Empty;
            //提示是否删除
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (obj, result) =>
            {
                client.DepartmentDeleteAsync(strid, strMsg);
            };
            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("确定要删除该部门信息？"), ComfirmWindow.titlename, Result);
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
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                if (Department != null)
                {
                    if (Department.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_DEPARTMENT", Department.OWNERID,
                    Department.OWNERPOSTID, Department.OWNERDEPARTMENTID, Department.OWNERCOMPANYID);
            }
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
        private string GetXmlString(string StrSource, T_HR_DEPARTMENT Info)
        {
            T_HR_POST headPost = (lkHead.DataContext as T_HR_POST);
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;

            decimal? compayTypeValue = Convert.ToDecimal((acbDepName.SelectedItem as T_HR_DEPARTMENTDICTIONARY).DEPARTMENTTYPE);
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY companyTypedict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "COMPANYTYPE" && s.DICTIONARYVALUE == compayTypeValue).FirstOrDefault();

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
            string dpn = companyTypedict == null ? "" : companyTypedict.DICTIONARYNAME;
            string departFullName = Info.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME + "(" + dpn + ")";
            AutoList.Add(basedata("T_HR_DEPARTMENT", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "DEPARTMENTDICTIONARYID", Info.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID, Info.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "COMPANYID", Info.T_HR_COMPANY.COMPANYID, Info.T_HR_COMPANY.CNAME));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "DEPARTMENTBOSSHEAD", Info.DEPARTMENTBOSSHEAD, string.IsNullOrEmpty(lkHead.TxtLookUp.Text) ? "" : lkHead.TxtLookUp.Text));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "FATHERID", Info.FATHERID, lkFather.TxtLookUp.Text));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "DEPARTMENTCODE", txtDepartmentCode.Text, txtDepartmentCode.Text));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "DEPARTMENTNAME", Info.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME, Info.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "DEPARTMENTTYPE", companyTypedict == null ? "" : companyTypedict.DICTIONARYVALUE.ToString(), companyTypedict == null ? "" : companyTypedict.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "DEPARTMENT", Info.DEPARTMENTID, departFullName));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "SORTINDEX", txtSortNumber.Value.ToString(), ""));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
            AutoList.Add(basedata("T_HR_DEPARTMENT", "EDITSTATE", Info.EDITSTATE, this.tbEdit.Text));//生效状态
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
            //if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID != Department.OWNERID && Department.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
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
            /// Added by 罗捷
            ///如果有可用的部门，则不允许再添加
            if (Department.EDITSTATE == "1")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result=SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("该部门已经存在，不能提交"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }


        }
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("DEPARTMENTNAME", Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);
            para.Add("CREATEUSERNAME", createUserName);
            para.Add("OWNER", createUserName);


            //Dictionary<string, string> para2 = new Dictionary<string, string>();
            //para2.Add("T_HR_COMPANYReference", Department.T_HR_COMPANY == null ? "" : Department.T_HR_COMPANY.COMPANYID + "#" + Department.T_HR_COMPANY.CNAME);
            //para2.Add("T_HR_DEPARTMENTDICTIONARYReference", Department.T_HR_DEPARTMENTDICTIONARY == null ? "" : Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID + "#" + Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);
            //para2.Add("DEPARTMENTBOSSHEAD", (lkHead.DataContext as T_HR_POST) == null ? "" : (lkHead.DataContext as T_HR_POST).POSTID + "#" + (lkHead.DataContext as T_HR_POST).T_HR_POSTDICTIONARY.POSTNAME);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, Department);
            // strXmlObjectSource = Utility.ObjListToXml<T_HR_DEPARTMENT>(Department, para, "HR", para2, null);
            Utility.SetAuditEntity(entity, "T_HR_DEPARTMENT", Department.DEPARTMENTID, strXmlObjectSource);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
            string strMsg = "";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    //if (FormType == FormTypes.Audit && Department.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    //{
                    //    state = Utility.GetCheckState(CheckStates.UnApproved);
                    //    Department.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    //}
                    //else
                    //{
                    //    state = Utility.GetCheckState(CheckStates.Approved);
                    //    Department.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    //}
                    state = Utility.GetCheckState(CheckStates.Approved);
                    if (Department.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        Department.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                    }
                    else
                    {
                        Department.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    }
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    if (Department.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        state = Utility.GetCheckState(CheckStates.Approved);
                        Department.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    }
                    else
                    {
                        state = Utility.GetCheckState(CheckStates.UnApproved);
                        Department.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    }
                    //state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (Department.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            Department.CHECKSTATE = state;
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
            // client.DepartmentUpdateAsync(Department, strMsg, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (Department != null)
                state = Department.CHECKSTATE;

            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        private bool Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            string strMsg = "";
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                needsubmit = false;
                return false;
            }
            else
            {
                //部门字典赋值
                T_HR_DEPARTMENTDICTIONARY companydef = acbDepName.SelectedItem as T_HR_DEPARTMENTDICTIONARY;
                if (companydef != null)
                {
                    Department.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                    Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = companydef.DEPARTMENTDICTIONARYID;

                    Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE = companydef.DEPARTMENTCODE;
                    T_HR_DEPARTMENTDICTIONARY dict = tempList.Where(s => s.DEPARTMENTDICTIONARYID == companydef.DEPARTMENTDICTIONARYID).FirstOrDefault();
                    if (dict != null)
                    {
                        Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = dict.OWNERPOSTID;//部门名称加了部门所属的类型，把原有的名字存在了ownerpostid字段
                    }
                    else
                    {
                        Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = companydef.DEPARTMENTNAME;
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "DEPARTMENTTYPE"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    acbDepName.Focus();
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                //是否为前台
                if (this.rbtYes.IsChecked == true)
                {
                    Department.ISBACKGROUND = 1;
                }
                else
                {
                    Department.ISBACKGROUND = 0;
                }

                if (FormType == FormTypes.New && canSave==true)
                {
                    //所属
                    Department.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    Department.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    Department.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    Department.OWNERCOMPANYID = Department.T_HR_COMPANY.COMPANYID;
                    Department.OWNERDEPARTMENTID = Department.DEPARTMENTID;
                    Department.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    Department.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    Department.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    Department.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    Department.CREATEDATE = DateTime.Now;
                    Department.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    client.DepartmentAddAsync(Department, strMsg);
                }
                else if (canSave==true)
                {
                    //如果状态为审核通过，修改时，则修改状态为审核中
                    //if (Department.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    //{
                    //    Department.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    //    Department.CHECKSTATE = Convert.ToInt32(CheckStates.Approving).ToString();
                    //}
                    if (Department.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        Department.CHECKSTATE = Convert.ToInt32(CheckStates.Approving).ToString();
                        if (Department.EDITSTATE != Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            Department.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                        }

                    }
                    Department.UPDATEDATE = DateTime.Now;
                    Department.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    client.DepartmentUpdateAsync(Department, strMsg, "Edit");
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("该公司已经存在这个部门"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            return true;
        }

        void client_DepartmentAddCompleted(object sender, DepartmentAddCompletedEventArgs e)
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
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ORGANIZATIONREPETITION", "COMPANY,DEPARTMENTNAME"),
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
                ToolbarItems = Utility.CreateFormEditButton();
                ToolbarItems.Add(ToolBarItems.Delete);
                RefreshUI(RefreshedTypes.All);
            }

            RefreshUI(RefreshedTypes.HideProgressBar);
        }


        void client_DepartmentUpdateCompleted(object sender, DepartmentUpdateCompletedEventArgs e)
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
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ORGANIZATIONREPETITION", "COMPANY,DEPARTMENTNAME"),
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
                    closeForm();
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
        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void closeForm()
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        void client_DepartmentCancelCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                           Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CANCELLEDSUCCESSED"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
        }

        public void Audit()
        {
            T_HR_DEPARTMENTHISTORY depHis = new T_HR_DEPARTMENTHISTORY();
            depHis.RECORDSID = Guid.NewGuid().ToString();
            depHis.DEPARTMENTID = Department.DEPARTMENTID;
            depHis.T_HR_DEPARTMENTDICTIONARY = Department.T_HR_DEPARTMENTDICTIONARY;
            depHis.COMPANYID = Department.T_HR_COMPANY.COMPANYID;
            depHis.DEPARTMENTFUNCTION = Department.DEPARTMENTFUNCTION;
            depHis.EDITSTATE = Department.EDITSTATE;
            if (Department.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
            {
                depHis.CANCELDATE = DateTime.Now;
            }
            depHis.REUSEDATE = DateTime.Now;
            depHis.CREATEDATE = Department.CREATEDATE;
            depHis.CREATEUSERID = Department.CREATEUSERID;
            depHis.UPDATEDATE = DateTime.Now;
            depHis.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            client.DepartmentHistoryAddAsync(depHis);

        }

        void client_DepartmentHistoryAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                           Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (Department.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                {
                    Department.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                    Department.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                }
                else
                {
                    Department.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                    Department.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                }
                Department.UPDATEDATE = DateTime.Now;
                Department.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.DepartmentUpdateAsync(Department, "Audit");
            }
        }

        private void HandleComapnyChanged()
        {
            //绑定部门字典
            client.GetDepartmentDictionaryAllAsync();
        }

        private void lkFather_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.All;
            lookup.TitleContent = Utility.GetResourceStr("ORGAN");
            lookup.SelectedClick += (obj, ev) =>
            {
                lkFather.DataContext = lookup.SelectedObj;
                if (lookup.SelectedObj is T_HR_COMPANY)
                {
                    lkFather.DisplayMemberPath = "CNAME";
                    Department.FATHERID = (lookup.SelectedObj as T_HR_COMPANY).COMPANYID;
                    Department.FATHERTYPE = "0";
                    txtCompanyName.Text = (lookup.SelectedObj as T_HR_COMPANY).CNAME;
                    Department.T_HR_COMPANY = new T_HR_COMPANY();
                    Department.T_HR_COMPANY.COMPANYID = (lookup.SelectedObj as T_HR_COMPANY).COMPANYID;
                    Department.T_HR_COMPANY.CNAME = (lookup.SelectedObj as T_HR_COMPANY).CNAME;
                    Department.T_HR_COMPANY.COMPANYTYPE = (lookup.SelectedObj as T_HR_COMPANY).COMPANYTYPE;
                    HandleComapnyChanged();
                }
                else if (lookup.SelectedObj is T_HR_DEPARTMENT)
                {
                    lkFather.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    Department.FATHERID = (lookup.SelectedObj as T_HR_DEPARTMENT).DEPARTMENTID;
                    Department.FATHERTYPE = "1";
                    txtCompanyName.Text = (lookup.SelectedObj as T_HR_DEPARTMENT).T_HR_COMPANY.CNAME;
                    Department.T_HR_COMPANY = new T_HR_COMPANY();
                    Department.T_HR_COMPANY.COMPANYID = (lookup.SelectedObj as T_HR_DEPARTMENT).T_HR_COMPANY.COMPANYID;
                    Department.T_HR_COMPANY.CNAME = (lookup.SelectedObj as T_HR_DEPARTMENT).T_HR_COMPANY.CNAME;
                    Department.T_HR_COMPANY.COMPANYTYPE = (lookup.SelectedObj as T_HR_DEPARTMENT).T_HR_COMPANY.COMPANYTYPE;
                    HandleComapnyChanged();
                }
                else if (lookup.SelectedObj is T_HR_POST)
                {
                    //cbxDepartMent.ItemsSource = null;
                    acbDepName.ItemsSource = null;
                    if (Department != null)
                    {
                        Department = null;
                    }
                    txtCompanyName.Text = "";
                    //  lkFather.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTEIONISNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else if (lookup.SelectedObj is T_HR_EMPLOYEE)
                {
                    // lkFather.DisplayMemberPath = "EMPLOYEECNAME";
                }

            };
            //by luojie
            client.GetDepartmentActivedByCompanyIDAsync(Department.FATHERID);
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        /// <summary>
        /// 部门负责人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkHead_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.Post;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_POST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                if (ent != null)
                {
                    lkHead.DataContext = ent;
                    Department.DEPARTMENTBOSSHEAD = ent.POSTID;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void btnEditIndex_Click(object sender, RoutedEventArgs e)
        {
            T_HR_DEPARTMENT temp = new T_HR_DEPARTMENT();
            temp.DEPARTMENTID = Department.DEPARTMENTID;
            temp.SORTINDEX = Department.SORTINDEX;
            string strMsg = string.Empty;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            client.DepartmentIndexUpdateAsync(temp, strMsg);
        }

        #region 单选按钮是否为前台
        private void rbtNo_Click(object sender, RoutedEventArgs e)
        {
            this.rbtNo.IsChecked = true;
            this.rbtYes.IsChecked = false;
        }

        private void rbtYes_Click(object sender, RoutedEventArgs e)
        {
            this.rbtYes.IsChecked = true;
            this.rbtNo.IsChecked = false;
        }
        #endregion

        private void acbDepName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (acbDepName.SelectedItem != null)
            {
                T_HR_DEPARTMENTDICTIONARY dict = acbDepName.SelectedItem as T_HR_DEPARTMENTDICTIONARY;
                if (dict != null)
                {
                    Department.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                    Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = dict.DEPARTMENTDICTIONARYID;
                    Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = DepartmentName;
                    txtDepartmentCode.Text = dict.DEPARTMENTCODE;
                    if (FormType == FormTypes.New)
                    {
                        if (!string.IsNullOrEmpty(dict.DEPARTMENTFUNCTION))
                        {
                            txtDepFun.Text = dict.DEPARTMENTFUNCTION;
                        }
                        else
                        {
                            txtDepFun.Text = string.Empty;
                        }
                    }
                    client.GetDepartmentActivedByCompanyIDAsync(Department.FATHERID);
                }
            }
        }
    }
}
