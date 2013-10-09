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
    public partial class FrmApprovalTempletManagement : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private string checkState = ((int)CheckStates.ALL).ToString();
        private SmtOAPersonOfficeClient OaPersonOfficeClient;
        private string SearchUserID = "";//已员工名为查询条件
        private string SearchPostID = "";//所属于人的岗位
        private string SearchDepartID = "";//所属于人的部门ID
        private string SearchCompanyID = "";//所属人的公司ID
        private T_OA_APPROVALINFOTEMPLET selApporvalInfo = null;
        
        #region 构造初始化及FormLoad
        public FrmApprovalTempletManagement()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(FrmApprovalManagement_Loaded);

        }

        private void InitEvent()
        {
            OaPersonOfficeClient = new SmtOAPersonOfficeClient();
            OaPersonOfficeClient.GetApporvalTempletListCompleted += new EventHandler<GetApporvalTempletListCompletedEventArgs>(approvalManagement_GetApporvalTempletListCompleted);
            OaPersonOfficeClient.DeleteApporvalTempletListCompleted += OaPersonOfficeClient_DeleteApporvalTempletListCompleted;
        }



        void FrmApprovalManagement_Loaded(object sender, RoutedEventArgs e)
        {

            this.dpStart.Text = System.DateTime.Now.AddDays(-20).ToShortDateString();
            this.dpEnd.Text = System.DateTime.Now.ToShortDateString();//30天
            txtOwnerName.Text = Common.CurrentLoginUserInfo.EmployeeName;
            InitEvent();
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_APPROVALINFOTEMPLET");
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_APPROVALINFOTEMPLET", true);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            SearchUserID = Common.CurrentLoginUserInfo.EmployeeID;
            string Name = "";
            Name = Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-" + Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            txtOwnerName.Text = Name;
            ToolTipService.SetToolTip(txtOwnerName, Name);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.ShowView(false);
            GetData();
        }

        #endregion
        
        #region 获取列表数据后绑定DataGrid

        void approvalManagement_GetApporvalTempletListCompleted(object sender, GetApporvalTempletListCompletedEventArgs e)
        {
            loadbar.Stop();

            List<T_OA_APPROVALINFOTEMPLET> calendarList = null;
            if (e.Result != null)
            {
                calendarList = e.Result.ToList();
            }
            BindDgv(calendarList, e.pageCount);
        }
        /// <summary>
        /// 列表显示
        /// </summary>
        private void BindDgv(List<T_OA_APPROVALINFOTEMPLET> vehicleList,int pagecount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pagecount);
            if (vehicleList == null || vehicleList.Count < 1)
            {
                dg.ItemsSource = null;
                return;
            }
            dg.ItemsSource = vehicleList;

        }
        #endregion

        #region 增删改查
        //新建
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {

            ApprovalTempletForm frm = new ApprovalTempletForm();
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
        //修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_APPROVALINFOTEMPLET> selApporvalInfoList = GetSelectedList(Permissions.Edit);
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
            if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(selApporvalInfo, "T_OA_APPROVALINFOTEMPLET", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
            {
                if (selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                {
                    //ApprovalForm_upd frm = new ApprovalForm_upd(FormTypes.Edit, selApporvalInfo.APPROVALID);
                    ApprovalTempletForm frm = new ApprovalTempletForm(FormTypes.Edit, selApporvalInfo.APPROVALID);
                    //Border aa = new Border();
                    //Utility.CreateFormFromEngine(selApporvalInfo.APPROVALID, "SMT.SaaS.OA.UI.UserControls.ApprovalTempletForm", "Audit", aa);
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

        //查看详情+提交审批
        private void btnShowDetail_Click(object sender, RoutedEventArgs e)
        {
            if (selApporvalInfo == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        #region 删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (dg.SelectedItems.Count > 0)
            {
                ObservableCollection<T_OA_APPROVALINFOTEMPLET> selectItemList = new ObservableCollection<T_OA_APPROVALINFOTEMPLET>();
                foreach (object obj in dg.SelectedItems)
                {

                    if (((T_OA_APPROVALINFOTEMPLET)obj).CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        selectItemList.Add((T_OA_APPROVALINFOTEMPLET)obj);
                    }
                    else
                    {
                        string strState = ((T_OA_APPROVALINFOTEMPLET)obj).CHECKSTATE;
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
                                             (T_OA_APPROVALINFOTEMPLET)obj,
                                             "T_OA_APPROVALINFOTEMPLET", OperationType.Delete,
                                             Common.CurrentLoginUserInfo.EmployeeID)))
                                {
                                    StrTip = "您不能删除您选中的第" + (i + 1).ToString() + "条，标题为" +
                                             ((T_OA_APPROVALINFOTEMPLET)obj).APPROVALTITLE +
                                             "的信息";
                                    ComfirmWindow.ConfirmationBoxs(
                                        Utility.GetResourceStr("ERROR"), StrTip,
                                        Utility.GetResourceStr("CONFIRM"),
                                        MessageIcon.Exclamation);
                                    IsTrue = false;
                                    return;
                                }
                            }

                            OaPersonOfficeClient.DeleteApporvalTempletListAsync(selectItemList);
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
        private ObservableCollection<T_OA_APPROVALINFOTEMPLET> GetSelectedList(Permissions PermissionState)
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_APPROVALINFOTEMPLET> selectItemList = new ObservableCollection<T_OA_APPROVALINFOTEMPLET>();
                if (PermissionState == Permissions.Delete)
                {

                    foreach (object obj in dg.SelectedItems)
                    {

                        if (((T_OA_APPROVALINFOTEMPLET)obj).CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                        {
                            selectItemList.Add((T_OA_APPROVALINFOTEMPLET)obj);
                        }


                    }
                }
                else
                {
                    foreach (object obj in dg.SelectedItems)
                    {
                        selectItemList.Add((T_OA_APPROVALINFOTEMPLET)obj);

                    }
                }
                if (selectItemList.Count > 0)
                {
                    return selectItemList;
                }
            }
            return null;
        }

        void OaPersonOfficeClient_DeleteApporvalTempletListCompleted(object sender, DeleteApporvalTempletListCompletedEventArgs e)
        {
            int n = e.Result;
            PopuMsg(n, n > 0 ? "DELETESUCCESSED" : "DELETEFAILED");
        }
        #endregion

        //查看
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_APPROVALINFOTEMPLET> selApporvalInfoList = GetSelectedList(Permissions.Browse);
            if (selApporvalInfoList == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            selApporvalInfo = selApporvalInfoList.FirstOrDefault();
            ApprovalTempletForm frm = new ApprovalTempletForm(FormTypes.Browse, selApporvalInfo.APPROVALID);
            //frm.ApprovalInfo = selApporvalInfo;
            EntityBrowser browser = new EntityBrowser(frm);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 760;
            browser.MinHeight = 360;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
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
                filter += "@" + paras.Count().ToString() + ".Contains(APPROVALTITLE) ";
                paras.Add(txtTitle.Text.Trim());
            }
            //if (!string.IsNullOrEmpty(txtCode.Text.Trim()))
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "@" + paras.Count().ToString() + ".Contains(APPROVALCODE) ";
            //    //paras.Add(txtCode.Text.Trim());
            //}
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
                OaPersonOfficeClient.GetApporvalTempletListAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
            }
            catch (Exception ex)
            {
                loadbar.Stop();
            }
        }

        #endregion

        //搜索
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dataPager.PageIndex = 1;
            GetData();
        }
        //刷新
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }
        //分页控件事件
        void dataPager_PageIndexChanged(object sender, EventArgs e)
        {
            GetData();
        }
        #endregion

        #region 页面控件事件


        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "T_OA_APPROVALINFOTEMPLET");
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

        void browser_ReloadDataEvent()
        {
            GetData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

            this.dpStart.Text = "";
            this.dpEnd.Text = "";
            txtTitle.Text="";
            //txtCode.Text="";
            SearchUserID="";
            SearchPostID = "";
            SearchDepartID = "";
            SearchCompanyID = "";
            txtOwnerName.Text = "";
            
        }

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
        #endregion

    }
}