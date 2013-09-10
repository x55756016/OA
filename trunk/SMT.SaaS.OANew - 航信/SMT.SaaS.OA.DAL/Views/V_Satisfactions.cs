using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using System.ComponentModel;


namespace SMT.SaaS.OA.DAL.Views
{
    #region 满意度调查自定义类
    public class V_Satisfactions
    {
        #region 所需表
        public T_OA_SATISFACTIONREQUIRE requireEntity { get; set; }//调查申请实体
        public T_OA_REQUIREMASTER masterEntity{get;set;}//调查方案实体
        public T_OA_SATISFACTIONDISTRIBUTE disibuteEntity { get; set; }//调查发布实体
        public IEnumerable<T_OA_SATISFACTIONRESULT> resultList { get; set; }//调查结果集合
        public IEnumerable<T_OA_DISTRIBUTEUSER> distributeuserList { get; set; }//分发范围集合
        public IEnumerable<T_OA_DISTRIBUTEUSER> oldDistributeuserList { get; set; }//待删除分发范围集合
        #endregion 

        #region   所需字段及相应属性
        private string _satisfactiontitle;//调查标题
        private string _content;//调查内容
        private string _satisfactionmasterid;//调查方案ID
        private string _satisfactionrequireid;//调查申请ID
        private string _satisfactiondistrbuteid;//调查发布ID
        private string _answerGroupid;//答案组ID
        private string _ownername;//所属人名称
        private DateTime? _createdate;//创建日期
        private string _answerid;//答案ID
        private decimal? _percent;//百分比

        public string SurveyTitle
        {
            get { return _satisfactiontitle; }
            set
            { 
                _satisfactiontitle = value;
            }
        }
        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
            }
        }
        public string Satisfactionmasterid
        {
            get { return _satisfactionmasterid; }
            set
            {
                _satisfactionmasterid = value;
            }
        }
        public string Satisfactionrequireid
        {
            get { return _satisfactionrequireid; }
            set
            {
                _satisfactionrequireid = value;
             }
        }

        public string OwnerName
        {
            get { return _ownername; }
            set
            {
                _ownername = value;
                
            }
        }

        public DateTime? CreateDate
        {
            get { return _createdate; }
            set
            {
                _createdate = value;
                
            }
        }
        public string AnswerGroupid
        {
            get { return _answerGroupid; }
            set { _answerGroupid = value; }
        }
        public string SatisfactiondistrbuteidDistrbuteid
        {
            get { return _satisfactiondistrbuteid; }
            set { _satisfactiondistrbuteid = value; }
        }

        public string AnswerId
        {
            get { return _answerid; }
            set { _answerid = value; }
        }
        #endregion

        public decimal? Percnet
        {
            get { return _percent; }
            set { _percent = value; }
        }
    }
    #endregion
}
