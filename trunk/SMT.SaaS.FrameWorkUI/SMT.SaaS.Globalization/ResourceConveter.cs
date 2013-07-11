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

namespace SMT.SaaS.Globalization
{
    public class ResourceConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string source = (string)value;

            string rslt = Localization.GetString(source);

            if (value != null && parameter != null)
            {
                string[] paras = parameter.ToString().Split(',');

                int i = 0;

                foreach (string str in paras)
                {
                    string tmp = Localization.GetString(str);
                    paras[i] = string.IsNullOrEmpty(tmp) ? str : tmp;
                    i++;
                }
                rslt = string.Format(rslt, paras);
            }
            return string.IsNullOrEmpty(rslt) ? source : rslt;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string source = (string)value;
            return source;
        }
    }
}
