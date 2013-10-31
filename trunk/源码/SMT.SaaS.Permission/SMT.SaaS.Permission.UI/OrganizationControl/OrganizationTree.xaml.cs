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


using SMT.SaaS.FrameworkUI.Helper;
using SMT.Saas.Tools.OrganizationWS;
using PersonnelWS = SMT.Saas.Tools.PersonnelWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
using System.Collections.ObjectModel;

namespace SMT.SaaS.Permission.UI.OrganizationControl
{
    public partial class OrganizationTree : UserControl
    {
        DataTemplate treeViewItemTemplate = null;
        Style treeViewItemStyle = null;
        OrganizationServiceClient client = null;
        private List<T_HR_COMPANY> allCompanys;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_POST> allPositions;

        #region 属性
        public string ModelCode { get; set; }

        public List<ExtOrgObj> SelectedObj
        {
            get
            {
                var _SelectedObj = new List<ExtOrgObj>();

                foreach (TreeViewItem item in treeOrganization.Items)
                {

                    TreeViewItem myItem =
                        (TreeViewItem)(treeOrganization.ItemContainerGenerator.ContainerFromItem(item));
                    myItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

                    CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                    if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
                    {
                        OrgTreeItemTypes type = (OrgTreeItemTypes)(item.Tag);

                        if (SelectedObjType == OrgTreeItemTypes.All || type == SelectedObjType)
                        {
                            _SelectedObj.Add(item.DataContext as ExtOrgObj);
                        }
                    }

                    GetChildSelectedCompany(item, _SelectedObj);
                }
                return _SelectedObj;
            }
        }
        public OrgTreeItemTypes SelectedObjType { get; set; }
        public bool MultiSelected = false;                   //是否支持多选
        public bool SelectSameGradeOnly = false;             //是否只能选择同级对像

        // Added by Zhangbf, 有些项目没有Application.Current.Resources["CurrentUserID"].
        private string currentUserID = null;
        public string CurrentUserID
        {
            get
            {
                if (currentUserID == null)
                {
                    currentUserID = Application.Current.Resources["CurrentUserID"].ToString();
                }
                return currentUserID;
            }
            set
            {
                currentUserID = value;
            }

        }

        public string Perm { get; set; }
        public string Entity { get; set; }
        #endregion

