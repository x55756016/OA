/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-03-23

** 修改人：刘锦

** 修改时间：2010-07-26

** 描述：

**    主要用于福利发放的业务逻辑处理

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
    public class WelfareProvisionBLL : BaseBll<T_OA_WELFAREDISTRIBUTEMASTER>
    {
        #region 获取所有的福利发放信息
        /// <summary>
        /// 获取所有的福利发放信息
        /// </summary>
        /// <returns>返回结果</returns>
        public IQueryable<T_OA_WELFAREDISTRIBUTEMASTER> GetWelfareProvisionStandard()
        {
            WelfareProvision wp = new WelfareProvision();
            var entity = from p in wp.GetTable()
                         orderby p.CREATEDATE descending
                         select p;
            return entity.Count() > 0 ? entity : null;
        }
        #endregion

        #region 根据申请ID获取福利发放信息
        /// <summary>
        /// 根据发放ID获取福利发放信息
        /// </summary>
        /// <param name="WelfareID">申请ID</param>
        /// <returns>返回结果</returns>
        public T_OA_WELFAREDISTRIBUTEMASTER GetWelfareProvisionById(string WelfareProvisionID)
        {
            WelfareProvision wp = new WelfareProvision();
            var ents = from a in dal.GetObjects()
                       where a.WELFAREDISTRIBUTEMASTERID == WelfareProvisionID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;


        }
        /// <summary>
        /// 根据福利标准申请ID查询
        /// </summary>
        /// <param name="WelfareStandardID">福利发放ID</param>
        /// <returns></returns>
        public T_OA_WELFAREMASERT GetWelfareMasert(string welfareId)
        {
            var entity = from p in dal.GetObjects<T_OA_WELFAREMASERT>()
                         where p.WELFAREID == welfareId
                         select p;
            return entity.Count() > 0 ? entity.FirstOrDefault() : null;
        }
        #endregion

        #region 添加福利发放
        /// <summary>
        /// 新增福利发放
        /// </summary>
        /// <param name="Welfare">发放实体</param>
        /// <returns></returns>
        public bool WelfareProvisionAdd(T_OA_WELFAREDISTRIBUTEMASTER welfareProvision, List<T_OA_WELFAREDISTRIBUTEDETAIL> WelfareDetails)
        {
            try
            {
                Utility.RefreshEntity(welfareProvision);

                foreach (var detail in WelfareDetails)
                {
                    welfareProvision.T_OA_WELFAREDISTRIBUTEDETAIL.Add(detail);
                    Utility.RefreshEntity(detail);
                }
                welfareProvision.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                dal.AddToContext(welfareProvision);
                int welfareProvisionAdd = dal.SaveContextChanges();
                if (welfareProvisionAdd > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准WelfareProvisionBLL-WelfareProvisionAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
            return false;
        }
        #endregion

        #region 修改福利发放信息
        /// <summary>
        /// 修改福利发放信息
        /// </summary>
        /// <param name="Welfare">申请名称</param>
        /// <returns></returns>
        public bool UpdateWelfareProvision(T_OA_WELFAREDISTRIBUTEMASTER WelfareProvisionEtit)
        {
            bool reslut = false;
            try
            {
                var users = from ent in dal.GetObjects()
                            where ent.WELFAREDISTRIBUTEMASTERID == WelfareProvisionEtit.WELFAREDISTRIBUTEMASTERID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    user.CHECKSTATE = WelfareProvisionEtit.CHECKSTATE;//申请状态
                    user.DISTRIBUTEDATE = WelfareProvisionEtit.DISTRIBUTEDATE;//发放日期
                    user.ISWAGE = WelfareProvisionEtit.ISWAGE;//是否随薪发 0：非随工资发 1：随工资发
                    user.WELFAREDISTRIBUTETITLE = WelfareProvisionEtit.WELFAREDISTRIBUTETITLE;//福利发放名
                    user.CONTENT = WelfareProvisionEtit.CONTENT;//发放内容
                    user.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());//修改时间
                    user.UPDATEUSERID = WelfareProvisionEtit.UPDATEUSERID;//修改人ID
                    user.UPDATEUSERNAME = WelfareProvisionEtit.UPDATEUSERNAME;//修改人姓名
                    user.OWNERCOMPANYID = WelfareProvisionEtit.OWNERCOMPANYID;//所属公司
                    user.OWNERPOSTID = WelfareProvisionEtit.OWNERPOSTID;//所属岗位
                    int i = dal.Update(user);
                    if (i > 0)
                    {
                        reslut = true;
                    }
                }
                return reslut;
            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准WelfareProvisionBLL-UpdateWelfareProvision" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除福利发放信息
        /// <summary>
        /// 删除福利发放信息
        /// </summary>
        /// <param name="WelfareProvisionID">福利发放申请ID</param>
        /// <returns></returns>
        public bool DeleteWelfareProvision(string[] WelfareProvisionID)
        {
            try
            {
                var entity = from ent in dal.GetObjects().ToList()
                             where WelfareProvisionID.Contains(ent.WELFAREDISTRIBUTEMASTERID)
                             select ent;

                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        //删除T_OA_WELFAREDISTRIBUTEDETAIL
                        var ent = from p in dal.GetObjects<T_OA_WELFAREDISTRIBUTEDETAIL>()
                                  where h.WELFAREDISTRIBUTEMASTERID == p.T_OA_WELFAREDISTRIBUTEMASTER.WELFAREDISTRIBUTEMASTERID
                                  select p;
                        foreach (var k in ent)
                        {
                            dal.DeleteFromContext(k);

                        }
                        //删除T_OA_WELFAREDISTRIBUTEMASTER
                        //WelfareProvisionInfo.DeleteObject(h);
                        dal.DeleteFromContext(h);
                    }
                }
                return dal.SaveContextChanges() > 0 ? true : false;

            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准WelfareProvisionBLL-DeleteWelfareProvision" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
            return false;
        }
        #endregion

        #region 检查是否存在该福利发放信息
        /// <summary>
        /// 检查是否存在该福利发放信息
        /// </summary>
        /// <param name="WelfarePPoID">福利发放名</param>
        /// <param name="WelfareProvisionID">福利发放编号</param>
        /// <returns></returns>
        public bool IsExistWelfareProvision(string WelfareProvisionName, string WelfareProvisionID)
        {
            bool IsExist = false;
            var q = from cnt in dal.GetObjects()
                    where cnt.WELFAREDISTRIBUTETITLE == WelfareProvisionName && cnt.WELFAREDISTRIBUTEMASTERID == WelfareProvisionID
                    orderby cnt.CREATEUSERID
                    select cnt;
            if (q.Count() > 0)
            {
                IsExist = true;
            }
            return IsExist;
        }
        #endregion

        #region 根据福利发放名检查

        public List<string> GetWelfareProvisionRoomNameInfos()
        {
            WelfareProvision wp = new WelfareProvision();
            var query = from p in wp.GetTable()
                        orderby p.CREATEDATE descending
                        select p.WELFAREDISTRIBUTETITLE;

            return query.ToList<string>();
        }

        #endregion

        #region 获取福利发放信息

        /// <summary>
        /// 获取福利发放信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_WELFAREDISTRIBUTEMASTER> GetWelfareRooms()
        {
            WelfareProvision wp = new WelfareProvision();
            var query = from p in wp.GetTable()
                        orderby p.CREATEDATE descending
                        select p;
            return query.ToList<T_OA_WELFAREDISTRIBUTEMASTER>();

        }

        #endregion

        #region 根据备注、ID获取福利的发放信息

        /// <summary>
        /// 根据发放名、发放内容获取福利发放信息
        /// </summary>
        /// <param name="WelfareProvisionName">福利发放名</param>
        /// <param name="strContent">发放内容</param>
        /// <returns></returns>
        public List<V_WelfareProvision> GetWelfareProvisionRoomInfosListBySearch(string WelfareProvisionName, string strContent)
        {
            try
            {
                WelfareProvision wp = new WelfareProvision();

                var q = from p in dal.GetTable().ToList()
                        select new V_WelfareProvision { welfareProvision = p };

                if (!string.IsNullOrEmpty(WelfareProvisionName))
                {
                    q = q.Where(s => WelfareProvisionName.Contains(s.welfareProvision.WELFAREDISTRIBUTETITLE));
                }
                if (!string.IsNullOrEmpty(strContent))
                {
                    q = q.Where(s => s.welfareProvision.CONTENT.Contains(strContent));
                }

                if (q.Count() > 0)
                {
                    return q.ToList();
                }
                return null;


            }
            catch (Exception ex)
            {
                Tracer.Debug("福利标准WelfareProvisionBLL-GetWelfareProvisionRoomInfosListBySearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 获取用户申请记录
        /// <summary>
        /// 获取用户福利发放的申请记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="searchObj"></param>
        /// <param name="flowInfoList"></param>
        /// <param name="checkState">状态</param>
        /// <returns></returns>
        public List<V_WelfareProvision> GetWelfareProvisionInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {
                WelfareProvision wp = new WelfareProvision();
                var ents = from p in dal.GetObjects()
                           select new V_WelfareProvision
                           {
                               welfareProvision = p,
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
                                join l in flowInfoList on a.welfareProvision.WELFAREDISTRIBUTEMASTERID equals l.FormID
                                select new V_WelfareProvision
                                {
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
                        ents = ents.Where(s => checkState == s.welfareProvision.CHECKSTATE);
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_WELFAREDISTRIBUTEMASTER");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                    ents = ents.AsQueryable().OrderBy(sort);
                    ents = Utility.Pager<V_WelfareProvision>(ents.AsQueryable(), pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public IQueryable<V_WelfareProvision> GetWelfareProvisionInfoById(string lendingID)
        {
            WelfareProvision wp = new WelfareProvision();
            var ents = from a in dal.GetObjects()
                       join b in dal.GetObjects<T_OA_WELFAREMASERT>() on a.WELFAREDISTRIBUTEMASTERID equals b.WELFAREID
                       where b.WELFAREID == lendingID
                       select new V_WelfareProvision { welfareProvision = a };
            if (ents.Count() > 0)
            {
                return ents;
            }

            return null;
        }
        #endregion

        #region 获取用户申请记录
        /// <summary>
        /// 获取用户福利发放的申请记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="searchObj"></param>
        /// <param name="flowInfoList"></param>
        /// <param name="checkState">状态</param>
        /// <returns></returns>
        public List<V_WelfareProvision> GetWelfarePSelectRecord(int pageIndex, int pageSize, string sort, IList<object> paras, ref int pageCount, string checkState)
        {
            try
            {

                var ents = from p in dal.GetObjects()
                           select new V_WelfareProvision { welfareProvision = p };
                if (ents.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.welfareProvision.CHECKSTATE);
                    }
                    ents = ents.AsQueryable().OrderBy(sort);
                    ents = Utility.Pager<V_WelfareProvision>(ents.AsQueryable(), pageIndex, pageSize, ref pageCount);
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

        #region 该发放信息是否能被查看
        /// <summary>
        /// 查询该发放信息是否能被查看
        /// </summary>
        /// <param name="WelfareProvisionID">发放信息ID</param>
        /// <returns></returns>
        public bool IsCanBrowser(string WelfareProvisionID)
        {
            var entity = from q in dal.GetObjects()
                         where q.T_OA_WELFAREMASERT.WELFAREID == WelfareProvisionID
                         && q.CHECKSTATE == "1"
                         select q;
            if (entity.Count() > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 根据福利项目ID查询出所有的公司
        public List<T_OA_WELFAREMASERT> GetWelfareByWelfareproID(string welfareValue, string checkState)
        {
            var entity = (from q in dal.GetObjects<T_OA_WELFAREMASERT>()
                          where q.WELFAREPROID == welfareValue && q.CHECKSTATE == checkState
                          select q).ToList();
            return entity == null ? null : entity;
        }
        #endregion

        #region 根据公司Id查询福利标准信息
        public T_OA_WELFAREMASERT GetWelfareByCompanyId(string welfreRoId, string companyId, DateTime releaseTime, string checkState, string welfareId)
        {
            var entity = (from a in dal.GetObjects<T_OA_WELFAREMASERT>()
                          where a.COMPANYID == companyId && a.WELFAREPROID == welfreRoId && a.STARTDATE <= releaseTime
                          && (a.ENDDATE > releaseTime || !a.ENDDATE.HasValue) && a.CHECKSTATE == checkState && a.WELFAREID == welfareId
                          select a).FirstOrDefault();
            return entity == null ? null : entity;
        }
        #endregion
    }
}
