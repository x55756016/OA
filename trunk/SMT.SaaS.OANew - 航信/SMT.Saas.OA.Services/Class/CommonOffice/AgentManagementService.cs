using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.SaaS.OA.BLL;
using System.ServiceModel;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOACommonOffice
    {

        #region 代理设置
        [OperationContract]//添加代理设置信息
        public string AgentSetAdd(T_OA_AGENTSET AgentSet)
        {
            using (ProxySettingsBLL psbll = new ProxySettingsBLL())
            {
                string returnStr = "";
                if (!psbll.IsExistAgentSet(AgentSet.SYSCODE, AgentSet.MODELCODE))
                {
                    if (!psbll.AgentSetAdd(AgentSet))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                else
                {
                    returnStr = "该代理设置已经存在,添加数据失败";
                }
                return returnStr;
            }
        }

        [OperationContract]//删除代理设置
        public bool DeleteAgentSet(string[] AgentSetId)
        {
            using (ProxySettingsBLL psbll = new ProxySettingsBLL())
            {
                return psbll.DeleteAgentSet(AgentSetId);
            }
        }

        [OperationContract]//更新代理设置
        public string UpdateAgentSet(T_OA_AGENTSET AgentSet)
        {
            using (ProxySettingsBLL psbll = new ProxySettingsBLL())
            {
                string returnStr = "";
                if (!psbll.UpdateAgentSet(AgentSet))
                {
                    returnStr = "更新数据失败！";
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 获取已设置的代理
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_AGENTSET> GetAgentSetListById(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (ProxySettingsBLL psbll = new ProxySettingsBLL())
            {
                List<T_OA_AGENTSET> ArchivesList = psbll.GetAgentSetInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

                return ArchivesList != null ? ArchivesList.ToList() : null;
            }
        }

        [OperationContract]
        public T_OA_AGENTSET GetAgentSetBysId(string AgentSetId)
        {
            using (ProxySettingsBLL psbll = new ProxySettingsBLL())
            {
                T_OA_AGENTSET AgentSetById = psbll.GetAgentSetBysId(AgentSetId);
                return AgentSetById == null ? null : AgentSetById;
            }
        }
        #endregion

        #region 使用代理时效设置
        [OperationContract]//添加代理设置信息
        public string AgentDataSetAdd(T_OA_AGENTDATESET AgentDataSet)
        {
            using (AgentAgingSetBLL aasbll = new AgentAgingSetBLL())
            {
                string returnStr = "";
                if (!aasbll.IsExistAgentDateSet(AgentDataSet.MODELCODE, AgentDataSet.OWNERCOMPANYID))
                {
                    if (!aasbll.AgentDateSetAdd(AgentDataSet))
                    {
                        returnStr = "添加数据失败";
                    }
                }
                else
                {
                    returnStr = "该数据已经存在,添加失败";
                }
                return returnStr;
            }
        }
        /// <summary>
        /// 检查是否存在代理
        /// </summary>
        /// <param name="modelCode"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [OperationContract]
        private bool IsExistAgent(string modelCode, string companyId)
        {
            using (AgentAgingSetBLL aasbll = new AgentAgingSetBLL())
            {
                return aasbll.IsExistAgentDateSet(modelCode, companyId);
            }
        }

        [OperationContract]//删除代理设置
        public bool DeleteAgentDataSet(string[] AgentDataSetId)
        {
            using (AgentAgingSetBLL aasbll = new AgentAgingSetBLL())
            {
                return aasbll.DeleteAgentDateSet(AgentDataSetId);
            }
        }

        [OperationContract]//更新代理设置
        public string UpdateAgentDataSet(T_OA_AGENTDATESET AgentSet)
        {
            using (AgentAgingSetBLL aasbll = new AgentAgingSetBLL())
            {
                string returnStr = "";
                if (!aasbll.UpdateAgentDateSet(AgentSet))
                {
                    returnStr = "更新数据失败！";
                }
                return returnStr;
            }
        }

        /// <summary>
        /// 获取已设置的代理
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_AGENTDATESET> GetAgentDataSetListById(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {
            using (AgentAgingSetBLL aasbll = new AgentAgingSetBLL())
            {
                List<T_OA_AGENTDATESET> AgentSetList = aasbll.GetAgentDateSetInfo(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);

                return AgentSetList != null ? AgentSetList.ToList() : null;
            }
        }

        [OperationContract]
        public T_OA_AGENTDATESET GetAgentDataSetBysId(string AgentDataSetId)
        {
            using (AgentAgingSetBLL aasbll = new AgentAgingSetBLL())
            {
                T_OA_AGENTDATESET AgentDataSetById = aasbll.GetAgentDataSetBysId(AgentDataSetId);
                return AgentDataSetById == null ? null : AgentDataSetById;
            }
        }
        #endregion
    }
}