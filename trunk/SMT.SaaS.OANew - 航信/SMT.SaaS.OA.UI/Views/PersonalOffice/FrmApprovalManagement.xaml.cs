using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PublicInterfaceWS;
namespace SMT.SaaS.OA.UI.Views.PersonalOffice
{
    public partial class FrmApprovalManagement : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private string checkState = ((int)CheckStates.ALL).ToString();
        private SmtOAPersonOfficeClient approvalManagement;
        private T_OA_APPROVALINFO approvalinfoInfo = null;
        private string SearchUserID = "";//已员工名为查询条件
        private string SearchPostID = "";//所属于人的岗位
        private string SearchDepartID = "";//所属于人的部门ID
        private string SearchCompanyID = "";//所属人的公司ID

        string OwnerCompanyid = "";//所属公司
        string OwnerDepartmentid = "";//获取事项审批时用 用的部门ID
        string Ownerid = "";//所属员工
        string OwnerPostid = "";//所属岗位
        
        private string StrApprovalOne = string.Empty;
        private string StrApprovalTwo = string.Empty;
        private string StrApprovalThird = string.Empty;
        private string StrApprovaltype = string.Empty;

        PermissionServiceClient permclient = new PermissionServiceClient();
        PublicServiceClient publicClient = new PublicServiceClient();
        public FrmApprovalManagement()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(FrmApprovalManagement_Loaded);
            _VM.GetApprovalTypeByCompanyandDepartmentidCompleted += new EventHandler<GetApprovalTypeByCompanyandDepartmentidCompletedEventArgs>(_VM_GetApprovalTypeByCompanyandDepartmentidCompleted);
            
        }

        #region 数据初始化
        private void InitEvent()
        {
            approvalManagement = new SmtOAPersonOfficeClient();
            approvalManagement.GetApporvalListCompleted += new EventHandler<GetApporvalListCompletedEventArgs>(calendarManagement_GetApporvalListCompleted);
            approvalManagement.DeleteApporvalListCompleted += new EventHandler<DeleteApporvalListCompletedEventArgs>(approvalManagement_DeleteApporvalListCompleted);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            //permclient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(permclient_GetSysDictionaryByCategoryCompleted);
            //permclient.GetSysDictionaryByCategoryByUpdateDateCompleted += new EventHandler<GetSysDictionaryByCategoryByUpdateDateCompletedEventArgs>(permclient_GetSysDictionaryByCategoryByUpdateDateCompleted);
            //permclient.GetSysDictionaryByCategoryAsync("TYPEAPPROVAL");
            //DateTime AA = new DateTime();
            //permclient.GetSysDictionaryByCategoryByUpdateDateAsync("TYPEAPPROVAL",AA);
        }

        void permclient_GetSysDictionaryByCategoryByUpdateDateCompleted(object sender, GetSysDictionaryByCategoryByUpdateDateCompletedEventArgs e)
        {
            List<V_Dictionary> bb = e.Result.ToList();
            //throw new NotImplementedException();
        }

        void permclient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            List<T_SYS_DICTIONARY> aa = e.Result.ToList();
            //throw new NotImplementedException();
        }
        #endregion

        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_APPROVALINFO> selectItems = GetSelectedList(Permissions.Edit);
            if (selectItems != null)
            {
                approvalinfoInfo = selectItems.FirstOrDefault();
                ApprovalForm_aud form = new ApprovalForm_aud(FormTypes.Resubmit, approvalinfoInfo.APPROVALID);
                form.ApprovalInfo = approvalinfoInfo;
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                browser.MinWidth = 750;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        #endregion


        SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();
        void FrmApprovalManagement_Loaded(object sender, RoutedEventArgs e)
        {
            
            this.dpStart.Text = System.DateTime.Now.AddDays(-20).ToShortDateString();
            this.dpEnd.Text = System.DateTime.Now.ToShortDateString();//30天
            txtOwnerName.Text = Common.CurrentLoginUserInfo.EmployeeName;
            //SearchUserID = Common.CurrentLoginUserInfo.EmployeeID;//初始化员工信息
            InitEvent();
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_APPROVALINFO");
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_APPROVALINFO", true);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            SearchUserID = Common.CurrentLoginUserInfo.EmployeeID;
            string Name = "";
            Name = Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-" + Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            txtOwnerName.Text = Name;
            ToolTipService.SetToolTip(txtOwnerName, Name);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看


            OwnerCompanyid = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            OwnerDepartmentid = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            _VM.GetApprovalTypeByCompanyandDepartmentidAsync(OwnerCompanyid, OwnerDepartmentid);
        }


        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "T_OA_APPROVALINFO");
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }

        void dataPager_PageIndexChanged(object sender, EventArgs e)
        {
            GetData();
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (dg.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dg.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_OA_APPROVALINFO ent = dg.SelectedItems[0] as T_OA_APPROVALINFO;
            if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                ApprovalForm_aud frm = new ApprovalForm_aud(FormTypes.Audit, ent.APPROVALID);
                frm.ApprovalInfo = selApporvalInfo;
                EntityBrowser browser = new EntityBrowser(frm);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 760;
                browser.MinHeight = 340;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDNOTOPERATEPLEASEAGAIN"));
                return;
            }
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_APPROVALINFO> selApporvalInfoList = GetSelectedList(Permissions.Browse);
            if (selApporvalInfoList == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            selApporvalInfo = selApporvalInfoList.FirstOrDefault();
            ApprovalForm_aud frm = new ApprovalForm_aud(FormTypes.Browse, selApporvalInfo.APPROVALID);
            //frm.ApprovalInfo = selApporvalInfo;
            EntityBrowser browser = new EntityBrowser(frm);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 760;
            browser.MinHeight = 360;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_APPROVALINFO");
                checkState = dict.DICTIONARYVALUE.ToString();
                GetData();
            }
        }
        void approvalManagement_DeleteApporvalListCompleted(object sender, DeleteApporvalListCompletedEventArgs e)
        {
            int n = e.Result;
            PopuMsg(n, n > 0 ? "DELETESUCCESSED" : "DELETEFAILED");
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {

            ApprovalForm_aud frm = new ApprovalForm_aud();
            EntityBrowser browser = new EntityBrowser(frm);
            browser.MinWidth = 860;
            browser.MinHeight = 500;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);

            //BusinessApplicationsForm AddWin = new BusinessApplicationsForm();
            //EntityBrowser browser = new EntityBrowser(AddWin);
            //browser.EntityBrowseToolBar.MaxHeight = 0;
            //AddWin.ParentEntityBrowser = browser;
            //browser.MinWidth = 980;
            //browser.MinHeight = 380;
            //browser.TitleContent = "出差申请";
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="selDateTime"></param>
        private void GetData()
        {
            
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            string StrStart = this.dpStart.Text.Trim().ToString();
            string StrEnd = this.dpEnd.Text.Trim().ToString();
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值 
            if (!string.IsNullOrEmpty(txtTitle.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "@"+  paras.Count().ToString()+".Contains(APPROVALTITLE) ";
                paras.Add(txtTitle.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtCode.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "@" + paras.Count().ToString() + ".Contains(APPROVALCODE) ";
                paras.Add(txtCode.Text.Trim());
            }
            if (!string.IsNullOrEmpty(StrApprovaltype))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter +=" typeapproval=@" + paras.Count().ToString() ;
                paras.Add(StrApprovaltype);
            }
            if (SearchUserID == Common.CurrentLoginUserInfo.EmployeeID)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "( OWNERID =@" + paras.Count().ToString();
                paras.Add(SearchUserID);//员工ID值
                filter += " or ";
                filter += "CREATEUSERID =@" + paras.Count().ToString();//添加人的ID
                paras.Add(SearchUserID);
                filter += " ) ";
            }
            else
            {
                if (!string.IsNullOrEmpty(SearchUserID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OWNERID =@" + paras.Count().ToString();
                    paras.Add(SearchUserID);
                }
            }
            
            if (!string.IsNullOrEmpty(SearchCompanyID))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "OWNERCOMPANYID =@" + paras.Count().ToString();
                paras.Add(SearchCompanyID);
            }
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("dtSearch"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                return;
            }
            if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("dtSearch"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                return;
            }
            if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                if (DtStart > DtEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("dtSearch"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                    return;
                }
                else
                {

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "CREATEDATE >=@" + paras.Count().ToString();
                    paras.Add(DtStart);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "CREATEDATE <=@" + paras.Count().ToString();
                    paras.Add(DtEnd);

                }
            }
            try
            {
                loadbar.Start();
                SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
                loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
                approvalManagement.GetApporvalListAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
            }
            catch (Exception ex)
            {
                loadbar.Stop();
            }
        }

        void calendarManagement_GetApporvalListCompleted(object sender, GetApporvalListCompletedEventArgs e)
        {
            loadbar.Stop();
            
            
            List<T_OA_APPROVALINFO> calendarList = null;
            if (e.Result != null)
            {
                calendarList = e.Result.ToList();
            }
            BindDgv(calendarList,e.pageCount);
        }
        #endregion

        #region 绑定网格
        /// <summary>
        /// 列表显示
        /// </summary>
        private void BindDgv(List<T_OA_APPROVALINFO> vehicleList,int pagecount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pagecount);
            if (vehicleList == null || vehicleList.Count < 1)
            {
                dg.ItemsSource = null;
                return;
            }
            dg.ItemsSource = vehicleList;

            //if (vehicleList != null && vehicleList.Count > 0)
            //{
            //    PagedCollectionView pcv = new PagedCollectionView(vehicleList);
            //    pcv.PageSize = 20;
            //    dataPager.DataContext = pcv;
            //    dg.ItemsSource = pcv;
            //}
            //else
            //    dg.ItemsSource = null;
        }
        #endregion

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_APPROVALINFO> selApporvalInfoList = GetSelectedList(Permissions.Edit);
            if (dg.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dg.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            selApporvalInfo = selApporvalInfoList[0];
            if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(selApporvalInfo, "T_OA_APPROVALINFO", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
            {
                if (selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() )
                {
                    //ApprovalForm_upd frm = new ApprovalForm_upd(FormTypes.Edit, selApporvalInfo.APPROVALID);
                    ApprovalForm_aud frm = new ApprovalForm_aud(FormTypes.Edit, selApporvalInfo.APPROVALID);
                    //Border aa = new Border();
                    //Utility.CreateFormFromEngine(selApporvalInfo.APPROVALID, "SMT.SaaS.OA.UI.UserControls.ApprovalForm_aud", "Audit", aa);
                    frm.ApprovalInfo = selApporvalInfo;
                    EntityBrowser browser = new EntityBrowser(frm);
                    browser.FormType = FormTypes.Edit;
                    browser.MinWidth = 860;
                    browser.MinHeight = 500;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            }
            else
            { 
                
                    
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                
            }
        }

        void browser_ReloadDataEvent()
        {
            GetData();
        }

        #region 删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (dg.SelectedItems.Count > 0)
            {
                ObservableCollection<T_OA_APPROVALINFO> selectItemList = new ObservableCollection<T_OA_APPROVALINFO>();
                foreach (object obj in dg.SelectedItems)
                {

                    if (((T_OA_APPROVALINFO) obj).CHECKSTATE == ((int) CheckStates.UnSubmit).ToString())
                    {
                        selectItemList.Add((T_OA_APPROVALINFO) obj);
                    }
                    else
                    {
                        string strState = ((T_OA_APPROVALINFO) obj).CHECKSTATE;
                        switch (strState)
                        {
                            case "1":
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "不能删除审核中的数据",
                                                               Utility.GetResourceStr("CONFIRM"),
                                                               MessageIcon.Exclamation);
                                break;
                            case "2":
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "不能删除审核通过的数据",
                                                               Utility.GetResourceStr("CONFIRM"),
                                                               MessageIcon.Exclamation);
                                break;
                            case "3":
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "不能删除审核不通过的数据",
                                                               Utility.GetResourceStr("CONFIRM"),
                                                               MessageIcon.Exclamation);
                                break;
                        }
                        return;
                    }



                }
                if (selectItemList == null)
                {
                    //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"),
                                                   Utility.GetResourceStr("SELECTERROR", "DELETE"),
                                                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }

                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (objcom, result) =>
                                                {

                                                    if (selectItemList.Count > 0)
                                                    {
                                                        try
                                                        {
                                                            string StrTip = "";
                                                            int i = 0;
                                                            bool IsTrue = true;
                                                            foreach (object obj in selectItemList)
                                                            {
                                                                if (
                                                                    !(SMT.SaaS.FrameworkUI.Common.Utility.
                                                                         ToolBarButtonOperationPermission(
                                                                             (T_OA_APPROVALINFO) obj,
                                                                             "T_OA_APPROVALINFO", OperationType.Delete,
                                                                             Common.CurrentLoginUserInfo.EmployeeID)))
                                                                {
                                                                    StrTip = "您不能删除您选中的第" + (i + 1).ToString() + "条，标题为" +
                                                                             ((T_OA_APPROVALINFO) obj).APPROVALTITLE +
                                                                             "的信息";
                                                                    ComfirmWindow.ConfirmationBoxs(
                                                                        Utility.GetResourceStr("ERROR"), StrTip,
                                                                        Utility.GetResourceStr("CONFIRM"),
                                                                        MessageIcon.Exclamation);
                                                                    IsTrue = false;
                                                                    return;
                                                                }
                                                            }
                                                            
                                                            approvalManagement.DeleteApporvalListAsync(selectItemList);
                                                        }
                                                        catch
                                                        {

                                                        }
                                                    }
                                                    else
                                                    {
                                                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYDIDNOSUBMITANDREVIEWTHEDATACANBEDELETEDBY"));
                                                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"),
                                                                                       Utility.GetResourceStr(
                                                                                           "NOTONLYDIDNOSUBMITANDREVIEWTHEDATACANBEDELETEDBY"),
                                                                                       Utility.GetResourceStr("CONFIRM"),
                                                                                       MessageIcon.Exclamation);
                                                    }
                                                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"),
                                 ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox("确认信息", "请先选择需要删除的记录", "确定");
            }
        }


        //获取选中列表
        private ObservableCollection<T_OA_APPROVALINFO> GetSelectedList(Permissions PermissionState)
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_APPROVALINFO> selectItemList = new ObservableCollection<T_OA_APPROVALINFO>();
                if (PermissionState == Permissions.Delete)
                {
                    
                    foreach (object obj in dg.SelectedItems)
                    {
                        
                        if (((T_OA_APPROVALINFO)obj).CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                        {
                            selectItemList.Add((T_OA_APPROVALINFO)obj);
                        }
                        

                    }
                }
                else
                {
                    foreach (object obj in dg.SelectedItems)
                    {                        
                        selectItemList.Add((T_OA_APPROVALINFO)obj);                       

                    }
                }
                if (selectItemList.Count > 0)
                {
                    return selectItemList;
                }
            }
            return null;
        }
        #endregion
        private T_OA_APPROVALINFO selApporvalInfo = null;

        //查看详情+提交审批
        private void btnShowDetail_Click(object sender, RoutedEventArgs e)
        {
            if (selApporvalInfo == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }
        /// <summary>
        /// 提示框
        /// </summary>
        /// <param name="n"></param>
        /// <param name="tip"></param>
        private void PopuMsg(int n, string tip)
        {
            if (n > 0)
            {
                GetData();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr(tip, ""));
            }
            else
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(tip, ""));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dataPager.PageIndex = 1;   
            GetData();
        }

        #region 重置按钮
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

            this.dpStart.Text = "";
            this.dpEnd.Text = "";
            txtTitle.Text="";
            txtCode.Text="";
            SearchUserID="";
            SearchPostID = "";
            SearchDepartID = "";
            SearchCompanyID = "";
            txtOwnerName.Text = "";
            StrApprovaltype = string.Empty;
            txtSelectPost.TxtSelectedApprovalType.Text = string.Empty;
            
        }
        #endregion


        #region 选择人员
        private void btnLookUpOwner_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    //SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;//岗位
                    SearchPostID = postid;
                    //postLevel = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID == postid).FirstOrDefault().POSTLEVEL.ToString();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    string deptName = dept.ObjectName;//部门
                    string depName = dept.ObjectName;//部门
                    SearchDepartID = deptid;

                    

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string corpName = corp.CNAME;//公司
                    SearchCompanyID = corpid;
                    
                    //txtOwnerName.Text = userInfo.ObjectName;
                    string Mobile = "";
                    string Tel = "";
                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE != null)
                        Mobile = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE.ToString();
                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE != null)
                        Tel = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE.ToString();
                    string StrEmployee = userInfo.ObjectName + "-" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME;
                    txtOwnerName.Text = StrEmployee;
                    //txtTel.Text = userInfo.te
                    ToolTipService.SetToolTip(txtOwnerName, StrEmployee);

                    SearchUserID = userInfo.ObjectID;
                    txtOwnerName.Text = StrEmployee;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion



        #region 选择事项审核类型

        //ObservableCollection<string> lstApprovalids = new ObservableCollection<string>();
        private void txtSelectApprovalType_SelectClick(object sender, EventArgs e)
        {
            SelectApprovalType txt = (SelectApprovalType)sender;
            string StrOld = txt.TxtSelectedApprovalType.Text.ToString();
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters.Add("CHARGEMONEY", fbCtr.Order.TOTALMONEY.ToString());
            //parameters.Add("CHARGEMONEY", approvalInfo.CHARGEMONEY.ToString());
            //parameters.Add("POSTLEVEL", postLevel);
            //parameters.Add("DEPARTMENTNAME", depName);
            //strXmlObjectSource = Utility.ObjListToXmlForTravel<T_OA_APPROVALINFO>(approvalInfo, "OA", parameters);
            ApprovalTypeList apptype = new ApprovalTypeList(StrOld, StrApprovaltype, lstApprovalids, OwnerCompanyid, OwnerDepartmentid, strXmlObjectSource);

            //ApprovalTypeList apptype = new ApprovalTypeList(StrOld, StrApprovaltype, lstApprovalids, OwnerCompanyid, OwnerDepartmentid, strXmlObjectSource);

            apptype.SelectedClicked += (obj, ea) =>
            {
                StrApprovaltype = "";
                string StrPost = apptype.Result.Keys.FirstOrDefault();
                if (!string.IsNullOrEmpty(StrPost))
                {
                    txt.TxtSelectedApprovalType.Text = StrPost;
                    //StrApprovalTypeName = StrPost;//用于传递给手机
                }
                StrApprovaltype = apptype.Result[apptype.Result.Keys.FirstOrDefault()].ToString();
                //根据选择回来的审批类型获取父值
                //将父级的值清为空
                StrApprovalOne = "";
                StrApprovalTwo = "";
                StrApprovalThird = "";
                GetFatherApprovalType(StrApprovaltype, "first");
                //_VM.Get_ApporvalTempletByApporvalTypeAsync(StrApprovaltype);
                RefreshUI(RefreshedTypes.ShowProgressBar);

            };
            var windows = SMT.SAAS.Controls.Toolkit.Windows.ProgramManager.ShowProgram(Utility.GetResourceStr("SELECTAPPROVALTYPE"), "", "123", apptype, false, false, null);
            if (apptype is ApprovalTypeList)
            {
                (apptype as ApprovalTypeList).Close += (o, args) =>
                {
                    windows.Close();
                };
            }
        }
        /// <summary>
        /// 获取 选取的事项审批的类型 父级的 字典值
        /// </summary>
        /// <param name="apptype"></param>
        /// <param name="forcount"></param>
        private void GetFatherApprovalType(string apptype, string forcount)
        {
            //获取缓存--字典值
            try
            {
                List<T_SYS_DICTIONARY> Dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;

                if (Dicts == null)
                {
                    return;
                }
                List<T_SYS_DICTIONARY> TopApproval = new List<T_SYS_DICTIONARY>();
                //获取  事项审批类型的字典集合
                var ents = from p in Dicts
                           where p.DICTIONCATEGORY == "TYPEAPPROVAL" && p.DICTIONARYVALUE == System.Convert.ToInt16(apptype)
                           orderby p.ORDERNUMBER
                           select p;
                T_SYS_DICTIONARY dict = new T_SYS_DICTIONARY();
                if (ents.Count() > 0)
                {
                    dict = ents.FirstOrDefault();
                    //获取父值的信息
                    if (dict.T_SYS_DICTIONARY2 != null)
                    {
                        var firstents = from ent in Dicts
                                        where ent.DICTIONCATEGORY == "TYPEAPPROVAL" && (ent.DICTIONARYID == dict.T_SYS_DICTIONARY2.DICTIONARYID && dict.T_SYS_DICTIONARY2 != null)
                                        orderby ent.ORDERNUMBER
                                        select ent;
                        if (firstents.Count() > 0)
                        {
                            if (forcount == "first")
                            {
                                StrApprovalOne = firstents.FirstOrDefault().DICTIONARYVALUE.ToString();
                                GetFatherApprovalType(StrApprovalOne, "second");
                            }
                            if (forcount == "second")
                            {
                                StrApprovalTwo = firstents.FirstOrDefault().DICTIONARYVALUE.ToString();
                                GetFatherApprovalType(StrApprovalTwo, "second");
                            }
                            if (forcount == "third")
                            {
                                StrApprovalThird = firstents.FirstOrDefault().DICTIONARYVALUE.ToString();
                                GetFatherApprovalType(StrApprovalThird, "third");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SetLogAndShowLog(ex.ToString());
            }
        }

        void _VM_Get_ApporvalTempletByApporvalTypeCompleted(object sender, Get_ApporvalTempletByApporvalTypeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            T_OA_APPROVALINFOTEMPLET templet = e.Result;
            if (templet != null)
            {
                txtTitle.Text = templet.APPROVALTITLE;
                //txtContent.Document = templet.CONTENT;
            }
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


        #region 获取公司或部门的事项审批类型

        ObservableCollection<string> lstApprovalids = new ObservableCollection<string>();
        void _VM_GetApprovalTypeByCompanyandDepartmentidCompleted(object sender, GetApprovalTypeByCompanyandDepartmentidCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                lstApprovalids.Clear();
                lstApprovalids = e.Result;
                List<T_SYS_DICTIONARY> Dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;

                if (Dicts == null)
                    return;

                T_SYS_DICTIONARY DictApproval = new T_SYS_DICTIONARY();

                var ents = from p in Dicts
                           where p.DICTIONCATEGORY == "TYPEAPPROVAL" && p.T_SYS_DICTIONARY2 != null && lstApprovalids.Contains(p.DICTIONARYVALUE.ToString())
                           orderby p.ORDERNUMBER
                           select p;
                if (ents.Count() > 0)
                {
                }
                else
                {
                }
                StrApprovaltype = "";
                txtSelectPost.TxtSelectedApprovalType.Text = "";
                if (DictApproval != null)
                {
                    if (!string.IsNullOrEmpty(DictApproval.DICTIONARYID))//存在则赋值
                    {
                        txtSelectPost.TxtSelectedApprovalType.Text = DictApproval.DICTIONARYNAME;
                        StrApprovaltype = DictApproval.DICTIONARYVALUE.ToString();
                        GetFatherApprovalType(StrApprovaltype, "first");
                    }
                }

            }
        }

        #endregion
    }
}