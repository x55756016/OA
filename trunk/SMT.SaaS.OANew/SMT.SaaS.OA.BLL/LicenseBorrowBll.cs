using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Log;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Data.Objects;
using SMT.SaaS.OA.DAL.Views;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;

namespace SMT.SaaS.OA.BLL
{
    public class LicenseBorrowBll : BaseBll<T_OA_LICENSEUSER>, ILookupEntity
    {
        //SMT_OA_EFModelContext context = new SMT_OA_EFModelContext();
        /// <summary>
        /// 获取证照外借记录
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public IQueryable<T_OA_LICENSEUSER> GetLicenseQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState,string userID)
        {
            try
            {
                
                var entity = from q in dal.GetObjects().Include("T_OA_LICENSEMASTER")
                                where q.HASRETURN == "0"
                                select q;
                if (flowInfoList != null)
                {
                    entity = from a in entity.ToList().AsQueryable()
                                join l in flowInfoList on a.LICENSEUSERID equals l.FormID
                                select a;
                }
                if (!string.IsNullOrEmpty(checkState))
                {
                    entity = entity.Where(s => s.CHECKSTATE == checkState);
                }
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAORGANLICENUSER");
                if (!string.IsNullOrEmpty(filterString))
                {
                    //entity = entity.Where(filterString, queryParas.ToArray());
                    entity = entity.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                }
                entity = entity.OrderBy(sort);
                entity = Utility.Pager<T_OA_LICENSEUSER>(entity, pageIndex, pageSize, ref pageCount);
                    return entity;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("证照管理LicenseBorrowBll-GetLicenseQueryWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw ex;
            }
        }
        
        /// <summary>
        /// 根据ID获取证照外借记录
        /// </summary>
        /// <param name="licenseID"></param>
        /// <returns></returns>
        public T_OA_LICENSEUSER GetLicenseById(string licenseID)
        {
            try
            {
                //SMT_OA_EFModelContext<T_OA_LICENSEUSER> context = new SMT_OA_EFModelContext<T_OA_LICENSEUSER>();
                var entity = (from q in dal.GetObjects().Include("T_OA_LICENSEMASTER")
                              select q).FirstOrDefault(s => s.LICENSEUSERID == licenseID);
                //var entity = dal.GetTable().FirstOrDefault(s => s.LICENSEUSERID == licenseID);
                return entity != null ? entity : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("证照管理LicenseBorrowBll-GetLicenseQueryWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 新增证照外借申请
        /// </summary>
        /// <param name="licenseObj"></param>
        /// <returns></returns>
        public bool AddLicenseBorrow(T_OA_LICENSEUSER licenseObj)
        {
            try
            {
                licenseObj.T_OA_LICENSEMASTER = dal.GetObjectByEntityKey(licenseObj.T_OA_LICENSEMASTER.EntityKey) as T_OA_LICENSEMASTER;
                //context.AddObject("T_OA_LICENSEUSER", licenseObj);
                int k = dal.Add(licenseObj);
                if (k > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("证照管理LicenseBorrowBll-AddLicenseBorrow" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                //throw (ex);
            }
        }

        /// <summary>
        /// 修改证照外借申请
        /// </summary>
        /// <param name="licenseObj"></param>
        /// <returns></returns>
        public bool UpdateLicenseBorrow(T_OA_LICENSEUSER licenseObj)
        {
            try
            {
                
                T_OA_LICENSEUSER entity = dal.GetObjectByEntityKey(licenseObj.EntityKey) as T_OA_LICENSEUSER;
                entity.T_OA_LICENSEMASTER = dal.GetObjectByEntityKey(licenseObj.T_OA_LICENSEMASTER.EntityKey) as T_OA_LICENSEMASTER;
                //context.ApplyPropertyChanges(licenseObj.EntityKey.EntitySetName, licenseObj);
                int i = dal.Update(licenseObj);
                if (i < 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("证照管理LicenseBorrowBll-UpdateLicenseBorrow" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        /// <summary>
        /// 删除证照外借申请
        /// </summary>
        /// <param name="licenseID"></param>
        /// <returns></returns>
        public bool DeleteLicenseBorrow(string[] licenseID)
        {
            try
            {
                bool result = false;
                var entity = from ent in dal.GetTable().ToList()
                              where licenseID.Contains(ent.LICENSEUSERID)
                              && ent.CHECKSTATE != ((int)CheckStates.Approving).ToString() && ent.CHECKSTATE != ((int)CheckStates.Approved).ToString()
                              select ent;
                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        dal.DeleteFromContext(h);
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                        result = true;
                }     
                
                return result;
            }
            catch (Exception ex)
            {
                //return false;
                throw (ex);
            }
        }

        /// <summary>
        /// 判断证照是否已外借
        /// </summary>
        /// <param name="licenseID"></param>
        /// <returns></returns>
        public bool IsBorrowed(string licenseID)
        {
            var entity = from q in dal.GetObjects<T_OA_LICENSEMASTER>()
                         where q.LICENSEMASTERID == licenseID
                         && q.LENDFLAG == "0"
                         select q;
            return entity.Count() > 0 ? true : false;
        }

        /// <summary>
        /// 选择证照 增加翻页功能
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public EntityObject[] GetLookupData(Dictionary<string, string> args, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {           
            IQueryable<T_OA_LICENSEMASTER> ents = from q in dal.GetObjects<T_OA_LICENSEMASTER>()
                                                  where q.LENDFLAG=="0"
                                                  select q;
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAORGANLICEN");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            ents = Utility.Pager<T_OA_LICENSEMASTER>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToArray() : null; 
        }
    }
}
