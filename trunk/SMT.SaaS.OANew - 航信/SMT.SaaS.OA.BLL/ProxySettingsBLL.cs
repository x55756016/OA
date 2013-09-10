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
    public class ProxySettingsBLL : BaseBll<T_OA_AGENTSET>
    {

        #region 新增代理设置
        /// <summary>
        /// 新增代理设置
        /// </summary>
        /// <param name="agentsetInfo">代理设置实体</param>
        /// <returns></returns>
        public bool AgentSetAdd(T_OA_AGENTSET agentsetInfo)
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
                Tracer.Debug("代理岗位设置ProxySettingsBLL-AgentSetAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除代理设置信息
        /// <summary>
        /// 删除代理设置信息
        /// </summary>
        /// <param name="AgentSetId">代理ID</param>
        /// <returns></returns>
        public bool DeleteAgentSet(string[] AgentSetId)
        {
            try
            {
                AgentManagementDAL Amd = new AgentManagementDAL();
                var entitys = from ent in Amd.GetTable().ToList()
                              where AgentSetId.Contains(ent.AGENTSETID)
                              select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        dal.DeleteFromContext(obj);
                    }
                    int i = dal.SaveContextChanges();
                    return i > 0 ? true : false;                    
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("代理岗位设置ProxySettingsBLL-DeleteAgentSet" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        #endregion

        #region 修改代理设置信息
        /// <summary>
        /// 修改代理设置信息
        /// </summary>
        /// <param name="contraApproval">代理设置实体</param>
        /// <returns></returns>
        public bool UpdateAgentSet(T_OA_AGENTSET AgentSet)
        {
            bool result = false;
            try
            {
                AgentManagementDAL Amd = new AgentManagementDAL();
                var users = from ent in Amd.GetTable()
                            where ent.AGENTSETID == AgentSet.AGENTSETID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    AgentSet.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                    user.SYSCODE = AgentSet.SYSCODE;//系统代码
                    user.MODELCODE = AgentSet.MODELCODE;//代理模块
                    user.AGENTPOSTID = AgentSet.AGENTPOSTID;//代理岗位
                    user.UPDATEUSERID = AgentSet.UPDATEUSERID;//修改人ID
                    user.UPDATEUSERNAME = AgentSet.UPDATEUSERNAME;//修改人姓名

                    int asUpdate = Amd.Update(user);

                    if (asUpdate > 0)
                    {
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("代理岗位设置ProxySettingsBLL-UpdateAgentSet" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 获取代理设置信息
        /// <summary>
        /// 获取代理设置信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<T_OA_AGENTSET> GetAgentSetInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                AgentManagementDAL Amd = new AgentManagementDAL();
                var ents = from a in dal.GetObjects()
                            select a;
                if (ents.Count() > 0)
                {
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_AGENTSET");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<T_OA_AGENTSET>(ents, pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("代理岗位设置ProxySettingsBLL-GetAgentSetInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }
        #endregion

        #region 检查是否存在该代理设置信息
        /// <summary>
        /// 检查是否存在该代理设置信息
        /// </summary>
        /// <param name="sysCode">系统代码</param>
        /// <param name="modelCode">模块代码</param>
        /// <returns></returns>
        public bool IsExistAgentSet(string sysCode, string modelCode)
        {
            AgentManagementDAL Amd = new AgentManagementDAL();
            bool IsExist = false;
            var q = from cnt in Amd.GetTable()
                    where cnt.SYSCODE == sysCode && cnt.MODELCODE == modelCode
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
        public T_OA_AGENTSET GetAgentSetBysId(string AgentSetID)
        {
            
            var ents = from a in dal.GetObjects()
                        where a.AGENTSETID == AgentSetID
                        select a;
            if (ents.Count() > 0)
            {
                return ents.FirstOrDefault();
            }
            
            return null;
        }
        #endregion

        #region 查询代理(接口用)

        public bool IsExistAGENTForUser(string UserId)
        {
            try
            {
                AgentManagementDAL Amd = new AgentManagementDAL();
                DateTime Now = System.DateTime.Now;
                var entity = from ent in dal.GetObjects<T_OA_AGENTDATESET>()
                                where Now >= ent.EFFECTIVEDATE && Now <= ent.PLANEXPIRATIONDATE
                                && ent.EXPIRATIONDATE == null && ent.USERID == UserId
                                select ent;

                if (entity.Count() > 0)
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
                Tracer.Debug("代理岗位设置ProxySettingsBLL-IsExistAGENTForUser" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        public T_OA_AGENTSET GetQueryAgent(string UserId, string ModCode)
        {
            try
            {
                AgentManagementDAL Amd = new AgentManagementDAL();
                var ents = from a in dal.GetObjects()
                            where a.MODELCODE == ModCode && a.USERID == UserId
                            select a;

                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("代理岗位设置ProxySettingsBLL-GetQueryAgent" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }
        #endregion
    }
}
