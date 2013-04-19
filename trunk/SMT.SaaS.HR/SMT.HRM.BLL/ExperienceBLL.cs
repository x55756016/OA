using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL
{
    /// <summary>
    /// 工作经历的数据处理类
    /// </summary>
    public class ExperienceBLL : BaseBll<T_HR_EXPERIENCE>
    {
        /// <summary>
        /// 根据简历ID获取所有当前简历的工作经历
        /// </summary>
        /// <param name="resumeID">简历ID为空时获取所有工作经历</param>
        /// <returns>返回工作经历列表</returns>
        public IQueryable<T_HR_EXPERIENCE> GetExperienceAll(string resumeID)
        {
            var ents = from ent in dal.GetObjects().Include("T_HR_RESUME")
                       where string.IsNullOrEmpty(resumeID) || ent.T_HR_RESUME.RESUMEID == resumeID
                       orderby ent.STARTDATE
                       select ent;
            return ents;
        }
        /// <summary>
        /// 添加工作经历
        /// </summary>
        /// <param name="career">工作经历实体</param>
        public int ExperienceAdd(T_HR_EXPERIENCE career)
        {
            int rslCode = 0;
            try
            {               
                if (career != null)
                {
                    T_HR_EXPERIENCE ent = new T_HR_EXPERIENCE();
                    ent.EXPERIENCEID = career.EXPERIENCEID;
                    if (ent.T_HR_RESUMEReference != null)
                    {
                        ent.T_HR_RESUMEReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", career.T_HR_RESUME.RESUMEID);
                    }
                    ent.COMPANYNAME = career.COMPANYNAME;
                    ent.POST = career.POST;
                    ent.SALARY = career.SALARY;
                    ent.STARTDATE = career.STARTDATE;
                    ent.ENDDATE = career.ENDDATE;
                    ent.JOBDESCRIPTION = career.JOBDESCRIPTION;
                    ent.REMARK = career.REMARK;
                    ent.CREATEDATE = System.DateTime.Now;
                    ent.CREATEUSERID = career.CREATEUSERID;
                    dal.Add(ent);

                    rslCode = dal.SaveContextChanges();
                }
                return rslCode;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ExperienceAdd:" + ex.Message);
                throw ex; 
            }
        }
        /// <summary>
        /// 更新工作经历
        /// </summary>
        /// <param name="experience">工作经历列表</param>
        /// <returns>>=0正常,-1异常</returns>
        public int ExperienceUpdate(List<T_HR_EXPERIENCE> experience)
        {
            try
            {
                int rslCode = 0;
                foreach (var career in experience)
                {
                    var ents = from a in dal.GetObjects()
                               where a.EXPERIENCEID == career.EXPERIENCEID
                               select a;
                    //如果有的就更新，反之就增加
                    if (ents.Count() > 0)
                    {
                        var ent = ents.FirstOrDefault();
                        if (ent.T_HR_RESUME != null && ent.T_HR_RESUMEReference != null)
                        {
                            ent.T_HR_RESUMEReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", career.T_HR_RESUME.RESUMEID);
                        }
                        ent.COMPANYNAME = career.COMPANYNAME;
                        ent.POST = career.POST;
                        ent.SALARY = career.SALARY;
                        ent.STARTDATE = career.STARTDATE;
                        ent.ENDDATE = career.ENDDATE;
                        ent.JOBDESCRIPTION = career.JOBDESCRIPTION;
                        ent.REMARK = career.REMARK;
                        ent.UPDATEDATE = System.DateTime.Now;
                        ent.UPDATEUSERID = career.UPDATEUSERID;
                        dal.Update(ent);
                        rslCode = dal.SaveContextChanges();
                    }
                    else
                    {
                        rslCode = ExperienceAdd(career);
                    }
                }
                return rslCode;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ExperienceUpdate:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 删除工作经历信息
        /// </summary>
        /// <param name="experienceIDs">工作经历ID数组</param>
        /// <returns>0以上：正常，-1异常</returns>
        public int ExperienceDelete(List<T_HR_EXPERIENCE> experienceIDs)
        {
            try
            {
                foreach (var experienceID in experienceIDs)
                {
                    var entitys = from ent in dal.GetObjects()
                                  where ent.EXPERIENCEID == experienceID.EXPERIENCEID
                                  select ent;
                    var entity = entitys.Count() > 0 ? entitys.FirstOrDefault() : null;
                    if (entity != null)
                    {
                        dal.DeleteFromContext(entity);
                    }
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                Foundation.Log.Tracer.Debug("ExperienceBLL-ExperienceDelete出错:"+ex.ToString());
                return (-1);
            }
        }
        /// <summary>
        /// 根据简历ID删除工作经历
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>是否成功删除工作经历</returns>
        public int ExperienceDeleteByResumeID(string resumeID)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.T_HR_RESUME.RESUMEID == resumeID
                               select ent);
                if (entitys.Count() > 0)
                {
                    foreach (var entity in entitys)
                    {
                        var ents = from e in dal.GetObjects()
                                   where e.EXPERIENCEID == entity.EXPERIENCEID
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
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ExperienceDeleteByResumeID:" + ex.Message);
                throw ex;
            }
        }
    }
}
