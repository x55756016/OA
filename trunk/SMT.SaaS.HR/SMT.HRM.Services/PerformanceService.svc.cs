/// <summary>
/// Log No.： 1
/// Modify Desc： 使用V_KPIRECORD（含用户和业务信息），V_PERFORMANCERECORD（含员工信息），添加KPIScorePost方法
///               改stepID为stepCode（当流程中有重复的步骤时，此方法不可取），接口专用，改实例模式
/// Modifier： 冉龙军
/// Modify Date： 2010-08-10
/// </summary>
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;

using SMT_HRM_EFModel;
using SMT.HRM.BLL;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.IO;
using System.Xml;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;

namespace SMT.HRM.Services
{
    // 1s 冉龙军
    //[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    // 1e
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PerformanceService
    {
        #region KPI类别、KPI点、KPI打分

        #region 考核评分服务
        // 1s 冉龙军
        /// <summary>
        /// 记录业务ID和流程模块关联ID（表单提交）
        /// </summary>
        /// <param name="BusinessCodePost">业务ID</param>
        /// <param name="RelationIDPost">关联ID</param>
        /// <param name="CreateCompanyIDPost">公司ID</param>
        /// <param name="CreateDepartmentIDPost">部门ID</param>
        /// <param name="CreatePostIDPost">岗位ID</param>
        /// <param name="CreateUserIDPost">用户ID</param>
        /// <returns>影响记录数量</returns>
        [OperationContract]
        public int KPIScorePost(string BusinessCodePost, string RelationIDPost, string CreateCompanyIDPost, string CreateDepartmentIDPost, string CreatePostIDPost, string CreateUserIDPost)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                using (KPIPointBll pbll = new KPIPointBll())
                {
                    List<T_HR_KPIPOINT> point = pbll.GetKPIPointListByBusinessCode(RelationIDPost);
                    if (point != null)
                    {
                        foreach (T_HR_KPIPOINT p in point)
                        {
                            T_HR_KPIRECORD record = new T_HR_KPIRECORD();
                            record.BUSINESSCODE = BusinessCodePost; //业务ID（FormID）
                            record.CREATECOMPANYID = CreateCompanyIDPost; //公司ID
                            record.CREATEDEPARTMENTID = CreateDepartmentIDPost; //部门ID
                            record.CREATEPOSTID = CreatePostIDPost; //岗位ID
                            record.CREATEUSERID = CreateUserIDPost; //用户ID
                            record.CREATEDATE = System.DateTime.Now; //新建时间
                            record.KPIDESCRIPTION = p.KPIPOINTREMARK;//kpi描述
                            record.FLOWDESCRIPTION = p.FLOWID;//流程或任务描述
                            record.T_HR_KPIPOINT = new T_HR_KPIPOINT();
                            //step
                            record.STEPDETAILCODE = p.STEPID;
                            //断开外键
                            // Utility.RefreshEntity(record);
                            record.T_HR_KPIPOINT.KPIPOINTID = p.KPIPOINTID; //KPI点ID
                            // record.T_HR_KPIPOINT = p;
                            record.SYSTEMWEIGHT = p.T_HR_SCORETYPE.SYSTEMWEIGHT; //系统打分权重
                            record.MANUALWEIGHT = p.T_HR_SCORETYPE.MANUALWEIGHT; //手动打分权重
                            record.RANDOMWEIGHT = p.T_HR_SCORETYPE.RANDOMWEIGHT; //抽查打分权重
                            record.KPIRECORDID = System.Guid.NewGuid().ToString();
                            bll.KPIRecordAdd(record); //添加KPI明细记录
                            SMT.Foundation.Log.Tracer.Debug("考核记录ID：" + record.KPIRECORDID);
                        }
                    }
                    return 1;
                }
            }
        }

        // 1e
        #region 系统评分服务

        /// <summary>
        /// 根据流程和业务的信息，进行系统评分
        /// </summary>
        /// <param name="companyID">公司ID</param>
        /// <param name="modelRelationID">业务ID</param>
        /// <param name="flowCode">流程Code</param>
        /// <param name="stepCode">步骤Code</param>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程单号</param>
        /// <param name="stepID">步骤单号</param>
        /// <param name="lastStepDate">前一步完成时间，没有前一步写“null”</param>
        /// <param name="stepDate">当前步骤完成时间</param>
        /// <param name="AppraiseeID">被考核人ID</param>
        /// <returns>系统评分结果</returns>
        [OperationContract]
        public int KPISystemScore(string companyID, string modelRelationID, string flowCode, string stepCode, string formCode,
            string flowID, string stepID, DateTime lastStepDate, DateTime stepDate, string AppraiseeID)
        {
            T_HR_KPIPOINT point = GetKPIPoint(companyID, modelRelationID, flowCode, stepCode);
            if (point != null)
            {
                if (point.T_HR_SCORETYPE != null)
                {
                    if (point.T_HR_SCORETYPE.ISSYSTEMSCORE == "1")
                    {
                        //SMT.Foundation.Log.Tracer.Debug("时间：" + System.DateTime.Now.ToString() + "被考核人：" + AppraiseeID + "是否系统打分：1" + "statecode" + stepID);
                        return KPISystemScoreByKPIPoint(point, formCode, flowID, stepID, lastStepDate, stepDate, AppraiseeID);
                    }
                    else
                    {
                        //SMT.Foundation.Log.Tracer.Debug("时间：" + System.DateTime.Now + "被考核人：" + AppraiseeID + "是否系统打分：0");
                        return 0;
                    }
                }
                else
                {
                    //SMT.Foundation.Log.Tracer.Debug("时间：" + System.DateTime.Now.ToString() + "被考核人：" + AppraiseeID + "评分标准为空");
                    return 0;
                }
            }
            else
            {
                //SMT.Foundation.Log.Tracer.Debug("时间：" + System.DateTime.Now.ToString() + "被考核人：" + AppraiseeID + "考核点为空" + " companyID:" + companyID + " modelRelationID:" + modelRelationID + " flowcode:" + flowCode + " stepCode:" + stepCode);
                return 0;
            }

        }

        /// <summary>
        /// 根据KPI点，进行系统评分
        /// </summary>
        /// <param name="kpiPoint">KPI点</param>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程单号</param>
        /// <param name="stepID">步骤单号</param>
        /// <param name="lastStepDate">前一步完成时间，没有前一步写“null”</param>
        /// <param name="stepDate">当前步骤完成时间</param>
        /// <param name="AppraiseeID">被考核人ID</param>
        /// <returns></returns>
        [OperationContract]
        public int KPISystemScoreByKPIPoint(T_HR_KPIPOINT kpiPoint, string formCode, string flowID, string stepID,
            DateTime lastStepDate, DateTime stepDate, string AppraiseeID)
        {
            // 1s 冉龙军
            //stepDate = System.DateTime.Now;
            // 1e
            int score = -1;
            //判断是否有KPI点
            // 1s 冉龙军
            //if (kpiPoint != null)
            //{
            //    score = SystemScoring(kpiPoint, lastStepDate, stepDate);

            //    SaveKPIRecord(kpiPoint, formCode, flowID, stepID, AppraiseeID, AppraiseeID, score, 0);

            //}
            if (kpiPoint != null)
            {
                // stepDate 当前步骤完成时间（实际是审核完成后， 运行到此的时间， 会有一点误差）
                stepDate = System.DateTime.Now;
                score = SystemScoring(kpiPoint, lastStepDate, stepDate);

                SaveKPIRecord(kpiPoint, formCode, flowID, stepID, AppraiseeID, AppraiseeID, score, 0);

            }
            // 1e
            return score;
        }

        #endregion 系统评分服务

        #region 手动评分服务

        /// <summary>
        /// 根据流程和业务的信息，记录手动评分信息
        /// </summary>
        /// <param name="companyID">公司ID</param>
        /// <param name="modelRelationID">业务ID</param>
        /// <param name="flowCode">流程Code</param>
        /// <param name="lastStepID">上一个步骤Code</param>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程单号</param>
        /// <param name="lastStepCode">上一个步骤单号</param>
        /// <param name="AppraiseeID">被考核人ID</param>
        /// <param name="AppraiserID">考核人ID</param>
        /// <param name="score">考核人打分</param>
        /// <returns>考核人打分</returns>
        [OperationContract]
        public T_HR_KPIRECORD KPIManualScore(string companyID, string modelRelationID, string flowCode, string lastStepID, string formCode, string flowID,
            string lastStepCode, string AppraiseeID, string AppraiserID, int score)
        {
            T_HR_KPIPOINT point = GetKPIPoint(companyID, modelRelationID, flowCode, lastStepID);
            return KPIManualScoreByKPIPoint(point, formCode, flowID, lastStepCode, AppraiseeID, AppraiserID, score);
        }

        /// <summary>
        /// 根据KPI点信息，记录手动评分信息
        /// </summary>
        /// <param name="kpiPoint">KPI点</param>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程单号</param>
        /// <param name="lastStepCode">上一个步骤单号</param>
        /// <param name="AppraiseeID">被考核人ID</param>
        /// <param name="AppraiserID">考核人ID</param>
        /// <param name="score">考核人打分</param>
        /// <returns>考核人打分</returns>
        [OperationContract]
        public T_HR_KPIRECORD KPIManualScoreByKPIPoint(T_HR_KPIPOINT kpiPoint, string formCode, string flowID, string lastStepCode,
            string AppraiseeID, string AppraiserID, int score)
        {
            //判断是否有KPI点
            // 1s 冉龙军
            //if (kpiPoint != null)
            //{
            //    return SaveKPIRecord(kpiPoint, formCode, flowID, lastStepCode, AppraiseeID, AppraiserID, score, 1);
            //}
            if (kpiPoint != null)
            {
                return SaveKPIRecord(kpiPoint, formCode, flowID, lastStepCode, AppraiseeID, AppraiserID, score, 1);
            }
            // 1e
            return null;
        }


        #endregion 手动评分服务

        #region 抽查评分服务

        /// <summary>
        /// 根据流程和业务的信息，记录抽查评分信息
        /// </summary>
        /// <param name="companyID">公司ID</param>
        /// <param name="modelRelationID">业务ID</param>
        /// <param name="flowCode">流程Code</param>
        /// <param name="lastStepID">上一个步骤Code</param>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程单号</param>
        /// <param name="lastStepCode">上一个步骤单号</param>
        /// <param name="AppraiseeID">被考核人ID</param>
        /// <param name="AppraiserID">抽查人ID</param>
        /// <param name="score">抽查人打分</param>
        /// <returns>抽查人打分</returns>
        [OperationContract]
        public T_HR_KPIRECORD KPIRandomScore(string companyID, string modelRelationID, string flowCode, string lastStepID, string formCode, string flowID,
            string lastStepCode, string AppraiseeID, string AppraiserID, int score)
        {
            T_HR_KPIPOINT point = GetKPIPoint(companyID, modelRelationID, flowCode, lastStepID);
            return KPIRandomScoreByKPIPoint(point, formCode, flowID, lastStepCode, AppraiseeID, AppraiserID, score);
        }

        /// <summary>
        /// 根据KPI点信息，记录抽查评分信息
        /// </summary>
        /// <param name="kpiPoint">KPI点</param>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程单号</param>
        /// <param name="lastStepCode">上一个步骤单号</param>
        /// <param name="AppraiseeID">被考核人ID</param>
        /// <param name="AppraiserID">抽查人ID</param>
        /// <param name="score">抽查人打分</param>
        /// <returns>抽查人打分</returns>
        [OperationContract]
        public T_HR_KPIRECORD KPIRandomScoreByKPIPoint(T_HR_KPIPOINT kpiPoint, string formCode, string flowID, string lastStepCode,
            string AppraiseeID, string AppraiserID, int score)
        {
            //判断是否有KPI点
            if (kpiPoint != null)
            {
                return SaveKPIRecord(kpiPoint, formCode, flowID, lastStepCode, AppraiseeID, AppraiserID, score, 2);
            }
            return null;
        }


        #endregion 抽查评分服务

        #endregion 考核评分服务

        #region 获取信息

        /// <summary>
        /// 获取KPI考核点信息
        /// </summary>
        /// <param name="companyID">公司</param>
        /// <param name="modelRelationID">业务ID</param>
        /// <param name="flowCode">流程Code</param>
        /// <param name="stepCode">步骤Code</param>
        /// <returns>KPI考核点</returns>
        [OperationContract]
        public T_HR_KPIPOINT GetKPIPoint(string companyID, string modelRelationID, string flowID, string stepID)
        {
            using (KPIPointBll bll = new KPIPointBll())
            {
                T_HR_KPIPOINT point = bll.GetKPIPoint(companyID, modelRelationID, flowID, stepID);
                return point;
            }
        }

        /// <summary>
        /// 获取KPI考核点信息
        /// </summary>
        /// <param name="companyID">公司</param>
        /// <param name="modelRelationID">业务ID</param>
        /// <param name="flowCode">流程Code</param>
        /// <param name="stepCode">步骤Code</param>
        /// <returns>KPI考核点</returns>
        [OperationContract]
        public List<T_HR_KPIPOINT> GetKPIPointAndLastPoint(string companyID, string modelRelationID, string flowID, string stepID, string LastStepID)
        {
            using (KPIPointBll bll = new KPIPointBll())
            {
                T_HR_KPIPOINT point = bll.GetKPIPoint(companyID, modelRelationID, flowID, stepID);
                T_HR_KPIPOINT lastPoint = bll.GetKPIPoint(companyID, modelRelationID, flowID, LastStepID);
                List<T_HR_KPIPOINT> list = new List<T_HR_KPIPOINT>();
                list.Add(point);
                list.Add(lastPoint);
                return list;
            }
        }

        /// <summary>
        /// 根据业务代码获取KPI考核点信息列表
        /// </summary>
        /// <param name="modelRelationID">业务ID</param>
        /// <returns>KPI考核点列表</returns>
        [OperationContract]
        public List<T_HR_KPIPOINT> GetKPIPointListByBusinessCode(string modelRelationID)
        {
            using (KPIPointBll bll = new KPIPointBll())
            {
                List<T_HR_KPIPOINT> point = bll.GetKPIPointListByBusinessCode(modelRelationID);
                return point;
            }
        }
        /// <summary>
        /// 根据业务代码获取KPI考核点信息 xml
        /// </summary>
        /// <param name="modelRelationID"></param>
        /// <returns></returns>
        [OperationContract]
        public string GetKPIPointsByBusinessCode(string modelRelationID)
        {
            List<T_HR_KPIPOINT> points = GetKPIPointListByBusinessCode(modelRelationID);
            if (points == null)
            {
                return "";
            }
            StringBuilder strRes = new StringBuilder();
            strRes.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            strRes.Append("<FlowRelation id=\"" + modelRelationID + "\">");
            foreach (var point in points)
            {
                strRes.Append("<FlowNode id=\"" + point.STEPID + "\"  value=\"" + point.T_HR_SCORETYPE.INITIALPOINT + "\"/>");
            }
            strRes.Append("</FlowRelation>");
            return strRes.ToString();
        }

        /// <summary>
        /// 获取KPI考核点信息
        /// </summary>
        /// <param name="kpiPointID">KPI考核点ID</param>
        /// <returns>KPI考核点</returns>
        [OperationContract]
        public T_HR_KPIPOINT GetKPIPointByID(string kpiPointID)
        {
            using (KPIPointBll bll = new KPIPointBll())
            {
                T_HR_KPIPOINT point = bll.GetKPIPoint(kpiPointID);
                return point;
            }
        }

        // 1s 冉龙军
        /// <summary>
        /// 随机获取KPI考核点的抽查组成员
        /// </summary>
        /// <param name="companyID">公司</param>
        /// <param name="modelRelationID">业务ID</param>
        /// <param name="flowCode">流程Code</param>
        /// <param name="stepCode">步骤Code</param>
        /// <returns>随机人员ID</returns>
        [OperationContract]
        public string GetKPIPointRandomPersonID(string companyID, string modelRelationID, string flowID, string stepID, string formID)
        {
            string groupRandomPersonID = "";
            using (KPIPointBll bll = new KPIPointBll())
            {
                //获取KPI点
                T_HR_KPIPOINT point = bll.GetKPIPoint(companyID, modelRelationID, flowID, stepID);

                List<T_HR_RAMDONGROUPPERSON> groupPersonList;
                if (point != null)
                {
                    if (point.T_HR_SCORETYPE != null)
                    {
                        if (point.T_HR_SCORETYPE.ISRANDOMSCORE == "1")
                        {
                            if (point.T_HR_SCORETYPE.T_HR_RANDOMGROUP != null)
                            {
                                //抽查人员
                                if (point.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON == null || point.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON.Count == 0)
                                {
                                    RandomGroupBll bllRandom = new RandomGroupBll();
                                    groupPersonList = bllRandom.GetRandomGroupPersonByGroupID(point.T_HR_SCORETYPE.T_HR_RANDOMGROUP.RANDOMGROUPID).ToList();
                                }
                                else
                                    groupPersonList = point.T_HR_SCORETYPE.T_HR_RANDOMGROUP.T_HR_RAMDONGROUPPERSON.ToList();
                                if (groupPersonList == null || groupPersonList.Count == 0) { }
                                else
                                {
                                    Random r = new Random();
                                    //获取随机数
                                    int i = r.Next(0, groupPersonList.Count);
                                    //获取抽查人员
                                    groupRandomPersonID = groupPersonList[i].PERSONID;
                                    if (groupRandomPersonID != null && groupRandomPersonID != "")
                                    {
                                        SaveKPIRecord(point, formID, flowID, stepID, "", groupRandomPersonID, 0, 3);
                                    }
                                }

                                //抽查时间
                                if (point.T_HR_SCORETYPE.T_HR_KPIREMIND != null)
                                {
                                    IEnumerator OperandEnum = point.T_HR_SCORETYPE.T_HR_KPIREMIND.OrderBy(s => s.FORWARDHOURS).GetEnumerator();
                                    //提醒计数
                                    int CharCount = 0;
                                    while (OperandEnum.MoveNext())
                                    {
                                        T_HR_KPIREMIND Remind = (T_HR_KPIREMIND)OperandEnum.Current;
                                        CharCount++;
                                        //第一条提醒
                                        if (CharCount == 1)
                                        {
                                            groupRandomPersonID = groupRandomPersonID + "|" + Remind.FORWARDHOURS.ToString();
                                        }
                                        //第二条提醒
                                        if (CharCount == 2)
                                        {
                                            groupRandomPersonID = groupRandomPersonID + "|" + Remind.FORWARDHOURS.ToString();
                                        }
                                        //第三条提醒
                                        if (CharCount == 3)
                                        {
                                            groupRandomPersonID = groupRandomPersonID + "|" + Remind.FORWARDHOURS.ToString();
                                        }
                                    }
                                    groupRandomPersonID = groupRandomPersonID + "|" + CharCount.ToString();
                                }

                            }
                        }
                    }
                }
                return groupRandomPersonID;
            }
        }
        // 1e
        /// <summary>
        /// 获取KPI类别信息
        /// </summary>
        /// <param name="kpiPointID">KPI类别ID</param>
        /// <returns>KPI类别</returns>
        [OperationContract]
        public T_HR_KPITYPE GetKPITypeByID(string kpiTypeID)
        {
            using (KPITypeBll bll = new KPITypeBll())
            {
                T_HR_KPITYPE kpiType = bll.GetKPIType(kpiTypeID);
                List<T_HR_KPIREMIND> list = bll.GetRemindByScoreTypeID(kpiType.T_HR_SCORETYPE.SCORETYPEID);
                foreach (T_HR_KPIREMIND remind in list)
                {
                    kpiType.T_HR_SCORETYPE.T_HR_KPIREMIND.Add(remind);
                }
                return kpiType;
            }
        }

        /// <summary>
        /// 获取KPI类别信息
        /// </summary>
        /// <param name="kpiPointID">KPI类别ID</param>
        /// <returns>KPI类别</returns>
        [OperationContract]
        public List<T_HR_KPITYPE> GetKPITypeAll()
        {
            using (KPITypeBll bll = new KPITypeBll())
            {
                List<T_HR_KPITYPE> kpiTypeList = bll.GetKPITypeAll();
                foreach (T_HR_KPITYPE kpiType in kpiTypeList)
                {
                    List<T_HR_KPIREMIND> list = bll.GetRemindByScoreTypeID(kpiType.T_HR_SCORETYPE.SCORETYPEID);
                    foreach (T_HR_KPIREMIND remind in list)
                    {
                        kpiType.T_HR_SCORETYPE.T_HR_KPIREMIND.Add(remind);
                    }
                }
                return kpiTypeList;
            }
        }
        /// <summary>
        ///根据权限获取KPI类别信息
        /// </summary>
        /// <param name="kpiPointID">KPI类别ID</param>
        /// <returns>KPI类别</returns>
        [OperationContract]
        public List<T_HR_KPITYPE> GetKPITypeWithPermission(string filterString, IList<object> paras, string userID)
        {
            using (KPITypeBll bll = new KPITypeBll())
            {
                return bll.GetKPITypeWithPermission(filterString, paras, userID);
            }
        }
        // 1s 冉龙军
        /// <summary>
        /// 获取KPI考核点信息
        /// </summary>
        /// <param name="companyID">公司</param>
        /// <param name="modelRelationID">业务ID</param>
        /// <param name="flowCode">流程Code</param>
        /// <param name="stepCode">步骤Code</param>
        /// <returns>KPI考核点</returns>
        [OperationContract]
        public string GetKPIRecordRandomPersonID(string formCode, string flowID, string stepID)
        {
            string KPIRecordRandomPersonID = "";
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                T_HR_KPIRECORD record = bll.GetKPIRecord(formCode, flowID, stepID);
                if (record != null && record.RANDOMPERSONID != null)
                {
                    KPIRecordRandomPersonID = record.RANDOMPERSONID;
                }
                return KPIRecordRandomPersonID;
            }
        }

        // 1e
        /// <summary>
        /// 获取KPI明细记录信息
        /// </summary>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程ID</param>
        /// <param name="stepID">步骤ID</param>
        /// <returns>KPI明细记录</returns>
        [OperationContract]
        public T_HR_KPIRECORD GetKPIRecord(string formCode, string flowID, string stepID)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                T_HR_KPIRECORD record = bll.GetKPIRecord(formCode, flowID, stepID);
                return record;
            }
        }

        // 1s 冉龙军
        /// <summary>
        /// 获取KPI明细记录的打分
        /// </summary>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程ID</param>
        /// <param name="stepID">步骤ID</param>
        /// <returns>KPI明细记录</returns>
        [OperationContract]
        public string GetKPIRecordScoreDetail(string formCode, string flowID, string stepID, int scoreType)
        {
            string score = "";
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                T_HR_KPIRECORD record = bll.GetKPIRecord(formCode, flowID, stepID);
                if (record != null && record.MANUALSCORE != null)
                {
                    switch (scoreType)
                    {
                        case 0:
                            score = record.SYSTEMSCORE.ToString();
                            break;
                        case 1:
                            score = record.MANUALSCORE.ToString();
                            break;
                        case 2:
                            score = record.RANDOMSCORE.ToString();
                            break;
                    }
                }
                else
                {
                    score = "";
                }
                return score;
            }
        }

        /// <summary>
        /// 获取KPI明细记录信息（接口专用）
        /// </summary>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程ID</param>
        /// <param name="stepID">步骤ID</param>
        /// <returns>KPI明细记录</returns>
        [OperationContract]
        public T_HR_KPIRECORD GetKPIRecordInterface(string formCode)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                T_HR_KPIRECORD record = bll.GetKPIRecordInterface(formCode);
                return record;
            }
        }

        // 1e
        /// <summary>
        /// 获取KPI明细记录信息
        /// </summary>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程ID</param>
        /// <param name="stepID">步骤ID</param>
        /// <returns>KPI明细记录</returns>
        [OperationContract]
        public decimal? GetKPIRecordScore(string formCode, string flowID, string stepID)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                T_HR_KPIRECORD record = GetKPIRecord(formCode, flowID, stepID);
                decimal? score;
                if (record == null || record.SUMSCORE == null)
                    score = -1;
                else
                    score = record.SUMSCORE;
                return score;
            }
        }

        /// <summary>
        /// 获取KPI明细记录信息
        /// </summary>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程单号</param>
        /// <param name="stepID">步骤单号</param>
        /// <returns>KPI明细记录</returns>
        [OperationContract]
        public T_HR_KPIRECORD GetKPIRecordById(string recordId)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                T_HR_KPIRECORD record = bll.GetKPIRecordById(recordId);
                return record;
            }
        }

        /// <summary>
        /// 检查人员中是否存在正在申诉的KPI明细记录
        /// </summary>
        /// <param name="userIdList"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [OperationContract]
        public bool CheckKPIRecordIsComplainingByUserID(List<string> userIdList, DateTime startTime, DateTime endTime)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                bool r = bll.CheckKPIRecordIsComplainingByUserID(userIdList, startTime, endTime);
                return r;
            }
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_KPITYPE> GetKPITypePaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (KPITypeBll bll = new KPITypeBll())
            {
                IQueryable<T_HR_KPITYPE> q = bll.GetKPITypesPaging(pageIndex, pageSize, sort, filterString, paras,
                                                                   ref pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        #endregion 获取信息

        #region 增删改操作

        /// <summary>
        /// 添加KPI类别
        /// </summary>
        /// <param name="entType">KPI类别实体，包括评分方式实体</param>
        [OperationContract]
        private void AddKPIType(T_HR_KPITYPE entType, ref string strMsg)
        {
            using (KPITypeBll bll = new KPITypeBll())
            {
                bll.AddKPIType(entType, ref strMsg);
            }
        }

        /// <summary>
        /// 修改KPI类别
        /// </summary>
        /// <param name="entType">KPI类别实体，包括评分方式实体</param>
        [OperationContract]
        private void UpdateKPIType(T_HR_KPITYPE entType, ref string strMsg)
        {
            using (KPITypeBll bll = new KPITypeBll())
            {
                bll.UpdateKPIType(entType, ref strMsg);
            }
        }

        /// <summary>
        /// 修改KPI类别和子表提醒方式
        /// </summary>
        /// <param name="entType">KPI类别实体，包括评分方式实体</param>
        /// <param name="addList">需要添加的提醒</param>
        /// <param name="updateList">需要更新的提醒</param>
        /// <param name="delList">需要删除的提醒</param>
        [OperationContract]
        private void UpdateKPITypeAndRemind(T_HR_KPITYPE entType, List<T_HR_KPIREMIND> addList, List<T_HR_KPIREMIND> updateList, List<T_HR_KPIREMIND> delList, ref string strMsg)
        {
            using (KPITypeBll bll = new KPITypeBll())
            {
                bll.UpdateKPIType(entType, addList, updateList, delList, ref strMsg);
            }
        }

        /// <summary>
        /// 删除KPI类别
        /// </summary>
        /// <param name="entType">KPI类别实体，包括评分方式实体</param>
        [OperationContract]
        private int DeleteKPIType(string kpiTypeId)
        {
            using (KPITypeBll bll = new KPITypeBll())
            {
                return bll.DeleteKPIType(kpiTypeId);
            }
        }

        /// <summary>
        /// 删除KPI类别列表
        /// </summary>
        /// <param name="entType">KPI类别实体，包括评分方式实体</param>
        [OperationContract]
        private int DeleteKPITypes(string[] kpiTypeId, ref string strMsg)
        {
            using (KPITypeBll bll = new KPITypeBll())
            {
                return bll.DeleteKPITypes(kpiTypeId, ref strMsg);
            }
        }

        /// <summary>
        /// 添加KPI考核点
        /// </summary>
        /// <param name="entType">KPI考核点实体，包括评分方式实体</param>
        [OperationContract]
        private void AddKPIPoint(T_HR_KPIPOINT entType)
        {
            using (KPIPointBll bll = new KPIPointBll())
            {
                bll.AddKPIPoint(entType);
            }
        }

        /// <summary>
        /// 修改KPI考核点
        /// </summary>
        /// <param name="entType">KPI考核点实体，包括评分方式实体</param>
        [OperationContract]
        private void UpdateKPIPoint(T_HR_KPIPOINT entType)
        {
            using (KPIPointBll bll = new KPIPointBll())
            {
                bll.UpdateKPIPoint(entType);
            }
        }

        /// <summary>
        /// 修改KPI类别和子表提醒方式
        /// </summary>
        /// <param name="entType">KPI点实体，包括评分方式实体</param>
        /// <param name="addList">需要添加的提醒</param>
        /// <param name="updateList">需要更新的提醒</param>
        /// <param name="delList">需要删除的提醒</param>
        [OperationContract]
        private void UpdateKPIPointAndRemind(T_HR_KPIPOINT entType, List<T_HR_KPIREMIND> addList, List<T_HR_KPIREMIND> updateList, List<T_HR_KPIREMIND> delList)
        {
            using (KPIPointBll bll = new KPIPointBll())
            {
                bll.UpdateKPIPoint(entType, addList, updateList, delList);
            }
        }

        /// <summary>
        /// 删除KPI考核点
        /// </summary>
        /// <param name="entType">KPI考核点实体，包括评分方式实体</param>
        [OperationContract]
        private int DeleteKPIPoint(string kpiTypeId)
        {
            using (KPIPointBll bll = new KPIPointBll())
            {
                return bll.DeleteKPIPoint(kpiTypeId);
            }
        }

        #endregion 增删改操作

        #region   KPI明细操作
        /// <summary>
        /// 修改KPI明细记录
        /// </summary>
        /// <param name="entity">公司实例</param>
        [OperationContract]
        public void KPIRecordUpdate(T_HR_KPIRECORD entity)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                bll.KPIRecordUpdate(entity);
            }
        }

        /// <summary>
        /// 根据申诉记录ID获取V_COMPLAINRECORD类型数据
        /// </summary>
        /// <param name="CompainRecordID">申诉记录ID</param>
        /// <returns></returns>
        [OperationContract]
        public V_COMPLAINRECORD GetVcomplainRecordByID(string CompainRecordID)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                return bll.GetVcomplainRecordByID(CompainRecordID);
            }
        }

        #endregion

        /// <summary>
        /// 系统评分
        /// </summary>
        /// <param name="point">KPI考核点</param>
        /// <param name="lastStepDate">上一步完成时间</param>
        /// <param name="stepDate">当前完成时间</param>
        /// <returns></returns>
        private int SystemScoring(T_HR_KPIPOINT point, DateTime lastStepDate, DateTime stepDate)
        {
            if (lastStepDate == null || lastStepDate < new System.DateTime(2010, 6, 30))
                return 100;
            decimal? score;
            T_HR_SCORETYPE scoreType = point.T_HR_SCORETYPE;

            //计算完成当前步骤和上一步骤的时间差
            TimeSpan ts1 = new TimeSpan(stepDate.Ticks);
            TimeSpan ts2 = new TimeSpan(lastStepDate.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            // 1s 冉龙军
            ////计算完成的时间与预定时间的天数差值
            //int finishDays = ts.Days - decimal.ToInt16(scoreType.INITIALPOINT.Value);
            ////延迟完成，计算扣分
            //if (finishDays > 0)
            //    score = scoreType.INITIALSCORE - scoreType.REDUCESCORE * (finishDays / scoreType.COUNTUNIT);
            ////提前完成，计算加分
            //else if (finishDays < 0)
            //    score = scoreType.INITIALSCORE + scoreType.ADDSCORE * (finishDays / scoreType.COUNTUNIT);
            //else
            //    score = scoreType.INITIALSCORE;

            ////比较分数上下限
            //if (score > scoreType.MAXSCORE)
            //    score = scoreType.MAXSCORE;
            //else if (score < scoreType.MINSCORE)
            //    score = scoreType.MINSCORE;
            //计算完成的时间与预定时间的天数差值
            int finishDays = ts.Hours - decimal.ToInt16(scoreType.INITIALPOINT.Value);
            //延迟完成，计算扣分
            if (finishDays > 0)
                if (scoreType.LATERUNIT > 0)
                {
                    score = 100 - scoreType.REDUCESCORE * (System.Math.Abs(finishDays) / scoreType.LATERUNIT);
                }
                else
                {
                    score = 100;
                }
            //提前完成，计算加分
            else if (finishDays < 0)
                if (scoreType.COUNTUNIT > 0)
                {
                    score = 100 + scoreType.ADDSCORE * (System.Math.Abs(finishDays) / scoreType.COUNTUNIT);
                }
                else
                {
                    score = 100;
                }
            else
                score = 100;

            //比较分数上下限
            if (score > (100 + scoreType.MAXSCORE))
                score = 100 + scoreType.MAXSCORE;
            else if (score < (100 - scoreType.MINSCORE))
                score = 100 - scoreType.MINSCORE;
            // 1e
            score = score ?? 0;
            return decimal.ToInt16(score.Value);
        }

        /// <summary>
        /// 保存KPI明细记录信息
        /// </summary>
        /// <param name="kpiPoint">KPI考核点</param>
        /// <param name="formCode">业务单号</param>
        /// <param name="flowID">流程单号</param>
        /// <param name="lastStepCode">考核步骤单号</param>
        /// <param name="AppraiseeID">被考核人</param>
        /// <param name="AppraiserID">考核人</param>
        /// <param name="score">得分</param>
        /// <param name="scoretype">考核类型：0、系统评分；1、手动评分；2、抽查打分。</param>
        private T_HR_KPIRECORD SaveKPIRecord(T_HR_KPIPOINT kpiPoint, string formCode, string flowID, string lastStepCode,
            string AppraiseeID, string AppraiserID, int score, int scoretype)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                T_HR_KPIRECORD record = bll.SaveKPIRecord(kpiPoint, formCode, flowID, lastStepCode,
             AppraiseeID, AppraiserID, score, scoretype);
                return record;
            }
        }

        // 1s 冉龙军
        /// <summary>
        /// 保存KPI明细记录信息（接口专用）
        /// </summary>
        /// <param name="kpiPoint">T_HR_KPIRECORD实体（含业务单号）</param>
        /// <param name="AppraiseeID">被考核人</param>
        /// <param name="AppraiserID">考核人</param>
        /// <param name="score">得分</param>
        /// <param name="scoretype">考核类型：0、系统评分；1、手动评分；2、抽查打分。</param>
        [OperationContract]
        public T_HR_KPIRECORD SaveKPIRecordInterface(T_HR_KPIRECORD kpirecord, string AppraiseeID, string AppraiserID, int score, int scoretype)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                T_HR_KPIRECORD record = bll.SaveKPIRecordInterface(kpirecord, AppraiseeID, AppraiserID, score, scoretype);
                return record;
            }
        }
        // 1e
        #endregion KPI服务

        #region  申诉记录服务
        /// <summary>
        /// 根据kpi记录获取所有相关申诉记录
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_KPIRECORDCOMPLAIN> GetKPIRecordComplainByRecord(string recordId)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                IQueryable<T_HR_KPIRECORDCOMPLAIN> q = bll.GetKPIRecordComplainByRecord(recordId);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 根据申诉ID获取申诉
        /// </summary>
        /// <param name="complainID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_KPIRECORDCOMPLAIN GetKPIRecordComplainByID(string complainID)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                T_HR_KPIRECORDCOMPLAIN complain = bll.GetKPIRecordComplain(complainID);
                return complain;
            }
        }

        /// <summary>
        /// 添加KPI明细申诉
        /// </summary>
        /// <param name="entType">申诉实体</param>
        [OperationContract]
        public void AddKPIRecordComplain(T_HR_KPIRECORDCOMPLAIN entType)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                bll.AddKPIRecordComplain(entType);
            }
        }

        /// <summary>
        /// 添加KPI明细申诉
        /// </summary>
        /// <param name="entType">申诉实体</param>
        [OperationContract]
        public void UpdateKPIRecordComplain(T_HR_KPIRECORDCOMPLAIN entType)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                bll.UpdateKPIRecordComplain(entType);
            }
        }

        /// <summary>
        /// 根据参数获取KPI明细记录的申诉记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_COMPLAINRECORD> GetComplainRecordPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, int sType, string sValue, string userID, string startDate, string endDate, string strCheckState)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                var q = bll.GetComplainRecordPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, sType,
                                                sValue, userID, startDate, endDate, strCheckState);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }

            }
        }

        /// <summary>
        /// 根据ID获取申诉记录实体
        /// </summary>
        /// <param name="CompainRecordID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_KPIRECORDCOMPLAIN GetCompainRecordByID(string CompainRecordID)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                return bll.GetCompainRecordByID(CompainRecordID);
            }
        }

        /// <summary>
        /// 更新自定义薪资设置
        /// </summary>
        /// <param name="entity">自定义薪资设置实体</param>
        [OperationContract]
        public void CompainRecordUpdate(T_HR_KPIRECORDCOMPLAIN entity)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                bll.CompainRecordUpdate(entity);
            }
        }

        /// <summary>
        /// 申诉实体
        /// </summary>
        /// <param name="kpirecordid">实体ID</param>
        [OperationContract]
        public void CompainRecordUpdateOver(string kpirecordid)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                bll.CompainRecordUpdateOver(kpirecordid);
            }
        }

        /// <summary>
        /// 删除申诉记录，可同时删除多行记录
        /// </summary>
        /// <param name="ComplainRecords">申诉ID数组</param>
        /// <returns></returns>
        [OperationContract]
        public int ComplainRecordDelete(string[] ComplainRecords)
        {
            using (KPIRecordComplainBll bll = new KPIRecordComplainBll())
            {
                return bll.ComplainRecordDelete(ComplainRecords);
            }
        }


        #endregion  申诉记录服务

        #region  KPI考核记录服务

        /// <summary>
        /// 获取抽查组列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        // 1s 冉龙军
        //public List<T_HR_KPIRECORD> GetKPIRecordPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string sType, string sValue, string userID)
        //{
        //    KPIRecordBll bll = new KPIRecordBll();
        //    IQueryable<T_HR_KPIRECORD> q = bll.GetKPIRecordPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue, userID);
        //    return q.Count() > 0 ? q.ToList() : null;
        //}

        public List<V_KPIRECORD> GetKPIRecordPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string sType, string sValue, string userID, string startDate, string endDate, string strCheckState)
        {
            using (KPIRecordBll bll = new KPIRecordBll())
            {
                IQueryable<V_KPIRECORD> q = bll.GetKPIRecordPaging(pageIndex, pageSize, sort, filterString, paras,
                                                                   ref pageCount, sType, sValue, userID, startDate,
                                                                   endDate, strCheckState);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        // 1e
        /// <summary>
        /// 获取抽查组列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        public bool CheckRecordIsSummarize(string recordId)
        {
            using (PerformanceRecordBll bll = new PerformanceRecordBll())
            {
                bool q = bll.CheckRecordIsSummarize(recordId);
                return q;
            }
        }

        #endregion  KPI考核记录服务

        #region  抽查组服务

        #region  抽查组获取信息服务

        /// <summary>
        /// 获取抽查组列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_RANDOMGROUP> GetRandomGroupPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                IQueryable<T_HR_RANDOMGROUP> q = bll.GetRandomGroupPaging(pageIndex, pageSize, sort, filterString, paras,
                                                                          ref pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 获取抽查组下所有人员名单
        /// </summary>
        /// <param name="randomGroupID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_RAMDONGROUPPERSON> GetRandomGroupPersonAll(string randomGroupID)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                IQueryable<T_HR_RAMDONGROUPPERSON> q = bll.GetRandomGroupPersonAll(randomGroupID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 获取抽查组下所有人员名单
        /// </summary>
        /// <param name="randomGroupID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetRandomGroupPersonPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string randomGroupID)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                List<V_EMPLOYEEVIEW> q = bll.GetRandomGroupPersonPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, randomGroupID);
                return q;
            }
        }
        /// <summary>
        /// 根据ID抽查组人员信息
        /// </summary>
        /// <returns>抽查组人员</returns>
        [OperationContract]
        public T_HR_RAMDONGROUPPERSON GetRandomGroupPersonByID(string randomGroupPersonID)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                T_HR_RAMDONGROUPPERSON q = bll.GetRandomGroupPersonByID(randomGroupPersonID);
                return q;
            }
        }

        /// <summary>
        /// 根据ID抽查组人员信息
        /// </summary>
        /// <returns>抽查组人员</returns>
        [OperationContract]
        public List<T_HR_RAMDONGROUPPERSON> GetRandomGroupPersonByGroupID(string randomGroupID)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                IQueryable<T_HR_RAMDONGROUPPERSON> q = bll.GetRandomGroupPersonByGroupID(randomGroupID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 获取所有抽查组信息
        /// </summary>
        /// <returns>所有抽查组</returns>
        [OperationContract]
        public List<T_HR_RANDOMGROUP> GetRandomGroupAll()
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                List<T_HR_RANDOMGROUP> q = bll.GetRandomGroupAll();
                return q.Count() > 0 ? q : null;
            }
        }

        /// <summary>
        /// 根据ID抽查组信息
        /// </summary>
        /// <returns>抽查组</returns>
        [OperationContract]
        public T_HR_RANDOMGROUP GetRandomGroupByID(string randomGroupID)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                T_HR_RANDOMGROUP q = bll.GetRandomGroupByID(randomGroupID);
                return q;
            }
        }

        #endregion  抽查组获取信息服务

        #region 抽查组增删改操作

        /// <summary>
        /// 添加抽查组
        /// </summary>
        /// <param name="entType">抽查组实体</param>
        [OperationContract]
        private void AddRandomGroup(T_HR_RANDOMGROUP entType, ref string strMsg)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                bll.AddRandomGroup(entType, ref strMsg);
            }
        }

        /// <summary>
        /// 修改抽查组
        /// </summary>
        /// <param name="entType">抽查组实体</param>
        [OperationContract]
        private void UpdateRandomGroup(T_HR_RANDOMGROUP entType, ref string strMsg)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                bll.UpdateRandomGroup(entType, ref strMsg);
            }
        }

        /// <summary>
        /// 删除抽查组
        /// </summary>
        /// <param name="entType">抽查组实体</param>
        [OperationContract]
        private int DeleteRandomGroup(string randomGroupId)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                return bll.DeleteRandomGroup(randomGroupId);
            }
        }

        /// <summary>
        /// 删除抽查组列表
        /// </summary>
        /// <param name="entType">抽查组实体</param>
        [OperationContract]
        private int DeleteRandomGroups(string[] randomGroupId)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                return bll.DeleteRandomGroups(randomGroupId);
            }
        }

        /// <summary>
        /// 添加抽查组人员
        /// </summary>
        /// <param name="entType">抽查组人员实体</param>
        [OperationContract]
        private void AddRandomGroupPerson(T_HR_RAMDONGROUPPERSON entType)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                bll.AddRandomGroupPerson(entType);
            }
        }

        /// <summary>
        /// 添加抽查组人员列表
        /// </summary>
        /// <param name="entList">抽查组人员实体列表</param>
        [OperationContract]
        private int AddRandomGroupPersonList(List<T_HR_RAMDONGROUPPERSON> entList)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                return bll.AddRandomGroupPersonList(entList);
            }
        }

        /// <summary>
        /// 修改抽查组人员
        /// </summary>
        /// <param name="entType">抽查组人员实体</param>
        [OperationContract]
        private void UpdateRandomGroupPerson(T_HR_RAMDONGROUPPERSON entType)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                bll.UpdateRandomGroupPerson(entType);
            }
        }

        /// <summary>
        /// 修改抽查组人员
        /// </summary>
        /// <param name="entList">需要添加的抽查组人员实体列表</param>
        /// <param name="groupPersonIDs">需要删除的抽查组人员ID列表</param>
        [OperationContract]
        private int[] UpdateRandomGroupPersonList(List<T_HR_RAMDONGROUPPERSON> entList, string[] employeeIDs)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                return bll.UpdateRandomGroupPersonList(entList, employeeIDs);
            }
        }

        /// <summary>
        /// 删除抽查组人员
        /// </summary>
        /// <param name="entType">抽查组人员实体</param>
        [OperationContract]
        private int DeleteRandomGroupPerson(string randomGroupId)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                return bll.DeleteRandomGroupPerson(randomGroupId);
            }
        }

        /// <summary>
        /// 删除抽查组人员列表
        /// </summary>
        /// <param name="groupPersonIDs">抽查组人员ID列表</param>
        [OperationContract]
        public bool DeleteRandomGroupPersons(string[] groupPersonIDs)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                int rslt = bll.DeleteRandomGroupPersons(groupPersonIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        ///  删除抽查组人员列表
        /// </summary>
        /// <param name="groupPersonIDs">人员ID</param>
        /// <param name="groupID">抽查组ID</param>
        /// <returns></returns>
        [OperationContract]
        public bool DeleteRandomPersons(string[] groupPersonIDs, string groupID)
        {
            using (RandomGroupBll bll = new RandomGroupBll())
            {
                int rslt = bll.DeleteRandomGroupPersons(groupPersonIDs, groupID);
                return (rslt > 0);
            }
        }
        #endregion 抽查组增删改操作

        #endregion  抽查组服务
        #region  绩效考核信息服务

        /// <summary>
        /// 获取抽查组列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_SUMPERFORMANCERECORD> GetSumPerformancePaging(int pageIndex, int pageSize, string sort, string filterString,
            string[] paras, ref int pageCount, string sType, string sValue, string userID, bool isSelf, string checkState)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                IQueryable<T_HR_SUMPERFORMANCERECORD> q = bll.GetSumPerformancePaging(pageIndex, pageSize, sort,
                                                                                      filterString, paras, ref pageCount,
                                                                                      sType, sValue, userID, isSelf,
                                                                                      checkState);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 根据时间获取抽出组
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <param name="isSelf"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_SUMPERFORMANCERECORD> GetSumPerformancePagingByTime(int pageIndex, int pageSize, string sort, string filterString,
            string[] paras, ref int pageCount, string sType, string sValue, string userID, bool isSelf, string startTime, string endTime)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                IQueryable<T_HR_SUMPERFORMANCERECORD> q = bll.GetSumPerformancePagingByTime(pageIndex, pageSize, sort,
                                                                                            filterString, paras,
                                                                                            ref pageCount, sType, sValue,
                                                                                            userID, isSelf, startTime,
                                                                                            endTime);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 根据条件获取考核明细
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_PERFORMANCERECORDDETAIL> GetPerformanceDetailEmployeeAllPaging(int pageIndex, int pageSize, string sort, string filterString,
           string[] paras, ref int pageCount, string userID, string startTime, string endTime)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                IQueryable<V_PERFORMANCERECORDDETAIL> q = bll.GetPerformanceDetailEmployeeAllPaging(pageIndex, pageSize,
                                                                                                    sort, filterString,
                                                                                                    paras, ref pageCount,
                                                                                                    userID, startTime,
                                                                                                    endTime);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 绩效个人汇总
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_PERFORMANCERECORD> GetEmployeePerformancePagingByTime(int pageIndex, int pageSize, string sort, string filterString,
          string[] paras, ref int pageCount, string sType, string sValue, string userID, string startTime, string endTime)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                IQueryable<V_PERFORMANCERECORD> q = bll.GetEmployeePerformancePagingByTime(pageIndex, pageSize, sort,
                                                                                           filterString,
                                                                                           paras, ref pageCount, sType,
                                                                                           sValue, userID, startTime,
                                                                                           endTime);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_PERFORMANCERECORD> GetPerformanceEmployeeAllPaging(int pageIndex, int pageSize, string sort, string filterString,
           string[] paras, ref int pageCount, string userID)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                IQueryable<V_PERFORMANCERECORD> q = bll.GetPerformanceEmployeeAllPaging(pageIndex, pageSize, sort,
                                                                                        filterString, paras,
                                                                                        ref pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 获取抽查组下所有人员名单
        /// </summary>
        /// <param name="randomGroupID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_PERFORMANCERECORD> GetPerformanceAllBySumID(string sumID)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                IQueryable<T_HR_PERFORMANCERECORD> q = bll.GetPerformanceAllBySumID(sumID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        // 1s 冉龙军
        /// <summary>
        /// 获取个人绩效考核记录（平均分）
        /// </summary>
        /// <param name="sumID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_PERFORMANCERECORD> GetPerformanceEmployeeAllBySumID(string sumID)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                IQueryable<V_PERFORMANCERECORD> q = bll.GetPerformanceEmployeeAllBySumID(sumID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 获取个人绩效考核记录明细
        /// </summary>
        /// <param name="sumID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_PERFORMANCERECORDDETAIL> GetPerformanceDetailEmployeeAllBySumID(string sumID)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                IQueryable<V_PERFORMANCERECORDDETAIL> q = bll.GetPerformanceDetailEmployeeAllBySumID(sumID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        // 1e
        /// <summary>
        /// 获取人员绩效考核分数
        /// </summary>
        /// <param name="employeIDs">人员ID列表</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        [OperationContract]
        public Dictionary<string, decimal> GetEmployePerformance(List<string> employeIDs, DateTime startTime, DateTime endTime)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                Dictionary<string, decimal> q = bll.GetEmployePerformance(employeIDs, startTime, endTime);
                return q;
            }
        }

        /// <summary>
        /// 根据ID抽查组信息
        /// </summary>
        /// <returns>抽查组</returns>
        [OperationContract]
        public T_HR_PERFORMANCERECORD GetPerformanceRecordByID(string recordID)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                T_HR_PERFORMANCERECORD q = bll.GetPerformanceRecordByID(recordID);
                return q;
            }
        }

        #region 绩效考核增删改操作

        /// <summary>
        /// 添加绩效考核
        /// </summary>
        /// <param name="entType">绩效考核实体</param>
        [OperationContract]
        private void AddSumPerformance(T_HR_SUMPERFORMANCERECORD entType)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                bll.AddSumPerformance(entType);
            }
        }

        /// <summary>
        /// 获取绩效考核汇总记录实体
        /// </summary>
        /// <param name="sumid"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_SUMPERFORMANCERECORD GetSumPerformanceRecordByID(string sumid)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                return bll.GetSumPerformanceRecordByID(sumid);
            }
        }

        /// <summary>
        /// 修改绩效考核
        /// </summary>
        /// <param name="entType">绩效考核实体</param>
        [OperationContract]
        private void UpdateSumPerformance(T_HR_SUMPERFORMANCERECORD entType)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                bll.UpdateSumPerformance(entType);
            }
        }

        /// <summary>
        /// 修改绩效考核,不重新汇总
        /// </summary>
        /// <param name="entType">绩效考核实体</param>
        [OperationContract]
        private void UpdateSumPerformanceAndSum(T_HR_SUMPERFORMANCERECORD entType)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                bll.UpdateSumPerformanceAndSum(entType);
            }
        }

        /// <summary>
        /// 删除绩效考核，并且重新汇总
        /// </summary>
        /// <param name="entType">绩效考核实体</param>
        [OperationContract]
        private int DeleteSumPerformance(string sumId)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                return bll.DeleteSumPerformance(sumId);
            }
        }

        /// <summary>
        /// 删除绩效考核列表
        /// </summary>
        /// <param name="entType">绩效考核ID列表</param>
        [OperationContract]
        private int DeleteSumPerformances(string[] sumIdList)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                return bll.DeleteSumPerformances(sumIdList);
            }
        }

        /// <summary>
        /// 添加个人绩效考核
        /// </summary>
        /// <param name="entType">个人绩效考核实体</param>
        [OperationContract]
        private void AddPerformanceRecord(T_HR_PERFORMANCERECORD entType)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                bll.AddPerformanceRecord(entType);
            }
        }

        /// <summary>
        /// 添加个人绩效考核列表
        /// </summary>
        /// <param name="entList">个人绩效考核实体列表</param>
        [OperationContract]
        private int AddPerformanceRecordList(List<T_HR_PERFORMANCERECORD> entList)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                return bll.AddPerformanceRecordList(entList);
            }
        }

        /// <summary>
        /// 修改个人绩效考核
        /// </summary>
        /// <param name="entType">个人绩效考核实体</param>
        [OperationContract]
        private void UpdatePerformanceRecord(T_HR_PERFORMANCERECORD entType)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                bll.UpdatePerformanceRecord(entType);
            }
        }

        /// <summary>
        /// 修改个人绩效考核
        /// </summary>
        /// <param name="entList">需要添加的个人绩效考核实体列表</param>
        /// <param name="groupPersonIDs">需要删除的个人绩效考核ID列表</param>
        [OperationContract]
        private int[] UpdatePerformanceRecordList(List<T_HR_PERFORMANCERECORD> entList, string[] employeeIDs)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                return bll.UpdatePerformanceRecordList(entList, employeeIDs);
            }
        }

        /// <summary>
        /// 删除个人绩效考核
        /// </summary>
        /// <param name="entType">个人绩效考核实体</param>
        [OperationContract]
        private int DeletePerformanceRecord(string randomGroupId)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                return bll.DeletePerformanceRecord(randomGroupId);
            }
        }

        /// <summary>
        /// 删除个人绩效考核列表
        /// </summary>
        /// <param name="groupPersonIDs">个人绩效考核ID列表</param>
        [OperationContract]
        public bool DeletePerformanceRecords(string[] groupPersonIDs)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {
                int rslt = bll.DeletePerformanceRecords(groupPersonIDs);

                return (rslt > 0);
            }
        }

        #endregion 绩效考核增删改操作

        #endregion  绩效考核信息服务
    }
}
