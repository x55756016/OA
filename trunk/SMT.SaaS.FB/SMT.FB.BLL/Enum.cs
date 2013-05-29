using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.FB.BLL
{
    public enum CompareResult
    {
        Equal, Bigger, Less, None
    }

    public enum SumValid
    {
        IsNotSum = 0, Valid=1, NotValid=2
    }
    /// <summary>
    /// 审批状态
    /// </summary>
    public enum CheckStates
    {
        /// <summary>
        /// 删除
        /// </summary>
        Delete = -1,
        /// <summary>
        /// 待审批
        /// </summary>
        UnSubmit = 0,
        /// <summary>
        /// 审核中
        /// </summary>
        Approving,
        /// <summary>
        /// 审核通过
        /// </summary>
        Approved,
        /// <summary>
        /// 审核不通过
        /// </summary>
        UnApproved,

        /// <summary>
        /// 待审核
        /// </summary>
        WaittingApproval,
       /// <summary>
       /// 重新提交
       /// </summary>
        ReSubmit, 

    }
    public enum EditStates
    {
        Deleted = 0, Actived, UnActived, PendingCancelled, Cancelled

    }
    public struct FieldName
    {
        public const string OwnerID = "OWNERID";
        public const string OwnerPostID = "OWNERPOSTID";
        public const string OwnerDepartmentID = "OWNERDEPARTMENTID";
        public const string OwnerCompanyID = "OWNERCOMPANYID";

        public const string CreateUserID = "CREATEUSERID";
        public const string CreatePostID = "CREATEPOSTID";
        public const string CreateDepartmentID = "CREATEDEPARTMENTID";
        public const string CreateCompanyID = "CREATECOMPANYID";

        public const string UpdateUserID = "UPDATEUSERID";
        public const string UpdateDate = "UPDATEDATE";
        public const string CreateDate = "CREATEDATE";

        public const string EditStates = "EDITSTATES";
        public const string CheckStates = "CHECKSTATES";

    }

    public enum ChargeType
    {
        Person =1, Commmon =2
    }


    // 摘要:
    //     The state of an entity object.
    [Flags]
    public enum FBEntityStates
    {
      
        Detached = 1,
      
        Unchanged = 2,
       
        Added = 4,
       
        Deleted = 8,
     
        Modified = 16,
    }

}
