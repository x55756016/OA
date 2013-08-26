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


namespace SMT.Workflow.Platform.Designer.Class.Converter
{

    public class DictionaryConverter : IValueConverter
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
}
