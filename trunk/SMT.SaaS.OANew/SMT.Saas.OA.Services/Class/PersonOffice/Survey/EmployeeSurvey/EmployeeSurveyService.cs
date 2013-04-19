using System;
using System.Collections.Generic;
using System.Linq;
using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using System.ServiceModel;
using SMT.SaaS.OA.DAL.Views;
using SMT.Foundation.Log;


namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {

        #region 新增
        /// <summary>
        /// 新增调查方案、题目、答案
        /// </summary>
        [OperationContract]
        public bool AddRequireMaster(V_EmployeeSurveyMaster addMasterKey)
        {
            try
            {
                if (addMasterKey != null&&addMasterKey.answerList.Count() > 0 )
                {
                    using (EmployeeSurveyBll addBll = new EmployeeSurveyBll())
                    {
                       bool add= addBll.AddRequireMaster(addMasterKey);
                       return add;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查方案EmployeeSurveyService-AddRequireMaster" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改调查方案、题目、答案
        /// </summary>
        [OperationContract]
        public bool UpdRequireMaster(V_EmployeeSurveyMaster updMaterKey)
        {
            try
            {
              if (updMaterKey != null&&updMaterKey.answerList.Count() > 0 &&!string.IsNullOrEmpty(updMaterKey.RequireMasterId))
              {
                  using (EmployeeSurveyBll updBll = new EmployeeSurveyBll())
                  {
                      return updBll.UpdRequireMaster(updMaterKey);
                  }
              }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查方案EmployeeSurveyService-UpdRequireMaster" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 载入时查询
        /// <summary>
        /// 方案子页面载入时查询
        /// </summary>
        [OperationContract]
        public V_EmployeeSurveyMaster GetMasterDataByLoading(string masterKey)
        {
            try
            {
                if (!string.IsNullOrEmpty(masterKey))
                {
                    V_EmployeeSurveyMaster loadList=null;
                    using (EmployeeSurveyBll loadBll = new EmployeeSurveyBll())
                    {
                        loadList = loadBll.GetMasterDataByLoading(masterKey);
                        return loadList != null ? loadList : null;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查方案EmployeeSurveyService-GetMasterDataByLoading" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 选择时查询
        #endregion

        #region 招聘模块查询接口

        /// <summary>
        /// 根据审核状态获取相关数据
        /// </summary>
         [OperationContract]
        public List<T_OA_REQUIREMASTER> GetSurveyDataByCultivateForCheckState(string checkState)
        {
            if (!string.IsNullOrEmpty(checkState))
            {
                using (EmployeeSurveyBll getBll = new EmployeeSurveyBll())
                {
                    return getBll.GetSurveyDataByCultivateForCheckState(checkState) != null ? getBll.GetSurveyDataByCultivateForCheckState(checkState).ToList() : null;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据申请ID和参与调查的员工ID获取结果数据
        /// </summary>
         [OperationContract]
        public List<T_OA_REQUIRERESULT> GetResultByCultivateForID(string requireID,string onwerID)
        {
            if (!(string.IsNullOrEmpty(requireID)&&(string.IsNullOrEmpty(onwerID))))
            {
                using (EmployeeSurveyBll getBll = new EmployeeSurveyBll())
                {
                    IQueryable<T_OA_REQUIRERESULT> handerList = null;
                    handerList = getBll.GetResultByCultivateForID(requireID, onwerID);
                    return handerList.Count()>0 ? handerList.ToList() : null;
                }
            }
            return null;
        }

        /// <summary>
         /// 选择员工调查方案，并排序分页
        /// </summary>
         [OperationContract]
        public List<V_EmployeeSurveyMaster> GetEmployeeSurveyByCheckstateAndDate(int pageCount, int pageIndex, int pageSize, string checkstate, DateTime[] datetimes)
        {
            if (datetimes.Count() >1)
            {
                IQueryable<V_EmployeeSurveyMaster> returnList;
                using (EmployeeSurveyBll getBll = new EmployeeSurveyBll())
                {
                    returnList=getBll.GetEmployeeSurveyByCheckstateAndDate(pageCount, pageIndex, pageSize,checkstate, datetimes);
                    return returnList.Count() > 0 ? returnList.ToList() : null;
                }
            }
            return null;
        }


        #endregion
    }
}