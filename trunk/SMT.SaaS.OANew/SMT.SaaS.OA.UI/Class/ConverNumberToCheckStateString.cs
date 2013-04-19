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
    public class ConverNumberToCheckStateString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.ToString() == "0")
                {
                    return "未提交";
                }
                else if (value.ToString() == "1")
                {
                    return "审核通过";
                }
                else if (value.ToString() == "2")
                {
                    return "审核中";
                }
                else if (value.ToString() == "3")
                {
                    return "审核未通过";
                }
                else if (value.ToString() == "4")
                {
                    return "待审核";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int retValue = 5;
            if (value != null)
            {
                
                switch (value.ToString())
                {
                    case "未提交":
                        retValue = 0;
                        break;
                    case "审核通过":
                        retValue = 1;
                        break;
                    case "审核中":
                        retValue = 2;
                        break;
                    case "审核未通过":
                        retValue = 3;
                        break;
                    case "待审核":
                        retValue = 4;
                        break;
                }
            }
            return retValue;
        }
    }
}