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
        #region 新建时初始化
        private void NewMaster_Golbal()
        {
            TravelReimbursement_Golbal = new T_OA_TRAVELREIMBURSEMENT();
            TravelReimbursement_Golbal.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            ReimbursementTime.Text = System.DateTime.Now.ToShortDateString();
            TravelReimbursement_Golbal.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            TravelReimbursement_Golbal.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            TravelReimbursement_Golbal.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            TravelReimbursement_Golbal.OWNERPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            TravelReimbursement_Golbal.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            TravelReimbursement_Golbal.OWNERDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            TravelReimbursement_Golbal.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            TravelReimbursement_Golbal.OWNERCOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            TravelReimbursement_Golbal.POSTLEVEL = Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();

        }
        #endregion

        /// <summary>
        /// 操作子表数据
        /// </summary>
        private void SetTravelDetailValueFromForm()
        {
            string StrStartDt = "";   //开始时间
            string StrStartTime = ""; //开始时：分
            string EndDt = "";    //结束时间
            string StrEndTime = ""; //结束时：分
            int i = 0;
            if (DaGrEdit.ItemsSource != null)
            {
                //foreach (Object obje in DaGrEdit.ItemsSource) 
                foreach (var obje in TravelDetailList_Golbal)
                {
                    TrDetail = obje;
                    TrDetail.REIMBURSEMENTDETAILID = (obje as T_OA_REIMBURSEMENTDETAIL).REIMBURSEMENTDETAILID;
                    
                    TrDetail.T_OA_TRAVELREIMBURSEMENT = TravelReimbursement_Golbal;
                    //出发时间
                    DateTimePicker StartDate = ((DateTimePicker)((StackPanel)DaGrEdit.Columns[0].GetCellContent(obje)).Children.FirstOrDefault()) as DateTimePicker;
                    //到达时间
                    DateTimePicker EndDate = ((DateTimePicker)((StackPanel)DaGrEdit.Columns[2].GetCellContent(obje)).Children.FirstOrDefault()) as DateTimePicker;
                    //出差天数
                    TextBox datys = ((TextBox)((StackPanel)DaGrEdit.Columns[4].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    //住宿天数
                    TextBox Newdatys = ((TextBox)((StackPanel)DaGrEdit.Columns[5].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    //交通工具类型
                    TravelDictionaryComboBox ToolType = ((TravelDictionaryComboBox)((StackPanel)DaGrEdit.Columns[6].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                    //交通工具级别
                    TravelDictionaryComboBox ToolLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrEdit.Columns[7].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                    
                    //交通费第8列
                    TextBox txtToolubsidies = ((TextBox)((StackPanel)DaGrEdit.Columns[8].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    //住宿费第9列
                    TextBox txtASubsidies = ((TextBox)((StackPanel)DaGrEdit.Columns[9].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    //交通补贴第10列
                    TextBox txtTFSubsidies = ((TextBox)((StackPanel)DaGrEdit.Columns[10].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    //餐费补贴第11列
                    TextBox txtMealSubsidies = ((TextBox)((StackPanel)DaGrEdit.Columns[11].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    //其他费用第12列
                    TextBox txtOthercosts = ((TextBox)((StackPanel)DaGrEdit.Columns[12].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    //是否私事
                    CheckBox IsCheck = ((CheckBox)((StackPanel)DaGrEdit.Columns[13].GetCellContent(obje)).Children.FirstOrDefault()) as CheckBox;
                    //是否会议
                    CheckBox IsCheckMeet = ((CheckBox)((StackPanel)DaGrEdit.Columns[14].GetCellContent(obje)).Children.FirstOrDefault()) as CheckBox;
                    //是否公司派车
                    CheckBox IsCheckCar = ((CheckBox)((StackPanel)DaGrEdit.Columns[15].GetCellContent(obje)).Children.FirstOrDefault()) as CheckBox;

                    StrStartDt = StartDate.Value.Value.ToString("d");//开始日期
                    EndDt = EndDate.Value.Value.ToString("d");//结束日期
                    StrStartTime = StartDate.Value.Value.ToString("HH:mm");//开始时间
                    StrEndTime = EndDate.Value.Value.ToString("HH:mm");//结束时间

                    DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime); ;
                    DateTime DtEnd = System.Convert.ToDateTime(EndDt + " " + StrEndTime);

                    TrDetail.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                    TrDetail.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                    TrDetail.CREATEDATE = DateTime.Now;
                    TrDetail.UPDATEDATE = DateTime.Now;
                    if (DtStart != null)//开始时间
                    {
                        TrDetail.STARTDATE = DtStart;
                    }
                    if (DtEnd != null)//结束时间
                    {
                        TrDetail.ENDDATE = DtEnd;
                    }
                    if (datys != null)//出差天数
                    {
                        TrDetail.BUSINESSDAYS = datys.Text;
                    }
                    if (Newdatys != null)//住宿天数
                    {
                        if (!string.IsNullOrEmpty(TrDetail.BUSINESSDAYS))
                        {
                            TrDetail.THENUMBEROFNIGHTS =TrDetail.BUSINESSDAYS;
                        }
                    }
                    if (TravelDetailList_Golbal.Count() > 0)
                    {
                        TrDetail.DEPCITY = TravelDetailList_Golbal[i].DEPCITY.Replace(",", "");//出发城市
                    }
                    if (TravelDetailList_Golbal.Count() > 0)
                    {
                        TrDetail.DESTCITY = TravelDetailList_Golbal[i].DESTCITY.Replace(",", "");//目标城市
                    }
                    if (ToolType != null)//乘坐交通工具类型
                    {
                        T_SYS_DICTIONARY ComVechileObj = ToolType.SelectedItem as T_SYS_DICTIONARY;
                        TrDetail.TYPEOFTRAVELTOOLS = ComVechileObj.DICTIONARYVALUE.ToString();
                    }
                    if (ToolLevel != null)//乘坐交通工具级别
                    {
                        T_SYS_DICTIONARY ComLevelObj = ToolLevel.SelectedItem as T_SYS_DICTIONARY;
                        TrDetail.TAKETHETOOLLEVEL = ComLevelObj.DICTIONARYVALUE.ToString();
                    }
                    if (txtToolubsidies != null)//交通费
                    {
                        if (!string.IsNullOrEmpty(txtToolubsidies.Text))
                        { 
                            TrDetail.TRANSPORTCOSTS = Convert.ToDecimal(txtToolubsidies.Text); }
                        else
                        {
                            TrDetail.TRANSPORTCOSTS = 0;
                        }
                    }
                    if (txtASubsidies != null)//住宿标准费用
                    {
                        if (!string.IsNullOrEmpty(txtASubsidies.Text))
                        {
                            TrDetail.ACCOMMODATION = Convert.ToDecimal(txtASubsidies.Text);
                        }
                        else
                        {
                            TrDetail.ACCOMMODATION = 0;
                        }
                    }
                    if (txtTFSubsidies != null)//交通费用补贴
                    {
                        if (!string.IsNullOrEmpty(txtTFSubsidies.Text))
                        {
                            TrDetail.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(txtTFSubsidies.Text);
                        }
                        else
                        {
                            TrDetail.TRANSPORTATIONSUBSIDIES = 0;
                        }
                    }
                    if (txtMealSubsidies != null)//餐费补贴
                    {
                        if (!string.IsNullOrEmpty(txtMealSubsidies.Text))
                        {
                            TrDetail.MEALSUBSIDIES = Convert.ToDecimal(txtMealSubsidies.Text);
                        }
                        else
                        {
                            TrDetail.MEALSUBSIDIES = 0;
                        }
                    }
                    if (txtOthercosts != null)//其他费用
                    {
                        if (!string.IsNullOrEmpty(txtOthercosts.Text))
                        {
                            TrDetail.OTHERCOSTS = Convert.ToDecimal(txtOthercosts.Text);
                        }
                        else
                        {
                            TrDetail.OTHERCOSTS = 0;
                        }
                    }
                    #region 上面的金额赋值存在丢失现象，再次赋值
                    ////交通费第8列
                    //TextBox txtToolubsidies_agin = ((TextBox)((StackPanel)DaGrEdit.Columns[8].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    ////住宿费第9列
                    //TextBox txtASubsidies_agin = ((TextBox)((StackPanel)DaGrEdit.Columns[9].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    ////交通补贴第10列
                    //TextBox txtTFSubsidies_agin = ((TextBox)((StackPanel)DaGrEdit.Columns[10].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    ////餐费补贴第11列
                    //TextBox txtMealSubsidies_agin = ((TextBox)((StackPanel)DaGrEdit.Columns[11].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    ////其他费用第12列
                    //TextBox txtOthercosts_agin = ((TextBox)((StackPanel)DaGrEdit.Columns[12].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;


                    //if (txtToolubsidies_agin != null)
                    //{
                    //    if (!string.IsNullOrEmpty(txtToolubsidies_agin.Text))
                    //    {
                    //        TrDetail.TRANSPORTCOSTS = Convert.ToDecimal(txtToolubsidies_agin.Text);//交通费
                    //    }
                    //}
                    //if (txtASubsidies_agin != null)
                    //{
                    //    if (!string.IsNullOrEmpty(txtASubsidies_agin.Text))
                    //    {
                    //         TrDetail.ACCOMMODATION = Convert.ToDecimal(txtASubsidies_agin.Text);//住宿费
                    //    }
                    //}

                    //if (txtTFSubsidies_agin != null)
                    //{
                    //    if (!string.IsNullOrEmpty(txtTFSubsidies_agin.Text))
                    //    {
                    //        TrDetail.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(txtTFSubsidies_agin.Text);//交通补贴
                    //    }
                    //}
                    //if (txtMealSubsidies != null)
                    //{
                    //    if (!string.IsNullOrEmpty(txtMealSubsidies.Text))
                    //    {
                    //        TrDetail.MEALSUBSIDIES = Convert.ToDecimal(txtMealSubsidies.Text);// 餐费补贴
                    //    }
                    //}
                    //if (txtOthercosts_agin != null)
                    //{
                    //    if (!string.IsNullOrEmpty(txtOthercosts_agin.Text))
                    //    {
                    //        TrDetail.OTHERCOSTS = Convert.ToDecimal(txtOthercosts_agin.Text); //其他费用        
                    //    }
                    //}
                    #endregion

                    if (IsCheck != null)//是否是私事
                    {
                        TrDetail.PRIVATEAFFAIR = (bool)IsCheck.IsChecked ? "1" : "0";
                    }
                    if (IsCheckMeet != null)//是否是开会
                    {
                        TrDetail.GOOUTTOMEET = (bool)IsCheckMeet.IsChecked ? "1" : "0";
                    }
                    if (IsCheckCar != null)//公司派车
                    {
                        TrDetail.COMPANYCAR = (bool)IsCheckCar.IsChecked ? "1" : "0";
                    }

                    //ListDetail.Add(TrDetail);
                    
                    i++;
                }
            }
        }

    }
}
