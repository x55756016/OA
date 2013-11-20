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

        #region InitFBControl
        private void InitFBControl(T_OA_TRAVELREIMBURSEMENT Travel)
        {
            if(OpenFrom=="FromMVC")return;//从mvc打卡，不使用预算科目
            InitFB = true;
            fbCtr.submitFBFormTypes = formType;//将FormType赋给FB
            //fbCtr.SetRemarkVisiblity(Visibility.Collapsed);//隐藏预算控件中的备注
            fbCtr.SetApplyTypeVisiblity(Visibility.Collapsed);//隐藏支付类型
            fbCtr.TravelSubject = new FrameworkUI.FBControls.TravelSubject();
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.ChargeApply;//费用报销

            if (formType == FormTypes.New)
            {
                fbCtr.Order.ORDERID = "";
                fbCtr.strExtOrderModelCode = "CCBX";
            }
            else
            {
                fbCtr.Order.ORDERID = TravelReimbursement_Golbal.TRAVELREIMBURSEMENTID;//费用对象
                fbCtr.strExtOrderModelCode = "CCBX";
            }
            fbCtr.Order.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.CREATECOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.CREATEDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.CREATEPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            if (Travel != null)//新增获取出差报告的时候用的是报告人的信息
            {
                fbCtr.Order.OWNERCOMPANYID = Travel.OWNERCOMPANYID;//出差人所属公司ID
                fbCtr.Order.OWNERCOMPANYNAME = Travel.OWNERCOMPANYNAME;//出差人所属公司名称
                fbCtr.Order.OWNERDEPARTMENTID = Travel.OWNERDEPARTMENTID;//出差人所属部门ID
                fbCtr.Order.OWNERDEPARTMENTNAME = Travel.OWNERDEPARTMENTNAME;//出差人所属部门名称
                fbCtr.Order.OWNERPOSTID = Travel.OWNERPOSTID;//出差人所属工岗位ID
                fbCtr.Order.OWNERPOSTNAME = Travel.OWNERPOSTNAME;//出差人所属岗位名称
                fbCtr.Order.OWNERID = Travel.OWNERID;//出差人ID
                fbCtr.Order.OWNERNAME = Travel.OWNERNAME;//出差人姓名
            }
            else//修改、查看、审核的时候获取的是报销人的信息
            {
                MessageBox.Show("没有获取到出差报销人相关组织架构信息，请联系管理员！");
                return;
            }
            fbCtr.Order.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            if (formType == FormTypes.Audit || formType == FormTypes.Browse)
            {
                fbCtr.strExtOrderModelCode = "CCBX";
                fbCtr.InitData(false);
            }
            else
            {
                if (formType == FormTypes.New && Travel != null)
                {
                    fbCtr.InitData();
                }
                else if (formType == FormTypes.Edit || formType == FormTypes.Resubmit && Travel != null)
                {
                    fbCtr.InitData();
                }
            }
        }

        void fbCtr_InitDataComplete(object sender, FrameworkUI.FBControls.ChargeApplyControl.InitDataCompletedArgs e)
        {
            if (OpenFrom == "FromMVC") return;
            if (e.Message != null && e.Message.Count() > 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Message[0]);
                DaGrEdit.IsEnabled = false;
                fbCtr.IsEnabled = false;
                if (needsubmit == false)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
            Binding bding = new Binding();
            bding.Path = new PropertyPath("TOTALMONEY");
            if (fbCtr.ListDetail.Count() > 0)
            {
                this.txtChargeApplyTotal.SetBinding(TextBox.TextProperty, bding);//报销费用总额
                this.txtChargeApplyTotal.DataContext = fbCtr.Order;
            }
            this.txtAvailableCredit.Text = fbCtr.TravelSubject.UsableMoney.ToString();//当前用户可用额度
            if (fbCtr.Order.PAYMENTINFO != null && !string.IsNullOrEmpty(fbCtr.Order.PAYMENTINFO))
            {
                this.txtPAYMENTINFO.Text = fbCtr.Order.PAYMENTINFO;//支付信息
                StrPayInfo = this.txtPAYMENTINFO.Text;
            }
            UsableMoney = txtAvailableCredit.Text;
            if (formType == FormTypes.Browse || formType == FormTypes.Audit)
            {
                fbCtr.Visibility = Visibility.Collapsed;
                lblFees.Visibility = Visibility.Collapsed;
                fbChkBox.Visibility = Visibility.Collapsed;

                fbCtr.strExtOrderModelCode = "CCBX";
                //费用报销
                if (fbCtr.ListDetail.Count() > 0)
                {
                    fbCtr.Visibility = Visibility.Visible;
                    scvFB.Visibility = Visibility.Visible;
                    fbChkBox.IsChecked = true;
                }
                //冲借款
                if (fbCtr.ListBorrowDetail.Count() > 0)
                {
                    var q = (from ent in fbCtr.ListBorrowDetail
                             select ent.REPAYMONEY).Sum();
                    if (q > 0)
                    {
                        fbCtr.Visibility = Visibility.Visible;
                        scvFB.Visibility = Visibility.Visible;
                        fbChkBox.IsChecked = true;
                    }
                }
            }
            if (formType == FormTypes.Edit)
            {
                scvFB.Visibility = Visibility.Visible;
                fbChkBox.IsChecked = true;
            }
        }
        #endregion


        #region IEntityEditor 成员

        public string GetTitle()
        {
            if (formType == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "TRAVELREIMBURSEMENTPAGE");
            }
            else if (formType == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "TRAVELREIMBURSEMENTPAGE");
            }
            else if (formType == FormTypes.Audit)
            {
                return Utility.GetResourceStr("AUDIT1", "TRAVELREIMBURSEMENTPAGE");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "TRAVELREIMBURSEMENTPAGE");
            }
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                //case "1":
                //    refreshType = RefreshedTypes.CloseAndReloadData;
                //    Save();
                //    break;
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (formType != FormTypes.Browse && formType != FormTypes.Audit)
            {
                //ToolbarItem item = new ToolbarItem
                //{
                //    DisplayType = ToolbarItemDisplayTypes.Image,
                //    Key = "1",
                //    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                //};
                //items.Add(item);

                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("SAVE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
                };
                items.Add(item);
            }
            return items;
        }
        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion


        #region IForm 成员

        public void ClosedWCFClient()
        {
            OaPersonOfficeClient.DoClose();
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

        #region 字典转换
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
        #endregion

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

        #region 获取DataGrid中的各项费用控件
        /// <summary>
        /// 获取DataGrid交通费
        /// </summary>
        /// <param name="txtTranSportcosts"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetTranSportcostsTextBox(TextBox txtTranSportcosts, int i,bool FromReadOnlyDataGrid)
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
            if (dataGrid.Columns[8].GetCellContent(TravelDetailList_Golbal[i - 1]) != null)
            {
                txtTranSportcosts = dataGrid.Columns[8].GetCellContent(TravelDetailList_Golbal[i - 1]).FindName("txtTRANSPORTCOSTS") as TextBox;//交通费
            }
            return txtTranSportcosts;
        }
        /// <summary>
        /// 获取DataGrid住宿费
        /// </summary>
        /// <param name="txtASubsidies"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetASubsidiesTextBox(TextBox txtASubsidies, int i,bool FromReadOnlyDataGrid)
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
            if (dataGrid.Columns[9].GetCellContent(TravelDetailList_Golbal[i - 1]) != null)
            {
                txtASubsidies = dataGrid.Columns[9].GetCellContent(TravelDetailList_Golbal[i - 1]).FindName("txtACCOMMODATION") as TextBox;//住宿费
            }
            return txtASubsidies;
        }
        /// <summary>
        /// 获取DataGrid交通补贴控件
        /// </summary>
        /// <param name="txtTFSubsidies"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetTFSubsidiesTextBox(TextBox txtTFSubsidies, int i, bool FromReadOnlyDataGrid)
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
                if (dataGrid.Columns[10].GetCellContent(TravelDetailList_Golbal[i - 1]) != null)
                {
                    txtTFSubsidies = dataGrid.Columns[10].GetCellContent(TravelDetailList_Golbal[i - 1]).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
                }
            }
            catch (Exception ex)
            {
                Utility.SetLog(ex.ToString());
                txtTFSubsidies = null;
            }

            return txtTFSubsidies;
        }
        /// <summary>
        /// 获取DataGrid餐费补贴控件
        /// </summary>
        /// <param name="txtMealSubsidies"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetMealSubsidiesTextBox(TextBox txtMealSubsidies, int i, bool FromReadOnlyDataGrid)
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
            if (dataGrid.Columns[11].GetCellContent(TravelDetailList_Golbal[i - 1]) != null)
            {
                txtMealSubsidies = dataGrid.Columns[11].GetCellContent(TravelDetailList_Golbal[i - 1]).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
            }
            return txtMealSubsidies;
        }
        /// <summary>
        /// 获取DataGrid其他费用
        /// </summary>
        /// <param name="txtMealSubsidies"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetOtherCostsTextBox(TextBox txtOtherCosts, int i, bool FromReadOnlyDataGrid)
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
            if (dataGrid.Columns[12].GetCellContent(TravelDetailList_Golbal[i - 1]) != null)
            {
                txtOtherCosts = dataGrid.Columns[12].GetCellContent(TravelDetailList_Golbal[i - 1]).FindName("txtOtherCosts") as TextBox;//其他费用
            }
            return txtOtherCosts;
        }
        #endregion

        #region 根据当前用户的级别过滤出该级别能乘坐的交通工具类型

        /// <summary>
        /// 根据当前用户的级别过滤出该级别能乘坐的交通工具类型
        /// </summary>
        /// <param name="TraveToolType">交通工具类型</param>
        /// <param name="postLevel">岗位级别</param>
        /// <returns>0：类型超标，1：类型不超标，2：级别不超标</returns>
        private int CheckTraveToolStand(string TraveToolType, string TraveToolLevel, string postLevel)
        {
            int i = 0;
            var q = from ent in takethestandardtransport
                    where ent.ENDPOSTLEVEL.Contains(postLevel) && ent.TYPEOFTRAVELTOOLS == TraveToolType
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

        private T_OA_TAKETHESTANDARDTRANSPORT GetVehicleTypeValue(string ToolType)
        {
            if(string.IsNullOrEmpty(EmployeePostLevel))
            {
                MessageBox.Show("当前报销员工岗位级别为空,请联系管理员");
                return null;
            }
            try
            {
                if (string.IsNullOrEmpty(ToolType))
                {
                    var q = from ent in takethestandardtransport
                            where ent.ENDPOSTLEVEL.Contains(EmployeePostLevel)
                            orderby ent.TAKETHETOOLLEVEL ascending
                            select ent;
                    q = q.OrderBy(n => n.TYPEOFTRAVELTOOLS);
                    if (q.Count() > 0)
                    {
                        return q.FirstOrDefault();
                    }
                }
                else
                {
                    var q = from ent in takethestandardtransport
                            where ent.ENDPOSTLEVEL.Contains(EmployeePostLevel) && ent.TYPEOFTRAVELTOOLS == ToolType
                            orderby ent.TAKETHETOOLLEVEL ascending
                            select ent;

                    if (q.Count() > 0)
                    {
                        return q.FirstOrDefault();
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            return null;
        }
        #endregion


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
                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = dataGrid.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                    double total = 0;
                    int i = 0;
                    foreach (var obje in objs)
                    {
                        i++;
                        double toodays = 0;
                        List<string> list = new List<string>
                    {
                            obje.BUSINESSDAYS
                    };

                        if (obje.BUSINESSDAYS != null && !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                        {
                            double totalHours = System.Convert.ToDouble(list[0]);
                            toodays = totalHours;
                        }
                        double totolDay = toodays;//计算本次出差的总天数

                        string cityValue = citysEndList_Golbal[i - 1].Replace(",", "");//目标城市值
                        entareaallowance = this.GetAllowanceByCityValue(cityValue);
                        if (travelsolutions != null && employeepost != null)
                        {

                            if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                            {
                                MessageBox.Show("您的岗位级别小于8，无交通补贴及住宿补贴");
                                obje.TRANSPORTATIONSUBSIDIES = 0;
                                obje.MEALSUBSIDIES = 0;
                            }
                            else
                            {
                                #region 根据本次出差的总天数,根据天数获取相应的补贴
                                if (totolDay <= travelsolutions.MINIMUMINTERVALDAYS.ToInt32())//本次出差总时间小于等于设定天数的报销标准
                                {
                                    if (entareaallowance != null)
                                    {
                                        if (obje.BUSINESSDAYS != null)
                                        {
                                            if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                obje.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                obje.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    obje.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES.ToDouble() * toodays);
                                                }
                                                else
                                                {
                                                    obje.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            obje.TRANSPORTATIONSUBSIDIES = 0;
                                        }
                                        if (obje.BUSINESSDAYS != null)
                                        {
                                            if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                obje.MEALSUBSIDIES = 0;
                                            }
                                            else if (obje.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                obje.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    obje.MEALSUBSIDIES = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES.ToDouble() * toodays);
                                                }
                                                else
                                                {
                                                    obje.MEALSUBSIDIES = 0;
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
                                            if (obje.BUSINESSDAYS != null)
                                            {
                                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                                {
                                                    obje.TRANSPORTATIONSUBSIDIES = 0;
                                                    obje.TRANSPORTCOSTS = 0;
                                                }
                                                else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                                {
                                                    obje.TRANSPORTATIONSUBSIDIES = 0;
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
                                                        obje.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney + lastmoney);
                                                    }
                                                    else
                                                    {
                                                        obje.TRANSPORTATIONSUBSIDIES = 0;
                                                    }
                                                }
                                            }
                                            else//如果天数为null的禁用住宿费控件
                                            {
                                            }
                                            if (obje.BUSINESSDAYS != null)
                                            {
                                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                                {
                                                    obje.MEALSUBSIDIES = 0;
                                                }
                                                else if (obje.GOOUTTOMEET == "1")//如果是开会
                                                {
                                                    obje.MEALSUBSIDIES = 0;
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
                                                        obje.MEALSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney + lastmoney);
                                                    }
                                                    else
                                                    {
                                                        obje.MEALSUBSIDIES = 0;
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
                                        if (obje.BUSINESSDAYS != null)
                                        {
                                            if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                obje.TRANSPORTATIONSUBSIDIES = 0;
                                                obje.TRANSPORTCOSTS = 0;
                                            }
                                            else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                obje.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
                                                    double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
                                                    obje.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney);
                                                }
                                                else
                                                {
                                                    obje.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            //txtASubsidies.IsReadOnly = true;
                                        }
                                        if (obje.BUSINESSDAYS != null)
                                        {
                                            if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                obje.MEALSUBSIDIES = 0;
                                            }
                                            else if (obje.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                obje.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    //最小区间段金额
                                                    double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
                                                    //中间区间段金额
                                                    double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
                                                    obje.MEALSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney);
                                                }
                                                else
                                                {
                                                    obje.MEALSUBSIDIES = 0;
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
                        if (obje.TRANSPORTATIONSUBSIDIES != null && obje.MEALSUBSIDIES != null)
                        {
                            total += Convert.ToDouble(obje.TRANSPORTATIONSUBSIDIES.Value + obje.MEALSUBSIDIES.Value);
                            this.txtSubTotal.Text = total.ToString();//总费用
                            this.txtChargeApplyTotal.Text = total.ToString();
                        }

                        Fees = total;
                    }

                    CountMoney();
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

        #region 获取出差报销标准并显示
        /// <summary>
        /// 获取出差报销补助
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private T_OA_AREAALLOWANCE StandardsMethod(int i)
        {
            T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
            string cityValue = citysEndList_Golbal[i - 1].Replace(",", "");//目标城市值
            entareaallowance = this.GetAllowanceByCityValue(cityValue);
            if (textStandards.Text.Contains(SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue)))
            {
                //已经包含，直接返回
                return entareaallowance;
            }
            if (i == TravelDetailList_Golbal.Count)
            {
                //出差结束城市无补贴
                return entareaallowance;
            }
            if (entareaallowance != null)//根据出差的城市及出差人的级别，将当前出差人的标准信息显示在备注中
            {
                if (i <= TravelDetailList_Golbal.Count() && TravelDetailList_Golbal.Count() > 1)
                {
                  
                    if (TravelDetailList_Golbal[i - 1].PRIVATEAFFAIR == "1")//如果是私事
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "的出差报销标准是：交通补贴：" + "无" + ",餐费补贴：" + "无" + ",住宿标准：" + "无。"
                            +"\n";
                    }
                    else if (TravelDetailList_Golbal[i - 1].GOOUTTOMEET == "1")//如果是内部会议及培训
                    {
                        //textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差为《内部会议、培训》，无各项差旅补贴。\n";
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "的出差为《内部会议、培训》，无各项差旅补贴。"
                            +"\n";
                    }
                    else if (TravelDetailList_Golbal[i - 1].COMPANYCAR == "1")//如果是公司派车
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "的出差报销标准是：交通补贴：" + "无" + "餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString() 
                            + "元,住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                            + "\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                    else if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的无各项补贴
                    {
                        //textStandards.Text = "您的岗位级别≥'I'级,无各项差旅补贴。";
                        textStandards.Text = textStandards.Text + "出差城市：" + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "  您的岗位级别≥'I'级，无各项差旅补贴。";
                        textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                            +"\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                    else
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "的出差报销标准是：交通补贴：" + entareaallowance.TRANSPORTATIONSUBSIDIES 
                            + "元，餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString() 
                            + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                            +"\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                }
                if (TravelDetailList_Golbal.Count() == 1)   //只有一条记录的情况
                {
                    if (TravelDetailList_Golbal[i - 1].PRIVATEAFFAIR == "1")//如果是私事
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "的出差报销标准是：交通补贴：" + "无" + "，餐费补贴：" 
                            + "无" + "，住宿标准：无。"
                            + "\n";
                    }
                    else if (TravelDetailList_Golbal[i - 1].GOOUTTOMEET == "1")//如果是内部会议及培训
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "的出差为《内部会议、培训》，无各项差旅补贴。"
                            +"\n";
                    }
                    else if (TravelDetailList_Golbal[i - 1].COMPANYCAR == "1")//如果是公司派车
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "的出差报销标准是：交通补贴：" + "无" + "餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString() 
                            + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                            + "\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                    else if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的无各项补贴
                    {
                        //textStandards.Text = "您的岗位级别≥'I'级，无各项差旅补贴。";
                        textStandards.Text = textStandards.Text + "出差城市：" + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "  您的岗位级别≥'I'级，无各项差旅补贴。";                        
                        textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                            + "\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                    else
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) 
                            + "的出差报销标准是：交通补贴：" + entareaallowance.TRANSPORTATIONSUBSIDIES 
                            + "元，餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString() 
                            + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                            + "\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                }
                bxbz = textStandards.Text;
            }
            else
            {
                textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "没有相应的出差标准。"
                    + "\n";
            }
            return entareaallowance;
        }


        /// <summary>
        /// 根据城市值  获取相应的出差补贴
        /// </summary>
        /// <param name="CityValue"></param>
        private T_OA_AREAALLOWANCE GetAllowanceByCityValue(string CityValue)
        {
            var q = from ent in areaallowance
                    join ac in areacitys on ent.T_OA_AREADIFFERENCE.AREADIFFERENCEID equals ac.T_OA_AREADIFFERENCE.AREADIFFERENCEID
                    where ac.CITY == CityValue && ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == travelsolutions.TRAVELSOLUTIONSID
                    select ent;

            if (q.Count() > 0)
            {
                return q.FirstOrDefault();
            }
            return null;
        }

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

        #region 获取出差方案

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


        
        //以下下为无用代码
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
                for (int i = 0; i < DaGrEdit.SelectedItems.Count; i++)
                {
                    int k = DaGrEdit.SelectedIndex;//当前选中行
                    T_OA_REIMBURSEMENTDETAIL entDel = DaGrEdit.SelectedItems[i] as T_OA_REIMBURSEMENTDETAIL;

                    if (TravelDetailList_Golbal.Contains(entDel))
                    {

                        TravelDetailList_Golbal.Remove(entDel);
                        if (citysEndList_Golbal.Count > k)
                        {

                            int EachCount = 0;
                            foreach (Object obje in DaGrEdit.ItemsSource)//将下一个出发城市的值修改
                            {
                                EachCount++;
                                if (DaGrEdit.Columns[1].GetCellContent(obje) != null)
                                {
                                    SearchCity mystarteachCity = DaGrEdit.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                                    if ((k + 1) == EachCount)
                                    {
                                        if (k > 0)
                                        {
                                            mystarteachCity.TxtSelectedCity.Text = GetCityName(citysEndList_Golbal[k - 1]);
                                            citysStartList_Golbal[k + 1] = citysEndList_Golbal[k - 1];//上一城市的城市值
                                        }
                                    }
                                }
                            }
                            citysEndList_Golbal.RemoveAt(k);//清除目标城市的值
                            citysStartList_Golbal.RemoveAt(k);//清除出发城市的值
                        }
                    }
                }
                DaGrEdit.ItemsSource = TravelDetailList_Golbal;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "必须保留一条出差时间及地点!", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }
        #endregion

        #region 隐藏附件控件
        //public void FileLoadedCompleted()
        //{
        //    //if (!ctrFile._files.HasAccessory)
        //    //{
        //    //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayoutRoot, 6);
        //    //    this.lblFile.Visibility = Visibility.Collapsed;
        //    //}
        //}
        #endregion

        #region 隐藏和显示FB控件
        private void fbChkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (fbChkBox.IsChecked == true)
            {
                scvFB.Visibility = Visibility.Visible;
            }
        }

        private void fbChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (fbChkBox.IsChecked == false)
            {
                scvFB.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(travelReimbursementID))
            {
                //ctrFile.Load_fileData(travelReimbursementID);
            }
            fbCtr.GetPayType.Visibility = Visibility.Visible;
        }
        #endregion

        #region 键盘事件
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    if (DaGrEdit.SelectedIndex == TrList.Count - 1)
            //    {
            //        T_OA_REIMBURSEMENTDETAIL buport = new T_OA_REIMBURSEMENTDETAIL();
            //        buport.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();
            //        buport.STARTDATE = DateTime.Now;
            //        buport.ENDDATE = DateTime.Now;
            //        TrList.Add(buport);
            //    }
            //}
        }
        #endregion

    }
}
