/********************************************************************************
//出差报销form，alter by ken 2013/3/27
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.TravelExpApplyMaster;
using SMT.Saas.Tools.FBServiceWS;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;
using System.Windows.Browser;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class TravelReimbursementControl 
    {
        #region 注销，使用JS检测用户关闭IE动作

        private void hide()
        {
            try
            {
                #region 隐藏entitybrowser中的toolbar按钮
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.BtnSaveSubmit.IsEnabled = false;
                if (entBrowser.EntityEditor is IEntityEditor)
                {
                    ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
                    if (bar != null)
                    {
                        bar.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(ex.ToString());

            }
                #endregion
        }



        public void RegisterOnBeforeUnload()
        {
            //将此页面注册为JS可以调用的对象
            const string scriptableObjectName = "Bridge";
            HtmlPage.RegisterScriptableObject(scriptableObjectName, this);

            //监听Javascript事件
            string pluginName = HtmlPage.Plugin.Id;

            HtmlPage.Window.Eval(string.Format(
                @"function getTraveChargeMoney(){{
                var slApp = document.getElementById('{0}');
                slApp.Content.{1}.Save();
                var result = " + txtSubTotal.Text + @"
                return result;
                
                }}", pluginName, scriptableObjectName)
                );

//            HtmlPage.Window.Eval(
//                @"window.onbeforeunload=function(){{
//                if(document.body.clientWidth-event.clientX< 170&&event.clientY< 0||event.altKey) 
//                {
//                var msg=' 已 注 销 用 户 信 息。\n\n 点  确认 或 关闭  自 动 退 出 系 统！'
//                window.event.returnValue = '点击确认退出神州通集团协同办公平台.';
//                }
//                }}"
//            );


        }
           

           /// <summary>
        /// 执行网络服务
        /// [ScriptableMember]指示可由 JavaScript 调用方访问的特定属性、方法或事件。
        /// </summary>
        [ScriptableMember]
        public string getTraveChargeMoney()
        {
            //MessageBox.Show("我是SilverLight项目中的MessageBox", "o(∩_∩)o 哈哈", MessageBoxButton.OK);
            Save();
            return txtSubTotal.Text;
        }

    
 //function buttonClick() {
 //           //获取SilverLight插件中 以创建的对象
 //           var plugin = document.getElementById("mySilverlightControl");
 //           //执行SilverLight项目中已经注册的可被Javascript调用的方法
 //           plugin.content.Button.ExecuteWebService();
        //}


        #endregion
    }
}
