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

            //IsSubmit = false;//点击了提交按钮
            needsubmit = true;
            isSubmit = true;
            Save();
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "3":
                    string Result = "";
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        bool FBControl = true;
                        ObservableCollection<string> businesstripId = new ObservableCollection<string>();//出差申请ID
                        businesstripId.Add(Master_Golbal.BUSINESSTRIPID);
                        this.RefreshUI(RefreshedTypes.ShowProgressBar);
                        OaPersonOfficeClient.DeleteTravelmanagementAsync(businesstripId, FBControl);
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), "确认是否删除此条记录？", ComfirmWindow.titlename, Result);

                    break;
            }
        }
        #endregion


        #region 保存出差申请单字段赋值
        /// <summary>
        /// 字段赋值
        /// </summary>
        private void SetTraveRequestMasterValue()
        {
            //计算出差时间
            TravelTimeCalculation();
            //设置出差明细值
            SetTraveRequestDetailValue();

            Master_Golbal.TEL = this.txtTELL.Text;//联系电话;
            Master_Golbal.CHARGEMONEY = 0;//费用;
            Master_Golbal.ISAGENT = (bool)ckEnabled.IsChecked ? "1" : "0";//是否启用代理
            Master_Golbal.CONTENT = this.txtSubject.Text;//出差事由
            
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
                    EndCity = TraveDetailList_Golbal[TraveDetailList_Golbal.Count() - 1].DESTCITY.ToString();
                    StartTime = TraveDetailList_Golbal[0].STARTDATE.ToString();
                    EndTime = TraveDetailList_Golbal[TraveDetailList_Golbal.Count() - 1].ENDDATE.ToString();
                }
                Master_Golbal.DEPCITY = StartCity;
                Master_Golbal.DESTCITY = EndCity;

                Master_Golbal.STARTCITYNAME = GetCityName(StartCity);
                Master_Golbal.ENDCITYNAME = GetCityName(EndCity);

                Master_Golbal.STARTDATE = Convert.ToDateTime(StartTime);
                Master_Golbal.ENDDATE = Convert.ToDateTime(EndTime);
            }
            if (formType != FormTypes.New)
            {
                Master_Golbal.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                Master_Golbal.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                Master_Golbal.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交
            }
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
            
            foreach (object obje in DaGrs.ItemsSource)
            {
                //TraveDetailOne_Golbal.T_OA_BUSINESSTRIP = Master_Golbal;

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

            #region "判断出差开始城市是否用重复"
            for (int i = 0; i < TraveDetailList_Golbal.Count; i++)
            {

                if (string.IsNullOrEmpty(TraveDetailList_Golbal[i].DEPCITY) || string.IsNullOrEmpty(TraveDetailList_Golbal[i].DESTCITY))
                {
                    //出发城市为空
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发或到达城市不能为空！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                if (i > 0)
                {
                    if (TraveDetailList_Golbal[i].DEPCITY == TraveDetailList_Golbal[i].DESTCITY)
                    {
                        //出发城市与开始城市不能相同
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发到达城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }
               
            }
            if (TraveDetailList_Golbal.Count > 1)
            {
                //如果上下两条出差记录城市一样
                string strStarCity = TraveDetailList_Golbal[0].DEPCITY;
                var q = from ent in TraveDetailList_Golbal
                         where ent.DEPCITY==strStarCity
                         select ent;
                    if (q.Count() > 1)
                    {
                        //出发城市与开始城市不能相同
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差出发城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                //var q = (from ent in TraveDetailList_Golbal
                //         group ent by ent.DEPCITY into g
                //         select new
                //         {
                //             g.Key,
                //             number = g.Count()
                //         });
                //foreach (var item in q)
                //{
                //    if (item.number > 1)
                //    {
                //        //出发城市与开始城市不能相同
                //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //        return false;
                //    }
                //}
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

            SetTraveRequestMasterValue();
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
                    string FirstStartCity =TraveDetailList_Golbal[0].DEPCITY;//出发城市中第一个城市代码
                    string LastEndCity = TraveDetailList_Golbal[TraveDetailList_Golbal.Count - 1].DESTCITY;//目标城市中最后一个城市代码

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

            if (formType == FormTypes.New)
            {
                OaPersonOfficeClient.TravelmanagementAddAsync(Master_Golbal, TraveDetailList_Golbal);
            }
            else
            {
                //UserState = "Edit";
                OaPersonOfficeClient.UpdateTravelmanagementAsync(Master_Golbal, TraveDetailList_Golbal, "Edit");
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
                    
                    this.formType = FormTypes.Edit;
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                        ParentEntityBrowser.ParentWindow.Close();
                    }
                    else
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                    }
                    RefreshUI(RefreshedTypes.All);//重新加载数据
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
            try
            {
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
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region 废弃代码

        //private V_EMPLOYEEDETAIL employeepost = new V_EMPLOYEEDETAIL();

        #region 获取出差用户的组织信息

        //void client_GetEmployeePostBriefByEmployeeIDCompleted(object sender, GetEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        //{
        //    V_EMPLOYEEDETAIL employeepost = new V_EMPLOYEEDETAIL();
        //    string StrName = string.Empty;
        //    bool isLoadingAct = string.IsNullOrEmpty(Master_Golbal.POSTLEVEL);
        //    if (e.Result != null)
        //    {
        //        employeepost = e.Result;
        //        //获取出差人的岗位级别
        //        if (!string.IsNullOrEmpty(Master_Golbal.OWNERPOSTID))//新建时如果选了出差人,获取被选出差人的岗位等级
        //        {
        //            if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == Master_Golbal.OWNERPOSTID).FirstOrDefault() != null)
        //            {
        //                Master_Golbal.POSTLEVEL = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == Master_Golbal.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();
        //            }
        //            else
        //            {
        //                var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
        //                Master_Golbal.POSTLEVEL = ent != null ? ent.POSTLEVEL.ToString() : "0 ";
        //            }
        //        }
        //        else //默认为当前用户的岗位等级
        //        {
        //            Master_Golbal.POSTLEVEL = Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();
        //        }
        //        if (formType == FormTypes.New && isLoadingAct)
        //        {
        //            var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
        //            Master_Golbal.OWNERPOSTNAME = ent != null ? ent.PostName.ToString() : employeepost.EMPLOYEEPOSTS[0].PostName;
        //            Master_Golbal.OWNERDEPARTMENTNAME = ent != null ? ent.DepartmentName.ToString() : employeepost.EMPLOYEEPOSTS[0].DepartmentName;
        //            Master_Golbal.OWNERCOMPANYNAME = ent != null ? ent.CompanyName.ToString() : employeepost.EMPLOYEEPOSTS[0].CompanyName;
        //            StrName = Common.CurrentLoginUserInfo.EmployeeName + "-" + Master_Golbal.OWNERPOSTNAME + "-" + Master_Golbal.OWNERDEPARTMENTNAME + "-" + Master_Golbal.OWNERCOMPANYNAME;
        //            txtTraveEmployee.Text = StrName;
        //            if (string.IsNullOrEmpty(Master_Golbal.OWNERNAME))//如果没有选择出差人的情况
        //            {
        //                Master_Golbal.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        //            }
        //            ToolTipService.SetToolTip(txtTraveEmployee, StrName);
        //            if (!string.IsNullOrEmpty(Master_Golbal.OWNERCOMPANYID))//如果是选出差人的情况下(获取所选用户公司)
        //            {
        //                OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Master_Golbal.OWNERCOMPANYID, null, null);
        //            }
        //            else //默认是当前用户(当前用户公司)
        //            {
        //                OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, null, null);
        //            }
        //        }
        //        else
        //        {
        //            RefreshUI(RefreshedTypes.AuditInfo);
        //            RefreshUI(RefreshedTypes.All);

        //            //2013/3/27 alter by ken 修改加载员工岗位信息方式
        //            Master_Golbal.OWNERPOSTNAME = employeepost.EMPLOYEEPOSTS.Where(c => c.POSTID == Master_Golbal.OWNERPOSTID).FirstOrDefault().PostName;
        //            Master_Golbal.OWNERDEPARTMENTNAME = employeepost.EMPLOYEEPOSTS.Where(c => c.DepartmentID == Master_Golbal.OWNERDEPARTMENTID).FirstOrDefault().DepartmentName;
        //            Master_Golbal.OWNERCOMPANYNAME = employeepost.EMPLOYEEPOSTS.Where(c => c.CompanyID == Master_Golbal.OWNERCOMPANYID).FirstOrDefault().CompanyName;
        //            //Master_Golbal.OWNERPOSTNAME = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == Businesstrip.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
        //            //Master_Golbal.OWNERDEPARTMENTNAME = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == Businesstrip.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //            //Master_Golbal.OWNERCOMPANYNAME = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == Businesstrip.OWNERCOMPANYID).FirstOrDefault().CNAME;
        //            StrName = Master_Golbal.OWNERNAME + "-" + Master_Golbal.OWNERPOSTNAME + "-" + Master_Golbal.OWNERDEPARTMENTNAME + "-" + Master_Golbal.OWNERCOMPANYNAME;
        //            txtTraveEmployee.Text = StrName;
        //            //strTravelEmployeeName = Master_Golbal.OWNERNAME;//修改、查看、审核时获取已保存在本地的出差人
        //            ToolTipService.SetToolTip(txtTraveEmployee, StrName);
        //            OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Master_Golbal.OWNERCOMPANYID, null, null);
        //        }
        //        if (!string.IsNullOrEmpty(employeepost.MOBILE))
        //        {
        //            txtTELL.Text = employeepost.MOBILE;//手机号码
        //        }
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("对不起，该员工已离职，不能进行该操作"));
        //    }
        //}
        
        //void client_GetAllEmployeePostBriefByEmployeeIDCompleted(object sender, GetAllEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        //{
        //    V_EMPLOYEEDETAIL employeepost = new V_EMPLOYEEDETAIL();
        //    string StrName = string.Empty;
        //    if (e.Result != null)
        //    {
        //        employeepost = e.Result;
        //        //获取出差人的岗位级别
        //        if (!string.IsNullOrEmpty(Master_Golbal.OWNERPOSTID))//新建时如果选了出差人,获取被选出差人的岗位等级
        //        {
        //            if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == Master_Golbal.OWNERPOSTID).FirstOrDefault() != null)
        //            {
        //                Master_Golbal.POSTLEVEL = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == Master_Golbal.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();
        //            }
        //            else
        //            {
        //                var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
        //                Master_Golbal.POSTLEVEL = ent != null ? ent.POSTLEVEL.ToString() : "0 ";
        //            }
        //        }
        //        else //默认为当前用户的岗位等级
        //        {
        //            Master_Golbal.POSTLEVEL = Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();
        //        }
        //        if (formType == FormTypes.New)
        //        {
        //            var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
        //            Master_Golbal.OWNERPOSTNAME = ent != null ? ent.PostName.ToString() : employeepost.EMPLOYEEPOSTS[0].PostName;
        //            Master_Golbal.OWNERDEPARTMENTNAME = ent != null ? ent.DepartmentName.ToString() : employeepost.EMPLOYEEPOSTS[0].DepartmentName;
        //            Master_Golbal.OWNERCOMPANYNAME = ent != null ? ent.CompanyName.ToString() : employeepost.EMPLOYEEPOSTS[0].CompanyName;
        //            StrName = Common.CurrentLoginUserInfo.EmployeeName + "-" + Master_Golbal.OWNERPOSTNAME + "-" + Master_Golbal.OWNERDEPARTMENTNAME + "-" + Master_Golbal.OWNERCOMPANYNAME;
        //            txtTraveEmployee.Text = StrName;
        //            if (string.IsNullOrEmpty(Master_Golbal.OWNERNAME))//如果没有选择出差人的情况
        //            {
        //                //strTravelEmployeeName = Common.CurrentLoginUserInfo.EmployeeName;
        //            }
        //            ToolTipService.SetToolTip(txtTraveEmployee, StrName);
        //            if (!string.IsNullOrEmpty(Master_Golbal.OWNERCOMPANYID))//如果是选出差人的情况下(获取所选用户公司)
        //            {
        //                OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Master_Golbal.OWNERCOMPANYID, null, null);
        //            }
        //            else //默认是当前用户(当前用户公司)
        //            {
        //                OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, null, null);
        //            }
        //        }
        //        else
        //        {
        //            RefreshUI(RefreshedTypes.AuditInfo);
        //            RefreshUI(RefreshedTypes.All);
        //            //2013/3/27 alter by ken 修改加载员工岗位信息方式
        //            Master_Golbal.OWNERPOSTNAME = employeepost.EMPLOYEEPOSTS.Where(c => c.POSTID == Master_Golbal.OWNERPOSTID).FirstOrDefault().PostName;
        //            Master_Golbal.OWNERDEPARTMENTNAME = employeepost.EMPLOYEEPOSTS.Where(c => c.DepartmentID == Master_Golbal.OWNERDEPARTMENTID).FirstOrDefault().DepartmentName;
        //            Master_Golbal.OWNERCOMPANYNAME = employeepost.EMPLOYEEPOSTS.Where(c => c.CompanyID == Master_Golbal.OWNERCOMPANYID).FirstOrDefault().CompanyName;
        //            StrName = Master_Golbal.OWNERNAME + "-" + Master_Golbal.OWNERPOSTNAME + "-" + Master_Golbal.OWNERDEPARTMENTNAME + "-" + Master_Golbal.OWNERCOMPANYNAME;
        //            txtTraveEmployee.Text = StrName;
        //            //strTravelEmployeeName = Master_Golbal.OWNERNAME;//修改、查看、审核时获取已保存在本地的出差人
        //            ToolTipService.SetToolTip(txtTraveEmployee, StrName);
        //            OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Master_Golbal.OWNERCOMPANYID, null, null);
        //        }
        //        if (employeepost.MOBILE != null && txtTELL.Text == null)
        //        {
        //            txtTELL.Text = employeepost.MOBILE;//手机号码
        //        }
        //        else if (txtTELL.Text == null)
        //        {
        //            if (employeepost.OFFICEPHONE != null)
        //            {
        //                txtTELL.Text = employeepost.OFFICEPHONE;//座机号码
        //            }
        //        }
        //    }

        //}

        #endregion

        #endregion
    }
}
