using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;
using System.Data.Objects;
using System.Linq.Dynamic;
using SMT.SaaS.BLLCommonServices.FBServiceWS;

namespace SMT.SaaS.OA.BLL
{
    public class OrganManagementBll : BaseBll<T_OA_ORGANIZATION>
    {
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_OA_ORGANIZATION> GetOrganQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState,string userID)        
        {
            var entity = from p in dal.GetTable()
                         select p;
            if (flowInfoList != null)
            {

                entity = from a in entity.ToList().AsQueryable()
                         join l in flowInfoList on a.ORGANIZATIONID equals l.FormID
                         select a;
            }
            if (paras.Count > 0)
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_ORGANIZATION");
                if (!string.IsNullOrEmpty(filterString))
                {
                    entity = entity.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                }
            }
            if (!string.IsNullOrEmpty(checkState))
            {
                entity = entity.Where(s => s.CHECKSTATE == checkState);
            }
            entity = entity.OrderBy(sort);
            entity = Utility.Pager<T_OA_ORGANIZATION>(entity, pageIndex, pageSize, ref pageCount);
            return entity;
        }


        public IQueryable<T_OA_ORGANIZATION> GetOrganByOrganId(string organID)
        {
            //DateTime dt1 = DateTime.Now;
            var entity = from p in dal.GetTable()
                         where p.ORGANIZATIONID == organID
                         select p;
            //DateTime dt2 = DateTime.Now;
            //string a = (dt2 - dt1).TotalSeconds.ToString();
            return entity.Count() > 0 ? entity : null;
        }

        public IQueryable<T_OA_LICENSEMASTER> GetLicenseMasterListByOrganId(string organID)
        {
            //DateTime dt1 = DateTime.Now;
            var entity = from p in dal.GetObjects<T_OA_LICENSEMASTER>().Include("T_OA_ORGANIZATION")
                         where p.T_OA_ORGANIZATION.ORGCODE == organID
                         select p;
            //DateTime dt2 = DateTime.Now;
            //string a = (dt2 - dt1).TotalSeconds.ToString();
            return entity.Count() > 0 ? entity : null;
        }

        /// <summary>
        /// 新增机构
        /// </summary>
        /// <param name="organObj"></param>
        /// <returns></returns>
        public bool AddOrgan(T_OA_ORGANIZATION organObj, List<T_OA_LICENSEMASTER> licenseMasterList)
        {
            try
            {
                //int i = dal.Add(organObj);
                //OrganContext.AddObject("T_OA_ORGANIZATION", organObj);
                //int i = OrganContext.SaveChanges();
                int i = dal.Add(organObj);
                if (licenseMasterList.Count() < 1)
                {
                    return i > 0 ? true : false;
                }
                foreach (var h in licenseMasterList)
                {
                    T_OA_LICENSEMASTER ent = new T_OA_LICENSEMASTER();
                    CloneEntity(h, ent);

                    ent.T_OA_ORGANIZATION = null;
                    ent.T_OA_ORGANIZATIONReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_ORGANIZATION", "ORGCODE", h.T_OA_ORGANIZATION.ORGCODE);
                    //h.T_OA_ORGANIZATION = OrganContext.GetObjectByKey(h.T_OA_ORGANIZATION.EntityKey) as T_OA_ORGANIZATION;                   
                    //OrganContext.AddObject("T_OA_LICENSEMASTER", ent);
                    //LicenseManagementBll.Add(ent);
                    //int i = base.Add(ent);
                    int k=dal.Add(ent);
                    if (!(k > 0))
                        return false;


                }
                
                
                return true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// 修改机构
        /// </summary>
        /// <param name="organObj"></param>
        /// <returns></returns>
        public bool UpdateOrgan(T_OA_ORGANIZATION organObj,List<T_OA_LICENSEMASTER> licenseMasterList)
        {

            try
            { 
                var entity = from ent in dal.GetObjects()
                            where ent.ORGANIZATIONID == organObj.ORGANIZATIONID
                            select ent;

                if (entity.Count() > 0)
                {
                    organObj.UPDATEDATE = System.DateTime.Now;
                    var entitys = entity.FirstOrDefault();
                    CloneEntity(organObj, entitys);
                    dal.Update(entitys);
                  
                        //return true;
                    if (licenseMasterList.Count > 0)
                    {
                        //var entmasters = from ent in dal.GetObjects<T_OA_LICENSEMASTER>().Include("T_OA_ORGANIZATION")
                        //           where ent.T_OA_ORGANIZATION.ORGANIZATIONID == organObj.ORGANIZATIONID
                        //           select ent;
                        //if (entmasters.Count() > 0)
                        //{
                        //    entmasters.ToList().ForEach(
                        //        item => {
                        //            dal.DeleteFromContext(item);
                        //        }
                        //        );
                        //    dal.SaveContextChanges();
                        //}
                        string[] licenseNameArr = new string[licenseMasterList.Count];
                        int i = 0;
                        foreach (var h in licenseMasterList)
                        {
                            licenseNameArr[i] = h.LICENSENAME;
                            i++;
                            if (h.T_OA_ORGANIZATION != null)              //如果存在外键表信息
                            {
                                var ent = from q in dal.GetObjects<T_OA_LICENSEMASTER>()
                                            where q.T_OA_ORGANIZATION.ORGCODE == h.T_OA_ORGANIZATION.ORGCODE && q.LICENSENAME == h.LICENSENAME
                                            select q;
                                if (ent.Count() > 0)   //如果有记录，则直接跳过
                                {
                                    if (organObj.CHECKSTATE == ((int)CheckStates.Approved).ToString())
                                    {
                                        var ents = ent.FirstOrDefault();
                                        //CloneEntity(h, ents);
                                        ents.ISVALID = h.ISVALID;
                                        ents.UPDATEDATE = h.UPDATEDATE;
                                        ents.UPDATEUSERID = h.UPDATEUSERID;
                                        ents.UPDATEUSERNAME = h.UPDATEUSERNAME;
                                    }
                                       
                                }
                                else                  //如果不存在，则判断是新增还是删除
                                {
                                    //先新增
                                    //h.T_OA_ORGANIZATION = dal.GetObjects<T_OA_ORGANIZATION>().g.OrganContext.GetObjectByKey(h.T_OA_ORGANIZATION.EntityKey) as T_OA_ORGANIZATION;
                                    //OrganContext.AddObject("T_OA_LICENSEMASTER", h);
                                    dal.Add(h);
                                    //再删除
                                }
                            }
                            else
                            {
                                //OrganContext.AddObject("T_OA_LICENSEMASTER", h);
                                dal.Add(h);
                            }

                        }
                        var entdel = from q in dal.GetObjects<T_OA_LICENSEMASTER>().Include("T_OA_ORGANIZATION")
                                        where q.T_OA_ORGANIZATION != null && q.T_OA_ORGANIZATION.ORGCODE == organObj.ORGCODE && !licenseNameArr.Contains(q.LICENSENAME)
                                        select q;
                        if (entdel.Count() > 0)
                        {
                            foreach (var h in entdel)
                            {
                                dal.DeleteFromContext(h);
                            }
                            int m = dal.SaveContextChanges();
                            return m > 0 ? true : false;
                        }
                        return true;

                    }
                    
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        /// <summary>
        /// 删除机构
        /// </summary>
        /// <param name="OrganID"></param>
        /// <returns></returns>
        public bool DeleteOrgan(string[] OrganCode,ref bool FBControl)
        {
            try
            {    
                bool result = false;
                FBServiceClient FBClient = new FBServiceClient();
                
                var entity = from q in dal.GetObjects()
                             where OrganCode.Contains(q.ORGANIZATIONID)
                             select q;
                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        var ent = from q in dal.GetObjects<T_OA_LICENSEMASTER>()
                                  where q.T_OA_ORGANIZATION.ORGCODE == h.ORGCODE && q.ISVALID == "0"
                                  select q;
                        foreach (var a in ent.ToList())
                        {
                            dal.DeleteFromContext(a);                            
                        }

                        //ent.ToList().ForEach(s => OrganContext.DeleteObject(s));
                        //OrganContext.DeleteObject(h);
                        //if (OrganContext.SaveChanges() > 0)
                        //{
                        //    result = true;
                        //}
                    }
                    string[] StrFBMessage=FBClient.RemoveExtensionOrder(OrganCode);
                    if (StrFBMessage.Count() > 0)
                    {
                        FBControl = false;  //删除机构对应的
                    }
                    int i = dal.SaveContextChanges();
                    result = i > 0 ? true : false;
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        /// <summary>
        /// 机构是否存在
        /// </summary>
        /// <param name="organCode">机构代码</param>
        /// <returns></returns>
        public bool IsExistOrgan(string organCode)
        {
            var entity = from q in dal.GetTable()
                         where q.ORGCODE == organCode
                         select q;
            if (entity.Count() > 0)
            {
                return true;
            }
            return false;
        }
    }
}
