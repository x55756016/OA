using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.DAL.Views;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        #region 新增
        /// <summary>
        /// 新增满意度调查申请
        /// </summary>
        /// <param name="addRequireEntity">满意度调查实体</param>
        /// <param name="distributeList">分布范围实体集合</param>
        /// <returns>返回是否新增成功</returns>
        [OperationContract]
        public bool AddSatisfactionApp(V_Satisfactions addView)
        {
            if (!string.IsNullOrEmpty(addView.Satisfactionmasterid))
            {
                using (SatisfactionAppBll appBll = new SatisfactionAppBll())
                {
                    return appBll.AddSatisfactionApp(addView);
                }
            }
            return false;
        }

        #endregion

        #region 修该
        /// <summary>
        /// 修改满意度调查申请
        /// </summary>
        /// <param name="updRequireEntity">满意度调查申请实体</param>
        /// <param name="updDistributeList">发布范围实体集合</param>
        /// <returns>返回是否修改成功</returns>
        [OperationContract]
        public bool UpdSatisfactionApp(V_Satisfactions updView)
        {
            if (!string.IsNullOrEmpty(updView.Satisfactionmasterid) && !string.IsNullOrEmpty(updView.Satisfactionrequireid))
            {
                using (SatisfactionAppBll updBll = new SatisfactionAppBll())
                {
                    return updBll.UpdSatisfactionApp(updView);
                }

            }
            return false;
        }
        #endregion

        #region 查询

        /// <summary>
        /// 根据审核条件,创建日期获取满意度调查方案集合
        /// </summary>
        /// <param name="pageCount">页面当前数目</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页面项的数目</param>
        /// <param name="checkstate">审核状态</param>
        /// <param name="dateTimes">开始与结束时间</param>
        /// <returns>返回符合条件的方案</returns>
        [OperationContract]
        public List<V_Satisfactions> GetRequireByCheckstateAndDate(int pageCount, int pageIndex, int pageSize, string checkstate, DateTime[] dateTimes)
        {

            if (!string.IsNullOrEmpty(checkstate) && (dateTimes.Count() >= 2))
            {
                using (SatisfactionAppBll appBll = new SatisfactionAppBll())
                {
                    IQueryable<V_Satisfactions> dataList = appBll.GetRequireByCheckstateAndDate(pageCount, pageIndex, pageSize, checkstate, dateTimes);
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
        public List<V_Satisfactions> GetSatisfactionAppChild(string appId)
        {
            if (!string.IsNullOrEmpty(appId))
            {
                using (SatisfactionAppBll getAppBll = new SatisfactionAppBll())
                {
                    return null;
                }
            }
            return null;
        }

        #endregion

    }
}