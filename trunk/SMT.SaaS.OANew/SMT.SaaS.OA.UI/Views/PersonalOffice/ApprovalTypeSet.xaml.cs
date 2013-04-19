using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.PersonalOffice
{
    public partial class ApprovalTypeSet : BasePage
    {
        SMTLoading loadbar = new SMTLoading();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient client;
        private PermissionServiceClient permclient;// = new PermissionServiceClient();
        OrganizationServiceClient orgClient;
        public SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE SelectedEmployee { get; set; }
        private SmtOAPersonOfficeClient OfficeClient;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;
        private List<T_SYS_DICTIONARY> ListDicts = new List<T_SYS_DICTIONARY>();
        private List<T_SYS_DICTIONARY> ListSelectedDicts = new List<T_SYS_DICTIONARY>();

        private ObservableCollection<string> lsCompanys = new ObservableCollection<string>();//公司ID集合
        private ObservableCollection<string> lsDepartment = new ObservableCollection<string>();//部门ID集合
        private ObservableCollection<T_OA_APPROVALTYPESET> lsSet = new ObservableCollection<T_OA_APPROVALTYPESET>();
        public ApprovalTypeSet()
        {
            InitializeComponent();
            this.Loaded+=new RoutedEventHandler(ApprovalTypeSet_Loaded);
        }
        private void InitPara()
        {
            try
            {
                PARENT.Children.Add(loadbar);

                SaveBtn.IsEnabled = false;
                SelectedList.DisplayMemberPath = "DICTIONARYNAME";
                
                orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
                OfficeClient = new SmtOAPersonOfficeClient();
                orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                //OfficeClient.GetApprovalSetByOrgType
                OfficeClient.GetApprovalSetByOrgTypeCompleted += new EventHandler<GetApprovalSetByOrgTypeCompletedEventArgs>(OfficeClient_GetApprovalSetByOrgTypeCompleted);
                //this.Loaded += new RoutedEventHandler(ApprovalTypeSet_Loaded);
                OfficeClient.AddApprovalSetCompleted += new EventHandler<AddApprovalSetCompletedEventArgs>(OfficeClient_AddApprovalSetCompleted);
                InitData();
                
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
        }

        void OfficeClient_AddApprovalSetCompleted(object sender, AddApprovalSetCompletedEventArgs e)
        {
            loadbar.Stop();
            SaveBtn.IsEnabled = true;

            lsSet = new ObservableCollection<T_OA_APPROVALTYPESET>();
            lsCompanys = new ObservableCollection<string>();
            lsDepartment = new ObservableCollection<string>();
            try
            {
                if (e.Result != "")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), "设置成功",Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    
                    
                }

            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            InitData();
            
        }

        void OfficeClient_GetApprovalSetByOrgTypeCompleted(object sender, GetApprovalSetByOrgTypeCompletedEventArgs e)
        {
            loadbar.Stop();
            this.SaveBtn.IsEnabled = false;
            if (e.Result != null)
            {
                SelectedList.Items.Clear();
                lsSet = e.Result;
                e.Result.ToList().ForEach(
                    item => {
                        var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                                   where a.DICTIONCATEGORY == "TYPEAPPROVAL" && a.DICTIONARYVALUE == System.Convert.ToInt32(item.TYPEAPPROVAL)
                                   select a;
                        if (ents.Count() > 0)
                        {
                            SelectedList.Items.Add(ents.FirstOrDefault());
                        }
                    }
                    );
                //SelectedList.ItemsSource = e.Result;
                SelectedList.DisplayMemberPath = "DICTIONARYNAME";
            }
        }

        void ApprovalTypeSet_Loaded(object sender, RoutedEventArgs e)
        {
            InitPara();
            //InitData();
        }
        private void InitData()
        {
            BindTree();
            BindApprovalTree();
            LoadAppSet();
        }
        private void LoadAppSet()
        {
            string filter = "";    //查询过滤条件
            int pageCount = 0;
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            
            string sType = "", sValue = "";
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "Company":
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        sType = "Company";
                        sValue = company.COMPANYID;
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "ORGANIZATIONID ==@" + paras.Count().ToString();//类型名称
                        paras.Add(sValue);
                        break;
                    case "Department":
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        sType = "Department";
                        sValue = department.DEPARTMENTID;
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "ORGANIZATIONID ==@" + paras.Count().ToString();//类型名称
                        paras.Add(sValue);
                        break;

                }
                
            }
            
            if (!(string.IsNullOrEmpty(sType)))
            {
                sType = sType == "Company" ? "0" : "1";//0是公司 1 是部门
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "ORGANIZATIONTYPE ==@" + paras.Count().ToString();//类型名称
                paras.Add(sType);
            }
            var entsets = from ent in lsSet
                          where ent.ORGANIZATIONID == sValue && ent.ORGANIZATIONTYPE == sType
                          select ent;
            if (entsets.Count() > 0)
            {
                List<string> appids = new List<string>();
                entsets.ToList().ForEach(
                    item =>
                    {
                        appids.Add(item.TYPEAPPROVAL);
                    }
                    );
                var dicts = from ent in ListDicts
                            where appids.Contains(ent.DICTIONARYVALUE.ToString())
                            select ent;
                SelectedList.Items.Clear();
                dicts.ToList().ForEach(
                    item =>
                    {
                        SelectedList.Items.Add(item);
                    }
                    );
                //SelectedList.ItemsSource = dicts.ToList();
                SelectedList.DisplayMemberPath = "DICTIONARYNAME";
            }
            else
            {
                SelectedList.Items.Clear();
                loadbar.Start();
                lsSet = new ObservableCollection<T_OA_APPROVALTYPESET>();
                OfficeClient.GetApprovalSetByOrgTypeAsync(sValue, sType);
            }
            
        }

        #region 树形事项审批类型操作
        private void BindApprovalTree()
        {
            if (Application.Current.Resources.Contains("SYS_DICTIONARY"))
            {

                BindApproval();
            }
        }

        private void BindApproval()
        {
            treeApproval.Items.Clear();
            List<T_SYS_DICTIONARY> Dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;

            

            if (ListDicts == null)
            {
                return;
            }

            List<T_SYS_DICTIONARY> TopApproval = new List<T_SYS_DICTIONARY>();
            
            var ents = from p in Dicts
                       where p.DICTIONCATEGORY == "TYPEAPPROVAL"
                       orderby p.ORDERNUMBER
                       select p;
            ListDicts = ents.Count() > 0 ? ents.ToList() : null;
            if (ListDicts != null)
            {

                foreach (T_SYS_DICTIONARY dict in ListDicts)
                {
                    if (dict.T_SYS_DICTIONARY2 ==null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = dict.DICTIONARYNAME;
                        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        item.DataContext = dict;
                        item.Tag = dict;
                        
                        //标记为公司
                        //item.Tag = OrgTreeItemTypes.Company;
                        treeApproval.Items.Add(item);
                        TopApproval.Add(dict);
                    }
                }
                if (TopApproval.Count() > 0)
                {
                    foreach (var topApp in TopApproval)
                    {
                        //存在子记录的则添加子节点
                        List<T_SYS_DICTIONARY> lstDict = new List<T_SYS_DICTIONARY>();
                        var lsents = from ent in ListDicts
                                        where ent.T_SYS_DICTIONARY2 != null && ent.T_SYS_DICTIONARY2.DICTIONARYID == topApp.DICTIONARYID
                                        select ent;
                        if (lsents.Count() > 0)
                        {
                            lstDict = lsents.ToList();
                            TreeViewItem parentItem = GetApprovalParentItem(topApp.DICTIONARYID);
                            AddApprovalNode(lstDict, parentItem);
                        }
                            
                            
                    }
                    
                }
                    
               
            }
            
            
        }

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        private TreeViewItem GetApprovalParentItem(string parentID)
        {
            TreeViewItem tmpItem = null;
            foreach (TreeViewItem item in treeApproval.Items)
            {
                tmpItem = GetApprovalParentItemFromChild(item, parentID);
                if (tmpItem != null)
                {
                    break;
                }
            }
            return tmpItem;
        }

        private TreeViewItem GetApprovalParentItemFromChild(TreeViewItem item,  string parentID)
        {
            TreeViewItem tmpItem = null;
            T_SYS_DICTIONARY tmpDict = item.DataContext as T_SYS_DICTIONARY;
            if (tmpDict != null)
            {
                if (tmpDict.DICTIONARYID == parentID)
                    return item;
            }
            
            if (item.Items != null && item.Items.Count > 0)
            {
                foreach (TreeViewItem childitem in item.Items)
                {
                    tmpItem = GetApprovalParentItemFromChild(childitem, parentID);
                    if (tmpItem != null)
                    {
                        break;
                    }
                }
            }
            return tmpItem;
        }

        private void AddApprovalNode(List<T_SYS_DICTIONARY> lsDict, TreeViewItem FatherNode)
        {
            //绑定事项审批的子项目
            foreach (var childDict in lsDict)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childDict.DICTIONARYNAME;
                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                item.DataContext = childDict;
                item.Tag = childDict;

                FatherNode.Items.Add(item);

                if (lsDict.Count() > 0)
                {
                    List<T_SYS_DICTIONARY> lsTempDict = (from ent in lsDict
                                                        where ent.T_SYS_DICTIONARY2.DICTIONARYID == childDict.DICTIONARYID
                                                        select ent).ToList();

                    List<T_SYS_DICTIONARY> lstDict2 = new List<T_SYS_DICTIONARY>();
                    var lsents = from ent in ListDicts
                                 where ent.T_SYS_DICTIONARY2 != null && ent.T_SYS_DICTIONARY2.DICTIONARYID == childDict.DICTIONARYID
                                 select ent;
                    if (lsents.Count() > 0)
                    {
                        lstDict2 = lsents.ToList();
                        TreeViewItem parentItem = GetApprovalParentItem(childDict.DICTIONARYID);
                        AddApprovalNode(lstDict2, parentItem);
                    }

                    AddApprovalNode(lsTempDict, item);
                }
            }
            
        }
        
        #endregion

        #region 树形控件的操作
        //绑定树
        private void BindTree()
        {

            if (Application.Current.Resources.Contains("ORGTREESYSCompanyInfo"))
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
                orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        {
            loadbar.Stop();
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

                BindCompany();

                

            }
        }

        void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        {

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
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany = (from ent in allCompanys
                                                                              where ent.FATHERTYPE == "0"
                                                                              && ent.FATHERID == topComp.COMPANYID
                                                                              select ent).ToList();

                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment = (from ent in allDepartments
                                                                                    where ent.FATHERID == topComp.COMPANYID && ent.FATHERTYPE == "0"
                                                                                    select ent).ToList();

                AddOrgNode(lsCompany, lsDepartment, parentItem);
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
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
        #region 保存按钮
        
        
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveApprovalSet();
        }

        private void SaveApprovalSet()
        {
            if (lsSet.Count() == 0 && lsCompanys.Count() == 0 && lsDepartment.Count() == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请选择事项审批",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            SaveBtn.IsEnabled = false;
            OfficeClient.AddApprovalSetAsync(lsSet, lsCompanys, lsDepartment);
            

            //else
            //{
            //if (lsSet.Count() > 0 && (lsCompanys.Count() > 0 || lsDepartment.Count() > 0))
            //if (lsCompanys.Count() > 0 || lsDepartment.Count() > 0)
            //{
            //SaveBtn.IsEnabled = false;
            //OfficeClient.AddApprovalSetAsync(lsSet, lsCompanys, lsDepartment);
            //}
            //else
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请选择相应的组织架构",
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return;
            //}
            //}

        }
        #endregion

        #region 组织架构改变选择事件


        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            LoadAppSet();
           
        }
        #endregion

        #region 添加事项审批
        
        
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //lsSet = new ObservableCollection<T_OA_APPROVALTYPESET>();
            SaveBtn.IsEnabled = true;
            string sType="",sValue="";
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "Company":
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        sType = "Company";
                        sValue = company.COMPANYID;
                        
                        break;
                    case "Department":
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        sType = "Department";
                        sValue = department.DEPARTMENTID;
                        
                        break;

                }
                string StrType = "";
                StrType = sType == "Company" ? "0" : "1";
                if (treeApproval.SelectedItem != null)
                {
                    T_SYS_DICTIONARY selectdic = new T_SYS_DICTIONARY();
                    TreeViewItem selectedItemapp = treeApproval.SelectedItem as TreeViewItem;
                    selectdic = selectedItemapp.Tag as T_SYS_DICTIONARY;
                    if(selectdic == null)
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "该字典信息为空",
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    if (SelectedList.Items.Count() > 0)
                    {
                        if (!(SelectedList.Items.Contains(selectdic)))
                        {
                            SelectedList.Items.Add(selectdic);
                            var ents = from ent in lsSet
                                       where ent.ORGANIZATIONID == sValue && ent.ORGANIZATIONTYPE == StrType && ent.TYPEAPPROVAL == selectdic.DICTIONARYVALUE.ToString()
                                       select ent;
                            if (ents.Count() == 0)
                            {
                                T_OA_APPROVALTYPESET set = new T_OA_APPROVALTYPESET();
                                set.TYPESETID = System.Guid.NewGuid().ToString();
                                set.TYPEAPPROVAL = selectdic.DICTIONARYVALUE.ToString();
                                set.ORGANIZATIONID = sValue;
                                set.ORGANIZATIONTYPE = StrType;
                                set.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                                set.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                                set.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                                set.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                                set.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                                set.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                                set.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                                set.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                                set.CREATEDATE = System.DateTime.Now;
                                lsSet.Add(set);
                            }
                            
                        }
                    }
                    else
                    {
                        SelectedList.Items.Add(selectdic);
                        var ents2= from ent in lsSet
                                   where ent.ORGANIZATIONID == sValue && ent.ORGANIZATIONTYPE == StrType && ent.TYPEAPPROVAL == selectdic.DICTIONARYVALUE.ToString()
                                   select ent;
                        if (ents2.Count() == 0)
                        {
                            T_OA_APPROVALTYPESET set = new T_OA_APPROVALTYPESET();
                            set.TYPESETID = System.Guid.NewGuid().ToString();
                            set.TYPEAPPROVAL = selectdic.DICTIONARYVALUE.ToString();
                            set.ORGANIZATIONID = sValue;
                            set.ORGANIZATIONTYPE = StrType;
                            set.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            set.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            set.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                            set.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                            set.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            set.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            set.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            set.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            set.CREATEDATE = System.DateTime.Now;
                            lsSet.Add(set);
                        }
                    }

                    if (StrType == "0")
                    {
                        if (lsCompanys.Count() > 0)
                        {
                            if (lsCompanys.IndexOf(sValue) > 0)
                            {
                                lsCompanys.Add(sValue);
                            }
                        }
                        else
                        {
                            lsCompanys.Add(sValue);
                        }
                    }
                    else
                    {
                        if (lsDepartment.Count() > 0)
                        {
                            if (lsDepartment.IndexOf(sValue) > 0)
                            {
                                lsDepartment.Add(sValue);
                            }
                        }
                        else
                        {
                            lsDepartment.Add(sValue);
                        }
                    }

                    //树形事项则去掉该数据
                }
                else
                {
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请选择小的事项审批信息",
                    //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    Utility.ShowCustomMessage(MessageTypes.Message, "警告", "请选择小的事项审批信息");
                }
                
                
            }
            else
            {
                //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请选择组织架构",
                //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                Utility.ShowCustomMessage(MessageTypes.Message, "警告", "请选择组织架构");
            }
            
            
            
            SelectedList.DisplayMemberPath = "DICTIONARYNAME";

        }
        #endregion

        #region 移除事项审批


        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {

            //if (SelectedList.SelectedItems.Count > 0)
            //{
            SaveBtn.IsEnabled = true;
            T_SYS_DICTIONARY selectdict = new T_SYS_DICTIONARY();
            selectdict = SelectedList.SelectedItem as T_SYS_DICTIONARY;
            if (selectdict == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, "警告", "请选择要移除的事项");
                return;
            }

            SelectedList.Items.Remove(selectdict);
            //当所有的记录被清空时  所选组织架构也清掉
            //if (SelectedList.Items.Count > 0)
            //{
            string sType = "", sValue = "";
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "Company":
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company =
                            selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        sType = "Company";
                        sValue = company.COMPANYID;

                        break;
                    case "Department":
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department =
                            selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        sType = "Department";
                        sValue = department.DEPARTMENTID;

                        break;

                }
                string StrType = "";
                StrType = sType == "Company" ? "0" : "1";

                var allents = from ent in lsSet
                              where
                                  ent.ORGANIZATIONID == sValue && ent.ORGANIZATIONTYPE == StrType &&
                                  ent.TYPEAPPROVAL == selectdict.DICTIONARYVALUE.ToString()
                              select ent;
                //删除已经存在的记录
                if (allents.Count() > 0)
                {
                    allents.ToList().ForEach(
                        item =>
                            {
                                lsSet.Remove(item);
                            });
                }

                if (StrType == "0")
                {
                    if (lsCompanys.Count() > 0)
                    {
                        if (lsCompanys.IndexOf(sValue) > 0)
                        {
                            lsCompanys.Add(sValue);
                        }
                    }
                    else
                    {
                        lsCompanys.Add(sValue);
                    }
                }
                else
                {
                    if (lsDepartment.Count() > 0)
                    {
                        if (lsDepartment.IndexOf(sValue) > 0)
                        {
                            lsDepartment.Add(sValue);
                        }
                    }
                    else
                    {
                        lsDepartment.Add(sValue);
                    }
                }
            }
            //}

            //}
            //else
            //{
            //Utility.ShowCustomMessage(MessageTypes.Caution, "CAUTION", "请选择公司");
            //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请选择类型",
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

            //if (StrType == "0")
            //{
            //    if (lsCompanys.Count() > 0)
            //    {
            //        if (lsCompanys.IndexOf(sValue) > 0)
            //        {
            //            lsCompanys.Add(sValue);
            //        }
            //    }
            //    else
            //    {
            //        lsCompanys.Add(sValue);
            //    }
            //}
            //else
            //{
            //    if (lsDepartment.Count() > 0)
            //    {
            //        if (lsDepartment.IndexOf(sValue) > 0)
            //        {
            //            lsDepartment.Add(sValue);
            //        }
            //    }
            //    else
            //    {
            //        lsDepartment.Add(sValue);
            //    }
            //}
            //}

            SelectedList.DisplayMemberPath = "DICTIONARYNAME";
        }

        #endregion

        private void treeApproval_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
        #region 移除所有选择的事项类型
        
        
        private void buttonDel_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedList.Items.Count == 0 )
            {
                Utility.ShowCustomMessage(MessageTypes.Message, "警告", "没有要移除的事项");
                return;
            }
            SaveBtn.IsEnabled = true;
            SelectedList.Items.Clear();
            string sType = "", sValue = "";
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "Company":
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        sType = "Company";
                        sValue = company.COMPANYID;

                        break;
                    case "Department":
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        sType = "Department";
                        sValue = department.DEPARTMENTID;

                        break;

                }
                string StrType = "";
                StrType = sType == "Company" ? "0" : "1";
                SelectedList.Items.Clear();
                var allents = from ent in lsSet
                              where ent.ORGANIZATIONID == sValue && ent.ORGANIZATIONTYPE == StrType
                              select ent;
                //删除已经存在的记录
                if (allents.Count() > 0)
                {
                    allents.ToList().ForEach(
                        item =>
                        {
                            lsSet.Remove(item);
                        });
                }
                if (ListDicts == null)
                    return;
                if (ListDicts.Count() == 0)
                {
                    return;
                }
                


                if (StrType == "0")
                {
                    lsCompanys.Add(sValue);
                    //if (lsCompanys.Count() > 0)
                    //{
                    //    if (lsCompanys.IndexOf(sValue) > 0)
                    //    {
                    //        lsCompanys.Remove(sValue);
                    //    }
                    //}
                    //else
                    //{
                    //    lsCompanys.Remove(sValue);
                    //}
                }
                else
                {
                    lsDepartment.Add(sValue);
                    //if (lsDepartment.Count() > 0)
                    //{
                    //    if (lsDepartment.IndexOf(sValue) > 0)
                    //    {
                    //        lsDepartment.Remove(sValue);
                    //    }
                    //}
                    //else
                    //{
                    //    lsDepartment.Remove(sValue);
                    //}
                }

                //树形事项则去掉该数据



            }
            else
            {
                //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请选择组织架构",
                //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                Utility.ShowCustomMessage(MessageTypes.Message, "警告", "请选择组织架构");
            }



            SelectedList.DisplayMemberPath = "DICTIONARYNAME";
        }
        #endregion

        #region 添加所有的子事项审批
               
        private void buttonAll_Click(object sender, RoutedEventArgs e)
        {
            lsSet = new ObservableCollection<T_OA_APPROVALTYPESET>();
            SaveBtn.IsEnabled = true;
            string sType = "", sValue = "";
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "Company":
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        sType = "Company";
                        sValue = company.COMPANYID;

                        break;
                    case "Department":
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        sType = "Department";
                        sValue = department.DEPARTMENTID;

                        break;

                }
                string StrType = "";
                StrType = sType == "Company" ? "0" : "1";
                SelectedList.Items.Clear();
                var allents = from ent in lsSet
                           where ent.ORGANIZATIONID == sValue && ent.ORGANIZATIONTYPE == StrType 
                           select ent;
                //删除已经存在的记录
                if (allents.Count() > 0)
                {
                    allents.ToList().ForEach(
                        item => {
                            lsSet.Remove(item);
                        });
                }
                if (ListDicts == null)
                    return;
                if (ListDicts.Count() == 0)
                {
                    return;
                }
                else
                {
                    ListDicts.ForEach(
                        item => {
                            if (!(SelectedList.Items.Contains(item)) )
                            {
                                SelectedList.Items.Add(item);
                                var ents = from ent in lsSet
                                           where ent.ORGANIZATIONID == sValue && ent.ORGANIZATIONTYPE == StrType && ent.TYPEAPPROVAL == item.DICTIONARYVALUE.ToString()
                                           select ent;
                                if (ents.Count() == 0)
                                {
                                    T_OA_APPROVALTYPESET set = new T_OA_APPROVALTYPESET();
                                    set.TYPESETID = System.Guid.NewGuid().ToString();
                                    set.TYPEAPPROVAL = item.DICTIONARYVALUE.ToString();
                                    set.ORGANIZATIONID = sValue;
                                    set.ORGANIZATIONTYPE = StrType;
                                    set.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                                    set.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                                    set.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                                    set.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                                    set.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                                    set.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                                    set.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                                    set.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                                    set.CREATEDATE = System.DateTime.Now;
                                    lsSet.Add(set);
                                }

                            }
                        }
                        );
                }
                
                    
                if (StrType == "0")
                {
                    if (lsCompanys.Count() > 0)
                    {
                        if (lsCompanys.IndexOf(sValue) > 0)
                        {
                            lsCompanys.Add(sValue);
                        }
                    }
                    else
                    {
                        lsCompanys.Add(sValue);
                    }
                }
                else
                {
                    if (lsDepartment.Count() > 0)
                    {
                        if (lsDepartment.IndexOf(sValue) > 0)
                        {
                            lsDepartment.Add(sValue);
                        }
                    }
                    else
                    {
                        lsDepartment.Add(sValue);
                    }
                }

                    //树形事项则去掉该数据
            }
            else
            {
                //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请选择组织架构",
                //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                Utility.ShowCustomMessage(MessageTypes.Message, "警告", "请选择组织架构");
            }

            SelectedList.DisplayMemberPath = "DICTIONARYNAME";
        }
        #endregion

    }
}
