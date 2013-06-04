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
            //isSubmit = true;
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
            ObservableCollection<T_OA_REIMBURSEMENTDETAIL> TripDetails = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;

            #region 出差时间验证
            foreach (object obje in DaGrs.ItemsSource)
            {
                SearchCity myCitys = DaGrs.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;
                DateTimePicker StartDate = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker EndDate = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;
                CheckBox chbPrivate = DaGrs.Columns[13].GetCellContent(obje).FindName("myChkBox") as CheckBox;
                CheckBox chbMeet = DaGrs.Columns[14].GetCellContent(obje).FindName("myChkBoxMeet") as CheckBox;

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


                TravelDictionaryComboBox ComVechile = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[6].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                if (ComVechile.SelectedIndex <= 0)//交通工具类型
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "TYPEOFTRAVELTOOLS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

                TravelDictionaryComboBox ComVechileLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[7].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                if (ComVechileLevel.SelectedIndex < 0)//交通工具级别
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"),"交通工具级别不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

            }
            #endregion
            
            #region "判断出差开始城市是否用重复"           
            for (int i = 0; i < TripDetails.Count; i++)
            {

                if (string.IsNullOrEmpty(TripDetails[i].DEPCITY) || string.IsNullOrEmpty(TripDetails[i].DESTCITY))
                {
                    //出发城市为空
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发或到达城市不能为空！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                if (TripDetails.Count > 1)
                {
                    //如果上下两条出差记录城市一样
                    if (i < TripDetails.Count - 1 && TripDetails[i].DEPCITY == TripDetails[i + 1].DEPCITY)
                    {
                        //出发城市与开始城市不能相同
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                    if (i > 0)
                    {
                        if (TripDetails[i].DEPCITY == TripDetails[i].DESTCITY)
                        {
                            //出发城市与开始城市不能相同
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发到达城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return false;
                        }
                    }
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
        /// 字段赋值
        /// </summary>
        private void SetTraveAndFBChargeValue()
        {
            //计算出差时间
            TravelTime();

            //计算补贴
            TravelAllowance();

            //添加子表数据
            NewDetail_Golbal();

            CountMoney();

            if (!string.IsNullOrEmpty(this.txtSubTotal.Text) && this.txtSubTotal.Text.Trim() != "0")
            {
                if (fbCtr.TravelSubject != null)
                {
                    fbCtr.TravelSubject.ApplyMoney = Convert.ToDecimal(this.txtSubTotal.Text);//将本次出差总费用给预算
                }
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差报销费用不可为零，请重新填写单据", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(txtPAYMENTINFO.Text))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "支付信息不能为空，请重新填写", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            else
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

        private void Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            SaveBtn = true;
            try
            {
                if (Check())
                {
                    //textStandards.Text = string.Empty;//清空报销标准说明
                    //字段赋值
                    SetTraveAndFBChargeValue();


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
                                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> TripDetails = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                                    TextBox textAccommodation = DaGrs.Columns[9].GetCellContent(TripDetails[i]).FindName("txtACCOMMODATION") as TextBox;
                                   
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


        #region 废弃的代码


        #region 查询出差报销明细
        //void TrC_GetTravelReimbursementDetailCompleted(object sender, GetTravelReimbursementDetailCompletedEventArgs e)//查询报销明细
        //{
        //    isloaded = true;
        //    if (clickSubmit == false)
        //    {
        //        RefreshUI(RefreshedTypes.HideProgressBar);
        //    }
        //    try
        //    {
        //        if (e.Result != null)
        //        {
        //            BindDataGrid(e.Result);
        //        }
        //        else
        //        {
        //            BindDataGrid(null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        isloaded = false;
        //        Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //    }
        //}
        #endregion

        #region 删除
        //private void myDelete_Click(object sender, RoutedEventArgs e)
        //{
        //    if (DaGrs.SelectedItems == null)
        //    {
        //        return;
        //    }

        //    if (DaGrs.SelectedItems.Count == 0)
        //    {
        //        return;
        //    }
        //    if (DaGrs.SelectedItems.Count > 0)//判断是否有选中数据,否则提醒用户
        //    {
        //        string Result = "";
        //        ComfirmWindow com = new ComfirmWindow();//提醒用户是否真的删除该数据,以免操作失误。
        //        com.OnSelectionBoxClosed += (obj, result) =>
        //        {
        //            TrList = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
        //            if (TrList.Count() > 1)
        //            {
        //                for (int i = 0; i < DaGrs.SelectedItems.Count; i++)
        //                {
        //                    T_OA_REIMBURSEMENTDETAIL entDel = DaGrs.SelectedItems[i] as T_OA_REIMBURSEMENTDETAIL;

        //                    if (TrList.Contains(entDel))
        //                    {
        //                        TrList.Remove(entDel);
        //                    }
        //                }
        //                DaGrs.ItemsSource = TrList;
        //            }
        //        };
        //        com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
        //    }
        //    else
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
        //    }
        //}
        #endregion

        #region 获取当前用户信息已废除

        //void client_GetAllEmployeePostBriefByEmployeeIDCompleted(object sender, GetAllEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        //{
        //    string StrName = "";
        //    if (e.Result != null)
        //    {
        //        employeepost = e.Result;
        //        if (TravelReimbursement_Golbal != null)
        //        {
        //            if (TravelReimbursement_Golbal.OWNERPOSTID != null && TravelReimbursement_Golbal.OWNERCOMPANYID != null && TravelReimbursement_Golbal.OWNERDEPARTMENTID != null)
        //            {
        //                if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == TravelReimbursement_Golbal.OWNERPOSTID).FirstOrDefault() != null)
        //                {
        //                    EmployeePostLevel = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == TravelReimbursement_Golbal.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();
        //                }
        //                else //存在岗位异动的情况下
        //                {
        //                    var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
        //                    EmployeePostLevel = ent != null ? ent.POSTLEVEL.ToString() : "0 ";
        //                }

        //                //2013/3/27 alter by ken 修改出差加载员工岗位信息方式
        //                postName = employeepost.EMPLOYEEPOSTS.Where(c => c.POSTID == TravelReimbursement_Golbal.OWNERPOSTID).FirstOrDefault().PostName;
        //                depName = employeepost.EMPLOYEEPOSTS.Where(c => c.DepartmentID == TravelReimbursement_Golbal.OWNERDEPARTMENTID).FirstOrDefault().DepartmentName;
        //                companyName = employeepost.EMPLOYEEPOSTS.Where(c => c.CompanyID == TravelReimbursement_Golbal.OWNERCOMPANYID).FirstOrDefault().CompanyName;
        //                //postName = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
        //                //depName = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == TravelReimbursement.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //                //companyName = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == TravelReimbursement.OWNERCOMPANYID).FirstOrDefault().CNAME;
        //                StrName = TravelReimbursement_Golbal.OWNERNAME + "-" + postName + "-" + depName + "-" + companyName;
        //                txtPeopleTravel.Text = StrName;
        //                ToolTipService.SetToolTip(txtPeopleTravel, StrName);
        //                EmployeeName = TravelReimbursement_Golbal.OWNERNAME;//出差人
        //            }
        //            if (InitFB == false)
        //            {
        //                InitFBControl();
        //            }
        //            OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(TravelReimbursement_Golbal.OWNERCOMPANYID, null, null);
        //        }
        //    }
        //}

        //void client_GetEmployeePostBriefByEmployeeIDCompleted(object sender, GetEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        //{
        //    string StrName = "";
        //    if (e.Result != null)
        //    {
        //        employeepost = e.Result;
        //        if (TravelReimbursement_Golbal != null)
        //        {
        //            if (TravelReimbursement_Golbal.OWNERPOSTID != null && TravelReimbursement_Golbal.OWNERCOMPANYID != null && TravelReimbursement_Golbal.OWNERDEPARTMENTID != null)
        //            {
        //                if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == TravelReimbursement_Golbal.OWNERPOSTID).FirstOrDefault() != null)
        //                {
        //                    EmployeePostLevel = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == TravelReimbursement_Golbal.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();
        //                }
        //                else //存在岗位异动的情况下
        //                {
        //                    var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
        //                    EmployeePostLevel = ent != null ? ent.POSTLEVEL.ToString() : "0 ";
        //                }

        //                //2013/3/27 alter by ken 修改加载员工岗位信息方式
        //                postName = employeepost.EMPLOYEEPOSTS.Where(c => c.POSTID == TravelReimbursement_Golbal.OWNERPOSTID).FirstOrDefault().PostName;
        //                depName = employeepost.EMPLOYEEPOSTS.Where(c => c.DepartmentID == TravelReimbursement_Golbal.OWNERDEPARTMENTID).FirstOrDefault().DepartmentName;
        //                companyName = employeepost.EMPLOYEEPOSTS.Where(c => c.CompanyID == TravelReimbursement_Golbal.OWNERCOMPANYID).FirstOrDefault().CompanyName;
        //                //postName = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
        //                //depName = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == TravelReimbursement.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //                //companyName = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == TravelReimbursement.OWNERCOMPANYID).FirstOrDefault().CNAME;
        //                StrName = TravelReimbursement_Golbal.OWNERNAME + "-" + postName + "-" + depName + "-" + companyName;
        //                txtPeopleTravel.Text = StrName;
        //                ToolTipService.SetToolTip(txtPeopleTravel, StrName);
        //                EmployeeName = TravelReimbursement_Golbal.OWNERNAME;//出差人
        //            }
        //            if (InitFB == false)
        //            {
        //                InitFBControl();
        //            }
        //            OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(TravelReimbursement_Golbal.OWNERCOMPANYID, null, null);
        //        }
        //    }
        //    else
        //    {
        //        //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("对不起，该员工已离职，不能进行该操作"));
        //        HrPersonnelclient.GetAllEmployeePostBriefByEmployeeIDAsync(TravelReimbursement_Golbal.OWNERID);
        //    }
        //}
        #endregion

        #region 上传附件
        //private void UploadFiles()
        //{
        //    //System.Windows.Controls.OpenFileDialog openFileWindow = new OpenFileDialog();
        //    //openFileWindow.Multiselect = true;
        //    //if (openFileWindow.ShowDialog() == true)
        //    //    foreach (FileInfo file in openFileWindow.Files)
        //    //        ctrFile.InitFiles(file.Name, file.OpenRead());
        //}
        ////void ctrFile_Event_AllFilesFinished(object sender, SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs e)
        ////{
        ////    //RefreshUI(RefreshedTypes.HideProgressBar);
        ////}
        #endregion
        #endregion
    }
}
