using System;
using System.Windows.Data;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;
using SMT.Saas.Tools.PermissionWS;
using System.Globalization;
using System.Linq;

namespace SMT.SAAS.Platform.WebParts.Converters
{
    /// <summary>
    /// 将表示时间的字符串进行格式化
    /// </summary>
    public class DataFormatConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string _temp = value.ToString();
                string formatStr = "yyyy年MM月dd日HH:mm";
                if (parameter != null)
                    formatStr = parameter.ToString();

                return DateTime.Parse(_temp).ToString(formatStr);
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
    #region 平台时间转换
    public class OAWebPartDateTimeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string _temp = value.ToString();
                return DateTime.Parse(_temp).ToString("yyyy年MM月dd日HH:mm");
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
    #endregion
    /// <summary>
    /// 将表示时间的字符串进行格式化
    /// </summary>
    public class PendTaskConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value.ToString()=="0")
                {
                    return "剩余1分钟";
                }
                string _temp = value.ToString();
                string heard = _temp.Substring(0, 1);
                string last = _temp.Substring(_temp.Length - 1, 1);

                string time = _temp.Substring(1, _temp.Length - 2);
                StringBuilder bulid = new StringBuilder();
                bulid.Append((heard == "-" ? "超时" : "剩余"));
                bulid.Append(time);
                bulid.Append((last == "H" ? "小时" : "分钟"));

                return bulid.ToString();
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class ColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string _temp = value.ToString();
                string heard = _temp.Substring(0, 1);

                int time = Int32.Parse(_temp.Substring(1, _temp.Length - 2));

                Color color = Colors.Black;
                if (heard == "-")
                    color = Colors.Red;
                if (heard == "+" && (time <= 3))
                    color = Colors.Blue;

                SolidColorBrush b = new SolidColorBrush(color);
                return b;
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// 新闻类别转换
    /// </summary>
    public class NewsTypeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string vl = value.ToString();
                switch (vl)
                {
                    case "0": return "新闻";
                    case "1": return "动态";
                    case "2": return "公告";
                    case "3": return "通知";
                    default: return "新闻";

                }
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// 新闻状态转换
    /// </summary>
    public class NewsStateConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string vl = value.ToString();

                switch (vl)
                {
                    case "0": return "未发布";
                    case "1": return "已发布";
                    case "2": return "已关闭";
                    default: return "未发布";

                }
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }


    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool _value = (bool)value;
                return _value ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }


    /// <summary>
    /// 字典转换
    /// </summary>
    public class DictionaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            if (dicts == null)
                return value;

            var objs = from a in dicts
                       where a.DICTIONARYVALUE.ToString() == value.ToString() && a.DICTIONCATEGORY == parameter.ToString()
                       select a;

            T_SYS_DICTIONARY dict = objs.Count() > 0 ? objs.FirstOrDefault() : null;
            return dict == null ? value : dict.DICTIONARYNAME;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// 菜单转换
    /// </summary>
    public class MenusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            List<V_EntityMenu> dicts = Application.Current.Resources["SYS_EntityMenus"] as List<V_EntityMenu>;
            if (dicts == null)
                return value;

            var objs = from a in dicts
                       where a.MENUCODE == value.ToString()
                       select a;

            V_EntityMenu dict = objs.Count() > 0 ? objs.FirstOrDefault() : null;
            return dict == null ? value : dict.MENUNAME;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
