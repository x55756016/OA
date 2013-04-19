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
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.AttendanceWS;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttendanceDeductDetailForm : BaseForm, IEntityEditor
    {
        #region 全局变量
        public FormTypes FormType { get; set; }

        public string DeductDetailID { get; set; }

        public T_HR_ATTENDANCEDEDUCTMASTER AttendanceDeductMaster { get; set; }

        public T_HR_ATTENDANCEDEDUCTDETAIL AttendanceDeductDetail { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private string strResMsg = string.Empty;

        private decimal dmaxTimes = 999999999999;
        #endregion

        #region 初始化
        public AttendanceDeductDetailForm(FormTypes formtype, string strDeductDetailID, T_HR_ATTENDANCEDEDUCTMASTER entAttendanceDeductMaster)
        {
            FormType = formtype;
            DeductDetailID = strDeductDetailID;
            AttendanceDeductMaster = entAttendanceDeductMaster;
            InitializeComponent();
            RegisterEvents();

            this.Loaded += new RoutedEventHandler(AttendanceDeductDetailForm_Loaded);
        }        
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("ATTENDANCEDEDUCTDETAILFORM");
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
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetAttendanceDeductDetailByIDCompleted += new EventHandler<GetAttendanceDeductDetailByIDCompletedEventArgs>(clientAtt_GetAttendanceDeductDetailByIDCompleted);
            clientAtt.AddAttendanceDeductDetailCompleted += new EventHandler<AddAttendanceDeductDetailCompletedEventArgs>(clientAtt_AddAttendanceDeductDetailCompleted);
            clientAtt.ModifyAttendanceDeductDetailCompleted += new EventHandler<ModifyAttendanceDeductDetailCompletedEventArgs>(clientAtt_ModifyAttendanceDeductDetailCompleted);
        }

        /// <summary>
        /// 页面初始化
        /// </summary>
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
                cbxkAttType.IsEnabled = false;
                if (FormType == FormTypes.Browse)
                {
                    this.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// 控制显示的按钮
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
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 表单初始化
        /// </summary>
        private void InitForm()
        {
            AttendanceDeductDetail = new T_HR_ATTENDANCEDEDUCTDETAIL();
            AttendanceDeductDetail.DEDUCTDETAILID = System.Guid.NewGuid().ToString().ToUpper();
            if (AttendanceDeductMaster == null)
            {
                return;
            }
            AttendanceDeductDetail.T_HR_ATTENDANCEDEDUCTMASTER = AttendanceDeductMaster;
            AttendanceDeductDetail.FINETYPE = AttendanceDeductMaster.FINETYPE;

            InitMasterInfo();
            InitDetailInfo();

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            AttendanceDeductDetail.CREATEDATE = DateTime.Now;
            AttendanceDeductDetail.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            AttendanceDeductDetail.UPDATEDATE = System.DateTime.Now;
            AttendanceDeductDetail.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;


            this.DataContext = AttendanceDeductDetail;
        }

        /// <summary>
        /// 显示关联主表信息
        /// </summary>
        private void InitMasterInfo()
        {
            if (cbxkAttType.Items.Count > 0)
            {
                foreach (var item in cbxkAttType.Items)
                {
                    T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                    if (dict != null)
                    {
                        if (dict.DICTIONARYVALUE.ToString() == AttendanceDeductMaster.ATTENDABNORMALTYPE)
                        {
                            cbxkAttType.SelectedItem = item;
                            break;
                        }
                    }
                }
            }

            cbxIsAccumulating.IsChecked = false;
            if (AttendanceDeductMaster.ISACCUMULATING == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
            {
                cbxIsAccumulating.IsChecked = true;
            }

            cbxIsPerfectAttendanceFactor.IsChecked = false;
            if (AttendanceDeductMaster.ISPERFECTATTENDANCEFACTOR == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
            {
                cbxIsPerfectAttendanceFactor.IsChecked = true;
            }
        }

        private void InitDetailInfo()
        {
            string strAttType = string.Empty;
            string strFineType = string.Empty;

            strAttType = AttendanceDeductDetail.T_HR_ATTENDANCEDEDUCTMASTER.ATTENDABNORMALTYPE;
            strFineType = AttendanceDeductDetail.FINETYPE;

            if (string.IsNullOrEmpty(strFineType))
            {
                spFinetype1.Visibility = Visibility.Collapsed;
                spFinetype2.Visibility = Visibility.Collapsed;
                spFinetype3.Visibility = Visibility.Collapsed;
                spFinetype4.Visibility = Visibility.Collapsed;
                return;
            }            

            if (strFineType == "1" || strFineType == "5")
            {
                spFinetype1.Visibility = Visibility.Visible;
                spFinetype2.Visibility = Visibility.Collapsed;
                spFinetype3.Visibility = Visibility.Collapsed;
                spFinetype4.Visibility = Visibility.Collapsed;

                nudFineSum.Value = double.Parse(AttendanceDeductDetail.FINESUM == null ? "0" : AttendanceDeductDetail.FINESUM.ToString());
            }
            
            if (strFineType == "2" || strFineType == "6")
            {
                spFinetype1.Visibility = Visibility.Collapsed;
                spFinetype2.Visibility = Visibility.Visible;
                spFinetype3.Visibility = Visibility.Collapsed;
                spFinetype4.Visibility = Visibility.Collapsed;

                nudParameterValue.Value = double.Parse(AttendanceDeductDetail.PARAMETERVALUE == null ? "0" : AttendanceDeductDetail.PARAMETERVALUE.ToString());
            }

            if (strFineType == "3" || strFineType == "7" || strFineType == "4" || strFineType == "8" || strFineType == "9")
            {
                spFinetype1.Visibility = Visibility.Collapsed;
                spFinetype2.Visibility = Visibility.Collapsed;
                spFinetype3.Visibility = Visibility.Visible;
                spFinetype4.Visibility = Visibility.Collapsed;


                nudLimitTimes31.Value = double.Parse(AttendanceDeductDetail.LOWESTTIMES == null ? "0" : AttendanceDeductDetail.LOWESTTIMES.ToString());
                nudLimitTimes32.Value = double.Parse(AttendanceDeductDetail.HIGHESTTIMES == null ? "0" : AttendanceDeductDetail.HIGHESTTIMES.ToString());

                nudFineSum3.Value = double.Parse(AttendanceDeductDetail.FINESUM == null ? "0" : AttendanceDeductDetail.FINESUM.ToString());

                if (strAttType == (Convert.ToInt32(AttendAbnormalType.Late) + 1).ToString())
                {
                    tbAttTypeTimesTitle.Text = Utility.GetResourceStr("LATETIMES");
                }
                else if (strAttType == (Convert.ToInt32(AttendAbnormalType.LeaveEarly) + 1).ToString())
                {
                    tbAttTypeTimesTitle.Text = Utility.GetResourceStr("LEAVEEARLYTIMES");
                }
                else if (strAttType == (Convert.ToInt32(AttendAbnormalType.DrainPunch) + 1).ToString())
                {
                    tbAttTypeTimesTitle.Text = Utility.GetResourceStr("FORGETCARDTIMES");
                }
            }

            if (strFineType == "10")
            {
                spFinetype1.Visibility = Visibility.Collapsed;
                spFinetype2.Visibility = Visibility.Collapsed;
                spFinetype3.Visibility = Visibility.Collapsed;
                spFinetype4.Visibility = Visibility.Visible;

                nudFineRatio.Value = double.Parse(AttendanceDeductDetail.FINERATIO == null ? "0" : AttendanceDeductDetail.FINERATIO.ToString());

            }
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(DeductDetailID))
            {
                return;
            }

            clientAtt.GetAttendanceDeductDetailByIDAsync(DeductDetailID);
        }

        /// <summary>
        /// 加载考勤异常扣款方式下拉菜单
        /// </summary>
        private void BindFineType()
        {
            string strCategory = "ATTEXFINETYPE";
            string strFatherID = ((T_SYS_DICTIONARY)cbxkAttType.SelectedItem).DICTIONARYID.ToString();
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var objs = from d in dicts
                       where d.DICTIONCATEGORY == strCategory && d.T_SYS_DICTIONARY2.DICTIONARYID == strFatherID
                       orderby d.DICTIONARYVALUE
                       select d;
            cbxkFineType.ItemsSource = objs.ToList();
            cbxkFineType.DisplayMemberPath = "DICTIONARYNAME";

            SetSelectItemByFineType();
        }

        /// <summary>
        /// 根据考勤异常扣款实体中FineType的值设定ComboBox选中项
        /// </summary>
        private void SetSelectItemByFineType()
        {
            if (cbxkFineType.Items.Count > 0)
            {
                foreach (var item in cbxkFineType.Items)
                {
                    T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                    if (dict != null)
                    {
                        if (dict.DICTIONARYVALUE.ToString() == AttendanceDeductMaster.FINETYPE)
                        {
                            cbxkFineType.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 效验提交的表单数据
        /// </summary>
        /// <param name="flag"></param>
        private void CheckSubmitForm(out bool flag)
        {
            flag = false;

            //固定扣款(针对迟到，早退)
            if (spFinetype1.Visibility == Visibility.Visible)
            {
                decimal dFineSum = 0;
                decimal.TryParse(nudFineSum.Value.ToString(), out dFineSum);

                if (dFineSum <= 0)
                {
                    flag = false;
                    return;
                }

                AttendanceDeductDetail.FINESUM = dFineSum;

                flag = true;
            }

            //最低扣款(针对迟到，早退)
            if (spFinetype2.Visibility == Visibility.Visible)
            {
                decimal dParameterValue = 0;
                decimal.TryParse(nudParameterValue.Value.ToString(), out dParameterValue);

                if (dParameterValue <= 0)
                {
                    flag = false;
                    return;
                }

                AttendanceDeductDetail.PARAMETERVALUE = dParameterValue;
                flag = true;
            }

            //分段扣款(针对迟到，早退，漏打卡)
            if (spFinetype3.Visibility == Visibility.Visible)
            {
                decimal dLimitTimes1 = 0, dLimitTimes2 = 0, dFineSum3 = 0;
                decimal.TryParse(nudLimitTimes31.Value.ToString(), out dLimitTimes1);
                decimal.TryParse(nudLimitTimes32.Value.ToString(), out dLimitTimes2);
                decimal.TryParse(nudFineSum3.Value.ToString(), out dFineSum3);

                if (dLimitTimes1 >= dLimitTimes2)
                {
                    flag = false;
                    return;
                }

                AttendanceDeductDetail.LOWESTTIMES = dLimitTimes1;
                AttendanceDeductDetail.HIGHESTTIMES = dLimitTimes2;
                AttendanceDeductDetail.FINESUM = dFineSum3;

                flag = true;
            }

            //按倍率扣款(针对旷工)
            if (spFinetype4.Visibility == Visibility.Visible)
            {
                decimal dFineRatio = 0;
                decimal.TryParse(nudFineRatio.Value.ToString(), out dFineRatio);

                if (dFineRatio <= 0)
                {
                    flag = false;
                    return;
                }

                AttendanceDeductDetail.FINERATIO = dFineRatio;
                flag = true;
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

                CheckSubmitForm(out flag);

                if (!flag)
                {
                    return false;
                }

                if (FormType == FormTypes.New)
                {
                    clientAtt.AddAttendanceDeductDetailAsync(AttendanceDeductDetail);
                }
                else
                {
                    clientAtt.ModifyAttendanceDeductDetailAsync(AttendanceDeductDetail);
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
        /// Form加载完毕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AttendanceDeductDetailForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
        }

        /// <summary>
        /// 根据主键索引，获得指定的考勤异常扣款明细记录以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendanceDeductDetailByIDCompleted(object sender, GetAttendanceDeductDetailByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                AttendanceDeductDetail = e.Result;
                this.DataContext = AttendanceDeductDetail;
                AttendanceDeductMaster = AttendanceDeductDetail.T_HR_ATTENDANCEDEDUCTMASTER;

                if (AttendanceDeductMaster != null)
                {
                    InitMasterInfo();
                    InitDetailInfo();
                }

                SetToolBar();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 新增考勤异常扣款明细记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddAttendanceDeductDetailCompleted(object sender, AddAttendanceDeductDetailCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
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
        /// 更新考勤异常扣款明细记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifyAttendanceDeductDetailCompleted(object sender, ModifyAttendanceDeductDetailCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "OVERTIMEREWARDSET")));
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
        /// 考勤异常类型选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxkAttType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindFineType();
        }
        #endregion
    }
}
