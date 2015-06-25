using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.LocalData.Tables;
using SMT.SaaS.LocalData.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SMT.SAAS.Platform.ViewModel.Common
{
    public class UserPermissionHelper
    {
        public static event EventHandler OnGetUserMenuPermissionCompleted;
        private static PermissionServiceClient client;
        private static V_UserMenuPermission CurrentMenu;
        public static PermissionServiceClient pmClient
        {
            get
            {
                if(client==null)
                {
                    client = new PermissionServiceClient();
                    client.GetEntityPermissionByUserCompleted += client_GetEntityPermissionByUserCompleted;
                }
                return client;
            }
        }

        //1. 获取用户菜单权限
        public static void GetUserMenuPermission(V_UserMenuPermission Menu)
        {
            CurrentMenu = Menu;
            //权限检查发现有变更时，权限需要重新从服务器获取
            if (SMT.SAAS.Main.CurrentContext.AppContext.IsPermUpdate)
            {
                pmClient.GetEntityPermissionByUserAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID, Menu.ENTITYMENUID);
                return;
            }

            //判断是否已存在此权限
            if (CheckUserPermissionIsExit(Menu))
            {
                if (OnGetUserMenuPermissionCompleted != null)
                {
                    OnGetUserMenuPermissionCompleted(null, EventArgs.Empty);
                }
            }
            else
            {
                if (!V_UserPermUILocalVM.IsExists(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, Menu.ENTITYMENUID))
                {
                    pmClient.GetEntityPermissionByUserAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID, Menu.ENTITYMENUID);
                    return;
                }
                GetPermissionInfoUIByLocal(Menu);
            }
        }


        static void client_GetEntityPermissionByUserCompleted(object sender, GetEntityPermissionByUserCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    SavePermissionByLocal(e.Result);
                }
            }else
            {
                MessageBox.Show("获取员工模块权限失败！");
                if (OnGetUserMenuPermissionCompleted != null)
                {
                    OnGetUserMenuPermissionCompleted(null, EventArgs.Empty);
                }
            }
        }

        static bool CheckUserPermissionIsExit(V_UserMenuPermission Menu)
        {
            var q = from ent in SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI
                    where ent.EntityMenuID == Menu.ENTITYMENUID
                    select ent;
            if(q.Count()>0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 保存权限到本地数据库
        /// </summary>
        private static void SavePermissionByLocal(ObservableCollection<SMT.Saas.Tools.PermissionWS.V_UserPermissionUI> list)
        {
            try
            {
                if (list == null)
                {
                    return;
                }

                List<V_UserPermUILocal> localUserPermUIs = new List<V_UserPermUILocal>();
                List<V_CustomerPermission> localCusPerms = new List<V_CustomerPermission>();
                List<V_PermissionValue> localPermValues = new List<V_PermissionValue>();
                List<V_OrgObject> localOrgObjs = new List<V_OrgObject>();

                string strEmployeeID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                foreach (var item in list)
                {
                    if (item.MenuCode == null)
                    {
                        continue;
                    }

                    V_UserPermUILocal info = item.CloneObject<V_UserPermUILocal>(new V_UserPermUILocal());
                    info.UserModuleID = System.Guid.NewGuid().ToString();
                    info.EmployeeID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    localUserPermUIs.Add(info);

                    //如果自定义权限为空，就不用再继续向下轮询
                    if (item.CustomerPermission == null)
                    {
                        continue;
                    }

                    V_CustomerPermission cusPerm = item.CustomerPermission.CloneObject<V_CustomerPermission>(new V_CustomerPermission());
                    cusPerm.UserModuleID = System.Guid.NewGuid().ToString();
                    cusPerm.EmployeeID = strEmployeeID;
                    cusPerm.PermissionUIID = info.UserModuleID;
                    localCusPerms.Add(cusPerm);

                    if (item.CustomerPermission.PermissionValue == null)
                    {
                        continue;
                    }

                    if (item.CustomerPermission.PermissionValue.Count == 0)
                    {
                        continue;
                    }

                    foreach (var p in item.CustomerPermission.PermissionValue)
                    {
                        V_PermissionValue permValue = p.CloneObject<V_PermissionValue>(new V_PermissionValue());
                        permValue.UserModuleID = info.UserModuleID;
                        permValue.EmployeeID = strEmployeeID;
                        permValue.CusPermID = cusPerm.UserModuleID;

                        if (p.OrgObjects != null)
                        {
                            if (p.OrgObjects.Count > 0)
                            {
                                foreach (var d in p.OrgObjects)
                                {
                                    V_OrgObject orgObj = d.CloneObject<V_OrgObject>(new V_OrgObject());
                                    orgObj.UserModuleID = info.UserModuleID;
                                    orgObj.EmployeeID = strEmployeeID;
                                    orgObj.PermValueID = permValue.UserModuleID;

                                    localOrgObjs.Add(orgObj);
                                }
                            }
                        }

                        localPermValues.Add(permValue);
                    }
                }

                V_UserPermUILocalVM.SaveV_UserPermUILocal(strEmployeeID, localUserPermUIs);
                V_CustomerPermissionVM.SaveV_CustomerPermission(strEmployeeID, localCusPerms);
                V_PermissionValueVM.SaveV_PermissionValue(strEmployeeID, localPermValues);
                V_OrgObjectVM.SaveV_OrgObject(strEmployeeID, localOrgObjs);
            }catch(Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.logAndShow(ex.ToString());
            }finally
            {
                GetPermissionInfoUIByLocal(CurrentMenu);
            }
        }

        private static void GetPermissionInfoUIByLocal(V_UserMenuPermission Menu)
        {
            try
            {
                string strEmployeeID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                List<SMT.SaaS.LocalData.V_UserPermissionUI> userPermissionUIs = new List<SMT.SaaS.LocalData.V_UserPermissionUI>();

                List<V_UserPermUILocal> userPermUILocals = V_UserPermUILocalVM.GetAllV_UserPermUILocal(strEmployeeID);
                List<V_CustomerPermission> customerPerms = V_CustomerPermissionVM.GetAllV_CustomerPermission(strEmployeeID);
                List<V_PermissionValue> permissionValues = V_PermissionValueVM.GetAllV_PermissionValue(strEmployeeID);
                List<V_OrgObject> v_OrgObjects = V_OrgObjectVM.GetAllV_OrgObject(strEmployeeID);

                if (userPermUILocals == null)
                {
                    return;
                }

                foreach (var item in userPermUILocals)
                {
                    if (item.EntityMenuID != null)
                    {
                        if (ViewModel.Context.CacheMenuPermissionList == null)
                        {
                            ViewModel.Context.CacheMenuPermissionList = new List<string>();
                        }

                        SMT.SaaS.LocalData.V_UserPermissionUI userPermissionUI = item.CloneObject<SMT.SaaS.LocalData.V_UserPermissionUI>(new SMT.SaaS.LocalData.V_UserPermissionUI());

                        V_CustomerPermission v_cusPerm = null;
                        foreach (var p in customerPerms)
                        {
                            if (p.UserModuleID != item.UserModuleID)
                            {
                                continue;
                            }

                            v_cusPerm = p;
                            break;
                        }

                        if (v_cusPerm == null)
                        {
                            userPermissionUIs.Add(userPermissionUI);
                            continue;
                        }

                        userPermissionUI.CustomerPermission = v_cusPerm.CloneObject<SMT.SaaS.LocalData.CustomerPermission>(new SMT.SaaS.LocalData.CustomerPermission());
                        List<SMT.SaaS.LocalData.PermissionValue> permValues = new List<SaaS.LocalData.PermissionValue>();
                        foreach (var d in permissionValues)
                        {
                            if (v_cusPerm == null)
                            {
                                break;
                            }

                            if (d.UserModuleID != v_cusPerm.UserModuleID)
                            {
                                continue;
                            }

                            SMT.SaaS.LocalData.PermissionValue permValue = d.CloneObject<SMT.SaaS.LocalData.PermissionValue>(new SaaS.LocalData.PermissionValue());
                            List<SMT.SaaS.LocalData.OrgObject> orgObjects = new List<SaaS.LocalData.OrgObject>();
                            foreach (var o in v_OrgObjects)
                            {
                                if (v_cusPerm == null)
                                {
                                    break;
                                }

                                if (o.UserModuleID != d.UserModuleID)
                                {
                                    continue;
                                }

                                SMT.SaaS.LocalData.OrgObject orgObject = o.CloneObject<SMT.SaaS.LocalData.OrgObject>(new SaaS.LocalData.OrgObject());
                                orgObjects.Add(orgObject);
                            }
                            permValue.OrgObjects.AddRange(orgObjects);
                            permValues.Add(permValue);
                        }

                        userPermissionUI.CustomerPermission.PermissionValue.AddRange(permValues);
                    }
                }
                if (userPermissionUIs != null)
                {
                    foreach (var u in userPermissionUIs)
                    {
                        if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI == null)
                        {
                            SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI = new List<SaaS.LocalData.V_UserPermissionUI>();
                        }

                        if (!SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI.Contains(u))
                        {
                            SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI.Add(u);
                        }
                    }
                }

                if (ViewModel.Context.CacheMenuPermissionList.Contains(Menu.ENTITYMENUID) == false)
                {
                    ViewModel.Context.CacheMenuPermissionList.Add(Menu.ENTITYMENUID);
                }

            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.logAndShow(ex.ToString());
            }
            finally
            {
                if (OnGetUserMenuPermissionCompleted != null)
                {
                    OnGetUserMenuPermissionCompleted(null, EventArgs.Empty);
                }
            }
        }
    }
}
