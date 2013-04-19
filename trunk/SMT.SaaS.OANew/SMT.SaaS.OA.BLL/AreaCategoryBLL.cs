using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    public class AreaCategoryBLL : BaseBll<T_OA_AREADIFFERENCE>
    {
        #region 地区
        /// <summary>
        /// 增加地区
        /// </summary>
        /// <param name="obj"></param>
        public string AreaCategoryADD(T_OA_AREADIFFERENCE obj, string solutionsId, string companyId)
        {
            try
            {
                var ent = from a in dal.GetObjects().Include("T_OA_TRAVELSOLUTIONS")
                          where a.AREACATEGORY == obj.AREACATEGORY && a.OWNERCOMPANYID
                          == companyId && a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == solutionsId
                          select a;

                obj.CREATEDATE = DateTime.Now;
                Utility.RefreshEntity(obj);
                if (ent.Count() > 0)
                {
                    return "EXIST";
                }
                int i = dal.Add(obj);
                if (i > 0)
                {
                    return "SUCCESSED";
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("城市分类AreaCategoryBLL-AreaCategoryADD" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "ERROR";
            }
        }
        public void AreaCategoryUpdate(T_OA_AREADIFFERENCE obj)
        {
            try
            {
                var ent = from a in dal.GetObjects()
                          where a.AREADIFFERENCEID == obj.AREADIFFERENCEID
                          select a;
                if (ent.Count() > 0)
                {
                    T_OA_AREADIFFERENCE tmpEnt = ent.FirstOrDefault();

                    Utility.CloneEntity<T_OA_AREADIFFERENCE>(obj, tmpEnt);

                    dal.Update(tmpEnt);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("城市分类AreaCategoryBLL-AreaCategoryADD" + System.DateTime.Now.ToString() + " " + ex.ToString());

            }

        }
        /// <summary>
        /// 获取地区差异
        /// </summary>
        /// <returns>地区差异</returns>
        public List<T_OA_AREADIFFERENCE> GetAreaCategory()
        {
            try
            {
                var ents = from o in dal.GetObjects()
                           select o;
                return ents.Count() > 0 ? ents.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("城市分类AreaCategoryBLL-GetAreaCategory" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 根据ID 获取地区
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T_OA_AREADIFFERENCE GetAreaCategoryByID(string ID)
        {
            try
            {
                var ents = from o in dal.GetObjects()
                           where o.AREADIFFERENCEID == ID
                           select o;
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("城市分类AreaCategoryBLL-GetAreaCategoryByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
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
        public IQueryable<T_OA_AREADIFFERENCE> QueryAreaWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strcompanyid, string strSoluid)
        {
            try
            {
                ////先从方案设置中读取当前登录人所在公司对应的方案
                ////以后考虑兼职人的情况
                //var entsetsolu = from ent in dal.GetObjects<T_OA_PROGRAMAPPLICATIONS>().Include("T_OA_TRAVELSOLUTIONS")
                //                 where ent.OWNERCOMPANYID == strcompanyid
                //                 select ent;
                //if (entsetsolu.Count() > 0)
                //{
                //    if (string.IsNullOrEmpty(strSoluid))
                //    {
                //        strSoluid = entsetsolu.FirstOrDefault().T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID;
                //    }
                //}
                var ents = from ent in dal.GetObjects()
                           //where ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == strSoluid
                           select ent;

                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, paras.ToArray());
                }
                ents = ents.OrderBy(sort);

                ents = Utility.Pager<T_OA_AREADIFFERENCE>(ents, pageIndex, pageSize, ref pageCount);

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("城市分类AreaCategoryBLL-QueryAreaWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
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
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);

                IQueryable<T_OA_AREADIFFERENCE> ents = dal.GetObjects();

                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                ents = ents.OrderBy("AREAINDEX");

                ents = Utility.Pager<T_OA_AREADIFFERENCE>(ents, pageIndex, pageSize, ref pageCount);
                return ents.Count() > 0 ? ents.ToArray() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("城市分类AreaCategoryBLL-GetLookupData" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
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
                    var cityent = from a in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
                                  where a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == id
                                  select a;
                    if (cityent.Count() > 0)
                    {
                        foreach (T_OA_AREACITY city in cityent)
                        {
                            dal.DeleteFromContext(city);
                        }
                        citysign = dal.SaveContextChanges() >= 0 ? true : false;
                    }

                    if (citysign)
                    {
                        var allowanceent = from a in dal.GetObjects<T_OA_AREAALLOWANCE>().Include("T_OA_AREADIFFERENCE")
                                           where a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == id
                                           select a;
                        if (allowanceent.Count() > 0)
                        {
                            foreach (T_OA_AREAALLOWANCE allowance in allowanceent)
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
                            i += dal.Delete(ent);
                            //dal.DeleteFromContext(ent);
                        }
                    }
                }
                //int i= dal.SaveContextChanges();
                dal.CommitTransaction();
                return i;
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("城市分类AreaCategoryBLL-AreaCategoryDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw ex;
            }
        }
    }
}
