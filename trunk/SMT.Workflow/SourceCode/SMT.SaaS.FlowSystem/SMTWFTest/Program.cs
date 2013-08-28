using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Activities;
using System.Workflow.ComponentModel.Compiler;
using System.Collections.ObjectModel;
using SMT.WFLib;
using System.IO;
using System.Xml;
using SMT.FLOWDAL;
using System.Data.Entity;

using System.ServiceModel;
using System.Configuration;
using WFTools.Services.Persistence.Ado;
using WFTools.Services.Tracking.Ado;
using WFTools.Services.Batching.Ado;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.ServiceModel.Description;
using SMT.FLOWDAL.ADO;
using System.Data.OracleClient;
using SMT.Workflow.Common.DataAccess;
using System.Xml.Linq;
using System.Runtime.Serialization;


namespace SMTWFTest
{
    class Program
    {
       // public static string GetTypeName(EntityObject t)
      //  {
            //if (t.GetType().BaseType != typeof(EntityObject))
            //{
            //    return "类不是规定的类型";

            //}
         //   Flow_FlowRecord_T cc = t as Flow_FlowRecord_T;
         //   return (t as Flow_FlowRecord_T).CreateCompanyID;
            //return cc.CreateCompanyID;
           
            //PropertyInfo[] propinfos = null;//属性
            //string tableName = null;
            //if (propinfos == null)
            //{
            //    Type objtype = cc.GetType();
            //    propinfos = objtype.GetProperties();
            //    //tableName = objtype.Name;
            //}
            //for (int i = 0; i < propinfos.Length; i++)
            //{
            //    tableName += "," + propinfos[i].Name + "," + propinfos[i].GetValue(cc, null);
            //    //propinfos[i].GetValue(a, null);
            //}
            //return tableName;
            //return t.GetType().BaseType.ToString ();
        //   }
        #region 龙康才测试ADO事务
        #region 对象
        /// <summary>
        /// [测试表]
        /// </summary>
        public class SMT_TEST
        {
            /// <summary>
            /// 主键ID
            /// </summary>
            public string TESTID { get; set; }
            /// <summary>
            /// 用户名
            /// </summary>
            public string USERID { get; set; }
            /// <summary>
            /// 密码
            /// </summary>
            public string PASSWORD { get; set; }
            /// <summary>
            /// 年龄
            /// </summary>
            public decimal? AGE { get; set; }
            /// <summary>
            /// 生日
            /// </summary>
            public DateTime? BIRTHDAY { get; set; }
            /// <summary>
            /// 备注
            /// </summary>
            public string REMARK { get; set; }

        }
        #endregion
        /// <summary>
        /// 增加一条数据(以参数传值)
        /// </summary>
        public static int Add(OracleConnection conn,SMT_TEST model)
        {


            string insSql = "INSERT INTO SMT_TEST (TESTID,USERID,PASSWORD,AGE,BIRTHDAY,REMARK) VALUES (:TESTID,:USERID,:PASSWORD,:AGE,:BIRTHDAY,:REMARK)";
            OracleParameter[] pageparm =
                {               
                    new OracleParameter(":TESTID",OracleType.NVarChar), 
                    new OracleParameter(":USERID",OracleType.NVarChar), 
                    new OracleParameter(":PASSWORD",OracleType.NVarChar), 
                    new OracleParameter(":AGE",OracleType.Int32), 
                    new OracleParameter(":BIRTHDAY",OracleType.DateTime), 
                    new OracleParameter(":REMARK",OracleType.Clob) 

                };
            pageparm[0].Value = model.TESTID;//主键ID
            pageparm[1].Value = model.USERID;//用户名
            pageparm[2].Value = model.PASSWORD;//密码
            pageparm[3].Value = model.AGE;//年龄
            pageparm[4].Value = model.BIRTHDAY;//生日
            pageparm[5].Value = model.REMARK;//备注

            return MsOracle.ExecuteSQLByTransaction(conn,insSql, pageparm);
        }
        private static void TestAdo()
        {
            
          OracleConnection conn=  ADOHelper.GetOracleConnection();
       
          SMT_TEST entity = new SMT_TEST();
          entity.TESTID =Guid.NewGuid().ToString().Replace("-","");//主键ID
          entity.USERID ="中国人";//用户名
          entity.PASSWORD = "abc";//密码
          entity.AGE =32;//年龄
          entity.BIRTHDAY =DateTime.Now;//生日
          entity.REMARK = "就是备注";//备注
          MsOracle.BeginTransaction(conn);


         // Add(conn,entity);       
          Add(conn, entity);
          entity.TESTID = Guid.NewGuid().ToString().Replace("-", "");//主键ID
          entity.AGE = 23;//年龄
          Add(conn, entity);  
            MsOracle.CommitTransaction(conn);
        }
        #endregion
        /// <summary>
        /// [待办任务列表]
        /// </summary>
        [DataContract]
        public class T_WF_DOTASK
        {

            /// <summary>
            /// 创建日期
            /// </summary>
            [DataMember]
            public DateTime CREATEDATETIME { get; set; }

            /// <summary>
            /// 备注
            /// </summary>
            [DataMember]
            public string REMARK { get; set; }

            /// <summary>
            /// 待办任务ID
            /// </summary>
            [DataMember]
            public string DOTASKID { get; set; }

            /// <summary>
            /// 公司ID
            /// </summary>
            [DataMember]
            public string COMPANYID { get; set; }

            /// <summary>
            /// 单据ID
            /// </summary>
            [DataMember]
            public string ORDERID { get; set; }

            /// <summary>
            /// 单据所属人ID
            /// </summary>
            [DataMember]
            public string ORDERUSERID { get; set; }

            /// <summary>
            /// 单据所属人名称
            /// </summary>
            [DataMember]
            public string ORDERUSERNAME { get; set; }

            /// <summary>
            /// 单据状态
            /// </summary>
            [DataMember]
            public decimal ORDERSTATUS { get; set; }

            /// <summary>
            /// 消息体
            /// </summary>
            [DataMember]
            public string MESSAGEBODY { get; set; }

            /// <summary>
            /// 应用URL
            /// </summary>
            [DataMember]
            public string APPLICATIONURL { get; set; }

            /// <summary>
            /// 接收用户ID
            /// </summary>
            [DataMember]
            public string RECEIVEUSERID { get; set; }

            /// <summary>
            /// 处理剩余时间（主要针对KPI考核）
            /// </summary>
            [DataMember]
            public DateTime? BEFOREPROCESSDATE { get; set; }

            /// <summary>
            /// 待办任务类型(0、待办任务、1、流程咨询、3 )
            /// </summary>
            [DataMember]
            public decimal DOTASKTYPE { get; set; }

            /// <summary>
            /// 待办关闭时间
            /// </summary>
            [DataMember]
            public DateTime CLOSEDDATE { get; set; }

            /// <summary>
            /// 引擎代码
            /// </summary>
            [DataMember]
            public string ENGINECODE { get; set; }

