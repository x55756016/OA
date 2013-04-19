using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL
{
    public class CheckPointSetBLL : BaseBll<T_HR_CHECKPOINTSET>
    {
        /// <summary>
        /// 根据考核项目ID获取考核点信息
        /// </summary>
        /// <returns></returns>
        public List<T_HR_CHECKPOINTSET> GetCheckPointSetByPrimaryID(string projectSetID, string type)
        {
            var ent = from a in dal.GetObjects()
                      where a.T_HR_CHECKPROJECTSET.CHECKPROJECTID == projectSetID
                      && (a.CHECKEMPLOYEETYPE == type || a.CHECKEMPLOYEETYPE == "2")
                      select a;
            return ent.Count() > 0 ? ent.ToList() : null;
        }
        /// <summary>
        /// 添加考核项目信息
        /// </summary>
        /// <param name="entity">考核项目实体</param>
        public void CheckPointSetAdd(T_HR_CHECKPOINTSET entity)
        {
            try
            {
                var ent = new T_HR_CHECKPOINTSET();
                ent.CHECKPOINTSETID = entity.CHECKPOINTSETID;
                ent.T_HR_CHECKPROJECTSETReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CHECKPROJECTSET", "CHECKPROJECTID", entity.T_HR_CHECKPROJECTSET.CHECKPROJECTID);
                ent.CHECKEMPLOYEETYPE = entity.CHECKEMPLOYEETYPE;
                ent.CHECKPOINT = entity.CHECKPOINT;
                ent.CHECKPOINTDES = entity.CHECKPOINTDES;
                ent.CHECKPOINTSCORE = entity.CHECKPOINTSCORE;
                ent.REMARK = entity.REMARK;
                ent.CREATEDATE = entity.CREATEDATE;
                ent.CREATEUSERID = entity.CREATEUSERID;
                dal.Add(ent);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CheckPointSetAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 更新考核项目实体
        /// </summary>
        /// <param name="entity"></param>
        public void CheckPointSetUpdate(T_HR_CHECKPOINTSET entity)
        {
            try
            {
                #region back
                //var ent = dal.GetObjects().FirstOrDefault(s => s.CHECKPOINTSETID == entity.CHECKPOINTSETID);
                //if (ent != null)
                //{
                //    if (ent.T_HR_CHECKPROJECTSET != null)
                //    {
                //        ent.T_HR_CHECKPROJECTSETReference.EntityKey =
                //                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CHECKPROJECTSET", "CHECKPROJECTID", entity.T_HR_CHECKPROJECTSET.CHECKPROJECTID);
                //    }
                //    ent.CHECKEMPLOYEETYPE = entity.CHECKEMPLOYEETYPE;
                //    ent.CHECKPOINT = entity.CHECKPOINT;
                //    ent.CHECKPOINTDES = entity.CHECKPOINTDES;
                //    ent.CHECKPOINTSCORE = entity.CHECKPOINTSCORE;
                //    ent.REMARK = entity.REMARK;
                //    ent.UPDATEUSERID = entity.UPDATEUSERID;
                //    ent.UPDATEDATE = entity.UPDATEDATE;
                //    dal.Update(ent);
                //}
                #endregion

                if (entity.T_HR_CHECKPROJECTSET != null)
                {
                    entity.T_HR_CHECKPROJECTSETReference.EntityKey =
                                       new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CHECKPROJECTSET", "CHECKPROJECTID", entity.T_HR_CHECKPROJECTSET.CHECKPROJECTID);
                    entity.T_HR_CHECKPROJECTSET.EntityKey =
                                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CHECKPROJECTSET", "CHECKPROJECTID", entity.T_HR_CHECKPROJECTSET.CHECKPROJECTID);
                }
                dal.Update(entity);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CheckPointSetUpdate:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 删除考核点和考核等级
        /// </summary>
        /// <param name="pointID">考核ID</param>
        /// <returns>返回0则成功</returns>
        public int CheckPointSetDelete(string pointID)
        {
            var ents = from a in dal.GetObjects()
                       where a.CHECKPOINTSETID == pointID
                       select a;
            if (ents.Count() > 0)
            {
                //删除考核等级
                var pointLevel = from a in dal.GetObjects<T_HR_CHECKPOINTLEVELSET>().Include("T_HR_CHECKPOINTSET")
                                 where a.T_HR_CHECKPOINTSET.CHECKPOINTSETID == pointID
                                 select a;
                foreach (var ent in pointLevel)
                {

                    dal.DeleteFromContext(ent);

                }
                //删除考核点项目
                dal.DeleteFromContext(ents.FirstOrDefault());
                return dal.SaveContextChanges();
            }
            return -1;
        }
        /// <summary>
        /// 根据考核项目点ID，获取考核点信息
        /// </summary>
        /// <param name="pointSetID">考核ID</param>
        /// <returns></returns>
        public T_HR_CHECKPOINTSET GetCheckPointSetByID(string pointSetID)
        {
            return dal.GetObjects().FirstOrDefault(s => s.CHECKPOINTSETID == pointSetID);
        }

        /// <summary>
        /// 根据员工类型，考核项目ID获取考核点可用分数
        /// </summary>
        /// <param name="type">员工类型</param>
        /// <param name="projectSetID">考核项目ID</param>
        /// <returns>返回可用的考核点分数</returns>
        public int GetCheckPointAvailable(string employeeTyee, string projectSetID)
        {
            int projectScore = 0, pointScore = 0;
            //获取考核项目的分数
            var ents = (from a in dal.GetObjects<T_HR_CHECKPROJECTSET>()
                        where a.CHECKPROJECTID == projectSetID
                        select a.CHECKPROJECTSCORE).FirstOrDefault();
            if (ents != null)
            {
                projectScore = Convert.ToInt32(ents.Value);
            }
            //获取此项目考核点的所有分数
            ents = (from a in dal.GetObjects().Include("T_HR_CHECKPROJECTSET")
                    where (a.CHECKEMPLOYEETYPE == employeeTyee || a.CHECKEMPLOYEETYPE == "2") && a.T_HR_CHECKPROJECTSET.CHECKPROJECTID == projectSetID
                    select a.CHECKPOINTSCORE).Sum();
            if (ents != null)
            {
                pointScore = Convert.ToInt32(ents.Value);
            }
            return projectScore - pointScore;
        }
        /// <summary>
        /// 根据员工类型获取总分
        /// </summary>
        /// <param name="employeeType">员工类型</param>
        /// <returns>员工类型的总分</returns>
        public int GetCheckPointByEmployeeTypeSum(string employeeType)
        {
            var ents = (from a in dal.GetObjects()
                        where a.CHECKEMPLOYEETYPE == employeeType
                        select a.CHECKPOINTSCORE).Sum();
            if (ents == null)
            {
                return 0;
            }
            return Convert.ToInt32(ents.Value);
        }
    }
}
