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

namespace SMT.SaaS.FrameworkUI.OrganizationControl
{
    public partial class OrgnizationTreeAll : UserControl
    {

        DataTemplate treeViewItemTemplate = null;
        Style treeViewItemStyle = null;
        OrganizationServiceClient client = null;
        private List<T_HR_COMPANY> allCompanys;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_POST> allPositions;
        SMTLoading loadbar = new SMTLoading();
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

                    CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);
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
        public OrgnizationTreeAll()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OrganizationTree_Loaded);
        }

        public OrgnizationTreeAll(string userID, string perm, string entity)
            : this()
        {
            CurrentUserID = userID;
            Perm = perm;
            Entity = entity;
        }

        public OrgnizationTreeAll(string perm, string entity)
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
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            client = new OrganizationServiceClient();
            client.GetCompanyByEntityPermCompleted += new EventHandler<GetCompanyByEntityPermCompletedEventArgs>(client_GetCompanyByEntityPermCompleted);
            client.GetDepartmentByEntityPermCompleted += new EventHandler<GetDepartmentByEntityPermCompletedEventArgs>(client_GetDepartmentByEntityPermCompleted);
            client.GetPostByEntityPermCompleted += new EventHandler<GetPostByEntityPermCompletedEventArgs>(client_GetPostByEntityPermCompleted);
            client.GetALLCompanyViewCompleted += new EventHandler<GetALLCompanyViewCompletedEventArgs>(client_GetALLCompanyViewCompleted);
            client.GetAllDepartmentViewCompleted += new EventHandler<GetAllDepartmentViewCompletedEventArgs>(client_GetAllDepartmentViewCompleted);
            client.GetAllPostViewCompleted += new EventHandler<GetAllPostViewCompletedEventArgs>(client_GetAllPostViewCompleted);

            treeOrganization.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(lookUpTree_SelectedItemChanged);

            treeViewItemStyle = Application.Current.Resources["TreeViewItemStyle"] as Style;
            treeViewItemTemplate = this.Resources["treeViewItemHead"] as DataTemplate;

            if (Application.Current.Resources.Contains("SYS_CompanyInfo"))
            {
                InitCompany();
            }
            else
            {
                BindTree();
            }

        }


        void client_GetAllPostViewCompleted(object sender, GetAllPostViewCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {

                    if (e.Result != null)
                    {
                        List<V_POST> vpostList = e.Result.ToList();
                        List<T_HR_POST> entlist = new List<T_HR_POST>();
                        foreach (var ent in vpostList)
                        {
                            T_HR_POST pt = new T_HR_POST();
                            pt.POSTID = ent.POSTID;
                            pt.FATHERPOSTID = ent.FATHERPOSTID;
                            pt.CHECKSTATE = ent.CHECKSTATE;
                            pt.EDITSTATE = ent.EDITSTATE;

                            pt.T_HR_POSTDICTIONARY = new T_HR_POSTDICTIONARY();
                            // pt.T_HR_POSTDICTIONARY.POSTDICTIONARYID = ent.POSTDICTIONARYID;
                            pt.T_HR_POSTDICTIONARY.POSTNAME = ent.POSTNAME;

                            // pt.T_HR_DEPARTMENT = new T_HR_DEPARTMENT();
                            pt.T_HR_DEPARTMENT = allDepartments.Where(s => s.DEPARTMENTID == ent.DEPARTMENTID).FirstOrDefault();

                            //pt.T_HR_DEPARTMENT = new T_HR_DEPARTMENT();
                            //pt.T_HR_DEPARTMENT.DEPARTMENTID = ent.DEPARTMENTID;

                            //pt.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                            //pt.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = Guid.NewGuid().ToString();
                            // pt.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = ent.DEPARTMENTNAME;

                            // pt.T_HR_DEPARTMENT.T_HR_COMPANY = new T_HR_COMPANY();
                            // pt.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID = ent.COMPANYID;
                            //  pt.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME = ent.CNAME;

                            entlist.Add(pt);
                        }
                        UICache.CreateCache("SYS_PostInfo", entlist);
                    }
                    //  BindPosition();
                }
            }
            catch (Exception ex)
            {
            }
            loadbar.Stop();
        }

        void client_GetAllDepartmentViewCompleted(object sender, GetAllDepartmentViewCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<V_DEPARTMENT> entTemps = e.Result;
                allDepartments = new List<T_HR_DEPARTMENT>();
                allDepartments.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                foreach (var ent in ents)
                {
                    T_HR_DEPARTMENT dep = new T_HR_DEPARTMENT();
                    dep.DEPARTMENTID = ent.DEPARTMENTID;
                    dep.FATHERID = ent.FATHERID;
                    dep.FATHERTYPE = ent.FATHERTYPE;
                    dep.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                    dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = ent.DEPARTMENTDICTIONARYID;
                    dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = ent.DEPARTMENTNAME;
                    //dep.T_HR_COMPANY = new T_HR_COMPANY();
                    //dep.T_HR_COMPANY.COMPANYID = ent.COMPANYID;
                    //dep.T_HR_COMPANY.CNAME = ent.CNAME;
                    dep.T_HR_COMPANY = new T_HR_COMPANY();
                    dep.T_HR_COMPANY = allCompanys.Where(s => s.COMPANYID == ent.COMPANYID).FirstOrDefault();
                    dep.DEPARTMENTBOSSHEAD = ent.DEPARTMENTBOSSHEAD;
                    dep.SORTINDEX = ent.SORTINDEX;
                    dep.CHECKSTATE = ent.CHECKSTATE;
                    dep.EDITSTATE = ent.EDITSTATE;
                    allDepartments.Add(dep);
                }

                UICache.CreateCache("SYS_DepartmentInfo", allDepartments);

                BindCompany();

                client.GetAllPostViewAsync("");
            }
            else
            {
                loadbar.Stop();
            }
        }

        void client_GetALLCompanyViewCompleted(object sender, GetALLCompanyViewCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<V_COMPANY> entTemps = e.Result;
                allCompanys = new List<T_HR_COMPANY>();
                allCompanys.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                foreach (var ent in ents)
                {
                    T_HR_COMPANY company = new T_HR_COMPANY();
                    company.COMPANYID = ent.COMPANYID;
                    company.CNAME = ent.CNAME;
                    company.ENAME = ent.ENAME;
                    if (!string.IsNullOrEmpty(ent.BRIEFNAME))
                    {
                        company.BRIEFNAME = ent.BRIEFNAME;
                    }
                    else
                    {
                        company.BRIEFNAME = ent.CNAME;
                    }

                    company.COMPANRYCODE = ent.COMPANRYCODE;
                    company.SORTINDEX = ent.SORTINDEX;
                    company.T_HR_COMPANY2 = new T_HR_COMPANY();
                    company.T_HR_COMPANY2.COMPANYID = ent.FATHERCOMPANYID;
                    company.FATHERID = ent.FATHERID;
                    company.FATHERTYPE = ent.FATHERTYPE;
                    company.CHECKSTATE = ent.CHECKSTATE;
                    company.EDITSTATE = ent.EDITSTATE;
                    allCompanys.Add(company);
                }

                UICache.CreateCache("SYS_CompanyInfo", allCompanys);

                client.GetAllDepartmentViewAsync("");
            }
            else
            {
                loadbar.Stop();
            }
        }

        public void BindTree()
        {
            //client.GetCompanyByEntityPermAsync("4afd1d50-b84d-4ea3-8c81-0078c35fea5e", "0", "T_FB_CHARGEAPPLYMASTER");
            loadbar.Start();
            // client.GetCompanyByEntityPermAsync(CurrentUserID, Perm, Entity);
            client.GetALLCompanyViewAsync("");

        }

        #region 绑定数据
        /// <summary>
        /// 绑定组织架构节点
        /// </summary>
        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            allCompanys = Application.Current.Resources["SYS_CompanyInfo"] as List<T_HR_COMPANY>;

            allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<T_HR_DEPARTMENT>;


            if (allCompanys == null)
            {
                loadbar.Stop();
                return;
            }
            else
            {
                allCompanys = allCompanys.Where(s => s.EDITSTATE == "1").ToList();
                allCompanys = allCompanys.OrderBy(s => s.SORTINDEX).ToList();
            }
            if (allDepartments != null)
            {
                allDepartments = allDepartments.Where(s => s.EDITSTATE == "1").ToList();
                allDepartments = allDepartments.OrderBy(s => s.SORTINDEX).ToList();
            }
            T_HR_COMPANY MOstTopCompany = GetFatherByDepartmentID(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);

            List<T_HR_COMPANY> TopCompany = new List<T_HR_COMPANY>();

            //foreach (T_HR_COMPANY tmpOrg in allCompanys)
            //{
            //    //如果当前公司没有父机构的ID，则为顶级公司
            //    if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
            //    {
            TreeViewItem item = new TreeViewItem();
            //item.Header = tmpOrg.CNAME;
            item.Header = MOstTopCompany.BRIEFNAME;
            item.HeaderTemplate = treeViewItemTemplate;
            item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

            ExtOrgObj obj = new ExtOrgObj();
            obj.ObjectInstance = MOstTopCompany;

            item.DataContext = obj;

            //标记为公司
            item.Tag = OrgTreeItemTypes.Company;
            treeOrganization.Items.Add(item);
            TopCompany.Add(MOstTopCompany);
            //    }
            //    else
            //    {
            //        //查询当前公司是否在公司集合内有父公司存在
            //        var ent = from c in allCompanys
            //                  where c.COMPANYID == tmpOrg.FATHERID && tmpOrg.FATHERTYPE == "0"
            //                  select c;

            //        var ent2 = from c in allDepartments
            //                   where tmpOrg.FATHERTYPE == "1" && tmpOrg.FATHERID == c.DEPARTMENTID
            //                   select c;

            //        //如果不存在，则为顶级公司
            //        if (ent.Count() == 0 && ent2.Count() == 0)
            //        {
            //            TreeViewItem item = new TreeViewItem();
            //            //item.Header = tmpOrg.CNAME;
            //            item.Header = tmpOrg.BRIEFNAME;
            //            item.HeaderTemplate = treeViewItemTemplate; ;
            //            item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

            //            ExtOrgObj obj = new ExtOrgObj();
            //            obj.ObjectInstance = tmpOrg;

            //            item.DataContext = obj;

            //            //标记为公司
            //            item.Tag = OrgTreeItemTypes.Company;
            //            treeOrganization.Items.Add(item);

            //            TopCompany.Add(tmpOrg);
            //        }
            //    }
            //}
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
                //item.Header = childCompany.CNAME;
                item.Header = childCompany.BRIEFNAME;
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

                //childItem.Header = childOrg.CNAME;
                childItem.Header = childOrg.BRIEFNAME;
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

        //private void BindDepartment()
        //{
        //    allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<T_HR_DEPARTMENT>;
        //    if (allDepartments == null)
        //    {
        //        return;
        //    }

        //    if (allDepartments.Count() == 0)
        //    {
        //        return;
        //    }

        //    foreach (T_HR_DEPARTMENT tmpDep in allDepartments)
        //    {
        //        if (tmpDep.T_HR_COMPANY == null)
        //            continue;

        //        TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, tmpDep.T_HR_COMPANY.COMPANYID);
        //        if (parentItem != null)
        //        {
        //            TreeViewItem item = new TreeViewItem();
        //            item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //            item.Header = tmpDep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

        //            ExtOrgObj obj = new ExtOrgObj();
        //            obj.ObjectInstance = tmpDep;
        //            item.DataContext = obj;

        //            //设置父级对像
        //            obj.ParentObject = parentItem.DataContext;
        //            item.HeaderTemplate = treeViewItemTemplate;
        //            //标记为部门
        //            item.Tag = OrgTreeItemTypes.Department;
        //            parentItem.Items.Add(item);
        //        }
        //    }
        //}

        private void BindPosition()
        {
            allPositions = Application.Current.Resources["SYS_PostInfo"] as List<T_HR_POST>;
            if (allPositions == null)
            {
                loadbar.Stop();
                return;
            }

            if (allPositions.Count() == 0)
            {
                loadbar.Stop();
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
            //if (treeOrganization.Items.Count > 0)
            //{
            //    TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
            //    selectedItem.IsSelected = true;
            //}
        }
        public Dictionary<string, string> depIDsCach = new Dictionary<string, string>();
        private void BindPosition(string departmentID)
        {
            loadbar.Start();
            List<T_HR_POST> Positions = Application.Current.Resources["SYS_PostInfo"] as List<T_HR_POST>;
            Positions = Positions.Where(p => p.T_HR_DEPARTMENT != null).Where(s => s.T_HR_DEPARTMENT.DEPARTMENTID == departmentID && s.EDITSTATE == "1").ToList();
            if (Positions == null)
            {
                loadbar.Stop();
                return;
            }

            if (Positions.Count() == 0)
            {
                loadbar.Stop();
                return;
            }


            foreach (T_HR_POST tmpPosition in Positions)
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
            depIDsCach.Add(departmentID, "1");
            //树全部展开
            //  treeOrganization.ExpandAll();
            //if (treeOrganization.Items.Count > 0)
            //{
            //    TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
            //    selectedItem.IsSelected = true;
            //}
            loadbar.Stop();
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

                CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);

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
                    if (item.Items.Count > 0)
                        return;
                    BindEmployee(item, obj.ObjectID);
                }

            }
            else if (type == OrgTreeItemTypes.Department)
            {
                ExtOrgObj obj = new ExtOrgObj();

                obj = item.DataContext as ExtOrgObj;
                //if (item.Items.Count > 0)
                //    return;
                if (depIDsCach.ContainsKey(obj.ObjectID)) return;
                BindPosition(obj.ObjectID);
            }

        }

        private void BindEmployee(TreeViewItem item, string postID)
        {
            //postIDsCach.Add(postID,"0");
            if (postIDsCach.ContainsKey(postID)) return;
            PersonnelWS.PersonnelServiceClient perclient = new PersonnelWS.PersonnelServiceClient();
            perclient.GetEmployeePostByPostIDCompleted += new EventHandler<PersonnelWS.GetEmployeePostByPostIDCompletedEventArgs>(perclient_GetEmployeePostByPostIDCompleted);
            loadbar.Start();
            perclient.GetEmployeePostByPostIDAsync(postID, item);
        }
        public Dictionary<string, string> postIDsCach = new Dictionary<string, string>();
        void perclient_GetEmployeePostByPostIDCompleted(object sender, PersonnelWS.GetEmployeePostByPostIDCompletedEventArgs e)
        {
            try
            {
                List<PersonnelWS.T_HR_EMPLOYEEPOST> eplist = new List<PersonnelWS.T_HR_EMPLOYEEPOST>();
                if (e.Result != null)
                {
                    eplist = e.Result.ToList();
                }

                TreeViewItem parentItem = e.UserState as TreeViewItem;

                if (eplist == null || eplist.Count == 0 || parentItem == null)
                {
                    loadbar.Stop();
                    return;
                }
                parentItem.Items.Clear();

                string postID = string.Empty;
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
                    #region
                    ExtOrgObj post = (ExtOrgObj)obj.ParentObject;
                    string postName = post.ObjectName;
                    //  fromPostLevel=(post as SMT.Saas.Tools.OrganizationWS.T_HR_POST).POSTLEVEL.ToString();
                    if (string.IsNullOrEmpty(postID))
                    {
                        postID = post.ObjectID;
                    }
                    ExtOrgObj dept = (ExtOrgObj)post.ParentObject;
                    string deptName = dept.ObjectName;

                    // ExtOrgObj corp = (ExtOrgObj)dept.ParentObject;
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpName = corp.CNAME;
                    obj.ObjectName = ep.T_HR_EMPLOYEE.EMPLOYEECNAME + "-" + postName + "-" + deptName + "-" + corpName;
                    #endregion
                    //标记为岗位
                    subItem.Tag = OrgTreeItemTypes.Personnel;
                    subItem.HeaderTemplate = treeViewItemTemplate;
                    parentItem.Items.Add(subItem);
                }
                postIDsCach.Add(postID, "1");
            }
            catch (Exception ex)
            {

            }
            loadbar.Stop();
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

                    CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);
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

                    CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);
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
                CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);
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
                CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);
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
                CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);
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
            CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);

            if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
            {
                cbx.IsChecked = false;
                return;
            }
            UnCheckParentCheckbox(myItem);
        }


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
            CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(item);
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
            try
            {
                if (e.Error == null)
                {

                    if (e.Result != null)
                    {
                        List<T_HR_POST> entlist = e.Result.ToList();
                        UICache.CreateCache("SYS_PostInfo", entlist);
                    }
                    BindPosition();
                }
            }
            catch (Exception ex)
            {
            }
            loadbar.Stop();
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
            else
            {
                loadbar.Stop();
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

                UICache.CreateCache("SYS_CompanyInfo", allCompanys);

                client.GetDepartmentByEntityPermAsync(CurrentUserID, Perm, Entity);
            }
            else
            {
                loadbar.Stop();
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
                        // BindPosition();
                        return;
                    }
                    //client.GetPostByEntityPermAsync("4afd1d50-b84d-4ea3-8c81-0078c35fea5e", "0", "T_FB_CHARGEAPPLYMASTER");
                    // client.GetPostByEntityPermAsync(CurrentUserID, Perm, Entity);
                    client.GetAllPostViewAsync("");
                }
                return;
            }
            //client.GetDepartmentByEntityPermAsync("4afd1d50-b84d-4ea3-8c81-0078c35fea5e", "0", "T_FB_CHARGEAPPLYMASTER");
            // client.GetDepartmentByEntityPermAsync(CurrentUserID, Perm, Entity);
            client.GetAllDepartmentViewAsync("");
        }

        /// <summary>
        /// 根据当前登录人的公司获取顶级公司
        /// </summary>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        public T_HR_COMPANY GetFatherByDepartmentID(string departmentID)
        {
            T_HR_DEPARTMENT department = new T_HR_DEPARTMENT();
            T_HR_COMPANY company = new T_HR_COMPANY();
            Dictionary<string, string> fathers = new Dictionary<string, string>();
            string fatherType = "0";
            string fatherID = "";
            bool hasFather = false;

            department = (from c in allDepartments
                          where c.DEPARTMENTID == departmentID
                          select c).FirstOrDefault();

            if (department != null)
            {
                if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                {
                    fatherType = department.FATHERTYPE;
                    fatherID = department.FATHERID;
                    hasFather = true;
                }
                else
                {
                    hasFather = false;
                }
            }

            while (hasFather)
            {
                if (fatherType == "1" && !string.IsNullOrEmpty(fatherID))
                {
                    department = (from de in allDepartments
                                  where de.DEPARTMENTID == fatherID
                                  select de).FirstOrDefault();
                    if (department != null)
                    {
                        //fathers.Add(department.DEPARTMENTID, "1");
                        if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                        {
                            fatherID = department.FATHERID;
                            fatherType = department.FATHERTYPE;
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else
                    {
                        hasFather = false;
                    }
                }
                else if (fatherType == "0" && !string.IsNullOrEmpty(fatherID))
                {
                    company = (from com in allCompanys
                               where com.COMPANYID == fatherID
                               select com).FirstOrDefault();

                    if (company != null)
                    {
                        //fathers.Add(company.COMPANYID, "0");
                        if (!string.IsNullOrEmpty(company.FATHERTYPE) && !string.IsNullOrEmpty(company.FATHERID))
                        {
                            fatherID = company.FATHERID;
                            fatherType = company.FATHERTYPE;
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else
                    {
                        hasFather = false;
                    }

                }
                else
                {
                    hasFather = false;
                }

            }
            return company;
        }
    }
}
