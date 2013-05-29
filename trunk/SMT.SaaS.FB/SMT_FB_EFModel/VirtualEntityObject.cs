using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace SMT_FB_EFModel
{
    #region VirtualEntityObject
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    [KnownType(typeof(VirtualCompany))]
    [KnownType(typeof(VirtualDepartment))]
    [KnownType(typeof(VirtualPost))]
    [KnownType(typeof(VirtualUser))]
    [KnownType(typeof(VirtualAudit))]
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
    public class VirtualAudit : VirtualEntityObject
    {
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public int Result { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Content { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string ModelCode { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string FormID { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string GUID { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string NextStateCode { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Op { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public int FlowSelectType { get; set; }

        

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
}
