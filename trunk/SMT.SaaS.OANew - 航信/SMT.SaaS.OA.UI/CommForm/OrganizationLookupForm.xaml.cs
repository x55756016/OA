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
using System.Reflection;
using System.Collections;
using SMT.SaaS.OA.UI.CommForm;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.OA.UI;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.CommForm
{
    public partial class OrganizationLookupForm : ChildWindow
    {


        private List<T_HR_COMPANY> allCompanys;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_POST> allPositions;

        public object SelectedObj
        {
            get;
            set;
        }
        public OrgTreeItemTypes SelectedObjType { get; set; }

        public event EventHandler SelectedClick;
        public OrganizationLookupForm()
        {
            InitializeComponent();
            InitParas();
        }

        OrganizationServiceClient client = new OrganizationServiceClient();
        private void InitParas()
        {
            //初始化控件的状态            

            client.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(client_GetCompanyActivedCompleted);
            client.GetDepartmentActivedCompleted += new EventHandler<GetDepartmentActivedCompletedEventArgs>(client_GetDepartmentActivedCompleted);
            client.GetPostActivedCompleted += new EventHandler<GetPostActivedCompletedEventArgs>(client_GetPostActivedCompleted);

            BindTree();
        }

        void client_GetCompanyActivedCompleted(object sender, GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                allCompanys = e.Result.ToList();
            }
            BindCompany();
            if (SelectedObjType != OrgTreeItemTypes.Company)
            {
                client.GetDepartmentActivedAsync(Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void client_GetDepartmentActivedCompleted(object sender, GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                allDepartments = e.Result.ToList();
            }
            BindDepartment();
            if (SelectedObjType != OrgTreeItemTypes.Department)
            {
                client.GetPostActivedAsync(Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void client_GetPostActivedCompleted(object sender, GetPostActivedCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                allPositions = e.Result.ToList();
            }
            BindPosition();
        }

        private void BindTree()
        {
            client.GetCompanyActivedAsync(Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            if (allCompanys != null)
            {
                foreach (T_HR_COMPANY tmpOrg in allCompanys)
                {
                    if (tmpOrg.T_HR_COMPANY2 == null || string.IsNullOrEmpty(tmpOrg.T_HR_COMPANY2.COMPANYID))
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpOrg.CNAME;
                        item.Style = Application.Current.Resources["OrganizationItemStyle"] as Style;
                        item.DataContext = tmpOrg;

                        //标记为公司
                        item.Tag = OrgTreeItemTypes.Company;
                        treeOrganization.Items.Add(item);

                        AddChildOrgItems(item, tmpOrg.COMPANYID);
                    }
                }
            }

        }

        private void AddChildOrgItems(TreeViewItem item, string companyID)
        {
            List<T_HR_COMPANY> childs = GetChildORG(companyID);
            if (childs == null || childs.Count <= 0)
                return;

            foreach (T_HR_COMPANY childOrg in childs)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Header = childOrg.CNAME;
                childItem.DataContext = childOrg;
                childItem.Style = Application.Current.Resources["OrganizationItemStyle"] as Style;

                //标记为公司
                childItem.Tag = OrgTreeItemTypes.Company;
                item.Items.Add(childItem);

                AddChildOrgItems(childItem, childOrg.COMPANYID);
            }
        }

        private List<T_HR_COMPANY> GetChildORG(string companyID)
        {
            List<T_HR_COMPANY> orgs = new List<T_HR_COMPANY>();

            foreach (T_HR_COMPANY org in allCompanys)
            {
                if (org.T_HR_COMPANY2 != null && org.T_HR_COMPANY2.COMPANYID == companyID)
                    orgs.Add(org);
            }
            return orgs;
        }

        private void BindDepartment()
        {
            if (allDepartments != null)
            {
                foreach (T_HR_DEPARTMENT tmpDep in allDepartments)
                {
                    if (tmpDep.T_HR_COMPANY == null)
                        continue;

                    TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, tmpDep.T_HR_COMPANY.COMPANYID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();

                        item.Header = tmpDep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        item.DataContext = tmpDep;
                        item.Style =  Application.Current.Resources["DepartmentItemStyle"] as Style;

                        //标记为部门
                        item.Tag = OrgTreeItemTypes.Department;
                        parentItem.Items.Add(item);
                    }
                }
            }
        }

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
                        item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
                        item.DataContext = tmpPosition;
                        item.Style = Application.Current.Resources["PositionItemStyle"] as Style;
                                                
                        //标记为岗位
                        item.Tag = OrgTreeItemTypes.Post;
                        parentItem.Items.Add(item);
                    }
                }
            }
            //树全部展开
            treeOrganization.ExpandAll();
            if (treeOrganization.Items.Count > 0)
            {
                TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
                selectedItem.IsSelected = true;
            }
        }
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

        private TreeViewItem GetParentItemFromChild(TreeViewItem item, OrgTreeItemTypes  parentType, string parentID)
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


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedObj == null)
            {
                MessageBox.Show(Utility.GetResourceStr("SELECTEIONISNULL"));
                return;
            }
            this.DialogResult = true;

            if (SelectedClick != null)
                SelectedClick(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void lookUpTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treeOrganization.SelectedItem.GetType() != typeof(TreeViewItem)) 
                return;
            TreeViewItem item = (TreeViewItem)treeOrganization.SelectedItem;

            if (item == null || item.DataContext == null) 
                return;

            OrgTreeItemTypes type = (OrgTreeItemTypes)(item.Tag);

            if (SelectedObjType== OrgTreeItemTypes.All || type == SelectedObjType)
            {
                SelectedObj = item.DataContext;
                if (SelectedObj is T_HR_POST)
                {
                    TreeViewItem pitem = item.Parent as TreeViewItem;
                    if (pitem != null)
                    {
                        ((T_HR_POST)SelectedObj).T_HR_DEPARTMENT = pitem.DataContext as T_HR_DEPARTMENT;
                    }
                }
            }
            else
            {
                SelectedObj = null;
            }

        }
    }
}

