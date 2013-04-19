using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Data.Objects;

namespace SMT.SaaS.OA.BLL
{
    public class LicenseDetailManagementBll : BaseBll<T_OA_LICENSEDETAIL>
    {
        
        //private SMT_OA_EFModelContext context = new SMT_OA_EFModelContext();

        public IQueryable<T_OA_LICENSEDETAIL> GetLicenseDetailById(string licenseID)
        {
            var entity = from q in dal.GetTable()
                         where q.T_OA_LICENSEMASTER.LICENSEMASTERID == licenseID
                         orderby q.CREATEDATE descending
                         select q;
            return entity.Count() > 0 ? entity : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="licenseObj"></param>
        /// <returns></returns>
        public bool AddLicenseDetail(T_OA_LICENSEDETAIL licenseObj)
        {
            try
            {
                int i = dal.Add(licenseObj);
                if (i == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        

        public bool UpdateLicenseDetail(T_OA_LICENSEDETAIL[] licenseDetailObj,T_OA_LICENSEMASTER licenseMasterObj)
        {
            dal.BeginTransaction();
            try
            {
                bool flag = true;
                //添加或更新子表信息
                if (licenseMasterObj == null || licenseDetailObj == null)
                {
                    return false;
                }
                foreach (var license in licenseDetailObj)
                {
                  
                    
                    license.T_OA_LICENSEMASTER = dal.GetObjectByEntityKey(license.T_OA_LICENSEMASTER.EntityKey) as T_OA_LICENSEMASTER;
                    //context.AddObject("T_OA_CONSERVATION", requireResultInfo);
                    //context.AddObject("T_OA_LICENSEDETAIL", license);
                    int a=dal.Add(license);
                    if (!(a > 0))
                        return false;
                    
                }

                if (flag)
                {
                    if (licenseDetailObj.Count() > 0)   //子表有更新时根据子表更新主表
                    {
                        T_OA_LICENSEDETAIL licenseLastDetailObj = GetLastUpdateObj(licenseDetailObj);
                        
                        //var entitys = licenseDal.GetTable().FirstOrDefault(s => s.LICENSEMASTERID == licenseLastDetailObj.LICENSEMASTERID);
                        var entitys = dal.GetObjects<T_OA_LICENSEMASTER>().FirstOrDefault(s => s.LICENSEMASTERID == licenseLastDetailObj.T_OA_LICENSEMASTER .LICENSEMASTERID);
                        
                        if (entitys != null)
                        {
                            //更新主表
                            entitys.LEGALPERSON = licenseLastDetailObj.LEGALPERSON;
                            entitys.ADDRESS = licenseLastDetailObj.ADDRESS;
                            entitys.LICENCENO = licenseLastDetailObj.LICENCENO;
                            entitys.BUSSINESSAREA = licenseLastDetailObj.BUSSINESSAREA;
                            entitys.FROMDATE = licenseLastDetailObj.FROMDATE;
                            entitys.TODATE = licenseLastDetailObj.TODATE;
                            entitys.DAY = licenseMasterObj.DAY;
                            entitys.POSITION = licenseMasterObj.POSITION;
                            entitys.UPDATEDATE = licenseLastDetailObj.UPDATEDATE;
                            entitys.UPDATEUSERID = licenseLastDetailObj.UPDATEUSERID;
                            entitys.UPDATEUSERNAME = licenseLastDetailObj.UPDATEUSERNAME;
                            int n=dal.Update(entitys);
                            if (!(n > 0))
                                return false;
                           
                        }
                        
                    }
                    else                              //子表没有更新时直接更新主表                     
                    {
                        var entitys = dal.GetObjects<T_OA_LICENSEMASTER>().FirstOrDefault(s => s.LICENSEMASTERID == licenseMasterObj.LICENSEMASTERID);
                        if (entitys != null)
                        {
                            entitys.DAY = licenseMasterObj.DAY;
                            entitys.POSITION = licenseMasterObj.POSITION;
                            entitys.UPDATEDATE = licenseMasterObj.UPDATEDATE;
                            entitys.UPDATEUSERID = licenseMasterObj.UPDATEUSERID;
                            entitys.UPDATEUSERNAME = licenseMasterObj.UPDATEUSERNAME;
                            int k=dal.Update(entitys);
                            if (!(k > 0))
                                return false;
                        }
                        
                    }
                    //dal.CommitTransaction();
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                //dal.RollbackTransaction();
                throw (ex);
                //return false;
            }
            return false;
        }

        /// <summary>
        /// 获取最新的记录
        /// </summary>
        /// <param name="licenseDetailObj"></param>
        /// <returns></returns>
        private T_OA_LICENSEDETAIL GetLastUpdateObj(T_OA_LICENSEDETAIL[] licenseDetailObj)
        {
            DateTime dTime = Convert.ToDateTime("1900-01-01");
            T_OA_LICENSEDETAIL retLicense = new T_OA_LICENSEDETAIL();
            foreach (var license in licenseDetailObj)
            {
                if (license.UPDATEDATE > dTime)
                {
                    retLicense = license;
                }
            }
            return retLicense;
        }
    }

}
