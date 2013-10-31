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
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.Permission.UI.Form;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.Helper;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;

namespace SMT.SaaS.Permission.UI.UserControls
{
    /// <summary>
    /// 角色自定义菜单权限
    /// </summary>
    public partial class RoleCustomMenuPermission : UserControl, IEntityEditor
    {
        #region 初始化参数
        //基础变量
        public FormTypes FormType { get; set; }
        string RoleID = string.Empty;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private string strResMsg = string.Empty;
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭

        private static PermissionServiceClient client = new PermissionServiceClient();//龙康才新增
        List<CustomerPermission> ListCustomerpermission = new List<CustomerPermission>(); //自定义权限
        List<CustomerPermission> SelectingCustomerpermission = new List<CustomerPermission>(); //刚选中的功能项

        #region 功能项,权限 变量
        private List<T_SYS_ENTITYMENU> allMenu; //菜单集合
        DataTemplate treeViewItemTemplate = null;

        private bool MenuIsCheck = false; //功能项是否有选中
        private bool PermissionIsCheck = false;//权限项是否有选中
        //权限值容器  用来装载选中的权限值
        ObservableCollection<PermissionValue> ListPermValue = new ObservableCollection<PermissionValue>();
        #endregion

        #region 组织架构变量
        Style treeViewItemStyle = null;
        #endregion

        #endregion

        #region 构造函数
        
        public RoleCustomMenuPermission(FormTypes formtype, string strRoleId)
        {
            InitializeComponent();
            FormType = formtype;
            RoleID = strRoleId;
            InitEvent();
            InitParas();
        }
        #endregion        

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("ROLECUSTOMMENUPERMISSION");
        }

