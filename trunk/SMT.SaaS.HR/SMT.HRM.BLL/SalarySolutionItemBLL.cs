using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using SMT.HRM.CustomModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
namespace SMT.HRM.BLL
{
    public class SalarySolutionItemBLL : BaseBll<T_HR_SALARYSOLUTIONITEM>
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="obj"></param>
        public string SalarySolutionItemAdd(T_HR_SALARYSOLUTIONITEM obj)
        {
            string strMsg = string.Empty;
            try
            {

                var solution = from b in dal.GetObjects<T_HR_SALARYSOLUTION>()
                               where b.SALARYSOLUTIONID == obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID
                               select b;
                if (solution.Count() <= 0)
                {
                    return "ADDSOLUTIONFIRST";
                }
                var ents = from c in dal.GetObjects<T_HR_SALARYSOLUTIONITEM>()
                           where c.T_HR_SALARYITEM.SALARYITEMID == obj.T_HR_SALARYITEM.SALARYITEMID && c.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID
                           select c;
                if (ents.Count() > 0)
                {
                    return "EXIST";
                }
                T_HR_SALARYSOLUTIONITEM tmpEnt = new T_HR_SALARYSOLUTIONITEM();
                Utility.CloneEntity<T_HR_SALARYSOLUTIONITEM>(obj, tmpEnt);

                if (obj.T_HR_SALARYSOLUTION != null)
                {
                    tmpEnt.T_HR_SALARYSOLUTIONReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);
                }
                if (obj.T_HR_SALARYITEM != null)
                {
                    tmpEnt.T_HR_SALARYITEMReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYITEM", "SALARYITEMID", obj.T_HR_SALARYITEM.SALARYITEMID);
                }
                // Utility.RefreshEntity(tmpEnt);
                dal.Add(tmpEnt);

                strMsg = "SAVESUCCESSED";
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                strMsg = ex.Message.ToString();
            }
            return strMsg;
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="obj"></param>
        public string SalarySolutionItemUpdate(T_HR_SALARYSOLUTIONITEM obj)
        {
            string strMsg = string.Empty;
            try
            {
                var ents = from c in dal.GetObjects<T_HR_SALARYSOLUTIONITEM>()
                           where c.SOLUTIONITEMID == obj.SOLUTIONITEMID
                           select c;
                if (ents != null && ents.Count() > 0)
                {
                    var tmpEnt = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_SALARYSOLUTIONITEM>(obj, tmpEnt);

                    if (obj.T_HR_SALARYSOLUTION != null)
                    {
                        tmpEnt.T_HR_SALARYSOLUTIONReference.EntityKey =
                                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);
                    }
                    if (obj.T_HR_SALARYITEM != null)
                    {
                        tmpEnt.T_HR_SALARYITEMReference.EntityKey =
                                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYITEM", "SALARYITEMID", obj.T_HR_SALARYITEM.SALARYITEMID);
                    }
                    dal.Update(tmpEnt);
                    strMsg = "SAVESUCCESSED";
                }
                else
                {
                    strMsg = "NOTFOUND";
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                strMsg = ex.Message.ToString();
            }
            return strMsg;
        }
        /// <summary>
        /// 增加多个薪资项目
        /// </summary>
        /// <returns></returns>
        //public string SalarySolutionItemsAdd(List<T_HR_SALARYSOLUTIONITEM> objs)
        //{
        //    string strMsg = "";
        //    try
        //    {
        //        foreach (var item in objs)
        //        {
        //            var solution = from b in DataContext.T_HR_SALARYSOLUTION
        //                           where b.SALARYSOLUTIONID == item.T_HR_SALARYSOLUTION.SALARYSOLUTIONID
        //                           select b;
        //            if (solution.Count() <= 0)
        //            {
        //                return "NOSOLUTION";
        //            }
        //            var ents = from c in DataContext.T_HR_SALARYSOLUTIONITEM
        //                       where c.T_HR_SALARYITEM.SALARYITEMID == item.T_HR_SALARYITEM.SALARYITEMID && c.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == item.T_HR_SALARYSOLUTION.SALARYSOLUTIONID
        //                       select c;
        //            if (ents.Count() > 0)
        //            {
        //                return "EXIST";
        //            }
        //            T_HR_SALARYSOLUTIONITEM tmpEnt = new T_HR_SALARYSOLUTIONITEM();
        //            Utility.CloneEntity<T_HR_SALARYSOLUTIONITEM>(item, tmpEnt);

