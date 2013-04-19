using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    /// <summary>
    /// 员工调查方案自定义类
    /// </summary>
    public class V_EmployeeSurveyMaster
    {
        public T_OA_REQUIREMASTER masterEntity { get; set; }//调查方案
        public IEnumerable<V_EmployeeSurveyInformation> SubjectList { get; set; }//答案与题目
        public IEnumerable<T_OA_REQUIREDETAIL> answerList { get; set; }//题目集合,用于添加或修改数据


        decimal _subjectId;//题目ID
        string _code;//答案编号
        string _requireTitle;//方案标题
        string _content;//方案内容
        string _requireMasterId;//员工调查方案ID
        string _requireQuestionId;//方案题目ID
        string _requireAnswerId;//方案答案ID
        string _question;//方案题目内容
        string _answer;//方案答案内容
        string _ownerName;//所属人名称
        DateTime _createDate;//创建日期

        public decimal SubjectId
        {
            get { return _subjectId; }
            set { _subjectId = value; }
        }

        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }

        public string SurveyTitle
        {
            get { return _requireTitle; }
            set { _requireTitle = value; }
        }

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public string RequireMasterId
        {
            get { return _requireMasterId; }
            set { _requireMasterId = value; }
        }

        public string RequireQuestionId
        {
            get { return _requireQuestionId; }
            set { _requireQuestionId = value; }
        }

        public string RequireAnswerId
        {
            get { return _requireAnswerId; }
            set { _requireAnswerId = value; }
        }

        public string Question
        {
            get { return _question; }
            set { _question = value; }
        }
        public string Answer
        {
            get { return _answer; }
            set { _answer = value; }
        }

        public string OwnerName
        {
            get { return _ownerName; }
            set { _ownerName = value; }
        }

        public DateTime CreateDate
        {
            get { return _createDate; }
            set { _createDate = value; }
        }
    }
}
