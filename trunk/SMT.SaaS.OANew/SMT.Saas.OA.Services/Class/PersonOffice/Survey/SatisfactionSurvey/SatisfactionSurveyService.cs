using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using System.ServiceModel;
using SMT.SaaS.OA.DAL.Views;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        #region 增
        /// <summary>
        /// 新增方案表数据
        /// </summary>
        /// <param name="AddRequireEntity">方案表数据</param>
        /// <returns>返回是否添加成功</returns>
        [OperationContract]
        public bool AddSatisfactionMaster(T_OA_SATISFACTIONMASTER addMasterEntity)
        {
            using (SatisfactionSurveyBll addBll = new SatisfactionSurveyBll())
            {
                return addBll.AddSatisfactionMaster(addMasterEntity);
            }
        }
        #endregion

        #region 删
        #endregion

        #region 该
        /// <summary>
        /// 修改方案和子表
        /// </summary>
        /// <param name="UpdEntity">主表带子表数据</param>
        /// <returns>是否修改成功</returns>
        [OperationContract]
        public bool UpdSatisfactionMaster(T_OA_SATISFACTIONMASTER updMasterEntity)
        {
            using (SatisfactionSurveyBll updBll = new SatisfactionSurveyBll())
            {
                return updBll.UpdSatisfactionMaster(updMasterEntity);
            }
        }

        #endregion

        #region 查
        /// <summary>
        /// 根据审核条件,创建日期获取满意度调查申请集合
        /// </summary>
        /// <param name="pageCount">页面当前数目</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页面项的数目</param>
        /// <param name="checkstate">审核状态</param>
        /// <param name="dateTimes">开始与结束时间</param>
        /// <returns>返回符合条件的申请</returns>
        [OperationContract]
        public List<V_Satisfactions> GetMasterByCheckstateAndDate(int pageCount, int pageIndex, int pageSize, string checkstate, DateTime[] dateTimes)
         {
             if(!string.IsNullOrEmpty(checkstate)&&dateTimes.Count()>=2)
             {
                 using (SatisfactionSurveyBll masterBll = new SatisfactionSurveyBll())
                 {
                    IQueryable<V_Satisfactions> dataList=  masterBll.GetMasterByCheckstateAndDate(pageCount, pageIndex, pageSize, checkstate, dateTimes);
                    return dataList != null && dataList.Count() > 0 ? dataList.ToList() : null;
                 }
              } 
             return null;
         }
        /// <summary>
        /// 子页面加载时取数据
        /// </summary>
        /// <param name="appId">申请表主键</param>
        /// <returns>对应主键的数据</returns>
        [OperationContract]
        public T_OA_SATISFACTIONMASTER GetSatisfactionMasterChild(string masterId)
        {
            if (!string.IsNullOrEmpty(masterId))
            {
                using (SatisfactionSurveyBll getBll = new SatisfactionSurveyBll())
                {
                   T_OA_SATISFACTIONMASTER masterEntity=getBll.GetSatisfactionMasterChild(masterId);
                return  masterEntity != null ? masterEntity : null;
                }
            }
            return null;
        }
        #endregion
    }
}