            /// <summary>
            /// 代办任务状态(0、未处理 1、已处理 、2、任务撤销 )
            /// </summary>
            [DataMember]
            public decimal DOTASKSTATUS { get; set; }

            /// <summary>
            /// 邮件状态(0、未发送 1、已发送、2、未知 )
            /// </summary>
            [DataMember]
            public decimal MAILSTATUS { get; set; }

            /// <summary>
            /// RTX状态(0、未发送 1、已发送、2、未知 )
            /// </summary>
            [DataMember]
            public decimal RTXSTATUS { get; set; }

            /// <summary>
            /// 是否已提醒(0、未提醒 1、已提醒、2、未知 )
            /// </summary>
            [DataMember]
            public decimal ISALARM { get; set; }

            /// <summary>
            /// 应用字段值
            /// </summary>
            [DataMember]
            public string APPFIELDVALUE { get; set; }

            /// <summary>
            /// 流程XML
            /// </summary>
            [DataMember]
            public string FLOWXML { get; set; }

            /// <summary>
            /// 应用XML
            /// </summary>
            [DataMember]
            public string APPXML { get; set; }

            /// <summary>
            /// 系统代码
            /// </summary>
            [DataMember]
            public string SYSTEMCODE { get; set; }

            /// <summary>
            /// 模块代码
            /// </summary>
            [DataMember]
            public string MODELCODE { get; set; }

