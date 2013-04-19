using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class PostLevelDistinctionBLL : BaseBll<T_HR_POSTLEVELDISTINCTION>
    {

        /// <summary>
        /// 获取所有的岗位级别
        /// </summary>
        /// <returns></returns>
        public List<T_HR_POSTLEVELDISTINCTION> GetAllPostLevelDistinction()
        {
            var ents = dal.GetObjects().ToList();
            return ents;
        }
        /// <summary>
        /// 获取指定薪资体系的岗位薪资和级差额
        /// </summary>
        /// <returns></returns>
        public List<T_HR_POSTLEVELDISTINCTION> GetPostLevelDistinctionBySystemID(string SalarySystemID)
        {
            var ents = from c in dal.GetObjects()
                       where c.T_HR_SALARYSYSTEM.SALARYSYSTEMID == SalarySystemID
                       select c;
            return ents.Count()>0? ents.ToList():null;
        }

        /// <summary>
        /// 获取指定薪资体系的岗位薪资和级差额
        /// </summary>
        /// <returns></returns>
        public T_HR_POSTLEVELDISTINCTION GetPostLevelDistinctionByID(string id)
        {
            var ents = from c in dal.GetObjects()
                       where c.POSTLEVELID == id
                       select c;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 岗位级别薪资修改
        /// </summary>
        /// <param name="ents"></param>
        public void PostLevelDistinctionUpdate(List<T_HR_POSTLEVELDISTINCTION> ents)
        {
            foreach (var ent in ents)
            {

                var tmpEnts = from p in dal.GetObjects()
                              where p.POSTLEVELID == ent.POSTLEVELID
                              select p;
                if (tmpEnts != null && tmpEnts.Count() > 0)
                {
                    var tmpEnt = tmpEnts.FirstOrDefault();

                    Utility.CloneEntity<T_HR_POSTLEVELDISTINCTION>(ent, tmpEnt);

                    dal.Update(tmpEnt);
                }
                else
                {
                    T_HR_POSTLEVELDISTINCTION tmpEnt = new T_HR_POSTLEVELDISTINCTION();
                    Utility.CloneEntity<T_HR_POSTLEVELDISTINCTION>(ent, tmpEnt);
                    if(ent.T_HR_SALARYSYSTEM!=null)
                    {
                        tmpEnt.T_HR_SALARYSYSTEMReference.EntityKey =
       new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYSYSTEM", "SALARYSYSTEMID", ent.T_HR_SALARYSYSTEM.SALARYSYSTEMID);
                    }
                    dal.Add(tmpEnt);
                }
            }
            //DataContext.SaveChanges();
        }
        /// <summary>
        ///新增
        /// </summary>
        /// <param name="ents"></param>
        public string PostLevelDistinctionADD(T_HR_POSTLEVELDISTINCTION ent)
        {


            var tmpEnts = from p in dal.GetObjects()
                          where p.POSTLEVEL == ent.POSTLEVEL
                          select p;
            if (tmpEnts != null && tmpEnts.Count() > 0)
            {
                return "EXIST";
            }
            else
            {
                dal.Add(ent);
            }
            return "SUCCESSED";
        }
    }
}
