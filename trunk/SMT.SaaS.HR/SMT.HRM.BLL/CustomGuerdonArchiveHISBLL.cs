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
    public class CustomGuerdonArchiveHISBLL : BaseBll<T_HR_CUSTOMGUERDONARCHIVEHIS>
    {

        /// <summary>
        /// 添加自定义薪资档案历史
        /// </summary>
        /// <param name="obj"></param>
        public void CustomGuerdonArchiveHISAdd(T_HR_CUSTOMGUERDONARCHIVEHIS obj)
        {
            try
            {
                T_HR_CUSTOMGUERDONARCHIVEHIS ent = new T_HR_CUSTOMGUERDONARCHIVEHIS();
                Utility.CloneEntity<T_HR_CUSTOMGUERDONARCHIVEHIS>(obj, ent);
                if (obj.T_HR_SALARYARCHIVEHIS != null)
                {
                    ent.T_HR_SALARYARCHIVEHISReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYARCHIVEHIS", "SALARYARCHIVEID", obj.T_HR_SALARYARCHIVEHIS.SALARYARCHIVEID);

                }
                dal.Add(ent);
            }
            catch (Exception ee)
            {
                throw ee;
            }
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
        public IQueryable<V_CUSTOMGUERDONARCHIVEHIS> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            //List<object> queryParas = new List<object>();
            //queryParas.AddRange(paras);

         //   SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_CUSTOMGUERDONARCHIVEHIS");

            var ents = from c in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVEHIS>()
                       join b in dal.GetObjects<T_HR_CUSTOMGUERDON>().Include("T_HR_CUSTOMGUERDONSET") on c.CUSTOMERGUERDONID equals b.CUSTOMGUERDONID
                       select new V_CUSTOMGUERDONARCHIVEHIS
                       {
                           T_HR_CUSTOMGUERDONARCHIVEHIS = c,
                           GUERDONNAME = b.T_HR_CUSTOMGUERDONSET.GUERDONNAME
                           //SALARYARCHIVEID = c.T_HR_SALARYARCHIVEHIS.sa
                       };



            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_CUSTOMGUERDONARCHIVEHIS>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
    }
}
