/********************************************************************************
//出差报销form，alter by ken 2013/3/27
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.TravelExpApplyMaster;
using SMT.Saas.Tools.FBServiceWS;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class TravelReimbursementControl 
    {

        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            needsubmit = true;
            clickSubmit = true;
            this.refreshType = RefreshedTypes.ShowAudit;
            Save();
        }

        #region 验证
        private bool Check()
        {
            string StrStartDt = string.Empty;
            string EndDt = string.Empty;
            string StrStartTime = string.Empty;
            string StrEndTime = string.Empty;
            bool checkPrivate = false;
            bool checkMeet = false;
            ObservableCollection<T_OA_REIMBURSEMENTDETAIL> TripDetails = DaGrEdit.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;

            #region 出差时间验证
            
            
            foreach (object obje in DaGrEdit.ItemsSource)
            {
                SearchCity myCitys = DaGrEdit.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;
                DateTimePicker StartDate = DaGrEdit.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker EndDate = DaGrEdit.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;
                CheckBox chbPrivate = DaGrEdit.Columns[13].GetCellContent(obje).FindName("myChkBox") as CheckBox;
                CheckBox chbMeet = DaGrEdit.Columns[14].GetCellContent(obje).FindName("myChkBoxMeet") as CheckBox;

                if (chbPrivate.IsChecked == true) checkPrivate = true;
                if (chbMeet.IsChecked == true) checkMeet = true;

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

                DateTimePicker dpStartTime = DaGrEdit.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker dpEndTime = DaGrEdit.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;
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

                T_OA_REIMBURSEMENTDETAIL entDetail = obje as T_OA_REIMBURSEMENTDETAIL;

                var queryData = from c in TripDetails
                                where c.STARTDATE > dpStartTime.Value && c.ENDDATE > dpEndTime.Value && c.REIMBURSEMENTDETAILID != entDetail.REIMBURSEMENTDETAILID
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


                TravelDictionaryComboBox ComVechile = ((TravelDictionaryComboBox)((StackPanel)DaGrEdit.Columns[6].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                if (ComVechile.SelectedIndex <= 0)//交通工具类型
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "TYPEOFTRAVELTOOLS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

                TravelDictionaryComboBox ComVechileLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrEdit.Columns[7].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                if (ComVechileLevel.SelectedIndex < 0)//交通工具级别
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"),"交通工具级别不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

            }
            #endregion

            #region "判断出差开始城市是否用重复,下一条开始时间是否小于上一条结束时间"           
            for (int i = 0; i < TravelDetailList_Golbal.Count; i++)
            {
                if (TravelDetailList_Golbal.Count > 1)
                {
                    //如果不是第一条记录，判断结束时间是否大于上一条开始时间
                    if (i>0)
                    {
                        if (TravelDetailList_Golbal[i].STARTDATE < TravelDetailList_Golbal[i - 1].ENDDATE)
                        {
                            //出发城市为空
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差开始时间大于上一条出差记录的结束时间！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return false;
                        }
                    }
                }
                if (string.IsNullOrEmpty(TravelDetailList_Golbal[i].DEPCITY) || string.IsNullOrEmpty(TravelDetailList_Golbal[i].DESTCITY))
                {
                    //出发城市为空
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发或到达城市不能为空！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                if (i > 0)
                {
                    if (TravelDetailList_Golbal[i].DEPCITY == TravelDetailList_Golbal[i].DESTCITY)
                    {
                        //出发城市与开始城市不能相同
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发到达城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }
            }
            if (TravelDetailList_Golbal.Count > 1)
            {
                //如果上下两条出差记录城市一样
                string strStarCity = TravelDetailList_Golbal[0].DEPCITY;
                var q = from ent in TravelDetailList_Golbal
                        where ent.DEPCITY == strStarCity
                        select ent;
                if (q.Count() > 1)
                {
                    //出发城市与开始城市不能相同
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差出发城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
            }
            #endregion  

            #region 出差报销其他验证
            if (string.IsNullOrEmpty(this.txtReport.Text.Trim()))//报告内容
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "REPORT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                this.txtRemark.Focus();
                return false;
            }
            if (OpenFrom != "FromMVC")
            {
                if (string.IsNullOrEmpty(txtPAYMENTINFO.Text))//支付信息
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "支付信息不能为空，请重新填写", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
            }
            if (string.IsNullOrEmpty(this.txtSubTotal.Text) && this.txtSubTotal.Text.Trim() != "0")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差报销费用不可为零，请重新填写单据", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (string.IsNullOrEmpty(txtPAYMENTINFO.Text))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "支付信息不能为空，请重新填写", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            //add by luojie
            //当没有报销金额时弹出提醒
            decimal totalFee = Convert.ToDecimal(txtChargeApplyTotal.Text);
            if (totalFee <= 0 && (!checkMeet && !checkPrivate))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出差报销费用不能为零，请填写报销费用!"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
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

        /// <summary>
        /// //计算出差时间，计算补贴，住宿费，交通费，其他费用
        /// </summary>
        private void SetTraveValueAndFBChargeValue()
        {
            try
            {
                //计算出差时间
                TravelTime();

                //计算补贴
                TravelAllowance(false);

                //住宿费，交通费，其他费用
                CountMoney();

                if (!string.IsNullOrEmpty(txtPAYMENTINFO.Text))
                {
                    fbCtr.Order.PAYMENTINFO = txtPAYMENTINFO.Text;//支付信息
                    StrPayInfo = txtPAYMENTINFO.Text;
                }
                TravelReimbursement_Golbal.TEL = this.txtTELL.Text;//联系电话;
                TravelReimbursement_Golbal.CONTENT = this.txtReport.Text;//报告内容    
                TravelReimbursement_Golbal.THETOTALCOST = Convert.ToDecimal(txtSubTotal.Text);//本次差旅总费用

                if (fbCtr.Order.TOTALMONEY != null)
                {   //总费用;
                    TravelReimbursement_Golbal.REIMBURSEMENTOFCOSTS = fbCtr.Order.TOTALMONEY;
                }
                TravelReimbursement_Golbal.REIMBURSEMENTTIME = Convert.ToDateTime(ReimbursementTime.Text);
                TravelReimbursement_Golbal.REMARKS = this.txtRemark.Text;
                if (string.IsNullOrEmpty(TravelReimbursement_Golbal.POSTLEVEL))
                {
                    TravelReimbursement_Golbal.POSTLEVEL = Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();
                }
                if (string.IsNullOrEmpty(TravelReimbursement_Golbal.OWNERNAME))
                {
                    TravelReimbursement_Golbal.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    TravelReimbursement_Golbal.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                }
                if (string.IsNullOrEmpty(TravelReimbursement_Golbal.OWNERPOSTNAME))
                {
                    TravelReimbursement_Golbal.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    TravelReimbursement_Golbal.OWNERPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
                }
                if (string.IsNullOrEmpty(TravelReimbursement_Golbal.OWNERDEPARTMENTNAME))
                {
                    TravelReimbursement_Golbal.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    TravelReimbursement_Golbal.OWNERDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                }
                if (string.IsNullOrEmpty(TravelReimbursement_Golbal.OWNERCOMPANYNAME))
                {
                    TravelReimbursement_Golbal.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    TravelReimbursement_Golbal.OWNERCOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
                }
            }
            catch (Exception ex)
            {
                Utility.SetLogAndShowLog(ex.ToString());
            }

        }

        private void Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            SaveBtn = true;
            try
            {
                //将页面上所有的数据赋值给报销子表，先赋值，才能Check（）测查子表记录是否正确
                SetTravelDetailValueFromForm();

                if (Check())
                {
                    //字段赋值及子表城市赋值
                    SetTraveValueAndFBChargeValue();

                    if (string.IsNullOrEmpty(this.txtSubTotal.Text) || this.txtSubTotal.Text.Trim() == "0")
                    {
                        //回程无住宿费
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差报销费用总额为0，请重新填写！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        return;
                    }
                    #region "判断回程住宿费"
                    
                    for (int i = 0; i < TravelDetailList_Golbal.Count; i++)
                    {
                        if (TravelDetailList_Golbal.Count > 1 && i == TravelDetailList_Golbal.Count - 1)
                        {
                            if (TravelDetailList_Golbal[i].ACCOMMODATION != null)
                            {
                                if (TravelDetailList_Golbal[i].ACCOMMODATION.Value > 0)
                                {
                                    //回程无住宿费
                                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "回程无住宿费，请重新填写！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                    //TravelDetailList_Golbal[i].ACCOMMODATION = 0;
                                    //只有这样能获取到Textbox
                                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> TripDetails = DaGrEdit.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                                    TextBox textAccommodation = DaGrEdit.Columns[9].GetCellContent(TripDetails[i]).FindName("txtACCOMMODATION") as TextBox;
                                   
                                    textAccommodation.BorderBrush = new SolidColorBrush(Colors.Red);
                                    textAccommodation.IsReadOnly = false;
                                    textAccommodation.IsEnabled = true;
                                    textAccommodation.Focus();
                                    RefreshUI(RefreshedTypes.HideProgressBar);
                                    return;
                                }
                            }
                        }
                    }
                    #endregion  


                    TravelReimbursement_Golbal.T_OA_REIMBURSEMENTDETAIL = null;//清空主表明细记录，以免保存时异常（子表字段为空）
                    if (OpenFrom == "FromMVC")
                    {
                        OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Edit");
                    }
                    else
                    {

                        if (TravelReimbursement_Golbal.CHECKSTATE == "0"
                            && TravelReimbursement_Golbal.REIMBURSEMENTOFCOSTS > 0)
                        {
                            fbCtr.Order.ORDERID = TravelReimbursement_Golbal.TRAVELREIMBURSEMENTID;
                            fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit);//提交费用 
                        }
                        else
                        {
                            OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Edit");
                        }
                    }
                }
                else
                {
                    needsubmit = false;
                    //isSubmit = false;
                    RefreshUI(RefreshedTypes.HideProgressBar);
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

        #region 费用保存Completed
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {
            if (e.Message != null && e.Message.Count() > 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), e.Message[0], Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            else
            {
                switch (formType)
                {
                    case FormTypes.Edit://修改
                        if (TravelReimbursement_Golbal.CHECKSTATE == Utility.GetCheckState(CheckStates.UnSubmit))
                        {
                            if (e.Message != null && e.Message.Count() > 0)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                            txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                            if (fbCtr.Order.PAYMENTINFO != null)
                            {
                                txtPAYMENTINFO.Text = fbCtr.Order.PAYMENTINFO;//支付信息
                                StrPayInfo = txtPAYMENTINFO.Text;
                            }
                            TravelReimbursement_Golbal.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号

                            if (needsubmit == true)
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Submit");
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Edit");
                            }
                        }
                        if (TravelReimbursement_Golbal.CHECKSTATE == Utility.GetCheckState(CheckStates.Approving))
                        {
                            RefreshUI(RefreshedTypes.AuditInfo);
                            IsAudit = false;
                        }
                        break;
                    case FormTypes.Resubmit://重新提交
                        if (TravelReimbursement_Golbal.CHECKSTATE == Utility.GetCheckState(CheckStates.UnApproved))
                        {
                            if (e.Message != null && e.Message.Count() > 0)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                            txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                            TravelReimbursement_Golbal.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号

                            if (needsubmit == true)
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Submit");
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Edit");
                            }
                        }
                        else if (TravelReimbursement_Golbal.CHECKSTATE == Utility.GetCheckState(CheckStates.UnSubmit))
                        {
                            if (e.Message != null && e.Message.Count() > 0)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                            if (fbCtr.Order.INNERORDERCODE != null)
                            {
                                txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                                TravelReimbursement_Golbal.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号
                            }
                            if (needsubmit == true)
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Submit");
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Edit");
                            }
                        }
                        break;
                    case FormTypes.Audit:
                        if (TravelReimbursement_Golbal.CHECKSTATE == Utility.GetCheckState(CheckStates.Approved) || TravelReimbursement_Golbal.CHECKSTATE == Utility.GetCheckState(CheckStates.UnApproved))
                        {
                            if (e.Message != null && e.Message.Count() > 0)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                            txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                            TravelReimbursement_Golbal.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号

                            if (needsubmit == true)
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Submit");
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Edit");
                            }
                        }//审核通过直接修改表单状态
                        else
                        {
                            if (IsAudit) //审核中
                            {
                                RefreshUI(RefreshedTypes.AuditInfo);
                                IsAudit = false;
                            }
                            else if (Resubmit)//重新提交
                            {
                                if (e.Message != null && e.Message.Count() > 0)
                                {
                                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                    return;
                                }
                                txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                                TravelReimbursement_Golbal.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号

                                if (needsubmit == true)
                                {
                                    OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Submit");
                                }
                                else
                                {
                                    OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Edit");
                                }
                                RefreshUI(RefreshedTypes.AuditInfo);
                                Resubmit = false;
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement_Golbal, TravelDetailList_Golbal, formType.ToString(), "Edit");
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        #region 修改Completed
        void TrC_UpdateTravelReimbursementCompleted(object sender, UpdateTravelReimbursementCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    if (e.Result != "")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Result), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        return;
                    }
                    else
                    {
                        //isSubmit = false;
                        if (e.UserState.ToString() == "Audit")//提示审核人审核成功
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                        }
                        else if (e.UserState.ToString() == "Submit")//提交成功
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "TRAVELREIMBURSEMENTPAGE"));

                        }
                        else//更新成功
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "TRAVELREIMBURSEMENTPAGE"));
                        }

                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                            ParentEntityBrowser.ParentWindow.Close();
                        }
                    }
                    canSubmit = true;

                    RefreshUI(RefreshedTypes.AuditInfo);
                    if (TravelReimbursement_Golbal.REIMBURSEMENTOFCOSTS > 0 || fbCtr.Order.TOTALMONEY > 0)
                    {
                        if (needsubmit == true)
                        {
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.ManualSubmit();
                            HideButtons();
                        }
                        else
                        {
                            needsubmit = false;
                            RefreshUI(RefreshedTypes.HideProgressBar);
                        }
                    }
                    else
                    {
                        needsubmit = false;
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出差报销费用不能为零，请填写报销费用!"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        if (clickSubmit == true)
                        {
                            RefreshUI(RefreshedTypes.HideProgressBar);
                            RefreshUI(RefreshedTypes.All);
                            //clickSubmit = false;
                        }
                    }
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

        /// <summary>
        /// 点提交后隐藏按钮
        /// </summary>
        public void HideButtons()
        {
            needsubmit = false;
            #region 隐藏entitybrowser中的toolbar按钮
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.IsEnabled = false;
            if (entBrowser.EntityEditor is IEntityEditor)
            {
                List<ToolbarItem> bars = GetToolBarItems();
                if (bars != null)
                {
                    ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
                    if (bar != null)
                    {
                        bar.Visibility = Visibility.Collapsed;
                    }
                }
            }

            #endregion
            RefreshUI(RefreshedTypes.AuditInfo);
        }
        #endregion

    }
}
