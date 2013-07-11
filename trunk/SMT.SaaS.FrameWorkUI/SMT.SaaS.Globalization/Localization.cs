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

namespace SMT.SaaS.Globalization
{
    public class Localization
    {
        public static string ResourceFullClassName = "SMT.SaaS.Globalization.Resource";
        public static string MessageResourceFullClassName = "SMT.SaaS.Globalization.MessageResource";
        public static string LMResourceClassName = "SMT.SaaS.Globalization.LMResource";
        public static string SMSResourceClassName = "SMT.SaaS.Globalization.SMSResource";
        public static string FormConfigResourceClassName = "SMT.SaaS.Globalization.FormsConfigResource";
    
        public static readonly ResourceManager ResourceMgr = new ResourceManager(ResourceFullClassName, Assembly.GetExecutingAssembly());
        public static readonly ResourceManager MessageResourceMgr = new ResourceManager(MessageResourceFullClassName, Assembly.GetExecutingAssembly());
        public static readonly ResourceManager LMResourceMgr = new ResourceManager(LMResourceClassName, Assembly.GetExecutingAssembly());
        public static readonly ResourceManager SMSResourceMgr = new ResourceManager(SMSResourceClassName, Assembly.GetExecutingAssembly());
        public static readonly ResourceManager FormConfigResourceMgr = new ResourceManager(FormConfigResourceClassName, Assembly.GetExecutingAssembly());

        private static CultureInfo uiCulture = Thread.CurrentThread.CurrentUICulture;
        public static CultureInfo UiCulture
        {
            get { return uiCulture; }
            set { uiCulture = value; }
        }

        public static string GetString(string key)
        {  
            string rslt = ResourceMgr.GetString(key);

            if (string.IsNullOrEmpty(rslt))
            {
                rslt = LMResourceMgr.GetString(key);
            }
            if (string.IsNullOrEmpty(rslt))
            {
                rslt = SMSResourceMgr.GetString(key);
            }            
            return string.IsNullOrEmpty(rslt) ? key : rslt;
        }
        public static string GetFormConfigString(string strKey)
        {
            string rslt = FormConfigResourceMgr.GetString(strKey);
            return string.IsNullOrEmpty(rslt) ? strKey : rslt; 
        }
        public static string GetString(string key,string parameter)
        {
            string rslt = GetString(key);
            string[] paras = parameter.ToString().Split(',');

            int i = 0;

            foreach (string str in paras)
            {
                string tmp = GetString(str);
                paras[i] = string.IsNullOrEmpty(tmp) ? str : tmp;
                i++;
            }
            rslt = string.Format(rslt, paras);

            return string.IsNullOrEmpty(rslt) ? key : rslt;
        }
    }
}
