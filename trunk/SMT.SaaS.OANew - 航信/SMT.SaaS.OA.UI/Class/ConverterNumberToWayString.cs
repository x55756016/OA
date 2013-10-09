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
    /// <summary>
    /// 状态转换 0  匿名   1  实名
    /// </summary>
    public class ConverterNumberToWayString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {

                    if (value.ToString() == "0")
                    {
                        return "匿名";
                    }
                    else
                    {
                        return "实名";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {

                    if (value.ToString() == "匿名")
                    {
                        return "0";
                    }
                    else
                    {
                        return "1";
                    }
                }
                else
                {
                    return "0";
                }
            }
            catch
            {
                return "0";
            }
        }
    }
}
