/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-22

** 修改人：刘锦

** 修改时间：2010-07-26

** 描述：

**    主要用于合同模板的业务逻辑处理

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;
using System.Linq.Dynamic;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{

    #region 合同管理
    public class ContractTemplateBLL : BaseBll<T_OA_CONTRACTTEMPLATE>
    {
        #region 获取所有的模板信息
        /// <summary>
        /// 获取所有的模板信息
        /// </summary>
        /// <returns>返回结果</returns>
        public List<V_ContractTemplate> GetContractTemplate(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string[][] flowInfoList, string userID)
        {
            try
            {
                string[] formIDList = null;
                var ents = (from a in dal.GetObjects<T_OA_CONTRACTTEMPLATE>()
                            join b in dal.GetObjects<T_OA_CONTRACTTYPE>() on a.T_OA_CONTRACTTYPE.CONTRACTTYPEID equals b.CONTRACTTYPEID
                            select new V_ContractTemplate
                            {
                                contractTemplate = a,
                                contractType = b.CONTRACTTYPE,
                                OWNERCOMPANYID = a.OWNERCOMPANYID,
                                OWNERID = a.OWNERID,
                                OWNERPOSTID = a.OWNERPOSTID,
                                OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                CREATEUSERID = a.CREATEUSERID
                            });
                if (ents.Count() > 0)
                {
                    if (flowInfoList != null)
                    {
                        ents = ents.ToList().Where(s => flowInfoList[0].Contains(s.contractTemplate.CONTRACTTEMPLATEID)).AsQueryable();
                        var entity = from q in ents
                                     select q.contractTemplate.CONTRACTTEMPLATEID;
                        if (entity.Count() > 0)
                        {
                            formIDList = new string[entity.Count()];
                            for (int i = 0; i < entity.Count(); i++)
                            {
                                formIDList[i] = entity.ToList()[i];
                            }
                        }
                        List<System.Collections.DictionaryEntry> tmpList = new List<System.Collections.DictionaryEntry>();

                        for (int j = 0; j < formIDList.Length; j++)
                        {
                            for (int i = 0; i < flowInfoList[0].Length; i++)
                            {
                                if (formIDList[j] == flowInfoList[0][i])
                                {
                                    System.Collections.DictionaryEntry item = new System.Collections.DictionaryEntry();
                                    item.Key = formIDList[j];
                                    item.Value = flowInfoList[0][i];
                                    tmpList.Add(item);
                                    break;
                                }
                            }
                        }
                        ents = (from a in ents
                                join l in tmpList on a.contractTemplate.CONTRACTTEMPLATEID equals l.Key
                                select new V_ContractTemplate
                                {
                                    contractTemplate = a.contractTemplate,
                                    contractType = a.contractType,
                                    OWNERCOMPANYID = a.OWNERCOMPANYID,
                                    OWNERID = a.OWNERID,
                                    OWNERPOSTID = a.OWNERPOSTID,
                                    OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                    CREATEUSERID = a.CREATEUSERID
                                });
                    }
                    //List<object> queryParas = new List<object>();
                    //queryParas.AddRange(paras);
                    //UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_CONTRACTTEMPLATE");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.AsQueryable().Where(filterString, paras.ToArray());
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<V_ContractTemplate>(ents, pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同模板ContractTemplateBLL-GetContractTemplate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 根据模板ID获取模板信息
        /// <summary>
        /// 根据模板ID获取模板信息
        /// </summary>
        /// <param name="contractTemplateID">模板ID</param>
        /// <returns>返回结果</returns>
        public T_OA_CONTRACTTEMPLATE GetContractTemplateById(string contractTemplateID)
        {
            ContractTemplate cm = new ContractTemplate();
            var entity = from p in cm.GetTable()
                         where p.CONTRACTTEMPLATEID == contractTemplateID
                         select p;
            return entity.Count() > 0 ? entity.FirstOrDefault() : null;
        }
        #endregion

        #region 新增合同模板
        /// <summary>
        /// 新增合同模板
        /// </summary>
        /// <param name="ContractTemplate">模板名称</param>
        /// <returns></returns>
        public bool ContractTemplateAdd(T_OA_CONTRACTTEMPLATE contractTemplate)
        {
            try
            {
                T_OA_CONTRACTTYPE Template = (from a in dal.GetObjects<T_OA_CONTRACTTYPE>()
                                              where a.CONTRACTTYPEID == contractTemplate.T_OA_CONTRACTTYPE.CONTRACTTYPEID
                                              select a).FirstOrDefault();

                contractTemplate.T_OA_CONTRACTTYPE = Template;
                contractTemplate.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());

                bool i = Add(contractTemplate);

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
                Tracer.Debug("合同模板ContractTemplateBLL-ContractTemplateAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 修改模板信息
        /// <summary>
        /// 修改模板信息
        /// </summary>
        /// <param name="contraTemplate">模板名称</param>
        /// <returns></returns>
        public bool UpdateContraTemplate(T_OA_CONTRACTTEMPLATE contraTemplate)
        {
            bool result = false;
            try
            {
                contraTemplate.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                T_OA_CONTRACTTEMPLATE tmpobj = dal.GetObjectByEntityKey(contraTemplate.EntityKey) as T_OA_CONTRACTTEMPLATE;
                tmpobj.T_OA_CONTRACTTYPE = dal.GetObjectByEntityKey(contraTemplate.T_OA_CONTRACTTYPE.EntityKey) as T_OA_CONTRACTTYPE;

                int i = dal.Update(contraTemplate);

                if (i > 0)
                {
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同模板ContractTemplateBLL-UpdateContraTemplate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除模板信息
        /// <summary>
        /// 删除模板信息
        /// </summary>
        /// <param name="contraTemplateID">合同模板ID</param>
        /// <returns></returns>
        public bool DeleteContraTemplate(string[] contraTemplateID)
        {
            try
            {
                ContractTemplate cm = new ContractTemplate();
                var entitys = from ent in cm.GetTable().ToList()
                              where contraTemplateID.Contains(ent.CONTRACTTEMPLATEID)
                              select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        Delete(obj);
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同模板ContractTemplateBLL-DeleteContraTemplate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        #endregion

        #region 检查是否存在该合同模板
        /// <summary>
        /// 检查是否存在该模板合同
        /// </summary>
        /// <param name="ContractTemplate">模板名称</param>
        /// <param name="ContractTemplateID">模板编号</param>
        /// <returns></returns>
        public bool IsExistContractTemplate(string templateName, string contractType, string contractLelve, string templateTitle)
        {
            ContractTemplate cm = new ContractTemplate();
            bool IsExist = false;
            var q = from cnt in cm.GetTable()
                    where cnt.CONTRACTTEMPLATENAME == templateName && cnt.CREATECOMPANYID == contractType
                    && cnt.CONTRACTLEVEL == contractLelve && cnt.CONTRACTTITLE == templateTitle || cnt.CONTRACTTITLE == templateTitle
                    orderby cnt.CREATEUSERID
                    select cnt;
            if (q.Count() > 0)
            {
                IsExist = true;
            }
            return IsExist;
        }
        #endregion

        #region GetTemplateRoomNameInfos
        public List<string> GetTemplateRoomNameInfos()
        {
            ContractTemplate cm = new ContractTemplate();
            var query = from p in cm.GetTable()
                        orderby p.CREATEDATE descending
                        select p.CONTRACTTEMPLATENAME;

            return query.ToList<string>();
        }
        #endregion

        #region 获取所有的模板信息
        /// <summary>
        /// 获取所有的模板信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_CONTRACTTEMPLATE> GetTemplateRooms()
        {
            ContractTemplate cm = new ContractTemplate();
            var query = from p in cm.GetTable()
                        orderby p.CREATEDATE descending
                        select p;
            return query.ToList<T_OA_CONTRACTTEMPLATE>();

        }
        #endregion

        #region 获取查询的模板信息
        /// <summary>
        /// 获取查询的模板信息
        /// </summary>
        /// <param name="ContractTemplateName"></param>
        /// <param name="strID"></param>
        /// <returns></returns>
        public List<V_ContractTemplate> GetTemplateRoomInfosListBySearch(string ContractTemplateName, string strTitle, string strContractLevel)
        {
            ContractTemplate cm = new ContractTemplate();
            var q = from p in cm.GetTable()
                    select new V_ContractTemplate { contractTemplate = p };
            if (!string.IsNullOrEmpty(ContractTemplateName))
            {
                q = q.Where(s => ContractTemplateName.Contains(s.contractTemplate.CONTRACTTEMPLATENAME));
            }
            if (!string.IsNullOrEmpty(strTitle))
            {
                q = q.Where(s => strTitle.Contains(s.contractTemplate.CONTRACTTITLE));
            }
            if (!string.IsNullOrEmpty(strContractLevel))
            {
                q = q.Where(s => strContractLevel.Contains(s.contractTemplate.CONTRACTLEVEL));
            }
            if (q.Count() > 0)
            {
                return q.ToList<V_ContractTemplate>();
            }
            return null;
        }
        #endregion

        #region 获取某一类型的模板名称
        /// <summary>
        /// 获取某一类型的模板名称
        /// </summary>
        /// <param name="StrContractTemplateType"></param>
        /// <returns></returns>
        public List<T_OA_CONTRACTTEMPLATE> GetContractTemplateInfosByContractType(string StrContractTemplateType)
        {
            ContractTemplate cm = new ContractTemplate();
            var query = from p in cm.GetTable()
                        where p.T_OA_CONTRACTTYPE.CONTRACTTYPEID == StrContractTemplateType
                        select p;
            return query.ToList<T_OA_CONTRACTTEMPLATE>();
        }
        #endregion
    }
    #endregion

}
