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
    public class AgentAgingSetBLL : BaseBll<T_OA_AGENTDATESET>
    {
        public AgentAgingSet AAS = new AgentAgingSet();
        //private SMT_OA_EFModelContext AgentManagement = new SMT_OA_EFModelContext();


        #region 新增使用代理时效设置
        /// <summary>
        /// 新增使用代理时效设置
        /// </summary>
        /// <param name="agentsetInfo">使用代理时效设置实体</param>
        /// <returns></returns>
        public bool AgentDateSetAdd(T_OA_AGENTDATESET agentsetInfo)
        {
            try
            {
                agentsetInfo.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                int contract = dal.Add(agentsetInfo);
                if (contract == 1)
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
                Tracer.Debug("代理时效设置AgentAgingSetBLL-AgentDateSetAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除使用代理时效设置信息
        /// <summary>
        /// 删除使用代理时效设置信息
        /// </summary>
        /// <param name="AgentSetId">代理ID</param>
        /// <returns></returns>
        public bool DeleteAgentDateSet(string[] AgentSetId)
        {
            try
            {
                var entitys = from ent in AAS.GetTable().ToList()
                              where AgentSetId.Contains(ent.AGENTDATESETID)
                              select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        //AAS.Delete(obj);
                        dal.DeleteFromContext(obj);
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                        return true;
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("代理时效设置AgentAgingSetBLL-DeleteAgentDateSet" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        #endregion

        #region 修改使用代理时效设置信息
        /// <summary>
        /// 修改使用代理时效设置信息
        /// </summary>
        /// <param name="contraApproval">使用代理时效设置实体</param>
        /// <returns></returns>
        public bool UpdateAgentDateSet(T_OA_AGENTDATESET AgentDateSet)
        {
            bool result = false;
            try
            {
                var users = from ent in AAS.GetTable()
                            where ent.AGENTDATESETID == AgentDateSet.AGENTDATESETID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    AgentDateSet.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                    user.MODELCODE = AgentDateSet.MODELCODE;//代理模块
                    user.EFFECTIVEDATE = AgentDateSet.EFFECTIVEDATE;//生效日期
                    user.PLANEXPIRATIONDATE = AgentDateSet.PLANEXPIRATIONDATE;//计划失效日期
                    user.EXPIRATIONDATE = AgentDateSet.EXPIRATIONDATE;//失效日期
                    user.UPDATEUSERID = AgentDateSet.UPDATEUSERID;//修改人ID
                    user.UPDATEUSERNAME = AgentDateSet.UPDATEUSERNAME;//修改人姓名

                    int aasUpdate = AAS.Update(user);

                    if (aasUpdate > 0)
                    {
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("代理时效设置AgentAgingSetBLL-UpdateAgentDateSet" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 获取使用代理时效设置信息
        /// <summary>
        /// 获取使用代理时效设置信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<T_OA_AGENTDATESET> GetAgentDateSetInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_OA_AGENTDATESET>()
                           select a;
                if (ents.Count() > 0)
                {
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_AGENTDATESET");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<T_OA_AGENTDATESET>(ents, pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }
        #endregion

        #region 检查是否存在该使用代理时效设置信息
        /// <summary>
        /// 检查是否存在该使用代理时效设置信息
        /// </summary>
        /// <param name="modelCode">模块代码</param>
        /// <returns></returns>
        public bool IsExistAgentDateSet(string modelCode, string companyId)
        {
            bool IsExist = false;
            var q = from cnt in AAS.GetTable()
                    where cnt.MODELCODE == modelCode && cnt.OWNERCOMPANYID == companyId && cnt.EXPIRATIONDATE == null
                    orderby cnt.CREATEUSERID
                    select cnt;
            if (q.Count() > 0)
            {
                IsExist = true;
            }
            return IsExist;
        }
        #endregion

        #region 根据ID查询代理信息
        public T_OA_AGENTDATESET GetAgentDataSetBysId(string AgentDataSetID)
        {
            var ents = from a in dal.GetObjects<T_OA_AGENTDATESET>()
                       where a.AGENTDATESETID == AgentDataSetID
                       select a;
            if (ents.Count() > 0)
            {
                return ents.FirstOrDefault();
            }
            return null;
        }
        #endregion
    }
}
