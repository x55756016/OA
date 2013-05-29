using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.Objects.DataClasses;

namespace SMT_FB_EFModel
{

    #region unused
    //class CustomEntities
    //{
    //}

    ///// <summary>
    ///// 月度汇总按科目
    ///// </summary>
    //[DataContract]
    //public class V_SubjectDetail1 : EntityObject
    //{
    //    [DataMember]
    //    public T_FB_SUBJECT T_FB_SUBJECT { get; set; }
    //    //[DataMember]
    //    //public string SubjectID{ get; set; }
    //    //[DataMember]
    //    //public string SubjectName { get; set; }
    //    [DataMember]
    //    public Decimal? BudgetMoney { get; set; }
    //}

    ///// <summary>
    ///// 月度汇总按科目
    ///// </summary>
    //[DataContract]
    //public class T_FB_DEPTBUDGETSUMDETAIL : EntityObject
    //{
    //    [DataMember]
    //    public T_FB_SUBJECT T_FB_SUBJECT { get; set; }
    //    //[DataMember]
    //    //public string SubjectID{ get; set; }
    //    //[DataMember]
    //    //public string SubjectName { get; set; }
    //    [DataMember]
    //    public T_FB_DEPTBUDGETAPPLYMASTER T_FB_DEPTBUDGETAPPLYMASTER { get; set; }
    //    [DataMember]
    //    public Decimal? BudgetMoney { get; set; }
    //}

    ///// <summary>
    ///// 月度汇总按科目
    ///// </summary>
    //[DataContract]
    //public class T_FB_COMPANYBUDGETSUMDETAIL : EntityObject
    //{
    //    [DataMember]
    //    public T_FB_SUBJECT T_FB_SUBJECT { get; set; }
    //    //[DataMember]
    //    //public string SubjectID{ get; set; }
    //    //[DataMember]
    //    //public string SubjectName { get; set; }
    //    [DataMember]
    //    public T_FB_COMPANYBUDGETAPPLYMASTER T_FB_COMPANYBUDGETAPPLYMASTER { get; set; }
    //    [DataMember]
    //    public Decimal? BudgetMoney { get; set; }
    //}

    ///// <summary>
    ///// 月度汇总按部门
    ///// </summary>
    //[DataContract]
    //public class V_DeptDetail1 : EntityObject
    //{
    //    //[DataMember]
    //    //public T_FB_SUBJECT T_FB_SUBJECT { get; set; }
    //    [DataMember]
    //    public T_FB_DEPTBUDGETAPPLYMASTER T_FB_DEPTBUDGETAPPLYMASTER { get; set; }
       
    //    [DataMember]
    //    public Decimal? BudgetMoney { get; set; }
    //}
    #endregion

    [DataContract]
    public class ChargeParameter
    {
        /// <summary>
        /// 当前总费用
        /// </summary>
        [DataMember]
        public decimal ChargeMoney { get; set; }
        /// <summary>
        /// 当前总借款
        /// </summary>
        [DataMember]
        public decimal BorrowMoney { get; set; }
        /// <summary>
        /// 已借款
        /// </summary>
        [DataMember]
        public decimal BorrowedMoney { get; set; }
        /// <summary>
        /// 差旅费
        /// </summary>
        [DataMember]
        public decimal TravelMoney { get; set; }
        /// <summary>
        /// 招待费
        /// </summary>
        [DataMember]
        public decimal EntertainmentMoney { get; set; }

        /// <summary>
        /// 差旅费科目ID
        /// </summary>
        [DataMember]
        public string TravelSubjectID { get; set; }

        /// <summary>
        /// 招待费科目ID
        /// </summary>
        [DataMember]
        public string EntertainmentSubjectID { get; set; }

    }

    public partial class T_FB_TRAVELEXPAPPLYDETAIL
    {
        [DataMember]
        public decimal? PersonUsableMoney { get; set; }
        [DataMember]
        public decimal? DeptUsableMoney { get; set; }

    }
    
}
