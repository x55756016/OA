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
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Text;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttSolRdLeaveSet : BaseForm
    {
        #region 全局变量
        private T_HR_ATTENDANCESOLUTION entAttendanceSolution { get; set; }
        private ObservableCollection<T_HR_LEAVETYPESET> entLeaveTypeSets { get; set; }
        private ObservableCollection<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        #endregion

        #region 初始化
        public AttSolRdLeaveSet()
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

            clientAtt.GetAttendanceSolutionByIDCompleted += new EventHandler<GetAttendanceSolutionByIDCompletedEventArgs>(clientAtt_GetAttendanceSolutionByIDCompleted);
            clientAtt.GetLeaveTypeSetRdListForAttendanceSolutionCompleted += new EventHandler<GetLeaveTypeSetRdListForAttendanceSolutionCompletedEventArgs>(clientAtt_GetLeaveTypeSetRdListForAttendanceSolutionCompleted);
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
            entAttendanceSolution = new T_HR_ATTENDANCESOLUTION();
            entAttendanceSolution.ISEXPIRED = "0";
            cbIsExpired.IsChecked = false;

            this.DataContext = entAttendanceSolution;
        }

        private void LoadData(string strAttendanceSolID)
        {
            if (string.IsNullOrEmpty(strAttendanceSolID))
            {
                return;
            }

            string strSortKey = " LEAVETYPESETID ";

            clientAtt.GetAttendanceSolutionByIDAsync(strAttendanceSolID);
            clientAtt.GetLeaveTypeSetRdListForAttendanceSolutionAsync(strAttendanceSolID, strSortKey);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 隐藏当前页不需要使用的吃GridToolBar按钮
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
        /// 控制调休是否填写有效时长
        /// </summary>
        /// <param name="cbxk"></param>
        private void IsEnabledAdjustExpiredValue(CheckBox cbx)
        {
            Visibility vib = Visibility.Collapsed;

            if (cbx.IsChecked.Value == true)
            {
                vib = Visibility.Visible;
            }

            tbAdjustExpiredValueTitle.Visibility = vib;
            nudAdjustExpiredValue.Visibility = vib;
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

            decimal dAdjustExpiredValue = 0;

            if (cbxkAnnualLeaveSet.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ANNUALLEAVESET"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ANNUALLEAVESET")));
                //entAttSol = null;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkAnnualLeaveSet.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ANNUALLEAVESET"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ANNUALLEAVESET")));
                    entAttSol = null;
                    return;
                }

                entAttSol.ANNUALLEAVESET = entDic.DICTIONARYVALUE.Value.ToString();
            }

            if (dgLeaveSetlist.ItemsSource == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVESETFORM"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("LEAVESETFORM")));
                return; 
            }
            //注释原因：在加班单终审时没有进行加1判断
            //entAttendanceSolution.ISEXPIRED = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            entAttendanceSolution.ISEXPIRED = (Convert.ToInt32(IsChecked.No)).ToString();
            entAttSol.ISEXPIRED = (Convert.ToInt32(IsChecked.No)).ToString();
            if (cbIsExpired.IsChecked.Value == true)
            {
                //entAttendanceSolution.ISEXPIRED = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
                //entAttSol.ISEXPIRED = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
                entAttendanceSolution.ISEXPIRED = (Convert.ToInt32(IsChecked.Yes)).ToString();
                entAttSol.ISEXPIRED = (Convert.ToInt32(IsChecked.Yes)).ToString();
                decimal.TryParse(nudAdjustExpiredValue.Value.ToString(), out dAdjustExpiredValue);
                if (dAdjustExpiredValue <= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ADJUSTEXPIREDVALUE"), string.Format(Utility.GetResourceStr("DATECOMPARE"), Utility.GetResourceStr("ADJUSTEXPIREDVALUE"), "0"));
                    entAttSol = null;
                    return;
                }
            }

            entAttSol.ADJUSTEXPIREDVALUE = dAdjustExpiredValue;
        }

        public bool Save(ref T_HR_ATTENDANCESOLUTION entAttSol, out ObservableCollection<T_HR_ATTENDFREELEAVE> entListTemp)
        {
            bool flag = false;
            entListTemp = new ObservableCollection<T_HR_ATTENDFREELEAVE>();

            CheckSubmitForm(ref entAttSol);

            if (entAttSol == null)
            {
                return false;
            }

            if (dgLeaveSetlist.ItemsSource == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "ATTENDANCEUNUSUALDEDUCT"));
                return false;
            }

            entLeaveTypeSets = dgLeaveSetlist.ItemsSource as ObservableCollection<T_HR_LEAVETYPESET>;
            entListTemp.Clear();


            for (int i = 0; i < entLeaveTypeSets.Count; i++)
            {
                T_HR_ATTENDFREELEAVE entTemp = new T_HR_ATTENDFREELEAVE();
                entTemp.ATTENDFREELEAVEID = System.Guid.NewGuid().ToString();
                entTemp.T_HR_LEAVETYPESET = entLeaveTypeSets[i] as T_HR_LEAVETYPESET;
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

                cbIsExpired.IsChecked = false;
                if (entAttendanceSolution.ISEXPIRED == (Convert.ToInt32(IsChecked.Yes)).ToString())
                {
                    cbIsExpired.IsChecked = true;
                    tbAdjustExpiredValueTitle.Visibility = Visibility.Visible;
                    nudAdjustExpiredValue.Visibility = Visibility.Visible;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 根据带薪假主键索引获取带薪假及其对应请假类型设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetLeaveTypeSetRdListForAttendanceSolutionCompleted(object sender, GetLeaveTypeSetRdListForAttendanceSolutionCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entLeaveTypeSets = e.Result;

                dgLeaveSetlist.ItemsSource = entLeaveTypeSets;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        private void cbIsExpired_Click(object sender, RoutedEventArgs e)
        {
            IsEnabledAdjustExpiredValue(cbIsExpired);
        }

        //private void lkLeaveTypeSet_FindClick(object sender, EventArgs e)
        //{
        //    Dictionary<string, string> cols = new Dictionary<string, string>();
        //    cols.Add("VACATIONNAME", "LEAVETYPENAME");
        //    cols.Add("FINETYPE", "FINETYPE");
        //    cols.Add("LEAVEMAXDAYS", "MAXDAYS");
        //    LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.LeaveTypeSet,
        //        typeof(T_HR_LEAVETYPESET[]), cols);

        //    lookup.SelectedClick += (o, ev) =>
        //    {
        //        T_HR_LEAVETYPESET ent = lookup.SelectedObj as T_HR_LEAVETYPESET;

        //        if (ent != null)
        //        {
        //            lkLeaveTypeSet.DataContext = ent;
        //        }
        //    };

        //    lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        //}

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            AddLeaveItemToList();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            if (dgLeaveSetlist.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgLeaveSetlist.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_LEAVETYPESET ent = dgLeaveSetlist.SelectedItems[0] as T_HR_LEAVETYPESET;

            strLeaveTypeSetID = ent.LEAVETYPESETID.ToString();
            LeaveTypeSetForm formLeaveTypeSet = new LeaveTypeSetForm(FormTypes.Browse, strLeaveTypeSetID);
            EntityBrowser entBrowser = new EntityBrowser(formLeaveTypeSet);
            entBrowser.MinWidth = 630;
            entBrowser.MinHeight = 600;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DelLeaveItemFromList();
        }


        /// <summary>
        /// 添加职员信息转存到银行
        /// </summary>
        private void AddLeaveItemToList()
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("VACATIONNAME", "LEAVETYPENAME");
            cols.Add("FINETYPE", "FINETYPE,LEAVEFINETYPE,DICTIONARYCONVERTER");
            cols.Add("LEAVEMAXDAYS", "MAXDAYS");

            StringBuilder strfilter = new StringBuilder();
            ObservableCollection<object> objArgs = new ObservableCollection<object>();

            strfilter.Append(" ISFREELEAVEDAY == @0");
            objArgs.Add((Convert.ToInt32(IsChecked.Yes) + 1).ToString());


            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.LeaveTypeSet,
                typeof(T_HR_LEAVETYPESET[]), cols, strfilter.ToString(), objArgs);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_LEAVETYPESET ent = lookup.SelectedObj as T_HR_LEAVETYPESET;

                if (ent != null)
                {
                    T_HR_LEAVETYPESET entView = ent;
                    ObservableCollection<T_HR_LEAVETYPESET> entList = new ObservableCollection<T_HR_LEAVETYPESET>();
                    if (dgLeaveSetlist.ItemsSource != null)
                    {
                        entList = dgLeaveSetlist.ItemsSource as ObservableCollection<T_HR_LEAVETYPESET>;

                        bool flag = false;
                        flag = IsContainsEntity(entList, entView);

                        if (flag)
                        {
                            return;
                        }
                    }

                    entList.Add(entView);
                    dgLeaveSetlist.ItemsSource = entList;
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
        private bool IsContainsEntity(ObservableCollection<T_HR_LEAVETYPESET> entList, T_HR_LEAVETYPESET entView)
        {
            bool flag = false;

            var q = from item in entList
                    where item.LEAVETYPENAME == entView.LEAVETYPENAME
                    select item;

            if (q.Count() == 0)
            {
                return flag;
            }

            flag = true;

            return flag;
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面(此处并不需要刷新，只是为了完成EntityBrowser的强制性实现方法)
        /// </summary>
        void entBrowser_ReloadDataEvent()
        {
            if (dgLeaveSetlist.ItemsSource == null)
            {
                return;
            }
        }

        /// <summary>
        /// 删除Grid中的指定项
        /// </summary>
        private void DelLeaveItemFromList()
        { 
            if (dgLeaveSetlist.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgLeaveSetlist.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            //ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_FREELEAVEDAYSET> entList = Utility.Clone<ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_FREELEAVEDAYSET>>(dgLeaveSetlist.ItemsSource as ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_FREELEAVEDAYSET>);
            ObservableCollection<T_HR_LEAVETYPESET> entList = dgLeaveSetlist.ItemsSource as ObservableCollection<T_HR_LEAVETYPESET>;

            ObservableCollection<T_HR_LEAVETYPESET> entTemps = new ObservableCollection<T_HR_LEAVETYPESET>();
            for (int i = 0; i < dgLeaveSetlist.SelectedItems.Count; i++)
            {
                entTemps.Add(dgLeaveSetlist.SelectedItems[i] as T_HR_LEAVETYPESET);
            }

            int iSel = entTemps.Count;

            for (int i = 0; i < iSel; i++)
            {
                T_HR_LEAVETYPESET entTemp = entTemps[i] as T_HR_LEAVETYPESET;

                for (int j = 0; j < entList.Count; j++)
                {
                    if (entList[j].LEAVETYPESETID == entTemp.LEAVETYPESETID)
                    {
                        entList.RemoveAt(j);
                    }
                }
            }

            dgLeaveSetlist.ItemsSource = entList;
        }
        #endregion

        private void dgAskOffList_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

    }
}
