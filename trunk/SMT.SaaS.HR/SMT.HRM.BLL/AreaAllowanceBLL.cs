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
    public class AreaAllowanceBLL : BaseBll<T_HR_AREAALLOWANCE>
    {
        # region 补贴
        /// <summary>
        /// 增加地区差异补贴
        /// </summary>
        /// <param name="obj"></param>
        public void AreaAllowanceAdd(T_HR_AREAALLOWANCE obj)
        {
            try
            {
                var temp = from a in dal.GetTable()
                           where a.POSTLEVEL == obj.POSTLEVEL && a.T_HR_AREADIFFERENCE.AREADIFFERENCEID == obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID
                           select a;
                if (temp.Count() > 0)
                {
                    return;
                }
                T_HR_AREAALLOWANCE ent = new T_HR_AREAALLOWANCE();
                Utility.CloneEntity<T_HR_AREAALLOWANCE>(obj, ent);
                if (obj.T_HR_AREADIFFERENCE != null)
                {
                    ent.T_HR_AREADIFFERENCEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID);

                }
                dal.Add(ent);
            }catch(Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
        }
        /// <summary>
        ///地区差异补贴修改
        /// </summary>
        /// <param name="entity"></param>
        public void AreaAllowanceUpdate(T_HR_AREAALLOWANCE obj)
        {
            try
            {
                var ent = from a in dal.GetTable()
                          where a.AREAALLOWANCEID == obj.AREAALLOWANCEID
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_AREAALLOWANCE tmpEnt = ent.FirstOrDefault();

                    Utility.CloneEntity<T_HR_AREAALLOWANCE>(obj, tmpEnt);

                    if (obj.T_HR_AREADIFFERENCE != null)
                    {
                        tmpEnt.T_HR_AREADIFFERENCEReference.EntityKey =
                                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID);
                    }

                    dal.Update(tmpEnt);
                }
            }catch (Exception ex)
            {
               SMT.Foundation.Log.Tracer.Debug(ex.Message);
               throw ex;
            }

        }

        public void AreaAllowance(List<T_HR_AREAALLOWANCE> objs)
        {
            foreach (var ent in objs)
            {

                var tmpEnts = from p in dal.GetTable()
                              where p.AREAALLOWANCEID == ent.AREAALLOWANCEID
                              select p;
                if (tmpEnts != null && tmpEnts.Count() > 0)
                {
                    var tmpEnt = tmpEnts.FirstOrDefault();

                    Utility.CloneEntity<T_HR_AREAALLOWANCE>(ent, tmpEnt);
                    AreaAllowanceUpdate(tmpEnt);

                }
                else
                {
                    AreaAllowanceAdd(ent);
                }
            }
        }

        public int AreaAllowanceDelete(string[] IDs)
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

        /// <summary>
        /// 根据地区分类ID获取地区差异补贴
        /// </summary>
        /// <param name="ID">地区分类ID</param>
        /// <returns>地区差异补贴集</returns>
        public List<T_HR_AREAALLOWANCE> GetAreaAllowanceByAreaID(string ID)
        {
            var ents = from o in dal.GetObjects().Include("T_HR_AREADIFFERENCE")
                       where o.T_HR_AREADIFFERENCE.AREADIFFERENCEID == ID
                       select o;
            ents = ents.OrderBy("POSTLEVEL");
            return ents.Count() > 0 ? ents.ToList() : null;
        }

        /// <summary>
        /// 根据ID获取地区差异补贴
        /// </summary>
        /// <param name="ID"></param>
        /// <returns>地区差异补贴</returns>
        public T_HR_AREAALLOWANCE GetAreaAllowanceByID(string ID)
        {
            var ents = from o in dal.GetObjects().Include("T_HR_AREADIFFERENCE")
                       where o.AREAALLOWANCEID == ID
                       select o;
            ents = ents.OrderBy("POSTLEVEL");
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
        public new IQueryable<T_HR_AREAALLOWANCE> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {

            IQueryable<T_HR_AREAALLOWANCE> ents = dal.GetObjects().Include("T_HR_AREADIFFERENCE");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_AREAALLOWANCE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        #endregion


        #region 城市
        /// <summary>
        /// 增加地区城市
        /// </summary>
        /// <param name="obj"></param>
        public void AreaCityAdd(T_HR_AREACITY obj)
        {
            var temp = from a in dal.GetObjects<T_HR_AREACITY>()
                       where a.T_HR_AREADIFFERENCE.AREADIFFERENCEID == obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID && a.CITY == obj.CITY
                       select a;
            if (temp.Count() > 0)
            {
                return;
            }

            T_HR_AREACITY ent = new T_HR_AREACITY();
            Utility.CloneEntity<T_HR_AREACITY>(obj, ent);
            if (obj.T_HR_AREADIFFERENCE != null)
            {
                ent.T_HR_AREADIFFERENCEReference.EntityKey =
            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID);

            }
            dal.Add(ent);
        }
        /// <summary>
        /// 删除地区城市
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int AreaCityDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_AREACITY>()
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
        /// <summary>
        /// 根据地区分类ID获取城市
        /// </summary>
        /// <param name="CategoryID">地区分类</param>
        /// <returns></returns>
        public List<T_HR_AREACITY> GetAreaCityByCategory(string CategoryID)
        {
            var ents = from o in dal.GetObjects<T_HR_AREACITY>().Include("T_HR_AREADIFFERENCE")
                       where o.T_HR_AREADIFFERENCE.AREADIFFERENCEID == CategoryID
                       select o;
            return ents.Count() > 0 ? ents.ToList() : null;
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
        public IQueryable<T_HR_AREACITY> QueryCityWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {

            IQueryable<T_HR_AREACITY> ents = dal.GetObjects<T_HR_AREACITY>().Include("T_HR_AREADIFFERENCE");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_AREACITY>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        # endregion


        /// <summary>
        /// 核查城市是否已存在
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public Dictionary<string, string> AreaCityCheck(List<T_HR_AREACITY> objs)
        {
            Dictionary<string, string> resultmsg = new Dictionary<string, string>();
            foreach (T_HR_AREACITY obj in objs)
            {
                var ents = from a in dal.GetObjects<T_HR_AREACITY>().Include("T_HR_AREADIFFERENCE")
                           where a.CITY == obj.CITY
                           select new
                           {
                               CITY = a.CITY,
                               AREACATEGORY = a.T_HR_AREADIFFERENCE.AREACATEGORY
                           };
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    if(ent!=null)
                    {
                        resultmsg.Add(ent.CITY, ent.AREACATEGORY);
                    }
                }
            }
            return resultmsg;
        }

        /// <summary>
        /// 根据地区类型删除城市
        /// </summary>
        /// <param name="CategoryID">地区类型ID</param>
        /// <returns></returns>
        public bool AreaCityByCategoryDelete(string CategoryID)
        {
            bool citysign = true;
            var cityent = from a in dal.GetObjects<T_HR_AREACITY>().Include("T_HR_AREADIFFERENCE")
                          where a.T_HR_AREADIFFERENCE.AREADIFFERENCEID == CategoryID
                          select a;
            if (cityent.Count() > 0)
            {
                foreach (T_HR_AREACITY city in cityent)
                {
                    dal.DeleteFromContext(city);
                }
                citysign = dal.SaveContextChanges() >= 0 ? true : false;
            }
            return citysign;
        }

        /// <summary>
        /// 多个城市增加
        /// </summary>
        /// <param name="objs"></param>
        public string AreaCityLotsofAdd(List<T_HR_AREACITY> objs)
        {
            int i = 0;
            foreach (T_HR_AREACITY obj in objs)
            {
                var temp = from a in dal.GetObjects<T_HR_AREACITY>()
                           where a.T_HR_AREADIFFERENCE.AREADIFFERENCEID == obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID && a.CITY == obj.CITY
                           select a;
                if (temp.Count() > 0) break;

                T_HR_AREACITY ent = new T_HR_AREACITY();
                Utility.CloneEntity<T_HR_AREACITY>(obj, ent);
                if (obj.T_HR_AREADIFFERENCE != null)
                {
                    ent.T_HR_AREADIFFERENCEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_AREADIFFERENCE", "AREADIFFERENCEID", obj.T_HR_AREADIFFERENCE.AREADIFFERENCEID);

                }
                //dal.AddToContext(ent);
                i+=dal.Add(ent);
            }
            return i>0 ? "OK":"FAIL";
        }
    }
}
