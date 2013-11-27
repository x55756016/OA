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
        
        #region LoadingRow事件

        //定义combox默认颜色变量
        Brush tempcomTypeBorderBrush;
        Brush tempcomTypeForeBrush;
        Brush tempcomLevelBorderBrush;
        Brush tempcomLevelForeBrush;
        Brush txtASubsidiesForeBrush;
        Brush txtASubsidiesBorderBrush;
        /// <summary>
        /// 编辑页面绑定grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DaGrEdit_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                T_OA_REIMBURSEMENTDETAIL tmp = (T_OA_REIMBURSEMENTDETAIL)e.Row.DataContext;
                //出发时间
                DateTimePicker dpStartTime = DaGrEdit.Columns[0].GetCellContent(e.Row).FindName("StartTime") as DateTimePicker;
                //出发城市
                SearchCity myCity = DaGrEdit.Columns[1].GetCellContent(e.Row).FindName("txtDEPARTURECITY") as SearchCity;
                //到达时间
                DateTimePicker dpEndTime = DaGrEdit.Columns[2].GetCellContent(e.Row).FindName("EndTime") as DateTimePicker;
                //到达城市
                SearchCity myCitys = DaGrEdit.Columns[3].GetCellContent(e.Row).FindName("txtTARGETCITIES") as SearchCity;
                //交通费
                TextBox txtTranSportcosts = DaGrEdit.Columns[8].GetCellContent(e.Row).FindName("txtTRANSPORTCOSTS") as TextBox;
                //住宿标准
                TextBox txtASubsidies = DaGrEdit.Columns[9].GetCellContent(e.Row).FindName("txtACCOMMODATION") as TextBox;
                //交通补贴
                TextBox txtTFSubsidies = DaGrEdit.Columns[10].GetCellContent(e.Row).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;
                //餐费补贴
                TextBox txtMealSubsidies = DaGrEdit.Columns[11].GetCellContent(e.Row).FindName("txtMEALSUBSIDIES") as TextBox;
                TravelDictionaryComboBox ComVechile = DaGrEdit.Columns[6].GetCellContent(e.Row).FindName("ComVechileType") as TravelDictionaryComboBox;
                TravelDictionaryComboBox ComLevel = DaGrEdit.Columns[7].GetCellContent(e.Row).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                //其他费用
                TextBox txtOtherCosts = DaGrEdit.Columns[12].GetCellContent(e.Row).FindName("txtOtherCosts") as TextBox;
                CheckBox IsCheck = DaGrEdit.Columns[13].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
                CheckBox IsCheckMeet = DaGrEdit.Columns[14].GetCellContent(e.Row).FindName("myChkBoxMeet") as CheckBox;
                CheckBox IsCheckCar = DaGrEdit.Columns[15].GetCellContent(e.Row).FindName("myChkBoxCar") as CheckBox;
                ImageButton MyButton_Delbaodao = DaGrEdit.Columns[16].GetCellContent(e.Row).FindName("myDelete") as ImageButton;

                //对默认控件的颜色进行赋值
                tempcomTypeBorderBrush = ComVechile.BorderBrush;
                tempcomTypeForeBrush = ComVechile.Foreground;
                tempcomLevelBorderBrush = ComLevel.BorderBrush;
                tempcomLevelForeBrush = ComLevel.Foreground;
                txtASubsidiesForeBrush = txtASubsidies.Foreground;
                txtASubsidiesBorderBrush = txtASubsidies.BorderBrush;
                T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();

                if (BtnNewButton == true)
                {
                    myCitys.TxtSelectedCity.Text = string.Empty;
                }
                else
                {
                    BtnNewButton = false;
                }

                MyButton_Delbaodao.Margin = new Thickness(0);
                MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
                MyButton_Delbaodao.Tag = tmp;
                myCity.Tag = tmp;
                myCitys.Tag = tmp;

                //查询出发城市&目标城市&&将ID转换为Name
                if (DaGrEdit.ItemsSource != null)
                {
                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrEdit.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                    int i = 0;
                    foreach (var obje in objs)
                    {

                        if (obje.REIMBURSEMENTDETAILID == tmp.REIMBURSEMENTDETAILID)//判断记录的ID是否相同
                        {
                            string dictid = "";
                            ComVechile.SelectedIndex = 0;
                            ComLevel.SelectedIndex = 0;
                            DaGrEdit.SelectedItem = e.Row;
                            T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                            T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();

                            entareaallowance = StandardsMethod(i);

                            if (formType != FormTypes.New)
                            {
                                if (myCity != null)//出发城市
                                {
                                    if (obje.DEPCITY != null)
                                    {
                                        //注释原因：obje.depcity仍然是中文而不是数字
                                        myCity.TxtSelectedCity.Text = GetCityName(tmp.DEPCITY);
                                        if (TravelDetailList_Golbal.Count() > 1)
                                        {
                                            if (i > 1)
                                            {
                                                myCity.IsEnabled = false;
                                                ((DataGridCell)((StackPanel)myCity.Parent).Parent).IsEnabled = false;
                                            }
                                        }
                                    }
                                }
                                if (myCitys != null)//目标城市
                                {
                                    if (obje.DESTCITY != null)
                                    {
                                        myCitys.TxtSelectedCity.Text = GetCityName(obje.DESTCITY);
                                    }
                                }
                                if (obje.PRIVATEAFFAIR == "1")//私事
                                {
                                    IsCheck.IsChecked = true;
                                }
                                if (obje.GOOUTTOMEET == "1")//外出开会
                                {
                                    IsCheckMeet.IsChecked = true;
                                }
                                if (obje.COMPANYCAR == "1")//公司派车
                                {
                                    IsCheckCar.IsChecked = true;
                                }
                                if (txtASubsidies != null)//住宿标准
                                {
                                    txtASubsidies.Text = obje.ACCOMMODATION.ToString();
                                }
                                if (txtTFSubsidies != null)//交通补贴
                                {
                                    txtTFSubsidies.Text = obje.TRANSPORTATIONSUBSIDIES.ToString();
                                    ((DataGridCell)((StackPanel)txtTFSubsidies.Parent).Parent).IsEnabled = false;
                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长I级以上级别的补贴标准
                                    {
                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                        if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
                                        {
                                            ComfirmWindow com = new ComfirmWindow();
                                            com.OnSelectionBoxClosed += (obj, result) =>
                                            {
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            };
                                            if (formType == FormTypes.Audit || formType == FormTypes.Browse
                                                  || obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1") return;
                                            if (obje.REIMBURSEMENTDETAILID == objs.LastOrDefault().REIMBURSEMENTDETAILID)
                                            {
                                                return;//最后一条无补贴
                                            }
                                            if (obje.TRANSPORTATIONSUBSIDIES == null || obje.TRANSPORTATIONSUBSIDIES == 0)
                                            {
                                                com.SelectionBox("操作确认", "当前单据没有获取到交通补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                            }
                                        }
                                    }
                                }
                                if (txtMealSubsidies != null)//餐费补贴
                                {
                                    txtMealSubsidies.Text = obje.MEALSUBSIDIES.ToString();
                                    ((DataGridCell)((StackPanel)txtMealSubsidies.Parent).Parent).IsEnabled = false;
                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                        if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
                                        {
                                            ComfirmWindow com = new ComfirmWindow();
                                            com.OnSelectionBoxClosed += (obj, result) =>
                                            {
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            };
                                            if (formType == FormTypes.Audit || formType == FormTypes.Browse
                                              || obje.GOOUTTOMEET == "1") return;
                                            if (obje.REIMBURSEMENTDETAILID == objs.LastOrDefault().REIMBURSEMENTDETAILID)
                                            {
                                                return;//最后一条无补贴
                                            }
                                            if (obje.MEALSUBSIDIES == null
                                                || obje.MEALSUBSIDIES == 0)
                                            {
                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                            }
                                        }
                                    }
                                }

                                #region 查看和审核时隐藏DataGrid模板中的控件
                                if (formType == FormTypes.Browse || formType == FormTypes.Audit)
                                {
                                    txtASubsidies.IsReadOnly = true;
                                    txtTFSubsidies.IsReadOnly = true;
                                    txtMealSubsidies.IsReadOnly = true;
                                    txtOtherCosts.IsReadOnly = true;
                                    txtTranSportcosts.IsReadOnly = true;
                                    ComVechile.IsEnabled = false;
                                    ComLevel.IsEnabled = false;
                                }
                                if (formType != FormTypes.New || formType != FormTypes.Edit)
                                {
                                    if (TravelReimbursement_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                                    {
                                        txtASubsidies.IsReadOnly = true;
                                        txtTFSubsidies.IsReadOnly = true;
                                        txtMealSubsidies.IsReadOnly = true;
                                        txtOtherCosts.IsReadOnly = true;
                                        txtTranSportcosts.IsReadOnly = true;
                                        ComVechile.IsEnabled = false;
                                        ComLevel.IsEnabled = false;
                                    }
                                }
                                if (entareaallowance != null)
                                {
                                    if (txtASubsidies.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * obje.THENUMBEROFNIGHTS.ToDouble())//判断住宿费超标
                                    {
                                        txtASubsidies.BorderBrush = new SolidColorBrush(Colors.Red);
                                        txtASubsidies.Foreground = new SolidColorBrush(Colors.Red);
                                        txtAccommodation.Visibility = Visibility.Visible;
                                        this.txtAccommodation.Text = "住宿费超标";
                                    }
                                    if (txtASubsidies.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * obje.THENUMBEROFNIGHTS.ToDouble())
                                    {
                                        if (txtASubsidiesForeBrush != null)
                                        {
                                            txtASubsidies.Foreground = txtASubsidiesForeBrush;
                                        }
                                        if (txtASubsidiesBorderBrush != null)
                                        {
                                            txtASubsidies.BorderBrush = txtASubsidiesBorderBrush;
                                        }
                                    }
                                }
                                #endregion

                                #region 获取交通工具类型和级别
                                if (ComVechile != null)//交通工具类型
                                {
                                    type = ComVechile.SelectedItem as T_SYS_DICTIONARY;
                                    level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                                    var thd = takethestandardtransport.FirstOrDefault();
                                    thd = this.GetVehicleTypeValue("");

                                    foreach (T_SYS_DICTIONARY Region in ComVechile.Items)
                                    {
                                        if (thd != null)
                                        {
                                            dictid = Region.DICTIONARYID;
                                            if (Region.DICTIONARYVALUE.ToString() == tmp.TYPEOFTRAVELTOOLS)
                                            {
                                                if (takethestandardtransport.Count() > 0)
                                                {
                                                    ComVechile.SelectedItem = Region;
                                                    if (thd.TYPEOFTRAVELTOOLS.ToInt32() > Region.DICTIONARYVALUE)
                                                    {
                                                        ComVechile.BorderBrush = new SolidColorBrush(Colors.Red);
                                                        ComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                                        ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                        ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                        this.txtTips.Visibility = Visibility.Visible;
                                                        this.txtTips.Text = "交通工具超标";
                                                    }
                                                    if (thd.TYPEOFTRAVELTOOLS.ToInt32() <= Region.DICTIONARYVALUE)
                                                    {
                                                        if (tempcomTypeBorderBrush != null)
                                                        {
                                                            ComVechile.BorderBrush = tempcomTypeBorderBrush;
                                                        }
                                                        if (tempcomTypeForeBrush != null)
                                                        {
                                                            ComVechile.Foreground = tempcomTypeForeBrush;
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (ComLevel != null)//交通工具级别
                                {
                                    var ents = from ent in ListVechileLevel
                                               where ent.T_SYS_DICTIONARY2.DICTIONARYID == dictid
                                               select ent;
                                    ComLevel.ItemsSource = ents.ToList();
                                    if (ents.Count() > 0)
                                    {
                                        type = ComVechile.SelectedItem as T_SYS_DICTIONARY;
                                        level = ComLevel.SelectedItem as T_SYS_DICTIONARY;

                                        var thd = takethestandardtransport.FirstOrDefault();
                                        if (type != null)
                                        {
                                            thd = this.GetVehicleTypeValue(type.DICTIONARYVALUE.ToString());
                                        }
                                        if (thd != null)
                                        {
                                            foreach (T_SYS_DICTIONARY RegionLevel in ComLevel.Items)
                                            {
                                                if (RegionLevel.DICTIONARYVALUE.ToString() == tmp.TAKETHETOOLLEVEL)
                                                {
                                                    ComLevel.SelectedItem = RegionLevel;
                                                    if (thd.TAKETHETOOLLEVEL.ToInt32() <= RegionLevel.DICTIONARYVALUE)
                                                    {
                                                        if (tempcomLevelForeBrush != null)
                                                        {
                                                            ComLevel.Foreground = tempcomLevelForeBrush;
                                                        }
                                                        if (tempcomLevelBorderBrush != null)
                                                        {
                                                            ComLevel.BorderBrush = tempcomLevelBorderBrush;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (thd.TAKETHETOOLLEVEL.ToInt32() > RegionLevel.DICTIONARYVALUE)
                                                        {
                                                            ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                            ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                            this.txtTips.Visibility = Visibility.Visible;
                                                            this.txtTips.Text = "交通工具超标";
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            if (tempcomLevelForeBrush != null)
                                                            {
                                                                ComLevel.Foreground = tempcomLevelForeBrush;
                                                            }
                                                            if (tempcomLevelBorderBrush != null)
                                                            {
                                                                ComLevel.BorderBrush = tempcomLevelBorderBrush;
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }// ComLevel != null
                                }
                                #endregion
                            }
                            else
                            {
                                continue;
                            }
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }


        /// <summary>
        /// 查看、审核时用(将DataGr模版中的控件全部替换为TextBlock,以便在新平台中节约空间)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DaGrReadOnly_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                #region 设置值
                T_OA_REIMBURSEMENTDETAIL tmp = (T_OA_REIMBURSEMENTDETAIL)e.Row.DataContext;

                TextBlock dpStartTimelook = DaGrReadOnly.Columns[0].GetCellContent(e.Row).FindName("tbStartTime") as TextBlock;
                TextBlock dpEndTime = DaGrReadOnly.Columns[2].GetCellContent(e.Row).FindName("tbEndTime") as TextBlock;
                TextBlock myCity = DaGrReadOnly.Columns[1].GetCellContent(e.Row).FindName("tbDEPARTURECITY") as TextBlock;
                TextBlock myCitys = DaGrReadOnly.Columns[3].GetCellContent(e.Row).FindName("tbTARGETCITIES") as TextBlock;
                TextBox txtTranSportcosts = DaGrReadOnly.Columns[8].GetCellContent(e.Row).FindName("txtTRANSPORTCOSTS") as TextBox;//交通费
                TextBox txtASubsidies = DaGrReadOnly.Columns[9].GetCellContent(e.Row).FindName("txtACCOMMODATION") as TextBox;//住宿标准
                TextBox txtTFSubsidies = DaGrReadOnly.Columns[10].GetCellContent(e.Row).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
                TextBox txtMealSubsidies = DaGrReadOnly.Columns[11].GetCellContent(e.Row).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
                TextBlock ComVechile = DaGrReadOnly.Columns[6].GetCellContent(e.Row).FindName("tbComVechileType") as TextBlock;
                TextBlock ComLevel = DaGrReadOnly.Columns[7].GetCellContent(e.Row).FindName("tbComVechileTypeLeve") as TextBlock;
                TextBox txtOtherCosts = DaGrReadOnly.Columns[12].GetCellContent(e.Row).FindName("txtOtherCosts") as TextBox;//其他费用
                CheckBox IsCheck = DaGrReadOnly.Columns[13].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
                CheckBox IsCheckMeet = DaGrReadOnly.Columns[14].GetCellContent(e.Row).FindName("myChkBoxMeet") as CheckBox;
                CheckBox IsCheckCar = DaGrReadOnly.Columns[15].GetCellContent(e.Row).FindName("myChkBoxCar") as CheckBox;

                //对默认控件的颜色进行赋值
                tempcomTypeForeBrush = ComVechile.Foreground;
                tempcomLevelForeBrush = ComLevel.Foreground;
                txtASubsidiesForeBrush = txtASubsidies.Foreground;
                txtASubsidiesBorderBrush = txtASubsidies.BorderBrush;
                //DaGrReadOnly.Columns[5].Visibility = Visibility.Collapsed;

                if (BtnNewButton == true)
                {
                    myCitys.Text = string.Empty;
                    //citysStartList_Golbal.Add(tmp.DEPCITY);
                }
                else
                {
                    BtnNewButton = false;
                }

                //查询出发城市&目标城市&&将ID转换为Name
                if (DaGrReadOnly.ItemsSource != null)
                {
                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrReadOnly.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                    int i = 0;
                    foreach (var obje in objs)
                    {

                        if (obje.REIMBURSEMENTDETAILID == tmp.REIMBURSEMENTDETAILID)//判断记录的ID是否相同
                        {
                            DaGrReadOnly.SelectedItem = e.Row;
                            T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                            T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();

                            T_OA_AREAALLOWANCE entareaallowance = StandardsMethod(i);

                            #region 修改、查看、审核

                            if (formType != FormTypes.New)
                            {
                                #region 获取目标城市、各项补贴数据
                                if (myCity != null)//出发城市
                                {
                                    if (obje.DEPCITY != null)
                                    {
                                        myCity.Text = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(obje.DEPCITY);
                                    }
                                }
                                if (myCitys != null)//目标城市
                                {
                                    if (obje.DESTCITY != null)
                                    {
                                        myCitys.Text = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(obje.DESTCITY);
                                    }
                                }
                                if (obje.TYPEOFTRAVELTOOLS != null)//交通工具类型
                                {
                                    ComVechile.Text = GetTypeName(obje.TYPEOFTRAVELTOOLS);
                                }
                                if (obje.TAKETHETOOLLEVEL != null)//交通工具级别
                                {
                                    ComLevel.Text = GetLevelName(obje.TAKETHETOOLLEVEL, GetTypeId(obje.TYPEOFTRAVELTOOLS));
                                }
                                if (obje.PRIVATEAFFAIR == "1")//私事
                                {
                                    IsCheck.IsChecked = true;
                                }
                                if (obje.GOOUTTOMEET == "1")//外出开会
                                {
                                    IsCheckMeet.IsChecked = true;
                                }
                                if (obje.COMPANYCAR == "1")//公司派车
                                {
                                    IsCheckCar.IsChecked = true;
                                }
                                if (txtASubsidies != null)//住宿标准
                                {
                                    txtASubsidies.Text = obje.ACCOMMODATION.ToString();
                                }
                                if (txtTFSubsidies != null)//交通补贴
                                {
                                    txtTFSubsidies.Text = obje.TRANSPORTATIONSUBSIDIES.ToString();
                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                        if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
                                        {
                                            ComfirmWindow com = new ComfirmWindow();
                                            com.OnSelectionBoxClosed += (obj, result) =>
                                            {
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            };
                                            if (formType == FormTypes.Audit || formType == FormTypes.Browse
                                                 || obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1") return;
                                            if (obje.REIMBURSEMENTDETAILID == objs.LastOrDefault().REIMBURSEMENTDETAILID)
                                            {
                                                return;//最后一条无补贴
                                            }
                                            if (obje.TRANSPORTATIONSUBSIDIES == null || obje.TRANSPORTATIONSUBSIDIES==0)
                                            {
                                                if (formType == FormTypes.Audit) return;
                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                            }
                                        }
                                    }
                                }
                                if (txtMealSubsidies != null)//餐费补贴
                                {
                                    txtMealSubsidies.Text = obje.MEALSUBSIDIES.ToString();
                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                        if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
                                        {
                                            ComfirmWindow com = new ComfirmWindow();
                                            com.OnSelectionBoxClosed += (obj, result) =>
                                            {
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            };
                                            if (formType == FormTypes.Audit || formType == FormTypes.Browse
                                               || obje.GOOUTTOMEET == "1") return;
                                            if (obje.REIMBURSEMENTDETAILID == objs.LastOrDefault().REIMBURSEMENTDETAILID)
                                            {
                                                return;//最后一条无补贴
                                            }
                                            if (obje.MEALSUBSIDIES == null
                                                || obje.MEALSUBSIDIES == 0)
                                            {
                                                if (formType == FormTypes.Audit) return;
                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region 查看和审核时隐藏DataGrid模板中的控件
                                if (formType == FormTypes.Browse || formType == FormTypes.Audit)
                                {
                                    txtASubsidies.IsReadOnly = true;
                                    txtTFSubsidies.IsReadOnly = true;
                                    txtMealSubsidies.IsReadOnly = true;
                                    txtOtherCosts.IsReadOnly = true;
                                    txtTranSportcosts.IsReadOnly = true;
                                }
                                if (formType != FormTypes.New || formType != FormTypes.Edit)
                                {
                                    if (TravelReimbursement_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                                    {
                                        txtASubsidies.IsReadOnly = true;
                                        txtTFSubsidies.IsReadOnly = true;
                                        txtMealSubsidies.IsReadOnly = true;
                                        txtOtherCosts.IsReadOnly = true;
                                        txtTranSportcosts.IsReadOnly = true;
                                    }
                                }
                                if (entareaallowance != null)
                                {
                                    if (txtASubsidies.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * obje.THENUMBEROFNIGHTS.ToDouble())//判断住宿费超标
                                    {
                                        txtASubsidies.BorderBrush = new SolidColorBrush(Colors.Red);
                                        txtASubsidies.Foreground = new SolidColorBrush(Colors.Red);
                                        txtAccommodation.Visibility = Visibility.Visible;
                                        this.txtAccommodation.Text = "住宿费超标";
                                    }
                                    if (txtASubsidies.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * obje.THENUMBEROFNIGHTS.ToDouble())
                                    {
                                        if (txtASubsidiesForeBrush != null && txtASubsidies.Foreground == null)
                                        {
                                            txtASubsidies.Foreground = txtASubsidiesForeBrush;
                                        }
                                        if (txtASubsidiesBorderBrush != null && txtASubsidies.BorderBrush == null)
                                        {
                                            txtASubsidies.BorderBrush = txtASubsidiesBorderBrush;
                                        }
                                    }
                                }
                                #endregion

                                #region 获取交通工具类型、级别
                                if (ComVechile != null)
                                {
                                    var thd = takethestandardtransport.FirstOrDefault();
                                    thd = this.GetVehicleTypeValue("");

                                    if (thd != null)
                                    {
                                        if (takethestandardtransport.Count() > 0)
                                        {
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() > obje.TYPEOFTRAVELTOOLS.ToInt32())
                                            {
                                                ComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                                ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                this.txtTips.Visibility = Visibility.Visible;
                                                this.txtTips.Text = "交通工具超标";
                                            }
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() <= obje.TYPEOFTRAVELTOOLS.ToInt32())
                                            {
                                                if (tempcomTypeForeBrush != null)
                                                {
                                                    ComVechile.Foreground = tempcomTypeForeBrush;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (ComLevel != null)//交通工具级别
                                {
                                    //获取交通工具类型
                                    int ii = CheckTraveToolStand(obje.TYPEOFTRAVELTOOLS.ToString(), obje.TAKETHETOOLLEVEL.ToString(), EmployeePostLevel);
                                    switch (ii)
                                    {
                                        case 0://类型超标
                                            ComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                            ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                            this.txtTips.Visibility = Visibility.Visible;
                                            this.txtTips.Text = "交通工具超标";
                                            break;
                                        case 1://级别超标
                                            ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                            this.txtTips.Visibility = Visibility.Visible;
                                            this.txtTips.Text = "交通工具超标";
                                            break;
                                        case 2://没超标，则隐藏
                                            this.txtTips.Visibility = Visibility.Collapsed;
                                            this.txtTips.Text = "";
                                            break;
                                    }
                                }
                                #endregion
                            }
                            #endregion
                        }
                        i++;
                    }

                #endregion
                    //CountMoneyA();
                    //CountTravelDays(tmp,e);
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
