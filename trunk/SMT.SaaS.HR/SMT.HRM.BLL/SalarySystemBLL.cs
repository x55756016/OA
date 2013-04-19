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
using BLLCommonServices = SMT.SaaS.BLLCommonServices;

namespace SMT.HRM.BLL
{
    public class SalarySystemBLL : BaseBll<T_HR_SALARYSYSTEM>, ILookupEntity
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="obj"></param>
        public void SalarySystemAdd(T_HR_SALARYSYSTEM obj)
        {
            try
            {
                dal.Add(obj);
                BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYSYSTEM>(obj);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="obj"></param>
        public void SalarySystemUpdate(T_HR_SALARYSYSTEM obj)
        {
            try
            {
                var ent = from a in dal.GetTable()
                          where a.SALARYSYSTEMID == obj.SALARYSYSTEMID
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_SALARYSYSTEM tmpEnt = ent.FirstOrDefault();

                    Utility.CloneEntity<T_HR_SALARYSYSTEM>(obj, tmpEnt);
                    dal.Update(tmpEnt);
                   // BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYSYSTEM>(obj);
                }
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug(e.Message);
                throw e;
            }

        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int SalarySystemDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYSYSTEM>()
                           where e.SALARYSYSTEMID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    var tmps = from t in dal.GetObjects<T_HR_POSTLEVELDISTINCTION>().Include("T_HR_SALARYSYSTEM")
                               where t.T_HR_SALARYSYSTEM.SALARYSYSTEMID == ent.SALARYSYSTEMID
                               select t;
                    if (tmps.Count() > 0)
                    {
                        foreach (T_HR_POSTLEVELDISTINCTION tmp in tmps)
                        {
                            dal.DeleteFromContext(tmp);
                            BLLCommonServices.Utility.SubmitMyRecord<T_HR_SALARYSYSTEM>(tmp);
                            //DataContext.DeleteObject(tmp);
                        }
                    }
                    dal.DeleteFromContext(ent);
                   // DataContext.DeleteObject(ent);
                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges(); //DataContext.SaveChanges();
        }

        /// <summary>
        /// 根据ID获取
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T_HR_SALARYSYSTEM GetSalarySystemByID(string ID)
        {
            var ents = from a in dal.GetTable()
                       where a.SALARYSYSTEMID == ID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        ///// <summary>
        ///// 用于实体Grid中显示数据的分页查询
        ///// </summary>
        ///// <param name="pageIndex">当前页</param>
        ///// <param name="pageSize">每页显示条数</param>
        ///// <param name="sort">排序字段</param>
        ///// <param name="filterString">过滤条件</param>
        ///// <param name="paras">过滤条件中的参数值</param>
        ///// <param name="pageCount">返回总页数</param>
        ///// <returns>查询结果集</returns>
        //public new IQueryable<T_HR_PERFORMANCEREWARDSET> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        //{

        //    IQueryable<T_HR_PERFORMANCEREWARDSET> ents = dal.GetTable(); ;
        //    if (!string.IsNullOrEmpty(filterString))
        //    {
        //        ents = ents.Where(filterString, paras.ToArray());
        //    }
        //    ents = ents.OrderBy(sort);

        //    ents = Utility.Pager<T_HR_PERFORMANCEREWARDSET>(ents, pageIndex, pageSize, ref pageCount);

        //    return ents;
        //}
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
        public IQueryable<T_HR_SALARYSYSTEM> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYSYSTEM");
            SetFilterWithflow("SALARYSYSTEMID", "T_HR_SALARYSYSTEM", userID, ref CheckState, ref  filterString, ref queryParas);
            if (!string.IsNullOrEmpty(CheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " and ";
                }
                filterString += "CHECKSTATE == @" + queryParas.Count();
                queryParas.Add(CheckState);
            }
            IQueryable<T_HR_SALARYSYSTEM> ents = dal.GetObjects<T_HR_SALARYSYSTEM>();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYSYSTEM>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYSYSTEM");

            IQueryable<T_HR_SALARYSYSTEM> ents = dal.GetObjects<T_HR_SALARYSYSTEM>();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYSYSTEM>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToArray() : null;
        }
    }
}
