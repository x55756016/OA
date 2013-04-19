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
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections.Generic;
//using System.Windows.Controls.Theming;
using SMT.SaaS.OA.UI.Class;


namespace SMT.SaaS.OA.UI
{
    ////public class AppConfig
    ////{
    ////    public LoginUser CurrentUser { get; set; }
        
    ////    public AppConfig()
    ////    {
    ////        CurrentUser = new LoginUser();
    ////    }
    ////    //定义全局的样式号
    ////    public static int _CurrentStyleCode = 0;

    ////    /// <summary>
    ////    /// 设置路径加载样式
    ////    /// </summary>
    ////    public static void SetStyles(string path, FrameworkElement panel)
    ////    {
    ////        Uri uri = new Uri(path, UriKind.Relative);
    ////        ApplyTheme(uri, panel);
    ////    }
        
    ////    /// <summary>
    ////    /// 根据路径加载样式到特定面板中
    ////    /// </summary>
    ////    /// <param name="uri">路径</param>
    ////    /// <param name="panel">面板</param>
    ////    private static void ApplyTheme(Uri uri, FrameworkElement panel)
    ////    {

    ////        ImplicitStyleManager.SetResourceDictionaryUri(panel, uri);
    ////        if (panel.GetType() == typeof(ChildWindow))
    ////        {
    ////            ImplicitStyleManager.SetApplyMode(panel, ImplicitStylesApplyMode.OneTime);
    ////        }
    ////        else
    ////        {
    ////            ImplicitStyleManager.SetApplyMode(panel, ImplicitStylesApplyMode.Auto);
    ////        }
    ////        ImplicitStyleManager.Apply(panel);
    ////    } 

    ////}


}
