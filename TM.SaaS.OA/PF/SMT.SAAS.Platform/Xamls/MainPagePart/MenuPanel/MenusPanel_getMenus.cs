
using System;
using System.Windows.Controls;
using System.Windows;
using SMT.SAAS.Platform.Controls.CWrapPanel;
using System.Collections.Generic;
using System.Windows.Media;
//using SMT.SAAS.Platform.Xamls.Icons;
using System.Collections.ObjectModel;
using SMT.SAAS.Platform.ViewModel;
using SMT.SAAS.Platform.Core.Modularity;
using System.Linq;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class MenusPanel
    {

        private List<T_SYS_DICTIONARY> AllsystemTypes;
        private List<V_UserMenuPermission> AllMenus;
        void Pmclient_GetSystemTypeListByUserIDCompleted(object sender, GetSystemTypeListByUserIDCompletedEventArgs e)
        {
            hideLoading();
            try
            {
                AllsystemTypes = e.Result.ToList();
                ViewModel.Context.CacheSystemType = AllsystemTypes;
                if (!Application.Current.Resources.Contains("AllsystemTypes"))
                {
                    Application.Current.Resources.Add("AllsystemTypes", AllsystemTypes);
                }
                //IosManager.SetValue("AllsystemTypes", AllsystemTypes);
                GetMenusAndShow();

            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 获取分系统及其菜单
        /// </summary>
        public void GetMenusAndShow()
        {
            if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo != null)
            {
                 AllMenus = Application.Current.Resources["AllMenus"] as List<V_UserMenuPermission>;
                if (AllMenus == null || AllMenus.Count == 0)
                {
                    showLoading();
                    Pmclient.GetSysLeftMenuFilterPermissionToNewFrameAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
                }
                else
                {
                    ViewModel.Context.CacheMenuAll = AllMenus;
                    //权限检查发现有变更时，权限需要重新从服务器获取
                    if (SMT.SAAS.Main.CurrentContext.AppContext.IsPermUpdate)
                    {
                        showLoading();
                        Application.Current.Resources.Remove("AllMenus");
                        Pmclient.GetSysLeftMenuFilterPermissionToNewFrameAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
                        return;
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        BindControls();
                    });
                }
            }
        }

        void Pmclient_GetSysLeftMenuFilterPermissionToNewFrameCompleted(object sender, Saas.Tools.PermissionWS.GetSysLeftMenuFilterPermissionToNewFrameCompletedEventArgs e)
        {
            try {
                 AllMenus = new List<V_UserMenuPermission>();
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(SMT.SaaS.FrameworkUI.Common.Utility.GetResourceStr("ERROR"), SMT.SaaS.FrameworkUI.Common.Utility.GetResourceStr("ERRORINFO"),
                           SMT.SaaS.FrameworkUI.Common.Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    if (e.Result==null)
                    {
                        MessageBox.Show("未获取到用户菜单，请检查员工是否有相关权限！");
                        return;
                    }
                    AllMenus = e.Result.ToList();
                    //缓存所有菜单
                    if (!Application.Current.Resources.Contains("AllMenus"))
                    {
                        Application.Current.Resources.Add("AllMenus", AllMenus);
                    }
                    ViewModel.Context.CacheMenuAll = AllMenus;
                    //IosManager.SetValue("AllMenus", AllMenus);


                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        BindControls();
                    });
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                hideLoading();
            }
           
        }
    }
}
