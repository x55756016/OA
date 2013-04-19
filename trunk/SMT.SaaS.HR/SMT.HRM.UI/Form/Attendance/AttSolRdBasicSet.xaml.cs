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
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttSolRdBasicSet : BaseForm
    {
        #region 全局变量
        internal T_HR_ATTENDANCESOLUTION entAttendanceSolution { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        #endregion

        #region 初始化

        /// <summary>
        /// 考勤方案-基本信息初始化
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="strAttendanceSolutionID"></param>
        public AttSolRdBasicSet()
        {
            InitializeComponent();
            RegisterEvents();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetAttendanceSolutionByIDCompleted += new EventHandler<GetAttendanceSolutionByIDCompletedEventArgs>(clientAtt_GetAttendanceSolutionByIDCompleted);
        }

        /// <summary>
        /// 页面初始化
        /// </summary>
        internal void InitParas(FormTypes FormType, string strAttendanceSolID)
        {
            if (FormType == FormTypes.New)
            {
                InitForm();
            }
            else if (FormType == FormTypes.Edit)
            {
                LoadData(strAttendanceSolID);
            }

            IsEnabledWorkDays(cbxkWorkDayType);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 
        /// </summary>
        private void InitForm()
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
            entAttendanceSolution.ISCURRENTMONTH = "0";
          
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
        /// 
        /// </summary>
        private void LoadData(string strAttendanceSolID)
        {
            if (string.IsNullOrEmpty(strAttendanceSolID))
            {
                return;
            }

            clientAtt.GetAttendanceSolutionByIDAsync(strAttendanceSolID);
        }

        /// <summary>
        /// 控制工作天数自定义
        /// </summary>
        /// <param name="cbxkWorkDayType"></param>
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
            txtWorkDays.Visibility = vib;
            tbWorkDaysTimeUnit.Visibility = vib;
        }

        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entAttendanceSolution"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = false;
            decimal dWorkTime = 0;

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

            if (cbxkCardType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDTYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("CARDTYPE")));
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

            if (cbxkWorkDayType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDTYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("CARDTYPE")));
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

            if (cbxkIsCurrentMonth.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ISCURRENTMONTH"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ISCURRENTMONTH")));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkIsCurrentMonth.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    flag = true;
                    entAttendanceSolution.ISCURRENTMONTH = entDic.DICTIONARYVALUE.ToString();
                }
            }

            flag = decimal.TryParse(txtWorkTime.Text, out dWorkTime);
            if (!flag)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKTIME"), Utility.GetResourceStr("REQUIREDNUMERICAL", "WORKTIME"));
                flag = false;
                return;
            }

            entAttendanceSolution.WORKTIMEPERDAY = dWorkTime;
        }        

        public bool Save(out T_HR_ATTENDANCESOLUTION entAttSol)
        {
            bool flag = false;
            entAttSol = new T_HR_ATTENDANCESOLUTION();

            CheckSubmitForm(out flag);

            if (!flag)
            {
                entAttSol = null;
                return false;
            }

            entAttSol = Utility.Clone(entAttendanceSolution);
            flag = true;
            return flag;
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

                entAttendanceSolution.UPDATEDATE = DateTime.Now;
                entAttendanceSolution.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                this.DataContext = entAttendanceSolution;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }        

        private void cbxkWorkDayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsEnabledWorkDays(cbxkWorkDayType);
        }
        #endregion



    }
}
