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
        #region 住宿费控件事件
        private void txtACCOMMODATION_LostFocus(object sender, RoutedEventArgs e)
        {
            CountMoney();
        }
        #endregion

        #region 其他费用控件事件
        private void txtOtherCosts_LostFocus(object sender, RoutedEventArgs e)
        {
            CountMoney();
        }
        #endregion

        #region 私事myChkBox_Checked事件
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chk = sender as CheckBox;
                if (chk.IsChecked.Value)
                {
                    T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                    if (btlist != null)
                    {
                        btlist.PRIVATEAFFAIR = "1";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chk = sender as CheckBox;
                if (!chk.IsChecked.Value)
                {
                    T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                    if (btlist != null)
                    {
                        btlist.PRIVATEAFFAIR = "0";
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


        #region 住宿费，交通费，其他费用
        /// <summary>
        /// 计算 住宿费，交通费，其他费用
        /// </summary>
        private void CountMoney()
        {
            try
            {
                TravelTimeCalculation();

                double totall = 0;
                int i = 0;
                if (DaGrEdit.ItemsSource == null)
                {
                    return;
                }
                //住宿费，交通费，其他费用
                bool IsPassEd = false;//住宿费是否超标
                foreach (object obj in DaGrEdit.ItemsSource)
                {
                    i++;
                    if (DaGrEdit.Columns[8].GetCellContent(obj) == null)
                    {
                        return;
                    }
                    if (DaGrEdit.Columns[9].GetCellContent(obj) == null)
                    {
                        return;
                    }
                    if (DaGrEdit.Columns[12].GetCellContent(obj) == null)
                    {
                        return;
                    }
                    TextBox myDaysTime = DaGrEdit.Columns[5].GetCellContent(obj).FindName("txtTHENUMBEROFNIGHTS") as TextBox;
                    TextBox textTransportcosts = DaGrEdit.Columns[8].GetCellContent(obj).FindName("txtTRANSPORTCOSTS") as TextBox;
                    TextBox textAccommodation = DaGrEdit.Columns[9].GetCellContent(obj).FindName("txtACCOMMODATION") as TextBox;
                    TextBox textOthercosts = DaGrEdit.Columns[12].GetCellContent(obj).FindName("txtOtherCosts") as TextBox;
                    TextBox txtTFSubsidies = DaGrEdit.Columns[10].GetCellContent(obj).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
                    TextBox txtMealSubsidies = DaGrEdit.Columns[11].GetCellContent(obj).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴

                    T_OA_REIMBURSEMENTDETAIL obje = obj as T_OA_REIMBURSEMENTDETAIL;
                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrEdit.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                    //出差天数
                    double toodays = 0;
                    //获取出差补贴
                    T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                    string cityValue = citysEndList_Golbal[i - 1].Replace(",", "");//目标城市值
                    //根据城市查出差标准补贴（已根据岗位级别过滤）
                    entareaallowance = this.GetAllowanceByCityValue(cityValue);

                    //循环出差报告的天数
                    int k = 0;
                    if (formType == FormTypes.New)
                    {
                        foreach (T_OA_BUSINESSTRIPDETAIL objDetail in buipList)
                        {
                            k++;
                            if (k == i)
                            {
                                if (!string.IsNullOrEmpty(objDetail.BUSINESSDAYS))
                                {
                                    double totalHours = System.Convert.ToDouble(objDetail.BUSINESSDAYS);
                                    //出差天数
                                    toodays = totalHours;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var objDetail in objs)
                        {
                            k++;
                            if (k == i)
                            {
                                if (myDaysTime != null && !string.IsNullOrEmpty(myDaysTime.Text) && myDaysTime.Text != "0")
                                {
                                    double totalHours = System.Convert.ToDouble(myDaysTime.Text);
                                    //住宿天数
                                    toodays = totalHours;

                                    if (entareaallowance != null)
                                    {

                                        if (textAccommodation.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * toodays)//判断住宿费超标
                                        {
                                            //文本框标红
                                            textAccommodation.BorderBrush = new SolidColorBrush(Colors.Red);
                                            textAccommodation.Foreground = new SolidColorBrush(Colors.Red);
                                            this.txtAccommodation.Visibility = Visibility.Visible;
                                            IsPassEd = true;
                                            //this.txtAccommodation.Text = "住宿费超标";
                                        }

                                        if (textAccommodation.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * toodays)
                                        {
                                            if (txtASubsidiesForeBrush != null)
                                            {
                                                textAccommodation.Foreground = txtASubsidiesForeBrush;
                                            }
                                            if (txtASubsidiesBorderBrush != null)
                                            {
                                                textAccommodation.BorderBrush = txtASubsidiesBorderBrush;
                                            }
                                            string StrMessage = "";
                                            StrMessage = this.txtAccommodation.Text;
                                            if (string.IsNullOrEmpty(StrMessage))
                                            {
                                                this.txtAccommodation.Visibility = Visibility.Collapsed;
                                            }
                                        }

                                    }

                                }
                                break;
                            }
                        }
                    }

                    double ta = textTransportcosts.Text.ToDouble() + textAccommodation.Text.ToDouble() + textOthercosts.Text.ToDouble();
                    totall = totall + ta;

                    Fees = txtTFSubsidies.Text.ToDouble() + txtMealSubsidies.Text.ToDouble();
                    totall += Fees;
                }
                if (IsPassEd)
                {
                    this.txtAccommodation.Text = "住宿费超标";
                }
                else
                {
                    this.txtAccommodation.Text = string.Empty;
                    this.txtAccommodation.Visibility = Visibility.Collapsed;
                }
                txtSubTotal.Text = totall.ToString();//差旅费总结
                txtChargeApplyTotal.Text = totall.ToString(); //费用报销总计包括其他费用，如业务招待费               
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }


        #endregion


        #region 开始时间控件事件
        private void StartTime_OnValueChanged(object sender, EventArgs e)
        {
            object obj = DaGrEdit.SelectedItem as T_OA_BUSINESSTRIPDETAIL;

            if (obj == null)
            {
                return;
            }

            DateTimePicker dpStartTime = DaGrEdit.Columns[0].GetCellContent(obj).FindName("StartTime") as DateTimePicker;
            DateTimePicker dpEndTime = DaGrEdit.Columns[2].GetCellContent(obj).FindName("EndTime") as DateTimePicker;
            TextBox myDaysTime = DaGrEdit.Columns[4].GetCellContent(obj).FindName("txtTOTALDAYS") as TextBox;

            if (dpStartTime == null || dpEndTime == null || myDaysTime == null)
            {
                return;
            }

            //如果出发时间与到达时间相等,视为当天往返，出差时间算一天
            if (dpStartTime.Value.Value.Date == dpEndTime.Value.Value.Date && travelsolutions.ANDFROMTHATDAY == "1")
            {
                myDaysTime.Text = "1天";
            }
            else
            {
                double TotalDays = 0;//出差天数
                int TotalHours = 0;//出差小时
                TimeSpan tsStart = new TimeSpan(dpStartTime.Value.Value.Ticks);
                TimeSpan tsEnd = new TimeSpan(dpEndTime.Value.Value.Ticks);
                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

                TotalDays = ts.Days;
                TotalHours = ts.Hours;

                int customhalfday = travelsolutions.CUSTOMHALFDAY.ToInt32();

                if (TotalHours >= customhalfday)//如果出差时间大于等于方案设置的时间，按方案标准时间计算
                {
                    TotalDays += 1;
                }
                else
                {
                    if (TotalHours > 0)
                        TotalDays += 0.5;
                }
                myDaysTime.Text = TotalDays.ToString() + "天";
            }
        }
        #endregion

        #region 结束时间控件事件
        private void EndTime_OnValueChanged(object sender, EventArgs e)
        {
            object obj = DaGrEdit.SelectedItem as T_OA_BUSINESSTRIPDETAIL;

            if (obj == null)
            {
                return;
            }

            DateTimePicker dpStartTime = DaGrEdit.Columns[0].GetCellContent(obj).FindName("StartTime") as DateTimePicker;
            DateTimePicker dpEndTime = DaGrEdit.Columns[2].GetCellContent(obj).FindName("EndTime") as DateTimePicker;
            TextBox myDaysTime = DaGrEdit.Columns[4].GetCellContent(obj).FindName("txtTOTALDAYS") as TextBox;

            if (dpStartTime == null || dpEndTime == null || myDaysTime == null)
            {
                return;
            }

            //如果出发时间与到达时间相等,视为当天往返，出差时间算一天
            if (dpStartTime.Value.Value.Date == dpEndTime.Value.Value.Date && travelsolutions.ANDFROMTHATDAY == "1")
            {
                myDaysTime.Text = "1天";
            }
            else
            {
                double TotalDays = 0;//出差天数
                int TotalHours = 0;//出差小时
                TimeSpan tsStart = new TimeSpan(dpStartTime.Value.Value.Ticks);
                TimeSpan tsEnd = new TimeSpan(dpEndTime.Value.Value.Ticks);
                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

                TotalDays = ts.Days;
                TotalHours = ts.Hours;

                int customhalfday = travelsolutions.CUSTOMHALFDAY.ToInt32();

                if (TotalHours >= customhalfday)//如果出差时间大于等于方案设置的时间，按方案标准时间计算
                {
                    TotalDays += 1;
                }
                else
                {
                    if (TotalHours > 0)
                        TotalDays += 0.5;
                }
                myDaysTime.Text = TotalDays.ToString() + "天";
            }
        }
        #endregion

        #region 外出开会控件事件
        private void myChkBoxMeet_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chk = sender as CheckBox;
                if (chk.IsChecked.Value)
                {
                    T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                    if (btlist != null)
                    {
                        var ents = from ent in TravelDetailList_Golbal
                                   where ent.REIMBURSEMENTDETAILID == btlist.REIMBURSEMENTDETAILID
                                   select ent;
                        if (ents.Count() > 0)
                        {
                            int k = TravelDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                            TravelDetailList_Golbal[k].GOOUTTOMEET = "1";
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("您已勾选内部会议或培训，无各项补贴！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void myChkBoxMeet_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TravelDetailList_Golbal
                               where ent.REIMBURSEMENTDETAILID == btlist.REIMBURSEMENTDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TravelDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TravelDetailList_Golbal[k].GOOUTTOMEET = "0";
                    }
                }
            }
        }
        #endregion

        #region 公司派车控件事件
        private void myChkBoxCar_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chk = sender as CheckBox;
                if (chk.IsChecked.Value)
                {
                    T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                    if (btlist != null)
                    {
                        var ents = from ent in TravelDetailList_Golbal
                                   where ent.REIMBURSEMENTDETAILID == btlist.REIMBURSEMENTDETAILID
                                   select ent;
                        if (ents.Count() > 0)
                        {
                            int k = TravelDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                            TravelDetailList_Golbal[k].COMPANYCAR = "1";
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("您已勾选公司派车，无交通补贴！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void myChkBoxCar_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TravelDetailList_Golbal
                               where ent.REIMBURSEMENTDETAILID == btlist.REIMBURSEMENTDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TravelDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TravelDetailList_Golbal[k].COMPANYCAR = "0";
                    }
                }
            }
        }
        #endregion

        #region 出发城市lookup选择
        private void txtDEPARTURECITY_SelectClick(object sender, EventArgs e)
        {
            SearchCity txt = (SearchCity)sender;
            AreaSortCity SelectCity = new AreaSortCity();

            SelectCity.SelectedClicked += (obj, ea) =>
            {
                txt.TxtSelectedCity.Text = SelectCity.Result.Keys.FirstOrDefault();
                if (DaGrEdit.SelectedItem != null)
                {
                    if (DaGrEdit.Columns[1].GetCellContent(DaGrEdit.SelectedItem) != null)
                    {
                        //T_OA_BUSINESSTRIPDETAIL list = DaGrEdit.SelectedItem as T_OA_BUSINESSTRIPDETAIL;
                        T_OA_REIMBURSEMENTDETAIL list = DaGrEdit.SelectedItem as T_OA_REIMBURSEMENTDETAIL;
                        SearchCity myCitys = DaGrEdit.Columns[1].GetCellContent(DaGrEdit.SelectedItem).FindName("txtDEPARTURECITY") as SearchCity;//出发城市
                        SearchCity mystartCity = DaGrEdit.Columns[3].GetCellContent(DaGrEdit.SelectedItem).FindName("txtTARGETCITIES") as SearchCity;//目标城市
                        int k = citysStartList_Golbal.IndexOf(list.DEPCITY);

                        if (k > -1)
                        {
                            citysStartList_Golbal[k] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                            list.DEPCITY = citysStartList_Golbal[k];
                        }
                        else
                        {
                            citysStartList_Golbal.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                        }
                        if (citysStartList_Golbal.Count > 1)
                        {
                            if (myCitys.TxtSelectedCity.Text.ToString().Trim() == mystartCity.TxtSelectedCity.Text.ToString().Trim())
                            {
                                myCitys.TxtSelectedCity.Text = string.Empty;
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                        }
                    }
                }
                if (citysStartList_Golbal.Last().Split(',').Count() > 2)
                {
                    txt.TxtSelectedCity.Text = string.Empty;
                    citysStartList_Golbal.RemoveAt(citysStartList_Golbal.Count - 1);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANONLYCHOOSEONE", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            };
            var windows = SMT.SAAS.Controls.Toolkit.Windows.ProgramManager.ShowProgram(Utility.GetResourceStr("CITY"), "", "123", SelectCity, false, false, null);
            if (SelectCity is AreaSortCity)
            {
                (SelectCity as AreaSortCity).Close += (o, args) =>
                {
                    windows.Close();
                };
            }
            SelectCity.GetSelectedCities.Visibility = Visibility.Collapsed;//隐藏已选中城市的Border
        }
        #endregion

        #region 目标城市LookUp选择事件
        private void txtTARGETCITIES_SelectClick(object sender, EventArgs e)
        {
            SearchCity txt = (SearchCity)sender;
            AreaSortCity SelectCity = new AreaSortCity();

            SelectCity.SelectedClicked += (obj, ea) =>
            {
                txt.TxtSelectedCity.Text = SelectCity.Result.Keys.FirstOrDefault();
                if (DaGrEdit.SelectedItem != null)
                {
                    int SelectIndex = DaGrEdit.SelectedIndex;//选择的行数，选择的行数也就是目的城市的位置
                    if (DaGrEdit.Columns[3].GetCellContent(DaGrEdit.SelectedItem) != null)
                    {
                        T_OA_REIMBURSEMENTDETAIL list = DaGrEdit.SelectedItem as T_OA_REIMBURSEMENTDETAIL;

                        //T_OA_BUSINESSTRIPDETAIL list = DaGrEdit.SelectedItem as T_OA_BUSINESSTRIPDETAIL;
                        SearchCity myCitys = DaGrEdit.Columns[3].GetCellContent(DaGrEdit.SelectedItem).FindName("txtTARGETCITIES") as SearchCity;
                        SearchCity mystartCity = DaGrEdit.Columns[1].GetCellContent(DaGrEdit.SelectedItem).FindName("txtDEPARTURECITY") as SearchCity;
                        if (citysStartList_Golbal.Count() > 0)
                        {
                            mystartCity.Tag = citysStartList_Golbal[SelectIndex];//将旧的传递起来
                        }
                        if (string.IsNullOrEmpty(mystartCity.TxtSelectedCity.Text))
                        {
                            txt.TxtSelectedCity.Text = string.Empty;
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("请先选择出发城市"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                        if (citysEndList_Golbal.Count > 1)
                        {
                            if (mystartCity.TxtSelectedCity.Text.ToString().Trim() == myCitys.TxtSelectedCity.Text.ToString().Trim())
                            {
                                myCitys.TxtSelectedCity.Text = string.Empty;
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                        }
                        if (citysEndList_Golbal.Count >= (SelectIndex + 1))
                        {
                            citysEndList_Golbal[SelectIndex] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                        }
                        else
                        {
                            citysEndList_Golbal.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                        }
                        if (citysStartList_Golbal.Count == (SelectIndex + 1))
                        {
                            citysStartList_Golbal.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                            SetNextDepartureCity(SelectIndex);
                        }
                        else
                        {
                            citysStartList_Golbal[SelectIndex] = mystartCity.Tag.ToString();
                            citysStartList_Golbal[SelectIndex + 1] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();//出发城市中下一条记录
                            SetNextDepartureCity(SelectIndex);
                        }
                        //将选择的城市值赋给对应的集合
                        if (TravelDetailList_Golbal.Count() > 0)
                        {
                            var ents = from ent in TravelDetailList_Golbal
                                       where ent.REIMBURSEMENTDETAILID == list.REIMBURSEMENTDETAILID
                                       select ent;
                            TravelDetailList_Golbal.ForEach(item =>
                            {
                                if (item.REIMBURSEMENTDETAILID == list.REIMBURSEMENTDETAILID)
                                {
                                    item.DESTCITY = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                                }
                            });

                        }
                    }
                }
                if (citysEndList_Golbal.Last().Split(',').Count() > 2)
                {
                    txt.TxtSelectedCity.Text = string.Empty;
                    citysEndList_Golbal.RemoveAt(citysEndList_Golbal.Count - 1);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANONLYCHOOSEONE", "ARRIVALCITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            };
            var windows = SMT.SAAS.Controls.Toolkit.Windows.ProgramManager.ShowProgram(Utility.GetResourceStr("CITY"), "", "123", SelectCity, false, false, null);
            if (SelectCity is AreaSortCity)
            {
                (SelectCity as AreaSortCity).Close += (o, args) =>
                {
                    windows.Close();
                };
            }
            SelectCity.GetSelectedCities.Visibility = Visibility.Collapsed;//隐藏已选中城市的Border
        }
        /// <summary>
        /// 设置选中的下一个出发城市的值
        /// </summary>
        /// <param name="SelectIndex"></param>
        private void SetNextDepartureCity(int SelectIndex)
        {
            int EachCount = 0;
            foreach (Object obje in DaGrEdit.ItemsSource)//将下一个出发城市的值修改
            {
                EachCount++;
                if (DaGrEdit.Columns[1].GetCellContent(obje) != null)
                {
                    SearchCity mystarteachCity = DaGrEdit.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                    if ((SelectIndex + 2) == EachCount)
                    {
                        mystarteachCity.TxtSelectedCity.Text = GetCityName(citysStartList_Golbal[SelectIndex + 1]);
                    }
                }
            }
        }
        #endregion
        
        #region 交通工具控件事件
        private void txtTRANSPORTCOSTS_LostFocus(object sender, RoutedEventArgs e)
        {
            CountMoney();
        }
        #endregion

        #region 交通工具类型、级别控件事件
      
        private void ComVechileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TravelDictionaryComboBox vechiletype = sender as TravelDictionaryComboBox;
                if (vechiletype.SelectedIndex >= 0)
                {
                    var thd = takethestandardtransport.FirstOrDefault();
                    thd = this.GetVehicleTypeValue("");
                    T_SYS_DICTIONARY VechileTypeObj = vechiletype.SelectedItem as T_SYS_DICTIONARY;
                    if (DaGrEdit.SelectedItem != null)
                    {
                        if (DaGrEdit.Columns[7].GetCellContent(DaGrEdit.SelectedItem) != null)
                        {
                            TravelDictionaryComboBox ComLevel = DaGrEdit.Columns[7].GetCellContent(DaGrEdit.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;

                            var ListObj = from ent in ListVechileLevel
                                          where ent.T_SYS_DICTIONARY2.DICTIONARYID == VechileTypeObj.DICTIONARYID
                                          orderby ent.DICTIONARYVALUE descending
                                          select ent;
                            if (ListObj.Count() > 0)
                            {
                                ComLevel.ItemsSource = ListObj;
                                ComLevel.SelectedIndex = 0;
                            }
                        }
                    }
                    if (employeepost != null)
                    {

                        if (DaGrEdit.SelectedItem != null)
                        {
                            if (DaGrEdit.Columns[7].GetCellContent(DaGrEdit.SelectedItem) != null)
                            {
                                TravelDictionaryComboBox ComLevel = DaGrEdit.Columns[7].GetCellContent(DaGrEdit.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                                TravelDictionaryComboBox ComType = DaGrEdit.Columns[6].GetCellContent(DaGrEdit.SelectedItem).FindName("ComVechileType") as TravelDictionaryComboBox;
                                T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                                T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                                level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                                type = ComType.SelectedItem as T_SYS_DICTIONARY;

                                if (thd != null)
                                {
                                    if (type != null)
                                    {
                                        if (thd.TYPEOFTRAVELTOOLS.ToInt32() <= type.DICTIONARYVALUE)
                                        {
                                            if (tempcomTypeBorderBrush != null)
                                            {
                                                ComType.BorderBrush = tempcomTypeBorderBrush;
                                            }
                                            if (tempcomTypeForeBrush != null)
                                            {
                                                ComType.Foreground = tempcomTypeForeBrush;
                                            }
                                            if (tempcomLevelForeBrush != null)
                                            {
                                                ComLevel.Foreground = tempcomLevelForeBrush;
                                            }
                                            if (tempcomLevelBorderBrush != null)
                                            {
                                                ComLevel.BorderBrush = tempcomLevelBorderBrush;
                                            }
                                            this.txtTips.Visibility = Visibility.Collapsed;
                                        }
                                        else
                                        {
                                            if (level != null)
                                            {
                                                if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE && thd.TAKETHETOOLLEVEL.ToInt32() > level.DICTIONARYVALUE)
                                                {
                                                    ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                    ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                    this.txtTips.Visibility = Visibility.Visible;
                                                    this.txtTips.Text = "交通工具超标";
                                                    return;
                                                }
                                                if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE)
                                                {
                                                    ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                    ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                    this.txtTips.Visibility = Visibility.Visible;
                                                    this.txtTips.Text = "交通工具超标";
                                                    return;
                                                }
                                            }
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() <= type.DICTIONARYVALUE && thd.TAKETHETOOLLEVEL.ToInt32() <= level.DICTIONARYVALUE)
                                            {
                                                this.txtTips.Visibility = Visibility.Collapsed;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void ComVechileTypeLeve_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TravelDictionaryComboBox vechiletype = sender as TravelDictionaryComboBox;
                if (vechiletype.SelectedIndex >= 0)
                {
                    if (employeepost != null)
                    {

                        var thd = takethestandardtransport.FirstOrDefault();

                        T_SYS_DICTIONARY VechileTypeObj = vechiletype.SelectedItem as T_SYS_DICTIONARY;
                        if (DaGrEdit.SelectedItem != null)
                        {
                            if (DaGrEdit.Columns[7].GetCellContent(DaGrEdit.SelectedItem) != null)
                            {
                                TravelDictionaryComboBox ComLevel = DaGrEdit.Columns[7].GetCellContent(DaGrEdit.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                                TravelDictionaryComboBox ComType = DaGrEdit.Columns[6].GetCellContent(DaGrEdit.SelectedItem).FindName("ComVechileType") as TravelDictionaryComboBox;
                                T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                                T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                                level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                                type = ComType.SelectedItem as T_SYS_DICTIONARY;
                                if (type != null)
                                {
                                    thd = this.GetVehicleTypeValue(type.DICTIONARYVALUE.ToString());
                                    if (thd == null)
                                    {
                                        ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                        ComType.Foreground = new SolidColorBrush(Colors.Red);
                                        return;
                                    }
                                    if (level != null)
                                    {
                                        if (thd.TAKETHETOOLLEVEL.ToInt32() < level.DICTIONARYVALUE)
                                        {
                                            if (tempcomLevelForeBrush != null)
                                            {
                                                ComLevel.Foreground = tempcomLevelForeBrush;
                                            }
                                            if (tempcomLevelBorderBrush != null)
                                            {
                                                ComLevel.BorderBrush = tempcomLevelBorderBrush;
                                            }
                                            if (tempcomTypeBorderBrush != null)
                                            {
                                                ComType.BorderBrush = tempcomTypeBorderBrush;
                                            }
                                            if (tempcomTypeForeBrush != null)
                                            {
                                                ComType.Foreground = tempcomTypeForeBrush;
                                            }
                                        }
                                        else
                                        {
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE)
                                            {
                                                ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                this.txtTips.Visibility = Visibility.Visible;
                                                this.txtTips.Text = "交通工具超标";
                                                return;
                                            }
                                            else if (thd.TAKETHETOOLLEVEL.ToInt32() > level.DICTIONARYVALUE)
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
                                                this.txtTips.Visibility = Visibility.Collapsed;
                                            }
                                        }
                                    }
                                }
                            }
                        }
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

        #region 新建出差明细按钮事件
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsFinishedCitys())
            {
                return;
            }

            BtnNewButton = true;
            int i = 0;
            T_OA_REIMBURSEMENTDETAIL buip = new T_OA_REIMBURSEMENTDETAIL();
            buip.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();

            if (formType != FormTypes.New)
            {
                if (TravelDetailList_Golbal.Count() > 0)
                {
                    //foreach (T_OA_REIMBURSEMENTDETAIL tailList in TrList)
                    //{
                    //    buip.STARTDATE = tailList.ENDDATE;
                    //    if (cityscode.Count() == 0)
                    //    {
                    //        return;
                    //    }
                    //    buip.DEPCITY = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityscode[i].Replace(",", ""));

                    //    i++;
                    //}
                    //将原有记录的最后一条记录的目的城市作为出发城市。
                    //buip.DEPCITY = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(TrList.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>().DESTCITY);
                    if (TravelDetailList_Golbal.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>() != null)
                    {
                        buip.DEPCITY = TravelDetailList_Golbal.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>().DESTCITY;
                        buip.STARTDATE = TravelDetailList_Golbal.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>().ENDDATE;

                    }
                }
                buip.ENDDATE = DateTime.Now;
                TravelDetailList_Golbal.Add(buip);
                DaGrEdit.ItemsSource = TravelDetailList_Golbal;

                foreach (Object obje in DaGrEdit.ItemsSource)
                {
                    if (DaGrEdit.Columns[1].GetCellContent(obje) != null)
                    {
                        SearchCity myCity = DaGrEdit.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;

                        if (myCity != null)
                        {
                            myCity.IsEnabled = false;
                            ((DataGridCell)((StackPanel)myCity.Parent).Parent).IsEnabled = false;
                        }
                    }
                }
            }
        }

        #region 检查是否选择了目标城市否则不给添加
        private bool CheckIsFinishedCitys()
        {
            bool IsResult = true;
            string StrStartDt = "";
            string EndDt = "";
            string StrStartTime = "";
            string StrEndTime = "";
            foreach (object obje in DaGrEdit.ItemsSource)
            {
                TrDetail = new T_OA_REIMBURSEMENTDETAIL();
                SearchCity myCitys = DaGrEdit.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;

                TrDetail.T_OA_TRAVELREIMBURSEMENT = TravelReimbursement_Golbal;
                DateTimePicker StartDate = DaGrEdit.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker EndDate = DaGrEdit.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;

                TravelDictionaryComboBox ToolType = ((TravelDictionaryComboBox)((StackPanel)DaGrEdit.Columns[6].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                TravelDictionaryComboBox ToolLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrEdit.Columns[7].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;

                StrStartDt = StartDate.Value.Value.ToString("d");//开始日期
                EndDt = EndDate.Value.Value.ToString("d");//结束日期
                StrStartTime = StartDate.Value.Value.ToString("HH:mm");//开始时间
                StrEndTime = EndDate.Value.Value.ToString("HH:mm");//结束时间

                if (string.IsNullOrEmpty(StrStartDt) || string.IsNullOrEmpty(StrStartTime))//开始日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "开始时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }

                if (string.IsNullOrEmpty(EndDt) || string.IsNullOrEmpty(StrEndTime))//结束日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "结束时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }
                DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
                DateTime DtEnd = System.Convert.ToDateTime(EndDt + " " + StrEndTime);
                if (DtStart >= DtEnd)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "开始时间不能大于等于结束时间", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }


                if (ToolType.SelectedIndex <= 0)//交通工具类型
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "TYPEOFTRAVELTOOLS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }
            }
            return IsResult;
        }
        #endregion
        #endregion

        private void myChkBoxMeet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrEdit.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                var temp = DaGrEdit.SelectedItem as T_OA_REIMBURSEMENTDETAIL;
                CheckBox chbMeet = DaGrEdit.Columns[14].GetCellContent(temp).FindName("myChkBoxMeet") as CheckBox;
                if (chbMeet.IsChecked == true)
                {
                    temp.GOOUTTOMEET = "1";
                }
                else
                {
                    temp.GOOUTTOMEET = "0";
                }

                TravelAllowance(this.DaGrEdit);
            }
            catch
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void myChkBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrEdit.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                var temp = DaGrEdit.SelectedItem as T_OA_REIMBURSEMENTDETAIL;
                CheckBox chbMeet = DaGrEdit.Columns[13].GetCellContent(temp).FindName("myChkBox") as CheckBox;
                if (chbMeet.IsChecked == true)
                {
                    temp.PRIVATEAFFAIR = "1";
                }
                else
                {
                    temp.PRIVATEAFFAIR = "0";
                }

                TravelAllowance(DaGrEdit);
            }
            catch
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

    }
}
