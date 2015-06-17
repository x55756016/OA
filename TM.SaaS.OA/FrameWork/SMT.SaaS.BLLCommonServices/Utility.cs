using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.ServiceModel.Description;
using System.Collections.ObjectModel;
using System.Resources;
using SMT.Foundation.Log;
using System.ServiceModel;
using TM_SaaS_OA_EFModel;
using TM.SaaS.CommonBll;

namespace SMT.SaaS.BLLCommonServices
{
    public class BLLException : Exception
    {
        public BLLException(string msg)
            : base(msg)
        {
        }
    }
    public class Utility
    {

        T_SYS_USER CacheUser;
        T_HR_EMPLOYEE CachePerson;

        List<string> LstCompanys = new List<string>();//公司ID集合
        List<string> LstDepartments = new List<string>();//部门ID集合

        public Utility()
        {

        }
        /// <summary>
        /// 分配对象类别
        /// </summary>


        /// <summary>
        /// 构造打开特定Form的查询条件xml
        /// </summary>
        /// <param name="strFormName">完整的Form名称(含命名空间)</param>
        /// <param name="strModelId">当前实体的主键ID</param>
        /// <param name="strFormType">打开Form时设置的FormType("VIEW", "EDIT", "AUDIT")</param>
        /// <returns></returns>
        public static string SetSubmitXmlObj(string strDBName, string strFormName, string strModelId, string strFormType)
        {
            StringBuilder strTemp = new StringBuilder();

            if (!string.IsNullOrEmpty(strDBName) && !string.IsNullOrEmpty(strFormName) && !string.IsNullOrEmpty(strModelId) && !string.IsNullOrEmpty(strFormType))
            {
                string strCha = "ChargeApplyForm";
                string strBor = "BorrowAppForm";
                string strRep = "RepayApplyForm";
                if (strFormName.Contains(strCha) || strFormName.Contains(strBor) || strFormName.Contains(strRep))
                {
                    strTemp.Append(SetFBAnalysisXmlobj(strDBName, strFormName, strModelId, strFormType));
                }
                else
                {
                    strTemp.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    strTemp.Append("<System>");
                    strTemp.Append("	<AssemblyName>" + GetResourceValue(strDBName + "ASSEMBLY") + "</AssemblyName>");
                    strTemp.Append("	<PublicClass>" + GetResourceValue(strDBName + "CLASS") + "</PublicClass>");
                    strTemp.Append("	<ProcessName>" + GetResourceValue(strDBName + "PROCESS") + "</ProcessName>");
                    strTemp.Append("	<PageParameter>" + GetResourceValue(strFormName) + "</PageParameter>");
                    strTemp.Append("	<ApplicationOrder>" + strModelId + "</ApplicationOrder>");
                    strTemp.Append("	<FormTypes>" + GetFormTypeByDBName(strDBName, strFormType) + "</FormTypes>");
                    strTemp.Append("</System>");
                }
            }
            return strTemp.ToString();
        }
        /// <summary>
        ///得到日常管理的xml
        /// </summary>
        /// <param name="strDBName"></param>
        /// <param name="strFormName"></param>
        /// <param name="strModelId"></param>
        /// <param name="strFormType"></param>
        /// <returns></returns>
        private static string SetFBAnalysisXmlobj(string strDBName, string strFormName, string strModelId, string strFormType)
        {
            StringBuilder strTemp = new StringBuilder();

            strTemp.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            strTemp.Append("<System>");
            strTemp.Append("	<AssemblyName>SMT.FBAnalysis.UI</AssemblyName>");
            strTemp.Append("	<PublicClass>SMT.FBAnalysis.UI.Common.Utility</PublicClass>");
            strTemp.Append("	<ProcessName>CreateFormFromEngine</ProcessName>");
            strTemp.Append("	<PageParameter>" + GetResourceValue(strFormName) + "</PageParameter>");
            strTemp.Append("	<ApplicationOrder>" + strModelId + "</ApplicationOrder>");
            strTemp.Append("	<FormTypes>" + GetFormTypeByDBName(strDBName, strFormType) + "</FormTypes>");
            strTemp.Append("</System>");

            return strTemp.ToString();
        }
        /// <summary>
        /// 根据分系统名，变换FormType
        /// </summary>
        /// <param name="strDBName"></param>
        /// <param name="strFormType"></param>
        /// <returns></returns>
        private static string GetFormTypeByDBName(string strDBName, string strFormType)
        {
            if (strDBName == "FB")
            {
                switch (strFormType)
                {
                    case "Edit":
                        strFormType = "Edit";
                        break;
                    case "View":
                        strFormType = "Browse";
                        break;
                }
            }

            return strFormType;
        }

