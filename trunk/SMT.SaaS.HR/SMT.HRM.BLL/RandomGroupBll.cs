/// <summary>
/// Log No.： 1
/// Modify Desc： 检查连接
/// Modifier： 冉龙军
/// Modify Date： 2010-08-31
/// </summary>
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
using System.Data.Objects;

namespace SMT.HRM.BLL
{
    public class RandomGroupBll : BaseBll<T_HR_RANDOMGROUP>
    {
        /// <summary>
        /// 获取全部抽查组信息
        /// </summary>
        /// <returns></returns>
        public List<T_HR_RANDOMGROUP> GetRandomGroupAll()
        {
            var q = from ent in dal.GetObjects()
                    select ent;
            return q.ToList();
        }

        public IQueryable<T_HR_RANDOMGROUP> GetRandomGroupPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount,string userID)
        {
            if (pageIndex == 1 && filterString.Equals("") && paras.Length == 0)
                DelWaste();
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_RANDOMGROUP");
            IQueryable<T_HR_RANDOMGROUP> ents = dal.GetObjects().Include("T_HR_RAMDONGROUPPERSON");
            //ents = from rg in dal.GetObjects().Include("T_HR_RAMDONGROUPPERSON")
            //       select rg;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_RANDOMGROUP>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }

        private void DelWaste()
        {
            IQueryable<T_HR_RAMDONGROUPPERSON> ents = dal.GetObjects<T_HR_RAMDONGROUPPERSON>();
            ents = from rgp in dal.GetObjects<T_HR_RAMDONGROUPPERSON>()
                   select rgp;
            if (ents.Count() > 0)
            {
                List<string> list = new List<string>();
                foreach (T_HR_RAMDONGROUPPERSON rgp in ents)
                {
                    IQueryable<T_HR_EMPLOYEE> entemployes = from rg in dal.GetObjects<T_HR_EMPLOYEE>()
                                                     where rg.EMPLOYEEID == rgp.PERSONID
                                                     select rg;
                    if (entemployes.Count() == 0)
                    {
                        list.Add(rgp.GROUPPERSONID);
                    }
                }
                foreach (string groupperson in list)
                {
                    DeleteRandomGroupPerson(groupperson);
                }
            }
        }

        public IQueryable<T_HR_RAMDONGROUPPERSON> GetRandomGroupPersonAll(string randomGroupID)
        {
            var q = from ent in dal.GetObjects<T_HR_RAMDONGROUPPERSON>()
                    where ent.T_HR_RANDOMGROUP.RANDOMGROUPID == randomGroupID
                    select ent;
            return q;
        }
        /// <summary>
        /// 获取抽查组人员
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="randomGroupID"></param>
        /// <returns></returns>
        public List<V_EMPLOYEEVIEW> GetRandomGroupPersonPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string randomGroupID)
        {
            var q = from ent in dal.GetObjects<T_HR_RAMDONGROUPPERSON>()
                    join employee in dal.GetObjects<T_HR_EMPLOYEE>() on ent.PERSONID equals employee.EMPLOYEEID
                    where ent.T_HR_RANDOMGROUP.RANDOMGROUPID == randomGroupID
                    select new V_EMPLOYEEVIEW
                    {
                        EMPLOYEEID = employee.EMPLOYEEID,
                        EMPLOYEECNAME = employee.EMPLOYEECNAME,
                        EMPLOYEECODE = employee.EMPLOYEECODE,
                        SEX = employee.SEX,
                        MOBILE = employee.MOBILE,
                        EMAIL = employee.EMAIL
                        
                    };
            return q.Count()>0? q.ToList():null;
        }

