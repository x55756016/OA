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
using System.Globalization;
using System.Windows.Data;

namespace SMT.SaaS.OA.UI.Class
{
    public class ConverterDateToShortDate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    return System.Convert.ToDateTime(value).ToShortDateString();
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "时间格式错误!";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    return System.Convert.ToDateTime(value).ToShortDateString();
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "时间格式错误!";
            }
        }
    }

    
}
