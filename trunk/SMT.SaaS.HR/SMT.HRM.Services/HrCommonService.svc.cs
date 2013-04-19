using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_HRM_EFModel;
using SMT.HRM.BLL;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.HRM.CustomModel;
using System.Configuration;
using System.Data;
using SMT.Foundation.Log;

namespace SMT.HRM.Services
{
    [ServiceContract(Namespace = "")]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class HrCommonService
    {
        //#region T_HR_SYSTEMSETTING 系统参数设置

        ///// <summary>
        ///// 用于实体Grid中显示数据的分页查询
        ///// </summary>
        ///// <param name="pageIndex">当前页</param>
        ///// <param name="pageSize">每页显示条数</param>
        ///// <param name="sort">排序字段</param>
        ///// <param name="filterString">过滤条件</param>
        ///// <param name="paras">过滤条件中的参数值</param>
        ///// <param name="pageCount">返回总页数</param>
        ///// <returns>查询结果集</returns>
        //[OperationContract]
        //public List<T_HR_SYSTEMSETTING> GetSystemParamSetPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        //{
        //    SystemSettingBLL bll = new SystemSettingBLL();
        //    IQueryable<T_HR_SYSTEMSETTING> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount,userID);
        //    return q.Count() > 0 ? q.ToList() : null;
        //}
        ///// <summary>
        ///// 根据系统参数ID查询实体
        ///// </summary>
        ///// <param name="SystemParamSetID">系统参数ID</param>
        ///// <returns>返回系统参数实体</returns>
        //[OperationContract]
        //public T_HR_SYSTEMSETTING GetSystemParamSet(string SystemParamSetID)
        //{
        //    SystemSettingBLL bll = new SystemSettingBLL();
        //    return bll.GetSystemParamSet(SystemParamSetID);
        //}
        ///// <summary>
        ///// 根据系统类型查询实体
        ///// </summary>
        ///// <param name="SystemParamSetID">参数类型</param>
        ///// <returns>返回系统参数实体</returns>
        //[OperationContract]
        //public List<T_HR_SYSTEMSETTING> GetSystemParamSetByType(string SystemParamSetID)
        //{
        //    SystemSettingBLL bll = new SystemSettingBLL();
        //    return bll.GetSystemParamSetByType(SystemParamSetID);
        //}
        ///// <summary>
        ///// 新增系统参数设置
        ///// </summary>
        ///// <param name="entity">系统参数设置实体</param>
        //[OperationContract]
        //public void AddSystemParamSet(T_HR_SYSTEMSETTING entity)
        //{
        //    SystemSettingBLL bll = new SystemSettingBLL();
        //    bll.Add(entity);
        //}
        ///// <summary>
        ///// 更新系统参数设置记录
        ///// </summary>
        ///// <param name="entity">系统参数设置实体</param>
        //[OperationContract]
        //public void SystemParamSetUpdate(T_HR_SYSTEMSETTING entity)
        //{
        //    SystemSettingBLL bll = new SystemSettingBLL();
        //    bll.SystemParamSetUpdate(entity);
        //}
        ///// <summary>
        ///// 删除系统参数设置实体
        ///// </summary>
        ///// <param name="CustomGuerdonSets">系统参数设置ID组</param>
        ///// <returns>是否删除成功</returns>
        //[OperationContract]
        //public bool SystemParamSetDelete(string[] SystemParamSetID)
        //{
        //    SystemSettingBLL bll = new SystemSettingBLL();
        //    int rslt = bll.SystemParamSetDelete(SystemParamSetID);
        //    return (rslt > 0);
        //}

        //#endregion

        #region "获取WebConfig配置信息"
        [OperationContract]
        public string GetAppConfigByName(string AppConfigName)
        {
            string strConfigValue = string.Empty;
            try
            {
                strConfigValue = ConfigurationManager.AppSettings[AppConfigName].ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            return strConfigValue;
        }
        #endregion 

        #region "获取实体名称的属性"
        [OperationContract]
        public List<string> GetEntityProNameByEnityName(string entityName, string masterIDName)
        {
            return Utility.GetProByEntityName(entityName, masterIDName);
        }
        #endregion

        [OperationContract]
        public DataTable CustomerQuery(string Sqlstring,ref string ReturnMSG)
        {
            DataTable dt = new DataTable();
            try
            {
              
                BaseBll<T_HR_COMPANY> bll = new BaseBll<T_HR_COMPANY>();
                dt = bll.CustomerQuery(Sqlstring) as DataTable;
            }
            catch (Exception ex)
            {
                ReturnMSG = ex.ToString();
                Tracer.Debug(ex.ToString());
            }
            return dt;
        }
    }

    //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    //[ServiceContract]
    //public class TestUsersService
    //{
    //    private TestUserBLL BllDntUsers = new TestUserBLL();

    //    [OperationContract]
    //    public string TestUsersDoWork()
    //    {
    //        return "ok";
    //    }
    //    [OperationContract]
    //    public List<TEST_USERS> TestUsersGetTable()
    //    {
    //        return BllDntUsers.GetTable().ToList();
    //    }

    //    [OperationContract]
    //    public TEST_USERS GetUserById(string UserName)
    //    {
    //        return BllDntUsers.GetUserById(UserName);
    //    }

    //    [OperationContract]
    //    public void TestUsersAdd(TEST_USERS obj)
    //    {
    //        bool sucess = BllDntUsers.Add(obj);
    //        if (sucess == false)
    //        {
    //            throw new Exception("添加数据失败");
    //        }
    //    }
    //    [OperationContract]
    //    public void TestUsersUpdate(TEST_USERS obj)
    //    {
    //        BllDntUsers.Update(obj);
    //    }
    //    [OperationContract]
    //    public bool TestUsersDelete(string userName)
    //    {
    //        return BllDntUsers.Delete(userName);
    //    }
    //    [OperationContract]
    //    public object CustomerQuery(string Sqlstring)
    //    {
    //        return BllDntUsers.CustomerQuery(Sqlstring);
    //    }
    //}
}
