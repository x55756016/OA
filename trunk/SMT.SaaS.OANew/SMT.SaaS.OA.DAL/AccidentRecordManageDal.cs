using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    public class AccidentRecordManageDal : CommDaL<T_OA_ACCIDENTRECORD>
    {
        public bool AddAccidentRecord(T_OA_ACCIDENTRECORD accidentRecordInfo)
        {
            try
            {
                accidentRecordInfo.T_OA_VEHICLE = base.GetObjectByEntityKey(accidentRecordInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                if (ExitAccidentRecord(accidentRecordInfo) == true)
                {
                    int j=base.Add(accidentRecordInfo);
                    int i = SaveContextChanges();
                    if (j > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        /// <summary>
        /// 判断是否存在事故记录
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool ExitAccidentRecord(T_OA_ACCIDENTRECORD info)
        {
            bool IsReturn = true;
            var ents = from ent in base.GetObjects<T_OA_ACCIDENTRECORD>().Include("T_OA_VEHICLE")
                       where ent.T_OA_VEHICLE.ASSETID == info.T_OA_VEHICLE.ASSETID && ent.ACCIDENTDATE == info.ACCIDENTDATE && ent.CONTENT == info.CONTENT && ent.OWNERID == info.OWNERID
                       select ent;
            if (ents.Count() > 0)
            {
                IsReturn = false;
            }
            return IsReturn;
        }
        public bool UpdateAccidentRecord(T_OA_ACCIDENTRECORD accidentRecordInfo)
        {
            try
            {
                //T_OA_ACCIDENTRECORD tmpobj = base.GetObjectByEntityKey(accidentRecordInfo.EntityKey) as T_OA_ACCIDENTRECORD;
                //accidentRecordInfo.T_OA_VEHICLE = base.GetObjectByEntityKey(accidentRecordInfo.T_OA_VEHICLE.EntityKey) as T_OA_VEHICLE;
                var users = from ent in base.GetObjects().Include("T_OA_VEHICLE")//.T_OA_SENDDOC
                            where ent.ACCIDENTRECORDID == accidentRecordInfo.ACCIDENTRECORDID
                            select ent;
                if (accidentRecordInfo.EntityKey == null)
                    accidentRecordInfo.EntityKey = users.FirstOrDefault().EntityKey;
                accidentRecordInfo.UPDATEDATE = System.DateTime.Now;
                int i=base.Update(accidentRecordInfo);
                //int i = SaveContextChanges();
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
                throw (ex);
            }
        }
    }
}