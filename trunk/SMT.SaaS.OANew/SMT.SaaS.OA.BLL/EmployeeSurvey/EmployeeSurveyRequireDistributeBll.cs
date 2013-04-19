using System;
using System.Linq;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using SMT.Foundation.Log;
using System.Data.Objects;

namespace SMT.SaaS.OA.BLL
{ 
    public class EmployeeSurveyRequireDistributeBll : BaseBll<T_OA_REQUIREDISTRIBUTE>
    {
        #region 新增
        /// <summary>
        /// 新增员工调查发布申请和分发范围
        /// </summary>
        public bool AddRequireDistribute(V_EmployeeSurveyRequireDistribute AddDistribute)
        {
            try
            {
                base.BeginTransaction();
                string _entityKey=AddDistribute.RequireId;
                if (AddDistribute.requiredistributeEntity.T_OA_REQUIREReference.EntityKey == null)
                {
                    AddDistribute.requiredistributeEntity.T_OA_REQUIREReference.EntityKey = Utility.AddEntityKey("T_OA_REQUIRE", "REQUIREID", _entityKey);
                }
                bool add = base.Add(AddDistribute.requiredistributeEntity);
                if (add)
                {
                    foreach (var item in AddDistribute.distributeuserList)
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
                Tracer.Debug("员工调查发布申请EmployeeSurveyRequireDistributeBll-AddRequireDistribute" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }

        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改员工调查发布申请和分发范围
        /// </summary>
        public bool UpdRequireDistribute(V_EmployeeSurveyRequireDistribute updDistribute)
        {
            try
            {
                string _requireID = updDistribute.RequireId;
                string _childID = updDistribute.requiredistributeEntity.REQUIREDISTRIBUTEID;
                base.BeginTransaction();
                foreach (var ents in updDistribute.oldDistributeuserList)
                {
                    var ent = from chlidData in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                              where new { chlidData.MODELNAME, chlidData.FORMID, chlidData.VIEWER, chlidData.VIEWTYPE } == new { ents.MODELNAME, ents.FORMID, ents.VIEWER, ents.VIEWTYPE }
                              select chlidData;
                    if (ent.Count()> 0)
                    {
                        var _delData = ent.FirstOrDefault();
                        dal.DeleteFromContext(_delData);
                    }
                }
                int _delFlag = dal.SaveContextChanges();
                if (_delFlag > 0)
                {
                    var data = from items in dal.GetObjects<T_OA_REQUIREDISTRIBUTE>()
                               where items.REQUIREDISTRIBUTEID == _childID
                               select items;
                    if (data.Count() >0)
                    {
                        //建立entityKey,表示数据库不更新此字段(主键不允许更新)
                        updDistribute.requiredistributeEntity.EntityKey = Utility.AddEntityKey("T_OA_REQUIREDISTRIBUTE", "REQUIREDISTRIBUTEID", _childID);
                        if (updDistribute.requiredistributeEntity.T_OA_REQUIREReference.EntityKey == null)
                        {
                            updDistribute.requiredistributeEntity.T_OA_REQUIREReference.EntityKey = Utility.AddEntityKey("T_OA_REQUIRE", "REQUIREID", _requireID);
                        }
                        int updFlag = dal.Update(updDistribute.requiredistributeEntity);
                        if (updFlag > 0)
                        {
                            foreach (var users in updDistribute.distributeuserList)
                            {
                                dal.Add(users);
                            }
                            base.CommitTransaction();
                            return true;
                        }

                    }
                }
                base.RollbackTransaction();
                return false;
            }
            catch (Exception ex)
            {
                base.RollbackTransaction();
                Tracer.Debug("员工调查发布申请EmployeeSurveyRequireDistributeBll-UpdRequireDistribute" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        #endregion

        #region 子页面加载查询
        /// <summary>
        ///  员工调查发布申请子页面加载取数据
        /// </summary>
        public V_EmployeeSurveyRequireDistribute GetDistributeData(string distributeId)
        {
            try
            {             
                var data = from ent in dal.GetObjects<T_OA_REQUIREDISTRIBUTE>()
                           .Include("T_OA_REQUIRE")
                           join users in dal.GetObjects<T_OA_DISTRIBUTEUSER>()
                           on ent.REQUIREDISTRIBUTEID equals users.FORMID into list
                           where ent.REQUIREDISTRIBUTEID == distributeId
                           select new V_EmployeeSurveyRequireDistribute
                           {
                               distributeuserList=list,
                               requiredistributeEntity=ent,
                               RequireId=ent.T_OA_REQUIRE.REQUIREID,
                               RequireDistributeId=ent.REQUIREDISTRIBUTEID
                           };
                return data.Count()>0?data.FirstOrDefault():null;
                                  
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查发布申请EmployeeSurveyRequireDistributeBll-GetDistributeData" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        #endregion

        #region 查询结果
        /// <summary>
        /// 员工调查发布申请管理页面显示结果用
        /// </summary>
        public IQueryable<V_EmployeeSurveyRequireDistribute> GetResultData(string requireId)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                base.RollbackTransaction();
                Tracer.Debug("员工调查发布申请EmployeeSurveyRequireDistributeBll-GetDistributeData" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 查看详细
        #endregion
    }
}
