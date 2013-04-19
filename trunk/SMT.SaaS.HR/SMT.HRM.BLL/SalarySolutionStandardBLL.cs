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
    public class SalarySolutionStandardBLL:BaseBll<T_HR_SALARYSOLUTIONSTANDARD>
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="obj"></param>
        public string  SalarySolutionStandardAdd(T_HR_SALARYSOLUTIONSTANDARD obj)
        {
            string  strmsg = string.Empty;
            try
            {
                if (obj == null)
                {
                    return "REQUIREDFIELDS";
                }
                var tmp = from q in dal.GetObjects<T_HR_SALARYSOLUTION>()
                          where q.SALARYSOLUTIONID == obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID
                          select q;
                if (tmp.Count() <= 0)
                {
                    return "ADDSOLUTIONFIRST";
                }
                var ents = from c in dal.GetObjects<T_HR_SALARYSOLUTIONSTANDARD>()
                           where c.T_HR_SALARYSOLUTION.SALARYSOLUTIONID == obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID
                            && c.T_HR_SALARYSTANDARD.SALARYSTANDARDID == obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID
                           select c;
                if (ents.Count() > 0)
                {
                    return "EXIST";
                }
                T_HR_SALARYSOLUTIONSTANDARD ent = new T_HR_SALARYSOLUTIONSTANDARD();
                Utility.CloneEntity<T_HR_SALARYSOLUTIONSTANDARD>(obj, ent);
                if (obj.T_HR_SALARYSOLUTION != null)
                {
                    ent.T_HR_SALARYSOLUTIONReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", obj.T_HR_SALARYSOLUTION.SALARYSOLUTIONID);

                }
                if (obj.T_HR_SALARYSTANDARD != null)
                {
                    ent.T_HR_SALARYSTANDARDReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSTANDARD", "SALARYSTANDARDID", obj.T_HR_SALARYSTANDARD.SALARYSTANDARDID);

                }
                dal.Add(ent);
                strmsg = "SUCCESSED";
            }
            catch(Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                strmsg = ex.Message;
            }
            return strmsg;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int SalarySolutionStandardDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_SALARYSOLUTIONSTANDARD>()
                           where e.SOLUTIONSTANDARDID == id
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
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public new IQueryable<T_HR_SALARYSOLUTIONSTANDARD> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_SALARYSOLUTIONSTANDARD");
            IQueryable<T_HR_SALARYSOLUTIONSTANDARD> ents = dal.GetObjects<T_HR_SALARYSOLUTIONSTANDARD>().Include("T_HR_SALARYSTANDARD");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SALARYSOLUTIONSTANDARD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
    }
}
