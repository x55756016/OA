using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class ImportSetMasterBLL : BaseBll<T_HR_IMPORTSETMASTER>
    {
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_IMPORTSETMASTER> ImportSetMasterPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_IMPORTSETMASTER");
            }
            IQueryable<T_HR_IMPORTSETMASTER> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_IMPORTSETMASTER>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        public void ImportSetMasterAdd(T_HR_IMPORTSETMASTER entity, List<T_HR_IMPORTSETDETAIL> entList, ref string strMsg)
        {
            try
            {
                T_HR_IMPORTSETMASTER tempEnt = new T_HR_IMPORTSETMASTER();
                var entTmp = from c in dal.GetObjects()
                             where c.CITY == entity.CITY && c.OWNERCOMPANYID == entity.OWNERCOMPANYID
                             select c;
                if (entTmp.Count() > 0)
                {
                    //  throw new Exception("EXISTIMPORTSETCITY");
                    strMsg = "EXISTIMPORTSETCITY";
                    return;
                }
                Utility.CloneEntity<T_HR_IMPORTSETMASTER>(entity, tempEnt);
                foreach (var ents in entList)
                {
                    T_HR_IMPORTSETDETAIL ent = new T_HR_IMPORTSETDETAIL();
                    Utility.CloneEntity<T_HR_IMPORTSETDETAIL>(ents, ent);
                    tempEnt.T_HR_IMPORTSETDETAIL.Add(ent);
                }
                //DataContext.AddObject("T_HR_IMPORTSETMASTER", tempEnt);
                //DataContext.SaveChanges();
                dal.AddToContext(tempEnt);
                dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ImportSetMasterAdd:" + ex.Message);
                throw ex;
            }
        }

        public void ImportSetMasterUpdate(T_HR_IMPORTSETMASTER entity, List<T_HR_IMPORTSETDETAIL> entList, ref string strMsg)
        {
            try
            {
                var entTmp = dal.GetObjects().Where(s => s.MASTERID != entity.MASTERID && s.CITY == entity.CITY && s.OWNERCOMPANYID == entity.OWNERCOMPANYID);
                if (entTmp.Count() > 0)
                {
                    // throw new Exception("EXISTIMPORTSETCITY");
                    strMsg = "EXISTIMPORTSETCITY";
                    return;
                }
                var ents = dal.GetObjects().FirstOrDefault(s => s.MASTERID == entity.MASTERID);
                if (ents != null)
                {
                    Utility.CloneEntity(entity, ents);
                    dal.UpdateFromContext(ents);
                    foreach (var tempEnt in entList)
                    {
                        var ent = dal.GetObjects<T_HR_IMPORTSETDETAIL>().FirstOrDefault(s => s.DETAILID == tempEnt.DETAILID);
                        if (ent != null)
                        {
                            Utility.CloneEntity(tempEnt, ent);
                            if (tempEnt.T_HR_IMPORTSETMASTER != null)
                            {
                                ent.T_HR_IMPORTSETMASTERReference.EntityKey =
                                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_IMPORTSETMASTER", "MASTERID", tempEnt.T_HR_IMPORTSETMASTER.MASTERID);
                            }
                            dal.UpdateFromContext(ent);
                        }
                    }
                    // DataContext.SaveChanges();
                    dal.SaveContextChanges();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ImportSetMasterUpdate:" + ex.Message);
                throw ex;
            }
        }

        public int ImportSetMasterDelete(string[] ids)
        {
            foreach (var id in ids)
            {
                var ent = dal.GetObjects().FirstOrDefault(s => s.MASTERID == id);
                if (ent != null)
                {
                    var ents = dal.GetObjects<T_HR_IMPORTSETDETAIL>().Where(s => s.T_HR_IMPORTSETMASTER.MASTERID == id);
                    if (ents != null)
                    {
                        foreach (var tempEnt in ents)
                        {
                            dal.DeleteFromContext(tempEnt);
                        }
                    }
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges();
        }

        public T_HR_IMPORTSETMASTER GetImportSetMasterByID(string masterID)
        {
            return dal.GetObjects().FirstOrDefault(s => s.MASTERID == masterID);
        }

        public T_HR_IMPORTSETMASTER GetImportSetMasterByEntityCode(string entityCode)
        {
            T_HR_IMPORTSETMASTER mast = dal.GetObjects().FirstOrDefault(s => s.ENTITYCODE == entityCode);
            mast.T_HR_IMPORTSETDETAIL.Load();
            return mast;
        }
        public T_HR_IMPORTSETMASTER GetImportSetMasterByEntityCode(string entityCode, string cityID)
        {
            T_HR_IMPORTSETMASTER mast = dal.GetObjects().FirstOrDefault(s => s.ENTITYCODE == entityCode && s.CITY == cityID);
            mast.T_HR_IMPORTSETDETAIL.Load();
            return mast;
        }
        public T_HR_IMPORTSETMASTER GetImportSetMasterByEntityCode(string entityCode, string cityID, string companyID)
        {
            T_HR_IMPORTSETMASTER mast = dal.GetObjects().FirstOrDefault(s => s.ENTITYCODE == entityCode && s.CITY == cityID && s.OWNERCOMPANYID == companyID);
            if (mast != null)
            {
                mast.T_HR_IMPORTSETDETAIL.Load();
            }
            return mast;
        }
    }
}
