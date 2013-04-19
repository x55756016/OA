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



namespace SMT.SaaS.FrameworkUI.Helper
{
    public class UIHelper
    {
        public static int _CurrentStyleCode_2;
        
        
        public void DownLoad(string url)
        {
            WebClient client = new WebClient();
            client.DownloadStringAsync(new Uri(url));
        }
        public string GetLaguageVaue(string FromValue,string Language)
        {
            string value = string.Empty;

            return value;
        }

        /// <summary>
        /// TreeViewItem myItem =(TreeViewItem)(treeOrganization.ItemContainerGenerator.ContainerFromItem(item));
        ///CheckBox cbx = FindChildControl<CheckBox>(myItem);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindChildControl<T>(DependencyObject obj)
            where T : DependencyObject
        {
            if (obj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return child as T;
                else
                {
                    T childOfChild = FindChildControl<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        public static string GetResourceStr(string message)
        {
            string rslt = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString(message, SMT.SaaS.Globalization.Localization.UiCulture);
            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }
        public static string GetResourceStr(string message, string parameter)
        {
            string rslt = SMT.SaaS.Globalization.Localization.GetString(message, parameter);

            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }
    }
}
