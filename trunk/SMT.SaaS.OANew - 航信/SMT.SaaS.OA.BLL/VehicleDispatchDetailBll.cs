using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Data.Objects;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    public class VehicleDispatchDetailBll : BaseBll<T_OA_VEHICLEDISPATCHDETAIL>
    {
        public int AddVehicleDispatchDetailList(List<T_OA_VEHICLEDISPATCHDETAIL> selectInfoList)
        {
            try
            {
                VehicleDispatchDetailDal vddDal = new VehicleDispatchDetailDal();
                foreach (T_OA_VEHICLEDISPATCHDETAIL obj in selectInfoList)
                {
                    if (vddDal.AddVehicleDispatchDetail(obj) == -1)
                    {
                        return -1;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("车辆管理VehicleDispatchDetailBll-AddVehicleDispatchDetailList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public int AddVehicleDispatchDetail(T_OA_VEHICLEDISPATCHDETAIL selectInfo)
        {
            try
            {
                VehicleDispatchDetailDal vddDal = new VehicleDispatchDetailDal();
                if (vddDal.AddVehicleDispatchDetail(selectInfo) == -1)
                {
                    return -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("车辆管理VehicleDispatchDetailBll-AddVehicleDispatchDetail" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        public int DeleteVehicleDispatchDetailByDiaspatchId(string vehicleDispatchId)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.T_OA_VEHICLEDISPATCH.VEHICLEDISPATCHID == vehicleDispatchId
                               select ent);
                if (entitys.Count() > 0)
                {
                    foreach (T_OA_VEHICLEDISPATCHDETAIL obj in entitys)
                    {
                        dal.DeleteFromContext(obj);
                    }
                    return dal.SaveContextChanges() > 0 ? 1 : -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("车辆管理VehicleDispatchDetailBll-DeleteVehicleDispatchDetailByDiaspatchId" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
                throw (ex);
            }
        }
        //删除派车明细
        public int Del_VDDetail(string[] ids)
        {
            try
            {
                string str = "";
                foreach (var id in ids)
                    str = "'"+id + "',";
                str = str.Remove(str.Length - 1);

                var entitys = (from ent in dal.GetTable()
                               where ent.T_OA_VEHICLEUSEAPP.VEHICLEUSEAPPID.Contains(str)
                               select ent);
                if (entitys.Count() > 0)
                {
                    foreach (T_OA_VEHICLEDISPATCHDETAIL obj in entitys)
                        dal.DeleteFromContext(obj);

                    return dal.SaveContextChanges() >0 ? 1 : -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("车辆管理VehicleDispatchDetailBll-Del_VDDetail" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
                throw (ex);
            }
        }
       
        //通过派车单获取明细
        public IQueryable<T_OA_VEHICLEDISPATCHDETAIL> GetDetailByDispatchId(string vehicleDispatchId)
        {
            var entitys = (from ent in dal.GetObjects<T_OA_VEHICLEDISPATCHDETAIL>().Include("T_OA_VEHICLEUSEAPP")
                           where ent.T_OA_VEHICLEDISPATCH.VEHICLEDISPATCHID == vehicleDispatchId
                           select ent);
            if (entitys.Count() > 0)
            {
                return entitys;
            }
            return null;
        }
        //通过派车单获取明细
        public List<T_OA_VEHICLEUSEAPP> Get_ByParentID(string parentID)
        {
            var en = from ent in dal.GetObjects<T_OA_VEHICLEUSEAPP>()
                     join s in dal.GetObjects<T_OA_VEHICLEDISPATCHDETAIL>()
                     on ent.VEHICLEUSEAPPID equals s.T_OA_VEHICLEUSEAPP.VEHICLEUSEAPPID
                     where s.T_OA_VEHICLEDISPATCH.VEHICLEDISPATCHID == parentID
                     select ent;          
            return en.ToList();
        }       
        public List<string> GetCanNotUseApp()
        {
            var entitys = from ent in dal.GetTable()
                          where ent.T_OA_VEHICLEDISPATCH.CHECKSTATE == "1" || ent.T_OA_VEHICLEDISPATCH.CHECKSTATE == "2"
                          select ent.T_OA_VEHICLEUSEAPP.VEHICLEUSEAPPID;
            if (entitys.Count() > 0)
            {
                return entitys.ToList();
            }
            return null;
        }
    }
}