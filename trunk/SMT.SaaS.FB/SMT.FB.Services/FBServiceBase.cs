using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.FB.DAL;
using System.Threading;
using SMT.FB.BLL;
using System.Xml.Linq;
using System.Xml;
using System.Configuration;
using SMT.Foundation.Log;

namespace SMT.FB.Services
{
    public class FBServiceBase
    {
        static FBServiceBase()
        {
            Init();
        }
        public string threadName = null;
        public FBServiceBase()
        {
            //threadName = DBContext.ManualRegister();
            // SMT.Foundation.Log.Tracer.Warn("ManualRegister" + threadName);
        }

        ~FBServiceBase()
        {
            //DBContext.ManualUnRegister(threadName);
           // SMT.Foundation.Log.Tracer.Warn("ManualUnRegister" + threadName);

        }
        
        #region 1.	初始化数据，从FB_UI_Config.xml获取实体集合， 从XML/*.xml中获取用于流程系统的XML数据模版

        private static void Init()
        {
            try
            {
                // 初始化OrderBLL
                SystemBLL.InitOrderBLL();
                List<EntityInfo> listResult = new List<EntityInfo>();
                List<XElement> listEntityXml = new List<XElement>();
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                //  string path = ConfigurationManager.AppSettings["UIXml"];
                string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

                doc.Load(path + "\\FB_UI_Config.xml");
                XmlNodeList list = doc.SelectNodes("FB/Orders/Order");
                foreach (XmlNode node in list)
                {

                    XElement xOrder = GetXOrder(node);
                    listEntityXml.Add(xOrder);

                    XmlNode nodeEntity = node.SelectSingleNode("OrderEntity/Entity");
                    if (nodeEntity != null)
                    {
                        EntityInfo eInfo = new EntityInfo();
                        eInfo.EntityCode = node.Attributes["Type"].Value;
                        eInfo.Type = nodeEntity.Attributes["Type"].Value;
                        eInfo.KeyName = nodeEntity.Attributes["KeyName"].Value;
                        if (nodeEntity.Attributes["CodeName"] != null)
                        {
                            eInfo.CodeName = nodeEntity.Attributes["CodeName"].Value;
                        }
                        listResult.Add(eInfo);
                    }
                }
                FBCommonBLL.FBCommonEntityList = listResult;

                FBCommonBLL.FBServiceUrl = ConfigurationManager.AppSettings["FBServiceUrl"];
                SystemBLL.DebugMode = ConfigurationManager.AppSettings["FBDebugMode"] == "True" ? true : false;
                #region beyond
                Dictionary<string, XElement> dictXE = new Dictionary<string, XElement>();
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"xml\");
                dirInfo.GetFiles().ToList().ForEach(item =>
                {
                    XElement xeroot = XElement.Load(item.FullName);
                    XElement xe = xeroot;
                    dictXE.Add(item.Name.ToLower().Replace(".xml", ""), xe);

                });
                AuditBLL.dictXML_XE = dictXE;

                #endregion
            }
            catch (Exception ex)
            {

                Tracer.Debug("系统初始化失败" + ex.ToString());
            }
        }

        private static XElement GetXOrder(XmlNode node)
        {
            string name = node.Attributes["Type"].Value;
            string desc = node.Attributes["Name"].Value;
            XElement xOrder = new XElement("Object", new XAttribute("Name", name), new XAttribute("Description", desc));

            XmlNodeList panels = node.SelectNodes("Form/DataPanel");
            if (panels == null)
            {
                return xOrder;
            }
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i].Attributes["Type"].Value == "DataPanelTwoSide")
                {
                    XmlNodeList listleft = panels[i].SelectNodes("LeftPanel/DataFieldItem");

                    XmlNodeList listRight = panels[i].SelectNodes("RightPanel/DataFieldItem");
                    List<XElement> listLeftX = GetXList(listleft);
                    List<XElement> listRightX = GetXList(listRight);
                    xOrder.Add(listLeftX.ToArray());
                    xOrder.Add(listRightX.ToArray());

                }
                else if (panels[i].Attributes["Type"].Value == "DataPanel")
                {
                    XmlNodeList list = panels[i].SelectNodes("DataFieldItem");

                    xOrder.Add(GetXList(list));
                }
            }
            return xOrder;
        }

        public static List<XElement> GetXList(XmlNodeList list)
        {

            List<XElement> listX = new List<XElement>();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {

                    string a = list[i].Attributes["PropertyDisplayName"].Value;
                    string b = list[i].Attributes["PropertyName"].Value;

                    XElement tempX = new XElement("Attribute", new XAttribute("Name", b), new XAttribute("Description", a),
                        new XAttribute("DataType", "NVARCHAR2"), new XAttribute("DataValue", ""));
                    listX.Add(tempX);
                }
            }

            return listX;
        }

        #endregion
    }
}