        //            if (item.T_HR_SALARYSOLUTION != null)
        //            {
        //                tmpEnt.T_HR_SALARYSOLUTIONReference.EntityKey =
        //                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", item.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);
        //            }
        //            if (item.T_HR_SALARYITEM != null)
        //            {
        //                tmpEnt.T_HR_SALARYITEMReference.EntityKey =
        //                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYITEM", "SALARYITEMID", item.T_HR_SALARYITEM.SALARYITEMID);
        //            }
        //            // Utility.RefreshEntity(tmpEnt);
        //            dal.Add(tmpEnt);

        //        }
        //        strMsg = "SAVESUCCESSED";
        //    }
        //    catch (Exception ex)
        //    {
        //        strMsg = ex.Message.ToString();
        //    }

        //    return strMsg;
        //}
        /// <summary>
        /// 必填项的薪资项目
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="para"></param>
        /// <param name="solutionID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public string SalarySolutionItemsAdd(string filter, IList<object> para, string solutionID, string userID)
        {
            string strMsg = "";
            try
            {
                SalaryItemSetBLL setbll = new SalaryItemSetBLL();
                var q = from solution in dal.GetObjects<T_HR_SALARYSOLUTION>()
                        where solution.SALARYSOLUTIONID == solutionID
                        select solution;
                if (q.Count() > 0)
                {
                }
                else
                {
                    strMsg = "ADDSOLUTIONFIRST";
                    SMT.Foundation.Log.Tracer.Debug("薪资方案薪资项目添加失败，找不到薪资方案");
                    return strMsg;
                }

                //var solution = from b in dal.GetObjects<T_HR_SALARYSOLUTION>()
                //               where b.SALARYSOLUTIONID == solutionID
                //               select b;
                //if (solution.Count() <= 0)
                //{
                //    return "ADDSOLUTIONFIRST";
                //}


                //临时注掉  全部采用集团的薪资项
                //  List<T_HR_SALARYITEM> itemSets = setbll.GetSalaryItemSetByFilter(filter, para, userID);
                var itemSets = from c in dal.GetObjects<T_HR_SALARYITEM>()
                               where c.OWNERCOMPANYID == q.FirstOrDefault().OWNERCOMPANYID
                               select c;
               

                foreach (var item in itemSets)
                {

                    var ents = from c in dal.GetObjects<T_HR_SALARYSOLUTIONITEM>()
                               where c.T_HR_SALARYITEM.SALARYITEMID == item.SALARYITEMID && c.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == solutionID
                               select c;
                    if (ents.Count() > 0)
                    {
                        continue;
                    }
                    T_HR_SALARYSOLUTIONITEM tmpEnt = new T_HR_SALARYSOLUTIONITEM();
                    tmpEnt.SOLUTIONITEMID = Guid.NewGuid().ToString();
                    tmpEnt.T_HR_SALARYSOLUTION = new T_HR_SALARYSOLUTION();
                    tmpEnt.T_HR_SALARYSOLUTION.SALARYSOLUTIONID = solutionID;
                    tmpEnt.T_HR_SALARYITEM = new T_HR_SALARYITEM();
                    tmpEnt.T_HR_SALARYITEM.SALARYITEMID = item.SALARYITEMID;
                    T_HR_SALARYSOLUTIONITEM itemSolution = new T_HR_SALARYSOLUTIONITEM();
                    Utility.CloneEntity<T_HR_SALARYSOLUTIONITEM>(tmpEnt, itemSolution);

                    if (tmpEnt.T_HR_SALARYSOLUTION != null)
                    {
                        itemSolution.T_HR_SALARYSOLUTIONReference.EntityKey =
                                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", tmpEnt.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);
                    }
                    if (tmpEnt.T_HR_SALARYITEM != null)
                    {
                        itemSolution.T_HR_SALARYITEMReference.EntityKey =
                                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYITEM", "SALARYITEMID", tmpEnt.T_HR_SALARYITEM.SALARYITEMID);
                    }
                    //   Utility.RefreshEntity(tmpEnt);
                    dal.Add(itemSolution);

                }
                strMsg = "SAVESUCCESSED";
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="obj"></param>
        //public string SalarySolutionItemUpdate(T_HR_SALARYTAXES obj)
        // {
        //     string strMsg = string.Empty;
        //     try
        //     {
        //         var ent = from a in dal.GetTable()
        //                   where a.SALARYTAXESID == obj.SALARYTAXESID
        //                   select a;
        //         if (ent.Count() > 0)
        //         {
        //             T_HR_SALARYTAXES tmpEnt = ent.FirstOrDefault();

        //             Utility.CloneEntity<T_HR_SALARYTAXES>(obj, tmpEnt);
        //             dal.Update(tmpEnt);
        //             strMsg = "SAVESUCCESSED";
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         strMsg = e.Message.ToString();
        //     }
        //     return strMsg;
        // }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int SalarySolutionItemsDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYSOLUTIONITEM>()
                           where e.SOLUTIONITEMID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    //DataContext.DeleteObject(ent);
                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 根据方案ID 删除薪资项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SalarySolutionItemsDeleteBySID(string id)
        {
            List<T_HR_SALARYSOLUTIONITEM> items = GetSalarySolutionItemsBySolutionID(id);
            if (items != null)
            {
                foreach (var item in items)
                {
                    dal.DeleteFromContext(item);
                    //DataContext.DeleteObject(item);
                }
            }
            return dal.SaveContextChanges();
        }

        /// <summary>
        /// 根据方案ID获取
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<T_HR_SALARYSOLUTIONITEM> GetSalarySolutionItemsBySolutionID(string ID)
        {
            var ents = from a in dal.GetTable()
                       where a.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == ID
                       select a;
            return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 根据方案薪资项目的ID获取方案薪资项目
        /// </summary>
        /// <param name="solutionItemID"></param>
        /// <returns></returns>
        public T_HR_SALARYSOLUTIONITEM GetSalarySolutionItemBysolutionItemID(string solutionItemID)
        {
            var ents = from a in dal.GetObjects<T_HR_SALARYSOLUTIONITEM>().Include("T_HR_SALARYITEM")
                       where a.SOLUTIONITEMID == solutionItemID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
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
        public IQueryable<V_SALARYSOLUTIONITEM> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            //   SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYTAXES");
            //   SetFilterWithflow("SALARYTAXESID", "T_HR_SALARYTAXES", userID, ref CheckState, ref  filterString, ref queryParas);
            //if (!string.IsNullOrEmpty(CheckState))
            //{
            //    if (!string.IsNullOrEmpty(filterString))
            //    {
            //        filterString += " and ";
            //    }
            //    filterString += "CHECKSTATE == @" + queryParas.Count();
            //    queryParas.Add(CheckState);
            //}
            IQueryable<V_SALARYSOLUTIONITEM> ents = from a in dal.GetObjects<T_HR_SALARYSOLUTIONITEM>().Include("T_HR_SALARYITEM")
                                                    select new V_SALARYSOLUTIONITEM
                                                    {
                                                        SOLUTIONITEMID = a.SOLUTIONITEMID,
                                                        SALARYSOLUTIONID = a.T_HR_SALARYSOLUTION.SALARYSOLUTIONID,
                                                        SALARYITEMNAME = a.T_HR_SALARYITEM.SALARYITEMNAME,
                                                        MUSTSELECTED = a.T_HR_SALARYITEM.MUSTSELECTED,
                                                        ORDERNUMBER = a.ORDERNUMBER
                                                        //  REMARK = a,
                                                        //  SUM = a.SUM
                                                    };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_SALARYSOLUTIONITEM>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
    }
}
