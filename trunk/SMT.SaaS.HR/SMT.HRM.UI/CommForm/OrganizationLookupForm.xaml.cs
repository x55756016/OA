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
using System.Collections.ObjectModel;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.Helper;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI
{
    public partial class OrganizationLookupForm : System.Windows.Controls.Window
    {
        private List<T_HR_COMPANY> allCompanys;
        private List<T_HR_DEPARTMENT> allDepartments;
        private List<T_HR_POST> allPositions;
        SMTLoading loadbar = new SMTLoading();
        public object SelectedObj
        {
            get;
            set;
        }
        public OrgTreeItemTypes SelectedObjType { get; set; }
        OrganizationServiceClient client = new OrganizationServiceClient();

        public event EventHandler SelectedClick;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OrganizationLookupForm()
        {
            InitializeComponent();
            InitParas();

            try
            {
                if (ParentWindow != null)
                {
                    SVShowContent.Width = ParentWindow.ActualWidth;
                    SVShowContent.Height = ParentWindow.ActualHeight - ButtonAreaPanel.ActualHeight;
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(ex.ToString());

            }
        }

        /// <summary>
        /// 绑定事件
        /// </summary>
        private void InitParas()
        {
            //初始化控件的状态            
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            client.GetALLCompanyViewCompleted += new EventHandler<GetALLCompanyViewCompletedEventArgs>(client_GetALLCompanyViewCompleted);
            client.GetAllDepartmentViewCompleted += new EventHandler<GetAllDepartmentViewCompletedEventArgs>(client_GetAllDepartmentViewCompleted);
            client.GetAllPostViewCompleted += new EventHandler<GetAllPostViewCompletedEventArgs>(client_GetAllPostViewCompleted);
            BindTree();
        }
        /// <summary>
        /// 绑定树型控件
        /// </summary>
        private void BindTree()
        {
            //触发获取正常的公司
            if (Application.Current.Resources["ORGTREESYSCompanyInfo"] == null)
            {
                loadbar.Start();
                // client.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                client.GetALLCompanyViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                BindCompany();
            }
        }

        /// <summary>
        /// 获取正常的公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

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
                    company.COMPANYTYPE = ent.COMPANYTYPE;
                    allCompanys.Add(company);
                }

                UICache.CreateCache("ORGTREESYSCompanyInfo", allCompanys);

                client.GetAllDepartmentViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                loadbar.Stop();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }
        /// <summary>
        /// 获取正常可用的部门
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                UICache.CreateCache("ORGTREESYSDepartmentInfo", allDepartments);

                BindCompany();

                client.GetAllPostViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                loadbar.Stop();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        /// <summary>
        /// 获取岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        UICache.CreateCache("ORGTREESYSPostInfo", entlist);
                    }
                    //  BindPosition();
                }
                else
                {
                    loadbar.Stop();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                           Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
            }

        }
        /// <summary>
        /// 绑定组织架构节点
        /// </summary>
        private void BindCompany()
        {
            try
            {
                treeOrganization.Items.Clear();
                allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<T_HR_COMPANY>;

                allDepartments = Application.Current.Resources["ORGTREESYSDepartmentInfo"] as List<T_HR_DEPARTMENT>;



                if (allCompanys == null)
                {
                    return;
                }
                else
                {
                    allCompanys = allCompanys.Where(s => s.CHECKSTATE == "2" && s.EDITSTATE == "1").ToList();
                    allCompanys = allCompanys.OrderBy(s => s.SORTINDEX).ToList();
                }
                if (allDepartments != null)
                {
                    allDepartments = allDepartments.Where(s => s.CHECKSTATE == "2" && s.EDITSTATE == "1").ToList();
                    allDepartments = allDepartments.OrderBy(s => s.SORTINDEX).ToList();
                }
                List<T_HR_COMPANY> TopCompany = new List<T_HR_COMPANY>();

                foreach (T_HR_COMPANY tmpOrg in allCompanys)
                {
                    //如果当前公司没有父机构的ID，则为顶级公司
                    if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
                    {
                        TreeViewItem item = new TreeViewItem();
                        //item.Header = tmpOrg.CNAME;
                        item.Header = tmpOrg.BRIEFNAME;
                        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        item.DataContext = tmpOrg;

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
                            //item.Header = tmpOrg.CNAME;
                            item.Header = tmpOrg.BRIEFNAME;
                            item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                            item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                            item.DataContext = tmpOrg;

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
                loadbar.Stop();
                //allPositions = Application.Current.Resources["ORGTREESYSPostInfo"] as List<T_HR_POST>;
                //BindPosition();
            }
            catch (Exception ex)
            {
                loadbar.Stop();
            }

        }

        private void AddOrgNode(List<T_HR_COMPANY> lsCompany, List<T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
        {
            //绑定公司的子公司
            foreach (var childCompany in lsCompany)
            {
                TreeViewItem item = new TreeViewItem();
                //item.Header = childCompany.CNAME;
                item.Header = childCompany.BRIEFNAME;
                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                item.DataContext = childCompany;
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
            try
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
                            item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
                            item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

                            //标记为岗位
                            item.Tag = OrgTreeItemTypes.Post;
                            parentItem.Items.Add(item);
                        }
                    }
                }
                loadbar.Stop();
            }
            catch (Exception ex)
            {
                loadbar.Stop();
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
        /// <summary>
        /// 根据部门绑定岗位
        /// </summary>
        /// <param name="departmentID"></param>
        private void BindPosition(string departmentID, TreeViewItem treeItem)
        {
            try
            {
                loadbar.Start();

                List<T_HR_POST> Positions = Application.Current.Resources["ORGTREESYSPostInfo"] as List<T_HR_POST>;
                Positions = Positions.Where(p => p.T_HR_DEPARTMENT != null).Where(s => s.T_HR_DEPARTMENT.DEPARTMENTID == departmentID && s.CHECKSTATE == "2" && s.EDITSTATE == "1").ToList();
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
                        item.DataContext = tmpPosition;
                        item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

                        //标记为岗位
                        item.Tag = OrgTreeItemTypes.Post;
                        parentItem.Items.Add(item);
                    }
                }
                depIDsCach.Add(departmentID, "1");
                loadbar.Stop();
            }
            catch (Exception ex)
            {
                loadbar.Stop();
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

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedObj == null)
            {
                // MessageBox.Show(Utility.GetResourceStr("SELECTEIONISNULL"));
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTEIONISNULL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTEIONISNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            this.Close();

            if (SelectedClick != null)
                SelectedClick(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lookUpTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treeOrganization.SelectedItem == null)
                return;
            if (treeOrganization.SelectedItem.GetType() != typeof(TreeViewItem))
                return;
            TreeViewItem item = (TreeViewItem)treeOrganization.SelectedItem;

            if (item == null || item.DataContext == null)
                return;

            OrgTreeItemTypes type = (OrgTreeItemTypes)(item.Tag);
            if (SelectedObjType == OrgTreeItemTypes.All || type == SelectedObjType)
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
                if (SelectedObj is T_HR_DEPARTMENT)
                {
                    if (depIDsCach.ContainsKey((item.DataContext as T_HR_DEPARTMENT).DEPARTMENTID))
                        return;
                    BindPosition((item.DataContext as T_HR_DEPARTMENT).DEPARTMENTID, item);
                }

            }
            else if (SelectedObjType == OrgTreeItemTypes.Post && type == OrgTreeItemTypes.Department)
            {
                if (depIDsCach.ContainsKey((item.DataContext as T_HR_DEPARTMENT).DEPARTMENTID))
                    return;
                BindPosition((item.DataContext as T_HR_DEPARTMENT).DEPARTMENTID, item);
            }
            else
            {
                SelectedObj = null;
            }

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.Remove("ORGTREESYSCompanyInfo");
            Application.Current.Resources.Remove("ORGTREESYSDepartmentInfo");
            Application.Current.Resources.Remove("ORGTREESYSPostInfo");
            depIDsCach.Clear();
            BindTree();
        }
    }
}

