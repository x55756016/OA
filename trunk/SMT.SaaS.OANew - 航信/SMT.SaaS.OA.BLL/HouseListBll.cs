using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Log;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.BLL
{
    public class HouseListBll : BaseBll<T_OA_HOUSELIST>    
    {
        //新增
        //private SMT_OA_EFModelContext context = new SMT_OA_EFModelContext();

        public bool AddHouseList(T_OA_HOUSELIST houseListObj)
        {
            try
            {
                //if (dal.Add(houseListObj) == 1)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
                houseListObj.T_OA_HOUSEINFO = dal.GetObjectByEntityKey(houseListObj.T_OA_HOUSEINFO.EntityKey) as T_OA_HOUSEINFO;
                houseListObj.T_OA_HOUSEINFOISSUANCE = dal.GetObjectByEntityKey(houseListObj.T_OA_HOUSEINFOISSUANCE.EntityKey) as T_OA_HOUSEINFOISSUANCE;
                int i = dal.Add(houseListObj); 
                if (i > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源列表HouseListBll-AddHouseList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }

        //更新
        public bool UpdateHouseList(T_OA_HOUSELIST houseListObj)
        {
            try
            {               
                //var entity = from q in dal.GetTable()
                //             where q.HOUSELISTID == houseListObj.HOUSELISTID
                //             select q;
                //if (entity.Count() > 0)
                //{

                //    var entitys = entity.FirstOrDefault();
                //    CloneEntity(houseListObj, entitys);
                //    if (dal.Update(entitys) == 1)
                //    {
                //        return true;
                //    }
                //}
                //return false;
                T_OA_HOUSELIST tmpobj = dal.GetObjectByEntityKey(houseListObj.EntityKey) as T_OA_HOUSELIST;
                tmpobj.T_OA_HOUSEINFO = dal.GetObjectByEntityKey(houseListObj.T_OA_HOUSEINFO.EntityKey) as T_OA_HOUSEINFO;
                tmpobj.T_OA_HOUSEINFOISSUANCE = dal.GetObjectByEntityKey(houseListObj.T_OA_HOUSEINFOISSUANCE.EntityKey) as T_OA_HOUSEINFOISSUANCE;
                //context.ApplyPropertyChanges(houseListObj.EntityKey.EntitySetName, houseListObj);
                int i = dal.Update(houseListObj);
                if (i > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("房源列表HouseListBll-UpdateHouseList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
    }
}
