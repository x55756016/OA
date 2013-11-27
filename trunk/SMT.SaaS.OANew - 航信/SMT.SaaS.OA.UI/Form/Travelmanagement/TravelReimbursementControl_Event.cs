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

        #region 新建出差明细按钮事件
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsFinishedCitys())
            {
                return;
            }

            BtnNewButton = true;
            int i = 0;
            T_OA_REIMBURSEMENTDETAIL NewDetail = new T_OA_REIMBURSEMENTDETAIL();
            NewDetail.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();

            if (formType != FormTypes.New)
            {
                if (TravelDetailList_Golbal.Count() > 0)
                {
                    if (TravelDetailList_Golbal.LastOrDefault().DESTCITY == TravelDetailList_Golbal.FirstOrDefault().DEPCITY)
                    {
                        MessageBox.Show("请修改最后一条记录的到达城市后再新增记录！");
                        return;
                    }
                    //将原有记录的最后一条记录的目的城市作为出发城市。
                    //NewDetail.DEPCITY = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(TrList.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>().DESTCITY);
                    if (TravelDetailList_Golbal.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>() != null)
                    {
                        //默认出发城市为上一条记录的到达城市
                        NewDetail.DEPCITY = TravelDetailList_Golbal.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>().DESTCITY;
                        //默认出发日期为上一条记录的结束时间+1天
                        NewDetail.STARTDATE = TravelDetailList_Golbal.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>().ENDDATE.Value.AddDays(1);
                        //默认结束城市为出差出发城市
                        NewDetail.DESTCITY = TravelDetailList_Golbal.FirstOrDefault<T_OA_REIMBURSEMENTDETAIL>().DEPCITY;
                        //默认结束日期为出发时间+1
                        NewDetail.ENDDATE = NewDetail.STARTDATE.Value.AddDays(1);
                    }
                }
                NewDetail.TYPEOFTRAVELTOOLS = "3";//默认乘坐交通工具为火车
                NewDetail.TAKETHETOOLLEVEL = "1";//默认交通工具级别为硬卧
                //NewDetail.ENDDATE = DateTime.Now;
                TravelDetailList_Golbal.Add(NewDetail);
                //禁用所有开始城市选择控件？
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

            int lastIndex = TravelDetailList_Golbal.Count() - 1;
            StandardsMethod(lastIndex);//显示选中的城市的出差标准
            //计算并给实体赋值
            SetTraveValueAndFBChargeValue();
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

        #region 出差报销行删除事件
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DaGrEdit.SelectedItems == null)
            {
                return;
            }

            if (DaGrEdit.SelectedItems.Count == 0)
            {
                return;
            }

            TravelDetailList_Golbal = DaGrEdit.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
            if (TravelDetailList_Golbal.Count() > 1)
            {                
                int index = DaGrEdit.SelectedIndex;//当前选中行
                var entity = TravelDetailList_Golbal[index];//当前选择实体
                if (index > 0 && index < TravelDetailList_Golbal.Count-1 )//选择的不是第一条也不是最后一条
                {
                    //下一城市出发城市改为上一城市的到达城市值
                    TravelDetailList_Golbal[index + 1].DEPCITY = TravelDetailList_Golbal[index - 1].DESTCITY;
                    SearchCity mystarteachCity = DaGrEdit.Columns[1].GetCellContent(TravelDetailList_Golbal[index + 1]).FindName("txtDEPARTURECITY") as SearchCity;
                    mystarteachCity.TxtSelectedCity.Text = GetCityName(TravelDetailList_Golbal[index + 1].DEPCITY);
                }

                TravelDetailList_Golbal.Remove(entity);
                             
                //DaGrEdit.ItemsSource = TravelDetailList_Golbal;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "必须保留一条出差时间及地点!", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }
        #endregion
    }
}
                                          