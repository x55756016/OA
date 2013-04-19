using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class ImportSetDetailBLL:BaseBll<T_HR_IMPORTSETDETAIL>
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
        public IQueryable<T_HR_IMPORTSETDETAIL> ImportSetDetailPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_IMPORTSETDETAIL");
            }
            IQueryable<T_HR_IMPORTSETDETAIL> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_IMPORTSETDETAIL>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        public T_HR_IMPORTSETDETAIL GetImportSetDetailByID(string detailID)
        {
            return dal.GetObjects().FirstOrDefault(s => s.DETAILID == detailID);
        }

        public List<T_HR_IMPORTSETDETAIL> GetImportSetDetailByMasterID(string masterID)
        {
            var ents = dal.GetObjects().Include("T_HR_IMPORTSETMASTER").Where(s => s.T_HR_IMPORTSETMASTER.MASTERID == masterID);
            return ents.Count() > 0 ? ents.ToList() : null;
        }
    }
}
