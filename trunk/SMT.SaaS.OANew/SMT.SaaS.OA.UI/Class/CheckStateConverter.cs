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
using System.Globalization;

namespace SMT.SaaS.OA.UI
{
    public class CheckStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {

                    switch (value.ToString().Trim())
                    {
                        case "0":
                            return "未提交";
                        case "1":
                            return "审核中";
                        case "2":
                            return "审核通过";
                        case "3":
                            return "审核未通过";
                        default:
                            return "未提交";
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
                //   5为所有
                if (value != null)
                {

                    switch (value.ToString().Trim())
                    {
                        case "未提交":
                            return "0";
                        case "审核中":
                            return "1";
                        case "审核通过":
                            return "2";
                        case "审核未通过":
                            return "3";
                        default:
                            return "0";
                    }
                }
                else
                {
                    return "5";
                }
            }
            catch
            {
                return "5";
            }
        }
    }

    /// <summary>
    /// 内容长度截取
    /// </summary>
    public class ConverterContentToFormatString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string StrReturn = "";
                if (value != null)
                {
                    if (value.ToString().Length > 40)
                    {
                        StrReturn = value.ToString().Substring(0, 40) + "......";
                    }
                    else
                    {
                        StrReturn = value.ToString();
                    }
                }
                return StrReturn;
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
                    return value.ToString();
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
    }

}
