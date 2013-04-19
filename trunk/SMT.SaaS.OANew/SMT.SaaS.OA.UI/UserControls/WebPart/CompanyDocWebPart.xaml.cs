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
using SMT.SaaS.PublicControls;
using Telerik.Windows.Documents.FormatProviders.Pdf;
using SMT.Saas.Tools.PublicInterfaceWS;
using System.Windows.Media;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class CompanyDocWebPart : BaseForm,IClient
    {
        string TmpSendoc = "";
        private string DocNum = "";
        private string DocPublishDate = "";

        private List<T_HR_POST> entlist;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_COMPANY> allCompanys;
        PublicServiceClient publicClient = new PublicServiceClient();
        private OrganizationServiceClient organClient = new OrganizationServiceClient();

        private SMTLoading loadbar = new SMTLoading();

        public CompanyDocWebPart(string SendDocID)
        {
            InitializeComponent();
            InitEvent();
            PARENT.Children.Add(loadbar);

            TmpSendoc = SendDocID;
            tblcontent.IsReadOnly = true;
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "CompanyDoc";
            
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            //ctrFile.EntityEditor = this;
            ////ctrFile.Event_AllFilesFinished += new EventHandler<FileCountEventArgs>(Event_AllFilesFinished);
            //ctrFile.Load_fileData(SendDocID);
            //DetailSendClient.GetSendDocSingleInfoByIdCompleted += new EventHandler<GetSendDocSingleInfoByIdCompletedEventArgs>(SendDocClient_GetSendDocSingleInfoByIdCompleted);
            
            this.Loaded += new RoutedEventHandler(CompanyDocWebPart_Loaded);
        }

        public CompanyDocWebPart(FormTypes formType,string SendDocID)
        {
            InitializeComponent();
            InitEvent();
            PARENT.Children.Add(loadbar);

            TmpSendoc = SendDocID;
            tblcontent.IsReadOnly = true;
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "CompanyDoc";

            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            //ctrFile.EntityEditor = this;
            ////ctrFile.Event_AllFilesFinished += new EventHandler<FileCountEventArgs>(Event_AllFilesFinished);
            //ctrFile.Load_fileData(SendDocID);
            //DetailSendClient.GetSendDocSingleInfoByIdCompleted += new EventHandler<GetSendDocSingleInfoByIdCompletedEventArgs>(SendDocClient_GetSendDocSingleInfoByIdCompleted);

            this.Loaded += new RoutedEventHandler(CompanyDocWebPart_Loaded);
        }

        void publicClient_GetContentCompleted(object sender, GetContentCompletedEventArgs e)
        {
            if (e.Error == null)
            {
               tblcontent.Document = e.Result;
            }
        }
        
        /// <summary>
        /// 供预览公文使用
        /// </summary>
        /// <param name="SendDocID"></param>
        /// <param name="StrNum"></param>
        /// <param name="StrPublish"></param>
        public CompanyDocWebPart(string SendDocID,string StrNum,string StrPublish)
        {
            InitializeComponent();
            InitEvent();
            TmpSendoc = SendDocID;
            DocNum = StrNum;
            DocPublishDate = StrPublish;
            tblcontent.IsReadOnly= true;
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "CompanyDoc";

            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            ////ctrFile.Event_AllFilesFinished += new EventHandler<FileCountEventArgs>(Event_AllFilesFinished);
            //ctrFile.Load_fileData(SendDocID);
            
            this.Loaded += new RoutedEventHandler(CompanyDocWebPart_Loaded);
        }
        private void InitEvent()
        {
            publicClient.GetContentCompleted += new EventHandler<GetContentCompletedEventArgs>(publicClient_GetContentCompleted);
            DetailSendClient.GetSendDocSingleInfoByIdCompleted += new EventHandler<GetSendDocSingleInfoByIdCompletedEventArgs>(SendDocClient_GetSendDocSingleInfoByIdCompleted);
            organClient.GetALLCompanyViewCompleted += new EventHandler<GetALLCompanyViewCompletedEventArgs>(organClient_GetALLCompanyViewCompleted);
            organClient.GetAllDepartmentViewCompleted += new EventHandler<GetAllDepartmentViewCompletedEventArgs>(organClient_GetAllDepartmentViewCompleted);
            organClient.GetAllPostViewCompleted += new EventHandler<GetAllPostViewCompletedEventArgs>(organClient_GetAllPostViewCompleted);

        }

        void CompanyDocWebPart_Loaded(object sender, RoutedEventArgs e)
        {
            //RefreshUI(RefreshedTypes.ShowProgressBar);//打开进度圈
            //loadbar.Start();
            organClient.GetALLCompanyViewAsync("");
            Utility.InitFileLoad("CompanyDoc", TmpSendoc,FormTypes.Browse , uploadFile);
            
        }
        SmtOACommonOfficeClient DetailSendClient = new SmtOACommonOfficeClient();
        public delegate void refreshGridView();
        private T_OA_SENDDOCTYPE SelectDocType = new T_OA_SENDDOCTYPE();
        private string tmpStrcbxGrade = "";//级别
        private string tmpStrcbxProritity = ""; //缓急
        T_OA_SENDDOC senddoc = new T_OA_SENDDOC();
        //private SmtOACommonOfficeClient SendDocClient = new SmtOACommonOfficeClient();

        void SendDocClient_GetSendDocSingleInfoByIdCompleted(object sender, GetSendDocSingleInfoByIdCompletedEventArgs e)
        {
            loadbar.Stop();
            //RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        
                        senddoc = e.Result;
                        publicClient.GetContentAsync(senddoc.SENDDOCID);
                        this.tbltitle.Text = string.IsNullOrEmpty(senddoc.SENDDOCTITLE) ? "":senddoc.SENDDOCTITLE;
                        this.tblsend.Text = string.IsNullOrEmpty( senddoc.SEND) ? "":senddoc.SEND;
                        this.tblcopy.Text =string.IsNullOrEmpty( senddoc.CC) ? "":senddoc.CC;                        
                        //tblcontent.Document = senddoc.CONTENT;                        
                        //this.tbldepartment.Text = senddoc.DEPARTID;
                        SelectDocType = senddoc.T_OA_SENDDOCTYPE;
                        if (SelectDocType != null)
                        {
                            this.tbldoctype.Text = SelectDocType.SENDDOCTYPE;
                        }
                        this.tblprioritity.Text =string.IsNullOrEmpty( senddoc.PRIORITIES) ? "":senddoc.PRIORITIES;
                        this.tblgrade.Text = string.IsNullOrEmpty(senddoc.GRADED) ? "":senddoc.GRADED;
                        tblKeyWord.Text = string.IsNullOrEmpty(senddoc.KEYWORDS) ? "":senddoc.KEYWORDS;
                        if (string.IsNullOrEmpty(DocNum))
                        {
                            this.tblnum.Text = senddoc.NUM;
                        }
                        else
                        {
                            this.tblnum.Text = DocNum;
                        }
                        this.tblcontenttitle.Text = senddoc.SENDDOCTITLE;
                        
                        GetDepartmentName(senddoc.DEPARTID);
                        string StrState = "";
                        string StrSave = "";                        
                        tmpStrcbxGrade = senddoc.GRADED;
                        tmpStrcbxProritity = senddoc.PRIORITIES;
                        //if (!ctrFile._files.HasAccessory)
                        //{
                        //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.GridInfo, 16);
                        //}
                        if (senddoc.ISREDDOC == "0")
                        {
                            SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.GridInfo, 6);
                            SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.GridInfo, 7);
                            SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.GridInfo, 8);
                        }
                        if (senddoc.CREATEDATE !=null)
                        {
                            //tbladddate.Text = System.Convert.ToDateTime(senddoc.PUBLISHDATE).ToShortDateString() + " " + System.Convert.ToDateTime(senddoc.PUBLISHDATE).ToShortTimeString();
                            tbladddate.Text = System.Convert.ToDateTime(senddoc.CREATEDATE).ToShortDateString() + " " + System.Convert.ToDateTime(senddoc.CREATEDATE).ToShortTimeString();
                        }
                        if (string.IsNullOrEmpty(DocPublishDate))
                        {
                           this.tblPublishDate.Text = DocPublishDate;
                        }
                        else
                        {
                            if (senddoc.PUBLISHDATE != null)
                            {
                                this.tblPublishDate.Text = System.Convert.ToDateTime(senddoc.PUBLISHDATE).ToLongDateString() + "印发";
                                //StrPublish = dt.ToString("yyyymmdd"); 
                            }
                        }
                        if (senddoc.ISDISTRIBUTE == "1")
                        {
                            this.tblStatus.Text = "已发布";
                        }
                        else
                        {
                            this.tblStatus.Text = "未发布";
                        }
                        if (!string.IsNullOrEmpty(senddoc.OWNERNAME))
                        {
                            tbladduser.Text = senddoc.OWNERNAME;
                        }
                        
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
                    else
                    {
                        this.tbltitlecompany.Text += "文件";
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
                    if (objc != null)
                    {
                        if (objc.Count() > 0)
                        {
                            Depart = new T_HR_DEPARTMENT();
                            Depart = objc.FirstOrDefault();
                        }
                    }
                    if (Depart != null)
                    {
                        this.tbldepartment.Text = Depart.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        GetCompanyName(Depart.T_HR_COMPANY.COMPANYID);
                    }
                    
                }



            }
        }


        #region 组织架构
        void organClient_GetAllPostViewCompleted(object sender, GetAllPostViewCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {

                    List<V_POST> vpostList = e.Result.ToList();
                    entlist = new List<T_HR_POST>();
                    foreach (var ent in vpostList)
                    {
                        T_HR_POST pt = new T_HR_POST();
                        pt.POSTID = ent.POSTID;
                        pt.FATHERPOSTID = ent.FATHERPOSTID;
                        pt.CHECKSTATE = ent.CHECKSTATE;
                        pt.EDITSTATE = ent.EDITSTATE;

                        pt.T_HR_POSTDICTIONARY = new T_HR_POSTDICTIONARY();
                        pt.T_HR_POSTDICTIONARY.POSTDICTIONARYID = Guid.NewGuid().ToString();
                        pt.T_HR_POSTDICTIONARY.POSTNAME = ent.POSTNAME;

                        pt.T_HR_DEPARTMENT = new T_HR_DEPARTMENT();
                        pt.T_HR_DEPARTMENT = allDepartments.Where(s => s.DEPARTMENTID == ent.DEPARTMENTID).FirstOrDefault();

                        entlist.Add(pt);
                    }
                    if (App.Current.Resources["SYS_PostInfo"] != null)
                    {
                        App.Current.Resources.Remove("SYS_PostInfo");
                        App.Current.Resources.Add("SYS_PostInfo", entlist);
                    }
                    else
                    {
                        App.Current.Resources.Add("SYS_PostInfo", entlist);
                    }
                }
            }
            //this.Loaded += new RoutedEventHandler(BusinessApplicationsForm_Loaded);
            loadbar.Start();
            DetailSendClient.GetSendDocSingleInfoByIdAsync(TmpSendoc);
        }

        void organClient_GetAllDepartmentViewCompleted(object sender, GetAllDepartmentViewCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    List<V_DEPARTMENT> entTemps = e.Result.ToList();
                    allDepartments = new List<T_HR_DEPARTMENT>();
                    var ents = entTemps.OrderBy(c => c.FATHERID);
                    foreach (var ent in ents)
                    {
                        T_HR_DEPARTMENT dep = new T_HR_DEPARTMENT();
                        dep.DEPARTMENTID = ent.DEPARTMENTID;
                        dep.FATHERID = ent.FATHERID;
                        dep.FATHERTYPE = ent.FATHERTYPE;
                        dep.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                        dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = ent.DEPARTMENTDICTIONARYID;
                        dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = ent.DEPARTMENTNAME;
                        dep.T_HR_COMPANY = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                        dep.T_HR_COMPANY = allCompanys.Where(s => s.COMPANYID == ent.COMPANYID).FirstOrDefault();

                        dep.DEPARTMENTBOSSHEAD = ent.DEPARTMENTBOSSHEAD;
                        dep.SORTINDEX = ent.SORTINDEX;
                        dep.CHECKSTATE = ent.CHECKSTATE;
                        dep.EDITSTATE = ent.EDITSTATE;
                        allDepartments.Add(dep);
                    }
                    if (App.Current.Resources["SYS_DepartmentInfo"] != null)
                    {
                        App.Current.Resources.Remove("SYS_DepartmentInfo");
                        App.Current.Resources.Add("SYS_DepartmentInfo", allDepartments);
                    }
                    else
                    {
                        App.Current.Resources.Add("SYS_DepartmentInfo", allDepartments);
                    }
                    organClient.GetAllPostViewAsync("");
                }
            }
        }

        void organClient_GetALLCompanyViewCompleted(object sender, GetALLCompanyViewCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    List<V_COMPANY> entTemps = e.Result.ToList();
                    allCompanys = new List<T_HR_COMPANY>();
                    var ents = entTemps.OrderBy(c => c.FATHERID);
                    foreach (var ent in ents)
                    {
                        T_HR_COMPANY company = new T_HR_COMPANY();
                        company.COMPANYID = ent.COMPANYID;
                        company.CNAME = ent.CNAME;
                        company.ENAME = ent.ENAME;
                        if (!string.IsNullOrEmpty(ent.BRIEFNAME))
                        {
                            company.BRIEFNAME = ent.BRIEFNAME;
                        }
                        else
                        {
                            company.BRIEFNAME = ent.CNAME;
                        }

                        company.COMPANRYCODE = ent.COMPANRYCODE;
                        company.SORTINDEX = ent.SORTINDEX;
                        if (!string.IsNullOrEmpty(ent.FATHERCOMPANYID))
                        {
                            company.T_HR_COMPANY2 = new T_HR_COMPANY();
                            company.T_HR_COMPANY2.COMPANYID = ent.FATHERCOMPANYID;
                            company.T_HR_COMPANY2.CNAME = entTemps.Where(s => s.COMPANYID == ent.FATHERCOMPANYID).FirstOrDefault().CNAME;
                        }
                        company.FATHERID = ent.FATHERID;
                        company.FATHERTYPE = ent.FATHERTYPE;
                        company.CHECKSTATE = ent.CHECKSTATE;
                        company.EDITSTATE = ent.EDITSTATE;
                        allCompanys.Add(company);
                    }
                    if (App.Current.Resources["SYS_CompanyInfo"] != null)
                    {
                        App.Current.Resources.Remove("SYS_CompanyInfo");
                        App.Current.Resources.Add("SYS_CompanyInfo", allCompanys);
                    }
                    else
                    {
                        App.Current.Resources.Add("SYS_CompanyInfo", allCompanys);
                    }

                    organClient.GetAllDepartmentViewAsync("");
                }
            }
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

        #region 显示上传控件        
        
        public void FileLoadedCompleted()
        {
            //_VM.Get_ApporvalAsync(approvalid);
            //if (!ctrFile._files.HasAccessory)
            //{
            //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.GridInfo, 16);
            //}
        }
        #endregion
    }
}
