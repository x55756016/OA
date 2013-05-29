using System;
using System.Collections.Generic;

using System.Data.Objects.DataClasses;
using System.Runtime.Serialization;

namespace SMT_FB_EFModel
{
    #region FBEntity

    // Summary:
    //     The state of an entity object.
    [Flags]
    public enum FBEntityState
    {
        // Summary:
        //     The object exists but it is not being tracked by Object Services. An entity
        //     is in this state immediately after it has been created and before it is added
        //     to the object context. An entity is also in this state after it has been
        //     removed from the context by calling the System.Data.Objects.ObjectContext.Detach(System.Object)
        //     method or if it is loaded using a System.Data.Objects.MergeOption.NoTrackingSystem.Data.Objects.MergeOption.
        Detached = 1,
        //
        // Summary:
        //     The object has not been modified since it was loaded into the context or
        //     since the last time that the System.Data.Objects.ObjectContext.SaveChanges()
        //     method was called.
        Unchanged = 2,
        //
        // Summary:
        //     The object is added to the object context and the System.Data.Objects.ObjectContext.SaveChanges()
        //     method has not been called. Objects are added to the object context by calling
        //     the System.Data.Objects.ObjectContext.AddObject(System.String,System.Object)
        //     method.
        Added = 4,
        //
        // Summary:
        //     The object is deleted from the object context by using the System.Data.Objects.ObjectContext.DeleteObject(System.Object)
        //     method.
        Deleted = 8,
        //
        // Summary:
        //     The object is changed but the System.Data.Objects.ObjectContext.SaveChanges()
        //     method has not been called.
        Modified = 16,
        /// <summary>
        /// 重新提交
        /// </summary>
        ReSubmit = 32,
    }

    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class FBEntity : EntityObject
    {
        public FBEntity()
        {
            FBEntityState = FBEntityState.Unchanged;
            ReferencedEntity = new List<RelationOneEntity>();
            CollectionEntity = new List<RelationManyEntity>();
        }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public EntityObject Entity { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public List<RelationOneEntity> ReferencedEntity { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public List<RelationManyEntity> CollectionEntity { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public FBEntityState FBEntityState { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public Boolean Actived { get; set; }
    }

    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    [KnownType(typeof(RelationManyEntity))]
    [KnownType(typeof(RelationOneEntity))]
    public class RelationEntity : EntityObject
    {
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string PropertyName { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string EntityType { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string RelationshipName { get; set; }
    }
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class RelationManyEntity : RelationEntity
    {
        public RelationManyEntity()
        {
            FBEntities = new List<FBEntity>();
        }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public List<FBEntity> FBEntities { get; set; }
    }

    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class RelationOneEntity : RelationEntity
    {
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public FBEntity FBEntity { get; set; }
    }
    public enum EditStatus
    {
        Delete, New, Edit, Remove
    }

#region 财务数据实体
    [DataContract]
    public class DebtInfo
    {
        [DataMember]
        public string EmployeeID { get; set; }
        [DataMember]
        public decimal UsableSalary { get; set; }
        [DataMember]
        public decimal Debt { get; set; }

        [DataMember]
        public string OrderType { get; set; }

        [DataMember]
        public string OrderCode { get; set; }

        [DataMember]
        public string OrderID { get; set; }
    }
    
#endregion

    #endregion
}
