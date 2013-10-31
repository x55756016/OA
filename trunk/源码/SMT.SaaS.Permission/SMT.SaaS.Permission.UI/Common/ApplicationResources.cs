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

namespace SMT.SaaS.Permission.UI
{
    public class ApplicationResources
    {
        private static readonly ResourceManager resourceManager =
            new ResourceManager("SMT.SaaS.Globalization.Resource",
                                Assembly.GetExecutingAssembly());

        private static CultureInfo uiCulture = Thread.CurrentThread.CurrentUICulture;
        public static CultureInfo UiCulture
        {
            get { return uiCulture; }
            set { uiCulture = value; }
        }

        public string Get(string resource)
        {
            return resourceManager.GetString(resource, UiCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var reader = (ApplicationResources)value;
            string rslt = reader.Get((string)parameter);
            return rslt;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
