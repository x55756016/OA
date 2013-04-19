using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
namespace SMT.HRM.BLL
{
    public class EmployeeEvectionReportBLL:BaseBll<T_HR_EMPLOYEEEVECTIONREPORT>
    {

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的出差记录信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEEEVECTIONREPORT> EmployeeEvectionReportPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            IQueryable<T_HR_EMPLOYEEEVECTIONREPORT> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEEEVECTIONREPORT>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 新增出差报告
        /// </summary>
        /// <param name="entity">出差记录实体</param>
        public void EmployeeEvectionReportADD(T_HR_EMPLOYEEEVECTIONREPORT entity)
        {
            try
            {
                dal.Add(entity);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
