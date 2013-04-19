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

using SMT.Saas.Tools.SalaryWS;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.HRM.UI.Form.Salary
{
    public partial class EmployeeSalaryRecordForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        public T_HR_EMPLOYEESALARYRECORD SalaryRecord { get; set; }
        public FormTypes FormType { get; set; }
        private bool auditsign = false;
        private bool flag = false;
        private SalaryServiceClient client = new SalaryServiceClient();

        public EmployeeSalaryRecordForm(FormTypes type, string employeesalaryrecordID)
        {

            InitializeComponent();
            InitParas();
            FormType = type;
            if (string.IsNullOrEmpty(employeesalaryrecordID))
            {
                SalaryRecord = new T_HR_EMPLOYEESALARYRECORD();
                SalaryRecord.EMPLOYEESALARYRECORDID = Guid.NewGuid().ToString();
                SalaryRecord.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SalaryRecord.CREATEDATE = System.DateTime.Now;

                //SalaryRecord.UPDATEDATE = System.DateTime.Now;
                SalaryRecord.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                this.DataContext = SalaryRecord;
            }
            else
            {
                NotShow(type);
                client.GetEmployeeSalaryRecordByIDAsync(employeesalaryrecordID);
            }
        }

        private void NotShow(FormTypes type)
        {
            if (type != FormTypes.Edit && type != FormTypes.Resubmit)
            {
                txtRemark.IsEnabled = false;
                StandardItemWinForm.ToolBar.IsEnabled = false;
            }
        }

        private void InitParas()
        {
            client.GetEmployeeSalaryRecordByIDCompleted += new EventHandler<GetEmployeeSalaryRecordByIDCompletedEventArgs>(client_GetEmployeeSalaryRecordByIDCompleted);

            client.EmployeeSalaryRecordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeSalaryRecordUpdateCompleted);
            client.EmployeeSalaryRecordAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeSalaryRecordAddCompleted);
            client.GetEmployeeSalaryRecordItemByIDCompleted += new EventHandler<GetEmployeeSalaryRecordItemByIDCompletedEventArgs>(client_GetEmployeeSalaryRecordItemByIDCompleted);
            #region  薪资项控制
            StandardItemWinForm.ToolBar.btnNew.Visibility = Visibility.Collapsed;
            StandardItemWinForm.ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            StandardItemWinForm.ToolBar.retNew.Visibility = Visibility.Collapsed;
            StandardItemWinForm.ToolBar.retDelete.Visibility = Visibility.Collapsed;
            StandardItemWinForm.ToolBar.retRead.Visibility = Visibility.Collapsed;
            StandardItemWinForm.IsEnabled = true;
            #endregion
        }

        void client_GetEmployeeSalaryRecordItemByIDCompleted(object sender, GetEmployeeSalaryRecordItemByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    try
                    {
                        StandardItemWinForm.DtGrid.ItemsSource = null;
                        ObservableCollection<V_EMPLOYEESALARYRECORDITEM> its = new ObservableCollection<V_EMPLOYEESALARYRECORDITEM>();
                        its = e.Result;
                        for (int i = 0; i < its.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(its[i].SUM)) its[i].SUM = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(its[i].SUM);
                        }
                        StandardItemWinForm.DtGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                        StandardItemWinForm.DtGrid.Height = 300;
                        StandardItemWinForm.DtGrid.ItemsSource = its;
                    }
                    catch (Exception ex)
                    {
                        ex.Message.ToString();
                    }
                }
            }
        }

        void client_EmployeeSalaryRecordAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                // if (!auditsign) Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EMPLOYEESALARYRECORD"));
                // else auditsign = false;
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_EmployeeSalaryRecordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (!auditsign) ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
                    auditsign = false;
                }
            }
            else
            {
                if (!auditsign)
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "EMPLOYEESALARYRECORD"));
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUSSESSEDSUBMIT", "EMPLOYEESALARYRECORD"));
                    auditsign = false;
                }

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
            if (flag)
            {
                flag = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }



        void client_GetEmployeeSalaryRecordByIDCompleted(object sender, GetEmployeeSalaryRecordByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    return;
                }
                if (e.Result.T_HR_SALARYRECORDBATCH != null)
                {
                    try
                    {
                        T_HR_SALARYRECORDBATCH temp = e.Result.T_HR_SALARYRECORDBATCH as T_HR_SALARYRECORDBATCH;
                        string strCollection = string.Empty;
                        strCollection = temp.BALANCEOBJECTTYPE + "," + temp.BALANCEOBJECTID + "," + temp.BALANCEYEAR.ToString() + "," + temp.BALANCEMONTH.ToString() + "," + temp.CHECKSTATE + "," + temp.MONTHLYBATCHID;
                        Form.Salary.SalaryRecordMassAudit form = new Form.Salary.SalaryRecordMassAudit(FormTypes.Audit, strCollection);

                        EntityBrowser browser = new EntityBrowser(form);
                        //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        //browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
                        browser.FormType = FormTypes.Audit;
                        browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.Close();
                    }
                    catch { }
                    return;
                }
                SalaryRecord = e.Result;
                this.DataContext = SalaryRecord;
                client.GetEmployeeSalaryRecordItemByIDAsync(SalaryRecord.EMPLOYEESALARYRECORDID);

                //EntityBrowser browsers = new EntityBrowser(this);
                ////browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                ////browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
                ////browsers.FormType = FormTypes.Audit;
                //browsers.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse)
            {
                ToolbarItems = null;
                return;
            }
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_EMPLOYEESALARYRECORD", SalaryRecord.OWNERID,
                    SalaryRecord.OWNERPOSTID, SalaryRecord.OWNERDEPARTMENTID, SalaryRecord.OWNERCOMPANYID);

            if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEESALARYRECORD", SalaryRecord.OWNERID,
                    SalaryRecord.OWNERPOSTID, SalaryRecord.OWNERDEPARTMENTID, SalaryRecord.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }

        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            //string strXmlObjectSource = string.Empty;
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEESALARYRECORD>(SalaryRecord, "HR");
            //Utility.SetAuditEntity(entity, "T_HR_EMPLOYEESALARYRECORD", SalaryRecord.EMPLOYEESALARYRECORDID, strXmlObjectSource);

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EMPLOYEECNAME", SalaryRecord.EMPLOYEENAME);
            para.Add("EMPLOYEEID", SalaryRecord.EMPLOYEEID);
            para.Add("EntityKey", SalaryRecord.EMPLOYEESALARYRECORDID);

            Dictionary<string, string> para2 = new Dictionary<string, string>();
            string strXmlObjectSource = string.Empty;
            string strKeyName = "EMPLOYEESALARYRECORDID";
            string strKeyValue = SalaryRecord.EMPLOYEESALARYRECORDID;

            strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEESALARYRECORD>(SalaryRecord, para, "HR", para2, strKeyName, strKeyValue);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", SalaryRecord.EMPLOYEEID);
            paraIDs.Add("CreatePostID", SalaryRecord.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", SalaryRecord.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", SalaryRecord.OWNERCOMPANYID);

            if (SalaryRecord.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEESALARYRECORD", SalaryRecord.EMPLOYEESALARYRECORDID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEESALARYRECORD", SalaryRecord.EMPLOYEESALARYRECORDID, strXmlObjectSource);
            }
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            auditsign = true;
            RefreshUI(RefreshedTypes.ProgressBar);
            //Utility.UpdateCheckState("T_HR_EMPLOYEESALARYRECORD", "EMPLOYEESALARYRECORDID", SalaryRecord.EMPLOYEESALARYRECORDID, args);
            string state = "";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            SalaryRecord.CHECKSTATE = state;
            SalaryRecord.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            //if (state == Utility.GetCheckState(CheckStates.UnApproved)) client.UndoRepaymentAsync(SalaryRecord.EMPLOYEEID, SalaryRecord.SALARYYEAR, SalaryRecord.SALARYMONTH);  //审核不通过撤消还款
            client.EmployeeSalaryRecordUpdateAsync(SalaryRecord);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (SalaryRecord != null)
                state = SalaryRecord.CHECKSTATE;
            return state;
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEESALARYRECORD");
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
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
            }

        }
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            if (FormType == FormTypes.Browse) items.Clear();
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        public void Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            //T_HR_EMPLOYEE employee = lkEmployee.DataContext as T_HR_EMPLOYEE;

            //if (employee == null)
            //{
            //    SalaryRecord.EMPLOYEEID = "";
            //    SalaryRecord.EMPLOYEECODE = "";
            //    SalaryRecord.EMPLOYEENAME = "";
            //}
            //else
            //{
            //    SalaryRecord.EMPLOYEEID = employee.EMPLOYEEID;
            //    SalaryRecord.EMPLOYEECODE = employee.EMPLOYEECODE;
            //    SalaryRecord.EMPLOYEENAME = employee.EMPLOYEECNAME;
            //}
            //ValidationHelper.ValidateProperty<T_HR_EMPLOYEE>(Employee);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //MessageBox.Show(validators.Count.ToString() + " invalid validators");
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            else
            {

                if (FormType == FormTypes.Edit)
                {
                    SalaryRecord.UPDATEDATE = System.DateTime.Now;
                    SalaryRecord.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    client.EmployeeSalaryRecordUpdateAsync(SalaryRecord);
                }
                else
                {
                    client.EmployeeSalaryRecordAddAsync(SalaryRecord);
                }

            }
        }
        public void Cancel()
        {
            flag = true;
            Save();
        }

        private void lkSalaryStandard_FindClick(object sender, EventArgs e)
        {
            //TODO: 根据薪资方案分配过滤出可用的标准
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("SALARYSTANDARDNAME", "SALARYSTANDARDNAME");
            cols.Add("POSTSALARY", "POSTSALARY");
            cols.Add("SECURITYALLOWANCE", "SECURITYALLOWANCE");
            cols.Add("HOUSINGALLOWANCE", "HOUSINGALLOWANCE");
            cols.Add("AREADIFALLOWANCE", "AREADIFALLOWANCE");

            LookupForm lookup = new LookupForm(EntityNames.SalaryStandard,
                typeof(List<T_HR_SALARYSTANDARD>), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_SALARYSTANDARD ent = lookup.SelectedObj as T_HR_SALARYSTANDARD;
                if (ent != null)
                {
                    //lkSalaryStandard.DataContext = ent;
                    CopySalaryStandardToArchive(ent);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        private void CopySalaryStandardToArchive(T_HR_SALARYSTANDARD stand)
        {
            if (stand == null)
                return;



        }
        private void lkEmployee_FindClick(object sender, EventArgs e)
        {
            //TODO: 根据薪资方案分配过滤出可用的标准
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            cols.Add("SEX", "SEX");
            cols.Add("MOBILE", "MOBILE");
            cols.Add("OFFICEPHONE", "OFFICEPHONE");

            LookupForm lookup = new LookupForm(EntityNames.Employee,
                typeof(List<T_HR_EMPLOYEE>), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST; ;

                if (ent != null)
                {
                    lkEmployee.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void CloseProgressBar()
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

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
