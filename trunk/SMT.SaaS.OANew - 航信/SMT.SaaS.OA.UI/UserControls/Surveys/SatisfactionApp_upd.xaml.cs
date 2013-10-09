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
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SatisfactionApp_upd : BaseForm, IClient, IEntityEditor, IAudit
    {
        /// <summary>
        /// 取字典 答案
        /// </summary>
        private PermissionServiceClient permissionClient = new PermissionServiceClient();
        /// <summary>
        /// 方案
        /// </summary>
        private T_OA_SATISFACTIONREQUIRE _survey;
        public T_OA_SATISFACTIONREQUIRE _Survey { get { return _survey; } set { _survey = value; } }
        public ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER> _osub = new ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER>();

        SmtOACommonOfficeClient DocDistrbuteClient = new SmtOACommonOfficeClient();
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();

        private bool isSubmitFlow = false;
        private RefreshedTypes saveType;
        private FormTypes types;
        private bool submitflag = false;  //用来控制是否对发布对象修改
        /// <summary>
        /// save 要保存的发布对象
        /// </summary>
        ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER> distributeLists = new ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER>();
        /// <summary>
        /// select操作之后, 最新选择的发布对象
        /// </summary>
        private List<ExtOrgObj> issuanceExtOrgObj = new List<ExtOrgObj>();

        public SatisfactionApp_upd(FormTypes type)
        {
            InitializeComponent();
            this.types = type;
            _VM.Upd_SSurveyAppCompleted += new EventHandler<Upd_SSurveyAppCompletedEventArgs>(Upd_SSurveyAppCompleted);
            //发布
            DocDistrbuteClient.GetDocDistrbuteInfosCompleted += new EventHandler<GetDocDistrbuteInfosCompletedEventArgs>(DocDistrbuteClient_GetDocDistrbuteInfosCompleted);
            //DocDistrbuteClient.DocDistrbuteInfoUpdateCompleted += new EventHandler<DocDistrbuteInfoUpdateCompletedEventArgs>(DocDistrbuteInfoUpdateCompleted);
            DocDistrbuteClient.DocDistrbuteInfoUpdateByBatchCompleted += new EventHandler<DocDistrbuteInfoUpdateByBatchCompletedEventArgs>(DocDistrbuteClient_DocDistrbuteInfoUpdateByBatchCompleted);
            permissionClient.GetSysDictionaryByFatherIDCompleted += new EventHandler<GetSysDictionaryByFatherIDCompletedEventArgs>(GetSysDictionaryByFatherIDCompleted);
        }


        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            DocDistrbuteClient.GetDocDistrbuteInfosAsync(_survey.SATISFACTIONREQUIREID);

            txtTitle.Text = _survey.SATISFACTIONTITLE;
            dpStartDate.Text = Convert.ToDateTime(_survey.STARTDATE).ToShortDateString();
            dpEndDate.Text = Convert.ToDateTime(_survey.ENDDATE).ToShortDateString();
            cmbWay.SelectedIndex = int.Parse(_survey.WAY);
            ckbOptFlag.IsChecked = _survey.OPTFLAG == "1" ? true : false;

            foreach (T_SYS_DICTIONARY i in cbDepCity.Items)
                if (i.DICTIONARYID != null && i.DICTIONARYID.Equals(_survey.ANSWERGROUPID))
                    cbDepCity.SelectedItem = i;

            if (types == FormTypes.Resubmit)//重新提交
            {
                _survey.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
            }
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            //InitAudit(_survey.SATISFACTIONREQUIREID);
        }
        /// <summary>
        /// 设置 申请其它信息
        /// </summary>
        /// <param name="i"></param>
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

        #region IEntityEditor

        public string GetStatus() { return ""; }

        public string GetTitle()
        {
            return Utility.GetResourceStr("OASatisfactionApp");
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()
        {
            object[,] arr = new object[,]{
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
        //检查
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
            // 发布
            if (issuanceExtOrgObj.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "DISTRBUTEOBJECT"));
                return false;
            }
            if (cbDepCity.SelectedIndex < 1)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "SATISFACTIONANSWERGROUP"));
                return false;
            }

            return true;
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    if (!Check()) return;
                    isSubmitFlow = false;
                    saveType = RefreshedTypes.HideProgressBar;
                    RefreshUI(saveType);
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

        private void Save()
        {
            T_SYS_DICTIONARY StrDepCity = cbDepCity.SelectedItem as T_SYS_DICTIONARY;
            _survey.ANSWERGROUPID = StrDepCity.DICTIONARYID;

            _survey.STARTDATE = DateTime.Parse(dpStartDate.Text);
            _survey.ENDDATE = DateTime.Parse(dpEndDate.Text);
            _survey.WAY = cmbWay.SelectedIndex.ToString();
            _survey.OPTFLAG = (bool)ckbOptFlag.IsChecked ? "1" : "0";
            if (issuanceExtOrgObj.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "DISTRBUTEOBJECT"));
                return;
            }
            //else
            //{
            //    foreach (var h in issuanceExtOrgObj)
            //    {
            //        //是更新还是新增
            //        var entity = _osub.Where(s => s.FORMID == h.ObjectID).FirstOrDefault();
            //        if (entity != null)
            //        {
            //            entity.UPDATEDATE = DateTime.Now;
            //            entity.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            //            entity.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            //            //entity.EntityKey = null;
            //            distributeLists.Add(entity);
            //        }
            //        else
            //        {
            //            AddDistributeObjList(h, _survey.SATISFACTIONREQUIREID);
            //        }
            //    }
            //}
            //_VM.Upd_SSurveyAppAsync(_survey, distributeLists,submitflag);
            _VM.Upd_SSurveyAppAsync(_survey);
        }

        void Upd_SSurveyAppCompleted(object sender, Upd_SSurveyAppCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                if (e.Result > 0)
                {
                    // 修改发布对象接口没有弄好               
                    //foreach (var h in issuanceExtOrgObj)
                    //    AddDistributeObjList(h, _survey.REQUIREID);
                    // DocDistrbuteClient.DocDistrbuteInfoUpdateAsync(distributeLists);
                    if (isSubmitFlow)
                        saveType = RefreshedTypes.CloseAndReloadData;
                    foreach (var h in issuanceExtOrgObj)
                    {
                        //是更新还是新增
                        var entity = distributeLists.Where(s => s.FORMID == h.ObjectID).FirstOrDefault();
                        if (entity != null)
                        {
                            entity.UPDATEDATE = DateTime.Now;
                            entity.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                            entity.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            distributeLists.Add(entity);
                        }
                        else
                        {
                            AddDistributeObjList(h, _survey.SATISFACTIONREQUIREID);
                        }
                    }
                    DocDistrbuteClient.DocDistrbuteInfoUpdateByBatchAsync(distributeLists, _survey.SATISFACTIONREQUIREID);
                    //DocDistrbuteClient.DocDistrbuteInfoUpdateAsync(distributeLists);

                }
                else
                {
                    Utility.ShowMessageBox("UPDATE", isSubmitFlow, false);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

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
        /// <summary>
        /// 添加发布对象
        /// </summary>
        private void AddIssuanObj()
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    submitflag = false;//修改了发布对象 在业务层需要更新
                    issuanceExtOrgObj = ent;
                    BindData();
                }
            };
            lookup.MultiSelected = true;
            lookup.SelectSameGradeOnly = true;
            lookup.Show();
        }
        /// <summary>
        ///保存 发布对象
        /// </summary>
        /// <param name="issuanceExtOrgObj"></param>
        /// <param name="issuanceID"></param>
        private void AddDistributeObjList(SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj issuanceExtOrgObj, string issuanceID)
        {
            SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER distributeTmp = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER();
            distributeTmp.DISTRIBUTEUSERID = Guid.NewGuid().ToString();
            distributeTmp.MODELNAME = "OASatisfactionApp";
            distributeTmp.FORMID = issuanceID;
            distributeTmp.VIEWTYPE = ((int)GetObjectType(issuanceExtOrgObj)).ToString();
            if (distributeTmp.VIEWTYPE == ((int)IssuanceObjectType.Post).ToString())    //如果是选择岗位，则保存岗位级别
            {
                //T_HR_POST hr = (T_HR_POST)issuanceExtOrgObj.ObjectInstance;
                //if (!string.IsNullOrEmpty(hr.POSTLEVEL.ToString()))
                //    distributeTmp.VIEWER = hr.POSTLEVEL.ToString();
                //else
                //    distributeTmp.VIEWER = hr.T_HR_POSTDICTIONARY.POSTLEVEL.ToString();   暂时不考虑部门中的岗位
            }
            else
            {
                //distributeTmp.VIEWER = issuanceExtOrgObj.ObjectID;
                if (!string.IsNullOrEmpty(issuanceExtOrgObj.ObjectID))
                {
                    distributeTmp.VIEWER = issuanceExtOrgObj.ObjectID;
                }
                else
                {
                    distributeTmp.VIEWER = Utility.ReturnIssuranceObjID(issuanceExtOrgObj);

                }
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
                                tmp.CNAME = Utility.GetCompanyName(h.VIEWER);
                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department).ToString())
                            {
                                T_HR_DEPARTMENT tmp = new T_HR_DEPARTMENT();
                                tmp.DEPARTMENTID = h.VIEWER;
                                T_HR_DEPARTMENTDICTIONARY tmpdict = new T_HR_DEPARTMENTDICTIONARY();
                                tmpdict.DEPARTMENTNAME = Utility.GetDepartmentName(h.VIEWER);
                                tmp.T_HR_DEPARTMENTDICTIONARY = tmpdict;
                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post).ToString())
                            {
                                T_HR_POST tmp = new T_HR_POST();
                                tmp.POSTLEVEL = System.Convert.ToDecimal(h.VIEWER);
                                T_HR_POSTDICTIONARY tmpdict = new T_HR_POSTDICTIONARY();
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
        //发布完成事件
        void DocDistrbuteClient_DocDistrbuteInfoUpdateByBatchCompleted(object sender, DocDistrbuteInfoUpdateByBatchCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    issuanceExtOrgObj.Clear();
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESED", "EmployeeSurveyApp"));
                    if (isSubmitFlow)
                        saveType = RefreshedTypes.CloseAndReloadData;
                }
                else
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("UPDATEISSUEFAILED", "EmployeeSurveyApp"));
            }
            else
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("UPDATEISSUEFAILED", "EmployeeSurveyApp"));
            RefreshUI(saveType);


        }
        #endregion 调查对象

        //答案组
        private void cbDepCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY sys = ((ComboBox)sender).SelectedItem as T_SYS_DICTIONARY;
            permissionClient.GetSysDictionaryByFatherIDAsync(sys.DICTIONARYID);
        }
        //答案
        void GetSysDictionaryByFatherIDCompleted(object sender, GetSysDictionaryByFatherIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                ObservableCollection<T_SYS_DICTIONARY> o = e.Result;
                cbAnswer.ItemsSource = o;
                cbAnswer.DisplayMemberPath = "DICTIONARYNAME";
                cbAnswer.SelectedIndex = 0;
            }
        }

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_SATISFACTIONREQUIRE>(_survey, "OA");
            Utility.SetAuditEntity(entity, "T_OA_SATISFACTIONREQUIRE", _survey.SATISFACTIONREQUIREID, strXmlObjectSource);
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
            permissionClient.DoClose();
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
