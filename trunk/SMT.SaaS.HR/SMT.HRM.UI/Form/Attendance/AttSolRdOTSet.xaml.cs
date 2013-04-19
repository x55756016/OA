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

using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttSolRdOTSet : BaseForm
    {
        #region 全局变量
        private T_HR_ATTENDANCESOLUTION entAttendanceSolution { get; set; }
        private T_HR_OVERTIMEREWARD entOvertimeReward { get; set; }
        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        #endregion

        #region 初始化

        public AttSolRdOTSet()
        {
            InitializeComponent();
            RegisterEvents();
            UnEnabledOTReward();
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
            else
            {
                LoadData(strAttendanceSolID);
                if (FormType == FormTypes.Browse)
                {
                    this.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// 加班设置的相关信息设置为只读状态，禁编辑
        /// </summary>
        private void UnEnabledOTReward()
        {
            txtUsualOverTimePayRate.IsEnabled = false;
            txtWeekendPayRate.IsEnabled = false;
            txtVacationPayRate.IsEnabled = false;
            txtRemark.IsEnabled = false;
        }

        /// <summary>
        /// 新增时，数据初始化
        /// </summary>
        private void InitForm()
        {
            entAttendanceSolution = new T_HR_ATTENDANCESOLUTION();
            entOvertimeReward = new T_HR_OVERTIMEREWARD();

            //加班设置(T_HR_ATTENDANCESOLUTION 仅设置此部分存储及初始化当前页面的设置)
            entAttendanceSolution.OVERTIMEVALID = "0";
            entAttendanceSolution.OVERTIMEPAYTYPE = "0";
            entAttendanceSolution.OVERTIMECHECK = "0";
            cbOTTimeCheck.IsChecked = false;

            entAttendanceSolution.T_HR_OVERTIMEREWARD = entOvertimeReward;

            this.DataContext = entAttendanceSolution;
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 编辑时，根据
        /// </summary>
        /// <param name="strAttendanceSolID"></param>
        private void LoadData(string strAttendanceSolID)
        {
            if (string.IsNullOrEmpty(strAttendanceSolID))
            {
                return;
            }

            clientAtt.GetAttendanceSolutionByIDAsync(strAttendanceSolID);
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckNeedEnterOTTimePay()
        {
            tbOneDayOvertimeHoursTitle.Visibility = Visibility.Collapsed;
            nudOneDayOvertimeHours.Visibility = Visibility.Collapsed;
            tbOneDayOvertimeHoursUnit.Visibility = Visibility.Collapsed;

            if (cbxkOTTimePayType.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkOTTimePayType.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
            {
                return;
            }

            if (entDic.DICTIONARYVALUE.ToString() == "1")
            {
                tbOneDayOvertimeHoursTitle.Visibility = Visibility.Visible;
                nudOneDayOvertimeHours.Visibility = Visibility.Visible;
                tbOneDayOvertimeHoursUnit.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entAttendanceSolution"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = false;

            if (cbxkOTValid.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("OVERTIMEVALID"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("OVERTIMEVALID")));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkOTValid.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    flag = true;
                    entAttendanceSolution.OVERTIMEVALID = entDic.DICTIONARYVALUE.ToString();
                }
            }

            if (cbxkOTTimePayType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("OVERTIMEPAYTYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("OVERTIMEPAYTYPE")));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkOTTimePayType.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    flag = true;
                    entAttendanceSolution.OVERTIMEPAYTYPE = entDic.DICTIONARYVALUE.ToString();
                }
            }

            entAttendanceSolution.OVERTIMECHECK = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbOTTimeCheck.IsChecked.Value == true)
            {
                entAttendanceSolution.OVERTIMECHECK = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            decimal dOneDayOvertimeHours = 0;
            decimal.TryParse(nudOneDayOvertimeHours.Value.ToString(), out dOneDayOvertimeHours);
            if (dOneDayOvertimeHours <= 0 && entAttendanceSolution.OVERTIMEPAYTYPE == "1")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ONEDAYOVERTIMEHOURS"), string.Format(Utility.GetResourceStr("DATECOMPARE"), Utility.GetResourceStr("ONEDAYOVERTIMEHOURS"), "0"));
                flag = false;
                return;
            }
            entAttendanceSolution.ONEDAYOVERTIMEHOURS = dOneDayOvertimeHours;

            if (string.IsNullOrEmpty((lkOTReward.DataContext as T_HR_OVERTIMEREWARD).OVERTIMEREWARDID))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("OVERTIMEREWARDSET"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("OVERTIMEREWARDSET")));
                flag = false;
                return;
            }
            else
            {
                T_HR_OVERTIMEREWARD entOTReward = lkOTReward.DataContext as T_HR_OVERTIMEREWARD;
                if (!string.IsNullOrEmpty(entOTReward.OVERTIMEREWARDID))
                {
                    flag = true;
                    entAttendanceSolution.T_HR_OVERTIMEREWARD = null;
                    entAttendanceSolution.T_HR_OVERTIMEREWARD = entOTReward;
                }
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="entAttSol"></param>
        /// <returns></returns>
        public bool Save(ref T_HR_ATTENDANCESOLUTION entAttSol)
        {
            bool flag = false;
            if (entAttSol == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(entAttSol.ATTENDANCESOLUTIONID))
            {
                return false;
            }

            if (entAttendanceSolution == null)
            {
                return false;
            }

            CheckSubmitForm(out flag);

            if (!flag)
            {
                return false;
            }

            entAttSol.OVERTIMEVALID = entAttendanceSolution.OVERTIMEVALID;
            entAttSol.OVERTIMEPAYTYPE = entAttendanceSolution.OVERTIMEPAYTYPE;
            entAttSol.OVERTIMECHECK = entAttendanceSolution.OVERTIMECHECK;
            entAttSol.ONEDAYOVERTIMEHOURS = entAttendanceSolution.ONEDAYOVERTIMEHOURS;

            entAttSol.T_HR_OVERTIMEREWARD = entAttendanceSolution.T_HR_OVERTIMEREWARD;
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

                this.DataContext = entAttendanceSolution;

                cbOTTimeCheck.IsChecked = false;
                if (entAttendanceSolution.OVERTIMECHECK == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
                {
                    cbOTTimeCheck.IsChecked = true;
                }

                CheckNeedEnterOTTimePay();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        ///  加班报酬方式如果是调休，则需要填写调休一天需要抵扣的加班时长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxkOTTimePayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckNeedEnterOTTimePay();
        }

        private void lkOTReward_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("OVERTIMEREWARDNAME", "OVERTIMEREWARDNAME");
            cols.Add("USUALOVERTIMEPAYRATE", "USUALOVERTIMEPAYRATE");
            cols.Add("WEEKENDPAYRATE", "WEEKENDPAYRATE");
            cols.Add("VACATIONPAYRATE", "VACATIONPAYRATE");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.OvertimeReward,
                typeof(T_HR_OVERTIMEREWARD[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_OVERTIMEREWARD ent = lookup.SelectedObj as T_HR_OVERTIMEREWARD;

                if (ent != null)
                {
                    lkOTReward.DataContext = ent;
                    txtUsualOverTimePayRate.Text = ent.USUALOVERTIMEPAYRATE.Value.ToString();
                    txtVacationPayRate.Text = ent.VACATIONPAYRATE.Value.ToString();
                    txtWeekendPayRate.Text = ent.WEEKENDPAYRATE.Value.ToString();
                    txtRemark.Text = ent.REMARK == null ? string.Empty : ent.REMARK;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }        
        #endregion

        
    }
}
