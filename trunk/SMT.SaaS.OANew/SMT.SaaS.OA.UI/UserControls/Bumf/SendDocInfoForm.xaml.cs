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

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SendDocInfoForm : BaseForm,IClient, IEntityEditor,IAudit
    {
        SmtOACommonOfficeClient DetailSendClient = new SmtOACommonOfficeClient();
        //OrganizationServiceClient Organ
        private Action action;
        public delegate void refreshGridView();
        private T_OA_SENDDOC tmpSendDocT = new T_OA_SENDDOC();
        private T_OA_SENDDOCTYPE tmptype = new T_OA_SENDDOCTYPE();
        private string StrUpdateReturn = "";//修改数据时返回的字符串
        private FormTypes actionTypes;//动作
        public SendDocInfoForm(V_BumfCompanySendDoc obj)
        {
            InitializeComponent();
            actionTypes = FormTypes.Browse;
            tmptype = obj.doctype;
            tmpSendDocT = obj.senddoc;
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);

            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "CompanyDoc";
            //ctrFile.Load_fileData(obj.senddoc.SENDDOCID);
            //DetailSendClient.GetDocDistrbuteSingleInfoByIdCompleted += new EventHandler<GetDocDistrbuteSingleInfoByIdCompletedEventArgs>(DetailSendClient_GetDocDistrbuteSingleInfoByIdCompleted);
            DetailSendClient.GetSendDocSingleInfoByIdCompleted += new EventHandler<GetSendDocSingleInfoByIdCompletedEventArgs>(DetailSendClient_GetSendDocSingleInfoByIdCompleted);
            //GetSendDocDetailInfo(obj);

            tblcontent.HideControls();//屏蔽富文本框的头部
            DetailSendClient.GetSendDocSingleInfoByIdAsync(obj.OACompanySendDoc.SENDDOCID);
            this.Loaded += new RoutedEventHandler(SendDocInfoForm_Loaded);
        }

        void SendDocInfoForm_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        void DetailSendClient_GetSendDocSingleInfoByIdCompleted(object sender, GetSendDocSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        T_OA_SENDDOC senddoc = new T_OA_SENDDOC();
                        senddoc = e.Result;
                        tmpSendDocT = senddoc;
                        this.tbltitle.Text = senddoc.SENDDOCTITLE;
                        this.tblsend.Text = senddoc.SEND;
                        this.tblcopy.Text = senddoc.CC;                        
                        tblcontent.RichTextBoxContext = senddoc.CONTENT;
                        this.tbldepartment.Text = senddoc.DEPARTID;
                        this.tbldoctype.Text = tmptype.SENDDOCTYPE;
                        this.tblprioritity.Text = senddoc.PRIORITIES;
                        this.tblgrade.Text = senddoc.GRADED;
                        this.tbladddate.Text = System.Convert.ToDateTime(senddoc.CREATEDATE).ToShortDateString();
                        //this.tblupdatedate.Text = senddoc.UPDATEDATE.ToString();
                        //senddoctab.TabStripPlacement = Dock.Left;
                        //if (!ctrFile._files.HasAccessory)
                        //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(DocInfo,16);
                        this.tblnum.Text = senddoc.NUM;
                        this.tblcontenttitle.Text = senddoc.SENDDOCTITLE;
                        //GetCompanyName(senddoc.OWNERCOMPANYID);
                        GetDepartmentName(senddoc.DEPARTID);
                        string StrState = "";
                        string StrSave = "";
                        string Strdistrbute = "";
                        switch (senddoc.CHECKSTATE)
                        {
                            case "0":
                                StrState = "未提交";
                                senddoctab.Items.Remove(senddoctab.Items[2]);

                                break;
                            case "1":
                                StrState = "审核中";
                                InitAudit(senddoc);
                                senddoctab.Items.Remove(senddoctab.Items[1]);
                                break;
                            case "2":
                                StrState = "审核通过";
                                InitAudit(senddoc);
                                break;
                            case "3":
                                StrState = "审核未通过";
                                senddoctab.Items.Remove(senddoctab.Items[1]);
                                InitAudit(senddoc);
                                break;
                        }
                        switch (senddoc.ISSAVE)
                        {
                            case "0":
                                StrSave = "未归档";
                                //this.spDistrbuteDetail.Visibility = Visibility.Collapsed;
                                break;
                            case "1":
                                StrSave = "已归档";
                                //获取发文的详细信息
                                //this.spDistrbuteDetail.Visibility = Visibility.Visible;                    
                                DetailSendClient.GetDocDistrbuteInfosCompleted += new EventHandler<GetDocDistrbuteInfosCompletedEventArgs>(DetailSendClient_GetDocDistrbuteInfosCompleted);
                                DetailSendClient.GetDocDistrbuteInfosAsync(senddoc.SENDDOCID);

                                break;
                        }
                        switch (senddoc.ISDISTRIBUTE)
                        {
                            case "0":
                                Strdistrbute = "未发布";
                                break;
                            case "1":
                                Strdistrbute = "已发布";
                                break;
                        }
                        //this.tbldistrbute.Text =Strdistrbute;
                        //this.tblsave.Text = StrSave;
                        if (senddoc.PUBLISHDATE != null)
                        {
                            this.tblPublishDate.Text = System.Convert.ToDateTime(senddoc.PUBLISHDATE).ToLongDateString() + "印发";
                        }
                        this.tblKeyWord.Text = senddoc.KEYWORDS;
                        this.tblStatus.Text = StrState;
                        //audit.IsEnabled = false;
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                        
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
                    if (objc.Count() == 0)
                        return;
                    if (objc.FirstOrDefault() == "集团本部")
                    {
                        this.tbltitlecompany.Text = "深圳市神州通投资集团有限公司";
                    }
                    else
                    {
                        this.tbltitlecompany.Text = objc.FirstOrDefault();
                    }
                    if (tmpSendDocT.ISREDDOC == "0")
                    {
                        this.tbltitlecompany.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        this.tbltitlecompany.Text += "文件";
                    }
                    
                    //this.tbltitlecompany.Text = objc.FirstOrDefault();
                }
            }
        }
        /// <summary>
        /// 从资源文件中获取部门信息
        /// </summary>
        /// <param name="StrCompanyID"></param>
        private void GetDepartmentName(string StrDepartmentID)
        {
            if (string.IsNullOrEmpty(StrDepartmentID))
                return;
            if (Application.Current.Resources["SYS_DepartmentInfo"] != null)
            {
                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> ListCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
                ListCompany = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

                if (ListCompany != null)
                {
                    var objc = from a in ListCompany
                               where a.DEPARTMENTID == StrDepartmentID
                               select a;
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT Depart = null;
                    if(objc != null)
                    {
                        if (objc.Count() > 0)
                        {
                            Depart = new T_HR_DEPARTMENT();
                            Depart = objc.FirstOrDefault();
                        }
                    }
                    if(Depart != null)
                    {
                        this.tbldepartment.Text = Depart.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        GetCompanyName(Depart.T_HR_COMPANY.COMPANYID);
                    }
                    
                }
            }
        }
        
        
        private void GetSendDocDetailInfo(V_BumfCompanySendDoc obj)
        {
            
            this.tbltitle.Text = obj.OACompanySendDoc.SENDDOCTITLE;
            this.tblsend.Text = obj.OACompanySendDoc.SEND;
            this.tblcopy.Text = obj.OACompanySendDoc.CC;            
            tblcontent.RichTextBoxContext = obj.OACompanySendDoc.CONTENT;
            this.tbldepartment.Text = obj.OACompanySendDoc.DEPARTID;
            this.tbldoctype.Text = obj.doctype.SENDDOCTYPE;
            this.tblprioritity.Text = obj.OACompanySendDoc.PRIORITIES;
            this.tblgrade.Text = obj.OACompanySendDoc.GRADED;
            //this.tbladddate.Text = obj.OACompanySendDoc.CREATEDATE.ToShortDateString();
            //this.tblupdatedate.Text = obj.OACompanySendDoc.UPDATEDATE.ToString();
            //senddoctab.TabStripPlacement = Dock.Left;
            this.tblnum.Text = obj.OACompanySendDoc.NUM;
            this.tblcontenttitle.Text = obj.OACompanySendDoc.SENDDOCTITLE;
            GetCompanyName(obj.OACompanySendDoc.OWNERCOMPANYID);
            GetDepartmentName(obj.OACompanySendDoc.OWNERDEPARTMENTID);
            string StrState = "";
            string StrSave = "";
            string Strdistrbute = "";
            switch (obj.OACompanySendDoc.CHECKSTATE)
            { 
                case "0":
                    StrState = "未提交";
                    senddoctab.Items.Remove(senddoctab.Items[2]);
                    
                    break;
                case "1":
                    StrState ="审核中";
                    InitAudit(obj.OACompanySendDoc);
                    senddoctab.Items.Remove(senddoctab.Items[1]);
                    break;
                case "2":
                    StrState = "审核通过";                    
                    InitAudit(obj.OACompanySendDoc);
                    break;
                case "3":
                    StrState = "审核未通过";
                    senddoctab.Items.Remove(senddoctab.Items[1]);
                    InitAudit(obj.OACompanySendDoc);
                    break;
            }
            switch (obj.OACompanySendDoc.ISSAVE)
            { 
                case "0":
                    StrSave = "未归档";
                    //this.spDistrbuteDetail.Visibility = Visibility.Collapsed;
                    break;
                case "1":
                    StrSave = "已归档";
                    //获取发文的详细信息
                    //this.spDistrbuteDetail.Visibility = Visibility.Visible;                    
                    DetailSendClient.GetDocDistrbuteInfosCompleted += new EventHandler<GetDocDistrbuteInfosCompletedEventArgs>(DetailSendClient_GetDocDistrbuteInfosCompleted);                    
                    DetailSendClient.GetDocDistrbuteInfosAsync(obj.OACompanySendDoc.SENDDOCID);
                    
                    break;
            }
            switch (obj.OACompanySendDoc.ISDISTRIBUTE)
            { 
                case "0":
                    Strdistrbute = "未发布";
                    break;
                case "1":
                    Strdistrbute = "已发布";
                    break;
            }
            //this.tbldistrbute.Text =Strdistrbute;
            //this.tblsave.Text = StrSave;
            //this.tblcheckstate.Text = StrState;
            
        }

        void DetailSendClient_GetDocDistrbuteInfosCompleted(object sender, GetDocDistrbuteInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    string StrViewType = "";
                    string ViewType = "";
                    string StrViewer = "发布对象  \n";
                    string StrDate = "";
                    string StrReturn = "";  //总输出
                    List<T_OA_DISTRIBUTEUSER> ListDistrbute = e.Result.ToList();
                    foreach (var bb in ListDistrbute)
                    { 
                        //bb.VIEWTYPE

                        switch (bb.VIEWTYPE)
                        {
                            case "0":
                                ViewType = "按公司发布";
                                //获取公司名称
                                StrViewer += bb.VIEWER +"\n";
                                break;
                            case "1":
                                ViewType = "按部门发布";
                                //获取部门名称
                                StrViewer += bb.VIEWER + "\n";
                                break;
                            case "2":
                                ViewType = "按个人发布";
                                //获取个人名称
                                StrViewer += bb.VIEWER + "\n";
                                break;
                        }
                        if (bb.VIEWTYPE != StrViewType)
                        {                            
                            StrViewType = bb.VIEWTYPE;
                            StrReturn += "发布类型："+ViewType + "\n";
                        }
                        if (bb.CREATEDATE.ToShortDateString() != StrDate)
                        {
                            StrDate = bb.CREATEDATE.ToShortDateString();
                            StrReturn +="发布时间："+ StrDate + "\n";
                        }
                        StrReturn += StrViewer;


                    }

                    this.tblDistrbuteDetail.Text = StrReturn;
                }
            }
        }

        
        private void InitAudit(T_OA_SENDDOC SendDoc)
        {
            //SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
            //entity.ModelCode = "CompanyDoc";
            //entity.FormID = SendDoc.SENDDOCID;
            //entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            //entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            //entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
            //entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
            //entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
            //entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
            //audit.BindingData();
        }


        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("VIEWTITLE", "COMPANYDOCUMENT");
            
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            SaveAndClose();
            
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

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };
            items.Add(item);
            
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

        #region 确定、取消
        
        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
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

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_SENDDOC>(tmpSendDocT, "OA");

            Utility.SetAuditEntity(entity, "T_OA_SENDDOC", tmpSendDocT.SENDDOCID, strXmlObjectSource);
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
            if (tmpSendDocT.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            tmpSendDocT.CHECKSTATE = state;
            //cmsfc.UpdateContraApprovalAsync(tmpSendDocT, UserState);
            DetailSendClient.SendDocInfoUpdateAsync(tmpSendDocT, StrUpdateReturn, UserState);
        }

        public string GetAuditState()
        {

            string state = "-1";
            if (tmpSendDocT != null)
                state = tmpSendDocT.CHECKSTATE;
            if (actionTypes == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }

        #endregion
    }
}
