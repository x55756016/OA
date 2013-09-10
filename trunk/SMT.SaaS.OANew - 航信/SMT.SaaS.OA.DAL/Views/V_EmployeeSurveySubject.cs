using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_EmployeeSurveySubject
    {
        public V_EmployeeSurveySubject()
        {
            subjectInfo = new T_OA_REQUIREDETAIL2();
        }
        private T_OA_REQUIREDETAIL2 subjectInfo;
        public T_OA_REQUIREDETAIL2 SubjectInfo
        {
            get { return subjectInfo; }
            set { subjectInfo = value; }
        }
        private T_OA_REQUIREDETAIL2 question;
        public T_OA_REQUIREDETAIL2 Question
        {
            get { return question; }
            set { question = value; }
        }
        private T_OA_REQUIREMASTER master;
        public T_OA_REQUIREMASTER Master
        { get { return master; }
          set { master = value; }
        }
        private T_OA_REQUIREDETAIL answer;
        public T_OA_REQUIREDETAIL Answer
        {
            get { return answer; }
            set { answer = value; }
        }
        private IEnumerable<T_OA_REQUIREDETAIL> answerList;
        public IEnumerable<T_OA_REQUIREDETAIL> AnswerList
        {
            get { return answerList; }
            set { answerList = value; }
        }
        private bool _IsAdd = false;
        /// <summary>
        /// 标识题目是否为新增，还是数据库已存在的， _Isadd==true表示 客户端新增数据
        /// </summary>
        public bool IsAdd { get { return _IsAdd; } set { _IsAdd = value; } }
    }
}
