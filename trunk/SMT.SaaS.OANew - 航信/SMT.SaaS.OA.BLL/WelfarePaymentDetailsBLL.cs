/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-03-23

** 修改人：刘锦

** 修改时间：2010-06-03

** 描述：

**    主要用于福利发放明细的业务逻辑处理

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    public class WelfarePaymentDetailsBLL : BaseBll<T_OA_WELFAREDISTRIBUTEDETAIL>
    {
        #region 添加福利发放明细
        /// <summary>
        /// 新增福利发放明细
        /// </summary>
        /// <param name="Welfare">发放实体</param>
        /// <returns></returns>
        public bool WelfarePaymentDetailsAdd(T_OA_WELFAREDISTRIBUTEDETAIL WelfarePaymentDetails)
        {
            try
            {
                WelfarePaymentDetails wpd = new WelfarePaymentDetails();
                int Benefits = wpd.Add(WelfarePaymentDetails);
                if (Benefits == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利明细WelfarePaymentDetailsBLL-WelfarePaymentDetailsAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 修改福利发放明细信息
        /// <summary>
        /// 修改福利明细
        /// </summary>
        /// <param name="Welfare">申请名称</param>
        /// <returns></returns>
        public bool UpdateWelfarePaymentDetails(T_OA_WELFAREDISTRIBUTEDETAIL WelfarePaymentDetails)
        {
            bool result = false;
            try
            {
                var users = from ent in dal.GetObjects<T_OA_WELFAREDISTRIBUTEDETAIL>()
                            where ent.WELFAREDISTRIBUTEDETAILID == WelfarePaymentDetails.WELFAREDISTRIBUTEDETAILID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    user.STANDARD = WelfarePaymentDetails.STANDARD;//发放金额

                    int i = dal.Update(user);
                    if (i > 0)
                    {
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利明细WelfarePaymentDetailsBLL-UpdateWelfarePaymentDetails" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除福利发放明细
        /// <summary>
        /// 删除福利发放明细
        /// </summary>
        /// <param name="welfarePaymentDetailId">合同类型ID</param>
        /// <returns></returns>
        public bool DeleteWelfarePaymentDetail(string[] welfarePaymentDetailId)
        {
            try
            {
                foreach (string strId in welfarePaymentDetailId)
                {
                    var entitys = from ent in dal.GetObjects<T_OA_WELFAREDISTRIBUTEDETAIL>().Include("T_OA_WELFAREDISTRIBUTEMASTER")
                                  where ent.WELFAREDISTRIBUTEDETAILID == strId
                                  select ent;

                    T_OA_WELFAREDISTRIBUTEDETAIL entDel = entitys.FirstOrDefault();
                    dal.DeleteFromContext(entDel);

                }
                return dal.SaveContextChanges() > 0 ? true : false;

            }
            catch (Exception ex)
            {
                Tracer.Debug("福利明细WelfarePaymentDetailsBLL-DeleteWelfarePaymentDetail" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        #endregion

        #region 获取福利发放明细
        /// <summary>
        /// 获取福利发放明细
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<V_WelfareDetails> GetWelfarePaymentDetails(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList)
        {
            try
            {
                var ents = (from p in dal.GetObjects<T_OA_WELFAREDISTRIBUTEDETAIL>().Include("T_OA_WELFAREDISTRIBUTEMASTER").Include("T_OA_WELFAREMASERT")
                            where p.T_OA_WELFAREDISTRIBUTEMASTER.CHECKSTATE == "2"
                            select new V_WelfareDetails
                            {
                                welfareDetailsViews = p,
                                welfareGrantViews = p.T_OA_WELFAREDISTRIBUTEMASTER,
                                welfareViews = p.T_OA_WELFAREDISTRIBUTEMASTER.T_OA_WELFAREMASERT,
                            });


                if (ents.Count() > 0)
                {
                    if (flowInfoList != null)
                    {
                        ents = from a in ents.AsQueryable()
                               join l in flowInfoList on a.welfareDetailsViews.WELFAREDISTRIBUTEDETAILID equals l.FormID
                               select a;
                    }
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, paras.ToArray());
                    }
                    ents = ents.AsQueryable().OrderBy(sort);
                    ents = Utility.Pager<V_WelfareDetails>(ents.AsQueryable(), pageIndex, pageSize, ref pageCount);
                    List<V_WelfareDetails> listResult = ents.ToList();
                    listResult.ForEach(item =>
                    {
                        item.welfareDetailsViews.T_OA_WELFAREDISTRIBUTEMASTER = item.welfareGrantViews;
                    });
                    return listResult;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利明细WelfarePaymentDetailsBLL-GetWelfarePaymentDetails" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 根据发放ID获取福利明细信息
        /// <summary>
        /// 根据发放ID获取福利明细信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_WELFAREDISTRIBUTEDETAIL> GetByIdWelfarePaymentDetails(string distributeMasterId)
        {
            var query = from p in dal.GetObjects<T_OA_WELFAREDISTRIBUTEDETAIL>().Include("T_OA_WELFAREDISTRIBUTEMASTER")
                        where p.T_OA_WELFAREDISTRIBUTEMASTER.WELFAREDISTRIBUTEMASTERID == distributeMasterId
                        select p;
            return query.Count() > 0 ? query.ToList() : null;
        }
        #endregion
    }
}
