using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
namespace SMT.HRM.BLL
{
    public class PensionAlarmSetBLL : BaseBll<T_HR_PENSIONALARMSET>
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
        public IQueryable<V_PENSIONALARMSET> GetPensionAlarmSetPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_PENSIONALARMSET");
            IQueryable<V_PENSIONALARMSET> ents = from c in dal.GetObjects()
                                                 join b in dal.GetObjects<T_HR_COMPANY>() on c.COMPANYID equals b.COMPANYID
                                                 select new V_PENSIONALARMSET
                                                 {
                                                     T_HR_PENSIONALARMSET = c,
                                                     CNAME = b.CNAME,
                                                     OWNERCOMPANYID = c.OWNERCOMPANYID,
                                                     OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                                                     OWNERID = c.OWNERID,
                                                     OWNERPOSTID = c.OWNERPOSTID,
                                                     CREATEUSERID = c.CREATEUSERID
                                                 };
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_PENSIONALARMSET>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// 更新社保提醒设置
        /// </summary>
        /// <param name="entity">社保提醒设置实体</param>
        public void PensionAlarmSetUpdate(T_HR_PENSIONALARMSET entity)
        {
            try
            {
                entity.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_PENSIONALARMSET", "PENSIONSETID", entity.PENSIONSETID);
                dal.Update(entity);
            }
            catch (Exception ex)
            {
                Utility.SaveLog("PensionAlarmSetUpdate:" + ex.Message);
                throw ex;
               
            }
        }
        /// <summary>
        /// 新增社保提醒
        /// </summary>
        /// <param name="entity"></param>
        public void PensionAlarmSetAdd(T_HR_PENSIONALARMSET entity, ref string strMsg)
        {
            try
            {
                var entTmp = from c in dal.GetObjects()
                             where c.COMPANYID == entity.COMPANYID
                             select c;
                if (entTmp.Count() > 0)
                {
                    // throw new Exception("EXIST");
                    strMsg = "EXIST";
                    return;
                }
                dal.Add(entity);
            }
            catch (Exception ex)
            {
                Utility.SaveLog("PensionAlarmSetAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 删除社保提醒设置
        /// </summary>
        /// <param name="pensionAlarmSetID">社保提醒设置ID组</param>
        /// <returns>是否删除成功</returns>
        public int PensionAlarmSetDelete(string[] pensionAlarmSetIDs)
        {
            foreach (string id in pensionAlarmSetIDs)
            {
                var ents = from a in dal.GetObjects()
                           where a.PENSIONSETID == id
                           select a;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 根据社保提醒设置ID获取实体
        /// </summary>
        /// <param name="pensionAlarmSetID">社保提醒设置ID</param>
        /// <returns>获取社保提醒设置信息</returns>
        public T_HR_PENSIONALARMSET GetPensionAlarmSetByID(string pensionAlarmSetID)
        {
            var ents = from a in dal.GetObjects()
                       where a.PENSIONSETID == pensionAlarmSetID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 根据社保提醒设置ID获取实体
        /// </summary>
        /// <param name="pensionAlarmSetID">社保提醒设置ID</param>
        /// <returns>获取社保提醒设置信息</returns>
        public V_PENSIONALARMSET GetPensionAlarmSetViewByID(string pensionAlarmSetID)
        {
            var ents = from a in dal.GetObjects()
                       join b in dal.GetObjects<T_HR_COMPANY>() on a.COMPANYID equals b.COMPANYID
                       where a.PENSIONSETID == pensionAlarmSetID
                       select new V_PENSIONALARMSET
                       {
                           T_HR_PENSIONALARMSET = a,
                           CNAME = b.CNAME
                       };
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
    }
}
