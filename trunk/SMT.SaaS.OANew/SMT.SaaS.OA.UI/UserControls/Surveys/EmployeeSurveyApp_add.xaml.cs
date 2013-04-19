using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmployeeSurveyApp_add : BaseForm, IClient, IEntityEditor, IAudit
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
        private FormTypes types;
        private bool isFlow = false;
        private bool _isAdd = true;

        private RefreshedTypes saveType;
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
        public EmployeeSurveyApp_add(FormTypes type)
        {
            InitializeComponent();
            this.types = type;
            this.Loaded += new RoutedEventHandler(EmployeeSurveyApp_add_Loaded);
            
        }

        void EmployeeSurveyApp_add_Loaded(object sender, RoutedEventArgs e)
        {
            _osub = new ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER>();
            issuanceExtOrgObj = new List<ExtOrgObj>();
            DocDistrbuteClient = new SmtOACommonOfficeClient();
            _VM = new SmtOAPersonOfficeClient();
            distributeLists = new ObservableCollection<SMT.SaaS.OA.UI.SmtOACommonOfficeService.T_OA_DISTRIBUTEUSER>();
            _VM.Add_EsurveyAppCompleted += new EventHandler<Add_EsurveyAppCompletedEventArgs>(Add_EsurveyAppCompleted);
            _VM.Upd_ESurveyAppCompleted += new EventHandler<Upd_ESurveyAppCompletedEventArgs>(Upd_ESurveyAppCompleted);
            //发布
            DocDistrbuteClient.DocDistrbuteBatchAddCompleted += new EventHandler<DocDistrbuteBatchAddCompletedEventArgs>(DocDistrbuteClient_DocDistrbuteBatchAddCompleted);


            //cmbWay.SelectedIndex = 0;//匿名和实名发布
            _survey = new T_OA_REQUIRE();
            _survey.T_OA_REQUIREMASTER = new T_OA_REQUIREMASTER();

            _survey.REQUIREID = Guid.NewGuid().ToString();
            _survey.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            dpStartDate.Text = DateTime.Now.ToShortDateString();
            dpEndDate.Text = DateTime.Now.AddMonths(3).ToShortDateString();

            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            SetSurvey();
            //修改发布接口还没弄好
            // DocDistrbuteClient.DocDistrbuteInfoUpdateCompleted += new EventHandler<DocDistrbuteInfoUpdateCompletedEventArgs>(DocDistrbuteInfoUpdateCompleted);
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
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
            return Utility.GetResourceStr("EmployeeSurveyApp");
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> toolBaritems = new List<ToolbarItem>()
             {
             new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="0",Title=Utility.GetResourceStr("SAVE"),
                 ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"},
            new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="1",Title=Utility.GetResourceStr("SAVEANDCLOSE"),ImageUrl="SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
             new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="2",Title=Utility.GetResourceStr("CHOOSEDISTRBUTEOBJECT"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},
             new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="3",Title=Utility.GetResourceStr("CHOOSEEMPLOYEESURVEY"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"}
             };
            return toolBaritems;
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
                  
                    //isFlow = false;
                  
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    Save();
                    break;
                case "1":
                    if (!Check()) return;
                    //isFlow = false;
                    saveType = RefreshedTypes.CloseAndReloadData;
                    RefreshUI(RefreshedTypes.HideProgressBar);
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
            string StrStartDt = "";   //开始时间
            string EndDt = "";    //结束时间

            if (!string.IsNullOrEmpty(this.dpStartDate.SelectedDate.ToString()))
            {
                StrStartDt = this.dpStartDate.SelectedDate.Value.ToString("d");//开始日期
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "STARTDATE"));
                this.dpStartDate.Focus();
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (!string.IsNullOrEmpty(this.dpEndDate.SelectedDate.ToString()))
            {
                EndDt = this.dpEndDate.SelectedDate.Value.ToString("d");//结束日期
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ENDDATE1"));
                this.dpEndDate.Focus();
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }

            DateTime DtStart = System.Convert.ToDateTime(StrStartDt);
            DateTime DtEnd = System.Convert.ToDateTime(EndDt);

            if (!string.IsNullOrEmpty(this.dpStartDate.Text) && !string.IsNullOrEmpty(this.dpEndDate.Text))
            {
                if (DtStart > DtEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEGREATERTHANENDDATE", "STARTDATE"));
                    this.dpStartDate.Focus();
                    RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
                    return false;
                }
            }

            //if (this.cmbWay.SelectedIndex <= 0)//是否匿名
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEEMPTYSELECT", "IsAnonymous"));
            //    this.cmbWay.Focus();
            //    return false;
            //}

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
            return true;
        }
        #endregion

        #region 保存
        private void Save()
        {
            if (Check())
            {
                _survey.WAY = (bool)ckbOName.IsChecked ? "1" : "0";
                _survey.STARTDATE = DateTime.Parse(dpStartDate.Text);
                _survey.ENDDATE = DateTime.Parse(dpEndDate.Text);
                //_survey.WAY = StrCmbWay.DICTIONARYVALUE.ToString();
                _survey.OPTFLAG = (bool)ckbOptFlag.IsChecked ? "1" : "0";
                RefreshUI(RefreshedTypes.ShowProgressBar);
                if (_isAdd)
                    _VM.Add_EsurveyAppAsync(_survey);
                else
                    _VM.Upd_ESurveyAppAsync(_survey);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "DISTRBUTEOBJECT"));
            }
        }
        #endregion

        #region 新增
        private void Add_EsurveyAppCompleted(object sender, Add_EsurveyAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
            if (e.Result > 0)
            {
                _isAdd = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);

                types = FormTypes.Edit;
                Utility.ShowMessageBox("ADD", isFlow, true);

                // 发布               
                foreach (var h in issuanceExtOrgObj)
                    AddDistributeObjList(h, _survey.REQUIREID);
                DocDistrbuteClient.DocDistrbuteBatchAddAsync(distributeLists);
            }
            else
            {
                Utility.ShowMessageBox("ADD", isFlow, false);
            }
        }
        #endregion

        #region 修改
        void Upd_ESurveyAppCompleted(object sender, Upd_ESurveyAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
            if (e.Result > 0)
            {
                Utility.ShowMessageBox("AUDITSUCCESSED", isFlow, true);
                if (isFlow)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);
                RefreshUI(RefreshedTypes.All);
            }
            else
            {
                Utility.ShowMessageBox("AUDITFAILURE", isFlow, false);
            }
        }
        #endregion

        #region 选择已审核通过的派车单 1
        private void sel_4()
        {
            frmD = new EmployeeSurvey_sel();
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
                txtTitle.Text = frmD._lst[0].RequireMaster.REQUIRETITLE;
                _survey.T_OA_REQUIREMASTER = frmD._lst[0].RequireMaster;
                _survey.APPTITLE = frmD._lst[0].RequireMaster.REQUIRETITLE;
                _survey.CONTENT = frmD._lst[0].RequireMaster.CONTENT;
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
        void DocDistrbuteClient_DocDistrbuteBatchAddCompleted(object sender, DocDistrbuteBatchAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    issuanceExtOrgObj.Clear();
                    if (isFlow)
                    {
                    }
                        saveType = RefreshedTypes.CloseAndReloadData;
                    RefreshUI(saveType);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ADDFAILED"));
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ADDFAILED"));
                RefreshUI(RefreshedTypes.HideProgressBar);
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
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EmployeeSurvey"));
                    if (isFlow)
                        saveType = RefreshedTypes.CloseAndReloadData;
                }
                else
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ADDFAILED"));
            }
            else
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ADDFAILED"));
            RefreshUI(saveType);
        }
        #endregion

        #region BtnDel_Click
        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region dg_LoadingRow
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //T_OA_WELFAREDETAIL tmp = (T_OA_WELFAREDETAIL)e.Row.DataContext;

            ImageButton MyButton_Delbaodao = dg.Columns[3].GetCellContent(e.Row).FindName("BtnDel") as ImageButton;
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            //MyButton_Delbaodao.Tag = tmp;
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_REQUIRE>(_survey, "OA");
            Utility.SetAuditEntity(entity, "T_OA_REQUIRE", _survey.REQUIREID, strXmlObjectSource);
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
            Save();
        }

        public string GetAuditState()
        {
            string state = "0";
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
