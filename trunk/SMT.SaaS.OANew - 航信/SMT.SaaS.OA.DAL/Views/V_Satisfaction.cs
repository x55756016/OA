using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    #region 方案、题目视图
    /// <summary>
    /// 满意度调查方案 、题目的视图
    /// </summary>
    public class V_Satisfaction
    {
        private T_OA_SATISFACTIONMASTER requireMaster;
        public T_OA_SATISFACTIONMASTER RequireMaster
        {
            get { return requireMaster; }
            set { requireMaster = value; }
        }
        private T_OA_SATISFACTIONDETAIL detail;
        public T_OA_SATISFACTIONDETAIL Detail{get{return detail;}set{detail=value;}}
        private IEnumerable<T_OA_SATISFACTIONDETAIL> subjectViewList ;
        public IEnumerable<T_OA_SATISFACTIONDETAIL> SubjectViewList
        {
            get
            {
                return subjectViewList;
            }
            set
            {
                subjectViewList = value;
            }
        }
      #endregion

    #region 公有字段和属性
        /// <summary>
        /// 所有用到表中都存在的字段及其属性
        /// </summary>
        private string ownercompanyid;
        private string ownerdepartmentid;
        private string ownerpostid;
        private string ownerid;
        private string createuserid;

        public string OWNERPOSTID
        {
            get { return ownerpostid; }
            set { ownerpostid = value; }
        }
        public string CREATEUSERID
        {
            get { return createuserid; }
            set { createuserid = value; }
        }
        public string OWNERID
        {
            get { return ownerid; }
            set { ownerid = value; }
        }
        public string OWNERDEPARTMENTID
        {
            get { return ownerdepartmentid; }
            set { ownerdepartmentid = value; }
        }
        public string OWNERCOMPANYID
        {
            get { return ownercompanyid; }
            set { ownercompanyid = value; }
        }
    }
        #endregion

    #region 调查统计结果
    /// <summary>
    ///员工调查(满意度调查)某个调查方案 、题目、答案的统计情况
    /// </summary>
    public class V_SatisfactionResult
    {
        private decimal subjectID = 0;
        int count = 0;
        /// <summary>
        /// 题目编号
        /// </summary>
        public decimal SubjectID { get { return subjectID; } set { subjectID = value; } }      
        
        private List<V_SSubjectAnswerResult> lstAnswer;
        public List<V_SSubjectAnswerResult> LstAnswer { get { return lstAnswer; } set { lstAnswer = value; } }
    }
    /// <summary>
    /// 答案统计
    /// </summary>
    public class V_SSubjectAnswerResult
    {
        string answerCode;
        /// <summary>
        /// 答案编号
        /// </summary>
       public string AnswerCode { get { return answerCode; } set { answerCode = value; } }
        int count = 0;
        /// <summary>
        /// 答案统计数
        /// </summary>
        public int Count { get { return count; } set { count = value; } }
    #endregion
    }
}