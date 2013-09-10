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
    public class OaNoticeBll : BaseBll<T_OA_MEETINGMESSAGE>
    {
        /// <summary>
        /// 获取信息
        /// </summary>
        /// <returns></returns>
        public IQueryable<V_SystemNotice> GetHouseInfoTree()
        {

            var entity = (from p in dal.GetObjects()
                          orderby p.CREATEDATE descending
                          select new V_SystemNotice { FormId = p.MEETINGMESSAGEID, FormTitle = p.TITLE, Formtype = "Notice", FormDate = p.CREATEDATE }
                         ).Take(10).Union
                         (
                         from k in dal.GetObjects<T_OA_HOUSEINFOISSUANCE>()
                         where k.CHECKSTATE == "2"
                         orderby k.CREATEDATE descending
                         select new V_SystemNotice { FormId = k.ISSUANCEID, FormTitle = k.ISSUANCETITLE, Formtype = "HouseIssue", FormDate = Convert.ToDateTime(k.CREATEDATE) }).Take(10);


            return entity;
        }

    }
}
