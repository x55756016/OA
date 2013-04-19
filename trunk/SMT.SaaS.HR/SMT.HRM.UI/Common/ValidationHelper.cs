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

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Data;
using SMT.Saas.Tools.OrganizationWS;

namespace SMT.HRM.UI
{
    public class ValidationHelper
    {

        public static List<string> ValidateProperty<T>(T obj) where T : class
        {
            Type type = typeof(T);
            List<string> errors = new List<string>();

            PropertyInfo[] infos = type.GetProperties();
            
            for (int i = 0; i < infos.Length; i++)
            {
                PropertyInfo pinfo = infos[i];

                System.ComponentModel.DataAnnotations.ValidationContext context
          = new System.ComponentModel.DataAnnotations.ValidationContext(obj, null, null) { MemberName = pinfo.Name };
                object value = pinfo.GetValue(obj, null);
                
                try
                {
                    System.ComponentModel.DataAnnotations.Validator.ValidateProperty(value, context);
                }
                catch (System.ComponentModel.DataAnnotations.ValidationException ve)
                {
                    //MethodInfo mi = type.GetMethod("RaisePropertyChanged");
                    //object[] para = new object[] { pinfo.Name };
                    //mi.Invoke(obj, para);                    
                    errors.Add(ve.Message);
                }
               
            }
            return errors;
        }

    }
}
