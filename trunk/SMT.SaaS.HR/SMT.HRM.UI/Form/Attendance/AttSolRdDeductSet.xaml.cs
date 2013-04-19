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
    public partial class AttSolRdDeductSet : BaseForm
    {
        #region 全局变量

        private T_HR_ATTENDANCESOLUTION entAttendanceSolution { get; set; }

        private ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER> entAttendanceDeductMasters { get; set; }

        private ObservableCollection<T_HR_ATTENDANCESOLUTIONDEDUCT> entAttendanceSolutionDeducts { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        #endregion

        #region 初始化
        public AttSolRdDeductSet()
        {
            InitializeComponent();
            RegisterEvents();
            UnVisibleGridToolControl();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);

            clientAtt.GetAttendanceSolutionDeductRdListByAttSolIDCompleted += new EventHandler<GetAttendanceSolutionDeductRdListByAttSolIDCompletedEventArgs>(clientAtt_GetAttendanceSolutionDeductRdListByAttSolIDCompleted);
        }

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

        private void InitForm()
        {
            nudLostCardTimes.Value = 0;
            nudLateMaxMinute.Value = 0;
            nudLateMaxTimes.Value = 0;

            entAttendanceSolutionDeducts = new ObservableCollection<T_HR_ATTENDANCESOLUTIONDEDUCT>();
        }

        private void LoadData(string strAttendanceSolID)
        {
            if (string.IsNullOrEmpty(strAttendanceSolID))
            {
                return;
            }

            string strSortKey = " SOLUTIONDEDUCTID ";

            clientAtt.GetAttendanceSolutionDeductRdListByAttSolIDAsync(strAttendanceSolID, strSortKey);
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 隐藏当前页不需要使用的GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl()
        {
            //toolbar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            toolbar1.btnAudit.Visibility = Visibility.Collapsed;
            //toolbar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            toolbar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbar1.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbar1.btnEdit.Visibility = Visibility.Collapsed;
            toolbar1.btnRefresh.Visibility = Visibility.Collapsed;

            toolbar1.retRefresh.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;
            toolbar1.retEdit.Visibility = Visibility.Collapsed;
            toolbar1.retAudit.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 将存储于OrganizationWS服务下T_HR_ATTENDANCEDEDUCTMASTER实体的数据，复制后改存到AttendanceWS服务下T_HR_ATTENDANCEDEDUCTMASTER实体
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        private T_HR_ATTENDANCEDEDUCTMASTER ReplicateDataToNewStructure(SMT.Saas.Tools.OrganizationWS.T_HR_ATTENDANCEDEDUCTMASTER ent)
        {
            T_HR_ATTENDANCEDEDUCTMASTER entTemp = new T_HR_ATTENDANCEDEDUCTMASTER();
            if (ent == null)
            {
                return null;
            }

            entTemp.DEDUCTMASTERID = ent.DEDUCTMASTERID;
            entTemp.ATTENDABNORMALTYPE = ent.ATTENDABNORMALTYPE;
            entTemp.FINETYPE = ent.FINETYPE;
            entTemp.PARAMETERVALUE = ent.PARAMETERVALUE;
            entTemp.FINERATIO = ent.FINERATIO;
            entTemp.ISACCUMULATING = ent.ISACCUMULATING;
            entTemp.FINESUM = ent.FINESUM;
            entTemp.ISPERFECTATTENDANCEFACTOR = ent.ISPERFECTATTENDANCEFACTOR;
            entTemp.REMARK = ent.REMARK;
            entTemp.CREATEUSERID = ent.CREATEUSERID;
            entTemp.CREATEDATE = ent.CREATEDATE;
            entTemp.UPDATEUSERID = ent.UPDATEUSERID;
            entTemp.UPDATEDATE = ent.UPDATEDATE;

            return entTemp;
        }

        /// <summary>
        /// 显示考勤异常扣款的设置记录列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addNewDeduct()
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("ATTENDABNORMALNAME", "ATTENDABNORMALNAME");
            cols.Add("ISACCUMULATING", "ISACCUMULATING,ISCHECKED,DICTIONARYCONVERTER");
            cols.Add("ISPERFECTATTENDANCEFACTOR", "ISPERFECTATTENDANCEFACTOR,ISCHECKED,DICTIONARYCONVERTER");

            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.AttendanceDeductMaster,
                typeof(SMT.Saas.Tools.OrganizationWS.T_HR_ATTENDANCEDEDUCTMASTER[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_ATTENDANCEDEDUCTMASTER ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_ATTENDANCEDEDUCTMASTER;

                if (ent != null)
                {
                    T_HR_ATTENDANCEDEDUCTMASTER entView = ReplicateDataToNewStructure(ent);
                    ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER> entList = new ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER>();
                    if (entView == null)
                    {
                        return;
                    }

                    if (dgAttendanceDeductlist.ItemsSource != null)
                    {
                        entList = dgAttendanceDeductlist.ItemsSource as ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER>;
                        bool flag = false;
                        flag = IsContainsEntity(entList, entView);

                        if (flag)
                        {
                            return;
                        }
                    }

                    entList.Add(entView);
                    dgAttendanceDeductlist.ItemsSource = entList;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 检测DataGrid是否存在重复记录
        /// </summary>
        /// <param name="entList"></param>
        /// <param name="entView"></param>
        /// <returns></returns>
        private bool IsContainsEntity(ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER> entList, T_HR_ATTENDANCEDEDUCTMASTER entView)
        {
            bool flag = false;

            var q = from item in entList
                    where item.ATTENDABNORMALTYPE == entView.ATTENDABNORMALTYPE
                    select item;

            if (q.Count() == 0)
            {
                return flag;
            }

            flag = true;

            return flag;
        }

        /// <summary>
        /// 效验输入基本信息内容
        /// </summary>
        /// <param name="entAttSol"></param>
        private void CheckSubmitForm(ref T_HR_ATTENDANCESOLUTION entAttSol)
        {
            if (entAttSol == null)
            {
                return;
            }

            decimal dAllowLostCardTimes = 0, dAllowLateMaxMinute = 0, dAllowLateMaxTimes = 0;

            decimal.TryParse(nudLostCardTimes.Value.ToString(), out dAllowLostCardTimes);
            decimal.TryParse(nudLateMaxMinute.Value.ToString(), out dAllowLateMaxMinute);
            decimal.TryParse(nudLateMaxTimes.Value.ToString(), out dAllowLateMaxTimes);

            entAttSol.ALLOWLOSTCARDTIMES = dAllowLostCardTimes;
            entAttSol.ALLOWLATEMAXMINUTE = dAllowLateMaxMinute;
            entAttSol.ALLOWLATEMAXTIMES = dAllowLateMaxTimes;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        internal bool Save(ref T_HR_ATTENDANCESOLUTION entAttSol, out ObservableCollection<T_HR_ATTENDANCESOLUTIONDEDUCT> entListTemp)
        {
            bool flag = false;

            entListTemp = new ObservableCollection<T_HR_ATTENDANCESOLUTIONDEDUCT>();

            if (dgAttendanceDeductlist.ItemsSource == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "ATTENDANCEUNUSUALDEDUCT"));
                return false;
            }

            entAttendanceDeductMasters = dgAttendanceDeductlist.ItemsSource as ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER>;
            entListTemp.Clear();

            CheckSubmitForm(ref entAttSol);

            for (int i = 0; i < entAttendanceDeductMasters.Count; i++)
            {
                T_HR_ATTENDANCESOLUTIONDEDUCT entTemp = new T_HR_ATTENDANCESOLUTIONDEDUCT();
                entTemp.SOLUTIONDEDUCTID = System.Guid.NewGuid().ToString();
                entTemp.T_HR_ATTENDANCEDEDUCTMASTER = entAttendanceDeductMasters[i] as T_HR_ATTENDANCEDEDUCTMASTER;
                entTemp.T_HR_ATTENDANCESOLUTION = entAttSol;

                entTemp.CREATEDATE = DateTime.Now;
                entTemp.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                entTemp.UPDATEDATE = System.DateTime.Now;
                entTemp.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                entListTemp.Add(entTemp);
            }

            if (entListTemp.Count > 0)
            {
                flag = true;
            }

            return flag;
        }

        #endregion

        #region 事件
        /// <summary>
        /// 获取考勤方案对应的考勤异常配置信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendanceSolutionDeductRdListByAttSolIDCompleted(object sender, GetAttendanceSolutionDeductRdListByAttSolIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entAttendanceSolutionDeducts = e.Result;

                if (entAttendanceSolutionDeducts == null)
                {
                    entAttendanceSolutionDeducts = new ObservableCollection<T_HR_ATTENDANCESOLUTIONDEDUCT>();
                }

                if (entAttendanceSolutionDeducts.Count > 0)
                {
                    entAttendanceSolution = new T_HR_ATTENDANCESOLUTION();

                    entAttendanceSolution = entAttendanceSolutionDeducts[0].T_HR_ATTENDANCESOLUTION;
                    nudLostCardTimes.Value = double.Parse(entAttendanceSolution.ALLOWLOSTCARDTIMES.Value.ToString());
                    nudLateMaxMinute.Value = double.Parse(entAttendanceSolution.ALLOWLATEMAXMINUTE.Value.ToString());
                    nudLateMaxTimes.Value = double.Parse(entAttendanceSolution.ALLOWLATEMAXTIMES.Value.ToString());

                    entAttendanceDeductMasters = new ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER>();
                    foreach (T_HR_ATTENDANCESOLUTIONDEDUCT item in entAttendanceSolutionDeducts)
                    {
                        entAttendanceDeductMasters.Add(item.T_HR_ATTENDANCEDEDUCTMASTER);
                    }

                    if (entAttendanceDeductMasters.Count > 0)
                    {
                        dgAttendanceDeductlist.ItemsSource = entAttendanceDeductMasters;
                    }
                }

                this.DataContext = entAttendanceSolutionDeducts;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 添加考勤异常扣款设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            addNewDeduct();
        }

        /// <summary>
        /// 删除指定的考勤异常扣款设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgAttendanceDeductlist.SelectedItems == null)
            {
                return;
            }

            //ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_ATTENDANCEDEDUCTMASTER> entList = Utility.Clone<ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_ATTENDANCEDEDUCTMASTER>>(dgAttendanceDeductlist.ItemsSource as ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_ATTENDANCEDEDUCTMASTER>);
            ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER> entList = dgAttendanceDeductlist.ItemsSource as ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER>;

            ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER> entTemps = new ObservableCollection<T_HR_ATTENDANCEDEDUCTMASTER>();
            for (int i = 0; i < dgAttendanceDeductlist.SelectedItems.Count; i++)
            {
                entTemps.Add(dgAttendanceDeductlist.SelectedItems[i] as T_HR_ATTENDANCEDEDUCTMASTER);
            }

            int iSel = entTemps.Count;

            for (int i = 0; i < iSel; i++)
            {
                T_HR_ATTENDANCEDEDUCTMASTER entTemp = entTemps[i] as T_HR_ATTENDANCEDEDUCTMASTER;

                for (int j = 0; j < entList.Count; j++)
                {
                    if (entList[j].DEDUCTMASTERID == entTemp.DEDUCTMASTERID)
                    {
                        entList.RemoveAt(j);
                    }
                }
            }

            dgAttendanceDeductlist.ItemsSource = entList;

        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (dgAttendanceDeductlist.SelectedItems == null)
            {
                return;
            }

            if (dgAttendanceDeductlist.SelectedItems.Count == 0)
            {
                return;
            }

            T_HR_ATTENDANCEDEDUCTMASTER tmpEnt = dgAttendanceDeductlist.SelectedItems[0] as T_HR_ATTENDANCEDEDUCTMASTER;

            string strAttendanceDeductMasterId = tmpEnt.DEDUCTMASTERID.ToString();
            AttendanceDeductMasterForm formAttDedMas = new AttendanceDeductMasterForm(FormTypes.Edit, strAttendanceDeductMasterId);
            EntityBrowser entBrowser = new EntityBrowser(formAttDedMas);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion
    }
}
