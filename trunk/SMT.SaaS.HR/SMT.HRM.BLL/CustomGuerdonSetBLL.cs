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
    public class CustomGuerdonSetBLL : BaseBll<T_HR_CUSTOMGUERDONSET>, ILookupEntity
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
        public IQueryable<T_HR_CUSTOMGUERDONSET> GetCustomGuerdonSetPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount,string strCheckState, string userid)
        {
            List<object> queryParas = new List<object>();

            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userid, "T_HR_CUSTOMGUERDONSET");

            SetFilterWithflow("CUSTOMGUERDONSETID", "T_HR_CUSTOMGUERDONSET", userid, ref strCheckState, ref filterString, ref queryParas);

            if (!string.IsNullOrEmpty(strCheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                filterString += " CHECKSTATE == @" + queryParas.Count();
                queryParas.Add(strCheckState);
            }


            IQueryable<T_HR_CUSTOMGUERDONSET> ents = dal.GetObjects(); // DataContext.T_HR_CUSTOMGUERDONSET;

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
                //ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_CUSTOMGUERDONSET>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 根据自定义薪资设置ID查询实体
        /// </summary>
        /// <param name="CustomGuerdonSetID">自定义薪资设置ID</param>
        /// <returns>返回自定义薪资设置实体</returns>
        public T_HR_CUSTOMGUERDONSET GetCustomGuerdonSetByID(string CustomGuerdonSetID)
        {
            var ents = from a in dal.GetTable()
                       where a.CUSTOMGUERDONSETID == CustomGuerdonSetID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据自定义薪资名称查询
        /// </summary>
        /// <param name="CustomGuerdonSetName">自定义薪资设置名</param>
        /// <returns>返回bool类型</returns>
        public bool GetCustomGuerdonSetName(string CustomGuerdonSetName)
        {
            var ents = from a in dal.GetTable()
                       where a.GUERDONNAME == CustomGuerdonSetName
                       select a; 
            return ents.Count() > 0 ? true: false;
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    IQueryable<T_HR_CUSTOMGUERDONSET> ents = from a in DataContext.T_HR_CUSTOMGUERDONSET
        //                                           select a;
        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            IQueryable<T_HR_CUSTOMGUERDONSET> ents = from a in dal.GetObjects()
                                                     select a;
            ents = ents.Where(filterString, paras.ToArray());
            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        /// <summary>
        /// 更新自定义薪资设置
        /// </summary>
        /// <param name="entity">自定义薪资设置实体</param>
        public void CustomGuerdonSetUpdate(T_HR_CUSTOMGUERDONSET entity)
        {
            try
            {
                var ents = from a in dal.GetTable()
                           where a.CUSTOMGUERDONSETID == entity.CUSTOMGUERDONSETID
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    ent.GUERDONNAME = entity.GUERDONNAME;
                    ent.GUERDONCATEGORY = entity.GUERDONCATEGORY;
                    ent.CALCULATORTYPE = entity.CALCULATORTYPE;
                    ent.GUERDONSUM = entity.GUERDONSUM;
                    ent.REMARK = entity.REMARK;
                    ent.UPDATEDATE = entity.UPDATEDATE;
                    ent.UPDATEUSERID = entity.UPDATEUSERID;
                    ent.CHECKSTATE = entity.CHECKSTATE;
                    ent.EDITSTATE = entity.EDITSTATE;
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除自定义薪资设置记录，可同时删除多行记录
        /// </summary>
        /// <param name="CustomGuerdonSets">自定义薪资设置ID数组</param>
        /// <returns></returns>
        public int CustomGuerdonSetDelete(string[] CustomGuerdonSets)
        {
            foreach (string id in CustomGuerdonSets)
            {
                var ents = from e in dal.GetObjects()
                           where e.CUSTOMGUERDONSETID == id
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
