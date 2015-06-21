using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.Objects.DataClasses;

namespace TM_SaaS_OA_EFModel
{

    #region VirtualEntityObject
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    [KnownType(typeof(VirtualCompany))]
    [KnownType(typeof(VirtualDepartment))]
    [KnownType(typeof(VirtualPost))]
    [KnownType(typeof(VirtualUser))]
    public class VirtualEntityObject : EntityObject
    {
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public virtual string ID { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public virtual string Name { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATECOMPANYID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATECOMPANYNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEDEPARTMENTID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEDEPARTMENTNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEPOSTID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEPOSTNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEUSERID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEUSERNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERCOMPANYID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERCOMPANYNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERDEPARTMENTID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERDEPARTMENTNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERPOSTID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERPOSTNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string UPDATEUSERID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string UPDATEUSERNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public decimal EDITSTATES { get; set; }

    }

    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class VirtualAuditOrder : VirtualEntityObject
    {
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public virtual string OrderID { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public virtual string OrderCode { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public virtual string OrderType { get; set; }
    }

    #region Subject
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class VirtualCompany : VirtualEntityObject
    {
        public VirtualCompany()
        {
            DepartmentCollection = new List<VirtualDepartment>();
        }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public override string ID
        {
            set
            {
                OWNERCOMPANYID = value;
            }
            get
            {
                return OWNERCOMPANYID;
            }
        }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public override string Name
        {
            set
            {
                OWNERCOMPANYNAME = value;
            }
            get
            {
                return OWNERCOMPANYNAME;
            }
        }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public List<VirtualDepartment> DepartmentCollection { get; set; }
    }

    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class VirtualDepartment : VirtualEntityObject
    {
        public VirtualDepartment()
        {
            PostCollection = new List<VirtualPost>();
        }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public override string ID
        {
            set
            {
                OWNERDEPARTMENTID = value;
            }
            get
            {
                return OWNERDEPARTMENTID;
            }
        }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public override string Name
        {
            set
            {
                OWNERDEPARTMENTNAME = value;
            }
            get
            {
                return OWNERDEPARTMENTNAME;
            }
        }

        private VirtualCompany virtualCompany;
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public VirtualCompany VirtualCompany
        {
            get
            {
                return virtualCompany;
            }
            set
            {
                virtualCompany = value;
                this.OWNERCOMPANYID = virtualCompany.ID;
                this.OWNERCOMPANYNAME = virtualCompany.Name;
            }
        }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public List<VirtualPost> PostCollection { get; set; }

    }
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class VirtualPost : VirtualEntityObject
    {
        public VirtualPost()
        {
        }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public override string ID
        {
            set
            {
                OWNERPOSTID = value;
            }
            get
            {
                return OWNERPOSTID;
            }
        }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public override string Name
        {
            set
            {
                OWNERPOSTNAME = value;
            }
            get
            {
                return OWNERPOSTNAME;
            }
        }

        private VirtualDepartment virtualDepartment;
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public VirtualDepartment VirtualDepartment
        {
            get
            {
                return virtualDepartment;
            }
            set
            {
                virtualDepartment = value;
                this.OWNERDEPARTMENTID = virtualDepartment.ID;
                this.OWNERDEPARTMENTNAME = virtualDepartment.Name;
            }
        }

        private VirtualCompany virtualCompany;
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public VirtualCompany VirtualCompany
        {
            get
            {
                return virtualCompany;
            }
            set
            {
                virtualCompany = value;
                this.OWNERCOMPANYID = virtualCompany.ID;
                this.OWNERCOMPANYNAME = virtualCompany.Name;
            }
        }
    }

    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class VirtualUser : VirtualEntityObject
    {

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public override string ID
        {
            set
            {
                OWNERID = value;
            }
            get
            {
                return OWNERID;
            }
        }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public override string Name
        {
            set
            {
                OWNERNAME = value;
            }
            get
            {
                return OWNERNAME;
            }
        }

        private VirtualPost virtualPost;
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public VirtualPost VirtualPost
        {
            get
            {
                return virtualPost;
            }
            set
            {
                virtualPost = value;
                this.OWNERPOSTID = virtualPost.OWNERPOSTID;
                this.OWNERPOSTNAME = virtualPost.OWNERPOSTNAME;

                this.OWNERCOMPANYID = virtualPost.OWNERCOMPANYID;
                this.OWNERCOMPANYNAME = virtualPost.OWNERCOMPANYNAME;

                this.OWNERDEPARTMENTID = virtualPost.OWNERDEPARTMENTID;
                this.OWNERDEPARTMENTNAME = virtualPost.OWNERDEPARTMENTNAME;
            }
        }

    }
    #endregion
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

    /// <summary>
    /// 预算设置
    /// </summary>
    public partial class T_FB_SYSTEMSETTINGS
    {
        [DataMember]
        public Dictionary<string, string> Settings { get; set; }
    }

    [DataContract]
    [Serializable]
    public partial class PersonMoneyAssignAA : T_FB_PERSONMONEYASSIGNMASTER
    {
    }
}
