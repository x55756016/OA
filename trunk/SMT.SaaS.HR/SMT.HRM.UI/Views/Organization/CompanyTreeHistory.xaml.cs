using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.Saas.Tools.OrganizationWS;
using SMT.HRM.UI.Form;
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.HRM.UI.Views.Organization
{
    public partial class CompanyTreeHistory : BasePage, IClient
    {
        SMTLoading loadbar = new SMTLoading();
        public CompanyTreeHistory()
        {
            InitializeComponent();
            InitParas();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private List<T_HR_COMPANYHISTORY> allCompanys;
        private List<T_HR_DEPARTMENTHISTORY> allDepartments;
        private List<T_HR_POSTHISTORY> allPositions;
        DateTime currentDate;

        private T_HR_COMPANYHISTORY companyHis;

        public T_HR_COMPANYHISTORY CompanyHis
        {
            get { return companyHis; }
            set { companyHis = value; }
        }

        OrganizationServiceClient client;
        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Start();
            client = new OrganizationServiceClient();
            client.GetCompanyHistoryCompleted += new EventHandler<GetCompanyHistoryCompletedEventArgs>(client_GetCompanyHistoryCompleted);
            client.GetDepartmentHistoryCompleted += new EventHandler<GetDepartmentHistoryCompletedEventArgs>(client_GetDepartmentHistoryCompleted);
            client.GetPostHistoryCompleted += new EventHandler<GetPostHistoryCompletedEventArgs>(client_GetPostHistoryCompleted);
            client.GetCompanyHistoryDateCompleted += new EventHandler<GetCompanyHistoryDateCompletedEventArgs>(client_GetCompanyHistoryDateCompleted);

            currentDate = DateTime.Now;
            ///TODO 当天新建，当天撤消的没有处理

            client.GetCompanyHistoryDateAsync();
        }

        void client_GetCompanyHistoryDateCompleted(object sender, GetCompanyHistoryDateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }
            else
            {
                if (e.Result != null)
                {
                    DtGrid.ItemsSource = e.Result;
                    PagedCollectionView pcv = new PagedCollectionView(e.Result);
                    pcv.PageSize = 3;
                    dataPager.DataContext = pcv;
                    client.GetCompanyHistoryAsync(currentDate);
                }
                else
                {
                    loadbar.Stop();
                }
            }
        }

        void btnFind_Click(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = Utility.FindChildControl<DatePicker>(expander, "datePicker");
            if (datePicker != null && !string.IsNullOrEmpty(datePicker.Text))
            {
                (sender as Button).IsEnabled = false;
                client.GetCompanyHistoryAsync(datePicker.SelectedDate.Value);
            }
            //else
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
            //}
        }

        void client_GetCompanyHistoryCompleted(object sender, GetCompanyHistoryCompletedEventArgs e)
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
                    allCompanys = e.Result.ToList();
                }
            }
            BindCompany();
            client.GetDepartmentHistoryAsync(currentDate);
        }

        void client_GetDepartmentHistoryCompleted(object sender, GetDepartmentHistoryCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                allDepartments = e.Result.ToList();
            }
            BindDepartment();
            client.GetPostHistoryAsync(currentDate);
        }

        void client_GetPostHistoryCompleted(object sender, GetPostHistoryCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                allPositions = e.Result.ToList();
            }
            BindPosition();
        }

        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            if (allCompanys != null)
            {
                foreach (T_HR_COMPANYHISTORY tmpOrg in allCompanys)
                {
                    if (tmpOrg.T_HR_COMPANY == null || string.IsNullOrEmpty(tmpOrg.T_HR_COMPANY.COMPANYID))
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpOrg.CNAME;
                        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        item.DataContext = tmpOrg;
                        //标记为公司
                        item.Tag = "0";
                        treeOrganization.Items.Add(item);

                        AddChildOrgItems(item, tmpOrg.COMPANYID);
                    }
                }
            }
        }

        private void AddChildOrgItems(TreeViewItem item, string companyID)
        {
            List<T_HR_COMPANYHISTORY> childs = GetChildORG(companyID);
            if (childs == null || childs.Count <= 0)
                return;

            foreach (T_HR_COMPANYHISTORY childOrg in childs)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Header = childOrg.CNAME;
                childItem.DataContext = childOrg;
                childItem.Style = Application.Current.Resources["OrganizationItemStyle"] as Style;

                //标记为公司
                childItem.Tag = "0";
                item.Items.Add(childItem);

                AddChildOrgItems(childItem, childOrg.COMPANYID);
            }
        }
        private List<T_HR_COMPANYHISTORY> GetChildORG(string companyID)
        {
            List<T_HR_COMPANYHISTORY> orgs = new List<T_HR_COMPANYHISTORY>();

            foreach (T_HR_COMPANYHISTORY org in allCompanys)
            {
                if (org.T_HR_COMPANY != null && org.T_HR_COMPANY.COMPANYID == companyID)
                    orgs.Add(org);
            }
            return orgs;
        }

        private void BindDepartment()
        {
            if (allDepartments != null)
            {
                foreach (T_HR_DEPARTMENTHISTORY tmpDep in allDepartments)
                {
                    //if (tmpDep.T_HR_COMPANY == null)
                    //    continue;

                    //TreeViewItem parentItem = GetParentItem("0", tmpDep.T_HR_COMPANY.COMPANYID);
                    if (tmpDep.COMPANYID == null)
                        continue;

                    string parentID = string.Empty;
                    //foreach (T_HR_COMPANYHISTORY tmpCom in allCompanys)
                    //{
                    //    if (tmpCom.COMPANYID == tmpDep.COMPANYID)
                    //    {
                    //        parentID = 
                    //    }
                    //}
                    TreeViewItem parentItem = GetParentItem1("0", tmpDep.COMPANYID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpDep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        item.DataContext = tmpDep;
                        item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        //标记为部门
                        item.Tag = "1";
                        parentItem.Items.Add(item);
                    }

                }
            }
        }

        private TreeViewItem GetParentItem1(string parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            foreach (TreeViewItem item in treeOrganization.Items)
            {
                tmpItem = GetParentItemFromChild1(item, parentType, parentID);
                if (tmpItem != null)
                {
                    break;
                }
            }
            return tmpItem;
        }

        private TreeViewItem GetParentItemFromChild1(TreeViewItem item, string parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            if (item.Tag != null && item.Tag.ToString() == parentType)
            {
                switch (parentType)
                {
                    case "0":
                        T_HR_COMPANYHISTORY tmpOrg = item.DataContext as T_HR_COMPANYHISTORY;
                        if (tmpOrg != null)
                        {
                            if (tmpOrg.COMPANYID == parentID)
                                return item;
                        }
                        break;
                    case "1":
                        T_HR_DEPARTMENTHISTORY tmpDep = item.DataContext as T_HR_DEPARTMENTHISTORY;
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
                    tmpItem = GetParentItemFromChild1(childitem, parentType, parentID);
                    if (tmpItem != null)
                    {
                        break;
                    }
                }
            }
            return tmpItem;
        }

        //private TreeViewItem GetParentItem(string parentType, string parentID)
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

        private void BindPosition()
        {
            if (allPositions != null)
            {
                foreach (T_HR_POSTHISTORY tmpPosition in allPositions)
                {
                    //if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
                    //    continue;
                    if (tmpPosition.DEPARTMENTID == null)
                        continue;
                    TreeViewItem parentItem = GetParentItem1("1", tmpPosition.DEPARTMENTID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        if (tmpPosition.T_HR_POSTDICTIONARY != null)
                        {
                            item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
                        }

                        item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

                        item.DataContext = tmpPosition;
                        //标记为岗位
                        item.Tag = "2";
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

            Button btnFind = Utility.FindChildControl<Button>(expander, "btnFind");
            if (btnFind != null)
            {
                btnFind.IsEnabled = true;
            }
            this.DtGrid.IsEnabled = true;
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treeOrganization.Items.Count <= 0)
            {
                return;
            }
            Container.Children.Clear();
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            T_HR_COMPANYHISTORY Company;
            T_HR_DEPARTMENTHISTORY Orgdepart;
            if (selectedItem == null)
            {
                loadbar.Stop();
                return;
            }
            switch (selectedItem.Tag.ToString())
            {
                case "0":
                    Company = selectedItem.DataContext as T_HR_COMPANYHISTORY;
                    CompanyHistoryForm orgForm = new CompanyHistoryForm(Company);
                    Container.Children.Add(orgForm);
                    break;
                case "1":
                    Orgdepart = selectedItem.DataContext as T_HR_DEPARTMENTHISTORY;
                    Company = selectedItem.GetParentTreeViewItem().DataContext as T_HR_COMPANYHISTORY;
                    if (Company != null)
                    {
                        DepartmentHistoryForm depForm = new DepartmentHistoryForm(Orgdepart, Company.CNAME);
                        Container.Children.Add(depForm);
                    }
                    break;
                case "2":
                    T_HR_POSTHISTORY Post = selectedItem.DataContext as T_HR_POSTHISTORY;
                    Company = selectedItem.GetParentTreeViewItem().GetParentTreeViewItem().DataContext as T_HR_COMPANYHISTORY;
                    Orgdepart = selectedItem.GetParentTreeViewItem().DataContext as T_HR_DEPARTMENTHISTORY;
                    if (Company != null && Orgdepart != null)
                    {

                        PostHistoryForm posForm = new PostHistoryForm(Post, Company.CNAME, Orgdepart.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);
                        Container.Children.Add(posForm);
                    }
                    break;
            }
            loadbar.Stop();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Button btnFind = Utility.FindChildControl<Button>(expander, "btnFind");
            if (btnFind != null)
            {
                btnFind.IsEnabled = false;
            }
            DtGrid.IsEnabled = false;
            T_HR_COMPANYHISTORY ent = ((System.Windows.Controls.DataGrid)(sender)).SelectedItem as T_HR_COMPANYHISTORY;
            client.GetCompanyHistoryAsync(ent.REUSEDATE.GetValueOrDefault());
        }
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
    }
}
