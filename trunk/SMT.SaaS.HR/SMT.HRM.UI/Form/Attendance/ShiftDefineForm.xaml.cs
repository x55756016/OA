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
    public partial class ShiftDefineForm : BaseForm, IEntityEditor
    {
        #region 全局变量
        public FormTypes FormType { get; set; }

        public string ShiftDefineID { get; set; }

        public T_HR_SHIFTDEFINE entShiftDefine { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private string strResMsg = string.Empty;
        #endregion

        #region 初始化
        public ShiftDefineForm(FormTypes formtype, string strShiftDefineID)
        {
            FormType = formtype;
            ShiftDefineID = strShiftDefineID;
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ShiftDefineForm_Loaded);
        }

        void ShiftDefineForm_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            InitParas();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetShiftDefineByIDCompleted += new EventHandler<GetShiftDefineByIDCompletedEventArgs>(clientAtt_GetShiftDefineByIDCompleted);
            clientAtt.AddShiftDefineCompleted += new EventHandler<AddShiftDefineCompletedEventArgs>(clientAtt_AddShiftDefineCompleted);
            clientAtt.ModifyShiftDefineCompleted += new EventHandler<ModifyShiftDefineCompletedEventArgs>(clientAtt_ModifyShiftDefineCompleted);
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
                if (FormType == FormTypes.Browse)
                {
                    this.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// 表单初始化
        /// </summary>
        private void InitForm()
        {
            entShiftDefine = new T_HR_SHIFTDEFINE();
            entShiftDefine.SHIFTDEFINEID = System.Guid.NewGuid().ToString().ToUpper();

            //权限控制
            entShiftDefine.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entShiftDefine.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entShiftDefine.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entShiftDefine.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            entShiftDefine.CREATEDATE = DateTime.Now;
            entShiftDefine.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entShiftDefine.UPDATEDATE = System.DateTime.Now;
            entShiftDefine.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据
            entShiftDefine.WORKTIME = decimal.Parse("0");
            entShiftDefine.NEEDFIRSTCARD = "0";
            entShiftDefine.NEEDSECONDCARD = "0";
            entShiftDefine.NEEDTHIRDCARD = "0";
            entShiftDefine.NEEDFOURTHCARD = "0";

            entShiftDefine.NEEDFIRSTOFFCARD = "0";
            entShiftDefine.NEEDSECONDOFFCARD = "0";
            entShiftDefine.NEEDTHIRDOFFCARD = "0";
            entShiftDefine.NEEDFOURTHOFFCARD = "0";

            IsNeedCard(entShiftDefine.NEEDFIRSTCARD, cbNeedFirstCard, nudFirstCardStartTime, nudFirstCardEndTime);
            IsNeedCard(entShiftDefine.NEEDSECONDCARD, cbNeedSecondCard, nudSecCardStartTime, nudSecCardEndTime);
            IsNeedCard(entShiftDefine.NEEDTHIRDCARD, cbNeedThirdCard, nudThirdCardStartTime, nudThirdCardEndTime);
            IsNeedCard(entShiftDefine.NEEDFOURTHCARD, cbNeedFourthCard, nudFourthCardStartTime, nudFourthCardEndTime);

            IsNeedCard(entShiftDefine.NEEDFIRSTOFFCARD, cbNeedFirstOffCard, nudFirstOffCardStartTime, nudFirstOffCardEndTime);
            IsNeedCard(entShiftDefine.NEEDSECONDOFFCARD, cbNeedSecondOffCard, nudSecOffCardStartTime, nudSecOffCardEndTime);
            IsNeedCard(entShiftDefine.NEEDTHIRDOFFCARD, cbNeedThirdOffCard, nudThirdOffCardStartTime, nudThirdOffCardEndTime);
            IsNeedCard(entShiftDefine.NEEDFOURTHOFFCARD, cbNeedFourthOffCard, nudFourthOffCardStartTime, nudFourthOffCardEndTime);

            this.DataContext = entShiftDefine;
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(ShiftDefineID))
            {
                return;
            }

            clientAtt.GetShiftDefineByIDAsync(ShiftDefineID);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("SHIFTDEFINEFORM");
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
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 控制打卡时间是否可填
        /// </summary>
        /// <param name="nudCardStartTime"></param>
        /// <param name="nudCardEndTime"></param>
        /// <param name="cb"></param>
        private void IsEnabledCardTime(TimePicker nudCardStartTime, TimePicker nudCardEndTime, CheckBox cbNeedCard)
        {
            nudCardStartTime.Value = null;
            nudCardEndTime.Value = null;
            nudCardStartTime.IsEnabled = false;
            nudCardEndTime.IsEnabled = false;

            if (cbNeedCard.IsChecked.Value == true)
            {
                nudCardStartTime.IsEnabled = true;
                nudCardEndTime.IsEnabled = true;
            }
        }

        /// <summary>
        /// 检查当前的CheckBox是否需要勾选
        /// </summary>
        /// <param name="strFlag"></param>
        private void IsNeedCard(string strNeedCardFlag, CheckBox cbNeedCard, TimePicker nudCardStartTime, TimePicker nudCardEndTime)
        {
            cbNeedCard.IsChecked = false;
            nudCardStartTime.IsEnabled = false;
            nudCardEndTime.IsEnabled = false;
            if (strNeedCardFlag == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
            {
                cbNeedCard.IsChecked = true;
                nudCardStartTime.IsEnabled = true;
                nudCardEndTime.IsEnabled = true;
            }
        }

        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entShiftDefine"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = false;
            decimal dWorkTime = 0;

            if (string.IsNullOrEmpty(txtShiftName.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SHIFTNAME"), Utility.GetResourceStr("REQUIRED", "SHIFTNAME"));
                flag = false;
                return;
            }
            else
            {
                flag = true;
            }

            flag = decimal.TryParse(nudWorkTime.Value.ToString(), out dWorkTime);
            if (!flag)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKTIME"), Utility.GetResourceStr("REQUIREDNUMERICAL", "WORKTIME"));
                flag = false;
                return;
            }

            CheckWorkTime(ref flag);

            if (!flag)
            {
                return;
            }

            CheckCardTime(ref flag);

            if (FormType == FormTypes.Edit)
            {
                entShiftDefine.UPDATEDATE = DateTime.Now;
                entShiftDefine.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            }
        }

        /// <summary>
        /// 效验班次定义的时间
        /// </summary>
        /// <param name="flag"></param>
        private void CheckWorkTime(ref bool flag)
        {
            DateTime dtFirstStart = new DateTime();
            DateTime dtSecondStart = new DateTime();
            DateTime dtThirdStart = new DateTime();
            DateTime dtFourthStart = new DateTime();
            DateTime dtFirstEnd = new DateTime();
            DateTime dtSecondEnd = new DateTime();
            DateTime dtThirdEnd = new DateTime();
            DateTime dtFourthEnd = new DateTime();
            decimal dTotalTime = 0, dRealTotalTime = 0;

            decimal.TryParse(nudWorkTime.Value.ToString(), out dTotalTime);
            dTotalTime = dTotalTime * 60;

            #region 第一段上下班时间验证
            if (nudFirstStartTime.Value == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFIRSTTIME") + Utility.GetResourceStr("WORKSTARTTIME")));
                flag = false;
                return;
            }
            else
            {
                dtFirstStart = nudFirstStartTime.Value.Value;
                DateTime.TryParse("1900-1-1 " + dtFirstStart.ToString("HH:mm"), out dtFirstStart);
                flag = true;
            }

            if (nudFirstEndTime.Value == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFIRSTTIME") + Utility.GetResourceStr("WORKENDTIME")));
                flag = false;
                return;
            }
            else
            {
                dtFirstEnd = nudFirstEndTime.Value.Value;
                DateTime.TryParse("1900-1-1 " + dtFirstEnd.ToString("HH:mm"), out dtFirstEnd);
                flag = true;
            }

            if (dtFirstStart > dtFirstEnd)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKENDTIME"), Utility.GetResourceStr("WORKFIRSTTIME") + Utility.GetResourceStr("DATECOMPARE", "WORKENDTIME,WORKSTARTTIME"));
                flag = false;
                return;
            }
            else
            {
                TimeSpan ts = dtFirstEnd.Subtract(dtFirstStart);
                dRealTotalTime = ts.Hours * 60 + ts.Minutes;
                flag = true;
            }
            #endregion

            if (!flag)
            {
                return;
            }

            #region 第二段上下班时间验证

            if (nudSecStartTime.Value != null)
            {
                dtSecondStart = nudSecStartTime.Value.Value;
                DateTime.TryParse("1900-1-1 " + dtSecondStart.ToString("HH:mm"), out dtSecondStart);
                if (dtSecondStart < dtFirstEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKSTARTTIME"), Utility.GetResourceStr("DATECOMPARE", Utility.GetResourceStr("WORKSECTIME") + Utility.GetResourceStr("WORKSTARTTIME") + "," + Utility.GetResourceStr("WORKFIRSTTIME") + Utility.GetResourceStr("WORKENDTIME")));
                    flag = false;
                    return;
                }

                flag = true;
            }

            if (!flag)
            {
                return;
            }

            if (nudSecStartTime.Value != null)
            {
                if (nudSecEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKSECTIME") + Utility.GetResourceStr("WORKENDTIME")));
                    flag = false;
                    return;
                }

                dtSecondEnd = nudSecEndTime.Value.Value;
                DateTime.TryParse("1900-1-1 " + dtSecondEnd.ToString("HH:mm"), out dtSecondEnd);
                if (dtSecondStart > dtSecondEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKENDTIME"), Utility.GetResourceStr("WORKSECTIME") + Utility.GetResourceStr("DATECOMPARE", Utility.GetResourceStr("WORKENDTIME,WORKSTARTTIME")));
                    flag = false;
                    return;
                }

                TimeSpan ts = dtSecondEnd.Subtract(dtSecondStart);
                dRealTotalTime += ts.Hours * 60 + ts.Minutes;
                flag = true;

            }
            else
            {
                if (nudSecEndTime.Value != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKSECTIME") + Utility.GetResourceStr("WORKSTARTTIME")));
                    flag = false;
                    return;
                }
            }

            #endregion

            if (!flag)
            {
                return;
            }

            #region 第三段上下班时间验证

            if (nudThirdStartTime.Value != null)
            {
                dtThirdStart = nudThirdStartTime.Value.Value;
                DateTime.TryParse("1900-1-1 " + dtThirdStart.ToString("HH:mm"), out dtThirdStart);
                if (dtThirdStart < dtSecondEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKSTARTTIME"), Utility.GetResourceStr("DATECOMPARE", Utility.GetResourceStr("WORKTHIRDTIME") + Utility.GetResourceStr("WORKSTARTTIME") + "," + Utility.GetResourceStr("WORKSECTIME") + Utility.GetResourceStr("WORKENDTIME")));
                    flag = false;
                    return;
                }

                flag = true;
            }


            if (!flag)
            {
                return;
            }


            if (nudThirdStartTime.Value != null)
            {
                if (nudThirdEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKTHIRDTIME") + Utility.GetResourceStr("WORKENDTIME")));
                    flag = false;
                    return;
                }

                dtThirdEnd = nudThirdEndTime.Value.Value;
                DateTime.TryParse("1900-1-1 " + dtThirdEnd.ToString("HH:mm"), out dtThirdEnd);
                if (dtThirdStart > dtThirdEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKENDTIME"), Utility.GetResourceStr("WORKTHIRDTIME") + Utility.GetResourceStr("DATECOMPARE", Utility.GetResourceStr("WORKENDTIME,WORKSTARTTIME")));
                    flag = false;
                    return;
                }

                TimeSpan ts = dtThirdEnd.Subtract(dtThirdStart);
                dRealTotalTime += ts.Hours * 60 + ts.Minutes;
                flag = true;

            }
            else
            {
                if (nudThirdEndTime.Value != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKTHIRDTIME") + Utility.GetResourceStr("WORKSTARTTIME")));
                    flag = false;
                    return;
                }
            }

            #endregion

            if (!flag)
            {
                return;
            }

            #region 第四段上下班时间验证

            if (nudFourthStartTime.Value != null)
            {
                dtFourthStart = nudFourthStartTime.Value.Value;
                DateTime.TryParse("1900-1-1 " + dtFourthStart.ToString("HH:mm"), out dtFourthStart);
                if (dtFourthStart < dtThirdEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKSTARTTIME"), Utility.GetResourceStr("DATECOMPARE", Utility.GetResourceStr("WORKFOURTHTIME") + Utility.GetResourceStr("WORKSTARTTIME") + "," + Utility.GetResourceStr("WORKTHIRDTIME") + Utility.GetResourceStr("WORKENDTIME")));
                    flag = false;
                    return;
                }

                flag = true;
            }


            if (!flag)
            {
                return;
            }

            if (nudFourthStartTime.Value != null)
            {
                if (nudFourthEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFOURTHTIME") + Utility.GetResourceStr("WORKENDTIME")));
                    flag = false;
                    return;
                }

                dtFourthEnd = nudFourthEndTime.Value.Value;
                DateTime.TryParse("1900-1-1 " + dtFourthEnd.ToString("HH:mm"), out dtFourthEnd);
                if (dtFourthStart > dtFourthEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKENDTIME"), Utility.GetResourceStr("WORKFOURTHTIME") + Utility.GetResourceStr("DATECOMPARE", Utility.GetResourceStr("WORKENDTIME,WORKSTARTTIME")));
                    flag = false;
                    return;
                }

                TimeSpan ts = dtFourthEnd.Subtract(dtFourthStart);
                dRealTotalTime += ts.Hours * 60 + ts.Minutes;
                flag = true;

            }
            else
            {
                if (nudFourthEndTime.Value != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFOURTHTIME") + Utility.GetResourceStr("WORKSTARTTIME")));
                    flag = false;
                    return;
                }
            }

            #endregion

            if (!flag)
            {
                return;
            }

            if (dRealTotalTime != dTotalTime)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("WORKTIMEPERDAY"), "设定工作段时长总和必须等于工作时长(小时/天)");
                flag = false;
                return;
            }

        }

        /// <summary>
        /// 效验打卡定义的时间
        /// </summary>
        /// <param name="flag"></param>
        private void CheckCardTime(ref bool flag)
        {
            entShiftDefine.NEEDFIRSTCARD = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbNeedFirstCard.IsChecked.Value == true)
            {
                if (nudFirstCardStartTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFIRSTTIME") + Utility.GetResourceStr("CARDSTARTTIME")));
                    flag = false;
                    return;
                }

                if (nudFirstCardEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFIRSTTIME") + Utility.GetResourceStr("CARDENDTIME")));
                    flag = false;
                    return;
                }

                entShiftDefine.NEEDFIRSTCARD = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            if (!flag)
            {
                return;
            }

            entShiftDefine.NEEDFIRSTOFFCARD = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbNeedFirstOffCard.IsChecked.Value == true)
            {
                if (nudFirstOffCardStartTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFIRSTTIME") + Utility.GetResourceStr("CARDSTARTTIME")));
                    flag = false;
                    return;
                }

                if (nudFirstOffCardEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFIRSTTIME") + Utility.GetResourceStr("CARDENDTIME")));
                    flag = false;
                    return;
                }

                entShiftDefine.NEEDFIRSTOFFCARD = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            if (!flag)
            {
                return;
            }

            entShiftDefine.NEEDSECONDCARD = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbNeedSecondCard.IsChecked.Value == true)
            {
                if (nudSecCardStartTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKSECTIME") + Utility.GetResourceStr("CARDSTARTTIME")));
                    flag = false;
                    return;
                }

                if (nudSecCardEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKSECTIME") + Utility.GetResourceStr("CARDENDTIME")));
                    flag = false;
                    return;
                }

                entShiftDefine.NEEDSECONDCARD = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            if (!flag)
            {
                return;
            }

            entShiftDefine.NEEDSECONDOFFCARD = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbNeedSecondOffCard.IsChecked.Value == true)
            {
                if (nudSecOffCardStartTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKSECTIME") + Utility.GetResourceStr("CARDSTARTTIME")));
                    flag = false;
                    return;
                }

                if (nudSecOffCardEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKSECTIME") + Utility.GetResourceStr("CARDENDTIME")));
                    flag = false;
                    return;
                }

                entShiftDefine.NEEDSECONDOFFCARD = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            if (!flag)
            {
                return;
            }

            entShiftDefine.NEEDTHIRDCARD = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbNeedThirdCard.IsChecked.Value == true)
            {
                if (nudThirdCardStartTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKTHIRDTIME") + Utility.GetResourceStr("CARDSTARTTIME")));
                    flag = false;
                    return;
                }

                if (nudThirdCardEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKTHIRDTIME") + Utility.GetResourceStr("CARDENDTIME")));
                    flag = false;
                    return;
                }

                entShiftDefine.NEEDTHIRDCARD = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            if (!flag)
            {
                return;
            }

            entShiftDefine.NEEDTHIRDOFFCARD = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbNeedThirdOffCard.IsChecked.Value == true)
            {
                if (nudThirdOffCardStartTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKTHIRDTIME") + Utility.GetResourceStr("CARDSTARTTIME")));
                    flag = false;
                    return;
                }

                if (nudThirdOffCardEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKTHIRDTIME") + Utility.GetResourceStr("CARDENDTIME")));
                    flag = false;
                    return;
                }

                entShiftDefine.NEEDTHIRDOFFCARD = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            if (!flag)
            {
                return;
            }

            entShiftDefine.NEEDFOURTHCARD = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbNeedFourthCard.IsChecked.Value == true)
            {
                if (nudFourthCardStartTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFOURTHTIME") + Utility.GetResourceStr("CARDSTARTTIME")));
                    flag = false;
                    return;
                }

                if (nudFourthCardEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFOURTHTIME") + Utility.GetResourceStr("CARDENDTIME")));
                    flag = false;
                    return;
                }

                entShiftDefine.NEEDFOURTHCARD = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            if (!flag)
            {
                return;
            }

            entShiftDefine.NEEDFOURTHOFFCARD = (Convert.ToInt32(IsChecked.No) + 1).ToString();
            if (cbNeedFourthOffCard.IsChecked.Value == true)
            {
                if (nudFourthOffCardStartTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDSTARTTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFOURTHTIME") + Utility.GetResourceStr("CARDSTARTTIME")));
                    flag = false;
                    return;
                }

                if (nudFourthOffCardEndTime.Value == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CARDENDTIME"), Utility.GetResourceStr("REQUIRED", Utility.GetResourceStr("WORKFOURTHTIME") + Utility.GetResourceStr("CARDENDTIME")));
                    flag = false;
                    return;
                }

                entShiftDefine.NEEDFOURTHOFFCARD = (Convert.ToInt32(IsChecked.Yes) + 1).ToString();
            }

            if (!flag)
            {
                return;
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
                    clientAtt.AddShiftDefineAsync(entShiftDefine);
                }
                else
                {
                    clientAtt.ModifyShiftDefineAsync(entShiftDefine);
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
        /// 根据主键索引，获得指定的假期记录以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetShiftDefineByIDCompleted(object sender, GetShiftDefineByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entShiftDefine = e.Result;
                //entShiftDefine.NEEDFIRSTCARD = entShiftDefine.NEEDFIRSTCARD == "2" ? "2" : "1";
                //entShiftDefine.NEEDSECONDCARD = entShiftDefine.NEEDSECONDCARD == "2" ? "2" : "1";
                //entShiftDefine.NEEDTHIRDCARD = entShiftDefine.NEEDTHIRDCARD == "2" ? "2" : "1";
                //entShiftDefine.NEEDFOURTHCARD = entShiftDefine.NEEDFOURTHCARD == "2" ? "2" : "1";

                this.DataContext = entShiftDefine;

                //上班
                IsNeedCard(entShiftDefine.NEEDFIRSTCARD, cbNeedFirstCard, nudFirstCardStartTime, nudFirstCardEndTime);
                IsNeedCard(entShiftDefine.NEEDSECONDCARD, cbNeedSecondCard, nudSecCardStartTime, nudSecCardEndTime);
                IsNeedCard(entShiftDefine.NEEDTHIRDCARD, cbNeedThirdCard, nudThirdCardStartTime, nudThirdCardEndTime);
                IsNeedCard(entShiftDefine.NEEDFOURTHCARD, cbNeedFourthCard, nudFourthCardStartTime, nudFourthCardEndTime);

                //下班
                IsNeedCard(entShiftDefine.NEEDFIRSTOFFCARD, cbNeedFirstOffCard, nudFirstOffCardStartTime, nudFirstOffCardEndTime);
                IsNeedCard(entShiftDefine.NEEDSECONDOFFCARD, cbNeedSecondOffCard, nudSecOffCardStartTime, nudSecOffCardEndTime);
                IsNeedCard(entShiftDefine.NEEDTHIRDOFFCARD, cbNeedThirdOffCard, nudThirdOffCardStartTime, nudThirdOffCardEndTime);
                IsNeedCard(entShiftDefine.NEEDFOURTHOFFCARD, cbNeedFourthOffCard, nudFourthOffCardStartTime, nudFourthOffCardEndTime);

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
        void clientAtt_AddShiftDefineCompleted(object sender, AddShiftDefineCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
                    FormType = FormTypes.Edit;
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
        void clientAtt_ModifyShiftDefineCompleted(object sender, ModifyShiftDefineCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "SHIFTDEFINEFORM")));
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

        #region 控制上班打卡
        /// <summary>
        /// 控制第一段上班打卡时间输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbNeedFirstCard_Click(object sender, RoutedEventArgs e)
        {
            IsEnabledCardTime(nudFirstCardStartTime, nudFirstCardEndTime, cbNeedFirstCard);
        }

        /// <summary>
        /// 控制第二段上班打卡时间输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbNeedSecondCard_Click(object sender, RoutedEventArgs e)
        {
            IsEnabledCardTime(nudSecCardStartTime, nudSecCardEndTime, cbNeedSecondCard);
        }

        /// <summary>
        /// 控制第三段上班打卡时间输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbNeedThirdCard_Click(object sender, RoutedEventArgs e)
        {
            IsEnabledCardTime(nudThirdCardStartTime, nudThirdCardEndTime, cbNeedThirdCard);
        }

        /// <summary>
        /// 控制第四段上班打卡时间输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbNeedFourthCard_Click(object sender, RoutedEventArgs e)
        {
            IsEnabledCardTime(nudFourthCardStartTime, nudFourthCardEndTime, cbNeedFourthCard);
        }
        #endregion

        #region 控制下班打卡
        /// <summary>
        /// 控制第一段下班打卡时间输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbNeedFirstOffCard_Click(object sender, RoutedEventArgs e)
        {
            IsEnabledCardTime(nudFirstOffCardStartTime, nudFirstOffCardEndTime, cbNeedFirstOffCard);
        }

        /// <summary>
        /// 控制第二段下班打卡时间输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbNeedSecondOffCard_Click(object sender, RoutedEventArgs e)
        {
            IsEnabledCardTime(nudSecOffCardStartTime, nudSecOffCardEndTime, cbNeedSecondOffCard);
        }

        /// <summary>
        /// 控制第三段下班打卡时间输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbNeedThirdOffCard_Click(object sender, RoutedEventArgs e)
        {
            IsEnabledCardTime(nudThirdOffCardStartTime, nudThirdOffCardEndTime, cbNeedThirdOffCard);
        }

        /// <summary>
        /// 控制第四段下班打卡时间输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbNeedFourthOffCard_Click(object sender, RoutedEventArgs e)
        {
            IsEnabledCardTime(nudFourthOffCardStartTime, nudFourthOffCardEndTime, cbNeedFourthOffCard);
        }
        #endregion
        #endregion

    }
}

