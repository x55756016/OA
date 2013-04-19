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
using System.Windows.Data;
using SMT.Saas.Tools.FlowDesignerWS;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace SMT.SaaS.OA.UI
{
    public class ModuleNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            List<FLOW_MODELDEFINE_T> dicts = Application.Current.Resources["FLOW_MODELDEFINE_T"] as List<FLOW_MODELDEFINE_T>;
            if (dicts == null)
                return value;

            var objs = from a in dicts
                       where a.MODELCODE.ToString() == value.ToString()
                       select a;

            FLOW_MODELDEFINE_T dict = objs.Count() > 0 ? objs.FirstOrDefault() : null;
            return dict == null ? value : dict.DESCRIPTION;//模块描述
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ModuleNameInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (parameter == null)
                return value;
            switch (parameter.ToString())
            {
                case "MododuleName":
                    List<FLOW_MODELDEFINE_T> dictc = Application.Current.Resources["FLOW_MododuleNameInfo"] as List<FLOW_MODELDEFINE_T>;
                    if (dictc == null)
                        return value;
                    var objc = from a in dictc
                               where a.MODELCODE == value.ToString()
                               select a.DESCRIPTION;
                    return objc.Count() > 0 ? objc.FirstOrDefault() : value;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
