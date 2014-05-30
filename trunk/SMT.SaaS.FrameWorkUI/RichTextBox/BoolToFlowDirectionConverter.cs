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
using System.Resources;
using System.Threading;

namespace SMT.SaaS.FrameworkUI.RichNotepad
{
    public class BoolToFlowDirectionConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = bool.Parse(value.ToString());

            if (targetType == typeof(string))
            {
                ResourceManager resourceManager = new ResourceManager("RichNotepad.Strings", GetType().Assembly);

                return (string)resourceManager.GetString("txt_Header_Direction", Thread.CurrentThread.CurrentUICulture);
            }
            else
            {
                if (val)
                    return FlowDirection.RightToLeft;
                else
                    return FlowDirection.LeftToRight;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FlowDirection fd = (FlowDirection)Enum.Parse(typeof(FlowDirection), value.ToString(), true);

            if (fd == FlowDirection.RightToLeft)
                return true;
            else
                return false;

        }

        #endregion
    }
}