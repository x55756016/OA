/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：文件注释说明.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/9/16 9:25:36   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Platform.Designer 
	 * 描　　述： 规范说明文件的注释
	 * 模块名称：工作流设计器
* ---------------------------------------------------------------------*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using SMT.Workflow.Platform.Designer.PlatformService;

namespace SMT.Workflow.Platform.Designer.Utils
{
    public static class XmlUtils
    {
        /// <summary>
        /// Model转化为XML的方法
        /// </summary>
        /// <param name="model">要转化的Model</param>
        /// <returns></returns>
        public static string ModelToXML(object model)
        {
            string xml = string.Empty;
            if (model != null)
            {
                XElement xeNode = new XElement(model.GetType().Name);
                foreach (PropertyInfo property in model.GetType().GetProperties())
                {
                    XElement xeProperty = null;
                    if (property.GetValue(model, null) != null)
                        xeProperty = new XElement(property.Name, property.GetValue(model, null).ToString());
                    else
                        xeProperty = new XElement(property.Name, "[Null]");

                    xeNode.Add(xeProperty);
                }

                xml = xeNode.ToString();
            }

            return xml;
        }

        /// <summary>
        /// XML转化为Model的方法
        /// </summary>
        /// <param name="xml">要转化的XML</param>
        /// <param name="SampleModel">Model的实体示例，New一个出来即可</param>
        /// <returns></returns>
        public static void XMLToModel(string xmlString, object model)
        {
            if (string.IsNullOrEmpty(xmlString) || model == null)
                return;
            else
            {
                if (string.IsNullOrEmpty(xmlString)) return;
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(xmlString);
                XElement xElement = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));

                foreach (XElement xe in xElement.Descendants())
                {
                    foreach (PropertyInfo property in model.GetType().GetProperties())
                    {
                        if (xe.Name.ToString().ToUpper() == property.Name.ToUpper())
                        {
                            if (xe.Value != "[Null]")
                            {
                                if (property.PropertyType == typeof(System.Guid))
                                    property.SetValue(model, new Guid(xe.Value), null);
                                else
                                {
                                    property.SetValue(model, Convert.ChangeType(xe.Value, property.PropertyType, null), null);
                                }
                            }
                            else
                                property.SetValue(xe.Value, null, null);
                        }
                    }
                }
            }

        }


        public static string GetSystemPath(string systemCode)
        {
            return systemCode + "BO";
        }


    }
}
