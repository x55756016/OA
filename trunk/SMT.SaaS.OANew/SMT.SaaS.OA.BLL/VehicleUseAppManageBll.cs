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
    /// 用车申请
    /// </summary>
    public class VehicleUseAppManageBll : BaseBll<T_OA_VEHICLEUSEAPP>
    {
        //private VehicleUseAppManageDal VehicleUseApp = new VehicleUseAppManageDal();
      
        //获取用车申请单
        public IQueryable<T_OA_VEHICLEUSEAPP> GetVehicleInfoList(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var q = from ent in dal.GetTable()
                    select ent;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    q = from ent in q
                        where guidStringList.Contains(ent.VEHICLEUSEAPPID)
                        select ent;
                    //q = q.ToList().Where(g => guidStringList.Contains(g.VEHICLEUSEAPPID)).AsQueryable();
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
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "OAVEHICLEAPP");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_VEHICLEUSEAPP>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }
        //平台审核 进入
        public T_OA_VEHICLEUSEAPP Get_VehicleUseApp(string id)
        {
            var q = from ent in dal.GetObjects() where ent.VEHICLEUSEAPPID == id  select ent;
            if (q.Count() > 0)
                return q.ToList()[0];
            else
                return null;
        }
        //获取申请用车已通过的信息
        public IQueryable<T_OA_VEHICLEUSEAPP> GetCanUseVehicleUseAppInfoList(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userId, string checkState)
        {
            VehicleDispatchDetailBll vddBll = new VehicleDispatchDetailBll();
           
            DateTime d = ((DateTime)paras[0]).Date;
            DateTime d2 = ((DateTime)paras[1]).Date.AddDays(1);

           // List<string> canNotUseList = vddBll.GetCanNotUseApp();
            var q = from ent in dal.GetTable()
                    where ent.CHECKSTATE == checkState && ent.STARTTIME >= d && ent.STARTTIME < d2
                    select ent;
            //if (canNotUseList != null)
            //{
            //    q = q.ToList().Where(e => !canNotUseList.Contains(e.VEHICLEUSEAPPID)).AsQueryable();
            //}
            //List<object> queryParas = new List<object>();
            //queryParas.AddRange(paras);
            ////SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_VEHICLEUSEAPP");
            //if (!string.IsNullOrEmpty(filterString))
            //{
            //    q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            //}
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_VEHICLEUSEAPP>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public bool AddVehicleUseApp(T_OA_VEHICLEUSEAPP entity)
        {
            try
            {
                int i = dal.Add(entity);
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
                Tracer.Debug("车辆管理VehicleUseAppManageBll-AddVehicleUseApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }

        public bool DeleteVehicleUseApp(string vehicleUseAppId)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.VEHICLEUSEAPPID == vehicleUseAppId
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
                Tracer.Debug("车辆管理VehicleUseAppManageBll-DeleteVehicleUseApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }



        public int UpdateVehicleUseApp(T_OA_VEHICLEUSEAPP entity)
        {
            try
            {
                var workerCordList = from ent in dal.GetTable()
                                     where ent.VEHICLEUSEAPPID == entity.VEHICLEUSEAPPID
                                     select ent;

                if (workerCordList.Count() > 0)
                {
                    var workerCordInfo = workerCordList.FirstOrDefault();
                    workerCordInfo.CHECKSTATE = entity.CHECKSTATE;
                    //workerCordInfo.CREATEDATE = entity.CREATEDATE;
                    workerCordInfo.UPDATEDATE = System.DateTime.Now;
                    workerCordInfo.UPDATEUSERID = entity.UPDATEUSERID;
                    workerCordInfo.CONTENT = entity.CONTENT;
                    //workerCordInfo.CREATECOMPANYID = entity.VEHICLEID;
                    //workerCordInfo.CREATEDEPARTMENTID = entity.VEHICLEMODEL;
                    //workerCordInfo.CREATEPOSTID = entity.VIN;
                    //workerCordInfo.CREATEUSERID = entity.VIN;
                    workerCordInfo.DEPARTMENTID = entity.DEPARTMENTID;
                    workerCordInfo.ENDTIME = entity.ENDTIME;
                    workerCordInfo.NUM = entity.NUM;
                    workerCordInfo.ROUTE = entity.ROUTE;
                    workerCordInfo.STARTTIME = entity.STARTTIME;
                    workerCordInfo.TEL = entity.TEL;
                    //workerCordInfo.VEHICLEUSEAPPID = entity.VEHICLEUSEAPPID;
                    if (dal.Update(workerCordInfo) != 1)
                    {
                        return -1;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("车辆管理VehicleUseAppManageBll-UpdateVehicleUseApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
                throw (ex);
            }
        }
        public T_OA_VEHICLEUSEAPP GetVehicleUseAppById(string entityId)
        {
            
            var ents = from a in dal.GetTable()
                        where a.VEHICLEUSEAPPID == entityId
                        select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            
        }
    }
}
