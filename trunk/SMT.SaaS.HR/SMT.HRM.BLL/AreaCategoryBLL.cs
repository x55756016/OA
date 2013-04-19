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

namespace SMT.HRM.BLL
{
    public class AreaCategoryBLL : BaseBll<T_HR_AREADIFFERENCE>, ILookupEntity
    {
        #region 地区
        /// <summary>
        /// 增加地区
        /// </summary>
        /// <param name="obj"></param>
        public string AreaCategoryADD(T_HR_AREADIFFERENCE obj)
        {
            try
            {
                var ent = from a in dal.GetObjects()
                          where a.AREACATEGORY == obj.AREACATEGORY
                          select a;
                if (ent.Count() > 0)
                {
                    return "EXIST";
                }
                dal.Add(obj);
                return "SUCCESSED";
            }catch(Exception ex)
            {
               SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
        }
        public void AreaCategoryUpdate(T_HR_AREADIFFERENCE obj)
        {
            try
            {
                var ent = from a in dal.GetObjects()
                          where a.AREADIFFERENCEID == obj.AREADIFFERENCEID
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_AREADIFFERENCE tmpEnt = ent.FirstOrDefault();

                    Utility.CloneEntity<T_HR_AREADIFFERENCE>(obj, tmpEnt);

                    dal.Update(tmpEnt);
                }
            }catch(Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }

        }
        /// <summary>
        /// 获取地区差异
        /// </summary>
        /// <returns>地区差异</returns>
        public List<T_HR_AREADIFFERENCE> GetAreaCategory()
        {
            var ents = from o in dal.GetObjects()
                       select o;
            return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 根据ID 获取地区
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T_HR_AREADIFFERENCE GetAreaCategoryByID(string ID)
        {
            var ents = from o in dal.GetObjects()
                       where o.AREADIFFERENCEID == ID
                       select o;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询(获取地区)
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_AREADIFFERENCE> QueryAreaWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {

            IQueryable<T_HR_AREADIFFERENCE> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_AREADIFFERENCE>(ents, pageIndex, pageSize, ref pageCount);

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

            IQueryable<T_HR_AREADIFFERENCE> ents = dal.GetObjects();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy("AREAINDEX");

            ents = Utility.Pager<T_HR_AREADIFFERENCE>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToArray() : null;
        }
        #endregion

        /// <summary>
        /// 删除地区分类,可同时删除多行记录
        /// </summary>
        /// <param name="AreaCategoryIDs">AreaCategoryIDs</param>
        /// <returns></returns>
        public int AreaCategoryDelete(string[] AreaCategoryIDs)
        {
            try
            {
                int i = -1;
                dal.BeginTransaction();
                foreach (string id in AreaCategoryIDs)
                {
                    bool citysign = true;
                    bool allowancesign = true;
                    var cityent = from a in dal.GetObjects<T_HR_AREACITY>().Include("T_HR_AREADIFFERENCE")
                                  where a.T_HR_AREADIFFERENCE.AREADIFFERENCEID == id
                                  select a;
                    if (cityent.Count() > 0)
                    {
                        foreach (T_HR_AREACITY city in cityent)
                        {
                            dal.DeleteFromContext(city);
                        }
                        citysign = dal.SaveContextChanges() >= 0 ? true : false;
                    }

                    if (citysign)
                    {
                        var allowanceent = from a in dal.GetObjects<T_HR_AREAALLOWANCE>().Include("T_HR_AREADIFFERENCE")
                                           where a.T_HR_AREADIFFERENCE.AREADIFFERENCEID == id
                                           select a;
                        if (allowanceent.Count() > 0)
                        {
                            foreach (T_HR_AREAALLOWANCE allowance in allowanceent)
                            {
                                dal.DeleteFromContext(allowance);
                            }
                            allowancesign = dal.SaveContextChanges() >= 0 ? true : false;
                        }
                    }

                    if (allowancesign)
                    {
                        var ents = from e in dal.GetObjects()
                                   where e.AREADIFFERENCEID == id
                                   select e;
                        var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                        if (ent != null)
                        {
                            i+=dal.Delete(ent);
                            //dal.DeleteFromContext(ent);
                        }
                    }
                }
                //int i= dal.SaveContextChanges();
                dal.CommitTransaction();
                return i;
             }catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                dal.RollbackTransaction();
                ex.Message.ToString();
                throw ex;
            }
        }
    }
}
