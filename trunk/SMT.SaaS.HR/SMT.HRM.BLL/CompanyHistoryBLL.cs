using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL
{
    public class CompanyHistoryBLL : BaseBll<T_HR_COMPANYHISTORY>
    {
        /// <summary>
        /// 根据日期查询公司信息
        /// </summary>
        /// <param name="currentDate"></param>
        /// <returns></returns>
        public List<T_HR_COMPANYHISTORY> GetCompanyHistory(DateTime currentDate)
        {
            //var ents = (from a in dal.GetObjects()
            //            //where a.REUSEDATE <= currentDate
            //            //&& (!a.CANCELDATE.HasValue || a.CANCELDATE >= currentDate)
            //            select a.COMPANYID).Distinct()
            //            .Select(p =>
            //              new
            //                {
            //                    his = (from b in dal.GetObjects()
            //                           where b.COMPANYID == p
            //                           orderby b.REUSEDATE descending
            //                           select b).FirstOrDefault()
            //                });
            //foreach (var v in ents)
            //{

            //    if (!v.his.T_HR_COMPANYDEFReference.IsLoaded)
            //    {

            //        v.his.T_HR_COMPANYDEFReference.Load();
            //    }

            //}
            //var ent = from c in ents
            //          select c.his;

            //return ent.ToList();
            //var ents1 =
            //       (from c in dal.GetObjects()
            //        select c.COMPANYID).Distinct()
            //           .Select(p =>
            //           new
            //           {
            //               lstc = (from lc in dal.GetObjects()
            //                           where lc.COMPANYID == p
            //                           orderby lc.REUSEDATE descending
            //                           select lc).FirstOrDefault()
            //           });
            currentDate = Convert.ToDateTime(currentDate.ToString("yyyy-MM-dd") + " 23:59:59");
            List<T_HR_COMPANYHISTORY> list = new List<T_HR_COMPANYHISTORY>();
            var ents = (from a in dal.GetObjects()
                        where a.REUSEDATE <= currentDate
                        && (!a.CANCELDATE.HasValue || a.CANCELDATE >= currentDate)
                        select new { Cid = a.COMPANYID }).Distinct();

            foreach (var h in ents)
            {
                var his = (from b in dal.GetObjects().Include("T_HR_COMPANY")
                           where b.COMPANYID == h.Cid && b.REUSEDATE <= currentDate
                           && (!b.CANCELDATE.HasValue || b.CANCELDATE >= currentDate)
                           orderby b.REUSEDATE descending
                           select b);

                //如果用FirstOrDefault 排序无效
                //var ent = his.FirstOrDefault();

                List<T_HR_COMPANYHISTORY> tmpList = his.ToList();

                if (tmpList != null && tmpList.Count > 0)
                {
                    list.Add(tmpList[0]);
                }
            }
            return list;
        }

        public T_HR_COMPANYHISTORY GetLastCompany(string companyID)
        {
            var ents = (from b in dal.GetObjects()
                        orderby b.REUSEDATE descending
                        select b).Take(1);
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        public void CompanyHistoryAdd(T_HR_COMPANYHISTORY compay)
        {
            try
            {
                T_HR_COMPANYHISTORY ent = new T_HR_COMPANYHISTORY();
                //ent.RECORDSID = compay.RECORDSID;
                //ent.COMPANYID = compay.COMPANYID;
                //ent.ENAME = compay.ENAME;
                //ent.CNAME = compay.CNAME;
                Utility.CloneEntity(compay, ent);
                if (compay.T_HR_COMPANY != null)
                {
                    ent.T_HR_COMPANYReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", compay.T_HR_COMPANY.COMPANYID);
                }
                //ent.SUPERIORID = compay.SUPERIORID;
                //ent.COMPANYDICTIONARYID = compay.COMPANYDICTIONARYID;
                //ent.COMPANYADDRESS = compay.COMPANYADDRESS;
                //ent.LEGALPERSON = compay.LEGALPERSON;
                //ent.LINKMAN = compay.LINKMAN;
                //ent.TELNUMBER = compay.TELNUMBER;
                //ent.ADDRESS = compay.ADDRESS;
                //ent.LEGALPERSONID = compay.LEGALPERSONID;
                //ent.BUSSINESSLICENCENO = compay.BUSSINESSLICENCENO;
                //ent.BUSSINESSAREA = compay.BUSSINESSAREA;
                //ent.ACCOUNTCODE = compay.ACCOUNTCODE;
                //ent.BANKID = compay.BANKID;
                //ent.EMAIL = compay.EMAIL;
                //ent.ZIPCODE = compay.ZIPCODE;
                //ent.FAXNUMBER = compay.FAXNUMBER;
                //ent.REUSEDATE = compay.REUSEDATE;
                //ent.CREATEDATE = compay.CREATEDATE;
                //ent.CREATEUSERID = compay.CREATEUSERID;l
                dal.Add(ent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 获取所有生效日期
        /// </summary>
        /// <returns></returns>
        public List<T_HR_COMPANYHISTORY> GetCompanyHistoryDate()
        {
            var ents = from a in dal.GetObjects()
                       orderby a.REUSEDATE descending
                       select a;
            return ents.Count() > 0 ? ents.ToList() : null;
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="EntityKeyName"></param>
        /// <param name="EntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            SMT.Foundation.Log.Tracer.Debug("手机审单调用了" + strEntityName + "表单ID为：" + EntityKeyValue + System.DateTime.Now.ToString());
            return dal.UpdateCheckState(strEntityName, EntityKeyName, EntityKeyValue, CheckState);
        }
    }
}
