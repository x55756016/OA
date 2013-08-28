/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：PersonalRecordService.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/26 9:19:12   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Engine.Services.BLL;
using EngineDataModel;
using SMT.SaaS.Common;
using System.Web;
using System.Web.Caching;

namespace SMT.Workflow.Engine.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class PersonalRecordService : IPersonalRecordService
    {

        public bool AddPersonalRecord(T_PF_PERSONALRECORD model)
        {
            Record.WriteLogFunction("新增AddPersonalRecord():SYSTYPE:" + model.SYSTYPE + "||MODELCODE:" + model.MODELCODE + "||MODELID:" + model.MODELID + "||OWNERID" + model.OWNERID + "||转发ISFORWARD:" + model.ISFORWARD + "");
            PersonalRecordBLL bll = new PersonalRecordBLL();
            return bll.AddPersonalRecord(model);
        }

        public bool AddPersonalRecords(List<T_PF_PERSONALRECORD> models)
        {

            Record.WriteLogFunction("新增AddPersonalRecords(2):SYSTYPE:" + models[0].SYSTYPE + "||MODELCODE:" + models[0].MODELCODE + "||MODELID:" + models[0].MODELID + "||OWNERID" + models[0].OWNERID + "||转发ISFORWARD:" + models[0].ISFORWARD + "");
            PersonalRecordBLL bll = new PersonalRecordBLL();
            return bll.AddPersonalRecords(models);
        }

        public bool UpdatePersonalRecord(T_PF_PERSONALRECORD model)
        {
            string message = "修改UpdatePersonalRecord():SYSTYPE:" + model.SYSTYPE + "||MODELCODE:" + model.MODELCODE + "||MODELID:" + model.MODELID + "||OWNERID" + model.OWNERID + "||OWNERCOMPANYID:" + model.OWNERCOMPANYID + "";
            message += "||CHECKSTATE:" + model.CHECKSTATE + "||CONFIGINFO:" + model.CONFIGINFO + "||PERSONALRECORDID:" + model.PERSONALRECORDID + "||OWNERPOSTID" + model.OWNERPOSTID + "";
            Record.WriteLogFunction(message);
            PersonalRecordBLL bll = new PersonalRecordBLL();
            return bll.UpdatePersonalRecord(model);
        }

        public bool DeletePersonalRecord(string _personalrecordID)
        {
            Record.WriteLogFunction("DeletePersonalRecord()||_personalrecordID:" + _personalrecordID + "");
            PersonalRecordBLL bll = new PersonalRecordBLL();
            return bll.DeletePersonalRecord(_personalrecordID);
        }

        public T_PF_PERSONALRECORD GetPersonalRecordModelByID(string _personalrecordID)
        {
            Record.WriteLogFunction("GetPersonalRecordModelByID()||_personalrecordID:" + _personalrecordID + "");
            PersonalRecordBLL bll = new PersonalRecordBLL();
            return bll.GetPersonalRecordModelByID(_personalrecordID);
        }

        public T_PF_PERSONALRECORD GetPersonalRecordModelByModelID(string _sysType, string _modelCode, string _modelID, string _IsForward)
        {
            Record.WriteLogFunction("GetPersonalRecordModelByModelID()||_sysType:" + _sysType + "||_modelCode:" + _modelCode + "||_modelID:" + _modelID + "||_IsForward:" + _IsForward + "");
            PersonalRecordBLL bll = new PersonalRecordBLL();
            return bll.GetPersonalRecordModelByModelID(_sysType, _modelCode, _modelID, _IsForward);
        }

        public List<T_PF_PERSONALRECORD> GetPersonalRecord(int pageIndex, string strOrderBy, string checkstate, string filterString, string strCreateID,
            string BeginDate, string EndDate, ref int pageCount)
        {
            PersonalRecordBLL bll = new PersonalRecordBLL();
            Record.WriteLogFunction(
           "调用方法GetPersonalRecord（）||pageIndex:" + pageIndex + "||strOrderBy:" + strOrderBy + "||checkstate:" + checkstate + "||filterString:" + filterString + "||strCreateID:" + strCreateID + "  ");
            if (string.IsNullOrWhiteSpace(strCreateID))
            {
                Record.WriteLogFunction("调用方法GetPersonalRecord（）||strCreateID:" + strCreateID + " ");
                return null;
            }
            return bll.GetPersonalRecord(pageIndex, strOrderBy, checkstate, filterString, strCreateID, BeginDate, EndDate, ref pageCount);
        }

        public List<T_PF_PERSONALRECORD> GetPersonalRecordList(int pageIndex, string strOrderBy, string checkstate, string filterString,
             string strCreateID, string Isforward, string BeginDate, string EndDate, ref int pageCount)
        {
            PersonalRecordBLL bll = new PersonalRecordBLL();
            Record.WriteLogFunction("调用方法GetPersonalRecordList（）||pageIndex:" + pageIndex + "||strOrderBy:" + strOrderBy + "||checkstate:" + checkstate + "||filterString:" + filterString + "||strCreateID:" + strCreateID + "  ");
            if (string.IsNullOrWhiteSpace(strCreateID))
            {
                Record.WriteLogFunction("调用方法GetPersonalRecordList（）||strCreateID:" + strCreateID + " ");
                return null;
            }
            return bll.GetPersonalRecordList(pageIndex, strOrderBy, checkstate, filterString, strCreateID, Isforward, BeginDate, EndDate, ref pageCount);
        }
        /// <summary>
        /// 返回我的单据实体列表[LONGKC]
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="checkstate"></param>
        /// <param name="filterString"></param>
        /// <param name="ownerid"></param>
        /// <param name="Isforward"></param>
        /// <param name="BeginDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_PF_PERSONALRECORD> GetPersonalRecordListNew(int pageIndex, string strOrderBy, string checkstate, string filterString,
             string ownerid, string Isforward, string BeginDate, string EndDate, ref int pageCount)
        {
            PersonalRecordBLL bll = new PersonalRecordBLL();
            Record.WriteLogFunction("调用方法GetPersonalRecordList（）||pageIndex:" + pageIndex + "||strOrderBy:" + strOrderBy + "||checkstate:" + checkstate + "||filterString:" + filterString + "||ownerid:" + ownerid + "  ");
            if (string.IsNullOrWhiteSpace(ownerid))
            {                
                Record.WriteLogFunction("调用方法GetPersonalRecordList（）||ownerid:" + ownerid + " 没有找到单据所属人的ID");
                return null;
            }
            return bll.GetPersonalRecordListNew(pageIndex, strOrderBy, checkstate, filterString, ownerid, Isforward, BeginDate, EndDate, ref pageCount);
        }

        #region ICacheContract
        /// <summary>
        /// 缓存数据,如果指定key已有缓存数据,则更新已有缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheObject"></param>
        /// <param name="mode">缓存的共享范围及生命周期</param>
        public void Cache(string key, object cacheObject)
        {

            object orginalObject = HttpRuntime.Cache.Get(key);
            if (orginalObject != null)
            {
                HttpRuntime.Cache.Remove(key);
            }
            HttpRuntime.Cache.Add(key, cacheObject, null, DateTime.Now.AddYears(1), TimeSpan.Zero, CacheItemPriority.Default, null);

        }

        /// <summary>
        /// 移除指定key值的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mode"></param>
        public void RemoveCache(string key)
        {

            HttpRuntime.Cache.Remove(key);

        }

        /// <summary>
        /// 获取指定key值的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public object GetCacheObject(string key)
        {
            return HttpRuntime.Cache.Get(key);

        }
        #endregion
    }
}
