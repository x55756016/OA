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

namespace SMT.HRM.UI
{
    public class IsEnabledConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            object objRes = null;

            switch (parameter.ToString().ToUpper())
            {
                case "BROWSE":
                    objRes = "False";
                    break;
                case "AUDIT":
                    objRes = "False";
                    break;
                default:
                    objRes = "True";
                    break;
            }

            return objRes;
        }       

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
