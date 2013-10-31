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
    public class DescriptionViewerConverter : IValueConverter
    {
        public static readonly ResourceManager ResourceMgr = new ResourceManager("SMT.SaaS.Permission.UI.Resources.Resource",
                Assembly.GetExecutingAssembly());

        private static CultureInfo uiCulture = Thread.CurrentThread.CurrentUICulture;
        public static CultureInfo UiCulture
        {
            get { return uiCulture; }
            set { uiCulture = value; }
        }

        public static Panel ParentLayoutRoot
        {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string rslt = ResourceMgr.GetString((string)value);

            return string.IsNullOrEmpty(rslt) ? value : rslt;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
