/// <summary>
/// Log No.： 1
/// Modify Desc： 添加滚动条，绑定数据，设置控件可用，角色（或节点）使用Remark（自写备注），隐藏修改人和日期，一个提醒方式多个选项，记录流程描述
/// Modifier： 冉龙军
/// Modify Date： 2010-08-09
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
using SMT.Saas.Tools.FlowDesignerWS;
using SMT.HRM.UI.Active;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Performance
{
    public partial class KPIPointInfo : BaseForm, IEntityEditor
    {
        public T_HR_KPIPOINT KPIPoint { get; set; }
        public FormTypes FormType { get; set; }
        private PerformanceServiceClient client = new PerformanceServiceClient();
        PermissionServiceClient PermissionServiceWcf = new PermissionServiceClient();
        private T_HR_KPIREMIND remind1;
        private T_HR_KPIREMIND remind2;
        private T_HR_KPIREMIND remind3;
        private string stepname;
        FLOW_MODELFLOWRELATION_T modelFlowRelation { get; set; }
        public List<StateType> StateList = new List<StateType>();
        private bool isClose = false;

        public KPIPointInfo(FormTypes type, ObservableCollection<T_HR_KPIPOINT> KPIPointList, string stepname, FLOW_MODELFLOWRELATION_T modelFlowRelation)
        {
            InitializeComponent();

            if (KPIPointList != null && KPIPointList.Count != 0 && stepname != null)
            {
                //获取KPI点信息
                foreach (T_HR_KPIPOINT point in KPIPointList)
                {
                    if (stepname.Equals(point.STEPID))
                    {
                        KPIPoint = point;
                        break;
                    }
                }
            }
            // 1s 冉龙军
            ////设置KPI点信息为不可用
            //SetPointInfoEnable(false);
            ////设置KPI点信息
            //SetControlsEnable(false);
            // 1e
            //设置系统评分信息为不可用
            SetSystemScoreEnable(false);
            //设置提醒信息为不可用
            SetRemindEnable(false);
            //抽查组
            chkIsRandom.IsEnabled = false;
            // 1s 冉龙军
            txtRandomWeight.IsEnabled = false;
            cboRandomGroup.IsEnabled = false;
            //机打
            chkIsMachine.IsEnabled = false;
            txtMachineWeight.IsEnabled = false;
            //人打
            chkIsPerson.IsEnabled = false;
            txtPersonWeight.IsEnabled = false;
            // 1e

            // 1s 冉龙军
            chkIsSetKPI.IsChecked = KPIPoint == null ? false : true;
            // 1e
            cboIsSetKPI.SelectedIndex = KPIPoint == null ? 1 : 0;
            this.FormType = type;
            this.stepname = stepname;
            this.modelFlowRelation = modelFlowRelation;
            //if (KPIPoint == null)
            //{
            //}

            //设置节点名称
            lblStatusName.Text = stepname;
            InitPara(stepname);
        }

        /// <summary>
        /// 初始化界面参数
        /// </summary>
        /// <param name="kpiTypeID"></param>
        private void InitPara(string kpiTypeID)
        {
            //KPIPOINT事件
            //加载事件 
            //client.GetKPITypeAllCompleted += new EventHandler<GetKPITypeAllCompletedEventArgs>(client_GetKPITypeAllCompleted);
            client.GetKPITypeByIDCompleted += new EventHandler<GetKPITypeByIDCompletedEventArgs>(client_GetKPITypeByIDCompleted);
            client.GetRandomGroupAllCompleted += new EventHandler<GetRandomGroupAllCompletedEventArgs>(client_GetRandomGroupAllCompleted);
            client.AddKPIPointCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AddKPIPointCompleted);
            client.UpdateKPIPointAndRemindCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_UpdateKPIPointAndRemindCompleted);
            client.GetKPITypeWithPermissionCompleted += new EventHandler<GetKPITypeWithPermissionCompletedEventArgs>(client_GetKPITypeWithPermissionCompleted);
            PermissionServiceWcf.GetSysRoleInfosCompleted += new EventHandler<GetSysRoleInfosCompletedEventArgs>(client_GetSysRoleInfosCompleted);

            this.Loaded += new RoutedEventHandler(KPITypeInfo_Loaded);
            // 1s 冉龙军
            BindItemContainer(itemContainer1);
            BindItemContainer(itemContainer2);
            BindItemContainer(itemContainer3);
            // 1e
            //窗口状态
            switch ((int)FormType)
            {
                case 0://NEW
                    break;
                case 1://EDIT
                    break;
                case 2: //BROWE
                    break;
                case 3: //ADUIT
                    break;
            }
        }



        void KPITypeInfo_Loaded(object sender, RoutedEventArgs e)
        {
            client.GetRandomGroupAllAsync();
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            // client.GetKPITypeAllAsync();
            client.GetKPITypeWithPermissionAsync("", paras, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            if (StateList.Count == 0)
                PermissionServiceWcf.GetSysRoleInfosAsync("", "");
            else
            {
                // 1s 冉龙军
                //string tmpStateName = (StateList.Where(s => s.StateCode.ToString() == stepname).ToList().First().StateName);
                //lblStatusName.Text = tmpStateName;
                lblStatusName.Text = "";
                // 1e
            }
        }

        /// <summary>
        /// 设置KPI点信息的可用性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetPointInfoEnable(bool isEnable)
        {
            //txtKPIPointName.IsEnabled = isEnable;
            txtKPIPointDESC.IsEnabled = isEnable;
            // 1s 冉龙军
            //chkIsMachine.IsEnabled = isEnable;
            //chkIsPerson.IsEnabled = isEnable;
            // 1e
            //foreach (Control control in KPILayout.Children)
            //{
            //    control.IsEnabled = false;
            //}
        }

        /// <summary>
        /// 选择是否设置KPI点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboIsSetKPI_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboIsSetKPI.SelectedIndex == 0)
            {

                SetControlsEnable(true);
                // 1s 冉龙军
                //SetSystemScoreEnable(chkIsMachine.IsChecked.Value);
                ////SetRandomGroupEnable(chkIsPerson.IsChecked.Value);
                //SetRemindEnable(chkIsRandom.IsChecked.Value);
                // 1e
                if (KPIPoint == null)
                {
                    KPIPointFactory();
                }

                //窗口绑定
                BindData();
            }
            else
            {
                //设置KPI点信息
                SetControlsEnable(false);
                //设置系统评分信息为不可用
                SetSystemScoreEnable(false);
                //设置抽查信息为不可用
                //SetRandomGroupEnable(false);
                //设置提醒信息为不可用
                SetRemindEnable(false);
            }
        }

        private void cboKPIType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboKPIType.SelectedIndex != -1)
            {
                //获取类别
                // 1s 冉龙军
                //CopyScoreType(((T_HR_KPITYPE)cboKPIType.SelectedItem),KPIPoint);
                T_HR_SCORETYPE temp = (cboKPIType.SelectedItem as T_HR_KPITYPE).T_HR_SCORETYPE;
                try
                {
                    KPIPoint.T_HR_SCORETYPE = temp;
                    KPIPoint.T_HR_SCORETYPE.SCORETYPEID = temp.SCORETYPEID;
                }
                catch (Exception ex)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                }
                // 1e
                BindData();
            }
        }

        /// <summary>
        /// 把KPI类型中的评分标准拷贝到KPI点中
        /// </summary>
        /// <param name="kpiType">KPI类型</param>
        /// <param name="kpiPoint">KPI点</param>
        private void CopyScoreType(T_HR_KPITYPE kpiType, T_HR_KPIPOINT kpiPoint)
        {
            kpiPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP = kpiType.T_HR_SCORETYPE.T_HR_RANDOMGROUP;
            kpiPoint.T_HR_SCORETYPE.ISSYSTEMSCORE = kpiType.T_HR_SCORETYPE.ISSYSTEMSCORE;
            kpiPoint.T_HR_SCORETYPE.SYSTEMWEIGHT = kpiType.T_HR_SCORETYPE.SYSTEMWEIGHT;
            kpiPoint.T_HR_SCORETYPE.ISMANUALSCORE = kpiType.T_HR_SCORETYPE.ISMANUALSCORE;
            kpiPoint.T_HR_SCORETYPE.MANUALWEIGHT = kpiType.T_HR_SCORETYPE.MANUALWEIGHT;
            kpiPoint.T_HR_SCORETYPE.ISRANDOMSCORE = kpiType.T_HR_SCORETYPE.ISRANDOMSCORE;
            kpiPoint.T_HR_SCORETYPE.RANDOMWEIGHT = kpiType.T_HR_SCORETYPE.RANDOMWEIGHT;
            kpiPoint.T_HR_SCORETYPE.INITIALPOINT = kpiType.T_HR_SCORETYPE.INITIALPOINT;
            kpiPoint.T_HR_SCORETYPE.INITIALSCORE = kpiType.T_HR_SCORETYPE.INITIALSCORE;
            kpiPoint.T_HR_SCORETYPE.COUNTUNIT = kpiType.T_HR_SCORETYPE.COUNTUNIT;
            // 1s 冉龙军
            kpiPoint.T_HR_SCORETYPE.LATERUNIT = kpiType.T_HR_SCORETYPE.LATERUNIT;
            // 1e
            kpiPoint.T_HR_SCORETYPE.ADDSCORE = kpiType.T_HR_SCORETYPE.ADDSCORE;
            kpiPoint.T_HR_SCORETYPE.REDUCESCORE = kpiType.T_HR_SCORETYPE.REDUCESCORE;
            kpiPoint.T_HR_SCORETYPE.MAXSCORE = kpiType.T_HR_SCORETYPE.MAXSCORE;
            kpiPoint.T_HR_SCORETYPE.MINSCORE = kpiType.T_HR_SCORETYPE.MINSCORE;
            if (kpiPoint.T_HR_SCORETYPE.T_HR_KPIREMIND == null)
                kpiPoint.T_HR_SCORETYPE.T_HR_KPIREMIND = new ObservableCollection<T_HR_KPIREMIND>();
            else
                kpiPoint.T_HR_SCORETYPE.T_HR_KPIREMIND.Clear();
            foreach (T_HR_KPIREMIND remind in kpiType.T_HR_SCORETYPE.T_HR_KPIREMIND)
            {
                remind.REMINDID = Guid.NewGuid().ToString();
                kpiPoint.T_HR_SCORETYPE.T_HR_KPIREMIND.Add(remind);
            }
        }


        /// <summary>
        /// 生产一个新的KPIType
        /// </summary>
        private void KPIPointFactory()
        {
            KPIPoint = new T_HR_KPIPOINT();

            // 1s 冉龙军
            //KPIPoint.T_HR_SCORETYPE = new T_HR_SCORETYPE();
            //KPIPoint.T_HR_SCORETYPE.SCORETYPEID = Guid.NewGuid().ToString();
            // 1e
            KPIPoint.KPIPOINTID = "-1";
            KPIPoint.SYSTEMID = modelFlowRelation.COMPANYID;//公司ID
            KPIPoint.BUSINESSID = modelFlowRelation.MODELFLOWRELATIONID;//业务ID
            // 1s 冉龙军
            //KPIPoint.FLOWID = modelFlowRelation.FLOW_FLOWDEFINE_T.FLOWCODE;//流程ID
            KPIPoint.FLOWID = modelFlowRelation.FLOW_MODELDEFINE_T.DESCRIPTION;//流程描述
            // 1e
            KPIPoint.STEPID = stepname;//步骤ID
            KPIPoint.SUMTYPE = "0";//汇总类型是0：“流程”。

            KPIPoint.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            KPIPoint.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            KPIPoint.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            KPIPoint.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            KPIPoint.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            KPIPoint.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            KPIPoint.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            KPIPoint.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            KPIPoint.CREATEDATE = DateTime.Now;
            KPIPoint.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            KPIPoint.UPDATEDATE = DateTime.Now;
        }

        /// <summary>
        /// 绑定窗口数据
        /// </summary>
        private void BindData()
        {
            //窗口绑定
            LayoutRoot.DataContext = KPIPoint;
            //评分标准绑定
            //expander.DataContext = KPIType;

            //绑定评分方式数据
            //AppraisalType.DataContext = KPIPoint.T_HR_SCORETYPE;
            //是否系统评分
            if (KPIPoint != null && KPIPoint.T_HR_SCORETYPE != null)
            {
                if (KPIPoint.T_HR_SCORETYPE.ISSYSTEMSCORE != null)
                {
                    chkIsMachine.IsChecked = KPIPoint.T_HR_SCORETYPE.ISSYSTEMSCORE.Trim().Equals("1") ? true : false;
                    chkIsMachine_Click(null, null);
                }

                if (KPIPoint.T_HR_SCORETYPE.ISMANUALSCORE != null)
                {
                    chkIsPerson.IsChecked = KPIPoint.T_HR_SCORETYPE.ISMANUALSCORE.Trim().Equals("1") ? true : false;
                    chkIsPerson_Click(null, null);
                }

                if (KPIPoint.T_HR_SCORETYPE.ISRANDOMSCORE != null)
                {
                    chkIsRandom.IsChecked = KPIPoint.T_HR_SCORETYPE.ISRANDOMSCORE.Trim().Equals("1") ? true : false;
                    chkIsRandom_Click(null, null);
                }
            }

            //绑定抽查组
            if (chkIsRandom.IsChecked.Value)
                for (int i = 0; i < cboRandomGroup.Items.Count; i++)
                {
                    if (KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null &&
                        ((T_HR_RANDOMGROUP)cboRandomGroup.Items[i]).RANDOMGROUPID.Equals(KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID))
                    {
                        cboRandomGroup.SelectedIndex = i;
                        break;
                    }
                }

            if (KPIPoint != null && KPIPoint.T_HR_SCORETYPE != null && KPIPoint.T_HR_SCORETYPE.T_HR_KPIREMIND != null)
            {
                //绑定提醒信息
                IEnumerator OperandEnum = KPIPoint.T_HR_SCORETYPE.T_HR_KPIREMIND.OrderBy(s => s.FORWARDHOURS).GetEnumerator();
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
                        // 1s 冉龙
                        //cboRemindType1.SelectedIndex = int.Parse(Remind.REMINDTYPE.Trim());

                        if (!string.IsNullOrWhiteSpace(Remind.REMINDTYPE))
                        {
                            SetItemContainerCheck(itemContainer1, Remind.REMINDTYPE);
                        }
                        else
                        {
                            SetItemContainerNoCheck(itemContainer1);
                        }
                        // 1e
                        txtForwardHours1.Value = (Double)Remind.FORWARDHOURS;
                        remind1 = Remind;
                    }
                    //第二条提醒
                    else if (CharCount == 2)
                    {
                        chkIsRemind2.IsChecked = true;
                        // 1s 冉龙
                        //cboRemindType2.SelectedIndex = int.Parse(Remind.REMINDTYPE.Trim());

                        if (!string.IsNullOrWhiteSpace(Remind.REMINDTYPE))
                        {
                            SetItemContainerCheck(itemContainer2, Remind.REMINDTYPE);
                        }
                        else
                        {
                            SetItemContainerNoCheck(itemContainer2);
                        }
                        // 1e

                        txtForwardHours2.Value = (Double)Remind.FORWARDHOURS;
                        remind2 = Remind;
                    }
                    //第三条提醒
                    else if (CharCount == 3)
                    {
                        chkIsRemind3.IsChecked = true;
                        // 1s 冉龙
                        //cboRemindType3.SelectedIndex = int.Parse(Remind.REMINDTYPE.Trim());

                        if (!string.IsNullOrWhiteSpace(Remind.REMINDTYPE))
                        {
                            SetItemContainerCheck(itemContainer3, Remind.REMINDTYPE);
                        }
                        else
                        {
                            SetItemContainerNoCheck(itemContainer3);
                        }
                        // 1e
                        txtForwardHours3.Value = (Double)Remind.FORWARDHOURS;
                        remind3 = Remind;
                    }
                    else
                        break;
                }
            }
        }
        // 1s 冉龙军
        /// <summary>
        /// 设置ItemControl选定项
        /// </summary>
        /// <param name="cboRemindList"></param>
        /// <param name="strRemindType"></param>
        private void SetItemContainerCheck(ItemsControl itemContainer, string strRemindType)
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

            List<CheckBoxModel> entList = itemContainer.ItemsSource as List<CheckBoxModel>;

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

            Utility.BindItemContainerList(entList, itemContainer);
        }
        /// <summary>
        /// 设置ItemControl选定项为空
        /// </summary>
        /// <param name="cboRemindList"></param>
        /// <param name="strRemindType"></param>
        private void SetItemContainerNoCheck(ItemsControl itemContainer)
        {
            List<CheckBoxModel> entList = itemContainer.ItemsSource as List<CheckBoxModel>;

            if (entList == null)
            {
                return;
            }

            if (entList.Count() == 0)
            {
                return;
            }

            foreach (CheckBoxModel item in entList)
            {
                item.ISCHECKED = false;
            }

            Utility.BindItemContainerList(entList, itemContainer);
        }
        /// <summary>
        /// 绑定ItemControl
        /// </summary>
        /// <param name="chk"></param>
        private void BindItemContainer(ItemsControl itemContainer)
        {
            List<CheckBoxModel> entlist = new List<CheckBoxModel>();
            entlist.Add(new CheckBoxModel(1, "IMMEDIATELYCOMMUNICATION", false));
            entlist.Add(new CheckBoxModel(2, "SMTGATE", false));
            entlist.Add(new CheckBoxModel(3, "EMAIL", false));
            entlist.Add(new CheckBoxModel(4, "PHONEMESSAGE", false));
            Utility.BindItemContainerList(entlist, itemContainer);
        }
        // 1e
        /// <summary>
        /// 设置窗口控件可用性
        /// </summary>
        void SetControlsEnable(bool isEnable)
        {
            //窗口控件
            txtKPIPointName.IsEnabled = isEnable;
            txtKPIPointDESC.IsEnabled = isEnable;
            // 1s 冉龙军
            //chkIsMachine.IsEnabled = isEnable;
            //chkIsPerson.IsEnabled = isEnable;
            //chkIsRandom.IsEnabled = isEnable && chkIsPerson.IsChecked.Value ? true : false;
            //txtMachineWeight.IsEnabled = isEnable && chkIsMachine.IsChecked.Value ? true : false;
            //txtPersonWeight.IsEnabled = isEnable && chkIsPerson.IsChecked.Value ? true : false;
            //txtRandomWeight.IsEnabled = isEnable && chkIsRandom.IsChecked.Value ? true : false;
            //cboRandomGroup.IsEnabled = isEnable && chkIsRandom.IsChecked.Value ? true : false;
            // 1e
            cboKPIType.IsEnabled = isEnable;
            //txtKPIPointName.IsEnabled = isEnable;
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

        /// <summary>
        /// 设置抽查相关控件可用性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetRandomGroupEnable(bool isEnable)
        {
            chkIsRandom.IsEnabled = isEnable;
            txtRandomWeight.IsEnabled = isEnable;
            cboRandomGroup.IsEnabled = isEnable;
            //如果需要手动评分，那么抽查保持原装，如果不用手动评分，那么抽查评分也要去掉
            chkIsRandom.IsChecked = chkIsPerson.IsChecked == true ? chkIsRandom.IsChecked : false;
        }

        /// <summary>
        /// 设置提醒窗口控件可用性
        /// </summary>
        void SetRemindEnable(bool isEnable)
        {
            //expanderRemind
            chkIsRemind1.IsEnabled = isEnable;
            chkIsRemind2.IsEnabled = isEnable;
            chkIsRemind3.IsEnabled = isEnable;
            cboRemindType1.IsEnabled = isEnable;
            cboRemindType2.IsEnabled = isEnable;
            cboRemindType3.IsEnabled = isEnable;
            txtForwardHours1.IsEnabled = isEnable;
            txtForwardHours2.IsEnabled = isEnable;
            txtForwardHours3.IsEnabled = isEnable;
            // 1s 冉龙军
            itemContainer1.IsEnabled = isEnable;
            itemContainer2.IsEnabled = isEnable;
            itemContainer3.IsEnabled = isEnable;
            // 1e
        }

        /// <summary>
        /// 保存页面信息
        /// </summary>
        private void Save()
        {
            //处理页面验证
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
            }
            if (CheckKPIInfo())
            {
                //KPI类别
                KPIPoint.KPIPOINTNAME = txtKPIPointName.Text.Trim();
                KPIPoint.KPIPOINTREMARK = txtKPIPointDESC.Text.Trim();

                #region 评分方式
                // 1s 冉龙军
                ////评分方式
                //if (chkIsMachine.IsChecked.Value)
                //{
                //    try
                //    {
                //        //需要进行数据验证
                //        //系统评分
                //        KPIPoint.T_HR_SCORETYPE.ISSYSTEMSCORE = "1";
                //        KPIPoint.T_HR_SCORETYPE.SYSTEMWEIGHT = int.Parse(txtMachineWeight.Text.Trim());
                //        //系统评分标准
                //        KPIPoint.T_HR_SCORETYPE.INITIALPOINT = int.Parse(txtInitailDate.Text.Trim()); //计分原点
                //        KPIPoint.T_HR_SCORETYPE.INITIALSCORE = int.Parse(txtInitailScore.Text.Trim());//初始分
                //        KPIPoint.T_HR_SCORETYPE.COUNTUNIT = int.Parse(txtScoreUnit.Text.Trim());      //单位天数  
                //        KPIPoint.T_HR_SCORETYPE.ADDSCORE = int.Parse(txtAddForForward.Text.Trim());   //加分 
                //        KPIPoint.T_HR_SCORETYPE.REDUCESCORE = int.Parse(txtReduceForDelay.Text.Trim());//减分 
                //        KPIPoint.T_HR_SCORETYPE.MAXSCORE = int.Parse(txtMaxScore.Text.Trim());         //上限 
                //        KPIPoint.T_HR_SCORETYPE.MINSCORE = int.Parse(txtMinScore.Text.Trim());         //下限 
                //    }
                //    catch
                //    {
                //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " string Convert int error.");
                //    }
                //}
                //else
                //    KPIPoint.T_HR_SCORETYPE.ISSYSTEMSCORE = "0";

                //if (chkIsPerson.IsChecked.Value)
                //{
                //    //手动评分
                //    KPIPoint.T_HR_SCORETYPE.ISMANUALSCORE = "1";
                //    KPIPoint.T_HR_SCORETYPE.MANUALWEIGHT = int.Parse(txtPersonWeight.Text.Trim());
                //}
                //else
                //    KPIPoint.T_HR_SCORETYPE.ISMANUALSCORE = "0";

                //if (chkIsRandom.IsChecked.Value)
                //{
                //    //抽查评分
                //    KPIPoint.T_HR_SCORETYPE.ISRANDOMSCORE = "1";
                //    KPIPoint.T_HR_SCORETYPE.RANDOMWEIGHT = int.Parse(txtRandomWeight.Text.Trim());
                //    //抽查组信息
                //    KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP = cboRandomGroup.SelectedItem as T_HR_RANDOMGROUP;
                //}
                //else
                //    KPIPoint.T_HR_SCORETYPE.ISRANDOMSCORE = "0";

                // 1e
                #endregion 评分方式

                //提醒信息

                KPIPoint.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                KPIPoint.UPDATEDATE = DateTime.Now;

                if (KPIPoint.KPIPOINTID != "-1")
                {
                    KPIPoint.FLOWID = modelFlowRelation.FLOW_MODELDEFINE_T.DESCRIPTION;
                    if (chkIsSetKPI.IsChecked != true)
                    {
                        KPIPoint.BUSINESSID = string.Empty;
                        KPIPoint.FLOWID = string.Empty;
                        KPIPoint.STEPID = string.Empty;
                    }
                    client.UpdateKPIPointAndRemindAsync(KPIPoint, GetRemindList("add"), GetRemindList("update"), GetRemindList("del"));
                }
                else
                {
                    KPIPoint.KPIPOINTID = Guid.NewGuid().ToString();
                    // 1s 冉龙军
                    //KPIPoint.T_HR_SCORETYPE.T_HR_KPIREMIND = GetRemindList("add");
                    // 1e
                    client.AddKPIPointAsync(KPIPoint);
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
            if (chkIsSetKPI.IsChecked != true && KPIPoint == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("请在设置KPI考核点前打勾"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("请在设置KPI考核点前打勾"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (cboKPIType.SelectedIndex < 0)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "KPITYPE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "KPITYPE"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (string.IsNullOrEmpty(txtKPIPointDESC.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "KPIPOINTDESC"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "KPIPOINTDESC"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (cboIsSetKPI.SelectedIndex == 1)
                return true;

            if (!chkIsMachine.IsChecked.Value && !chkIsPerson.IsChecked.Value && !chkIsRandom.IsChecked.Value)
            {
                //必须选择一种或多种评分方式
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SCORETYPEMAST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SCORETYPEMAST"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (txtInitailDate.Value.Equals("") || txtInitailScore.Value.Equals("") || txtScoreUnit.Value.Equals("")
                   || txtLaterUnit.Value.Equals("") || txtAddForForward.Value.Equals("") || txtReduceForDelay.Value.Equals("")
                   || txtMaxScore.Value.Equals("") || txtMinScore.Value.Equals(""))
            {
                //必须填写完整的系统评分标准
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须填写完整的系统评分标准");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须填写完整的系统评分标准"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (chkIsMachine.IsChecked.Value)
            {
                if (txtMachineWeight.Value.Equals(""))
                {
                    //必须填写系统评分权重
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须填写系统评分权重");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须填写系统评分权重"),
        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

            }
            if (chkIsPerson.IsChecked.Value)
            {
                if (txtPersonWeight.Value.Equals(""))
                {
                    //必须填写手动评分权重
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须填写手动评分权重");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须填写手动评分权重"),
      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
            }
            if (chkIsRandom.IsChecked.Value)
            {
                if (txtRandomWeight.Value.Equals(""))
                {
                    //必须填写抽查评分权重
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须填写抽查评分权重");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须填写抽查评分权重"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                if (cboRandomGroup.SelectedIndex == -1)
                {
                    //必须选择抽查组
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "必须选择抽查组");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("必须选择抽查组"),
   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
            }
            // 1s 冉龙军
            //if (chkIsRemind1.IsChecked.Value && (cboRemindType1.SelectedIndex == -1 || txtForwardHours1.Text.Trim().Equals("")))
            //{
            //    //提醒一信息不完整
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "提醒一信息不完整");
            //    return false;
            //}
            //if (chkIsRemind2.IsChecked.Value && (cboRemindType2.SelectedIndex == -1 || txtForwardHours2.Text.Trim().Equals("")))
            //{
            //    //提醒二信息不完整
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "提醒二信息不完整");
            //    return false;
            //}
            //if (chkIsRemind3.IsChecked.Value && (cboRemindType3.SelectedIndex == -1 || txtForwardHours3.Text.Trim().Equals("")))
            //{
            //    //提醒三信息不完整
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "提醒三信息不完整");
            //    return false;
            //}
            if (chkIsRemind1.IsChecked.Value && (txtForwardHours1.Value.Equals("")))
            {
                //提醒一信息不完整
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "提醒一信息不完整");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("提醒一信息不完整"),
 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (chkIsRemind2.IsChecked.Value && (txtForwardHours2.Value.Equals("")))
            {
                //提醒二信息不完整
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "提醒二信息不完整");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("提醒二信息不完整"),
 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (chkIsRemind3.IsChecked.Value && (txtForwardHours3.Value.Equals("")))
            {
                //提醒三信息不完整
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "提醒三信息不完整");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("提醒三信息不完整"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            // 1e
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
                remind1.REMINDTYPE = cboRemindType1.SelectedIndex.ToString();
                remind1.FORWARDHOURS = Convert.ToInt32(txtForwardHours1.Value);
                list.Add(remind1);
            }
            if (chkIsRemind2.IsChecked.Value == isCheck && remind2 != null && remind2.REMINDID.Equals("-1") == isNew)
            {
                if (remindType.Equals("add"))
                    remind2.REMINDID = Guid.NewGuid().ToString();
                remind2.REMINDTYPE = cboRemindType2.SelectedIndex.ToString();
                remind2.FORWARDHOURS = Convert.ToInt32(txtForwardHours2.Value);
                list.Add(remind2);
            }
            if (chkIsRemind3.IsChecked.Value == isCheck && remind3 != null && remind3.REMINDID.Equals("-1") == isNew)
            {
                if (remindType.Equals("add"))
                    remind3.REMINDID = Guid.NewGuid().ToString();
                remind3.REMINDTYPE = cboRemindType3.SelectedIndex.ToString();
                remind3.FORWARDHOURS = Convert.ToInt32(txtForwardHours3.Value);
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
        void client_GetKPITypeWithPermissionCompleted(object sender, GetKPITypeWithPermissionCompletedEventArgs e)
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
                cboKPIType.ItemsSource = e.Result.ToList();
                cboKPIType.DisplayMemberPath = "KPITYPENAME";
                // 1s 冉龙军
                if (FormType != FormTypes.New && KPIPoint != null && KPIPoint.T_HR_SCORETYPE != null)
                {
                    cboKPIType.SelectedItem = e.Result.ToList().Where(T_HR_KPITYPE => T_HR_KPITYPE.T_HR_SCORETYPE.SCORETYPEID == KPIPoint.T_HR_SCORETYPE.SCORETYPEID).First();
                }
                // 1e
            }
        }
        private void client_GetKPITypeAllCompleted(object sender, GetKPITypeAllCompletedEventArgs e)
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
                cboKPIType.ItemsSource = e.Result.ToList();
                cboKPIType.DisplayMemberPath = "KPITYPENAME";
                // 1s 冉龙军
                if (FormType != FormTypes.New && KPIPoint != null && KPIPoint.T_HR_SCORETYPE != null)
                {
                    cboKPIType.SelectedItem = e.Result.ToList().Where(T_HR_KPITYPE => T_HR_KPITYPE.T_HR_SCORETYPE.SCORETYPEID == KPIPoint.T_HR_SCORETYPE.SCORETYPEID).First();
                }
                // 1e
            }
        }

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
                //KPIPoint = e.Result;
                //BindData();
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
                if (FormType != FormTypes.New && chkIsRandom.IsChecked.Value && KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                    for (int i = 0; i < cboRandomGroup.Items.Count; i++)
                    {
                        if (((T_HR_RANDOMGROUP)cboRandomGroup.Items[i]).RANDOMGROUPID.Equals(KPIPoint.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID))
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
        void client_AddKPIPointCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.Error.Message == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPIPOINTNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPIPOINTNAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                }
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "KPIPOINT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
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
        void client_UpdateKPIPointAndRemindCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.Error.Message == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPIPOINTNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "KPIPOINTNAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "KPIPOINT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                RefreshUI(RefreshedTypes.All);


                lblUserName.Visibility = Visibility.Visible;
                //txtKPIPointName.Width = 140;

            }
        }

        /// <summary>
        /// 獲取角色的東東
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetSysRoleInfosCompleted(object sender, GetSysRoleInfosCompletedEventArgs e)
        {

            if (e.Result != null)
            {
                StateList.Clear();

                List<T_SYS_ROLE> dt = e.Result.ToList<T_SYS_ROLE>(); ;

                for (int i = 0; i < dt.Count; i++)
                {
                    StateType tmp = new StateType();
                    tmp.StateCode = "State" + new Guid(dt[i].ROLEID).ToString("N");
                    tmp.StateName = dt[i].ROLENAME;
                    StateList.Add(tmp);
                }
                // 1s 冉龙军
                //string tmpStateName = (StateList.Where(s => s.StateCode.ToString() == stepname).ToList().First().StateName);
                //lblStatusName.Text = tmpStateName;

                lblStatusName.Text = "";
                // 1e
            }
        }

        #region CheckBox事件
        // 1s 冉龙军
        /// <summary>
        /// 是否设置KPI点CheckBox的Click事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkIsSetKPI_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsSetKPI.IsChecked.Value)
            {
                cboIsSetKPI.SelectedIndex = 0;
            }
            else
            {
                cboIsSetKPI.SelectedIndex = 1;
            }
        }
        // 1e
        private void chkIsMachine_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1s 冉龙军
            //txtMachineWeight.IsEnabled = chkIsMachine.IsChecked.Value;
            //SetSystemScoreEnable(chkIsMachine.IsChecked.Value);
            //if (KPIPoint.T_HR_SCORETYPE.INITIALPOINT == null && KPIPoint.T_HR_SCORETYPE.INITIALSCORE == null)
            //{
            //    KPIPoint.T_HR_SCORETYPE.INITIALPOINT = 3;
            //    KPIPoint.T_HR_SCORETYPE.INITIALSCORE = 80;
            //    KPIPoint.T_HR_SCORETYPE.COUNTUNIT = 1;
            //    KPIPoint.T_HR_SCORETYPE.ADDSCORE = 10;
            //    KPIPoint.T_HR_SCORETYPE.REDUCESCORE = 10;
            //    KPIPoint.T_HR_SCORETYPE.MAXSCORE = 100;
            //    KPIPoint.T_HR_SCORETYPE.MINSCORE = 100;
            //}
            // 1e
        }

        private void chkIsPerson_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1s 冉龙军
            //txtPersonWeight.IsEnabled = chkIsPerson.IsChecked.Value;
            //SetRandomGroupEnable(chkIsPerson.IsChecked.Value);
            //chkIsRandom_Click(null, null);
            // 1e
        }

        private void chkIsRandom_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1s 冉龙军
            //txtRandomWeight.IsEnabled = chkIsRandom.IsChecked.Value;
            //cboRandomGroup.IsEnabled = chkIsRandom.IsChecked.Value;
            //SetRemindEnable(chkIsRandom.IsChecked.Value);
            // 1e
        }

        private void chkIsRemind1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1s 冉龙军
            //if (chkIsRemind1.IsChecked.Value && remind1 == null)
            //{
            //    remind1 = new T_HR_KPIREMIND();
            //    remind1.REMINDID = "-1";
            //    if (KPIPoint != null && KPIPoint.T_HR_SCORETYPE != null)
            //        remind1.T_HR_SCORETYPE = KPIPoint.T_HR_SCORETYPE;
            //}
            // 1e
        }

        private void chkIsRemind2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1s 冉龙军
            //if (chkIsRemind2.IsChecked.Value && remind2 == null)
            //{
            //    remind2 = new T_HR_KPIREMIND();
            //    remind2.REMINDID = "-1";
            //    if (KPIPoint != null && KPIPoint.T_HR_SCORETYPE != null)
            //        remind2.T_HR_SCORETYPE = KPIPoint.T_HR_SCORETYPE;
            //}
            // 1e
        }

        private void chkIsRemind3_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1s 冉龙军
            //if (chkIsRemind3.IsChecked.Value && remind3 == null)
            //{
            //    remind3 = new T_HR_KPIREMIND();
            //    remind3.REMINDID = "-1";
            //    if (KPIPoint != null && KPIPoint.T_HR_SCORETYPE != null)
            //        remind3.T_HR_SCORETYPE = KPIPoint.T_HR_SCORETYPE;
            //}
            //1e
        }

        #endregion CheckBox事件

        #endregion 所有事件

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("KPIPOINT");
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
