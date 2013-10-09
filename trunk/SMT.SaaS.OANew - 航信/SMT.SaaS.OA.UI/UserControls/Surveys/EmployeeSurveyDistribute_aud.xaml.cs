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

using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmployeeSurveyDistribute_aud : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        EmployeeSurveyApp_sel frmD;
        /// <summary>
        /// 方案
        /// </summary>
        private T_OA_REQUIREDISTRIBUTE _survey;
        public T_OA_REQUIREDISTRIBUTE _Survey { get { return _survey; } set { _survey = value; } }

        SmtOACommonOfficeClient DocDistrbuteClient = new SmtOACommonOfficeClient();
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();
        private FormTypes types;
        private bool isSubmitFlow = false;

        private RefreshedTypes saveType;
        /// <summary>
        /// save 要保存的发布对象
        /// </summary>
        ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER> distributeLists = new ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER>();
        /// <summary>
        /// select操作之后, 最新选择的发布对象
        /// </summary>
        private List<ExtOrgObj> issuanceExtOrgObj = new List<ExtOrgObj>();
        #endregion

        #region 构造函数
        public EmployeeSurveyDistribute_aud()
        {
            InitializeComponent();
            _VM.Upd_ESurveyResultCompleted += new EventHandler<Upd_ESurveyResultCompletedEventArgs>(Upd_ESurveyResultCompleted);
            //发布
            DocDistrbuteClient.GetDocDistrbuteInfosCompleted += new EventHandler<GetDocDistrbuteInfosCompletedEventArgs>(DocDistrbuteClient_GetDocDistrbuteInfosCompleted);
        }
        public EmployeeSurveyDistribute_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            this.types = operationType;
            _VM.Upd_ESurveyResultCompleted += new EventHandler<Upd_ESurveyResultCompletedEventArgs>(Upd_ESurveyResultCompleted);
            //发布
            DocDistrbuteClient.GetDocDistrbuteInfosCompleted += new EventHandler<GetDocDistrbuteInfosCompletedEventArgs>(DocDistrbuteClient_GetDocDistrbuteInfosCompleted);
            _VM.Get_ESurveyResultCompleted += new EventHandler<Get_ESurveyResultCompletedEventArgs>(Get_ESurveyResultCompleted);
            _VM.Get_ESurveyResultAsync(SendDocID);
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (types == FormTypes.Browse || types == FormTypes.Audit)
            {
                this.dg.IsEnabled = false;
            }
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
            _VM.Upd_ESurveyResultAsync(_survey, null);
        }
        #endregion

        #region 修改
        void Upd_ESurveyResultCompleted(object sender, Upd_ESurveyResultCompletedEventArgs e)
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
                    Utility.ShowMessageBox("SUCCESSAUDIT", isSubmitFlow, true);
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                }
                else
                {
                    Utility.ShowMessageBox("AUDITFAILURE", isSubmitFlow, false);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region 根据ID获取
        void Get_ESurveyResultCompleted(object sender, Get_ESurveyResultCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                _survey = e.Result;

                DocDistrbuteClient.GetDocDistrbuteInfosAsync(_survey.REQUIREDISTRIBUTEID);
                txtTitle.Text = _survey.DISTRIBUTETITLE;

                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
            }
        }
        #endregion

        #region 调查对象

        //绑定发布
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
        //获取发布对象
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

        #endregion 调查对象

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
