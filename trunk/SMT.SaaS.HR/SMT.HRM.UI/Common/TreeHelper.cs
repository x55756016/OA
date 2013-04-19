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

namespace SMT.HRM.UI
{
    public static class TreeHelper
    {
        public static T FindParentByType<T>(this DependencyObject child) where T : DependencyObject
        {
            Type type = typeof(T);
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent == null)
            {
                return null;
            }
            else if (parent.GetType() == type)
            {
                return parent as T;
            }
            else
            {
                return parent.FindParentByType<T>();
            }
        }
    }
}
