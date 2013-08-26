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
using System.Collections.ObjectModel;
using SMT.Workflow.Platform.Designer.Form;
using SMT.Workflow.Platform.Designer.PlatformService;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace SMT.Workflow.Platform.Designer.Class
{
   /// <summary>
    /// 功能参数操作
    /// </summary>
    public static class ParamOperate
    {

        public static ObservableCollection<Param> ListParam;
        public static ObservableCollection<Param> Init()
        {
            ListParam = new ObservableCollection<Param>();
            return ListParam;
        }
        public static void AddParam(Param param)
        {
            ListParam.Add(param);
        }
        public static void Remove(Param param)
        {
            ListParam.Remove(param);
            //if (ListParam != null)
            //{
            //    for (int i = 0; i < ListParam.Count();i++)
            //    {
            //        if (ListParam[i].FieldName == param.FieldName)
            //        {
            //            ListParam.RemoveAt(i);
            //        }
            //    }
            //}
        }
        public static string CollectionToString(ObservableCollection<Param> paramCollection)
        {
            if (paramCollection == null || paramCollection.Count == 0)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (Param param in paramCollection)
            {
                sb.Append("<Para TableName=\"" + param.TableName + "\"  Name=\"" + param.ParamID + "\"  Description=\"" + param.ParamName + "\" Value=\"{" + param.FieldID + "}\" ValueName=\"" + param.FieldName + "\" ></Para>" + "\r\n");
            }
            return sb.ToString();
        }
        public static ObservableCollection<Param> FieldToCollection(string strXml)
        {
            string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
                            "<Paras>" + "\r\n" +
                            "{0}" +
                                     "</Paras>";
            XmlTemplete = string.Format(XmlTemplete, strXml);
            Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(XmlTemplete);
            XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));

            var Param = from item in xele.Descendants("Para")
                        select item;
            ObservableCollection<Param> List = new ObservableCollection<Param>();
            foreach (var vv in Param)
            {
                Param r = new Param();
                r.FieldID = vv.Attribute("Value").Value.CvtString().Replace("{", "").Replace("}", "");
                try
                {
                    r.FieldName = vv.Attribute("ValueName").Value.CvtString();
                }
                catch
                {
                    r.FieldName = string.Empty;
                }
                r.Description = vv.Attribute("Description").Value.CvtString();
                r.TableName = vv.Attribute("TableName").Value.CvtString();
                r.ParamID = vv.Attribute("Name").Value.CvtString();
                r.ParamName = vv.Attribute("Description").Value.CvtString();
                List.Add(r);
            }
            return List;
        }
        /// <summary>
        /// 转换成字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string CvtString(this object obj)
        {
            if (obj == null) return "";
            try
            {
                return obj.ToString();
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="List"></param>
        /// <param name="IsSave"></param>
        /// <param name="strMsgBody"></param>
        /// <returns></returns>
        public static string MessageBodyExchange(List<TableColumn> List, bool IsSave, string strMsgBody)
        {
            //保存时消息内容汉字转换成具体字段
            if (IsSave)
            {
                foreach (TableColumn column in List)
                {
                    string str = "{new:" + column.Description + "}";
                    string strNew = "{new:" + column.FieldName + "}";
                    strMsgBody = strMsgBody.Replace(str, strNew);
                }
            }
            else
            {
                foreach (TableColumn column in List)
                {
                    string str = "{new:" + column.Description + "}";
                    string strNew = "{new:" + column.FieldName + "}";
                    strMsgBody = strMsgBody.Replace(strNew, str);
                }
            }
            return strMsgBody;
        }
    }
    public class CycleOperate
    {
        public static decimal CycleExchangeTo(string strCycleName)
        {
            switch (strCycleName)
            {
                case "一次":
                    return 0;
                case "分钟":
                    return 1;

                case "小时":
                    return 2;

                case "天":
                    return 3;

                case "月":
                    return 4;

                case "年":
                    return 5;
                default:
                    return 6;
            }
        }
        public static string CycleExchangeFrom(decimal strCycleName)
        {
            switch (strCycleName.ToString())
            {
                case "0":
                    return "一次";
                case "1":
                    return "分钟";

                case "2":
                    return "小时";

                case "3":
                    return "天";

                case "4":
                    return "月";

                case "5":
                    return "年";
                default:
                    return "";
            }
        }
    }
}
