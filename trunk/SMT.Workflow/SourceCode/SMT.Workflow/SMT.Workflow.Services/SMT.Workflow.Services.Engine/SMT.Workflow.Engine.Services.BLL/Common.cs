/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：Common.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/21 15:10:40   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services.BLL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Linq;
using System.IO;
using SMT.Workflow.Common.DataAccess;

namespace SMT.Workflow.Engine.Services.BLL
{
    public static class Common
    {
         
        /// <summary>
        /// 将业务数据字段，组合成以Ё分割的字符串
        /// </summary>
        /// <param name="xe"></param>
        /// <returns></returns>
        public static string ConbinString(XElement xe)
        {
            StringBuilder sb = new StringBuilder();
            var v = from item in xe.Descendants("Message").Descendants("Attribute")
                    select item;
            if (v.Count() > 0)
            {
                foreach (var vv in v)
                {
                    string Name = vv.Attribute("Name").Value;
                    string Value = vv.Attribute("DataValue").Value;
                    sb.Append(Name + "|" + Value + "Ё");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 元数据替换 DataValue 适用于替换 Guid
        /// </summary>
        /// <param name="PorcessString"></param>
        /// <param name="SourceValueDT"></param>
        /// <returns></returns>
        public static string ReplaceValue(string PorcessString, DataTable SourceValueDT)
        {
            foreach (DataRow dr in SourceValueDT.Rows)
            {
                PorcessString = PorcessString.Replace("{" + dr["ColumnName"].ToString() + "}", dr["ColumnValue"].ToString());
            }
            return PorcessString;
        }
        public static void ReplaceUrl(ref string strContent, ref string strUrl, DataTable dtValue)
        {
            if (dtValue != null && dtValue.Rows.Count > 0)
            {
                foreach (DataRow dr in dtValue.Rows)
                {
                    strContent = strContent.Replace(string.Concat("{" + dr["ColumnName"].ToString().ToUpper() + "}"), string.Concat(dr["ColumnText"]));
                    strUrl = strUrl.Replace("{" + string.Concat(dr["ColumnName"].ToString().ToUpper()) + "}", string.Concat(dr["ColumnValue"]));
                }
                string replaceUrl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                              "<System>" +
                              "{0}" +
                              "</System>";
                strUrl = string.Format(replaceUrl, strUrl);
            }
        }
        /// <summary>
        /// 特殊模块需要改变接收人
        /// </summary>
        /// <returns></returns>
        public static bool CheckModelName(string strModelName)
        {
            if (strModelName == "T_HR_EMPLOYEECHECK" ||
                strModelName == "T_HR_EMPLOYEECONTRACT" ||
                strModelName == "T_HR_EMPLOYEEENTRY" ||
                strModelName == "T_HR_EMPLOYEEINSURANCE" ||
                strModelName == "T_HR_EMPLOYEEPOSTCHANGE" ||
                strModelName == "T_HR_LEFTOFFICECONFIRM" ||
                strModelName == "T_HR_LEFTOFFICE" ||
                 strModelName == "T_HR_PENSIONMASTER" ||
                 strModelName == "T_HR_SALARYARCHIVE")
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 根据节点属性得到值（公共方法）
        /// </summary>
        /// <param name="xelement">XML节点</param>
        /// <param name="strAttrName">属性</param>
        /// <returns></returns>
        public static string XMLToAttribute(XElement xelement, string strAttrName)
        {
            try
            {
                string strReturn = (from item in xelement.Descendants("Message").Descendants("Attribute")
                                    where item.Attribute("Name").Value.ToUpper() == strAttrName.ToUpper()
                                    select item).FirstOrDefault().Attribute("DataValue").Value;
                return strReturn;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 将数据源字段转换成数据表
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static DataTable FieldStringToDataTable(string strAppValue, ref string TmpFieldValueString)
        {
            DataRow[] list;
            DataRow drvalue;
            DataTable valueTable = new DataTable();
            valueTable.Columns.Add("FieldType", typeof(string));
            valueTable.Columns.Add("ColumnName", typeof(string));
            valueTable.Columns.Add("ColumnValue", typeof(string));
            valueTable.Columns.Add("ColumnText", typeof(string));
            TmpFieldValueString = strAppValue;
            string[] valuerownode = TmpFieldValueString.Split('Ё');
            for (int j = 0; j < valuerownode.Length; j++)
            {
                if (valuerownode[j] != "")
                {
                    string[] valuecolnode = valuerownode[j].Split('|');
                    list = valueTable.Select("ColumnName='" + valuecolnode[0] + "'");
                    if (list.Length > 0)
                    {
                        drvalue = list[0];
                    }
                    else
                    {
                        drvalue = valueTable.NewRow();
                        valueTable.Rows.Add(drvalue);
                    }
                    drvalue["FieldType"] = "sys";
                    drvalue["ColumnName"] = valuecolnode[0];
                    string strValueText = valuecolnode[1];
                    string strValue = string.Empty;
                    string strText = string.Empty;
                    if (strValueText.IndexOf("Г") != -1)
                    {
                        strValue = strValueText.Split('Г')[0];
                        strText = strValueText.Split('Г')[1];
                    }
                    else
                    {
                        strValue = strValueText;
                    }
                    if (string.IsNullOrEmpty(strText))
                    {
                        strText = strValue;
                    }
                    drvalue["ColumnValue"] = strValue;
                    drvalue["ColumnText"] = strText;
                }
            }
            return valueTable;
        }

        #region 将业务数据XML转换成DataTable
        public static DataTable EncrytXmlToDataTable(string strXml)
        {
            try
            {
                DataTable table = new DataTable();
                table.Columns.Add("Name");
                table.Columns.Add("Value");
                table.Columns.Add("Text");
                if (!string.IsNullOrEmpty(strXml))
                {
                    StringBuilder sb = new StringBuilder();
                    Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strXml);
                    XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                    var attributes = from c in xele.Descendants("Attribute")
                                     select c;
                    if (attributes.Count() > 0)
                    {
                        foreach (var attr in attributes)
                        {
                            DataRow dr = table.NewRow();
                            dr["Name"] = attr.Attribute("Name").Value;
                            dr["Value"] = attr.Attribute("DataValue").Value;
                            string strText = dr["Value"] == null ? "" : dr["Value"].ToString();
                            try
                            {
                                strText = attr.Attribute("DataText").Value;
                            }
                            catch { }
                            dr["Text"] = strText;
                            table.Rows.Add(dr);
                        }
                    }
                }
                return table;
            }
            catch
            {
                throw new Exception();
            }
        }
        #endregion

        #region 替换消息内容与消息链接中的变量
        public static void ReplaceValue(ref string strContent, ref string strUrl, DataTable dtValue)
        {
            if (dtValue != null && dtValue.Rows.Count > 0)
            {
                foreach (DataRow dr in dtValue.Rows)
                {  
                    strContent = strContent.Replace(string.Concat("{" + dr["Name"] + "}"), string.Concat(dr["Text"]));                
                    strUrl = strUrl.Replace("{" + string.Concat(dr["Name"]) + "}", string.Concat(dr["Value"]));
                }
                string replaceUrl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                              "<System>" +
                              "{0}" +
                              "</System>";
                strUrl = string.Format(replaceUrl, strUrl);
            }
        }
        public static void Replace(ref string strContent, ref string strUrl, DataTable dtValue)
        {
            if (dtValue != null && dtValue.Rows.Count > 0)
            {
                foreach (DataRow dr in dtValue.Rows)
                {
                    strContent = strContent.Replace(string.Concat("{" + dr["ColumnName"].ToString().ToUpper() + "}"), string.Concat(dr["ColumnText"]));
                    strUrl = strUrl.Replace("{" + string.Concat(dr["ColumnName"].ToString().ToUpper()) + "}", string.Concat(dr["ColumnValue"]));
                }
                string replaceUrl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                              "<System>" +
                              "{0}" +
                              "</System>";
                strUrl = string.Format(replaceUrl, strUrl);
            }
        }
        #endregion

        public static string EncyptUrl(string Url, string FormID)
        {
            if (!string.IsNullOrEmpty(Url))
            {
              
                try
                {
                    Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Url);
                    XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                    (from c in xele.Descendants("ApplicationOrder") select c).FirstOrDefault().Value = FormID;
                    string xmlHead = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + "\r\n" + xele.ToString();
                    return xmlHead;
                }
                catch
                {

                }
                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string EncyptUrlNeedXMLHeader(string Url, string FormID)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                string strUrl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                               "<System>" +
                               "{0}" +
                               "</System>";
                Url = string.Format(strUrl, Url);
                try
                {
                    Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Url);
                    XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                    (from c in xele.Descendants("ApplicationOrder") select c).FirstOrDefault().Value = FormID;
                    string xmlHead = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + "\r\n" + xele.ToString();
                    return xmlHead;
                }
                catch
                {

                }
                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string BOObjectEscapeString(string strBOObject)
        {
            if (!string.IsNullOrEmpty(strBOObject))
            {
                StringBuilder sb = new StringBuilder();
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strBOObject);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var Object = (from item in xele.Descendants("Object")
                              select item).FirstOrDefault();
                var SysteName = (from item in xele.Descendants("Name")
                                 select item).FirstOrDefault().Value;
                string ObjectName = Object.Attribute("Name").Value;
                string ObjectDesc = Object.Attribute("Description").Value;
                var attributes = from c in xele.Descendants("Attribute")
                                 select c;
                if (attributes.Count() > 0)
                {
                    var list = attributes.ToList();
                    if (list.Count == 1)
                    {
                        // "Г"
                        var attr = list[0];
                        string name = attr.Attribute("Name").Value;
                        string desc = attr.Attribute("Description").Value;
                        string datatype = attr.Attribute("DataType").Value;
                        string datavalue = attr.Attribute("DataValue").Value;
                        string dataText = datavalue;
                        try
                        {
                            dataText = attr.Attribute("DataText").Value;
                        }
                        catch
                        {

                        }
                        sb.Append(name + "|" + datavalue + "Г" + dataText + "Ё");
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        try
                        {
                            var attr = list[i];
                            string name = attr.Attribute("Name").Value;
                            string desc = attr.Attribute("Description").Value;
                            string datatype = attr.Attribute("DataType").Value;
                            string datavalue = attr.Attribute("DataValue").Value;
                            string dataText = datavalue;
                            try
                            {
                                dataText = attr.Attribute("DataText").Value;
                            }
                            catch
                            {

                            }
                            if (i == list.Count - 1)
                            {

                                sb.Append(name + "|" + datavalue + "Г" + dataText);
                            }
                            else
                            {
                                sb.Append(name + "|" + datavalue + "Г" + dataText + "Ё");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog("解析XML出错URL第：" + i + "个出错" + list[i].Attribute("Name").Value + "||" + ex.Message);
                        }
                    }

                }
                return sb.ToString();
            }
            return string.Empty;
        }
    }
}
