using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Data.Objects;
using System.Linq.Dynamic;
using SMT.Foundation.Log;
namespace SMT.SaaS.OA.BLL
{
    /// <summary>
    /// 派车管理
    /// </summary>
    public class VehicleDispatchManageBll : BaseBll<T_OA_VEHICLEDISPATCH>
    {
        //private SMT_OA_EFModelContext BenefitsAdministration = new SMT_OA_EFModelContext();
        //private VehicleDispatchManageDal VehicleDispatch = new VehicleDispatchManageDal();
        #region 派车管理

        //获取派车信息
        public IQueryable<T_OA_VEHICLEDISPATCH> Gets_VDInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var q = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCH>().Include("T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEUSEAPP")

                    select ent;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    //q = from ent in q
                    //    where guidStringList.Contains(ent.VEHICLEDISPATCHID)
                    //    select ent;
                    q = q.ToList().Where(g => guidStringList.Contains(g.VEHICLEDISPATCHID)).AsQueryable();
                }
            }
            else//创建人
            {
                q = q.Where(ent => ent.OWNERID == userId);
                if (checkState != "5")
                {
                    q = q.Where(ent => ent.CHECKSTATE == checkState);
                }
            }
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "OAVEHICLEDISPATCH");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_VEHICLEDISPATCH>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q.OrderBy(x => x.STARTTIME);
            }
            return null;
        }
        /// <summary>
        ///调度记录用 添加  ||平台审核 进入
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public List<T_OA_VEHICLEDISPATCH> Get_VDInfo(string id)
        {
            var q = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCH>().Include("T_OA_VEHICLE").Include("T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEUSEAPP")
                    where ent.VEHICLEDISPATCHID == id
                    select ent;
            var q1 = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCH>().Include("T_OA_VEHICLE")
                    //where ent.VEHICLEDISPATCHID == id
                    select ent;
            var q2 = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCH>().Include("T_OA_VEHICLE").Include("T_OA_VEHICLEUSEAPP")
                    //where ent.VEHICLEDISPATCHID == id
                    select ent;
            var q3 = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCH>().Include("T_OA_VEHICLEUSEAPP")
                     //where ent.VEHICLEDISPATCHID == id
                     select ent;
            var q4 = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCH>().Include("T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEUSEAPP")
                     where ent.VEHICLEDISPATCHID == id
                     select ent;
            return q.ToList();
        }
        //调度记录 修改
        public List<T_OA_VEHICLEDISPATCHRECORD> Get_VDRecord(string id)
        {
            var q = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCHRECORD>().Include("T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.T_OA_VEHICLE")
                    where ent.VEHICLEDISPATCHRECORDID == id
                    select ent;

            return q.ToList();
        }
        //获取审核通过的派车单
        public List<T_OA_VEHICLEDISPATCH> Gets_VDChecked(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userId, string checkState)
        {
            string assetID = (string)paras[0];
            DateTime d = ((DateTime)paras[1]).Date;
            DateTime d2 = d.AddDays(1);
            var q = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCH>()
                    where ent.CHECKSTATE == checkState && ent.T_OA_VEHICLE.ASSETID == assetID && ent.STARTTIME >= d //&& ent.STARTTIME < d2
                    select ent;
            return q.ToList();
        }
        //获取已经派车的申请单
        public List<T_OA_VEHICLEDISPATCH> Gets_VDAndDetail(string sort, string filterString, IList<object> paras)
        {
            var q = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCH>().Include("T_OA_VEHICLEDISPATCHDETAIL")
                    where ent.VEHICLEDISPATCHID == paras[0].ToString()
                    select ent;
            return q.ToList();
        }
        public int AddVehicleDispatch(T_OA_VEHICLEDISPATCH entity)
        {
            try
            {
                VehicleDispatchManageDal vdmDal = new VehicleDispatchManageDal();
                return vdmDal.AddVehicleDispatch(entity);
            }
            catch (Exception ex)
            {
                Tracer.Debug("派车管理VehicleDispatchManageBll-AddVehicleDispatch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        //添加派车单 及明细
        public int AddVehicleDispatchAndDetail(T_OA_VEHICLEDISPATCH entity, List<T_OA_VEHICLEDISPATCHDETAIL> vddList)
        {
            try
            {
                //VehicleDispatchManageDal vdmDal = new VehicleDispatchManageDal();

                //return vdmDal.AddVehicleDispatchAndDetail(entity, vddList);
                try
                {
                    BeginTransaction();
                    foreach (T_OA_VEHICLEDISPATCHDETAIL obj in vddList)
                    {

                        Utility.RefreshEntity(obj);
                        entity.T_OA_VEHICLEDISPATCHDETAIL.Add(obj);
                    }
                    bool aa = Add(entity);
                    //int i = dal.SaveContextChanges();
                    if (!aa)
                    {
                        RollbackTransaction();
                        return -1;
                    }
                    else
                    {
                        CommitTransaction();
                        return 1;
                    }
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    throw (ex);
                }

            }
            catch (Exception ex)
            {
                Tracer.Debug("派车管理VehicleDispatchManageBll-AddVehicleDispatchAndDetail" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        public bool DeleteVehicleDispatch(string vehicleUseAppId)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.VEHICLEDISPATCHID == vehicleUseAppId
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("派车管理VehicleDispatchManageBll-DeleteVehicleDispatch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        public int UpdateVehicleDispatch(T_OA_VEHICLEDISPATCH entity)
        {
            try
            {
                VehicleDispatchManageDal vdmDal = new VehicleDispatchManageDal();
                return vdmDal.UpdateVehicleDispatch(entity);
            }
            catch (Exception ex)
            {
                Tracer.Debug("派车管理VehicleDispatchManageBll-UpdateVehicleDispatch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public int UpdateVehicleDispatchAndDetail(T_OA_VEHICLEDISPATCH entity, List<T_OA_VEHICLEDISPATCHDETAIL> vddList)
        {
            try
            {
                
                //VehicleDispatchManageDal vdmDal = new VehicleDispatchManageDal();
                ////更新主表
                //return vdmDal.UpdateVehicleDispatchAndDetail(entity, vddList);

                BeginTransaction();

                ///////////////////
                int aaa = Update(entity);
                if (!(aaa > 0))
                {
                    RollbackTransaction();
                    return -1;
                }

                if (vddList == null || vddList.Count == 0)
                {
                    CommitTransaction();
                    return 1;
                }
                var ents = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCHDETAIL>().Include("T_OA_VEHICLEDISPATCH")
                            where ent.T_OA_VEHICLEDISPATCH.VEHICLEDISPATCHID == entity.VEHICLEDISPATCHID
                            select ent;
                if (ents.Count() > 0)
                {
                    ents.ToList().ForEach(item =>
                    {
                        dal.DeleteFromContext(item);
                    });
                    int k = dal.SaveContextChanges();

                }

                foreach (T_OA_VEHICLEDISPATCHDETAIL obj in vddList)
                {

                    Utility.RefreshEntity(obj);
                    //entity.T_OA_VEHICLEDISPATCHDETAIL.Add(obj);
                    dal.AddToContext(obj);
                }

                //bool aa = Add(entity);
                //int aa = Update(entity);
                int aa = dal.SaveContextChanges();
                if (aa <= 0)
                {
                    RollbackTransaction();
                    return -1;
                }
                else
                {
                    CommitTransaction();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("派车管理VehicleDispatchManageBll-UpdateVehicleDispatchAndDetail" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public List<string> GetVehicleDispUsedAssList()
        {
            var q = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCH>().Include("T_OA_VEHICLE")
                    where ent.CHECKSTATE == "1" || ent.CHECKSTATE == "2"
                    orderby ent.STARTTIME
                    select ent.T_OA_VEHICLE.ASSETID;
            if (q.Count() > 0)
            {
                return q.ToList();
            }
            return null;
        }

        public T_OA_VEHICLEDISPATCH GetVehicleDispatchById(string entityId)
        {
            
            
            var ents = from a in dal.GetTable()
                        where a.VEHICLEDISPATCHID == entityId
                        select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            
        }
        #endregion 派车管理

        #region 调度记录

        //获取调度记录信息
        public IQueryable<T_OA_VEHICLEDISPATCHRECORD> Gets_VDRecord(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var q = from ent in dal.GetObjects<T_OA_VEHICLEDISPATCHRECORD>().Include("T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEDISPATCH.T_OA_VEHICLE") select ent;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    q = from ent in q
                        where guidStringList.Contains(ent.VEHICLEDISPATCHRECORDID)
                        select ent;
                }
            }
            else//创建人
            {
                q = q.Where(ent => ent.OWNERID == userId);
                if (checkState != "5")
                {
                    q = q.Where(ent => ent.CHECKSTATE == checkState);
                }
            }
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_VEHICLEDISPATCHRECORD");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_VEHICLEDISPATCHRECORD>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q.OrderBy(x => x.STARTTIME);
            }
            return null;
        }
        //调度更新记录
        public int Upd_VDRecord(List<T_OA_VEHICLEDISPATCHRECORD> vddList)
        {
            CommDaL<T_OA_VEHICLEDISPATCHRECORD> dal1 = new CommDaL<T_OA_VEHICLEDISPATCHRECORD>();
            int n = 0;
            foreach (T_OA_VEHICLEDISPATCHRECORD info in vddList)
            {
                var ds = from ent in dal1.GetObjects()
                         where ent.VEHICLEDISPATCHRECORDID == info.VEHICLEDISPATCHRECORDID
                         select ent;
                if (ds.Count() > 0)
                {
                    var d = ds.FirstOrDefault();
                    d.STARTTIME = info.STARTTIME;
                    d.ENDTIME = info.ENDTIME;
                    d.NUM = info.NUM;
                    d.ROUTE = info.ROUTE;
                    d.TEL = info.TEL;
                    d.FUEL = info.FUEL;
                    d.RANGE = info.RANGE;
                    d.CONTENT = info.CONTENT;
                    d.CHARGEMONEY = info.CHARGEMONEY;
                    d.ISCHARGE = info.ISCHARGE;
                    d.UPDATEUSERID = info.UPDATEUSERID;
                    d.UPDATEDATE = info.UPDATEDATE;
                    d.CHECKSTATE = info.CHECKSTATE;
                    n = dal1.Update(d);
                    //n = dal1.Update(d);
                    //dal.UpdateFromContext(d);
                }
            }
            //n = dal.SaveContextChanges();
            return n;
        }
        //添加调度记录
        public int Add_VDRecord(List<T_OA_VEHICLEDISPATCHRECORD> vddList)
        {
            try
            {
                VehicleDispatchManageDal vdmDal = new VehicleDispatchManageDal();

                int n = vdmDal.Add_VDRecord(vddList);
                return n;
            }
            catch (Exception ex)
            {
                Tracer.Debug("派车管理VehicleDispatchManageBll-Add_VDRecord" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        //删除
        public int Del_VDRecord(List<string> ids)
        {
            int n = 0;
            try
            {
                CommDaL<T_OA_VEHICLEDISPATCHRECORD> dal1 = new CommDaL<T_OA_VEHICLEDISPATCHRECORD>();
                foreach (string id in ids)
                {
                    var entitys = (from ent in dal1.GetTable()
                                   where ent.VEHICLEDISPATCHRECORDID == id
                                   select ent);
                    if (entitys.Count() > 0)
                    {
                        var entity = entitys.FirstOrDefault();
                        dal.DeleteFromContext(entity);
                    }

                }
                n = dal.SaveContextChanges();
                return n;
            }
            catch (Exception ex)
            {
                Tracer.Debug("派车管理VehicleDispatchManageBll-Del_VDRecord" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return n;
                throw (ex);
            }
        }
        #endregion 调度记录
    }
}
