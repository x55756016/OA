using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using SMT.Foundation.Log;

namespace SMT.SaaS.Permission.BLL
{
    public class provinceBll : BaseBll<T_SYS_PROVINCECITY>
    {
        /// <summary>
        ///新增字典
        /// </summary>
        /// <param name="dict"></param>
        public void SysDictionaryAdd(T_SYS_PROVINCECITY dict, ref string strMsg)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_SYS_PROVINCECITY>().Include("T_SYS_COUNTRY")
                           where ent.AREANAME== dict.AREANAME && ent.COUNTRYID == dict.COUNTRYID && ent.T_SYS_PROVINCECITY2.PROVINCEID == dict.T_SYS_PROVINCECITY2.PROVINCEID
                           select ent;
                var countrys = from ent in dal.GetObjects<T_SYS_COUNTRY>()
                               where ent.COUNTRYID == dict.COUNTRYID
                               select ent;
                T_SYS_COUNTRY country = new T_SYS_COUNTRY();
                country = countrys.FirstOrDefault();
                if (ents.Count() > 0)
                {
                    strMsg = "REPETITION";
                    return;
                }
                decimal? deInt = 0;
                var deIntss = from ent in dal.GetObjects<T_SYS_PROVINCECITY>()
                              //where ent.COUNTRYID == dict.COUNTRYID

                              select ent;
                deInt = deIntss.Max(p => p.AREAVALUE);

                
                
                if (deInt == null)
                {
                    deInt = 1;
                }
                else
                {
                    deInt += 1;
                }
                if (dict.ISCITY == "1" || dict.ISCOUNTRYTOWN == "1")
                {
                    
                }
                dict.AREAVALUE = deInt;
                T_SYS_PROVINCECITY temp = new T_SYS_PROVINCECITY();
                //temp.T_SYS_COUNTRY = country;
                //temp.T_SYS_COUNTRYReference.EntityKey =
                //new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_COUNTRY", "COUNTRYID", country.COUNTRYID);
                Utility.CloneEntity<T_SYS_PROVINCECITY>(dict, temp);
                temp.CREATEDATE = DateTime.Now;
                temp.UPDATEDATE = DateTime.Now;

                if (dict.T_SYS_PROVINCECITY2 != null)
                {
                    temp.T_SYS_PROVINCECITY2Reference.EntityKey =
                new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PROVINCECITY", "PROVINCEID", dict.T_SYS_PROVINCECITY2.PROVINCEID);

                }
                
                this.Add(temp);
               

            }
            catch (Exception ex)
            {
                Tracer.Debug("权限系统SysDictionaryBLL-SysDictionaryAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //throw (ex);
            }
        }


        /// <summary>
        ///新增字典
        /// </summary>
        /// <param name="dict"></param>
        //public void SysDictionaryAdd(T_SYS_PROVINCECITY dict, ref string strMsg)
        //{
        //    try
        //    {
        //        var ents = from ent in dal.GetObjects<T_SYS_PROVINCECITY>().Include("T_SYS_COUNTRY")
        //                   where ent.AREANAME == dict.AREANAME && ent.COUNTRYID == dict.COUNTRYID
        //                   select ent;
        //        var countrys = from ent in dal.GetObjects<T_SYS_COUNTRY>()
        //                       where ent.COUNTRYID == dict.COUNTRYID
        //                       select ent;
        //        T_SYS_COUNTRY country = new T_SYS_COUNTRY();
        //        country = countrys.FirstOrDefault();
        //        if (ents.Count() > 0)
        //        {
        //            strMsg = "REPETITION";
        //            return;
        //        }
        //        decimal? deInt = 0;
        //        var deIntss = from ent in dal.GetObjects<T_SYS_PROVINCECITY>()
        //                      //where ent.COUNTRYID == dict.COUNTRYID

        //                      select ent;
        //        deInt = deIntss.Max(p => p.AREAVALUE);



        //        if (deInt == null)
        //        {
        //            deInt = 1;
        //        }
        //        else
        //        {
        //            deInt += 1;
        //        }
        //        if (dict.ISCITY == "1" || dict.ISCOUNTRYTOWN == "1")
        //        {

        //        }
        //        dict.AREAVALUE = deInt;
        //        T_SYS_PROVINCECITY temp = new T_SYS_PROVINCECITY();
        //        //temp.T_SYS_COUNTRY = country;
        //        //temp.T_SYS_COUNTRYReference.EntityKey =
        //        //new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_COUNTRY", "COUNTRYID", country.COUNTRYID);
        //        Utility.CloneEntity<T_SYS_PROVINCECITY>(dict, temp);
        //        temp.CREATEDATE = DateTime.Now;
        //        temp.UPDATEDATE = DateTime.Now;

        //        if (dict.T_SYS_PROVINCECITY2 != null)
        //        {
        //            temp.T_SYS_PROVINCECITY2Reference.EntityKey =
        //        new System.Data.EntityKey("SMT_System_EFModelContext.T_SYS_PROVINCECITY", "PROVINCEID", dict.T_SYS_PROVINCECITY2.PROVINCEID);

        //        }

        //        this.Add(temp);


        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.Debug("权限系统SysDictionaryBLL-SysDictionaryAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
        //        //throw (ex);
        //    }
        //}

        /// <summary>
        /// 获取国家信息
        /// </summary>
        /// <returns></returns>
        public List<T_SYS_COUNTRY> GetCountry()
        {
            var ents = from ent in dal.GetObjects<T_SYS_COUNTRY>()
                       select ent;
            return ents.ToList();
        }

        /// <summary>
        /// 获取省会城市
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public List<V_ProvinceCity> GetProvinceCity(string countryId)
        {


            var ents = from ent in dal.GetObjects<T_SYS_PROVINCECITY>().Include("T_SYS_PROVINCECITY2")
                       //where ent.COUNTRYID == countryId
                       select ent;
            List<V_ProvinceCity> listprovince = new List<V_ProvinceCity>();
            if (ents != null)
            {
                if (ents.Count() > 0)
                {
                    ents.ToList().ForEach(item =>
                    {
                        V_ProvinceCity ent = new V_ProvinceCity();
                        ent.PROVINCEID = item.PROVINCEID;
                           ent.COUNTRYID = item.COUNTRYID;
                           ent.AREANAME = item.AREANAME;
                           ent.FATHERID = item.T_SYS_PROVINCECITY2== null ? "":item.T_SYS_PROVINCECITY2.PROVINCEID;
                           ent.AREAVALUE = item.AREAVALUE;
                           ent.ISCITY = item.ISCITY;
                           ent.ISPROVINCE = item.ISPROVINCE;
                           ent.ISCOUNTRYTOWN = item.ISCOUNTRYTOWN;

                           listprovince.Add(ent);

                    });
                }
            }
            return listprovince.Count() > 0 ? listprovince : null;

        }
        
    }
}
