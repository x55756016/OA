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
using SMT.Saas.Tools.PermissionWS;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;

namespace SMT.SaaS.OA.UI
{
    public class DictionaryConverter : IValueConverter
    {
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();

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
    public class WayToRemind : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch (value.ToString().Trim())
                {
                    case "NOTHING":
                        return "一次提醒";
                    case "DAY":
                        return "每天提醒";
                    case "WEEK":
                        return "每周提醒";
                    case "MONTH":
                        return "每月提醒";
                    case "YEAR":
                        return "每年提醒";
                    default:
                        return "一次提醒";
                }
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch (value.ToString().Trim())
                {
                    case "一次提醒":
                        return "NOTHING";
                    case "每天提醒":
                        return "DAY";
                    case "每周提醒":
                        return "WEEK";
                    case "每月提醒":
                        return "MONTH";
                    case "每年提醒":
                        return "YEAR";
                    default:
                        return "NOTHING";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
