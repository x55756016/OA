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
    public partial class SchedulingTemplateDetailSet : BaseForm
    {
        #region 全局变量

        internal ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL> TemplateDetailList { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();

        #endregion

        #region 初始化
        public SchedulingTemplateDetailSet()
        {
            InitializeComponent();
            RegisterEvents();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetAllSchedulingTemplateDetailRdListByMasterIdCompleted += new EventHandler<GetAllSchedulingTemplateDetailRdListByMasterIdCompletedEventArgs>(clientAtt_GetAllSchedulingTemplateDetailRdListByMasterIdCompleted);
            
            clientAtt.AddDetailForTemplateMasterCompleted += new EventHandler<AddDetailForTemplateMasterCompletedEventArgs>(clientAtt_AddDetailForTemplateMasterCompleted);
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        public void LoadData(string strTemplateMasterID)
        {
            if (string.IsNullOrEmpty(strTemplateMasterID))
            {
                return;
            }

            string strSortKey = " TEMPLATEDETAILID ";

            hdTemplateMasterID.Text = strTemplateMasterID;
            clientAtt.GetAllSchedulingTemplateDetailRdListByMasterIdAsync(strTemplateMasterID, strSortKey);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 效验提交的数据
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private T_HR_SCHEDULINGTEMPLATEDETAIL CheckSubmitForm(ref bool flag)
        {
            flag = false;
            T_HR_SCHEDULINGTEMPLATEDETAIL SchedulingTemplateDetail = new T_HR_SCHEDULINGTEMPLATEDETAIL();

            SchedulingTemplateDetail.T_HR_SCHEDULINGTEMPLATEMASTER = GetMaster();
            SchedulingTemplateDetail.TEMPLATEDETAILID = System.Guid.NewGuid().ToString().ToUpper();

            int iDayNo = 0;
            flag = int.TryParse(txtSchedulingDate.Text, out iDayNo);
            if (!flag)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SCHEDULINGDATE"), Utility.GetResourceStr("REQUIREDNUMERICAL", "SCHEDULINGDATE"));
                return null;
            }
            else
            {
                SchedulingTemplateDetail.SCHEDULINGDATE = txtSchedulingDate.Text;
            }

            if (lkShiftName.DataContext == null)
            {
                flag = false;
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SHIFTNAME"), Utility.GetResourceStr("REQUIRED", "SHIFTNAME"));
                return null;
            }
            else
            {
                T_HR_SHIFTDEFINE ent = lkShiftName.DataContext as T_HR_SHIFTDEFINE;
                if (string.IsNullOrEmpty(ent.SHIFTDEFINEID))
                {
                    flag = false;
                    return null;
                }

                SchedulingTemplateDetail.T_HR_SHIFTDEFINE = ent;
                flag = true;
            }

            if (!string.IsNullOrEmpty(txtRemark.Text))
            {
                SchedulingTemplateDetail.REMARK = txtRemark.Text;
            }

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            SchedulingTemplateDetail.CREATEDATE = DateTime.Now;
            SchedulingTemplateDetail.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            SchedulingTemplateDetail.UPDATEDATE = System.DateTime.Now;
            SchedulingTemplateDetail.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            return SchedulingTemplateDetail;
        }

        /// <summary>
        /// 获取临时主表实体
        /// </summary>
        /// <returns></returns>
        private T_HR_SCHEDULINGTEMPLATEMASTER GetMaster()
        {
            if (string.IsNullOrEmpty(hdTemplateMasterID.Text))
            {
                return null;
            }

            T_HR_SCHEDULINGTEMPLATEMASTER TemplateMaster = new T_HR_SCHEDULINGTEMPLATEMASTER();
            TemplateMaster.TEMPLATEMASTERID = hdTemplateMasterID.Text;
            return TemplateMaster;

        }

        /// <summary>
        /// 重置表单
        /// </summary>
        private void ResetDetailForm()
        {
            txtSchedulingDate.Text = string.Empty;
            lkShiftName.DataContext = null;
            txtRemark.Text = string.Empty;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public bool SaveDetail()
        {
            bool flag = false;

            try
            {
                string strTemplateMasterID = hdTemplateMasterID.Text.Trim();

                ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL> entAdd = new ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>();

                if (TemplateDetailList.Count > 0)
                {

                    entAdd = Utility.Clone(TemplateDetailList);
                }

                clientAtt.AddDetailForTemplateMasterAsync(strTemplateMasterID, entAdd);
                flag = true;
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            return flag;
        }
        #endregion

        #region 事件
        /// <summary>
        /// 根据主键索引，获得指定的排班模板明细设置以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAllSchedulingTemplateDetailRdListByMasterIdCompleted(object sender, GetAllSchedulingTemplateDetailRdListByMasterIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                TemplateDetailList = e.Result;

                if (TemplateDetailList == null)
                {
                    TemplateDetailList = new ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>();                    
                }                

                dgTemplateDetails.ItemsSource = TemplateDetailList.OrderBy(c => c.SCHEDULINGDATE);
                this.DataContext = TemplateDetailList;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 排班模板主表添加相关子记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddDetailForTemplateMasterCompleted(object sender, AddDetailForTemplateMasterCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (string.IsNullOrEmpty(e.Result))
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

                if (ent != null)
                {
                    lkShiftName.DataContext = ent;
                }
            };
            
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            bool flag = false;
            T_HR_SCHEDULINGTEMPLATEDETAIL entSchedulingTemplateDetail = CheckSubmitForm(ref flag);
            if (!flag)
            {
                return;
            }

            if (TemplateDetailList == null)
            {
                TemplateDetailList = new ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>();
            }

            if (TemplateDetailList.Count > 0)
            {
                for (int i = 0; i < TemplateDetailList.Count; i++)
                {
                    T_HR_SCHEDULINGTEMPLATEDETAIL item = TemplateDetailList[i] as T_HR_SCHEDULINGTEMPLATEDETAIL;
                    if (item.SCHEDULINGDATE == entSchedulingTemplateDetail.SCHEDULINGDATE)
                    {
                        TemplateDetailList.Remove(item);
                    }
                }
            }

            TemplateDetailList.Add(entSchedulingTemplateDetail);

            dgTemplateDetails.ItemsSource = TemplateDetailList.OrderBy(c => c.SCHEDULINGDATE);
            this.DataContext = TemplateDetailList;

            ResetDetailForm();
        }

        /// <summary>
        /// 删除指定的明细设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            ComfirmBox delComfirmBox = new ComfirmBox();
            delComfirmBox.Title = Utility.GetResourceStr("DELETECONFIRM");
            delComfirmBox.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
            delComfirmBox.ButtonOK.Click += new RoutedEventHandler(ButtonOK_Click);
            delComfirmBox.Show();
        }

        /// <summary>
        /// 轮询数据列表行，对选定行进行删除操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (dgTemplateDetails.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                return;
            }

            if (dgTemplateDetails.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                return;
            }

            ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL> entList = new ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>();
            ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL> entDels = new ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>();

            ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL> entSels = new ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL>();
            foreach (object ovj in dgTemplateDetails.SelectedItems)
            {
                T_HR_SCHEDULINGTEMPLATEDETAIL ent = ovj as T_HR_SCHEDULINGTEMPLATEDETAIL;
                entSels.Add(ent);
            }


            entList = Utility.Clone(TemplateDetailList);
            entDels = Utility.Clone(entSels);

            for (int j = 0; j < entDels.Count; j++)
            {
                T_HR_SCHEDULINGTEMPLATEDETAIL entDel = entDels[j] as T_HR_SCHEDULINGTEMPLATEDETAIL;

                for (int i = 0; i < entList.Count; i++)
                {
                    T_HR_SCHEDULINGTEMPLATEDETAIL item = entList[i] as T_HR_SCHEDULINGTEMPLATEDETAIL;
                    if (item.SCHEDULINGDATE == entDel.SCHEDULINGDATE)
                    {
                        entList.Remove(item);
                        //i = 0;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            TemplateDetailList.Clear();

            TemplateDetailList = entList;

            dgTemplateDetails.ItemsSource = TemplateDetailList.OrderBy(c => c.SCHEDULINGDATE);
            this.DataContext = TemplateDetailList;

        }
        #endregion
    }
}
