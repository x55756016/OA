using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.Globalization;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class EmployeeLeaveDays : BasePage, IClient
    {
        #region 全局变量

        private class EmployeeLeaveMode
        {
            public string EMPLOYEENAME { get; set; }
            public string VACATIONTYPE { get; set; }
            public decimal? DAYS { get; set; }
            public DateTime? EFFICDATE { get; set; }
            public string TERMINATEDATE { get; set; }
        }

        AttendanceServiceClient client;
        OrganizationServiceClient orgClient;
        private SMTLoading loadbar = new SMTLoading();

        private List<T_HR_COMPANY> allCompanys;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_POST> allPositions;
        //请假类型
        public string strleaveType { get; set; }
        #endregion

        #region 初始化
        public EmployeeLeaveDays()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeLeaveDays_Loaded);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            Nuyear.Value = DateTime.Now.Year;
            NuStartmounth.Value = DateTime.Now.Month;
            NuEndmounth.Value = DateTime.Now.Month;
            tbEmpSex.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SexID;
            client.GetEmployeeLevelDayCountRdListByMultSearchCompleted += new EventHandler<GetEmployeeLevelDayCountRdListByMultSearchCompletedEventArgs>(client_GetEmployeeLevelDayCountRdListByMultSearchCompleted);

            //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            //loadbar.Start();
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            BindGrid();
        }


        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            this.dgEmpLeaveDays1.ItemsSource = null;

            string strEmployeeID = string.Empty, strOrgType = string.Empty, strOrgValue = string.Empty;
            string strSortKey = string.Empty, strOwnerID = string.Empty;
            string strEfficDateFrom = string.Empty, strEfficDateTo = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            strSortKey = " EFFICDATE ";
            CheckInputFilter(ref strEmployeeID, ref strOrgType, ref strOrgValue, ref strEfficDateFrom, ref strEfficDateTo);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;
            client.GetEmployeeLevelDayCountRdListByMultSearchAsync(strOrgType, strOrgValue, strleaveType, strOwnerID, strEmployeeID, strEfficDateFrom, strEfficDateTo, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }


        /// <summary>
        /// 校验输入的查询条件
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="strOrgType"></param>
        /// <param name="strOrgValue"></param>
        /// <param name="strEfficDateFrom"></param>
        /// <param name="strEfficDateTo"></param>
        private void CheckInputFilter(ref string strEmployeeID, ref string strOrgType, ref string strOrgValue, ref string strEfficDateFrom, ref string strEfficDateTo)
        {
            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    strEmployeeID = ent.EMPLOYEEID.Trim();
                }
            }
            strOrgType = treeOrganization.sType;
            strOrgValue = treeOrganization.sValue;
            //TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            //if (selectedItem != null)
            //{
            //    string IsTag = selectedItem.Tag.ToString();
            //    switch (IsTag)
            //    {
            //        case "0":
            //            T_HR_COMPANY company = selectedItem.DataContext as T_HR_COMPANY;
            //            strOrgType = "Company";
            //            strOrgValue = company.COMPANYID;
            //            break;
            //        case "1":
            //            T_HR_DEPARTMENT department = selectedItem.DataContext as T_HR_DEPARTMENT;
            //            strOrgType = "Department";
            //            strOrgValue = department.DEPARTMENTID;
            //            break;
            //        case "2":
            //            T_HR_POST post = selectedItem.DataContext as T_HR_POST;
            //            strOrgType = "Post";
            //            strOrgValue = post.POSTID;
            //            break;
            //    }
            //}

            strEfficDateFrom = Nuyear.Value.ToString() + "-" + NuStartmounth.Value.ToString() + "-1";
            if (DateTime.Parse(strEfficDateFrom) <= DateTime.Parse("1900-1-1"))
            {
                strEfficDateFrom = string.Empty;
            }

            int iMaxDay = DateTime.DaysInMonth(Nuyear.Value.ToInt32(), NuEndmounth.Value.ToInt32());
            strEfficDateTo = Nuyear.Value.ToString() + "-" + NuEndmounth.Value.ToString() + "-" + iMaxDay.ToString();
            if (DateTime.Parse(strEfficDateTo) <= DateTime.Parse("1900-1-1"))
            {
                strEfficDateTo = string.Empty;
            }
        }
        #endregion

        #region 事件
        void EmployeeLeaveDays_Loaded(object sender, RoutedEventArgs e)
        {
            this.dgEmpLeaveDays.SelectionChanged += new SelectionChangedEventHandler(dgEmpLeaveDays_SelectionChanged);

            client = new AttendanceServiceClient();
            orgClient = new OrganizationServiceClient();
            GetEntityLogo("T_HR_EMPLOYEELEVELDAYCOUNT");
            RegisterEvents();
            //  BindTree();
            client.GetEmployeeleaverecordByMultSearchIdCompleted += new EventHandler<GetEmployeeleaverecordByMultSearchIdCompletedEventArgs>(client_GetEmployeeleaverecordByMultSearchIdCompleted);
        }

        void dgEmpLeaveDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_HR_EMPLOYEELEVELDAYCOUNT ENT = this.dgEmpLeaveDays.SelectedItem as T_HR_EMPLOYEELEVELDAYCOUNT;
            if (ENT != null)
            {
                //绑定数据
                //this.dgEmpLeaveDays1.ItemsSource=GetEmployeeleaverecordByMultSearchId
                string strEmployeeID = string.Empty, strOrgType = string.Empty, strOrgValue = string.Empty;
                string strSortKey = string.Empty, strOwnerID = string.Empty;
                string strEfficDateFrom = string.Empty, strEfficDateTo = string.Empty;
                int pageIndex = 0, pageSize = 0, pageCount = 0;

                pageIndex = dataPager1.PageIndex;
                pageSize = dataPager1.PageSize;
                //employeeid, leavetypesetid, OWNERCOMPANYID, strEfficDateFrom, strEfficDateTo,pageIndex, pageSize, ref pageCount
                //T_HR_EMPLOYEELEVELDAYCOUNT ENT=this.dgEmpLeaveDays.SelectedItem as T_HR_EMPLOYEELEVELDAYCOUNT;
                if (ENT!=null)
                {
                    client.GetEmployeeleaverecordByMultSearchIdAsync(ENT.EMPLOYEEID, ENT.LEAVETYPESETID, ENT.OWNERCOMPANYID, ENT.EFFICDATE.ToString(), ENT.TERMINATEDATE.ToString(), pageIndex, pageSize, pageCount);
                }

                loadbar.Start();
            }
        }

        void client_GetEmployeeleaverecordByMultSearchIdCompleted(object sender, GetEmployeeleaverecordByMultSearchIdCompletedEventArgs e)
        {
            if (e.Error==null)
            {
                List<SMT.Saas.Tools.AttendanceWS.T_HR_EMPLOYEELEAVERECORD> list = new List<SMT.Saas.Tools.AttendanceWS.T_HR_EMPLOYEELEAVERECORD>();
                if (e.Result!=null)
                {
                    list = e.Result.ToList();
                }
                this.dgEmpLeaveDays1.ItemsSource = list;
                dataPager1.PageCount = e.pageCount;
            }

            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            loadbar.Stop();
        }

        /// <summary>
        /// 加载员工可休假数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeLevelDayCountRdListByMultSearchCompleted(object sender, GetEmployeeLevelDayCountRdListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_HR_EMPLOYEELEVELDAYCOUNT> list = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                    //foreach (T_HR_EMPLOYEELEVELDAYCOUNT item in e.Result.ToList())
                    //{
                    //    EmployeeLeaveMode EMPLOYEELEVELDAYCOUNT = new EmployeeLeaveMode();

                    //    EMPLOYEELEVELDAYCOUNT.EMPLOYEENAME = item.EMPLOYEENAME;
                    //    EMPLOYEELEVELDAYCOUNT.VACATIONTYPE = item.VACATIONTYPE;
                    //    EMPLOYEELEVELDAYCOUNT.DAYS = item.DAYS;
                    //    EMPLOYEELEVELDAYCOUNT.EFFICDATE = item.EFFICDATE;

                    //    if (item.TERMINATEDATE.ToString() == "9999/12/31 0:00:00")
                    //    {
                    //        EMPLOYEELEVELDAYCOUNT.TERMINATEDATE = "永久";
                    //    }
                    //    else
                    //    {
                    //        EMPLOYEELEVELDAYCOUNT.TERMINATEDATE = item.TERMINATEDATE.ToString();
                    //    }

                    //    list.Add(EMPLOYEELEVELDAYCOUNT);
                    //}
                }

                //var v = from t in list
                //        orderby t.EMPLOYEENAME
                //        select t;
                
                dgEmpLeaveDays.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }

        /// <summary>
        /// 根据查询参数，搜寻符合条件的员工可休假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// Grid首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgEmpLeaveDays_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgEmpLeaveDays, e.Row, "T_HR_EMPLOYEELEVELDAYCOUNT");
        }

        /// <summary>
        /// Grid首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgEmpLeaveDays_LoadingRow1(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgEmpLeaveDays1, e.Row, "T_HR_EMPLOYEELEAVERECORD");
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 获取员工资料
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkEmpName_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                if (ent != null)
                {
                    lkEmpName.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        //#region 树形控件的操作
        ////绑定树
        //private void BindTree()
        //{
        //    if (Application.Current.Resources.Contains("ORGTREESYSCompanyInfo"))
        //    {
        //        BindCompany();
        //    }
        //    else
        //    {
        //        orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //}

        //void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"));
        //    }
        //    else
        //    {
        //        if (e.Result == null)
        //        {
        //            return;
        //        }

        //        ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> entTemps = e.Result;
        //        allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
        //        allCompanys.Clear();
        //        var ents = entTemps.OrderBy(c => c.FATHERID);
        //        ents.ForEach(item =>
        //        {
        //            allCompanys.Add(item);
        //        });

        //        UICache.CreateCache("ORGTREESYSCompanyInfo", allCompanys);
        //        orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //}

        //void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"));
        //    }
        //    else
        //    {
        //        if (e.Result == null)
        //        {
        //            return;
        //        }

        //        ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> entTemps = e.Result;
        //        allDepartments = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
        //        allDepartments.Clear();
        //        var ents = entTemps.OrderBy(c => c.FATHERID);
        //        ents.ForEach(item =>
        //        {
        //            allDepartments.Add(item);
        //        });

        //        UICache.CreateCache("ORGTREESYSDepartmentInfo", allDepartments);

        //        BindCompany();

        //        orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        //    }
        //}

        //void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            allPositions = e.Result.ToList();
        //        }
        //        UICache.CreateCache("ORGTREESYSPostInfo", allPositions);
        //        BindPosition();
        //    }
        //}

        //private void BindCompany()
        //{
        //    treeOrganization.Items.Clear();
        //    allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

        //    allDepartments = Application.Current.Resources["ORGTREESYSDepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

        //    if (allCompanys == null)
        //    {
        //        return;
        //    }

        //    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> TopCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();

        //    foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
        //    {
        //        //如果当前公司没有父机构的ID，则为顶级公司
        //        if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
        //        {
        //            TreeViewItem item = new TreeViewItem();
        //            item.Header = tmpOrg.CNAME;
        //            item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //            item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //            item.DataContext = tmpOrg;

        //            //状态在未生效和撤消中时背景色为红色
        //            SolidColorBrush brush = new SolidColorBrush();
        //            if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //            {
        //                brush.Color = Colors.Red;
        //                item.Foreground = brush;
        //            }
        //            else
        //            {
        //                brush.Color = Colors.Black;
        //                item.Foreground = brush;
        //            }
        //            //标记为公司
        //            item.Tag = OrgTreeItemTypes.Company;
        //            treeOrganization.Items.Add(item);
        //            TopCompany.Add(tmpOrg);
        //        }
        //        else
        //        {
        //            //查询当前公司是否在公司集合内有父公司存在
        //            var ent = from c in allCompanys
        //                      where tmpOrg.FATHERTYPE == "0" && c.COMPANYID == tmpOrg.FATHERID
        //                      select c;
        //            var ent2 = from c in allDepartments
        //                       where tmpOrg.FATHERTYPE == "1" && tmpOrg.FATHERID == c.DEPARTMENTID
        //                       select c;

        //            //如果不存在，则为顶级公司
        //            if (ent.Count() == 0 && ent2.Count() == 0)
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpOrg.CNAME;
        //                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //                item.DataContext = tmpOrg;

        //                //状态在未生效和撤消中时背景色为红色
        //                SolidColorBrush brush = new SolidColorBrush();
        //                if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //                {
        //                    brush.Color = Colors.Red;
        //                    item.Foreground = brush;
        //                }
        //                else
        //                {
        //                    brush.Color = Colors.Black;
        //                    item.Foreground = brush;
        //                }
        //                //标记为公司
        //                item.Tag = OrgTreeItemTypes.Company;
        //                treeOrganization.Items.Add(item);

        //                TopCompany.Add(tmpOrg);
        //            }
        //        }
        //    }
        //    //开始递归
        //    foreach (var topComp in TopCompany)
        //    {
        //        TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, topComp.COMPANYID);
        //        List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany = (from ent in allCompanys
        //                                                                      where ent.FATHERTYPE == "0"
        //                                                                      && ent.FATHERID == topComp.COMPANYID
        //                                                                      select ent).ToList();

        //        List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment = (from ent in allDepartments
        //                                                                            where ent.FATHERID == topComp.COMPANYID && ent.FATHERTYPE == "0"
        //                                                                            select ent).ToList();

        //        AddOrgNode(lsCompany, lsDepartment, parentItem);
        //    }
        //    allPositions = Application.Current.Resources["ORGTREESYSPostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
        //    if (allPositions != null)
        //    {
        //        BindPosition();
        //    }
        //}

        //private void AddOrgNode(List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany, List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
        //{
        //    //绑定公司的子公司
        //    foreach (var childCompany in lsCompany)
        //    {
        //        TreeViewItem item = new TreeViewItem();
        //        item.Header = childCompany.CNAME;
        //        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //        item.DataContext = childCompany;
        //        //状态在未生效和撤消中时背景色为红色
        //        SolidColorBrush brush = new SolidColorBrush();
        //        if (childCompany.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //        {
        //            brush.Color = Colors.Red;
        //            item.Foreground = brush;
        //        }
        //        else
        //        {
        //            brush.Color = Colors.Black;
        //            item.Foreground = brush;
        //        }
        //        //标记为公司
        //        item.Tag = OrgTreeItemTypes.Company;
        //        FatherNode.Items.Add(item);

        //        if (lsCompany.Count() > 0)
        //        {
        //            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
        //                                                                          where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
        //                                                                          select ent).ToList();
        //            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
        //                                                                             where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
        //                                                                             select ent).ToList();

        //            AddOrgNode(lsTempCom, lsTempDep, item);
        //        }
        //    }
        //    //绑定公司下的部门
        //    foreach (var childDepartment in lsDepartment)
        //    {
        //        TreeViewItem item = new TreeViewItem();
        //        item.Header = childDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //        item.DataContext = childDepartment;
        //        item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
        //        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //        //状态在未生效和撤消中时背景色为红色
        //        SolidColorBrush brush = new SolidColorBrush();
        //        if (childDepartment.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //        {
        //            brush.Color = Colors.Red;
        //            item.Foreground = brush;
        //        }
        //        else
        //        {
        //            brush.Color = Colors.Black;
        //            item.Foreground = brush;
        //        }
        //        //标记为部门
        //        item.Tag = OrgTreeItemTypes.Department;
        //        FatherNode.Items.Add(item);

        //        if (lsDepartment.Count() > 0)
        //        {
        //            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
        //                                                                          where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
        //                                                                          select ent).ToList();
        //            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
        //                                                                             where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
        //                                                                             select ent).ToList();

        //            AddOrgNode(lsTempCom, lsTempDep, item);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 绑定岗位
        ///// </summary>
        //private void BindPosition()
        //{
        //    if (allPositions != null)
        //    {
        //        foreach (SMT.Saas.Tools.OrganizationWS.T_HR_POST tmpPosition in allPositions)
        //        {
        //            if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
        //                continue;
        //            TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Department, tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
        //            if (parentItem != null)
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
        //                item.DataContext = tmpPosition;
        //                item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //                //状态在未生效和撤消中时背景色为红色
        //                SolidColorBrush brush = new SolidColorBrush();
        //                if (tmpPosition.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //                {
        //                    brush.Color = Colors.Red;
        //                    item.Foreground = brush;
        //                }
        //                else
        //                {
        //                    brush.Color = Colors.Black;
        //                    item.Foreground = brush;
        //                }
        //                //标记为岗位
        //                item.Tag = OrgTreeItemTypes.Post;
        //                parentItem.Items.Add(item);
        //            }
        //        }
        //    }
        //    //树全部展开
        //    //  treeOrganization.ExpandAll();
        //    if (treeOrganization.Items.Count > 0)
        //    {
        //        TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
        //        selectedItem.IsSelected = true;
        //    }
        //}

        ///// <summary>
        ///// 获取节点
        ///// </summary>
        ///// <param name="parentType"></param>
        ///// <param name="parentID"></param>
        ///// <returns></returns>
        //private TreeViewItem GetParentItem(OrgTreeItemTypes parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    foreach (TreeViewItem item in treeOrganization.Items)
        //    {
        //        tmpItem = GetParentItemFromChild(item, parentType, parentID);
        //        if (tmpItem != null)
        //        {
        //            break;
        //        }
        //    }
        //    return tmpItem;
        //}

        //private TreeViewItem GetParentItemFromChild(TreeViewItem item, OrgTreeItemTypes parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    if (item.Tag != null && item.Tag.ToString() == parentType.ToString())
        //    {
        //        switch (parentType)
        //        {
        //            case OrgTreeItemTypes.Company:
        //                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
        //                if (tmpOrg != null)
        //                {
        //                    if (tmpOrg.COMPANYID == parentID)
        //                        return item;
        //                }
        //                break;
        //            case OrgTreeItemTypes.Department:
        //                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmpDep = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
        //                if (tmpDep != null)
        //                {
        //                    if (tmpDep.DEPARTMENTID == parentID)
        //                        return item;
        //                }
        //                break;
        //        }

        //    }
        //    if (item.Items != null && item.Items.Count > 0)
        //    {
        //        foreach (TreeViewItem childitem in item.Items)
        //        {
        //            tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
        //            if (tmpItem != null)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return tmpItem;
        //}

        //private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    BindGrid();
        //}

        //#endregion

        #region IClient 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
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

        //休假记录
        public SMT.Saas.Tools.AttendanceWS.T_HR_EMPLOYEELEAVERECORD LeaveRecord { get; set; }

         //<summary>
         //选择当前员工的请假类型
         //</summary>
         //<param name="sender"></param>
         //<param name="e"></param>
        private void lkLeaveTypeName_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("LEAVETYPENAME", "LEAVETYPENAME");
            cols.Add("ISFREELEAVEDAY", "ISFREELEAVEDAY,ISCHECKED,DICTIONARYCONVERTER");
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            filter += " ISFREELEAVEDAY=@" + paras.Count().ToString() + "";
            paras.Add("2");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.LeaveTypeSet,
                typeof(SMT.Saas.Tools.AttendanceWS.T_HR_LEAVETYPESET[]), cols, filter, paras);

            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.AttendanceWS.T_HR_LEAVETYPESET ent = lookup.SelectedObj as SMT.Saas.Tools.AttendanceWS.T_HR_LEAVETYPESET;
                
                if (ent != null)
                {
                    if (ent.SEXRESTRICT != "2")
                    {
                        if (ent.SEXRESTRICT != tbEmpSex.Text)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEELEAVERECORD"), Utility.GetResourceStr("LEAVETYPERESTRICT", "SEXRESTRICT"));
                            return;
                        }
                    }

                    if (!string.IsNullOrEmpty(ent.POSTLEVELRESTRICT))
                    {
                        decimal dlevel = 0, dCheckLevel = 0;

                        //decimal.TryParse(tbEmpLevel.Text, out dlevel);
                        decimal.TryParse(ent.POSTLEVELRESTRICT, out dCheckLevel);

                        if (dlevel > dCheckLevel)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEELEAVERECORD"), Utility.GetResourceStr("LEAVETYPERESTRICT", "POSTLEVELRESTRICT"));
                            return;
                        }
                    }

                    //LeaveRecord = new Saas.Tools.AttendanceWS.T_HR_EMPLOYEELEAVERECORD();
                    //LeaveRecord.T_HR_LEAVETYPESET = ent;

                    this.lkLeaveTypeName.DataContext = ent;

                    strleaveType = ent.LEAVETYPESETID;

                    if (ent.FINETYPE == (Convert.ToInt32(LeaveFineType.Free) + 1).ToString() || ent.FINETYPE == (Convert.ToInt32(LeaveFineType.Deduct) + 1).ToString())
                    {
                        //toolbar1.IsEnabled = false;
                        //toolbar1.Visibility = System.Windows.Visibility.Collapsed;
                        //dgLevelDayList.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 
        /// </summary>
        private List<T_HR_EMPLOYEELEVELDAYCOUNT> GetRowDataGrid()
        {
            List<T_HR_EMPLOYEELEVELDAYCOUNT> liststr = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
            var rowItems = dgEmpLeaveDays.SelectedItems.Cast<T_HR_EMPLOYEELEVELDAYCOUNT>().ToArray();
            if (rowItems.Count()>0)
            {
                foreach (T_HR_EMPLOYEELEVELDAYCOUNT item in rowItems)
                {
                    T_HR_EMPLOYEELEVELDAYCOUNT EMPLOYEELEVELDAYCOUNT = new T_HR_EMPLOYEELEVELDAYCOUNT();
                    EMPLOYEELEVELDAYCOUNT.LEAVETYPESETID = item.LEAVETYPESETID;
                    EMPLOYEELEVELDAYCOUNT.EFFICDATE = item.EFFICDATE;
                    EMPLOYEELEVELDAYCOUNT.EMPLOYEEID = item.EMPLOYEEID;

                    liststr.Add(EMPLOYEELEVELDAYCOUNT);
                }
                return liststr;
            }
            else
            {
                return null;
            }
        }
    }
}
