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
    public class SalaryStandardItemBLL : BaseBll<T_HR_SALARYSTANDARDITEM>
    {
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
        public IQueryable<V_SALARYSTANDARDITEM> GetSalaryStandardItemPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckState, string userid)
        {
            List<object> queryParas = new List<object>();

            queryParas.AddRange(paras);

            //SetOrganizationFilter(ref filterString, ref queryParas, userid, "T_HR_SALARYSTANDARDITEM");

            //SetFilterWithflow("SALARYITEMID", "T_HR_SALARYSTANDARDITEM", userid, ref strCheckState, ref filterString, ref queryParas);

            //if (!string.IsNullOrEmpty(strCheckState))
            //{
            //    if (!string.IsNullOrEmpty(filterString))
            //    {
            //        filterString += " AND";
            //    }

            //    filterString += " CHECKSTATE == @" + queryParas.Count();
            //    queryParas.Add(strCheckState);
            //}


            IQueryable<V_SALARYSTANDARDITEM> ents = from a in dal.GetObjects<T_HR_SALARYSTANDARDITEM>().Include("T_HR_SALARYITEM")
                                                    select new V_SALARYSTANDARDITEM
                                                    {
                                                        STANDRECORDITEMID = a.STANDRECORDITEMID,
                                                        SALARYSTANDARDID = a.T_HR_SALARYSTANDARD.SALARYSTANDARDID,
                                                        SALARYITEMNAME = a.T_HR_SALARYITEM.SALARYITEMNAME,
                                                        REMARK = a.REMARK,
                                                        SUM = a.SUM,
                                                        ORDERNUMBER = a.ORDERNUMBER
                                                    };

            if (!string.IsNullOrEmpty(filterString))
            {
                //ents = ents.Where(filterString, queryParas.ToArray());
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_SALARYSTANDARDITEM>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 根据薪资标准项ID查询实体
        /// </summary>
        /// <param name="SalaryItemSetID">薪资标准项ID</param>
        /// <returns>返回薪资标准项实体</returns>
        public T_HR_SALARYSTANDARDITEM GetSalaryStandardItemByID(string SalaryStandardItemID)
        {
            var ents = from a in dal.GetTable()
                       where a.STANDRECORDITEMID == SalaryStandardItemID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据薪资标准ID查询薪资项目
        /// </summary>
        /// <param name="SalaryItemSetID">薪资标准ID</param>
        /// <returns>返回薪资标准项实体集合</returns>
        public List<T_HR_SALARYSTANDARDITEM> GetSalaryStandardItemsByStandardID(string standerdID)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYSTANDARDITEM>().Include("T_HR_SALARYITEM")
                       where a.T_HR_SALARYSTANDARD.SALARYSTANDARDID == standerdID
                       select a;
            return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 根据薪资标准ID查询薪资项目视图
        /// </summary>
        /// <param name="SalaryItemSetID">薪资标准ID</param>
        /// <returns>返回薪资标准项视图实体集合</returns>
        public List<V_SALARYSTANDARDITEM> GetSalaryStandardItemsViewByStandarID(string standardID)
        {
            IQueryable<V_SALARYSTANDARDITEM> ents = from a in dal.GetObjects<T_HR_SALARYSTANDARDITEM>().Include("T_HR_SALARYITEM")
                                                    where a.T_HR_SALARYSTANDARD.SALARYSTANDARDID == standardID
                                                    select new V_SALARYSTANDARDITEM
                                                    {
                                                        STANDRECORDITEMID = a.STANDRECORDITEMID,
                                                        SALARYSTANDARDID = a.T_HR_SALARYSTANDARD.SALARYSTANDARDID,
                                                        SALARYITEMNAME = a.T_HR_SALARYITEM.SALARYITEMNAME,
                                                        REMARK = a.REMARK,
                                                        SUM = a.SUM,
                                                        ORDERNUMBER = a.ORDERNUMBER
                                                    };
            return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 根据薪资项类型查询
        /// </summary>
        /// <param name="SalaryItemType">薪资项类型名称</param>
        /// <returns>返回薪资项检索结果</returns>
        public List<T_HR_SALARYSTANDARDITEM> GetSalaryStandardItemSets(string SalaryItemType)
        {
            //var ents = from a in dal.GetTable()
            //           where a.SALARYITEMTYPE == SalaryItemType
            //           select a;
            //return ents.Count() > 0 ? ents.ToList() : null;
            return null;
        }

        /// <summary>
        /// 获取所有薪资项
        /// </summary>
        /// <returns>返回所有薪资项结果</returns>
        public List<T_HR_SALARYSTANDARDITEM> GetSalaryStandardItems()
        {
            var ents = dal.GetObjects<T_HR_SALARYSTANDARDITEM>();
            return ents.Count() > 0 ? ents.ToList() : null;
        }

        /// <summary>
        /// 根据薪资项名称查询
        /// </summary>
        /// <param name="SalaryItemSetName">薪资项名</param>
        /// <returns>返回bool类型</returns>
        public bool GetSalaryStandardItemName(string SalaryItemSetName)
        {
            //var ents = from a in dal.GetTable()
            //           where a. == SalaryItemSetName
            //           select a;
            //return ents.Count() > 0 ? true : false;
            return false;
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    IQueryable<T_HR_SALARYSTANDARDITEM> ents = from a in DataContext.T_HR_SALARYSTANDARDITEM
        //                                           select a;
        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            IQueryable<T_HR_SALARYSTANDARDITEM> ents = from a in dal.GetObjects<T_HR_SALARYSTANDARDITEM>()
                                                       select a;
            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        /// <summary>
        /// 添加薪资标准薪资项
        /// </summary>
        /// <param name="entity">薪资标准薪资项实体</param>
        public int SalaryStandardItemAdd(T_HR_SALARYSTANDARDITEM entity)
        {
            if (!string.IsNullOrEmpty(entity.T_HR_SALARYSTANDARD.SALARYSTANDARDID))
            {
                try
                {
                    T_HR_SALARYSTANDARDITEM ent = new T_HR_SALARYSTANDARDITEM();
                    Utility.CloneEntity<T_HR_SALARYSTANDARDITEM>(entity, ent);
                    ent.T_HR_SALARYSTANDARDReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", entity.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
                    ent.T_HR_SALARYITEMReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYITEM", "SALARYITEMID", entity.T_HR_SALARYITEM.SALARYITEMID);
                    Utility.RefreshEntity(ent);
                    return dal.Add(ent);
                }
                catch (Exception exc)
                {
                    SMT.Foundation.Log.Tracer.Debug(exc.Message);
                    exc.Message.ToString();
                }
            }
            return 0;
        }
        /// <summary>
        /// 添加多个薪资项
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public void SalaryStandardItemsAdd(List<T_HR_SALARYSTANDARDITEM> objs)
        {
            foreach(var item in objs)
            {
                SalaryStandardItemAdd(item);
            }
        }
        /// <summary>
        /// 更新薪资标准项
        /// </summary>
        /// <param name="entity">薪资标准项实体</param>
        public void SalaryStandardItemUpdate(T_HR_SALARYSTANDARDITEM entity)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.STANDRECORDITEMID == entity.STANDRECORDITEMID
                           select a;
                if (ents.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(ents.FirstOrDefault().T_HR_SALARYSTANDARD.SALARYSTANDARDID))
                    {
                        T_HR_SALARYSTANDARDITEM ent = new T_HR_SALARYSTANDARDITEM();
                        Utility.CloneEntity<T_HR_SALARYSTANDARDITEM>(entity, ent);
                        ent.T_HR_SALARYSTANDARDReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", entity.T_HR_SALARYSTANDARD.SALARYSTANDARDID);
                        ent.T_HR_SALARYITEMReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYITEM", "SALARYITEMID", entity.T_HR_SALARYITEM.SALARYITEMID);
                        Utility.RefreshEntity(ent);

                        dal.Update(ent);
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 删除薪资标准项记录，可同时删除多行记录
        /// </summary>
        /// <param name="SalaryItemSetIDs">薪资标准项ID数组</param>
        /// <returns></returns>
        public int SalaryStandardItemDelete(string[] SalaryItemSetIDs)
        {
            foreach (string id in SalaryItemSetIDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYSTANDARDITEM>()
                           where e.STANDRECORDITEMID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges();
        }


       /// <summary>
       /// 根据薪资标准ID删除薪资标准的薪资项
       /// </summary>
       /// <param name="standID"></param>
       /// <returns></returns>
        public int SalaryStandardItemsDeleteByStandID(string standID)
        {
            var ents = from e in dal.GetObjects<T_HR_SALARYSTANDARDITEM>()
                       where e.T_HR_SALARYSTANDARD.SALARYSTANDARDID == standID
                       select e;

            if (ents == null)
            {
                return 0;
            }

            int iCount = ents.Count();
            if (iCount == 0)
            {
                return 0;
            }

            foreach (var ent in ents)
            {                
                if (ent != null)
                {
                    dal.Delete(ent);
                }
            }
            return iCount;
        }
    }
}
