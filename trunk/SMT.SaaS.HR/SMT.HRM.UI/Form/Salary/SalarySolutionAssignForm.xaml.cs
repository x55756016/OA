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
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalarySolutionAssignForm : BaseForm, IEntityEditor, IAudit,IClient
    {
        public V_SALARYSOLUTIONASSIGN SalarySolutionAssignView { get; set; }
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();

        private T_HR_SALARYSTANDARD salaryStander = new T_HR_SALARYSTANDARD();
        private List<T_HR_CUSTOMGUERDONARCHIVE> salaryCustom = new List<T_HR_CUSTOMGUERDONARCHIVE>();
        private List<V_CUSTOMGUERDON> salaryCusomvView = new List<V_CUSTOMGUERDON>();
        private T_HR_SALARYARCHIVE archive = new T_HR_SALARYARCHIVE();
        public string SalarySolutionAssignID { get; set; }
        public SalarySolutionAssignForm(FormTypes type, string solutionID)
        {
            InitializeComponent();
            //InitParas();
            FormType = type;
            SalarySolutionAssignID = solutionID;
            this.Loaded += new RoutedEventHandler(SalarySolutionAssignForm_Loaded);
            //BindAssignObjectType();

            //if (string.IsNullOrEmpty(solutionID))
            //{
            //    SalarySolutionAssignView = new V_SALARYSOLUTIONASSIGN();
            //    SalarySolutionAssignView.SalarySolutionAssign = new T_HR_SALARYSOLUTIONASSIGN();
            //    SalarySolutionAssignView.SalarySolutionAssign.SALARYSOLUTIONASSIGNID = Guid.NewGuid().ToString();
            //    SalarySolutionAssignView.SalarySolutionAssign.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    SalarySolutionAssignView.SalarySolutionAssign.CREATEDATE = System.DateTime.Now;

            //    SalarySolutionAssignView.SalarySolutionAssign.UPDATEDATE = System.DateTime.Now;
            //    SalarySolutionAssignView.SalarySolutionAssign.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //    SalarySolutionAssignView.SalarySolutionAssign.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //    SalarySolutionAssignView.SalarySolutionAssign.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            //    SalarySolutionAssignView.SalarySolutionAssign.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    SalarySolutionAssignView.SalarySolutionAssign.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            //    SalarySolutionAssignView.SalarySolutionAssign.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
            //    this.DataContext = SalarySolutionAssignView.SalarySolutionAssign;
            //    BindAssignObjectLookup();
            //}
            //else
            //{
            //    client.GetSalarySolutionAssignViewByIDAsync(solutionID);
            //}
                
        }

        void SalarySolutionAssignForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            BindAssignObjectType();
            //Utility.CbxItemBinder(cbxObjectType1, "ASSIGNEDOBJECTTYPE", "");
            if (string.IsNullOrEmpty(SalarySolutionAssignID))
            {
                SalarySolutionAssignView = new V_SALARYSOLUTIONASSIGN();
                SalarySolutionAssignView.SalarySolutionAssign = new T_HR_SALARYSOLUTIONASSIGN();
                SalarySolutionAssignView.SalarySolutionAssign.SALARYSOLUTIONASSIGNID = Guid.NewGuid().ToString();
                SalarySolutionAssignView.SalarySolutionAssign.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SalarySolutionAssignView.SalarySolutionAssign.CREATEDATE = System.DateTime.Now;

                SalarySolutionAssignView.SalarySolutionAssign.UPDATEDATE = System.DateTime.Now;
                SalarySolutionAssignView.SalarySolutionAssign.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                SalarySolutionAssignView.SalarySolutionAssign.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                SalarySolutionAssignView.SalarySolutionAssign.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                SalarySolutionAssignView.SalarySolutionAssign.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SalarySolutionAssignView.SalarySolutionAssign.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                SalarySolutionAssignView.SalarySolutionAssign.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                this.DataContext = SalarySolutionAssignView.SalarySolutionAssign;
                BindAssignObjectLookup();
            }
            else
            {
                client.GetSalarySolutionAssignViewByIDAsync(SalarySolutionAssignID);
            }
        }
        private void InitParas()
        {
            client.GetSalarySolutionAssignViewByIDCompleted += new EventHandler<GetSalarySolutionAssignViewByIDCompletedEventArgs>(client_GetSalarySolutionAssignViewByIDCompleted);
            client.SalarySolutionAssignUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalarySolutionAssignUpdateCompleted);
            client.SalarySolutionAssignAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalarySolutionAssignAddCompleted);

            //client.GetSalarySolutionStandardWithPagingCompleted += new EventHandler<GetSalarySolutionStandardWithPagingCompletedEventArgs>(client_GetSalarySolutionStandardWithPagingCompleted);
            //client.SalarySolutionStandardAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalarySolutionStandardAddCompleted);
            //client.SalarySolutionStandardDeleteCompleted += new EventHandler<SalarySolutionStandardDeleteCompletedEventArgs>(client_SalarySolutionStandardDeleteCompleted);
            client.GetSalaryStandardByIDCompleted += new EventHandler<GetSalaryStandardByIDCompletedEventArgs>(client_GetSalaryStandardByIDCompleted);
            client.GetCustomGuerdonCompleted += new EventHandler<GetCustomGuerdonCompletedEventArgs>(client_GetCustomGuerdonCompleted);
            client.CustomGuerdonArchiveAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CustomGuerdonArchiveAddCompleted);

        }

        private void BindAssignObjectType()
        {
            cbxObjectType.Items.Clear();
            foreach (var cbx in cbxObjectType1.Items)
            {
                if (cbxObjectType.Items.Count >= 4) break;
                cbxObjectType.Items.Add((cbx as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYNAME);
            }
        }

        //void client_GetSalarySolutionStandardWithPagingCompleted(object sender, GetSalarySolutionStandardWithPagingCompletedEventArgs e)
        //{
        //    List<T_HR_SALARYSOLUTIONSTANDARD> list = new List<T_HR_SALARYSOLUTIONSTANDARD>();
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //        return;
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            list = e.Result.ToList();
        //        }
        //        DtGrid.ItemsSource = list;

        //        dataPager.PageCount = e.pageCount;
        //    }
        //}

        //void client_SalarySolutionStandardDeleteCompleted(object sender, SalarySolutionStandardDeleteCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSOLUTIONSTANDARD"));
        //    }
        //    RefreshUI(RefreshedTypes.All);
        //}

        //void client_SalarySolutionStandardAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYSOLUTIONSTANDARD"));
        //    }
        //    RefreshUI(RefreshedTypes.All);
        //}

        void client_CustomGuerdonArchiveAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CUSTOMGUERDONARCHIVE"));
            }
            RefreshUI(RefreshedTypes.All);
          //  RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_GetCustomGuerdonCompleted(object sender, GetCustomGuerdonCompletedEventArgs e)
        {
            T_HR_CUSTOMGUERDONARCHIVE customArchive = new T_HR_CUSTOMGUERDONARCHIVE();
            if (e.Result != null)
            {
                salaryCusomvView = e.Result.ToList();
                foreach (V_CUSTOMGUERDON a in salaryCusomvView)
                {
                    customArchive.SUM = a.GUERDONSUM;
                    customArchive.CUSTOMERGUERDONID = a.CUSTOMGUERDONID;
                    salaryCustom.Add(customArchive);
                }
            }
        }

        void client_GetSalaryStandardByIDCompleted(object sender, GetSalaryStandardByIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                salaryStander = e.Result;
            }
        }

        void client_GetSalarySolutionAssignViewByIDCompleted(object sender, GetSalarySolutionAssignViewByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    return;
                }
                SalarySolutionAssignView = e.Result;
                this.DataContext = SalarySolutionAssignView.SalarySolutionAssign;
                //  salaryStander = SalarySolutionAssignView.SalarySolutionAssign.T_HR_SALARYSTANDARD;
                archive.SALARYSOLUTIONID = SalarySolutionAssignView.SalarySolutionAssign.T_HR_SALARYSOLUTION.SALARYSOLUTIONID;
                archive.SALARYSOLUTIONNAME = SalarySolutionAssignView.SalarySolutionAssign.T_HR_SALARYSOLUTION.SALARYSOLUTIONNAME;
                cbxObjectType.SelectedIndex =SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTTYPE!=null?SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTTYPE.ToInt32():-1;
                BindAssignObjectLookup();
               
            }
        }
        private void BindAssignObjectLookup()
        {
            if (SalarySolutionAssignView == null || SalarySolutionAssignView.SalarySolutionAssign == null)
            {
                lkAssignObject.DataContext = null;
                return;

            }
            switch (SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTTYPE)
            {
                case "1":
                    OrganizationWS.T_HR_COMPANY company = new OrganizationWS.T_HR_COMPANY();
                    company.CNAME = SalarySolutionAssignView.AssignObjectName;
                    company.COMPANYID = SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID;
                    lkAssignObject.DisplayMemberPath = "CNAME";
                    lkAssignObject.DataContext = company;
                    break;

                case "2":
                    OrganizationWS.T_HR_DEPARTMENT depart = new OrganizationWS.T_HR_DEPARTMENT();
                    depart.DEPARTMENTID = SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID;
                    depart.T_HR_DEPARTMENTDICTIONARY = new OrganizationWS.T_HR_DEPARTMENTDICTIONARY();
                    depart.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = SalarySolutionAssignView.AssignObjectName;
                    lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    lkAssignObject.DataContext = depart;
                    break;

                case "3":
                    OrganizationWS.T_HR_POST post = new OrganizationWS.T_HR_POST();
                    post.POSTID = SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID;
                    post.T_HR_POSTDICTIONARY = new OrganizationWS.T_HR_POSTDICTIONARY();
                    post.T_HR_POSTDICTIONARY.POSTNAME = SalarySolutionAssignView.AssignObjectName;
                    lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                    lkAssignObject.DataContext = post;
                    break;

                case "4":
                    OrganizationWS.T_HR_EMPLOYEE employee = new OrganizationWS.T_HR_EMPLOYEE();
                    employee.EMPLOYEEID = SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID;
                    employee.EMPLOYEECNAME = SalarySolutionAssignView.AssignObjectName;
                    lkAssignObject.DisplayMemberPath = "EMPLOYEECNAME";
                    lkAssignObject.DataContext = employee;
                    break;

                default:
                    lkAssignObject.DataContext = null;
                    break;
            }

        }
        void client_SalarySolutionAssignAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                FormType = FormTypes.Edit;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYSOLUTION"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_SalarySolutionAssignUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYSOLUTION"));
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            //  return "";// Utility.GetResourceStr("SALARYSOLUTION") + ":" + SalarySolution.SALARYSOLUTIONNAME;
            return "薪资方案应用"; //Utility.GetResourceStr("SALARYSOLUTIONASSIGN");
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
                Title = Utility.GetResourceStr("BASICINFO"),
                Tooltip = Utility.GetResourceStr("BASICINFO")
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
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            if (FormType == FormTypes.Browse) items.Clear();
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion
        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Utility.SetAuditEntity(entity, "T_HR_SALARYSOLUTIONASSIGN", SalarySolutionAssignView.SalarySolutionAssign.SALARYSOLUTIONASSIGNID);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            Utility.UpdateCheckState("T_HR_SALARYSOLUTIONASSIGN", "SALARYSOLUTIONASSIGNID", SalarySolutionAssignView.SalarySolutionAssign.SALARYSOLUTIONASSIGNID, args);
        }
        public string GetAuditState()
        {
            return "";
        }
        #endregion
        public bool Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (SetAssignFromLookup())
            {

                List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
                if (string.IsNullOrEmpty(lkSalarySolution.TxtLookUp.Text.Trim()))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYSOLUTION"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                   // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYSOLUTION"));
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return false;
                }
                if (cbxObjectType.SelectedIndex <= 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ASSIGNEDOBJECTTYPE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ASSIGNEDOBJECTTYPE"));
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return false;
                }
                if (string.IsNullOrEmpty(lkAssignObject.TxtLookUp.Text.Trim()))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ASSIGNEDOBJECT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                   // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ASSIGNEDOBJECT"));
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return false;
                }
                if (Convert.ToDateTime(dpStartDate.Text) > Convert.ToDateTime(dpEndDate.Text))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"));
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return false;
                }
                if (validators.Count > 0)
                {
                    //could use the content of the list to show an invalid message summary somehow
                    //MessageBox.Show(validators.Count.ToString() + " invalid validators");
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return false;
                }
                else
                {
                    if (FormType == FormTypes.Edit)
                    {
                        SalarySolutionAssignView.SalarySolutionAssign.UPDATEDATE = System.DateTime.Now;
                        SalarySolutionAssignView.SalarySolutionAssign.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;


                        client.SalarySolutionAssignUpdateAsync(SalarySolutionAssignView.SalarySolutionAssign);
                    }
                    else
                    {
                        client.SalarySolutionAssignAddAsync(SalarySolutionAssignView.SalarySolutionAssign);

                    }

                    archive.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    archive.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    archive.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    archive.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    archive.CHECKSTATE = Convert.ToInt16(CheckStates.UnSubmit).ToString();
                    archive.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    // CopySalaryStandardToArchive(salaryStander);//薪资标准复制到薪资档案中
                    archive.SALARYARCHIVEID = Guid.NewGuid().ToString();
                    client.CreateSalaryArchiveAsync(int.Parse(SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTTYPE), SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID, archive,true);
                }
            }
            else 
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("选择的类型不匹配"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("选择的类型不匹配"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            return true;
        }

        private bool SetAssignFromLookup()
        {
            object obj = lkAssignObject.DataContext;
            if (obj == null)
            {
                SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID = "";
                SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTTYPE = "";
                return false;
            }

            if (obj is OrganizationWS.T_HR_COMPANY)
            {
                if (cbxObjectType.SelectedIndex == 1)
                {
                    SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID = ((OrganizationWS.T_HR_COMPANY)obj).COMPANYID;
                    SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTTYPE = "1";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (obj is OrganizationWS.T_HR_DEPARTMENT)
            {
                if (cbxObjectType.SelectedIndex == 2)
                {
                  SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID = ((OrganizationWS.T_HR_DEPARTMENT)obj).DEPARTMENTID;
                  SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTTYPE = "2";
                return true;
                }
                else
                {
                    return false;
                 }
            }
            else if (obj is OrganizationWS.T_HR_POST)
            {
                if (cbxObjectType.SelectedIndex == 3)
                {
                   SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID = ((OrganizationWS.T_HR_POST)obj).POSTID;
                   SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTTYPE = "3";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
            //else if (obj is OrganizationWS.T_HR_EMPLOYEE)
            //{
            //    SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTID = ((OrganizationWS.T_HR_EMPLOYEE)obj).EMPLOYEEID;
            //    SalarySolutionAssignView.SalarySolutionAssign.ASSIGNEDOBJECTTYPE = "3";
            //}

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

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        private void lkSalarySolution_FindClick(object sender, EventArgs e)
        {

            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("SALARYSOLUTIONNAME", "SALARYSOLUTIONNAME");
            //cols.Add("BANKNAME", "BANKNAME");
            cols.Add("BANKACCOUNTNO", "BANKACCOUNTNO");
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            string filter = "";
            filter = "CHECKSTATE==@" + paras.Count;
            paras.Add(Convert.ToInt16(CheckStates.Approved).ToString());
            //LookupForm lookup = new LookupForm(EntityNames.SalarySolution,
            //    typeof(List<T_HR_SALARYSOLUTION>), cols);
            LookupForm lookup = new LookupForm(EntityNames.SalarySolution,
              typeof(List<T_HR_SALARYSOLUTION>), cols, filter, paras);
            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_SALARYSOLUTION ent = lookup.SelectedObj as T_HR_SALARYSOLUTION;
                if (ent != null)
                {
                    lkSalarySolution.DataContext = ent;
                    archive.SALARYSOLUTIONID = ent.SALARYSOLUTIONID;
                    archive.SALARYSOLUTIONNAME = ent.SALARYSOLUTIONNAME;
                    
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        //private void lkSalaryStander_FindClick(object sender, EventArgs e)
        //{
        //    Dictionary<string, string> cols = new Dictionary<string, string>();
        //    cols.Add("SALARYSTANDARDNAME", "SALARYSTANDARDNAME");
        //    cols.Add("POSTSALARY", "POSTSALARY");
        //    cols.Add("SECURITYALLOWANCE", "SECURITYALLOWANCE");
        //    cols.Add("HOUSINGALLOWANCE", "HOUSINGALLOWANCE");
        //    cols.Add("AREADIFALLOWANCE", "AREADIFALLOWANCE");

        //    LookupForm lookup = new LookupForm(EntityNames.SalaryStandard,
        //        typeof(List<T_HR_SALARYSTANDARD>), cols);

        //    lookup.SelectedClick += (o, ev) =>
        //    {
        //        T_HR_SALARYSTANDARD ent = lookup.SelectedObj as T_HR_SALARYSTANDARD;
        //        if (ent != null)
        //        {
        //            lkSalaryStander.DataContext = ent;
        //            salaryStander = ent;
        //         //   client.GetCustomGuerdonAsync(salaryStander.SALARYSTANDARDID);
        //        }
        //    };

        //    lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        //}
        private void lkAssignObject_FindClick(object sender, EventArgs e)
        {

            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.All;
            lookup.TitleContent = Utility.GetResourceStr("ORGAN");
            lookup.SelectedClick += (obj, ev) =>
            {
                lkAssignObject.DataContext = lookup.SelectedObj;
                if (lookup.SelectedObj is OrganizationWS.T_HR_COMPANY)
                {
                    lkAssignObject.DisplayMemberPath = "CNAME";
                }
                else if (lookup.SelectedObj is OrganizationWS.T_HR_DEPARTMENT)
                {
                    lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                }
                else if (lookup.SelectedObj is OrganizationWS.T_HR_POST)
                {
                    lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                }
                else if (lookup.SelectedObj is OrganizationWS.T_HR_EMPLOYEE)
                {
                    lkAssignObject.DisplayMemberPath = "EMPLOYEECNAME";
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        # region
        private void CopySalaryStandardToArchive(T_HR_SALARYSTANDARD stander)
        {
            archive.SALARYARCHIVEID = Guid.NewGuid().ToString();
            archive.AREADIFALLOWANCE = stander.AREADIFALLOWANCE;
            archive.FOODALLOWANCE = stander.FOODALLOWANCE;

            archive.HOUSINGALLOWANCEDEDUCT = stander.HOUSINGALLOWANCEDEDUCT;
            archive.OTHERADDDEDUCT = stander.OTHERADDDEDUCT;
            archive.OTHERADDDEDUCTDESC = stander.OTHERADDDEDUCTDESC;

            archive.OTHERSUBJOIN = stander.OTHERSUBJOIN;
            archive.OTHERSUBJOINDESC = stander.OTHERSUBJOINDESC;
            archive.PERSONALINCOMERATIO = stander.PERSONALINCOMERATIO;
            archive.PERSONALSIRATIO = stander.PERSONALSIRATIO;

            archive.POSTSALARY = stander.POSTSALARY;
            archive.HOUSINGALLOWANCE = stander.HOUSINGALLOWANCE;
            archive.SECURITYALLOWANCE = stander.SECURITYALLOWANCE;

            archive.T_HR_SALARYSTANDARD = new T_HR_SALARYSTANDARD();
            archive.T_HR_SALARYSTANDARD.SALARYSTANDARDID = stander.SALARYSTANDARDID;
            //if (stander.T_HR_SALARYSOLUTION != null)
            //{
            //    archive.SALARYSOLUTIONID = stander.T_HR_SALARYSOLUTION.SALARYSOLUTIONID;
            //}

            archive.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            archive.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

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

        private void cbxObjectType_Loaded(object sender, RoutedEventArgs e)
        {
            //if (cbxObjectType.Items.Count > 4)
            //{
            //    cbxObjectType.Items.RemoveAt(4);
            //}
            Utility.CbxItemBinder(cbxObjectType1, "ASSIGNEDOBJECTTYPE", "");
        }

        /// <summary>
        /// 生成自定义薪资档案
        /// </summary>
        /// <param name="salaryArchiveID"></param>
        //void AddCustomArchive(string salaryArchiveID)
        //{

        //    for (int i = 0; i < salaryCustom.Count; i++)
        //    {
        //        salaryCustom[i].CUSTOMGUERDONARCHIVEID = Guid.NewGuid().ToString();
        //        salaryCustom[i].T_HR_SALARYARCHIVE = new T_HR_SALARYARCHIVE();
        //        salaryCustom[i].T_HR_SALARYARCHIVE.SALARYARCHIVEID = salaryArchiveID;
        //        client.CustomGuerdonArchiveAddAsync(salaryCustom[i]);
        //    }

        //}
        #endregion
        //private void GridPager_Click(object sender, RoutedEventArgs e)
        //{
        //    LoadData();
        //}

        //private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{

        //}

        //private void btnStandardAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    T_HR_SALARYSOLUTIONSTANDARD solutionSatandard = new T_HR_SALARYSOLUTIONSTANDARD();
        //    if (SalarySolutionAssignView != null && SalarySolutionAssignView.SalarySolutionAssign.T_HR_SALARYSOLUTION!=null)
        //    {
        //        solutionSatandard.T_HR_SALARYSOLUTION = new T_HR_SALARYSOLUTION();
        //        solutionSatandard.T_HR_SALARYSOLUTION.SALARYSOLUTIONID = SalarySolutionAssignView.SalarySolutionAssign.T_HR_SALARYSOLUTION.SALARYSOLUTIONID;
        //        Dictionary<string, string> cols = new Dictionary<string, string>();
        //        cols.Add("SALARYSTANDARDNAME", "SALARYSTANDARDNAME");
        //        cols.Add("POSTSALARY", "POSTSALARY");
        //        cols.Add("SECURITYALLOWANCE", "SECURITYALLOWANCE");
        //        cols.Add("HOUSINGALLOWANCE", "HOUSINGALLOWANCE");
        //        cols.Add("AREADIFALLOWANCE", "AREADIFALLOWANCE");

        //        LookupForm lookup = new LookupForm(EntityNames.SalaryStandard,
        //            typeof(List<T_HR_SALARYSTANDARD>), cols);

        //        lookup.SelectedClick += (o, ev) =>
        //        {
        //            T_HR_SALARYSTANDARD ent = lookup.SelectedObj as T_HR_SALARYSTANDARD;
        //            if (ent != null)
        //            {
        //                solutionSatandard.SOLUTIONSTANDARDID = Guid.NewGuid().ToString();
        //                solutionSatandard.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        //                solutionSatandard.CREATEDATE = System.DateTime.Now;
        //                solutionSatandard.T_HR_SALARYSTANDARD = new T_HR_SALARYSTANDARD();
        //                solutionSatandard.T_HR_SALARYSTANDARD.SALARYSTANDARDID = ent.SALARYSTANDARDID;
        //                client.SalarySolutionStandardAddAsync(solutionSatandard);
        //            }
        //        };

        //        lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        //    }

        //}

        //private void btnStandardDel_Click(object sender, RoutedEventArgs e)
        //{

        //}
        //void LoadData()
        //{
        //    if (SalarySolutionAssignView != null)
        //    {
        //        int pageCount = 0;
        //        string filter = "";
        //        System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

        //        //TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
        //        if (!string.IsNullOrEmpty(SalarySolutionAssignView.SalarySolutionAssign.T_HR_SALARYSOLUTION.SALARYSOLUTIONID))
        //        {
        //            filter += "  T_HR_SALARYSOLUTION.SALARYSOLUTIONID==@" + paras.Count().ToString();
        //            paras.Add(SalarySolutionAssignView.SalarySolutionAssign.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);
        //        }
        //        client.GetSalarySolutionStandardWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SOLUTIONSTANDARDID", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //}
    }
}
