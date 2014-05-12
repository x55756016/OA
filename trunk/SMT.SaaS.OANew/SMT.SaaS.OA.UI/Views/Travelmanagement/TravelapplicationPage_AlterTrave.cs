/********************************************************************************

** 作者： ken

** 创始时间：2014-03-27

** 修改人：刘锦

** 修改时间：2010-07-12

** 描述：

**    主要用于出差申请信息的数据展示，将已保存的出差申请数据展示在DataGrid列表控件上

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using System.Globalization;
using SMT.SAAS.Controls.Toolkit.Windows;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SAAS.Application;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;
using SMT.SAAS.Platform.Logging;

namespace SMT.SaaS.OA.UI.Views.Travelmanagement
{
    public partial class TravelapplicationPage : BasePage
    {


        #region 修改行程，跟重新提交一样
        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                V_Travelmanagement ent = new V_Travelmanagement();
                ent = (DaGr.SelectedItems[0] as V_Travelmanagement);

                ///增加重新提交的判断，审核通过的不允许重新提交
                if (ent.TraveAppCheckState!="1" && ent.Travelmanagement.ISALTERTRAVE == "1"  && (ent.TrCheckState == "0" || string.IsNullOrEmpty(ent.TrCheckState)))
                {
                    alterTravelBusiness(ent);
                }
                else
                {
                    if (ent.TraveAppCheckState == "2" && ent.TrCheckState == "2")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("已审核通过的出差报销不能修改行程"), Utility.GetResourceStr("确定"), MessageIcon.Exclamation);
                        return;
                    }
                    if (ent.TraveAppCheckState == "2" && (ent.TrCheckState == "0" || string.IsNullOrEmpty(ent.TrCheckState)))
                    {
                        alterTravelBusiness(ent);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差申请终审后出差报销未提交之前方可修改行程。"
                            , Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SetLogAndShowLog(ex.ToString());
            }
        }

        private void alterTravelBusiness(V_Travelmanagement ent)
        {
            BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Resubmit, ent.Travelmanagement.BUSINESSTRIPID);
            AddWin.isAlterTrave = true;
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.RemoveSMTLoading();
            browser.EntityBrowseToolBar.MaxHeight = 0;
            browser.FormType = FormTypes.Resubmit;
            browser.MinWidth = 980;
            browser.MinHeight = 445;
            browser.TitleContent = "出差申请修改行程";
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.EntityEditor = AddWin;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }
        #endregion
    }
}
