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
        #region 上传附件
        private void UploadFiles()
        {
            //System.Windows.Controls.OpenFileDialog openFileWindow = new OpenFileDialog();
            //openFileWindow.Multiselect = true;
            //if (openFileWindow.ShowDialog() == true)
            //    foreach (FileInfo file in openFileWindow.Files)
            //        ctrFile.InitFiles(file.Name, file.OpenRead());
        }
        //void ctrFile_Event_AllFilesFinished(object sender, SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs e)
        //{
        //    //RefreshUI(RefreshedTypes.HideProgressBar);
        //}
        #endregion

        #region 费用控件
        private void HiddenFBControl()
        {
            if (scvFB.Visibility == Visibility.Visible)
            {
                scvFB.Visibility = Visibility.Collapsed;
                RefreshUI(RefreshedTypes.All);
            }
            else
            {
                scvFB.Visibility = Visibility.Visible;
                RefreshUI(RefreshedTypes.All);
            }
        }

        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;
            fbCtr.OrderType = FrameworkUI.FBControls.ChargeApplyControl.OrderTypes.Travel;
            if (actions == FormTypes.New)
            {
                fbCtr.Order.ORDERID = "";
                fbCtr.strExtOrderModelCode = "CCSQ";
            }
            else
            {
                fbCtr.Order.ORDERID = Master_Golbal.BUSINESSTRIPID;//费用对象
                fbCtr.strExtOrderModelCode = "CCSQ";
            }
            fbCtr.Order.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.CREATECOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.CREATEDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.CREATEPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            fbCtr.Order.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.OWNERCOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.OWNERDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.OWNERPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;


            fbCtr.Order.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            fbCtr.InitDataComplete += (o, e) =>
            {
                Binding bding = new Binding();
                bding.Path = new PropertyPath("TOTALMONEY");
                this.txtFee.SetBinding(TextBox.TextProperty, bding);
                this.txtFee.DataContext = fbCtr.Order;
                if (actions == FormTypes.Browse || actions == FormTypes.Audit)
                {
                    fbCtr.strExtOrderModelCode = "CCSQ";
                    if (fbCtr.ListDetail.Count() <= 0)
                    {
                        fbCtr.Visibility = Visibility.Collapsed;//查看或审核时如果没有费用就隐藏
                    }
                }
            };
            if (actions == FormTypes.Audit || actions == FormTypes.Browse)
            {
                fbCtr.strExtOrderModelCode = "CCSQ";
                fbCtr.InitData(false);
                fbCtr.GetParamenter();
            }
            else
            {
                fbCtr.InitData();
            }
        }
        #endregion

        #region 隐藏附件控件
        public void FileLoadedCompleted()
        {
            //if (!ctrFile._files.HasAccessory)
            //{
            //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayoutRoot, 6);
            //    this.lblFile.Visibility = Visibility.Collapsed;
            //}
        }
        #endregion

        private void AddAgent(int i)
        {
            AgentDateSet = new T_OA_AGENTDATESET();
            AgentDateSet.AGENTDATESETID = Master_Golbal.BUSINESSTRIPID;//出差ID
            AgentDateSet.USERID = Master_Golbal.OWNERID;//出差人ID
            AgentDateSet.MODELCODE = "T_OA_BUSINESSTRIP";//模块代码
            AgentDateSet.EFFECTIVEDATE = Convert.ToDateTime(TraveDetailList_Golbal[0].STARTDATE).Date;//生效时间
            AgentDateSet.PLANEXPIRATIONDATE = Convert.ToDateTime(TraveDetailList_Golbal[i].ENDDATE).Date;//计划失效时间
            AgentDateSet.OWNERID = Master_Golbal.OWNERID;//所属人ID
            AgentDateSet.OWNERNAME = Master_Golbal.OWNERNAME;//所属用户名称
            AgentDateSet.OWNERPOSTID = Master_Golbal.OWNERPOSTID;//所属岗位ID
            AgentDateSet.OWNERDEPARTMENTID = Master_Golbal.OWNERDEPARTMENTID;//所属部门ID
            AgentDateSet.OWNERCOMPANYID = Master_Golbal.OWNERCOMPANYID;//所属公司ID
            AgentDateSet.CREATEUSERID = Master_Golbal.CREATEUSERID;//创建人ID
            AgentDateSet.CREATEUSERNAME = Master_Golbal.CREATEUSERNAME;//创建人姓名
            AgentDateSet.CREATEPOSTID = Master_Golbal.OWNERPOSTID;//创建岗位ID
            AgentDateSet.CREATEDEPARTMENTID = Master_Golbal.OWNERDEPARTMENTID;//创建部门ID
            AgentDateSet.CREATECOMPANYID = Master_Golbal.OWNERCOMPANYID;//创建公司ID
            AgentDateSet.CREATEDATE = Convert.ToDateTime(Master_Golbal.UPDATEDATE);
        }


        /// <summary>
        /// 检查代理是否存在
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SoaChannel_IsExistAgentCompleted(object sender, IsExistAgentCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                int i = e.Result.ToInt32();
                if (i > 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "已经设置了代理,请不要重复设置!", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    ckEnabled.IsChecked = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        #region 审核时候显示的视图

        #region DaGrss_LoadingRow
        /// <summary>
        /// 查看或审核时用(不显示DataGrid模板中的控件只显示数据)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DaGrss_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_BUSINESSTRIPDETAIL tmp = (T_OA_BUSINESSTRIPDETAIL)e.Row.DataContext;
            TextBlock TxtMyCity = DaGridReadOnly.Columns[1].GetCellContent(e.Row).FindName("tbDEPARTURECITY") as TextBlock;
            TextBlock TxtMyCitys = DaGridReadOnly.Columns[3].GetCellContent(e.Row).FindName("tbTARGETCITIES") as TextBlock;
            CheckBox IsCheck = DaGridReadOnly.Columns[7].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            CheckBox IsCheckMeet = DaGridReadOnly.Columns[8].GetCellContent(e.Row).FindName("myChkBoxMeet") as CheckBox;
            CheckBox IsCheckCar = DaGridReadOnly.Columns[9].GetCellContent(e.Row).FindName("myChkBoxCar") as CheckBox;
            TextBlock TxtComVechile = DaGridReadOnly.Columns[4].GetCellContent(e.Row).FindName("tbComVechileType") as TextBlock;
            TextBlock TxtComLevel = DaGridReadOnly.Columns[5].GetCellContent(e.Row).FindName("tbComVechileTypeLeve") as TextBlock;

            Brush tempcomTypeForeBrush;
            Brush tempcomLevelForeBrush;

            if (BtnNewButton == true)
            {
                TxtMyCitys.Text = string.Empty;
                citysStartList_Golbal.Add(tmp.DEPCITY);
            }
            else
            {
                BtnNewButton = false;
            }
            TxtMyCity.Tag = tmp;

            TxtMyCitys.Tag = tmp;

            //对默认颜色进行赋值
            tempcomTypeForeBrush = TxtComVechile.Foreground;
            tempcomLevelForeBrush = TxtComLevel.Foreground;

            ObservableCollection<T_OA_BUSINESSTRIPDETAIL> objs = DaGridReadOnly.ItemsSource as ObservableCollection<T_OA_BUSINESSTRIPDETAIL>;
            if (actions != FormTypes.New)
            {
                if (DaGridReadOnly.ItemsSource != null && TraveDetailList_Golbal != null)
                {
                    foreach (var TraveDetailRow in objs)
                    {
                        if (TraveDetailRow.BUSINESSTRIPDETAILID == tmp.BUSINESSTRIPDETAILID)//判断记录的ID是否相同
                        {
                            if (TxtMyCity != null)//出发城市
                            {
                                if (TraveDetailRow.DEPCITY != null)
                                {
                                    TxtMyCity.Text = GetCityName(TraveDetailRow.DEPCITY);
                                }
                            }
                            if (TxtMyCitys != null)//目标城市
                            {
                                if (TraveDetailRow.DESTCITY != null)
                                {
                                    TxtMyCitys.Text = GetCityName(TraveDetailRow.DESTCITY);
                                }
                            }
                            if (TraveDetailRow.TYPEOFTRAVELTOOLS != null)//交通工具类型
                            {
                                TxtComVechile.Text = GetTypeName(TraveDetailRow.TYPEOFTRAVELTOOLS);
                            }
                            if (TraveDetailRow.TAKETHETOOLLEVEL != null)//交通工具级别
                            {
                                TxtComLevel.Text = GetLevelName(TraveDetailRow.TAKETHETOOLLEVEL, GetTypeId(TraveDetailRow.TYPEOFTRAVELTOOLS));
                            }
                            if (TraveDetailRow.PRIVATEAFFAIR == "1")//私事
                            {
                                IsCheck.IsChecked = true;
                            }
                            if (TraveDetailRow.GOOUTTOMEET == "1")//外出开会
                            {
                                IsCheckMeet.IsChecked = true;
                            }
                            if (TraveDetailRow.COMPANYCAR == "1")//公司派车
                            {
                                IsCheckCar.IsChecked = true;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        if (TxtComVechile != null)
                        {
                            if (TraveDetailRow.TYPEOFTRAVELTOOLS == null)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("交通工具有误，请审核不通过"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                TxtComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                continue;
                            }
                            if (TraveDetailRow.TAKETHETOOLLEVEL == null)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("交通工具级别有误，请审核不通过"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                TxtComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                continue;
                            }
                            //获取交通工具类型
                            int i = CheckTraveToolStand(TraveDetailRow.TYPEOFTRAVELTOOLS.ToString(), TraveDetailRow.TAKETHETOOLLEVEL.ToString(), postLevel);
                            switch (i)
                            {
                                case 0://类型超标
                                    TxtComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                    TxtComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                    break;
                                case 1://级别超标
                                    TxtComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                    break;
                                case 2://没超标
                                    break;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #endregion

        #region 启用代理按钮ckEnabled_Click
        private void ckEnabled_Click(object sender, RoutedEventArgs e)
        {
            OaCommonOfficeClient.IsExistAgentAsync("T_OA_BUSINESSTRIP", employeepost.EMPLOYEEPOSTS[0].CompanyID);
        }
        #endregion

        //废弃代码
        #region IForm 成员

        public void ClosedWCFClient()
        {
            OaPersonOfficeClient.DoClose();
            HrPersonnelclient.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 将审核通过的出差数据写入HR系统
        //private ObservableCollection<Saas.Tools.AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD> GetEmployeeEvectionRdList()
        //{
        //    try
        //    {
        //        ObservableCollection<Saas.Tools.AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD> entResList = new ObservableCollection<Saas.Tools.AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD>();

        //        if (Businesstrip == null || TraveDetailList == null)
        //        {
        //            return entResList;
        //        }

        //        if (Businesstrip.CHECKSTATE != "2" || TraveDetailList.Count() == 0)
        //        {
        //            return entResList;
        //        }

        //        if (TraveDetailList.Count() == 1)//如果只有一条数据
        //        {
        //            Saas.Tools.AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD entRes = new Saas.Tools.AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD();
        //            entRes.EVECTIONRECORDID = System.Guid.NewGuid().ToString();
        //            entRes.CHECKSTATE = Businesstrip.CHECKSTATE;
        //            entRes.EMPLOYEENAME = Businesstrip.OWNERNAME;//出差人
        //            entRes.EVECTIONREASON = Businesstrip.CONTENT;//出差事由
        //            entRes.EMPLOYEEID = Businesstrip.OWNERID;//出差人ID
        //            entRes.CHECKSTATE = Businesstrip.CHECKSTATE;//审批状态
        //            entRes.OWNERID = Businesstrip.OWNERID;
        //            entRes.OWNERPOSTID = Businesstrip.OWNERPOSTID;
        //            entRes.OWNERDEPARTMENTID = Businesstrip.OWNERDEPARTMENTID;
        //            entRes.OWNERCOMPANYID = Businesstrip.OWNERCOMPANYID;
        //            entRes.CREATEUSERID = Businesstrip.CREATEUSERID;
        //            if (TraveDetailList[0].PRIVATEAFFAIR == "1")//如果是私事
        //            {
        //                entRes.STARTDATE = null;
        //                entRes.ENDDATE = null;
        //                entRes.STARTTIME = string.Empty;
        //                entRes.ENDTIME = string.Empty;
        //                entRes.TOTALDAYS = null;
        //                entRes.DESTINATION = string.Empty;
        //            }
        //            else
        //            {
        //                entRes.STARTDATE = Convert.ToDateTime(TraveDetailList[0].STARTDATE).Date;//开始日期
        //                entRes.ENDDATE = Convert.ToDateTime(TraveDetailList[0].ENDDATE).Date;//结束日期
        //                entRes.STARTTIME = Convert.ToDateTime(TraveDetailList[0].STARTDATE).ToShortTimeString();//开始时间
        //                entRes.ENDTIME = Convert.ToDateTime(TraveDetailList[0].ENDDATE).ToShortTimeString();//结束时间

        //                //计算时间
        //                double TotalDays = 0;//出差天数
        //                int TotalHours = 0;//出差小时
        //                TimeSpan tsStart = new TimeSpan(Convert.ToDateTime(TraveDetailList[0].STARTDATE).Ticks);
        //                TimeSpan tsEnd = new TimeSpan(Convert.ToDateTime(TraveDetailList[0].ENDDATE).Ticks);
        //                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

        //                TotalDays = ts.Days;
        //                TotalHours = ts.Hours;

        //                if (TotalHours > 0)
        //                {
        //                    TotalDays += 0.5;
        //                }
        //                entRes.TOTALDAYS = Convert.ToDecimal(TotalDays);
        //                //entRes.TOTALDAYS = Convert.ToInt32((buipList[0].ENDDATE - buipList[0].STARTDATE).Days);//出差天数
        //            }
        //            entResList.Add(entRes);

        //        }
        //        if (TraveDetailList.Count() == 2)//如果只有两条数据
        //        {
        //            Saas.Tools.AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD entRes = new Saas.Tools.AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD();
        //            entRes.EVECTIONRECORDID = System.Guid.NewGuid().ToString();
        //            entRes.CHECKSTATE = Businesstrip.CHECKSTATE;
        //            entRes.EMPLOYEENAME = Businesstrip.OWNERNAME;//出差人
        //            entRes.EVECTIONREASON = Businesstrip.CONTENT;//出差事由
        //            entRes.EMPLOYEEID = Businesstrip.OWNERID;//出差人ID
        //            entRes.CHECKSTATE = Businesstrip.CHECKSTATE;//审批状态
        //            entRes.OWNERID = Businesstrip.OWNERID;
        //            entRes.OWNERPOSTID = Businesstrip.OWNERPOSTID;
        //            entRes.OWNERDEPARTMENTID = Businesstrip.OWNERDEPARTMENTID;
        //            entRes.OWNERCOMPANYID = Businesstrip.OWNERCOMPANYID;
        //            entRes.CREATEUSERID = Businesstrip.CREATEUSERID;
        //            if (TraveDetailList[0].PRIVATEAFFAIR == "1")
        //            {
        //                entRes.STARTDATE = null;
        //                entRes.ENDDATE = null;
        //                entRes.STARTTIME = string.Empty;
        //                entRes.ENDTIME = string.Empty;
        //                entRes.TOTALDAYS = null;
        //                entRes.DESTINATION = string.Empty;
        //            }
        //            else
        //            {
        //                entRes.STARTDATE = Convert.ToDateTime(TraveDetailList[0].STARTDATE).Date;//开始日期
        //                entRes.ENDDATE = Convert.ToDateTime(TraveDetailList[1].ENDDATE).Date;//结束日期
        //                entRes.STARTTIME = Convert.ToDateTime(TraveDetailList[0].STARTDATE).ToShortTimeString();//开始时间
        //                entRes.ENDTIME = Convert.ToDateTime(TraveDetailList[1].ENDDATE).ToShortTimeString();//结束时间

        //                //计算时间
        //                double TotalDays = 0;//出差天数
        //                int TotalHours = 0;//出差小时
        //                TimeSpan tsStart = new TimeSpan(Convert.ToDateTime(TraveDetailList[0].STARTDATE).Ticks);
        //                TimeSpan tsEnd = new TimeSpan(Convert.ToDateTime(TraveDetailList[1].ENDDATE).Ticks);
        //                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

        //                TotalDays = ts.Days;
        //                TotalHours = ts.Hours;

        //                if (TotalHours > 0)
        //                {
        //                    TotalDays += 0.5;
        //                }
        //                entRes.TOTALDAYS = Convert.ToDecimal(TotalDays);
        //                //entRes.TOTALDAYS = Convert.ToInt32((buipList[1].ENDDATE - buipList[0].STARTDATE).Days);//出差天数
        //            }
        //            entResList.Add(entRes);
        //        }
        //        #region 存在多条的处理
        //        if (TraveDetailList.Count() > 2)
        //        {
        //            for (int i = 0; i < TraveDetailList.Count(); i++)
        //            {
        //                Saas.Tools.AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD entRes = new Saas.Tools.AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD();
        //                entRes.EVECTIONRECORDID = System.Guid.NewGuid().ToString();
        //                entRes.CHECKSTATE = Businesstrip.CHECKSTATE;
        //                entRes.EMPLOYEENAME = Businesstrip.OWNERNAME;//出差人
        //                entRes.EVECTIONREASON = Businesstrip.CONTENT;//出差事由
        //                entRes.EMPLOYEEID = Businesstrip.OWNERID;//出差人ID
        //                entRes.CHECKSTATE = Businesstrip.CHECKSTATE;//审批状态
        //                entRes.OWNERID = Businesstrip.OWNERID;
        //                entRes.OWNERPOSTID = Businesstrip.OWNERPOSTID;
        //                entRes.OWNERDEPARTMENTID = Businesstrip.OWNERDEPARTMENTID;
        //                entRes.OWNERCOMPANYID = Businesstrip.OWNERCOMPANYID;
        //                entRes.CREATEUSERID = Businesstrip.CREATEUSERID;
        //                if (i > 0)
        //                {
        //                    //如果是最后一条记录
        //                    #region 最后一条记录处理

        //                    if (i == TraveDetailList.Count() - 1)
        //                    {
        //                        if (TraveDetailList[i - 1].PRIVATEAFFAIR == "1")//如果是私事
        //                        {
        //                            int lastCount = TraveDetailList.Count();
        //                            int dgcount1 = 0;
        //                            foreach (T_OA_BUSINESSTRIPDETAIL entDetail in TraveDetailList)
        //                            {
        //                                dgcount1 += 1;
        //                                if (dgcount1 == lastCount)
        //                                {
        //                                    entRes.STARTDATE = Convert.ToDateTime(TraveDetailList[i].STARTDATE).Date;//开始日期
        //                                    entRes.ENDDATE = Convert.ToDateTime(TraveDetailList[i].ENDDATE).Date;//结束日期
        //                                    entRes.STARTTIME = Convert.ToDateTime(TraveDetailList[i].STARTDATE).ToShortTimeString();//开始时间
        //                                    entRes.ENDTIME = Convert.ToDateTime(TraveDetailList[i].ENDDATE).ToShortTimeString();//结束时间

        //                                    //计算时间
        //                                    double TotalDays = 0;//出差天数
        //                                    int TotalHours = 0;//出差小时
        //                                    TimeSpan tsStart = new TimeSpan(Convert.ToDateTime(TraveDetailList[i].STARTDATE).Ticks);
        //                                    TimeSpan tsEnd = new TimeSpan(Convert.ToDateTime(TraveDetailList[i].ENDDATE).Ticks);
        //                                    TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

        //                                    TotalDays = ts.Days;
        //                                    TotalHours = ts.Hours;

        //                                    if (TotalHours > 0)
        //                                    {
        //                                        TotalDays += 0.5;
        //                                    }
        //                                    entRes.TOTALDAYS = Convert.ToDecimal(TotalDays);
        //                                    //entRes.TOTALDAYS = Convert.ToInt32((buipList[i].ENDDATE - buipList[i].STARTDATE).Days);//出差天数
        //                                }
        //                                if (dgcount1 == lastCount - 1)
        //                                {
        //                                    entRes.STARTDATE = null;
        //                                    entRes.ENDDATE = null;
        //                                    entRes.STARTTIME = string.Empty;
        //                                    entRes.ENDTIME = string.Empty;
        //                                    entRes.TOTALDAYS = null;
        //                                    entRes.DESTINATION = string.Empty;
        //                                }
        //                            }
        //                        }
        //                        else //不是私事的处理
        //                        {
        //                            entRes.STARTDATE = Convert.ToDateTime(TraveDetailList[i - 1].STARTDATE).Date;//开始日期
        //                            entRes.ENDDATE = Convert.ToDateTime(TraveDetailList[i].ENDDATE).Date;//结束日期
        //                            entRes.STARTTIME = Convert.ToDateTime(TraveDetailList[i - 1].STARTDATE).ToShortTimeString();//开始时间
        //                            entRes.ENDTIME = Convert.ToDateTime(TraveDetailList[i].ENDDATE).ToShortTimeString();//结束时间

        //                            //计算时间
        //                            double TotalDays = 0;//出差天数
        //                            int TotalHours = 0;//出差小时
        //                            TimeSpan tsStart = new TimeSpan(Convert.ToDateTime(TraveDetailList[i - 1].STARTDATE).Ticks);
        //                            TimeSpan tsEnd = new TimeSpan(Convert.ToDateTime(TraveDetailList[i].ENDDATE).Ticks);
        //                            TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

        //                            TotalDays = ts.Days;
        //                            TotalHours = ts.Hours;

        //                            if (TotalHours > 0)
        //                            {
        //                                TotalDays += 0.5;
        //                            }
        //                            entRes.TOTALDAYS = Convert.ToDecimal(TotalDays);
        //                            //entRes.TOTALDAYS = Convert.ToInt32((buipList[i].ENDDATE - buipList[i - 1].STARTDATE).Days);//出差天数
        //                        }
        //                        entResList.Add(entRes);
        //                    }
        //                    #endregion
        //                    else
        //                    {
        //                        if (i > 0)
        //                        {
        //                            entRes.ENDDATE = Convert.ToDateTime(TraveDetailList[i].STARTDATE).Date;//开始日期
        //                            entRes.STARTDATE = Convert.ToDateTime(TraveDetailList[i - 1].STARTDATE).Date;//结束日期
        //                            entRes.ENDTIME = Convert.ToDateTime(TraveDetailList[i].STARTDATE).ToShortTimeString();//开始时间
        //                            entRes.STARTTIME = Convert.ToDateTime(TraveDetailList[i - 1].STARTDATE).ToShortTimeString();//结束时间

        //                            //计算时间
        //                            double TotalDays = 0;//出差天数
        //                            int TotalHours = 0;//出差小时
        //                            TimeSpan tsStart = new TimeSpan(Convert.ToDateTime(TraveDetailList[i - 1].STARTDATE).Ticks);
        //                            TimeSpan tsEnd = new TimeSpan(Convert.ToDateTime(TraveDetailList[i].STARTDATE).Ticks);
        //                            TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

        //                            TotalDays = ts.Days;
        //                            TotalHours = ts.Hours;

        //                            if (TotalHours > 0)
        //                            {
        //                                TotalDays += 0.5;
        //                            }
        //                            entRes.TOTALDAYS = Convert.ToDecimal(TotalDays);
        //                            //entRes.TOTALDAYS = Convert.ToInt32((buipList[i].STARTDATE - buipList[i - 1].STARTDATE).Days);//出差天数
        //                        }
        //                        entResList.Add(entRes);
        //                    }
        //                }
        //            }
        //        }
        //        return entResList;
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //        return null;
        //    }
        //}
        #endregion

        #region DaGrs_SelectionChanged
        private void DaGrs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        #endregion

        #region 开始时间ValueChanged事件
        private void StartTime_ValueChanged(object sender, EventArgs e)
        {
            //DateTimePicker txt = (DateTimePicker)sender;
            //if (buipList.Count() > 0)
            //{
            //    if (buipList.Count() == 1)
            //    {
            //        txt.Value = null;
            //    }
            //}
        }
        #endregion

        #region 结束时间ValueChanged事件
        private void EndTime_ValueChanged(object sender, EventArgs e)
        {
            //DateTimePicker txt = (DateTimePicker)sender; 
            //endTime.Add(txt.Value.ToString());
            //txtEndTime = txt;
            //if (buipList.Count() >0)
            //{
            //    if (buipList.Count() == 1)
            //    {
            //        txt.Value = null;
            //    }
            //}
            //BtnNewButton = true;
            //int i = 0;
            //T_OA_BUSINESSTRIPDETAIL buip = new T_OA_BUSINESSTRIPDETAIL();
            //buip.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();

            //if (actions == FormTypes.New)
            //{
            //    if (buipList.Count() > 0)
            //    {
            //        foreach (T_OA_BUSINESSTRIPDETAIL obje in DaGrs.ItemsSource)
            //        {
            //            if (DaGrs.Columns[0].GetCellContent(obje) != null)
            //            {
            //                DateTimePicker myDaysTime = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
            //                myDaysTime.Value = buipList[0].ENDDATE;
            //            }
            //            txt.Value = null;
            //            i++;
            //        }
            //    }
            //}

            //if (actions != FormTypes.New)
            //{
            //    if (buipList.Count() > 0)
            //    {
            //        foreach (T_OA_BUSINESSTRIPDETAIL tailList in buipList)
            //        {
            //            buip.STARTDATE = tailList.ENDDATE;
            //            i++;
            //        }
            //    }
            //    buipList.Add(buip);
            //    DaGrs.ItemsSource = buipList;
            //}
        }
        #endregion

        #region "旧的判断出差城市"
        //List<string> checkCity = new List<string>();
        //string cityCheck = string.Empty;
        //SearchCity cityStar = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
        //SearchCity cityEnd = DaGrs.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;
        //T_OA_BUSINESSTRIPDETAIL temp = (T_OA_BUSINESSTRIPDETAIL)obje;
        //citysStartList.Add(temp.DEPCITY);
        //citysEndList.Add(temp.DESTCITY);
        //foreachCount++;

        //TravelDictionaryComboBox ComVechile = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[4].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
        //if (string.IsNullOrEmpty(cityStar.TxtSelectedCity.Text))//判断出发城市
        //{
        //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //    return false;
        //}
        //else
        //{
        //    cityCheck = cityStar.TxtSelectedCity.Text.Trim();
        //}
        //checkCity.Add(cityCheck);
        //if (string.IsNullOrEmpty(cityEnd.TxtSelectedCity.Text))//判断目标城市
        //{
        //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "ARRIVALCITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //    return false;
        //}

        //从第2条记录开始检测出发城市是否重复
        //if (foreachCount > 1)
        //{
        //    if (cityCheck == checkCity[0])
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("THESAMECANNOTAPPEAR", "INITIALDEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //        return false;
        //    }
        //}
        //if (cityStar.TxtSelectedCity.Text != null)
        //{
        //    if (cityStar.TxtSelectedCity.Text == cityEnd.TxtSelectedCity.Text)//出发城市不能与目标城市相同
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTTHESAMEASTHETARGECITY", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //        return false;
        //    }
        //}
        //if (cityEnd.TxtSelectedCity.Text != null)
        //{
        //    if (cityEnd.TxtSelectedCity.Text == cityStar.TxtSelectedCity.Text)//目标城市不能与出发城市相同
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTTHESAMEASTHETARGECITY", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //        return false;
        //    }
        //}
        #endregion
    }
}
