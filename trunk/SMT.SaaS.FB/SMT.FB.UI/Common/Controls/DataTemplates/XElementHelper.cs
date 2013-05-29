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
using System.Collections.Generic;
using System.Text;

namespace SMT.FB.UI.Common.Controls.DataTemplates
{
    public class XElementHelper
    {

    }
    public class XObjectString
    {
        public XObjectString(string name)
            : this()
        {
            this.Name = name;
        }
        public XObjectString()
        {
            XObjectType = this.GetType().Name;
        }
        public string XObjectType { get; set; }
        public string Name { get; set; }
    }
    public class XAttributeString : XObjectString
    {
        public XAttributeString()
            : base()
        {
        }
        public XAttributeString(string name, string value)
            : base(name)
        {
            this.Value = "'" + value + "'";
        }
        public string Value { get; set; }
    }
    public class XElementString : XObjectString
    {
        public XElementString()
            : base()
        {
            XElementStrings = new List<XElementString>();
            XAttributeStrings = new List<XAttributeString>();
        }
        public XElementString(string name)
            : this()
        {
            this.Name = name;
        }
        public XElementString(string name, params XObjectString[] objectList)
            : this(name)
        {
            if (objectList != null)
            {
                for (int i = 0; i < objectList.Length; i++)
                {
                    if (objectList[i].GetType() == typeof(XElementString))
                    {
                        Add(objectList[i] as XElementString);
                    }
                    else
                    {
                        Add(objectList[i] as XAttributeString);
                    }
                }
            }
        }

        public void Add(XElementString xObjectString)
        {
            XElementStrings.Add(xObjectString);
        }
        public void Add(XAttributeString xObjectString)
        {
            XAttributeStrings.Add(xObjectString);
        }
        public List<XElementString> XElementStrings { get; set; }
        public List<XAttributeString> XAttributeStrings { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<");
            sb.Append(this.Name);
            XAttributeStrings.ForEach(attr =>
            {
                sb.AppendFormat(" {0}={1}", attr.Name, attr.Value);
            });
            sb.Append(">");
            XElementStrings.ForEach(element =>
            {
                sb.Append(element.ToString());
            });
            sb.Append("</" + this.Name + ">");
            return sb.ToString();
        }
    }
}
