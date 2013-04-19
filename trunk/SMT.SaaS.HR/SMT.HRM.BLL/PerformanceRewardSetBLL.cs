using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public class PerformanceRewardSetBLL:BaseBll<T_HR_PERFORMANCEREWARDSET>
    {
        /// <summary>
        /// 添加绩效奖金设置
        /// </summary>
        /// <param name="obj"> 绩效奖金设置实例</param>
        public void PerformanceRewardSetAdd(T_HR_PERFORMANCEREWARDSET obj)
        {
            try
            {
                dal.Add(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 更新绩效奖金设置
        /// </summary>
        /// <param name="entity">绩效奖金设置实例</param>
        public void PerformanceRewardSetUpdate(T_HR_PERFORMANCEREWARDSET obj)
        {
            try
            {
                var ent = from a in dal.GetTable()
                          where a.PERFORMANCEREWARDSETID == obj.PERFORMANCEREWARDSETID
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_PERFORMANCEREWARDSET tmpEnt = ent.FirstOrDefault();

                    Utility.CloneEntity<T_HR_PERFORMANCEREWARDSET>(obj, tmpEnt);
                    dal.Update(tmpEnt);

                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        /// <summary>
        /// 删除绩效奖金设置
        /// </summary>
        /// <param name="IDs">绩效奖金设置ID</param>
        /// <returns>是否删除标志</returns>
        public int PerformanceRewardSetDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_PERFORMANCEREWARDSET>()
                           where e.PERFORMANCEREWARDSETID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                   // DataContext.DeleteObject(ent);
                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges();
        }
 
        /// <summary>
        /// 根据ID获取绩效奖金设置
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T_HR_PERFORMANCEREWARDSET PerformanceRewardSetByID(string ID)
        {
            var ents = from a in dal.GetTable()
                       where a.PERFORMANCEREWARDSETID == ID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public new IQueryable<T_HR_PERFORMANCEREWARDSET> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {

            IQueryable<T_HR_PERFORMANCEREWARDSET> ents = dal.GetTable(); ;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_PERFORMANCEREWARDSET>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// 带有权限的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页 </param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件的参数 </param>
        /// <param name="pageCount">总页数</param>
        /// <param name="userID">用户ID</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_PERFORMANCEREWARDSET> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID,string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
       
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_PERFORMANCEREWARDSET");
            SetFilterWithflow("PERFORMANCEREWARDSETID", "T_HR_PERFORMANCEREWARDSET", userID, ref CheckState, ref  filterString, ref queryParas);
            if (!string.IsNullOrEmpty(CheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " and ";
                }
                filterString += "CHECKSTATE==@" + queryParas.Count();
                queryParas.Add(CheckState);
            }
            IQueryable<T_HR_PERFORMANCEREWARDSET> ents = dal.GetObjects<T_HR_PERFORMANCEREWARDSET>();
           
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_PERFORMANCEREWARDSET>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

    }
}
