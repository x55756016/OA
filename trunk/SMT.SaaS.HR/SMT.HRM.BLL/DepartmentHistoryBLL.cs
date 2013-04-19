using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL
{
    public class DepartmentHistoryBLL : BaseBll<T_HR_DEPARTMENTHISTORY>
    {
        /// <summary>
        /// 根据创建日期查询部门历史记录，为空显示所有
        /// </summary>
        /// <param name="currentDate">创建日期</param>
        /// <returns>返回部门历史记录信息</returns>
        public List<T_HR_DEPARTMENTHISTORY> GetDepartmentHistory(DateTime currentDate)
        {
            List<T_HR_DEPARTMENTHISTORY> list = new List<T_HR_DEPARTMENTHISTORY>();
            var ents = (from a in dal.GetObjects()
                        where a.REUSEDATE <= currentDate
                        && (!a.CANCELDATE.HasValue || a.CANCELDATE >= currentDate)
                        select new { Cid = a.DEPARTMENTID }).Distinct();

            foreach (var h in ents)
            {
                var his = (from b in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY")
                           where b.DEPARTMENTID == h.Cid && b.REUSEDATE <= currentDate
                           && (!b.CANCELDATE.HasValue || b.CANCELDATE >= currentDate)
                           orderby b.REUSEDATE descending
                           select b).Take(1);
                //如果用FirstOrDefault 排序无效
                //var ent = his.FirstOrDefault();
                List<T_HR_DEPARTMENTHISTORY> tmpList = his.ToList();
                if (tmpList != null && tmpList.Count > 0)
                {
                    list.Add(tmpList[0]);
                }
            }
            return list;
        }

        /// <summary>
        /// 添加部门历史记录
        /// </summary>
        /// <param name="depart">部门信息</param>
        public void DepartmentHistoryAdd(T_HR_DEPARTMENTHISTORY depart)
        {
            try
            {
                T_HR_DEPARTMENTHISTORY ent = new T_HR_DEPARTMENTHISTORY();
                //ent.RECORDSID = depart.RECORDSID;
                //ent.COMPANYID = depart.COMPANYID;
                //ent.DEPARTMENTID = depart.DEPARTMENTID;
                Utility.CloneEntity(depart, ent);
                if (depart.T_HR_DEPARTMENTDICTIONARY != null)
                {
                    ent.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                  new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", depart.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                }
                //ent.DEPARTMENTDICTIONARYID = depart.DEPARTMENTDICTIONARYID;
                //ent.DEPARTMENTFUNCTION = depart.DEPARTMENTFUNCTION;
                //ent.EDITSTATE = depart.EDITSTATE;
                //ent.REUSEDATE = depart.REUSEDATE;
                //ent.CREATEDATE = depart.CREATEDATE;
                //ent.CREATEUSERID = depart.CREATEUSERID;
                dal.Add(ent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
