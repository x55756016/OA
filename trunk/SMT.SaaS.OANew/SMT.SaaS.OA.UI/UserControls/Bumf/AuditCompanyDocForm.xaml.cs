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

using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.Saas.Tools.OrganizationWS;
using System.Windows.Browser;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
using System.Runtime.Serialization;

//using SMT.SaaS.FrameworkUI.FileUpload;

using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.Class;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class AuditCompanyDocForm : BaseForm, IClient, IEntityEditor, IAudit
    {
        

        string TmpSendoc = "";
        SmtOACommonOfficeClient DetailSendClient = new SmtOACommonOfficeClient();
        private PersonnelServiceClient personClient = new PersonnelServiceClient();
        public delegate void refreshGridView();
        private T_OA_SENDDOCTYPE SelectDocType = new T_OA_SENDDOCTYPE();
        private string tmpStrcbxGrade = "";//级别
        private string tmpStrcbxProritity = ""; //缓急
        T_OA_SENDDOC tmpSendDoc = new T_OA_SENDDOC();
        private string StrUpdateReturn = "";//修改数据时返回的字符串
        private FormTypes action;


        public AuditCompanyDocForm(FormTypes ActionType, string SendDocID)
        {
            InitializeComponent();
            action = ActionType;
            TmpSendoc = SendDocID;
            tblcontent.HideControls();
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "CompanyDoc";
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            //ctrFile.Event_AllFilesFinished += new EventHandler<FileCountEventArgs>(Event_AllFilesFinished);
            DetailSendClient.SendDocInfoUpdateCompleted += new EventHandler<SendDocInfoUpdateCompletedEventArgs>(SendDocClient_SendDocInfoUpdateCompleted);
            DetailSendClient.GetSendDocSingleInfoByIdCompleted += new EventHandler<GetSendDocSingleInfoByIdCompletedEventArgs>(SendDocClient_GetSendDocSingleInfoByIdCompleted);
            personClient.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(personClient_GetEmployeeByIDCompleted);
            this.Loaded += new RoutedEventHandler(CompanyDocWebPart_Loaded);
            //ctrFile.Load_fileData(SendDocID);
        }
        void personClient_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.tbladduser.Text = e.Result.EMPLOYEECNAME;
                //StrAddUserID = e.Result.EMPLOYEEID;
            }
        }
        void CompanyDocWebPart_Loaded(object sender, RoutedEventArgs e)
        {            
            DetailSendClient.GetSendDocSingleInfoByIdAsync(TmpSendoc);
        }
        void SendDocClient_SendDocInfoUpdateCompleted(object sender, SendDocInfoUpdateCompletedEventArgs e)
        {

            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    if (e.StrResult != "")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(e.StrResult, "COMPANYDOCUMENT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                    else
                    {

                        
                        if (e.UserState.ToString() == "Audit")
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        }
                        else if (e.UserState.ToString() == "Submit")
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        }
                        RefreshUI(RefreshedTypes.All);

                    }

                }


            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }

        }
        //private SmtOACommonOfficeClient SendDocClient = new SmtOACommonOfficeClient();

        void SendDocClient_GetSendDocSingleInfoByIdCompleted(object sender, GetSendDocSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        T_OA_SENDDOC senddoc = new T_OA_SENDDOC();
                        senddoc = e.Result;
                        tmpSendDoc = e.Result;
                        this.tbltitle.Text = senddoc.SENDDOCTITLE;
                        this.tblsend.Text = senddoc.SEND;
                        this.tblcopy.Text = senddoc.CC;                        
                        tblcontent.RichTextBoxContext = senddoc.CONTENT;                        
                        this.tbldepartment.Text = senddoc.DEPARTID;
                        SelectDocType = senddoc.T_OA_SENDDOCTYPE;
                        this.tbldoctype.Text = SelectDocType.SENDDOCTYPE;
                        this.tblprioritity.Text = senddoc.PRIORITIES;
                        this.tblgrade.Text = senddoc.GRADED;                        
                        this.tblnum.Text = senddoc.NUM;
                        personClient.GetEmployeeByIDAsync(senddoc.OWNERID);
                        this.tblcontenttitle.Text = senddoc.SENDDOCTITLE;
                        GetCompanyName(senddoc.OWNERCOMPANYID);
                        GetDepartmentName(senddoc.OWNERDEPARTMENTID);
                        string StrState = "";
                        string StrSave = "";                        
                        tmpStrcbxGrade = senddoc.GRADED;
                        tmpStrcbxProritity = senddoc.PRIORITIES;
                        if (senddoc.PUBLISHDATE !=null)
                        {
                            //tbladddate.Text = System.Convert.ToDateTime(senddoc.PUBLISHDATE).ToShortDateString() + " " + System.Convert.ToDateTime(senddoc.PUBLISHDATE).ToShortTimeString();
                            tbladddate.Text = System.Convert.ToDateTime(senddoc.PUBLISHDATE).ToString("d") + " " + System.Convert.ToDateTime(senddoc.PUBLISHDATE).ToShortTimeString();
                        }
                        
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                        //ctrFile.IsEnabled = false;
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    return;
                }
            }

        }

        /// <summary>
        /// 从资源文件中获取公司信息
        /// </summary>
        /// <param name="StrCompanyID"></param>
        private void GetCompanyName(string StrCompanyID)
        {
            if (Application.Current.Resources["SYS_CompanyInfo"] != null)
            {
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> ListCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
                ListCompany = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

                if (ListCompany != null)
                {
                    var objc = from a in ListCompany
                               where a.COMPANYID == StrCompanyID
                               select a.CNAME;
                    if (objc.FirstOrDefault() == "集团本部")
                    {
                        this.tbltitlecompany.Text = "深圳市神州通投资集团有限公司";
                    }
                    else
                    {
                        this.tbltitlecompany.Text = objc.FirstOrDefault() ;
                    }
                }



            }
        }
        /// <summary>
        /// 从资源文件中获取部门信息
        /// </summary>
        /// <param name="StrCompanyID"></param>
        private void GetDepartmentName(string StrDepartmentID)
        {
            if (Application.Current.Resources["SYS_DepartmentInfo"] != null)
            {
                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> ListCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
                ListCompany = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

                if (ListCompany != null)
                {
                    var objc = from a in ListCompany
                               where a.DEPARTMENTID == StrDepartmentID
                               select a.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    this.tbldepartment.Text = objc.FirstOrDefault();
                }



            }
        }



       

        #region IEntityEditor 成员
        public string GetTitle()
        {
            
            return Utility.GetResourceStr("AUDITTITLE", "COMPANYDOC");
            

        }

        public string GetStatus()
        {

            return "";
        }

        public void DoAction(string actionenum)
        {

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

            

            return items;
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

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_SENDDOC>(tmpSendDoc, "OA");

            Utility.SetAuditEntity(entity, "CompanyDoc", tmpSendDoc.SENDDOCID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
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
            if (tmpSendDoc.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            tmpSendDoc.CHECKSTATE = state;
            //cmsfc.UpdateContraApprovalAsync(tmpSendDocT, UserState);
            DetailSendClient.SendDocInfoUpdateAsync(tmpSendDoc, StrUpdateReturn, UserState);
        }

        public string GetAuditState()
        {

            string state = "-1";
            //if (tmpSendDocT != null)
            //    state = tmpSendDocT.CHECKSTATE;
            //if (action == FormTypes.Browse)
            //{
            //    state = "-1";
            //}
            return state;
        }

        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            DetailSendClient.DoClose();
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
    }
}

