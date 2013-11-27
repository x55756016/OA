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
    public class TravelReimbursementControl
    {
        #region 查询出差报销主表，本页面打开的主入口1
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
                    isPageloadCompleted = true;
                    TravelReimbursement_Golbal = e.Result;

                    //ljx  2011-8-29  
                    if (formType == FormTypes.Edit)
                    {
                        if (TravelReimbursement_Golbal.CHECKSTATE == (Convert.ToInt32(CheckStates.Approving)).ToString()
                            || TravelReimbursement_Golbal.CHECKSTATE == (Convert.ToInt32(CheckStates.Approved)).ToString()
                            || TravelReimbursement_Golbal.CHECKSTATE == (Convert.ToInt32(CheckStates.UnApproved)).ToString())
                        {
                            formType = FormTypes.Audit;
                            DaGrEditScrollView.Visibility = Visibility.Collapsed;
                            DaGrReadOnlyScrollView.Visibility = Visibility.Visible;
                            Utility.InitFileLoad("TravelRequest", TravelReimbursement_Golbal.TRAVELREIMBURSEMENTID, formType, uploadFile);
                        }
                    }
                    if (formType == FormTypes.Resubmit)//重新提交
                    {
                        TravelReimbursement_Golbal.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                    }

                    txtPeopleTravel.Text = TravelReimbursement_Golbal.CLAIMSWERENAME;//报销人
                    if (!string.IsNullOrEmpty(TravelReimbursement_Golbal.TEL))
                    {
                        txtTELL.Text = TravelReimbursement_Golbal.TEL;//联系电话
                    }
                    ReimbursementTime.Text = TravelReimbursement_Golbal.CREATEDATE.Value.ToShortDateString();//报销时间
                    txtChargeApplyTotal.Text = TravelReimbursement_Golbal.REIMBURSEMENTOFCOSTS.ToString();//本次差旅总费用
                    txtSubTotal.Text = TravelReimbursement_Golbal.THETOTALCOST.ToString();//差旅合计

                    if (!string.IsNullOrEmpty(TravelReimbursement_Golbal.NOBUDGETCLAIMS))//报销单号
                    {
                        txtNoClaims.Text = string.Empty;
                        txtNoClaims.Text = TravelReimbursement_Golbal.NOBUDGETCLAIMS;
                    }
                    if (!string.IsNullOrEmpty(TravelReimbursement_Golbal.REMARKS))
                    {
                        txtRemark.Text = TravelReimbursement_Golbal.REMARKS;//备注
                    }

                    if (InitFB == false)
                    {
                        InitFBControl(TravelReimbursement_Golbal);
                    }
                    //HrPersonnelclient.GetEmployeePostBriefByEmployeeIDAsync(TravelReimbursement.OWNERID);
                    postName = TravelReimbursement_Golbal.OWNERPOSTNAME;
                    depName = TravelReimbursement_Golbal.OWNERDEPARTMENTNAME;
                    companyName = TravelReimbursement_Golbal.OWNERCOMPANYNAME;
                    string StrName = TravelReimbursement_Golbal.OWNERNAME + "-" + postName + "-" + depName + "-" + companyName;

                    txtPeopleTravel.Text = StrName;
                    ToolTipService.SetToolTip(txtPeopleTravel, StrName);

                    EmployeeName = TravelReimbursement_Golbal.OWNERNAME;//出差人

                    EmployeePostLevel = TravelReimbursement_Golbal.POSTLEVEL;


                    if (formType != FormTypes.New || formType != FormTypes.Edit)
                    {
                        if (TravelReimbursement_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                        {
                            BrowseShieldedControl();
                        }
                    }
                    else if (formType != FormTypes.Resubmit)
                    {
                        if (TravelReimbursement_Golbal.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                            TravelReimbursement_Golbal.CHECKSTATE == ((int)CheckStates.Approved).ToString() ||
                            TravelReimbursement_Golbal.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString())
                        {
                            BrowseShieldedControl();
                        }
                    }
                    //我的单据中用到(判断出差报告如果在未提交状态,FormType状态改为可编辑)
                    if (TravelReimbursement_Golbal.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        //将Form状态改为编辑
                        //formType = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                        //重新启用Form中的控件
                        txtTELL.IsReadOnly = false;
                        fbCtr.IsEnabled = true;
                        txtRemark.IsReadOnly = false;
                        textStandards.IsReadOnly = false;
                    }

                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(TravelReimbursement_Golbal.OWNERCOMPANYID, null, null);
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

        #region 获取出差方案2
        /// <summary>
        /// 获取出差方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TrC_GetTravelSolutionByCompanyIDCompleted(object sender, GetTravelSolutionByCompanyIDCompletedEventArgs e)//判断能否乘坐哪种类型的交通工具及级别
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                if (e.Result != null)
                {

                    travelsolutions = e.Result;//出差方案
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "您公司没有关联出差方案，请关联一套出差方案以便报销", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                if (e.PlaneObj != null)
                {
                    cantaketheplaneline = e.PlaneObj.ToList();//乘坐飞机线路设置
                }
                if (e.StandardObj != null)
                {
                    if (e.StandardObj.Count() > 0)
                    {
                        takethestandardtransport = e.StandardObj.ToList();//乘坐交通工具设置
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差方案中没有关联对应的交通工具设置", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差方案中没有关联对应的交通工具设置", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                OaPersonOfficeClient.GetTravleAreaAllowanceByPostValueAsync(EmployeePostLevel, travelsolutions.TRAVELSOLUTIONSID, null);
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        #endregion      

        #region "获取出差报销补助3"

        /// <summary>
        /// 根据岗位级别获取出差报销补助
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TrC_GetTravleAreaAllowanceByPostValueCompleted(object sender, GetTravleAreaAllowanceByPostValueCompletedEventArgs e)
        {
            try
            {

                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);

                }
                else
                {
                    if (e.Result != null)
                    {
                        areaallowance = e.Result.ToList();
                        areacitys = e.citys.ToList();
                    }
                    if (e.Result.Count() == 0)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "您公司的出差方案没有对应的出差补贴", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }

                if (TravelReimbursement_Golbal.T_OA_REIMBURSEMENTDETAIL.Count() > 0)
                {
                    BindDataGrid(TravelReimbursement_Golbal.T_OA_REIMBURSEMENTDETAIL);
                    RefreshUI(RefreshedTypes.All);
                    if (TravelReimbursement_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                    {
                        RefreshUI(RefreshedTypes.AuditInfo);
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

        #endregion

        #region DataGrid BindData 绑定显示出差报销数据4
        private void BindDataGrid(ObservableCollection<T_OA_REIMBURSEMENTDETAIL> obj)//加载出差报销子表
        {
            TravelDetailList_Golbal = obj;

            //citysStartList_Golbal.Clear();
            //citysEndList_Golbal.Clear();
            //foreach (T_OA_REIMBURSEMENTDETAIL detail in obj)
            //{
            //    citysStartList_Golbal.Add(detail.DEPCITY);
            //    citysEndList_Golbal.Add(detail.DESTCITY);
            //}
            if (formType != FormTypes.New && formType != FormTypes.Edit && formType != FormTypes.Resubmit)
            {
                TravelAllowance(true);
                DaGrReadOnly.ItemsSource = TravelDetailList_Golbal;
            }
            else
            {
                TravelAllowance(false);
                DaGrEdit.ItemsSource = TravelDetailList_Golbal;
            }
        }


        #endregion
    }
}
