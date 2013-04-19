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
    public class WelfarePaymentWithdrawalBLL : BaseBll<T_OA_WELFAREDISTRIBUTEUNDO>
    {
        #region 新增撤销申请
        /// <summary>
        /// 新增撤销申请
        /// </summary>
        /// <param name="WelfarePaymentWithdrawalView"></param>
        /// <returns></returns>
        public bool WelfarePaymentWithdrawalAdd(T_OA_WELFAREDISTRIBUTEUNDO WelfarePaymentWithdrawalView)
        {
            try
            {
                T_OA_WELFAREDISTRIBUTEMASTER Master = (from a in dal.GetObjects<T_OA_WELFAREDISTRIBUTEMASTER>()
                                                       where a.WELFAREDISTRIBUTEMASTERID == WelfarePaymentWithdrawalView.T_OA_WELFAREDISTRIBUTEMASTER.WELFAREDISTRIBUTEMASTERID
                                                       select a).FirstOrDefault();

                WelfarePaymentWithdrawalView.T_OA_WELFAREDISTRIBUTEMASTER = Master;
                WelfarePaymentWithdrawalView.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());

                bool WithdrawalAdd = Add(WelfarePaymentWithdrawalView);
                if (WithdrawalAdd == true)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准WelfarePaymentWithdrawalBLL-WelfarePaymentWithdrawalAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
            return false;
        }
        #endregion

        #region 修改发放撤销(提交审核)
        /// <summary>
        /// 修改发放撤销
        /// </summary>
        /// <param name="WelfarePaymentWithdrawal">发放撤销实体</param>
        /// <returns></returns>
        public bool UpdateWelfarePaymentWithdrawal(T_OA_WELFAREDISTRIBUTEUNDO WelfarePaymentWithdrawal)
        {
            bool result = false;
            try
            {
                var users = from ent in dal.GetObjects<T_OA_WELFAREDISTRIBUTEUNDO>()
                            where ent.WELFAREDISTRIBUTEUNDOID == WelfarePaymentWithdrawal.WELFAREDISTRIBUTEUNDOID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    user.CHECKSTATE = WelfarePaymentWithdrawal.CHECKSTATE;//审批状态
                    user.TEL = WelfarePaymentWithdrawal.TEL;//联系电话
                    user.REMARK = WelfarePaymentWithdrawal.REMARK;//备注
                    user.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());//修改时间
                    user.UPDATEUSERID = WelfarePaymentWithdrawal.UPDATEUSERID;//修改人ID
                    user.UPDATEUSERNAME = WelfarePaymentWithdrawal.UPDATEUSERNAME;//修改人姓名

                    int i = Update(user);
                    if (i > 0)
                    {
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准WelfarePaymentWithdrawalBLL-UpdateWelfarePaymentWithdrawal" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除发放撤销信息
        /// <summary>
        /// 删除发放撤销信息
        /// </summary>
        /// <param name="paymentWithdrawalID">发放撤销ID</param>
        /// <returns></returns>
        public bool DeletePaymentWithdrawal(string[] paymentWithdrawalID)
        {
            try
            {
                WelfarePaymentWithdrawal wpw = new WelfarePaymentWithdrawal();
                var entitys = from ent in wpw.GetTable().ToList()
                              where paymentWithdrawalID.Contains(ent.WELFAREDISTRIBUTEUNDOID)
                              select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        Delete(obj);
                    }
                    return dal.SaveContextChanges() > 0 ? true : false;

                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准WelfarePaymentWithdrawalBLL-DeletePaymentWithdrawal" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        #endregion

        #region 根据撤销ID查询

        public T_OA_WELFAREDISTRIBUTEUNDO GetWelfarePaymentWithdrawalById(string beingWithdrawnId)
        {
            WelfarePaymentWithdrawal wpw = new WelfarePaymentWithdrawal();
            var ents = from q in dal.GetObjects<T_OA_WELFAREDISTRIBUTEUNDO>().Include("T_OA_WELFAREDISTRIBUTEMASTER.T_OA_WELFAREDISTRIBUTEDETAIL")
                       where q.WELFAREDISTRIBUTEUNDOID == beingWithdrawnId
                       select q;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        #endregion

        #region 获取福利撤销申请记录
        /// <summary>
        /// 获取福利撤销申请记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="searchObj"></param>
        /// <param name="flowInfoList"></param>
        /// <param name="checkState">状态</param>
        /// <returns></returns>
        public List<V_WelfarePaymentWithdrawal> GetWelfarePaymentWithdrawal(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {
                var ents = from p in dal.GetObjects()
                           join b in dal.GetObjects<T_OA_WELFAREDISTRIBUTEMASTER>() on p.T_OA_WELFAREDISTRIBUTEMASTER.WELFAREDISTRIBUTEMASTERID equals b.WELFAREDISTRIBUTEMASTERID
                           select new V_WelfarePaymentWithdrawal
                           {
                               beingWithdrawn = p,
                               welfareProvision = b,
                               OWNERCOMPANYID = p.OWNERCOMPANYID,
                               OWNERID = p.OWNERID,
                               OWNERPOSTID = p.OWNERPOSTID,
                               OWNERDEPARTMENTID = p.OWNERDEPARTMENTID,
                               CREATEUSERID = p.CREATEUSERID
                           };
                if (ents.Count() > 0)
                {
                    if (flowInfoList != null)
                    {
                        ents = (from a in ents.ToList().AsQueryable()
                                join l in flowInfoList on a.beingWithdrawn.WELFAREDISTRIBUTEUNDOID equals l.FormID
                                select new V_WelfarePaymentWithdrawal
                                {
                                    beingWithdrawn = a.beingWithdrawn,
                                    welfareProvision = a.welfareProvision,
                                    OWNERCOMPANYID = a.OWNERCOMPANYID,
                                    OWNERID = a.OWNERID,
                                    OWNERPOSTID = a.OWNERPOSTID,
                                    OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                    CREATEUSERID = a.CREATEUSERID
                                });

                    }
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.beingWithdrawn.CHECKSTATE);
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_WELFAREDISTRIBUTEUNDO");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                    ents = ents.AsQueryable().OrderBy(sort);
                    ents = Utility.Pager<V_WelfarePaymentWithdrawal>(ents.AsQueryable(), pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }

                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准WelfarePaymentWithdrawalBLL-GetWelfarePaymentWithdrawal" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 检查是否存在重复撤销记录
        /// <summary>
        /// 检查是否存在重复撤销记录
        /// </summary>
        /// <param name="revocationId">撤销ID</param>
        /// <param name="grantId">发放ID</param>
        /// <returns></returns>
        public bool IsExistWelfarePaymentWithdrawal(string revocationId, string grantId)
        {
            bool IsExist = false;
            var detect = from cnt in dal.GetObjects<T_OA_WELFAREDISTRIBUTEUNDO>()
                         where cnt.WELFAREDISTRIBUTEUNDOID == revocationId && cnt.T_OA_WELFAREDISTRIBUTEMASTER.WELFAREDISTRIBUTEMASTERID == grantId && cnt.CHECKSTATE == "2"
                         orderby cnt.CREATEUSERID
                         select cnt;
            if (detect.Count() > 0)
            {
                IsExist = true;
            }
            return IsExist;
        }
        #endregion
    }
}
