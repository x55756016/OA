using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;
using SMT.SaaS.BLLCommonServices.PersonnelWS;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    public class ContractViewapplicationsBLL : BaseBll<T_OA_CONTRACTVIEW>
    {
        #region 新增查看申请
        /// <summary>
        /// 新增查看申请
        /// </summary>
        /// <param name="ContractPrintingInfo">申请查看实体</param>
        /// <returns></returns>
        public bool ContractViewapplicationsAdd(T_OA_CONTRACTVIEW ContractApplicationView)
        {
            try
            {
                T_OA_CONTRACTPRINT Printing = (from a in dal.GetObjects<T_OA_CONTRACTPRINT>()
                                               where a.CONTRACTPRINTID == ContractApplicationView.T_OA_CONTRACTPRINT.CONTRACTPRINTID
                                               select a).FirstOrDefault();

                ContractApplicationView.T_OA_CONTRACTPRINT = Printing;
                ContractApplicationView.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());

                bool i = Add(ContractApplicationView);
                if (i == true)
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
                Tracer.Debug("合同申请ContractViewapplicationsBLL-ContractViewapplicationsAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 修改查看申请(提交审核)
        /// <summary>
        /// 修改查看申请
        /// </summary>
        /// <param name="contraTemplate">查看申请实体</param>
        /// <returns></returns>
        public bool UpdateContractView(T_OA_CONTRACTVIEW contractView)
        {
            bool result = false;
            try
            {
                var users = from ent in dal.GetObjects<T_OA_CONTRACTVIEW>()
                            where ent.CONTRACTVIEWID == contractView.CONTRACTVIEWID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    user.CHECKSTATE = contractView.CHECKSTATE;//审批状态
                    user.TEL = contractView.TEL;//联系电话
                    user.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());//修改时间
                    user.UPDATEUSERID = contractView.UPDATEUSERID;//修改人ID
                    user.UPDATEUSERNAME = contractView.UPDATEUSERNAME;//修改人姓名

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
                Tracer.Debug("合同申请ContractViewapplicationsBLL-ContractViewapplicationsAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除合同查看申请信息
        /// <summary>
        /// 删除合同查看申请信息
        /// </summary>
        /// <param name="viewapplicationsID">合同查看申请ID</param>
        /// <returns></returns>
        public bool DeleteViewapplications(string[] viewapplicationsID)
        {
            try
            {
                ContractViewapplicationsDal cViewDal = new ContractViewapplicationsDal();
                var entitys = from ent in cViewDal.GetTable().ToList()
                              where viewapplicationsID.Contains(ent.CONTRACTVIEWID)
                              select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        Delete(obj);
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractViewapplicationsBLL-DeleteViewapplications" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        #endregion

        #region 根据查看申请ID查询

        public T_OA_CONTRACTVIEW GetContractViewById(string contractViewID)
        {
            var ents = from q in dal.GetObjects<T_OA_CONTRACTVIEW>().Include("T_OA_CONTRACTPRINT.T_OA_CONTRACTAPP")
                       where q.CONTRACTVIEWID == contractViewID
                       select q;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        #endregion

        #region 查询合同查看申请记录
        /// <summary>
        /// 查询合同查看申请记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="flowInfoList"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public List<V_ContractView> GetInquiryViewContractApplication(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_OA_CONTRACTVIEW>().Include("T_OA_CONTRACTPRINT")
                           join b in dal.GetObjects<T_OA_CONTRACTTYPE>() on a.T_OA_CONTRACTPRINT.T_OA_CONTRACTAPP.CONTRACTTYPEID equals b.CONTRACTTYPEID
                           select new V_ContractView
                           {
                               viewContract = a,
                               contractApp = new V_ContractApplications { contractApp = a.T_OA_CONTRACTPRINT.T_OA_CONTRACTAPP, contractType = b.CONTRACTTYPE },
                               OWNERCOMPANYID = a.OWNERCOMPANYID,
                               OWNERID = a.OWNERID,
                               OWNERPOSTID = a.OWNERPOSTID,
                               OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                               CREATEUSERID = a.CREATEUSERID
                           };

                if (ents.ToList().Count > 0)
                {
                    if (flowInfoList != null)
                    {
                        ents = (from a in ents.ToList().AsQueryable()
                                join l in flowInfoList on a.viewContract.CONTRACTVIEWID equals l.FormID
                                select new V_ContractView
                                {
                                    viewContract = a.viewContract,
                                    OWNERCOMPANYID = a.OWNERCOMPANYID,
                                    OWNERID = a.OWNERID,
                                    OWNERPOSTID = a.OWNERPOSTID,
                                    OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                    CREATEUSERID = a.CREATEUSERID
                                });
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_CONTRACTVIEW");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }

                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.viewContract.CHECKSTATE);
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<V_ContractView>(ents, pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractViewapplicationsBLL-GetInquiryViewContractApplication" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }
        #endregion
    }
}