        public void AddRandomGroup(T_HR_RANDOMGROUP entType,ref string strMsg)
        {
            try
            {
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.RANDOMGROUPNAME == entType.RANDOMGROUPNAME
                || s.RANDOMGROUPID == entType.RANDOMGROUPID);
                if (tempEnt != null)
                {
                  //  throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                entType.UPDATEDATE = System.DateTime.Now;
                entType.CREATEDATE = System.DateTime.Now;
                dal.Add(entType);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " AddRandomGroup:" + ex.Message);
                throw (ex);
            }
        }

        public void UpdateRandomGroup(T_HR_RANDOMGROUP entType,ref string strMsg)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => s.RANDOMGROUPNAME == entType.RANDOMGROUPNAME
                   && s.RANDOMGROUPID != entType.RANDOMGROUPID);
                if (temp != null)
                {
                    //throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                var ents = from ent in dal.GetTable()
                           where ent.RANDOMGROUPID == entType.RANDOMGROUPID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    entType.UPDATEDATE = System.DateTime.Now;
                    Utility.CloneEntity(entType, ent);
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateRandomGroup:" + ex.Message);
                throw (ex);
            }
        }

        public int DeleteRandomGroup(string randomGroupId)
        {
            try
            {
                var ents = from e in dal.GetObjects()
                           where e.RANDOMGROUPID == randomGroupId
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteRandomGroup:" + ex.Message);
                throw (ex);
            }
        }

        public void AddRandomGroupPerson(T_HR_RAMDONGROUPPERSON entType)
        {
            try
            {
                var tempEnt = dal.GetObjects<T_HR_RAMDONGROUPPERSON>().FirstOrDefault(s => s.T_HR_RANDOMGROUP.RANDOMGROUPID == entType.T_HR_RANDOMGROUP.RANDOMGROUPID
                || s.PERSONID == entType.PERSONID);
                if (tempEnt == null)
                {
                    dal.Add(entType);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " AddRandomGroupPerson:" + ex.Message);
                throw (ex);
            }
        }

        public int AddRandomGroupPersonList(List<T_HR_RAMDONGROUPPERSON> entList)
        {
            try
            {
                if (entList== null || entList.Count == 0)
                    return 0;
                foreach (T_HR_RAMDONGROUPPERSON entType in entList)
                {
                    var tempEnt = dal.GetObjects<T_HR_RAMDONGROUPPERSON>().FirstOrDefault(s => s.T_HR_RANDOMGROUP.RANDOMGROUPID == entType.T_HR_RANDOMGROUP.RANDOMGROUPID
                    && s.PERSONID == entType.PERSONID);
                    if (tempEnt == null)
                    {
                        //抽查组外键
                        //if (entType.T_HR_RANDOMGROUP != null)
                        //{
                        //    //外键关联
                        //    entType.T_HR_RANDOMGROUPReference.EntityKey =
                        //            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RAMDONGROUPPERSON", "RANDOMGROUPID", entType.T_HR_RANDOMGROUP.RANDOMGROUPID);
                        //}
                        Utility.RefreshEntity(entType);
                        //dal.Add(entType);
                        //i++;
                        this.dal.AddToContext(entType);
                    }
                }
                //i = this.dal.GetObjects<SaveChanges();
                return this.dal.SaveContextChanges(); ;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " AddRandomGroupPersonList:" + ex.Message);
                throw (ex);
            }
        }

        public void UpdateRandomGroupPerson(T_HR_RAMDONGROUPPERSON entType)
        {
            try
            {
                var temp = dal.GetObjects<T_HR_RAMDONGROUPPERSON>().FirstOrDefault(s => s.T_HR_RANDOMGROUP.RANDOMGROUPID == entType.T_HR_RANDOMGROUP.RANDOMGROUPID
                   && s.PERSONID == entType.PERSONID && s.GROUPPERSONID != entType.GROUPPERSONID);
                if (temp != null)
                {
                    throw new Exception("Repetition");
                }
                var ents = from ent in dal.GetObjects<T_HR_RAMDONGROUPPERSON>()
                           where ent.GROUPPERSONID == entType.GROUPPERSONID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity(entType, ent);
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateRandomGroupPerson:" + ex.Message);
                throw (ex);
            }
        }

        public int DeleteRandomGroupPerson(string groupPersonId)
        {
            try
            {
                var ents = from e in dal.GetObjects<T_HR_RAMDONGROUPPERSON>()
                           where e.GROUPPERSONID == groupPersonId
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteRandomGroupPerson:" + ex.Message);
                throw (ex);
            }
        }

        public int DeleteRandomGroupPersons(string[] groupPersonIds)
        {
            try
            {
                if (groupPersonIds == null || groupPersonIds.Length == 0)
                    return 0;
                foreach (string groupPersonId in groupPersonIds)
                {
                    var ents = from e in dal.GetObjects<T_HR_RAMDONGROUPPERSON>()
                               where e.GROUPPERSONID == groupPersonId
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                    }
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteRandomGroupPersons:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 根据员工ID和抽查组ID删除抽查人员
        /// </summary>
        /// <param name="groupPersonIds"></param>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public int DeleteRandomGroupPersons(string[] groupPersonIds,string groupID)
        {
            try
            {
                if (groupPersonIds == null || groupPersonIds.Length == 0)
                    return 0;
                foreach (string groupPersonId in groupPersonIds)
                {
                    var ents = from e in dal.GetObjects<T_HR_RAMDONGROUPPERSON>()
                               where e.PERSONID == groupPersonId && e.T_HR_RANDOMGROUP.RANDOMGROUPID == groupID
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                    }
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteRandomGroupPersons:" + ex.Message);
                throw (ex);
            }
        }
        public T_HR_RAMDONGROUPPERSON GetRandomGroupPersonByID(string groupPersonId)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_HR_RAMDONGROUPPERSON>()
                           where a.GROUPPERSONID == groupPersonId
                           select a;
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetRandomGroupPersonByID:" + ex.Message);
                throw ex;
            }
            return null;
        }

        public IQueryable<T_HR_RAMDONGROUPPERSON> GetRandomGroupPersonByGroupID(string groupId)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_HR_RAMDONGROUPPERSON>()
                           where a.T_HR_RANDOMGROUP.RANDOMGROUPID == groupId
                           select a;
                return ents;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetRandomGroupPersonByGroupID:" + ex.Message);
                throw ex;
            }
            return null;
        }

        public T_HR_RANDOMGROUP GetRandomGroupByID(string randomGroupID)
        {
            try
            {
                var ents = from a in dal.GetObjects()
                           where a.RANDOMGROUPID == randomGroupID
                           select a;
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetRandomGroupByID:" + ex.Message);
                throw ex;
            }
            return null;
        }

        public int DeleteRandomGroups(string[] randomGroupIds)
        {
            try
            {
                foreach (string groupPersonId in randomGroupIds)
                {
                    var ents = from e in dal.GetObjects()
                               where e.RANDOMGROUPID == groupPersonId
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                    }
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DeleteRandomGroups:" + ex.Message);
                throw (ex);
            }
        }

        /// <summary>
        /// 修改抽查组人员
        /// </summary>
        /// <param name="entList">需要添加的抽查组人员实体列表</param>
        /// <param name="groupPersonIDs">需要删除的抽查组人员ID列表</param>
        public int[] UpdateRandomGroupPersonList(List<T_HR_RAMDONGROUPPERSON> entList, string[] groupPersonIDs)
        {
            dal.BeginTransaction();
            int[] act = {0, 0};
            try
            {
                //edm = this.DataContext;
                //// 1s 冉龙军
                ////edm.Connection.Open();
                //if (edm.Connection.State == System.Data.ConnectionState.Closed)
                //{
                //    edm.Connection.Open();
                //}
                //// 1e
                //tran = edm.Connection.BeginTransaction();
                act[0] = AddRandomGroupPersonList(entList);
                act[1] = DeleteRandomGroupPersons(groupPersonIDs);
                dal.CommitTransaction();
                return act;

            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                //if (tran != null)
                //    tran.Rollback();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " UpdateRandomGroupPersonList:" + ex.Message);
                throw ex;
            }
            finally
            {
                //if (edm != null && edm.Connection.State != System.Data.ConnectionState.Closed)
                //    edm.Connection.Close();
            }

        }
    }
}