        #region 构造
        public OrganizationTree()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OrganizationTree_Loaded);
        }

        public OrganizationTree(string userID, string perm, string entity)
            : this()
        {
            CurrentUserID = userID;
            Perm = perm;
            Entity = entity;
        }

        public OrganizationTree(string perm, string entity)
            : this()
        {
            Perm = perm;
            Entity = entity;
        }

        #endregion

        public string ValidSelection()
        {
            var selectedItems = this.SelectedObj;
            if (selectedItems == null || selectedItems.Count == 0)
            {
                string rslt = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString("SELECTORGISNULL", SMT.SaaS.Globalization.Localization.UiCulture);
                string type = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString("DATA", SMT.SaaS.Globalization.Localization.UiCulture);
                if (SelectedObjType == OrgTreeItemTypes.Company)
                {
                    type = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString("COMPANY", SMT.SaaS.Globalization.Localization.UiCulture);
                }
                if (SelectedObjType == OrgTreeItemTypes.Department)
                {
                    type = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString("DEPARTMENT", SMT.SaaS.Globalization.Localization.UiCulture);
                }
                if (SelectedObjType == OrgTreeItemTypes.Personnel)
                {
                    type = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString("PERSONEL", SMT.SaaS.Globalization.Localization.UiCulture);
                }
                if (SelectedObjType == OrgTreeItemTypes.Post)
                {
                    type = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString("POST", SMT.SaaS.Globalization.Localization.UiCulture);
                }
                rslt = string.Format(rslt, type);
                return rslt;
            }
            return string.Empty;
        }

        private void OrganizationTree_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
        }

        private void InitParas()
        {
            client = new OrganizationServiceClient();
            client.GetCompanyByEntityPermCompleted += new EventHandler<GetCompanyByEntityPermCompletedEventArgs>(client_GetCompanyByEntityPermCompleted);
            client.GetDepartmentByEntityPermCompleted += new EventHandler<GetDepartmentByEntityPermCompletedEventArgs>(client_GetDepartmentByEntityPermCompleted);
            client.GetPostByEntityPermCompleted += new EventHandler<GetPostByEntityPermCompletedEventArgs>(client_GetPostByEntityPermCompleted);
            treeOrganization.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(lookUpTree_SelectedItemChanged);

            treeViewItemStyle = Application.Current.Resources["TreeViewItemStyle"] as Style;
            treeViewItemTemplate = this.Resources["treeViewItemHead"] as DataTemplate;

            if (Application.Current.Resources.Contains("SYS_DepartmentInfo"))
            {
                InitCompany();
            }
            else
            {
                BindTree();
            }

        }

        public void BindTree()
        {
            //client.GetCompanyByEntityPermAsync("4afd1d50-b84d-4ea3-8c81-0078c35fea5e", "0", "T_FB_CHARGEAPPLYMASTER");
            client.GetCompanyByEntityPermAsync(CurrentUserID, Perm, Entity);

        }

        #region 绑定数据
        /// <summary>
        /// 绑定组织架构节点
        /// </summary>
        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            allCompanys = Application.Current.Resources["SYS_DepartmentInfo"] as List<T_HR_COMPANY>;

            allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<T_HR_DEPARTMENT>;

            if (allCompanys == null)
            {
                return;
            }

            List<T_HR_COMPANY> TopCompany = new List<T_HR_COMPANY>();

            foreach (T_HR_COMPANY tmpOrg in allCompanys)
            {
                //如果当前公司没有父机构的ID，则为顶级公司
                if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = tmpOrg.CNAME;
                    item.HeaderTemplate = treeViewItemTemplate;
                    item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

                    ExtOrgObj obj = new ExtOrgObj();
                    obj.ObjectInstance = tmpOrg;

                    item.DataContext = obj;

                    //标记为公司
                    item.Tag = OrgTreeItemTypes.Company;
                    treeOrganization.Items.Add(item);
                    TopCompany.Add(tmpOrg);
                }
                else
                {
                    //查询当前公司是否在公司集合内有父公司存在
                    var ent = from c in allCompanys
                              where c.COMPANYID == tmpOrg.FATHERID && tmpOrg.FATHERTYPE == "0"
                              select c;

                    var ent2 = from c in allDepartments
                               where tmpOrg.FATHERTYPE == "1" && tmpOrg.FATHERID == c.DEPARTMENTID
                               select c;

                    //如果不存在，则为顶级公司
                    if (ent.Count() == 0 && ent2.Count() == 0)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpOrg.CNAME;
                        item.HeaderTemplate = treeViewItemTemplate; ;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

                        ExtOrgObj obj = new ExtOrgObj();
                        obj.ObjectInstance = tmpOrg;

                        item.DataContext = obj;

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
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="lsCompany"></param>
        /// <param name="lsDepartment"></param>
        /// <param name="FatherNode"></param>
        private void AddOrgNode(List<T_HR_COMPANY> lsCompany, List<T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
        {
            //绑定公司的子公司
            foreach (var childCompany in lsCompany)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childCompany.CNAME;
                item.HeaderTemplate = treeViewItemTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

                ExtOrgObj obj = new ExtOrgObj();
                obj.ObjectInstance = childCompany;

                item.DataContext = obj;

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
                item.HeaderTemplate = treeViewItemTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

                ExtOrgObj obj = new ExtOrgObj();
                obj.ObjectInstance = childDepartment;

                item.DataContext = obj;

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

        private void AddChildOrgItems(TreeViewItem item, string companyID)
        {
            List<T_HR_COMPANY> childs = GetChildORG(companyID);
            if (childs == null || childs.Count <= 0)
                return;

            foreach (T_HR_COMPANY childOrg in childs)
            {
                TreeViewItem childItem = new TreeViewItem();

                childItem.Header = childOrg.CNAME;
                childItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                ExtOrgObj obj = new ExtOrgObj();
                obj.ObjectInstance = childOrg;

                childItem.DataContext = obj;
                //设置父级对像
                obj.ParentObject = item.DataContext;

                // childItem.Style = this.Resources["CheckBoxTreeItemStyle"] as Style;
                childItem.HeaderTemplate = treeViewItemTemplate;
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
                if (org.T_HR_COMPANY2 == null)
                {
                    continue;
                }
                if (org.T_HR_COMPANY2.COMPANYID == companyID)
                {
                    continue;
                }
                orgs.Add(org);
            }
            return orgs;
        }

        private void BindDepartment()
        {
            allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<T_HR_DEPARTMENT>;
            if (allDepartments == null)
            {
                return;
            }

            if (allDepartments.Count() == 0)
            {
                return;
            }

            foreach (T_HR_DEPARTMENT tmpDep in allDepartments)
            {
                if (tmpDep.T_HR_COMPANY == null)
                    continue;

                TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, tmpDep.T_HR_COMPANY.COMPANYID);
                if (parentItem != null)
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    item.Header = tmpDep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

                    ExtOrgObj obj = new ExtOrgObj();
                    obj.ObjectInstance = tmpDep;
                    item.DataContext = obj;

                    //设置父级对像
                    obj.ParentObject = parentItem.DataContext;
                    item.HeaderTemplate = treeViewItemTemplate;
                    //标记为部门
                    item.Tag = OrgTreeItemTypes.Department;
                    parentItem.Items.Add(item);
                }
            }
        }

        private void BindPosition()
        {
            allPositions = Application.Current.Resources["SYS_PostInfo"] as List<T_HR_POST>;
            if (allPositions == null)
            {
                return;
            }

            if (allPositions.Count() == 0)
            {
                return;
            }


            foreach (T_HR_POST tmpPosition in allPositions)
            {
                if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
                    continue;
                TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Department, tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
                if (parentItem != null)
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
                    item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    ExtOrgObj obj = new ExtOrgObj();
                    obj.ObjectInstance = tmpPosition;
                    item.DataContext = obj;

                    //设置父级对像
                    obj.ParentObject = parentItem.DataContext;

                    // item.Style = this.Resources["CheckBoxTreeItemStyle"] as Style;
                    item.HeaderTemplate = treeViewItemTemplate;
                    //标记为岗位
                    item.Tag = OrgTreeItemTypes.Post;
                    parentItem.Items.Add(item);
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

            ExtOrgObj obj = item.DataContext as ExtOrgObj;

            if (obj != null && item.Tag != null && item.Tag.ToString() == parentType.ToString())
            {
                switch (parentType)
                {
                    case OrgTreeItemTypes.Company:
                        T_HR_COMPANY tmpOrg = obj.ObjectInstance as T_HR_COMPANY;
                        if (tmpOrg != null)
                        {
                            if (tmpOrg.COMPANYID == parentID)
                                return item;
                        }
                        break;
                    case OrgTreeItemTypes.Department:
                        T_HR_DEPARTMENT tmpDep = obj.ObjectInstance as T_HR_DEPARTMENT;
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

        private void GetChildSelectedCompany(TreeViewItem item, List<ExtOrgObj> selObj)
        {

            foreach (TreeViewItem childItem in item.Items)
            {
                TreeViewItem myItem =
                (TreeViewItem)(item.ItemContainerGenerator.ContainerFromItem(childItem));

                CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);

                if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
                {
                    OrgTreeItemTypes type = (OrgTreeItemTypes)(childItem.Tag);

                    if (SelectedObjType == OrgTreeItemTypes.All || type == SelectedObjType)
                    {
                        if (type == OrgTreeItemTypes.Post)
                        {
                            ExtOrgObj obj = childItem.DataContext as ExtOrgObj;
                            ExtOrgObj pobj = item.DataContext as ExtOrgObj;
                            if (obj != null && pobj != null)
                            {
                                //附父节点的值
                                ((T_HR_POST)obj.ObjectInstance).T_HR_DEPARTMENT = pobj.ObjectInstance as T_HR_DEPARTMENT;
                            }
                        }

                        selObj.Add(childItem.DataContext as ExtOrgObj);
                    }
                }

                GetChildSelectedCompany(childItem, selObj);
            }
        }



        protected void lookUpTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            //return;
            if (treeOrganization.SelectedItem == null)
            {
                return;
            }
            if (treeOrganization.SelectedItem.GetType() != typeof(TreeViewItem))
                return;

            TreeViewItem item = (TreeViewItem)treeOrganization.SelectedItem;

            //TODO: 这个方法在单选时选择有bug，可能是Selected事件与Checked事件有冲突
            //CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(item);
            //if (cbx != null)
            //    cbx.IsChecked = true;

            if (item == null || item.DataContext == null)
                return;

            OrgTreeItemTypes type = (OrgTreeItemTypes)(item.Tag);

            if ((SelectedObjType == OrgTreeItemTypes.All || SelectedObjType == OrgTreeItemTypes.Personnel)
                && type == OrgTreeItemTypes.Post)
            {
                //TODO:多选 
                ExtOrgObj obj = new ExtOrgObj();

                obj = item.DataContext as ExtOrgObj;

                if (type == OrgTreeItemTypes.Post && obj != null)
                {
                    //加载员工
                    BindEmployee(item, obj.ObjectID);
                }
            }


        }

        private void BindEmployee(TreeViewItem item, string postID)
        {
            PersonnelWS.PersonnelServiceClient perclient = new PersonnelWS.PersonnelServiceClient();
            perclient.GetEmployeePostByPostIDCompleted += new EventHandler<PersonnelWS.GetEmployeePostByPostIDCompletedEventArgs>(perclient_GetEmployeePostByPostIDCompleted);

            perclient.GetEmployeePostByPostIDAsync(postID, item);
        }

        void perclient_GetEmployeePostByPostIDCompleted(object sender, PersonnelWS.GetEmployeePostByPostIDCompletedEventArgs e)
        {
            List<PersonnelWS.T_HR_EMPLOYEEPOST> eplist = new List<PersonnelWS.T_HR_EMPLOYEEPOST>();
            if (e.Result != null)
            {
                eplist = e.Result.ToList();
            }

            TreeViewItem parentItem = e.UserState as TreeViewItem;

            if (eplist == null || eplist.Count == 0 || parentItem == null)
                return;

            parentItem.Items.Clear();

            foreach (PersonnelWS.T_HR_EMPLOYEEPOST ep in eplist)
            {
                TreeViewItem subItem = new TreeViewItem();
                subItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                subItem.Header = ep.T_HR_EMPLOYEE.EMPLOYEECNAME;

                ExtOrgObj obj = new ExtOrgObj();
                obj.ObjectInstance = ep.T_HR_EMPLOYEE;
                subItem.DataContext = obj;

                //设置父级对像
                obj.ParentObject = parentItem.DataContext;

                //标记为岗位
                subItem.Tag = OrgTreeItemTypes.Personnel;
                subItem.HeaderTemplate = treeViewItemTemplate;
                parentItem.Items.Add(subItem);
            }

        }

        protected void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked.GetValueOrDefault() == false)
            {
                return;
            }
            if (MultiSelected && !SelectSameGradeOnly)
            {
                return;
            }
            if (MultiSelected && SelectSameGradeOnly)
            {
                TreeViewItem currentItem = null;
                foreach (TreeViewItem item in treeOrganization.Items)
                {

                    TreeViewItem myItem =
                        (TreeViewItem)(treeOrganization.ItemContainerGenerator.ContainerFromItem(item));

                    CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                    if (cbx == (CheckBox)sender)
                    {
                        currentItem = myItem;
                        break;
                    }
                    FindTreeViewItemByChkBox(item, (CheckBox)sender, ref currentItem);
                }
                if (currentItem != null)
                {
                    UnCheckChildCheckbox(currentItem, (CheckBox)sender);
                    UnCheckParentCheckbox(currentItem);
                }
            }
            else
            {
                foreach (TreeViewItem item in treeOrganization.Items)
                {

                    TreeViewItem myItem =
                        (TreeViewItem)(treeOrganization.ItemContainerGenerator.ContainerFromItem(item));

                    CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                    BindEmployeeByTreeViewItem(myItem);
                    if (cbx == (CheckBox)sender)
                    {
                        continue;
                    }
                    if (cbx != (CheckBox)sender)
                    {
                        if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
                        {
                            cbx.IsChecked = false;
                        }
                    }
                    UnCheckChildCheckbox(item, (CheckBox)sender);
                }
            }
        }

        private void UnCheckChildCheckbox(TreeViewItem item, CheckBox selfCheckbox)
        {

            foreach (TreeViewItem childItem in item.Items)
            {
                TreeViewItem myItem =
                (TreeViewItem)(item.ItemContainerGenerator.ContainerFromItem(childItem));
                BindEmployeeByTreeViewItem(myItem);
                CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                if (cbx == selfCheckbox)
                {
                    continue;
                }
                if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
                {
                    cbx.IsChecked = false;
                }

                UnCheckChildCheckbox(childItem, selfCheckbox);
            }
        }


        private void BindEmployeeByTreeViewItem(TreeViewItem item)
        {
            if (item == null)
                return;
            ExtOrgObj tmp = new ExtOrgObj();
            tmp = item.DataContext as ExtOrgObj;
            if (tmp.ObjectType == OrgTreeItemTypes.Personnel)
            {
                foreach (var h in ((PersonnelWS.T_HR_EMPLOYEE)tmp.ObjectInstance).T_HR_EMPLOYEEPOST)
                {
                    PersonnelWS.T_HR_POST tmpPost_P = new PersonnelWS.T_HR_POST();
                    OrganizationWS.T_HR_POST tmpPost_O = new OrganizationWS.T_HR_POST();
                    tmpPost_O = BindEmployeePost(item);
                }
            }
        }

        private OrganizationWS.T_HR_POST BindEmployeePost(TreeViewItem item)
        {
            T_HR_POST tempPost = new T_HR_POST();
            if (item == null || item.Parent == null)
                return null;
            if (((ExtOrgObj)(((TreeViewItem)item.Parent).DataContext)).ObjectType == OrgTreeItemTypes.Post)
            {
                tempPost = ((ExtOrgObj)((TreeViewItem)item.Parent).DataContext).ObjectInstance as OrganizationWS.T_HR_POST;

            }
            if (((ExtOrgObj)(((TreeViewItem)((TreeViewItem)item.Parent).Parent).DataContext)).ObjectType == OrgTreeItemTypes.Department)
            {
                tempPost.T_HR_DEPARTMENT = ((ExtOrgObj)(((TreeViewItem)((TreeViewItem)item.Parent).Parent).DataContext)).ObjectInstance as OrganizationWS.T_HR_DEPARTMENT;

            }
            return tempPost;
        }

        private void FindTreeViewItemByChkBox(TreeViewItem item, CheckBox selfCheckbox, ref TreeViewItem currentItem)
        {
            foreach (TreeViewItem childItem in item.Items)
            {
                TreeViewItem myItem =
                        (TreeViewItem)(item.ItemContainerGenerator.ContainerFromItem(childItem));
                CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                if (cbx == selfCheckbox)
                {
                    currentItem = myItem;
                    return;
                }
                FindTreeViewItemByChkBox(childItem, selfCheckbox, ref currentItem);
            }
        }

        private TreeViewItem FindTreeViewItemByChkBox1(TreeViewItem item, CheckBox selfCheckbox)
        {
            foreach (TreeViewItem childItem in item.Items)
            {
                TreeViewItem myItem =
                        (TreeViewItem)(treeOrganization.ItemContainerGenerator.ContainerFromItem(item));
                CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                if (cbx == selfCheckbox)
                {
                    return myItem;
                }
                FindTreeViewItemByChkBox1(childItem, selfCheckbox);
            }
            return null;
        }

        private void UnCheckParentCheckbox(TreeViewItem item)
        {
            if (item == null || item.Parent == null)
                return;
            TreeViewItem myItem = item.Parent as TreeViewItem;
            CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);

            if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
            {
                cbx.IsChecked = false;
                return;
            }
            UnCheckParentCheckbox(myItem);
        }
        //private string parentName = "";
        //private void UnCheckChildCheckbox(TreeViewItem item, CheckBox selfCheckbox)
        //{

        //    foreach (TreeViewItem childItem in item.Items)
        //    {
        //        TreeViewItem myItem =
        //        (TreeViewItem)(item.ItemContainerGenerator.ContainerFromItem(childItem));
        //        CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);
        //        if (cbx == selfCheckbox)
        //        {
        //            UnCheckChildCheckbox(myItem, selfCheckbox);
        //        }
        //        else
        //        {

        //            //if (MultiSelected && parentName == GetParentName(myItem) && cbx.IsChecked==true)
        //            if (MultiSelected && !HaveChildItemChecked(myItem, selfCheckbox))
        //            {
        //                return;
        //            }
        //            if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
        //            {
        //                cbx.IsChecked = false;
        //            }
        //            UnCheckChildCheckbox(childItem, selfCheckbox);
        //        }
        //    }
        //}


        private bool IsSameParent(TreeViewItem item, CheckBox selfCheckbox)
        {
            if (item == null || item.Parent == null)
                return false;
            ExtOrgObj obj = new ExtOrgObj();
            TreeViewItem parentItem = new TreeViewItem();
            parentItem = item.Parent as TreeViewItem;
            obj = parentItem.DataContext as ExtOrgObj;
            //ExtOrgObj obj1 = new ExtOrgObj();
            //obj1= selfCheckbox.DataContext as ExtOrgObj;
            return obj.ObjectName == selfCheckbox.DataContext.ToString();
        }

        private bool HaveChildItemChecked(TreeViewItem item, CheckBox selfCheckbox)
        {
            if (item == null)
                return false;
            CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(item);
            if (cbx.IsChecked == true && !IsSameParent(item, selfCheckbox))
            {
                return true;
            }
            foreach (TreeViewItem childItem in item.Items)
            {
                return HaveChildItemChecked(childItem, selfCheckbox);
            }
            return false;
        }
        #endregion

        void client_GetPostByEntityPermCompleted(object sender, GetPostByEntityPermCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<T_HR_POST> entlist = e.Result.ToList();
                UICache.CreateCache("SYS_PostInfo", entlist);
            }
            BindPosition();
        }

        void client_GetDepartmentByEntityPermCompleted(object sender, GetDepartmentByEntityPermCompletedEventArgs e)
        {
            if (e.Error == null)
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

                UICache.CreateCache("SYS_DepartmentInfo", allDepartments);

                BindCompany();

                client.GetPostByEntityPermAsync(CurrentUserID, Perm, Entity);
            }            
        }

        void client_GetCompanyByEntityPermCompleted(object sender, GetCompanyByEntityPermCompletedEventArgs e)
        {
            if (e.Error == null)
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

                UICache.CreateCache("SYS_DepartmentInfo", allCompanys);

                client.GetDepartmentByEntityPermAsync(CurrentUserID, Perm, Entity);
            }
        }

        private void InitCompany()
        {
            if (Application.Current.Resources["SYS_DepartmentInfo"] != null)
            {
                BindCompany();
                if (SelectedObjType != OrgTreeItemTypes.Company && SelectedObjType != OrgTreeItemTypes.Department)
                {
                    if (Application.Current.Resources["SYS_PostInfo"] != null)
                    {
                        BindPosition();
                        return;
                    }
                    //client.GetPostByEntityPermAsync("4afd1d50-b84d-4ea3-8c81-0078c35fea5e", "0", "T_FB_CHARGEAPPLYMASTER");
                    client.GetPostByEntityPermAsync(CurrentUserID, Perm, Entity);
                }
                return;
            }
            //client.GetDepartmentByEntityPermAsync("4afd1d50-b84d-4ea3-8c81-0078c35fea5e", "0", "T_FB_CHARGEAPPLYMASTER");
            client.GetDepartmentByEntityPermAsync(CurrentUserID, Perm, Entity);
        }
    }
}
