using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using SMT.Foundation.Log;


namespace SMT.SaaS.PublicInterface.BLL
{
    public class BusinessObject
    {
         public string GetBusinessObject(string SystemCode, string BusinessObjectName)
        {
            string text = string.Empty;
            try
            {
                if (SystemCode == null || SystemCode == "" || BusinessObjectName == null || BusinessObjectName == "")
                    return null;
                var Path = ConfigurationManager.AppSettings["BOPath"];
                var BusinessObjectFolder = GetBusinessObjectFolder(Path, SystemCode);

                if (BusinessObjectFolder == null || BusinessObjectFolder == "")
                    return null;
                Path = Path + "\\" + BusinessObjectFolder + "\\" + BusinessObjectName + ".xml";
                Tracer.Debug("GetBusinessObject,filePath" + Path);
                if (!File.Exists(Path))
                    return null;
                StreamReader file = new StreamReader(Path);
                text = file.ReadToEnd();
                file.Close();
                file.Dispose();
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
           return text;
        }

         string GetBusinessObjectFolder(string Path,string SystemCode)
         {

             Path = Path + "\\BOSystemList.xml";
             if (!File.Exists(Path))
                 return null;
             XDocument doc = XDocument.Load(Path);

             IEnumerable<XElement> childList =
             from el in doc.Root.Elements("System")
             where el.Attribute("Name").Value == SystemCode
             select el;
             if (childList.Count() == 0)
                 return null;
             
            return  childList.FirstOrDefault().Attribute("ObjectFolder").Value;
         }
    }
}
