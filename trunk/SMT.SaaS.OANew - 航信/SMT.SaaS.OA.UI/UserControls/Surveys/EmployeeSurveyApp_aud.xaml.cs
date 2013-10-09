using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmployeeSurveyApp_aud : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        EmployeeSurvey_sel frmD;
        /// <summary>
        /// 方案
        /// </summary>
        private T_OA_REQUIRE _survey;
        public T_OA_REQUIRE _Survey { get { return _survey; } set { _survey = value; } }
        public ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER> _osub;

        SmtOACommonOfficeClient DocDistrbuteClient;
        private SmtOAPersonOfficeClient _VM;

        private RefreshedTypes saveType;
        private bool isSubmitFlow = false;
        private FormTypes types;
        private string requireId = string.Empty;

        /// <summary>
        /// save 要保存的发布对象
        /// </summary>
        ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER> distributeLists;
        /// <summary>
        /// select操作之后, 最新选择的发布对象
        /// </summary>
        private List<ExtOrgObj> issuanceExtOrgObj;
        #endregion

        #region 构造函数
       void EmployeeSurveyApp_aud_Loaded(object sender, RoutedEventArgs e)
        {
            _osub = new ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER>();
            distributeLists = new ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER>();
            _VM = new SmtOAPersonOfficeClient();
            DocDistrbuteClient = new SmtOACommonOfficeClient();
            issuanceExtOrgObj = new List<ExtOrgObj>();
            _VM.Upd_ESurveyAppCompleted += new EventHandler<Upd_ESurveyAppCompletedEventArgs>(Upd_ESurveyAppCompleted);
            //发布
            DocDistrbuteClient.GetDocDistrbuteInfosCompleted += new EventHandler<GetDocDistrbuteInfosCompletedEventArgs>(DocDistrbuteClient_GetDocDistrbuteInfosCompleted);
            _VM.Upd_ESurveyAppCompleted += new EventHandler<Upd_ESurveyAppCompletedEventArgs>(Upd_ESurveyAppCompleted);
            //发布
            DocDistrbuteClient.GetDocDistrbuteInfosCompleted += new EventHandler<GetDocDistrbuteInfosCompletedEventArgs>(DocDistrbuteClient_GetDocDistrbuteInfosCompleted);
            _VM.Get_ESurveyAppCompleted += new EventHandler<Get_ESurveyAppCompletedEventArgs>(Get_ESurveyAppCompleted);
            Load_Data();
        }
        public EmployeeSurveyApp_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            this.types = operationType;
            this.requireId = SendDocID;
            this.Loaded += new RoutedEventHandler(EmployeeSurveyApp_aud_Loaded);
        }
        #endregion

        #region Load_Data()
        private void Load_Data()
        {
            DocDistrbuteClient.GetDocDistrbuteInfosAsync(requireId);
            _VM.Get_ESurveyAppAsync(requireId);
            if (types == FormTypes.Browse || types == FormTypes.Audit)
            {
                this.dpStartDate.IsEnabled = false;
                this.dpEndDate.IsEnabled = false;
                this.ckbOName.IsEnabled = false;
                this.ckbOptFlag.IsEnabled = false;
                this.dg.IsEnabled = false;
            }
        }
        #endregion

        #region IEntityEditor

        public string GetStatus() { return ""; }

        public string GetTitle()
        {
            return Utility.GetResourceStr("EmployeeSurveyApp");
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            return items;
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("InfoDetail"),
                Tooltip = Utility.GetResourceStr("InfoDetail")
            };
            items.Add(item);
            return items;
        }

        public void DoAction(string actionType)
        {

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

        #region 保存
        private void Save()
        {
            _survey.STARTDATE = DateTime.Parse(dpStartDate.Text);
            _survey.ENDDATE = DateTime.Parse(dpEndDate.Text);
            _survey.WAY = (bool)ckbOName.IsChecked ? "1" : "0";
            _survey.OPTFLAG = (bool)ckbOptFlag.IsChecked ? "1" : "0";
            _VM.Upd_ESurveyAppAsync(_survey);
        }
        #endregion

        #region 修改
        void Upd_ESurveyAppCompleted(object sender, Upd_ESurveyAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {         
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
                if (e.Result > 0)
                {
                    Utility.ShowMessageBox("SUCCESSAUDIT", isSubmitFlow, true);//审核成功
                    RefreshUI(RefreshedTypes.CloseAndReloadData);

                    RefreshUI(RefreshedTypes.All);
                }
                else
                {
                    Utility.ShowMessageBox("FAILURETOAPPROVE", isSubmitFlow, false);//审核失败！
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region 根据ID查询
        void Get_ESurveyAppCompleted(object sender, Get_ESurveyAppCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                _survey = e.Result;

                txtTitle.Text = _survey.APPTITLE;
                dpStartDate.Text = Convert.ToDateTime(_survey.STARTDATE).ToShortDateString();
                dpEndDate.Text = Convert.ToDateTime(_survey.ENDDATE).ToShortDateString();
                //if (!string.IsNullOrEmpty(_survey.WAY.ToString()))
                //{
                //    foreach (T_SYS_DICTIONARY Region in cmbWay.Items)
                //    {
                //        if (Region.DICTIONARYVALUE == Convert.ToDecimal(_survey.WAY))
                //        {
                //            cmbWay.SelectedItem = Region;
                //            break;
                //        }
                //    }
                //}
                //cmbWay.SelectedIndex = int.Parse(_survey.WAY);
                ckbOptFlag.IsChecked = _survey.OPTFLAG == "1" ? true : false;
                ckbOName.IsChecked = _survey.WAY == "1" ? true : false;
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
        }
        #endregion

        #region 调查对象
        private IssuanceObjectType GetObjectType(SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj issuanceExtOrgObj)
        {
            if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company)
            {
                return IssuanceObjectType.Company;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department)
            {
                return IssuanceObjectType.Department;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post)
            {
                return IssuanceObjectType.Post;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel)
            {
                return IssuanceObjectType.Employee;
            }
            return IssuanceObjectType.Company;
        }
        #endregion 调查对象

        #region 添加发布对象
        private void AddIssuanObj()
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    issuanceExtOrgObj = ent;
                    BindData();
                }
            };
            lookup.MultiSelected = true;
            lookup.SelectSameGradeOnly = true;
            lookup.Show();
        }
        #endregion

        #region 保存 发布对象
        private void AddDistributeObjList(SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj issuanceExtOrgObj, string issuanceID)
        {
            SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER distributeTmp = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER();
            distributeTmp.DISTRIBUTEUSERID = Guid.NewGuid().ToString();
            distributeTmp.MODELNAME = "EmployeeSurveyApp";
            distributeTmp.FORMID = issuanceID;
            distributeTmp.VIEWTYPE = ((int)GetObjectType(issuanceExtOrgObj)).ToString();
            if (distributeTmp.VIEWTYPE == ((int)IssuanceObjectType.Post).ToString())    //如果是选择岗位，则保存岗位级别
            {
                T_HR_POST hr = (T_HR_POST)issuanceExtOrgObj.ObjectInstance;
                if (!string.IsNullOrEmpty(hr.POSTLEVEL.ToString()))
                    distributeTmp.VIEWER = hr.POSTLEVEL.ToString();
                else
                    distributeTmp.VIEWER = hr.T_HR_POSTDICTIONARY.POSTLEVEL.ToString();
            }
            else
            {
                distributeTmp.VIEWER = issuanceExtOrgObj.ObjectID;
            }
            distributeTmp.CREATEDATE = DateTime.Now;
            distributeTmp.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            distributeTmp.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            distributeTmp.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            distributeTmp.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            distributeTmp.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            distributeTmp.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            distributeTmp.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            distributeTmp.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            distributeTmp.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            distributeTmp.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            distributeLists.Add(distributeTmp);
        }
        #endregion

        #region 绑定发布
        private void BindData()
        {

            if (issuanceExtOrgObj == null || issuanceExtOrgObj.Count < 1)
            {
                dg.ItemsSource = null;
                return;
            }
            else
                dg.ItemsSource = issuanceExtOrgObj;

        }
        #endregion

        #region 获取发布对象
        void DocDistrbuteClient_GetDocDistrbuteInfosCompleted(object sender, GetDocDistrbuteInfosCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        List<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER> distributeList = e.Result.ToList();

                        foreach (var h in distributeList)
                        {
                            object obj = new object();
                            SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj extOrgObj = new SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj();
                            if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company).ToString())
                            {
                                T_HR_COMPANY tmp = new T_HR_COMPANY();
                                tmp.COMPANYID = h.VIEWER;
                                //tmp.CNAME = "";
                                tmp.CNAME = Utility.GetCompanyName(tmp.COMPANYID);
                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department).ToString())
                            {
                                T_HR_DEPARTMENT tmp = new T_HR_DEPARTMENT();
                                tmp.DEPARTMENTID = h.VIEWER;
                                T_HR_DEPARTMENTDICTIONARY tmpdict = new T_HR_DEPARTMENTDICTIONARY();
                                //tmpdict.DEPARTMENTNAME = "";
                                tmpdict.DEPARTMENTNAME = Utility.GetDepartmentName(h.VIEWER);
                                tmp.T_HR_DEPARTMENTDICTIONARY = tmpdict;
                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post).ToString())
                            {
                                T_HR_POST tmp = new T_HR_POST();
                                tmp.POSTLEVEL = System.Convert.ToDecimal(h.VIEWER);
                                T_HR_POSTDICTIONARY tmpdict = new T_HR_POSTDICTIONARY();
                                //tmpdict.POSTNAME = "";
                                tmpdict.POSTNAME = Utility.GetPostName(h.VIEWER);
                                tmp.T_HR_POSTDICTIONARY = tmpdict;

                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel).ToString())
                            {
                                T_HR_EMPLOYEE tmp = new T_HR_EMPLOYEE();
                                tmp.EMPLOYEEID = h.VIEWER;
                                tmp.EMPLOYEECNAME = "";
                                obj = tmp;
                            }
                            extOrgObj.ObjectInstance = obj;

                            issuanceExtOrgObj.Add(extOrgObj);
                        }
                        BindData();
                    }
                }
                else
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }
        #endregion

        #region 发布完成事件
        void DocDistrbuteInfoUpdateCompleted(object sender, DocDistrbuteInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    issuanceExtOrgObj.Clear();
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ISSUEDOCUMENTSUCCESED"));
                    if (isSubmitFlow)
                        saveType = RefreshedTypes.CloseAndReloadData;
                }
                else
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ISSUEDOCUMENTFAILED"));
            }
            else
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ISSUEDOCUMENTFAILED"));
            RefreshUI(saveType);
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_REQUIRE>(_survey, "OA");
            Utility.SetAuditEntity(entity, "T_OA_REQUIRE", requireId, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
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
            _survey.CHECKSTATE = state;
            isSubmitFlow = true;
            Save();
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (_survey != null)
                state = _survey.CHECKSTATE;
            if (types == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
            DocDistrbuteClient.DoClose();
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
