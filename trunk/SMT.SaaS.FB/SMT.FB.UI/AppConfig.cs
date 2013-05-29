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

using System.Collections.Generic;
using SMT.SaaS.FrameworkUI.Helper;

namespace SMT.FB.UI
{
    public class AppConfig
    {

        //定义全局的样式号
        public static int _CurrentStyleCode = 0;
        /// <summary>
        /// 设置路径加载样式
        /// </summary>
        public static void SetStyles(string path, FrameworkElement panel)
        {
            UIHelper._CurrentStyleCode_2 = _CurrentStyleCode;
            Uri uri = new Uri(path, UriKind.Relative);
            ApplyTheme(uri, panel);
        }

        /// <summary>
        /// 根据路径加载样式到特定面板中
        /// </summary>
        /// <param name="uri">路径</param>
        /// <param name="panel">面板</param>
        private static void ApplyTheme(Uri uri, FrameworkElement panel)
        {
            // ImplicitStyleManager.UseApplicationResources = false;

            //ImplicitStyleManager.SetResourceDictionaryUri(panel, uri);
            //  ImplicitStyleManager.SetApplyMode(panel, ImplicitStylesApplyMode.Auto);
            //  ImplicitStyleManager.Apply(panel);
        }

        public static T FindParentControl<T>(DependencyObject item) where T : class
        {
            if (item != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);
                T parentGrid = parent as T;
                return (parentGrid != null) ? parentGrid : FindParentControl<T>(parent);
            }
            return null;
        }
    }
}
