using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT_OA_EFModel;
using System.ServiceModel;
using SMT.SaaS.OA.BLL;

namespace SMT.SaaS.OA.Services
{
    public class SatisfactionsService
    {
        #region 满意度调查方案
        #region 增
        /// <summary>
        /// 增加方案和子表
        /// </summary>
        /// <param name="AddEntity">主表带子表数据</param>
        /// <returns>返回是否添加成功</returns>
        [OperationContract]
        public bool AddSatisfactionMaster(T_OA_SATISFACTIONMASTER AddEntity)
        {
            bool flag = false;
            if (AddEntity != null)
            {
                using (SatisfactionSurveyBll bll = new SatisfactionSurveyBll())
                {
                    flag = bll.AddSatisfactionMaster(AddEntity);
                }
            }
            else
            {
                flag = false;
            }
            return flag;
        }

        #endregion

        //#region 删
        ///// <summary>
        ///// 使用事务批量删除方案和子表
        ///// </summary>
        ///// <param name="masterIdList">主表主键ID集合</param>
        ///// <returns>返回是否删除成功</returns>
        //[OperationContract]
        //public bool DelSatisfactionMaster(List<string> masterIdList)
        //{
        //    bool flag = false;
        //    if (masterIdList != null && masterIdList.Count() > 0)
        //    {
        //        using (SatisfactionSurveyBll bll = new SatisfactionSurveyBll())
        //        {
        //            flag = bll.DelSatisfactionMaster(masterIdList);
        //        }
        //    }
        //    else
        //    {
        //        flag = false;
        //    }
        //    return flag;
        //}
        //#endregion

        #region 改
        /// <summary>
        /// 修改方案和子表
        /// </summary>
        /// <param name="UpdEntity">主表带子表数据</param>
        /// <returns>是否修改成功</returns>
        [OperationContract]
        public bool UpdSatisfactionMaster(T_OA_SATISFACTIONMASTER UpdEntity)
        {
            bool flag = false;
            if (UpdEntity != null)
            {
                using (SatisfactionSurveyBll bll = new SatisfactionSurveyBll())
                {
                    flag = bll.UpdSatisfactionMaster(UpdEntity);
                }
            }
            else
            {
                flag = false;
            }
            return flag;

        }
        #endregion

        #region 查
        /// <summary>
        /// 子页面载入时查询数据
        /// </summary>
        /// <param name="primaryKey">主键ID</param>
        /// <returns>返回子页面需用数据</returns>
        [OperationContract]
        public IQueryable<T_OA_SATISFACTIONMASTER> GetMasterInfo(string primaryKey)
        {
            IQueryable<T_OA_SATISFACTIONMASTER> masterInfo = null;
            if (!string.IsNullOrEmpty(primaryKey))
            {
                using (SatisfactionSurveyBll bll = new SatisfactionSurveyBll())
                {
                    masterInfo = bll.GetMasterInfo(primaryKey);
                }
            }
            else
            {
                masterInfo = null;
            }
            return masterInfo = masterInfo != null && masterInfo.Count() > 0 ? masterInfo : null;
        }
        #endregion
        #endregion

    }
}