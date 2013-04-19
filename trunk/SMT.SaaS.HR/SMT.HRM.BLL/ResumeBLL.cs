using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL
{
    public class ResumeBLL : BaseBll<T_HR_RESUME>
    {
        /// <summary>
        /// 根据简历ID返回简历实体
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>返回简历实体</returns>
        public T_HR_RESUME GetResumeByid(string resumeID)
        {
            var ents = from ent in dal.GetObjects()
                       where ent.RESUMEID == resumeID
                       select ent;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 修改简历信息
        /// </summary>
        /// <param name="entity">简历实体</param>
        public void ResumeUpdate(T_HR_RESUME entity,ref string strMsg)
        {
            try
            {
                var entTmp = from c in dal.GetObjects()
                             where c.IDCARDNUMBER == entity.IDCARDNUMBER && c.RESUMEID != entity.RESUMEID
                             select c;
                if (entTmp.Count() > 0)
                {
                   // throw new Exception("IDCARDNUMBERREPETITION");
                    strMsg = "IDCARDNUMBERREPETITION";
                    return;
                }
                var ents = from a in dal.GetObjects()
                           where a.RESUMEID == entity.RESUMEID
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_RESUME>(entity, ent);
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ResumeUpdate:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public void ResumeAdd(T_HR_RESUME entity,ref string strMsg)
        {
            try
            {
                var entTmp = from c in dal.GetObjects()
                             where c.IDCARDNUMBER == entity.IDCARDNUMBER
                             select c;
                if (entTmp.Count() > 0)
                {
                    //throw new Exception("IDCARDNUMBERREPETITION");
                    strMsg = "IDCARDNUMBERREPETITION";
                    return;
                }
             //   Utility.CloneEntity<T_HR_RESUME>(entity, ent);
                dal.Add(entity);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ResumeAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 删除简历记录，可同时删除多行记录
        /// </summary>
        /// <param name="blackLists">简历ID数组</param>
        /// <returns>返回0既成功</returns>
        public int ResumeDelete(string[] resumes)
        {
            EducateHistoryBLL educateHistory = new EducateHistoryBLL();
            ExperienceBLL experience = new ExperienceBLL();
            foreach (string id in resumes)
            {
                var ents = from e in dal.GetObjects()
                           where e.RESUMEID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    //删除教育记录
                    educateHistory.EducateHistoryDeleteByResumeID(ent.RESUMEID);
                    //删除工作经历
                    experience.ExperienceDeleteByResumeID(ent.RESUMEID);
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges();
        }

        /// <summary>
        /// 根据身份证号码读取简历基本信息
        /// </summary>
        /// <param name="sNumberId"></param>
        /// <returns></returns>
        public T_HR_RESUME GetResumeByNumber(string sNumberId)
        {
            return dal.GetObjects().FirstOrDefault(s => s.IDCARDNUMBER == sNumberId);
        }
    }
}
