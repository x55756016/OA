using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using SMT.HRM.CustomModel;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class AssessmentFormMasterBLL:BaseBll<T_HR_ASSESSMENTFORMMASTER>
    {
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的考核信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_ASSESSMENTFORMMASTER> AssessmentFormMasterPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_ASSESSMENTFORMMASTER");

            IQueryable<T_HR_ASSESSMENTFORMMASTER> ents = dal.GetObjects<T_HR_ASSESSMENTFORMMASTER>();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_ASSESSMENTFORMMASTER>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// 添加考核
        /// </summary>
        /// <param name="entity">人事考核主表</param>
        /// <param name="tmpList">人事考核明细表</param>
        public void AssessmentFormMasterAdd(T_HR_ASSESSMENTFORMMASTER entity,List<T_HR_ASSESSMENTFORMDETAIL> tmpList)
        {
            T_HR_ASSESSMENTFORMMASTER temp = new T_HR_ASSESSMENTFORMMASTER();
            Utility.CloneEntity<T_HR_ASSESSMENTFORMMASTER>(entity, temp);
            if (entity.T_HR_EMPLOYEECHECK != null)
            {
                temp.T_HR_EMPLOYEECHECKReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEECHECK", "BEREGULARID", entity.T_HR_EMPLOYEECHECK.BEREGULARID);
            }
            if (entity.T_HR_EMPLOYEEPOSTCHANGE != null)
            {
                temp.T_HR_EMPLOYEEPOSTCHANGEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEPOSTCHANGE", "POSTCHANGEID", entity.T_HR_EMPLOYEEPOSTCHANGE.POSTCHANGEID);
            }
            foreach (var ent in tmpList)
            {
                T_HR_ASSESSMENTFORMDETAIL sign = new T_HR_ASSESSMENTFORMDETAIL();                
                Utility.CloneEntity(ent, sign);
                if (ent.T_HR_CHECKPOINTSET != null)
                {
                    sign.T_HR_CHECKPOINTSETReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CHECKPOINTSET", "CHECKPOINTSETID", ent.T_HR_CHECKPOINTSET.CHECKPOINTSETID);
                }
                temp.T_HR_ASSESSMENTFORMDETAIL.Add(sign);
            }
            dal.AddToContext(temp);
            dal.SaveContextChanges();
            //DataContext.AddObject("T_HR_ASSESSMENTFORMMASTER", temp);
            //DataContext.SaveChanges();
        }
        /// <summary>
        /// 修改考核评分
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tmpList"></param>
        public void AssessmentFormMasterUpdate(T_HR_ASSESSMENTFORMMASTER entity, List<T_HR_ASSESSMENTFORMDETAIL> tmpList)
        {
            var temp = dal.GetObjects<T_HR_ASSESSMENTFORMMASTER>().FirstOrDefault(s => s.ASSESSMENTFORMMASTERID == entity.ASSESSMENTFORMMASTERID);
            if (temp != null)
            {
                Utility.CloneEntity<T_HR_ASSESSMENTFORMMASTER>(entity, temp);
                if (temp.T_HR_EMPLOYEECHECK != null)
                {
                    temp.T_HR_EMPLOYEECHECKReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEECHECK", "BEREGULARID", entity.T_HR_EMPLOYEECHECK.BEREGULARID);
                }
                if (temp.T_HR_EMPLOYEEPOSTCHANGE != null)
                {
                    temp.T_HR_EMPLOYEEPOSTCHANGEReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEPOSTCHANGE", "POSTCHANGEID", entity.T_HR_EMPLOYEEPOSTCHANGE.POSTCHANGEID);
                }
                foreach (var ent in tmpList)
                {
                    T_HR_ASSESSMENTFORMDETAIL sign = dal.GetObjects<T_HR_ASSESSMENTFORMDETAIL>().FirstOrDefault(
                        s => s.ASSESSMENTFORMDETAILID == ent.ASSESSMENTFORMDETAILID);
                    if (sign != null)
                    {
                        Utility.CloneEntity(ent, sign);
                        temp.T_HR_ASSESSMENTFORMDETAIL.Add(sign);
                    }
                }
                dal.SaveContextChanges();
            }
        }
        /// <summary>
        /// 根据对象ID查找考核信息
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        public T_HR_ASSESSMENTFORMMASTER GetAssessMentFormMasterByObjectID(string objectID)
        {
            var ent = dal.GetObjects<T_HR_ASSESSMENTFORMMASTER>().Include("T_HR_EMPLOYEECHECK").FirstOrDefault(s => s.T_HR_EMPLOYEECHECK.BEREGULARID == objectID);
            if (ent == null)
            {
                ent = dal.GetObjects<T_HR_ASSESSMENTFORMMASTER>().Include("T_HR_EMPLOYEEPOSTCHANGE").FirstOrDefault(s => s.T_HR_EMPLOYEEPOSTCHANGE.POSTCHANGEID == objectID);
            }
            return ent;
        }
        /// <summary>
        /// 根据考核表ID获取考核信息
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        public T_HR_ASSESSMENTFORMMASTER GetAssessMentFormMasterByID(string strID)
        {
            return dal.GetObjects<T_HR_ASSESSMENTFORMMASTER>().FirstOrDefault(s => s.ASSESSMENTFORMMASTERID == strID);
        }
    }
}
