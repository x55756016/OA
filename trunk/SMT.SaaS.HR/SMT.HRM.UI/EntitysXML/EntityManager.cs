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
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SMT.HRM.UI
{
    public class EntityManager
    {
        public static List<string> GetAllEntityName()
        {
          //WebClient client = new WebClient(); 
          //client.DownloadStringAsync(new Uri(HtmlPage.Document.DocumentUri, "projects.xml")); 
          //client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted); 
          //} 
            XElement xele = XElement.Load("Entity/SMT_HRM_EFModel.xml");
            var qNames = from ent in xele.Elements()
                         select ent;

            var q = from ent in qNames                    
                    select ent.Attribute("Name").Value;
            return q.ToList();
        }

        public static List<string> GetEntityPropertyByName(string EntityName)
        {

            XElement xele = XElement.Load("Entity/SMT_HRM_EFModel.xml");
            var qNames = from ent in xele.Elements()
                         select ent;

            var q = from ent in qNames
                    where ent.Attribute("Name").Value == EntityName
                    select ent;


            var eType = from c in q.Descendants()                       
                        select c;

            var prolist = from ent in eType
                          where ent.Name.LocalName == "Property"
                          select ent.Attribute("Name").Value;

            return prolist.ToList();


        }
    }
}
