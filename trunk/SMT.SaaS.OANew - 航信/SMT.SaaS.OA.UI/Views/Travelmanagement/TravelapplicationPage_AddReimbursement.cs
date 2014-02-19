/********************************************************************************

** 作者： ken  

** 创始时间：2013-04-24

** 修改人：ken

** 修改时间：2013-04-24

** 描述：

**    主要用于出差列表点报销时新增出差报销功能

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using System.Globalization;
using SMT.SAAS.Controls.Toolkit.Windows;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SAAS.Application;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;
using SMT.SAAS.Platform.Logging;

namespace SMT.SaaS.OA.UI.Views.Travelmanagement
{
    public partial class TravelapplicationPage : BasePage
    {

        #region 1DataGrid报销按钮事件
        private void myBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            btn.IsEnabled = false;
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "OPERATION"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "OPERATION"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            ///luojie 20120808
            ///未报销按钮的权限控制，同修改按钮
            V_Travelmanagement entrb = new V_Travelmanagement();
            entrb = DaGr.SelectedItems[0] as V_Travelmanagement;
            if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(entrb, "T_OA_BUSINESSTRIP", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    V_Travelmanagement ent = new V_Travelmanagement();
                    ent = (DaGr.SelectedItems[i] as V_Travelmanagement);

                    reportid = ent.ReportId;
                    businesstrID = ent.Travelmanagement.BUSINESSTRIPID;
                    travelreimbursementId = ent.TrId;
                    Tdetail = ent.Tdetail;
                }

                if (!string.IsNullOrEmpty(travelreimbursementId) && travelreimbursementId != "空" && Tdetail > 0)//如果已生成报销单，直接打开表单提交
                {
                    WhetherReimbursement = false;
                    BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Edit, businesstrID, WhetherReimbursement);
                    EntityBrowser browser = new EntityBrowser(AddWin);
                    browser.RemoveSMTLoading();
                    browser.FormType = FormTypes.Edit;
                    browser.EntityBrowseToolBar.MaxHeight = 0;
                    browser.MinWidth = 980;
                    browser.MinHeight = 445;
                    browser.TitleContent = "出差申请";
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ReimbursementSwitch = true;//出差报销开关
                    Travelmanagement.GetTravelmanagementByIdAsync(businesstrID, btn);
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("对不起，您没有修改该用户报销的权限"));
            }
        }
        #endregion

        #region 2获取出差申请数据
        /// <summary>
        /// 获取出差报告主表数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Travelmanagement_GetTravelmanagementByIdCompleted(object sender, GetTravelmanagementByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    if (e.Result != null)
                    {
                        businesstripInfo = e.Result;
                        if (ReimbursementSwitch == true)//如果是操作是否报销按钮
                        {
                            travelReimbursement.TRAVELREIMBURSEMENTID = Guid.NewGuid().ToString();
                            travelReimbursement.T_OA_BUSINESSTRIP = businesstripInfo;
                            travelReimbursement.T_OA_BUSINESSTRIP.BUSINESSTRIPID = businesstripInfo.BUSINESSTRIPID;
                            travelReimbursement.CLAIMSWERE = businesstripInfo.OWNERID;
                            travelReimbursement.CLAIMSWERENAME = businesstripInfo.OWNERNAME;
                            travelReimbursement.REIMBURSEMENTTIME = DateTime.Now;
                            travelReimbursement.CHECKSTATE = "0";
                            travelReimbursement.TEL = businesstripInfo.TEL;
                            travelReimbursement.CREATEDATE = businesstripInfo.UPDATEDATE;

                            travelReimbursement.OWNERID = businesstripInfo.OWNERID;
                            travelReimbursement.OWNERNAME = businesstripInfo.OWNERNAME;

                            travelReimbursement.OWNERPOSTID = businesstripInfo.OWNERPOSTID;
                            travelReimbursement.OWNERPOSTNAME = businesstripInfo.OWNERPOSTNAME;

                            travelReimbursement.OWNERDEPARTMENTID = businesstripInfo.OWNERDEPARTMENTID;
                            travelReimbursement.OWNERDEPARTMENTNAME = businesstripInfo.OWNERDEPARTMENTNAME;

                            travelReimbursement.OWNERCOMPANYID = businesstripInfo.OWNERCOMPANYID;
                            travelReimbursement.OWNERCOMPANYNAME = businesstripInfo.OWNERCOMPANYNAME;

                            travelReimbursement.POSTLEVEL = businesstripInfo.POSTLEVEL;

                            travelReimbursement.STARTCITYNAME = businesstripInfo.STARTCITYNAME;
                            travelReimbursement.ENDCITYNAME = businesstripInfo.ENDCITYNAME;

                            travelReimbursement.CREATEUSERID = businesstripInfo.CREATEUSERID;
                            travelReimbursement.CREATEUSERNAME = businesstripInfo.CREATEUSERNAME;
                            travelReimbursement.CREATEPOSTID = businesstripInfo.CREATEPOSTID;
                            travelReimbursement.CREATEDEPARTMENTID = businesstripInfo.CREATEDEPARTMENTID;
                            travelReimbursement.CREATECOMPANYID = businesstripInfo.CREATECOMPANYID;

                            postLevel = businesstripInfo.POSTLEVEL;
                            //client.GetEmployeePostBriefByEmployeeIDAsync(businesstripInfo.OWNERID, e.UserState);
                            //if (businesstripInfo.BUSINESSTRIPID != null)
                            //{
                            //    Travelmanagement.GetBusinesstripDetailAsync(businesstripInfo.BUSINESSTRIPID);//申请明细
                            //}
                            Travelmanagement.GetTravelSolutionByCompanyIDAsync(businesstripInfo.OWNERCOMPANYID, null, null, e.UserState);//出差方案
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        //跳过一下步骤
        #region 3获出差人岗位级别
        void client_GetEmployeePostBriefByEmployeeIDCompleted(object sender, GetEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }

            if (e.Result != null)
            {
                employeepost = e.Result;
                if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == businesstripInfo.OWNERPOSTID).FirstOrDefault() != null)
                {
                    postLevel = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == businesstripInfo.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();//获取出差人的岗位级别
                }
                else
                {
                    var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
                    postLevel = ent != null ? ent.POSTLEVEL.ToString() : "0 ";
                }
                //if (ReportSwitch == true)
                //{
                //    Travelmanagement.GetTravelSolutionByCompanyIDAsync(businesstripInfo.OWNERCOMPANYID, null, null);//出差方案
                //}
                if (ReimbursementSwitch == true)
                {
                    Travelmanagement.GetTravelSolutionByCompanyIDAsync(businesstripInfo.OWNERCOMPANYID, null, null, e.UserState);//出差方案
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("对不起，该员工已离职，不能进行该操作"));
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
            }
        }
        #endregion

        #region 4获取出差人所在公司的出差方案
        /// <summary>
        /// 获取出差方案设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Travelmanagement_GetTravelSolutionByCompanyIDCompleted(object sender, GetTravelSolutionByCompanyIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    if (e.UserState != null)
                    {
                        Button btn = e.UserState as Button;
                        btn.IsEnabled = true;
                    }
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                if (e.Result != null)
                {
                    travelsolutions = e.Result;//出差方案
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "没有找到对应的出差方案，不能产生出差报销单", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                if (e.PlaneObj != null)
                {
                    cantaketheplaneline = e.PlaneObj.ToList();//乘坐飞机线路设置
                }
                if (e.StandardObj != null)
                {
                    takethestandardtransport = e.StandardObj.ToList();//乘坐交通工具设置
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "没有获取到交通工具设置，不能产生出差报销单", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                Travelmanagement.GetTravleAreaAllowanceByPostValueAsync(postLevel, travelsolutions.TRAVELSOLUTIONSID, null, e.UserState);
            }
            catch (Exception ex)
            {
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 5获取出差人交通工具级别设置
        void TrC_GetTravleAreaAllowanceByPostValueCompleted(object sender, GetTravleAreaAllowanceByPostValueCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    if (e.UserState != null)
                    {
                        Button btn = e.UserState as Button;
                        btn.IsEnabled = true;
                    }
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != null)
                    {
                        areaallowance = e.Result.ToList();
                        areacitys = e.citys.ToList();

                        if (businesstripInfo.BUSINESSTRIPID != null)
                        {
                            Travelmanagement.GetBusinesstripDetailAsync(businesstripInfo.BUSINESSTRIPID);//申请明细
                        }
                    }
                    else
                    {
                        IsCanSave = false;
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "您公司对应的出差方案没有补贴，请重新关联出差方案", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion
        //停止跳过

        #region 6获取出差申请子表数据，并新增出差报销主子表数据
        /// <summary>
        /// 获取出差报告子表数据(查询完后将报告的明细保存到报销中)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Travelmanagement_GetBusinesstripDetailCompleted(object sender, GetBusinesstripDetailCompletedEventArgs e)//查询报告明细
        {
            try
            {
                if (ReimbursementSwitch == true)
                {
                    List<T_OA_BUSINESSTRIPDETAIL> BusinessTripDetail = new List<T_OA_BUSINESSTRIPDETAIL>();
                    if (e.Result.Count > 0)
                    {
                        TrDetail_Gloabal.Clear();//清理报销子表
                        BusinessTripDetail = e.Result.ToList();
                        List<string> cityscode = new List<string>();
                        double BusinessDays = 0;
                        int i = 0;
                        double total = 0;

                        foreach (var detail in BusinessTripDetail)
                        {
                            i++;
                            double toodays = 0;

                            //计算本次出差的时间
                            List<string> list = new List<string>{detail.BUSINESSDAYS};
                            if (detail.BUSINESSDAYS != null)
                            {
                                double totalHours = System.Convert.ToDouble(list[0]);
                                BusinessDays += totalHours;//总天数
                                toodays = totalHours;//单条数据的天数
                            }
                            double tresult = toodays;//计算本次出差的总天数

                            T_OA_REIMBURSEMENTDETAIL TrListInfo = new T_OA_REIMBURSEMENTDETAIL();
                            TrListInfo.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();

                            TrListInfo.STARTDATE = detail.STARTDATE;//开始时间
                            TrListInfo.ENDDATE = detail.ENDDATE;//结束时间
                            TrListInfo.BUSINESSDAYS = detail.BUSINESSDAYS;//出差天数
                            TrListInfo.DEPCITY = detail.DEPCITY;//出发城市
                            TrListInfo.DESTCITY = detail.DESTCITY;//目标城市
                            TrListInfo.PRIVATEAFFAIR = detail.PRIVATEAFFAIR;//是否私事
                            TrListInfo.GOOUTTOMEET = detail.GOOUTTOMEET;//外出开会
                            TrListInfo.COMPANYCAR = detail.COMPANYCAR;//公司派车
                            TrListInfo.TYPEOFTRAVELTOOLS = detail.TYPEOFTRAVELTOOLS;//交通工具类型
                            TrListInfo.TAKETHETOOLLEVEL = detail.TAKETHETOOLLEVEL;//交通工具级别
                            TrListInfo.CREATEDATE = Convert.ToDateTime(businesstripInfo.UPDATEDATE);//创建时间
                            TrListInfo.CREATEUSERNAME = businesstripInfo.CREATEUSERNAME;//创建人
                            cityscode.Add(TrListInfo.DESTCITY);
                           
                            #region 废弃逻辑
                            T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                            string cityValue = cityscode[i - 1];//目标城市值
                            entareaallowance = GetAllowanceByCityValue(cityValue);

                            #region 根据本次出差的总天数,根据天数获取相应的补贴
                            if (travelsolutions != null)
                            {
                                if (tresult <= int.Parse(travelsolutions.MINIMUMINTERVALDAYS))//本次出差总时间小于等于设定天数的报销标准
                                {
                                    if (entareaallowance != null)
                                    {
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;//交通补贴
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    if (entareaallowance.TRANSPORTATIONSUBSIDIES != null)
                                                    {
                                                        TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * toodays).ToString());
                                                    }
                                                }
                                                else
                                                {
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//餐费补贴
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    TrListInfo.MEALSUBSIDIES = decimal.Parse((Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * toodays).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (int.Parse(postLevel) <= travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                }
                            }
                            #endregion

                            #region 如果出差天数大于设定的最大天数,按驻外标准获取补贴
                            if (travelsolutions != null)
                            {
                                if (tresult > int.Parse(travelsolutions.MAXIMUMRANGEDAYS))
                                {
                                    if (entareaallowance != null)
                                    {
                                        double DbTranceport = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES);
                                        double DbMeal = Convert.ToDouble(entareaallowance.MEALSUBSIDIES);
                                        double tfSubsidies = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);
                                        double mealSubsidies = Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbTranceport;
                                                    double middlemoney = (Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS) - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * tfSubsidies;
                                                    double lastmoney = (tresult - Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS)) * Convert.ToDouble(entareaallowance.OVERSEASSUBSIDIES);
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((minmoney + middlemoney + lastmoney).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbMeal;
                                                    double middlemoney = (Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS) - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * mealSubsidies;
                                                    double lastmoney = (tresult - Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS)) * Convert.ToDouble(entareaallowance.OVERSEASSUBSIDIES);
                                                    TrListInfo.MEALSUBSIDIES = decimal.Parse((minmoney + middlemoney + lastmoney).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (int.Parse(postLevel) <= travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                }
                            }
                            #endregion

                            #region 如果出差时间大于设定的最小天数并且小于设定的最大天数的报销标准
                            if (travelsolutions != null)
                            {
                                if (tresult >= Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) && tresult <= Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS))
                                {
                                    if (entareaallowance != null)
                                    {
                                        double DbTranceport = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES);
                                        double DbMeal = Convert.ToDouble(entareaallowance.MEALSUBSIDIES);
                                        double tfSubsidies = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);
                                        double mealSubsidies = Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbTranceport;
                                                    double middlemoney = (tresult - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * tfSubsidies;
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((minmoney + middlemoney).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    //最小区间段金额
                                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbMeal;
                                                    //中间区间段金额
                                                    double middlemoney = (tresult - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * mealSubsidies;
                                                    TrListInfo.MEALSUBSIDIES = decimal.Parse((minmoney + middlemoney).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (int.Parse(postLevel) <= travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                }
                            }
                            total += Convert.ToDouble(TrListInfo.TRANSPORTATIONSUBSIDIES + TrListInfo.MEALSUBSIDIES);
                            travelReimbursement.THETOTALCOST = decimal.Parse(total.ToString());//差旅费用总和
                            travelReimbursement.REIMBURSEMENTOFCOSTS = decimal.Parse(total.ToString());//报销费用总和

                            #endregion
                            #endregion

                            TrDetail_Gloabal.Add(TrListInfo);
                        }
                        string result = BusinessDays.ToString(); //计算本次出差的总时间,超过24小时天数加1
                        travelReimbursement.COMPUTINGTIME = result;//总时间
                        Button btn = e.UserState as Button;
                        travelReimbursement.T_OA_REIMBURSEMENTDETAIL = null;//清空子表，以免增加多余子表数据
                        Travelmanagement.TravelReimbursementAddAsync(travelReimbursement, TrDetail_Gloabal, btn);//保存出差报销
                    }
                }
            }
            catch (Exception ex)
            {
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 7添加出差报销Completed
        void TrC_TravelReimbursementAddCompleted(object sender, TravelReimbursementAddCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    if (e.UserState != null)
                    {
                        Button btn = e.UserState as Button;
                        btn.IsEnabled = true;
                    }
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    if (e.Result != "")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Result), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        return;
                    }
                    Travelmanagement.GetTravelReimbursementByIdAsync(travelReimbursement.TRAVELREIMBURSEMENTID);
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 8根据生成的报销ID查询出差报销数据，并弹出显示出差报销页面
        void TrC_GetTravelReimbursementByIdCompleted(object sender, GetTravelReimbursementByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    if (e.Result == null)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }
                    travelReimbursement = e.Result;
                    if (!string.IsNullOrEmpty(travelReimbursement.TRAVELREIMBURSEMENTID))
                    {
                        BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Edit, businesstrID);
                        EntityBrowser browser = new EntityBrowser(AddWin);
                        browser.RemoveSMTLoading();
                        browser.FormType = FormTypes.Edit;
                        browser.MinWidth = 980;
                        browser.MinHeight = 445;
                        browser.TitleContent = "出差申请";
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                    }
                    LoadData();//重新加载数据(主要用于刷新"是否报销"按钮的状态)
                    ReimbursementSwitch = false;//关闭开关
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion
    }
}