        public string GetStatus()
        {
            string strTemp = string.Empty;
            if (this.DataContext != null)
            {
                strTemp = Utility.GetResourceStr("EDIT");
            }

            return strTemp;
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
            }
        }

        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        #region 私有方法
        /// <summary>
        /// 注册事件
        /// </summary>
        private void InitEvent()
        {
            client.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(client_GetSysDictionaryByCategoryCompleted);
            client.GetSysMenuByTypeCompleted += new EventHandler<GetSysMenuByTypeCompletedEventArgs>(client_GetSysMenuByTypeCompleted);
            client.GetSysPermissionAllCompleted += new EventHandler<GetSysPermissionAllCompletedEventArgs>(client_GetSysPermissionAllCompleted);
                      

            client.GetCutomterPermissionObjCompleted += new EventHandler<GetCutomterPermissionObjCompletedEventArgs>(client_GetCutomterPermissionObjCompleted);
            client.SetCutomterPermissionObjCompleted += new EventHandler<SetCutomterPermissionObjCompletedEventArgs>(client_SetCutomterPermissionObjCompleted);
                        
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitParas()
        {
            treeViewItemTemplate = this.Resources["NodeTemplate"] as DataTemplate;
            
            //绑定系统类型
            client.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
            client.GetSysPermissionAllAsync();
            SetToolBar();
        }        

        /// <summary>
        /// 显示按钮
        /// </summary>
        private void SetToolBar()
        {
            ToolbarItems = Utility.CreateFormSaveButton();
            RefreshUI(RefreshedTypes.All);
        }        

        /// <summary>
        /// 加载功能项的树控件
        /// </summary>
        private void BindTree()
        {
            var menulist = from m in allMenu
                           where m.T_SYS_ENTITYMENU2 == null
                           select m;

            foreach (T_SYS_ENTITYMENU menu in menulist)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Style= Application.Current.Resources["TreeViewItemStyle"] as Style;
                childItem.Header = menu.MENUNAME;
                childItem.Tag = menu.ENTITYMENUID;
                childItem.HeaderTemplate = treeViewItemTemplate;
                childItem.IsExpanded = true;
                childItem.DataContext = menu;

                AddChildItems(childItem, menu);

                treeMenu.Items.Add(childItem);
            }
        }

        /// <summary>
        /// 增加功能项的树添加节点
        /// </summary>
        /// <param name="treeitem"></param>
        /// <param name="menu"></param>
        private void AddChildItems(TreeViewItem treeitem, T_SYS_ENTITYMENU menu)
        {
            var menulist = from m in allMenu
                           where m.T_SYS_ENTITYMENU2 != null && m.T_SYS_ENTITYMENU2.ENTITYMENUID == menu.ENTITYMENUID
                           select m;

            foreach (var tmpMenu in menulist)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                childItem.Header = tmpMenu.MENUNAME;
                childItem.Tag = menu.ENTITYMENUID;
                childItem.HeaderTemplate = treeViewItemTemplate;
                childItem.IsExpanded = true;
                childItem.DataContext = tmpMenu;

                AddChildItems(childItem, tmpMenu);

                treeitem.Items.Add(childItem);
            }
        }        

        /// <summary>
        /// 添加功能项
        /// </summary>
        /// <param name="CustomerP"></param>
        /// <param name="Menu"></param>
        private void AddListCustomerPermission(CustomerPermission CustomerP, T_SYS_ENTITYMENU Menu)
        {
            CustomerP.EntityMenuId = Menu.ENTITYMENUID;
            if (SelectingCustomerpermission.Count() > 0)
            {
                var listperm = from ent in SelectingCustomerpermission
                               where ent.EntityMenuId == Menu.ENTITYMENUID
                               select ent;
                if (!(listperm.Count() > 0))
                    SelectingCustomerpermission.Add(CustomerP);
            }
            else
            {
                SelectingCustomerpermission.Add(CustomerP);
            }
        }

        /// <summary>
        /// 清空权限的列表的选中值
        /// </summary>
        private void ClearPermissionDataGridCheckBox()
        {
            foreach (object obj in DtGrid.ItemsSource)
            {
                if (DtGrid.Columns[0].GetCellContent(obj) != null)
                {
                    CheckBox cb1 = DtGrid.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                    cb1.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// 移除已经存在的功能项
        /// </summary>
        /// <param name="Menu"></param>
        private void RemoveListCustomerPermission(T_SYS_ENTITYMENU Menu)
        {
            var menulist = from ent in ListCustomerpermission
                           where ent.EntityMenuId == Menu.ENTITYMENUID
                           select ent;

            if (menulist.Count() > 0)
                ListCustomerpermission.Remove(menulist.ToList().FirstOrDefault()); //去掉已经存
        }

        /// <summary>
        /// 根据功能项的CheckBox勾选，确定添加/删除已登记的功能项
        /// </summary>
        /// <param name="item"></param>
        /// <param name="selfCheckbox"></param>
        /// <param name="currentItem"></param>
        private void FindTreeViewItemByChkBox(TreeViewItem item, CheckBox selfCheckbox, ref TreeViewItem currentItem)
        {
            CustomerPermission CustomerP = new CustomerPermission();
            T_SYS_ENTITYMENU Menu = new T_SYS_ENTITYMENU();
            foreach (TreeViewItem childItem in item.Items)
            {
                TreeViewItem myItem =
                        (TreeViewItem)(item.ItemContainerGenerator.ContainerFromItem(childItem));
                CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                if (cbx == selfCheckbox)
                {
                    currentItem = myItem;
                    if (cbx.IsChecked == true)
                    {
                        Menu = myItem.DataContext as T_SYS_ENTITYMENU;
                        if (Menu.T_SYS_ENTITYMENU2 != null && Menu.HASSYSTEMMENU == "1")
                        {
                            AddListCustomerPermission(CustomerP, Menu);
                        }
                    }
                    else
                    {
                        Menu = myItem.DataContext as T_SYS_ENTITYMENU;
                        if (Menu.T_SYS_ENTITYMENU2 != null && Menu.HASSYSTEMMENU == "1")
                        {
                            RemoveListCustomerPermission(Menu);//清除已存在的功能项                            
                        }
                    }
                    return;
                }
                FindTreeViewItemByChkBox(childItem, selfCheckbox, ref currentItem);
            }
        }

        /// <summary>
        /// 勾销功能项的树父节点CheckBox
        /// </summary>
        /// <param name="item"></param>
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

        /// <summary>
        /// 勾销功能项的树子节点CheckBox
        /// </summary>
        /// <param name="item"></param>
        /// <param name="selfCheckbox"></param>
        private void UnCheckChildCheckbox(TreeViewItem item, CheckBox selfCheckbox)
        {
            T_SYS_ENTITYMENU Menu = new T_SYS_ENTITYMENU();

            foreach (TreeViewItem childItem in item.Items)
            {
                CustomerPermission CustomerP = new CustomerPermission();
                TreeViewItem myItem =
                (TreeViewItem)(item.ItemContainerGenerator.ContainerFromItem(childItem));

                CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                if (cbx == selfCheckbox)
                {
                    continue;
                }
                if (cbx != null)
                {
                    if (cbx.IsChecked == true)
                    {
                        cbx.IsChecked = false;
                        RemoveListCustomerPermission(Menu);//清除已存在的功能项
                    }
                    if (cbx.IsChecked == false)
                    {
                        cbx.IsChecked = true;
                        Menu = myItem.DataContext as T_SYS_ENTITYMENU;
                        if (Menu.T_SYS_ENTITYMENU2 != null && Menu.HASSYSTEMMENU == "1")
                        {
                            AddListCustomerPermission(CustomerP, Menu);
                        }
                    }
                }

                UnCheckChildCheckbox(childItem, selfCheckbox);
            }
        }

        /// <summary>
        /// 添加功能项对应的权限值  
        /// </summary>
        /// <param name="chkbox">单击的checkbox</param>
        private void PermissionGridSetPermissionByCheckBox(CheckBox chkbox)
        {
            T_SYS_PERMISSION TablePermission = new T_SYS_PERMISSION();
            TablePermission = chkbox.Tag as T_SYS_PERMISSION;
            PermissionValue PermValue = new PermissionValue();
            PermValue.Permission = TablePermission.PERMISSIONID;
            if (chkbox.IsChecked == true)
            {
                foreach (var ent in SelectingCustomerpermission)
                {
                    //ent.PermissionValue aa = new ObservableCollection<PermissionValue>();
                    var ents = from a in ListPermValue
                               where a.Permission == PermValue.Permission
                               select a;

                    if (ent.PermissionValue == null)
                    {
                        ListPermValue.Add(PermValue);
                    }
                    else
                    {
                        if (!(ents.Count() > 0))
                            ListPermValue.Add(PermValue);
                    }
                    if (ListPermValue.Count() > 0)
                        ent.PermissionValue = ListPermValue;
                    ListCustomerpermission.Add(ent);
                }
                SelectingCustomerpermission.Clear();//将上一次选中的功能项清空
            }
            else  //没选中  则删除 相对应的 权限
            {
                foreach (var ent in ListCustomerpermission)
                {
                    PermValue.Permission = TablePermission.PERMISSIONID;
                    var EntPermValue = from entPerm in ent.PermissionValue
                                       where entPerm.Permission == TablePermission.PERMISSIONID
                                       select entPerm;
                    if (EntPermValue.Count() > 0)
                        ent.PermissionValue.Remove(PermValue);
                }
            }
        }

        /// <summary>
        /// 设置组织架构树节点CheckBox勾选
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parent"></param>
        /// <param name="cb"></param>
        private void SetOrgTreeViewItemCheckState(TreeViewItem item, TreeViewItem parentitem, CheckBox cb)
        {
            if (cb == null)
            {
                return;
            }

            switch (cb.IsChecked)
            {
                case null:
                    UpdateUnselect(parentitem, item);
                    break;
                case true:
                    UpdateSelect(parentitem, item);
                    if (parentitem == null)
                    {
                        AddOrgObjectInList(item);
                    }
                    break;
                case false:
                    UpdateUnselect(parentitem, item);
                    break;
            }
        }

        /// <summary>
        /// 选中
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="item"></param>
        private void UpdateSelect(TreeViewItem parent, TreeViewItem item)
        {
            if (item.Header != null)
            {
                (UIHelper.FindChildControl<CheckBox>(item)).IsChecked = true;
                if (item.HasItems)
                {
                    foreach (TreeViewItem tvi in item.Items)
                    {
                        if (tvi == null)
                        {
                            continue;
                        }

                        if ((UIHelper.FindChildControl<CheckBox>(tvi)) == null)
                        {
                            continue;
                        }

                        (UIHelper.FindChildControl<CheckBox>(tvi)).IsChecked = true;
                        if (tvi.HasItems)
                        {
                            UpdateChildSelect(tvi);
                        }
                    }
                }
            }
            if (parent != null)
            {
                bool anyChildUnChecked = false;
                foreach (TreeViewItem tvi in parent.Items)
                {
                    if (tvi != item)
                    {
                        if (tvi == null)
                        {
                            continue;
                        }

                        if ((UIHelper.FindChildControl<CheckBox>(tvi)) == null)
                        {
                            continue;
                        }

                        if ((UIHelper.FindChildControl<CheckBox>(tvi)).IsChecked == false)
                        {
                            anyChildUnChecked = true;
                            break;
                        }
                    }
                }
                if (anyChildUnChecked)
                {
                    UpdateParentUnSelect(parent);
                }
                else
                {
                    (UIHelper.FindChildControl<CheckBox>(parent)).IsChecked = true;
                    UpdateSelect(parent.Parent as TreeViewItem, parent);
                }
            }
        }

        /// <summary>
        /// 选中子节点
        /// </summary>
        /// <param name="item"></param>
        private void UpdateChildSelect(TreeViewItem item)
        {
            if (item.Header != null)
            {
                (UIHelper.FindChildControl<CheckBox>(item)).IsChecked = true;
                if (item.HasItems)
                {
                    foreach (TreeViewItem tvi in item.Items)
                    {
                        if (tvi == null)
                        {
                            continue;
                        }

                        if ((UIHelper.FindChildControl<CheckBox>(tvi)) == null)
                        {
                            continue;
                        }

                        (UIHelper.FindChildControl<CheckBox>(tvi)).IsChecked = true;
                        if (tvi.HasItems)
                        {
                            UpdateChildSelect(tvi);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 勾销
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="item"></param>
        public void UpdateUnselect(TreeViewItem parent, TreeViewItem item)
        {
            if (item.Header != null)
            {
                (UIHelper.FindChildControl<CheckBox>(item)).IsChecked = false;
                if (item.HasItems)
                {
                    foreach (TreeViewItem tvi in item.Items)
                    {
                        if (tvi == null)
                        {
                            continue;
                        }

                        if ((UIHelper.FindChildControl<CheckBox>(tvi)) == null)
                        {
                            continue;
                        }

                        (UIHelper.FindChildControl<CheckBox>(tvi)).IsChecked = false;
                        if (tvi.HasItems)
                        {
                            UpdateChildUnselect(tvi);
                        }
                    }
                }
            }
            if (parent != null)
            {
                bool anyChildChecked = false;
                foreach (TreeViewItem tvi in parent.Items)
                {
                    if (tvi != item)
                    {
                        if (tvi == null)
                        {
                            continue;
                        }

                        if ((UIHelper.FindChildControl<CheckBox>(tvi)) == null)
                        {
                            continue;
                        }

                        if ((UIHelper.FindChildControl<CheckBox>(tvi)).IsChecked != false)
                        {
                            anyChildChecked = true;
                            break;
                        }
                    }
                }
                if (anyChildChecked)
                {
                    UpdateParentUnSelect(parent);
                }
                else
                {
                    (UIHelper.FindChildControl<CheckBox>(parent)).IsChecked = false;
                    UpdateUnselect(parent.Parent as TreeViewItem, parent);
                }
            }
        }

        /// <summary>
        /// 勾销子节点
        /// </summary>
        /// <param name="item"></param>
        private void UpdateChildUnselect(TreeViewItem item)
        {
            if (item.Header != null)
            {
                if (item.HasItems)
                {
                    (UIHelper.FindChildControl<CheckBox>(item)).IsChecked = false;
                    foreach (TreeViewItem tvi in item.Items)
                    {
                        if (tvi == null)
                        {
                            continue;
                        }

                        if ((UIHelper.FindChildControl<CheckBox>(tvi)) == null)
                        {
                            continue;
                        }

                        (UIHelper.FindChildControl<CheckBox>(tvi)).IsChecked = false;
                        if (tvi.HasItems)
                        {
                            UpdateChildUnselect(tvi);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 勾销父节点
        /// </summary>
        /// <param name="parent"></param>
        public void UpdateParentUnSelect(TreeViewItem parent)
        {
            if (parent != null)
            {
                (UIHelper.FindChildControl<CheckBox>(parent)).IsChecked = null;
                if ((parent.Parent as TreeViewItem) != null)
                {
                    UpdateParentUnSelect(parent.Parent as TreeViewItem);
                }
            }
        }

        /// <summary>
        /// 选中父节点
        /// </summary>
        /// <param name="parent"></param>
        public void UpdateParentSelect(TreeViewItem parent)
        {
            if (parent != null)
            {
                (UIHelper.FindChildControl<CheckBox>(parent)).IsChecked = true;
                if ((parent.Parent as TreeViewItem) != null)
                {
                    UpdateParentSelect(parent.Parent as TreeViewItem);
                }
            }
        }

        /// <summary>
        /// 获取组织树父节点
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static TreeViewItem GetParentTreeViewItem(DependencyObject item)
        {
            if (item != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);
                TreeViewItem parentTreeViewItem = parent as TreeViewItem;
                return (parentTreeViewItem != null) ? parentTreeViewItem : GetParentTreeViewItem(parent);
            }
            return null;
        }

        private void AddOrgObjectInList(TreeViewItem item)
        {
            if (item == null)
            {
                return;
            }

            if (ListCustomerpermission.Count() == 0)
            {
                return;
            }

            TreeViewItem menuitem = (TreeViewItem)treeMenu.SelectedItem;
            T_SYS_ENTITYMENU entMenu = menuitem.DataContext as T_SYS_ENTITYMENU;
            T_SYS_PERMISSION entPermission = DtGrid.SelectedItem as T_SYS_PERMISSION;

            ExtOrgObj obj = item.DataContext as ExtOrgObj;
            FrameworkUI.OrgTreeItemTypes nodeType = obj.ObjectType;
            OrgObject entTemp = new OrgObject();

            switch (nodeType)
            {
                case FrameworkUI.OrgTreeItemTypes.Company:
                    T_HR_COMPANY tmpOrg = (T_HR_COMPANY)obj.ObjectInstance;
                    if (tmpOrg != null)
                    {
                        entTemp.OrgID = tmpOrg.COMPANYID;
                        entTemp.OrgType = Convert.ToInt32(OrgTreeItemTypes.Company).ToString();
                    }
                    break;
                case FrameworkUI.OrgTreeItemTypes.Department:
                    T_HR_DEPARTMENT tmpDep = obj.ObjectInstance as T_HR_DEPARTMENT;
                    if (tmpDep != null)
                    {
                        entTemp.OrgID = tmpDep.DEPARTMENTID;
                        entTemp.OrgType = Convert.ToInt32(OrgTreeItemTypes.Department).ToString();
                    }
                    break;
                case FrameworkUI.OrgTreeItemTypes.Post:
                    T_HR_POST tmpPost = obj.ObjectInstance as T_HR_POST;
                    if (tmpPost != null)
                    {
                        entTemp.OrgID = tmpPost.POSTID;
                        entTemp.OrgType = Convert.ToInt32(OrgTreeItemTypes.Post).ToString();
                    }
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(entTemp.OrgID) && !string.IsNullOrEmpty(entTemp.OrgType))
            {

                foreach (CustomerPermission parentitem in ListCustomerpermission)
                {
                    if (parentitem.EntityMenuId != entMenu.ENTITYMENUID)
                    {
                        continue;
                    }

                    var per = from p in parentitem.PermissionValue
                              where p.Permission == entPermission.PERMISSIONID
                              select p;

                    if (per.Count() == 0)
                    {
                        break;
                    }

                    foreach (PermissionValue childitem in per)
                    {
                        ObservableCollection<OrgObject> entsOrg = new ObservableCollection<OrgObject>();
                        if (childitem.OrgObjects == null)
                        {
                            entsOrg.Add(entTemp);
                            childitem.OrgObjects = entsOrg;
                        }
                        else if (childitem.OrgObjects.Count() == 0)
                        {
                            entsOrg.Add(entTemp);
                            childitem.OrgObjects = entsOrg;
                        }
                        else if ((childitem.OrgObjects.Count() > 0))
                        {
                            var pcc = from o in childitem.OrgObjects
                                      where o.OrgID == entTemp.OrgID && o.OrgType == entTemp.OrgType
                                      select o;
                            if (pcc.Count() == 0)
                            {
                                childitem.OrgObjects.Add(entTemp);
                            }
                        }
                    }
                }

                this.DataContext = ListCustomerpermission;

            }

        }

        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            bool flag = false;

            try
            {
                if (ListCustomerpermission == null)
                {
                    return false;
                }

                if (ListCustomerpermission.Count() == 0)
                {
                    return false;
                }

                ObservableCollection<CustomerPermission> entsSubmit = new ObservableCollection<CustomerPermission>();
                ListCustomerpermission.ForEach(item =>
                {
                    entsSubmit.Add(item);
                });

                if (FormType == FormTypes.New)
                {
                    client.SetCutomterPermissionObjAsync(RoleID, entsSubmit, strResMsg);
                }
                else
                {
                    client.SetCutomterPermissionObjAsync(RoleID, entsSubmit, strResMsg);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            return flag;
        }

        /// <summary>
        /// 保存并关闭
        /// </summary>
        private void Cancel()
        {
            bool flag = false;
            flag = Save();
            if (!flag)
            {
                return;
            }

            closeFormFlag = true;
        }
        #endregion

        #region 事件
        /// <summary>
        /// 获取系统列表加载到ComboBox上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            //绑定系统类型
            if (e.Result != null)
            {                
                List<T_SYS_DICTIONARY> dicts = e.Result.ToList();

                cbxSystemType.ItemsSource = dicts;
                cbxSystemType.DisplayMemberPath = "DICTIONARYNAME";

                if (cbxSystemType.Items.Count() > 0)
                {
                    cbxSystemType.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// 获取指定系统下的功能项集合，并加载到树控件上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetSysMenuByTypeCompleted(object sender, GetSysMenuByTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

                allMenu = e.Result.ToList();
                BindTree();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        /// <summary>
        /// 获取所有权限项，并加载到DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetSysPermissionAllCompleted(object sender, GetSysPermissionAllCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    return;
                }
                if (e.Result != null)
                {
                    DtGrid.ItemsSource = e.Result.ToList();
                }
            }
        }        

        /// <summary>
        /// 根据角色ID获取自定义权限数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetCutomterPermissionObjCompleted(object sender, GetCutomterPermissionObjCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ListCustomerpermission = e.Result.ToList();
                if (ListCustomerpermission == null)
                {
                    return;
                }

                if (ListCustomerpermission.Count() == 0)
                {
                    return;
                }

                this.DataContext = ListCustomerpermission;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
            }
        }

        /// <summary>
        /// 存储当前角色的自定义权限配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_SetCutomterPermissionObjCompleted(object sender, SetCutomterPermissionObjCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ROLECUSTOMMENUPERMISSION")));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
            }
        }

        /// <summary>
        /// 选择系统，切换加载功能项的树
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxSystemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            treeMenu.Items.Clear();
            T_SYS_DICTIONARY dict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                client.GetSysMenuByTypeAsync(dict.DICTIONARYVALUE.ToString(), "");
            }
        }

        /// <summary>
        /// 功能项树节点选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        /// <summary>
        /// 功能项树节点的CheckBox勾选事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemCheckbox_Click(object sender, RoutedEventArgs e)
        {
            ClearPermissionDataGridCheckBox();//清空权限菜单的选择项
            ListPermValue.Clear();//清空权限的列表值
            CustomerPermission CustomerP = new CustomerPermission();
            T_SYS_ENTITYMENU Menu = new T_SYS_ENTITYMENU();
            TreeViewItem currentItem = null;

            foreach (TreeViewItem item in treeMenu.Items)
            {

                TreeViewItem myItem =
                    (TreeViewItem)(treeMenu.ItemContainerGenerator.ContainerFromItem(item));
                Menu = myItem.DataContext as T_SYS_ENTITYMENU;
                CheckBox cbx = SMT.SaaS.FrameworkUI.Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                if (cbx == (CheckBox)sender)
                {
                    currentItem = myItem;
                    //父级菜单不为空 且为菜单
                    if (Menu.T_SYS_ENTITYMENU2 != null && Menu.HASSYSTEMMENU == "1")
                    {
                        AddListCustomerPermission(CustomerP, Menu);
                    }
                    break;
                }
                FindTreeViewItemByChkBox(item, (CheckBox)sender, ref currentItem);
            }
            if (((CheckBox)sender).IsChecked.GetValueOrDefault() == false)
            {
                //将菜单ID清除
                //if ((((CheckBox)sender).Tag as T_SYS_ENTITYMENU).ENTITYMENUID.IndexOf(ListCustomerpermission.e))
                var menulist = from ent in SelectingCustomerpermission
                               where ent.EntityMenuId == Menu.ENTITYMENUID
                               select ent;

                if (menulist.Count() > 0)
                    ListCustomerpermission.Remove(menulist.ToList().FirstOrDefault()); //去掉已经存
                return;
            }


            if (currentItem != null)
            {
                UnCheckChildCheckbox(currentItem, (CheckBox)sender);
                UnCheckParentCheckbox(currentItem);
            }
        }

        /// <summary>
        /// 权限项行加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_SYS_PERMISSION MenuInfoT = (T_SYS_PERMISSION)e.Row.DataContext;
            CheckBox mychkBox = DtGrid.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            mychkBox.Tag = MenuInfoT;
        }

        /// <summary>
        /// 权限项选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectingCustomerpermission.Count() > 0 || ListCustomerpermission.Count() > 0)
            {
                CheckBox chkbox = sender as CheckBox;
                //添加功能项对应的权限值
                PermissionGridSetPermissionByCheckBox(chkbox);
                //ListPermValue.Clear();//赋值完后清空
            }
            else
            {
                //将选中的权限全部清空
                if (!(ListCustomerpermission.Count() > 0))
                {
                    ClearPermissionDataGridCheckBox();
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECTENTITYMENU"));
                }

            }
        }                
        #endregion

        private void OrgItemCheckbox_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
