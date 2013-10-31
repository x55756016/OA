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
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;
using System.Windows.Browser;
using System.IO;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.UserControls
{

    public partial class SysRoleSetMenu2 : UserControl, IEntityEditor
    {
        /// <summary>
        /// 字典实体列表
        /// </summary>
        private List<T_SYS_DICTIONARY> tmpDictionary = new List<T_SYS_DICTIONARY>();
        

        private void GetSysDictionaryInfos()
        {
            RoleClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(RoleClient_GetSysDictionaryByCategoryCompleted);
            RoleClient.GetSysDictionaryByCategoryAsync("ASSIGNEDOBJECTTYPE");
        }
        /// <summary>
        /// 获取字典数据范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    tmpDictionary = e.Result.ToList();
                }
            }

        }
        /// <summary>
        /// 获取所有权限项列表
        /// </summary>
        private void LoadPermissionInfos()
        {
           
        }
        /// <summary>
        /// 获取权限项目列表并保存在tmpPermission权限列表中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_GetSysPermissionAllCompleted(object sender, GetSysPermissionAllCompletedEventArgs e)
        {           
            if (!e.Cancelled)
            {
                if (e.Error != null)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    return;
                }
                if (e.Result != null)
                {
                    tmpPermission = e.Result.ToList();                                       
                }
            }
            //开始获取HR菜单
            RoleClient.GetHRSysMenuByTypeAsync("0");
        }
        /// <summary>
        /// 获取单个角色的角色实体菜单列表
        /// </summary>
        private void GetRoleIDRoleEntityInfos()
        {
           
        }
        /// <summary>
        /// 获取角色实体菜单完毕后保存在tmpEditRoleEntityLIst列表中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_GetRoleEntityIDListInfosByRoleIDCompleted(object sender, GetRoleEntityIDListInfosByRoleIDCompletedEventArgs e)
        {
            //if (e.Error != null)
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
            //    return;
            //}
            //if (!e.Cancelled)
            //{
            //    if (e.Result != null)
            //    {
            //        tmpEditRoleEntityLIst = e.Result.ToList();
            //        foreach (T_SYS_ROLEENTITYMENU menu in tmpEditRoleEntityLIst)
            //        {
            //            tmpRoleEntityIDsList.Add(menu.ROLEENTITYMENUID);
            //        }
            //        RoleClient.GetPermissionByRoleIDSecondAsync(tmprole.ROLEID);
            //        //RoleClient.GetRolePermsAsync(tmpRoleEntityIDsList);
            //    }
            //    else
            //    {
            //        IsAdd = true;
            //        RefreshUI(RefreshedTypes.HideProgressBar);
            //        this.IsEnabled = true;
            //        DataGridColumnsAdd(DaGrOA, "myOACellTemplate");
            //        DataGridColumnsAdd(DaGrOAHead,"");
            //        DataGridColumnsAdd(DaGrHR, "HRCellTemplate");
            //        DataGridColumnsAdd(DaGrHRHead,"");
            //        DataGridColumnsAdd(DaGrFB, "myFBCellTemplate");
            //        DataGridColumnsAdd(DaGrFBHead,"");
            //        DataGridColumnsAdd(DaGrLM, "myLMCellTemplate");                    
            //        DataGridColumnsAdd(DaGrLMHead,"");
            //        DataGridColumnsAdd(DaGrEDM, "myEDMCellTemplate");
            //        DataGridColumnsAdd(DaGrEDMHead, "");
                    
                    
            //        if (HrMenu != null) this.DaGrHR.ItemsSource = HrMenu;
            //        if (OAMenu != null)
            //        {
            //            SetDataGridHeaderCollasped(DaGrHR, HrMenu);
            //        }
            //        if (FBMenu != null) this.DaGrFB.ItemsSource = FBMenu;
            //        if (LMMenu != null)
            //        {
            //            this.DaGrLM.ItemsSource = LMMenu;
            //            //DaGrLM.HeadersVisibility = DataGridHeadersVisibility.None;
            //        }
            //        if (EDMMenu != null) this.DaGrEDM.ItemsSource = EDMMenu;
            //        //if (HrMenu != null) DataGridBindingPcv(DaGrHR,HrMenu);// this.DaGrHR.ItemsSource = HrMenu;
            //        //if (OAMenu != null) DataGridBindingPcv(DaGrOA, OAMenu); //this.DaGrOA.ItemsSource = OAMenu;
            //        //if (FBMenu != null) DataGridBindingPcv(DaGrFB, FBMenu); //this.DaGrFB.ItemsSource = FBMenu;
            //        //if (LMMenu != null) DataGridBindingPcv(DaGrLM, LMMenu); //this.DaGrLM.ItemsSource = LMMenu;
            //    }

            //}
        }

        /// <summary>
        /// 获取角色实体菜单完毕后保存在tmpEditRoleEntityLIst列表中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_GetRoleEntityIDListInfosByRoleIDNewCompleted(object sender, GetRoleEntityIDListInfosByRoleIDNewCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                return;
            }
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    tmpEditRoleEntityLIst = e.Result.ToList();
                    foreach (V_RoleEntity menu in tmpEditRoleEntityLIst)
                    {
                        tmpRoleEntityIDsList.Add(menu.ROLEENTITYMENUID);
                    }
                    RoleClient.GetPermissionByRoleIDSecondAsync(tmprole.ROLEID);
                    //RoleClient.GetRolePermsAsync(tmpRoleEntityIDsList);
                }
                else
                {
                    IsAdd = true;
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    this.IsEnabled = true;
                    DataGridColumnsAdd(DaGrOA, "myOACellTemplate");
                    DataGridColumnsAdd(DaGrOAHead, "");
                    DataGridColumnsAdd(DaGrHR, "HRCellTemplate");
                    DataGridColumnsAdd(DaGrHRHead, "");
                    DataGridColumnsAdd(DaGrFB, "myFBCellTemplate");
                    DataGridColumnsAdd(DaGrFBHead, "");
                    DataGridColumnsAdd(DaGrLM, "myLMCellTemplate");
                    DataGridColumnsAdd(DaGrLMHead, "");
                    DataGridColumnsAdd(DaGrEDM, "myEDMCellTemplate");
                    DataGridColumnsAdd(DaGrEDMHead, "");

                    DataGridColumnsAdd(DaGrPM, "myPMCellTemplate");
                    DataGridColumnsAdd(DaGrPMHead, "");
                    if (HrMenu != null) this.DaGrHR.ItemsSource = HrMenu;
                    if (OAMenu != null)
                    {
                        SetDataGridHeaderCollasped(DaGrHR, HrMenu);
                    }
                    if (FBMenu != null) this.DaGrFB.ItemsSource = FBMenu;
                    if (LMMenu != null)
                    {
                        this.DaGrLM.ItemsSource = LMMenu;
                        //DaGrLM.HeadersVisibility = DataGridHeadersVisibility.None;
                    }
                    if (EDMMenu != null) this.DaGrEDM.ItemsSource = EDMMenu;
                    if (PMMenu != null) this.DaGrPM.ItemsSource = PMMenu;
                    //if (HrMenu != null) DataGridBindingPcv(DaGrHR,HrMenu);// this.DaGrHR.ItemsSource = HrMenu;
                    //if (OAMenu != null) DataGridBindingPcv(DaGrOA, OAMenu); //this.DaGrOA.ItemsSource = OAMenu;
                    //if (FBMenu != null) DataGridBindingPcv(DaGrFB, FBMenu); //this.DaGrFB.ItemsSource = FBMenu;
                    //if (LMMenu != null) DataGridBindingPcv(DaGrLM, LMMenu); //this.DaGrLM.ItemsSource = LMMenu;
                }

            }
        }


        /// <summary>
        /// 隐藏datagrid的表头
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="listmenu"></param>
        private void SetDataGridHeaderCollasped(DataGrid dt, List<V_MenuSetRole> listmenu)
        {
            dt.ItemsSource = listmenu;
            dt.HeadersVisibility = DataGridHeadersVisibility.None;
        }

        private void DataGridBindingPcv(DataGrid dt, List<V_MenuSetRole> OAMenu)
        {
            PagedCollectionView pcv = new PagedCollectionView(OAMenu);
            Type a = ((PagedCollectionView)pcv).CurrentItem.GetType();
            pcv.PageSize = 300;
            //dataPager.DataContext = pcv;
             
            dt.ItemsSource = pcv;
        }
        /// <summary>
        /// 权限视图集合获取完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoleClient_GetPermissionByRoleIDCompleted(object sender, GetPermissionByRoleIDCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        EntityPermissionInfosList = e.Result;//权限视图集合

                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    return;
                }
            }
        }


        #region 填充星星
        
        
        /// <summary>
        /// 填充Grid星星★★★★★权限
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="chx"></param>
        /// <param name="btn"></param>
        private void FillPermissionDataRange(DataGrid dg, string chx, string btn)
        {
            if (dg.Tag == "1")
            {//如果填充过了，就不填了
                return;
            }
            //注消事件
            //RoleClient.GetRolePermsCompleted -= new EventHandler<GetRolePermsCompletedEventArgs>(RoleClient_GetRolePermsCompleted);
            //if (tmpEditRoleEntityPermList.Count() == 0) return;
            if (EntityPermissionInfosListSecond.Count() == 0) return;
            if (dg.Columns.Count < tmpPermission.Count) return;//还没有动态生成列
            if (dg.ItemsSource != null)
            {
                #region
                //T_SYS_ROLEENTITYMENU tmpRoleEntity = new T_SYS_ROLEENTITYMENU();
                V_UserPermissionRoleID tmpRoleEntity = new V_UserPermissionRoleID();
                //Button hrrate;
                Button hrrate;
                if (dg.ItemsSource == null)
                    return;
                foreach (V_MenuSetRole obj in (List<V_MenuSetRole>)dg.ItemsSource)
                {
                        //var bb = from a in tmpEditRoleEntityLIst
                        //         where a.T_SYS_ENTITYMENU.ENTITYMENUID == obj.ENTITYMENUID
                        //         select a;
                    var bb = from a in EntityPermissionInfosListSecond
                             where a.EntityMenuID == obj.ENTITYMENUID
                             select a;
                        if (bb.Count() > 0)
                        {
                            tmpRoleEntity = bb.FirstOrDefault();
                            int PermCount = tmpPermission.Count;                            
                            for (int i = 1; i < PermCount + 1; i++)
                            {
                                if (dg.Columns[i].GetCellContent(obj) != null)
                                {
                                    var roles = from cc in EntityPermissionInfosListSecond
                                                where cc.RoleEntityMenuID == tmpRoleEntity.RoleEntityMenuID
                                                && cc.PermissionID == tmpPermission[i - 1].PERMISSIONID
                                                select cc;
                                    if (roles.Count() > 0)
                                    {
                                        //hrrate = dg.Columns[i].GetCellContent(obj).FindName(btn) as Button;
                                        hrrate = dg.Columns[i].GetCellContent(obj).FindName(btn) as Button;
                                        if (hrrate == null) continue;
                                        #region 填星星
                                        switch (roles.FirstOrDefault().PermissionDataRange)
                                        {
                                            //case "0"://集团
                                            //    //hrrate.Value = 1;
                                            //    hrrate.Content = "★★★★★";
                                            //    break;
                                            case "1"://公司
                                                //hrrate.Value = 0.8;
                                                hrrate.Content = "★★★★";
                                                break;
                                            case "2"://部门
                                                //hrrate.Value = 0.6;
                                                hrrate.Content = "★★★";
                                                break;
                                            case "3"://岗位
                                                //hrrate.Value = 0.4;
                                                hrrate.Content = "★★";
                                                break;
                                            case "4"://个人
                                                //hrrate.Value = 0.2;
                                                hrrate.Content = "★";
                                                break;

                                        }
                                        #endregion
                                        if (dg.Tag != "1")
                                        {
                                            dg.Tag = "1";
                                        }                                                                   
                                    }
                                }
                            }
                        }
                    //}
                }
                #endregion
            }
        }
        #endregion

        #region 保存事件完毕
        
        
        /// <summary>
        /// 保存完毕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RoleClient_BatchAddRoleEntityPermissionInfosCompleted(object sender, BatchAddRoleEntityPermissionInfosCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {
                if (e.Result)
                {

                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "设置成功", Utility.GetResourceStr("CONFIRMBUTTON"));

                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                }
                else
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "设置失败", Utility.GetResourceStr("CONFIRMBUTTON"));
                    return;
                }
            }
            
        }
        #endregion
        /// <summary>
        /// 点击星星按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image CBAll = sender as Image;
            TabControl RoleTabControl = new TabControl();
            RoleTabControl = tabrolemenu;
            //switch (RoleTabControl.SelectedIndex)
            //{
            //    case 1:
            //        SetTabControlRate(DaGrOA, "OArating");
            //        break;
            //    case 2:
            //        SetTabControlRate(DaGrHR, "HRrating");
            //        break;
            //    case 3:
            //        SetTabControlRate(DaGrLM, "LMrating");
            //        break;
            //    case 4:
            //        SetTabControlRate(DaGrFB, "FBrating");
            //        break;
            //    case 5:
            //        SetTabControlRate(DaGrEDM, "EDMrating");
            //        break;
            //    case 6:
            //        SetTabControlRate(DaGrPM, "PMrating");
            //        break;
            //}
        }

        /// <summary>
        /// 设置角色菜单权限数据范围
        /// </summary>
        /// <param name="dtGrid"></param>
        /// <param name="rateName"></param>
        private void SetTabControlRate(DataGrid dtGrid, string rateName)
        {
            //if (RoleTabControl.SelectedIndex == 3)
            //{
            if (dtGrid.ItemsSource != null)
            {
                //////////////////////////////////////////////////////

                foreach (object obj in dtGrid.ItemsSource)
                {
                    T_SYS_ENTITYMENU menu = new T_SYS_ENTITYMENU();

                    if (dtGrid.Columns[0].GetCellContent(obj) != null)
                    {

                        string StrContent = "";
                        int PermCount = 0;
                        PermCount = tmpPermission.Count;

                        for (int i = 1; i < PermCount + 1; i++)
                        {
                            Button mybtn = dtGrid.Columns[i].GetCellContent(obj).FindName(rateName) as Button;
                            if (mybtn.Content == null)
                            {
                                StrContent = "";
                            }
                            else
                            {
                                StrContent = mybtn.Content.ToString();
                            }

                            switch (StrContent)
                            {
                                case "":
                                    StrContent = "★";
                                    break;
                                case "★":
                                    StrContent = "★★";
                                    break;
                                case "★★":
                                    StrContent = "★★★";
                                    break;
                                case "★★★":
                                    StrContent = "★★★★";
                                    break;
                                //case "★★★★":
                                //    StrContent = "★★★★★";
                                //    break;
                                case "★★★★":
                                    StrContent = "";
                                    break;
                            }
                            mybtn.Content = StrContent;
                        }
                    }
                }
            }
            //}
        }
        /// <summary>
        /// 点击事件-设置权限数据范围
        /// </summary>
        /// <param name="sender"></param>
        private static void SetPermissionRate(Button sender)
        {
            Button Hrrating = sender as Button;
            string StrContent = "";
            if (Hrrating.Content == null)
            {
                StrContent = "";
            }
            else
            {
                StrContent = Hrrating.Content.ToString();
            }

            switch (StrContent)
            {
                case "":
                    StrContent = "★";
                    break;
                case "★":
                    StrContent = "★★";
                    break;
                case "★★":
                    StrContent = "★★★";
                    break;
                case "★★★":
                    StrContent = "★★★★";
                    break;
                //case "★★★★":
                //    StrContent = "★★★★★";
                //    break;
                case "★★★★":
                    StrContent = "";
                    break;
            }
            Hrrating.Content = StrContent;
        }


        /// <summary>
        /// 菜单点击事件-设置所有权限数据范围
        /// </summary>
        /// <param name="sender"></param>
        private void MenuSetPermissionRate(Button sender, DataGrid dtGrid, string rateName)
        {
            Button BtnPM = sender as Button;
            if (BtnPM.Tag == null)
            {
                MessageBox.Show("无法选择此行！");
                return;
            }
            DataGridRowEventArgs PMrating = BtnPM.Tag as DataGridRowEventArgs;
            string StrContent = "";
            int PermCount = 0;
            PermCount = tmpPermission.Count;

            for (int i = 1; i < PermCount + 1; i++)
            {
                Button mybtn = dtGrid.Columns[i].GetCellContent(PMrating.Row).FindName(rateName) as Button;
                //StrContent = mybtn.Content.ToString();
                if (mybtn.Content == null)
                {
                    StrContent = "";
                }
                else
                {
                    StrContent = mybtn.Content.ToString();
                }
                
                switch (StrContent)
                {
                    case "":
                        StrContent = "★";
                        break;
                    case "★":
                        StrContent = "★★";
                        break;
                    case "★★":
                        StrContent = "★★★";
                        break;
                    case "★★★":
                        StrContent = "★★★★";
                        break;
                    //case "★★★★":
                    //    StrContent = "★★★★★";
                    //    break;
                    case "★★★★":
                        StrContent = "";
                        break;
                }
                mybtn.Content = StrContent;
            }

            
        }



    }


}
#region 注释掉的方法


        ///// <summary>
        ///// dg DAtagrd 
        ///// </summary>
        ///// <param name="dg"></param>
        ///// <param name="chx">checkbox</param>
        ///// <param name="strtxt">textbox</param>
        ///// <param name="btn">button</param>
        //private void FillPermissionDataRangeNewfill(DataGrid dg, string chx, string btn)
        //{
        //    if (tmpEditRoleEntityPermList.Count() == 0) return;
        //    if (dg.Columns.Count < tmpPermission.Count) return;//还没有动态生成列
        //    if (dg.ItemsSource != null)
        //    {
        //        foreach (object obj in dg.ItemsSource)
        //        {
        //            T_SYS_ENTITYMENU menu = new T_SYS_ENTITYMENU();
        //            if (dg.Columns[0].GetCellContent(obj) != null)
        //            {
        //                CheckBox cb1 = dg.Columns[0].GetCellContent(obj).FindName(chx) as CheckBox; //cb为
        //                if (cb1 == null) continue;
        //                //cb1.Tag
        //                menu = cb1.Tag as T_SYS_ENTITYMENU;
        //                var bb = from a in tmpEditRoleEntityLIst
        //                         where a.T_SYS_ENTITYMENU.ENTITYMENUID == menu.ENTITYMENUID
        //                         select a;
        //                if (bb.Count() > 0)
        //                {
        //                    cb1.IsChecked = true;
        //                    T_SYS_ROLEENTITYMENU tmpRoleEntity = new T_SYS_ROLEENTITYMENU();
        //                    tmpRoleEntity = bb.FirstOrDefault();
        //                    int PermCount = 0;
        //                    PermCount = tmpPermission.Count;
        //                    int IndexCount = 2;
        //                    //IndexCount = PermCount
        //                    for (int i = 2; i < PermCount + 2; i++)
        //                    {
        //                        IndexCount = IndexCount + i;
        //                        if (dg.Columns[i].GetCellContent(obj) != null)
        //                        {
        //                            T_SYS_PERMISSION tmpPerm = new T_SYS_PERMISSION();
        //                            tmpPerm = tmpPermission[i - 2];
        //                            var roles = from cc in tmpEditRoleEntityPermList
        //                                        where cc.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == tmpRoleEntity.ROLEENTITYMENUID && cc.T_SYS_PERMISSION.PERMISSIONID == tmpPerm.PERMISSIONID
        //                                        //where cc.T_SYS_ROLEENTITYMENU.ROLEENTITYMENUID == tmpRoleEntity.ROLEENTITYMENUID 
        //                                        select cc;
        //                            if (roles.Count() > 0)
        //                            {
        //                                //Rating hrrate = dg.Columns[i].GetCellContent(obj).FindName("HRrating") as Rating;
        //                                Button hrrate = dg.Columns[i].GetCellContent(obj).FindName(btn) as Button;

        //                                switch (roles.FirstOrDefault().DATARANGE)
        //                                {
        //                                    case "0"://集团
        //                                        //hrrate.Value = 1;
        //                                        hrrate.Content = "★★★★★";
        //                                        break;
        //                                    case "1"://公司
        //                                        //hrrate.Value = 0.8;
        //                                        hrrate.Content = "★★★★";
        //                                        break;
        //                                    case "2"://部门
        //                                        //hrrate.Value = 0.6;
        //                                        hrrate.Content = "★★★";
        //                                        break;
        //                                    case "3"://岗位
        //                                        //hrrate.Value = 0.4;
        //                                        hrrate.Content = "★★";
        //                                        break;
        //                                    case "4"://个人
        //                                        //hrrate.Value = 0.2;
        //                                        hrrate.Content = "★";
        //                                        break;


        //                                }
        //                                //hrrate.Value =
        //                            }

        //                        }// if dg.columns

        //                    }// for int i

        //                }//bb.cout


        //            }
        //        }//foreach(dg)
        //    }
        //}

#endregion