        /// <summary>
        /// 获取Form名称(含命名空间)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetResourceValue(string key)
        {
            ResourceManager ResourceMgr = new ResourceManager("SMT.SaaS.BLLCommonServices.EntityFormResource", Assembly.GetExecutingAssembly());
            string rslt = ResourceMgr.GetString(key);
            return string.IsNullOrEmpty(rslt) ? key : rslt;
        }

        /// <summary>
        /// 添加"我的单据"
        /// </summary>
        /// <param name="entity">源实体</param>
        public static void SubmitMyRecord<TEntity>(object entity)
        {
            SubmitMyRecord<TEntity>(entity, "0");
        }



        /// <summary>
        /// 添加"我的单据"
        /// </summary>
        /// <param name="entity">源实体</param>
        public static void SubmitMyRecord<TEntity>(object entity, string strIsForward)
        {
            try
            {
                //PersonalRecordServiceClient client = new PersonalRecordServiceClient();
                string strSystype = string.Empty, strSysName = string.Empty, strFormName = string.Empty, strModelCode = string.Empty, strModelDesp = string.Empty, strFormId = string.Empty, strSubmitXmlObj = string.Empty;

                Type a = entity.GetType();
                PropertyInfo[] piSource = a.GetProperties();

                var n = from m in piSource
                        where m.Name == "CHECKSTATE" || m.Name == "CHECKSTATES" || m.Name == "AUDITSTATE"
                        select m;

                if (n == null)
                {
                    return;
                }

                if (n.Count() == 0)
                {
                    return;
                }

                strSysName = GetSysName(a.FullName);
                strSystype = GetSystypeByName(strSysName);
                strModelCode = a.Name;
                strFormName = GetResourceValue(strModelCode);

                bool bIsExcludeTable = false;
                bIsExcludeTable = CheckExcludeTable(strSystype, strModelCode);
                if (bIsExcludeTable)
                {
                    return;
                }

                T_PF_PERSONALRECORD entSubmit = null;

                foreach (PropertyInfo prop in piSource)
                {
                    if (prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityReference)
                        || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.RelatedEnd)
                        || prop.PropertyType == typeof(System.Data.EntityState)
                        || prop.PropertyType == typeof(System.Data.EntityKey)
                        || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityObject))
                        continue;

