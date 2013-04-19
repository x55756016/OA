/********************************************************************************
//出差申请form，alter by ken 2013/3/27
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Input;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;

namespace SMT.SaaS.OA.UI.Views.Travelmanagement
{
    public partial class  TravelRequestForm
    {
        #region 保存按钮
        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();

            IsSubmit = false;//点击了提交按钮
            needsubmit = true;
            isSubmit = true;
            Save();
        }
        #endregion

        #region 出差时间计算
        public void TravelTimeCalculation()
        {
            if (TraveDetailList_Golbal == null || DaGrs.ItemsSource == null)
            {
                return;
            }
            #region 存在多条的处理
            TextBox myDaysTime = new TextBox();
            bool OneDayTrave = false;
            for (int i = 0; i < TraveDetailList_Golbal.Count; i++)
            {
                GetTraveDayTextBox(myDaysTime, i).Text = string.Empty;
                OneDayTrave = false;
                //记录本条记录以便处理
                DateTime FirstStartTime = Convert.ToDateTime(TraveDetailList_Golbal[i].STARTDATE);
                DateTime FirstEndTime = Convert.ToDateTime(TraveDetailList_Golbal[i].ENDDATE);
                string FirstTraveFrom = TraveDetailList_Golbal[i].DEPCITY;
                string FirstTraveTo = TraveDetailList_Golbal[i].DESTCITY;
                //遍历剩余的记录
                for (int j = i + 1; j < TraveDetailList_Golbal.Count; j++)
                {
                    DateTime NextStartTime = Convert.ToDateTime(TraveDetailList_Golbal[j].STARTDATE);
                    DateTime NextEndTime = Convert.ToDateTime(TraveDetailList_Golbal[j].ENDDATE);
                    string NextTraveFrom = TraveDetailList_Golbal[j].DEPCITY;
                    string NextTraveTo = TraveDetailList_Golbal[j].DESTCITY;
                    GetTraveDayTextBox(myDaysTime, j).Text = string.Empty;
                    if (NextEndTime.Date == FirstStartTime.Date)
                    {
                        if (NextTraveTo == FirstTraveFrom)
                        {
                            myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                            myDaysTime.Text = "1";
                            i = j - 1;
                            OneDayTrave = true;
                            break;
                        }
                        else continue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (OneDayTrave == true) continue;
                //非当天往返
                decimal TotalDays = 0;
                switch (TraveDetailList_Golbal.Count())
                {
                    case 1:
                        TotalDays = CaculateTravDays(FirstStartTime, FirstEndTime);
                        myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    case 2:
                        if (i == 1) break;
                        DateTime NextEndTime = Convert.ToDateTime(TraveDetailList_Golbal[i + 1].ENDDATE);
                        TotalDays = CaculateTravDays(FirstStartTime, NextEndTime);
                        myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    default:
                        if (i == TraveDetailList_Golbal.Count() - 1) break;//最后一条记录不处理
                        if (i == TraveDetailList_Golbal.Count() - 2)//倒数第二条记录=最后一条结束时间-上一条开始时间
                        {
                            DateTime NextENDDATETime = Convert.ToDateTime(TraveDetailList_Golbal[i + 1].ENDDATE);
                            TotalDays = CaculateTravDays(FirstStartTime, NextENDDATETime);
                            myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                            myDaysTime.Text = TotalDays.ToString();
                            break;
                        }
                        //否则出差时间=下一条开始时间-上一条开始时间
                        DateTime NextStartTime = Convert.ToDateTime(TraveDetailList_Golbal[i + 1].STARTDATE);
                        TotalDays = CaculateTravDays(FirstStartTime, NextStartTime);
                        myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                }
            }
            #endregion
        }
        /// <summary>
        /// 获取出差申请每列后面的出差天数文本框
        /// </summary>
        /// <param name="txtDaysCount">出差天数文本框</param>
        /// <param name="i">行数</param>
        /// <returns></returns>
        private TextBox GetTraveDayTextBox(TextBox txtDaysCount, int i)
        {
            if (DaGrs.Columns[6].GetCellContent(TraveDetailList_Golbal[i]) != null)
            {
                txtDaysCount = DaGrs.Columns[6].GetCellContent(TraveDetailList_Golbal[i]).FindName("txtTOTALDAYS") as TextBox;
            }
            return txtDaysCount;
        }
        /// <summary>
        /// 计算出差时长结算-开始时间NextStartTime-FirstStartTime
        /// </summary>
        /// <param name="FirstStartTime">开始时间</param>
        /// <param name="NextStartTime">结束时间</param>
        /// <returns></returns>
        private decimal CaculateTravDays(DateTime FirstStartTime, DateTime NextStartTime)
        {
            //计算出差时间（天数）
            TimeSpan TraveDays = NextStartTime.Subtract(FirstStartTime);
            decimal TotalDays = 0;//出差天数
            decimal TotalHours = 0;//出差小时
            TotalDays = TraveDays.Days;
            TotalHours = TraveDays.Hours;
            int customhalfday = travelsolutions_Golbal.CUSTOMHALFDAY.ToInt32();
            if (TotalHours >= customhalfday)//如果出差时间大于等于方案设置的时间，按方案标准时间计算
            {
                TotalDays += 1;
            }
            else
            {
                if (TotalHours > 0)
                    TotalDays += Convert.ToDecimal("0.5");//TotalDays += decimal.Round(TotalHours / 24,1);
            }
            return TotalDays;
        }
        #endregion

        #region 字段赋值
        /// <summary>
        /// 字段赋值
        /// </summary>
        private void SetTraveRequestValue()
        {
            //计算出差时间
            TravelTimeCalculation();

            businesstripMaster_Golbal.TEL = this.txtTELL.Text;//联系电话;
            businesstripMaster_Golbal.CHARGEMONEY = 0;//费用;
            businesstripMaster_Golbal.ISAGENT = (bool)ckEnabled.IsChecked ? "1" : "0";//是否启用代理
            businesstripMaster_Golbal.CONTENT = this.txtSubject.Text;//出差事由
            NewDetail();
            string StartCity = string.Empty;//出发城市
            string EndCity = string.Empty;//目标城市
            string StartTime = string.Empty;//开始时间
            string EndTime = string.Empty;//结束时间

            if (TraveDetailList_Golbal.Count() > 0)
            {
                if (TraveDetailList_Golbal.Count() == 1)//只有一条数据
                {
                    StartCity = TraveDetailList_Golbal[0].DEPCITY.ToString();
                    EndCity = TraveDetailList_Golbal[0].DESTCITY.ToString();
                    StartTime = TraveDetailList_Golbal[0].STARTDATE.ToString();
                    EndTime = TraveDetailList_Golbal[0].ENDDATE.ToString();
                }
                if (TraveDetailList_Golbal.Count() > 1)
                {
                    StartCity = TraveDetailList_Golbal[0].DEPCITY.ToString();
                    EndCity = TraveDetailList_Golbal[TraveDetailList_Golbal.Count() - 1].DEPCITY.ToString();
                    StartTime = TraveDetailList_Golbal[0].STARTDATE.ToString();
                    EndTime = TraveDetailList_Golbal[TraveDetailList_Golbal.Count() - 1].ENDDATE.ToString();
                }
                businesstripMaster_Golbal.DEPCITY = StartCity;
                businesstripMaster_Golbal.DESTCITY = EndCity;

                businesstripMaster_Golbal.STARTCITYNAME = GetCityName(StartCity);
                businesstripMaster_Golbal.ENDCITYNAME = GetCityName(EndCity);

                businesstripMaster_Golbal.STARTDATE = Convert.ToDateTime(StartTime);
                businesstripMaster_Golbal.ENDDATE = Convert.ToDateTime(EndTime);
            }
            if (actions == FormTypes.New)
            {
                //businesstrip.BUSINESSTRIPID = System.Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(businesstripMaster_Golbal.OWNERID))
                {
                    businesstripMaster_Golbal.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//岗位ID
                    businesstripMaster_Golbal.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//公司ID
                    businesstripMaster_Golbal.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//部门ID
                    businesstripMaster_Golbal.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//员工ID
                    businesstripMaster_Golbal.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName; //员工姓名
                }
                businesstripMaster_Golbal = lkLicenseBorrow.DataContext as T_OA_BUSINESSTRIP;//出差人
                businesstripMaster_Golbal.CREATEDATE = DateTime.Now;//创建时间
                businesstripMaster_Golbal.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交
                businesstripMaster_Golbal.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                businesstripMaster_Golbal.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                businesstripMaster_Golbal.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                businesstripMaster_Golbal.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人ID
                businesstripMaster_Golbal.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名

                //ctrFile.FormID = businesstrip.BUSINESSTRIPID;
                //ctrFile.Save();
            }
            else
            {
                businesstripMaster_Golbal = lkLicenseBorrow.DataContext as T_OA_BUSINESSTRIP;//出差人
                businesstripMaster_Golbal.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                businesstripMaster_Golbal.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                businesstripMaster_Golbal.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交

                //ctrFile.FormID = businesstrip.BUSINESSTRIPID;
                //ctrFile.Save();
            }
            //if (businesstrip.CHARGEMONEY > 0)
            //{
            //    fbCtr.Order.ORDERID = Businesstrip.BUSINESSTRIPID;
            //    fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit);//提交费用
            //}
        }
        #endregion

        #region 验证
        private bool Check()
        {
            string StrStartDt = "";
            string EndDt = "";
            string StrStartTime = "";
            string StrEndTime = "";

            ObservableCollection<T_OA_BUSINESSTRIPDETAIL> entBusinessTripDetails = DaGrs.ItemsSource as ObservableCollection<T_OA_BUSINESSTRIPDETAIL>;
            
            int foreachCount = 0;
            foreach (object obje in DaGrs.ItemsSource)
            {
                TraveDetailOne_Golbal.T_OA_BUSINESSTRIP = Master_Golbal;

                #region "判断出差日期"
                DateTimePicker StartDate = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker EndDate = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;

                if (StartDate.Value != null)
                    StrStartDt = StartDate.Value.Value.ToString("d");//开始日期
                if (EndDate.Value != null)
                    EndDt = EndDate.Value.Value.ToString("d");//结束日期
                if (StartDate.Value != null)
                    StrStartTime = StartDate.Value.Value.ToString("HH:mm");//开始时间
                if (EndDate.Value != null)
                    StrEndTime = EndDate.Value.Value.ToString("HH:mm");//结束时间

                if (string.IsNullOrEmpty(StrStartDt) || string.IsNullOrEmpty(StrStartTime))//开始日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

                if (string.IsNullOrEmpty(EndDt) || string.IsNullOrEmpty(StrEndTime))//结束日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "到达时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
                DateTime DtEnd = System.Convert.ToDateTime(EndDt + " " + StrEndTime);
                if (DtStart >= DtEnd)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发时间不能大于等于到达时间", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                #endregion

                #region "判断出差时间"
                DateTimePicker dpStartTime = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker dpEndTime = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;
                if (dpStartTime.Value != null)
                {
                    TimeSpan tsStart = new TimeSpan(dpStartTime.Value.Value.Hour);
                    if (tsStart == null)//开始时间不能为空
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "STARTDATETIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                    if (dpStartTime.Value.Value.Date == null)//开始日期不能为空
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "STARTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }

                    if (dpEndTime.Value != null)
                    {
                        TimeSpan tsEnd = new TimeSpan(dpEndTime.Value.Value.Hour);
                        if (tsEnd == null)//结束时间不能为空
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "ENDDATETIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return false;
                        }
                        if (dpEndTime.Value.Value.Date == null)//结束日期不能为空
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "ENDDATETITLE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return false;
                        }
                        if (dpStartTime.Value >= dpEndTime.Value)
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return false;
                        }
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "ENDDATETIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "STARTDATETIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

                T_OA_BUSINESSTRIPDETAIL entDetail = obje as T_OA_BUSINESSTRIPDETAIL;

                var queryData = from c in entBusinessTripDetails
                                where c.STARTDATE > dpStartTime.Value && c.ENDDATE > dpEndTime.Value && c.BUSINESSTRIPDETAILID != entDetail.BUSINESSTRIPDETAILID
                                orderby c.STARTDATE
                                select c;

                if (queryData.Count() > 0)
                {
                    if (queryData.FirstOrDefault().STARTDATE < entDetail.ENDDATE)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANNOTBEREPEATEDTOADD", "KPIRECEIVEDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }
                #endregion

                #region "判断交通工具类型"
                TravelDictionaryComboBox ComVechile = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[4].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                if (ComVechile.SelectedIndex <= 0)//交通工具类型
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "TYPEOFTRAVELTOOLS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                #endregion
            }

            #region "判断出差城市"
            for (int i=0; i < citysStartList_Golbal.Count; i++)
            {

                if (string.IsNullOrEmpty(citysStartList_Golbal[i].Trim()) || string.IsNullOrEmpty(citysEndList_Golbal[i].Trim()))
                {
                    //出发城市为空
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发或到达城市不能为空！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                if (citysStartList_Golbal.Count > 1)
                {
                    //如果上下两条出差记录城市一样
                    if (i < citysStartList_Golbal.Count - 1 && citysStartList_Golbal[i] == citysStartList_Golbal[i + 1])
                    {
                        //出发城市与开始城市不能相同
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }
            }
            #endregion  

            #region "其他验证 出差事由不能为空"
            //
            if (string.IsNullOrEmpty(this.txtSubject.Text.Trim()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "TRAVELSUBJECT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                this.txtSubject.Focus();
                return false;
            }

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            #endregion
            return true;
        }
        #endregion

        #region 保存函数
        private void Save()
        {
            try
            {
                if (Check())
                {
                    string FirstStartCity = citysStartList_Golbal[0].Replace(",", "");//出发城市中第一个城市代码
                    string LastEndCity = citysEndList_Golbal[citysEndList_Golbal.Count - 1].Replace(",", "");//目标城市中最后一个城市代码

                    if (FirstStartCity != LastEndCity)
                    {
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            SaveApplication();
                        };
                        com.SelectionBox("提交确认", "起始出发城市和最后的目标城市不一致，是否提交？", ComfirmWindow.titlename, "");
                    }
                    else
                    {
                        SaveApplication();
                    }
                }
                else
                {
                    needsubmit = false;
                    isSubmit = false;
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        private void SaveApplication()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);//点击保存后显示进度条
            SetTraveRequestValue();

            if (actions == FormTypes.New)
            {
                OaPersonOfficeClient.TravelmanagementAddAsync(businesstripMaster_Golbal, TraveDetailList_Golbal);
            }
            else
            {
                UserState = "Edit";
                OaPersonOfficeClient.UpdateTravelmanagementAsync(businesstripMaster_Golbal, TraveDetailList_Golbal, UserState);
            }
        }
        #endregion

        #region 添加Completed
        void Travelmanagement_TravelmanagementAddCompleted(object sender, TravelmanagementAddCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    if (e.Result != "")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        return;
                    }
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EVECTIONFORM"));
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                        ParentEntityBrowser.ParentWindow.Close();
                    }
                    else
                    {
                        actions = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 修改Completed
        void Travelmanagement_UpdateTravelmanagementCompleted(object sender, UpdateTravelmanagementCompletedEventArgs e)
        {
            //RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                OaPersonOfficeClient.GetTravelmanagementByIdAsync(businesstrID);
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    if (e.Result != "")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        return;
                    }
                    if (e.UserState != null && e.UserState.ToString() == "Edit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "EVECTIONFORM"));
                        if (e.UserState != null && e.UserState.ToString() != "Submit")
                        {
                            if (GlobalFunction.IsSaveAndClose(refreshType))
                            {
                                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                                RefreshUI(refreshType);
                                ParentEntityBrowser.ParentWindow.Close();
                            }
                        }
                    }
                    if (e.UserState != null && e.UserState.ToString() == "Audit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    }
                    if (e.UserState != null && e.UserState.ToString() == "Submit" && !isSubmit)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                        isSubmit = false;
                    }

                    RefreshUI(RefreshedTypes.AuditInfo);
                    if (needsubmit == true)
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.ManualSubmit();
                        BackToSubmit();
                    }
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion
    }
}
