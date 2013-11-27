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
        #region 出差时间计算
        /// <summary>
        /// 计算出差天数
        /// </summary>
        public void TravelTime()
        {
            if (TravelDetailList_Golbal == null || DaGrEdit.ItemsSource == null)
            {
                return;
            }
            #region 存在多条的处理
            TextBox myDaysTime = new TextBox();
            bool OneDayTrave = false;
            for (int i = 0; i < TravelDetailList_Golbal.Count; i++)
            {
                GetTraveDayTextBox(myDaysTime, i).Text = string.Empty;
                OneDayTrave = false;
                //记录本条记录以便处理
                DateTime FirstStartTime = Convert.ToDateTime(TravelDetailList_Golbal[i].STARTDATE);
                DateTime FirstEndTime = Convert.ToDateTime(TravelDetailList_Golbal[i].ENDDATE);
                string FirstTraveFrom = TravelDetailList_Golbal[i].DEPCITY;
                string FirstTraveTo = TravelDetailList_Golbal[i].DESTCITY;
                //遍历剩余的记录
                for (int j = i + 1; j < TravelDetailList_Golbal.Count; j++)
                {
                    DateTime NextStartTime = Convert.ToDateTime(TravelDetailList_Golbal[j].STARTDATE);
                    DateTime NextEndTime = Convert.ToDateTime(TravelDetailList_Golbal[j].ENDDATE);
                    string NextTraveFrom = TravelDetailList_Golbal[j].DEPCITY;
                    string NextTraveTo = TravelDetailList_Golbal[j].DESTCITY;
                    GetTraveDayTextBox(myDaysTime, j).Text = string.Empty;
                    if (NextEndTime.Date == FirstStartTime.Date)
                    {
                        if (NextTraveTo == FirstTraveFrom && (TravelDetailList_Golbal.Count == 2 || TravelDetailList_Golbal.Count == 1))
                        {
                            myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                            myDaysTime.Text = "1";
                            //i = j - 1;
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
                switch (TravelDetailList_Golbal.Count())
                {
                    case 1:
                        TotalDays = CaculateTravDays(FirstStartTime, FirstEndTime);
                        myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    case 2:
                        if (i == 1) break;
                        DateTime NextEndTime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].ENDDATE);
                        TotalDays = CaculateTravDays(FirstStartTime, NextEndTime);
                        myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    default:
                        if (i == TravelDetailList_Golbal.Count() - 1) break;//最后一条记录不处理
                        if (i == TravelDetailList_Golbal.Count() - 2)//倒数第二条记录=最后一条结束时间-上一条开始时间
                        {
                            DateTime NextENDDATETime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].ENDDATE);
                            TotalDays = CaculateTravDays(FirstStartTime, NextENDDATETime);
                            myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                            myDaysTime.Text = TotalDays.ToString();
                            break;
                        }
                        //否则出差时间=下一条开始时间-上一条开始时间
                        DateTime NextStartTime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].STARTDATE);
                        TotalDays = CaculateTravDays(FirstStartTime, NextStartTime);
                        myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                }
            }
            #endregion
        }

        private TextBox GetTraveDayTextBox(TextBox myDaysTime, int i)
        {
            if (DaGrEdit.Columns[4].GetCellContent(TravelDetailList_Golbal[i]) != null)
            {
                myDaysTime = DaGrEdit.Columns[4].GetCellContent(TravelDetailList_Golbal[i]).FindName("txtTOTALDAYS") as TextBox;
            }
            return myDaysTime;
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
            int customhalfday = travelsolutions.CUSTOMHALFDAY.ToInt32();
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

        #region 住宿时间计算

        public void TravelTimeCalculation()
        {
            if (TravelDetailList_Golbal == null || DaGrEdit.ItemsSource == null)
            {
                return;
            }
            #region 存在多条的处理
            TextBox myDaysTime = new TextBox();
            bool OneDayTrave = false;
            for (int i = 0; i < TravelDetailList_Golbal.Count; i++)
            {
                GetTraveTimeCalculationTextBox(myDaysTime, i).Text = string.Empty;
                OneDayTrave = false;
                //记录本条记录以便处理
                DateTime FirstStartTime = Convert.ToDateTime(TravelDetailList_Golbal[i].STARTDATE);
                DateTime FirstEndTime = Convert.ToDateTime(TravelDetailList_Golbal[i].ENDDATE);
                string FirstTraveFrom = TravelDetailList_Golbal[i].DEPCITY;
                string FirstTraveTo = TravelDetailList_Golbal[i].DESTCITY;
                //遍历剩余的记录
                for (int j = i + 1; j < TravelDetailList_Golbal.Count; j++)
                {
                    DateTime NextStartTime = Convert.ToDateTime(TravelDetailList_Golbal[j].STARTDATE);
                    DateTime NextEndTime = Convert.ToDateTime(TravelDetailList_Golbal[j].ENDDATE);
                    string NextTraveFrom = TravelDetailList_Golbal[j].DEPCITY;
                    string NextTraveTo = TravelDetailList_Golbal[j].DESTCITY;
                    GetTraveTimeCalculationTextBox(myDaysTime, j).Text = string.Empty;
                    if (NextEndTime.Date == FirstStartTime.Date)
                    {
                        if (NextTraveTo == FirstTraveFrom)
                        {
                            myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
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
                switch (TravelDetailList_Golbal.Count())
                {
                    case 1:
                        TotalDays = CaculateTravCalculationDays(FirstStartTime, FirstEndTime);
                        myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    case 2:
                        if (i == 1) break;
                        DateTime NextEndTime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].ENDDATE);
                        TotalDays = CaculateTravCalculationDays(FirstStartTime, NextEndTime);
                        myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    default:
                        if (i == TravelDetailList_Golbal.Count() - 1) break;//最后一条记录不处理
                        if (i == TravelDetailList_Golbal.Count() - 2)//倒数第二条记录=最后一条结束时间-上一条开始时间
                        {
                            DateTime NextENDDATETime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].ENDDATE);
                            TotalDays = CaculateTravCalculationDays(FirstStartTime, NextENDDATETime);
                            myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                            myDaysTime.Text = TotalDays.ToString();
                            break;
                        }
                        //否则出差时间=下一条开始时间-上一条开始时间
                        DateTime NextStartTime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].STARTDATE);
                        TotalDays = CaculateTravCalculationDays(FirstStartTime, NextStartTime);
                        myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                }
            }
            #endregion
        }

        private TextBox GetTraveTimeCalculationTextBox(TextBox myDaysTime, int i)
        {
            if (DaGrEdit.Columns[5].GetCellContent(TravelDetailList_Golbal[i]) != null)
            {
                myDaysTime = DaGrEdit.Columns[5].GetCellContent(TravelDetailList_Golbal[i]).FindName("txtTHENUMBEROFNIGHTS") as TextBox;
            }
            return myDaysTime;
        }

        /// <summary>
        /// 计算出差时长结算-开始时间NextStartTime-FirstStartTime
        /// </summary>
        /// <param name="FirstStartTime">开始时间</param>
        /// <param name="NextStartTime">结束时间</param>
        /// <returns></returns>
        private decimal CaculateTravCalculationDays(DateTime FirstStartTime, DateTime NextStartTime)
        {
            //计算出差时间（天数）
            TimeSpan TraveDays = NextStartTime.Subtract(FirstStartTime.Date);
            decimal TotalDays = 0;//出差天数
            decimal TotalHours = 0;//出差小时
            TotalDays = TraveDays.Days;
            TotalHours = TraveDays.Hours;

            return TotalDays;
        }
        #endregion

        #region 计算出差补贴补贴
        /// <summary>
        /// 计算补贴
        /// </summary>
        /// <param name="FromReadOnlyDataGrid">是否显示只读的查看Grid</param>
        private void TravelAllowance(bool FromReadOnlyDataGrid)
        {
            try
            {
                DataGrid dataGrid = new DataGrid();
                if (FromReadOnlyDataGrid)//查看模式下
                {
                    dataGrid = this.DaGrReadOnly;
                }
                else
                {
                    dataGrid = this.DaGrEdit;
                }

                if (dataGrid.ItemsSource != null)
                {
                    T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();

                    double total = 0;
                    int i = 0;
                    foreach (var detail in TravelDetailList_Golbal)
                    {

                        double toodays = 0;
                        List<string> list = new List<string> { detail.BUSINESSDAYS };

                        if (detail.BUSINESSDAYS != null && !string.IsNullOrEmpty(detail.BUSINESSDAYS))
                        {
                            double totalHours = System.Convert.ToDouble(list[0]);
                            toodays = totalHours;
                        }
                        double totolDay = toodays;//计算本次出差的总天数

                        string cityValue = detail.DESTCITY.Replace(",", "");//目标城市值
                        entareaallowance = this.GetAllowanceByCityValue(cityValue);
                        if (travelsolutions != null && employeepost != null)
                        {

                            if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                            {
                                //textStandards.Text = textStandards.Text +"报销人的岗位级别小于等于I级，无交通补贴及住宿补贴";
                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                detail.MEALSUBSIDIES = 0;
                            }
                            else
                            {
                                #region 根据本次出差的总天数,根据天数获取相应的补贴
                                if (totolDay <= travelsolutions.MINIMUMINTERVALDAYS.ToInt32())//本次出差总时间小于等于设定天数的报销标准
                                {
                                    if (entareaallowance != null)
                                    {
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES.ToDouble() * toodays);
                                                }
                                                else
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            detail.TRANSPORTATIONSUBSIDIES = 0;
                                        }
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                detail.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                detail.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    detail.MEALSUBSIDIES = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES.ToDouble() * toodays);
                                                }
                                                else
                                                {
                                                    detail.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            //txtASubsidies.IsReadOnly = true;
                                        }
                                    }
                                }

                                #endregion

                                #region 如果出差天数大于设定的最大天数,按驻外标准获取补贴

                                else if (totolDay > travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
                                {
                                    if (entareaallowance != null)
                                    {
                                        double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
                                        double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
                                        //区间补贴标准 区间报销比例（50）/100
                                        double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                        double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                        if (entareaallowance != null)
                                        {
                                            if (detail.BUSINESSDAYS != null)
                                            {
                                                if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = 0;
                                                    detail.TRANSPORTCOSTS = 0;
                                                }
                                                else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                                else
                                                {
                                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                    {
                                                        //可全额报销天数*每天的补贴
                                                        double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
                                                        //区间可以报销天数*报销比例50%
                                                        double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
                                                        //除以2是因为驻外标准不分餐费和交通补贴，2者合2为一，否则会多加 （餐补及交通补贴都按驻外标准计算）
                                                        double lastmoney = (totolDay - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
                                                        detail.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney + lastmoney);
                                                    }
                                                    else
                                                    {
                                                        detail.TRANSPORTATIONSUBSIDIES = 0;
                                                    }
                                                }
                                            }
                                            else//如果天数为null的禁用住宿费控件
                                            {
                                            }
                                            if (detail.BUSINESSDAYS != null)
                                            {
                                                if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                                {
                                                    detail.MEALSUBSIDIES = 0;
                                                }
                                                else if (detail.GOOUTTOMEET == "1")//如果是开会
                                                {
                                                    detail.MEALSUBSIDIES = 0;
                                                }
                                                else
                                                {
                                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                    {
                                                        double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
                                                        //double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
                                                        double IntMaxDays = travelsolutions.MAXIMUMRANGEDAYS.ToDouble();
                                                        double IntMinDAys = travelsolutions.MINIMUMINTERVALDAYS.ToDouble();
                                                        double middlemoney = (IntMaxDays - IntMinDAys) * mealSubsidies;
                                                        //double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble();
                                                        //驻外标准：交通费和餐费补贴为一起的，所以除以2
                                                        double lastmoney = (totolDay - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
                                                        detail.MEALSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney + lastmoney);
                                                    }
                                                    else
                                                    {
                                                        detail.MEALSUBSIDIES = 0;
                                                    }
                                                }
                                            }
                                            else//如果天数为null的禁用住宿费控件
                                            {
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region 如果出差时间大于设定的最小天数并且小于设定的最大天数的报销标准

                                else if (totolDay >= travelsolutions.MINIMUMINTERVALDAYS.ToInt32() && totolDay <= travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
                                {
                                    if (entareaallowance != null)
                                    {
                                        double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
                                        double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
                                        double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                        double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                                detail.TRANSPORTCOSTS = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
                                                    double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
                                                    detail.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney);
                                                }
                                                else
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            //txtASubsidies.IsReadOnly = true;
                                        }
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                detail.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                detail.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    //最小区间段金额
                                                    double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
                                                    //中间区间段金额
                                                    double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
                                                    detail.MEALSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney);
                                                }
                                                else
                                                {
                                                    detail.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            //txtASubsidies.IsReadOnly = true;
                                        }
                                    }
                                }

                                #endregion
                            }



                        }

                        //交通补贴，
                        if (detail.TRANSPORTATIONSUBSIDIES != null)
                        {
                            try
                            {
                                //交通补贴,在grid没有绑定之前获取不到。
                                TextBox txtTFSubsidies = DaGrEdit.Columns[10].GetCellContent(detail).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;
                                if (txtTFSubsidies != null)
                                {
                                    txtTFSubsidies.Text = detail.TRANSPORTATIONSUBSIDIES.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                Utility.SetLogAndShowLog(ex.ToString());
                            }
                            total += Convert.ToDouble(detail.TRANSPORTATIONSUBSIDIES.Value);
                        }
                        //餐费补贴
                        if (detail.MEALSUBSIDIES != null)
                        {
                            try
                            {
                                //餐费补贴，在grid没有绑定之前获取不到。
                                TextBox txtMealSubsidies = DaGrEdit.Columns[11].GetCellContent(detail).FindName("txtMEALSUBSIDIES") as TextBox;
                                if (txtMealSubsidies != null)
                                {
                                    txtMealSubsidies.Text = detail.MEALSUBSIDIES.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                Utility.SetLogAndShowLog(ex.ToString());
                            }
                            total += Convert.ToDouble(detail.MEALSUBSIDIES.Value);
                        }
                        if (detail.TRANSPORTCOSTS != null)//交通费
                        {
                            total += Convert.ToDouble(detail.TRANSPORTCOSTS.Value);
                        }
                        if (detail.ACCOMMODATION != null)//住宿费
                        {
                            total += Convert.ToDouble(detail.ACCOMMODATION);
                        }
                        if (detail.OTHERCOSTS != null)//其他费用
                        {
                            total += Convert.ToDouble(detail.OTHERCOSTS);
                        }
                        this.txtSubTotal.Text = total.ToString();//总费用
                      
                        //Fees = total;
                        i++;
                    }
                    //总费用加上费用报销费用
                    if (fbCtr.totalMoney > 0)
                    {
                        total = total + Convert.ToDouble(fbCtr.totalMoney);
                    }
                    this.txtChargeApplyTotal.Text = total.ToString();
                }
            }
            catch (Exception ex)
            {
                Utility.SetLogAndShowLog(ex.ToString());
            }
        }
        ///// <summary>
        ///// 计算补贴
        ///// </summary>
        ///// <param name="FromReadOnlyDataGrid">是否显示只读的查看Grid</param>
        //private void TravelAllowance(bool FromReadOnlyDataGrid)
        //{
        //    TextBox txtTFSubsidies = new TextBox();//初始化交通补贴控件
        //    TextBox txtMealSubsidies = new TextBox();//初始化餐费补贴控件
        //    TextBox txtASubsidies = new TextBox();//初始化住宿费控件
        //    TextBox txtTranSportcosts = new TextBox();//初始化交通费控件
        //    TextBox txtOtherCosts = new TextBox();//初始化其他费用控件

        //    DataGrid dataGrid = new DataGrid();
        //    if (FromReadOnlyDataGrid)//查看模式下
        //    {
        //        dataGrid = this.DaGrReadOnly;
        //    }
        //    else
        //    {
        //        dataGrid = this.DaGrEdit;
        //    }

        //    if (dataGrid.ItemsSource != null)
        //    {
        //        T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();

        //        ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = dataGrid.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
        //        double total = 0;
        //        int i = 0;
        //        foreach (var obje in objs)
        //        {
        //            i++;
        //            double toodays = 0;
        //            //if (FromReadOnlyDataGrid)
        //            //{
        //            //    if (i > 0) txtTFSubsidies = dataGrid.Columns[10].GetCellContent(obje).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
        //            //    if (i > 0) txtMealSubsidies = dataGrid.Columns[11].GetCellContent(obje).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
        //            //}
        //            //else
        //            //{
        //            //    if (i >0) txtTFSubsidies = dataGrid.Columns[10].GetCellContent(obje).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
        //            //    if (i >0) txtMealSubsidies = dataGrid.Columns[11].GetCellContent(obje).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
        //            //}

        //            List<string> list = new List<string>
        //                        {
        //                             obje.BUSINESSDAYS
        //                        };

        //            if (obje.BUSINESSDAYS != null && !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //            {
        //                double totalHours = System.Convert.ToDouble(list[0]);
        //                toodays = totalHours;
        //            }
        //            double totolDay = toodays;//计算本次出差的总天数

        //            string cityValue = citysEndList_Golbal[i - 1].Replace(",", "");//目标城市值
        //            entareaallowance = this.GetAllowanceByCityValue(cityValue);

        //            #region 根据本次出差的总天数,根据天数获取相应的补贴
        //            if (travelsolutions != null && employeepost != null)
        //            {

        //                txtTFSubsidies = GetTFSubsidiesTextBox(txtTFSubsidies, i, FromReadOnlyDataGrid);//交通补贴控件赋值
        //                txtTranSportcosts = GetTranSportcostsTextBox(txtTranSportcosts, i,FromReadOnlyDataGrid);//交通费控件赋值
        //                txtASubsidies = GetASubsidiesTextBox(txtASubsidies, i, FromReadOnlyDataGrid);//住宿费控件赋值
        //                txtOtherCosts = GetOtherCostsTextBox(txtOtherCosts, i, FromReadOnlyDataGrid);//其他费用控件赋值
        //                txtMealSubsidies = GetMealSubsidiesTextBox(txtMealSubsidies, i, FromReadOnlyDataGrid);//餐费补贴控件赋值

        //                if (totolDay <= travelsolutions.MINIMUMINTERVALDAYS.ToInt32())//本次出差总时间小于等于设定天数的报销标准
        //                {
        //                    if (entareaallowance != null)
        //                    {
        //                        if (txtTFSubsidies != null)//交通补贴
        //                        {
        //                            if (obje.BUSINESSDAYS != null)
        //                            {
        //                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                {
        //                                    txtTFSubsidies.Text = "0";
        //                                    txtTFSubsidies.IsReadOnly = true;
        //                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                }
        //                                else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
        //                                {
        //                                    txtTFSubsidies.Text = "0";
        //                                }
        //                                else
        //                                {
        //                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                    {
        //                                        txtTFSubsidies.Text = (entareaallowance.TRANSPORTATIONSUBSIDIES.ToDouble() * toodays).ToString();
        //                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                        if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
        //                                        {
        //                                            ComfirmWindow com = new ComfirmWindow();
        //                                            com.OnSelectionBoxClosed += (obj, result) =>
        //                                            {
        //                                                txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                txtOtherCosts.IsReadOnly = true;//其他费用
        //                                            };
        //                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                            {
        //                                                if (formType == FormTypes.Audit) return;
        //                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        txtTFSubsidies.Text = "0";
        //                                        txtTFSubsidies.IsReadOnly = false;
        //                                    }
        //                                }
        //                            }
        //                            else//如果天数为null的禁用住宿费控件
        //                            {
        //                                txtASubsidies.IsReadOnly = true;
        //                            }
        //                        }
        //                        if (txtMealSubsidies != null)//餐费补贴
        //                        {
        //                            if (obje.BUSINESSDAYS != null)
        //                            {
        //                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                {
        //                                    txtMealSubsidies.Text = "0";
        //                                    txtMealSubsidies.IsReadOnly = true;
        //                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                }
        //                                else if (obje.GOOUTTOMEET == "1")//如果是开会
        //                                {
        //                                    txtMealSubsidies.Text = "0";
        //                                }
        //                                else
        //                                {
        //                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                    {
        //                                        txtMealSubsidies.Text = (entareaallowance.MEALSUBSIDIES.ToDouble() * toodays).ToString();
        //                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                        if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
        //                                        {
        //                                            ComfirmWindow com = new ComfirmWindow();
        //                                            com.OnSelectionBoxClosed += (obj, result) =>
        //                                            {
        //                                                txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                txtOtherCosts.IsReadOnly = true;//其他费用
        //                                            };
        //                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                            {
        //                                                if (formType == FormTypes.Audit) return;
        //                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        txtMealSubsidies.Text = "0";
        //                                        txtMealSubsidies.IsReadOnly = false;
        //                                    }
        //                                }
        //                            }
        //                            else//如果天数为null的禁用住宿费控件
        //                            {
        //                                txtASubsidies.IsReadOnly = true;
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                    {
        //                        txtTFSubsidies.Text = "0";
        //                        txtMealSubsidies.Text = "0";
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region 如果出差天数大于设定的最大天数,按驻外标准获取补贴
        //            if (travelsolutions != null && employeepost != null)
        //            {
        //                TextBox txtTranSportcosts = new TextBox();//初始化交通费控件
        //                TextBox txtOtherCosts = new TextBox();//初始化其他费用控件

        //                txtTFSubsidies = GetTFSubsidiesTextBox(txtTFSubsidies, i, FromReadOnlyDataGrid);//交通补贴控件赋值
        //                txtTranSportcosts = GetTranSportcostsTextBox(txtTranSportcosts, i, FromReadOnlyDataGrid);//交通费控件赋值
        //                txtASubsidies = GetASubsidiesTextBox(txtASubsidies, i, FromReadOnlyDataGrid);//住宿费控件赋值
        //                txtOtherCosts = GetOtherCostsTextBox(txtOtherCosts, i, FromReadOnlyDataGrid);//其他费用控件赋值
        //                txtMealSubsidies = GetMealSubsidiesTextBox(txtMealSubsidies, i, FromReadOnlyDataGrid);//餐费补贴控件赋值

        //                if (totolDay > travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
        //                {
        //                    if (entareaallowance != null)
        //                    {
        //                        double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
        //                        double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
        //                        double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
        //                        double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
        //                        if (entareaallowance != null)
        //                        {
        //                            if (txtTFSubsidies != null)//交通补贴
        //                            {
        //                                if (obje.BUSINESSDAYS != null)
        //                                {
        //                                    if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                    {
        //                                        txtTFSubsidies.Text = "0";
        //                                        txtTFSubsidies.IsReadOnly = true;
        //                                        txtTranSportcosts.IsReadOnly = true;//交通费
        //                                        txtASubsidies.IsReadOnly = true;//住宿标准
        //                                        txtOtherCosts.IsReadOnly = true;//其他费用
        //                                    }
        //                                    else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
        //                                    {
        //                                        txtTFSubsidies.Text = "0";
        //                                    }
        //                                    else
        //                                    {
        //                                        if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                        {
        //                                            double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
        //                                            double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
        //                                            //double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
        //                                            //除以2是因为驻外标准不分餐费和交通补贴，2者合2为一，否则会多加
        //                                            double lastmoney = (totolDay - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
        //                                            txtTFSubsidies.Text = (minmoney + middlemoney + lastmoney).ToString();

        //                                            //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                            if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
        //                                            {
        //                                                ComfirmWindow com = new ComfirmWindow();
        //                                                com.OnSelectionBoxClosed += (obj, result) =>
        //                                                {
        //                                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                                };
        //                                                if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                                {
        //                                                    if (formType == FormTypes.Audit) return;
        //                                                    com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            txtTFSubsidies.Text = "0";
        //                                            txtTFSubsidies.IsReadOnly = false;
        //                                        }
        //                                    }
        //                                }
        //                                else//如果天数为null的禁用住宿费控件
        //                                {
        //                                    txtASubsidies.IsReadOnly = true;
        //                                }
        //                            }
        //                            if (txtMealSubsidies != null)//餐费补贴
        //                            {
        //                                if (obje.BUSINESSDAYS != null)
        //                                {
        //                                    if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                    {
        //                                        txtMealSubsidies.Text = "0";
        //                                        txtMealSubsidies.IsReadOnly = true;
        //                                        txtTranSportcosts.IsReadOnly = true;//交通费
        //                                        txtASubsidies.IsReadOnly = true;//住宿标准
        //                                        txtOtherCosts.IsReadOnly = true;//其他费用
        //                                    }
        //                                    else if (obje.GOOUTTOMEET == "1")//如果是开会
        //                                    {
        //                                        txtMealSubsidies.Text = "0";
        //                                    }
        //                                    else
        //                                    {
        //                                        if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                        {
        //                                            double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
        //                                            //double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
        //                                            double IntMaxDays = travelsolutions.MAXIMUMRANGEDAYS.ToDouble();
        //                                            double IntMinDAys = travelsolutions.MINIMUMINTERVALDAYS.ToDouble();
        //                                            double middlemoney = (IntMaxDays - IntMinDAys) * mealSubsidies;
        //                                            //double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble();
        //                                            //驻外标准：交通费和餐费补贴为一起的，所以除以2
        //                                            double lastmoney = (totolDay - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
        //                                            txtMealSubsidies.Text = (minmoney + middlemoney + lastmoney).ToString();

        //                                            //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                            if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
        //                                            {
        //                                                ComfirmWindow com = new ComfirmWindow();
        //                                                com.OnSelectionBoxClosed += (obj, result) =>
        //                                                {
        //                                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                                };
        //                                                if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                                {
        //                                                    if (formType == FormTypes.Audit) return;
        //                                                    com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            txtMealSubsidies.Text = "0";
        //                                            txtMealSubsidies.IsReadOnly = false;
        //                                        }
        //                                    }
        //                                }
        //                                else//如果天数为null的禁用住宿费控件
        //                                {
        //                                    txtASubsidies.IsReadOnly = true;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                    {
        //                        txtTFSubsidies.Text = "0";
        //                        txtMealSubsidies.Text = "0";
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region 如果出差时间大于设定的最小天数并且小于设定的最大天数的报销标准
        //            if (travelsolutions != null && employeepost != null)
        //            {
        //                TextBox txtTranSportcosts = new TextBox();//初始化交通费控件
        //                TextBox txtOtherCosts = new TextBox();//初始化其他费用控件

        //                txtTFSubsidies = GetTFSubsidiesTextBox(txtTFSubsidies, i, FromReadOnlyDataGrid);//交通补贴控件赋值
        //                txtTranSportcosts = GetTranSportcostsTextBox(txtTranSportcosts, i, FromReadOnlyDataGrid);//交通费控件赋值
        //                txtASubsidies = GetASubsidiesTextBox(txtASubsidies, i, FromReadOnlyDataGrid);//住宿费控件赋值
        //                txtOtherCosts = GetOtherCostsTextBox(txtOtherCosts, i, FromReadOnlyDataGrid);//其他费用控件赋值
        //                txtMealSubsidies = GetMealSubsidiesTextBox(txtMealSubsidies, i, FromReadOnlyDataGrid);//餐费补贴控件赋值

        //                if (totolDay >= travelsolutions.MINIMUMINTERVALDAYS.ToInt32() && totolDay <= travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
        //                {
        //                    if (entareaallowance != null)
        //                    {
        //                        double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
        //                        double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
        //                        double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
        //                        double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
        //                        if (txtTFSubsidies != null)//交通补贴
        //                        {
        //                            if (obje.BUSINESSDAYS != null)
        //                            {
        //                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                {
        //                                    txtTFSubsidies.Text = "0";
        //                                    txtTFSubsidies.IsReadOnly = true;
        //                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                }
        //                                else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
        //                                {
        //                                    txtTFSubsidies.Text = "0";
        //                                }
        //                                else
        //                                {
        //                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                    {
        //                                        double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
        //                                        double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
        //                                        txtTFSubsidies.Text = (minmoney + middlemoney).ToString();

        //                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                        if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
        //                                        {
        //                                            ComfirmWindow com = new ComfirmWindow();
        //                                            com.OnSelectionBoxClosed += (obj, result) =>
        //                                            {
        //                                                txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                txtOtherCosts.IsReadOnly = true;//其他费用
        //                                            };
        //                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                            {
        //                                                if (formType == FormTypes.Audit) return;
        //                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        txtTFSubsidies.Text = "0";
        //                                        txtTFSubsidies.IsReadOnly = false;
        //                                    }
        //                                }
        //                            }
        //                            else//如果天数为null的禁用住宿费控件
        //                            {
        //                                txtASubsidies.IsReadOnly = true;
        //                            }
        //                        }
        //                        if (txtMealSubsidies != null)//餐费补贴
        //                        {
        //                            if (obje.BUSINESSDAYS != null)
        //                            {
        //                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                {
        //                                    txtMealSubsidies.Text = "0";
        //                                    txtMealSubsidies.IsReadOnly = true;
        //                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                }
        //                                else if (obje.GOOUTTOMEET == "1")//如果是开会
        //                                {
        //                                    txtMealSubsidies.Text = "0";
        //                                }
        //                                else
        //                                {
        //                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                    {
        //                                        //最小区间段金额
        //                                        double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
        //                                        //中间区间段金额
        //                                        double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
        //                                        txtMealSubsidies.Text = (minmoney + middlemoney).ToString();

        //                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                        if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
        //                                        {
        //                                            ComfirmWindow com = new ComfirmWindow();
        //                                            com.OnSelectionBoxClosed += (obj, result) =>
        //                                            {
        //                                                txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                txtOtherCosts.IsReadOnly = true;//其他费用
        //                                            };
        //                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                            {
        //                                                if (formType == FormTypes.Audit) return;
        //                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        txtMealSubsidies.Text = "0";
        //                                        txtMealSubsidies.IsReadOnly = false;
        //                                    }
        //                                }
        //                            }
        //                            else//如果天数为null的禁用住宿费控件
        //                            {
        //                                txtASubsidies.IsReadOnly = true;
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                    {
        //                        txtTFSubsidies.Text = "0";
        //                        txtMealSubsidies.Text = "0";
        //                    }
        //                }
        //            }
        //            #endregion

        //            total += txtTFSubsidies.Text.ToDouble() + txtMealSubsidies.Text.ToDouble();
        //            this.txtSubTotal.Text = total.ToString();//总费用
        //            this.txtChargeApplyTotal.Text = total.ToString();

        //            Fees = total;
        //        }

        //        CountMoney();
        //    }
        //}
        #endregion

        #region 计算出差天数 ljx
        private void CountTravelDays(T_OA_REIMBURSEMENTDETAIL detail, DataGridRowEventArgs e)
        {
            try
            {
                int i = 0;
                if (DaGrReadOnly.ItemsSource == null)
                {
                    return;
                }
                //住宿费，交通费，其他费用

                TextBox myDaysTime = DaGrReadOnly.Columns[5].GetCellContent(e.Row).FindName("txtTHENUMBEROFNIGHTS") as TextBox;
                TextBox textAccommodation = DaGrReadOnly.Columns[9].GetCellContent(e.Row).FindName("txtACCOMMODATION") as TextBox;

                foreach (object obj in DaGrReadOnly.ItemsSource)
                {
                    i++;

                    //if (DaGrReadOnly.Columns[9].GetCellContent(obj) == null)
                    //{
                    //    break;
                    //}
                    if (((T_OA_REIMBURSEMENTDETAIL)obj).REIMBURSEMENTDETAILID == detail.REIMBURSEMENTDETAILID)
                    {

                        T_OA_REIMBURSEMENTDETAIL obje = obj as T_OA_REIMBURSEMENTDETAIL;
                        ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrReadOnly.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                        //出差天数
                        double toodays = 0;
                        //获取出差补贴
                        T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                        string cityValue = TravelDetailList_Golbal[i - 1].DESTCITY.Replace(",", "");//目标城市值
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
                            if (detail.BUSINESSDAYS != null && detail.BUSINESSDAYS != "")
                            {
                                toodays = System.Convert.ToDouble(detail.BUSINESSDAYS);
                            }

                        }
                        if (entareaallowance != null)
                        {
                            if (toodays > 0)
                            {
                                if (textAccommodation.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * Convert.ToDouble(detail.THENUMBEROFNIGHTS))//判断住宿费超标
                                {
                                    //文本框标红
                                    textAccommodation.BorderBrush = new SolidColorBrush(Colors.Red);
                                    textAccommodation.Foreground = new SolidColorBrush(Colors.Red);
                                    this.txtAccommodation.Visibility = Visibility.Visible;
                                    this.txtAccommodation.Text = "住宿费超标";
                                }
                            }
                            if (textAccommodation.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * Convert.ToDouble(detail.THENUMBEROFNIGHTS))
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

                }

                DaGrReadOnly.Columns[5].Visibility = Visibility.Collapsed;
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
                //int i = 0;
                if (TravelDetailList_Golbal == null)
                {
                    return;
                }
                //住宿费，交通费，其他费用
                bool IsPassEd = false;//住宿费是否超标
                foreach (var obj in TravelDetailList_Golbal)
                {

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
                    double totaldays = 0;
                    //获取出差补贴
                    T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                    string cityValue = obj.DESTCITY;//目标城市值
                    //根据城市查出差标准补贴（已根据岗位级别过滤）
                    entareaallowance = this.GetAllowanceByCityValue(cityValue);
                    if (!string.IsNullOrEmpty(obj.BUSINESSDAYS) && obj.BUSINESSDAYS != "0")
                    {
                        //住宿天数
                        totaldays = System.Convert.ToDouble(obj.BUSINESSDAYS);
                        if (entareaallowance != null)
                        {
                            if (textAccommodation.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * totaldays)//判断住宿费超标
                            {
                                //文本框标红
                                textAccommodation.BorderBrush = new SolidColorBrush(Colors.Red);
                                textAccommodation.Foreground = new SolidColorBrush(Colors.Red);
                                this.txtAccommodation.Visibility = Visibility.Visible;
                                IsPassEd = true;
                                //this.txtAccommodation.Text = "住宿费超标";
                            }

                            if (textAccommodation.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * totaldays)
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

                    double ta = textTransportcosts.Text.ToDouble() + textAccommodation.Text.ToDouble() + textOthercosts.Text.ToDouble();
                    totall = totall + ta;

                    totall += txtTFSubsidies.Text.ToDouble() + txtMealSubsidies.Text.ToDouble();
                    //totall += Fees;
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
                if (fbCtr.totalMoney > 0)
                {
                    totall = totall + Convert.ToDouble(fbCtr.totalMoney);
                }
                txtChargeApplyTotal.Text = totall.ToString(); //费用报销总计包括其他费用，如业务招待费               
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
