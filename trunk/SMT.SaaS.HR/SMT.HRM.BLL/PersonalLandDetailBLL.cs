using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT.HRM.CustomModel;
using System.Data;

namespace SMT.HRM.BLL
{
    public class PersonalLandDetailBLL : BaseBll<V_LandDetail>
    {
        public List<V_LandDetail> GetPersonalLandDetailListByMultSearch(string strOwnerID, string strOrderBy, int pageIndex,
            int pageSize, ref int pageCount, ref int iLoginTimes, ref int iLoginPersonCount, string filterString, List<object> objArgs)
        {
            List<V_LandDetail> entList = new List<V_LandDetail>();
            PersonalLandDetailDAL dalPersonalLandDetail = new PersonalLandDetailDAL();
            
            if (string.IsNullOrEmpty(strOrderBy))
            {
                strOrderBy = "p.ownercompanyid";
            }

            entList = dalPersonalLandDetail.GetPersonalLandDetailList(strOrderBy, pageIndex, pageSize, ref pageCount, ref iLoginTimes, ref iLoginPersonCount, filterString, objArgs.ToArray());
            return entList;
        }

        /// <summary>
        /// 获取部门的执行一览的导出数据
        /// </summary>
        /// <param name="strOrgType"></param>
        /// <param name="strOrgID"></param>
        /// <param name="strOwnerId"></param>
        /// <param name="iYear"></param>
        /// <param name="iMonthStart"></param>
        /// <param name="iMonthEnd"></param>
        /// <param name="strFilter"></param>
        /// <param name="objArgs"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public byte[] OutFilePersonalLandDetailList(string strOwnerID, string strOrderBy, string filterString, List<object> objArgs)
        {
            PersonalLandDetailDAL dalPersonalLandDetail = new PersonalLandDetailDAL();
            
            if (string.IsNullOrEmpty(strOrderBy))
            {
                strOrderBy = "p.ownercompanyid";
            }
            DataTable dtExport = dalPersonalLandDetail.GetPersonalLandDetailForExport(strOrderBy, filterString, objArgs.ToArray());
            
            return Utility.OutFileStream("员工登录记录明细", dtExport);
        }
    }
}
