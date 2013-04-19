using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Data.Objects;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{
    public class LicenseManagementBll : BaseBll<T_OA_LICENSEMASTER>
    {
        //private SMT_OA_EFModelContext context = new SMT_OA_EFModelContext();

        /// <summary>
        /// 查询证照主表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        /// 
        public List<T_OA_LICENSEMASTER> GetLicenseById(string licenseID)    
        {
            var entity = from q in dal.GetObjects().Include("T_OA_ORGANIZATION")
                         where q.LICENSEMASTERID == licenseID && q.ISVALID=="1"
                         select q;            
            return entity.Count() > 0 ? entity.ToList() : null;
        }

        public IQueryable<T_OA_LICENSEMASTER> GetLicenseList(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            var entity = from q in dal.GetObjects().Include("T_OA_ORGANIZATION")
                         select q;
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAORGANLICEN");
            if (!string.IsNullOrEmpty(filterString))
            {
                entity = entity.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            entity = entity.OrderBy(sort);
            entity = Utility.Pager<T_OA_LICENSEMASTER>(entity, pageIndex, pageSize, ref pageCount);
            return entity.Count() > 0 ? entity : null;     
        }
    }
}
