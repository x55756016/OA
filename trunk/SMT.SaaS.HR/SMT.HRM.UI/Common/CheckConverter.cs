/*
 * 文件名：CheckConverter.xaml.cs
 * 作  用：CheckBox勾选值转化
 * 创建人：吴鹏
 * 创建时间：2010年2月23日, 14:26:11
 * 修改人：
 * 修改时间：
 */

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

namespace SMT.HRM.UI
{
    public class CheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            object objRes = null;

            switch (parameter.ToString())
            {
                case "CN":
                    objRes = value.ToString() == "0" ? "否" : "是";
                    break;
                case "EN":
                    objRes = FormatValueAsEN(value);
                    break;
                default:
                    objRes = value.ToString() == "0" ? false : true;
                    break;
            }

            return objRes;
        }

        private object FormatValueAsEN(object value)
        {
            string strTemp = value.ToString();
            switch (strTemp.ToUpper())
            {
                case "FALSE":
                    strTemp = "False";
                    break;
                case "TRUE":
                    strTemp = "True";
                    break;
                case "0":
                    strTemp = "False";
                    break;
                case "1":
                    strTemp = "True";
                    break;
            }

            return strTemp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
