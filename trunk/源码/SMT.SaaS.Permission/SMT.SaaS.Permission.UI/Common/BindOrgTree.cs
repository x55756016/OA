using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using SMT.Saas.Tools.OrganizationWS;
using System.Collections.Generic;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.Permission.UI
{
    public class BindOrgTree
    {
        OrganizationServiceClient orgClient;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        public List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;
        public event EventHandler BindOrgTreeCompleted;
        public TreeView treeOrganization = new TreeView();
        public Dictionary<string, string> depIDsCach = new Dictionary<string, string>();
        public BindOrgTree()
        {
            orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);

        }

        public void BeginBind()
        {
            try
            {
                BindTree();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region 树形控件的操作
        //绑定树
        private void BindTree()
        {
            if (Application.Current.Resources.Contains("SYS_CompanyInfo"))
            {
                BindCompany();
            }
            else
            {
                orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }

        }

        void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //loadbar.Stop();//有错误停止转圈                
                SMT.SAAS.Application.ExceptionManager.SendException("用户管理", "Views/SysUserManagement--GetCompanyActived");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
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

                UICache.CreateCache("SYS_CompanyInfo", allCompanys);
                orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {

                //loadbar.Stop();//有错误停止转圈                
                SMT.SAAS.Application.ExceptionManager.SendException("用户管理", "Views/SysUserManagement--GetDepartmentActived");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
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

                UICache.CreateCache("SYS_DepartmentInfo", allDepartments);

                BindCompany();
            }
        }

        //void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        //{

        //}

        void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //loadbar.Stop();//有错误停止转圈                
                SMT.SAAS.Application.ExceptionManager.SendException("用户管理", "Views/SysUserManagement--GetDepartmentActived");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            else
            {
                if (e.Result != null)
                {
                    allPositions = e.Result.ToList();
                }
                UICache.CreateCache("SYS_PostInfo", allPositions);
                //BindPosition();
            }
        }

        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            allCompanys = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

            allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

            if (allCompanys == null)
            {
                return;
            }
            if (allDepartments == null)
            {
                return;
            }
            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> TopCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();

            foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
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
                    if (allCompanys != null && allDepartments != null)
                    {
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
            allPositions = Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            if (allPositions == null)
            {
                orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        private void AddOrgNode(List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany, List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
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
        /// 根据部门绑定岗位
        /// </summary>
        /// <param name="departmentID">部门ID</param>
        public void BindPost(string departmentID)
        {
            try
            {
               allPositions= Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
                allPositions = allPositions.Where(p => p.T_HR_DEPARTMENT != null).Where(s => s.T_HR_DEPARTMENT.DEPARTMENTID == departmentID).ToList();
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
                    {
                        continue;
                    }

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
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 绑定岗位
        /// </summary>
        public void BindPosition()
        {
            //loadbar.Stop();
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
            if (BindOrgTreeCompleted != null)
            {
                BingdOrgTreeArg bindArg = new BingdOrgTreeArg();
                bindArg.treeOrganization = treeOrganization;
                this.BindOrgTreeCompleted(this, bindArg);
            }
            //树全部展开
            //  treeOrganization.ExpandAll();
            //if (treeOrganization.Items.Count > 0)
            //{
            //    TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
            //    selectedItem.IsSelected = true;
            //}
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
    }

    public class BingdOrgTreeArg : EventArgs
    {
        public TreeView treeOrganization;
    }
}
