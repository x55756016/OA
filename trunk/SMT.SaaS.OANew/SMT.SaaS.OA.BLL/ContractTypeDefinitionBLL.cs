/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-22

** 修改人：刘锦

** 修改时间：2010-05-25

** 描述：

**    主要用于合同类型的业务逻辑处理

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
    public class ContractTypeDefinitionBLL : BaseBll<T_OA_CONTRACTTYPE>
    {
        #region 获取所有的类型信息
        /// <summary>
        /// //获取所有的类型信息
        /// </summary>
        /// <returns>返回结果</returns>
        public List<V_ContractType> GetContractType()
        {
            var query = from p in dal.GetTable().ToList()
                        select new V_ContractType { contractType = p };
            return query.ToList<V_ContractType>();
        }
        #endregion

        #region 根据类型ID获取类型信息
        /// <summary>
        /// 根据类型ID获取类型信息
        /// </summary>
        /// <param name="contractTypeID">类型ID</param>
        /// <returns>返回结果</returns>
        public T_OA_CONTRACTTYPE GetContractTypeById(string contractTypeID)
        {
            var entity = from p in dal.GetObjects<T_OA_CONTRACTTYPE>()
                         where p.CONTRACTTYPEID == contractTypeID
                         select p;
            return entity.Count() > 0 ? entity.FirstOrDefault() : null;
        }
        #endregion

        #region 新增合同类型定义
        /// <summary>
        /// 新增合同类型
        /// </summary>
        /// <param name="ContractType">合同类型</param>
        /// <returns></returns>
        public bool ContractTypeAdd(T_OA_CONTRACTTYPE ContractType)
        {
            try
            {
                ContractType.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                int add = dal.Add(ContractType);
                if (add > 0)
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
                Tracer.Debug("合同类型ContractTypeDefinitionBLL-ContractTypeAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 修改类型信息
        /// <summary>
        /// 修改类型信息
        /// </summary>
        /// <param name="contratype">合同类型</param>
        /// <returns></returns>
        public bool UpdateContraType(T_OA_CONTRACTTYPE contratype)
        {
            bool result = false;
            try
            {
                var users = from ent in dal.GetObjects<T_OA_CONTRACTTYPE>()
                            where ent.CONTRACTTYPEID == contratype.CONTRACTTYPEID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    contratype.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                    CloneEntity(contratype, user);
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
                Tracer.Debug("合同类型ContractTypeDefinitionBLL-UpdateContraType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除类型信息
        /// <summary>
        /// 删除类型信息
        /// </summary>
        /// <param name="contratypeID">合同类型ID</param>
        /// <returns></returns>
        public bool DeleteContraType(string[] contratypeID)
        {
            try
            {
                var entitys = from ent in dal.GetObjects<T_OA_CONTRACTTYPE>()
                              where contratypeID.Contains(ent.CONTRACTTYPEID)
                              select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        dal.DeleteFromContext(obj);
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同类型ContractTypeDefinitionBLL-DeleteContraType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        #endregion

        #region 检查是否存在该类型合同
        /// <summary>
        /// 检查是否存在该类型合同
        /// </summary>
        /// <param name="ContractType">类型名称</param>
        /// <param name="ContractTypeID">类型编号</param>
        /// <returns></returns>
        public bool IsExistContractType(string contractType, string contractLevel)
        {
            ContractManagementDAL cmd = new ContractManagementDAL();
            bool IsExist = false;
            var q = from cnt in cmd.GetTable()
                    where cnt.CONTRACTTYPE == contractType && cnt.CONTRACTLEVEL == contractLevel 
                    || cnt.CONTRACTTYPE == contractType
                    orderby cnt.CREATEDATE
                    select cnt;
            if (q.Count() > 0)
            {
                IsExist = true;
            }
            return IsExist;
        }
        #endregion

        public List<string> GetContractTypeRoomNameInfos()
        {
            var query = from p in dal.GetObjects<T_OA_CONTRACTTYPE>()
                        orderby p.CREATEDATE descending
                        select p.CONTRACTTYPE;

            return query.ToList<string>();

        }
        //获取所有的类型信息
        public List<T_OA_CONTRACTTYPE> GetContractTypeRooms()
        {
            ContractManagementDAL cmd = new ContractManagementDAL();
            var query = from p in cmd.GetTable()
                        orderby p.CREATEDATE descending
                        select p;
            return query.ToList<T_OA_CONTRACTTYPE>();

        }
        //获取所有的类型名
        public List<T_OA_CONTRACTTYPE> GetContractTypeNameInfos()
        {
            ContractManagementDAL cmd = new ContractManagementDAL();
            var query = from p in cmd.GetTable()
                        orderby p.CREATEDATE descending
                        select p;

            return query.ToList();

        }
        //获取查询的合同类型信息
        public List<V_ContractType> GetContractTypeRoomInfosListBySearch(string ContractTypName, string strID, string strContractLevel)
        {
            var q = from p in dal.GetTable()
                    select new V_ContractType { contractType = p };
            if (!string.IsNullOrEmpty(ContractTypName))
            {
                q = q.Where(s => ContractTypName.Contains(s.contractType.CONTRACTTYPE));
            }
            if (!string.IsNullOrEmpty(strID))
            {
                q = q.Where(s => strID.Contains(s.contractType.CONTRACTTYPEID));
            }
            if (!string.IsNullOrEmpty(strContractLevel))
            {
                q = q.Where(s => strContractLevel.Contains(s.contractType.CONTRACTLEVEL));
            }
            if (q.Count() > 0)
            {
                return q.ToList<V_ContractType>();
            }
            return null;
        }
        public List<T_OA_CONTRACTTYPE> GetContractTemplateNameInfosByContractType(string StrContractTemplateTypeName)
        {
            ContractManagementDAL cmd = new ContractManagementDAL();
            var query = from p in cmd.GetTable()
                        where p.CONTRACTTYPE == StrContractTemplateTypeName
                        orderby p.CREATEDATE descending
                        select p;
            return query.ToList<T_OA_CONTRACTTYPE>();
        }

        #region 获取所有的类型信息
        /// <summary>
        /// 获取所有的类型信息
        /// </summary>
        /// <returns>返回结果</returns>
        public List<T_OA_CONTRACTTYPE> GetInquiryContractType(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                var ents = from p in dal.GetObjects<T_OA_CONTRACTTYPE>()
                           select p;
                if (ents.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.Where(filterString, paras.ToArray());
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<T_OA_CONTRACTTYPE>(ents, pageIndex, pageSize, ref pageCount);
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
    }
    #endregion
}