            /// <summary>
            /// 模块名称
            /// </summary>
            [DataMember]
            public string MODELNAME { get; set; }

        }
        /// <summary>
        /// 根据节点属性得到值（公共方法）
        /// </summary>
        /// <param name="xelement">XML节点</param>
        /// <param name="strAttrName">属性</param>
        /// <returns></returns>
        private static string XMLToAttribute(XElement xelement, string strAttrName)
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
        /// 特殊模块需要改变接收人
        /// </summary>
        /// <returns></returns>
        private static bool CheckModelName(string strModelName)
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
        private static string BOObjectEscapeString(string strBOObject)
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

                }
                return sb.ToString();
            }
            return string.Empty;
        }
        /// <summary>
        /// 将业务数据字段，组合成以Ё分割的字符串
        /// </summary>
        /// <param name="xe"></param>
        /// <returns></returns>
        private static string ConbinString(XElement xe)
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
        private static string GetAttribute(XmlReader xr, string attrName,string attrValue)
        {
            string value = "";
            if (xr[attrName].ToUpper() == attrValue.ToUpper())
            {
                value= xr["DataValue"];
            }
            return value;
        }
        static void Main(string[] args)
        {         
        
            #region 
            #region flow
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<System>");
            sb.AppendLine("	<Name>\"Flow\"</Name>");
            sb.AppendLine("	<SystemCode>\"EDM\"</SystemCode>");
            sb.AppendLine("	<Message>");
            sb.AppendLine("		<Attribute Name=\"CompanyID\" DataValue=\"8350ad70-4fac-476d-b008-2ec506362480\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("		<Attribute Name=\"ModelCode\" DataValue=\"T_EDM_APPLYFORMASTER\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("		<Attribute Name=\"ModelName\" DataValue=\"采购申请\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("		<Attribute Name=\"FormID\" DataValue=\"609b4d22-b9ff-4545-a5df-c0b4ff4328e6\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("		<Attribute Name=\"StateCode\" DataValue=\"Stateabd697bc1f834a288ed2b085dad8478a\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("		<Attribute Name=\"CheckState\" DataValue=\"1\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("		<Attribute Name=\"IsTask\" DataValue=\"1\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("		<Attribute Name=\"AppUserID\" DataValue=\"0276288d-ab8e-41ed-abc5-cee659e0909f\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("		<Attribute Name=\"AppUserName\" DataValue=\"进销存测试公司-进销存行政部-进销存老板-进销存老板\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("		<Attribute Name=\"OutTime\" DataValue=\"\">");
            sb.AppendLine("		</Attribute>");
            sb.AppendLine("	</Message>");
            sb.AppendLine("</System>");
            #endregion
            #region strBusiness
            StringBuilder strBusiness = new StringBuilder();
            strBusiness.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            strBusiness.AppendLine("<System>");
            strBusiness.AppendLine("	<Name>EDM</Name>");
            strBusiness.AppendLine("	<Object Name=\"Approval\" Description=\"\">");
            strBusiness.AppendLine("		<Attribute Name=\"APPLYFORCODE\" Description=\"\" DataType=\"\" DataValue=\"CS01-20111228-0049\" />");
            strBusiness.AppendLine("		<Attribute Name=\"APPLYFORDATE\" Description=\"\" DataType=\"\" DataValue=\"2011/12/28 0:00:00\" />");
            strBusiness.AppendLine("		<Attribute Name=\"APPLYFORMASTERID\" Description=\"\" DataType=\"\" DataValue=\"609b4d22-b9ff-4545-a5df-c0b4ff4328e6\" />");
            strBusiness.AppendLine("		<Attribute Name=\"APPLYFORUSER\" Description=\"\" DataType=\"\" DataValue=\"0276288d-ab8e-41ed-abc5-cee659e0909f\" />");
            strBusiness.AppendLine("		<Attribute Name=\"AUDITSTATE\" Description=\"\" DataType=\"\" DataValue=\"0\" />");
            strBusiness.AppendLine("		<Attribute Name=\"CGFLOWMASTERID\" Description=\"\" DataType=\"\" DataValue=\"\" />");
            strBusiness.AppendLine("		<Attribute Name=\"CREATECOMPANYID\" Description=\"\" DataType=\"\" DataValue=\"8350ad70-4fac-476d-b008-2ec506362480\" />");
            strBusiness.AppendLine("		<Attribute Name=\"CREATEDATE\" Description=\"\" DataType=\"\" DataValue=\"2011/12/28 0:00:00\" />");
            strBusiness.AppendLine("		<Attribute Name=\"CREATEDEPARTMENTID\" Description=\"\" DataType=\"\" DataValue=\"22ee79f9-cf7e-47f3-85c1-4b2ac6012adc\" />");
            strBusiness.AppendLine("		<Attribute Name=\"CREATEPOSTID\" Description=\"\" DataType=\"\" DataValue=\"47d387db-182d-4fe5-867d-18af89fdcf06\" />");
            strBusiness.AppendLine("		<Attribute Name=\"CREATEUSERID\" Description=\"\" DataType=\"\" DataValue=\"0276288d-ab8e-41ed-abc5-cee659e0909f\" />");
            strBusiness.AppendLine("		<Attribute Name=\"CREATEUSERNAME\" Description=\"\" DataType=\"\" DataValue=\"进销存老板\" />");
            strBusiness.AppendLine("		<Attribute Name=\"OPERATEUSERNAME\" Description=\"\" DataType=\"\" DataValue=\"进销存老板\" />");
            strBusiness.AppendLine("		<Attribute Name=\"OWNERCOMPANYID\" Description=\"\" DataType=\"\" DataValue=\"8350ad70-4fac-476d-b008-2ec506362480\" />");
            strBusiness.AppendLine("		<Attribute Name=\"OWNERDEPARTMENTID\" Description=\"\" DataType=\"\" DataValue=\"22ee79f9-cf7e-47f3-85c1-4b2ac6012adc\" />");
            strBusiness.AppendLine("		<Attribute Name=\"OWNERID\" Description=\"\" DataType=\"\" DataValue=\"0276288d-ab8e-41ed-abc5-cee659e0909f\" />");
            strBusiness.AppendLine("		<Attribute Name=\"OWNERNAME\" Description=\"\" DataType=\"\" DataValue=\"进销存行政部\" />");
            strBusiness.AppendLine("		<Attribute Name=\"OWNERPOSTID\" Description=\"\" DataType=\"\" DataValue=\"47d387db-182d-4fe5-867d-18af89fdcf06\" />");
            strBusiness.AppendLine("		<Attribute Name=\"PURCHASETYPE\" Description=\"\" DataType=\"\" DataValue=\"1\" />");
            strBusiness.AppendLine("		<Attribute Name=\"REMARKS\" Description=\"\" DataType=\"\" DataValue=\"\" />");
            strBusiness.AppendLine("		<Attribute Name=\"T_EDM_APPLYFORDETAIL\" Description=\"\" DataType=\"\" DataValue=\"System.Collections.ObjectModel.ObservableCollection`1[SMT.EDM.UI.EntityRef.T_EDM_APPLYFORDETAIL]\" />");
            strBusiness.AppendLine("		<Attribute Name=\"UPDATEDATE\" Description=\"\" DataType=\"\" DataValue=\"2011/12/28 0:00:00\" />");
            strBusiness.AppendLine("		<Attribute Name=\"UPDATEUSERID\" Description=\"\" DataType=\"\" DataValue=\"0276288d-ab8e-41ed-abc5-cee659e0909f\" />");
            strBusiness.AppendLine("		<Attribute Name=\"UPDATEUSERNAME\" Description=\"\" DataType=\"\" DataValue=\"进销存老板\" />");
            strBusiness.AppendLine("		<Attribute Name=\"EntityKey\" Description=\"\" DataType=\"\" DataValue=\"SMT.EDM.UI.EntityRef.EntityKey\" />");
            strBusiness.AppendLine("		<Attribute Name=\"CURRENTEMPLOYEENAME\" Description=\"提交者\" DataType=\"\" DataValue=\"进销存老板\" />");
            strBusiness.AppendLine("	</Object>");
            strBusiness.AppendLine("</System>");
              #endregion
            #endregion
            string XElementString = "";
            string XmlReaderString = "";
            #region XmlReader
            DateTime dta = DateTime.Now;
            T_WF_DOTASK model = new T_WF_DOTASK();
            string modelType = string.Empty;//EntityType (表名)
            string modelKey = string.Empty;//EntityKey (主键)
            string modelIsTask = "1";//是否任务
            #region FLOW

            StringReader strRdr = new StringReader(sb.ToString());
            XmlReader xr = XmlReader.Create(strRdr);
            StringBuilder _ConbinString = new StringBuilder();
            while (xr.Read())
            {
                if (xr.NodeType == XmlNodeType.Element)
                {
                    string elementName = xr.Name;
                    if (elementName == "Message")
                    {
                        while (xr.Read())
                        {
                            string type = xr.NodeType.ToString();
                            #region

                            if (xr["Name"] != null)
                            {
                                _ConbinString.Append(xr["Name"] + "|" + xr["DataValue"] + "Ё");
                                if (xr["Name"].ToUpper() == "COMPANYID")
                                {
                                    model.COMPANYID = xr["DataValue"];
                                }
                                if (xr["Name"].ToUpper() == "MODELCODE")
                                {
                                    model.MODELCODE = xr["DataValue"];
                                }
                                if (xr["Name"].ToUpper() == "MODELNAME")
                                {
                                    model.MODELNAME = xr["DataValue"];
                                }
                                if (xr["Name"].ToUpper() == "FORMID")
                                {
                                    model.ORDERID = xr["DataValue"];
                                }
                                if (xr["Name"].ToUpper() == "CHECKSTATE")
                                {
                                    model.ORDERSTATUS = int.Parse(xr["DataValue"]);
                                }
                                if (xr["Name"].ToUpper() == "APPUSERID")
                                {
                                    model.RECEIVEUSERID = xr["DataValue"];
                                }
                                if (xr["Name"].ToUpper() == "ISTASK")
                                {
                                    modelIsTask = xr["DataValue"];
                                }
                                if (xr["Name"].ToUpper() == "OUTTIME")
                                {

                                    if (!string.IsNullOrEmpty(xr["DataValue"]))
                                    {
                                        model.BEFOREPROCESSDATE = DateTime.Now.AddMinutes(int.Parse(xr["DataValue"]));
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
                model.FLOWXML = sb.ToString();
                model.APPXML = strBusiness.ToString();
            }
            #endregion
            if (!string.IsNullOrEmpty(strBusiness.ToString()))
            {
                StringReader rd = new StringReader(strBusiness.ToString());
                XmlReader xdr = XmlReader.Create(rd);
                StringBuilder BOObject = new StringBuilder();
                while (xdr.Read())
                {
                    if (xdr.NodeType == XmlNodeType.Element)
                    {
                        string elementName = xdr.Name;
                        if (elementName == "Name")
                        {
                            while (xdr.Read())
                            {
                                model.SYSTEMCODE = xdr.Value;
                                break;
                            }
                        }
                        if (elementName == "Object")
                        {
                            try
                            { //非手机的XML时没有表明和主键的
                                modelType = xdr["Name"];
                                modelKey = xdr["Key"];
                            }
                            catch
                            {
                                //Log.WriteLog("异常无法取得业务数据表，单据:" + entity.ORDERID + "：表：" + strEntityType + ":主键：" + strEntityKey + "");
                            }
                            while (xdr.Read())
                            {
                                if (xdr.Name == "Attribute")
                                {
                                    if (xdr["Name"] != null)
                                    {
                                        BOObject.Append(xdr["Name"] + "|" + xdr["DataValue"] + "Г" + (xdr["DataText"] != null ? xdr["DataText"] : xdr["DataValue"]) + "Ё");
                                    }
                                    #region
                                    if (xdr["Name"].ToUpper() == "OWNERID")
                                    {
                                        model.ORDERUSERID = xdr["DataValue"];
                                    }
                                    if (xdr["Name"].ToUpper() == "OWNERNAME")
                                    {
                                        model.ORDERUSERNAME = xdr["DataValue"];
                                    }
                                    if (xdr["Name"].ToUpper() == "CREATEUSERID")
                                    {
                                        //有些特殊的模块需要改变接收人
                                        if (CheckModelName(model.MODELCODE) && model.ORDERSTATUS == 2)
                                        {
                                            model.RECEIVEUSERID = xdr["DataValue"];
                                        }
                                    }

                                    #endregion
                                }
                            }
                        }
                    }
                }
                model.APPFIELDVALUE = _ConbinString.ToString() + BOObject.ToString().TrimEnd('Ё');
                XmlReaderString = model.APPFIELDVALUE;
            }
            DateTime dtb = DateTime.Now;
            Console.WriteLine("新方法:{0}毫秒。", (dtb - dta).TotalMilliseconds);
            #endregion
            #region XElement

            //DateTime dt1 = DateTime.Now;
            //T_WF_DOTASK entity = new T_WF_DOTASK();
            //string strEntityType = string.Empty;//EntityType (表名)
            //string strEntityKey = string.Empty;//EntityKey (主键)
            //string IsTask = "1";//是否任务
            //Byte[] bFlow = System.Text.UTF8Encoding.UTF8.GetBytes(sb.ToString());
            //XElement xeFlow = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(bFlow)));
            //entity.COMPANYID = XMLToAttribute(xeFlow, "COMPANYID");
            //entity.MODELCODE = XMLToAttribute(xeFlow, "MODELCODE");
            //entity.MODELNAME = XMLToAttribute(xeFlow, "ModelName");
            //entity.ORDERID = XMLToAttribute(xeFlow, "FORMID");
            //entity.ORDERSTATUS = int.Parse(XMLToAttribute(xeFlow, "CheckState"));
            //entity.RECEIVEUSERID = XMLToAttribute(xeFlow, "APPUSERID");
            //entity.FLOWXML = sb.ToString();
            //entity.APPXML = strBusiness.ToString();
            //IsTask = XMLToAttribute(xeFlow, "IsTask");
            //if (!string.IsNullOrEmpty(XMLToAttribute(xeFlow, "OutTime")))
            //{
            //    entity.BEFOREPROCESSDATE = DateTime.Now.AddMinutes(int.Parse(XMLToAttribute(xeFlow, "OutTime")));
            //}
            //if (!string.IsNullOrEmpty(strBusiness.ToString()))
            //{
            //    Byte[] BBusiness = System.Text.UTF8Encoding.UTF8.GetBytes(strBusiness.ToString());
            //    XElement xeBusiness = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(BBusiness)));
            //    entity.SYSTEMCODE = (from item in xeBusiness.Descendants("Name") select item).FirstOrDefault().Value;
            //    try
            //    { //非手机的XML时没有表明和主键的
            //        strEntityType = (from item in xeBusiness.Descendants("Object") select item).FirstOrDefault().Attribute("Name").Value;
            //        strEntityKey = (from item in xeBusiness.Descendants("Object") select item).FirstOrDefault().Attribute("Key").Value;
            //    }
            //    catch
            //    {
            //        //Log.WriteLog("异常无法取得业务数据表，单据:" + entity.ORDERID + "：表：" + strEntityType + ":主键：" + strEntityKey + "");
            //    }
            //    entity.ORDERUSERID = (from item in xeBusiness.Descendants("Object").Descendants("Attribute")
            //                          where item.Attribute("Name").Value.ToUpper() == "OWNERID"
            //                          select item).FirstOrDefault().Attribute("DataValue").Value;
            //    entity.ORDERUSERNAME = (from item in xeBusiness.Descendants("Object").Descendants("Attribute")
            //                            where item.Attribute("Name").Value.ToUpper() == "OWNERNAME"
            //                            select item).FirstOrDefault().Attribute("DataValue").Value;
            //    //有些特殊的模块需要改变接收人
            //    if (CheckModelName(entity.MODELCODE) && entity.ORDERSTATUS == 2)
            //    {
            //        entity.RECEIVEUSERID = (from item in xeBusiness.Descendants("Object").Descendants("Attribute")
            //                                where item.Attribute("Name").Value.ToUpper() == "CREATEUSERID"
            //                                select item).FirstOrDefault().Attribute("DataValue").Value;
            //    }
            //    entity.APPFIELDVALUE = ConbinString(xeFlow) + BOObjectEscapeString(strBusiness.ToString());
            //    XElementString = entity.APPFIELDVALUE;
            //}
            //DateTime dt2 = DateTime.Now;
            //Console.WriteLine("原方法:{0}毫秒。", (dt2 - dt1).TotalMilliseconds);
            
            #endregion
         

                //Console.WriteLine("＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝原方法＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝");
                //Console.WriteLine("{0}", XElementString);
                //Console.WriteLine("＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝新方法＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝");
                //Console.WriteLine("{0}", XmlReaderString);
            Console.Read();
           // TestAdo();
            return;
            //FLOW_FLOWRECORDDETAIL_TDAL as1 = new FLOW_FLOWRECORDDETAIL_TDAL();
            //var sd = as1.DataContext;
          //  string sdfs = "";
            // FlowSysDAL Dal = new FlowSysDAL();
            //Flow_SysDictionary_TDAL dal = new Flow_SysDictionary_TDAL();
            //var q = (from ent in Dal.GetSysDictionary()
            //         select ent).ToList();
            //Console.WriteLine(q.First().CnName);

            //Flow_FlowRecord_T entity = new Flow_FlowRecord_T();

            //entity.CreateCompanyID = "1";
            ////entity.Content = "dfwfw";
            //entity.CreateDate = DateTime.Now;
            ////entity.EditDate = DateTime.Now;
            //entity.EditUserID = "6464";
            //entity.Flag = "1";
            //entity.FlowCode = "gefege";
            //entity.FormID = "jthr";
            //entity.GUID = Guid.NewGuid().ToString();
            //entity.InstanceID = "gsdfwfw";
            //entity.ModelCode = "hefefe";
            //entity.CreatePostID = "hdvfge";
            //entity.ParentStateCode = "dgbfwe";
            //entity.StateCode = "hfewgeh";
            //entity.CreateUserID = "sh";

            //Console.WriteLine(GetTypeName(entity));
            //Dal.AddFlowRecord(entity);
            
            //Flow_SysDictionary_T entity = new Flow_SysDictionary_T();
            //entity.CnName = "";
            //entity.CreateDate = DateTime.Now;
            //entity.CreateUserId = "sdf";
            //entity.EditDate = DateTime.Now;
            //entity.EditUserId = "sdf";
            //entity.EnName = "sdf";
            //entity.GUID = Guid.NewGuid().ToString();
            //entity.HkName = "sf";
            //entity.Key = "Asdf";

            //Flow_ModelDefine_T entity = new Flow_ModelDefine_T();
            //entity.CreateDate = DateTime.Now;
            //entity.CreateUserID = "gsgg";
            //entity.Description = "gsgf";
            //entity.EditDate = DateTime.Now;
            //entity.EditUserID = "gsg";
            //entity.GUID = Guid.NewGuid().ToString();
            //entity.ModelCode = "ggfwge";

            //  Dal.AddModelDefine(entity);

             string sss = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<System>\r\n<Name>OA</Name>\r\n<Object Name=\"T_OA_BUSINESSREPORT\" Description=\"\">\r\n<Attribute Name=\"BUSINESSREPORTID\" Description=\"\" DataType=\"\" DataValue=\"005e2e52-bc2d-4801-a524-4411a1d7de2e\"/>\r\n<Attribute Name=\"CHARGEMONEY\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"CONTENT\" Description=\"\" DataType=\"\" DataValue=\"回公司总部参加系统培训,学习！\"/>\r\n<Attribute Name=\"CREATECOMPANYID\" Description=\"\" DataType=\"\" DataValue=\"bac05c76-0f5b-40ae-b73b-8be541ed35ed\"/>\r\n<Attribute Name=\"CREATEDATE\" Description=\"\" DataType=\"\" DataValue=\"2011-03-03 15:41:04\"/>\r\n<Attribute Name=\"CREATEDEPARTMENTID\" Description=\"\" DataType=\"\" DataValue=\"1a9fc682-82b1-4088-9ba0-03c57db9aa79\"/>\r\n<Attribute Name=\"CREATEPOSTID\" Description=\"\" DataType=\"\" DataValue=\"854fc028-62f9-41b5-9019-b6ef708425d2\"/>\r\n<Attribute Name=\"CREATEUSERID\" Description=\"\" DataType=\"\" DataValue=\"8574fb49-89da-4e4c-8ca8-d349ca6b5317\"/>\r\n<Attribute Name=\"CREATEUSERNAME\" Description=\"\" DataType=\"\" DataValue=\"刘勇\"/>\r\n<Attribute Name=\"OWNERCOMPANYID\" Description=\"\" DataType=\"\" DataValue=\"bac05c76-0f5b-40ae-b73b-8be541ed35ed\"/>\r\n<Attribute Name=\"OWNERDEPARTMENTID\" Description=\"\" DataType=\"\" DataValue=\"1a9fc682-82b1-4088-9ba0-03c57db9aa79\"/>\r\n<Attribute Name=\"OWNERID\" Description=\"\" DataType=\"\" DataValue=\"8574fb49-89da-4e4c-8ca8-d349ca6b5317\"/>\r\n<Attribute Name=\"OWNERNAME\" Description=\"\" DataType=\"\" DataValue=\"刘勇\"/>\r\n<Attribute Name=\"OWNERPOSTID\" Description=\"\" DataType=\"\" DataValue=\"854fc028-62f9-41b5-9019-b6ef708425d2\"/>\r\n<Attribute Name=\"REMARKS\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"TEL\" Description=\"\" DataType=\"\" DataValue=\"18626211899\"/>\r\n<Attribute Name=\"T_OA_BUSINESSREPORTDETAIL\" Description=\"\" DataType=\"\" DataValue=\"System.Collections.ObjectModel.ObservableCollection`1[SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_BUSINESSREPORTDETAIL]\"/>\r\n<Attribute Name=\"T_OA_BUSINESSTRIP\" Description=\"\" DataType=\"\" DataValue=\"SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_BUSINESSTRIP\"/>\r\n<Attribute Name=\"T_OA_BUSINESSTRIPReference\" Description=\"\" DataType=\"\" DataValue=\"SMT.SaaS.OA.UI.SmtOAPersonOfficeService.EntityReferenceOfT_OA_BUSINESSTRIPUlJdEHjk\"/>\r\n<Attribute Name=\"T_OA_TRAVELREIMBURSEMENT\" Description=\"\" DataType=\"\" DataValue=\"System.Collections.ObjectModel.ObservableCollection`1[SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_TRAVELREIMBURSEMENT]\"/>\r\n<Attribute Name=\"UPDATEDATE\" Description=\"\" DataType=\"\" DataValue=\"2011-03-03 16:06:08\"/>\r\n<Attribute Name=\"UPDATEUSERID\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"UPDATEUSERNAME\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"EntityKey\" Description=\"\" DataType=\"\" DataValue=\"SMT.SaaS.OA.UI.SmtOAPersonOfficeService.EntityKey\"/>\r\n<Attribute Name=\"TOTALCOST\" Description=\"\" DataType=\"\" DataValue=\"0\"/>\r\n<Attribute Name=\"POSTLEVEL\" Description=\"\" DataType=\"\" DataValue=\"16\"/>\r\n<Attribute Name=\"DEPARTMENTNAME\" Description=\"\" DataType=\"\" DataValue=\"市场营销部\"/>\r\n<Attribute Name=\"CURRENTEMPLOYEENAME\" Description=\"提交者\" DataType=\"\" DataValue=\"梅黠\"/>\r\n</Object>\r\n</System>\r\n";
           // string sss = "<?xml version=\"1.0\" encoding=\"utf-8\"?><System><Name>HR</Name><Object Name=\"T_HR_EMPLOYEESALARYRECORD\" Description=\"费用报销申请单\">\r\n  <Attribute Name=\"OrderTypeName\" Description=\"单据类型\" DataType=\"string\" DataValue=\"448c0dfb-1ff4-41df-84d3-230aa00fe8ce\" />\r\n  <Attribute Name=\"CHARGEAPPLYMASTERID\" Description=\"单据编号\" DataType=\"string\" DataValue=\"448c0dfb-1ff4-41df-84d3-230aa00fe8ce\" />\r\n  <Attribute Name=\"CHECKSTATES\" Description=\"状态\" DataType=\"string\" DataValue=\"0\" />\r\n  <Attribute Name=\"CREATEUSERNAME\" Description=\"创建人\" DataType=\"string\" DataValue=\"杜春雷\" />\r\n  <Attribute Name=\"CREATEDATE\" Description=\"创建时间\" DataType=\"datetime\" DataValue=\"2011-1-17 9:35:36\" />\r\n  <Attribute Name=\"T_FB_BORROWAPPLYMASTER\" Description=\"借款单\" DataType=\"string\" DataValue=\"\" />\r\n  <Attribute Name=\"BUDGETARYMONTH\" Description=\"预算月份\" DataType=\"string\" DataValue=\"2011-1-1 0:00:00\" />\r\n  <Attribute Name=\"OWNERCOMPANYNAME\" Description=\"申请公司\" DataType=\"string\" DataValue=\"集团本部\" />\r\n  <Attribute Name=\"OWNERDEPARTMENTNAME\" Description=\"申请部门\" DataType=\"string\" DataValue=\"人力资源部\" />\r\n  <Attribute Name=\"OWNERID\" Description=\"申请人\" DataType=\"string\" DataValue=\"4839a7d7-09ef-484d-9f4b-8e3f34538579\" />\r\n  <Attribute Name=\"PAYTYPE\" Description=\"付款类型\" DataType=\"string\" DataValue=\"4\" />\r\n  <Attribute Name=\"REMARK\" Description=\"备注\" DataType=\"string\" DataValue=\"集团画册3000本的印刷费，7.1元一本，共21300元。付客户款，账号名称在发票上已注明。\" />\r\n  <Attribute Name=\"TOTALMONEY\" Description=\"总金额\" DataType=\"decimal\" DataValue=\"21300\" />\r\n  <Attribute Name=\"POSTLEVEL\" Description=\"岗位级别\" DataType=\"decimal\" DataValue=\"0\" />\r\n  <Attribute Name=\"BorrowedMoney\" Description=\"已借款金额\" DataType=\"decimal\" DataValue=\"0\" />\r\n  <Attribute Name=\"TravelMoney\" Description=\"差旅费\" DataType=\"decimal\" DataValue=\"0\" />\r\n  <Attribute Name=\"BALANCEOBJECTNAME\" Description=\"招待费\" DataType=\"\" DataValue=\"0\" />\r\n</Object></System>";
            StringBuilder xml = new StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""?>");



            xml.Append(Environment.NewLine);
            xml.Append(@"    <System>");
            xml.Append(Environment.NewLine);
            xml.Append(@"       <Name>OA</Name>");
            xml.Append(Environment.NewLine);
            xml.Append(@"       <Object Name=""Test"">");
            xml.Append(Environment.NewLine);
            xml.Append(@"           <Attribute Name=""UserID""  DataType=""string"" DataValue=""" + sss + @"""></Attribute>");
            xml.Append(Environment.NewLine);
            xml.Append(@"           <Attribute  Name=""Price""  DataType=""decimal"" DataValue=""100""></Attribute>");
            xml.Append(@"       </Object>");
            xml.Append(Environment.NewLine);
            xml.Append(@"</System>");
          //  Console.WriteLine(sss);
           // sss = xml.ToString();
            FlowDataType.FlowData ss = new FlowDataType.FlowData();
          //  ss.xml = xml.ToString();

          //  var x = ss.GetString(sss, "HR", "T_HR_EMPLOYEESALARYRECORD", "BALANCEOBJECTNAME");
          //  int c;
           // if(null==0)
          //  Convert.ToDecimal(null);


            //WorkflowRuntime workflowRuntime = new WorkflowRuntime();

            //ConnectionStringSettings defaultConnectionString = ConfigurationManager.ConnectionStrings["OracleConnection"];
            //workflowRuntime.AddService(new AdoPersistenceService(defaultConnectionString, true, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)));
            //workflowRuntime.AddService(new AdoTrackingService(defaultConnectionString));
            //workflowRuntime.AddService(new AdoWorkBatchService());

            //FlowEvent ExternalEvent = new FlowEvent();
            //ExternalDataExchangeService objService = new ExternalDataExchangeService();
            //workflowRuntime.AddService(objService);
            //objService.AddService(ExternalEvent);
            //TypeProvider typeProvider = new TypeProvider(null);
            //workflowRuntime.AddService(typeProvider);
            //WorkflowInstance instance = workflowRuntime.CreateWorkflow(typeof(TestFlow.TestFlow));
            //instance.Start();
            //FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
            //FlowData.xml = sss;


            //ExternalEvent.OnDoFlow(instance.InstanceId, FlowData); //激发流程引擎执行到一下流程
            //System.Threading.Thread.Sleep(2000);
            //StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(workflowRuntime, instance.InstanceId);
           // SMTStateMachineWorkflowActivity MainActivity =
           //          instance.GetWorkflowDefinition() as SMTStateMachineWorkflowActivity;
            
            //System.Threading.Thread.Sleep(2000);
            #region 工作流接口测试
            //string xx = Guid.NewGuid().ToString("N");
            //PersonnelWS.PersonnelServiceClient WcfPersonnel = new PersonnelWS.PersonnelServiceClient();
            //PersonnelWS.T_HR_EMPLOYEE[] User = WcfPersonnel.GetEmployeeLeaders("25717c1c-ed32-4f0d-8556-6438960ad115", 0); 

            Console.WriteLine(DateTime.Now.ToString());

            //PermissionWS.PermissionServiceClient Permission = new PermissionWS.PermissionServiceClient();
            //var aaa = Permission.GetSysUserByRole("604392c4-ffed-45ae-97f1-7dfed22f970a");

            //OAWS.AgentServicesClient oaws = new OAWS.AgentServicesClient();
            //var aaa = oaws.GetQueryAgent("BF06E969-1B2C-4a89-B0AE-A91CA1244053", "TravelApplication"); 
           // string sss = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<System>\r\n<Name>OA</Name>\r\n<Object Name=\"T_OA_BUSINESSREPORT\" Description=\"\">\r\n<Attribute Name=\"BUSINESSREPORTID\" Description=\"\" DataType=\"\" DataValue=\"005e2e52-bc2d-4801-a524-4411a1d7de2e\"/>\r\n<Attribute Name=\"CHARGEMONEY\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"CONTENT\" Description=\"\" DataType=\"\" DataValue=\"回公司总部参加系统培训,学习！\"/>\r\n<Attribute Name=\"CREATECOMPANYID\" Description=\"\" DataType=\"\" DataValue=\"bac05c76-0f5b-40ae-b73b-8be541ed35ed\"/>\r\n<Attribute Name=\"CREATEDATE\" Description=\"\" DataType=\"\" DataValue=\"2011-03-03 15:41:04\"/>\r\n<Attribute Name=\"CREATEDEPARTMENTID\" Description=\"\" DataType=\"\" DataValue=\"1a9fc682-82b1-4088-9ba0-03c57db9aa79\"/>\r\n<Attribute Name=\"CREATEPOSTID\" Description=\"\" DataType=\"\" DataValue=\"854fc028-62f9-41b5-9019-b6ef708425d2\"/>\r\n<Attribute Name=\"CREATEUSERID\" Description=\"\" DataType=\"\" DataValue=\"8574fb49-89da-4e4c-8ca8-d349ca6b5317\"/>\r\n<Attribute Name=\"CREATEUSERNAME\" Description=\"\" DataType=\"\" DataValue=\"刘勇\"/>\r\n<Attribute Name=\"OWNERCOMPANYID\" Description=\"\" DataType=\"\" DataValue=\"bac05c76-0f5b-40ae-b73b-8be541ed35ed\"/>\r\n<Attribute Name=\"OWNERDEPARTMENTID\" Description=\"\" DataType=\"\" DataValue=\"1a9fc682-82b1-4088-9ba0-03c57db9aa79\"/>\r\n<Attribute Name=\"OWNERID\" Description=\"\" DataType=\"\" DataValue=\"8574fb49-89da-4e4c-8ca8-d349ca6b5317\"/>\r\n<Attribute Name=\"OWNERNAME\" Description=\"\" DataType=\"\" DataValue=\"刘勇\"/>\r\n<Attribute Name=\"OWNERPOSTID\" Description=\"\" DataType=\"\" DataValue=\"854fc028-62f9-41b5-9019-b6ef708425d2\"/>\r\n<Attribute Name=\"REMARKS\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"TEL\" Description=\"\" DataType=\"\" DataValue=\"18626211899\"/>\r\n<Attribute Name=\"T_OA_BUSINESSREPORTDETAIL\" Description=\"\" DataType=\"\" DataValue=\"System.Collections.ObjectModel.ObservableCollection`1[SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_BUSINESSREPORTDETAIL]\"/>\r\n<Attribute Name=\"T_OA_BUSINESSTRIP\" Description=\"\" DataType=\"\" DataValue=\"SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_BUSINESSTRIP\"/>\r\n<Attribute Name=\"T_OA_BUSINESSTRIPReference\" Description=\"\" DataType=\"\" DataValue=\"SMT.SaaS.OA.UI.SmtOAPersonOfficeService.EntityReferenceOfT_OA_BUSINESSTRIPUlJdEHjk\"/>\r\n<Attribute Name=\"T_OA_TRAVELREIMBURSEMENT\" Description=\"\" DataType=\"\" DataValue=\"System.Collections.ObjectModel.ObservableCollection`1[SMT.SaaS.OA.UI.SmtOAPersonOfficeService.T_OA_TRAVELREIMBURSEMENT]\"/>\r\n<Attribute Name=\"UPDATEDATE\" Description=\"\" DataType=\"\" DataValue=\"2011-03-03 16:06:08\"/>\r\n<Attribute Name=\"UPDATEUSERID\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"UPDATEUSERNAME\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"EntityKey\" Description=\"\" DataType=\"\" DataValue=\"SMT.SaaS.OA.UI.SmtOAPersonOfficeService.EntityKey\"/>\r\n<Attribute Name=\"TOTALCOST\" Description=\"\" DataType=\"\" DataValue=\"0\"/>\r\n<Attribute Name=\"POSTLEVEL\" Description=\"\" DataType=\"\" DataValue=\"16\"/>\r\n<Attribute Name=\"DEPARTMENTNAME\" Description=\"\" DataType=\"\" DataValue=\"市场营销部\"/>\r\n<Attribute Name=\"CURRENTEMPLOYEENAME\" Description=\"提交者\" DataType=\"\" DataValue=\"梅黠\"/>\r\n</Object>\r\n</System>\r\n";
            string aaa = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<System>\r\n<Name>OA</Name>\r\n<Object Name=\"T_OA_APPROVALINFO\" Description=\"\">\r\n<Attribute Name=\"APPROVALCODE\" Description=\"\" DataType=\"\" DataValue=\"BPJ201106170009\"/>\r\n<Attribute Name=\"APPROVALID\" Description=\"\" DataType=\"\" DataValue=\"4e93f242-ffa8-4600-9286-54c629488078\"/>\r\n<Attribute Name=\"APPROVALTITLE\" Description=\"\" DataType=\"\" DataValue=\"测试\"/>\r\n<Attribute Name=\"CHARGEMONEY\" Description=\"\" DataType=\"\" DataValue=\"0\"/>\r\n<Attribute Name=\"CONTENT\" Description=\"\" DataType=\"\" DataValue=\"System.Byte[]\"/>\r\n<Attribute Name=\"CREATECOMPANYID\" Description=\"\" DataType=\"\" DataValue=\"7c61f756-8d90-4b83-a8dd-5314fb1baf46\"/>\r\n<Attribute Name=\"CREATEDATE\" Description=\"\" DataType=\"\" DataValue=\"2011/06/17 11:27:20\"/>\r\n<Attribute Name=\"CREATEDEPARTMENTID\" Description=\"\" DataType=\"\" DataValue=\"89a7c04b-ffd8-4b0f-87fd-5620832bc855\"/>\r\n<Attribute Name=\"CREATEPOSTID\" Description=\"\" DataType=\"\" DataValue=\"73685fce-6da0-42e1-b314-650b75ec287c\"/>\r\n<Attribute Name=\"CREATEUSERID\" Description=\"\" DataType=\"\" DataValue=\"6375b923-028d-4523-8032-054bdfa43bb1\"/>\r\n<Attribute Name=\"CREATEUSERNAME\" Description=\"\" DataType=\"\" DataValue=\"雷中蓉\"/>\r\n<Attribute Name=\"ISCHARGE\" Description=\"\" DataType=\"\" DataValue=\"0\"/>\r\n<Attribute Name=\"OWNERCOMPANYID\" Description=\"\" DataType=\"\" DataValue=\"7c61f756-8d90-4b83-a8dd-5314fb1baf46\"/>\r\n<Attribute Name=\"OWNERDEPARTMENTID\" Description=\"\" DataType=\"\" DataValue=\"89a7c04b-ffd8-4b0f-87fd-5620832bc855\"/>\r\n<Attribute Name=\"OWNERID\" Description=\"\" DataType=\"\" DataValue=\"6375b923-028d-4523-8032-054bdfa43bb1\"/>\r\n<Attribute Name=\"OWNERNAME\" Description=\"\" DataType=\"\" DataValue=\"雷中蓉\"/>\r\n<Attribute Name=\"OWNERPOSTID\" Description=\"\" DataType=\"\" DataValue=\"73685fce-6da0-42e1-b314-650b75ec287c\"/>\r\n<Attribute Name=\"TEL\" Description=\"\" DataType=\"\" DataValue=\"13800138000\"/>\r\n<Attribute Name=\"TYPEAPPROVAL\" Description=\"\" DataType=\"\" DataValue=\"129\"/>\r\n<Attribute Name=\"TYPEAPPROVALONE\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"TYPEAPPROVALTHREE\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"TYPEAPPROVALTWO\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"UPDATEDATE\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"UPDATEUSERID\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"UPDATEUSERNAME\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"EntityKey\" Description=\"\" DataType=\"\" DataValue=\"SMT.SaaS.OA.UI.SmtOAPersonOfficeService.EntityKey\"/>\r\n<Attribute Name=\"CHARGEMONEY\" Description=\"\" DataType=\"\" DataValue=\"0\"/>\r\n<Attribute Name=\"POSTLEVEL\" Description=\"\" DataType=\"\" DataValue=\"20\"/>\r\n<Attribute Name=\"DEPARTMENTNAME\" Description=\"\" DataType=\"\" DataValue=\"\"/>\r\n<Attribute Name=\"CURRENTEMPLOYEENAME\" Description=\"提交者\" DataType=\"\" DataValue=\"雷中蓉\"/>\r\n</Object>\r\n</System>\r\n";
            WcfFlowService.ServiceClient aa = new WcfFlowService.ServiceClient();

            foreach (var operation in aa.Endpoint.Contract.Operations)
            {
                operation.Behaviors.Find<DataContractSerializerOperationBehavior>().MaxItemsInObjectGraph = 2147483647;
            }

         //   var ccc = aa.GetFlowInfo("e2d18c92-e307-49ea-a129-baf4413bbf28", "", "", "", "T_OA_APPROVALINFO", "", "");
         
            WcfFlowService.SubmitData SubmitData = new WcfFlowService.SubmitData();
           SubmitData.FlowSelectType = WcfFlowService.FlowSelectType.FixedFlow;
           SubmitData.FormID = "e2d18c92-e307-49ea-a129-baf4413bbf28";
           SubmitData.ModelCode = "T_OA_APPROVALINFO";
            SubmitData.ApprovalUser = new WcfFlowService.UserInfo();
            SubmitData.ApprovalUser.CompanyID = "7a613fc2-4431-4a46-ae01-232222e9fcb5";

            SubmitData.ApprovalUser.DepartmentID = "22caf251-27de-42f5-ac4c-47aa3ea321fc";
            SubmitData.ApprovalUser.PostID = "1f9c8317-0f38-44d1-ac01-dcf012855915";
            SubmitData.ApprovalUser.UserID = "e8252d35-9f48-43d6-a313-9505b827904e";
            SubmitData.ApprovalUser.UserName = "谭小凤";
            SubmitData.ApprovalContent = "sgsg";
            SubmitData.NextStateCode = "";

            SubmitData.NextApprovalUser = new WcfFlowService.UserInfo();
            SubmitData.NextApprovalUser.CompanyID = "";
            SubmitData.NextApprovalUser.DepartmentID = "";
            SubmitData.NextApprovalUser.PostID = "";
            SubmitData.NextApprovalUser.UserID = "";
            SubmitData.NextApprovalUser.UserName = "";
          //  SubmitData.SubmitFlag = WcfFlowService.SubmitFlag.New;
           SubmitData.SubmitFlag = WcfFlowService.SubmitFlag.Approval;
           // SubmitData.XML = aaa;
            SubmitData.FlowType = WcfFlowService.FlowType.Approval;

            SubmitData.ApprovalResult = WcfFlowService.ApprovalResult.Pass;
            SubmitData.ApprovalContent = "审核通过";


            WcfFlowService.DataResult cc = aa.SubimtFlow(SubmitData);
            if (cc.FlowResult == WcfFlowService.FlowResult.MULTIUSER)
            {
                SubmitData.NextApprovalUser = new WcfFlowService.UserInfo();
                SubmitData.NextApprovalUser.CompanyID = cc.UserInfo[1].CompanyID;
                SubmitData.NextApprovalUser.DepartmentID = cc.UserInfo[1].DepartmentID;
                SubmitData.NextApprovalUser.PostID = cc.UserInfo[1].PostID;
                SubmitData.NextApprovalUser.UserID = cc.UserInfo[1].UserID;
                SubmitData.NextApprovalUser.UserName = cc.UserInfo[1].UserName;
                SubmitData.NextStateCode = cc.AppState;
                cc = aa.SubimtFlow(SubmitData);
            }
         //  WcfFlowService.FLOW_FLOWRECORDDETAIL_T[]  ccc = aa.GetFlowInfo("", "", "", "", "TestModel","33c1dd98-3cd5-4737-9601-e895063248b7", "");
           string a = "";
           // FLOW_FLOWRECORDDETAIL_TDAL a = new FLOW_FLOWRECORDDETAIL_TDAL();
           // var cc = aa.GetFlowRecordMaster("", "", "", "", "", "", "");
            //Service aa = new Service();
            //Flow_FlowRecord_T entity = new WcfFlowService.Flow_FlowRecord_T();
            //Console.WriteLine(cc.FlowResult .ToString ());
            //Console.WriteLine(cc.Err );
            //Console.WriteLine("RunTime:"+ cc.RunTime );
            //Console.WriteLine(DateTime.Now.ToString());
            //entity.CompanyID = "smt";
            ////entity.Content = "dfwfw";
            //entity.CreateDate = DateTime.Now;
            ////entity.EditDate = DateTime.Now;
            //entity.EditUserID = "";
            //entity.Flag = "F";
            //entity.FlowCode = "testflow";
            //entity.FormID = "fgdgd";

            //entity.GUID = "";
            //entity.InstanceID = "";
            //entity.ModelCode = "testflow";
            //entity.OfficeID = "ss";
            //entity.CreateUserID = "EditUserId";
            //entity.StateCode = "StartFlow";
            //entity.ParentStateCode = "StartFlow";



            // aa.StartFlow();
            //  FlowSysDAL Dal = new FlowSysDAL();
            // Flow_FlowRecord_T[] ss = aa.GetFlowInfo(entity);
            //List< Flow_FlowRecord_T> ss = aa.GetFlowInfo(entity);
            // List< Flow_FlowRecord_T> ss = Dal.GetFlowRecord();

            //Console.WriteLine("rfe");

            //ss[0].Content = "aaaaaaaaaa";
            //entity = ss[0];
            //entity.Content = "gdfvegebe";
            //  aa.UpdateFlow(entity);
            //Dal.UpdateFlowRecord(entity);

            
            //   aa.Close();
            //Type serviceType = typeof(Class1);
            //using (ServiceHost host = new ServiceHost(serviceType))
            //{
            //    host.Open();
            //    host.Close();
            //}

            #endregion
            
            #region 工作流测试
            /*
            
            using(WorkflowRuntime workflowRuntime = new WorkflowRuntime())
            {
                ConnectionStringSettings defaultConnectionString = ConfigurationManager.ConnectionStrings["OracleConnection"];
                workflowRuntime.AddService(new AdoPersistenceService(defaultConnectionString, true, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)));
                workflowRuntime.AddService(new AdoTrackingService(defaultConnectionString));
                workflowRuntime.AddService(new AdoWorkBatchService());

                FlowEvent ExternalEvent = new FlowEvent();
                ExternalDataExchangeService objService = new ExternalDataExchangeService();
                workflowRuntime.AddService(objService);
                objService.AddService(ExternalEvent);
               TypeProvider typeProvider = new TypeProvider(null);
                workflowRuntime.AddService(typeProvider);

                AutoResetEvent waitHandle = new AutoResetEvent(false);
                workflowRuntime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e) {waitHandle.Set();};
                workflowRuntime.WorkflowTerminated += delegate(object sender, WorkflowTerminatedEventArgs e)
                {
                    Console.WriteLine(e.Exception.Message);
                    waitHandle.Set();
                };
                
                WorkflowInstance instance = workflowRuntime.CreateWorkflow(typeof(SMT.WFLib.Workflow2));
               // XmlReader readerxoml = XmlReader.Create("TestFlow.xml");
               // WorkflowInstance instance = workflowRuntime.CreateWorkflow(readerxoml);
               // XmlReader readerrules = XmlReader.Create("Workflow.rules");
              //  WorkflowInstance instance = workflowRuntime.CreateWorkflow(readerxoml, readerrules, null, Guid.NewGuid());
                instance.Start();
                
                FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
                while (workflowRuntime.IsStarted)
               {

            //        // SMTFlowArg objDataEventArgs = new SMTFlowArg(InstanceId,true);
            //        // objDataEventArgs.WaitForIdle = true;




                   Console.WriteLine("输入公司ID");
                    FlowData.Flow_FlowRecord_T.CompanyID = Console.ReadLine();
                    Console.WriteLine("输入审批意见");
                    FlowData.Flow_FlowRecord_T .Content = Console.ReadLine();
                    ExternalEvent.OnDoFlow(instance.InstanceId, FlowData);
                    System.Threading.Thread.Sleep(1 * 1000);
                    ReadOnlyCollection<WorkflowQueueInfo> queueInfoData = instance.GetWorkflowQueueData();
                    if (queueInfoData.Count == 0)
                    {
                        workflowRuntime.StopRuntime();

                    }


                }
                
                Console.ReadLine();
              //  waitHandle.WaitOne();
            }
              */
            #endregion

            Console.ReadLine();
        }
    }
}
