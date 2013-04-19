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


namespace SMT.SaaS.OA.UI
{
    public class ConvertLanguage:IValueConverter
    {
        private static readonly ResourceManager resourceManager =
            new ResourceManager("SMT.SaaS.OA.UI.Assets.Resources.OASystem", Assembly.GetExecutingAssembly());

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
            if (((string)parameter) == "BDAY")
            {
                string s = (string)parameter;
            }
            var reader = (ConvertLanguage)value;
            string rslt = reader.Get((string)parameter);
            return rslt;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
