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
    public class CustomDateConverter : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(value.ToString()))
            {
                return value;
            }

            if (parameter == null)
            {
                return null;
            }

            object retValue = null;
            switch (parameter.ToString())
            {
                case "DATE": //日期                    
                    retValue = GetDate(value);
                    break;
                case "TIME": //小时：分
                    retValue = GetHourMin(value);
                    break;
                case "DATETIME": //日期 小时：分
                    retValue = GetDateTime(value);
                    break;
                case "DATETIMES": //日期 小时：分：秒
                    retValue = GetDateTimes(value);
                    break;
                default:
                    retValue = value;
                    break;
            }
            return retValue;
        }

        /// <summary>
        /// 日期格式化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetDate(object value)
        {
            DateTime dt;
            DateTime.TryParse(value.ToString(), out dt);
            string d = dt.ToString("yyyy-MM-dd");
            if (dt.ToString("yyyy-MM-dd") != "0001-01-01")
            {
                return dt.ToString("yyyy-MM-dd");
            }
            else 
            {
                return null;
            }
        }

        /// <summary>
        /// 时间格式化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetHourMin(object value)
        {
            DateTime dtTemp = new DateTime();
            DateTime.TryParse(value.ToString(), out dtTemp);

            return dtTemp.ToString("HH:mm");
        }

        /// <summary>
        /// 日期与时间格式化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetDateTime(object value)
        {
            DateTime dt;
            DateTime.TryParse(value.ToString(), out dt);

            return dt.ToString("yyyy-MM-dd HH:mm");
        }

        /// <summary>
        /// 日期与时间格式化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetDateTimes(object value)
        {
            DateTime dt;
            DateTime.TryParse(value.ToString(), out dt);

            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            string strValue = value.ToString();
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            return value;
        }

        #endregion
    }

    public class ConverterDateToFormatString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime Dt = new DateTime();
            if (value != null && value != "")
            {
                Dt = (DateTime)value;
                return Dt.ToShortDateString() + " " + Dt.ToShortTimeString();
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDateTime(value).ToString();
        }
    }
}
