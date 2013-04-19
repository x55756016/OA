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
    public class SalaryTaxesBLL : BaseBll<T_HR_SALARYTAXES>
    {

        /// <summary>
        /// 检测是否允许新增
        /// </summary>
        /// <param name="salarySolutionID">税率ID</param>
        /// <returns></returns>
        public bool CheckSalaryTaxes(string salarySolutionID, decimal minSum)
        {
            var ents = from a in dal.GetTable()
                       where a.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == salarySolutionID 
                       select a;
            if (ents.Count() > 0)
            {
                var e = ents.Select(m => m.TAXESSUM).Max();
                if (e!=null && minSum==e.Value) return true;
                else return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="obj"></param>
        public string SalaryTaxesAdd(T_HR_SALARYTAXES obj)
        {
            string strMsg = string.Empty;
            try
            {

                var ents = from c in dal.GetObjects<T_HR_SALARYSOLUTION>()
                          where c.SALARYSOLUTIONID == obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID
                          select c;
                if (ents.Count() <= 0)
                {
                    return "NOSOLUTION";
                }
                T_HR_SALARYTAXES tmpEnt = new T_HR_SALARYTAXES();

                Utility.CloneEntity<T_HR_SALARYTAXES>(obj, tmpEnt);
                if (obj.T_HR_SALARYSOLUTION != null)
                {
                    tmpEnt.T_HR_SALARYSOLUTIONReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);

                }
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
        /// 更新
        /// </summary>
        /// <param name="obj"></param>
        public string SalaryTaxesUpdate(T_HR_SALARYTAXES obj)
        {
            string strMsg = string.Empty;
            try
            {
                var ent = from a in dal.GetTable()
                          where a.SALARYTAXESID == obj.SALARYTAXESID
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_SALARYTAXES tmpEnt = ent.FirstOrDefault();

                    Utility.CloneEntity<T_HR_SALARYTAXES>(obj, tmpEnt);
                    if (tmpEnt.T_HR_SALARYSOLUTION != null)
                    {
                        tmpEnt.T_HR_SALARYSOLUTIONReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);

                    }
                    dal.Update(tmpEnt);
                    strMsg = "SAVESUCCESSED";
                }
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug(e.Message);
                strMsg = e.Message.ToString();
            }
            return strMsg;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int SalaryTaxesDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYTAXES>()
                           where e.SALARYTAXESID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    //DataContext.DeleteObject(ent);
                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges();  //DataContext.SaveChanges();
        }

        /// <summary>
        /// 根据ID获取
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<T_HR_SALARYTAXES> GetSalaryTaxesBySolutionID(string ID)
        {
            var ents = from a in dal.GetTable()
                       where a.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == ID
                       select a;
            return ents.Count() > 0 ? ents.ToList() : null;
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
        public IQueryable<T_HR_SALARYTAXES> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
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
            IQueryable<T_HR_SALARYTAXES> ents = dal.GetObjects<T_HR_SALARYTAXES>();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYTAXES>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
    }
}
