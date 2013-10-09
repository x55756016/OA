/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-15

** 修改人：刘锦

** 修改时间：2010-06-28

** 描述：

**    主要用于福利发放信息的资料录入

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.SalaryWS;

namespace SMT.SaaS.OA.UI.Views.BenefitsAdministration
{
    public partial class WelfareProvisionChildWindows : BaseForm, IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        private SmtOADocumentAdminClient BenefitsAdministration;
        private SalaryServiceClient ssc;//判断薪资结算
        private T_OA_WELFAREDISTRIBUTEMASTER WelfareProvisionInfo;
        private List<T_OA_WELFAREMASERT> welfareList = new List<T_OA_WELFAREMASERT>();
        private ObservableCollection<T_HR_EMPLOYEEADDSUM> emploeeaddsumList = new ObservableCollection<T_HR_EMPLOYEEADDSUM>();//薪资
        private T_OA_WELFAREMASERT welfare = new T_OA_WELFAREMASERT();
        private List<T_OA_WELFAREDETAIL> ListWelfare = new List<T_OA_WELFAREDETAIL>();
        private T_OA_WELFAREDETAIL welfareDetail = new T_OA_WELFAREDETAIL();
        ObservableCollection<T_OA_WELFAREDETAIL> welfareDetailList = new ObservableCollection<T_OA_WELFAREDETAIL>();
        ObservableCollection<T_OA_WELFAREDISTRIBUTEDETAIL> detailList = new ObservableCollection<T_OA_WELFAREDISTRIBUTEDETAIL>();
        private FormTypes actions; //操作类型
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private string checkState = ((int)CheckStates.Approved).ToString();
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private ObservableCollection<string> delteWelfarePaymentDetail = new ObservableCollection<string>();
        private string welfareInfoID = "";

        List<T_OA_WELFAREDISTRIBUTEDETAIL> detailTemps = new List<T_OA_WELFAREDISTRIBUTEDETAIL>();
        public T_OA_WELFAREDISTRIBUTEMASTER InfoObj
        {
            get { return WelfareProvisionInfo; }
            set
            {
                this.DataContext = value;
                WelfareProvisionInfo = value;
            }
        }
        #endregion

        #region 构造函数
        public WelfareProvisionChildWindows(FormTypes action, string welfareInfoId)
        {
            InitializeComponent();
            this.actions = action;
            this.welfareInfoID = welfareInfoId;
            this.Loaded += (sender, args) =>
            {
                #region 原来的
                InitEvent();
                InitData();
                BenefitsAdministration.GetWelfareInformationAsync();//获取福利项目
                if (action == FormTypes.New)
                {
                    this.StartTime.Text = string.Empty;//生效时间
                    this.EndTime.Text = string.Empty;//失效时间
                    //this.audit.Visibility = Visibility.Collapsed;
                    this.DaGrs.Columns[4].Visibility = Visibility.Collapsed;//隐藏操作列
                }
                if (action == FormTypes.Edit || action == FormTypes.Audit || action == FormTypes.Browse)
                {
                    this.cbWelfareID.IsEnabled = false;
                    this.btnLookUpPartya.IsEnabled = false;
                    this.StartTime.IsEnabled = false;
                    this.welfareItem.Visibility = Visibility.Collapsed;
                    this.tbcContainer.SelectedIndex = 1;
                }
                if (action == FormTypes.Audit || action == FormTypes.Browse)//审批，查看
                {
                    ShieldedControl();
                }
                #endregion
            };
        }
        #endregion

