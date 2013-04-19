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
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
   public class AttendMachineSetBLL : BaseBll<T_HR_ATTENDMACHINESET>
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
        public IQueryable<T_HR_ATTENDMACHINESET> GetAttendMachineSetPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckState, string userid)
        {
            List<object> queryParas = new List<object>();

            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userid, "T_HR_ATTENDMACHINESET");

            #region----
            //SetFilterWithflow("ATTENDMACHINESETID", "T_HR_ATTENDMACHINESET", userid, ref strCheckState, ref filterString, ref queryParas);

            //if (!string.IsNullOrEmpty(strCheckState))
            //{
            //    if (!string.IsNullOrEmpty(filterString))
            //    {
            //        filterString += " AND";
            //    }

            //    filterString += " CHECKSTATE == @" + queryParas.Count();
            //    queryParas.Add(strCheckState);
            //}
            #endregion

            IQueryable<T_HR_ATTENDMACHINESET> ents = dal.GetObjects();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
                //ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_ATTENDMACHINESET>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
         
        /// <summary>
        /// 根据考勤机设置ID查询实体
        /// </summary>
        /// <param name="AttendMachineSetID">考勤机设置ID</param>
        /// <returns>返回考勤机设置实体</returns>
        public T_HR_ATTENDMACHINESET GetAttendMachineSetByID(string AttendMachineSetID)
        {
            var ents = from a in dal.GetTable()
                       where a.ATTENDMACHINESETID == AttendMachineSetID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 更新考勤机设置
        /// </summary>
        /// <param name="entity">考勤机设置实体</param>
        public void AttendMachineSetUpdate(T_HR_ATTENDMACHINESET entity)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.ATTENDMACHINESETID == entity.ATTENDMACHINESETID
                           select a;
                if (ents.Count() > 0)
                {
                    T_HR_ATTENDMACHINESET ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_ATTENDMACHINESET>(entity, ent);
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 删除考勤机设置记录，可同时删除多行记录
        /// </summary>
        /// <param name="attendMachineSetIDs">考勤机设置ID数组</param>
        /// <returns></returns>
        public int AttendMachineSetDelete(string[] attendMachineSetIDs)
        {
            foreach (string id in attendMachineSetIDs)
            {
                var ents = from e in dal.GetObjects()
                           where e.ATTENDMACHINESETID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }

            return dal.SaveContextChanges();
        } 
   }
}
