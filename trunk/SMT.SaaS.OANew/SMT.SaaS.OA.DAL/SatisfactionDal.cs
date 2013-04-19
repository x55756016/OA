using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;
using System.Data.Objects;
using SMT.Foundation.Core;
using SMT.SaaS.OA.DAL.Views;
using System.Data.Objects.DataClasses;
using System.Data;

namespace SMT.SaaS.OA.DAL
{
    /// <summary>
    /// 满意度 调查方案
    /// </summary>
    public class SatisfactionDal : CommDaL<T_OA_SATISFACTIONMASTER>
    {
        public int Add_SSurvey(V_Satisfaction v)
        {
            int n = 0;
            base.Add(v.RequireMaster);
            foreach (T_OA_SATISFACTIONDETAIL subjectView in v.SubjectViewList)
            {
                subjectView.T_OA_SATISFACTIONMASTER = base.GetObjectByEntityKey(v.RequireMaster.EntityKey) as T_OA_SATISFACTIONMASTER;
                //base.Add(subjectView);
                base.AddToContext(subjectView);
                
            }
            n = SaveContextChanges();
            return n;
        }
        public bool Add_SurveySatisfactionsMaster(T_OA_SATISFACTIONMASTER key)
        {
           
                if (key != null)
                {
                    base.Add(key);
                    return true;
                }
                else
                {
                    return false;
                }
            
            
        }
        /// <summary>
        /// 更新方案 
        /// </summary>
        /// <param name="requireMasterInfo"></param>
        /// <returns></returns>
        public int Upd_SSurvey(T_OA_SATISFACTIONMASTER requireMasterInfo)
        {
            int n = 0;
            try
            {
                if (requireMasterInfo.EntityKey == null)
                    requireMasterInfo.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_REQUIREMASTER", "SATISFACTIONMASTERID", requireMasterInfo.SATISFACTIONMASTERID);
                T_OA_SATISFACTIONMASTER tmpobj = base.GetObjectByEntityKey(requireMasterInfo.EntityKey) as T_OA_SATISFACTIONMASTER;
                base.Update(requireMasterInfo);
                n = SaveContextChanges();
                n = n == 0 ? 1 : n; // 没有任何更新时，SaveChanges会返回0
            }
            catch (Exception ex)
            {
                return n;
                throw (ex);
            }
            return n;
        }
        /// <summary>
        /// 更新题目
        /// </summary>
        /// <param name="surveySubjectInfo"></param>
        /// <returns></returns>
        public int Upd_SSurveySub(T_OA_SATISFACTIONDETAIL surveySubjectInfo)
        {
            try
            {
                if (surveySubjectInfo.EntityKey == null)
                    surveySubjectInfo.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_SATISFACTIONDETAIL", "SATISFACTIONDETAILID", surveySubjectInfo.SATISFACTIONDETAILID);
                T_OA_SATISFACTIONDETAIL tmpobj = base.GetObjectByEntityKey(surveySubjectInfo.EntityKey) as T_OA_SATISFACTIONDETAIL;
                if (surveySubjectInfo.T_OA_SATISFACTIONMASTERReference.EntityKey == null)
                    surveySubjectInfo.T_OA_SATISFACTIONMASTERReference.EntityKey = new EntityKey("SMT_OA_EFModelContext.T_OA_SATISFACTIONMASTER", "SATISFACTIONMASTERID", surveySubjectInfo.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID);
                tmpobj.T_OA_SATISFACTIONMASTER = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_SATISFACTIONMASTERReference.EntityKey) as T_OA_SATISFACTIONMASTER;
                base.UpdateFromContext(surveySubjectInfo);

                int i = SaveContextChanges();
                if (i < 0)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #region 调查申请
        public int Upd_SSurveyApp(T_OA_SATISFACTIONREQUIRE surveySubjectInfo)
        {
            int n = 0;
            try
            {
                if (surveySubjectInfo.EntityKey == null)
                {
                    surveySubjectInfo.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_SATISFACTIONREQUIRE", "SATISFACTIONREQUIREID", surveySubjectInfo.SATISFACTIONREQUIREID);
                    surveySubjectInfo.T_OA_SATISFACTIONMASTER = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_SATISFACTIONMASTER.EntityKey) as T_OA_SATISFACTIONMASTER;
                    base.Add(surveySubjectInfo);
                }
                else
                {
                    T_OA_SATISFACTIONREQUIRE tmpobj = base.GetObjectByEntityKey(surveySubjectInfo.EntityKey) as T_OA_SATISFACTIONREQUIRE;
                    tmpobj.T_OA_SATISFACTIONMASTER = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_SATISFACTIONMASTERReference.EntityKey) as T_OA_SATISFACTIONMASTER;
                    base.Update(surveySubjectInfo);
                }
                n = SaveContextChanges();
                n = n == 0 ? 1 : n; // 没有任何更新时，SaveChanges会返回0

            }
            catch (Exception ex)
            {
                return 0;
                throw (ex);
            }
            return n;
        }

        #endregion

        #region 调查发布
        public int Upd_SSurveyResult(T_OA_SATISFACTIONDISTRIBUTE surveySubjectInfo)
        {
            int n = 0;
            try
            {
                if (surveySubjectInfo.EntityKey == null)  //添加界面 修改时报错，只会在此出现这种情况
                {
                    surveySubjectInfo.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_SATISFACTIONDISTRIBUTE", "SATISFACTIONDISTRIBUTEID", surveySubjectInfo.SATISFACTIONDISTRIBUTEID);
                    surveySubjectInfo.T_OA_SATISFACTIONREQUIRE = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_SATISFACTIONREQUIRE.EntityKey) as T_OA_SATISFACTIONREQUIRE;
                    base.Add(surveySubjectInfo);
                }
                else //修改界面，进入这里
                {
                    T_OA_SATISFACTIONDISTRIBUTE tmpobj = base.GetObjectByEntityKey(surveySubjectInfo.EntityKey) as T_OA_SATISFACTIONDISTRIBUTE;
                    tmpobj.T_OA_SATISFACTIONREQUIRE = base.GetObjectByEntityKey(surveySubjectInfo.T_OA_SATISFACTIONREQUIREReference.EntityKey) as T_OA_SATISFACTIONREQUIRE;
                    base.Update(surveySubjectInfo);
                }
                n = SaveContextChanges();
                n = n == 0 ? 1 : n; // 没有任何更新时，SaveChanges会返回0
            }
            catch (Exception ex)
            {
                return 0;
                throw (ex);
            }
            return n;
        }
        #endregion
    }
}
