using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL
{
    public class EducateHistoryBLL:BaseBll<T_HR_EDUCATEHISTORY>
    {
        /// <summary>
        /// 根据简历ID返回教育培训记录
        /// </summary>
        /// <param name="resumeID">简历ID为空就返回所有教育记录</param>
        /// <returns>返回教育记录列表</returns>
        public IQueryable<T_HR_EDUCATEHISTORY> GetEducateHistoryAll(string resumeID)
        {
            var ents = from ent in dal.GetObjects()
                       where string.IsNullOrEmpty(resumeID) || ent.T_HR_RESUME.RESUMEID == resumeID
                       orderby ent.STARTDATE
                       select ent;
            return ents;
        }
        /// <summary>
        /// 添加教育培训记录
        /// </summary>
        /// <param name="entity">教育培训记录实体</param>
        public int EducateHistoryAdd(T_HR_EDUCATEHISTORY entity)
        {
            try
            {
                int rslCode = 0;
                T_HR_EDUCATEHISTORY ent = new T_HR_EDUCATEHISTORY();
                //ent = entity;
                ent.EDUCATEHISTORYID = entity.EDUCATEHISTORYID;
                ent.T_HR_RESUMEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", entity.T_HR_RESUME.RESUMEID);
                //ent.T_HR_RESUMEReference.EntityKey =
                //    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", entity.T_HR_RESUME.RESUMEID);
                ent.SCHOONAME = entity.SCHOONAME;
                ent.SPECIALTY = entity.SPECIALTY;
                ent.MAJOR = entity.MAJOR;
                ent.STARTDATE = entity.STARTDATE;
                ent.ENDDATE = entity.ENDDATE;
                ent.REMARK = entity.REMARK;
                ent.EDUCATIONHISTORY = entity.EDUCATIONHISTORY;
                ent.EDUCATIONPROPERTIE = entity.EDUCATIONPROPERTIE;
                ent.CREATEDATE = System.DateTime.Now;
                ent.CREATEUSERID = entity.CREATEUSERID;
                dal.Add(ent);
                rslCode = dal.SaveContextChanges();
                return rslCode;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EducateHistoryAdd:" + ex.Message);
                throw ex;
                return -1;
            }
        }
        /// <summary>
        /// 更新教育培训记录信息
        /// </summary>
        /// <param name="entity">教育培训记录实体</param>
        public int EducateHistoryUpdate(List<T_HR_EDUCATEHISTORY> educateHistory)
        {
            try
            {
                int rslCode = 0;
                foreach (var entity in educateHistory)
                {
                    var ents = from a in dal.GetObjects()
                               where a.EDUCATEHISTORYID == entity.EDUCATEHISTORYID
                               select a;
                    if (ents.Count() > 0)
                    {
                        var ent = ents.FirstOrDefault();
                        //ent.T_HR_RESUMEReference.EntityKey =
                        //new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", entity.T_HR_RESUME.RESUMEID);
                        ent.SCHOONAME = entity.SCHOONAME;
                        ent.SPECIALTY = entity.SPECIALTY;
                        ent.MAJOR = entity.MAJOR;
                        ent.STARTDATE = entity.STARTDATE;
                        ent.ENDDATE = entity.ENDDATE;
                        ent.REMARK = entity.REMARK;
                        ent.EDUCATIONHISTORY = entity.EDUCATIONHISTORY;
                        ent.EDUCATIONPROPERTIE = entity.EDUCATIONPROPERTIE;
                        ent.UPDATEDATE = System.DateTime.Now;
                        ent.UPDATEUSERID = entity.UPDATEUSERID;
                        dal.Update(ent);
                        rslCode = dal.SaveContextChanges();
                    }
                    else
                    {
                        rslCode = EducateHistoryAdd(entity);
                    }
                }
                return rslCode;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EducateHistoryUpdate:" + ex.Message);
                throw ex;
                return -1;
            }
        }
        /// <summary>
        /// 删除教育培训记录实体
        /// </summary>
        /// <param name="educateHistoryID">教育培训记录ID</param>
        /// <returns>是否成功删除教育记录信息</returns>
        public int EducateHistoryDelete(List<T_HR_EDUCATEHISTORY> educateHistoryIDs)
        {
            try
            {
                foreach (var educateHistoryID in educateHistoryIDs)
                {
                    var ents = (from ent in dal.GetObjects()
                                where ent.EDUCATEHISTORYID == educateHistoryID.EDUCATEHISTORYID
                                select ent);
                    var entity = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    if (entity != null)
                    {
                        //DataContext.DeleteObject(entity);
                        dal.DeleteFromContext(entity);
                    }
                }
                // return DataContext.SaveChanges();
                return dal.SaveContextChanges();
            }
            catch(Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EducateHistoryDelete:" + ex.Message);
                return (-1);
            }
        }
        /// <summary>
        /// 根据简历ID删除教育记录信息
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>是否成功删除</returns>
        public int EducateHistoryDeleteByResumeID(string resumeID)
        {
            try
            {
                var entitys = from ent in dal.GetObjects()
                               where ent.T_HR_RESUME.RESUMEID == resumeID
                               select ent;
                if (entitys.Count() > 0)
                {
                    foreach(var entity in entitys)
                    {
                        var ents = from e in dal.GetObjects()
                                   where e.EDUCATEHISTORYID == entity.EDUCATEHISTORYID
                                   select e;
                        var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                        if (ent != null)
                        {
                            dal.DeleteFromContext(ent);
                        }
                    }
                    return dal.SaveContextChanges();
                }
                return 0;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EducateHistoryDeleteByResumeID:" + ex.Message);
                throw ex;
            }
        } 
    }
}
