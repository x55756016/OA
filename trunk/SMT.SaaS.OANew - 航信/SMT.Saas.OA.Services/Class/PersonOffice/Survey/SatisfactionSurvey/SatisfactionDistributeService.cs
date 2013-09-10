using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using System.ServiceModel;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.DAL.Views;


namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        #region 增
        /// <summary>
        /// 新增满意度调查发布申请
        /// </summary>
        /// <param name="addDistributEntity"></param>
        /// <param name="addDistributeList"></param>
        /// <returns></returns>
        [OperationContract]
        public bool AddSatisfactionDistribute(V_Satisfactions addView)
        {
            if (!string.IsNullOrEmpty(addView.Satisfactionrequireid))
            {
                using (SatisfactionDistributeBll addBll = new SatisfactionDistributeBll())
                {
                    return addBll.AddSatisfactionDistribute(addView);
                }
            }
            return false;
        }
        #endregion

        #region 该
        /// <summary>
        /// 更新满意度调查发布申请
        /// </summary>
        [OperationContract]
        public bool UpdSatisfactionDistribute(V_Satisfactions updView)
        {
            if (!string.IsNullOrEmpty(updView.Satisfactionrequireid)&&!string.IsNullOrEmpty(updView.SatisfactiondistrbuteidDistrbuteid))
            {
                using (SatisfactionDistributeBll updbuteBll = new SatisfactionDistributeBll())
                {
                    return updbuteBll.UpdSatisfactionDistribute(updView);
                }
            }
            return false;
        }
        #endregion

        #region 查
        /// <summary>
        /// 发布申请子页面加载时取数据
        /// </summary>
        /// <param name="distributeId">发布申请表主键</param>
        /// <returns>对应的数据</returns>
        [OperationContract]
        public V_Satisfactions GetSatisfactionDistributeChild(string distributeId)
        {
            if (!string.IsNullOrEmpty(distributeId))
            {
                using (SatisfactionDistributeBll getDistributeBll = new SatisfactionDistributeBll())
                {
                   return  getDistributeBll.GetSatisfactionDistributeChild(distributeId);
                    
                }
            }
            return null;
        }
        #endregion
    }
}