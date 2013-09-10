//******满意度调查发布申请BLL**************×
//编写人:勒中玉
//编写时间：2011-6-7
//**************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using System.Collections.ObjectModel;
using SMT.Foundation.Log;
using SMT.SaaS.OA.DAL.Views;

namespace SMT.SaaS.OA.BLL
{
    public class SatisfactionDistributeBll : BaseBll<T_OA_SATISFACTIONDISTRIBUTE>
    {
         
        #region 增
        /// <summary>
        /// 新增满意度调查发布申请
        /// </summary>
        public bool AddSatisfactionDistribute(V_Satisfactions addView)
        {
            try
            {
                base.BeginTransaction();
                string _foreignKey = addView.Satisfactionrequireid;
                if (addView.disibuteEntity.T_OA_SATISFACTIONREQUIREReference.EntityKey== null)
                {
                    addView.requireEntity.T_OA_SATISFACTIONMASTERReference.EntityKey = Utility.AddEntityKey("T_OA_SATISFACTIONREQUIRE", "SATISFACTIONREQUIREID", _foreignKey);
                }
                bool add = base.Add(addView.disibuteEntity);
                if (add)
                {
                    foreach (var item in addView.distributeuserList)
                    {
                        dal.Add(item);
                    }
                    base.CommitTransaction();
                    return true;
                }
                base.RollbackTransaction();
                return false;
            }
            catch (Exception ex)
            {
                base.RollbackTransaction();
                Tracer.Debug("满意度调查发布申请SatisfactionDistributeBll-AddSatisfactionDistribute" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        #endregion


        #region 该
        /// <summary>
        /// 更新满意度调查发布申请
        /// </summary>
        public bool UpdSatisfactionDistribute(V_Satisfactions updView)
        {
            try
            {
                string _foreignKey = updView.disibuteEntity.SATISFACTIONDISTRIBUTEID;
                string _primeKey = updView.Satisfactionrequireid;
                base.BeginTransaction();
                foreach (var ents in updView.oldDistributeuserList)
                {
                    var ent = from chlidData in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                              where new { chlidData.MODELNAME, chlidData.FORMID, chlidData.VIEWER, chlidData.VIEWTYPE } == new { ents.MODELNAME, ents.FORMID, ents.VIEWER, ents.VIEWTYPE }
                              select chlidData;
                    if (ent.Count() > 0)
                    {
                        var _delData = ent.FirstOrDefault();
                        dal.DeleteFromContext(_delData);
                    }
                }
                int _delFlag = dal.SaveContextChanges();
                if (_delFlag > 0)
                {
                    foreach (var ents in updView.distributeuserList)
                    {
                        dal.Add(ents);
                    } 
                     updView.disibuteEntity.EntityKey = Utility.AddEntityKey("T_OA_SATISFACTIONDISTRIBUTE", "SATISFACTIONDISTRIBUTEID", _primeKey);
                     if (updView.disibuteEntity.T_OA_SATISFACTIONREQUIREReference.EntityKey == null)
                     {
                         updView.disibuteEntity.T_OA_SATISFACTIONREQUIREReference.EntityKey = Utility.AddEntityKey("T_OA_SATISFACTIONREQUIRE", "SATISFACTIONREQUIREID", _foreignKey);
                     }
                     if (base.Add(updView.disibuteEntity))
                     {
                         base.CommitTransaction();
                         return true;
                     }
                }
                base.RollbackTransaction();
                return false;
            }

            catch (Exception ex)
            {
                base.RollbackTransaction();
                Tracer.Debug("满意度调查发布申请SatisfactionDistributeBll-UpdSatisfactionDistribute" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        #endregion

       

        #region 查
        /// <summary>
        /// 发布申请子页面加载时取数据
        /// </summary>
        /// <param name="distributeId">发布申请表主键</param>
        /// <returns>对应的数据</returns>
        public V_Satisfactions GetSatisfactionDistributeChild(string distributeId)
        {
            try
            {
                var disibuteEnt = from ent in dal.GetObjects<T_OA_SATISFACTIONDISTRIBUTE>()
                                   .Include("T_OA_SATISFACTIONREQUIRE")
                                   join ents in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                                   on ent.SATISFACTIONDISTRIBUTEID equals ents.FORMID into list
                                   where ent.SATISFACTIONDISTRIBUTEID == distributeId
                                   select new V_Satisfactions
                                   {
                                      disibuteEntity=ent,
                                      distributeuserList=list,
                                      SatisfactiondistrbuteidDistrbuteid=ent.SATISFACTIONDISTRIBUTEID,
                                      Satisfactionrequireid=ent.T_OA_SATISFACTIONREQUIRE.SATISFACTIONREQUIREID
                                   };
                return disibuteEnt != null && disibuteEnt.Count() > 0 ? disibuteEnt.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查发布申请SatisfactionDistributeBll-GetSatisfactionDistributeChild" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion
       
    }

}
