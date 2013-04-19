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
    public class SalaryArchiveItemBLL : BaseBll<T_HR_SALARYARCHIVEITEM>
    {

        /// <summary>
        /// 新增薪资项目
        /// </summary>
        /// <param name="obj"></param>
        public int SalaryArchiveItemAdd(T_HR_SALARYARCHIVEITEM obj)
        {
            try
            {
                T_HR_SALARYARCHIVEITEM ent = new T_HR_SALARYARCHIVEITEM();
                Utility.CloneEntity<T_HR_SALARYARCHIVEITEM>(obj, ent);
                if (obj.T_HR_SALARYARCHIVE != null)
                {
                    ent.T_HR_SALARYARCHIVEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYARCHIVE", "SALARYARCHIVEID", obj.T_HR_SALARYARCHIVE.SALARYARCHIVEID);

                }
                Utility.RefreshEntity(ent);
                return dal.Add(ent);
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug(e.Message);
                e.Message.ToString();
            }
            return 0;
        }
        /// <summary>
        /// 增加一个薪资档案的所有薪资项目
        /// </summary>
        /// <param name="objs"></param>
        //public void SalaryArchiveItemAdd(List<T_HR_SALARYARCHIVEITEM> objs)
        //{
        //    foreach(var obj in objs)
        //    {
        //        SalaryArchiveItemAdd(obj);
        //    }
        //    DataContext.SaveChanges();
        //}

        /// <summary>
        /// 更新薪资档案
        /// </summary>
        /// <param name="entity"></param>
        //public void SalaryArchiveItemUpdate(T_HR_SALARYARCHIVEITEM obj)
        //{
        //    try
        //    {
        //        var ent = from a in dal.GetTable()
        //                  where a.SALARYARCHIVEITEM == obj.SALARYARCHIVEITEM
        //                  select a;
        //        if (ent.Count() > 0)
        //        {
        //            T_HR_SALARYARCHIVEITEM tmpEnt = ent.FirstOrDefault();

        //            Utility.CloneEntity<T_HR_SALARYARCHIVEITEM>(obj, tmpEnt);

        //            if (obj.T_HR_SALARYARCHIVE != null)
        //            {
        //                tmpEnt.T_HR_SALARYARCHIVEReference.EntityKey =
        //                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYARCHIVE", "SALARYARCHIVEID", obj.T_HR_SALARYARCHIVE.SALARYARCHIVEID);
        //            }
        //            else
        //            {
        //                tmpEnt.T_HR_SALARYARCHIVEReference.EntityKey = null;
        //            }
        //            dal.Update(tmpEnt);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}

        /// <summary>
        /// 更新薪资档案
        /// </summary>
        /// <param name="entity"></param>
        public void SalaryArchiveItemUpdate(T_HR_SALARYARCHIVEITEM obj)
        {
            try
            {
                var ent = from a in dal.GetTable()
                          where a.SALARYARCHIVEITEM == obj.SALARYARCHIVEITEM
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_SALARYARCHIVEITEM tmpEnt = ent.FirstOrDefault();
                    tmpEnt.SUM = obj.SUM;
                    tmpEnt.REMARK = obj.REMARK;
                    dal.Update(tmpEnt);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }

        }
        /// <summary>
        /// 删除薪资档案项目
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int SalaryArchiveItemDelete(string[] IDs)
        {
            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYARCHIVEITEM>()
                           where e.SALARYARCHIVEITEM == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.Delete(ent);
                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges();
        }
        public int SalaryArchiveItemDeleteByArchiveID(string archiveID)
        {
            List<T_HR_SALARYARCHIVEITEM> items = new List<T_HR_SALARYARCHIVEITEM>();
            items = GetSalaryArchiveItemsByArchiveID(archiveID);
            if (items != null)
            {
                foreach (var item in items)
                {
                    dal.Delete(item);
                }
            }

            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 根据薪资档案ID获取薪资项目
        /// </summary>
        /// <param name="archiveID"></param>
        /// <returns></returns>
        public List<T_HR_SALARYARCHIVEITEM> GetSalaryArchiveItemsByArchiveID(string archiveID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVEITEM>().Include("T_HR_SALARYARCHIVE")
                       where o.T_HR_SALARYARCHIVE.SALARYARCHIVEID == archiveID
                       select o;

            return ents.Count() > 0 ? ents.ToList() : null;
        }

        /// <summary>
        /// 根据ID获取薪资项目
        /// </summary>
        /// <param name="archiveID"></param>
        /// <returns></returns>
        public V_SALARYARCHIVEITEM GetSalaryArchiveItemViewByID(string itemID)
        {
            var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVEITEM>()
                       join b in dal.GetObjects<T_HR_SALARYITEM>() on o.SALARYITEMID equals b.SALARYITEMID
                       where o.SALARYARCHIVEITEM == itemID
                       select new V_SALARYARCHIVEITEM
                       {
                           SALARYITEMNAME = b.SALARYITEMNAME,
                           SALARYARCHIVEID = o.T_HR_SALARYARCHIVE.SALARYARCHIVEID,
                           SALARYARCHIVEITEM = o.SALARYARCHIVEITEM,
                           SALARYSTANDARDID = o.SALARYSTANDARDID,
                           SALARYITEMID = b.SALARYITEMID,
                           SUM = o.SUM,
                           CALCULATEFORMULA = b.CALCULATEFORMULA,
                           CALCULATEFORMULACODE = b.CALCULATEFORMULACODE,
                           ORDERNUMBER = o.ORDERNUMBER,
                           REMARK = o.REMARK
                       };

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        //public T_HR_SALARYARCHIVEITEM GetSalaryArchiveItemsByArchiveID(string archiveID)
        //{
        //    var ents = from o in DataContext.T_HR_SALARYARCHIVEITEM.Include("T_HR_SALARYARCHIVE")
        //               where o.SALARYARCHIVEITEM == archiveID
        //               select o;

        //    return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        //}

        public List<V_SALARYARCHIVEITEM> GetSalaryArchiveItemsByArchiveIDs(List<string> archiveIDs)
        {
            List<V_SALARYARCHIVEITEM> items = new List<V_SALARYARCHIVEITEM>();
            foreach (var id in archiveIDs)
            {
                var ents = from o in dal.GetObjects<T_HR_SALARYARCHIVEITEM>().Include("T_HR_SALARYARCHIVE")
                           join b in dal.GetObjects<T_HR_SALARYITEM>() on o.SALARYITEMID equals b.SALARYITEMID
                           where o.T_HR_SALARYARCHIVE.SALARYARCHIVEID == id
                           select new V_SALARYARCHIVEITEM
                           {
                               SALARYITEMNAME = b.SALARYITEMNAME,
                               SALARYARCHIVEID = o.T_HR_SALARYARCHIVE.SALARYARCHIVEID,
                               SALARYARCHIVEITEM = o.SALARYARCHIVEITEM,
                               SALARYSTANDARDID = o.SALARYSTANDARDID,
                               SALARYITEMID = b.SALARYITEMID,
                               SUM = o.SUM,
                               CALCULATEFORMULA = b.CALCULATEFORMULA,
                               CALCULATEFORMULACODE = b.CALCULATEFORMULACODE,
                               ORDERNUMBER = o.ORDERNUMBER,
                               REMARK = o.REMARK
                           };
                items.AddRange(ents);
            }

            return items.Count() > 0 ? items.ToList() : null;
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
        //public new IQueryable<T_HR_SALARYARCHIVE> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        //{

        //    IQueryable<T_HR_SALARYARCHIVE> ents = DataContext.T_HR_SALARYARCHIVE.Include("T_HR_SALARYSTANDARD");
        //    if (!string.IsNullOrEmpty(filterString))
        //    {
        //        ents = ents.Where(filterString, paras.ToArray());
        //    }
        //    ents = ents.OrderBy(sort);

        //    ents = Utility.Pager<T_HR_SALARYARCHIVE>(ents, pageIndex, pageSize, ref pageCount);

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
        public IQueryable<V_SALARYARCHIVEITEM> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            //  SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYARCHIVEITEM");
            IQueryable<V_SALARYARCHIVEITEM> ents = from c in dal.GetObjects<T_HR_SALARYARCHIVEITEM>().Include("T_HR_SALARYARCHIVE")
                                                   join b in dal.GetObjects<T_HR_SALARYITEM>() on c.SALARYITEMID equals b.SALARYITEMID
                                                   select new V_SALARYARCHIVEITEM
                                                   {
                                                       SALARYITEMNAME = b.SALARYITEMNAME,
                                                       SALARYARCHIVEITEM = c.SALARYARCHIVEITEM,
                                                       SALARYARCHIVEID = c.T_HR_SALARYARCHIVE.SALARYARCHIVEID,
                                                       SALARYSTANDARDID = c.SALARYSTANDARDID,
                                                       SALARYITEMID = b.SALARYITEMID,
                                                       CALCULATEFORMULA = c.CALCULATEFORMULA,
                                                       SUM = c.SUM,
                                                       CALCULATEFORMULACODE = c.CALCULATEFORMULACODE,
                                                       REMARK = c.REMARK,
                                                       ORDERNUMBER = c.ORDERNUMBER
                                                   };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_SALARYARCHIVEITEM>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
    }
}
