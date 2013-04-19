using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL
{
    public class PostHistoryBLL : BaseBll<T_HR_POSTHISTORY>
    {
        /// <summary>
        /// 根据创建日期查询部门历史记录，为空显示所有
        /// </summary>
        /// <param name="currentDate">创建日期</param>
        /// <returns>返回部门历史记录信息</returns>
        public List<T_HR_POSTHISTORY> GetPostHistory(DateTime currentDate)
        {
            List<T_HR_POSTHISTORY> list = new List<T_HR_POSTHISTORY>();
            var ents = (from a in dal.GetObjects()
                        where a.REUSEDATE <= currentDate
                        && (!a.CANCELDATE.HasValue || a.CANCELDATE >= currentDate)
                        select new { Cid = a.POSTID }).Distinct();

            foreach (var h in ents)
            {
                var his = (from b in dal.GetObjects().Include("T_HR_POSTDICTIONARY")
                           where b.POSTID == h.Cid && b.REUSEDATE <= currentDate
                            && (!b.CANCELDATE.HasValue || b.CANCELDATE >= currentDate)
                           orderby b.REUSEDATE descending
                           select b).Take(1);
                //如果用FirstOrDefault 排序无效
                //var ent = his.FirstOrDefault();
                List<T_HR_POSTHISTORY> tmpList = his.ToList();
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
        public void PostHistoryAdd(T_HR_POSTHISTORY entity)
        {
            try
            {
                T_HR_POSTHISTORY ent = new T_HR_POSTHISTORY();
                Utility.CloneEntity(entity, ent);
                //ent.RECORDSID = entity.RECORDSID;
                //ent.POSTID = entity.POSTID;
                if (entity.T_HR_POSTDICTIONARY != null)
                {
                    ent.T_HR_POSTDICTIONARYReference.EntityKey =
                 new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POSTDICTIONARY", "POSTDICTIONARYID", entity.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                }

                //ent.DEPARTMENTID = entity.DEPARTMENTID;
                //ent.DEPARTMENTNAME = entity.DEPARTMENTNAME;
                ////ent.COMPANYNAME = entity.COMPANYNAME;
                //ent.POSTFUNCTION = entity.POSTFUNCTION;
                //ent.POSTNUMBER = entity.POSTNUMBER;
                //ent.POSTGOAL = entity.POSTGOAL;
                //ent.CHECKUSER = entity.CHECKUSER;
                //ent.FATHERPOSTID = entity.FATHERPOSTID;
                //ent.UNDERNUMBER = entity.UNDERNUMBER;
                //ent.PROMOTEDIRECTION = entity.PROMOTEDIRECTION;
                //ent.CHANGEPOST = entity.CHANGEPOST;
                ////ent.EDITSTATE = entity.EDITSTATE;
                //ent.CREATEDATE = entity.CREATEDATE;
                //ent.CREATEUSERID = entity.CREATEUSERID;
                dal.Add(ent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
