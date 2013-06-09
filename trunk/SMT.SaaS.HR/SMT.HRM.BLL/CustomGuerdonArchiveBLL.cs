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
    public class CustomGuerdonArchiveBLL : BaseBll<T_HR_CUSTOMGUERDONARCHIVE>
    {
        /// <summary>
        /// 添加自定义薪资档案
        /// </summary>
        /// <param name="obj"> 自定义薪资档案实例</param>
        public void CustomGuerdonArchiveAdd(T_HR_CUSTOMGUERDONARCHIVE obj)
        {
            try
            {
                T_HR_CUSTOMGUERDONARCHIVE ent = new T_HR_CUSTOMGUERDONARCHIVE();
                Utility.CloneEntity<T_HR_CUSTOMGUERDONARCHIVE>(obj, ent);
                if (obj.T_HR_SALARYARCHIVE != null)
                {
                    ent.T_HR_SALARYARCHIVEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_SALARYARCHIVE", "SALARYARCHIVEID", obj.T_HR_SALARYARCHIVE.SALARYARCHIVEID);

                }
                dal.Add(ent);
            }
            catch (Exception ee)
            {
                throw ee;
            }
        }
        /// <summary>
        /// 更新自定义薪资档案
        /// </summary>
        /// <param name="entity">自定义薪资档案实例</param>
        public void CustomGuerdonArchiveUpdate(T_HR_CUSTOMGUERDONARCHIVE obj)
        {
            try
            {
                //读取配置文件查找下拨人，薪资审核通过也是这个下拨人ID
                string strAssignOwnerID = System.Configuration.ConfigurationManager.AppSettings["PersonMoneyAssignOwner"];
                if (!string.IsNullOrEmpty(strAssignOwnerID))
                {
                    obj.ASSIGNERID = strAssignOwnerID;
                }
                var ent = from a in dal.GetTable()
                          where a.CUSTOMGUERDONARCHIVEID == obj.CUSTOMGUERDONARCHIVEID
                          select a;
                if (ent.Count() > 0)
                {
                    T_HR_CUSTOMGUERDONARCHIVE tmpEnt = ent.FirstOrDefault();

                    Utility.CloneEntity<T_HR_CUSTOMGUERDONARCHIVE>(obj, tmpEnt);
                    dal.Update(tmpEnt);
                }
                else
                {
                    CustomGuerdonArchiveAdd(obj);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        /// <summary>
        /// 删除自定义薪资档案
        /// </summary>
        /// <param name="IDs">自定义薪资档案ID</param>
        /// <returns></returns>
        public int CustomGuerdonArchiveDelete(string[] IDs)
        {

            foreach (string id in IDs)
            {
                var ents = from e in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>()
                           where e.CUSTOMGUERDONARCHIVEID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }

                //TODO:删除项目所包含的明细
            }

            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 删除指定薪资档案的自定义薪资档案
        /// </summary>
        /// <param name="IDs">薪资档案ID</param>
        /// <returns></returns>
        public int CustomGuerdonArchiveDelByArchiveID(string ArchiveID)
        {
           // List<T_HR_CUSTOMGUERDONARCHIVE> CustomArchiveList = new List<T_HR_CUSTOMGUERDONARCHIVE>();
            //foreach (string id in IDs)
            //{
            int IntResult = 0;
            var ents = from e in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>().Include("T_HR_SALARYARCHIVE")
                       where e.T_HR_SALARYARCHIVE.SALARYARCHIVEID == ArchiveID
                       select e;
            
            if (ents != null)
            {
                //添加了判断是否存在自定义薪资，如果没存在则返回为1
                //否则引用处事务会回滚
                if (ents.Count() > 0)
                {
                    foreach (var ent in ents)
                    {
                        dal.DeleteFromContext(ent);
                        // dal.Delete(ent);
                    }
                }
                else
                {
                    IntResult = 1;
                }
            }
            //TODO:删除项目所包含的明细
            //}
            if (IntResult == 0)
            {
                return dal.SaveContextChanges();
            }
            else
            {
                return IntResult;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{
        //    IQueryable<T_HR_SALARYSTANDARD> ents = from a in DataContext.T_HR_SALARYSTANDARD
        //                                           select a;
        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}
        /// <summary>
        /// 根据ID获取自定义薪资档案
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T_HR_CUSTOMGUERDONARCHIVE GetCustomGuerdonArchiveByID(string ID)
        {
            var ents = from a in dal.GetTable()
                       where a.CUSTOMGUERDONARCHIVEID == ID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        ///根据薪资档案ID获取自定义薪资档案
        /// </summary>
        /// <param name="ArchiveID"></param>
        /// <returns></returns>
        public List<T_HR_CUSTOMGUERDONARCHIVE> GetCustomGuerdonArchiveByArchiveID(string ArchiveID)
        {
            var ents = from a in dal.GetTable()
                       where a.T_HR_SALARYARCHIVE.SALARYARCHIVEID == ArchiveID
                       select a;
            return ents.Count() > 0 ? ents.ToList() : null;
        }
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
        public new IQueryable<V_CUSTOMGUERDONARCHIVE> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {

            //    IQueryable<T_HR_CUSTOMGUERDONARCHIVE> ents = dal.GetTable();
            var ents = from a in dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>().Include("T_HR_SALARYARCHIVE")
                       join g in dal.GetObjects<T_HR_CUSTOMGUERDON>().Include("T_HR_CUSTOMGUERDONSET") on a.CUSTOMERGUERDONID equals g.CUSTOMGUERDONID
                       select new V_CUSTOMGUERDONARCHIVE
                       {
                           T_HR_CUSTOMGUERDONARCHIVE = a,
                           GUERDONNAME = g.T_HR_CUSTOMGUERDONSET.GUERDONNAME,
                           SALARYARCHIVEID = a.T_HR_SALARYARCHIVE.SALARYARCHIVEID
                       };
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_CUSTOMGUERDONARCHIVE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// 带有权限的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页 </param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件的参数 </param>
        /// <param name="pageCount">总页数</param>
        /// <param name="userID">用户ID</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_CUSTOMGUERDONARCHIVE> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_CUSTOMGUERDONARCHIVE");

            IQueryable<T_HR_CUSTOMGUERDONARCHIVE> ents = dal.GetObjects<T_HR_CUSTOMGUERDONARCHIVE>();

            //var q = from a in DataContext.T_HR_CUSTOMGUERDONARCHIVE
            //        join g in DataContext.T_HR_CUSTOMGUERDON.Include("T_HR_CUSTOMGUERDONSET") on a.CUSTOMERGUERDONID equals g.CUSTOMGUERDONID
            //        select new V_CUSTOMGUERDONARCHIVE
            //        {
            //            T_HR_CUSTOMGUERDONARCHIVE = a,
            //            GUERDONNAME = g.T_HR_CUSTOMGUERDONSET.GUERDONNAME
            //        };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_CUSTOMGUERDONARCHIVE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
    }
}
