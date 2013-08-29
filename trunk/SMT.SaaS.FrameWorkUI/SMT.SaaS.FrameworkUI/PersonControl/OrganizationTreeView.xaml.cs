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
using System.Collections.ObjectModel;

using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.FrameworkUI.OrganizationTreeView
{
    public partial class OrganizationTreeView : UserControl
    {
        /// <summary>
        /// 组织架构类型
        /// </summary>
        public string sType = "";
        /// <summary>
        /// 对应组织架构ID
        /// </summary>
        public string sValue = "";
        /// <summary>
        /// 对应组织架构名称
        /// </summary>
        public string sName = "";
        public event EventHandler SelectedClick;
        OrganizationServiceClient orgClient;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;
        SMTLoading loadbar = new SMTLoading();
        public OrganizationTreeView()
        {
            InitializeComponent();
            initPara();
        }
        void initPara()
        {
            orgClient = new OrganizationServiceClient();
            //初始化控件的状态            
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            //client.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(client_GetCompanyActivedCompleted);
            //client.GetDepartmentActivedCompleted += new EventHandler<GetDepartmentActivedCompletedEventArgs>(client_GetDepartmentActivedCompleted);
            //client.GetPostActivedCompleted += new EventHandler<GetPostActivedCompletedEventArgs>(client_GetPostActivedCompleted);
            orgClient.GetALLCompanyViewCompleted += new EventHandler<GetALLCompanyViewCompletedEventArgs>(orgClient_GetALLCompanyViewCompleted);
            orgClient.GetAllDepartmentViewCompleted += new EventHandler<GetAllDepartmentViewCompletedEventArgs>(orgClient_GetAllDepartmentViewCompleted);
            orgClient.GetAllPostViewCompleted += new EventHandler<GetAllPostViewCompletedEventArgs>(orgClient_GetAllPostViewCompleted);
            BindTree();
        }
        #region 树形控件的操作
        //绑定树
        private void BindTree()
        {

            if (Application.Current.Resources.Contains("ORGTREESYSCompanyInfo"))
            {
                // allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
                BindCompany();
            }
            else
            {
                //orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                orgClient.GetALLCompanyViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }

        }

        void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> entTemps = e.Result;
                allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
                allCompanys.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allCompanys.Add(item);
                });

                UICache.CreateCache("ORGTREESYSCompanyInfo", allCompanys);
                UICache.CreateCache("SYS_CompanyInfo", allCompanys);
                orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> entTemps = e.Result;
                allDepartments = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
                allDepartments.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allDepartments.Add(item);
                });

                UICache.CreateCache("ORGTREESYSDepartmentInfo", allDepartments);
                UICache.CreateCache("SYS_DepartmentInfo", allDepartments);

                BindCompany();

                orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

            }
        }

        void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        {

        }

        void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    allPositions = e.Result.ToList();
                }
                UICache.CreateCache("ORGTREESYSPostInfo", allPositions);
                UICache.CreateCache("SYS_PostInfo", allDepartments);
                BindPosition();
            }
        }
        /// <summary>
        /// 获取正常的公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void orgClient_GetALLCompanyViewCompleted(object sender, GetALLCompanyViewCompletedEventArgs e)
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

                UICache.CreateCache("ORGTREESYSCompanyInfo", allCompanys);

                orgClient.GetAllDepartmentViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                loadbar.Stop();

            }
        }
        /// <summary>
        /// 获取正常可用的部门
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orgClient_GetAllDepartmentViewCompleted(object sender, GetAllDepartmentViewCompletedEventArgs e)
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

                orgClient.GetAllPostViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                loadbar.Stop();

            }
        }

        /// <summary>
        /// 获取岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orgClient_GetAllPostViewCompleted(object sender, GetAllPostViewCompletedEventArgs e)
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

                            pt.T_HR_DEPARTMENT = allDepartments.Where(s => s.DEPARTMENTID == ent.DEPARTMENTID).FirstOrDefault();
                            entlist.Add(pt);
                        }
                        UICache.CreateCache("ORGTREESYSPostInfo", entlist);
                    }
                    //  BindPosition();
                }
                else
                {
                    loadbar.Stop();

                }
            }
            catch (Exception ex)
            {
            }

        }
        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

            allDepartments = Application.Current.Resources["ORGTREESYSDepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

            if (allCompanys == null)
            {
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

            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> TopCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();

            foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
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
                        //item.Header = tmpOrg.CNAME;
                        item.Header = tmpOrg.BRIEFNAME;
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
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany = (from ent in allCompanys
                                                                              where ent.FATHERTYPE == "0"
                                                                              && ent.FATHERID == topComp.COMPANYID
                                                                              select ent).ToList();

                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment = (from ent in allDepartments
                                                                                    where ent.FATHERID == topComp.COMPANYID && ent.FATHERTYPE == "0"
                                                                                    select ent).ToList();

                AddOrgNode(lsCompany, lsDepartment, parentItem);
            }
            //allPositions = Application.Current.Resources["ORGTREESYSPostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            //if (allPositions != null)
            //{
            //    BindPosition();
            //}
        }

        private void AddOrgNode(List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany, List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
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
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
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
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
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
                foreach (SMT.Saas.Tools.OrganizationWS.T_HR_POST tmpPosition in allPositions)
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
                //if (treeItem.Items.Count > 0)
                //{
                //    loadbar.Stop();
                //    return;
                //}
                List<T_HR_POST> Positions = Application.Current.Resources["ORGTREESYSPostInfo"] as List<T_HR_POST>;
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
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        if (tmpOrg != null)
                        {
                            if (tmpOrg.COMPANYID == parentID)
                                return item;
                        }
                        break;
                    case OrgTreeItemTypes.Department:
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmpDep = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
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


        #endregion

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "Company":
                        T_HR_COMPANY company = selectedItem.DataContext as T_HR_COMPANY;
                        sType = "Company";
                        sValue = company.COMPANYID;
                        sName = company.CNAME;
                        break;
                    case "Department":
                        T_HR_DEPARTMENT department = selectedItem.DataContext as T_HR_DEPARTMENT;
                        sType = "Department";
                        sValue = department.DEPARTMENTID;
                        sName = department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        if (depIDsCach.ContainsKey(sValue)) return;
                        BindPosition(sValue, selectedItem);
                        break;
                    case "Post":
                        T_HR_POST post = selectedItem.DataContext as T_HR_POST;
                        sType = "Post";
                        sValue = post.POSTID;
                        sName = post.T_HR_POSTDICTIONARY.POSTNAME;
                        break;
                }
                if (this.SelectedClick != null)
                {
                    this.SelectedClick(this, null);
                }
            }
        }
    }
}
