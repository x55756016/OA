//******满意度调查方案BLL**************×
//编写人:勒中玉
//编写时间：2011-6-7
//**************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Log;
using System.Data.Objects.DataClasses;
using System.Data;
using SMT.SaaS.OA.DAL.Views;

namespace SMT.SaaS.OA.BLL
{
    public class SatisfactionSurveyBll : BaseBll<T_OA_SATISFACTIONMASTER>
    {
        #region 满意度调查方案

        #region 增
        /// <summary>
        /// 增加方案和子表
        /// </summary>
        /// <param name="masterEntity">主表带子表数据</param>
        /// <returns>返回是否添加成功</returns>
        public bool AddSatisfactionMaster(T_OA_SATISFACTIONMASTER addMasterEntity)
        {
            bool addFlag = false;
            try
            {
                addFlag = base.Add(addMasterEntity);
                return addFlag;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查方案SatisfactionSurveyBll-AddSatisfactionMaster" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return addFlag;
            }
        }
        #endregion

        //#region 删
        ///// <summary>
        ///// 使用事务批量删除方案和子表
        ///// </summary>
        ///// <param name="masterIdList">主表主键ID集合</param>
        ///// <returns>返回是否删除成功</returns>
        //public bool DelSatisfactionMaster(List<string> masterIdList)
        //{
        //    bool flag = false;
        //    try
        //    {
        //        dal.BeginTransaction();
        //        foreach (string id in masterIdList)
        //        {
        //            T_OA_SATISFACTIONREQUIRE r = new T_OA_SATISFACTIONREQUIRE();
        //            //var xm= from x in dal.GetObjects<T_OA_SATISFACTIONREQUIRE>().Include("T_OA_SATISFACTIONRESULT")
        //            //      .Include("T_OA_SATISFACTIONDISTRIBUTE")
        //            //      select x;
        //            //    var lm=  

        //            //var m=from i in dal.GetObjects<T_OA_SATISFACTIONMASTER>().Include("T_OA_SATISFACTIONDETAIL")
        //            //          .Include("T_OA_SATISFACTIONREQUIRE")
        //            //      join 

        //            var ents = dal.GetObjects<T_OA_SATISFACTIONMASTER>()
        //                    .Include("T_OA_SATISFACTIONREQUIRE")
        //                    .Include("T_OA_SATISFACTIONDETAIL")
        //                    .Where(x => x.SATISFACTIONMASTERID == id)
        //                    .Select(x => x);
        //            var ent = ents != null && ents.Count() > 0 ? ents.FirstOrDefault() : null;
        //            if (ent != null)
        //            {
        //                flag = this.Delete(ent);
        //            }
        //            else
        //            {
        //                flag = false;
        //            }
        //        }
        //        if (flag)
        //        {
        //            dal.CommitTransaction();
        //        }
        //        dal.RollbackTransaction();
        //    }
        //    catch (Exception ex)
        //    {
        //        dal.RollbackTransaction();
        //        flag = false;
        //        Tracer.Debug("满意度调查方案SatisfactionSurveyBll-DelSatisfactionMaster" + System.DateTime.Now.ToString() + " " + ex.ToString());
        //    }
        //    return flag;
        //}
        //#endregion

        #region 改
        /// <summary>
        /// 更新满意度调查方案和子表
        /// </summary>
        /// <param name="UpdEntity">主表带子表数据</param>
        /// <returns>是否修改成功</returns>
        public bool UpdSatisfactionMaster(T_OA_SATISFACTIONMASTER updMasterEntity)
        {
            bool updFlag = false;
            try
            {
                var ent = (from ents in dal.GetObjects()
                           where ents.SATISFACTIONMASTERID == updMasterEntity.SATISFACTIONMASTERID
                           select ents).First();
                if (ent != null)
                {
                    updMasterEntity.EntityKey = updMasterEntity.EntityKey != null ? updMasterEntity.EntityKey : ent.EntityKey;
                    updFlag = base.Update(updMasterEntity) > 0 ? true : false;
                }
                return updFlag;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查方案SatisfactionSurveyBll-UpdSatisfactionMaster" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return updFlag;
            }
        }
        #endregion

        #region 查
        /// <summary>
        /// 根据审核条件,创建日期获取满意度调查方案集合
        /// </summary>
        /// <param name="pageCount">页面当前数目</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页面项的数目</param>
        /// <param name="checkstate">审核状态</param>
        /// <param name="dateTimes">开始与结束时间</param>
        /// <returns>返回符合条件的方案</returns>
        public IQueryable<V_Satisfactions> GetMasterByCheckstateAndDate(int pageCount, int pageIndex, int pageSize, string checkstate, DateTime[] dateTimes)
        {
            DateTime startTime = dateTimes[0];
            DateTime endTime = dateTimes[1];
            try
            {
                var entMaster = from ents in dal.GetObjects()
                                where ents.CHECKSTATE == checkstate && ents.CREATEDATE >= startTime && ents.CREATEDATE <= endTime
                                select new V_Satisfactions
                                {
                                    Satisfactionmasterid = ents.SATISFACTIONMASTERID,
                                    SurveyTitle = ents.SATISFACTIONTITLE,
                                    Content = ents.CONTENT,
                                    OwnerName = ents.OWNERNAME,
                                    CreateDate = ents.CREATEDATE,
                                };
                if (entMaster != null)
                {
                    entMaster = Utility.Pager<V_Satisfactions>(entMaster, pageIndex, pageSize, ref pageCount);
                    return entMaster != null && entMaster.Count() > 0 ? entMaster : null;
                }
                return null;

            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查方案SatisfactionSurveyBll-GetMasterByCheckstateAndDate" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 子页面加载时取数据
        /// </summary>
        /// <param name="appId">申请表主键</param>
        /// <returns>对应主键的数据</returns>
        public T_OA_SATISFACTIONMASTER GetSatisfactionMasterChild(string masterId)
        {
            try
            {
                var childData = (from data in dal.GetObjects<T_OA_SATISFACTIONMASTER>()
                                 .Include("T_OA_SATISFACTIONDETAIL")
                                 where data.SATISFACTIONMASTERID == masterId
                                 select data).Single();
                return childData;
            }
            catch (Exception ex)
            {
                Tracer.Debug("满意度调查方案SatisfactionAppBll-UpdSatisfactionApp" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 子页面载入时查询数据
        /// </summary>
        /// <param name="primaryKey">主键ID</param>
        /// <returns>返回子页面需用数据</returns>
        public IQueryable<T_OA_SATISFACTIONMASTER> GetMasterInfo(string primaryKey)
        {
            IQueryable<T_OA_SATISFACTIONMASTER> masterInfo = null;
            try
            {
                var ent = dal.GetObjects<T_OA_SATISFACTIONMASTER>()
                               .Include("T_OA_SATISFACTIONDETAIL")
                               .Where(x => x.SATISFACTIONMASTERID == primaryKey)
                               .Select(y => y);
                masterInfo = ent;
            }
            catch (Exception ex)
            {
                masterInfo = null;
                Tracer.Debug("满意度调查方案SatisfactionSurveyBll-GetMasterInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
            }
            return masterInfo != null ? masterInfo : null;
        }
        #endregion


        #endregion

    }
}
