/*
 * 文件名：AttSolRdForm.xaml.cs
 * 作  用：考勤方案设置表单，实现如下功能：新增，编辑，审核
 * 创建人：吴鹏
 * 创建时间：2010年1月19日, 16:31:45
 * 修改人：
 * 修改时间：
 */

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
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttSolRdForm : BaseForm, IEntityEditor, IAudit
    {
        #region 全局变量
        public FormTypes FormType { get; set; }

        public string AttendanceSolutionID { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private T_HR_ATTENDANCESOLUTION entAttendanceSolution;// = new T_HR_ATTENDANCESOLUTION();
        private ObservableCollection<T_HR_ATTENDANCESOLUTIONDEDUCT> entAttendanceSolutionDeducts = new ObservableCollection<T_HR_ATTENDANCESOLUTIONDEDUCT>();
        private ObservableCollection<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves = new ObservableCollection<T_HR_ATTENDFREELEAVE>();
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭

        #endregion

        #region 初始化

        public AttSolRdForm(FormTypes ftType, string strAttendanceSolutionID)
        {
            InitializeComponent();
            AttendanceSolutionID = strAttendanceSolutionID;
            FormType = ftType;
            RegEvents();
            InitParas();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegEvents()
        {
            clientAtt.GetAttendanceSolutionByIDCompleted += new EventHandler<GetAttendanceSolutionByIDCompletedEventArgs>(clientAtt_GetAttendanceSolutionByIDCompleted);
            clientAtt.AddAttendanceSolutionAndCreateRelationCompleted += new EventHandler<AddAttendanceSolutionAndCreateRelationCompletedEventArgs>(clientAtt_AddAttendanceSolutionAndCreateRelationCompleted);
            clientAtt.ModifyAttendanceSolutionAndChangeRelationCompleted += new EventHandler<ModifyAttendanceSolutionAndChangeRelationCompletedEventArgs>(clientAtt_ModifyAttendanceSolutionAndChangeRelationCompleted);
            //clientAtt.ModifyAttendanceSolutionCompleted += new EventHandler<ModifyAttendanceSolutionCompletedEventArgs>(clientAtt_ModifyAttendanceSolutionCompleted);
            clientAtt.AuditAttSolCompleted += new EventHandler<AuditAttSolCompletedEventArgs>(clientAtt_AuditAttSolCompleted);
            clientAtt.CheckAttSolIsExistsAsignRdCompleted += new EventHandler<CheckAttSolIsExistsAsignRdCompletedEventArgs>(clientAtt_CheckAttSolIsExistsAsignRdCompleted);
            //添加删除考勤方案事件
            clientAtt.RemoveAttendanceSolutionCompleted += new EventHandler<RemoveAttendanceSolutionCompletedEventArgs>(clientAtt_RemoveAttendanceSolutionCompleted);
        }
        //删除考勤方案
        void clientAtt_RemoveAttendanceSolutionCompleted(object sender, RemoveAttendanceSolutionCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTENDANCESOLUTION")));
                FormType = FormTypes.Browse;
                RefreshUI(RefreshedTypes.All);
                CloseForm();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                
            }
            //FormType = FormTypes.Browse;
            //RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 加载分Tab 的Form数据
        /// </summary>
        private void InitParas()
        {
            InitBasicParas(FormType, AttendanceSolutionID);
            ucOTReward.InitParas(FormType, AttendanceSolutionID);
            ucDeduct.InitParas(FormType, AttendanceSolutionID);
            ucLeave.InitParas(FormType, AttendanceSolutionID);
            ucDef.InitParas(FormType, AttendanceSolutionID);
        }

        /// <summary>
        /// 页面基本信息初始化
        /// </summary>
        void InitBasicParas(FormTypes FormType, string strAttendanceSolID)
        {
            if (FormType == FormTypes.New)
            {
                InitBasicForm();
                SetToolBar();
            }
            else
            {
                LoadBasicData(strAttendanceSolID);

                if (FormType == FormTypes.Browse)
                {
                    UnEnableFormControl();
                }
            }

            IsEnabledWorkDays(cbxkWorkDayType);
        }

        /// <summary>
        /// 禁用表单控件
        /// </summary>
        private void UnEnableFormControl()
        {
            txtAttSolName.IsEnabled = false;
            cbxkAttendanceType.IsEnabled = false;
            cbxkCardType.IsEnabled = false;
            nudWorkMode.IsEnabled = false;
            nudWorkTime.IsEnabled = false;
            cbxkWorkDayType.IsEnabled = false;
            nudWorkDays.IsEnabled = false;
            nudSettlementDate.IsEnabled = false;
            nudAlarmDate.IsEnabled = false;
            cbIsCurrentMonth.IsEnabled = false;
            txtAttSolRemark.IsEnabled = false;
        }

        /// <summary>
        /// 非编辑状态，定义初始化数据
        /// </summary>
        private void InitBasicForm()
        {
            entAttendanceSolution = new T_HR_ATTENDANCESOLUTION();
            entAttendanceSolution.ATTENDANCESOLUTIONID = System.Guid.NewGuid().ToString().ToUpper();

            //权限控制
            entAttendanceSolution.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entAttendanceSolution.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entAttendanceSolution.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entAttendanceSolution.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;


            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            entAttendanceSolution.CREATEDATE = DateTime.Now;
            entAttendanceSolution.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entAttendanceSolution.UPDATEDATE = System.DateTime.Now;
            entAttendanceSolution.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据
            entAttendanceSolution.ATTENDANCETYPE = "0";
            entAttendanceSolution.CARDTYPE = "0";
            entAttendanceSolution.WORKDAYTYPE = "0";
            entAttendanceSolution.ISCURRENTMONTH = "1";
            cbIsCurrentMonth.IsChecked = false;

            //加班设置
            entAttendanceSolution.OVERTIMEVALID = "0";
            entAttendanceSolution.OVERTIMEPAYTYPE = "0";
            entAttendanceSolution.OVERTIMECHECK = "0";

            //考勤异常设置
            entAttendanceSolution.ALLOWLOSTCARDTIMES = 0;
            entAttendanceSolution.ALLOWLATEMAXMINUTE = 0;
            entAttendanceSolution.ALLOWLATEMAXTIMES = 0;

            //请假设置
            entAttendanceSolution.ANNUALLEAVESET = "0";
            entAttendanceSolution.ISEXPIRED = "0";

            //审核
            entAttendanceSolution.CHECKSTATE = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();
            entAttendanceSolution.EDITSTATE = Convert.ToInt32(SMT.SaaS.FrameworkUI.EditStates.UnActived).ToString();

            this.DataContext = entAttendanceSolution;
        }

        /// <summary>
        /// 根据考勤方案主键索引，获取考勤方案的信息
        /// </summary>
        /// <param name="strAttendanceSolID"></param>
        private void LoadBasicData(string strAttendanceSolID)
        {
            if (string.IsNullOrEmpty(strAttendanceSolID))
            {
                return;
            }

            if (FormType != FormTypes.Resubmit)
            {
                clientAtt.GetAttendanceSolutionByIDAsync(strAttendanceSolID);
            }
            else
            {
                clientAtt.CheckAttSolIsExistsAsignRdAsync(strAttendanceSolID, "2");
            }
        }

        /// <summary>
        /// 控制工作天数自定义
        /// </summary>
        /// <param name="cbxk"></param>
        private void IsEnabledWorkDays(SMT.HRM.UI.AppControl.DictionaryComboBox cbxk)
        {
            Visibility vib = Visibility.Collapsed;
            T_SYS_DICTIONARY entDic = cbxk.SelectedItem as T_SYS_DICTIONARY;
            if (entDic != null)
            {
                if (entDic.DICTIONARYVALUE != null)
                {
                    vib = entDic.DICTIONARYVALUE == 1 ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            tbWorkDaysTitle.Visibility = vib;
            nudWorkDays.Visibility = vib;
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("ATTSOLRDFORM");
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
                case "Delete":
                    //删除考勤方案定义
                    delete(AttendanceSolutionID);
                    break;
                //case "2":
                //    Submit();
                //    break;
            }
        }

        //删除考勤方案
        public void delete(string strID)
        {
            string Result = "";            
            
            //提示是否删除
            ComfirmWindow delComfirm = new ComfirmWindow();
            delComfirm.OnSelectionBoxClosed += (obj, result) =>
            {
                clientAtt.RemoveAttendanceSolutionAsync(strID);
            };
            delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), "确定要删除此考勤方案？", ComfirmWindow.titlename, Result);
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
            //ToolbarItem item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "2",
            //    Title = Utility.GetResourceStr("AUDITPASS"),// "审核通过",
            //    ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/18_audit.png"
            //};

            //ToolbarItems.Add(item);

            if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
            {
                if (entAttendanceSolution != null)
                {
                    if (entAttendanceSolution.CHECKSTATE == "1" && FormType == FormTypes.Edit)
                    {
                        ToolbarItems = new List<ToolbarItem>();
                    }
                }
            }
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            string strKeyName = "ATTENDANCESOLUTIONASIGNID";
            string strKeyValue = entAttendanceSolution.ATTENDANCESOLUTIONID;

            Dictionary<string, string> paraValue = new Dictionary<string, string>();
            paraValue.Add("EntityKey", entAttendanceSolution.ATTENDANCESOLUTIONID);

            Dictionary<string, string> paraText = new Dictionary<string, string>();
            strXmlObjectSource = Utility.ObjListToXml<T_HR_ATTENDANCESOLUTION>(entAttendanceSolution, paraValue, "HR", paraText, strKeyName, strKeyValue);

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras["CreateCompanyID"] = entAttendanceSolution.OWNERCOMPANYID;
            paras["CreateDepartmentID"] = entAttendanceSolution.OWNERDEPARTMENTID;
            paras["CreatePostID"] = entAttendanceSolution.OWNERPOSTID;
            paras["CreateUserID"] = entAttendanceSolution.OWNERID;
            
            Utility.SetAuditEntity(entity, "T_HR_ATTENDANCESOLUTION", AttendanceSolutionID, strXmlObjectSource, paras);
        }

        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            string strCheckState = string.Empty;
            string strEditState = Convert.ToInt32(EditStates.UnActived).ToString();
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    strCheckState = Utility.GetCheckState(CheckStates.Approving);
                    strEditState = Convert.ToInt32(EditStates.Actived).ToString();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    strCheckState = Utility.GetCheckState(CheckStates.Approved);
                    strEditState = Convert.ToInt32(EditStates.Actived).ToString();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    strCheckState = Utility.GetCheckState(CheckStates.UnApproved);
                    strEditState = Convert.ToInt32(EditStates.Canceled).ToString();
                    break;
            }
            entAttendanceSolution.EDITSTATE = strEditState;
            entAttendanceSolution.CHECKSTATE = strCheckState;

            //clientAtt.ModifyAttendanceSolutionAsync(entAttendanceSolution);
            clientAtt.AuditAttSolAsync(entAttendanceSolution.ATTENDANCESOLUTIONID, strCheckState);
            //Utility.UpdateCheckState("T_HR_ATTENDANCESOLUTION", "ATTENDANCESOLUTIONID", AttendanceSolutionID, args);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (entAttendanceSolution != null)
                state = entAttendanceSolution.CHECKSTATE;

            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }

            return state;
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 
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
                if (entAttendanceSolution != null)
                {
                    if (entAttendanceSolution.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
                //ToolbarItems.Add(ToolBarItems.Delete);
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_ATTENDANCESOLUTION", entAttendanceSolution.OWNERID,
                    entAttendanceSolution.OWNERPOSTID, entAttendanceSolution.OWNERDEPARTMENTID, entAttendanceSolution.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        private bool Save()
        {
            bool flag = false;

            flag = CheckAndSubmitTabForm(ref entAttendanceSolution, ref entAttendanceSolutionDeducts, ref entAttendFreeLeaves);

            if (!flag)
            {
                return false;
            }

            if (FormType == FormTypes.New)
            {
                AttendanceSolutionID = entAttendanceSolution.ATTENDANCESOLUTIONID;
                clientAtt.AddAttendanceSolutionAndCreateRelationAsync(entAttendanceSolution, entAttendanceSolutionDeducts, entAttendFreeLeaves);
            }
            else
            {
                clientAtt.ModifyAttendanceSolutionAndChangeRelationAsync(entAttendanceSolution, entAttendanceSolutionDeducts, entAttendFreeLeaves);
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

        /// <summary>
        /// 效验并整理提交的表单
        /// </summary>
        /// <param name="entAttendanceSolutionDeducts"></param>
        /// <param name="entAttendFreeLeaves"></param>
        /// <returns></returns>
        private bool CheckAndSubmitTabForm(ref T_HR_ATTENDANCESOLUTION entAttSol, ref ObservableCollection<T_HR_ATTENDANCESOLUTIONDEDUCT> entAttendanceSolutionDeducts, ref ObservableCollection<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves)
        {
            bool flag = false;
            CheckBasicForm(out flag);

            if (!flag)
            {
                return false;
            }

            flag = ucOTReward.Save(ref entAttSol);
            if (!flag)
            {
                return false;
            }

            flag = ucDeduct.Save(ref entAttSol, out entAttendanceSolutionDeducts);
            if (!flag)
            {
                return false;
            }

            flag = ucLeave.Save(ref entAttSol, out entAttendFreeLeaves);
            if (!flag)
            {
                return false;
            }

            flag = ucDef.Save(ref entAttSol);
            if (!flag)
            {
                return false;
            }

            this.DataContext = entAttSol;

            return flag;
        }

        /// <summary>
        /// 效验提交的内容
        /// </summary>
        /// <param name="flag"></param>
        private void CheckBasicForm(out bool flag)
        {
            flag = false;

            if (string.IsNullOrEmpty(txtAttSolName.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ATTENDANCESOLUTIONNAME"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ATTENDANCESOLUTIONNAME")));
                flag = false;
                return;
            }
            else
            {
                flag = true;
            }

            if (cbxkAttendanceType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ATTENDANCETYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ATTENDANCETYPE")));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkAttendanceType.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    flag = true;
                    entAttendanceSolution.ATTENDANCETYPE = entDic.DICTIONARYVALUE.ToString();
                }
            }

            if (cbxkCardType.Visibility == System.Windows.Visibility.Visible)
            {
                if (cbxkCardType.SelectedItem == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDTYPE"), Utility.GetResourceStr("REQUIRED", "CARDTYPE"));
                    flag = false;
                    return;
                }
                else
                {
                    T_SYS_DICTIONARY entDic = cbxkCardType.SelectedItem as T_SYS_DICTIONARY;
                    if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                    {
                        flag = true;
                        entAttendanceSolution.CARDTYPE = entDic.DICTIONARYVALUE.ToString();
                    }
                }
            }

            if (cbxkWorkDayType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKDAYTYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("WORKDAYTYPE")));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkWorkDayType.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    flag = true;
                    entAttendanceSolution.WORKDAYTYPE = entDic.DICTIONARYVALUE.ToString();
                }
            }

            entAttendanceSolution.ISCURRENTMONTH = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbIsCurrentMonth.IsChecked.Value == true)
            {
                entAttendanceSolution.ISCURRENTMONTH = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            //是否自动导入打卡记录
            if (RadY.IsChecked ==true)
            {
                entAttendanceSolution.ISAUTOIMPORTPUNCH = "1";
            }
            else
            {
                entAttendanceSolution.ISAUTOIMPORTPUNCH = "0";
            }

            entAttendanceSolution.REMARK = txtAttSolRemark.Text;
            if (string.IsNullOrWhiteSpace(entAttendanceSolution.REMARK))
            {
                entAttendanceSolution.REMARK = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(entAttendanceSolution.YEARLYBALANCEDATE))
            {
                entAttendanceSolution.YEARLYBALANCEDATE = "1";
            }

            if (string.IsNullOrWhiteSpace(entAttendanceSolution.REMARK))
            {
                entAttendanceSolution.YEARLYBALANCETYPE = "0";
            }

        }
        #endregion

        #region 事件
        /// <summary>
        /// 根据主键索引，获得指定的假期记录以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendanceSolutionByIDCompleted(object sender, GetAttendanceSolutionByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entAttendanceSolution = e.Result;
                if (FormType == FormTypes.Resubmit)
                {
                    entAttendanceSolution.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                }

                entAttendanceSolution.UPDATEDATE = DateTime.Now;
                entAttendanceSolution.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                this.DataContext = entAttendanceSolution;

                cbIsCurrentMonth.IsChecked = false;
                if (entAttendanceSolution.ISCURRENTMONTH == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
                {
                    cbIsCurrentMonth.IsChecked = true;
                }

                //如果ISAUTOIMPORTPUNCH=0 表示不自动导入； =1表示自动导入
                if (entAttendanceSolution.ISAUTOIMPORTPUNCH=="0")
                {
                    this.RadN.IsChecked = true;
                }
                else
                {
                    this.RadY.IsChecked = true;
                }

                if (entAttendanceSolution.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    UnEnableFormControl();
                    this.ucOTReward.IsEnabled = false;
                    this.ucDeduct.IsEnabled = false;
                    this.ucDef.IsEnabled = false;
                    this.ucLeave.IsEnabled = false;
                }

                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 检查当前考勤方案是否已经被应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_CheckAttSolIsExistsAsignRdCompleted(object sender, CheckAttSolIsExistsAsignRdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == 0)
                {
                    clientAtt.GetAttendanceSolutionByIDAsync(AttendanceSolutionID);
                    return;
                }

                this.IsEnabled = false;
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ReSubmit"), "审核通过的考勤方案不能重新提交");
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), string.Format("读取数据失败：{0}", Utility.GetResourceStr("ERROR")));
            }
        }

        /// <summary>
        /// 执行新增考勤方案及建立其关联设置的关系的事件，显示返回的结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddAttendanceSolutionAndCreateRelationCompleted(object sender, AddAttendanceSolutionAndCreateRelationCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (string.IsNullOrWhiteSpace(e.Result))
                {
                    return;
                }

                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));

                    if (closeFormFlag)
                    {
                        CloseForm();
                        return;
                    }

                    FormType = FormTypes.Edit;
                    AttendanceSolutionID = entAttendanceSolution.ATTENDANCESOLUTIONID;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    //添加删除按钮
                    ToolbarItems = Utility.CreateFormEditButton();
                    ToolbarItems.Add(ToolBarItems.Delete);
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 返回审核处理结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AuditAttSolCompleted(object sender, AuditAttSolCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTSOLRDFORM")));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
        }

        //void clientAtt_ModifyAttendanceSolutionCompleted(object sender, ModifyAttendanceSolutionCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result == "{SAVESUCCESSED}")
        //        {
        //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTSOLRDFORM")));                    
        //        }
        //        else
        //        {
        //            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
        //        }
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }

        //    RefreshUI(RefreshedTypes.HideProgressBar);
        //    RefreshUI(RefreshedTypes.All);
        //}

        /// <summary>
        /// 执行更新考勤方案及修改其关联设置的关系的事件，显示返回的结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifyAttendanceSolutionAndChangeRelationCompleted(object sender, ModifyAttendanceSolutionAndChangeRelationCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (string.IsNullOrWhiteSpace(e.Result))
                {
                    return;
                }

                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));

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
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 控制打卡方式的显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxkAttendanceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Visibility vib = Visibility.Collapsed;
            T_SYS_DICTIONARY entDic = cbxkAttendanceType.SelectedItem as T_SYS_DICTIONARY;
            if (entDic != null)
            {
                if (entDic.DICTIONARYVALUE != null)
                {
                    if (entDic.DICTIONARYVALUE == (Convert.ToInt32(AttendanceType.NoCheck) + 1) || entDic.DICTIONARYVALUE == (Convert.ToInt32(AttendanceType.LoginCheck) + 1))
                    {
                        vib = Visibility.Collapsed;
                    }
                    else
                    {
                        vib = Visibility.Visible;
                    }
                }
            }

            tbCardTypeTitle.Visibility = vib;
            cbxkCardType.Visibility = vib;
        }

        /// <summary>
        /// 控制工作天数的输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxkWorkDayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsEnabledWorkDays(cbxkWorkDayType);
        }

        #endregion
    }
}

