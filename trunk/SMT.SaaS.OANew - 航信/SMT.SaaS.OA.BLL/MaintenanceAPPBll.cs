using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Data.Objects;
using System.Linq.Dynamic;
namespace SMT.SaaS.OA.BLL
{
    public class MaintenanceAPPBll : BaseBll<T_OA_MAINTENANCEAPP>
    {
        //平台审核 进入
        public T_OA_MAINTENANCEAPP Get_VMApp(string id)
        {
            var q = from ent in dal.GetObjects<T_OA_MAINTENANCEAPP>().Include("T_OA_VEHICLE")
                    where ent.MAINTENANCEAPPID == id select ent;
            if (q.Count() > 0)
                return q.ToList()[0];
            else
                return null;
        }
        //审核通过 的申请单
        public List<T_OA_MAINTENANCEAPP> Get_VMAppChecked(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId)
        {
            DateTime d = ((DateTime)paras[0]).Date;
            DateTime d2 = ((DateTime)paras[1]).Date.AddDays(1);
            string assetID = (string)paras[2].ToString();

            var q = from ent in dal.GetObjects().Include("T_OA_VEHICLE")
                    where ent.CHECKSTATE == "2" && ent.REPAIRDATE >= d && ent.REPAIRDATE < d2 && ent.T_OA_VEHICLE.ASSETID == assetID
                    select ent;

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_MAINTENANCEAPP");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_MAINTENANCEAPP>(q, pageIndex, pageSize, ref pageCount);
            if (q != null && q.Count() > 0)
            {
                return q.ToList();
            }
            return null;
        }
        public IEnumerable<T_OA_MAINTENANCEAPP> GetInfoList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var q = from ent in dal.GetObjects().Include("T_OA_VEHICLE")
                    select ent;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    q = q.ToList().Where(g => guidStringList.Contains(g.MAINTENANCEAPPID)).AsQueryable();
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
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "OAVEHICLEMAINTENANCE");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_MAINTENANCEAPP>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }

        public bool AddInfo(T_OA_MAINTENANCEAPP ApprovalInfo)
        {
            MaintenanceAPPDal maDal = new MaintenanceAPPDal();
            return maDal.AddMaintenanceApp(ApprovalInfo);
        }
        public bool DeleteInfo(T_OA_MAINTENANCEAPP ApprovalInfo)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.MAINTENANCEAPPID == ApprovalInfo.MAINTENANCEAPPID
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
                return false;
                throw (ex);
            }
        }
        public int UpdateInfo(T_OA_MAINTENANCEAPP ApprovalInfo)
        {
            MaintenanceAPPDal maDal = new MaintenanceAPPDal();
            return maDal.UpdateMaintenanceApp(ApprovalInfo) == true ? 1 : -1;
        }
        #region 维修记录
        //平台审核 进入
        public T_OA_MAINTENANCERECORD Get_VMRecord(string id)
        {
            var q = from ent in dal.GetObjects<T_OA_MAINTENANCERECORD>().Include("T_OA_MAINTENANCEAPP.T_OA_VEHICLE")
                    where ent.MAINTENANCERECORDID == id select ent;
            if (q.Count() > 0)
                return q.ToList()[0];
            else
                return null;
        }
       
        public IEnumerable<T_OA_MAINTENANCERECORD> Get_VMRecords(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var q = from ent in dal.GetObjects<T_OA_MAINTENANCERECORD>().Include("T_OA_MAINTENANCEAPP.T_OA_VEHICLE")
                    select ent;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    q = q.ToList().Where(g => guidStringList.Contains(g.MAINTENANCERECORDID)).AsQueryable();
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
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_MAINTENANCERECORD");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_MAINTENANCERECORD>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
                return q.ToList();
            return null;
        }
        //修改维修记录
        public int Upd_VMRecord(T_OA_MAINTENANCERECORD ApprovalInfo)
        {
            MaintenanceAPPDal maDal = new MaintenanceAPPDal();
            return maDal.Upd_VMRecord(ApprovalInfo);
        }
        //添加维修记录
        public int Add_VMRecord(T_OA_MAINTENANCERECORD ApprovalInfo)
        {
            MaintenanceAPPDal maDal = new MaintenanceAPPDal();
            return maDal.Add_VMRecord(ApprovalInfo);
        }
        public int Del_VMRecord(List<T_OA_MAINTENANCERECORD> lst)
        {
            int n = 0;
            try
            {
                CommDaL<T_OA_MAINTENANCERECORD> dal1 = new CommDaL<T_OA_MAINTENANCERECORD>();
                foreach (T_OA_MAINTENANCERECORD info in lst)
                {
                    var entitys = (from ent in dal1.GetTable()
                                   where ent.MAINTENANCERECORDID == info.MAINTENANCERECORDID
                                   select ent);
                    if (entitys.Count() > 0)
                    {
                        var entity = entitys.FirstOrDefault();
                        dal.DeleteFromContext(entity);
                    }
                }
                n = dal.SaveContextChanges();
            }
            catch (Exception ex) {   throw (ex); }
            return n;
        }
        #endregion
    }
}
