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
    public class GridHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Controls.ContentPresenter pres = value as System.Windows.Controls.ContentPresenter;
            if (pres == null)
            {
                return parameter;
            }
            else
            {
                if ((string)pres.Content!= null)
                {
                    string rslt = Localization.GetString((string)pres.Content);
                    return rslt !=null ? rslt:pres.Content;
                }
                else
                {
                    return null;
                }              
            }
        }   

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
