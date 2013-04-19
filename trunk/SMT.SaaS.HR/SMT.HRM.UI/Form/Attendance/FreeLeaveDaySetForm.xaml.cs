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
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.AttendanceWS;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class FreeLeaveDaySetForm : BaseForm, IEntityEditor
    {
        #region 全局变量
        public FormTypes FormType { get; set; }

        public string FreeLeaveDaySetID { get; set; }

        public T_HR_FREELEAVEDAYSET entFreeLeaveDaySet { get; set; }

        public T_HR_LEAVETYPESET LeaveTypeSet { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private int dmaxMonth = 9999;

        private string strResMsg = string.Empty;
        #endregion

        #region 初始化
        public FreeLeaveDaySetForm(FormTypes formtype, string strFreeLeaveDaySetID, T_HR_LEAVETYPESET entLeaveTypeSet)
        {
            FormType = formtype;
            FreeLeaveDaySetID = strFreeLeaveDaySetID;
            LeaveTypeSet = entLeaveTypeSet;
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(FreeLeaveDaySetForm_Loaded);
        }

        void FreeLeaveDaySetForm_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            InitParas();
        }

        private void RegisterEvents()
        {
            clientAtt.GetFreeLeaveDaySetByIDCompleted += new EventHandler<GetFreeLeaveDaySetByIDCompletedEventArgs>(clientAtt_GetFreeLeaveDaySetByIDCompleted);
            clientAtt.AddFreeLeaveDaySetCompleted += new EventHandler<AddFreeLeaveDaySetCompletedEventArgs>(clientAtt_AddFreeLeaveDaySetCompleted);
            clientAtt.ModifyFreeLeaveDaySetCompleted += new EventHandler<ModifyFreeLeaveDaySetCompletedEventArgs>(clientAtt_ModifyFreeLeaveDaySetCompleted);
        }

        private void InitParas()
        {
            if (FormType == FormTypes.New)
            {
                InitForm();
                SetToolBar();
            }
            else
            {
                LoadData();
                if (FormType == FormTypes.Browse)
                {
                    this.IsEnabled = false;
                }
            }

            lkLeaveTypeName.IsEnabled = false;
        }

        /// <summary>
        /// 表单初始化
        /// </summary>
        private void InitForm()
        {
            entFreeLeaveDaySet = new T_HR_FREELEAVEDAYSET();
            
            entFreeLeaveDaySet.FREELEAVEDAYSETID = System.Guid.NewGuid().ToString().ToUpper();
            if (LeaveTypeSet == null)
            {
                LeaveTypeSet = new T_HR_LEAVETYPESET();
            }           

            entFreeLeaveDaySet.T_HR_LEAVETYPESET = LeaveTypeSet;


            //权限控制
            entFreeLeaveDaySet.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entFreeLeaveDaySet.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entFreeLeaveDaySet.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entFreeLeaveDaySet.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            entFreeLeaveDaySet.CREATEDATE = DateTime.Now;
            entFreeLeaveDaySet.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entFreeLeaveDaySet.UPDATEDATE = System.DateTime.Now;
            entFreeLeaveDaySet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据           
            entFreeLeaveDaySet.MINIMONTH = decimal.Parse("0");
            entFreeLeaveDaySet.MAXMONTH = decimal.Parse("0");
            entFreeLeaveDaySet.LEAVEDAYS = decimal.Parse("0");
            entFreeLeaveDaySet.ISPERFECTATTENDANCEFACTOR = "1";//默认为否
            entFreeLeaveDaySet.OFFESTTYPE = "0";

            this.DataContext = entFreeLeaveDaySet;
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(FreeLeaveDaySetID))
            {
                return;
            }

            clientAtt.GetFreeLeaveDaySetByIDAsync(FreeLeaveDaySetID);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("FREELEAVEDAYSETFORM");
        }

        public string GetStatus()
        {
            return string.Empty;
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
            }
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
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;

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
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }

            if (LeaveTypeSet != null)
            {
                if (LeaveTypeSet.ISFREELEAVEDAY != (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
                {
                    ToolbarItems = new List<ToolbarItem>();
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVETYPESET"), Utility.GetResourceStr("LEAVETYPESETISFREEDAY"));
                    this.IsEnabled = false;
                }
            }

            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entFreeLeaveDaySet"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = false;

            if (lkLeaveTypeName.DataContext == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVETYPESET"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("LEAVETYPESET")));
                flag = false;
                return;
            }
            else
            {
                T_HR_LEAVETYPESET entLeaveTypeSet = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
                if (string.IsNullOrEmpty(entLeaveTypeSet.LEAVETYPESETID))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVETYPESET"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("LEAVETYPESET")));
                    flag = false;
                    return;
                }

                flag = true;
                entFreeLeaveDaySet.T_HR_LEAVETYPESET = entLeaveTypeSet;
            }

            if (nudMaxMonth.Value == 0)
            {
                nudMaxMonth.Value = dmaxMonth;
                entFreeLeaveDaySet.MAXMONTH = dmaxMonth;
            }

            double dLimitMinMonth = 0, dLimitMaxMonth = 0;
            dLimitMinMonth = nudMiniMonth.Value;
            dLimitMaxMonth = nudMaxMonth.Value;
            if (dLimitMinMonth > dLimitMaxMonth)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FREELEAVEDAYSET_MAXMONTH"), Utility.GetResourceStr("DATECOMPARE", "FREELEAVEDAYSET_MAXMONTH, FREELEAVEDAYSET_MINIMONTH"));
                flag = false;
                return;
            }

            entFreeLeaveDaySet.ISPERFECTATTENDANCEFACTOR = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbxIsPerfectAttendanceFactor.IsChecked.Value == true)
            {
                entFreeLeaveDaySet.ISPERFECTATTENDANCEFACTOR = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }
            
            if (!flag)
            {
                return;
            }

            if (FormType == FormTypes.Edit)
            {
                entFreeLeaveDaySet.UPDATEDATE = DateTime.Now;
                entFreeLeaveDaySet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            bool flag = false;

            try
            {
                List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
                if (validators.Count > 0)
                {
                    return false;
                }

                CheckSubmitForm(out flag);

                if (!flag)
                {
                    return false;
                }

                if (FormType == FormTypes.New)
                {
                    clientAtt.AddFreeLeaveDaySetAsync(entFreeLeaveDaySet);
                }
                else
                {
                    clientAtt.ModifyFreeLeaveDaySetAsync(entFreeLeaveDaySet);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            return flag;
        }

        /// <summary>
        /// 保存并退出
        /// </summary>
        private void Cancel()
        {
            bool flag = false;
            flag = Save();
            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        #endregion

        #region 事件
        /// <summary>
        /// 根据主键索引，获得指定的假期记录以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetFreeLeaveDaySetByIDCompleted(object sender, GetFreeLeaveDaySetByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entFreeLeaveDaySet = e.Result;
                this.DataContext = entFreeLeaveDaySet;
                cbxIsPerfectAttendanceFactor.IsChecked = false;
                if (entFreeLeaveDaySet.ISPERFECTATTENDANCEFACTOR == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
                {
                    cbxIsPerfectAttendanceFactor.IsChecked = true;
                }

                SetToolBar();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 新增假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddFreeLeaveDaySetCompleted(object sender, AddFreeLeaveDaySetCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
                    InitParas();
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
        /// 更新假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifyFreeLeaveDaySetCompleted(object sender, ModifyFreeLeaveDaySetCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "FREELEAVEDAYSETFORM")));
                    InitParas();
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
        /// 考勤方案查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void lkAttendanceSolutionName_FindClick(object sender, EventArgs e)
        //{
        //    Dictionary<string, string> cols = new Dictionary<string, string>();
        //    cols.Add("ATTENDANCESOLUTIONID", "ATTENDANCESOLUTIONID");
        //    cols.Add("ATTENDANCESOLUTIONNAME", "ATTENDANCESOLUTIONNAME");
        //    LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.AttendanceSolution,
        //        typeof(T_HR_ATTENDANCESOLUTION[]), cols);

        //    lookup.SelectedClick += (o, ev) =>
        //    {
        //        T_HR_ATTENDANCESOLUTION ent = lookup.SelectedObj as T_HR_ATTENDANCESOLUTION;

        //        if (ent != null)
        //        {
        //            lkAttendanceSolutionName.DataContext = ent;                   
        //        }
        //    };
       // 
        //    lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        //}

        /// <summary>
        /// 请假类型查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkLeaveTypeName_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("VACATIONNAME", "LEAVETYPENAME");
            cols.Add("FINETYPE", "FINETYPE");
            cols.Add("LEAVEMAXDAYS", "MAXDAYS");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.LeaveTypeSet,
                typeof(T_HR_LEAVETYPESET[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_LEAVETYPESET ent = lookup.SelectedObj as T_HR_LEAVETYPESET;

                if (ent != null)
                {
                    lkLeaveTypeName.DataContext = ent;                    
                }
            };
            
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }
        #endregion

    }
}
