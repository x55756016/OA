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

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttSolDefSet : BaseForm
    {
        #region 全局变量
        private T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster { get; set; }
        private ObservableCollection<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        #endregion

        #region 初始化
        public AttSolDefSet()
        {
            InitializeComponent();
            RegisterEvents();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetSchedulingTemplateMasterByAttSolIDCompleted += new EventHandler<GetSchedulingTemplateMasterByAttSolIDCompletedEventArgs>(clientAtt_GetSchedulingTemplateMasterByAttSolIDCompleted);
            clientAtt.GetAllSchedulingTemplateDetailRdListByMasterIdCompleted += new EventHandler<GetAllSchedulingTemplateDetailRdListByMasterIdCompletedEventArgs>(clientAtt_GetAllSchedulingTemplateDetailRdListByMasterIdCompleted);
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
            entTemplateMaster = new T_HR_SCHEDULINGTEMPLATEMASTER();
            this.DataContext = entTemplateMaster;
        }

        private void LoadData(string strAttendanceSolID)
        {
            if (string.IsNullOrEmpty(strAttendanceSolID))
            {
                return;
            }

            clientAtt.GetSchedulingTemplateMasterByAttSolIDAsync(strAttendanceSolID);
        }

        #endregion

        #region 私有方法
        public bool Save(ref T_HR_ATTENDANCESOLUTION entAttSol)
        {
            bool flag = false;
            if (lkSchedulingTemplateMaster.DataContext == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "SCHEDULINGTEMPLATEMASTER"));
                return false;
            }

            if (dgSEList.ItemsSource == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "SCHEDULINGTEMPLATEDETAILSET"));
                return false;
            }

            entAttSol.T_HR_SCHEDULINGTEMPLATEMASTER = entTemplateMaster;

            flag = true;

            return flag;
        }

        /// <summary>
        /// 将LookUp选取的数据抽取出来，转存到AttendanceWS提供的T_HR_SCHEDULINGTEMPLATEMASTER实体中，并返回
        /// </summary>
        /// <param name="obj">LookUp选取的数据</param>
        /// <returns></returns>
        private T_HR_SCHEDULINGTEMPLATEMASTER ReplicateDataToNewStructure(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            T_HR_SCHEDULINGTEMPLATEMASTER entRes = new T_HR_SCHEDULINGTEMPLATEMASTER();
            SMT.Saas.Tools.OrganizationWS.T_HR_SCHEDULINGTEMPLATEMASTER entTemp = obj as SMT.Saas.Tools.OrganizationWS.T_HR_SCHEDULINGTEMPLATEMASTER;

            entRes.TEMPLATEMASTERID = entTemp.TEMPLATEMASTERID;
            entRes.TEMPLATENAME = entTemp.TEMPLATENAME;
            entRes.SCHEDULINGCIRCLETYPE = entTemp.SCHEDULINGCIRCLETYPE;
            entRes.REMARK = entTemp.REMARK;
            entRes.UPDATEDATE = entTemp.UPDATEDATE;
            entRes.UPDATEUSERID = entTemp.UPDATEUSERID;
            entRes.CREATEDATE = entTemp.CREATEDATE;
            entRes.CREATEUSERID = entTemp.CREATEUSERID;

            return entRes;

        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetSchedulingTemplateMasterByAttSolIDCompleted(object sender, GetSchedulingTemplateMasterByAttSolIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entTemplateMaster = e.Result;
                this.DataContext = entTemplateMaster;

                if (entTemplateMaster != null)
                {
                    lkSchedulingTemplateMaster.DataContext = entTemplateMaster;
                    string strTemplateMasterID = string.Empty, strSortkey = string.Empty;
                    strTemplateMasterID = entTemplateMaster.TEMPLATEMASTERID;
                    strSortkey = "SCHEDULINGDATE";

                    clientAtt.GetAllSchedulingTemplateDetailRdListByMasterIdAsync(strTemplateMasterID, strSortkey);
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAllSchedulingTemplateDetailRdListByMasterIdCompleted(object sender, GetAllSchedulingTemplateDetailRdListByMasterIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entTemplateDetails = e.Result;

                dgSEList.ItemsSource = entTemplateDetails;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        private void lkSchedulingTemplateMaster_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("TEMPLATENAME", "TEMPLATENAME");
            cols.Add("REMARK", "REMARK");

            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.SchedulingTemplateMaster,
                typeof(SMT.Saas.Tools.OrganizationWS.T_HR_SCHEDULINGTEMPLATEMASTER[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                entTemplateMaster = ReplicateDataToNewStructure(lookup.SelectedObj);

                if (entTemplateMaster != null)
                {
                    lkSchedulingTemplateMaster.DataContext = entTemplateMaster;
                    txtRemark.Text = entTemplateMaster.REMARK == null ? string.Empty : entTemplateMaster.REMARK;
                    if (cbxkSchedulingCircleType.ItemsSource != null)
                    {
                        foreach (object obj in cbxkSchedulingCircleType.ItemsSource)
                        {
                            T_SYS_DICTIONARY ent = obj as T_SYS_DICTIONARY;
                            if (entTemplateMaster.SCHEDULINGCIRCLETYPE == ent.DICTIONARYVALUE.Value.ToString())
                            {
                                cbxkSchedulingCircleType.SelectedItem = obj;
                                break;
                            }
                        }
                    }

                    cbxkSchedulingCircleType.IsEnabled = false;
                    txtRemark.IsEnabled = false;

                    string strTemplateMasterID = string.Empty, strSortkey = string.Empty;
                    strTemplateMasterID = entTemplateMaster.TEMPLATEMASTERID;
                    strSortkey = "SCHEDULINGINDEX";

                    clientAtt.GetAllSchedulingTemplateDetailRdListByMasterIdAsync(strTemplateMasterID, strSortkey);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
    }
}
