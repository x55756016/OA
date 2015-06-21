using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections;

namespace SMT.SaaS.MobileXml
{
    /// <summary>
    /// Xml数据匹配填充 2011-07-01 王继华创建   2011-08-11 修改Xml模板自动复制功能 2011-08-22 多个子表问题的解决
    /// </summary>
    public class MobileXml
    {
        /// <summary>
        /// Xml数据匹配
        /// </summary>
        public MobileXml()
        {

        }

        /// <summary>
        /// 返回错误结果
        /// </summary>
        private string error = string.Empty;  

        /// <summary>
        /// 绑定Xml与数据的匹配填充(主表+子表)
        /// </summary>
        /// <param name="objMaster">带数据的实体（主表）</param>
        /// <param name="objDetailList">带数据的实体集（子表集），没有时可以为NULL</param>
        /// <param name="strXmlSource">Xml模板</param>
        /// <param name="AutoList">需转换值的列表</param>
        /// <returns></returns>
        public string TableToXml(Object objMaster, Object objDetailList, string strXmlSource, List<AutoDictionary> AutoList)
        {
            try
            {
                XDocument xml = null;
                if (objMaster == null)
                {
                    error = "objMaster is Null;";
                    return string.Empty;
                }
                Type objtype = objMaster.GetType();
                if (strXmlSource != string.Empty)
                {
                    xml = XDocument.Parse(strXmlSource);
                }
                else
                {
                    error = "strXmlSource is Null;";
                    return string.Empty;
                }

                #region 主表的实体映射
                PropertyInfo[] propinfos = objtype.GetProperties();
                foreach (PropertyInfo propinfo in propinfos)
                {
                    string keyValue = propinfo.GetValue(objMaster, null) != null ? propinfo.GetValue(objMaster, null).ToString() : string.Empty;

                    //处理Key值
                    IEnumerable<XElement> xElementID = xml.Elements("System").Elements("Object");
                    XElement lastMaster = xml.Elements("System").Elements("Object").LastOrDefault();

                    if (keyValue != string.Empty && lastMaster.Attribute("Key") != null && lastMaster.Attribute("id") != null && lastMaster.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                    {
                        lastMaster.Attribute("id").SetValue(keyValue);
                    }

                    //读取Xml主表节点
                    IEnumerable<XElement> xElementList = xml.Elements("System").Elements("Object").Elements("Attribute");
                    foreach (XElement item in xElementList)
                    {
                        //注释掉的原因：有的显示在界面上的字段可以为空
                        //if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                        //{
                        //    item.Attribute("DataValue").SetValue(keyValue);
                        //    //item.Attribute("DataText").SetValue(keyValue);
                        //    item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                        //}
                        if ( item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                        {
                            item.Attribute("DataValue").SetValue(keyValue);
                            //item.Attribute("DataText").SetValue(keyValue);
                            item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                        }
                        //处理需转换值的列表
                        foreach (AutoDictionary Dictionary in AutoList)
                        {
                            if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objtype.Name.ToUpper() == Dictionary.TableName.ToUpper() )
                            {
                                item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                            }
                        }
                    }
                }
                #endregion

                #region 子表的实体映射
                if (objDetailList != null && objDetailList != string.Empty && typeof(IEnumerable).IsAssignableFrom(objDetailList.GetType()))
                {
                    //分解数据集
                    IEnumerable objDetail = objDetailList as IEnumerable;
                    foreach (var itemObjectList in xml.Elements("System").Elements("Object").Elements("ObjectList"))
                    {
                        int i = 0;
                        foreach (var v in objDetail)
                        {
                           
                            v.GetType();
                            Type objdetailtype = v.GetType();
                            PropertyInfo[] propinfosdetail = objdetailtype.GetProperties();

                             XElement DetailTableName = itemObjectList.Elements("Object").LastOrDefault();
                             if (objdetailtype.Name.ToUpper() == DetailTableName.Attribute("Name").Value.ToUpper().ToString())
                             {
                                 i++;
                                 if (i == 1)
                                 {
                                     #region 第一次加载Xml模板中的子表
                                     foreach (PropertyInfo propinfo in propinfosdetail)
                                     {
                                         string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                         //处理Key值
                                         XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                         if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                         {
                                             lastDetail.Attribute("id").SetValue(keyValue);
                                         }

                                         //读取Xml子表节点
                                         IEnumerable<XElement> xElementDetailList = itemObjectList.Elements("Object").Elements("Attribute");
                                         foreach (XElement item in xElementDetailList)
                                         {
                                             if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                             {
                                                 item.Attribute("DataValue").SetValue(keyValue);
                                                 //item.Attribute("DataText").SetValue(keyValue);
                                                 item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                             }
                                             //处理字典等相类似的值
                                             foreach (AutoDictionary Dictionary in AutoList)
                                             {
                                                 if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                 {
                                                     item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                     item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                 }
                                             }
                                         }
                                     }
                                     #endregion
                                 }
                                 else
                                 {
                                     #region 第二次以上加载Xml模板中的子表
                                     //查询Xml子表节点
                                     XElement xDetailObject = itemObjectList.Elements("Object").FirstOrDefault();

                                     //生成Xml子表节点                                
                                     XElement xObjectDetail = new XElement("Object");

                                     foreach (var nv in xDetailObject.Attributes())
                                     {
                                         xObjectDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                     }

                                     foreach (XElement xda in itemObjectList.Elements("Object").FirstOrDefault().Elements("Attribute"))
                                     {
                                         XElement xAttributeDetail = new XElement("Attribute");
                                         foreach (var nv in xda.Attributes())
                                         {
                                             xAttributeDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                         }
                                         xObjectDetail.Add(xAttributeDetail);
                                     }
                                     itemObjectList.Add(xObjectDetail);

                                     foreach (PropertyInfo propinfo in propinfosdetail)
                                     {
                                         string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                         //处理Key值
                                         XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                         if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                         {
                                             lastDetail.Attribute("id").SetValue(keyValue);
                                         }

                                         //读取Xml子表节点
                                         XElement lastx = itemObjectList.Elements("Object").LastOrDefault();
                                         IEnumerable<XElement> xElementDetailList = lastx.Elements("Attribute");

                                         foreach (XElement item in xElementDetailList)
                                         {
                                            // if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                             //keyValue != string.Empty && 去掉 防止为空的数据不能清空wjh 2012-2-16
                                             if ( item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                             {
                                                 item.Attribute("DataValue").SetValue(keyValue);
                                                 //  item.Attribute("DataText").SetValue(keyValue);
                                                 item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                             }
                                             //处理字典等相类似的值
                                             foreach (AutoDictionary Dictionary in AutoList)
                                             {
                                                 if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                 {
                                                     item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                     item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                 }
                                             }
                                         }
                                     }
                                     #endregion
                                 }
                             }
                        }
                    }
                }
                #endregion    
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine(xml.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                error += " " + ex.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// 绑定Xml与数据的匹配填充(主表+子表)
        /// </summary>
        /// <param name="objMaster">带数据的实体（主表）</param>
        /// <param name="objDetailList">带数据的实体集（子表集），没有时可以为NULL</param>
        /// <param name="strXmlSource">Xml模板</param>
        /// <param name="AutoList">需转换值的列表</param>
        /// <returns></returns>
        public string TableToXmlForTravel(Object objMaster, Object objDetailList, string strXmlSource, List<AutoDictionary> AutoList)
        {
            try
            {
                XDocument xml = null;
                if (objMaster == null)
                {
                    error = "objMaster is Null;";
                    return string.Empty;
                }
                Type objtype = objMaster.GetType();
                if (strXmlSource != string.Empty)
                {
                    xml = XDocument.Parse(strXmlSource);
                }
                else
                {
                    error = "strXmlSource is Null;";
                    return string.Empty;
                }

                #region 主表的实体映射
                PropertyInfo[] propinfos = objtype.GetProperties();
                foreach (PropertyInfo propinfo in propinfos)
                {
                    string keyValue = propinfo.GetValue(objMaster, null) != null ? propinfo.GetValue(objMaster, null).ToString() : string.Empty;

                    //处理Key值
                    IEnumerable<XElement> xElementID = xml.Elements("System").Elements("Object");
                    XElement lastMaster = xml.Elements("System").Elements("Object").LastOrDefault();

                    if (keyValue != string.Empty && lastMaster.Attribute("Key") != null && lastMaster.Attribute("id") != null && lastMaster.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                    {
                        lastMaster.Attribute("id").SetValue(keyValue);
                    }

                    //读取Xml主表节点
                    IEnumerable<XElement> xElementList = xml.Elements("System").Elements("Object").Elements("Attribute");
                    foreach (XElement item in xElementList)
                    {
                        //注释掉的原因：有的显示在界面上的字段可以为空
                        //if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                        //{
                        //    item.Attribute("DataValue").SetValue(keyValue);
                        //    //item.Attribute("DataText").SetValue(keyValue);
                        //    item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                        //}
                        if (item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                        {
                            item.Attribute("DataValue").SetValue(keyValue);
                            //item.Attribute("DataText").SetValue(keyValue);
                            item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                        }
                        //处理需转换值的列表
                        foreach (AutoDictionary Dictionary in AutoList)
                        {
                            if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objtype.Name.ToUpper() == Dictionary.TableName.ToUpper())
                            {
                                item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                            }
                        }
                    }
                }
                #endregion

                #region 子表的实体映射
                if (objDetailList != null && objDetailList != string.Empty && typeof(IEnumerable).IsAssignableFrom(objDetailList.GetType()))
                {
                    //分解数据集
                    IEnumerable objDetail = objDetailList as IEnumerable;
                    var qq = from ent in xml.Elements("System").Elements("Object").Elements("ObjectList")
                            where ent.Attribute("Name").Value == "BUSINESSREPORTDETAILDetailList"
                            select ent;
                    foreach (var itemObjectList in qq)
                    {
                        int i = 0;
                        foreach (var v in objDetail)
                        {

                            v.GetType();
                            Type objdetailtype = v.GetType();
                            PropertyInfo[] propinfosdetail = objdetailtype.GetProperties();

                            XElement DetailTableName = itemObjectList.Elements("Object").LastOrDefault();
                            if (objdetailtype.Name.ToUpper() == DetailTableName.Attribute("Name").Value.ToUpper().ToString())
                            {
                                i++;
                                if (i == 1)
                                {
                                    #region 第一次加载Xml模板中的子表
                                    foreach (PropertyInfo propinfo in propinfosdetail)
                                    {
                                        string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                        //处理Key值
                                        XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                        if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                        {
                                            lastDetail.Attribute("id").SetValue(keyValue);
                                        }

                                        //读取Xml子表节点
                                        IEnumerable<XElement> xElementDetailList = itemObjectList.Elements("Object").Elements("Attribute");
                                        foreach (XElement item in xElementDetailList)
                                        {
                                            if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                item.Attribute("DataValue").SetValue(keyValue);
                                                //item.Attribute("DataText").SetValue(keyValue);
                                                item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                            }
                                            //处理字典等相类似的值
                                            foreach (AutoDictionary Dictionary in AutoList)
                                            {
                                                if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                {
                                                    item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                    item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region 第二次以上加载Xml模板中的子表
                                    //查询Xml子表节点
                                    XElement xDetailObject = itemObjectList.Elements("Object").FirstOrDefault();

                                    //生成Xml子表节点                                
                                    XElement xObjectDetail = new XElement("Object");

                                    foreach (var nv in xDetailObject.Attributes())
                                    {
                                        xObjectDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                    }

                                    foreach (XElement xda in itemObjectList.Elements("Object").FirstOrDefault().Elements("Attribute"))
                                    {
                                        XElement xAttributeDetail = new XElement("Attribute");
                                        foreach (var nv in xda.Attributes())
                                        {
                                            xAttributeDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                        }
                                        xObjectDetail.Add(xAttributeDetail);
                                    }
                                    itemObjectList.Add(xObjectDetail);

                                    foreach (PropertyInfo propinfo in propinfosdetail)
                                    {
                                        string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                        //处理Key值
                                        XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                        if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                        {
                                            lastDetail.Attribute("id").SetValue(keyValue);
                                        }

                                        //读取Xml子表节点
                                        XElement lastx = itemObjectList.Elements("Object").LastOrDefault();
                                        IEnumerable<XElement> xElementDetailList = lastx.Elements("Attribute");

                                        foreach (XElement item in xElementDetailList)
                                        {
                                            // if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            //keyValue != string.Empty && 去掉 防止为空的数据不能清空wjh 2012-2-16
                                            if (item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                item.Attribute("DataValue").SetValue(keyValue);
                                                //  item.Attribute("DataText").SetValue(keyValue);
                                                item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                            }
                                            //处理字典等相类似的值
                                            foreach (AutoDictionary Dictionary in AutoList)
                                            {
                                                if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                {
                                                    item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                    item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    var q = from ent in xml.Elements("System").Elements("Object").Elements("ObjectList")
                            where ent.Attribute("Name").Value == "EXTENSIONORDERDETAILList"
                            select ent;
                    var qChilds = from ent in q.Elements("Attribute")
                                  select ent;
                    if (qChilds.Count() > 0)
                    {
                        IEnumerable<XElement> xElementDetailList = q.Elements("Attribute");
                        foreach (XElement item in xElementDetailList)
                        {
                            
                            //处理字典等相类似的值
                            foreach (AutoDictionary Dictionary in AutoList)
                            {
                                if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() )
                                {
                                    item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                    item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                }
                            }
                        }

                    }
                    
                    
                    foreach (var itemObjectList in q)
                    {
                        int i = 0;
                        
                        foreach (var v in objDetail)
                        {
                            if (v.GetType().Name == "T_FB_EXTENSIONORDERDETAIL")
                            {

                                v.GetType();
                                Type objdetailtype = v.GetType();
                                PropertyInfo[] propinfosdetail = objdetailtype.GetProperties();

                                XElement DetailTableName = itemObjectList.Elements("Object").LastOrDefault();
                                if ("T_FB_EXTENSIONORDERDETAIL" == DetailTableName.Attribute("Name").Value.ToUpper().ToString())
                                {
                                    i++;
                                    if (i == 1)
                                    {
                                        #region 第一次加载Xml模板中的子表
                                        foreach (PropertyInfo propinfo in propinfosdetail)
                                        {
                                            string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                            //处理Key值
                                            XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                            if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                lastDetail.Attribute("id").SetValue(keyValue);
                                            }

                                            //读取Xml子表节点
                                            IEnumerable<XElement> xElementDetailList = itemObjectList.Elements("Object").Elements("Attribute");
                                            foreach (XElement item in xElementDetailList)
                                            {
                                                if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                                {
                                                    item.Attribute("DataValue").SetValue(keyValue);
                                                    //item.Attribute("DataText").SetValue(keyValue);
                                                    item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                                }
                                                //处理字典等相类似的值
                                                foreach (AutoDictionary Dictionary in AutoList)
                                                {
                                                    if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                    {
                                                        item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                        item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 第二次以上加载Xml模板中的子表
                                        //查询Xml子表节点
                                        XElement xDetailObject = itemObjectList.Elements("Object").FirstOrDefault();

                                        //生成Xml子表节点                                
                                        XElement xObjectDetail = new XElement("Object");

                                        foreach (var nv in xDetailObject.Attributes())
                                        {
                                            xObjectDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                        }

                                        foreach (XElement xda in itemObjectList.Elements("Object").FirstOrDefault().Elements("Attribute"))
                                        {
                                            XElement xAttributeDetail = new XElement("Attribute");
                                            foreach (var nv in xda.Attributes())
                                            {
                                                xAttributeDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                            }
                                            xObjectDetail.Add(xAttributeDetail);
                                        }
                                        itemObjectList.Add(xObjectDetail);

                                        foreach (PropertyInfo propinfo in propinfosdetail)
                                        {
                                            string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                            //处理Key值
                                            XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                            if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                lastDetail.Attribute("id").SetValue(keyValue);
                                            }

                                            //读取Xml子表节点
                                            XElement lastx = itemObjectList.Elements("Object").LastOrDefault();
                                            IEnumerable<XElement> xElementDetailList = lastx.Elements("Attribute");

                                            foreach (XElement item in xElementDetailList)
                                            {
                                                // if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                                //keyValue != string.Empty && 去掉 防止为空的数据不能清空wjh 2012-2-16
                                                if (item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                                {
                                                    item.Attribute("DataValue").SetValue(keyValue);
                                                    //  item.Attribute("DataText").SetValue(keyValue);
                                                    item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                                }
                                                //处理字典等相类似的值
                                                foreach (AutoDictionary Dictionary in AutoList)
                                                {
                                                    if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                    {
                                                        item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                        item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                            
                        }
                    }
                    #region 冲借款明细
                    var ChangeEnts = from ent in xml.Elements("System").Elements("Object").Elements("ObjectList")
                                     where ent.Attribute("Name").Value == "CHARGEAPPLYREPAYDETAILList"
                            select ent;

                    foreach (var itemObjectList in ChangeEnts)
                    {
                        int i = 0;

                        foreach (var v in objDetail)
                        {
                            if (v.GetType().Name == "T_FB_CHARGEAPPLYREPAYDETAIL")
                            {

                                v.GetType();
                                Type objdetailtype = v.GetType();
                                PropertyInfo[] propinfosdetail = objdetailtype.GetProperties();

                                XElement DetailTableName = itemObjectList.Elements("Object").LastOrDefault();
                                if ("T_FB_CHARGEAPPLYREPAYDETAIL" == DetailTableName.Attribute("Name").Value.ToUpper().ToString())
                                {
                                    i++;
                                    if (i == 1)
                                    {
                                        #region 第一次加载Xml模板中的子表
                                        foreach (PropertyInfo propinfo in propinfosdetail)
                                        {
                                            string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                            //处理Key值
                                            XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                            if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                lastDetail.Attribute("id").SetValue(keyValue);
                                            }

                                            //读取Xml子表节点
                                            IEnumerable<XElement> xElementDetailList = itemObjectList.Elements("Object").Elements("Attribute");
                                            foreach (XElement item in xElementDetailList)
                                            {
                                                if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                                {
                                                    item.Attribute("DataValue").SetValue(keyValue);
                                                    //item.Attribute("DataText").SetValue(keyValue);
                                                    item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                                }
                                                //处理字典等相类似的值
                                                foreach (AutoDictionary Dictionary in AutoList)
                                                {
                                                    if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                    {
                                                        item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                        item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 第二次以上加载Xml模板中的子表
                                        //查询Xml子表节点
                                        XElement xDetailObject = itemObjectList.Elements("Object").FirstOrDefault();

                                        //生成Xml子表节点                                
                                        XElement xObjectDetail = new XElement("Object");

                                        foreach (var nv in xDetailObject.Attributes())
                                        {
                                            xObjectDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                        }

                                        foreach (XElement xda in itemObjectList.Elements("Object").FirstOrDefault().Elements("Attribute"))
                                        {
                                            XElement xAttributeDetail = new XElement("Attribute");
                                            foreach (var nv in xda.Attributes())
                                            {
                                                xAttributeDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                            }
                                            xObjectDetail.Add(xAttributeDetail);
                                        }
                                        itemObjectList.Add(xObjectDetail);

                                        foreach (PropertyInfo propinfo in propinfosdetail)
                                        {
                                            string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                            //处理Key值
                                            XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                            if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                lastDetail.Attribute("id").SetValue(keyValue);
                                            }

                                            //读取Xml子表节点
                                            XElement lastx = itemObjectList.Elements("Object").LastOrDefault();
                                            IEnumerable<XElement> xElementDetailList = lastx.Elements("Attribute");

                                            foreach (XElement item in xElementDetailList)
                                            {
                                                // if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                                //keyValue != string.Empty && 去掉 防止为空的数据不能清空wjh 2012-2-16
                                                if (item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                                {
                                                    item.Attribute("DataValue").SetValue(keyValue);
                                                    //  item.Attribute("DataText").SetValue(keyValue);
                                                    item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                                }
                                                //处理字典等相类似的值
                                                foreach (AutoDictionary Dictionary in AutoList)
                                                {
                                                    if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                    {
                                                        item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                        item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }

                        }
                    }
                    #endregion
                }
                #endregion
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine(xml.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                error += " " + ex.ToString();
                return string.Empty;
            }
        }


        /// <summary>
        /// 绑定Xml与数据的匹配填充(主表+子表)
        /// </summary>
        /// <param name="objMaster">带数据的实体（主表）</param>
        /// <param name="objDetailList">带数据的实体集（子表集），没有时可以为NULL</param>
        /// <param name="strXmlSource">Xml模板</param>
        /// <param name="AutoList">需转换值的列表</param>
        /// <returns></returns>
        public string TableToXmlForTravel(Object objMaster, Object objDetailFirst, Object objDetailSecond, string strXmlSource, List<AutoDictionary> AutoList)
        {
            try
            {
                XDocument xml = null;
                if (objMaster == null)
                {
                    error = "objMaster is Null;";
                    return string.Empty;
                }
                Type objtype = objMaster.GetType();
                if (strXmlSource != string.Empty)
                {
                    xml = XDocument.Parse(strXmlSource);
                }
                else
                {
                    error = "strXmlSource is Null;";
                    return string.Empty;
                }

                #region 主表的实体映射
                PropertyInfo[] propinfos = objtype.GetProperties();
                foreach (PropertyInfo propinfo in propinfos)
                {
                    string keyValue = propinfo.GetValue(objMaster, null) != null ? propinfo.GetValue(objMaster, null).ToString() : string.Empty;

                    //处理Key值
                    IEnumerable<XElement> xElementID = xml.Elements("System").Elements("Object");
                    XElement lastMaster = xml.Elements("System").Elements("Object").LastOrDefault();

                    if (keyValue != string.Empty && lastMaster.Attribute("Key") != null && lastMaster.Attribute("id") != null && lastMaster.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                    {
                        lastMaster.Attribute("id").SetValue(keyValue);
                    }

                    //读取Xml主表节点
                    IEnumerable<XElement> xElementList = xml.Elements("System").Elements("Object").Elements("Attribute");
                    foreach (XElement item in xElementList)
                    {
                        //注释掉的原因：有的显示在界面上的字段可以为空
                        //if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                        //{
                        //    item.Attribute("DataValue").SetValue(keyValue);
                        //    //item.Attribute("DataText").SetValue(keyValue);
                        //    item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                        //}
                        if (item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                        {
                            item.Attribute("DataValue").SetValue(keyValue);
                            //item.Attribute("DataText").SetValue(keyValue);
                            item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                        }
                        //处理需转换值的列表
                        foreach (AutoDictionary Dictionary in AutoList)
                        {
                            if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objtype.Name.ToUpper() == Dictionary.TableName.ToUpper())
                            {
                                item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                            }
                        }
                    }
                }
                #endregion

                #region 第一个子表的实体映射
              

                if (objDetailFirst != null && typeof(IEnumerable).IsAssignableFrom(objDetailFirst.GetType()))
                {
                    //分解数据集
                    IEnumerable objDetail = objDetailFirst as IEnumerable;

                    var q = from ent in xml.Elements("System").Elements("Object").Elements("ObjectList").Elements("Object")
                            where ent.Attribute("Name").Value == "T_OA_REIMBURSEMENTDETAIL"
                            select ent;
                  
                    foreach (var itemObjectList in xml.Elements("System").Elements("Object").Elements("ObjectList"))
                    {
                        int i = 0;
                        foreach (var v in objDetail)
                        {

                            v.GetType();
                            Type objdetailtype = v.GetType();
                            PropertyInfo[] propinfosdetail = objdetailtype.GetProperties();

                           

                            XElement DetailTableName = q.LastOrDefault();
                            if (objdetailtype.Name.ToUpper() == DetailTableName.Attribute("Name").Value.ToUpper().ToString())
                            {
                                i++;
                                if (i == 1)
                                {
                                    #region 第一次加载Xml模板中的子表
                                    foreach (PropertyInfo propinfo in propinfosdetail)
                                    {
                                        string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                        //处理Key值
                                        XElement lastDetail = q.LastOrDefault();
                                        if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                        {
                                            lastDetail.Attribute("id").SetValue(keyValue);
                                        }

                                        //读取Xml子表节点
                                        IEnumerable<XElement> xElementDetailList = itemObjectList.Elements("Object").Elements("Attribute");
                                        foreach (XElement item in xElementDetailList)
                                        {
                                            if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                item.Attribute("DataValue").SetValue(keyValue);
                                                //item.Attribute("DataText").SetValue(keyValue);
                                                item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                            }
                                            //处理字典等相类似的值
                                            var dicTable = from ent in AutoList.AsQueryable()
                                                          where ent.TableName.ToUpper() == objdetailtype.Name.ToUpper()
                                                          select ent;

                                            var dicName = from ent in dicTable
                                                          where ent.Name.ToUpper() == item.Attribute("Name").Value.ToUpper()
                                                          select ent;

                                            var dicValue = from ent in dicName
                                                           where ent.KeyValue == lastDetail.Attribute("id").Value
                                                           select ent;
                                            if (dicValue.Count() > 0)
                                            {
                                                item.Attribute("DataValue").SetValue(dicValue.FirstOrDefault().DataValue ?? string.Empty);
                                                item.Attribute("DataText").SetValue(dicValue.FirstOrDefault().DataText ?? string.Empty);
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region 第二次以上加载Xml模板中的子表
                                    //查询Xml子表节点
                                    XElement xDetailObject = q.FirstOrDefault();

                                    //生成Xml子表节点                                
                                    XElement xObjectDetail = new XElement("Object");

                                    foreach (var nv in xDetailObject.Attributes())
                                    {
                                        xObjectDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                    }

                                    foreach (XElement xda in q.FirstOrDefault().Elements("Attribute"))
                                    {
                                        XElement xAttributeDetail = new XElement("Attribute");
                                        foreach (var nv in xda.Attributes())
                                        {
                                            xAttributeDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                        }
                                        xObjectDetail.Add(xAttributeDetail);
                                    }
                                    itemObjectList.Add(xObjectDetail);

                                    foreach (PropertyInfo propinfo in propinfosdetail)
                                    {
                                        string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                        //处理Key值
                                        XElement lastDetail = q.LastOrDefault(); 
                                        if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                        {
                                            lastDetail.Attribute("id").SetValue(keyValue);
                                        }

                                        //读取Xml子表节点
                                        XElement lastx = q.LastOrDefault();
                                        IEnumerable<XElement> xElementDetailList = lastx.Elements("Attribute");

                                        foreach (XElement item in xElementDetailList)
                                        {
                                            // if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            //keyValue != string.Empty && 去掉 防止为空的数据不能清空wjh 2012-2-16
                                            if (item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                item.Attribute("DataValue").SetValue(keyValue);
                                                //  item.Attribute("DataText").SetValue(keyValue);
                                                item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                            }
                                            //处理字典等相类似的值
                                            foreach (AutoDictionary Dictionary in AutoList)
                                            {
                                                if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                {
                                                    item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                    item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 第二个子表的实体映射
                if (objDetailSecond != null  && typeof(IEnumerable).IsAssignableFrom(objDetailSecond.GetType()))
                {
                    //分解数据集
                    IEnumerable objDetail = objDetailSecond as IEnumerable;
                    var q = from ent in xml.Elements("System").Elements("Object").Elements("ObjectList").Elements("Object")
                            where ent.Attribute("Name").Value == "T_FB_EXTENSIONORDERDETAIL"
                            select ent;

                    foreach (var itemObjectList in xml.Elements("System").Elements("Object").Elements("ObjectList"))
                    {
                        int i = 0;
                        foreach (var v in objDetail)
                        {

                            v.GetType();
                            Type objdetailtype = v.GetType();
                            PropertyInfo[] propinfosdetail = objdetailtype.GetProperties();

                            XElement DetailTableName = q.LastOrDefault();
                            if (objdetailtype.Name.ToUpper() == DetailTableName.Attribute("Name").Value.ToUpper().ToString())
                            {
                                i++;
                                if (i == 1)
                                {
                                    #region 第一次加载Xml模板中的子表
                                    foreach (PropertyInfo propinfo in propinfosdetail)
                                    {
                                        string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                        //处理Key值
                                        XElement lastDetail = q.LastOrDefault();
                                        if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                        {
                                            lastDetail.Attribute("id").SetValue(keyValue);
                                        }

                                        //读取Xml子表节点
                                        XElement lastx = q.LastOrDefault();
                                        IEnumerable<XElement> xElementDetailList = lastx.Elements("Attribute");
                                        foreach (XElement item in xElementDetailList)
                                        {
                                            if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                item.Attribute("DataValue").SetValue(keyValue);
                                                //item.Attribute("DataText").SetValue(keyValue);
                                                item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                            }
                                            //处理字典等相类似的值
                                            var dicTable = from ent in AutoList.AsQueryable()
                                                           where ent.TableName.ToUpper() == objdetailtype.Name.ToUpper()
                                                           select ent;

                                            var dicName = from ent in dicTable
                                                          where ent.Name.ToUpper() == item.Attribute("Name").Value.ToUpper()
                                                          select ent;

                                            var dicValue = from ent in dicName
                                                           where ent.KeyValue == lastDetail.Attribute("id").Value
                                                           select ent;
                                            if (dicValue.Count() > 0)
                                            {
                                                item.Attribute("DataValue").SetValue(dicValue.FirstOrDefault().DataValue ?? string.Empty);
                                                item.Attribute("DataText").SetValue(dicValue.FirstOrDefault().DataText ?? string.Empty);
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region 第二次以上加载Xml模板中的子表
                                    //查询Xml子表节点
                                    XElement xDetailObject = q.FirstOrDefault();

                                    //生成Xml子表节点                                
                                    XElement xObjectDetail = new XElement("Object");

                                    foreach (var nv in xDetailObject.Attributes())
                                    {
                                        xObjectDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                    }

                                    foreach (XElement xda in q.FirstOrDefault().Elements("Attribute"))
                                    {
                                        XElement xAttributeDetail = new XElement("Attribute");
                                        foreach (var nv in xda.Attributes())
                                        {
                                            xAttributeDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                        }
                                        xObjectDetail.Add(xAttributeDetail);
                                    }
                                    itemObjectList.Add(xObjectDetail);

                                    foreach (PropertyInfo propinfo in propinfosdetail)
                                    {
                                        string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                        //处理Key值
                                        XElement lastDetail = q.LastOrDefault();
                                        if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                        {
                                            lastDetail.Attribute("id").SetValue(keyValue);
                                        }

                                        //读取Xml子表节点
                                        XElement lastx = q.LastOrDefault();
                                        IEnumerable<XElement> xElementDetailList = lastx.Elements("Attribute");

                                        foreach (XElement item in xElementDetailList)
                                        {
                                            // if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            //keyValue != string.Empty && 去掉 防止为空的数据不能清空wjh 2012-2-16
                                            if (item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                item.Attribute("DataValue").SetValue(keyValue);
                                                //  item.Attribute("DataText").SetValue(keyValue);
                                                item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                            }
                                            //处理字典等相类似的值
                                            foreach (AutoDictionary Dictionary in AutoList)
                                            {
                                                if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                {
                                                    item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                    item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                }
                #endregion
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine(xml.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                error += " " + ex.ToString();
                return string.Empty;
            }
        }



        /// <summary>
        /// 绑定Xml与数据的匹配填充(主表+子表)
        /// </summary>
        /// <param name="objMaster">带数据的实体（主表）</param>
        /// <param name="objDetailList">带数据的实体集（子表集），没有时可以为NULL</param>
        /// <param name="strXmlSource">Xml模板</param>
        /// <param name="AutoList">需转换值的列表</param>
        /// <returns></returns>
        public string TableToXmlaa(Object objMaster, Object objDetailList, string strXmlSource, List<AutoDictionary> AutoList)
        {
            try
            {
                XDocument xml = null;
                if (objMaster == null)
                {
                    error = "objMaster is Null;";
                    return string.Empty;
                }
                Type objtype = objMaster.GetType();
                if (strXmlSource != string.Empty)
                {
                    xml = XDocument.Parse(strXmlSource);
                }
                else
                {
                    error = "strXmlSource is Null;";
                    return string.Empty;
                }

                #region 主表的实体映射
                PropertyInfo[] propinfos = objtype.GetProperties();
                foreach (PropertyInfo propinfo in propinfos)
                {
                    string keyValue = propinfo.GetValue(objMaster, null) != null ? propinfo.GetValue(objMaster, null).ToString() : string.Empty;

                    //处理Key值
                    IEnumerable<XElement> xElementID = xml.Elements("System").Elements("Object");
                    XElement lastMaster = xml.Elements("System").Elements("Object").LastOrDefault();

                    if (keyValue != string.Empty && lastMaster.Attribute("Key") != null && lastMaster.Attribute("id") != null && lastMaster.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                    {
                        lastMaster.Attribute("id").SetValue(keyValue);
                    }

                    //读取Xml主表节点
                    IEnumerable<XElement> xElementList = xml.Elements("System").Elements("Object").Elements("Attribute");
                    foreach (XElement item in xElementList)
                    {
                        //注释掉的原因：有的显示在界面上的字段可以为空
                        //if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                        //{
                        //    item.Attribute("DataValue").SetValue(keyValue);
                        //    //item.Attribute("DataText").SetValue(keyValue);
                        //    item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                        //}
                        if (item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                        {
                            item.Attribute("DataValue").SetValue(keyValue);
                            //item.Attribute("DataText").SetValue(keyValue);
                            item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                        }
                        //处理需转换值的列表
                        foreach (AutoDictionary Dictionary in AutoList)
                        {
                            if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objtype.Name.ToUpper() == Dictionary.TableName.ToUpper())
                            {
                                item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                            }
                        }
                    }
                }
                #endregion

                #region 子表的实体映射
                if (objDetailList != null && objDetailList != string.Empty && typeof(IEnumerable).IsAssignableFrom(objDetailList.GetType()))
                {
                    //分解数据集
                    IEnumerable objDetail = objDetailList as IEnumerable;
                    foreach (var itemObjectList in xml.Elements("System").Elements("Object").Elements("ObjectList"))
                    {
                        int i = 0;
                        foreach (var v in objDetail)
                        {

                            v.GetType();
                            Type objdetailtype = v.GetType();
                            PropertyInfo[] propinfosdetail = objdetailtype.GetProperties();

                            XElement DetailTableName = itemObjectList.Elements("Object").LastOrDefault();
                            if (objdetailtype.Name.ToUpper() == DetailTableName.Attribute("Name").Value.ToUpper().ToString())
                            {
                                i++;
                                if (i == 1)
                                {
                                    #region 第一次加载Xml模板中的子表
                                    foreach (PropertyInfo propinfo in propinfosdetail)
                                    {
                                        string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                        //处理Key值
                                        XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                        if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                        {
                                            lastDetail.Attribute("id").SetValue(keyValue);
                                        }

                                        //读取Xml子表节点
                                        IEnumerable<XElement> xElementDetailList = itemObjectList.Elements("Object").Elements("Attribute");
                                        foreach (XElement item in xElementDetailList)
                                        {
                                            if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                item.Attribute("DataValue").SetValue(keyValue);
                                                //item.Attribute("DataText").SetValue(keyValue);
                                                item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                            }
                                            //处理字典等相类似的值
                                            foreach (AutoDictionary Dictionary in AutoList)
                                            {
                                                if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                {
                                                    item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                    item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region 第二次以上加载Xml模板中的子表
                                    //查询Xml子表节点
                                    XElement xDetailObject = itemObjectList.Elements("Object").FirstOrDefault();

                                    //生成Xml子表节点                                
                                    XElement xObjectDetail = new XElement("Object");

                                    foreach (var nv in xDetailObject.Attributes())
                                    {
                                        xObjectDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                    }

                                    foreach (XElement xda in itemObjectList.Elements("Object").FirstOrDefault().Elements("Attribute"))
                                    {
                                        XElement xAttributeDetail = new XElement("Attribute");
                                        foreach (var nv in xda.Attributes())
                                        {
                                            xAttributeDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                                        }
                                        xObjectDetail.Add(xAttributeDetail);
                                    }
                                    itemObjectList.Add(xObjectDetail);

                                    foreach (PropertyInfo propinfo in propinfosdetail)
                                    {
                                        string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                        //处理Key值
                                        XElement lastDetail = itemObjectList.Elements("Object").LastOrDefault();
                                        if (keyValue != string.Empty && lastDetail.Attribute("Key") != null && lastDetail.Attribute("id") != null && lastDetail.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                        {
                                            lastDetail.Attribute("id").SetValue(keyValue);
                                        }

                                        //读取Xml子表节点
                                        XElement lastx = itemObjectList.Elements("Object").LastOrDefault();
                                        IEnumerable<XElement> xElementDetailList = lastx.Elements("Attribute");

                                        foreach (XElement item in xElementDetailList)
                                        {
                                            // if (keyValue != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            //keyValue != string.Empty && 去掉 防止为空的数据不能清空wjh 2012-2-16
                                            if (item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                                            {
                                                item.Attribute("DataValue").SetValue(keyValue);
                                                //  item.Attribute("DataText").SetValue(keyValue);
                                                item.Attribute("Description").SetValue(GetResourceStr(item.Attribute("LableResourceID").Value));
                                            }
                                            //处理字典等相类似的值
                                            foreach (AutoDictionary Dictionary in AutoList)
                                            {
                                                if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objdetailtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && lastDetail.Attribute("id").Value == Dictionary.KeyValue)
                                                {
                                                    item.Attribute("DataValue").SetValue(Dictionary.DataValue ?? string.Empty);
                                                    item.Attribute("DataText").SetValue(Dictionary.DataText ?? string.Empty);
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                }
                #endregion
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine(xml.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                error += " " + ex.ToString();
                return string.Empty;
            }
        }




        /// <summary>
        /// 绑定Xml与数据的匹配填充（表集）
        /// </summary>
        /// <param name="objList">带数据的实体集（现最多三层）</param>
        /// <param name="strXmlSource">Xml模板</param>
        /// <param name="AutoList">需转换值的列表</param>
        /// <returns></returns>
        public string ObjectToXml(Object objList, string strXmlSource, List<AutoDictionary> AutoList)
        {
            try
            {
                XDocument xml = null;
                if (objList == null || objList != string.Empty)
                {
                    error = "objList is Null;";
                    return string.Empty;
                }
                
                if (strXmlSource != string.Empty)
                {
                    xml = XDocument.Parse(strXmlSource);
                }
                else
                {
                    error = "strXmlSource is Null;";
                    return string.Empty;
                }

                #region 表的实体映射
                if (objList != null && objList != string.Empty && typeof(IEnumerable).IsAssignableFrom(objList.GetType()))
                {
                    //分解数据集
                    IEnumerable obj = objList as IEnumerable;

                    #region 第一层主表的实体映射
                    foreach (var v in obj)
                    {
                        v.GetType();
                        Type objtype = v.GetType();
                        PropertyInfo[] propinfos = objtype.GetProperties();
                        string xTableName = xml.Elements("System").Elements("Object").FirstOrDefault().Attribute("Name").Value;
                        if (xTableName != null && xTableName.ToUpper() == objtype.Name.ToUpper())
                        {
                            foreach (PropertyInfo propinfo in propinfos)
                            {
                                //取数据值
                                string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;
                                IEnumerable<XElement> xElements = xml.Elements("System").Elements("Object");
                                foreach (XElement lastMaster in xElements)
                                {
                                    XElemertFormat(lastMaster, keyValue, objtype, propinfo, AutoList);
                                }
                            }
                        }
                    }
                    #endregion

                    foreach (var itemObjectList in xml.Elements("System").Elements("Object").Elements("ObjectList"))
                    {
                        #region 第二层子表的实体映射

                        bool fig1 = true;//第二层子表的第一次加载
                        foreach (var v in obj)
                        {
                            bool nfig1 = true;//第二层子表的未生成Xml过                   
                            bool tf1 = true;//第二层子表的未加载过
                            v.GetType();
                            Type objtype = v.GetType();
                            PropertyInfo[] propinfos = objtype.GetProperties();
                            string xTableName = itemObjectList.Elements("Object").FirstOrDefault().Attribute("Name").Value;
                            if (xTableName != null && xTableName.ToUpper() == objtype.Name.ToUpper())
                            {
                                foreach (PropertyInfo propinfo in propinfos)
                                {
                                    //取数据值
                                    string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;

                                    if (fig1)
                                    {
                                        #region 第一次加载Xml模板中的第二层子表
                                        IEnumerable<XElement> lastDetails = itemObjectList.Elements("Object");
                                        foreach (XElement lastDetail in lastDetails)
                                        {
                                            if (lastDetail.Attribute("Name") != null && lastDetail.Attribute("Name").Value == objtype.Name.ToUpper())
                                            {
                                                XElemertFormat(lastDetail, keyValue, objtype, propinfo, AutoList);
                                                tf1 = false;
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 第二次以上加载Xml模板中的子表

                                        XElement objectlist = itemObjectList;
                                        //查询Xml子表节点
                                        XElement xDetailFirst = itemObjectList.Elements("Object").FirstOrDefault();
                                      
                                            if (xDetailFirst != null && xDetailFirst.Attribute("Name") != null && xDetailFirst.Attribute("Name").Value == objtype.Name.ToUpper())
                                            {
                                                //生成Xml子表节点
                                                if (nfig1)
                                                {
                                                    objectlist = CreateNodeXml(xDetailFirst, objectlist);
                                                    nfig1 = false;
                                                }
                                                //绑定数据
                                                XElement lastx = objectlist.Elements("Object").LastOrDefault();
                                                XElemertFormat(lastx, keyValue, objtype, propinfo, AutoList);
                                            }
                                      
                                        #endregion
                                    }
                                }
                            }
                            if (!tf1)
                            {
                                fig1 = false;
                            }
                        }

                        #endregion

                        #region 第三层子表的实体映射
                        List<bool> fig = new List<bool>();//第三层子表的第一次加载
                        List<bool> afig = new List<bool>();//第三层子表的未生成Xml过
                        List<bool> atf = new List<bool>();//第三层子表的未加载过
                        int i = 0;
                        foreach (var v in obj)
                        {
                            fig.Add(true);
                            afig.Add(true);
                            atf.Add(true);
                            i++;
                            v.GetType();
                            Type objtype = v.GetType();
                            PropertyInfo[] propinfos = objtype.GetProperties();
                            string xTableName = itemObjectList.Elements("Object").Elements("ObjectList").Elements("Object").FirstOrDefault().Attribute("Name").Value;
                            if (xTableName != null && xTableName.ToUpper() == objtype.Name.ToUpper())
                            {
                                IEnumerable<XElement> xlistdd = itemObjectList.Elements("Object");
                                foreach (XElement xDetailD in xlistdd)
                                {
                                    if (xDetailD.Elements("ObjectList").FirstOrDefault().Attribute("ParentName") != null)
                                    {
                                        IEnumerable a = propinfos.Where(item => item.Name.ToUpper() == xDetailD.Elements("ObjectList").FirstOrDefault().Attribute("ParentName").Value.ToUpper() && item.GetValue(v, null).ToString().ToLower() == xDetailD.Attribute("id").Value.ToLower());
                                        foreach (var b in a)
                                        {
                                            foreach (PropertyInfo propinfo in propinfos)
                                            {
                                                //取数据值
                                                string keyValue = propinfo.GetValue(v, null) != null ? propinfo.GetValue(v, null).ToString() : string.Empty;
                                                IEnumerable<XElement> listDetail2 = xDetailD.Elements("ObjectList");
                                                foreach (XElement x in listDetail2)
                                                {
                                                    if ((bool)fig[i - 1])
                                                    {
                                                        #region 第一次加载Xml模板中的第三层子表
                                                        IEnumerable<XElement> lastDetail2s = x.Elements("Object");
                                                        foreach (XElement lastDetail2 in lastDetail2s)
                                                        {
                                                            if (lastDetail2.Attribute("Name") != null && lastDetail2.Attribute("Name").Value == objtype.Name.ToUpper())
                                                            {
                                                                XElemertFormat(lastDetail2, keyValue, objtype, propinfo, AutoList);
                                                                afig[i - 1] = false;
                                                            }
                                                        }

                                                        //处理ParentID数据
                                                        XElemertListKey(x, keyValue, objtype, propinfo, AutoList);
                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        #region 第二次以上加载Xml模板中的第三层子表

                                                        //查询Xml子表节点
                                                        XElement xDetaillist2 = x.Elements("Object").Elements("ObjectList").FirstOrDefault();
                                                        XElement xDetailFirst2 = x.Elements("Object").Elements("ObjectList").Elements("Object").FirstOrDefault();
                                                        //foreach (XElement lastDetail in xDetaillist2)
                                                        //{
                                                            if (xDetailFirst2 != null && xDetailFirst2.Attribute("Name") != null && xDetailFirst2.Attribute("Name").Value == objtype.Name.ToUpper())
                                                            {
                                                                //生成Xml子表节点
                                                                if ((bool)atf[i - 1])
                                                                {
                                                                    xDetaillist2 = CreateNodeXml(xDetailFirst2, xDetaillist2);
                                                                    atf[i - 1] = false;
                                                                }

                                                                //绑定数据
                                                                XElement lastx = x.Elements("Object").Elements("ObjectList").Elements("Object").LastOrDefault();
                                                                XElemertFormat(lastx, keyValue, objtype, propinfo, AutoList);
                                                            }
                                                       // }
                                                        #endregion

                                                        //处理ParentID数据
                                                        XElemertListKey(x, keyValue, objtype, propinfo, AutoList);

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (!(bool)atf[i - 1])
                            {
                                fig[i - 1] = false;
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    error = "objList is Error";
                    return string.Empty;
                }
                #endregion

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine(xml.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                error += " " + ex.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// Xml节点数据处理
        /// </summary>
        /// <param name="XItem"></param>
        /// <param name="Value"></param>
        /// <param name="objtype"></param>
        /// <param name="propinfo"></param>
        /// <param name="AutoList"></param>
        private void XElemertFormat(XElement XItem, string Value, Type objtype, PropertyInfo propinfo, List<AutoDictionary> AutoList)
        {
            if (XItem != null && XItem.Attribute("Name") != null && XItem.Attribute("Name").Value == objtype.Name.ToUpper())
            {
                //处理Key值
                if (Value != string.Empty && XItem.Attribute("Key") != null && XItem.Attribute("id") != null && XItem.Attribute("Key").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                {
                    XItem.Attribute("id").SetValue(Value);
                }

                //读取Xml表节点
                IEnumerable<XElement> xElementList = XItem.Elements("Attribute");
                foreach (XElement item in xElementList)
                {
                    if (Value != string.Empty && item.Attribute("Name").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
                    {
                        item.Attribute("DataValue").SetValue(Value);
                        //item.Attribute("DataText").SetValue(Value);
                        item.Attribute("Description").SetValue(GetResourceStr(propinfo.Name));
                    }
                    //处理需转换值的列表
                    foreach (AutoDictionary Dictionary in AutoList)
                    {
                        if (item.Attribute("Name").Value.ToUpper() == Dictionary.Name.ToUpper() && objtype.Name.ToUpper() == Dictionary.TableName.ToUpper() && Value == Dictionary.DataValue)
                        {
                            item.Attribute("DataValue").SetValue(Dictionary.DataValue);
                            item.Attribute("DataText").SetValue(Dictionary.DataText);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Xml父节点数据处理
        /// </summary>
        /// <param name="XItem"></param>
        /// <param name="Value"></param>
        /// <param name="objtype"></param>
        /// <param name="propinfo"></param>
        /// <param name="AutoList"></param>
        private void XElemertListKey(XElement XItem, string Value, Type objtype, PropertyInfo propinfo, List<AutoDictionary> AutoList)
        {
            //处理Key值
            if (Value != string.Empty && XItem.Attribute("ParentName") != null && XItem.Attribute("ParentID") != null && XItem.Attribute("ParentName").Value.ToUpper().ToString() == propinfo.Name.ToUpper().ToString())
            {
                XItem.Attribute("ParentID").SetValue(Value);
            }
        }

        /// <summary>
        /// 生成节点XML
        /// </summary>
        /// <param name="xFirst"></param>
        /// <param name="xlist"></param>
        /// <returns></returns>
        private XElement CreateNodeXml(XElement xFirst, XElement xlist)
        {
            //生成节点XML           
               XElement xObjectDetail = new XElement("Object");
                foreach (var nv in xFirst.Attributes())
                {
                    xObjectDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                }

                foreach (XElement xda in xlist.Elements("Object").FirstOrDefault().Elements("Attribute"))
                {
                    XElement xAttributeDetail = new XElement("Attribute");
                    foreach (var nv in xda.Attributes())
                    {                       
                        xAttributeDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                    }
                    xObjectDetail.Add(xAttributeDetail);
                }

                //子表的子表添加
                XElement xLast = xFirst.Elements("ObjectList").Elements("Object").FirstOrDefault();
                if (xLast != null)
                {
                    XElement xObjectList = new XElement("ObjectList");
                      foreach (var nv in xFirst.Elements("Object").Elements("ObjectList").Attributes())
                        {
                            xObjectList.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                        }
                        xObjectDetail.Add(xObjectList);
                 
                    XElement xObjectDetail1 = new XElement("Object");
                    foreach (XElement xda in xlist.Elements("Object").Elements("ObjectList").Elements("Object"))
                    {
                        foreach (var nv in xda.Attributes())
                        {
                            xObjectDetail1.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                        }
                        xObjectList.Add(xObjectDetail1);
                    }

                    foreach (XElement xda in xlist.Elements("Object").Elements("ObjectList").Elements("Object").FirstOrDefault().Elements("Attribute"))
                    {
                        XElement xAttributeDetail = new XElement("Attribute");
                        foreach (var nv in xda.Attributes())
                        {
                            xAttributeDetail.Add(new XAttribute(nv.Name.ToString(), nv.Value));
                        }
                        xObjectDetail.Add(xAttributeDetail);
                    }

                xlist.Add(xObjectDetail);
            }
            return xlist;
        }

        /// <summary>
        /// 获取资源文件
        /// </summary>
        /// <param name="ResourceID">资源文件ID</param>
        /// <returns></returns>
        private static string GetResourceStr(string ResourceID)
        {
            string rslt = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString(ResourceID, SMT.SaaS.Globalization.Localization.UiCulture);
            return string.IsNullOrEmpty(rslt) ? ResourceID : rslt;
        }

        /// <summary>
        /// 返回错误结果
        /// </summary>
        public string Error
        {
            get { return error; }
        }
    }

    /// <summary>
    /// 需转换值的列表功能
    /// </summary>
    public class AutoDictionary
    {
        /// <summary>
        /// 需转换值
        /// </summary>
        public AutoDictionary()
        {

        }
       
        private string tablename = string.Empty;
        private string name = string.Empty;
        private string datavalue = string.Empty;
        private string datatext = string.Empty;
        private string keyvalue = string.Empty;//主键值
       
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName
        {
            get { return tablename; }
            set { tablename = value; }
        }
        /// <summary>
        /// 字段名
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        /// <summary>
        /// 字段值
        /// </summary>
        public string DataValue
        {
            get { return datavalue; }
            set { datavalue = value; }
        }
        /// <summary>
        /// 字段转换值
        /// </summary>
        public string DataText
        {
            get { return datatext; }
            set { datatext = value; }
        }
        /// <summary>
        /// 主键值
        /// </summary>
        public string KeyValue
        {
            get { return keyvalue; }
            set { keyvalue = value; }
        }
        /// <summary>
        /// 数组填充
        /// </summary>
        /// <param name="str">string[4]数组：tablename表名、name字段名、datavalue字段名、datatext字段转换值</param>
        public void AutoDictionaryList(string[] str)
        {
            this.tablename = str[0];
            this.name = str[1];
            this.datavalue = str[2];
            this.datatext = str[3];

        }

        /// <summary>
        /// 数组填充
        /// </summary>
        /// <param name="str">string[4]数组：tablename表名、name字段名、datavalue字段名、datatext字段转换值、keyvalue 外键值ID</param>
        public void AutoDictionaryChiledList(string[] str)
        {
            this.tablename = str[0];
            this.name = str[1];
            this.datavalue = str[2];
            this.datatext = str[3];
            this.keyvalue = str[4];

        }
    }
}