                    if (entity is System.Data.Objects.DataClasses.EntityObject)
                    {
                        System.Data.Objects.DataClasses.EntityObject ent = entity as System.Data.Objects.DataClasses.EntityObject;
                        if (ent != null && ent.EntityKey != null && ent.EntityKey.EntityKeyValues != null && ent.EntityKey.EntityKeyValues.Count() > 0)
                        {
                            bool isKeyField = false;
                            foreach (var key in ent.EntityKey.EntityKeyValues)
                            {
                                if (key.Key == prop.Name)
                                {
                                    isKeyField = true;
                                    object obj = prop.GetValue(entity, null);
                                    if (obj != null)
                                    {
                                        strFormId = obj.ToString();
                                    }
                                    break;
                                }
                            }

                            if (isKeyField)
                            {
                                //if (entSubmit == null && !string.IsNullOrEmpty(strFormId))
                                //{
                                //    entSubmit = client.GetPersonalRecordModelByModelID(strSystype, strModelCode, strFormId, strIsForward);
                                //}

                                continue;
                            }
                        }

                        if (entSubmit == null)
                        {
                            entSubmit = new T_PF_PERSONALRECORD();
                            entSubmit.PERSONALRECORDID = System.Guid.NewGuid().ToString();
                            entSubmit.SYSTYPE = strSystype;
                            entSubmit.MODELCODE = strModelCode;
                            entSubmit.MODELID = strFormId;
                            entSubmit.ISFORWARD = strIsForward;
                        };


                        if (entSubmit != null)
                        {
                            Type b = entSubmit.GetType();
                            PropertyInfo[] piTarget = b.GetProperties();

                            //prop.Name
                            object valueTemp = prop.GetValue(entity, null);
                            try
                            {
                                string strMemberName = prop.Name;
                                if (strMemberName == "CHECKSTATE" || strMemberName == "CHECKSTATES" || strMemberName == "AUDITSTATE")
                                {
                                    strMemberName = "CHECKSTATE";
                                }

                                var q = from t in piTarget
                                        where t.Name == strMemberName
                                        select t;

                                if (q.Count() == 0)
                                {
                                    continue;
                                }

                                PropertyInfo piTemp = q.FirstOrDefault();

                                if (valueTemp == null)
                                {
                                    if (piTemp.PropertyType.Name == "String")
                                    {
                                        valueTemp = "";
                                    }
                                    else if (piTemp.PropertyType.FullName.Contains("System.DateTime"))
                                    {
                                        DateTime dtTemp = new DateTime();
                                        valueTemp = dtTemp;
                                    }
                                    else if (piTemp.PropertyType.FullName.Contains("System.Decimal"))
                                    {
                                        valueTemp = 0;
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(valueTemp.ToString()))
                                    {
                                        if (piTemp.PropertyType.Name == "String")
                                        {
                                            valueTemp = "";
                                        }
                                        else if (piTemp.PropertyType.FullName.Contains("System.DateTime"))
                                        {
                                            DateTime dtTemp = new DateTime();
                                            valueTemp = dtTemp;
                                        }
                                        else if (piTemp.PropertyType.FullName.Contains("System.Decimal"))
                                        {
                                            valueTemp = 0;
                                        }
                                    }
                                }

                                if (piTemp.CanWrite)
                                {
                                    //piTemp.SetValue(entSubmit, Convert.ChangeType(valueTemp, piTemp.PropertyType), null);
                                    //if (piTemp.PropertyType.Name == "String")
                                    //    piTemp.SetValue(entSubmit, valueTemp.ToString(), null);
                                    //else if (piTemp.PropertyType.FullName.Contains("System.Decimal"))
                                    //    piTemp.SetValue(entSubmit, Convert.ToDecimal(valueTemp), null);
                                    //else if (piTemp.PropertyType.FullName.Contains("System.DateTime"))
                                    //    piTemp.SetValue(entSubmit, Convert.ToDateTime(valueTemp), null);
                                    if (piTemp.PropertyType.Name == "String")
                                        piTemp.SetValue(entSubmit, valueTemp.ToString(), null);
                                    else if (piTemp.PropertyType.FullName.Contains("System.Decimal"))
                                        piTemp.SetValue(entity, Convert.ToDecimal(valueTemp), null);
                                    else
                                        piTemp.SetValue(entSubmit, valueTemp, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                string e = ex.ToString();
                                Tracer.Debug("我的单据调用发生了错误:" + ex.ToString() + entSubmit.MODELCODE + " " + entSubmit.MODELID);
                            }
                        }
                    }
                }

                switch (entSubmit.CHECKSTATE)
                {
                    case "0":
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "Edit");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]还未提交！");
                        break;
                    case "1":
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "Audit");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]正在审核中！");
                        break;
                    case "2":
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "View");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]已经审核通过！");
                        break;
                    case "3":
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "View");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]已审核未通过！");
                        break;
                    default:
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "View");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]，还未提交！");
                        break;
                }

                entSubmit.MODELDESCRIPTION = strModelDesp;
                entSubmit.CONFIGINFO = strSubmitXmlObj;
                if (entSubmit != null)
                {
                    //Tracer.Debug("开始调用我的单据服务：");
                    //Tracer.Debug("CHECKSTATE：" + entSubmit.CHECKSTATE);
                    //Tracer.Debug("CONFIGINFO：" + entSubmit.CONFIGINFO);
                    //Tracer.Debug("CREATEDATE：" + entSubmit.CREATEDATE.ToString());
                    //Tracer.Debug("ISFORWARD：" + entSubmit.ISFORWARD);
                    //Tracer.Debug("ISVIEW：" + entSubmit.ISVIEW);
                    //Tracer.Debug("MODELCODE：" + entSubmit.MODELCODE);
                    //Tracer.Debug("MODELDESCRIPTION：" + entSubmit.MODELDESCRIPTION);
                    //Tracer.Debug("MODELID：" + entSubmit.MODELID);
                    //Tracer.Debug("OWNERCOMPANYID：" + entSubmit.OWNERCOMPANYID);
                    //Tracer.Debug("OWNERDEPARTMENTID：" + entSubmit.OWNERDEPARTMENTID);
                    //Tracer.Debug("OWNERID：" + entSubmit.OWNERID);
                    //Tracer.Debug("OWNERPOSTID：" + entSubmit.OWNERPOSTID);
                    //Tracer.Debug("PERSONALRECORDID：" + entSubmit.PERSONALRECORDID);
                    //Tracer.Debug("SYSTYPE：" + entSubmit.SYSTYPE);
                    //Tracer.Debug("单据ID："+entSubmit.MODELID + "模块名称："+entSubmit.CONFIGINFO);

                }
                //目前修改为只新增
                //if (AddPersonalRecord(entSubmit))
                //{
                //    Tracer.Debug("模块" + entSubmit.MODELCODE + " ID:" + entSubmit.MODELID + "调用了我的单据新建服务成功BLLCOMMONSERVICES！");
                //}
                //else
                //{
                //    Tracer.Debug("模块" + entSubmit.MODELCODE + " ID:" + entSubmit.MODELID + "调用了我的单据新建服务失败BLLCOMMONSERVICES！");
                //}


                //if (entSubmit.EntityKey != null)
                //{
                //    Tracer.Debug("调用了我的单据更新服务" + System.DateTime.Now.ToString());
                //    client.UpdatePersonalRecord(entSubmit);
                //}
                //else
                //{
                //    Tracer.Debug("调用了我的单据新建服务" + System.DateTime.Now.ToString());
                //    client.AddPersonalRecord(entSubmit);
                //}
            }
            catch (Exception ex)
            {
                Tracer.Debug("我的单据出现错误：" + ex.ToString() + System.DateTime.Now.ToString());
            }
        }
        private static string GetModelDescription(DateTime str1,string str2,string str3)
        {
            return string.Empty;
        }

        #region 添加我的单据，将OWNERID转换
        /// <summary>
        /// 添加我的单据 edit ljx
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="strIsForward">传递过来的参数默认为0</param>
        /// <param name="createuserid">添加人ID，替换属人ID</param>
        public static void SubmitMyRecord<TEntity>(object entity, string strIsForward, string createuserid)
        {
            try
            {
                //如果为空 直接退出
                if (string.IsNullOrEmpty(createuserid))
                {
                    return;
                }
                if (string.IsNullOrEmpty(strIsForward))
                {
                    strIsForward = "0";//如果为空，默认为0
                }
                //PersonalRecordServiceClient client = new PersonalRecordServiceClient();
                string strSystype = string.Empty, strSysName = string.Empty, strFormName = string.Empty, strModelCode = string.Empty, strModelDesp = string.Empty, strFormId = string.Empty, strSubmitXmlObj = string.Empty;

                Type a = entity.GetType();
                PropertyInfo[] piSource = a.GetProperties();

                var n = from m in piSource
                        where m.Name == "CHECKSTATE" || m.Name == "CHECKSTATES" || m.Name == "AUDITSTATE"
                        select m;

                if (n == null)
                {
                    return;
                }

                if (n.Count() == 0)
                {
                    return;
                }

                strSysName = GetSysName(a.FullName);
                strSystype = GetSystypeByName(strSysName);
                strModelCode = a.Name;
                strFormName = GetResourceValue(strModelCode);

                bool bIsExcludeTable = false;
                bIsExcludeTable = CheckExcludeTable(strSystype, strModelCode);
                if (bIsExcludeTable)
                {
                    return;
                }

                T_PF_PERSONALRECORD entSubmit = null;

                foreach (PropertyInfo prop in piSource)
                {
                    if (prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityReference)
                        || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.RelatedEnd)
                        || prop.PropertyType == typeof(System.Data.EntityState)
                        || prop.PropertyType == typeof(System.Data.EntityKey)
                        || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityObject))
                        continue;

                    if (entity is System.Data.Objects.DataClasses.EntityObject)
                    {
                        System.Data.Objects.DataClasses.EntityObject ent = entity as System.Data.Objects.DataClasses.EntityObject;
                        if (ent != null && ent.EntityKey != null && ent.EntityKey.EntityKeyValues != null && ent.EntityKey.EntityKeyValues.Count() > 0)
                        {
                            bool isKeyField = false;
                            foreach (var key in ent.EntityKey.EntityKeyValues)
                            {
                                if (key.Key == prop.Name)
                                {
                                    isKeyField = true;
                                    object obj = prop.GetValue(entity, null);
                                    if (obj != null)
                                    {
                                        strFormId = obj.ToString();
                                    }
                                    break;
                                }
                            }

                            if (isKeyField)
                            {
                                //if (entSubmit == null && !string.IsNullOrEmpty(strFormId))
                                //{
                                //    entSubmit = client.GetPersonalRecordModelByModelID(strSystype, strModelCode, strFormId, strIsForward);
                                //}

                                continue;
                            }
                        }

                        if (entSubmit == null)
                        {
                            entSubmit = new T_PF_PERSONALRECORD();
                            entSubmit.PERSONALRECORDID = System.Guid.NewGuid().ToString();
                            entSubmit.SYSTYPE = strSystype;
                            entSubmit.MODELCODE = strModelCode;
                            entSubmit.MODELID = strFormId;
                            entSubmit.ISFORWARD = strIsForward;
                        };


                        if (entSubmit != null)
                        {
                            Type b = entSubmit.GetType();
                            PropertyInfo[] piTarget = b.GetProperties();

                            //prop.Name
                            object valueTemp = prop.GetValue(entity, null);
                            try
                            {
                                string strMemberName = prop.Name;
                                if (strMemberName == "CHECKSTATE" || strMemberName == "CHECKSTATES" || strMemberName == "AUDITSTATE")
                                {
                                    strMemberName = "CHECKSTATE";
                                }

                                var q = from t in piTarget
                                        where t.Name == strMemberName
                                        select t;

                                if (q.Count() == 0)
                                {
                                    continue;
                                }

                                PropertyInfo piTemp = q.FirstOrDefault();

                                if (valueTemp == null)
                                {
                                    if (piTemp.PropertyType.Name == "String")
                                    {
                                        valueTemp = "";
                                    }
                                    else if (piTemp.PropertyType.FullName.Contains("System.DateTime"))
                                    {
                                        DateTime dtTemp = new DateTime();
                                        valueTemp = dtTemp;
                                    }
                                    else if (piTemp.PropertyType.FullName.Contains("System.Decimal"))
                                    {
                                        valueTemp = 0;
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(valueTemp.ToString()))
                                    {
                                        if (piTemp.PropertyType.Name == "String")
                                        {
                                            valueTemp = "";
                                        }
                                        else if (piTemp.PropertyType.FullName.Contains("System.DateTime"))
                                        {
                                            DateTime dtTemp = new DateTime();
                                            valueTemp = dtTemp;
                                        }
                                        else if (piTemp.PropertyType.FullName.Contains("System.Decimal"))
                                        {
                                            valueTemp = 0;
                                        }
                                    }
                                }

                                if (piTemp.CanWrite)
                                {

                                    if (piTemp.PropertyType.Name == "String")
                                        piTemp.SetValue(entSubmit, valueTemp.ToString(), null);
                                    else if (piTemp.PropertyType.FullName.Contains("System.Decimal"))
                                        piTemp.SetValue(entity, Convert.ToDecimal(valueTemp), null);
                                    else
                                        piTemp.SetValue(entSubmit, valueTemp, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                string e = ex.ToString();
                                Tracer.Debug("我的单据调用发生了错误:" + ex.ToString() + entSubmit.MODELCODE + " " + entSubmit.MODELID);
                            }
                        }
                    }
                }

                switch (entSubmit.CHECKSTATE)
                {
                    case "0":
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "Edit");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]还未提交！");
                        break;
                    case "1":
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "Audit");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]正在审核中！");
                        break;
                    case "2":
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "View");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]已经审核通过！");
                        break;
                    case "3":
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "View");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]已审核未通过！");
                        break;
                    default:
                        strSubmitXmlObj = SetSubmitXmlObj(strSysName, strFormName, strFormId, "View");
                        strModelDesp = GetModelDescription(entSubmit.CREATEDATE.Value, strModelCode, "您{0}的[{1}]，还未提交！");
                        break;
                }

                entSubmit.MODELDESCRIPTION = strModelDesp;
                entSubmit.CONFIGINFO = strSubmitXmlObj;
                if (!string.IsNullOrEmpty(createuserid))
                {
                    entSubmit.OWNERID = createuserid;
                }
                //if (entSubmit != null)
                //{
                //    Tracer.Debug("开始调用我的单据服务：");
                //    Tracer.Debug("CHECKSTATE：" + entSubmit.CHECKSTATE);
                //    Tracer.Debug("CONFIGINFO：" + entSubmit.CONFIGINFO);
                //    Tracer.Debug("CREATEDATE：" + entSubmit.CREATEDATE.ToString());
                //    Tracer.Debug("ISFORWARD：" + entSubmit.ISFORWARD);
                //    Tracer.Debug("ISVIEW：" + entSubmit.ISVIEW);
                //    Tracer.Debug("MODELCODE：" + entSubmit.MODELCODE);
                //    Tracer.Debug("MODELDESCRIPTION：" + entSubmit.MODELDESCRIPTION);
                //    Tracer.Debug("MODELID：" + entSubmit.MODELID);
                //    Tracer.Debug("OWNERCOMPANYID：" + entSubmit.OWNERCOMPANYID);
                //    Tracer.Debug("OWNERDEPARTMENTID：" + entSubmit.OWNERDEPARTMENTID);
                //    Tracer.Debug("OWNERID：" + entSubmit.OWNERID);
                //    Tracer.Debug("OWNERPOSTID：" + entSubmit.OWNERPOSTID);
                //    Tracer.Debug("PERSONALRECORDID：" + entSubmit.PERSONALRECORDID);
                //    Tracer.Debug("SYSTYPE：" + entSubmit.SYSTYPE);
                //    //Tracer.Debug("单据ID："+entSubmit.MODELID + "模块名称："+entSubmit.CONFIGINFO);

                //}
                //if (client.AddPersonalRecord(entSubmit))
                //{
                //    Tracer.Debug("模块" + entSubmit.MODELCODE + "ID:" + entSubmit.MODELID + "调用了我的单据新建服务" + "成功！");
                //}
                //else
                //{
                //    Tracer.Debug("模块" + entSubmit.MODELCODE + "ID:" + entSubmit.MODELID + "调用了我的单据新建服务" + "失败！");
                //}
                //if (entSubmit.EntityKey != null)
                //{
                //    Tracer.Debug("调用了我的单据更新服务" + System.DateTime.Now.ToString());
                //    client.UpdatePersonalRecord(entSubmit);
                //}
                //else
                //{
                //    Tracer.Debug("调用了我的单据新建服务" + System.DateTime.Now.ToString());
                //    client.AddPersonalRecord(entSubmit);
                //}
            }
            catch (Exception ex)
            {
                Tracer.Debug("我的单据出现错误：" + ex.ToString() + System.DateTime.Now.ToString());
            }
        }
        #endregion

        /// <summary>
        /// 检查当前提交的源单据是否有无对应需显示的Form
        /// </summary>
        /// <param name="strSystype">分系统代号</param>
        /// <param name="strModelCode">分系统下源单据对应的数据库表名</param>
        /// <returns></returns>
        private static bool CheckExcludeTable(string strSystype, string strModelCode)
        {
            bool bCheck = false;
            if (string.IsNullOrWhiteSpace(strSystype) || string.IsNullOrWhiteSpace(strModelCode))
            {
                return true;
            }

            string strCheckCode = strSystype + "_EXCLUDETABLES";
            string strRes = GetResourceValue(strCheckCode);
            bCheck = strRes.Contains(strModelCode);
            return bCheck;
        }


        /// <summary>
        /// 获取当前分系统的类型值
        /// </summary>
        /// <param name="strSysName"></param>
        /// <returns></returns>
        private static string GetSystypeByName(string strSysName)
        {
            string strRes = string.Empty;
            switch (strSysName)
            {
                case "HRM":
                    strRes = "HR";
                    break;
                case "OA":
                    strRes = "OA";
                    break;
                case "FB":
                    strRes = "FB";
                    break;
                default:
                    strRes = string.Empty;
                    break;
            }

            return strRes;
        }

        /// <summary>
        /// 获取当前分系统的类型名
        /// </summary>
        /// <param name="strFullClassName"></param>
        /// <returns></returns>
        private static string GetSysName(string strFullClassName)
        {
            string strRes = string.Empty;

            string[] strlist = strFullClassName.Split('_');
            if (strlist.Length > 2)
            {
                strRes = strlist[1].ToString().ToUpper();
            }

            return strRes;
        }

        /// <summary>
        /// 删除"我的单据"
        /// </summary>
        /// <param name="entity">源实体</param>
        public static void RemoveMyRecord<TEntity>(object entity)
        {
            try
            {

                //PersonalRecordServiceClient client = new PersonalRecordServiceClient();
                string strSystype = string.Empty, strSysName = string.Empty, strModelCode = string.Empty, strFormId = string.Empty, strIsForward = string.Empty;

                bool isKeyField = false;

                Type a = entity.GetType();
                PropertyInfo[] piSource = a.GetProperties();

                var n = from m in piSource
                        where m.Name == "CHECKSTATE" || m.Name == "CHECKSTATES" || m.Name == "AUDITSTATE"
                        select m;

                if (n == null)
                {
                    return;
                }

                if (n.Count() == 0)
                {
                    return;
                }

                strSysName = GetSysName(a.FullName);
                strSystype = GetSystypeByName(strSysName);
                strModelCode = a.Name;
                strIsForward = "0";

                foreach (PropertyInfo prop in piSource)
                {
                    if (prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityReference)
                        || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.RelatedEnd)
                        || prop.PropertyType == typeof(System.Data.EntityState)
                        || prop.PropertyType == typeof(System.Data.EntityKey)
                        || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityObject))
                        continue;

                    if (entity is System.Data.Objects.DataClasses.EntityObject)
                    {
                        System.Data.Objects.DataClasses.EntityObject ent = entity as System.Data.Objects.DataClasses.EntityObject;
                        if (ent != null && ent.EntityKey != null && ent.EntityKey.EntityKeyValues != null && ent.EntityKey.EntityKeyValues.Count() > 0)
                        {

                            foreach (var key in ent.EntityKey.EntityKeyValues)
                            {
                                if (key.Key == prop.Name)
                                {
                                    isKeyField = true;
                                    object obj = prop.GetValue(entity, null);
                                    if (obj != null)
                                    {
                                        strFormId = obj.ToString();
                                    }
                                    break;
                                }
                            }

                            if (isKeyField && !string.IsNullOrEmpty(strFormId))
                            {
                                break;
                            }
                        }
                    }
                }

                if (!isKeyField || string.IsNullOrEmpty(strFormId))
                {
                    return;
                }

                //T_PF_PERSONALRECORD entSubmit = client.GetPersonalRecordModelByModelID(strSystype, strModelCode, strFormId, strIsForward);

                //if (entSubmit == null)
                //{
                //    return;
                //}

                //if (!string.IsNullOrEmpty(entSubmit.PERSONALRECORDID))
                //{
                //    client.DeletePersonalRecord(entSubmit.PERSONALRECORDID);
                //}
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
        }

        /// <summary>
        /// 删除"我的单据"
        /// </summary>
        /// <param name="strSystype">源单据的系统类型</param>
        /// <param name="strModelCode">源单据的模块代码</param>
        /// <param name="strFormId">源单据的单据ID</param>
        public static void RemoveMyRecord(string strSystype, string strModelCode, string strFormId)
        {
            //try
            //{
            //    if (string.IsNullOrWhiteSpace(strSystype) || string.IsNullOrWhiteSpace(strModelCode) || string.IsNullOrWhiteSpace(strFormId))
            //    {
            //        return;
            //    }

            //    PersonalRecordServiceClient client = new PersonalRecordServiceClient();
            //    T_PF_PERSONALRECORD entSubmit = client.GetPersonalRecordModelByModelID(strSystype, strModelCode, strFormId, "0");

            //    if (entSubmit == null)
            //    {
            //        return;
            //    }

            //    if (!string.IsNullOrEmpty(entSubmit.PERSONALRECORDID))
            //    {
            //        client.DeletePersonalRecord(entSubmit.PERSONALRECORDID);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Tracer.Debug(ex.ToString());
            //}
        }

        #region 令牌服务 lizh

        /// <summary>
        /// 仅供CreateToken使用
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        //private static TokenServiceWS.T_SYS_USER ToTokenUser(object myUser)
        //{
        //    if (myUser == null)
        //        return null;
        //    Type type = myUser.GetType();
        //    TokenServiceWS.T_SYS_USER tokenUser = new TokenServiceWS.T_SYS_USER();
        //    PropertyInfo pi = null;
        //    pi = type.GetProperty("CREATEDATE");

        //    tokenUser.CREATEDATE = GetPropertyValue<DateTime?>("CREATEDATE", type, myUser);
        //    tokenUser.CREATEUSER = GetPropertyValue<string>("CREATEUSER", type, myUser);
        //    tokenUser.EMPLOYEECODE = GetPropertyValue<string>("EMPLOYEECODE", type, myUser);
        //    tokenUser.EMPLOYEEID = GetPropertyValue<string>("EMPLOYEEID", type, myUser);
        //    tokenUser.EMPLOYEENAME = GetPropertyValue<string>("EMPLOYEENAME", type, myUser);
        //    tokenUser.ISENGINEMANAGER = GetPropertyValue<string>("ISENGINEMANAGER", type, myUser);
        //    tokenUser.ISFLOWMANAGER = GetPropertyValue<string>("ISFLOWMANAGER", type, myUser);
        //    tokenUser.ISMANGER = GetPropertyValue<decimal?>("ISMANGER", type, myUser);
        //    tokenUser.OWNERCOMPANYID = GetPropertyValue<string>("OWNERCOMPANYID", type, myUser);
        //    tokenUser.OWNERDEPARTMENTID = GetPropertyValue<string>("OWNERDEPARTMENTID", type, myUser);
        //    tokenUser.OWNERID = GetPropertyValue<string>("OWNERID", type, myUser);
        //    tokenUser.OWNERPOSTID = GetPropertyValue<string>("OWNERPOSTID", type, myUser);
        //    tokenUser.PASSWORD = GetPropertyValue<string>("PASSWORD", type, myUser);
        //    tokenUser.REMARK = GetPropertyValue<string>("REMARK", type, myUser);
        //    tokenUser.STATE = GetPropertyValue<string>("STATE", type, myUser);
        //    tokenUser.SYSUSERID = GetPropertyValue<string>("SYSUSERID", type, myUser);
        //    //tokenUser.T_SYS_USERROLE = (DateTime?)type.GetProperty("T_SYS_USERROLE").GetValue(myUser, null);
        //    tokenUser.UPDATEDATE = GetPropertyValue<DateTime?>("UPDATEDATE", type, myUser);
        //    tokenUser.UPDATEUSER = GetPropertyValue<string>("UPDATEUSER", type, myUser);
        //    tokenUser.USERNAME = GetPropertyValue<string>("USERNAME", type, myUser);

        //    return tokenUser;
        //}

        //private static T GetPropertyValue<T>(string propertyName, Type type, object source)
        //{
        //    PropertyInfo pi = type.GetProperty(propertyName);
        //    if (pi == null)
        //        return default(T);
        //    return (T)pi.GetValue(source, null);

        //}
        ///// <summary>
        ///// 供权限登录方法调用，返回该用户的令牌
        ///// </summary>
        ///// <param name="permissionUser">权限系统中的T_SYS_USER类型</param>
        ///// <returns>令牌</returns>
        //public static string CreateToken(object permissionUser)
        //{
        //    if (tokenServiceClient == null)
        //    {
        //        tokenServiceClient = new TokenServiceWS.TokenServiceClient();
        //    }
        //    #region 令牌代码
        //    TokenServiceWS.T_SYS_USER tokenUser = ToTokenUser(permissionUser);
        //    //TokenServiceWS.TokenServiceClient tokenServiceClient = new TokenServiceWS.TokenServiceClient();
        //    TokenServiceWS.TokenResult tokenResult = tokenServiceClient.CreateToken(tokenUser);
        //    //表示令牌生成成功
        //    if (string.IsNullOrEmpty(tokenResult.ErrorInfo))
        //    {
        //        return tokenResult.Token;
        //    }
        //    else//表示令牌服务报错
        //    {
        //        return null;
        //    }
        //    #endregion
        //}

        ///// <summary>
        ///// 供平台后台注销方法使用，注销令牌
        ///// </summary>
        ///// <param name="token"></param>
        ///// <returns>注销是否成功</returns>
        //public static bool Logout(string token)
        //{
        //    if (tokenServiceClient == null)
        //    {
        //        tokenServiceClient = new TokenServiceWS.TokenServiceClient();
        //    }
        //    //TokenServiceWS.TokenServiceClient tokenServiceClient = new TokenServiceWS.TokenServiceClient();
        //    TokenServiceWS.TokenResult tokenResult = tokenServiceClient.RemoveToken(token);
        //    //表示令牌注销成功
        //    if (string.IsNullOrEmpty(tokenResult.ErrorInfo))
        //    {

        //        return true;

        //    }
        //    else//表示令牌服务报错
        //    {
        //        return false;
        //    }
        //}


        private static string GetHeaderValue(string name, string ns)
        {
            var headers = OperationContext.Current.IncomingMessageHeaders;
            var index = headers.FindHeader(name, ns);
            if (index > -1)
                return headers.GetHeader<string>(index);
            else
                return null;
        }
        #endregion


/// <summary>
        /// 通知MVC缓存更新缓存的实体
        /// </summary>
        /// <param name="entityName">实体命</param>
        /// <param name="entityKey">实体id</param>
        /// <param name="action">动作：增，删，改</param>
        //public static void MvcCacheClearAsync(string entityName, string entityKey, SMT.SaaS.BLLCommonServices.MVCCacheSV.EntityAction action)
        //{
            
        //    try
        //    {
        //        CacheServiceClient c = new CacheServiceClient();
        //        c.UpdateCacheDataAsync(entityName, entityKey, action);
               
        //    }
        //    catch(Exception ex)
        //    {
        //        SMT.Foundation.Log.Tracer.Debug("通知MVC缓存清空错误，实体：" + entityName+" 实体id:"+entityKey + ex.ToString());
        //    }
        //}

    
    }


}
