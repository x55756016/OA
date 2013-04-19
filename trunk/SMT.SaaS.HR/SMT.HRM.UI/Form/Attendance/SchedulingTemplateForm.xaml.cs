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
using System.Collections.ObjectModel;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class SchedulingTemplateForm : BaseForm, IEntityEditor
    {
        #region 全局变量

        public FormTypes FormType { get; set; }

        public string TemplateMasterID { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();

        private string strResMsg = string.Empty;

        public T_HR_SCHEDULINGTEMPLATEMASTER SchedulingTemplateMaster { get; set; }

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        #endregion

        #region 初始化
        public SchedulingTemplateForm(FormTypes formtype, string strTemplateMasterID)
        {
            FormType = formtype;
            TemplateMasterID = strTemplateMasterID;
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SchedulingTemplateForm_Loaded);
        }

        void SchedulingTemplateForm_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            InitParas();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            //Master
            clientAtt.GetSchedulingTemplateMasterByIDCompleted += new EventHandler<GetSchedulingTemplateMasterByIDCompletedEventArgs>(clientAtt_GetSchedulingTemplateMasterByIDCompleted);
            clientAtt.AddSchedulingTemplateMasterAndDetailCompleted += new EventHandler<AddSchedulingTemplateMasterAndDetailCompletedEventArgs>(clientAtt_AddSchedulingTemplateMasterAndDetailCompleted);
            clientAtt.ModifySchedulingTemplateMasterAndDetailCompleted += new EventHandler<ModifySchedulingTemplateMasterAndDetailCompletedEventArgs>(clientAtt_ModifySchedulingTemplateMasterAndDetailCompleted);

            //Detail
            clientAtt.GetAllSchedulingTemplateDetailRdListByMasterIdCompleted += new EventHandler<GetAllSchedulingTemplateDetailRdListByMasterIdCompletedEventArgs>(clientAtt_GetAllSchedulingTemplateDetailRdListByMasterIdCompleted);
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
            SchedulingTemplateMaster = null;

            SchedulingTemplateMaster = new T_HR_SCHEDULINGTEMPLATEMASTER();
            SchedulingTemplateMaster.TEMPLATEMASTERID = System.Guid.NewGuid().ToString().ToUpper();

            //权限控制所需信息
            SchedulingTemplateMaster.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            SchedulingTemplateMaster.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            SchedulingTemplateMaster.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            SchedulingTemplateMaster.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //创建人员的信息
            SchedulingTemplateMaster.CREATEDATE = DateTime.Now;
            SchedulingTemplateMaster.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            SchedulingTemplateMaster.UPDATEDATE = System.DateTime.Now;
            SchedulingTemplateMaster.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据
            SchedulingTemplateMaster.SCHEDULINGCIRCLETYPE = "0";


            this.DataContext = SchedulingTemplateMaster;
        }

        /// <summary>
        /// 加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(TemplateMasterID))
            {
                return;
            }

            clientAtt.GetSchedulingTemplateMasterByIDAsync(TemplateMasterID);
        }


        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("SCHEDULINGTEMPLATESET");
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

        public void RefreshUI(RefreshedTypes type)
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
        /// 效验提交的表单
        /// </summary>
        /// <param name="entSchedulingTemplateMaster"></param>
        /// <returns></returns>
        private void CheckSubmitForm(FormTypes FormType, out bool flag)
        {
            flag = false;

            if (string.IsNullOrEmpty(txtTemplateName.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TEMPLATENAME"), Utility.GetResourceStr("REQUIRED", "TEMPLATENAME"));
                flag = false;
                return;
            }
            else
            {
                flag = true;
            }

            if (cbxkSchedulingCircleType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SCHEDULINGCIRCLETYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("SCHEDULINGCIRCLETYPE")));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkSchedulingCircleType.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SCHEDULINGCIRCLETYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("SCHEDULINGCIRCLETYPE")));
                    flag = false;
                    return;
                }

                flag = true;
            }

            if (!flag)
            {
                return;
            }

            if (FormType == FormTypes.Edit)
            {
                SchedulingTemplateMaster.UPDATEDATE = DateTime.Now;
                SchedulingTemplateMaster.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            }
        }

        /// <summary>
        /// 根据选择的循环周期及默认班次方案，生成排班明细记录(临时)
        /// </summary>
        private void MakeDetailItemSource()
        {
            if (cbxkSchedulingCircleType.SelectedItem == null)
            {
                return;
            }

            ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL> TemplateDetailList = new ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>();

            T_HR_SHIFTDEFINE entShiftDefine = new T_HR_SHIFTDEFINE();
            if (lkDefaultShiftName.DataContext != null)
            {
                entShiftDefine = lkDefaultShiftName.DataContext as T_HR_SHIFTDEFINE;
            }

            T_SYS_DICTIONARY entDic = cbxkSchedulingCircleType.SelectedItem as T_SYS_DICTIONARY;
            if (entDic.DICTIONARYVALUE.Value.ToString() == (Convert.ToInt32(SchedulingCircleType.Month) + 1).ToString())
            {
                int iMonthDays = 31;
                for (int i = 0; i < iMonthDays; i++)
                {
                    T_HR_SCHEDULINGTEMPLATEDETAIL SchedulingTemplateDetail = new T_HR_SCHEDULINGTEMPLATEDETAIL();
                    SchedulingTemplateDetail.TEMPLATEDETAILID = System.Guid.NewGuid().ToString().ToUpper();
                    SchedulingTemplateDetail.T_HR_SCHEDULINGTEMPLATEMASTER = SchedulingTemplateMaster;
                    SchedulingTemplateDetail.SCHEDULINGDATE = (i + 1).ToString();
                    SchedulingTemplateDetail.SCHEDULINGINDEX = i + 1;
                    SchedulingTemplateDetail.T_HR_SHIFTDEFINE = entShiftDefine;
                    SchedulingTemplateDetail.REMARK = string.Empty;

                    //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
                    SchedulingTemplateDetail.CREATEDATE = DateTime.Now;
                    SchedulingTemplateDetail.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    SchedulingTemplateDetail.UPDATEDATE = System.DateTime.Now;
                    SchedulingTemplateDetail.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    TemplateDetailList.Add(SchedulingTemplateDetail);
                }
            }
            else if (entDic.DICTIONARYVALUE.Value.ToString() == (Convert.ToInt32(SchedulingCircleType.Week) + 1).ToString())
            {
                int iWeekDays = 7;
                for (int j = 0; j < iWeekDays; j++)
                {
                    T_HR_SCHEDULINGTEMPLATEDETAIL SchedulingTemplateDetail = new T_HR_SCHEDULINGTEMPLATEDETAIL();
                    SchedulingTemplateDetail.TEMPLATEDETAILID = System.Guid.NewGuid().ToString().ToUpper();
                    SchedulingTemplateDetail.SCHEDULINGDATE = (j + 1).ToString();
                    SchedulingTemplateDetail.SCHEDULINGINDEX = j + 1;
                    SchedulingTemplateDetail.T_HR_SCHEDULINGTEMPLATEMASTER = SchedulingTemplateMaster;
                    SchedulingTemplateDetail.T_HR_SHIFTDEFINE = entShiftDefine;
                    SchedulingTemplateDetail.REMARK = string.Empty;

                    //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
                    SchedulingTemplateDetail.CREATEDATE = DateTime.Now;
                    SchedulingTemplateDetail.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    SchedulingTemplateDetail.UPDATEDATE = System.DateTime.Now;
                    SchedulingTemplateDetail.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    TemplateDetailList.Add(SchedulingTemplateDetail);
                }
            }

            dgTemplateDetails.ItemsSource = TemplateDetailList;
        }

        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            bool flag = false;

            try
            {
                CheckSubmitForm(FormType, out flag);

                if (!flag)
                {
                    return false;
                }

                ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL> TemplateDetailList = dgTemplateDetails.ItemsSource as ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>;
                if (TemplateDetailList == null)
                {
                    TemplateDetailList = new ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>();
                }

                if (FormType == FormTypes.New)
                {
                    clientAtt.AddSchedulingTemplateMasterAndDetailAsync(SchedulingTemplateMaster, TemplateDetailList);
                }
                else
                {
                    //T_HR_SCHEDULINGTEMPLATEDETAIL temp = new T_HR_SCHEDULINGTEMPLATEDETAIL();
                    //temp.T_HR_SHIFTDEFINE = new T_HR_SHIFTDEFINE();

                    //for (int i = 0; i < TemplateDetailList.Count; i++)
                    //{
                    //    temp.TEMPLATEDETAILID = TemplateDetailList[i].TEMPLATEDETAILID;
                    //    temp.T_HR_SHIFTDEFINE.SHIFTDEFINEID = TemplateDetailList[i].T_HR_SHIFTDEFINE.SHIFTDEFINEID;
                    //    TemplateDetailList[i] = temp;
                    //}
                    clientAtt.ModifySchedulingTemplateMasterAndDetailAsync(SchedulingTemplateMaster, TemplateDetailList);
                }

                RefreshUI(RefreshedTypes.All);

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

            RefreshUI(RefreshedTypes.All);

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        #endregion

        #region 事件

        /// <summary>
        /// 根据主键索引，获得指定的排班模板基本设置以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetSchedulingTemplateMasterByIDCompleted(object sender, GetSchedulingTemplateMasterByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SchedulingTemplateMaster = e.Result;
                this.DataContext = SchedulingTemplateMaster;
                SetToolBar();

                string strSortKey = " SCHEDULINGINDEX ";
                clientAtt.GetAllSchedulingTemplateDetailRdListByMasterIdAsync(TemplateMasterID, strSortKey);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 新增排班模板及明细记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddSchedulingTemplateMasterAndDetailCompleted(object sender, AddSchedulingTemplateMasterAndDetailCompletedEventArgs e)
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
        /// 更新排班模板及明细记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifySchedulingTemplateMasterAndDetailCompleted(object sender, ModifySchedulingTemplateMasterAndDetailCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "SCHEDULINGTEMPLATESET")));
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
        /// 根据主键索引，获得指定的排班模板明细设置以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAllSchedulingTemplateDetailRdListByMasterIdCompleted(object sender, GetAllSchedulingTemplateDetailRdListByMasterIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL> TemplateDetailList = e.Result;
                if (TemplateDetailList == null)
                {
                    TemplateDetailList = new ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>();
                }
                dgTemplateDetails.ItemsSource = TemplateDetailList;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 选择班次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>       
        private void lkShiftName_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("SHIFTNAME", "SHIFTNAME");
            cols.Add("WORKTIME", "WORKTIME");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.ShiftDefine,
                typeof(T_HR_SHIFTDEFINE[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_SHIFTDEFINE ent = lookup.SelectedObj as T_HR_SHIFTDEFINE;
                LookUp lkShiftName = sender as LookUp;

                if (ent != null)
                {
                    lkShiftName.DataContext = ent;

                    if (lkDefaultShiftName.DataContext != null)
                    {
                        T_HR_SHIFTDEFINE entDefault = lkDefaultShiftName.DataContext as T_HR_SHIFTDEFINE;
                        if (ent.SHIFTDEFINEID != entDefault.SHIFTDEFINEID)
                        {
                            lkDefaultShiftName.DataContext = null;
                        }
                    }
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void cbxkSchedulingCircleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MakeDetailItemSource();
        }       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkDefaultShiftName_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("SHIFTNAME", "SHIFTNAME");
            cols.Add("WORKTIME", "WORKTIME");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.ShiftDefine,
                typeof(T_HR_SHIFTDEFINE[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_SHIFTDEFINE ent = lookup.SelectedObj as T_HR_SHIFTDEFINE;

                if (ent != null)
                {
                    lkDefaultShiftName.DataContext = ent;
                    MakeDetailItemSource();
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        #endregion
    }
}
