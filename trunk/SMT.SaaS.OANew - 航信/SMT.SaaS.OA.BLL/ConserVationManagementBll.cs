using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Linq.Dynamic;

using System.Data.Objects.DataClasses;
using System.Reflection;
//using SMT.SaaS.OA.BLL.DataSource;
using SMT.SaaS.BLLCommonServices.EngineConfigWS;
namespace SMT.SaaS.OA.BLL
{
    public class ConserVationManagementBll : BaseBll<T_OA_CONSERVATION>
    {
        ConserVationManagementDal cvDal = new ConserVationManagementDal();
        //平台审核 进入
        public T_OA_CONSERVATION Get_VConserVation(string id)
        {
            var q = from ent in dal.GetObjects<T_OA_CONSERVATION>().Include("T_OA_VEHICLE")
                    where ent.CONSERVATIONID == id 
                    select ent;
            if (q.Count() > 0)
                return q.ToList()[0];
            else
                return null;
        }
        public IQueryable<T_OA_CONSERVATION> GetInfoList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            var q = from ent in dal.GetObjects<T_OA_CONSERVATION>().Include("T_OA_VEHICLE")
                    select ent;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                {
                    q = q.ToList().Where(g => guidStringList.Contains(g.CONSERVATIONID)).AsQueryable();
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
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "OACONSERVATION");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_CONSERVATION>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }
        //查询
        public IEnumerable<T_OA_CONSERVATION> Sel_VCCheckeds(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            DateTime d = ((DateTime)paras[0]).Date;
            DateTime d2 = ((DateTime)paras[1]).Date.AddDays(1);
            string assetID = (string)paras[2].ToString();

            var q = from ent in dal.GetObjects<T_OA_CONSERVATION>().Include("T_OA_VEHICLE")
                    where ent.CHECKSTATE == "2" && ent.REPAIRDATE >= d && ent.REPAIRDATE < d2 && ent.T_OA_VEHICLE.ASSETID == assetID
                    select ent;

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_CONSERVATION");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_CONSERVATION>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }
        //添加
        public bool AddInfo(T_OA_CONSERVATION ApprovalInfo)
        {
            return cvDal.AddConserVation(ApprovalInfo);
        }
        //删除
        public bool DeleteInfo(T_OA_CONSERVATION ApprovalInfo)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.CONSERVATIONID == ApprovalInfo.CONSERVATIONID
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
        //修改
        public int UpdateInfo(T_OA_CONSERVATION ApprovalInfo)
        {
            bool isSuccess = cvDal.UpdateConserVation(ApprovalInfo);
            if (isSuccess)
            {
                return 1;
            }
            return -1;
        }
        #region 保养记录

        //平台审核 进入
        public T_OA_CONSERVATIONRECORD Get_VCRecord(string id)
        {
            var q = from ent in dal.GetObjects<T_OA_CONSERVATIONRECORD>().Include("T_OA_CONSERVATION.T_OA_VEHICLE")
                    where ent.CONSERVATIONRECORDID == id
                    select ent;
            if (q.Count() > 0)
                return q.ToList()[0];
            else
                return null;
        }
        public IEnumerable<T_OA_CONSERVATIONRECORD> Get_VCRecords(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userId, List<string> guidStringList, string checkState)
        {
            VehicleInfoManageDal vimDal = new VehicleInfoManageDal();
            var q = from ent in dal.GetObjects<T_OA_CONSERVATIONRECORD>().Include("T_OA_CONSERVATION.T_OA_VEHICLE")
                    select ent;
            if (checkState == "4")//审批人
            {
                if (guidStringList != null)
                    q = q.ToList().Where(g => guidStringList.Contains(g.CONSERVATIONRECORDID)).AsQueryable();
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
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userId, "T_OA_CONSERVATIONRECORD");
            if (!string.IsNullOrEmpty(filterString))
            {
                q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
            }
            q = q.OrderBy(sort);
            q = Utility.Pager<T_OA_CONSERVATIONRECORD>(q, pageIndex, pageSize, ref pageCount);
            if (q.Count() > 0)
            {
                return q;
            }
            return null;
        }
        public int Add_VCRecord(T_OA_CONSERVATIONRECORD info)
        {
            return cvDal.Add_VCRecord(info);
        }
        public int Upd_VCRecord(T_OA_CONSERVATIONRECORD info)
        {
            return cvDal.Upd_VCRecord(info);
        }
        public int Del_VCRecords(List<T_OA_CONSERVATIONRECORD> lst)
        {
            int n = 0;
            try
            {
                foreach (T_OA_CONSERVATIONRECORD info in lst)
                {
                    var entitys = (from ent in dal.GetObjects<T_OA_CONSERVATIONRECORD>()
                                   where ent.CONSERVATIONRECORDID == info.CONSERVATIONRECORDID
                                   select ent);
                    if (entitys.Count() > 0)
                    {
                        var entity = entitys.FirstOrDefault();
                        //n += dal.Delete(entity);
                        dal.DeleteFromContext(entity);
                    }
                }
                n = dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                return 0;
                throw (ex);
            }
            return n;
        }


        #endregion
    }
}