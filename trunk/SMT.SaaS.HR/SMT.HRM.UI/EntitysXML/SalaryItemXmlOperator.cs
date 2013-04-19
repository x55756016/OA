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
using System.Reflection;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using SMT.Saas.Tools.SalaryWS;
using System.IO;


namespace SMT.HRM.UI
{
    public class SalaryItemXmlOperator
    {
        public static List<T_HR_SALARYITEM> GetSalaryItemXML()
        {
            List<T_HR_SALARYITEM> result = new List<T_HR_SALARYITEM>();
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SMT.HRM.UI.EntitysXML.SalaryItemXML.xml");  
            XElement xelement = XElement.Load(stream);
            var xmls = from ent in xelement.Elements()
                       select ent;
            foreach (var ent in xmls)
            {
                T_HR_SALARYITEM salaryitem = new T_HR_SALARYITEM();
                Type type = salaryitem.GetType();
                PropertyInfo[] typePro = type.GetProperties();
                foreach (var en in ent.Nodes())
                {
                    foreach (PropertyInfo prop in typePro)
                    {
                        if (prop.PropertyType.BaseType == typeof(EntityReference) || prop.PropertyType.BaseType == typeof(RelatedEnd))
                            continue;
                        try
                        {
                            if ((en as XElement).Name == prop.Name)
                            {
                                if (prop.Name.IndexOf("GUERDONSUM")!=-1)
                                    prop.SetValue(salaryitem, Convert.ToDecimal((en as XElement).Value), null);                                
                                else
                                    prop.SetValue(salaryitem, (en as XElement).Value, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Message.ToString();
                        }
                    }
                }
                if (salaryitem != null) result.Add(salaryitem);
            }
            return result;
        }
    }
}
