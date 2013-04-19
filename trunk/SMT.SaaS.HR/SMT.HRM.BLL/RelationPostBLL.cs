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
    public class RelationPostBLL:BaseBll<T_HR_RELATIONPOST>
    {
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的关联岗位信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public List<V_RELATIONPOST> RelationPostPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            IQueryable<T_HR_RELATIONPOST> ents = dal.GetObjects().Include("T_HR_POST");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_RELATIONPOST>(ents, pageIndex, pageSize, ref pageCount);
            if (ents != null)
            {
                foreach(var ent in ents)
                {
                    //加载岗位字典
                    ent.T_HR_POST.T_HR_POSTDICTIONARYReference.Load();
                }
            }
            List<T_HR_RELATIONPOST> tmplist = ents.ToList();
            List<V_RELATIONPOST> vents = new List<V_RELATIONPOST>();
            var tmps = from e in tmplist
                       select new V_RELATIONPOST
                       {
                           PostCode = e.T_HR_POST.T_HR_POSTDICTIONARY.POSTCODE,
                           PostName = e.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                           RelationPostID = e.RELATIONPOSTID,
                           Post = GetPostByID(e.RELATEPOSTID)
                       };
            vents = tmps.ToList();
            return vents;
        }

        private T_HR_POST GetPostByID(string postID)
        {
            return dal.GetObjects<T_HR_POST>().Include("T_HR_POSTDICTIONARY").FirstOrDefault(s => s.POSTID == postID);
        }

        /// <summary>
        /// 添加关联岗位信息
        /// </summary>
        /// <param name="entity"></param>
        public void RelationPostAdd(T_HR_RELATIONPOST entity)
        {
            try
            {
                T_HR_RELATIONPOST ent = new T_HR_RELATIONPOST();
                Utility.CloneEntity<T_HR_RELATIONPOST>(entity, ent);
                //岗位
                ent.T_HR_POSTReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POST", "POSTID", entity.T_HR_POST.POSTID);
                
                dal.Add(ent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 修改关联岗位信息
        /// </summary>
        /// <param name="entity"></param>
        public void RelationPostUpdate(T_HR_RELATIONPOST entity)
        {
            try
            {
                var ents = from ent in dal.GetTable()
                           where ent.RELATIONPOSTID == entity.RELATIONPOSTID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_RELATIONPOST>(entity, ent);

                    //if (entity.T_HR_POST != null)
                    //{
                    //    ent.T_HR_POSTReference.EntityKey =
                    //         new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POST", "POSTID", entity.T_HR_POST.POSTID);
                    //}
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 删除关联岗位信息
        /// </summary>
        /// <param name="relationPostIDs">关联岗位ID组</param>
        /// <returns>返回受影响的行数</returns>
        public int RelationPostDelete(string[] relationPostIDs)
        {
            foreach (string id in relationPostIDs)
            {
                var ent = dal.GetObjects().FirstOrDefault(s => s.RELATIONPOSTID == id);
                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 根据关联岗位ID查询岗位信息列表
        /// </summary>
        /// <param name="strID">关联岗位ID</param>
        /// <returns>返回关联岗位信息</returns>
        public V_RELATIONPOST GetRelationPostByID(string strID)
        {
            var ents = dal.GetObjects().Include("T_HR_POST").FirstOrDefault(s => s.RELATIONPOSTID == strID);
            //List<T_HR_RELATIONPOST> tmplist = ents.ToList();
            V_RELATIONPOST vents = new V_RELATIONPOST();
            vents.RelationPostID = ents.RELATIONPOSTID;
            var post = dal.GetObjects<T_HR_POST>().Include("T_HR_POSTDICTIONARY").FirstOrDefault(s => s.POSTID == ents.RELATEPOSTID);
            vents.Post = post;
            return vents;
        }
    }
}
