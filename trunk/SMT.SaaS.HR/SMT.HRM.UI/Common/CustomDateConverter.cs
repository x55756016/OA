/*
 * 文件名：FreeLeaveDaySet.xaml.cs
 * 作  用：日期时间格式化
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

using System.Windows.Data;

namespace SMT.HRM.UI
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

            object retValue = null ;
            switch (parameter.ToString())
            {
                case "DATE":    //日期                    
                    retValue = GetDate(value);
                    break;
                case "TIME":    //小时：分
                    retValue = GetHourMin(value);
                    break;
                case "DATETIME":    //日期 小时：分
                    retValue = GetDateTime(value);
                    break;
                case "DATETIMES":    //日期 小时：分：秒
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

            return dt.ToString("yyyy-MM-dd");
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

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
}
