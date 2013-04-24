/********************************************************************************
//出差主页面，alter by ken 2013/3/27
*********************************************************************************/
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
using SMT.SaaS.OA.UI.Views.Travelmanagement;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.Saas.Tools.OrganizationWS;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class BusinessApplicationsForm : BaseForm, IEntityEditor
    {
        #region 全局变量
        private string businesstrID = string.Empty;
        //public string missionReportsID = string.Empty;
        public string travelReimbursementID = string.Empty;
        //public string reportCheckState = string.Empty;
        public string trCheckState = string.Empty;
        public string traverlCheck = string.Empty;
        public string FormId = string.Empty;//窗体id
        public V_Travelmanagement TraveView = new V_Travelmanagement();
        public TravelType tlType;//类型
        private FormTypes actions;
        private SmtOAPersonOfficeClient Travelmanagement;
        public EntityBrowser ParentEntityBrowser { get; set; }
        //private List<T_HR_POST> entlist;
        //private List<T_HR_DEPARTMENT> allDepartments;
        //private List<T_HR_COMPANY> allCompanys;
        //private OrganizationServiceClient organClient = new OrganizationServiceClient();
        #endregion

        public BusinessApplicationsForm(FormTypes action, string businesstrID)
        {

            InitializeComponent();

            this.actions = action;
            this.businesstrID = businesstrID;
            InitEvent();
            this.Loaded += new RoutedEventHandler(BusinessApplicationsForm_Loaded);
        }

        public BusinessApplicationsForm(FormTypes action, string businesstrID, bool WhetherReimbursement)
        {
            InitializeComponent();
            if (WhetherReimbursement == true)
            {
                TabTravel.SelectedIndex = 1;
            }
            else
            {
                TabTravel.SelectedIndex = 2;
            }
            this.actions = action;
            this.businesstrID = businesstrID;
            InitEvent();
            this.Loaded += new RoutedEventHandler(BusinessApplicationsForm_Loaded);
        }

        private void InitEvent()
        {
            Travelmanagement = new SmtOAPersonOfficeClient();
            Travelmanagement.GetAccordingToBusinesstripIdCheckCompleted += new EventHandler<GetAccordingToBusinesstripIdCheckCompletedEventArgs>(Travelmanagement_GetAccordingToBusinesstripIdCheckCompleted);

            //organClient.GetALLCompanyViewCompleted += new EventHandler<GetALLCompanyViewCompletedEventArgs>(organClient_GetALLCompanyViewCompleted);
            //organClient.GetAllDepartmentViewCompleted += new EventHandler<GetAllDepartmentViewCompletedEventArgs>(organClient_GetAllDepartmentViewCompleted);
            //organClient.GetAllPostViewCompleted += new EventHandler<GetAllPostViewCompletedEventArgs>(organClient_GetAllPostViewCompleted);
        }

        /// <summary>
        /// 通过出差申请ID查询报告、报销的ID及状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BusinessApplicationsForm_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);//打开进度圈
            //organClient.GetALLCompanyViewAsync("");
            //2013/3/27停止加载所有组织架构，直接加载出差业务数据
            Travelmanagement.GetAccordingToBusinesstripIdCheckAsync(businesstrID);
        }

        void Travelmanagement_GetAccordingToBusinesstripIdCheckCompleted(object sender, GetAccordingToBusinesstripIdCheckCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                TraveView = e.Result;
                //missionReportsID = TraveView.ReportId;
                travelReimbursementID = TraveView.TrId;
                traverlCheck = TraveView.TraveAppCheckState;
                //reportCheckState = TraveView.ReportCheckState;
                trCheckState = TraveView.TrCheckState;

            }
            //if (string.IsNullOrEmpty(missionReportsID) || missionReportsID == "空")//如果没有报告隐藏Tab
            //{
            //    TabReport.Visibility = Visibility.Collapsed;//出差报告Tab
            //}
            //else
            //{
            //    if (reportCheckState == "0" || reportCheckState == "1" || reportCheckState == "3")
            //    {
            //        TabTravel.SelectedIndex = 1;
            //        if (actions != FormTypes.Browse && actions != FormTypes.Edit)
            //        {
            //            tbbTravelapplication.Visibility = Visibility.Collapsed;//隐藏出差申请Tab
            //            traveformFather.Visibility = Visibility.Collapsed;//隐藏出差报销Tab
            //        }
            //    }
            //}
            //if (string.IsNullOrEmpty(travelReimbursementID) || travelReimbursementID == "空")
            //修改如果出差申请没审核通过则出差报销不显示
            //if (traverlCheck != Convert.ToInt32(CheckStates.Approved).ToString())
            //{
            //    traveformFather.Visibility = Visibility.Collapsed;//出差报销Tab
            //}
            //判断出差报销是否存在
            bool BoolReimID = !string.IsNullOrEmpty(travelReimbursementID) && travelReimbursementID != "空";
            if (false == BoolReimID) traveformFather.Visibility = Visibility.Collapsed;//出差报销Tab

            if (traverlCheck != Convert.ToInt32(CheckStates.Approved).ToString())
            {
                traveformFather.Visibility = Visibility.Collapsed;//出差报销Tab
            }
            else
            {
                if (trCheckState == "0" || trCheckState == "1" || trCheckState == "3")
                {
                    TabTravel.SelectedIndex = 2;
                    if (actions != FormTypes.Browse && actions != FormTypes.Edit)
                    {
                        tbbTravelapplication.Visibility = Visibility.Collapsed;//隐藏出差申请Tab
                        //TabReport.Visibility = Visibility.Collapsed;//隐藏出差报告Tab
                    }
                }
            }

            if (actions == FormTypes.New)//新增
            {
                RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.New, "");
                TravelapplicationForm.ParentEntityBrowser = this.ParentEntityBrowser;
                EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                TravelBrowser.FormType = FormTypes.New;
                TravelBrowser.MinWidth = 980;
                TravelBrowser.MinHeight = 445;
                TravelapplicationGd.Children.Add(TravelBrowser);
            }
            else if (actions == FormTypes.Edit)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                //TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Edit, businesstrID);
                //2012-9-21 ljx 
                TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Edit, TraveView.Travelmanagement.BUSINESSTRIPID);
                TravelapplicationForm.ParentEntityBrowser = this.ParentEntityBrowser;
                EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                TravelBrowser.FormType = FormTypes.Edit;
                //TravelBrowser.MinWidth = 980;
                TravelBrowser.MinHeight = 445;
                TravelapplicationGd.Children.Add(TravelBrowser);


                if (BoolReimID && traverlCheck == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    //出差报销
                    TravelReimbursementControl TravelReimbursementForm = new TravelReimbursementControl(FrameworkUI.FormTypes.Edit, travelReimbursementID, businesstrID);
                    TravelReimbursementForm.ParentEntityBrowser = this.ParentEntityBrowser;
                    EntityBrowser TravelReimbursementBrowser = new EntityBrowser(TravelReimbursementForm);
                    TravelReimbursementBrowser.FormType = FormTypes.Edit;
                    //TravelReimbursementBrowser.MinWidth = 980;
                    TravelReimbursementBrowser.MinHeight = 445;
                    TravelReimbursementGd.Children.Add(TravelReimbursementBrowser);
                }
            }
            else if (actions == FormTypes.Browse)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Browse, businesstrID);
                EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                TravelBrowser.FormType = FormTypes.Browse;
                TravelBrowser.MinWidth = 728;
                TravelBrowser.MinHeight = 445;
                TravelapplicationGd.Children.Add(TravelBrowser);


                if (BoolReimID && traverlCheck == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    //出差报销
                    TravelReimbursementControl TravelReimbursementForm = new TravelReimbursementControl(FrameworkUI.FormTypes.Browse, travelReimbursementID, businesstrID);
                    EntityBrowser TravelReimbursementBrowser = new EntityBrowser(TravelReimbursementForm);
                    TravelReimbursementBrowser.FormType = FormTypes.Browse;
                    TravelReimbursementBrowser.MinWidth = 728;
                    TravelReimbursementBrowser.MinHeight = 445;
                    TravelReimbursementGd.Children.Add(TravelReimbursementBrowser);
                }
            }
            else if (actions == FormTypes.Audit)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Audit, businesstrID);
                EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                TravelBrowser.FormType = FormTypes.Audit;
                TravelBrowser.MinWidth = 728;
                TravelBrowser.MinHeight = 445;
                TravelapplicationGd.Children.Add(TravelBrowser);


                if (BoolReimID && traverlCheck == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    //出差报销
                    TravelReimbursementControl TravelReimbursementForm = new TravelReimbursementControl(FrameworkUI.FormTypes.Audit, travelReimbursementID, businesstrID);
                    EntityBrowser TravelReimbursementBrowser = new EntityBrowser(TravelReimbursementForm);
                    TravelReimbursementBrowser.FormType = FormTypes.Audit;
                    TravelReimbursementBrowser.MinWidth = 728;
                    TravelReimbursementBrowser.MinHeight = 445;
                    TravelReimbursementGd.Children.Add(TravelReimbursementBrowser);
                }
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                if (traverlCheck != "3")
                {
                    tbbTravelapplication.Visibility = Visibility.Collapsed;//出差申请Tab
                }
                else
                {
                    TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Resubmit, businesstrID);
                    EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                    TravelBrowser.FormType = FormTypes.Resubmit;
                    TravelBrowser.MinWidth = 980;
                    TravelBrowser.MinHeight = 445;
                    TravelapplicationGd.Children.Add(TravelBrowser);
                }

                if (trCheckState != "3")
                {
                    traveformFather.Visibility = Visibility.Collapsed;//出差报销Tab
                }
                else
                {
                    if (BoolReimID && traverlCheck == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //出差报销
                        TravelReimbursementControl TravelReimbursementForm = new TravelReimbursementControl(FrameworkUI.FormTypes.Resubmit, travelReimbursementID, businesstrID);
                        EntityBrowser TravelReimbursementBrowser = new EntityBrowser(TravelReimbursementForm);
                        TravelReimbursementBrowser.FormType = FormTypes.Resubmit;
                        TravelReimbursementBrowser.MinWidth = 980;
                        TravelReimbursementBrowser.MinHeight = 445;
                        TravelReimbursementGd.Children.Add(TravelReimbursementBrowser);
                    }
                }
            }
        }

        #region 组织架构
        //void organClient_GetAllPostViewCompleted(object sender, GetAllPostViewCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {

        //            List<V_POST> vpostList = e.Result.ToList();
        //            entlist = new List<T_HR_POST>();
        //            foreach (var ent in vpostList)
        //            {
        //                T_HR_POST pt = new T_HR_POST();
        //                pt.POSTID = ent.POSTID;
        //                pt.FATHERPOSTID = ent.FATHERPOSTID;
        //                pt.CHECKSTATE = ent.CHECKSTATE;
        //                pt.EDITSTATE = ent.EDITSTATE;

        //                pt.T_HR_POSTDICTIONARY = new T_HR_POSTDICTIONARY();
        //                pt.T_HR_POSTDICTIONARY.POSTDICTIONARYID = Guid.NewGuid().ToString();
        //                pt.T_HR_POSTDICTIONARY.POSTNAME = ent.POSTNAME;

        //                pt.T_HR_DEPARTMENT = new T_HR_DEPARTMENT();
        //                pt.T_HR_DEPARTMENT = allDepartments.Where(s => s.DEPARTMENTID == ent.DEPARTMENTID).FirstOrDefault();

        //                entlist.Add(pt);
        //            }
        //            if (App.Current.Resources["SYS_PostInfo"] != null)
        //            {
        //                App.Current.Resources.Remove("SYS_PostInfo");
        //                App.Current.Resources.Add("SYS_PostInfo", entlist);
        //            }
        //            else
        //            {
        //                App.Current.Resources.Add("SYS_PostInfo", entlist);
        //            }
        //        }
        //    }
        //    //this.Loaded += new RoutedEventHandler(BusinessApplicationsForm_Loaded);
        //    Travelmanagement.GetAccordingToBusinesstripIdCheckAsync(businesstrID);
        //}

        //void organClient_GetAllDepartmentViewCompleted(object sender, GetAllDepartmentViewCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            List<V_DEPARTMENT> entTemps = e.Result.ToList();
        //            allDepartments = new List<T_HR_DEPARTMENT>();
        //            var ents = entTemps.OrderBy(c => c.FATHERID);
        //            foreach (var ent in ents)
        //            {
        //                T_HR_DEPARTMENT dep = new T_HR_DEPARTMENT();
        //                dep.DEPARTMENTID = ent.DEPARTMENTID;
        //                dep.FATHERID = ent.FATHERID;
        //                dep.FATHERTYPE = ent.FATHERTYPE;
        //                dep.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
        //                dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = ent.DEPARTMENTDICTIONARYID;
        //                dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = ent.DEPARTMENTNAME;
        //                dep.T_HR_COMPANY = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
        //                dep.T_HR_COMPANY = allCompanys.Where(s => s.COMPANYID == ent.COMPANYID).FirstOrDefault();

        //                dep.DEPARTMENTBOSSHEAD = ent.DEPARTMENTBOSSHEAD;
        //                dep.SORTINDEX = ent.SORTINDEX;
        //                dep.CHECKSTATE = ent.CHECKSTATE;
        //                dep.EDITSTATE = ent.EDITSTATE;
        //                allDepartments.Add(dep);
        //            }
        //            if (App.Current.Resources["SYS_DepartmentInfo"] != null)
        //            {
        //                App.Current.Resources.Remove("SYS_DepartmentInfo");
        //                App.Current.Resources.Add("SYS_DepartmentInfo", allDepartments);
        //            }
        //            else
        //            {
        //                App.Current.Resources.Add("SYS_DepartmentInfo", allDepartments);
        //            }
        //            organClient.GetAllPostViewAsync("");
        //        }
        //    }
        //}

        //void organClient_GetALLCompanyViewCompleted(object sender, GetALLCompanyViewCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            List<V_COMPANY> entTemps = e.Result.ToList();
        //            allCompanys = new List<T_HR_COMPANY>();
        //            var ents = entTemps.OrderBy(c => c.FATHERID);
        //            foreach (var ent in ents)
        //            {
        //                T_HR_COMPANY company = new T_HR_COMPANY();
        //                company.COMPANYID = ent.COMPANYID;
        //                company.CNAME = ent.CNAME;
        //                company.ENAME = ent.ENAME;
        //                if (!string.IsNullOrEmpty(ent.BRIEFNAME))
        //                {
        //                    company.BRIEFNAME = ent.BRIEFNAME;
        //                }
        //                else
        //                {
        //                    company.BRIEFNAME = ent.CNAME;
        //                }

        //                company.COMPANRYCODE = ent.COMPANRYCODE;
        //                company.SORTINDEX = ent.SORTINDEX;
        //                if (!string.IsNullOrEmpty(ent.FATHERCOMPANYID))
        //                {
        //                    company.T_HR_COMPANY2 = new T_HR_COMPANY();
        //                    company.T_HR_COMPANY2.COMPANYID = ent.FATHERCOMPANYID;
        //                    company.T_HR_COMPANY2.CNAME = entTemps.Where(s => s.COMPANYID == ent.FATHERCOMPANYID).FirstOrDefault().CNAME;
        //                }
        //                company.FATHERID = ent.FATHERID;
        //                company.FATHERTYPE = ent.FATHERTYPE;
        //                company.CHECKSTATE = ent.CHECKSTATE;
        //                company.EDITSTATE = ent.EDITSTATE;
        //                allCompanys.Add(company);
        //            }
        //            if (App.Current.Resources["SYS_CompanyInfo"] != null)
        //            {
        //                App.Current.Resources.Remove("SYS_CompanyInfo");
        //                App.Current.Resources.Add("SYS_CompanyInfo", allCompanys);
        //            }
        //            else
        //            {
        //                App.Current.Resources.Add("SYS_CompanyInfo", allCompanys);
        //            }

        //            organClient.GetAllDepartmentViewAsync("");
        //        }
        //    }
        //}
        #endregion

      
        #region IEntityEditor 成员
        public string GetTitle()
        {
            return "出差管理";
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
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
        private void Close()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion
    }
}
