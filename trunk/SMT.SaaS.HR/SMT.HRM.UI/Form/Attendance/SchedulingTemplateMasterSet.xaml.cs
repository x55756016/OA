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
    public partial class SchedulingTemplateMasterSet : BaseForm
    {
        #region 全局变量
        public T_HR_SCHEDULINGTEMPLATEMASTER SchedulingTemplateMaster { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        
        #endregion


        #region 初始化
        public SchedulingTemplateMasterSet()
        {
            InitializeComponent();
            RegisterEvents();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetSchedulingTemplateMasterByIDCompleted += new EventHandler<GetSchedulingTemplateMasterByIDCompletedEventArgs>(clientAtt_GetSchedulingTemplateMasterByIDCompleted);
            clientAtt.AddSchedulingTemplateMasterCompleted += new EventHandler<AddSchedulingTemplateMasterCompletedEventArgs>(clientAtt_AddSchedulingTemplateMasterCompleted);
            clientAtt.ModifySchedulingTemplateMasterCompleted += new EventHandler<ModifySchedulingTemplateMasterCompletedEventArgs>(clientAtt_ModifySchedulingTemplateMasterCompleted);
        }

        /// <summary>
        /// 表单初始化
        /// </summary>
        public void InitForm()
        {
            SchedulingTemplateMaster = null;

            SchedulingTemplateMaster = new T_HR_SCHEDULINGTEMPLATEMASTER();
            SchedulingTemplateMaster.TEMPLATEMASTERID = System.Guid.NewGuid().ToString().ToUpper();

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            SchedulingTemplateMaster.CREATEDATE = DateTime.Now;
            SchedulingTemplateMaster.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            SchedulingTemplateMaster.UPDATEDATE = System.DateTime.Now;
            SchedulingTemplateMaster.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据
            SchedulingTemplateMaster.SCHEDULINGCIRCLETYPE = "0";


            this.DataContext = SchedulingTemplateMaster;
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

            clientAtt.GetSchedulingTemplateMasterByIDAsync(strTemplateMasterID);
        }
        #endregion

        #region 私有方法
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TEMPLATENAME"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("TEMPLATENAME")));
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
        /// 保存
        /// </summary>
        public bool SaveMaster(FormTypes FormType)
        {
            bool flag = false;

            try
            {
                List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
                if (validators.Count > 0)
                {
                    return false;
                }

                CheckSubmitForm(FormType, out flag);

                if (!flag)
                {
                    return false;
                }

                if (FormType == FormTypes.New)
                {
                    clientAtt.AddSchedulingTemplateMasterAsync(SchedulingTemplateMaster);
                }
                else
                {
                    clientAtt.ModifySchedulingTemplateMasterAsync(SchedulingTemplateMaster);
                }
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
        void clientAtt_AddSchedulingTemplateMasterCompleted(object sender, AddSchedulingTemplateMasterCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
                    InitForm();
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
        /// 更新假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifySchedulingTemplateMasterCompleted(object sender, ModifySchedulingTemplateMasterCompletedEventArgs e)
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
        #endregion

    }
}
