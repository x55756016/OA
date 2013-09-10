using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Linq.Dynamic;
using SMT.SaaS.OA.DAL.Views;
using SMT.Foundation.Log;


namespace SMT.SaaS.OA.BLL
{
    public class HouseInfoIssuanceBll : BaseBll<T_OA_HOUSEINFOISSUANCE>
    {
        //private SMT_OA_EFModelContext context = new SMT_OA_EFModelContext();
        //private HouseListBll houseListBll = new HouseListBll();
        //private DistributeUserBll distributeBll = new DistributeUserBll();
        

        public List<T_OA_HOUSEINFOISSUANCE> GetIssuanceListById(string issuanceID)
        {
            try
            {
                var entity = from q in dal.GetTable()
                             where q.ISSUANCEID == issuanceID
                             select q;
                return entity.Count() > 0 ? entity.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源发布HouseListBll-GetIssuanceListById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public List<T_OA_HOUSEINFO> GetIssuanceHouseInfoListById(string issuanceID)
        {
            try
            {
                List<T_OA_HOUSEINFO> houseInfoList = new List<T_OA_HOUSEINFO>();

                var entity = from q in dal.GetObjects<T_OA_HOUSELIST>().Include("T_OA_HOUSEINFO")
                             where q.T_OA_HOUSEINFOISSUANCE.ISSUANCEID == issuanceID
                             select q;
                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        var ent = dal.GetObjects<T_OA_HOUSEINFO>().Where(s => s.HOUSEID == h.T_OA_HOUSEINFO.HOUSEID);
                        if (ent != null)
                        {
                            houseInfoList.Add(ent.ToList()[0]);
                        }
                    }
                }
                return houseInfoList.Count > 0 ? houseInfoList : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源发布HouseListBll-GetIssuanceHouseInfoListById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public List<T_OA_HOUSELIST> GetIssuanceHouseList(string issuanceID)
        {
            try
            {
                var entity = from q in dal.GetObjects<T_OA_HOUSELIST>().Include("T_OA_HOUSEINFO").Include("T_OA_HOUSEINFOISSUANCE")
                             where q.T_OA_HOUSEINFOISSUANCE.ISSUANCEID == issuanceID
                             select q;
                return entity.Count() > 0 ? entity.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源发布HouseListBll-GetIssuanceHouseList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public List<T_OA_DISTRIBUTEUSER> GetDistributeUserList(string issuanceID)
        {
            try
            {
                var entity = from q in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                             where q.FORMID == issuanceID
                             select q;
                return entity.Count() > 0 ? entity.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源发布HouseListBll-GetDistributeUserList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public IQueryable<T_OA_HOUSEINFOISSUANCE> GetIssuanceQueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState,string userID)
        {
            try
            {
                var ents = from q in dal.GetObjects()
                           //where q.CHECKSTATE == checkState
                           select q;
                if (!string.IsNullOrEmpty(checkState))
                {
                    ents = ents.Where(s => s.CHECKSTATE == checkState);
                }
                if (flowInfoList != null)
                {
                    ents = from a in ents.ToList().AsQueryable()
                           join l in flowInfoList on a.ISSUANCEID equals l.FormID
                           select a;
                }
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_HOUSEINFOISSUANCE");
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_HOUSEINFOISSUANCE>(ents, pageIndex, pageSize, ref pageCount);
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源发布HouseListBll-GetIssuanceQueryWithPaging" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public IQueryable<T_OA_HOUSEINFOISSUANCE> GetIssuanceForWebPart()
        {
            try
            {
                var ents = from q in dal.GetObjects()
                           where q.CHECKSTATE == ((int)CheckStates.Approved).ToString()
                           orderby q.UPDATEDATE descending
                           select q;
                return ents;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源发布HouseListBll-GetIssuanceForWebPart" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public bool AddHouseInfoIssuance(T_OA_HOUSEINFOISSUANCE issuanceObj, List<T_OA_HOUSELIST> houseListObj,List<T_OA_DISTRIBUTEUSER> DistributeListObj)
        {
            try
            {
                //issuanceObj.T_OA_HOUSELIST = context.GetObjectByKey(issuanceObj.T_OA_HOUSELIST.EntityKey)
                //issuanceObj.EntityKey
                //Utility.RefreshEntity(issuanceObj);
                T_OA_HOUSEINFOISSUANCE masterent = new T_OA_HOUSEINFOISSUANCE();
                Utility.CloneEntity(issuanceObj, masterent);                
                foreach (var h in houseListObj)
                {
                    //h.T_OA_HOUSEINFO = context.GetObjectByKey(h.T_OA_HOUSEINFO.EntityKey) as T_OA_HOUSEINFO;
                    //h.T_OA_HOUSEINFOISSUANCE = context.GetObjectByKey(h.T_OA_HOUSEINFOISSUANCE.EntityKey) as T_OA_HOUSEINFOISSUANCE;
                    T_OA_HOUSELIST houseListObjEnt = new T_OA_HOUSELIST();                   
                    Utility.CloneEntity(h, houseListObjEnt);

                    houseListObjEnt.T_OA_HOUSEINFOISSUANCEReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_HOUSEINFOISSUANCE","ISSUANCEID",h.T_OA_HOUSEINFOISSUANCE.ISSUANCEID);
                    houseListObjEnt.T_OA_HOUSEINFOReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_HOUSEINFO", "HOUSEID", h.T_OA_HOUSEINFO.HOUSEID);
                    //context.AddObject("T_OA_HOUSELIST", houseListObjEnt);
                    dal.AddToContext(houseListObjEnt);
                    
                }
                foreach (var h in DistributeListObj)
                {
                    //context.AddObject("T_OA_DISTRIBUTEUSER", h);
                    dal.AddToContext(h);
                }
                //context.AddObject("T_OA_HOUSEINFOISSUANCE", masterent);
                //int p=dal.Add(masterent);
                dal.AddToContext(masterent);
                int p = dal.SaveContextChanges();
                if (!(p > 0))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源发布HouseListBll-AddHouseInfoIssuance" + System.DateTime.Now.ToString() + " " + ex.ToString());
                //throw (ex);
                return false;
            }
        }

        /// <summary>
        /// 修改房源发布信息  
        /// 旧版本在审核时会把所有的房源列表删除 
        /// SubmitFlag 如果为审核状态只对发布表的状态进行改变 
        /// </summary>
        /// <param name="issuanceObj">房源发布</param>
        /// <param name="houseListObj">房源列表</param>
        /// <param name="DistributeListObj">发布对象</param>
        /// <param name="SubmitFlag">审核状态</param>
        /// <returns></returns>
        public bool UpdateHouseInfoIssuance(T_OA_HOUSEINFOISSUANCE issuanceObj, List<T_OA_HOUSELIST> houseListObj, List<T_OA_DISTRIBUTEUSER> DistributeListObj,bool SubmitFlag)
        {
            try
            {
                this.BeginTransaction();
                int i = 0;
                issuanceObj.UPDATEDATE = System.DateTime.Now;
                var entity = dal.GetObjects().Where(s => s.ISSUANCEID == issuanceObj.ISSUANCEID).FirstOrDefault();
                //var entity = dal.GetTable().Where(s => s.ISSUANCEID == issuanceObj.ISSUANCEID).FirstOrDefault();
                if (entity != null)
                {
                    CloneEntity(issuanceObj, entity);

                    //if (dal.Update(entity) == -1)
                    //{
                    //    return false;
                    //}
                }
                i =dal.Update(entity);
                //先删除houselist
                if (!SubmitFlag)
                {
                    var ent = dal.GetObjects<T_OA_HOUSELIST>().Where(s => s.T_OA_HOUSEINFOISSUANCE.ISSUANCEID == entity.ISSUANCEID);
                    //var ent = houseListBll.dal.GetTable().Where(s => s.T_OA_HOUSEINFOISSUANCE.ISSUANCEID == issuanceObj.ISSUANCEID);
                    if (ent != null)
                    {
                        foreach (var h in ent)
                        {
                            dal.DeleteFromContext(h);
                            
                        }
                        
                    }
                    //再插入houselist
                    foreach (var h in houseListObj)
                    {
                        T_OA_HOUSELIST houseListObjEnt = new T_OA_HOUSELIST();
                        Utility.CloneEntity(h, houseListObjEnt);
                        houseListObjEnt.HOUSELISTID = h.HOUSELISTID;
                        houseListObjEnt.T_OA_HOUSEINFOISSUANCEReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_HOUSEINFOISSUANCE", "ISSUANCEID", h.T_OA_HOUSEINFOISSUANCE.ISSUANCEID);
                        houseListObjEnt.T_OA_HOUSEINFOReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_HOUSEINFO", "HOUSEID", h.T_OA_HOUSEINFO.HOUSEID);
                        //context.AddObject("T_OA_HOUSELIST", houseListObjEnt);
                        //dal.Add(houseListObjEnt);
                        dal.AddToContext(houseListObjEnt);
                        
                    }
                    //先删除distributelist
                    var entdis = dal.GetObjects<T_OA_DISTRIBUTEUSER>().Where(s => s.FORMID == entity.ISSUANCEID);
                    //var entdis = distributeBll.GetTable().Where(s => s.FORMID == issuanceObj.ISSUANCEID);
                    if (entdis != null)
                    {
                        foreach (var h in entdis)
                        {
                            dal.DeleteFromContext(h);                           
                        }
                        
                    }
                    //再插入distributelist
                    foreach (var h in DistributeListObj)
                    {
                        //context.AddObject("T_OA_DISTRIBUTEUSER", h);
                        //dal.Add(h);
                        dal.AddToContext(h);

                    }
                    i = dal.SaveContextChanges();
                    
                }

                if (i > 0)
                {
                    this.CommitTransaction();
                    return true;
                }
                else
                {
                    this.RollbackTransaction();
                    return false;
                }
                
                
            }
            catch (Exception ex)
            {
                this.RollbackTransaction();
                Tracer.Debug("房源发布HouseListBll-UpdateHouseInfoIssuance" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;                
                throw (ex);
            }
            
        }

        public bool DeleteHouseInfoIssuance(string[] issuanceID)
        {
            try
            {
                var entity = from q in dal.GetObjects().ToList()
                             where issuanceID.Contains(q.ISSUANCEID)
                             select q;
                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        //删除houselist
                        var ent = from p in dal.GetObjects<T_OA_HOUSELIST>()
                                  where h.ISSUANCEID == p.T_OA_HOUSEINFOISSUANCE.ISSUANCEID
                                  select p;
                        foreach (var k in ent)
                        {
                            dal.DeleteFromContext(k);
                        }        
                        //删除distributelist
                        var entdis = from p in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                                     where h.ISSUANCEID == p.FORMID
                                     select p;
                        foreach (var k in entdis)
                        {
                            dal.DeleteFromContext(k);

                        }  
                        //删除houseissuanceinfo
                        //context.DeleteObject(h);
                        dal.DeleteFromContext(h);
                        
                    }
                    int i = dal.SaveContextChanges();
                    return i > 0 ? true : false;
                }
                return false;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源发布HouseListBll-DeleteHouseInfoIssuance" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
            
        }

        public bool UpdateIssuance(T_OA_HOUSEINFOISSUANCE issuanceObj)
        {
            try
            {
                var entity = dal.GetTable().Where(s => s.ISSUANCEID == issuanceObj.ISSUANCEID).FirstOrDefault();
                if (entity != null)
                {
                    CloneEntity(issuanceObj, entity);
                    if (dal.Update(entity) == 1)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源发布HouseListBll-UpdateIssuance" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
    }

    
}
