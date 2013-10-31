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
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

using System.Collections;
using System.Collections.Generic;
using SMT.Saas.Tools.PermissionWS;
using System.Linq;

namespace SMT.SaaS.Permission.UI
{
    public class BoolenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return value;
            object rslt;

            //返回0或1
            if (parameter != null && parameter.ToString() == "1")
            {
                if (value.ToString().ToLower() == "true" || value.ToString() == "1")
                    rslt = "1";
                else
                    rslt = "0";
            }
            else
            {
                if (value.ToString().ToLower() == "true" || value.ToString() == "1")
                    rslt = "true";
                else
                    rslt = "false";
            }

            return rslt;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }


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
