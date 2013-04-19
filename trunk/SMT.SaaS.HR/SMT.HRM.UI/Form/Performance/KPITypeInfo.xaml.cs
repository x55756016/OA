/// <summary>
/// Log No.： 1
/// Modify Desc： 添加滚动条，绑定数据，设置控件可用，隐藏修改用户和日期
/// Modifier： 冉龙军
/// Modify Date： 2010-08-04
/// Log No.： 2
/// Modify Desc： 设置打分标准及其校验
/// Modifier： 冉龙军
/// Modify Date： 2010-08-31
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PerformanceWS;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Performance
{
    public partial class KPITypeInfo : BaseForm, IEntityEditor
    {
        public T_HR_KPITYPE KPIType { get; set; }
        public FormTypes FormType { get; set; }
        private PerformanceServiceClient client = new PerformanceServiceClient();
        private T_HR_KPIREMIND remind1;
        private T_HR_KPIREMIND remind2;
        private T_HR_KPIREMIND remind3;
        private bool isClose = false;

        public KPITypeInfo(FormTypes type, string kpiTypeID)
        {
            FormType = type;
            InitializeComponent();
            // 2s 冉龙军
            //SetSystemScoreEnable(false);
            SetSystemScoreEnable(true);
            // 2e
            SetRemindEnable(false);
            //   chkIsRandom.IsEnabled = false;
            txtRandomWeight.IsEnabled = false;
            cboRandomGroup.IsEnabled = false;
            InitPara(kpiTypeID);

        }

        void KPITypeInfo_Loaded(object sender, RoutedEventArgs e)
        {
            #region 初始化expander中的控件

            /*expander MachineStandar
            txtInitailDate = Utility.FindChildControl<TextBox>(expander, "txtInitailDate");
            txtInitailScore = Utility.FindChildControl<TextBox>(expander, "txtInitailScore");
            txtScoreUnit = Utility.FindChildControl<TextBox>(expander, "txtScoreUnit");
            txtAddForForward = Utility.FindChildControl<TextBox>(expander, "txtAddForForward");
            txtMaxScore = Utility.FindChildControl<TextBox>(expander, "txtMaxScore");
            txtReduceForDelay = Utility.FindChildControl<TextBox>(expander, "txtReduceForDelay");
            txtMinScore = Utility.FindChildControl<TextBox>(expander, "txtMinScore");

            //expanderRemind
            chkIsRemind1 = Utility.FindChildControl<CheckBox>(expanderRemind, "chkbIsRemind1");
            chkIsRemind2 = Utility.FindChildControl<CheckBox>(expanderRemind, "chkbIsRemind2");
            chkIsRemind3 = Utility.FindChildControl<CheckBox>(expanderRemind, "chkbIsRemind3");
            cboRemindType1 = Utility.FindChildControl<ComboBox>(expanderRemind, "cboRemindType1");
            cboRemindType2 = Utility.FindChildControl<ComboBox>(expanderRemind, "cboRemindType2");
            cboRemindType3 = Utility.FindChildControl<ComboBox>(expanderRemind, "cboRemindType3");
            txtForwardHours1 = Utility.FindChildControl<TextBox>(expanderRemind, "txtForwardHours1");
            txtForwardHours2 = Utility.FindChildControl<TextBox>(expanderRemind, "txtForwardHours2");
            txtForwardHours3 = Utility.FindChildControl<TextBox>(expanderRemind, "txtForwardHours3"); 
            */

            #endregion 初始化expander中的控件
        }

        /// <summary>
        /// 初始化界面参数
        /// </summary>
        /// <param name="kpiTypeID"></param>
        private void InitPara(string kpiTypeID)
        {
            //加载事件
            client.GetKPITypeByIDCompleted += new EventHandler<GetKPITypeByIDCompletedEventArgs>(client_GetKPITypeByIDCompleted);
            client.GetRandomGroupAllCompleted += new EventHandler<GetRandomGroupAllCompletedEventArgs>(client_GetRandomGroupAllCompleted);
            client.AddKPITypeCompleted += new EventHandler<AddKPITypeCompletedEventArgs>(client_AddKPITypeCompleted);
            client.UpdateKPITypeAndRemindCompleted += new EventHandler<UpdateKPITypeAndRemindCompletedEventArgs>(client_UpdateKPITypeAndRemindCompleted);
            this.Loaded += new RoutedEventHandler(KPITypeInfo_Loaded);
            BindCheckBoxes(cboRemindList1);
            BindCheckBoxes(cboRemindList2);
            BindCheckBoxes(cboRemindList3);

            client.GetRandomGroupAllAsync();
            //窗口状态
            switch ((int)FormType)
            {
                case 0://NEW
                    KPITypeFactory();
                    chkIsMachine.IsEnabled = true;
                    chkIsPerson.IsEnabled = true;
                    //txtKPITypeName.Width = 356;
                    //chkIsRemind1.IsEnabled = true;
                    //chkIsRemind2.IsEnabled = true;
                    //chkIsRemind3.IsEnabled = true;
                    // 1s 冉龙军
                    txtMachineWeight.IsEnabled = false;
                    txtPersonWeight.IsEnabled = false;
                    txtInitailScore.Value = 100;
                    // 1e

                    break;
                case 1://EDIT
                    client.GetKPITypeByIDAsync(kpiTypeID);

                    chkIsMachine.IsEnabled = true;
                    chkIsPerson.IsEnabled = true;

                    break;
                case 2: //BROWE
                    client.GetKPITypeByIDAsync(kpiTypeID);
                    SetControlsEnable(false);
                    SetSystemScoreEnable(false);
                    SetRemindEnable(false);
                    // 1s 冉龙军
                    SetChkRemaindDisable();
                    // 1e
                    break;
                case 3: //ADUIT
                    break;
            }
        }



        /// <summary>
        /// 生产一个新的KPIType
        /// </summary>
        private void KPITypeFactory()
        {
            KPIType = new T_HR_KPITYPE();
            KPIType.T_HR_SCORETYPE = new T_HR_SCORETYPE();
            KPIType.T_HR_SCORETYPE.SCORETYPEID = Guid.NewGuid().ToString();
            KPIType.KPITYPEID = Guid.NewGuid().ToString();

            KPIType.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            KPIType.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            KPIType.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            KPIType.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            KPIType.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            KPIType.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            KPIType.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            KPIType.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            KPIType.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            KPIType.CREATEDATE = DateTime.Now;
            KPIType.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            KPIType.UPDATEDATE = DateTime.Now;
        }

        /// <summary>
        /// 绑定窗口数据
        /// </summary>
        private void BindData()
        {
            //窗口绑定
            LayoutRoot.DataContext = KPIType;
            //评分标准绑定
            //expander.DataContext = KPIType;

            //绑定评分方式数据
            //AppraisalType.DataContext = KPIType.T_HR_SCORETYPE;
            //是否系统评分
            chkIsMachine.IsChecked = KPIType.T_HR_SCORETYPE.ISSYSTEMSCORE.Trim().Equals("1") ? true : false;
            chkIsMachine_Click(null, null);

            chkIsPerson.IsChecked = KPIType.T_HR_SCORETYPE.ISMANUALSCORE.Trim().Equals("1") ? true : false;
            chkIsPerson_Click(null, null);

            chkIsRandom.IsChecked = KPIType.T_HR_SCORETYPE.ISRANDOMSCORE.Trim().Equals("1") ? true : false;
            chkIsRandom_Click(null, null);

            //绑定抽查组
            if (chkIsRandom.IsChecked.Value)
                for (int i = 0; i < cboRandomGroup.Items.Count; i++)
                {
                    if (((T_HR_RANDOMGROUP)cboRandomGroup.Items[i]).RANDOMGROUPID.Equals(KPIType.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID))
                    {
                        cboRandomGroup.SelectedIndex = i;
                        break;
                    }
                }
            //client.GETkpire

            //绑定提醒信息
            IEnumerator OperandEnum = KPIType.T_HR_SCORETYPE.T_HR_KPIREMIND.OrderBy(s => s.FORWARDHOURS).GetEnumerator();
            //提醒计数
            int CharCount = 0;
            while (OperandEnum.MoveNext())
            {
                T_HR_KPIREMIND Remind = (T_HR_KPIREMIND)OperandEnum.Current;
                CharCount++;
                //第一条提醒
                if (CharCount == 1)
                {
                    chkIsRemind1.IsChecked = true;
                    cboRemindList1.IsEnabled = true;
                    if (!string.IsNullOrWhiteSpace(Remind.REMINDTYPE))
                    {
                        SetListBoxCheck(cboRemindList1, Remind.REMINDTYPE);
                    }
                    txtForwardHours1.Value = (Double)Remind.FORWARDHOURS;
                    remind1 = Remind;
                }
                //第二条提醒
                else if (CharCount == 2)
                {
                    chkIsRemind2.IsChecked = true;
                    cboRemindList2.IsEnabled = true;
                    if (!string.IsNullOrWhiteSpace(Remind.REMINDTYPE))
                    {
                        SetListBoxCheck(cboRemindList2, Remind.REMINDTYPE);
                    }
                    txtForwardHours2.Value = (Double)Remind.FORWARDHOURS;
                    remind2 = Remind;
                }
                //第三条提醒
                else if (CharCount == 3)
                {
                    chkIsRemind3.IsChecked = true;
                    cboRemindList3.IsEnabled = true;
                    if (!string.IsNullOrWhiteSpace(Remind.REMINDTYPE))
                    {
                        SetListBoxCheck(cboRemindList3, Remind.REMINDTYPE);
                    }
                    txtForwardHours3.Value = (Double)Remind.FORWARDHOURS;
                    remind3 = Remind;
                }
                else
                    break;
            }
        }

        /// <summary>
        /// 设置ListBox选定项
        /// </summary>
        /// <param name="cboRemindList"></param>
        /// <param name="strRemindType"></param>
        private void SetListBoxCheck(ListBox cboRemindList, string strRemindType)
        {
            if (string.IsNullOrWhiteSpace(strRemindType))
            {
                return;
            }

            string[] strlist = strRemindType.Split(',');
            if (strlist.Length == 0)
            {
                return;
            }

            List<CheckBoxModel> entList = cboRemindList.ItemsSource as List<CheckBoxModel>;

            if (entList == null)
            {
                return;
            }

            if (entList.Count() == 0)
            {
                return;
            }

            for (int i = 0; i < strlist.Length; i++)
            {
                int j = 0;
                int.TryParse(strlist[i].ToString(), out j);

                if (j == 0)
                {
                    continue;
                }

                foreach (CheckBoxModel item in entList)
                {
                    if (item.ID == j)
                    {
                        item.ISCHECKED = true;
                        break;
                    }
                }
            }

            Utility.BindCheckBoxList(entList, cboRemindList);
        }

        /// <summary>
        /// 获取ListBox选中项的Id，并连接成字符串返回
        /// </summary>
        /// <param name="cboRemindList"></param>
        /// <returns></returns>
        private string GetListBoxSelectIndex(ListBox cboRemindList)
        {
            StringBuilder strTemp = new StringBuilder();

            IEnumerable<CheckBoxModel> entList = (IEnumerable<CheckBoxModel>)cboRemindList.ItemsSource;
            IEnumerable<CheckBoxModel> selectedList = entList.Where(c => c.ISCHECKED == true);
            selectedList.ForEach(item =>
            {
                strTemp.Append(item.ID.ToString() + ",");
            });

            return strTemp.ToString();
        }

        /// <summary>
        /// 绑定ListBox
        /// </summary>
        /// <param name="chk"></param>
        private void BindCheckBoxes(ListBox chk)
        {
            List<CheckBoxModel> entlist = new List<CheckBoxModel>();
            entlist.Add(new CheckBoxModel(1, "IMMEDIATELYCOMMUNICATION", false));
            entlist.Add(new CheckBoxModel(2, "SMTGATE", false));
            entlist.Add(new CheckBoxModel(3, "EMAIL", false));
            entlist.Add(new CheckBoxModel(4, "PHONEMESSAGE", false));
            Utility.BindCheckBoxList(entlist, chk);
        }

        /// <summary>
        /// 设置窗口控件可用性
        /// </summary>
        void SetControlsEnable(bool isEnable)
        {
            //窗口控件
            txtKPITypeName.IsEnabled = isEnable;
            txtKPITYPEDESC.IsEnabled = isEnable;
            chkIsMachine.IsEnabled = isEnable;
            chkIsPerson.IsEnabled = isEnable;
            chkIsRandom.IsEnabled = isEnable;
            txtMachineWeight.IsEnabled = isEnable;
            txtPersonWeight.IsEnabled = isEnable;
            txtRandomWeight.IsEnabled = isEnable;
            cboRandomGroup.IsEnabled = isEnable;
            txtInitailDate.IsEnabled = isEnable;
            txtKPITypeName.IsEnabled = isEnable;
        }

        /// <summary>
        /// 设置评分标准窗口控件可用性
        /// </summary>
        void SetSystemScoreEnable(bool isEnable)
        {
            //expander MachineStandar
            txtInitailDate.IsEnabled = isEnable;
            txtInitailScore.IsEnabled = isEnable;
            txtScoreUnit.IsEnabled = isEnable;
            // 1s 冉龙军
            txtLaterUnit.IsEnabled = isEnable;
            // 1e
            txtAddForForward.IsEnabled = isEnable;
            txtMaxScore.IsEnabled = isEnable;
            txtReduceForDelay.IsEnabled = isEnable;
            txtMinScore.IsEnabled = isEnable;
        }
        // 1s 冉龙军
        /// <summary>
        /// 设置提醒窗口CheckBox状态
        /// </summary>
        void SetChkRemaindDisable()
        {
            chkIsRemind1.IsChecked = false;
            chkIsRemind2.IsChecked = false;
            chkIsRemind3.IsChecked = false;
        }
        // 1e
        /// <summary>
        /// 设置提醒窗口控件可用性
        /// </summary>
        void SetRemindEnable(bool isEnable)
        {
            //expanderRemind
            chkIsRemind1.IsEnabled = isEnable;
            chkIsRemind2.IsEnabled = isEnable;
            chkIsRemind3.IsEnabled = isEnable;

            cboRemindList1.IsEnabled = isEnable;
            cboRemindList2.IsEnabled = isEnable;
            cboRemindList3.IsEnabled = isEnable;

            txtForwardHours1.IsEnabled = isEnable;
            txtForwardHours2.IsEnabled = isEnable;
            txtForwardHours3.IsEnabled = isEnable;

        }

        /// <summary>
        /// 保存页面信息
        /// </summary>
        private void Save()
        {
            //处理页面验证
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "KPITYPENAME"));
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else if (CheckKPIInfo())
            {
                //KPI类别
                KPIType.KPITYPENAME = txtKPITypeName.Text.Trim();
                KPIType.KPITYPEREMARK = txtKPITYPEDESC.Text.Trim();
                #region 评分标准
                try
                {
                    //系统评分标准
                    KPIType.T_HR_SCORETYPE.INITIALPOINT = System.Convert.ToDecimal(txtInitailDate.Value); //计分原点
                    KPIType.T_HR_SCORETYPE.INITIALSCORE = System.Convert.ToDecimal(txtInitailScore.Value);//初始分
                    KPIType.T_HR_SCORETYPE.COUNTUNIT = System.Convert.ToDecimal(txtScoreUnit.Value);      //单位天数  
                    // 1s 冉龙军
                    KPIType.T_HR_SCORETYPE.LATERUNIT = System.Convert.ToDecimal(txtLaterUnit.Value);      //延迟小时数  
                    // 1e
                    KPIType.T_HR_SCORETYPE.ADDSCORE = System.Convert.ToDecimal(txtAddForForward.Value);   //加分 
                    KPIType.T_HR_SCORETYPE.REDUCESCORE = System.Convert.ToDecimal(txtReduceForDelay.Value);//减分 
                    KPIType.T_HR_SCORETYPE.MAXSCORE = System.Convert.ToDecimal(txtMaxScore.Value);         //上限 
                    KPIType.T_HR_SCORETYPE.MINSCORE = System.Convert.ToDecimal(txtMinScore.Value);         //下限 
                }
                catch
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " string Convert int error.");
                }
                #endregion
                #region 评分方式
                //评分方式
                if (chkIsMachine.IsChecked.Value)
                {
                    try
                    {
                        //需要进行数据验证
                        //系统评分
                        KPIType.T_HR_SCORETYPE.ISSYSTEMSCORE = "1";
                        KPIType.T_HR_SCORETYPE.SYSTEMWEIGHT = System.Convert.ToDecimal(txtMachineWeight.Value);

                    }
                    catch
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " string Convert int error.");
                    }
                }
                else
                {
                    KPIType.T_HR_SCORETYPE.ISSYSTEMSCORE = "0";
                }

                if (chkIsPerson.IsChecked.Value)
                {
                    //手动评分
                    KPIType.T_HR_SCORETYPE.ISMANUALSCORE = "1";
                    KPIType.T_HR_SCORETYPE.MANUALWEIGHT = System.Convert.ToDecimal(txtPersonWeight.Value);
                }
                else
                {
                    KPIType.T_HR_SCORETYPE.ISMANUALSCORE = "0";
                }

                if (chkIsRandom.IsChecked.Value)
                {
                    //抽查评分
                    KPIType.T_HR_SCORETYPE.ISRANDOMSCORE = "1";
                    KPIType.T_HR_SCORETYPE.RANDOMWEIGHT = System.Convert.ToDecimal(txtRandomWeight.Value);
                    //抽查组信息
                    KPIType.T_HR_SCORETYPE.T_HR_RANDOMGROUP = new T_HR_RANDOMGROUP();
                    KPIType.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID = (cboRandomGroup.SelectedItem as T_HR_RANDOMGROUP).RANDOMGROUPID;
                }
                else
                {
                    KPIType.T_HR_SCORETYPE.ISRANDOMSCORE = "0";
                    KPIType.T_HR_SCORETYPE.T_HR_RANDOMGROUP = null;
                }

                #endregion 评分方式

                //提醒信息

                KPIType.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                KPIType.UPDATEDATE = DateTime.Now;
                string strMsg = string.Empty;
                if (FormType == FormTypes.Edit)
                {
                    client.UpdateKPITypeAndRemindAsync(KPIType, GetRemindList("add"), GetRemindList("update"), GetRemindList("del"), strMsg);
                }
                else if (FormType == FormTypes.New)
                {
                    //所属
                    KPIType.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    KPIType.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    KPIType.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    KPIType.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    KPIType.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    KPIType.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    KPIType.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    KPIType.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    KPIType.CREATEDATE = DateTime.Now;
                    KPIType.T_HR_SCORETYPE.T_HR_KPIREMIND = GetRemindList("add");
                    client.AddKPITypeAsync(KPIType, strMsg);
                }
                //this.DialogResult = true;
            }

        }

        /// <summary>
        /// 验证输入信息
        /// </summary>
        /// <returns></returns>
        private bool CheckKPIInfo()
        {
            if (!chkIsMachine.IsChecked.Value && !chkIsPerson.IsChecked.Value && !chkIsRandom.IsChecked.Value)
            {
                //必须选择一种或多种评分方式
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SCORETYPEMAST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SCORETYPEMAST"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            //评分标准
            if (txtInitailDate.Value.Equals("") || txtInitailScore.Value.Equals("") || txtScoreUnit.Value.Equals("")
                    || txtLaterUnit.Value.Equals("") || txtAddForForward.Value.Equals("") || txtReduceForDelay.Value.Equals("")
                    || txtMaxScore.Value.Equals("") || txtMinScore.Value.Equals(""))
            {
                //必须填写完整的系统评分标准
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须填写完整的系统评分标准");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须填写完整的系统评分标准"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            // 机打
            if (chkIsMachine.IsChecked.Value)
            {
                if (txtMachineWeight.Value.Equals(""))
                {
                    //必须填写系统评分权重
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须填写系统评分权重");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须填写系统评分权重"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }

            }
            else
            {
                txtMachineWeight.Value = 0;
            }
            //人工打分
            if (chkIsPerson.IsChecked.Value)
            {
                if (txtPersonWeight.Value.Equals(""))
                {
                    //必须填写手动评分权重
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须填写手动评分权重");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须填写手动评分权重"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
            }
            else
            {
                txtPersonWeight.Value = 0;
            }

            // 抽查组打分
            if (chkIsRandom.IsChecked.Value)
            {
                if (txtRandomWeight.Value.Equals(""))
                {
                    //必须填写抽查评分权重
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须填写抽查评分权重");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须填写抽查评分权重"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                if (cboRandomGroup.SelectedIndex == -1)
                {
                    //必须选择抽查组
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须选择抽查组");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须选择抽查组"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
            }
            else
            {
                txtRandomWeight.Value = 0;
            }
            // 1s 冉龙军
            if (txtMachineWeight.Value.Equals(""))
            {
                txtMachineWeight.Value = 0;
            }
            if (txtPersonWeight.Value.Equals(""))
            {
                txtPersonWeight.Value = 0;
            }
            if (txtRandomWeight.Value.Equals(""))
            {
                txtRandomWeight.Value = 0;
            }
            if ((txtMachineWeight.Value + txtPersonWeight.Value + txtRandomWeight.Value) != 100)
            {
                //必须填写完整的评分权重
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "评分权重之和不等于100");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("评分权重之和不等于100"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            // 1e
            if (chkIsRemind1.IsChecked.Value)
            {
                IEnumerable<CheckBoxModel> list1 = (IEnumerable<CheckBoxModel>)cboRemindList1.ItemsSource;
                IEnumerable<CheckBoxModel> selectedList1 = list1.Where(a => a.ISCHECKED == true);

                if (selectedList1.Count() == 0 || string.IsNullOrWhiteSpace(txtForwardHours1.Value.ToString()))
                {
                    //提醒一信息不完整
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "提醒一信息不完整");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("提醒一信息不完整"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
            }
            if (chkIsRemind2.IsChecked.Value)
            {
                IEnumerable<CheckBoxModel> list2 = (IEnumerable<CheckBoxModel>)cboRemindList2.ItemsSource;
                IEnumerable<CheckBoxModel> selectedList2 = list2.Where(a => a.ISCHECKED == true);

                if (selectedList2.Count() == 0 || string.IsNullOrWhiteSpace(txtForwardHours2.Value.ToString()))
                {
                    //提醒二信息不完整
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "提醒二信息不完整");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("提醒二信息不完整"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
            }
            if (chkIsRemind3.IsChecked.Value)
            {
                IEnumerable<CheckBoxModel> list3 = (IEnumerable<CheckBoxModel>)cboRemindList3.ItemsSource;
                IEnumerable<CheckBoxModel> selectedList3 = list3.Where(a => a.ISCHECKED == true);

                if (selectedList3.Count() == 0 || string.IsNullOrWhiteSpace(txtForwardHours3.Value.ToString()))
                {
                    //提醒三信息不完整
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "提醒三信息不完整");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("提醒三信息不完整"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取提醒信息列表
        /// </summary>
        /// <param name="remindType">列表类型，"add"为添加，"update"为更新，"del"为删除</param>
        /// <returns></returns>
        private ObservableCollection<T_HR_KPIREMIND> GetRemindList(string remindType)
        {
            bool isCheck = true;
            bool isNew = true;
            if (remindType.Equals("update"))
            {
                isCheck = true;
                isNew = false;
            }
            else if (remindType.Equals("del"))
            {
                isCheck = false;
                isNew = false;
            }
            ObservableCollection<T_HR_KPIREMIND> list = new ObservableCollection<T_HR_KPIREMIND>();
            if (chkIsRemind1.IsChecked.Value == isCheck && remind1 != null && remind1.REMINDID.Equals("-1") == isNew)
            {
                if (remindType.Equals("add"))
                    remind1.REMINDID = Guid.NewGuid().ToString();
                remind1.REMINDTYPE = GetListBoxSelectIndex(cboRemindList1);
                remind1.FORWARDHOURS = System.Convert.ToDecimal(txtForwardHours1.Value);
                list.Add(remind1);
            }
            if (chkIsRemind2.IsChecked.Value == isCheck && remind2 != null && remind2.REMINDID.Equals("-1") == isNew)
            {
                if (remindType.Equals("add"))
                    remind2.REMINDID = Guid.NewGuid().ToString();
                remind2.REMINDTYPE = GetListBoxSelectIndex(cboRemindList2);
                remind2.FORWARDHOURS = System.Convert.ToDecimal(txtForwardHours2.Value);
                list.Add(remind2);
            }
            if (chkIsRemind3.IsChecked.Value == isCheck && remind3 != null && remind3.REMINDID.Equals("-1") == isNew)
            {
                if (remindType.Equals("add"))
                    remind3.REMINDID = Guid.NewGuid().ToString();
                remind3.REMINDTYPE = GetListBoxSelectIndex(cboRemindList3);
                remind3.FORWARDHOURS = System.Convert.ToDecimal(txtForwardHours3.Value);
                list.Add(remind3);
            }
            return list.Count == 0 ? null : list;
        }

        #region 所有事件

        /// <summary>
        /// 获取KPI类别完成事件，加载KPI类别的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetKPITypeByIDCompleted(object sender, GetKPITypeByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                    return;
                }
                //获取类别
                KPIType = e.Result;
                BindData();
            }
        }

        /// <summary>
        /// 获取所有抽查组完成事件，加载抽查组的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetRandomGroupAllCompleted(object sender, GetRandomGroupAllCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                    return;
                }
                //获取类别
                cboRandomGroup.ItemsSource = e.Result.ToList();
                cboRandomGroup.DisplayMemberPath = "RANDOMGROUPNAME";

                //绑定抽查组
                if (FormType != FormTypes.New && chkIsRandom.IsChecked.Value && KPIType.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                    for (int i = 0; i < cboRandomGroup.Items.Count; i++)
                    {
                        if (((T_HR_RANDOMGROUP)cboRandomGroup.Items[i]).RANDOMGROUPID.Equals(KPIType.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID))
                        {
                            cboRandomGroup.SelectedIndex = i;
                            break;
                        }
                    }
            }
        }

        /// <summary>
        /// 添加完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 


        void client_AddKPITypeCompleted(object sender, AddKPITypeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                //if (e.Error.Message == "Repetition")
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPITYPENAME"));
                //}
                //else
                //{
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                // }
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "KPITYPE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

                lblUserName.Text = KPIType.UPDATEUSERID;
                lblUserDate.Text = Convert.ToString(KPIType.UPDATEDATE);

                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 更新完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void client_UpdateKPITypeAndRemindCompleted(object sender, UpdateKPITypeAndRemindCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                //if (e.Error.Message == "Repetition")
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPITYPENAME"));
                //}
                //else
                //{
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                //  }
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "KPITYPE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                RefreshUI(RefreshedTypes.All);
                lblUser.Visibility = Visibility.Visible;
                lblUserName.Visibility = Visibility.Visible;
                // txtKPITypeName.Width = 140;

            }
        }


        #region CheckBox事件

        private void chkIsMachine_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1s 冉龙军
            //txtMachineWeight.IsEnabled = chkIsMachine.IsChecked.Value;
            //SetSystemScoreEnable(chkIsMachine.IsChecked.Value);
            //if (KPIType.T_HR_SCORETYPE.INITIALPOINT == null && KPIType.T_HR_SCORETYPE.INITIALSCORE == null)
            //{
            //    KPIType.T_HR_SCORETYPE.INITIALPOINT = 3;
            //    KPIType.T_HR_SCORETYPE.INITIALSCORE = 80;
            //    KPIType.T_HR_SCORETYPE.COUNTUNIT = 1;
            //    KPIType.T_HR_SCORETYPE.ADDSCORE = 10;
            //    KPIType.T_HR_SCORETYPE.REDUCESCORE = 10;
            //    KPIType.T_HR_SCORETYPE.MAXSCORE = 100;
            //    KPIType.T_HR_SCORETYPE.MINSCORE = 100;
            //}
            if (FormType != FormTypes.Browse)
            {
                txtMachineWeight.IsEnabled = chkIsMachine.IsChecked.Value;
                // 2s 冉龙军
                //SetSystemScoreEnable(chkIsMachine.IsChecked.Value);
                // 2e
                if (!chkIsMachine.IsChecked.Value)
                {
                    txtMachineWeight.Value = 0;
                }
            }
            // 1e
        }

        private void chkIsPerson_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1s 冉龙军
            //txtPersonWeight.IsEnabled = chkIsPerson.IsChecked.Value;
            //chkIsRandom.IsEnabled = chkIsPerson.IsChecked.Value;
            //txtRandomWeight.IsEnabled = chkIsPerson.IsChecked.Value;
            //cboRandomGroup.IsEnabled = chkIsPerson.IsChecked.Value;
            ////如果需要手动评分，那么抽查保持原装，如果不用手动评分，那么抽查评分也要去掉
            //chkIsRandom.IsChecked = chkIsPerson.IsChecked == true ? chkIsRandom.IsChecked : false;
            //chkIsRandom_Click(null, null);
            if (FormType != FormTypes.Browse)
            {
                txtPersonWeight.IsEnabled = chkIsPerson.IsChecked.Value;
                if (!chkIsPerson.IsChecked.Value)
                {
                    txtPersonWeight.Value = 0;
                }
                //chkIsRandom.IsEnabled = chkIsPerson.IsChecked.Value;
                //txtRandomWeight.IsEnabled = chkIsPerson.IsChecked.Value;
                //cboRandomGroup.IsEnabled = chkIsPerson.IsChecked.Value;
                //如果需要手动评分，那么抽查保持原装，如果不用手动评分，那么抽查评分也要去掉
                // chkIsRandom.IsChecked = chkIsPerson.IsChecked == true ? chkIsRandom.IsChecked : false;
                // chkIsRandom_Click(null, null);
            }
            // 1e
        }

        private void chkIsRandom_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1s 冉龙军
            //txtRandomWeight.IsEnabled = chkIsRandom.IsChecked.Value;
            //cboRandomGroup.IsEnabled = chkIsRandom.IsChecked.Value;
            //SetRemindEnable(chkIsRandom.IsChecked.Value);
            if (FormType != FormTypes.Browse)
            {
                txtRandomWeight.IsEnabled = chkIsRandom.IsChecked.Value;
                cboRandomGroup.IsEnabled = chkIsRandom.IsChecked.Value;
                if (!chkIsRandom.IsChecked.Value)
                {
                    SetChkRemaindDisable();
                    txtRandomWeight.Value = 0;
                    cboRandomGroup.SelectedIndex = -1;
                }
                SetRemindEnable(chkIsRandom.IsChecked.Value);
            }
            // 1e
        }

        private void chkIsRemind1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (chkIsRemind1.IsChecked.Value && remind1 == null)
            {
                remind1 = new T_HR_KPIREMIND();
                remind1.REMINDID = "-1";
                if (KPIType != null && KPIType.T_HR_SCORETYPE != null)
                    remind1.T_HR_SCORETYPE = KPIType.T_HR_SCORETYPE;
            }
        }

        private void chkIsRemind2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (chkIsRemind2.IsChecked.Value && remind2 == null)
            {
                remind2 = new T_HR_KPIREMIND();
                remind2.REMINDID = "-1";
                if (KPIType != null && KPIType.T_HR_SCORETYPE != null)
                    remind2.T_HR_SCORETYPE = KPIType.T_HR_SCORETYPE;
            }
        }

        private void chkIsRemind3_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (chkIsRemind3.IsChecked.Value && remind3 == null)
            {
                remind3 = new T_HR_KPIREMIND();
                remind3.REMINDID = "-1";
                if (KPIType != null && KPIType.T_HR_SCORETYPE != null)
                    remind3.T_HR_SCORETYPE = KPIType.T_HR_SCORETYPE;
            }
        }

        #endregion CheckBox事件

        #endregion 所有事件

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("KPITYPE");
        }
        public string GetStatus()
        {
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    isClose = false;
                    break;
                case "1":
                    isClose = true;
                    break;
            }
            Save();
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType != FormTypes.Browse)
            {
                items = Utility.CreateFormSaveButton();
            }

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion
    }
}