        #region InitData
        private void InitData()
        {
            try
            {
                if (actions == FormTypes.New)
                {
                    InfoObj = new T_OA_WELFAREDISTRIBUTEMASTER();
                    InfoObj.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                }
                else
                {
                    if (actions == FormTypes.Audit)
                    {
                        actionFlag = DataActionFlag.SubmitComplete;
                    }
                    BenefitsAdministration.GetProvisionByIdAsync(welfareInfoID);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        #endregion

        #region 屏蔽控件
        private void ShieldedControl()
        {
            this.cbWelfareID.IsEnabled = false;
            this.txtReleaseName.IsReadOnly = true;
            this.RedYes.IsEnabled = false;
            this.txtContent.IsReadOnly = true;
            this.btnLookUpPartya.IsEnabled = false;
            this.StartTime.IsEnabled = false;
            this.ReleaseTime.IsEnabled = false;
            this.DaGrs.Columns[4].Visibility = Visibility.Collapsed;//隐藏操作列
            this.DaGrs.IsReadOnly = true;
        }
        #endregion

        #region DataGrid 数据加载
        private void BindDetailsDataGrid(ObservableCollection<T_OA_WELFAREDETAIL> obj)//标准明细查询
        {
            if (obj == null || obj.Count < 1)
            {
                DaGrss.ItemsSource = null;
                return;
            }
            DaGrss.ItemsSource = obj;
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
            BenefitsAdministration = new SmtOADocumentAdminClient();
            ssc = new SalaryServiceClient();
            BenefitsAdministration.GetWelfareInformationCompleted += new EventHandler<GetWelfareInformationCompletedEventArgs>(wsscs_GetWelfareInformationCompleted);//查询出福利项目
            BenefitsAdministration.UpdateWelfarePaymentDetailsCompleted += new EventHandler<UpdateWelfarePaymentDetailsCompletedEventArgs>(BenefitsAdministration_UpdateWelfarePaymentDetailsCompleted);//修改明细
            BenefitsAdministration.WelfareProvisionAddCompleted += new EventHandler<WelfareProvisionAddCompletedEventArgs>(wpsc_WelfareProvisionAddCompleted);//添加
            BenefitsAdministration.UpdateWelfareProvisionCompleted += new EventHandler<UpdateWelfareProvisionCompletedEventArgs>(BenefitsAdministration_UpdateWelfareProvisionCompleted);//修改
            BenefitsAdministration.GetBenefitsDetailsAdministrationCompleted += new EventHandler<GetBenefitsDetailsAdministrationCompletedEventArgs>(BenefitsAdministration_GetBenefitsDetailsAdministrationCompleted);//查询标准明细
            BenefitsAdministration.GetByIdWelfarePaymentDetailsCompleted += new EventHandler<GetByIdWelfarePaymentDetailsCompletedEventArgs>(BenefitsAdministration_GetByIdWelfarePaymentDetailsCompleted);
            BenefitsAdministration.DeleteWelfarePaymentDetailCompleted += new EventHandler<DeleteWelfarePaymentDetailCompletedEventArgs>(BenefitsAdministration_DeleteWelfarePaymentDetailCompleted);
            BenefitsAdministration.GetProvisionByIdCompleted += new EventHandler<GetProvisionByIdCompletedEventArgs>(BenefitsAdministration_GetProvisionByIdCompleted);
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        void BenefitsAdministration_GetProvisionByIdCompleted(object sender, GetProvisionByIdCompletedEventArgs e)//根据发放ID查询
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DIDNOTFINDRELEVANT", "WELFAREPROVISIONPAGE"));
                        return;
                    }
                    InfoObj = e.Result;
                    if (actions == FormTypes.Resubmit)//重新提交
                    {
                        InfoObj.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                    }
                    this.ReleaseTime.Text = Convert.ToDateTime(InfoObj.DISTRIBUTEDATE).ToShortDateString();//发放时间
                    if (InfoObj.ISWAGE == "1")//是否随薪发？0：非随工资发 1：随工资发
                    {
                        RedYes.IsChecked = true;
                    }
                    //if (actions == FormTypes.Audit)
                    //{
                    //    audit.XmlObject = DataObjectToXml<T_OA_WELFAREDISTRIBUTEMASTER>.ObjListToXml(InfoObj, "OA");
                    //}
                    BenefitsAdministration.GetByIdWelfarePaymentDetailsAsync(InfoObj.WELFAREDISTRIBUTEMASTERID);
                    //InitAudit();//获取审核控件的数据
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        void BenefitsAdministration_GetByIdWelfarePaymentDetailsCompleted(object sender, GetByIdWelfarePaymentDetailsCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    detailTemps = e.Result.ToList();
                    if (InfoObj.ISWAGE == "1")//是否随薪发？0：非随工资发 1：随工资发
                    {
                        foreach (var detailTemp in detailTemps)
                        {
                            T_HR_EMPLOYEEADDSUM emploeeaddsum = new T_HR_EMPLOYEEADDSUM();
                            emploeeaddsum.ADDSUMID = Guid.NewGuid().ToString();//加扣款ID
                            emploeeaddsum.EMPLOYEEID = detailTemp.USERID;//员工编号
                            emploeeaddsum.EMPLOYEENAME = detailTemp.OWNERNAME;//员工姓名
                            emploeeaddsum.PROJECTNAME = InfoObj.WELFAREDISTRIBUTETITLE;//福利发放名
                            emploeeaddsum.PROJECTCODE = "-2";
                            emploeeaddsum.PROJECTMONEY = detailTemp.STANDARD;//项目金额(发放金额)
                            emploeeaddsum.DEALYEAR = Convert.ToDateTime(InfoObj.DISTRIBUTEDATE).Year.ToString();//年
                            emploeeaddsum.DEALMONTH = Convert.ToDateTime(InfoObj.DISTRIBUTEDATE).Month.ToString();//月
                            emploeeaddsum.REMARK = detailTemp.REMARK;//备注
                            emploeeaddsum.OWNERID = InfoObj.OWNERID;
                            emploeeaddsum.OWNERPOSTID = InfoObj.OWNERPOSTID;
                            emploeeaddsum.OWNERDEPARTMENTID = InfoObj.OWNERDEPARTMENTID;
                            emploeeaddsum.OWNERCOMPANYID = InfoObj.OWNERCOMPANYID;
                            emploeeaddsum.CREATEPOSTID = InfoObj.CREATEPOSTID;
                            emploeeaddsum.CREATEDEPARTMENTID = InfoObj.CREATEDEPARTMENTID;
                            emploeeaddsum.CREATECOMPANYID = InfoObj.CREATECOMPANYID;
                            emploeeaddsum.CREATEUSERID = InfoObj.CREATEUSERID;
                            emploeeaddsum.CREATEDATE = InfoObj.CREATEDATE;
                            emploeeaddsumList.Add(emploeeaddsum);
                        }
                    }
                    BindDataGrid(e.Result.ToList());
                }
                else
                {
                    BindDataGrid(null);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        void BenefitsAdministration_GetBenefitsDetailsAdministrationCompleted(object sender, GetBenefitsDetailsAdministrationCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DIDNOTFINDRELEVANT", "WELFARESTANDARD"));
                        BindDetailsDataGrid(null);
                        return;
                    }

                    BindDetailsDataGrid(e.Result);
                    ListWelfare = e.Result.ToList();
                    welfareDetail = e.Result.FirstOrDefault();
                    if (welfareDetail != null)
                    {
                        txtNotes.Text = welfareDetail.T_OA_WELFAREMASERT.REMARK;//备注
                        EndTime.Text = welfareDetail.T_OA_WELFAREMASERT.ENDDATE.ToString();//失效时间

                        welfare = welfareDetail.T_OA_WELFAREMASERT;
                    }
                    PostLevel();
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 保存明细修改
        private void myBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DaGrs.SelectedItems.Count > 0)
                {
                    BenefitsAdministration.UpdateWelfarePaymentDetailsAsync(DaGrs.SelectedItems[0] as T_OA_WELFAREDISTRIBUTEDETAIL);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        void BenefitsAdministration_UpdateWelfarePaymentDetailsCompleted(object sender, UpdateWelfarePaymentDetailsCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "PAYMENTDETAILS"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }
        #endregion

        #region 删除福利发放明细
        private void myDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DaGrs.SelectedItems.Count > 0)
                {
                    delteWelfarePaymentDetail.Add((DaGrs.SelectedItem as T_OA_WELFAREDISTRIBUTEDETAIL).WELFAREDISTRIBUTEDETAILID);

                    string Result = "";
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        BenefitsAdministration.DeleteWelfarePaymentDetailAsync(delteWelfarePaymentDetail);
                        BenefitsAdministration.GetByIdWelfarePaymentDetailsAsync(InfoObj.WELFAREDISTRIBUTEMASTERID);
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
                else
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETESUCCESSED"), Utility.GetResourceStr("PAYMENTDETAILS"));
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        void BenefitsAdministration_DeleteWelfarePaymentDetailCompleted(object sender, DeleteWelfarePaymentDetailCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "PAYMENTDETAILS"));
                BenefitsAdministration.GetByIdWelfarePaymentDetailsAsync(InfoObj.WELFAREDISTRIBUTEMASTERID);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 福利项目Combox处理
        void wsscs_GetWelfareInformationCompleted(object sender, GetWelfareInformationCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                welfareList = e.Result.ToList();
                if (welfareList != null)
                {
                    this.cbWelfareID.ItemsSource = welfareList;
                    cbWelfareID.DisplayMemberPath = "welfareStandard.WELFAREPROID";//福利项目
                    Utility.CbxItemBinders(cbWelfareID, "WELFAREPROID", "0");
                }
            }
        }
        private void cbWelfareID_SelectionChanged(object sender, SelectionChangedEventArgs e)//根据福利项目查询出具体的公司
        {
            if (txtCompanyObject.Text != null && txtCompanyObject.Text != string.Empty && StartTime.Text != null && StartTime.Text != string.Empty)
            {
                DateTime releaseTime = Convert.ToDateTime(StartTime.Text);
                BenefitsAdministration.GetBenefitsDetailsAdministrationAsync(cbWelfareID.SelectedIndex.ToString(), welfare.COMPANYID, releaseTime, Utility.GetCheckState(CheckStates.Approved));
            }
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            if (actions == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "WELFAREPROVISIONPAGE");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "WELFAREPROVISIONPAGE");
            }
            else if (actions == FormTypes.Audit)
            {
                return Utility.GetResourceStr("AUDIT", "WELFAREPROVISIONPAGE");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "WELFAREPROVISIONPAGE");
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
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
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
            if (actions != FormTypes.Browse && actions != FormTypes.Audit)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1",
                    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                };
                items.Add(item);

