using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Data.Objects;
using System.Net;
using System.Linq.Dynamic;
using SMT.SaaS.BLLCommonServices;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    public class VehicleInfoManageBll : BaseBll<T_OA_VEHICLE>
    {
        //private SMT_OA_EFModelContext vehicleContext = new SMT_OA_EFModelContext();

        SMT.SaaS.BLLCommonServices.Utility UtilityClass = new SMT.SaaS.BLLCommonServices.Utility();
        public IQueryable<T_OA_VEHICLE> GetVehicleInfoList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            var q = from ent in dal.GetTable()
                    select ent;
            List<object> queryParas = new List<object>();
            if (paras != null)
            {
                queryParas.AddRange(paras);
            }

            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_VEHICLE"); //2010-6-22 ljx
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_VEHICLE>(q, pageIndex, pageSize, ref pageCount);

            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public IQueryable<T_OA_VEHICLE> GetVehicleInfoList()
        {
            var q = from ent in dal.GetTable()
                    where ent.VEHICLEFLAG == "0"
                    select ent;
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public IQueryable<T_OA_VEHICLE> GetCanUseVehicleInfoList()
        {
            VehicleDispatchManageBll vdmBll = new VehicleDispatchManageBll();
            List<string> canNotUseAssList = vdmBll.GetVehicleDispUsedAssList();
            var q = from ent in dal.GetTable()
                    select ent;
            if (canNotUseAssList != null && q.Count() > 0)
            {
                q = from ent in q
                    where canNotUseAssList.Contains(ent.ASSETID)
                    select ent;
                //q = q.ToList().Where(g => canNotUseAssList.Contains(g.ASSETID)).AsQueryable();
            }
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public bool AddVehicleInfo(T_OA_VEHICLE entity)
        {
            try
            {
                int add = dal.Add(entity);
                if (add > 0)
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
                Tracer.Debug("车辆管理VehicleInfoManageBll-AddVehicleInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        public bool DeleteVehicleInfo(string assetId)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.ASSETID == assetId
                               select ent);
                if (entitys.Count() > 0)
                {
                    // by ldx
                    // 2011-08-09
                    // 先得判断有没有停车卡信息，要是有停车卡信息的话先得把停车卡信息删除
                    // 得先删除停车卡
                    var items = from ent in dal.GetTable<T_OA_VEHICLECARD>()
                                where ent.T_OA_VEHICLE.ASSETID == assetId
                                select ent;
                    if (items.Count() > 0)
                    {
                        foreach (var item in items)
                        {
                            dal.Delete(item);
                        }
                    }

                    var entity = entitys.FirstOrDefault();
                    dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("车辆管理VehicleInfoManageBll-DeleteVehicleInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        public int UpdateVehicleInfo(T_OA_VEHICLE entity)
        {
            try
            {
                var workerCordList = from ent in dal.GetTable()
                                     where ent.ASSETID == entity.ASSETID
                                     select ent;
                if (workerCordList.Count() > 0)
                {
                    var workerCordInfo = workerCordList.FirstOrDefault();
                    workerCordInfo.COMPANYID = entity.COMPANYID;
                    workerCordInfo.UPDATEDATE = entity.UPDATEDATE;
                    workerCordInfo.UPDATEUSERID = entity.CREATEUSERID;
                    workerCordInfo.UPDATEUSERNAME = entity.UPDATEUSERNAME;
                    workerCordInfo.VEHICLEFLAG = entity.VEHICLEFLAG;

                    workerCordInfo.BUYDATE = entity.BUYDATE;
                    workerCordInfo.BUYPRICE = entity.BUYPRICE;
                    workerCordInfo.INITIALRANGE = entity.INITIALRANGE;
                    workerCordInfo.INTERVALRANGE = entity.INTERVALRANGE;
                    workerCordInfo.MAINTAINCOMPANY = entity.MAINTAINCOMPANY;
                    workerCordInfo.MAINTAINTEL = entity.MAINTAINTEL;
                    workerCordInfo.MAINTENANCECYCLE = entity.MAINTENANCECYCLE;
                    workerCordInfo.MAINTENANCEREMIND = entity.MAINTENANCEREMIND;
                    workerCordInfo.SEATQUANTITY = entity.SEATQUANTITY;
                    workerCordInfo.VEHICLEBRANDS = entity.VEHICLEBRANDS;
                    workerCordInfo.VEHICLETYPE = entity.VEHICLETYPE;
                    workerCordInfo.WEIGHT = entity.WEIGHT;

                    workerCordInfo.OWNERCOMPANYID = entity.OWNERCOMPANYID;
                    workerCordInfo.OWNERDEPARTMENTID = entity.OWNERDEPARTMENTID;
                    workerCordInfo.OWNERID = entity.OWNERID;
                    workerCordInfo.OWNERNAME = entity.OWNERNAME;
                    workerCordInfo.OWNERPOSTID = entity.OWNERPOSTID;

                    workerCordInfo.UPDATEDATE = System.DateTime.Now;
                    workerCordInfo.VEHICLEMODEL = entity.VEHICLEMODEL;
                    workerCordInfo.VIN = entity.VIN;
                    if (dal.Update(workerCordInfo) > 0)
                    {
                        return 1;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("车辆管理VehicleInfoManageBll-UpdateVehicleInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
                throw (ex);
            }
        }
        /// <summary>
        ///获取停车卡 修改界面
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public List<T_OA_VEHICLECARD> Get_VICard(string parentID)
        {
            var q = from ent in dal.GetObjects<T_OA_VEHICLECARD>()
                    where ent.T_OA_VEHICLE.ASSETID == parentID
                    select ent;
            return q.ToList();
        }
        //更新停车卡
        public int Upd_VICard(List<T_OA_VEHICLECARD> vddList,T_OA_VEHICLE vechile)
        {
            CommDaL<T_OA_VEHICLECARD> dal1 = new CommDaL<T_OA_VEHICLECARD>();
            int n = 0;
            
            var ents = from ent in dal.GetObjects<T_OA_VEHICLECARD>().Include("T_OA_VEHICLE")
                       where ent.T_OA_VEHICLE.ASSETID == vechile.ASSETID
                       select ent;
            dal.BeginTransaction();
            if (ents.Count() > 0)
            {
                if (vddList.Count > 0)
                {
                    vddList.ForEach(item =>
                    {
                        dal.DeleteFromContext(item);
                    });
                    dal.SaveContextChanges();//先保存删除记录
                }
            }
            foreach (T_OA_VEHICLECARD info in vddList)
            {
                info.T_OA_VEHICLE.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_VEHICLE", "ASSETID", info.T_OA_VEHICLE.ASSETID);
                info.T_OA_VEHICLE = dal.GetObjectByEntityKey(info.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                dal.AddToContext(info);

                
            }

            n = dal.SaveContextChanges();
            if (n > 0)
            {
                dal.CommitTransaction();
            }
            else
            {
                dal.RollbackTransaction();
            }
            return n;
        }
        //添加停车卡
        public int Add_VICard(List<T_OA_VEHICLECARD> vddList)
        {
            int n = 0;
            try
            {
                foreach (T_OA_VEHICLECARD obj in vddList)
                {
                    obj.T_OA_VEHICLE.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_VEHICLE", "ASSETID", obj.T_OA_VEHICLE.ASSETID);
                    obj.T_OA_VEHICLE = dal.GetObjectByEntityKey(obj.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                    dal.AddToContext(obj);
                }
                n = dal.SaveContextChanges();
                return n;
            }
            catch (Exception ex)
            {
                Tracer.Debug("车辆管理VehicleInfoManageBll-Add_VICard" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return n;
                throw (ex);
            }
        }
        //删除
        public int Del_VICard(List<string> ids)
        {
            int n = 0;
            try
            {
                CommDaL<T_OA_VEHICLECARD> dal1 = new CommDaL<T_OA_VEHICLECARD>();
                foreach (string id in ids)
                {
                    var entitys = (from ent in dal1.GetTable()
                                   where ent.VEHICLECARDID == id
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
                Tracer.Debug("车辆管理VehicleInfoManageBll-Del_VICard" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return n;
                throw (ex);
            }
        }
    }
}