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

        #region 选择出差人LookUP
        private void lookupTraveEmployee_FindClick(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    //Master_Golbal.OWNERPOSTNAME = post.ObjectName;//岗位                 

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string selectCompanyId = corp.COMPANYID;

                    string StrEmployee = userInfo.ObjectName + "[" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME + "]";
                    txtTraveEmployee.Text = StrEmployee;//出差人
                    //strTravelEmployeeName = userInfo.ObjectName;
                    ToolTipService.SetToolTip(txtTraveEmployee, StrEmployee);

                    Master_Golbal.POSTLEVEL = (ent.FirstOrDefault().ObjectInstance
                     as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID
                         == postid).FirstOrDefault().POSTLEVEL.ToString();

                    Master_Golbal.OWNERCOMPANYID = selectCompanyId;
                    Master_Golbal.OWNERCOMPANYNAME = corp.CNAME;

                    Master_Golbal.OWNERDEPARTMENTID = deptid;
                    Master_Golbal.OWNERDEPARTMENTNAME = dept.ObjectName;

                    Master_Golbal.OWNERPOSTID = postid;
                    Master_Golbal.OWNERPOSTNAME = post.ObjectName;

                    Master_Golbal.OWNERID = userInfo.ObjectID;
                    Master_Golbal.OWNERNAME = userInfo.ObjectName;


                    //fb控件
                    fbCtr.Order.OWNERCOMPANYID = selectCompanyId;
                    fbCtr.Order.OWNERCOMPANYNAME = corp.CNAME;

                    fbCtr.Order.OWNERDEPARTMENTID = deptid;
                    fbCtr.Order.OWNERDEPARTMENTNAME = dept.ObjectName;

                    fbCtr.Order.OWNERPOSTID = postid;
                    fbCtr.Order.OWNERPOSTNAME = post.ObjectName;

                    fbCtr.Order.OWNERID = userInfo.ObjectID;
                    fbCtr.Order.OWNERNAME = userInfo.ObjectName;
                    fbCtr.RefreshData();

                    if (!string.IsNullOrEmpty(selectCompanyId))//如果是选出差人的情况下(获取所选用户公司)
                    {
                        OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Master_Golbal.OWNERCOMPANYID, null, null);
                    }
                    else //默认是当前用户(当前用户公司)
                    {
                        MessageBox.Show("请选择出差员工");
                    }

                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion

        #region 屏蔽控件
        private void HideControl()
        {
            lookupTraveEmployee.IsEnabled = false;
            txtSubject.IsReadOnly = true;
            txtTELL.IsReadOnly = true;
            ckEnabled.IsEnabled = false;

            svdgEdit.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            DaGrs.IsEnabled = false;
            DaGrs.Columns[9].Visibility = Visibility.Collapsed;
            svdgEdit.IsEnabled = true;
            fbCtr.IsEnabled = false;
            FormToolBar1.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 新建按钮事件
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsFinishedCitys())
            {
                return;
            }
            BtnNewButton = true;

            T_OA_BUSINESSTRIPDETAIL NewBussnessTripDetail = new T_OA_BUSINESSTRIPDETAIL();
            NewBussnessTripDetail.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();

            if (TraveDetailList_Golbal.Count() > 0)
            {
                //新的赋值方式
                NewBussnessTripDetail.STARTDATE = TraveDetailList_Golbal[TraveDetailList_Golbal.Count() - 1].ENDDATE;
                NewBussnessTripDetail.DEPCITY = citysEndList_Golbal[TraveDetailList_Golbal.Count() - 1];
                NewBussnessTripDetail.ENDDATE = DateTime.Now;
                //citysStartList_Golbal.Add(citysEndList_Golbal[TraveDetailList_Golbal.Count() - 1]);绑定后会增加一条
                TraveDetailList_Golbal.Add(NewBussnessTripDetail);
                DaGrs.ItemsSource = TraveDetailList_Golbal;
                //禁用此记录开始城市选择控件
                SearchCity myCity = DaGrs.Columns[1].GetCellContent(NewBussnessTripDetail).FindName("txtDEPARTURECITY") as SearchCity;
                if (myCity != null)
                {
                    myCity.TxtSelectedCity.Text = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(citysStartList_Golbal[TraveDetailList_Golbal.Count() - 1]);
                    myCity.IsEnabled = false;
                    ((DataGridCell)((StackPanel)myCity.Parent).Parent).IsEnabled = false;
                }
                //旧的赋值方式
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
            foreach (object obje in DaGrs.ItemsSource)
            {
                SearchCity myCitys = DaGrs.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;

                TraveDetailOne_Golbal.T_OA_BUSINESSTRIP = Master_Golbal;
                DateTimePicker StartDate = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker EndDate = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;

                TravelDictionaryComboBox ToolType = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[4].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                TravelDictionaryComboBox ToolLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[5].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;

                SearchCity DepartCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                SearchCity TargetCity = DaGrs.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;
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

                if (string.IsNullOrEmpty(StrEndTime))//结束日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "结束时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }

                if (string.IsNullOrEmpty(DepartCity.TxtSelectedCity.Text))//出发城市不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发城市不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }

                if (string.IsNullOrEmpty(TargetCity.TxtSelectedCity.Text))//到达城市不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "到达城市不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
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

        #region 行删除事件
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DaGrs.SelectedItems == null)
            {
                return;
            }

            if (DaGrs.SelectedItems.Count == 0)
            {
                return;
            }

            try
            {
                TraveDetailList_Golbal = DaGrs.ItemsSource as ObservableCollection<T_OA_BUSINESSTRIPDETAIL>;
                if (TraveDetailList_Golbal.Count() > 1)
                {
                    int selectGridRowIndex = DaGrs.SelectedIndex;
                    T_OA_BUSINESSTRIPDETAIL entDel = DaGrs.SelectedItems[selectGridRowIndex] as T_OA_BUSINESSTRIPDETAIL;
                    if (TraveDetailList_Golbal.Contains(entDel))
                    {
                        TraveDetailList_Golbal.Remove(entDel);
                        if (citysEndList_Golbal.Count >= selectGridRowIndex)
                        {
                            citysEndList_Golbal.RemoveAt(selectGridRowIndex - 1);
                        }
                        if (citysStartList_Golbal.Count >= selectGridRowIndex)
                        {
                            citysStartList_Golbal.RemoveAt(selectGridRowIndex - 1);
                        }
                    }
                    //如果选中的不是第一条也不是最后一行，那么修改选中行的下一行的开始城市
                    if (1 < selectGridRowIndex && selectGridRowIndex < TraveDetailList_Golbal.Count - 1)
                    {
                        Object obje = DaGrs.SelectedItems[selectGridRowIndex + 1];
                        SearchCity mystarteachCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                        mystarteachCity.TxtSelectedCity.Text = GetCityName(citysEndList_Golbal[selectGridRowIndex - 1]);
                        citysStartList_Golbal[selectGridRowIndex + 1] = citysEndList_Golbal[selectGridRowIndex - 1];//上一城市的城市值
                    }
                    DaGrs.ItemsSource = TraveDetailList_Golbal;
                    //for (int i = 0; i < DaGrs.SelectedItems.Count; i++)
                    //{
                    //    int k = DaGrs.SelectedIndex;//当前选中行
                    //    T_OA_BUSINESSTRIPDETAIL entDel = DaGrs.SelectedItems[i] as T_OA_BUSINESSTRIPDETAIL;

                    //    if (TraveDetailList_Golbal.Contains(entDel))
                    //    {

                    //        TraveDetailList_Golbal.Remove(entDel);
                    //        if (citysEndList_Golbal.Count > k)
                    //        {

                    //            int EachCount = 0;
                    //            foreach (Object obje in DaGrs.ItemsSource)//将下一个出发城市的值修改
                    //            {
                    //                EachCount++;
                    //                if (DaGrs.Columns[1].GetCellContent(obje) != null)
                    //                {
                    //                    SearchCity mystarteachCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                    //                    if ((k + 1) == EachCount)
                    //                    {
                    //                        if (k > 0)
                    //                        {
                    //                            mystarteachCity.TxtSelectedCity.Text = GetCityName(citysEndList_Golbal[k - 1]);
                    //                            citysStartList_Golbal[k + 1] = citysEndList_Golbal[k - 1];//上一城市的城市值
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            citysEndList_Golbal.RemoveAt(k);//清除目标城市的值
                    //            citysStartList_Golbal.RemoveAt(k);//清除出发城市的值
                    //        }
                    //        //buipList[k].PRIVATEAFFAIR = "0";//清除私事的勾选
                    //        //buipList[k].GOOUTTOMEET = "0";//清除内部开会\培训的勾选
                    //        //buipList[k].COMPANYCAR = "0";//清除公司派车的勾选
                    //    }
                    //}
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "必须保留一条出差时间及地点!", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统错误，请联系管理员：" + ex.ToString());
            }
        }
        #endregion

        #region 出发城市 目标城市 lookup选择
        /// <summary>
        /// 出发城市lookup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtDEPARTURECITY_SelectClick(object sender, EventArgs e)
        {
            SearchCity senderCity = (SearchCity)sender;
            AreaSortCity SelectCity = new AreaSortCity();

            SelectCity.SelectedClicked += (obj, ea) =>
            {
                string selectCityName = SelectCity.Result.Keys.FirstOrDefault();
                string selectCityValue = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                selectCityValue = selectCityValue.Replace(',', ' ').Trim();

                try
                {
                    int selectGridRowIndex = DaGrs.SelectedIndex;


                    if (selectGridRowIndex > 0 && citysEndList_Golbal.Count >= selectGridRowIndex + 1)
                    {
                        if (selectCityValue == citysEndList_Golbal[selectGridRowIndex])
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                    }
                    for (int i = 0; i < citysStartList_Golbal.Count; i++)
                    {
                        if (citysStartList_Golbal.Count > 1)
                        {
                            //如果上下两条出差记录城市一样
                            if (i < citysStartList_Golbal.Count - 1 && citysStartList_Golbal[i] == citysStartList_Golbal[i + 1])
                            {
                                //出发城市与开始城市不能相同
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                        }
                    }

                    senderCity.TxtSelectedCity.Text = selectCityName;
                    if (citysStartList_Golbal.Count >= selectGridRowIndex + 1)
                    {
                        citysStartList_Golbal[selectGridRowIndex] = selectCityValue;
                    }
                    else
                    {
                        citysStartList_Golbal.Add(selectCityValue);
                    }


                }
                catch (Exception ex)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统错误，请联系管理员：" + ex.ToString());
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                }
                //if (DaGrs.SelectedItem != null)
                //{
                //    int selectGridRowIndex = DaGrs.SelectedIndex;
                //    if (DaGrs.Columns[1].GetCellContent(DaGrs.SelectedItem) != null)
                //    {
                //        //T_OA_BUSINESSTRIPDETAIL travDetaillist = DaGrs.SelectedItem as T_OA_BUSINESSTRIPDETAIL;
                //        SearchCity thisEndCity = DaGrs.Columns[1].GetCellContent(DaGrs.SelectedItem).FindName("txtDEPARTURECITY") as SearchCity;//出发城市
                //        SearchCity thisStartCity = DaGrs.Columns[3].GetCellContent(DaGrs.SelectedItem).FindName("txtTARGETCITIES") as SearchCity;//目标城市
                //        //int k = citysStartList.IndexOf(travDetaillist.DEPCITY);

                //        if (citysStartList.Count() >= selectGridRowIndex + 1)
                //        {
                //            citysStartList[selectGridRowIndex] = selectCityValue;
                //        }
                //        else
                //        {
                //            citysStartList.Add(selectCityValue);
                //        }
                //        if (citysStartList.Count > 1)
                //        {
                //            if (thisEndCity.TxtSelectedCity.Text.ToString().Trim() == thisStartCity.TxtSelectedCity.Text.ToString().Trim())
                //            {
                //                thisEndCity.TxtSelectedCity.Text = string.Empty;
                //                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //                return;
                //            }
                //        }
                //    }
                //    else
                //    {

                //    }
                //}
                //if (citysStartList.Last().Split(',').Count() > 2)
                //{
                //    senderCity.TxtSelectedCity.Text = string.Empty;
                //    citysStartList.RemoveAt(citysStartList.Count - 1);
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANONLYCHOOSEONE", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
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
        /// 到达城市lookup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtTARGETCITIES_SelectClick(object sender, EventArgs e)
        {
            SearchCity serchCitySender = (SearchCity)sender;
            AreaSortCity SelectCity = new AreaSortCity();

            SelectCity.SelectedClicked += (obj, ea) =>
            {
                string selectCityName = SelectCity.Result.Keys.FirstOrDefault();
                string selectCityValue = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                selectCityValue = selectCityValue.Replace(',', ' ').Trim();

                try
                {
                    int selectGridRowIndex = DaGrs.SelectedIndex;

                    if (selectGridRowIndex >= 0 && citysStartList_Golbal.Count >= selectGridRowIndex + 1)
                    {
                        if (selectCityValue == citysStartList_Golbal[selectGridRowIndex])
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                    }

                    if (citysEndList_Golbal.Count >= selectGridRowIndex + 1)
                    {
                        citysEndList_Golbal[selectGridRowIndex] = selectCityValue;
                    }
                    else
                    {
                        citysEndList_Golbal.Add(selectCityValue);
                    }
                    serchCitySender.TxtSelectedCity.Text = selectCityName;
                    //设置下一行的起始城市 如果为最后一行数据，且grid有下一行，增加下一行
                    if (citysStartList_Golbal.Count == (selectGridRowIndex + 1))
                    {
                        ObservableCollection<T_OA_BUSINESSTRIPDETAIL> objs = DaGrs.ItemsSource as ObservableCollection<T_OA_BUSINESSTRIPDETAIL>;
                        if (objs.Count > selectGridRowIndex + 1)
                        {
                            citysStartList_Golbal.Add(selectCityValue);
                            SetNextDepartureCity(selectGridRowIndex);
                        }
                    }
                    else
                    {
                        //citysStartList[selectGridRowIndex] = thisSelectStartCity.Tag.ToString();
                        citysStartList_Golbal[selectGridRowIndex + 1] = selectCityValue;//出发城市中下一条记录
                        SetNextDepartureCity(selectGridRowIndex);
                    }
                }
                catch (Exception ex)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统错误，请联系管理员：" + ex.ToString());
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                }
                //serchCitySender.TxtSelectedCity.Text = SelectCity.Result.Keys.FirstOrDefault();
                //if (DaGrs.SelectedItem != null)
                //{
                //    int selectGridRowIndex = DaGrs.SelectedIndex;//选择的行数，选择的行数也就是目的城市的位置
                //    if (DaGrs.Columns[3].GetCellContent(DaGrs.SelectedItem) != null)
                //    {
                //        //T_OA_BUSINESSTRIPDETAIL travDetaillist = DaGrs.SelectedItem as T_OA_BUSINESSTRIPDETAIL;
                //        SearchCity thisSelectEndCity = DaGrs.Columns[3].GetCellContent(DaGrs.SelectedItem).FindName("txtTARGETCITIES") as SearchCity;
                //        SearchCity thisSelectStartCity = DaGrs.Columns[1].GetCellContent(DaGrs.SelectedItem).FindName("txtDEPARTURECITY") as SearchCity;

                //        if (citysEndList.Count > 1)
                //        {
                //            if (thisSelectStartCity.TxtSelectedCity.Text.ToString().Trim() == thisSelectEndCity.TxtSelectedCity.Text.ToString().Trim())
                //            {
                //                thisSelectEndCity.TxtSelectedCity.Text = string.Empty;
                //                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //                return;
                //            }
                //        }
                //        if (citysEndList.Count >= (selectGridRowIndex + 1))
                //        {
                //            citysEndList[selectGridRowIndex] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                //        }
                //        else
                //        {
                //            citysEndList.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                //        }
                //        //设置下一行的起始城市
                //        if (citysStartList.Count == (selectGridRowIndex + 1))
                //        {
                //            citysStartList.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                //            SetNextDepartureCity(selectGridRowIndex);
                //        }
                //        else
                //        {
                //            citysStartList[selectGridRowIndex] = thisSelectStartCity.Tag.ToString();
                //            citysStartList[selectGridRowIndex + 1] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();//出发城市中下一条记录
                //            SetNextDepartureCity(selectGridRowIndex);
                //        }
                //    }
                //}
                //if (citysEndList.Last().Split(',').Count() > 2)
                //{
                //    serchCitySender.TxtSelectedCity.Text = string.Empty;
                //    citysEndList.RemoveAt(citysEndList.Count - 1);
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANONLYCHOOSEONE", "ARRIVALCITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
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
            foreach (Object obje in DaGrs.ItemsSource)//将下一个出发城市的值修改
            {
                EachCount++;
                if (DaGrs.Columns[1].GetCellContent(obje) != null)
                {
                    SearchCity mystarteachCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                    //DateTimePicker myDaysTime = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                    if ((SelectIndex + 2) == EachCount)
                    {
                        mystarteachCity.TxtSelectedCity.Text = GetCityName(citysStartList_Golbal[SelectIndex + 1]);
                        //myDaysTime.Value = Convert.ToDateTime(endTime[SelectIndex+1]);
                    }
                }
            }
        }
        #endregion

        #region 私事myChkBox_Checked事件
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].PRIVATEAFFAIR = "1";
                    }
                }
            }
        }

        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].PRIVATEAFFAIR = "0";
                    }
                }
            }
        }
        #endregion

        #region 交通工具类型、级别控件
        /// <summary>
        /// 获取交通工具的级别
        /// </summary>
        void GetVechileLevelInfos()
        {
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var objs = from d in dicts
                       where d.DICTIONCATEGORY == "VICHILELEVEL"
                       orderby d.DICTIONARYVALUE
                       select d;
            ListVechileLevel = objs.ToList();
        }

        private void ComVechileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TravelDictionaryComboBox vechiletype = sender as TravelDictionaryComboBox;
            if (vechiletype.SelectedIndex >= 0)
            {
                var thd = transportToolStand.FirstOrDefault();
                thd = this.GetVehicleTypeValue("");
                T_SYS_DICTIONARY VechileTypeObj = vechiletype.SelectedItem as T_SYS_DICTIONARY;
                if (DaGrs.SelectedItem != null)
                {
                    if (DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem) != null)
                    {
                        TravelDictionaryComboBox ComLevel = DaGrs.Columns[5].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;

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
                //if (employeepost != null)
                //{
                if (!string.IsNullOrEmpty(Master_Golbal.POSTLEVEL))
                {
                    if (DaGrs.SelectedItem != null)
                    {
                        if (DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem) != null)
                        {
                            TravelDictionaryComboBox ComLevel = DaGrs.Columns[5].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                            TravelDictionaryComboBox ComType = DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileType") as TravelDictionaryComboBox;
                            T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                            T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                            level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                            type = ComType.SelectedItem as T_SYS_DICTIONARY;

                            if (transportToolStand.Count() > 0)
                            {
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
                                        }
                                        else
                                        {
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE && thd.TAKETHETOOLLEVEL.ToInt32() > level.DICTIONARYVALUE)
                                            {
                                                ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                return;
                                            }
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE)
                                            {
                                                ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //}
            }
        }


        private void ComVechileTypeLeve_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TravelDictionaryComboBox vechiletype = sender as TravelDictionaryComboBox;
            if (vechiletype.SelectedIndex >= 0)
            {
                //if (employeepost != null)
                //{
                if (!string.IsNullOrEmpty(Master_Golbal.POSTLEVEL))
                {
                    if (DaGrs.SelectedItem != null)
                    {
                        var thd = transportToolStand.FirstOrDefault();

                        T_SYS_DICTIONARY VechileTypeObj = vechiletype.SelectedItem as T_SYS_DICTIONARY;

                        if (DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem) != null)
                        {
                            TravelDictionaryComboBox ComLevel = DaGrs.Columns[5].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                            TravelDictionaryComboBox ComType = DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileType") as TravelDictionaryComboBox;

                            T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                            T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                            level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                            type = ComType.SelectedItem as T_SYS_DICTIONARY;
                            if (type != null)
                            {
                                thd = this.GetVehicleTypeValue(type.DICTIONARYVALUE.ToString());
                                if (transportToolStand.Count() > 0)
                                {
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
                                            if (type != null)
                                            {
                                                if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE)
                                                {
                                                    ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                    return;
                                                }
                                                if (thd.TAKETHETOOLLEVEL.ToInt32() > level.DICTIONARYVALUE)
                                                {
                                                    ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComLevel.Foreground = new SolidColorBrush(Colors.Red);
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
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 外出开会控件
        private void myChkBoxMeet_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].GOOUTTOMEET = "1";
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("您已勾选内部会议或培训，无各项补贴！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
            }
        }

        private void myChkBoxMeet_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].GOOUTTOMEET = "0";
                    }
                }
            }
        }
        #endregion

        #region 公司派车控件
        private void myChkBoxCar_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].COMPANYCAR = "1";
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("您已勾选公司派车，无交通补贴！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
            }
        }

        private void myChkBoxCar_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].COMPANYCAR = "0";
                    }
                }
            }
        }
        #endregion

        #region KeyDown事件
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int i = 0;
            if (e.Key == Key.Enter)
            {
                if (TraveDetailList_Golbal.Count() > 0)
                {
                    if (DaGrs.SelectedIndex == TraveDetailList_Golbal.Count - 1)
                    {
                        T_OA_BUSINESSTRIPDETAIL buip = new T_OA_BUSINESSTRIPDETAIL();
                        buip.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();
                        if (TraveDetailList_Golbal.Count() > 0)
                        {
                            foreach (T_OA_BUSINESSTRIPDETAIL tailList in TraveDetailList_Golbal)
                            {
                                tailList.DESTCITY = citysEndList_Golbal[i].Replace(",", "");
                                buip.DEPCITY = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(tailList.DESTCITY);
                                buip.STARTDATE = tailList.ENDDATE;
                            }
                            i++;
                        }
                        buip.ENDDATE = DateTime.Now;
                        TraveDetailList_Golbal.Add(buip);
                    }
                }
            }
        }
        #endregion

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


        #region DataGrid 数据加载、字典数据转换
        private void BindDataGrid(ObservableCollection<T_OA_BUSINESSTRIPDETAIL> obj)
        {
            if (obj == null) return;
            citysStartList_Golbal.Clear();
            citysEndList_Golbal.Clear();
            foreach (T_OA_BUSINESSTRIPDETAIL detail in obj)
            {
                citysStartList_Golbal.Add(detail.DEPCITY);
                citysEndList_Golbal.Add(detail.DESTCITY);
            }
            if (formType != FormTypes.New && formType != FormTypes.Edit && formType != FormTypes.Resubmit)
            {
                DaGridReadOnly.ItemsSource = obj;
            }
            else
            {
                DaGrs.ItemsSource = obj;
            }
        }
        /// <summary>
        /// 审核状态转换
        /// </summary>
        /// <param name="checkStateValue"></param>
        /// <returns></returns>
        private string GetCheckState(string checkStateValue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "CHECKSTATE" && a.DICTIONARYVALUE == Convert.ToDecimal(checkStateValue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 城市值转换
        /// </summary>
        /// <param name="cityvalue"></param>
        /// <returns></returns>
        private string GetCityName(string cityvalue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "CITY" && a.DICTIONARYVALUE == Convert.ToDecimal(cityvalue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 交通工具类型值转换
        /// </summary>
        /// <param name="cityvalue"></param>
        /// <returns></returns>
        private string GetTypeName(string typeValue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "VICHILESTANDARD" && a.DICTIONARYVALUE == Convert.ToDecimal(typeValue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 获取级别对应的类型ID
        /// </summary>
        /// <param name="cityvalue"></param>
        /// <returns></returns>
        private string GetTypeId(string typeValue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "VICHILESTANDARD" && a.DICTIONARYVALUE == Convert.ToDecimal(typeValue)
                           select new
                           {
                               DICTIONARYGUID = a.DICTIONARYID
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYGUID : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 交通工具级别值转换
        /// </summary>
        /// <param name="typevalue"></param>
        /// <returns></returns>
        private string GetLevelName(string levelValue, string typeId)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "VICHILELEVEL" && (a.T_SYS_DICTIONARY2 != null && a.T_SYS_DICTIONARY2.DICTIONARYID == typeId) && a.DICTIONARYVALUE == Convert.ToDecimal(levelValue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 添加子表数据
        /// </summary>
        private void NewDetail()
        {
            try
            {

                ObservableCollection<T_OA_BUSINESSTRIPDETAIL> ListDetail = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
                string StrStartDt = "";   //开始时间
                string StrStartTime = ""; //开始时：分
                string EndDt = "";    //结束时间
                string StrEndTime = ""; //结束时：分
                int i = 0;
                if (DaGrs.ItemsSource != null)
                {
                    foreach (Object obje in DaGrs.ItemsSource)//判断所选的出发城市是否与目标城市相同
                    {
                        TraveDetailOne_Golbal = new T_OA_BUSINESSTRIPDETAIL();
                        TraveDetailOne_Golbal.BUSINESSTRIPDETAILID = (obje as T_OA_BUSINESSTRIPDETAIL).BUSINESSTRIPDETAILID;
                        TraveDetailOne_Golbal.T_OA_BUSINESSTRIP = Master_Golbal;
                        DateTimePicker StartDate = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                        DateTimePicker EndDate = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;
                        TextBox datys = ((TextBox)((StackPanel)DaGrs.Columns[6].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                        CheckBox IsCheck = DaGrs.Columns[7].GetCellContent(obje).FindName("myChkBox") as CheckBox;
                        CheckBox IsCheckMeet = DaGrs.Columns[8].GetCellContent(obje).FindName("myChkBoxMeet") as CheckBox;
                        CheckBox IsCheckCar = DaGrs.Columns[9].GetCellContent(obje).FindName("myChkBoxCar") as CheckBox;
                        TravelDictionaryComboBox ToolType = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[4].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                        TravelDictionaryComboBox ToolLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[5].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;

                        if (StartDate.Value != null)
                        {
                            StrStartDt = StartDate.Value.Value.ToString("d");//开始日期
                            StrStartTime = StartDate.Value.Value.ToString("HH:mm");//开始时间
                            if (EndDate.Value != null)
                            {
                                EndDt = EndDate.Value.Value.ToString("d");//结束日期
                                StrEndTime = EndDate.Value.Value.ToString("HH:mm");//结束时间
                            }
                        }

                        DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
                        DateTime DtEnd = System.Convert.ToDateTime(EndDt + " " + StrEndTime);

                        if (citysStartList_Golbal != null)//出发城市
                        {
                            TraveDetailOne_Golbal.DEPCITY = citysStartList_Golbal[i].Replace(",", "");
                        }
                        if (citysEndList_Golbal != null)//目标城市
                        {
                            TraveDetailOne_Golbal.DESTCITY = citysEndList_Golbal[i].Replace(",", "");
                        }
                        if (DtStart != null)
                        {
                            TraveDetailOne_Golbal.STARTDATE = DtStart;
                        }
                        if (datys != null)//出差天数
                        {
                            TraveDetailOne_Golbal.BUSINESSDAYS = datys.Text;
                        }
                        if (DtEnd != null)
                        {
                            TraveDetailOne_Golbal.ENDDATE = DtEnd;
                        }
                        if (IsCheck != null)//是否是私事
                        {
                            TraveDetailOne_Golbal.PRIVATEAFFAIR = (bool)IsCheck.IsChecked ? "1" : "0";
                        }
                        if (IsCheckMeet != null)//是否是开会
                        {
                            TraveDetailOne_Golbal.GOOUTTOMEET = (bool)IsCheckMeet.IsChecked ? "1" : "0";
                        }
                        if (IsCheckCar != null)//公司派车
                        {
                            TraveDetailOne_Golbal.COMPANYCAR = (bool)IsCheckCar.IsChecked ? "1" : "0";
                        }
                        if (ToolType != null)//乘坐交通工具类型
                        {
                            T_SYS_DICTIONARY ComVechileObj = ToolType.SelectedItem as T_SYS_DICTIONARY;
                            if (ComVechileObj != null)
                                TraveDetailOne_Golbal.TYPEOFTRAVELTOOLS = ComVechileObj.DICTIONARYVALUE.ToString();
                        }
                        if (ToolLevel != null)//乘坐交通工具级别
                        {
                            T_SYS_DICTIONARY ComLevelObj = ToolLevel.SelectedItem as T_SYS_DICTIONARY;
                            if (ComLevelObj != null)
                                TraveDetailOne_Golbal.TAKETHETOOLLEVEL = ComLevelObj.DICTIONARYVALUE.ToString();
                        }
                        ListDetail.Add(TraveDetailOne_Golbal);
                        i++;
                    }
                    TraveDetailList_Golbal = ListDetail;
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion


        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (DaGrs.ItemsSource == null)
            {
                TraveDetailList_Golbal = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
                T_OA_BUSINESSTRIPDETAIL buip = new T_OA_BUSINESSTRIPDETAIL();
                buip.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();
                buip.STARTDATE = DateTime.Now;
                buip.ENDDATE = DateTime.Now;
                TraveDetailList_Golbal.Add(buip);

                T_OA_BUSINESSTRIPDETAIL buipd = new T_OA_BUSINESSTRIPDETAIL();
                buipd.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();
                TraveDetailList_Golbal.Add(buipd);

                DaGrs.ItemsSource = TraveDetailList_Golbal;
                DaGrs.SelectedIndex = 0;
            }
            //if (!string.IsNullOrEmpty(Master_Golbal.BUSINESSTRIPID))
            //{
            //    //ctrFile.Load_fileData(Master_Golbal.BUSINESSTRIPID);
            //}
        }
        #endregion


        #region DaGrs_LoadingRow
        /// <summary>
        /// 默认显示出差时间、地点数据的DataGrid
        /// </summary>
        Brush tempcomTypeBorderBrush;//定义combox默认颜色变量
        Brush tempcomTypeForeBrush;
        Brush tempcomLevelBorderBrush;
        Brush tempcomLevelForeBrush;
        private void DaGrs_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_BUSINESSTRIPDETAIL tmp = (T_OA_BUSINESSTRIPDETAIL)e.Row.DataContext;
            ImageButton MyButton_Delbaodao = DaGrs.Columns[10].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
            SearchCity myCity = DaGrs.Columns[1].GetCellContent(e.Row).FindName("txtDEPARTURECITY") as SearchCity;
            SearchCity myCitys = DaGrs.Columns[3].GetCellContent(e.Row).FindName("txtTARGETCITIES") as SearchCity;
            CheckBox IsCheck = DaGrs.Columns[7].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            CheckBox IsCheckMeet = DaGrs.Columns[8].GetCellContent(e.Row).FindName("myChkBoxMeet") as CheckBox;
            CheckBox IsCheckCar = DaGrs.Columns[9].GetCellContent(e.Row).FindName("myChkBoxCar") as CheckBox;
            TravelDictionaryComboBox ComVechile = DaGrs.Columns[4].GetCellContent(e.Row).FindName("ComVechileType") as TravelDictionaryComboBox;
            TravelDictionaryComboBox ComLevel = DaGrs.Columns[5].GetCellContent(e.Row).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
            if (BtnNewButton == true)
            {
                myCitys.TxtSelectedCity.Text = string.Empty;
                citysStartList_Golbal.Add(tmp.DEPCITY);
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
            ComVechile.SelectedIndex = 0;
            ComLevel.SelectedIndex = 0;
            //对默认颜色进行赋值
            tempcomTypeBorderBrush = ComVechile.BorderBrush;
            tempcomTypeForeBrush = ComVechile.Foreground;
            tempcomLevelBorderBrush = ComLevel.BorderBrush;
            tempcomLevelForeBrush = ComLevel.Foreground;

            ObservableCollection<T_OA_BUSINESSTRIPDETAIL> objs = DaGrs.ItemsSource as ObservableCollection<T_OA_BUSINESSTRIPDETAIL>;
            if (formType != FormTypes.New)
            {
                int j = 0;
                if (DaGrs.ItemsSource != null && TraveDetailList_Golbal != null)
                {
                    foreach (var obje in objs)
                    {
                        j++;
                        if (obje.BUSINESSTRIPDETAILID == tmp.BUSINESSTRIPDETAILID)//判断记录的ID是否相同
                        {
                            if (myCity != null)//出发城市
                            {
                                if (obje.DEPCITY != null)
                                {
                                    myCity.TxtSelectedCity.Text = GetCityName(obje.DEPCITY);
                                    if (TraveDetailList_Golbal.Count() > 1)
                                    {
                                        if (j > 1)
                                        {
                                            //By  : Sam
                                            //Date: 2011-9-6
                                            //For : 此处之前设置双击的时候还会出现打开状态
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
                        }
                        else
                        {
                            continue;
                        }

                        string dictid = "";
                        //获取交通工具类型、级别
                        if (ComVechile != null)
                        {
                            T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                            T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                            type = ComVechile.SelectedItem as T_SYS_DICTIONARY;
                            level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                            var thd = transportToolStand.FirstOrDefault();
                            if (type != null)
                            {
                                thd = this.GetVehicleTypeValue(type.DICTIONARYVALUE.ToString());
                                foreach (T_SYS_DICTIONARY Region in ComVechile.Items)
                                {
                                    if (thd != null)
                                    {
                                        dictid = Region.DICTIONARYID;
                                        if (Region.DICTIONARYVALUE.ToString() == tmp.TYPEOFTRAVELTOOLS)
                                        {
                                            if (transportToolStand.Count() > 0)
                                            {
                                                ComVechile.SelectedItem = Region;
                                                if (thd.TYPEOFTRAVELTOOLS.ToInt32() > Region.DICTIONARYVALUE)
                                                {
                                                    ComVechile.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                                    ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComLevel.Foreground = new SolidColorBrush(Colors.Red);
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
                        }

                        if (ComLevel != null)
                        {
                            var ents = from ent in ListVechileLevel
                                       where ent.T_SYS_DICTIONARY2.DICTIONARYID == dictid
                                       select ent;
                            ComLevel.ItemsSource = ents.ToList();
                            if (ents.Count() > 0)
                            {
                                T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                                T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                                type = ComVechile.SelectedItem as T_SYS_DICTIONARY;
                                level = ComLevel.SelectedItem as T_SYS_DICTIONARY;

                                var thd = transportToolStand.FirstOrDefault();
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
                    }
                }
            }
        }
        #endregion

    }
}