                item = new ToolbarItem
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

        #region 添加Completed
        void wpsc_WelfareProvisionAddCompleted(object sender, WelfareProvisionAddCompletedEventArgs e)//新增福利发放
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                else
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                        return;
                    }
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "APPLICATIONRECORD"));
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                    }
                    else
                    {
                        actions = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 修改Completed
        void BenefitsAdministration_UpdateWelfareProvisionCompleted(object sender, UpdateWelfareProvisionCompletedEventArgs e)//修改福利发放
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                else
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                        return;
                    }
                    if (e.UserState.ToString() == "Edit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "WELFAREPROVISIONPAGE"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                        }
                    }
                    else if (e.UserState.ToString() == "Audit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "WELFAREPROVISIONPAGE"));
                    }
                    else if (e.UserState.ToString() == "Submit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "WELFAREPROVISIONPAGE"));
                    }
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 提交审核
        private void SubmitAuditToClose()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();
        }
        #endregion

        #region 保存函数
        private void Save()
        {
            try
            {
                if (Check())
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);//点击保存后显示进度条
                    string strContractCreated = DateTime.Now.ToShortDateString();//创建时间

                    if (actions == FormTypes.New)
                    {
                        WelfareProvisionInfo.WELFAREDISTRIBUTEMASTERID = System.Guid.NewGuid().ToString();//发放ID
                        WelfareProvisionInfo.T_OA_WELFAREMASERT = welfare;
                        WelfareProvisionInfo.WELFAREDISTRIBUTETITLE = this.txtReleaseName.Text;//福利发放名
                        WelfareProvisionInfo.CONTENT = txtContent.Text.ToString();//发放内容
                        WelfareProvisionInfo.CREATEDATE = DateTime.Now;//创建时间
                        WelfareProvisionInfo.DISTRIBUTEDATE = Convert.ToDateTime(ReleaseTime.Text);//发放时间
                        WelfareProvisionInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//审批状态
                        WelfareProvisionInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人ID
                        WelfareProvisionInfo.ISWAGE = (bool)RedYes.IsChecked ? "1" : "0";//是否随薪发？
                        WelfareProvisionInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                        WelfareProvisionInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属用户ID
                        WelfareProvisionInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                        WelfareProvisionInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                        WelfareProvisionInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                        WelfareProvisionInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                        WelfareProvisionInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                        WelfareProvisionInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                        WelfareProvisionInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//所属用户姓名

                        if (DaGrs.ItemsSource == null)
                        {
                            return;
                        }
                        //List<T_OA_WELFAREDISTRIBUTEDETAIL> detailTemps = DaGrs.ItemsSource as List<T_OA_WELFAREDISTRIBUTEDETAIL>;
                        foreach (var detailTemp in detailTemps)
                        {
                            T_OA_WELFAREDISTRIBUTEDETAIL WelfareDetails = new T_OA_WELFAREDISTRIBUTEDETAIL();
                            WelfareDetails.WELFAREDISTRIBUTEDETAILID = System.Guid.NewGuid().ToString();//发放明细ID
                            WelfareDetails.T_OA_WELFAREDISTRIBUTEMASTER = WelfareProvisionInfo;//发放编号
                            WelfareDetails.STANDARD = detailTemp.STANDARD;//标准
                            WelfareDetails.REMARK = txtNotes.Text.ToString();//发放项
                            WelfareDetails.OWNERID = detailTemp.USERID;
                            WelfareDetails.USERID = detailTemp.USERID;//用户ID
                            WelfareDetails.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人ID
                            WelfareDetails.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                            WelfareDetails.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                            WelfareDetails.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                            WelfareDetails.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                            WelfareDetails.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                            WelfareDetails.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                            WelfareDetails.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                            WelfareDetails.OWNERNAME = detailTemp.OWNERNAME;//所属用户姓名
                            detailList.Add(WelfareDetails);
                        }
                        BenefitsAdministration.WelfareProvisionAddAsync(WelfareProvisionInfo, detailList);
                    }
                    else
                    {
                        DateTime ProvisionTime = Convert.ToDateTime(WelfareProvisionInfo.DISTRIBUTEDATE);//发放日期
                        WelfareProvisionInfo.ISWAGE = (bool)RedYes.IsChecked ? "1" : "0";//是否随薪发？0：非随工资发 1：随工资发
                        WelfareProvisionInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                        WelfareProvisionInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//修改人姓名
                        WelfareProvisionInfo.CONTENT = txtContent.Text;//发放内容
                        WelfareProvisionInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//审批状态
                        WelfareProvisionInfo.WELFAREDISTRIBUTETITLE = txtReleaseName.Text;//福利发放名
                        WelfareProvisionInfo.DISTRIBUTEDATE = ProvisionTime;//发放日期

                        BenefitsAdministration.UpdateWelfareProvisionAsync(WelfareProvisionInfo, "Edit");
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {

            if (string.IsNullOrEmpty(txtReleaseName.Text.ToString()))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "RELEASENAME"));
                this.txtReleaseName.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtContent.Text.ToString()))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CONTENTDISTRIBUTION"));
                this.txtContent.Focus();
                return false;
            }

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
            }
            return true;
        }
        #endregion

        #region DataGrid 数据加载
        private void BindDataGrid(List<T_OA_WELFAREDISTRIBUTEDETAIL> obj)
        {
            if (obj == null || obj.Count < 1)
            {
                DaGrs.ItemsSource = null;
                return;
            }
            PagedCollectionView pcv = null;//分页

            if (obj != null)
            {
                var q = from ent in obj
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 6;
            }
            dpGrids.DataContext = pcv;

            DaGrs.ItemsSource = pcv;
        }
        #endregion

        #region 根据岗位级别获取对应的员工
        private void PostLevel()
        {
            PersonnelServiceClient Personne = new PersonnelServiceClient();
            Personne.GetEmployeesByPostLevelIntervalCompleted += new EventHandler<GetEmployeesByPostLevelIntervalCompletedEventArgs>(Personne_GetEmployeesByPostLevelIntervalCompleted);
            Personne.GetEmployeePostByPostIDsCompleted += new EventHandler<GetEmployeePostByPostIDsCompletedEventArgs>(Personne_GetEmployeePostByPostIDsCompleted);
            if (welfareDetail != null)
            {
                ObservableCollection<int> welfareLevela = new ObservableCollection<int>();
                ObservableCollection<int> welfareLevelb = new ObservableCollection<int>();
                ObservableCollection<string> welfarePostId = new ObservableCollection<string>();//根据岗位获取数据

                if (welfareDetail.ISLEVEL == "1")//岗位级别
                {
                    foreach (var detail in ListWelfare)
                    {
                        welfareLevela.Add(Convert.ToInt32(detail.POSTLEVELA));
                        welfareLevelb.Add(Convert.ToInt32(detail.POSTLEVELB));
                    }
                    if (welfareLevela.Count > 0)
                    {
                        Personne.GetEmployeesByPostLevelIntervalAsync(welfareLevela, welfareLevelb);
                    }
                }
                else  //岗位
                {
                    foreach (var detail in ListWelfare)
                    {
                        welfarePostId.Add(detail.POSTID);
                    }
                    if (welfarePostId.Count > 0)
                    {
                        Personne.GetEmployeePostByPostIDsAsync(welfarePostId);
                    }
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DIDNOTFINDRELEVANT", "WELFARESTANDARD"));
                txtNotes.Text = "";
                EndTime.Text = "";
            }
        }

        void Personne_GetEmployeePostByPostIDsCompleted(object sender, GetEmployeePostByPostIDsCompletedEventArgs e)//根据岗位获取员工
        {
            if (e.Result != null)
            {
                ObservableCollection<T_HR_EMPLOYEEPOST> Employee = e.Result;

                List<T_OA_WELFAREDISTRIBUTEDETAIL> details = new List<T_OA_WELFAREDISTRIBUTEDETAIL>();
                foreach (var detail in ListWelfare)
                {
                    List<T_OA_WELFAREDISTRIBUTEDETAIL> wdetails = new List<T_OA_WELFAREDISTRIBUTEDETAIL>();
                    wdetails = (from employeeInfo in Employee
                                where employeeInfo.T_HR_POST.POSTID == detail.POSTID
                                select new T_OA_WELFAREDISTRIBUTEDETAIL
                                {
                                    USERID = employeeInfo.T_HR_EMPLOYEE.EMPLOYEEID,
                                    OWNERNAME = employeeInfo.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                    STANDARD = Convert.ToDecimal(detail.STANDARD),
                                    REMARK = detail.REMARK,
                                    OWNERPOSTID = employeeInfo.T_HR_POST.POSTID,
                                    OWNERCOMPANYID = employeeInfo.T_HR_POST.COMPANYID
                                }).ToList();
                    details.AddRange(wdetails);
                }
                PagedCollectionView pcv = null;//分页
                if (e.Result != null)
                {
                    var q = from ent in details
                            select ent;

                    pcv = new PagedCollectionView(q);
                    pcv.PageSize = 5;
                }
                dpGrids.DataContext = pcv;
                detailTemps = details;
                DaGrs.ItemsSource = pcv;
            }
        }

        void Personne_GetEmployeesByPostLevelIntervalCompleted(object sender, GetEmployeesByPostLevelIntervalCompletedEventArgs e)//根据岗位级别获取员工
        {
            if (e.Result != null)
            {
                ObservableCollection<V_EMPOYEEPOSTLEVEL> Employee = e.Result;
                List<T_OA_WELFAREDISTRIBUTEDETAIL> details = new List<T_OA_WELFAREDISTRIBUTEDETAIL>();

                foreach (var listitme in ListWelfare)
                {
                    List<T_OA_WELFAREDISTRIBUTEDETAIL> detail = new List<T_OA_WELFAREDISTRIBUTEDETAIL>();
                    detail = (from employeeInfo in Employee
                              where employeeInfo.POSTLEVEL >= Convert.ToInt32(listitme.POSTLEVELA) && employeeInfo.POSTLEVEL <= Convert.ToInt32(listitme.POSTLEVELB)
                              select new T_OA_WELFAREDISTRIBUTEDETAIL
                              {
                                  USERID = employeeInfo.T_HR_EMPLOYEE.EMPLOYEEID,
                                  OWNERNAME = employeeInfo.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                  STANDARD = Convert.ToDecimal(listitme.STANDARD),
                                  REMARK = listitme.REMARK,
                                  OWNERPOSTID = listitme.POSTID,
                                  OWNERCOMPANYID = welfare.COMPANYID
                              }).ToList();
                    details.AddRange(detail);
                }
                PagedCollectionView pcv = null;//分页
                if (e.Result != null)
                {
                    var q = from ent in details
                            select ent;

                    pcv = new PagedCollectionView(q);
                    pcv.PageSize = 5;
                }
                dpGrids.DataContext = pcv;
                detailTemps = details;
                DaGrs.ItemsSource = pcv;
            }
        }
        #endregion

        #region DaGrs_LoadingRow
        private void DaGrs_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_WELFAREDISTRIBUTEDETAIL tmp = (T_OA_WELFAREDISTRIBUTEDETAIL)e.Row.DataContext;

            ImageButton MyButton_Addbaodao = DaGrs.Columns[4].GetCellContent(e.Row).FindName("myBtn") as ImageButton;
            ImageButton MyButton_Delbaodao = DaGrs.Columns[4].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
            MyButton_Addbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Addbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png", Utility.GetResourceStr("SAVE"));
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            MyButton_Addbaodao.Tag = tmp;
            MyButton_Delbaodao.Tag = tmp;
        }
        #endregion

        #region GridPager_Click
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BenefitsAdministration.GetByIdWelfarePaymentDetailsAsync(InfoObj.WELFAREDISTRIBUTEMASTERID);
        }
        #endregion

        #region 获取公司ID
        private void btnLookUpPartya_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    welfare.COMPANYID = companyInfo.ObjectID;
                    txtCompanyObject.Text = companyInfo.ObjectName;
                }
                if (this.StartTime.Text != null && StartTime.Text != string.Empty && cbWelfareID.SelectedIndex > -1 && txtCompanyObject.Text != null && txtCompanyObject.Text != string.Empty)
                {
                    BenefitsAdministration.GetBenefitsDetailsAdministrationAsync(cbWelfareID.SelectedIndex.ToString(), welfare.COMPANYID, Convert.ToDateTime(StartTime.Text), Utility.GetCheckState(CheckStates.Approved));
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        #endregion

        #region 选择发放时间触发
        private void StartTime_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.StartTime.Text != null && StartTime.Text != string.Empty && txtCompanyObject.Text != null && txtCompanyObject.Text != string.Empty && cbWelfareID.SelectedIndex > -1)
            {
                BenefitsAdministration.GetBenefitsDetailsAdministrationAsync(cbWelfareID.SelectedIndex.ToString(), welfare.COMPANYID, Convert.ToDateTime(StartTime.Text), Utility.GetCheckState(CheckStates.Approved));
                ReleaseTime.Text = StartTime.Text;//发放时间
            }
        }
        #endregion

        #region 判断是否存在金额
        private void RedYes_Click(object sender, RoutedEventArgs e)//判断是否存在金额
        {
            try
            {
                if (welfareDetail.STANDARD != null)
                {
                    WelfareProvisionInfo.ISWAGE = (bool)RedYes.IsChecked ? "1" : "0";
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AMOUNTNOTPAID", "NOTWITHTHESALARYPAYMENT"));
                    RedYes.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_WELFAREDISTRIBUTEMASTER>(InfoObj, "OA");
            Utility.SetAuditEntity(entity, "T_OA_WELFAREDISTRIBUTEMASTER", InfoObj.WELFAREDISTRIBUTEMASTERID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing://审核中
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful://审核通过
                    state = Utility.GetCheckState(CheckStates.Approved);
                    ssc.EmployeeAddSumLotsofADDAsync(emploeeaddsumList);//薪资
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail://审核未通过
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (InfoObj.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            InfoObj.CHECKSTATE = state;
            BenefitsAdministration.UpdateWelfareProvisionAsync(InfoObj, UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (InfoObj != null)
                state = InfoObj.CHECKSTATE;
            if (actions == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            BenefitsAdministration.DoClose();
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
    }
}
