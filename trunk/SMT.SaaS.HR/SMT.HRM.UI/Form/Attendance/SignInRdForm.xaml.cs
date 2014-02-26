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

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;
using SMT.SaaS.FrameworkUI.OrganizationControl;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class SignInRdForm : BaseForm, IEntityEditor, IAudit
    {
        #region 全局变量
        public FormTypes FormType { set; get; }

        public string SignInID { get; set; }

        public T_HR_EMPLOYEESIGNINRECORD SignInRecord { get; set; }

        public ObservableCollection<T_HR_EMPLOYEEABNORMRECORD> AbnormRecordList { get; set; }
        public ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL> SignInDetailList { get; set; }
        public ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL> tempSignInDetailList { get; set; }
        public ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL> tempList = new ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL>();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool recordsign = true;
        AttendanceServiceClient clientAtt;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient perClient;
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭

        #endregion

        #region 初始化
        //Modified by:Sam
        //Date:2011-10-9
        //For:增加一个无参数的构造函数来实现待办任务新建单据
        public SignInRdForm()
        {
            if (Application.Current.Resources["CustomDateConverter"] == null)
            {
                Application.Current.Resources.Add("CustomDateConverter", new CustomDateConverter());
            }
            InitializeComponent();
            FormType = FormTypes.New;
            SignInID = string.Empty;
            this.Loaded += new RoutedEventHandler(SignInRdForm_Loaded);
        }

        public SignInRdForm(FormTypes type, string strSignInID)
        {
            InitializeComponent();
            FormType = type;
            SignInID = strSignInID;
            this.Loaded += new RoutedEventHandler(SignInRdForm_Loaded);
        }

        void SignInRdForm_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            perClient = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
            RegisterEvents();
            InitParas();
            tempSignInDetailList = new ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL>();
        }

        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEESIGNINRECORD");
        }

        public string GetStatus()
        {
            string strTemp = string.Empty;
            if (!string.IsNullOrEmpty(SignInID))
                strTemp = "编辑中";

            return strTemp;
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    closeFormFlag = true;
                    Save();
                    //Cancel();
                    break;
                case "Delete":
                    //删除异常签卡申请
                    delete(SignInID);
                    break;
            }
        }
        public void delete(string strid)
        {
            string Result = "";
            ObservableCollection<string> delIDs = new ObservableCollection<string>();
            delIDs.Add(strid);
            ComfirmWindow delComfirm = new ComfirmWindow();
            delComfirm.OnSelectionBoxClosed += (obj, result) =>
            {
                clientAtt.EmployeeSigninRecordDeleteAsync(delIDs);
            };
            delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), "确定要删除异常签卡记录？", ComfirmWindow.titlename, Result);
        }


        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_EMPLOYEESIGNINRECORD Info)
        {
            //异常类型
            var abnormcategoryDicts = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "ABNORMCATEGORY");
            Dictionary<string, string> abnormcategory = new Dictionary<string, string>();
            foreach (var abnorm in abnormcategoryDicts)
            {
                abnormcategory.Add(abnorm.DICTIONARYVALUE.ToString(), abnorm.DICTIONARYNAME);
            }

            //异常原因
            var reasoncategoryDicts = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "REASONCATEGORY");
            Dictionary<string, string> reson = new Dictionary<string, string>();
            foreach (var reason in reasoncategoryDicts)
            {
                reson.Add(reason.DICTIONARYVALUE.ToString(), reason.DICTIONARYNAME);
            }

            //异常时间段
            var dperiodDicts = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "ATTENDPERIOD");
            Dictionary<string, string> dperiod = new Dictionary<string, string>();
            foreach (var item in dperiodDicts)
            {
                dperiod.Add(item.DICTIONARYVALUE.ToString(), item.DICTIONARYNAME);
            }

            //审核状态
            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;

            //岗位级别
            decimal? postlevelValue = Convert.ToDecimal(tbPostLevel.Text.Trim());
            string postLevelName = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "POSTLEVEL" && s.DICTIONARYVALUE == postlevelValue).FirstOrDefault();
            postLevelName = postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME;

            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEESIGNINRECORD", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEESIGNINRECORD", "POSTLEVEL", tbPostLevel.Text, postLevelName));
            AutoList.Add(basedata("T_HR_EMPLOYEESIGNINRECORD", "OWNERCOMPANYID", Info.OWNERCOMPANYID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEESIGNINRECORD", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEESIGNINRECORD", "OWNERPOSTID", Info.OWNERPOSTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName));


            AutoList.Add(basedata("T_HR_EMPLOYEESIGNINRECORD", "EMPLOYEEFULLNAME", Info.EMPLOYEENAME, lkEmployeeName.TxtLookUp.Text));
            if (SignInDetailList.Count > 0)
            {
                //签卡类型
                AutoList.Add(basedata("T_HR_EMPLOYEESIGNINRECORD", "SIGNINCATEGORY", SignInDetailList[0].REASONCATEGORY, reson[SignInDetailList[0].REASONCATEGORY.ToString()]));
            }
            foreach (var v in SignInDetailList)
            {
                AutoList.Add(basedataForChild("T_HR_EMPLOYEESIGNINDETAIL", "ABNORMCATEGORY", v.ABNORMCATEGORY, abnormcategory[v.ABNORMCATEGORY.ToString()], v.SIGNINDETAILID));
                AutoList.Add(basedataForChild("T_HR_EMPLOYEESIGNINDETAIL", "REASONCATEGORY", v.REASONCATEGORY, reson[v.REASONCATEGORY.ToString()], v.SIGNINDETAILID));
                AutoList.Add(basedataForChild("T_HR_EMPLOYEESIGNINDETAIL", "ATTENDPERIOD", v.ATTENDPERIOD, dperiod[v.ATTENDPERIOD.ToString()], v.SIGNINDETAILID));
                AutoList.Add(basedataForChild("T_HR_EMPLOYEESIGNINDETAIL", "SIGNINID", Info.SIGNINID, "", v.SIGNINDETAILID));
                if (v.ABNORMALDATE!=null)
                {
                    //日期格式tostring的时候程序会根据本机电脑设置的事情格式去生成，所以这里要加和格式设置（很奇怪吧，很多情况都会这样，例如我把我电脑日期设置后面加上中文"哈哈"，那么日期格式tostring的时候就会变成"2013-8-1 哈哈"）
                    AutoList.Add(basedataForChild("T_HR_EMPLOYEESIGNINDETAIL", "ABNORMALDATE", v.ABNORMALDATE.Value.ToString("yyyy-MM-dd"), v.ABNORMALDATE.Value.ToString("yyyy-MM-dd"), v.SIGNINDETAILID));
                }
               
            }
            string a = mx.TableToXml(Info, SignInDetailList, StrSource, AutoList);

            return a;
        }

        private AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }

        private AutoDictionary basedataForChild(string TableName, string Name, string Value, string Text, string keyValue)
        {
            string[] strlist = new string[5];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            strlist[4] = keyValue;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryChiledList(strlist);//这里需要传递5个参数过去，keyvalue就是该表的主键ID
            return ad;
        }
        #endregion

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("POSTLEVEL", tbPostLevel.Text);
            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;

            //strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEESIGNINRECORD>(SignInRecord, para, "HR");
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, SignInRecord);
            Utility.SetAuditEntity(entity, "T_HR_EMPLOYEESIGNINRECORD", SignInID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
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

            if (SignInRecord.CHECKSTATE == Utility.GetCheckState(CheckStates.UnSubmit))
            {
                SignInRecord.CHECKSTATE = state;
                ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL> entSubmits = dgSignInDetailList.ItemsSource as ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL>;
                if (entSubmits.Count == 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEESIGNINRECORD"), Utility.GetResourceStr("NORECORDSUBMIT", "EMPLOYEESIGNINRECORD"));
                    return;
                }
                clientAtt.EmployeeSigninRecordUpdateAsync(SignInRecord, entSubmits);
            }
            else
            {
                clientAtt.EmployeeSigninRecordAuditAsync(SignInID, state);
            }

        }

        public string GetAuditState()
        {
            string state = "-1";
            if (SignInRecord != null)
                state = SignInRecord.CHECKSTATE;
            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 设置窗口可显示的按钮
        /// </summary>
        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                ToolbarItems.Add(ToolBarItems.Delete);
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEESIGNINRECORD", SignInRecord.OWNERID,
                    SignInRecord.OWNERPOSTID, SignInRecord.OWNERDEPARTMENTID, SignInRecord.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetEmployeeSigninRecordByIDCompleted += new EventHandler<GetEmployeeSigninRecordByIDCompletedEventArgs>(clientAtt_GetEmployeeSigninRecordByIDCompleted);
            clientAtt.GetAbnormRecordByEmployeeIDCompleted += new EventHandler<GetAbnormRecordByEmployeeIDCompletedEventArgs>(clientAtt_GetAbnormRecordByEmployeeIDCompleted);
            clientAtt.GetEmployeeSignInDetailBySigninIDCompleted += new EventHandler<GetEmployeeSignInDetailBySigninIDCompletedEventArgs>(clientAtt_GetEmployeeSignInDetailBySigninIDCompleted);

            clientAtt.EmployeeSignInRecordAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_EmployeeSignInRecordAddCompleted);
            clientAtt.EmployeeSigninRecordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_EmployeeSigninRecordUpdateCompleted);
            clientAtt.EmployeeSigninRecordAuditCompleted += new EventHandler<EmployeeSigninRecordAuditCompletedEventArgs>(clientAtt_EmployeeSigninRecordAuditCompleted);

            //perClient.GetEmployeeDetailByIDCompleted += new EventHandler<SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs>(perClient_GetEmployeeDetailByIDCompleted);
           // perClient.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(perClient_GetEmployeePostBriefByEmployeeIDCompleted);
           // psClient.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(psClient_GetEmployeePostBriefByEmployeeIDCompleted);
            perClient.GetEmpOrgInfoByIDCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmpOrgInfoByIDCompletedEventArgs>(perClient_GetEmpOrgInfoByIDCompleted);
            clientAtt.EmployeeSigninRecordDeleteCompleted += new EventHandler<EmployeeSigninRecordDeleteCompletedEventArgs>(clientAtt_EmployeeSigninRecordDeleteCompleted);
        }

        void clientAtt_EmployeeSigninRecordDeleteCompleted(object sender, EmployeeSigninRecordDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEESIGNINRECORD"));
                closeForm();
            }
            FormType = FormTypes.Browse;
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void closeForm()
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }



        /// <summary>
        /// 页面数据初始化
        /// </summary>
        private void InitParas()
        {
            if (FormType == FormTypes.New)
            {
                SignInRecord = new T_HR_EMPLOYEESIGNINRECORD();
                SignInRecord.SIGNINID = Guid.NewGuid().ToString();
                SignInRecord.SIGNINTIME = DateTime.Now;
                SignInRecord.SIGNINCATEGORY = string.Empty;
                SignInRecord.EMPLOYEEID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SignInRecord.EMPLOYEECODE = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeCode;
                SignInRecord.EMPLOYEENAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SignInRecord.CHECKSTATE = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();

                //权限控制
                SignInRecord.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                SignInRecord.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                SignInRecord.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                SignInRecord.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;


                //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
                SignInRecord.CREATEDATE = DateTime.Now;
                SignInRecord.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SignInRecord.UPDATEDATE = System.DateTime.Now;
                SignInRecord.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                SignInRecord.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                SignInRecord.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                SignInRecord.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;


                //赋初始值
                //SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                //lkEmpName.
                //tbEmpName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                tbOrgName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName + " - " + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + " - " + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
                tbPostLevel.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString(); ;
                if (!string.IsNullOrWhiteSpace(tbOrgName.Text))
                {
                    //tbEmpName.Text = tbEmpName.Text + "-" + tbOrgName.Text;
                    lkEmployeeName.TxtLookUp.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "-" + tbOrgName.Text; ;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                clientAtt.GetAbnormRecordByEmployeeIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                //psClient.GetEmployeePostBriefByEmployeeIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

                this.DataContext = SignInRecord;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                clientAtt.GetEmployeeSigninRecordByIDAsync(SignInID);

                if (FormType == FormTypes.Browse)
                {
                    dgSignInDetailList.IsReadOnly = true;
                    txtRemark.IsEnabled = false;
                }
            }
            #region 工具栏初始化
            ToolBar.txtOtherName.Visibility = Visibility.Collapsed;
            ToolBar.txtOtherName.Text = string.Empty;
            ToolBar.btnImport.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Visible;
            ToolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Collapsed;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            ToolBar.retRefresh.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            #endregion
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSignInDetailList.SelectedItems.Count > 0)
            {
                try
                {
                    string Result = "";
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        if (recordsign)
                        {
                            foreach (var sign in SignInDetailList)
                            {
                                tempList.Add(sign);
                            }
                            recordsign = false;
                        }
                        List<string> idlist = new List<string>();
                        foreach (T_HR_EMPLOYEESIGNINDETAIL detail in dgSignInDetailList.SelectedItems)
                        {
                            idlist.Add(detail.SIGNINDETAILID);
                        }
                        foreach (string id in idlist)
                        {
                            tempList.Remove(tempList.Where(x => x.SIGNINDETAILID == id).FirstOrDefault());
                        }
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEESIGNINRECORD"), Utility.GetResourceStr("成功删除了签卡记录"));
                        dgSignInDetailList.ItemsSource = tempList;
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
                catch { }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEESIGNINRECORD"), Utility.GetResourceStr("请选择一条记录"));
            }
        }

        private void deleteRecord()
        {
            if (dgSignInDetailList.SelectedItems.Count > 0)
            {
                try
                {
                    if (recordsign)
                    {
                        foreach (var sign in SignInDetailList)
                        {
                            tempList.Add(sign);
                        }
                        recordsign = false;
                    }

                    //Modified by:Sam   
                    //Date:2011-08-18
                    //For: 原本删除的时候先取实体的ID，再根据ID过滤实体，再删除实体
                    //     增加删除SignInDetailList变量，这里是导致如果删除一条明细，但是传到手机版还是没有改过的原因
                    //string id = (dgSignInDetailList.SelectedItems[0] as T_HR_EMPLOYEESIGNINDETAIL).SIGNINDETAILID;
                    //tempList.Remove(tempList.Where(x => x.SIGNINDETAILID == id).FirstOrDefault());
                    tempList.Remove(dgSignInDetailList.SelectedItems[0] as T_HR_EMPLOYEESIGNINDETAIL);
                    SignInDetailList.Remove(dgSignInDetailList.SelectedItems[0] as T_HR_EMPLOYEESIGNINDETAIL);
                    dgSignInDetailList.ItemsSource = tempList;
                }
                catch { }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEESIGNINRECORD"), Utility.GetResourceStr("请选择一条记录"));
            }
        }


        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            bool flag = false;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (dgSignInDetailList.ItemsSource == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEESIGNINRECORD"), Utility.GetResourceStr("NORECORDSUBMIT", "EMPLOYEESIGNINRECORD"));
                return false;
            }

            ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL> entSubmits = dgSignInDetailList.ItemsSource as ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL>;
            if (entSubmits.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEESIGNINRECORD"), Utility.GetResourceStr("NORECORDSUBMIT", "EMPLOYEESIGNINRECORD"));
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }

            if (FormType == FormTypes.New)
            {
                SignInID = SignInRecord.SIGNINID;
                clientAtt.EmployeeSignInRecordAddAsync(SignInRecord, entSubmits);
            }
            else if (FormType == FormTypes.Edit || FormType == FormTypes.Audit || FormType == FormTypes.Resubmit)
            {
                clientAtt.EmployeeSigninRecordUpdateAsync(SignInRecord, entSubmits);
            }


            return flag;
        }

        /// <summary>
        /// 保存并关闭
        /// </summary>
        private void Cancel()
        {
            bool flag = false;
            flag = Save();
            if (!flag)
            {
                return;
            }

            closeFormFlag = true;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void CloseForm()
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        #endregion

        #region 事件
        /// <summary>
        /// 获取签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetEmployeeSigninRecordByIDCompleted(object sender, GetEmployeeSigninRecordByIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    SignInRecord = e.Result;

                    if (FormType == FormTypes.Resubmit)
                    {
                        SignInRecord.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                    }

                    this.DataContext = SignInRecord;

                    string strEmployeeId = string.Empty;

                    if (SignInRecord != null)
                    {
                        strEmployeeId = SignInRecord.EMPLOYEEID;
                    }
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    perClient.GetEmpOrgInfoByIDAsync(SignInRecord.EMPLOYEEID, SignInRecord.OWNERPOSTID, SignInRecord.OWNERDEPARTMENTID, SignInRecord.OWNERCOMPANYID);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        /// <summary>
        ///  获取员工个人信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void perClient_GetEmpOrgInfoByIDCompleted(object sender, Saas.Tools.PersonnelWS.GetEmpOrgInfoByIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW ent = e.Result;
                    if (ent == null)
                    {
                        return;
                    }

                    tbOrgName.Text = ent.POSTNAME + " - " + ent.DEPARTMENTNAME + " - " + ent.COMPANYNAME;
                    SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW employeeView = e.Result;
                    string strOrgName = employeeView.POSTNAME + " - " + employeeView.DEPARTMENTNAME + " - " + employeeView.COMPANYNAME;
                    if (!string.IsNullOrWhiteSpace(strOrgName))
                    {
                        strOrgName = ent.EMPLOYEECNAME + " - " + strOrgName;
                    }
                    lkEmployeeName.TxtLookUp.Text = strOrgName;
                    tbPostLevel.Text = ent.POSTLEVEL.ToString();
                    string strSignInId = string.Empty;
                    if (SignInRecord != null)
                    {
                        strSignInId = SignInRecord.SIGNINID;
                    }
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    clientAtt.GetEmployeeSignInDetailBySigninIDAsync(strSignInId);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message + ex.Message));
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }

        }
       
        /// <summary>
        /// 根据员工ID，获取考勤异常记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAbnormRecordByEmployeeIDCompleted(object sender, GetAbnormRecordByEmployeeIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    AbnormRecordList = e.Result;

                    //AbnormRecordList = AbnormRecordList.Where(m => m.SINGINSTATE != "1").ToList();

                    if (AbnormRecordList == null)
                    {
                        MessageBox.Show("未查询到异常考勤");
                        return;
                    }

                    if (AbnormRecordList.Count() == 0)
                    {
                        //return;
                    }

                    SignInDetailList = MakeSignInDetailByAbnormRecord(AbnormRecordList);
                    dgSignInDetailList.ItemsSource = SignInDetailList;

                    //string strLoginUserId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    //if (SignInRecord.EMPLOYEEID != strLoginUserId || SignInRecord.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    //{
                    //    dgSignInDetailList.IsEnabled = false;
                    //    txtRemark.IsEnabled = false;
                    //}
                    //if (SignInRecord.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    //{
                    //    dgSignInDetailList.IsEnabled = false;
                    //    txtRemark.IsEnabled = false;
                    //}
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        /// <summary>
        /// 重构签卡记录子表
        /// </summary>
        /// <param name="entAbnormRecordList"></param>
        /// <returns></returns>
        private ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL> MakeSignInDetailByAbnormRecord(ObservableCollection<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecordList)
        {
            ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL> entSignInDetails = new ObservableCollection<T_HR_EMPLOYEESIGNINDETAIL>();

            foreach (T_HR_EMPLOYEEABNORMRECORD item in entAbnormRecordList)
            {
                T_HR_EMPLOYEESIGNINDETAIL entTemp = new T_HR_EMPLOYEESIGNINDETAIL();
                entTemp.SIGNINDETAILID = System.Guid.NewGuid().ToString().ToUpper();
                entTemp.T_HR_EMPLOYEESIGNINRECORD = SignInRecord;
                entTemp.T_HR_EMPLOYEEABNORMRECORD = item;

                entTemp.ABNORMALDATE = item.ABNORMALDATE;
                entTemp.ABNORMCATEGORY = item.ABNORMCATEGORY;
                entTemp.ATTENDPERIOD = item.ATTENDPERIOD;
                entTemp.ABNORMALTIME = item.ABNORMALTIME;
                entTemp.REASONCATEGORY = (Convert.ToInt32(AbnormReasonCategory.DrainPunch) + 1).ToString();
                entTemp.DETAILREASON = string.Empty;
                entTemp.REMARK = string.Empty;

                //权限控制
                entTemp.OWNERCOMPANYID = item.OWNERCOMPANYID;
                entTemp.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                entTemp.OWNERPOSTID = item.OWNERPOSTID;
                entTemp.OWNERID = item.OWNERID;

                //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
                entTemp.CREATEDATE = DateTime.Now;
                entTemp.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                entTemp.UPDATEDATE = System.DateTime.Now;
                entTemp.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                entTemp.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                entTemp.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                entTemp.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                entSignInDetails.Add(entTemp);
            }

            return entSignInDetails;
        }

        /// <summary>
        /// 根据签卡表主键索引，获取考勤异常记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetEmployeeSignInDetailBySigninIDCompleted(object sender, GetEmployeeSignInDetailBySigninIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    SignInDetailList = e.Result;
                    dgSignInDetailList.ItemsSource = SignInDetailList;

                    string strLoginUserId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    if (SignInRecord.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())//SignInRecord.EMPLOYEEID != strLoginUserId || 
                    {
                        dgSignInDetailList.IsEnabled = false;
                        txtRemark.IsEnabled = false;
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }

                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        /// <summary>
        /// 新增签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_EmployeeSignInRecordAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", ""));

                if (closeFormFlag)
                {
                    CloseForm();
                    return;
                }

                FormType = FormTypes.Edit;
                SignInID = SignInRecord.SIGNINID;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                //添加删除按钮
                ToolbarItems = Utility.CreateFormEditButton();
                ToolbarItems.Add(ToolBarItems.Delete);
                RefreshUI(RefreshedTypes.AuditInfo);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 更新签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_EmployeeSigninRecordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", ""));

                if (closeFormFlag)
                {
                    CloseForm();
                    return;
                }

                FormType = FormTypes.Edit;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                RefreshUI(RefreshedTypes.AuditInfo);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 审核签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_EmployeeSigninRecordAuditCompleted(object sender, EmployeeSigninRecordAuditCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 删除签卡明细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_del_Click(object sender, RoutedEventArgs e)
        {
            deleteRecord();
        }


        /// <summary>
        /// 选择员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LookUp_FindClick(object sender, EventArgs e)
        {
            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;//岗位

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    string deptName = dept.ObjectName;//部门


                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string corpName = corp.CNAME;//公司

                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE emp = userInfo.ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;


                    string StrEmployee = userInfo.ObjectName + "-" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME;
                    lkEmployeeName.TxtLookUp.Text = StrEmployee;
                    lkEmployeeName.DataContext = emp;
                    ToolTipService.SetToolTip(lkEmployeeName.TxtLookUp, StrEmployee);

                    if (ent != null)
                    {
                        RefreshUI(RefreshedTypes.ShowProgressBar);
                        clientAtt.GetAbnormRecordByEmployeeIDAsync(emp.EMPLOYEEID);
                    }
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }



        #endregion
    }
}

