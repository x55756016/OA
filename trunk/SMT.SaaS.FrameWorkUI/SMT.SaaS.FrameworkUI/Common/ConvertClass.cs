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

namespace SMT.SaaS.FrameworkUI.Common
{
    public class ConvertClass
    {

    }
    #region 内容格式化
    /// <summary>
    /// 显示长度如果超过20个字符 则改为……
    /// </summary>
    public class ConverterContentToFormatString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            if (value.ToString().Length > 20)
            {
                StrReturn = value.ToString().Substring(0, 20) + "......";
            }
            else
            {
                StrReturn = value.ToString();
            }
            return StrReturn;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            //return System.Convert.ToDateTime(value).ToString();
            return value.ToString();

        }
    }

    #endregion
}
