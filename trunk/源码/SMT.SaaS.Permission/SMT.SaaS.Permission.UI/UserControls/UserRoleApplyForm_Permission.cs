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
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;


namespace SMT.SaaS.Permission.UI.UserControls
{
    public partial class UserRoleApplyForm : UserControl, IClient, IEntityEditor, IAudit
    {
        #region 选择菜单按钮
        SelectMultiMenu addFrm;//选择菜单窗体
        private List<T_SYS_ENTITYMENU> SelectedMenus = new List<T_SYS_ENTITYMENU>(); //选择的菜单
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string StrType = "";
            T_SYS_DICTIONARY selectDict = new T_SYS_DICTIONARY();
            selectDict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;
            if (selectDict != null)
                StrType = selectDict.DICTIONARYVALUE.ToString();
            addFrm = new SelectMultiMenu(StrType, SelectedMenus);
            EntityBrowser browser = new EntityBrowser(addFrm);
            browser.FormType = FormTypes.Browse;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 删除按钮
        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            IsRoleEntityMenuChange = true;
        }
        #endregion

        #region checkbox事件
        private void ChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb1 = sender as CheckBox;
            cb1.IsChecked = true;
            SetCheckedPermission(cb1);
        }

        private void ChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb1 = sender as CheckBox;
            cb1.IsChecked = false;
            SetUnCheckedPermission(cb1);
            //ClearPermissionDataGridCheckBox();
        }

        private void SetCheckedPermission(CheckBox chBox)
        {
            if (this.DataContext == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECTENTITYMENU"));
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                return;
            }

            T_SYS_ENTITYMENU entPerm = chBox.Tag as T_SYS_ENTITYMENU;

            //PermissionValue PerObj = new PermissionValue();
            //PerObj.Permission = entPerm.PERMISSIONID;
            //T_SYS_ENTITYMENU entTemp = DaGrMenu.SelectedItems[0] as T_SYS_ENTITYMENU;
            //AddCustomPermissionByPerm(entTemp, PerObj);
        }

        private void SetUnCheckedPermission(CheckBox chBox)
        {
            if (DaGr.SelectedItems.Count == 0)
            {
                return;
            }
            T_SYS_ENTITYMENU entPerm = chBox.Tag as T_SYS_ENTITYMENU;

            //PermissionValue PerObj = new PermissionValue();
            //PerObj.Permission = entPerm.PERMISSIONID;
            //T_SYS_ENTITYMENU entTemp = DaGrMenu.SelectedItems[0] as T_SYS_ENTITYMENU;
            //RemoveCustomPermissionByPerm(entTemp, PerObj);
        }

        #endregion

        #region 选择菜单加载数据事件
        
        private void browser_ReloadDataEvent()
        {
            if (addFrm.SelectedMultiMenu == null)
            {
                return;
            }
            if (addFrm.SelectedMultiMenu.Count == 0)
            {
                return;
            }

            List<T_SYS_ENTITYMENU> entMenuChecks = DaGr.ItemsSource as List<T_SYS_ENTITYMENU>;
            List<T_SYS_ENTITYMENU> entMenuAdds = new List<T_SYS_ENTITYMENU>();
            if (addFrm.SelectedMultiMenu.Count() > 0)
            {
                foreach (var h in addFrm.SelectedMultiMenu)
                {
                    var entity = from q in SelectedMenus
                                 where q.ENTITYMENUID == h.ENTITYMENUID
                                 select q;
                    if (entity.Count() == 0)
                    {
                        entMenuChecks.Add(h);
                        //SelectedMenus.Add(h);
                    }

                }

            }
            if (entMenuChecks == null)
            {
                entMenuChecks = addFrm.SelectedMultiMenu;
            }
            else
            {
                entMenuChecks = SelectedMenus;
            }

            entMenuAdds.AddRange(entMenuChecks);
            //DataGridBindingPcv(DaGr, SelectedMenus);

            DaGr.Tag = "0";//设为没填充
            //FillPermissionDataRange(DaGr, "myChkBtnHR", "HRrating");
            if (entMenuChecks != null) this.DaGr.ItemsSource = entMenuChecks;
            //DaGr.Loaded += new RoutedEventHandler(DaGr_Loaded);  
            FillPermissionDataRange(DaGr, "myChkBtnHR", "HRrating");
            
        }

        #region 将选择的菜单绑定到dagr,默认不进行翻页
        
        
        private void DataGridBindingPcv(DataGrid dt, List<T_SYS_ENTITYMENU> OAMenu)
        {
            PagedCollectionView pcv = new PagedCollectionView(OAMenu);
            Type a = ((PagedCollectionView)pcv).CurrentItem.GetType();
            pcv.PageSize = 600;
            //dataPager.DataContext = pcv;

            dt.ItemsSource = pcv;

            //InitSetPermissionValue();
            
        }

        #endregion

        #endregion

    }
}
