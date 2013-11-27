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

        #region "获取交通工具的级别"
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
        #endregion

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
            if (dataGrid.Columns[8].GetCellContent(TravelDetailList_Golbal[i]) != null)
            {
                txtTranSportcosts = dataGrid.Columns[8].GetCellContent(TravelDetailList_Golbal[i]).FindName("txtTRANSPORTCOSTS") as TextBox;//交通费
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
            if (dataGrid.Columns[9].GetCellContent(TravelDetailList_Golbal[i]) != null)
            {
                txtASubsidies = dataGrid.Columns[9].GetCellContent(TravelDetailList_Golbal[i]).FindName("txtACCOMMODATION") as TextBox;//住宿费
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
                if (dataGrid.Columns[10].GetCellContent(TravelDetailList_Golbal[i]) != null)
                {
                    txtTFSubsidies = dataGrid.Columns[10].GetCellContent(TravelDetailList_Golbal[i]).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
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
            if (dataGrid.Columns[11].GetCellContent(TravelDetailList_Golbal[i]) != null)
            {
                txtMealSubsidies = dataGrid.Columns[11].GetCellContent(TravelDetailList_Golbal[i]).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
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
            if (dataGrid.Columns[12].GetCellContent(TravelDetailList_Golbal[i]) != null)
            {
                txtOtherCosts = dataGrid.Columns[12].GetCellContent(TravelDetailList_Golbal[i]).FindName("txtOtherCosts") as TextBox;//其他费用
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

        #region 根据城市设置出差报销标准并显示
        /// <summary>
        /// 获取出差报销补助
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private T_OA_AREAALLOWANCE StandardsMethod(int i)
        {
            T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
            textStandards.Text = string.Empty;
            if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
            {
                textStandards.Text = textStandards.Text + "报销人的岗位级别小于等于I级，无交通补贴及住宿补贴";
                //detail.TRANSPORTATIONSUBSIDIES = 0;
                //detail.MEALSUBSIDIES = 0;
                return null;
            }
            if (TravelDetailList_Golbal.Count() == 1)   //只有一条记录的情况
            {
                string city = TravelDetailList_Golbal[0].DESTCITY.Replace(",", "");//目标城市值
                entareaallowance = this.GetAllowanceByCityValue(city);
                if (textStandards.Text.Contains(SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)))
                {
                    //已经包含，直接跳过
                    return entareaallowance;
                }
                
                if (TravelDetailList_Golbal[0].PRIVATEAFFAIR == "1")//如果是私事
                {
                    textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                        + "的出差报销标准是：交通补贴：" + "无" + "，餐费补贴："
                        + "无" + "，住宿标准：无。"
                        + "\n";
                }
                else if (TravelDetailList_Golbal[0].GOOUTTOMEET == "1")//如果是内部会议及培训
                {
                    textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                        + "的出差为《内部会议、培训》，无各项差旅补贴。"
                        + "\n";
                }
                else if (TravelDetailList_Golbal[0].COMPANYCAR == "1")//如果是公司派车
                {
                    textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                        + "的出差报销标准是：交通补贴：" + "无" + "餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString()
                        + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                        + "\n";
                    //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                }
                else if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的无各项补贴
                {
                    //textStandards.Text = "您的岗位级别≥'I'级，无各项差旅补贴。";
                    textStandards.Text = textStandards.Text + "出差城市：" + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                        + "  您的岗位级别≥'I'级，无各项差旅补贴。";
                    textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                        + "\n";
                    //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                }
                else
                {
                    textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                        + "的出差报销标准是：交通补贴：" + entareaallowance.TRANSPORTATIONSUBSIDIES
                        + "元，餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString()
                        + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                        + "\n";
                    //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                }
            }
            else
            {
                for (int j = 0; j < TravelDetailList_Golbal.Count() - 1; j++)//最后一条记录没有补贴
                {
                    string city = TravelDetailList_Golbal[j].DESTCITY.Replace(",", "");//目标城市值
                    entareaallowance = this.GetAllowanceByCityValue(city);
                    if (textStandards.Text.Contains(SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)))
                    {
                        //已经包含，直接跳过
                        continue;
                    }
                    if (entareaallowance != null)//根据出差的城市及出差人的级别，将当前出差人的标准信息显示在备注中
                    {
                        if (TravelDetailList_Golbal[j].PRIVATEAFFAIR == "1")//如果是私事
                        {
                            textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "的出差报销标准是：交通补贴：" + "无" + ",餐费补贴：" + "无" + ",住宿标准：" + "无。"
                                + "\n";
                        }
                        else if (TravelDetailList_Golbal[j].GOOUTTOMEET == "1")//如果是内部会议及培训
                        {
                            //textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差为《内部会议、培训》，无各项差旅补贴。\n";
                            textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "的出差为《内部会议、培训》，无各项差旅补贴。"
                                + "\n";
                        }
                        else if (TravelDetailList_Golbal[j].COMPANYCAR == "1")//如果是公司派车
                        {
                            textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "的出差报销标准是：交通补贴：" + "无" + ",餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString()
                                + "元,住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                                + "\n";
                            //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                        }
                        else if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的无各项补贴
                        {
                            //textStandards.Text = "您的岗位级别≥'I'级,无各项差旅补贴。";
                            textStandards.Text = textStandards.Text + "出差城市：" + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "  您的岗位级别≥'I'级，无各项差旅补贴。";
                            textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                                + "\n";
                            //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                        }
                        else
                        {
                            textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city)
                                + "的出差报销标准是：交通补贴：" + entareaallowance.TRANSPORTATIONSUBSIDIES
                                + "元，餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString()
                                + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。"
                                + "\n";
                            //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                        }
                    }
                    else
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(city) + "没有相应的出差标准。"
                            + "\n";
                    }
                }
            }


            string cityValue = TravelDetailList_Golbal[i].DESTCITY.Replace(",", "");//目标城市值
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
