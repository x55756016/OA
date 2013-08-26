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

namespace SMT.Workflow.Platform.Designer.Class.Converter
{
    public class LendFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().Trim())
            {
                case "0":
                    return "未借出";
                case "1":
                    return "已借出";
                default:
                    return "未借出";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().Trim())
            {
                case "未借出":
                    return "0";
                case "已借出":
                    return "1";
                default:
                    return "0";
            }
        }
    }

    public class ReturnFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().Trim())
            {
                case "0":
                    return "未归还";
                case "1":
                    return "已归还";
                default:
                    return "未归还";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().Trim())
            {
                case "未归还":
                    return "0";
                case "已归还":
                    return "1";
                default:
                    return "0";
            }
        }
    }

    public class RentFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().Trim())
            {
                case "0":
                    return "未出租";
                case "1":
                    return "已出租";
                default:
                    return "未出租";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().Trim())
            {
                case "未出租":
                    return "0";
                case "已出租":
                    return "1";
                default:
                    return "0";
            }
        }
    }

    public class ObjectTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString().Trim())
            {
                case "Company":
                    return "公司";
                case "Department":
                    return "部门";
                case "Post":
                    return "岗位";
                case "Personel":
                    return "个人";
                default:
                    return "个人";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
