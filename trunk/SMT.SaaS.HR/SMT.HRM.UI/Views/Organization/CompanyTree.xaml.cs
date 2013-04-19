using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.OrganizationWS;
using SMT.HRM.UI.Form;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Organization
{
    public partial class CompanyTree : BasePage, IClient
    {
        SMTLoading loadbar = new SMTLoading();
        public CompanyTree()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                InitParas();
            };
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        private List<T_HR_COMPANY> allCompanys;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_POST> allPositions;

        private string isTag;

        public string IsTag
        {
            get { return isTag; }
            set { isTag = value; }
        }

        OrganizationServiceClient client = new OrganizationServiceClient();
        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Start();
            client.GetCompanyAllCompleted += new EventHandler<GetCompanyAllCompletedEventArgs>(client_GetCompanyAllCompleted);
            client.GetDepartmentAllCompleted += new EventHandler<GetDepartmentAllCompletedEventArgs>(client_GetDepartmentAllCompleted);
            client.GetPostAllCompleted += new EventHandler<GetPostAllCompletedEventArgs>(client_GetPostAllCompleted);

            BindTree();
        }

        private void BindTree()
        {
            // client.GetCompanyAllAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            if (Application.Current.Resources.Contains("ORGTREESYSCompanyInfoALL"))
            {
                // allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfoALL"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
                BindCompany();
            }
            else
            {
                client.GetCompanyAllAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            //client.GetCompanyActivedAsync("BF06E969-1B2C-4a89-B0AE-A91CA1244053");
        }
        #region
        //private void BindCompany()
        //{
        //    treeOrganization.Items.Clear();            
        //    if (allCompanys != null)
        //    {
        //        foreach (T_HR_COMPANY tmpOrg in allCompanys)
        //        {
        //            if (tmpOrg.T_HR_COMPANY2 == null || string.IsNullOrEmpty(tmpOrg.T_HR_COMPANY2.COMPANYID))
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
        //                item.Tag = "0";
        //                treeOrganization.Items.Add(item);

        //                AddChildOrgItems(item, tmpOrg.COMPANYID);
        //            }
        //        }

        //        if (Application.Current.Resources.Contains("ORGTREESYSDepartmentInfoALL"))
        //        {
        //            allDepartments = Application.Current.Resources["ORGTREESYSDepartmentInfoALL"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
        //            BindDepartment();
        //        }
        //        else
        //        {
        //            client.GetDepartmentAllAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //        }
        //    }

        //}

        //private void AddChildOrgItems(TreeViewItem item, string companyID)
        //{
        //    List<T_HR_COMPANY> childs = GetChildORG(companyID);
        //    if (childs == null || childs.Count <= 0)
        //        return;

        //    foreach (T_HR_COMPANY childOrg in childs)
        //    {
        //        TreeViewItem childItem = new TreeViewItem();
        //        childItem.Header = childOrg.CNAME;
        //        childItem.DataContext = childOrg;

        //        childItem.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //        childItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;


        //        //状态在未生效和撤消中时背景色为红色
        //        SolidColorBrush brush = new SolidColorBrush();
        //        if (childOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
        //        {
        //            brush.Color = Colors.Red;
        //            childItem.Foreground = brush;
        //        }
        //        else
        //        {
        //            brush.Color = Colors.Black;
        //            item.Foreground = brush;
        //        }

        //        //标记为公司
        //        childItem.Tag = "0";
        //        item.Items.Add(childItem);

        //        AddChildOrgItems(childItem, childOrg.COMPANYID);
        //    }
        //}
        #endregion

        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfoALL"] as List<T_HR_COMPANY>;

            allDepartments = Application.Current.Resources["ORGTREESYSDepartmentInfoALL"] as List<T_HR_DEPARTMENT>;

            if (allCompanys == null)
            {
                return;
            }
            else
            {
                allCompanys = allCompanys.OrderBy(s => s.SORTINDEX).ToList();
            }
            if (allDepartments != null)
            {
                allDepartments = allDepartments.OrderBy(s => s.SORTINDEX).ToList();
            }

            List<T_HR_COMPANY> TopCompany = new List<T_HR_COMPANY>();

            foreach (T_HR_COMPANY tmpOrg in allCompanys)
            {
                //如果当前公司没有父机构的ID，则为顶级公司
                if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = tmpOrg.CNAME;
                    item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                    item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    item.DataContext = tmpOrg;

                    //状态在未生效和撤消中时背景色为红色
                    SolidColorBrush brush = new SolidColorBrush();
                    if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                    {
                        brush.Color = Colors.Red;
                        item.Foreground = brush;
                    }
                    else
                    {
                        brush.Color = Colors.Black;
                        item.Foreground = brush;
                    }
                    //标记为公司
                    item.Tag = OrgTreeItemTypes.Company;
                    treeOrganization.Items.Add(item);
                    TopCompany.Add(tmpOrg);
                }
                else
                {
                    //查询当前公司是否在公司集合内有父公司存在
                    var ent = from c in allCompanys
                              where tmpOrg.FATHERTYPE == "0" && c.COMPANYID == tmpOrg.FATHERID
                              select c;
                    var ent2 = from c in allDepartments
                               where tmpOrg.FATHERTYPE == "1" && tmpOrg.FATHERID == c.DEPARTMENTID
                               select c;

                    //如果不存在，则为顶级公司
                    if (ent.Count() == 0 && ent2.Count() == 0)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpOrg.CNAME;
                        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        item.DataContext = tmpOrg;

                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为公司
                        item.Tag = OrgTreeItemTypes.Company;
                        treeOrganization.Items.Add(item);

                        TopCompany.Add(tmpOrg);
                    }
                }
            }
            //开始递归
            foreach (var topComp in TopCompany)
            {
                TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, topComp.COMPANYID);
                List<T_HR_COMPANY> lsCompany = (from ent in allCompanys
                                                where ent.FATHERTYPE == "0"
                                                && ent.FATHERID == topComp.COMPANYID
                                                select ent).ToList();

                List<T_HR_DEPARTMENT> lsDepartment = (from ent in allDepartments
                                                      where ent.FATHERID == topComp.COMPANYID && ent.FATHERTYPE == "0"
                                                      select ent).ToList();

                AddOrgNode(lsCompany, lsDepartment, parentItem);
            }
            allPositions = Application.Current.Resources["ORGTREESYSPostInfoALL"] as List<T_HR_POST>;
            if (allPositions != null)
            {
                BindPosition();
            }
        }

        private void AddOrgNode(List<T_HR_COMPANY> lsCompany, List<T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
        {
            //绑定公司的子公司
            foreach (var childCompany in lsCompany)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childCompany.CNAME;
                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                item.DataContext = childCompany;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childCompany.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为公司
                item.Tag = OrgTreeItemTypes.Company;
                FatherNode.Items.Add(item);

                if (lsCompany.Count() > 0)
                {
                    List<T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                    where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                    select ent).ToList();
                    List<T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                       where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                       select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
            //绑定公司下的部门
            foreach (var childDepartment in lsDepartment)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                item.DataContext = childDepartment;
                item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childDepartment.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为部门
                item.Tag = OrgTreeItemTypes.Department;
                FatherNode.Items.Add(item);

                if (lsDepartment.Count() > 0)
                {
                    List<T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                    where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                    select ent).ToList();
                    List<T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                       where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                       select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
        }

        /// <summary>
        /// 绑定岗位
        /// </summary>
        private void BindPosition()
        {
            if (allPositions != null)
            {
                foreach (T_HR_POST tmpPosition in allPositions)
                {
                    if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
                        continue;
                    TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Department, tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        if(tmpPosition.T_HR_POSTDICTIONARY!=null)
                            item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
                        item.DataContext = tmpPosition;
                        item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpPosition.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为岗位
                        item.Tag = OrgTreeItemTypes.Post;
                        parentItem.Items.Add(item);
                    }
                }
            }
            //树全部展开
            //  treeOrganization.ExpandAll();
            if (treeOrganization.Items.Count > 0)
            {
                TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
                selectedItem.IsSelected = true;
            }

        }

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        private TreeViewItem GetParentItem(OrgTreeItemTypes parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            foreach (TreeViewItem item in treeOrganization.Items)
            {
                tmpItem = GetParentItemFromChild(item, parentType, parentID);
                if (tmpItem != null)
                {
                    break;
                }
            }
            return tmpItem;
        }

        private TreeViewItem GetParentItemFromChild(TreeViewItem item, OrgTreeItemTypes parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            if (item.Tag != null && item.Tag.ToString() == parentType.ToString())
            {
                switch (parentType)
                {
                    case OrgTreeItemTypes.Company:
                        T_HR_COMPANY tmpOrg = item.DataContext as T_HR_COMPANY;
                        if (tmpOrg != null)
                        {
                            if (tmpOrg.COMPANYID == parentID)
                                return item;
                        }
                        break;
                    case OrgTreeItemTypes.Department:
                        T_HR_DEPARTMENT tmpDep = item.DataContext as T_HR_DEPARTMENT;
                        if (tmpDep != null)
                        {
                            if (tmpDep.DEPARTMENTID == parentID)
                                return item;
                        }
                        break;
                }

            }
            if (item.Items != null && item.Items.Count > 0)
            {
                foreach (TreeViewItem childitem in item.Items)
                {
                    tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
                    if (tmpItem != null)
                    {
                        break;
                    }
                }
            }
            return tmpItem;
        }

        void client_GetCompanyAllCompleted(object sender, GetCompanyAllCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<T_HR_COMPANY> entTemps = e.Result;
                allCompanys = new List<T_HR_COMPANY>();
                allCompanys.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allCompanys.Add(item);
                });
                UICache.CreateCache("ORGTREESYSCompanyInfoALL", allCompanys);
                //  BindCompany();
                client.GetDepartmentAllAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void client_GetDepartmentAllCompleted(object sender, GetDepartmentAllCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }
                ObservableCollection<T_HR_DEPARTMENT> entTemps = e.Result;
                allDepartments = new List<T_HR_DEPARTMENT>();
                allDepartments.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allDepartments.Add(item);
                });

                UICache.CreateCache("ORGTREESYSDepartmentInfoALL", allDepartments);
                //  BindDepartment();
                BindCompany();
                client.GetPostAllAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void client_GetPostAllCompleted(object sender, GetPostAllCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    allPositions = e.Result.ToList();
                }
                UICache.CreateCache("ORGTREESYSPostInfoALL", allPositions);
                BindPosition();
            }
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Container.Children.Clear();
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem == null)
            {
                loadbar.Stop();
                return;
            }
            IsTag = selectedItem.Tag.ToString();
            switch (IsTag)
            {
                case "Company":
                    T_HR_COMPANY company = selectedItem.DataContext as T_HR_COMPANY;
                    CompanyForm orgForm = new CompanyForm(FormTypes.Browse, company.COMPANYID);
                    Container.Children.Add(orgForm);
                    break;
                case "Department":
                    T_HR_DEPARTMENT department = selectedItem.DataContext as T_HR_DEPARTMENT;
                    DepartmentForm depForm = new DepartmentForm(FormTypes.Browse, department.DEPARTMENTID);
                    Container.Children.Add(depForm);
                    break;
                case "Post":
                    T_HR_POST post = selectedItem.DataContext as T_HR_POST;
                    PostForm posForm = new PostForm(FormTypes.Browse, post.POSTID);
                    Container.Children.Add(posForm);
                    break;
            }
            loadbar.Stop();
        }

        #region 新增，变更，撤消
        //private void btnOrgAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    Container.Children.Clear();
        //    CompanyForm orgForm = new CompanyForm(FormTypes.New, Company.COMPANYID);
        //    orgForm.OnDataChanged += new EventHandler(OnSubFormDataChanged);
        //    Container.Children.Add(orgForm);
        //}

        private void btnDepAdd_Click(object sender, RoutedEventArgs e)
        {
            //if (Company != null)
            //{
            //    Container.Children.Clear();
            //    DepartmentForm depForm = new DepartmentForm(FormTypes.New, Company);
            //    depForm.OnDataChanged += new EventHandler(OnSubFormDataChanged);
            //    Container.Children.Add(depForm);
            //}
        }

        private void btnPosAdd_Click(object sender, RoutedEventArgs e)
        {
            //Container.Children.Clear();
            //if (Company != null && Orgdepart != null)
            //{
            //    PostForm posForm = new PostForm(FormTypes.New, Company, Orgdepart);
            //    posForm.OnDataChanged += new EventHandler(OnSubFormDataChanged);
            //    Container.Children.Add(posForm);
            //}
        }


        #endregion
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
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
        //private void btnAudit_Click(object sender, RoutedEventArgs e)
        //{
        //    switch (IsTag)
        //    {
        //        case "0"://公司
        //            CompanyForm companyForm = Container.Children[0] as CompanyForm;
        //            if(companyForm!=null)
        //            {
        //                companyForm.Audit();
        //            }
        //            break;
        //        case "1"://部门
        //            DepartmentForm depForm = Container.Children[0] as DepartmentForm;
        //            if (depForm != null)
        //            {
        //                depForm.Audit();
        //            }
        //            break;
        //        case "2"://岗位
        //            PostForm posForm = Container.Children[0] as PostForm;
        //            if (posForm != null)
        //            {
        //                posForm.Audit();
        //            }
        //            break;
        //    }
        //}

        //private void btnRemove_Click(object sender, RoutedEventArgs e)
        //{
        //    switch (IsTag)
        //    {
        //        case "0"://公司
        //            CompanyForm companyForm = Container.Children[0] as CompanyForm;
        //            if (companyForm != null)
        //            {
        //                companyForm.Cancel();
        //            }
        //            break;
        //        case "1"://部门
        //            DepartmentForm depForm = Container.Children[0] as DepartmentForm;
        //            if (depForm != null)
        //            {
        //                depForm.Cancel();
        //            }
        //            break;
        //        case "2"://岗位
        //            PostForm posForm = Container.Children[0] as PostForm;
        //            if (posForm != null)
        //            {
        //                posForm.Cancel();
        //            }
        //            break;
        //    }
        //}

        //private void btnDelete_Click(object sender, RoutedEventArgs e)
        //{
        //    switch (IsTag)
        //    {
        //        case "0"://公司
        //            CompanyForm companyForm = Container.Children[0] as CompanyForm;
        //            if (companyForm != null)
        //            {
        //                companyForm.delete(Company.COMPANYID);
        //            }
        //            break;
        //        case "1"://部门
        //            DepartmentForm depForm = Container.Children[0] as DepartmentForm;
        //            if (depForm != null)
        //            {
        //                depForm.delete(Orgdepart.DEPARTMENTID);
        //            }
        //            break;
        //        case "2"://岗位
        //            PostForm posForm = Container.Children[0] as PostForm;
        //            if (posForm != null)
        //            {
        //                posForm.delete(Post.POSTID);
        //            }
        //            break;
        //    }
        //}

        //private void btnSubmitAudit_Click(object sender, RoutedEventArgs e)
        //{
        //    switch (IsTag)
        //    {
        //        case "0"://公司
        //            CompanyForm companyForm = Container.Children[0] as CompanyForm;
        //            if (companyForm != null)
        //            {
        //                companyForm.SubmitAudit();
        //            }
        //            break;
        //        case "1"://部门
        //            DepartmentForm depForm = Container.Children[0] as DepartmentForm;
        //            if (depForm != null)
        //            {
        //                depForm.SubmitAudit();
        //            }
        //            break;
        //        case "2"://岗位
        //            PostForm posForm = Container.Children[0] as PostForm;
        //            if (posForm != null)
        //            {
        //                posForm.SubmitAudit();
        //            }
        //            break;
        //    }
        //}


        //private void btnMerge_Click(object sender, RoutedEventArgs e)
        //{
        //    List<T_HR_COMPANY> selCompany = GetSelectedCompany();

        //}
        //private List<T_HR_COMPANY> GetSelectedCompany()
        //{
        //    List<T_HR_COMPANY> selCompany = new List<T_HR_COMPANY>();
        //    foreach (TreeViewItem item in treeOrganization.Items)
        //    {
        //        if (Convert.ToInt32(item.Tag) != 0)
        //            continue;

        //        TreeViewItem myItem =
        //            (TreeViewItem)(treeOrganization.ItemContainerGenerator.ContainerFromItem(item));

        //        CheckBox cbx = Utility.FindChildControl<CheckBox>(myItem);
        //        if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
        //        {
        //            selCompany.Add(item.DataContext as T_HR_COMPANY);
        //        }

        //        GetChildSelectedCompany(item, selCompany);
        //    }
        //    return selCompany;
        //}
        //private void GetChildSelectedCompany(TreeViewItem item, List<T_HR_COMPANY> selCompany)
        //{
        //    if (Convert.ToInt32(item.Tag) != 0)
        //        return;

        //    foreach (TreeViewItem childItem in item.Items)
        //    {
        //        if (Convert.ToInt32(childItem.Tag) != 0)
        //            continue;

        //        TreeViewItem myItem =
        //        (TreeViewItem)(item.ItemContainerGenerator.ContainerFromItem(childItem));

        //        CheckBox cbx = Utility.FindChildControl<CheckBox>(myItem);
        //        if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
        //        {
        //            selCompany.Add(childItem.DataContext as T_HR_COMPANY);
        //        }

        //        GetChildSelectedCompany(childItem, selCompany);
        //    }
        //}

        //#region 操作控件状态
        //private void InitBtnState()
        //{
        //    btnOrgAdd.IsEnabled = true;
        //    btnDepAdd.IsEnabled = false;
        //    btnPosAdd.IsEnabled = false;
        //    btnAudit.IsEnabled = false;
        //    btnDelete.IsEnabled = false;
        //    btnRemove.IsEnabled = false;
        //    btnMerge.IsEnabled = false;
        //    btnSubmitAudit.IsEnabled = false;
        //}
        //private void BtnEnableFalse()
        //{
        //    btnOrgAdd.IsEnabled = false;
        //    btnDepAdd.IsEnabled = false;
        //    btnPosAdd.IsEnabled = false;
        //    btnAudit.IsEnabled = false;
        //    btnDelete.IsEnabled = false;
        //    btnRemove.IsEnabled = false;
        //    btnMerge.IsEnabled = false;
        //    btnSubmitAudit.IsEnabled = false;
        //}
        //private void BtnVisibleAll()
        //{
        //    btnOrgAdd.Visibility = Visibility.Visible;
        //    btnDepAdd.Visibility = Visibility.Visible;
        //    btnPosAdd.Visibility = Visibility.Visible;
        //    btnAudit.Visibility = Visibility.Visible;
        //    btnDelete.Visibility = Visibility.Visible;
        //    btnRemove.Visibility = Visibility.Visible;
        //    btnMerge.Visibility = Visibility.Visible;
        //    btnSubmitAudit.Visibility = Visibility.Visible;
        //}
        //private void BtnEnableByState(string CheckState)
        //{
        //    BtnVisibleAll();
        //    GetPermission();
        //    if (IsTag == "0")
        //    {
        //        btnOrgAdd.IsEnabled = true;
        //    }
        //    if (CheckState == Convert.ToInt32(CheckStates.UnSubmit).ToString())
        //    {
        //        btnSubmitAudit.IsEnabled = true;
        //        btnDelete.IsEnabled = true;
        //    }
        //    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
        //    {
        //        if (IsTag == "0")
        //        {
        //            btnDepAdd.IsEnabled = true;
        //        }
        //        if (IsTag == "1")
        //        {
        //            btnPosAdd.IsEnabled = true;
        //        }
        //        btnRemove.IsEnabled = true;
        //    }
        //    if (CheckState == Convert.ToInt32(CheckStates.Approving).ToString())
        //    {
        //        btnAudit.IsEnabled = true;
        //    }
        //}

        //private void GetPermission()
        //{
        //    switch (IsTag)
        //    {
        //        case "0":
        //            //添加公司
        //            if (PermissionHelper.GetPermissionValue(Permissions.Company_Add) <= 0)
        //                btnOrgAdd.Visibility = Visibility.Collapsed;
        //            //删除公司
        //            if (PermissionHelper.GetPermissionValue(Permissions.Company_Delete) <= 0)
        //                btnDelete.Visibility = Visibility.Collapsed;
        //            //审核公司
        //            if (PermissionHelper.GetPermissionValue(Permissions.Company_Audit) <= 0)
        //                btnAudit.Visibility = Visibility.Collapsed;
        //            //合并公司
        //            if (PermissionHelper.GetPermissionValue(Permissions.Company_Audit) <= 0)
        //                btnMerge.Visibility = Visibility.Collapsed;

        //            //添加部门
        //            if (PermissionHelper.GetPermissionValue(Permissions.Department_Add) <= 0)
        //                btnOrgAdd.Visibility = Visibility.Collapsed;
        //            break;
        //        case "1":                    
        //            //删除部门
        //            if (PermissionHelper.GetPermissionValue(Permissions.Department_Delete) <= 0)
        //                btnDelete.Visibility = Visibility.Collapsed;
        //            //审核部门
        //            if (PermissionHelper.GetPermissionValue(Permissions.Department_Audit) <= 0)
        //                btnAudit.Visibility = Visibility.Collapsed;

        //            //添加岗位
        //            if (PermissionHelper.GetPermissionValue(Permissions.Post_Add) <= 0)
        //                btnOrgAdd.Visibility = Visibility.Collapsed;
        //            break;
        //        case "2":

        //            //删除岗位
        //            if (PermissionHelper.GetPermissionValue(Permissions.Post_Delete) <= 0)
        //                btnDelete.Visibility = Visibility.Collapsed;
        //            //审核岗位
        //            if (PermissionHelper.GetPermissionValue(Permissions.Post_Audit) <= 0)
        //                btnAudit.Visibility = Visibility.Collapsed;
        //            break;
        //    }
        //}
        //#endregion
    }
}
