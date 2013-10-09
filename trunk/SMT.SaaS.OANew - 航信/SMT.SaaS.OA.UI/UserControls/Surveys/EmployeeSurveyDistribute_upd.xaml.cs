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

using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmployeeSurveyDistribute_upd : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        EmployeeSurveyApp_sel frmD;
        /// <summary>
        /// 方案
        /// </summary>
        private T_OA_REQUIREDISTRIBUTE _survey;
        public T_OA_REQUIREDISTRIBUTE _Survey { get { return _survey; } set { _survey = value; } }
        public ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER> _osub = new ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER>();

        SmtOACommonOfficeClient DocDistrbuteClient = new SmtOACommonOfficeClient();
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();
        private FormTypes types;
        private bool isSubmitFlow = false;

        private RefreshedTypes saveType;
        /// <summary>
        /// save 要保存的发布对象
        /// </summary>
        ObservableCollection<SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_DISTRIBUTEUSER> distributeLists = new ObservableCollection<SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_DISTRIBUTEUSER>();
        /// <summary>
        /// select操作之后, 最新选择的发布对象
        /// </summary>
        private List<ExtOrgObj> issuanceExtOrgObj = new List<ExtOrgObj>();
        private List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST> vemployeeObj = new List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST>();
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID
        #endregion

        #region 构造函数
        public EmployeeSurveyDistribute_upd(FormTypes type)
        {
            InitializeComponent();
            this.types = type;
            
            _VM.Upd_ESurveyResultCompleted += new EventHandler<Upd_ESurveyResultCompletedEventArgs>(Upd_ESurveyResultCompleted);
            //发布
            DocDistrbuteClient.GetDocDistrbuteInfosByFormIDCompleted += new EventHandler<GetDocDistrbuteInfosByFormIDCompletedEventArgs>(DocDistrbuteClient_GetDocDistrbuteInfosByFormIDCompleted);
            DocDistrbuteClient.GetDocDistrbuteInfosCompleted += new EventHandler<GetDocDistrbuteInfosCompletedEventArgs>(DocDistrbuteClient_GetDocDistrbuteInfosCompleted);
            DocDistrbuteClient.DocDistrbuteBatchAddCompleted += new EventHandler<DocDistrbuteBatchAddCompletedEventArgs>(DocDistrbuteClient_DocDistrbuteBatchAddCompleted);
            personclient.GetEmployeeDetailByIDsCompleted += new EventHandler<GetEmployeeDetailByIDsCompletedEventArgs>(personclient_GetEmployeeDetailByIDsCompleted);
            //DocDistrbuteClient.DocDistrbuteInfoUpdateCompleted += new EventHandler<DocDistrbuteInfoUpdateCompletedEventArgs>(DocDistrbuteInfoUpdateCompleted);
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            DocDistrbuteClient.GetDocDistrbuteInfosByFormIDAsync(_survey.REQUIREDISTRIBUTEID);
            //DocDistrbuteClient.GetDocDistrbuteInfosAsync(_survey.REQUIREDISTRIBUTEID);
            txtTitle.Text = _survey.DISTRIBUTETITLE;

            if (types == FormTypes.Resubmit)//重新提交
            {
                _survey.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
            }
            
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            //InitAudit(_survey.REQUIREDISTRIBUTEID);
        }
        #endregion

        #region 设置 申请其它信息
        private void SetSurvey()
        {
            _Survey.CREATEDATE = System.DateTime.Now;
            _Survey.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            _Survey.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            _Survey.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            _Survey.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            _Survey.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            _Survey.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            _Survey.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            _Survey.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            _Survey.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            _Survey.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        }
        #endregion

        #region IEntityEditor

        public string GetStatus() { return ""; }

        public string GetTitle()
        {
            return Utility.GetResourceStr("EmployeeSurveyDistribute");
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()
        {
            object[,] arr = new object[,]{
             {ToolbarItemDisplayTypes.Image,"3","CHOOSEEMPLOYEESURVEYAPP","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},             
             {ToolbarItemDisplayTypes.Image,"2","CHOOSEDISTRBUTEOBJECT","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},                       
             {ToolbarItemDisplayTypes.Image,"1","SAVEANDCLOSE", "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
             {ToolbarItemDisplayTypes.Image,"0","SAVE","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"}
            };
            return VehicleMgt.GetToolBarItems(ref arr);
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
            switch (actionType)
            {
                case "0":
                    if (!Check()) return;
                    isSubmitFlow = false;
                    saveType = RefreshedTypes.HideProgressBar;
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    Save();
                    break;
                case "1":
                    if (!Check()) return;
                    isSubmitFlow = false;
                    saveType = RefreshedTypes.CloseAndReloadData;
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    Save();
                    break;
                case "2":
                    AddIssuanObj();
                    break;
                case "3":
                    sel_4();
                    break;
            }
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

        #region 验证
        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            //// 发布 暂时无用
            //if (issuanceExtOrgObj.Count == 0)
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "DISTRBUTEOBJECT"));
            //    return false;
            //}
            return true;
        }
        #endregion

       

        #region 保存
        bool _isAdd = true;
        private void Save()
        {
            foreach (var h in issuanceExtOrgObj)
                AddDistributeObjList(h, _survey.REQUIREDISTRIBUTEID);
            _VM.Upd_ESurveyResultAsync(_survey, distributeLists, "Edit");
        }
        #endregion

        #region 修改
        void Upd_ESurveyResultCompleted(object sender, Upd_ESurveyResultCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
            if (e.Result > 0)
            {
                // 修改发布对象接口没有弄好 
                distributeLists.Clear();
                foreach (var h in issuanceExtOrgObj)
                    AddDistributeObjList(h, _survey.REQUIREDISTRIBUTEID);
                //DocDistrbuteClient.DocDistrbuteInfoUpdateByBatchAsync(distributeLists, _survey.REQUIREDISTRIBUTEID);
                if (isSubmitFlow)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);

                if (e.UserState.ToString() == "Edit")
                {
                    Utility.ShowMessageBox("UPDATE", isSubmitFlow, true);
                }
                if (e.UserState.ToString() == "Audit")
                {
                    Utility.ShowMessageBox("SUCCESSAUDIT", isSubmitFlow, true);
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    Utility.ShowMessageBox("SUCCESSSUBMITAUDIT", isSubmitFlow, true);
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                }
                RefreshUI(RefreshedTypes.All);
            }
            else
            {
                if (e.UserState.ToString() == "Edit")
                {
                    Utility.ShowMessageBox("UPDATE", isSubmitFlow, false);//修改失败!
                    RefreshUI(saveType);
                }
                else
                {
                    Utility.ShowMessageBox("AUDITFAILURE", isSubmitFlow, false);//提交审核失败,请重试!
                }
            }
        }
        #endregion

        #region 选择已审核通过的派车单 1
        private void sel_4()
        {
            frmD = new EmployeeSurveyApp_sel();
            EntityBrowser browser = new EntityBrowser(frmD);
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(sel_4_end);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        #endregion

        #region 选择已审核通过的派车单 2
        private void sel_4_end()
        {
            if (frmD._lst != null && frmD._lst.Count > 0)
            {
                txtTitle.Text = frmD._lst[0].APPTITLE;
                _survey.T_OA_REQUIRE = frmD._lst[0];
                _survey.DISTRIBUTETITLE = frmD._lst[0].APPTITLE;
                _survey.CONTENT = frmD._lst[0].CONTENT;
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
            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_DISTRIBUTEUSER distributeTmp = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_DISTRIBUTEUSER();
            distributeTmp.DISTRIBUTEUSERID = Guid.NewGuid().ToString();
            distributeTmp.MODELNAME = "EmployeeSurveyDistribute";
            distributeTmp.FORMID = issuanceID;
            distributeTmp.VIEWTYPE = ((int)GetObjectType(issuanceExtOrgObj)).ToString();
            if (distributeTmp.VIEWTYPE == ((int)IssuanceObjectType.Post).ToString())    //如果是选择岗位，则保存岗位级别
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_POST hr = (SMT.Saas.Tools.OrganizationWS.T_HR_POST)issuanceExtOrgObj.ObjectInstance;
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
                                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmp = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                                tmp.COMPANYID = h.VIEWER;
                                //tmp.CNAME = "";
                                tmp.CNAME = Utility.GetCompanyName(tmp.COMPANYID);
                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department).ToString())
                            {
                                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmp = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
                                tmp.DEPARTMENTID = h.VIEWER;
                                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENTDICTIONARY tmpdict = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENTDICTIONARY();
                                //tmpdict.DEPARTMENTNAME = "";
                                tmpdict.DEPARTMENTNAME = Utility.GetDepartmentName(h.VIEWER);
                                tmp.T_HR_DEPARTMENTDICTIONARY = tmpdict;
                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post).ToString())
                            {
                                SMT.Saas.Tools.OrganizationWS.T_HR_POST tmp = new SMT.Saas.Tools.OrganizationWS.T_HR_POST();
                                tmp.POSTLEVEL = System.Convert.ToDecimal(h.VIEWER);
                                SMT.Saas.Tools.OrganizationWS.T_HR_POSTDICTIONARY tmpdict = new SMT.Saas.Tools.OrganizationWS.T_HR_POSTDICTIONARY();
                                //tmpdict.POSTNAME = "";
                                tmpdict.POSTNAME = Utility.GetPostName(h.VIEWER);
                                tmp.T_HR_POSTDICTIONARY = tmpdict;

                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel).ToString())
                            {
                                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE tmp = new SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE();
                                tmp.EMPLOYEEID = h.VIEWER;
                                tmp.EMPLOYEECNAME = Utility.GetDistrbuteUserName(tmp.EMPLOYEEID, vemployeeObj);
                                obj = tmp;
                                //T_HR_EMPLOYEE tmp = new T_HR_EMPLOYEE();
                                //tmp.EMPLOYEEID = h.VIEWER;
                                //tmp.EMPLOYEECNAME = "";
                                //obj = tmp;
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
        void DocDistrbuteClient_DocDistrbuteBatchAddCompleted(object sender, DocDistrbuteBatchAddCompletedEventArgs e)
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


        #region 代码补充
        /// <summary>
        /// 获取参会人员并填充
        /// </summary>
        private void GetDistrbuteStaff(ObservableCollection<string> staffs)
        {
            personclient.GetEmployeeDetailByIDsAsync(staffs);

        }

        void personclient_GetEmployeeDetailByIDsCompleted(object sender, GetEmployeeDetailByIDsCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    vemployeeObj.Clear();
                    StrStaffList.Clear();//清空员工ID集合 否则会逐条记录添加

                    if (e.Result != null)
                    {
                        vemployeeObj = e.Result.ToList();


                    }
                    DocDistrbuteClient.GetDocDistrbuteInfosAsync(_survey.REQUIREDISTRIBUTEID);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }


        void DocDistrbuteClient_GetDocDistrbuteInfosByFormIDCompleted(object sender, GetDocDistrbuteInfosByFormIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    StrStaffList = e.Result;
                    if (StrStaffList != null) //发布对象有选择单个员工
                    {
                        GetDistrbuteStaff(StrStaffList);
                    }
                    else //发布对象没有选择单个员工
                    {
                        DocDistrbuteClient.GetDocDistrbuteInfosAsync(_survey.REQUIREDISTRIBUTEID);
                    }
                }
            }
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_REQUIREDISTRIBUTE>(_survey, "OA");
            Utility.SetAuditEntity(entity, "T_OA_REQUIREDISTRIBUTE", _survey.REQUIREDISTRIBUTEID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = string.Empty;
            string UserState = string.Empty;
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
            if (_survey.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            _survey.CHECKSTATE = state;
            isSubmitFlow = true;
            _VM.Upd_ESurveyResultAsync(_survey, null, UserState);
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
            DocDistrbuteClient.DoClose();
            _VM.DoClose();
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
