using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Common;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public class CheckProjectSetBLL : BaseBll<T_HR_CHECKPROJECTSET>
    {
        /// <summary>
        /// 更新考核项目
        /// </summary>
        /// <param name="entity">考核项目实体</param>
        public void CheckProjectSetUpdate(T_HR_CHECKPROJECTSET entity)
        {
            try
            {
                #region
                //var ent = dal.GetObjects().FirstOrDefault(s => s.CHECKPROJECTID == entity.CHECKPROJECTID);
                //if (ent != null)
                //{
                //    ent.CHECKPROJECT = entity.CHECKPROJECT;
                //    ent.CHECKPROJECTSCORE = entity.CHECKPROJECTSCORE;
                //    ent.REMARK = entity.REMARK;
                //    ent.UPDATEUSERID = entity.UPDATEUSERID;
                //    ent.UPDATEDATE = entity.UPDATEDATE;
                //    dal.Update(ent);
                //}
                #endregion

                dal.Update(entity);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CheckProjectSetUpdate:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 删除考核项目、对应项目点、考核等级。
        /// </summary>
        /// <param name="projectSetID">考核项目ID</param>
        /// <returns>是否成功删除</returns>
        public int CheckProjectSetDelete(string projectSetID)
        {
            var ents = from a in dal.GetObjects()
                       where a.CHECKPROJECTID == projectSetID
                       select a;
            if (ents.Count() > 0)
            {
                //考核点
                var points = from a in dal.GetObjects<T_HR_CHECKPOINTSET>().Include("T_HR_CHECKPROJECTSET")
                             where a.T_HR_CHECKPROJECTSET.CHECKPROJECTID == projectSetID
                             select a;
                foreach (var point in points)
                {
                    //考核点等级
                    var pointLevel = from a in dal.GetObjects<T_HR_CHECKPOINTLEVELSET>().Include("T_HR_CHECKPOINTSET")
                                     where a.T_HR_CHECKPOINTSET.CHECKPOINTSETID == point.CHECKPOINTSETID
                                     select a;
                    foreach (var ent in pointLevel)
                    {
                        dal.DeleteFromContext(ent);
                    }
                    dal.DeleteFromContext(point);
                }
                dal.DeleteFromContext(ents.FirstOrDefault());
                return dal.SaveContextChanges();
            }
            return -1;
        }
        /// <summary>
        /// 根据考核项目ID获取考核项目信息
        /// </summary>
        /// <param name="projectSetID">考核项目ID</param>
        /// <returns></returns>
        public T_HR_CHECKPROJECTSET GetCheckProjectSetByID(string projectSetID)
        {
            return dal.GetObjects().FirstOrDefault(s => s.CHECKPROJECTID == projectSetID);
        }
        /// <summary>
        /// 根据考核员工类型，获取考核点信息
        /// </summary>
        /// <param name="pointSetID">考核类型</param>
        /// <returns></returns>
        public List<V_PROJECTPOINT> GetCheckProjectSetByType(string sType)
        {
            List<V_PROJECTPOINT> projectSet = new List<V_PROJECTPOINT>();
            var ents = (from a in dal.GetObjects<T_HR_CHECKPOINTSET>().Include("T_HR_CHECKPROJECTSET")
                        where a.CHECKEMPLOYEETYPE == sType || a.CHECKEMPLOYEETYPE == "2"
                        select a.T_HR_CHECKPROJECTSET.CHECKPROJECTID).Distinct();
            foreach (var ent in ents)
            {
                var temp = dal.GetObjects().FirstOrDefault(s => s.CHECKPROJECTID == ent);
                if (temp != null)
                {
                    V_PROJECTPOINT tempProject = new V_PROJECTPOINT();
                    tempProject.CheckProject = temp.CHECKPROJECT;
                    tempProject.CheckProjectScore = temp.CHECKPROJECTSCORE;
                    List<V_POINTSET> pointList = new List<V_POINTSET>();

                    //获取考核点
                    var entity = from a in dal.GetObjects<T_HR_CHECKPOINTSET>()
                                 where a.T_HR_CHECKPROJECTSET.CHECKPROJECTID == temp.CHECKPROJECTID
                                 select a;
                    if (entity != null)
                    {
                        foreach (var tem in entity)
                        {
                            V_POINTSET tempPointSet = new V_POINTSET();
                            var tempEnt = dal.GetObjects<T_HR_ASSESSMENTFORMDETAIL>().Include("T_HR_CHECKPOINTSET").FirstOrDefault(
                                          s => s.T_HR_CHECKPOINTSET.CHECKPOINTSETID == tem.CHECKPOINTSETID);
                            if (tempEnt != null)
                            {
                                tempPointSet.FirstScore = tempEnt.FIRSTSCORE.Value;
                                tempPointSet.SecondScore = tempEnt.SECONDSCORE.Value;
                            }
                            else
                            {
                                tempPointSet.FirstScore = 0;
                                tempPointSet.SecondScore = 0;
                            }
                            tempPointSet.CheckProjectID = temp.CHECKPROJECTID;
                            tempPointSet.CheckPointSetID = tem.CHECKPOINTSETID;
                            tempPointSet.CheckPointScore = tem.CHECKPOINTSCORE;
                            tempPointSet.CheckPointDes = tem.CHECKPOINTDES;
                            tempPointSet.CheckPoint = tem.CHECKPOINT;
                            tempPointSet.CheckEmployeeType = tem.CHECKEMPLOYEETYPE;
                            //考核等级赋值
                            var e = from a in dal.GetObjects<T_HR_CHECKPOINTLEVELSET>().Include("T_HR_CHECKPOINTSET")
                                    where a.T_HR_CHECKPOINTSET.CHECKPOINTSETID == tem.CHECKPOINTSETID
                                    orderby a.POINTSCORE descending
                                    select a;

                            tempPointSet.LeavelList = e.Count() > 0 ? e.ToList() : null;
                            pointList.Add(tempPointSet);
                        }
                        tempProject.PointList = pointList;
                    }
                    projectSet.Add(tempProject);
                }
            }
            return projectSet;
        }
    }
}
