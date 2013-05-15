using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Objects.DataClasses;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;
using System.Data;
using System.Xml;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Mapping;
using System.Configuration;
using System.Runtime.InteropServices;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using System.Linq.Dynamic;
using SMT.HRM.DAL;
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    public class Utility
    {

        public static List<string> GetProByEntityName(string entityName, string masterIDName)
        {
            try
            {
                string qualifiedEntitySetName = entityName + ConfigurationManager.AppSettings["EntityDllPath"];
                List<string> list = new List<string>();
                object a = (object)Activator.CreateInstance(Type.GetType("SMT_HRM_EFModel." + qualifiedEntitySetName));
                Type type = a.GetType();
                PropertyInfo[] infos = type.GetProperties();
                foreach (PropertyInfo info in infos)
                {
                    if (info.Name == "COMPANYID" || info.Name == "OWNERID" || info.Name == "OWNERPOSTID" || info.Name == "OWNERDEPARTMENTID"
                        || info.Name == "OWNERCOMPANYID" || info.Name == "CREATEPOSTID" || info.Name == "CREATEDEPARTMENTID" || info.Name == "CREATECOMPANYID"
                        || info.Name == "CREATEUSERID" || info.Name == "CREATEDATE" || info.Name == "UPDATEUSERID" || info.Name == "UPDATEDATE" || info.Name == masterIDName)
                        continue;

                    if (info.PropertyType == typeof(System.Data.EntityKey) || info.PropertyType == typeof(System.Data.EntityState)
                        || info.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityReference)
                        || info.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityObject))
                        continue;
                    list.Add(info.Name);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string strEngineFuncWSSite = ConfigurationManager.AppSettings["EngineFuncWSSite"];

        public static object GetLookupData(EntityNames entity, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            object ents = null;
            Type type = Type.GetType("SMT.HRM.BLL." + entity.ToString() + "BLL");

            if (type != null)
            {
                ILookupEntity bll = (ILookupEntity)Activator.CreateInstance(type);
                ents = bll.GetLookupData(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
            }
            return ents;
        }
        public static int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            new Utility().initEntityRelation();
            int i = 0;
            try
            {
                Type type = Type.GetType("SMT.HRM.BLL." + EntityRelation[strEntityName]);
                if (type != null)
                {
                    IOperate bll = (IOperate)Activator.CreateInstance(type);
                    //SMT.Foundation.Log.Tracer.Debug("UpdateCheckState start;" + bll.GetType().Name);
                    i = bll.UpdateCheckState(strEntityName, EntityKeyName, EntityKeyValue, CheckState);
                    bll.Dispose();                    
                    //SMT.Foundation.Log.Tracer.Debug("手机调用业务逻辑层 bll.Dispose();");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("UpdateCheckState错误： strEntityName：" + strEntityName
                    +" EntityKeyValue：" + EntityKeyValue+" 异常信息："+ex.ToString());
            }

            return i;
        }
        public static string SerializeObject(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer serializer =
                        new XmlSerializer(obj.GetType());
                serializer.Serialize(ms, obj);
                ms.Position = 0;

                using (StreamReader reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        public static Dictionary<string, string> EntityRelation;
        void initEntityRelation()
        {
            if (EntityRelation == null)
            {
                EntityRelation = new Dictionary<string, string>();
                EntityRelation.Add("T_HR_ATTENDANCESOLUTION", "AttendanceSolutionBLL");
                EntityRelation.Add("T_HR_ATTENDANCESOLUTIONASIGN", "AttendanceSolutionAsignBLL");
                EntityRelation.Add("T_HR_ATTENDMONTHLYBATCHBALANCE", "AttendMonthlyBatchBalanceBLL");
                EntityRelation.Add("T_HR_COMPANY", "CompanyBLL");
                EntityRelation.Add("T_HR_DEPARTMENT", "DepartmentBLL");
                EntityRelation.Add("T_HR_DEPARTMENTDICTIONARY", "DepartmentDictionaryBLL");
                EntityRelation.Add("T_HR_EMPLOYEEADDSUMBATCH", "EmployeeAddSumBatchBLL");
                EntityRelation.Add("T_HR_EMPLOYEEADDSUM", "EmployeeAddSumBLL");
                EntityRelation.Add("T_HR_EMPLOYEECHECK", "EmployeeCheckBLL");
                EntityRelation.Add("T_HR_EMPLOYEECONTRACT", "EmployeeContractBLL");
                EntityRelation.Add("T_HR_EMPLOYEEENTRY", "EmployeeEntryBLL");
                EntityRelation.Add("T_HR_EMPLOYEELEAVERECORD", "EmployeeLeaveRecordBLL");
                //员工销假
                EntityRelation.Add("T_HR_EMPLOYEECANCELLEAVE", "EmployeeCancelLeaveBLL");
                EntityRelation.Add("T_HR_EMPLOYEEPOSTCHANGE", "EmployeePostChangeBLL");
                EntityRelation.Add("T_HR_PENSIONMASTER", "PensionMasterBLL");
                EntityRelation.Add("T_HR_EMPLOYEESIGNINRECORD", "EmployeeSignInRecordBLL");
                EntityRelation.Add("T_HR_LEFTOFFICE", "LeftOfficeBLL");
                EntityRelation.Add("T_HR_LEFTOFFICECONFIRM", "LeftOfficeConfirmBLL");
                EntityRelation.Add("T_HR_EMPLOYEEOVERTIMERECORD", "OverTimeRecordBLL");
                EntityRelation.Add("T_HR_POST", "PostBLL");
                EntityRelation.Add("T_HR_POSTDICTIONARY", "PostDictionaryBLL");
                EntityRelation.Add("T_HR_SALARYARCHIVE", "SalaryArchiveBLL");
                EntityRelation.Add("T_HR_SALARYSOLUTION", "SalarySolutionBLL");
                EntityRelation.Add("T_HR_SALARYSOLUTIONASSIGN", "SalarySolutionAssignBLL");
                EntityRelation.Add("T_HR_SALARYRECORDBATCH", "SalaryRecordBatchBLL");
                EntityRelation.Add("T_HR_SALARYSYSTEM", typeof(SalarySystemBLL).Name);
            }
        }
        /// <summary>
        /// 克隆对像
        /// </summary>
        /// <param name="source">要被克隆的对像</param>
        /// <returns>克隆的新对像</returns>
        public static T CloneObject<T>(object source) where T : class
        {
            if (source == null)
            {
                return null;
            }

            string str = SerializeObject(source);
            T tmpObj = DeserializeObject<T>(str);
            return tmpObj;
        }
        public static T DeserializeObject<T>(string objString)
        {
            //List<Person> persons = DeserializeObject<List<Person>>( jsonString )
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                return (T)serializer.Deserialize(ms);
            }
        }
        public static object DeserializeObject(string objString, Type type)
        {
            //List<Person> persons = DeserializeObject<List<Person>>( jsonString )
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(type);

                return serializer.Deserialize(ms);
            }
        }

        /// <summary>
        /// 对IQueryable对像分页处理
        /// </summary>
        /// <typeparam name="T">需要分页对像类型</typeparam>
        /// <param name="ents">需要分页的IQueryable对像,必需是有排过序的</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页记录条数</param>
        /// <param name="pageCount">页数</param>
        /// <returns>IQueryable对像</returns>
        public static IQueryable<T> Pager<T>(IQueryable<T> ents, int pageIndex, int pageSize, ref int pageCount)
        {
            if (ents == null)
            {
                return null;
            }
            int count = ents.Count();
            pageCount = count / pageSize;
            int tmp = count % pageSize;

            pageCount = pageCount + (tmp > 0 ? 1 : 0);
            if (pageIndex > pageCount)
                pageIndex = 2;

            ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return ents;
        }

        public static void CloneEntity<T>(T sourceObj, T targetObj) where T : class
        {
            Type a = sourceObj.GetType();
            PropertyInfo[] infos = a.GetProperties();
            foreach (PropertyInfo prop in infos)
            {
                //System.Data.Objects.DataClasses.
                if (prop.PropertyType.BaseType == typeof(EntityReference)
                    || prop.PropertyType.BaseType == typeof(RelatedEnd)
                    || prop.PropertyType == typeof(System.Data.EntityState)
                    || prop.PropertyType == typeof(System.Data.EntityKey)
                    || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityObject))
                    continue;
                if (sourceObj is EntityObject)
                {
                    EntityObject ent = sourceObj as EntityObject;

                    if (ent != null && ent.EntityKey != null && ent.EntityKey.EntityKeyValues != null && ent.EntityKey.EntityKeyValues.Count() > 0)
                    {
                        bool isKeyField = false;
                        foreach (var key in ent.EntityKey.EntityKeyValues)
                        {
                            if (key.Key == prop.Name)
                            {
                                isKeyField = true;
                                break;
                            }
                        }
                        if (isKeyField)
                            continue;
                    }
                }
                //prop.Name
                object value = prop.GetValue(sourceObj, null);
                try
                {
                    prop.SetValue(targetObj, value, null);
                }
                catch (Exception ex)
                {
                    string e = ex.Message;
                }
            }
        }

        public static string GetResourceStr(string message)
        {
            string rslt = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString(message, SMT.SaaS.Globalization.Localization.UiCulture);
            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }
        public static string GetResourceStr(string message, string parameter)
        {
            string rslt = SMT.SaaS.Globalization.Localization.GetString(message, parameter);

            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }

        public static void RefreshEntity(EntityObject entity)
        {
            var rs = (entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();

            foreach (IRelatedEnd re in rs)
            {
                List<EntityObject> list = new List<EntityObject>();
                foreach (var item in re)
                {
                    list.Add(item as EntityObject);
                }
                list.ForEach(p =>
                {
                    if (re.GetType().BaseType == typeof(EntityReference))
                    {
                        EntityKey eKey = p.EntityKey;
                        if (eKey != null)
                        {
                            (re as EntityReference).EntityKey = eKey;
                            re.Remove(p);
                        }
                    }

                });
            }
        }

        /// <summary>
        /// 获取每周上班天数
        /// </summary>
        /// <param name="iWorkMode"></param>
        /// <param name="iWorkDays"></param>
        public static void GetWorkDays(int iWorkMode, ref List<int> iWorkDays)
        {
            // 摘要: Sunday = 0, Monday = 1, Tuesday = 2, Wednesday = 3, Thursday = 4, Friday = 5, Saturday = 6.
            switch (iWorkMode)
            {
                case 1:
                    iWorkDays.Add(1);
                    break;
                case 2:
                    iWorkDays.Add(1);
                    iWorkDays.Add(2);
                    break;
                case 3:
                    iWorkDays.Add(1);
                    iWorkDays.Add(2);
                    iWorkDays.Add(3);
                    break;
                case 4:
                    iWorkDays.Add(1);
                    iWorkDays.Add(2);
                    iWorkDays.Add(3);
                    iWorkDays.Add(4);
                    break;
                case 5:
                    iWorkDays.Add(1);
                    iWorkDays.Add(2);
                    iWorkDays.Add(3);
                    iWorkDays.Add(4);
                    iWorkDays.Add(5);
                    break;
                case 6:
                    iWorkDays.Add(1);
                    iWorkDays.Add(2);
                    iWorkDays.Add(3);
                    iWorkDays.Add(4);
                    iWorkDays.Add(5);
                    iWorkDays.Add(6);
                    break;
                case 7:
                    iWorkDays.Add(0);
                    iWorkDays.Add(1);
                    iWorkDays.Add(2);
                    iWorkDays.Add(3);
                    iWorkDays.Add(4);
                    iWorkDays.Add(5);
                    iWorkDays.Add(6);
                    break;
            }
        }

        /// <summary>
        /// 将数据转为指定html格式，并以流的形式返回，以便导出成指定格式的文件
        /// </summary>
        /// <param name="title"></param>
        /// <param name="dt"></param>
        public static byte[] OutFileStream(string title, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetHeader().ToString());
            sb.Append(GetBody(title, dt).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }

        /// <summary>
        /// 获取EXCEL头文件
        /// </summary>
        /// <returns></returns>
        public static StringBuilder GetHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns=\"http://www.w3.org/TR/REC-html40\">\n\r");
            sb.Append("<head>\n\r");
            sb.Append("<meta http-equiv=Content-Type content=\"text/html; charset=utf-8\">\n\r");
            sb.Append("<meta name=ProgId content=Excel.Sheet>\n\r");
            sb.Append("<meta name=Generator content=\"Microsoft Excel 11\">\n\r");

            sb.Append(" <xml>\n\r");
            sb.Append("<x:ExcelWorkbook>\n\r");
            sb.Append("<x:ExcelWorksheets>\n\r");
            sb.Append("<x:ExcelWorksheet>\n\r");
            sb.Append("<<x:Name>Sheet1</x:Name>\n\r");
            sb.Append("<x:WorksheetOptions>\n\r");
            sb.Append("<x:Print>\n\r");
            sb.Append("</x:Print>\n\r");
            sb.Append("</x:WorksheetOptions>\n\r");
            sb.Append("</x:ExcelWorksheet>\n\r");
            sb.Append("</x:ExcelWorksheets>\n\r");
            sb.Append("</x:ExcelWorkbook>\n\r");
            sb.Append(" </xml>\n\r");
            sb.Append("<style>\n\r");
            sb.Append("td {font-size:12px;}\n\r");
            sb.Append(".title {font-size:14px; font-weight:bold;height:30px;}\n\r");
            sb.Append(".thead{font-weight:bold;}\n\r");
            sb.Append(".style0{mso-number-format:General;text-align:general;vertical-align:middle;white-space:normal;" +
                              "mso-rotate:0;mso-background-source:auto;mso-pattern:auto;color:windowtext;" +
                              "font-weight:400;font-style:normal;text-decoration:none;font-family:宋体;" +
                              "mso-generic-font-family:auto;mso-font-charset:134;border:none;" +
                              "mso-protection:locked visible;mso-style-name:常规;mso-style-id:0;" +
                              "font-size:9.0pt;border:.5pt solid black;}\n\r");
            sb.Append(".x1281{mso-style-parent:style0;mso-number-format:\"\\@\";border:.5pt solid black;font-weight:bold;}\n\r");
            sb.Append(".x1282{mso-style-parent:style0;mso-number-format:\"\\@\";border:.5pt solid black;}\n\r");
            sb.Append(".x0{mso-style-parent:style0;mso-number-format:\"0_ \";text-align:right;border:.5pt solid black;}\n\r");
            sb.Append(".x1{mso-style-parent:style0;mso-number-format:\"0\\.0_ \";text-align:right;border:.5pt solid black;}\n\r");
            sb.Append(".x2{mso-style-parent:style0;mso-number-format:\"0\\.00_ \";text-align:right;border:.5pt solid black;}\n\r");
            sb.Append(".x3{mso-style-parent:style0;mso-number-format:\"0\\.000_ \";text-align:right;border:.5pt solid black;}\n\r");
            sb.Append(".x4{mso-style-parent:style0;mso-number-format:\"0\\.0000_ \";text-align:right;border:.5pt solid black;}\n\r");
            sb.Append(".x5{mso-style-parent:style0;mso-number-format:\"0\\.00000_ \";text-align:right;border:.5pt solid black;}\n\r");
            sb.Append("</style>\n\r");
            sb.Append("</head>\n\r");
            return sb;
        }

        /// <summary>
        /// 获取EXCEL内容
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static StringBuilder GetBody(string title, DataTable dt)
        {
            StringBuilder s = new StringBuilder();
            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" border=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");
            int cols = dt.Columns.Count;
            if (cols > 12) cols = 12;
            s.Append("<td colspan=\"" + cols + "\" align=center class=\"title\">" + title + "</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");
            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                s.Append("<td class='x1281'>" + dt.Columns[i].Caption.ToString().Replace("*" + i, "") + "</td>");
            }
            s.Append("</tr>");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                s.Append("<tr>");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    s.Append("<td class='" + GetCss(dt.Rows[i][j].ToString(), dt.Columns[j].DataType.Name) + "'>" + dt.Rows[i][j].ToString() + "</td>");
                }
                s.Append("</tr>");
            }
            s.Append("</table>");
            s.Append("</body></html>");
            return s;
        }

        /// <summary>
        /// 获取单元格样式
        /// </summary>
        /// <param name="str"></param>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static string GetCss(string str, string typename)
        {
            string tempStr = "x1282";
            if (!str.Equals("") && ("Int32,Decimal,Double".IndexOf(typename) >= 0))
            {

                int m = 0;
                if (str.LastIndexOf(".") >= 0)
                    m = str.Length - str.LastIndexOf('.') - 1;
                if (m >= 0) tempStr = "x" + m;
            }
            return tempStr;
        }
        ///   <summary>      
        ///   返回两个日期之间的时间间隔（y：年份间隔、M：月份间隔、d：天数间隔、h：小时间隔、m：分钟间隔、s：秒钟间隔、ms：微秒间隔）      
        ///   </summary>      
        ///   <param   name="Date1">开始日期</param>      
        ///   <param   name="Date2">结束日期</param>      
        ///   <param   name="Interval">间隔标志</param>      
        ///   <returns>返回间隔标志指定的时间间隔</returns>      
        public static int DateDiff(System.DateTime Date1, System.DateTime Date2, string Interval)
        {
            double dblYearLen = 365;//年的长度，365天      
            double dblMonthLen = (365 / 12);//每个月平均的天数      
            System.TimeSpan objT;
            objT = Date2.Subtract(Date1);
            switch (Interval)
            {
                case "y"://返回日期的年份间隔      
                    return System.Convert.ToInt32(objT.Days / dblYearLen);
                case "M"://返回日期的月份间隔      
                    return System.Convert.ToInt32(objT.Days / dblMonthLen);
                case "d"://返回日期的天数间隔      
                    return objT.Days;
                case "h"://返回日期的小时间隔      
                    return objT.Hours;
                case "m"://返回日期的分钟间隔      
                    return objT.Minutes;
                case "s"://返回日期的秒钟间隔      
                    return objT.Seconds;
                case "ms"://返回时间的微秒间隔      
                    return objT.Milliseconds;
                default:
                    break;
            }
            return 0;
        }
        /// <summary>
        /// 保存数据变化到引擎服务
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="oldObject">修改前的实体</param>
        /// <param name="newObject">修改后的数据</param>
        /// <returns>保存结果</returns>
        public static string SaveTriggerData<T>(T oldObject, T newObject) where T : class
        {
            Type etype = oldObject.GetType();
            PropertyInfo[] infos = etype.GetProperties();

            EntityObject oldEnt = oldObject as EntityObject;
            EntityObject newEnt = newObject as EntityObject;

            XmlDocument xd = new XmlDocument();//表示XML文档
            XmlDeclaration xde;//表示 XML 声明节点：<?xml version='1.0'...?>
            xde = xd.CreateXmlDeclaration("1.0", null, null);
            xde.Encoding = "gb2312";
            xde.Standalone = "yes";
            xd.AppendChild(xde);//<?xml version="1.0" encoding="UTF-8" standalone="yes"?>生成结束

            XmlElement xe = xd.CreateElement("table");//创建一个table根元素
            xd.AppendChild(xe);//table根元素创建完成

            //查找<table>
            XmlNode table = xd.SelectSingleNode("table");

            //在<table>之下创建元素<ApplicationSystem>
            XmlElement ApplicationSystem = xd.CreateElement("ApplicationSystem");
            //人事系统
            ApplicationSystem.AppendChild(xd.CreateTextNode("0"));
            table.AppendChild(ApplicationSystem);


            //在<table>之下创建元素<CompanyCode>
            XmlElement CompanyCode = xd.CreateElement("CompanyCode");
            PropertyInfo corpProp = infos.SingleOrDefault(p => p.Name == "UPDATEDEPARTMENTID");
            if (corpProp != null)
            {
                object corp = corpProp.GetValue(newObject, null);
                CompanyCode.AppendChild(xd.CreateTextNode(corp == null ? "" : corp.ToString()));
                table.AppendChild(CompanyCode);
            }

            //在<table>之下创建元素<OperationUser>
            XmlElement OperationUser = xd.CreateElement("OperationUser");
            //获取修改人的信息
            PropertyInfo userProp = infos.SingleOrDefault(p => p.Name == "UPDATEUSERID");
            if (userProp != null)
            {
                object user = userProp.GetValue(newObject, null);
                OperationUser.AppendChild(xd.CreateTextNode(user == null ? "" : user.ToString()));
                table.AppendChild(CompanyCode);
            }

            //在<table>之下创建元素<TableName>
            XmlElement TableName = xd.CreateElement("TableName");
            TableName.AppendChild(xd.CreateTextNode(oldObject.GetType().Name));
            table.AppendChild(TableName);


            if (oldEnt == null || newEnt == null)
            {
                return "";
            }

            //添加<TableKey>
            XmlElement TableKey = xd.CreateElement("TableKey");
            table.AppendChild(TableKey);

            if (oldEnt != null && oldEnt.EntityKey != null && oldEnt.EntityKey.EntityKeyValues != null && oldEnt.EntityKey.EntityKeyValues.Count() > 0)
            {

                foreach (var key in oldEnt.EntityKey.EntityKeyValues)
                {
                    //TableKeyName
                    XmlElement TableKeyName = xd.CreateElement("TableKeyName");
                    TableKeyName.AppendChild(xd.CreateTextNode(key.Key));
                    TableKey.AppendChild(TableKeyName);

                    XmlElement TableKeyValue = xd.CreateElement("TableKeyValue");
                    TableKeyValue.AppendChild(xd.CreateTextNode((key.Value == null) ? "" : key.Value.ToString()));
                    TableKey.AppendChild(TableKeyValue);
                }

            }

            //添加<FieldString>	            
            foreach (PropertyInfo prop in infos)
            {
                if (prop.PropertyType.BaseType == typeof(EntityReference)
                    || prop.PropertyType.BaseType == typeof(RelatedEnd)
                    || prop.PropertyType == typeof(System.Data.EntityState)
                    || prop.PropertyType == typeof(System.Data.EntityKey)
                    )
                    continue;


                //关键字段跳过
                if (oldEnt != null && oldEnt.EntityKey != null && oldEnt.EntityKey.EntityKeyValues != null && oldEnt.EntityKey.EntityKeyValues.Count() > 0)
                {
                    bool isKeyField = false;
                    foreach (var key in oldEnt.EntityKey.EntityKeyValues)
                    {
                        if (key.Key == prop.Name)
                        {
                            isKeyField = true;
                            break;
                        }
                    }
                    if (isKeyField)
                        continue;
                }


                //生成外键的
                if (prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityObject))
                {
                    PropertyInfo refProp = infos.SingleOrDefault(p => p.Name == (prop.Name + "Reference"));

                    EntityReference reference = refProp.GetValue(oldEnt, null) as EntityReference;

                    if (!reference.IsLoaded)
                        reference.Load();

                    EntityObject oldEntRef = prop.GetValue(oldEnt, null) as EntityObject;
                    EntityObject newEntRef = prop.GetValue(newEnt, null) as EntityObject;

                    foreach (var key in oldEntRef.EntityKey.EntityKeyValues)
                    {
                        //在<table>之下创建元素<FieldString>
                        XmlElement FieldString = xd.CreateElement("FieldString");
                        table.AppendChild(FieldString);

                        XmlElement FieldName = xd.CreateElement("FieldName");
                        FieldName.AppendChild(xd.CreateTextNode(key.Key));
                        FieldString.AppendChild(FieldName);

                        XmlElement Field_Old_Value = xd.CreateElement("Field_Old_Value");
                        Field_Old_Value.AppendChild(xd.CreateTextNode((key.Value == null) ? "" : key.Value.ToString()));
                        FieldString.AppendChild(Field_Old_Value);

                        XmlElement Field_New_Value = xd.CreateElement("Field_New_Value");

                        PropertyInfo[] refinfos = prop.PropertyType.GetProperties();
                        PropertyInfo refEntProp = refinfos.SingleOrDefault(p => p.Name == key.Key);
                        object refEntPropValue = refEntProp.GetValue(newEntRef, null);

                        Field_New_Value.AppendChild(xd.CreateTextNode((refEntPropValue == null) ? "" : refEntPropValue.ToString()));
                        FieldString.AppendChild(Field_New_Value);
                    }
                }
                else
                {
                    //prop.Name
                    //在<table>之下创建元素<FieldString>
                    XmlElement FieldString = xd.CreateElement("FieldString");
                    table.AppendChild(FieldString);

                    XmlElement FieldName = xd.CreateElement("FieldName");
                    FieldName.AppendChild(xd.CreateTextNode(prop.Name));
                    FieldString.AppendChild(FieldName);

                    XmlElement Field_Old_value = xd.CreateElement("Field_Old_Value");
                    object oldvalue = prop.GetValue(oldEnt, null);
                    Field_Old_value.AppendChild(xd.CreateTextNode((oldvalue == null) ? "" : oldvalue.ToString()));
                    FieldString.AppendChild(Field_Old_value);

                    XmlElement Field_New_value = xd.CreateElement("Field_New_Value");
                    object newvalue = prop.GetValue(newEnt, null);
                    Field_New_value.AppendChild(xd.CreateTextNode((newvalue == null) ? "" : newvalue.ToString()));
                    FieldString.AppendChild(Field_New_value);
                }
            }

            EngineWS.EngineWcfGlobalFunctionClient engClient = new EngineWS.EngineWcfGlobalFunctionClient();
            string rslt = string.Empty;// engClient.SaveTriggerData(xd.OuterXml);

            return rslt;
        }

        public static void SendEngineEventTriggerData(IList<object> paras)
        {
            try{
            StringBuilder strRes = new StringBuilder();
            EngineWS.T_WF_TIMINGTRIGGERACTIVITY trigger = new EngineWS.T_WF_TIMINGTRIGGERACTIVITY();
            trigger.TRIGGERID = System.Guid.NewGuid().ToString();
            trigger.COMPANYID = paras[0].ToString();
            trigger.SYSTEMCODE = paras[1].ToString();
            trigger.MODELCODE = paras[2].ToString();
            trigger.BUSINESSID = paras[3].ToString();
            trigger.TRIGGERACTIVITYTYPE = 2;
            if (paras[4].ToString().IndexOf(':') < 0)
                trigger.TRIGGERTIME = Convert.ToDateTime(paras[4].ToString() + " 8:00:00");
            else
                trigger.TRIGGERTIME = Convert.ToDateTime(paras[4].ToString());
            trigger.TRIGGERROUND = 0;
            trigger.MESSAGEBODY = paras[8].ToString();
            trigger.WCFURL = "EngineTriggerService.svc";
            trigger.FUNCTIONNAME = paras[11].ToString();
            trigger.FUNCTIONPARAMTER = paras[12].ToString();
            trigger.PAMETERSPLITCHAR = paras[13].ToString();
            trigger.WCFBINDINGCONTRACT = "customBinding";
            trigger.TRIGGERSTATUS = 0;
            trigger.TRIGGERDESCRIPTION = "EventTrigger";
            trigger.TRIGGERTYPE = "user";
            

            strRes.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            strRes.Append("<System>");
            strRes.Append("<CompanyCode>" + paras[0].ToString() + "</CompanyCode>");
            strRes.Append("<SystemCode>" + paras[1].ToString() + "</SystemCode>");
            strRes.Append("<ModelCode>" + paras[2].ToString() + "</ModelCode>");
            strRes.Append("<ApplicationOrderCode>" + paras[3].ToString() + "</ApplicationOrderCode>");
            //strRes.Append("<TaskStartDate>" + paras[4].ToString() + "</TaskStartDate>");
            if(paras[4].ToString().IndexOf(':')<0)
                strRes.Append("<TaskStartDate>" + paras[4].ToString()+" 8:00:00" + "</TaskStartDate>");
            else
                strRes.Append("<TaskStartDate>" + paras[4].ToString() + "</TaskStartDate>");
            //strRes.Append("<TaskStartDate>" + Convert.ToDateTime("2012/12/6 16:40:26").ToString() + "</TaskStartDate>");
            strRes.Append("<TaskStartTime>" + paras[5].ToString() + "</TaskStartTime>");
            strRes.Append("<ProcessCycle>" + paras[6].ToString() + "</ProcessCycle>");
            strRes.Append("<ReceiveUser>" + paras[7].ToString() + "</ReceiveUser>");
            strRes.Append("<MessageBody>" + paras[8].ToString() + "</MessageBody>");
            strRes.Append("<MsgLinkUrl>" + paras[9].ToString() + "</MsgLinkUrl>");
            strRes.Append("<ProcessWcfUrl>" + "EngineTriggerService.svc" + "</ProcessWcfUrl>");
            strRes.Append("<WcfFuncName>" + paras[11].ToString() + "</WcfFuncName>");
            strRes.Append("<WcfFuncParamter>" + paras[12].ToString() + "</WcfFuncParamter>");
            strRes.Append("<WcfParamSplitChar>" + paras[13].ToString() + "</WcfParamSplitChar>");
            strRes.Append("<WcfBinding>" + paras[14].ToString() + "</WcfBinding>");
            strRes.Append("</System>");

            //return strRes.ToString();
            //SMT.Foundation.Log.Tracer.Debug("发出合同提醒定时触发数据:\r\n"+strRes.ToString());
            EngineWS.EngineWcfGlobalFunctionClient EngineClient = new EngineWS.EngineWcfGlobalFunctionClient();
            //EngineClient.SaveEventData(strRes.ToString());
            SMT.Foundation.Log.Tracer.Debug("发出合同提醒定时触发数据,调用方法：" + EngineClient.Endpoint.Address.ToString()
                + "WFAddTimingTrigger:" + strRes.ToString());
            EngineClient.WFAddTimingTrigger(trigger);
            //EngineClient.WFAddTimingTrigger(trigger);
            }catch(Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
        }

        /// <summary>
        /// AES算法加密数据
        /// </summary>
        /// <param name="input">加密前的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string Encrypt(string input)
        {
            // 盐值
            string saltValue = "saltValue";
            // 密码值
            string pwdValue = "pwdValue";

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(input);
            byte[] salt = System.Text.UTF8Encoding.UTF8.GetBytes(saltValue);

            // AesManaged - 高级加密标准(AES) 对称算法的管理类
            System.Security.Cryptography.AesManaged aes = new System.Security.Cryptography.AesManaged();

            // Rfc2898DeriveBytes - 通过使用基于 HMACSHA1 的伪随机数生成器，实现基于密码的密钥派生功能 (PBKDF2 - 一种基于密码的密钥派生函数)
            // 通过 密码 和 salt 派生密钥
            System.Security.Cryptography.Rfc2898DeriveBytes rfc = new System.Security.Cryptography.Rfc2898DeriveBytes(pwdValue, salt);

            /**/
            /*
         * AesManaged.BlockSize - 加密操作的块大小（单位：bit）
         * AesManaged.LegalBlockSizes - 对称算法支持的块大小（单位：bit）
         * AesManaged.KeySize - 对称算法的密钥大小（单位：bit）
         * AesManaged.LegalKeySizes - 对称算法支持的密钥大小（单位：bit）
         * AesManaged.Key - 对称算法的密钥
         * AesManaged.IV - 对称算法的密钥大小
         * Rfc2898DeriveBytes.GetBytes(int 需要生成的伪随机密钥字节数) - 生成密钥
         */

            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // 用当前的 Key 属性和初始化向量 IV 创建对称加密器对象
            System.Security.Cryptography.ICryptoTransform encryptTransform = aes.CreateEncryptor();

            // 加密后的输出流
            System.IO.MemoryStream encryptStream = new System.IO.MemoryStream();

            // 将加密后的目标流（encryptStream）与加密转换（encryptTransform）相连接
            System.Security.Cryptography.CryptoStream encryptor = new System.Security.Cryptography.CryptoStream
                (encryptStream, encryptTransform, System.Security.Cryptography.CryptoStreamMode.Write);

            // 将一个字节序列写入当前 CryptoStream （完成加密的过程）
            encryptor.Write(data, 0, data.Length);
            encryptor.Close();

            // 将加密后所得到的流转换成字节数组，再用Base64编码将其转换为字符串
            string encryptedString = Convert.ToBase64String(encryptStream.ToArray());

            return encryptedString;
        }

        /// <summary>
        /// 引擎需要的XML形式的实体字符串转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXml<T>(T objectdata, string SystemCode, string currentUserName)
        {
            StringBuilder sb = new StringBuilder();
            try
            {                
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine("<System>");
                Type objtype = objectdata.GetType();
                sb.AppendLine("<Name>" + SystemCode + "</Name>");
                sb.AppendLine("<Object Name=\"Approval\" Description=\"\">");
                PropertyInfo[] propinfos = objtype.GetProperties();
                foreach (PropertyInfo propinfo in propinfos)
                {
                    if (propinfo.Name.ToUpper() != "CHECKSTATE")
                    {
                        sb.AppendLine("<Attribute Name=\"" + propinfo.Name + "\" Description=\"" + "" + "\" DataType=\"" + GetDataTypeName(propinfo.PropertyType) + "\" DataValue=\"" + propinfo.GetValue(objectdata, null) + "\"/>");
                    }
                }
                sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + string.Empty + "\" DataValue=\"" + currentUserName + "\"/>");
                sb.AppendLine("</Object>");
                sb.AppendLine("</System>");
            }
            catch (Exception ex)
            {
                Utility.SaveLog("向员工" + currentUserName
                    + "发送薪资发放确认提醒的消息及邮件失败！错误信息为APPXML生成失败：" + ex.ToString()); 
            }
            return sb.ToString();

        }

        /// <summary>
        /// 获取Type类型名称
        /// </summary>
        /// <param name="tSourceDataType"></param>
        /// <returns></returns>
        private static string GetDataTypeName(Type tSourceDataType)
        {
            string strRes = string.Empty;
            if (tSourceDataType.Name == "String")
            {
                strRes = tSourceDataType.Name;
            }
            else if (tSourceDataType.FullName.Contains("System.DateTime"))
            {
                strRes = "DateTime";
            }
            else if (tSourceDataType.FullName.Contains("System.Decimal"))
            {
                strRes = "Decimal";
            }

            return strRes;
        }

        /// <summary>
        /// 删除指定地址下文件
        /// </summary>
        /// <param name="strPhysicalPath"></param>
        public static void DeleteUploadFile(string strPhysicalPath)
        {
            if (Directory.Exists(strPhysicalPath)) //如果存在这个文件夹删除之   
            {
                foreach (string d in Directory.GetFileSystemEntries(strPhysicalPath))
                {
                    if (File.Exists(d))
                    {
                        File.Delete(d); //直接删除其中的文件                          
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="strMsg"></param>
        public static void SaveLog(string strMsg)
        {
            if (!string.IsNullOrWhiteSpace(strMsg))
            {
                Tracer.Debug(strMsg);
            }
        }


    }
}
