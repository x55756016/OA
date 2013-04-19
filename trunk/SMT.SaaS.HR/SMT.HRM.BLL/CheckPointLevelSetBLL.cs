using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL
{
    public class CheckPointLevelSetBLL:BaseBll<T_HR_CHECKPOINTLEVELSET>
    {
        /// <summary>
        /// 根据考核点ID获取考核点等级信息
        /// </summary>
        /// <param name="pointID">考核点ID</param>
        /// <returns></returns>
        public List<T_HR_CHECKPOINTLEVELSET> GetCheckPointLevelSetByPrimaryID(string pointID)
        {
            var ent = from a in dal.GetObjects()
                      where a.T_HR_CHECKPOINTSET.CHECKPOINTSETID == pointID
                      orderby a.POINTSCORE descending
                      select a;
            return ent.Count() > 0 ? ent.ToList() : null;
        }

        /// <summary>
        /// 一次性添加多项记录
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public bool CheckPointLevelSetAdd(List<T_HR_CHECKPOINTLEVELSET> entitys)
        {
            try
            {
                foreach (var entity in entitys)
                {
                    var ent = new T_HR_CHECKPOINTLEVELSET();
                    ent.POINTLEVEID = entity.POINTLEVEID;
                    ent.T_HR_CHECKPOINTSETReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CHECKPOINTSET", "CHECKPOINTSETID", entity.T_HR_CHECKPOINTSET.CHECKPOINTSETID);
                    ent.POINTLEVEL = entity.POINTLEVEL;
                    ent.POINTSCORE = entity.POINTSCORE;
                    ent.REMARK = entity.REMARK;
                    ent.CREATEUSERID = entity.CREATEUSERID;
                    ent.CREATEDATE = entity.CREATEDATE;
                   // dal.Add(ent);
                   // DataContext.AddObject("T_HR_CHECKPOINTLEVELSET", ent);
                    dal.AddToContext(ent);
                }
                //return (DataContext.SaveChanges() > 0);
                return (dal.SaveContextChanges()>0);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CheckPointLevelSetAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 更新考核等级信息
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public bool CheckPointLevelSetUpdate(List<T_HR_CHECKPOINTLEVELSET> entitys)
        {
            try
            {
                #region back
                //foreach (var entity in entitys)
                //{
                //    var ent = dal.GetObjects().FirstOrDefault(s => s.POINTLEVEID == entity.POINTLEVEID);
                //    if (ent != null)
                //    {
                //        if (entity.T_HR_CHECKPOINTSET != null)
                //        {
                //            ent.T_HR_CHECKPOINTSETReference.EntityKey =
                //               new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CHECKPOINTSET", "CHECKPOINTSETID", entity.T_HR_CHECKPOINTSET.CHECKPOINTSETID);
                //        }
                //        ent.POINTLEVEL = entity.POINTLEVEL;
                //        ent.POINTSCORE = entity.POINTSCORE;
                //        ent.REMARK = entity.REMARK;
                //        ent.UPDATEUSERID = entity.UPDATEUSERID;
                //        ent.UPDATEDATE = entity.UPDATEDATE;
                //        //DataContext.AddObject("T_HR_CHECKPOINTLEVELSET", ent);
                //       // dal.AddToContext(ent);
                //        dal.UpdateFromContext(ent);
                       
                //    }
                //}
                #endregion
                foreach (var entity in entitys)
                {
                    //var ent = dal.GetObjects().FirstOrDefault(s => s.POINTLEVEID == entity.POINTLEVEID);
                    //if (ent != null)
                    //{
                        if (entity.T_HR_CHECKPOINTSET != null)
                        {
                            entity.T_HR_CHECKPOINTSETReference.EntityKey =
                               new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CHECKPOINTSET", "CHECKPOINTSETID", entity.T_HR_CHECKPOINTSET.CHECKPOINTSETID);
                            entity.T_HR_CHECKPOINTSET.EntityKey =
                             new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_CHECKPOINTSET", "CHECKPOINTSETID", entity.T_HR_CHECKPOINTSET.CHECKPOINTSETID);
                        }
                        entity.POINTLEVEL = entity.POINTLEVEL;
                        entity.POINTSCORE = entity.POINTSCORE;
                        entity.REMARK = entity.REMARK;
                        entity.UPDATEUSERID = entity.UPDATEUSERID;
                        entity.UPDATEDATE = entity.UPDATEDATE;
                        //DataContext.AddObject("T_HR_CHECKPOINTLEVELSET", ent);
                        // dal.AddToContext(ent);
                        dal.UpdateFromContext(entity);

                   // }
                }
               // return (DataContext.SaveChanges() > 0);
                return dal.SaveContextChanges()>0;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CheckPointLevelSetUpdate:" + ex.Message);
                throw ex;
            }
        }
        public bool CheckPointLevelSetDelete(string pointID)
        {
            var ents = from q in dal.GetObjects().Include("T_HR_CHECKPOINTSET")
                       where q.T_HR_CHECKPOINTSET.CHECKPOINTSETID == pointID
                       select q;
            foreach (var ent in ents)
            {
                dal.DeleteFromContext(ent);
            }
            return (dal.SaveContextChanges() > 0);
        }
    }
}
