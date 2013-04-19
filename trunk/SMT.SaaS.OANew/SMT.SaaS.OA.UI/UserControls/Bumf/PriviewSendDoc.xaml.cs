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

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class PriviewSendDoc : BaseForm,IClient
    {
        
        string TmpSendoc = "";
        private string DocNum = "";
        private string DocPublishDate = "";
        private T_OA_SENDDOC senddoc = new T_OA_SENDDOC();
        public PriviewSendDoc(string SendDocID,string StrNum,string StrPublish)
        {
            InitializeComponent();
            TmpSendoc = SendDocID;
            DocNum = StrNum;
            DocPublishDate = StrPublish;
            
            tblcontent.HideControls();
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "CompanyDoc";
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            ////ctrFile.Event_AllFilesFinished += new EventHandler<FileCountEventArgs>(Event_AllFilesFinished);
            //ctrFile.Load_fileData(SendDocID);
            DetailSendClient.GetSendDocSingleInfoByIdCompleted += new EventHandler<GetSendDocSingleInfoByIdCompletedEventArgs>(SendDocClient_GetSendDocSingleInfoByIdCompleted);
            this.Loaded += new RoutedEventHandler(CompanyDocWebPart_Loaded);
        }

        void CompanyDocWebPart_Loaded(object sender, RoutedEventArgs e)
        {            
            DetailSendClient.GetSendDocSingleInfoByIdAsync(TmpSendoc);
        }
        SmtOACommonOfficeClient DetailSendClient = new SmtOACommonOfficeClient();
        public delegate void refreshGridView();
        private T_OA_SENDDOCTYPE SelectDocType = new T_OA_SENDDOCTYPE();
        private string tmpStrcbxGrade = "";//级别
        private string tmpStrcbxProritity = ""; //缓急
        //private SmtOACommonOfficeClient SendDocClient = new SmtOACommonOfficeClient();

        void SendDocClient_GetSendDocSingleInfoByIdCompleted(object sender, GetSendDocSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        //T_OA_SENDDOC senddoc = new T_OA_SENDDOC();
                        senddoc = e.Result;
                        
                        this.tbltitle.Text = senddoc.SENDDOCTITLE;
                        this.tblsend.Text = senddoc.SEND;
                        this.tblcopy.Text = senddoc.CC;                        
                        tblcontent.RichTextBoxContext = senddoc.CONTENT;                        
                        this.tbldepartment.Text = senddoc.DEPARTID;
                        SelectDocType = senddoc.T_OA_SENDDOCTYPE;
                        this.tbldoctype.Text = SelectDocType.SENDDOCTYPE;
                        this.tblprioritity.Text = senddoc.PRIORITIES;
                        this.tblgrade.Text = senddoc.GRADED;
                        this.tblnum.Text = DocNum;//获取新的公文编号senddoc.NUM; 
                        this.tblcontenttitle.Text = senddoc.SENDDOCTITLE;
                        GetCompanyName(senddoc.OWNERCOMPANYID);
                        GetDepartmentName(senddoc.OWNERDEPARTMENTID);
                        string StrState = "";
                        string StrSave = "";                        
                        tmpStrcbxGrade = senddoc.GRADED;
                        tmpStrcbxProritity = senddoc.PRIORITIES;
                        string StrPublish = "";
                        if (!string.IsNullOrEmpty(senddoc.PUBLISHDATE.ToString()))
                        {
                            StrPublish = System.Convert.ToDateTime(senddoc.PUBLISHDATE.ToString()).ToShortDateString() + "印发";
                        }
                        if (!string.IsNullOrEmpty(senddoc.CREATEDATE.ToString()))
                        {
                            this.tbladddate.Text = System.Convert.ToDateTime(senddoc.CREATEDATE.ToString()).ToShortDateString().Replace("/","-");
                        }
                        //if (!ctrFile._files.HasAccessory)
                        //{
                        //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(GridInfo,16);
                        //}
                        this.tblKeyWord.Text = senddoc.KEYWORDS;
                        this.tblStatus.Text = "待发布";
                        tblPublishDate.Text = DocPublishDate;
                        //if (!string.IsNullOrEmpty(StrPublish))
                        //{
                        //    this.tblPublishDate.Text = DocPublishDate.Substring(0, 4) + "年" + DocPublishDate.Substring(5, 2) + "月" + DocPublishDate.Substring(8, 2) + "日印发";
                        //}
                        
                        
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
                    if (senddoc.ISREDDOC == "0")
                    {
                        this.tbltitlecompany.Foreground = new SolidColorBrush(Colors.Black); 
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
