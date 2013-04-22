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

        #region 获取出差方案并获取方案里设置的相关的设置项目

        /// <summary>
        /// 根据当前用户的岗位级别与方案设置的岗位级别匹配，确认该出差人是否能够申请借款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Travelmanagement_GetTravelSolutionByCompanyIDCompleted(object sender, GetTravelSolutionByCompanyIDCompletedEventArgs e)
        {
            try
            {
                travelsolutions_Golbal = new T_OA_TRAVELSOLUTIONS();
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                if (e.Result != null)
                {
                    travelsolutions_Golbal = e.Result;//出差方案
                }
                if (Master_Golbal.POSTLEVEL.ToInt32() <= travelsolutions_Golbal.RANGEPOSTLEVEL.ToInt32())
                {
                    fbCtr.IsEnabled = false;//如果当前用户的级别与方案设置的"报销范围级别"相同则不能申请费用
                }
                if (e.PlaneObj != null)
                {
                    cantaketheplaneline = e.PlaneObj.ToList();//乘坐飞机线路设置
                }
                if (e.StandardObj != null)
                {
                    transportToolStand = e.StandardObj.ToList();//乘坐交通工具标准设置
                }
                if (formType!=FormTypes.New && Master_Golbal.T_OA_BUSINESSTRIPDETAIL.Count > 0)
                {
                    BindDataGrid(Master_Golbal.T_OA_BUSINESSTRIPDETAIL);
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
        /// 根据当前用户的级别过滤出该级别能乘坐的交通工具类型
        /// </summary>
        /// <param name="ToolType">交通工具类型</param>
        /// <returns></returns>
        private T_OA_TAKETHESTANDARDTRANSPORT GetVehicleTypeValue(string TraveToolType)
        {
            if (string.IsNullOrEmpty(TraveToolType))
            {
                var q = from ent in transportToolStand
                        where ent.ENDPOSTLEVEL.Contains(Master_Golbal.POSTLEVEL)
                        select ent;
                q = q.OrderBy(n => n.TYPEOFTRAVELTOOLS);
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
            }
            else
            {
                var q = from ent in transportToolStand
                        where ent.ENDPOSTLEVEL.Contains(Master_Golbal.POSTLEVEL) && ent.TYPEOFTRAVELTOOLS == TraveToolType
                        orderby ent.TAKETHETOOLLEVEL ascending
                        select ent;

                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
            }
            return null;
        }

        /// <summary>
        /// 根据当前用户的级别过滤出该级别能乘坐的交通工具类型
        /// </summary>
        /// <param name="TraveToolType">交通工具类型</param>
        /// <param name="Master_Golbal.POSTLEVEL">岗位级别</param>
        /// <returns>0：类型超标，1：类型不超标，2：级别不超标</returns>
        private int CheckTraveToolStand(string TraveToolType, string TraveToolLevel, string POSTLEVEL)
        {
            int i = 0;
            var q = from ent in transportToolStand
                    where ent.ENDPOSTLEVEL.Contains(POSTLEVEL) && ent.TYPEOFTRAVELTOOLS == TraveToolType
                    orderby ent.TAKETHETOOLLEVEL ascending
                    select ent;
            if (q.Count() > 0)
            {
                i = 1;
            }
            var qLevel = from ent in q
                         where ent.TAKETHETOOLLEVEL.Contains(TraveToolLevel)
                         select ent;
            if (qLevel.Count() > 0)
            {
                i = 2;
            }
            return i;
        }
        #endregion

    }
}
