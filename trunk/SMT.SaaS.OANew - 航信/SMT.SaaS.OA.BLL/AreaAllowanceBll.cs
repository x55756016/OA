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
    #region 地区津贴


    public class AreaAllowanceBll : BaseBll<T_OA_AREAALLOWANCE>
    {
        # region 补贴
        /// <summary>
        /// 增加地区差异补贴
        /// </summary>
        /// <param name="obj"></param>
        public void AreaAllowanceAdd(T_OA_AREAALLOWANCE obj, string travelSolutionsId)
        {
            try
            {
                var temp = from a in dal.GetTable()
                           where a.POSTLEVEL == obj.POSTLEVEL && a.T_OA_AREADIFFERENCE.AREADIFFERENCEID 
                           == obj.T_OA_AREADIFFERENCE.AREADIFFERENCEID && a.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == travelSolutionsId
                           select a;
                if (temp.Count() > 0)
                {
                    return;
                }
                T_OA_AREAALLOWANCE ent = new T_OA_AREAALLOWANCE();
                Utility.CloneEntity<T_OA_AREAALLOWANCE>(obj, ent);
                if (obj.T_OA_AREADIFFERENCE != null)
                {
                    ent.T_OA_AREADIFFERENCEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_OA_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_OA_AREADIFFERENCE.AREADIFFERENCEID);
                    ent.T_OA_TRAVELSOLUTIONSReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_OA_TRAVELSOLUTIONS", "TRAVELSOLUTIONSID", travelSolutionsId);

                }
                dal.Add(ent);
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-AreaAllowanceAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
            }
        }
        /// <summary>
        ///地区差异补贴修改
        /// </summary>
        /// <param name="entity"></param>
        public void AreaAllowanceUpdate(T_OA_AREAALLOWANCE obj)
        {
            try
            {
                var ent = from a in dal.GetTable()
                          where a.AREAALLOWANCEID == obj.AREAALLOWANCEID
                          select a;
                if (ent.Count() > 0)
                {
                    T_OA_AREAALLOWANCE tmpEnt = ent.FirstOrDefault();

                    Utility.CloneEntity<T_OA_AREAALLOWANCE>(obj, tmpEnt);

                    if (obj.T_OA_AREADIFFERENCE != null)
                    {
                        tmpEnt.T_OA_AREADIFFERENCEReference.EntityKey =
                                new System.Data.EntityKey(qualifiedEntitySetName + "T_OA_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_OA_AREADIFFERENCE.AREADIFFERENCEID);
                    }

                    dal.Update(tmpEnt);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-AreaAllowanceUpdate" + System.DateTime.Now.ToString() + " " + ex.ToString());
            }

        }

        public void AreaAllowance(List<T_OA_AREAALLOWANCE> objs, string travelSolutionsId)
        {
            try
            {
                foreach (var ent in objs)
                {

                    var tmpEnts = from p in dal.GetTable()
                                  where p.AREAALLOWANCEID == ent.AREAALLOWANCEID
                                  select p;
                    if (tmpEnts != null && tmpEnts.Count() > 0)
                    {
                        var tmpEnt = tmpEnts.FirstOrDefault();

                        Utility.CloneEntity<T_OA_AREAALLOWANCE>(ent, tmpEnt);
                        AreaAllowanceUpdate(tmpEnt);

                    }
                    else
                    {
                        AreaAllowanceAdd(ent, travelSolutionsId);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-AreaAllowance" + System.DateTime.Now.ToString() + " " + ex.ToString());
            }
        }

        public int AreaAllowanceDelete(string[] IDs)
        {
            try
            {
                foreach (string id in IDs)
                {
                    var ents = from e in dal.GetObjects()
                               where e.AREAALLOWANCEID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                    }

                    //TODO:删除项目所包含的明细
                }

                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-AreaAllowanceDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// 根据地区分类ID获取地区差异补贴
        /// </summary>
        /// <param name="ID">地区分类ID</param>
        /// <returns>地区差异补贴集</returns>
        public List<T_OA_AREAALLOWANCE> GetAreaAllowanceByAreaID(string ID, string SolutionID)
        {
            try
            {
                var ents = from o in dal.GetObjects().Include("T_OA_AREADIFFERENCE")
                           where o.T_OA_AREADIFFERENCE.AREADIFFERENCEID == ID
                           && o.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == SolutionID
                           select o;
                ents = ents.OrderBy("POSTLEVEL");
                return ents.Count() > 0 ? ents.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-GetAreaAllowanceByAreaID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据ID获取地区差异补贴
        /// </summary>
        /// <param name="ID"></param>
        /// <returns>地区差异补贴</returns>
        public T_OA_AREAALLOWANCE GetAreaAllowanceByID(string ID)
        {
            try
            {
                var ents = from o in dal.GetObjects().Include("T_OA_AREADIFFERENCE")
                           where o.AREAALLOWANCEID == ID
                           select o;
                ents = ents.OrderBy("POSTLEVEL");
                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-GetAreaAllowanceByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
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
        public new IQueryable<T_OA_AREAALLOWANCE> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            try
            {
                IQueryable<T_OA_AREAALLOWANCE> ents = dal.GetObjects().Include("T_OA_AREADIFFERENCE");
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, paras.ToArray());
                }
                ents = ents.OrderBy(sort);

                ents = Utility.Pager<T_OA_AREAALLOWANCE>(ents, pageIndex, pageSize, ref pageCount);

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-QueryWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }



        #endregion


        #region 城市
        /// <summary>
        /// 增加地区城市
        /// </summary>
        /// <param name="obj"></param>
        public void AreaCityAdd(T_OA_AREACITY obj)
        {
            try
            {
                var temp = from a in dal.GetObjects<T_OA_AREACITY>()
                           where a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == obj.T_OA_AREADIFFERENCE.AREADIFFERENCEID && a.CITY == obj.CITY
                           select a;
                if (temp.Count() > 0)
                {
                    return;
                }

                T_OA_AREACITY ent = new T_OA_AREACITY();
                Utility.CloneEntity<T_OA_AREACITY>(obj, ent);
                if (obj.T_OA_AREADIFFERENCE != null)
                {
                    ent.T_OA_AREADIFFERENCEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_OA_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_OA_AREADIFFERENCE.AREADIFFERENCEID);

                }
                dal.Add(ent);
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-AreaCityAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());

            }
        }
        /// <summary>
        /// 删除地区城市
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int AreaCityDelete(string[] IDs)
        {
            try
            {
                foreach (string id in IDs)
                {
                    var ents = from e in dal.GetObjects<T_OA_AREACITY>()
                               where e.AREACITYID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        //DataContext.DeleteObject(ent);
                        dal.DeleteFromContext(ent);
                    }

                    //TODO:删除项目所包含的明细
                }

                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-AreaCityDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return 0;
            }
        }
        /// <summary>
        /// 根据地区分类ID获取城市
        /// </summary>
        /// <param name="CategoryID">地区分类</param>
        /// <returns></returns>
        public List<T_OA_AREACITY> GetAreaCityByCategory(string CategoryID)
        {
            try
            {
                var ents = from o in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
                           where o.T_OA_AREADIFFERENCE.AREADIFFERENCEID == CategoryID
                           select o;
                return ents.Count() > 0 ? ents.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-GetAreaCityByCategory" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询(获取城市)
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_OA_AREACITY> QueryCityWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            try
            {
                IQueryable<T_OA_AREACITY> ents = dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE");
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, paras.ToArray());
                }
                ents = ents.OrderBy(sort);

                ents = Utility.Pager<T_OA_AREACITY>(ents, pageIndex, pageSize, ref pageCount);

                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-QueryCityWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        # endregion


        /// <summary>
        /// 核查城市是否已存在
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public Dictionary<string, string> AreaCityCheck(List<T_OA_AREACITY> objs, string areadifferenceid)
        {
            try
            {
                Dictionary<string, string> resultmsg = new Dictionary<string, string>();
                foreach (T_OA_AREACITY obj in objs)
                {
                    var ents = from a in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
                               where a.CITY == obj.CITY && a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == areadifferenceid
                               select new
                               {
                                   CITY = a.CITY,
                                   AREACATEGORY = a.T_OA_AREADIFFERENCE.AREACATEGORY
                               };
                    if (ents.Count() > 0)
                    {
                        var ent = ents.FirstOrDefault();
                        if (ent != null)
                        {
                            resultmsg.Add(ent.CITY, ent.AREACATEGORY);
                        }
                    }
                }
                return resultmsg;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-AreaCityCheck" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据地区类型删除城市
        /// </summary>
        /// <param name="CategoryID">地区类型ID</param>
        /// <returns></returns>
        public bool AreaCityByCategoryDelete(string CategoryID,string delCode)
        {
            bool Checker = true;
            //by luojie
            //添加delCode参数，如不为空则删除delCode里的城市
            if (!string.IsNullOrEmpty(delCode))
            {
                //delCode须是以‘，’隔离的
                string[] DelArray = delCode.Split(',');
                foreach (string cityCode in DelArray)
                {
                    var cityent = (from a in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
                                  where a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == CategoryID && a.CITY == cityCode
                                  select a).FirstOrDefault();

                    if (cityent != null)
                    {
                        dal.DeleteFromContext(cityent);
                        if (dal.SaveContextChanges() < 0) return false;
                    }
                }
            }
            //else
            //{
            //    var cityent = from a in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
            //                  where a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == CategoryID
            //                  select a;
            //    if (cityent.Count() > 0)
            //    {
            //        foreach (T_OA_AREACITY city in cityent)
            //        {
            //            dal.DeleteFromContext(city);
            //        }
            //        Checker = dal.SaveContextChanges() >= 0 ? true : false;
            //    }
            //}
            return Checker;

            //try
            //{
            //    bool citysign = true;
            //    var cityent = from a in dal.GetObjects<T_OA_AREACITY>().Include("T_OA_AREADIFFERENCE")
            //                  where a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == CategoryID
            //                  select a;
            //    if (cityent.Count() > 0)
            //    {
            //        foreach (T_OA_AREACITY city in cityent)
            //        {
            //            dal.DeleteFromContext(city);
            //        }
            //        citysign = dal.SaveContextChanges() >= 0 ? true : false;
            //    }
            //    return citysign;
            //}
            //catch (Exception ex)
            //{
            //    Tracer.Debug("出差补贴AreaAllowanceBll-AreaCityByCategoryDelete" + System.DateTime.Now.ToString() + " " + ex.ToString());
            //    return false;
            //}
        }

        /// <summary>
        /// 多个城市增加
        /// </summary>
        /// <param name="objs"></param>
        public string AreaCityLotsofAdd(List<T_OA_AREACITY> objs)
        {
            try
            {
                int i = 0;
                foreach (T_OA_AREACITY obj in objs)
                {
                    var temp = from a in dal.GetObjects<T_OA_AREACITY>()
                               where a.T_OA_AREADIFFERENCE.AREADIFFERENCEID == obj.T_OA_AREADIFFERENCE.AREADIFFERENCEID && a.CITY == obj.CITY
                               select a;
                    if (temp.Count() > 0) break;

                    T_OA_AREACITY ent = new T_OA_AREACITY();
                    Utility.CloneEntity<T_OA_AREACITY>(obj, ent);
                    if (obj.T_OA_AREADIFFERENCE != null)
                    {
                        ent.T_OA_AREADIFFERENCEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_OA_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_OA_AREADIFFERENCE.AREADIFFERENCEID);

                    }
                    //dal.AddToContext(ent);
                    i += dal.Add(ent);
                }
                return i > 0 ? "OK" : "FAIL";
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差补贴AreaAllowanceBll-AreaCityLotsofAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "FAIL";
            }
        }
    }
    #endregion


}
