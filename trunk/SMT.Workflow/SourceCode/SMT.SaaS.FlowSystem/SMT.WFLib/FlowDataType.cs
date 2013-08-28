using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace SMT.WFLib
{
    [Serializable]
    public class FlowDataType
    {
        [Serializable]
        /// <summary>
        /// 审批意见数据
        /// </summary>
       // public Flow_FlowRecord_T FlowData;
        public class FlowData  
        {

            public string GetValueA(System.Data.Objects.DataClasses.EntityObject eob)
            {
                return "smt";
            }
            private  string GetValue(string xml, string system, string Object, string node)
            {
                try
                {
                    if (xml == null || xml == "")
                        return null;

                    XmlReader XmlReader;
                    StringReader bb = new StringReader(xml);
                    XmlReader = XmlReader.Create(bb);

                    XElement xEle = XElement.Load(XmlReader);
                    var DataVale = from c in xEle.Descendants("Attribute")
                                   where c.Attribute("Name").Value == node
                                   select c.Attribute("DataValue").Value;


                    #region 匹配系统，对象查询值 --取消匹配系统和对象，直接取值
                    //string strName = xEle.Element("Name").Value == system ? system : null;

                    //if (strName == null)
                    //{
                    //  //  throw new Exception("没有找到对象");
                    //    return null;
                    //}
                    //var tblist = xEle.Elements("Object").Where(c => c.Attribute("Name").Value == Object).ToList();

                    //if (tblist.Count == 0)
                    //{
                       
                    //       // throw new Exception("没有找到对象");
                    //         return null;
                        
                    //}
                       
                    ////return null;

                    //var DataVale = from c in tblist.Elements("Attribute")
                    //               where c.Attribute("Name").Value == node
                    //               select c.Attribute("DataValue").Value;

                    #endregion

                    return DataVale.ToList().Count > 0 ? DataVale.ToList().FirstOrDefault().ToString() : null;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public string GetString(string xml,string system,string Object, string node)
            {

                return GetValue(xml, system, Object,  node);
            }

            public decimal? GetDecimal(string xml, string system, string Object, string node)
            {
                decimal? tmpDecimal;
                string tmpData = GetValue(xml, system, Object, node);
                tmpData = string.IsNullOrEmpty(tmpData)? "0" : tmpData;
                tmpDecimal = null;
                return tmpData == null ? tmpDecimal : Convert.ToDecimal(tmpData);
            }

            public DateTime  GetDate(string xml, string system, string Object, string node)
            {

                return Convert.ToDateTime(GetValue(xml, system, Object, node));
            }

            public string  xml;

            //public string ModelCode { get; set; }                      //模块代码
            //public string FlowCode { get; set; }                       //流程代码
            //public string StateCode { get; set; }                      //本次审批步骤代码
            //public string ParentStateCode { get; set; }                //父审批步骤代码
            //public string SheetID { get; set; }                        //表单ID
            //public string Content { get; set; }                        //审批意见
            //public string CompanyID { get; set; }                      //机构ID
            //public string OfficeID { get; set; }                       //岗位ID
            //public string UserID { get;set;}                           //用户ID
            
         }
    }
}
