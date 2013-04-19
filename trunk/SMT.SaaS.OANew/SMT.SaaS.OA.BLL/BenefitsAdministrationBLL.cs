/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-23

** 修改人：刘锦

** 修改时间：2010-07-03

** 描述：

**    主要用于福利标准、标准明细的业务逻辑处理

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
    public class BenefitsAdministrationBLL : BaseBll<T_OA_WELFAREMASERT>
    {
        #region 获取所有的福利标准信息
        /// <summary>
        /// 获取所有的福利标准信息
        /// </summary>
        /// <returns>返回结果</returns>
        public IQueryable<T_OA_WELFAREMASERT> GetWelfareStandard()
        {
            BenefitsAdministrationDAL bad = new BenefitsAdministrationDAL();
            var entity = from p in bad.GetTable()
                         orderby p.CREATEDATE descending
                         select p;
            return entity.Count() > 0 ? entity : null;
        }
        #endregion

        #region 根据申请ID获取福利标准信息
        /// <summary>
        /// 根据标准ID获取福利标准信息
        /// </summary>
        /// <param name="WelfareID">申请ID</param>
        /// <returns>返回结果</returns>
        public T_OA_WELFAREMASERT GetWelfareById(string WelfareID)
        {
            var ents = from a in dal.GetObjects<T_OA_WELFAREMASERT>().Include("T_OA_WELFAREDETAIL")
                       join b in dal.GetObjects<T_OA_WELFAREDETAIL>() on a.WELFAREID equals b.T_OA_WELFAREMASERT.WELFAREID
                       where a.WELFAREID == WelfareID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 根据岗位&岗位级别获取福利标准
        /// </summary>
        /// <param name="PostId">岗位ID</param>
        /// <param name="PostLevel">岗位级别</param>
        /// <returns></returns>
        public List<T_OA_WELFAREMASERT> GetWelfareStandardById(string PostLevela, string PostLevelb, string PostId)
        {
            var entity = (from a in dal.GetObjects<T_OA_WELFAREMASERT>()
                          join b in dal.GetObjects<T_OA_WELFAREDETAIL>() on a.WELFAREID equals b.T_OA_WELFAREMASERT.WELFAREID
                          where b.POSTLEVELA == PostLevela && b.POSTLEVELB == PostLevelb || b.POSTID == PostId
                          select a);
            return entity.Count() > 0 ? entity.ToList() : null;
        }
        #endregion

        #region 添加福利标准
        /// <summary>
        /// 新增福利标准
        /// </summary>
        /// <param name="Welfare">标准实体</param>
        /// <returns></returns>
        public bool WelfareAdd(T_OA_WELFAREMASERT Welfare, List<T_OA_WELFAREDETAIL> WelfaredDetails)
        {
            try
            {
                Utility.RefreshEntity(Welfare);

                foreach (var detail in WelfaredDetails)
                {
                    Welfare.T_OA_WELFAREDETAIL.Add(detail);
                    Utility.RefreshEntity(detail);
                }
                Welfare.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                bool welfareAdd = Add(Welfare);
                if (welfareAdd == true)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准BenefitsAdministrationBLL-WelfareAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
            return false;
        }
        #endregion

        #region 查询标准明细
        public List<T_OA_WELFAREDETAIL> GetBenefitsAdministrationDetails(string welfreRoId, string companyId, string welfareId)
        {
            var entity = (from p in dal.GetObjects<T_OA_WELFAREDETAIL>()
                          join b in dal.GetObjects<T_OA_WELFAREMASERT>() on p.T_OA_WELFAREMASERT.WELFAREID equals b.WELFAREID
                          where p.T_OA_WELFAREMASERT.WELFAREPROID == welfreRoId && p.T_OA_WELFAREMASERT.COMPANYID == companyId && p.T_OA_WELFAREMASERT.WELFAREID == welfareId
                          && !p.T_OA_WELFAREMASERT.ENDDATE.HasValue
                          select p).ToList();
            return entity;
        }
        /// <summary>
        /// 查询标准明细
        /// </summary>
        /// <param name="welfreRoId">福利项目ID</param>
        /// <param name="companyId">公司ID</param>
        /// <param name="releaseTime">生效时间</param>
        /// <param name="checkState">审批状态</param>
        /// <returns></returns>
        public List<T_OA_WELFAREDETAIL> GetBenefitsDetailsAdministration(string welfreRoId, string companyId, DateTime releaseTime, string checkState)
        {
            var entity = (from p in dal.GetObjects<T_OA_WELFAREDETAIL>().Include("T_OA_WELFAREMASERT")
                          where p.T_OA_WELFAREMASERT.WELFAREPROID == welfreRoId && p.T_OA_WELFAREMASERT.COMPANYID == companyId
                          && p.T_OA_WELFAREMASERT.STARTDATE <= releaseTime && (p.T_OA_WELFAREMASERT.ENDDATE > releaseTime || !p.T_OA_WELFAREMASERT.ENDDATE.HasValue)
                          && p.T_OA_WELFAREMASERT.CHECKSTATE == checkState
                          select p).ToList();
            return entity;
        }
        /// <summary>
        /// 查询标准
        /// </summary>
        /// <param name="welfreRoId">福利项目ID</param>
        /// <param name="companyId">公司ID</param>
        /// <param name="chckState">审批状态</param>
        /// <param name="welfareId">福利标准ID</param>
        /// <returns></returns>
        public T_OA_WELFAREMASERT GetBenefitsAdministrationInfo(string welfreRoId, string companyId, string chckState, string welfareId)
        {
            var entity = (from a in dal.GetObjects<T_OA_WELFAREMASERT>()
                          join b in dal.GetObjects<T_OA_WELFAREDETAIL>() on a.WELFAREID equals b.T_OA_WELFAREMASERT.WELFAREID
                          where a.WELFAREPROID == welfreRoId && a.COMPANYID == companyId && a.CHECKSTATE == chckState && a.WELFAREID == welfareId
                          && !a.ENDDATE.HasValue
                          select a);
            return entity.FirstOrDefault();
        }
        #endregion

        #region 修改福利标准信息
        /// <summary>
        /// 修改福利标准信息
        /// </summary>
        /// <param name="Welfare">申请名称</param>
        /// <returns></returns>
        public bool UpdateWelfare(T_OA_WELFAREMASERT Welfare, List<T_OA_WELFAREDETAIL> WelfaredDetails)//标准主表
        {
            bool returnStr = true;
            dal.BeginTransaction();
            try
            {
                var entity = dal.GetObjects<T_OA_WELFAREMASERT>().Where(s => s.WELFAREID == Welfare.WELFAREID).FirstOrDefault();//福利标准主表

                if (entity != null)
                {
                    Welfare.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                    Utility.CloneEntity(Welfare, entity);
                    entity.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_WELFAREMASERT", "WELFAREID", entity.WELFAREID);
                    int i = Update(entity);
                    if (i > 0)
                    {
                        returnStr = true;
                    }
                    else
                    {
                        returnStr = false;
                    }
                }

                int iResult = dal.SaveContextChanges();

                if (iResult > 0)
                {
                    UpdateEndDateByCheckState(Welfare.WELFAREPROID, Welfare.COMPANYID, Welfare.WELFAREID, Welfare.CHECKSTATE, Convert.ToDateTime(Welfare.STARTDATE));
                    returnStr = true;
                }

                //先删除T_OA_WELFAREDETAIL
                var ent = dal.GetObjects<T_OA_WELFAREDETAIL>().Where(s => s.T_OA_WELFAREMASERT.WELFAREID == entity.WELFAREID);//福利标准子表

                if (ent != null)
                {
                    foreach (var deleteDetails in ent)
                    {
                        dal.DeleteFromContext(deleteDetails);
                    }
                    dal.SaveContextChanges();
                }
                //再插入T_OA_WELFAREDETAIL
                foreach (var updateDetails in WelfaredDetails)
                {
                    T_OA_WELFAREDETAIL welfareDetail = new T_OA_WELFAREDETAIL();
                    Utility.CloneEntity(updateDetails, welfareDetail);
                    welfareDetail.WELFAREDETAILID = Guid.NewGuid().ToString();
                    //entity.T_OA_WELFAREDETAIL.Add(welfareDetail);
                    welfareDetail.T_OA_WELFAREMASERTReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_WELFAREMASERT", "WELFAREID", entity.WELFAREID);
                    int iResulto = dal.Add(welfareDetail);
                    if (iResulto > 0)
                    {
                        returnStr = true;
                    }
                    else
                    {
                        returnStr = false;
                    }
                }
                dal.CommitTransaction();
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准BenefitsAdministrationBLL-UpdateWelfare" + System.DateTime.Now.ToString() + " " + ex.ToString());
                dal.RollbackTransaction();
                return false;
            }
            return returnStr;
        }
        public void UpdateEndDateByCheckState(string welfreRoId, string companyId, string welfareId, string chckState, DateTime releaseTime)
        {
            if (chckState == "2")
            {
                var ents = dal.GetObjects<T_OA_WELFAREMASERT>().Where(s => s.CHECKSTATE == "2" && s.ENDDATE == null && s.WELFAREPROID == welfreRoId && s.COMPANYID == companyId && s.WELFAREID != welfareId);

                foreach (T_OA_WELFAREMASERT item in ents)
                {
                    item.ENDDATE = releaseTime;
                }
            }
        }
        public bool UpdateWelfareDetails(T_OA_WELFAREDETAIL WelfareDetails)//福利标准子表(标准明细)
        {
            bool result = false;
            try
            {
                T_OA_WELFAREDETAIL tmpobj = dal.GetObjectByEntityKey(WelfareDetails.EntityKey) as T_OA_WELFAREDETAIL;
                tmpobj.T_OA_WELFAREMASERT = dal.GetObjectByEntityKey(WelfareDetails.T_OA_WELFAREMASERT.EntityKey) as T_OA_WELFAREMASERT;
                dal.Update(WelfareDetails);
                int i = dal.SaveContextChanges();

                if (i > 0)
                {
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准BenefitsAdministrationBLL-UpdateWelfareDetails" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除福利标准信息
        /// <summary>
        /// 删除福利标准信息
        /// </summary>
        /// <param name="WelfareID">福利标准申请ID</param>
        /// <returns></returns>
        public bool DeleteWelfare(string[] WelfareID)
        {
            try
            {
                var entity = from ent in dal.GetObjects<T_OA_WELFAREMASERT>().ToList()
                             where WelfareID.Contains(ent.WELFAREID)
                             select ent;

                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        //删除T_OA_WELFAREDETAIL
                        var ent = from p in dal.GetObjects<T_OA_WELFAREDETAIL>()
                                  where h.WELFAREID == p.T_OA_WELFAREMASERT.WELFAREID
                                  select p;
                        foreach (var k in ent)
                        {
                            dal.DeleteFromContext(k);
                        }
                        //删除T_OA_WELFAREMASERT
                        dal.DeleteFromContext(h);
                    }
                }
                int iResult = dal.SaveContextChanges();
                if (iResult > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准BenefitsAdministrationBLL-DeleteWelfare" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
            return false;
        }
        #endregion

        #region 检查是否存在该福利
        /// <summary>
        /// 检查是否存在该福利
        /// </summary>
        /// <param name="WelfareproId">福利项目ID</param>
        /// <param name="PostId">岗位ID</param>
        /// <param name="PostLevel">岗位级别</param>
        /// <param name="CompanyId">公司ID</param>
        /// <returns></returns>
        public bool IsExistWelfare(string WelfareproId, string CompanyId, List<T_OA_WELFAREDETAIL> WelfaredDetails)//检查主表
        {
            //SMT_OA_EFModelContext context = new SMT_OA_EFModelContext();
            bool IsExist = false;
            var ents = from a in dal.GetObjects<T_OA_WELFAREMASERT>()
                       where a.WELFAREPROID == WelfareproId && a.COMPANYID == CompanyId
                       select a;
            if (ents.Count() > 0)
            {
                return true;
            }
            //foreach (T_OA_WELFAREDETAIL DetailsWelfare in WelfaredDetails)
            //{
            //    if (IsExistWelfareDetails(DetailsWelfare))
            //    {
            //        return true;
            //    }
            //}
            return IsExist;
        }
        //private bool IsExistWelfareDetails(T_OA_WELFAREDETAIL DetailsWelfare)//检查子表
        //{
        //    SMT_OA_EFModelContext context = new SMT_OA_EFModelContext();
        //    bool IsExist = false;
        //    var ents = from a in context.T_OA_WELFAREDETAIL
        //               where ((a.ISLEVEL == DetailsWelfare.ISLEVEL && a.POSTLEVELA == DetailsWelfare.POSTLEVELA && a.POSTLEVELB == DetailsWelfare.POSTLEVELB) ||
        //               (a.ISLEVEL == DetailsWelfare.ISLEVEL && a.POSTID == DetailsWelfare.POSTID))
        //               select a;

        //    //var ents = from a in context.T_OA_WELFAREDETAIL
        //    //           where a.POSTID == DetailsWelfare.POSTID && a.POSTLEVELA == DetailsWelfare.POSTLEVELA && a.POSTLEVELB == DetailsWelfare.POSTLEVELB
        //    //           select a;
        //    if (ents.Count() > 0)
        //    {
        //        IsExist = true;
        //    }
        //    return IsExist;
        //}
        #endregion

        #region 根据福利备注检查

        public List<string> GetWelfareRoomNameInfos()
        {
            BenefitsAdministrationDAL bad = new BenefitsAdministrationDAL();
            var query = from p in bad.GetTable()
                        orderby p.CREATEDATE descending
                        select p.REMARK;

            return query.ToList<string>();
        }

        #endregion

        #region 获取福利标准信息

        /// <summary>
        /// 获取福利标准信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_WELFAREMASERT> GetWelfareRooms()
        {
            BenefitsAdministrationDAL bad = new BenefitsAdministrationDAL();
            var query = from p in bad.GetTable()
                        orderby p.CREATEDATE descending
                        select p;
            return query.ToList<T_OA_WELFAREMASERT>();

        }

        #endregion

        #region 根据备注、福利项目ID获取福利的标准信息

        /// <summary>
        /// 根据备注、ID获取福利标准信息
        /// </summary>
        /// <param name="WelfareRemark">备注</param>
        /// <param name="strID"></param>
        /// <returns></returns>
        public List<V_WelfareStandard> GetWelfareRoomInfosListBySearch(string WelfareRemark, string strID)
        {
            try
            {
                var q = from p in dal.GetTable().ToList()
                        select new V_WelfareStandard { welfareStandard = p };

                if (!string.IsNullOrEmpty(WelfareRemark))
                {
                    q = q.Where(s => WelfareRemark.Contains(s.welfareStandard.REMARK));
                }
                if (!string.IsNullOrEmpty(strID))
                {
                    q = q.Where(s => s.welfareStandard.WELFAREPROID.Contains(strID));
                }

                if (q.Count() > 0)
                {
                    return q.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准BenefitsAdministrationBLL-GetWelfareRoomInfosListBySearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 获取用户申请记录
        /// <summary>
        /// 获取用户申请记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<V_WelfareStandard> GetWelfareInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {
                var ents = from p in dal.GetObjects<T_OA_WELFAREMASERT>()
                           select new V_WelfareStandard
                           {
                               welfareStandard = p,
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
                                join l in flowInfoList on a.welfareStandard.WELFAREID equals l.FormID
                                select new V_WelfareStandard
                                {
                                    welfareStandard = a.welfareStandard,
                                    OWNERCOMPANYID = a.OWNERCOMPANYID,
                                    OWNERID = a.OWNERID,
                                    OWNERPOSTID = a.OWNERPOSTID,
                                    OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                    CREATEUSERID = a.CREATEUSERID
                                });

                    }
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.welfareStandard.CHECKSTATE);
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_WELFAREMASERT");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                    ents = ents.AsQueryable().OrderBy(sort);
                    ents = Utility.Pager<V_WelfareStandard>(ents.AsQueryable(), pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 该标准是否能被查看
        /// <summary>
        /// 查询该标准是否能被查看
        /// </summary>
        /// <param name="archivesID"></param>
        /// <returns></returns>
        public bool IsCanBrowser(string WelfareID)
        {
            var entity = from q in dal.GetObjects<T_OA_WELFAREMASERT>()
                         where q.WELFAREID == WelfareID
                         && q.CHECKSTATE == "1"
                         select q;
            if (entity.Count() > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 获取某一福利项目的所有信息
        /// <summary>
        /// 获取某一福利项目的所有信息
        /// </summary>
        /// <param name="StrContractTemplateType"></param>
        /// <returns></returns>
        public List<T_OA_WELFAREMASERT> GetWelfareByInformation()
        {
            var entity = from a in dal.GetObjects<T_OA_WELFAREMASERT>()
                         join b in dal.GetObjects<T_OA_WELFAREDETAIL>() on a.WELFAREID equals b.T_OA_WELFAREMASERT.WELFAREID
                         select a;
            return entity.Count() > 0 ? entity.ToList() : null;
        }
        #endregion

        #region 获取所有的岗位级别
        public List<T_OA_WELFAREDETAIL> GetWelfarePostId()
        {
            var query = from p in dal.GetObjects<T_OA_WELFAREDETAIL>()
                        orderby p.POSTLEVELA, p.POSTLEVELB descending
                        select p;
            return query.ToList();

        }
        #endregion
    }
}
