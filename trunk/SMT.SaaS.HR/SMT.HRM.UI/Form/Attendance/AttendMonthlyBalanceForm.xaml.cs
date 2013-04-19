/*
 * 文件名：AttendMonthlyBalanceForm.xaml.cs
 * 作  用：考勤月度结算表单，实现如下功能：编辑，审核
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
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.AttendanceWS;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttendMonthlyBalanceForm : BaseForm, IEntityEditor, IAudit
    {
        #region 全局变量
        public FormTypes FormType { get; set; }

        public string MonthlyBalanceId { get; set; }

        public T_HR_ATTENDMONTHLYBALANCE entAttendMonthlyBalance { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient clientPer = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private string strResMsg = string.Empty;
        #endregion

        #region 初始化
        public AttendMonthlyBalanceForm(FormTypes formtype, string strMonthlyBalanceId)
        {
            FormType = formtype;
            MonthlyBalanceId = strMonthlyBalanceId;
            InitializeComponent();
            RegisterEvents();
            InitParas();
        }

        private void InitParas()
        {
            if (FormType != FormTypes.Audit && FormType != FormTypes.Browse)
            {
                SetToolBar();
                return;
            }

            LoadData();
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(MonthlyBalanceId))
            {
                return;
            }

            clientAtt.GetAttendMonthlyBalanceByIDAsync(MonthlyBalanceId);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("ATTENDANCESOLUTIONASIGNFORM");
        }

        public string GetStatus()
        {
            string strTemp = string.Empty;
            if (!string.IsNullOrEmpty(MonthlyBalanceId))
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

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Utility.SetAuditEntity(entity, "T_HR_ATTENDMONTHLYBALANCE", MonthlyBalanceId);
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            Utility.UpdateCheckState("T_HR_ATTENDMONTHLYBALANCE", "MONTHLYBALANCEID", MonthlyBalanceId, args);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (entAttendMonthlyBalance != null)
                state = entAttendMonthlyBalance.CHECKSTATE;
            return state;
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 权限控制按钮显示
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
            else
            {
                if (entAttendMonthlyBalance == null)
                {
                    ToolbarItems = new List<ToolbarItem>();
                    return;
                }

                ToolbarItems = Utility.CreateFormEditButton("T_HR_ATTENDMONTHLYBALANCE", entAttendMonthlyBalance.OWNERID,
                    entAttendMonthlyBalance.OWNERPOSTID, entAttendMonthlyBalance.OWNERDEPARTMENTID, entAttendMonthlyBalance.OWNERCOMPANYID);

            }

            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 显示员工信息
        /// </summary>
        /// <param name="entEmpPost"></param>
        private void ShowEmployeeInfo(SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST entEmpPost)
        {
            if (entEmpPost == null)
            {
                return;
            }

            tbPostName.Text = entEmpPost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
            tbDepName.Text = entEmpPost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            tbCPYName.Text = entEmpPost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
        }


        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entAttendMonthlyBalance"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = true;

            if (string.IsNullOrEmpty(tbAttNeedAttendDays.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("NEEDATTENDDAYS"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("NEEDATTENDDAYS")));
                flag = false;
                return;
            }

            if (string.IsNullOrEmpty(tbAttRealAttendDays.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("REALATTENDDAYS"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("REALATTENDDAYS")));
                flag = false;
                return;
            }

            if (!flag)
            {
                return;
            }

            if (FormType == FormTypes.Edit)
            {
                entAttendMonthlyBalance.UPDATEDATE = DateTime.Now;
                entAttendMonthlyBalance.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
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

                if (FormType == FormTypes.Edit)
                {
                    clientAtt.ModifyAttendMonthlyBalanceAsync(entAttendMonthlyBalance);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            return flag;
        }

        /// <summary>
        /// 关闭当前窗口
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
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetAttendMonthlyBalanceByIDCompleted += new EventHandler<GetAttendMonthlyBalanceByIDCompletedEventArgs>(clientAtt_GetAttendMonthlyBalanceByIDCompleted);
            clientAtt.ModifyAttendMonthlyBalanceCompleted += new EventHandler<ModifyAttendMonthlyBalanceCompletedEventArgs>(clientAtt_ModifyAttendMonthlyBalanceCompleted);
            clientPer.GetEmployeeDetailByIDCompleted += new EventHandler<SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs>(clientPer_GetEmployeeDetailByIDCompleted);
        }

        void clientAtt_GetAttendMonthlyBalanceByIDCompleted(object sender, GetAttendMonthlyBalanceByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entAttendMonthlyBalance = e.Result;
                this.DataContext = entAttendMonthlyBalance;

                if (entAttendMonthlyBalance != null)
                {
                    clientPer.GetEmployeeDetailByIDAsync(entAttendMonthlyBalance.EMPLOYEEID);
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
        /// 编辑月度结算信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifyAttendMonthlyBalanceCompleted(object sender, ModifyAttendMonthlyBalanceCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTENDANCESOLUTIONASIGNFORM")));
                    InitParas();
                    RefreshUI(RefreshedTypes.All);
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
        }

        /// <summary>
        /// 获取员工信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientPer_GetEmployeeDetailByIDCompleted(object sender, SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST entEmpPost = e.Result;

                ShowEmployeeInfo(entEmpPost);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }
        #endregion
    }
}
