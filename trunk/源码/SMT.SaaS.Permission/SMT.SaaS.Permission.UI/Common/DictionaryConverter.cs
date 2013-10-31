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

using System.Collections;
using System.Collections.Generic;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.OrganizationWS;
using System.Linq;
using SMT.Saas.Tools.FlowDesignerWS;

namespace SMT.SaaS.Permission.UI
{
    #region 字典转换
        
    public class DictionaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return value;

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
    #endregion

    #region 公司转换


    public class CompanyInfoConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (parameter == null)
                return value;
            switch (parameter.ToString())
            {
                case "Company":
                    List<T_HR_COMPANY> dictc = Application.Current.Resources["SYS_CompanyInfo"] as List<T_HR_COMPANY>;
                    if (dictc == null)
                        return value;
                    var objc = from a in dictc
                               where a.COMPANYID == value.ToString()
                               select a.CNAME;
                    return objc.Count() > 0 ? objc.FirstOrDefault() : value;
                case "Department":
                    List<T_HR_DEPARTMENT> dictd = Application.Current.Resources["SYS_DepartmentInfo"] as List<T_HR_DEPARTMENT>;
                    if (dictd == null)
                        return value;
                    var objd = from a in dictd
                               where a.DEPARTMENTID == value.ToString()
                               select a.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    return objd.Count() > 0 ? objd.FirstOrDefault() : value;
                case "Post":
                    List<T_HR_POST> dictp = Application.Current.Resources["SYS_PostInfo"] as List<T_HR_POST>;
                    if (dictp == null)
                        return value;
                    var objp = from a in dictp
                               where a.POSTID == value.ToString()
                               select a.T_HR_POSTDICTIONARY.POSTNAME;
                    return objp.Count() > 0 ? objp.FirstOrDefault() : value;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    #endregion

    #region 组织架构转换
    
    
    /// 组织架构类型转换
    /// </summary>
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
    #endregion
    
    #region 审核状态转换    
   
    public class CheckStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
    }
    #endregion

    #region 模块名称
    
    
    public class ModuleNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            List<FLOW_MODELDEFINE_T> dicts = Application.Current.Resources["FLOW_MODELDEFINE_T"] as List<FLOW_MODELDEFINE_T>;
            if (dicts == null)
                return value;

            var objs = from a in dicts
                       where a.MODELCODE.ToString() == value.ToString()
                       select a;

            FLOW_MODELDEFINE_T dict = objs.Count() > 0 ? objs.FirstOrDefault() : null;
            return dict == null ? value : dict.DESCRIPTION;//模块描述
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    #endregion

    #region 时间转换

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
    #endregion

    #region 员工状态
    public class ConverterUserSate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "0":
                    StrReturn = "禁用";
                    break;
                case "1":
                    StrReturn = "正常";
                    break;                
            }
            return StrReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string StrReturn = "";
            switch (value.ToString())
            {
                case "禁用":
                    StrReturn = "0";
                    break;
                case "正常":
                    StrReturn = "1";
                    break;                
            }
            return StrReturn;

        }
    }
    #endregion

}
