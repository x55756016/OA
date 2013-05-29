using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.FB.UI.FBCommonWS;
using System.Collections.Specialized;
using System.Linq;
namespace SMT.FB.UI.Common
{
    #region EntityAdapter
    public class EntityAdapter : EntityObject
    {
        protected EntityHelper entityHelper { get; set; }
        protected EntityAdapter()
        {
            entityHelper = new EntityHelper();
        }



        public EntityAdapter(FBEntity fbEntity)
        {
            FBEntity = fbEntity;
            
        }

        #region 属性

        private FBEntity fbEntity = null;
        public FBEntity FBEntity
        {
            get
            {
                return fbEntity;
            }
            set
            {
                if (!object.Equals(fbEntity, value))
                {
                    fbEntity = value;
                     entityHelper.MasterFBEntity = fbEntity;        
                }
            }
        }

        public EntityObject Entity
        {
            get
            {
                return FBEntity.Entity;
            }
            //set
            //{
            //    // throw new Exception("暂时不能设置Entity");
            //    FBEntity.Entity = value;
            //}
        }

        public ObservableCollection<RelationManyEntity> CollectionEntity
        {
            get
            {
                return FBEntity.CollectionEntity;
            }
        }

        public ObservableCollection<RelationOneEntity> ReferencedEntity
        {
            get
            {
                return FBEntity.ReferencedEntity;
            }
        }

        public FBEntityState FBEntityState
        {
            get
            {
                return this.fbEntity.FBEntityState;
            }
            set
            {
                this.fbEntity.FBEntityState = value;
            }
        }

        #endregion

        private EmployeerData _LoginUser = null;
        public EmployeerData LoginUser
        {
            get
            {
                if (_LoginUser == null)
                {
                    _LoginUser = DataCore.CurrentUser;
                }
                return _LoginUser;
            }
            set
            {
                _LoginUser = value;
            }
        }

        public bool IsEntityChanged
        {
            get
            {
                return (fbEntity.FBEntityState != FBEntityState.Unchanged);
            }
        }

        public FBEntity CreateFBEntity<T>() where T : EntityObject
        {
            return entityHelper.CreateEntity<T>();
        }

        public bool DeleteFBEntity(FBEntity fbEntity) 
        {
            return entityHelper.DeleteFBEntity(fbEntity);
        }

        public FBEntity GetModifiedFBEntity()
        {
            FBEntity newEntity = this.Entity.ToFBEntity();
            newEntity.FBEntityState = this.FBEntity.FBEntityState;

            if (newEntity.FBEntityState != FBEntityState.Unchanged)
            {
                newEntity.SetObjValue(EntityFieldName.UpdateUserID, this.LoginUser.Value);
                //if (newEntity.FBEntityState == FBCommonWS.FBEntityState.Added)
                //{
                //    newEntity.Entity.EntityKey = null;
                //}
            }

            foreach (RelationManyEntity rme in this.CollectionEntity)
            {
                RelationManyEntity rmeNew = new RelationManyEntity();
                rmeNew.EntityType = rme.EntityType;
                rmeNew.FBEntities = new ObservableCollection<FBEntity>();
                rme.FBEntities.ToList().ForEach(item =>
                {
                    if (item.FBEntityState != FBEntityState.Unchanged)
                    {
                        if (item.FBEntityState == FBEntityState.Added)
                        {
                            item.SetObjValue("Entity.CREATEUSERID", this.GetObjValue("Entity.CREATEUSERID"));
                            item.SetObjValue("Entity.CREATEDATE", this.GetObjValue("Entity.CREATEDATE"));
                           // item.Entity.EntityKey = null;
                        }

                        item.SetObjValue("Entity.UPDATEUSERID", this.GetObjValue("Entity.UPDATEUSERID"));
                        item.SetObjValue("Entity.UPDATEDATE", this.GetObjValue("Entity.UPDATEDATE"));

                        FBEntity fbEntityDetail = item.Entity.ToFBEntity();
                        fbEntityDetail.FBEntityState = item.FBEntityState;
                        rmeNew.FBEntities.Add(fbEntityDetail);
                    }
                });

                newEntity.CollectionEntity.Add(rmeNew);
            }

            if (entityHelper.DeleteList.Count > 0)
            {
                RelationManyEntity rmeDel = new RelationManyEntity();
                rmeDel.EntityType = Args.DELETE_ENTITYTYPE;
                rmeDel.FBEntities = this.entityHelper.DeleteList;
                newEntity.CollectionEntity.Add(rmeDel);
            }
            
            return newEntity;
        }

        public object GetEntityValue(string propertyName)
        {
            if (propertyName.StartsWith("{") && propertyName.EndsWith("}"))
            {
                return this.GetObjValue(propertyName);
            }
            else
            {
                return propertyName;
            }
        }
    }

    public class OrderEntity : EntityAdapter
    {
        //public static readonly DependencyProperty ReferencedDataProperty
        //    = DependencyProperty.RegisterAttached("ReferencedData", typeof(ObjectList<ITextValueItem>), typeof(OrderEntity), new PropertyMetadata(""));
        [DefaultValue(false)]
        public bool IsReSubmit { get; set; }
        public OrderEntity()
            : base()
        {
            
            this.ReferencedData = new ObjectList<ITextValueItem>();
            this.OrderDetailData = new ObjectList<ObservableCollection<FBEntity>>();
            this.ReferencedData.ValueChanged += new EventHandler<ObjectList<ITextValueItem>.ObjectListValueChangedArg>(ReferencedData_ValueChanged);

            entityHelper.EntityCharged += new EventHandler<EntityChangedArgs>(entityHelper_EntityCharged);
        }

        void ReferencedData_ValueChanged(object sender, ObjectList<ITextValueItem>.ObjectListValueChangedArg e)
        {
            string propertyName = e.Key;
            ITextValueItem item = e.Value;
            object value = default(object);
            if (item != null)
            {
                value = item.Value;
            }
            this.SetObjValue(e.Key, value);

        }

        public OrderEntity(Type type)
            : this()
        {
            InitOrder(type);

            Type typeOfEntity = CommonFunction.GetType(this.OrderInfo.OrderEntity.Entity.Type, CommonFunction.TypeCategory.EntityObject);
            EntityObject entity = Activator.CreateInstance(typeOfEntity) as EntityObject;
            FBEntity fbEntity = new FBEntity();
            fbEntity.Entity = entity;
            this.FBEntity = fbEntity;
            
            
            this.OrderID = Guid.NewGuid().ToString();
            this.SetObjValue("Entity." + KeyName, OrderID);
            
            //this.SetValue(EntityFieldName.CheckStates, 0);
            this.OrderStatesName = DataCore.FindReferencedData<OrderStatusData>(decimal.Parse("0")).Text;
            FBEntity.FBEntityState = FBEntityState.Added;
        }

        public OrderEntity(EntityObject obj)
            : this()
        {
            if (obj == null)
            {
                return;
            }
            if (obj.GetType() != typeof(FBEntity))
            {
                FBEntity fbEntity = new FBEntity();
                fbEntity.Entity = obj;
                this.FBEntity = fbEntity;
            }
            else
            {
                this.FBEntity = obj as FBEntity;
            }
            
            InitOrder(obj);
            if (FBEntity.FBEntityState == FBEntityState.Modified)
            {
                FBEntity.FBEntityState = FBEntityState.Unchanged;
            }
        }

        private void InitOrder(EntityObject obj)
        {
            OrderInfo = OrderHelper.GetOrderInfo(this.Entity);
            InitOrder();

            this.OrderID = CommonFunction.TryConvertValue(this.GetObjValue("Entity." + KeyName), this.OrderID);

            //if (CodeName != null)
            //{
            //    this.OrderCode = CommonFunction.TryConvertValue(this.GetValue("Entity." + CodeName), this.OrderCode);
            //}
            object value = this.GetObjValue("Entity.CHECKSTATES");
            if (value != null)
            {
                ITextValueItem item = DataCore.FindReferencedData<OrderStatusData>(value);
                if( item  != null)
                {
                    this.OrderStatesName = item.Text;
                }
                
            }

            object objEditStates = this.GetObjValue("Entity.EDITSTATES");
            if (objEditStates != null)
            {
                ITextValueItem item = DataCore.FindReferencedData<BudgetSumStatesData>(objEditStates);
                if (item != null)
                {
                    this.OrderEditStates = item.Text;
                }
                
            }
        }

        private void InitOrder(Type type)
        {
            OrderInfo = OrderHelper.GetOrderInfo(type.Name);
            InitOrder();  
        }
        private void InitOrder()
        {
            if (OrderInfo == null)
            {
                return;
            }
            this.OrderType = CommonFunction.GetType(OrderInfo.Type, CommonFunction.TypeCategory.EntityObject);
            this.OrderTypeName = OrderInfo.Name;
            this.KeyName = OrderInfo.OrderEntity.Entity.KeyName;
            this.CodeName = OrderInfo.OrderEntity.Entity.CodeName;

        }
        #region 共公参数
        public string KeyName { get; set; }
        public string CodeName { get; set; }
        public Type OrderType { get; set; }
        public string OrderID { get; set; }
        public string OrderCode
        {
            get
            {
                return Convert.ToString( this.GetObjValue("Entity." + CodeName));
            }
            set
            {

            }
        }
        public string OrderTypeName { get; set; }
        public string OrderStatesName { get; set; }
        //beyond
        public string OrderEditStates { get; set; }

        private bool isErrorData;
        public bool IsErrorData
        {
            get
            {
                return isErrorData;
            }
            set
            {
                isErrorData = value;
                this.RaisePropertyChanged("IsErrorData");
            }
        }
        private List<string> errorMessage;
        public List<string> ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
                this.RaisePropertyChanged("ErrorMessage");
            }
        }
        //public string OwnerName { get; set; }
        //public string OwnerCompanyName { get; set; }
        //public string OwnerDepartmentName { get; set; }
        //public string CreateUserName { get; set; }
        //public string CreateCompanyName { get; set; }
        //public string CreateDepartmentName { get; set; }
        //public string UpdateUserName { get; set; }
        
        //public DateTime CreateDate { get; set; }
        //public DateTime UpdateDate { get; set; }

        #endregion
        public OrderInfo OrderInfo { get; set; }
        public ObjectList<ITextValueItem> ReferencedData { get; set; }
        public ObjectList<ObservableCollection<FBEntity>> OrderDetailData { get; set; }
        public event EventHandler<OrderPropertyChangedArgs> OrderPropertyChanged;
        public event EventHandler<EntityChangedArgs> CollectionEntityChanged;
        public DateTimeData Now
        {
            get
            {
                DateTimeData dtData = new DateTimeData();
                dtData.Value = DateTime.Now;

                return dtData;
            }
        }

        public MonthItem CurrentMonth
        {
            get
            {
                DateTime dtCurrentMonth = DateTime.Now.Date.AddDays(1- DateTime.Now.Date.Day);
                return DataCore.FindReferencedData<MonthItem>(dtCurrentMonth) as MonthItem;
            }
        }

        public YearItem CurrentYear
        {
            get
            {
                return (YearItem)(DataCore.FindReferencedData<YearItem>(DateTime.Now.Year));

                //                 return dtData;
            }
        }


        void entityHelper_EntityCharged(object sender, EntityChangedArgs ea)
        {
            PropertyChangedEventArgs e = ea.ChangedEventArgs;
            List<string> propertyNameList = new List<string>();
            if (object.Equals(this.Entity, sender))
            {
                if (OwnerList.Contains(e.PropertyName))
                {
                    propertyNameList = OnOwnerPropertyChanged(sender, e);

                }
                else
                {
                    propertyNameList.Add(e.PropertyName.ToEntityString());
                }
                if (OrderPropertyChanged != null)
                {
                    OrderPropertyChangedArgs args = new OrderPropertyChangedArgs(propertyNameList);
                    OrderPropertyChanged(sender, args);
                }
            }
            else
            {
                if (this.CollectionEntityChanged != null)
                {
                    this.CollectionEntityChanged(sender, ea);
                }
            }
        }

        public List<string> OwnerList = new List<string>() { FieldName.OwnerCompanyID, FieldName.OwnerDepartmentID, FieldName.OwnerPostID, FieldName.OwnerID };

        private List<string> OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            List<string> propertyNameList = new List<string>();
            entityHelper.EntityCharged -= new EventHandler<EntityChangedArgs>(entityHelper_EntityCharged);
            try
            {
                switch (e.PropertyName)
                {
                    case "OWNERID":

                        EmployeerData emplyeerData = this.ReferencedData["Entity.OWNERID"] as EmployeerData;
                        if (emplyeerData != null)
                        {
                            this.SetObjValue(EntityFieldName.OwnerName, emplyeerData.Text);

                            this.SetObjValue(EntityFieldName.OwnerPostID, emplyeerData.Post.Value);
                            this.SetObjValue(EntityFieldName.OwnerPostName, emplyeerData.Post.Text);

                            this.SetObjValue(EntityFieldName.OwnerDepartmentID, emplyeerData.Department.Value);
                            this.SetObjValue(EntityFieldName.OwnerDepartmentName, emplyeerData.Department.Text);

                            this.SetObjValue(EntityFieldName.OwnerCompanyID, emplyeerData.Company.Value);
                            this.SetObjValue(EntityFieldName.OwnerCompanyName, emplyeerData.Company.Text);
                            propertyNameList.Add(EntityFieldName.OwnerID);
                            propertyNameList.Add(EntityFieldName.OwnerName);

                            propertyNameList.Add(EntityFieldName.OwnerPostID);
                            propertyNameList.Add(EntityFieldName.OwnerPostName);

                            propertyNameList.Add(EntityFieldName.OwnerDepartmentID);
                            propertyNameList.Add(EntityFieldName.OwnerDepartmentName);

                            propertyNameList.Add(EntityFieldName.OwnerCompanyID);
                            propertyNameList.Add(EntityFieldName.OwnerCompanyName);
                        }


                        break;
                    case "OWNERCOMPANYID":
                        SetTextFromValue<CompanyData>("Entity.OWNERCOMPANYID", "Entity.OWNERCOMPANYNAME");

                        propertyNameList.Add(EntityFieldName.OwnerCompanyID);
                        propertyNameList.Add(EntityFieldName.OwnerCompanyName);

                        break;
                    case "OWNERDEPARTMENTID":
                        DepartmentData dData = SetTextFromValue<DepartmentData>("Entity.OWNERDEPARTMENTID", "Entity.OWNERDEPARTMENTNAME");
                        propertyNameList.Add(EntityFieldName.OwnerDepartmentID);
                        propertyNameList.Add(EntityFieldName.OwnerDepartmentName);
                        try
                        {
                            if (dData != null)
                            {
                                this.SetObjValue(EntityFieldName.OwnerCompanyID, dData.Company.Value);
                                this.SetObjValue(EntityFieldName.OwnerCompanyName, dData.Company.Text);

                                propertyNameList.Add(EntityFieldName.OwnerCompanyID);
                                propertyNameList.Add(EntityFieldName.OwnerCompanyName);
                            }
                        }
                        catch (Exception ex)
                        {
                            
                        }
                        break;
                    case "OWNERPOSTID":
                        PostData pData = SetTextFromValue<PostData>(EntityFieldName.OwnerPostID, "Entity.OWNERPOSTNAME");
                        //this.SetObjValue(EntityFieldName.OwnerCompanyID, pData.Company.Value);
                        //this.SetObjValue(EntityFieldName.OwnerCompanyName, pData.Company.Text);

                        //this.SetObjValue(EntityFieldName.OwnerDepartmentID, pData.Department.Value);
                        //this.SetObjValue(EntityFieldName.OwnerDepartmentName, pData.Department.Text);

                        //propertyNameList.Add(EntityFieldName.OwnerPostID);
                        //propertyNameList.Add(EntityFieldName.OwnerPostName);

                        //propertyNameList.Add(EntityFieldName.OwnerDepartmentID);
                        //propertyNameList.Add(EntityFieldName.OwnerDepartmentName);

                        //propertyNameList.Add(EntityFieldName.OwnerCompanyID);
                        //propertyNameList.Add(EntityFieldName.OwnerCompanyName);

                        break;
                    default:
                        propertyNameList.Add(e.PropertyName.ToEntityString());
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            entityHelper.EntityCharged += new EventHandler<EntityChangedArgs>(entityHelper_EntityCharged);
            return propertyNameList;
        }
        private T SetTextFromValue<T>(string valuePropertyName, string NamePropertyName) where T : ITextValueItem
        {
            object value = this.GetObjValue(valuePropertyName);
            string name = string.Empty;
            ITextValueItem item = DataCore.FindReferencedData<T>(value);
            if (item != null)
            {
                name = item.Text;
            }

            this.SetObjValue(NamePropertyName, name);
            return (T)item;

        }

        public EmployeerData GetOwnerInfo()
        {
            string OwnerCompanyID = Convert.ToString(this.GetObjValue(EntityFieldName.OwnerCompanyID));
            string OwnerDepartmentID = Convert.ToString(this.GetObjValue(EntityFieldName.OwnerDepartmentID));
            string OwnerPostID = Convert.ToString(this.GetObjValue(EntityFieldName.OwnerPostID));
            string OwnerID = Convert.ToString(this.GetObjValue(EntityFieldName.OwnerID));
            string OnwerName = Convert.ToString(this.GetObjValue(EntityFieldName.OwnerName));
            string OwnerPostName = Convert.ToString(this.GetObjValue(EntityFieldName.OwnerPostName));
            string ownerDepartmentName = Convert.ToString(this.GetObjValue(EntityFieldName.OwnerDepartmentName));
            string OwnerCompanyName = Convert.ToString(this.GetObjValue(EntityFieldName.OwnerCompanyName));

            EmployeerData result = new EmployeerData
            {
                Value = OwnerID,
                Text = OnwerName,
                Company = new CompanyData { Value = OwnerCompanyID, Text = OwnerCompanyName },
                Department = new DepartmentData { Value = OwnerDepartmentID, Text = ownerDepartmentName },
                Post = new PostData { Value = OwnerPostID, Text = OwnerPostName }
            };
            result.Department.Company = result.Company;
            result.Post.Department = result.Department;
            result.Post.Company = result.Company;
            return result;
            
        }

        public EmployeerData GetCreateInfo()
        {

            string OwnerCompanyID = Convert.ToString(this.GetObjValue(EntityFieldName.CreateCompanyID));
            string OwnerDepartmentID = Convert.ToString(this.GetObjValue(EntityFieldName.CreateDepartmentID));
            string OwnerPostID = Convert.ToString(this.GetObjValue(EntityFieldName.CreatePostID));
            string OwnerID = Convert.ToString(this.GetObjValue(EntityFieldName.CreateUserID));
            string OnwerName = Convert.ToString(this.GetObjValue("Entity.CREATEUSERNAME"));
            string OwnerPostName = Convert.ToString(this.GetObjValue("Entity.CREATEPOSTNAME"));
            string ownerDepartmentName = Convert.ToString(this.GetObjValue("Entity.CREATEDEPARTMENTNAME"));
            string OwnerCompanyName = Convert.ToString(this.GetObjValue("Entity.CREATECOMPANYNAME"));
            
            EmployeerData result = new EmployeerData
            {
                Value = OwnerID,
                Text = OnwerName,
                Company = new CompanyData { Value = OwnerCompanyID, Text = OwnerCompanyName },
                Department = new DepartmentData { Value = OwnerDepartmentID, Text = ownerDepartmentName },
                Post = new PostData { Value = OwnerPostID, Text = OwnerPostName }
            };
            result.Department.Company = result.Company;
            result.Post.Department = result.Department;
            result.Post.Company = result.Company;
            return result;

        }

    }

    public class ObjectList<T> : ObservableCollection<T>
    {
        private Dictionary<string, int> dictionary;
        public Dictionary<string, int> Dictionary
        {
            get
            {
                return dictionary;
            }
            set
            {
                dictionary = value;
                int count = dictionary.Count;
                base.Clear();
                for (int i = 0; i < count; i++)
                {
                    base.Add(default(T));

                }
            }
        }
        public event EventHandler<ObjectListValueChangedArg> ValueChanged;
        public new T this[int index]
        {
            get
            {
                if (base.Count > index)
                {
                    return base[index];
                }
                else
                {
                    return default(T);
                }
            }
            set
            {
                if (index < this.Count)
                {
                    Object oldValue = base[index];  // get old value using collection[index];

                    base[index] = value;
                    OnValuedChanged(index);

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, index));

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error Index");
                }
            }
        }
        public T this[string key]
        {
            get
            {
                int index = Dictionary.Count;
                if (Dictionary.ContainsKey(key))
                {
                    index = Dictionary[key];
                }
                else
                {
                    Dictionary.Add(key, index);
                    base.Add(default(T));
                }
                return this[index];


            }
            set
            {
                int index = Dictionary.Count;
                if (Dictionary.ContainsKey(key))
                {
                    index = Dictionary[key];
                    this[index] = value;
                }
                else
                {

                    this.Add(key, value);
                    OnValuedChanged(index);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
                }


            }
        }

        public ObjectList()
        {
            dictionary = new Dictionary<string, int>();
        }
        public new void Add(T t)
        {
            int index = base.Count;
            this.Add(typeof(T).Name + index.ToString(), t);
        }
        public void Add(string key, T t)
        {
            if (Dictionary.ContainsKey(key))
            {
                this[key] = t;
            }
            else
            {
                Dictionary.Add(key, base.Count);
                base.Add(t);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, t, base.Count));
        }
        private void OnValuedChanged(int index)
        {
            if (ValueChanged != null)
            {
                IEnumerator enumerator = Dictionary.Keys.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Current.ToString();
                    int idx = Dictionary[key];
                    if (idx == index)
                    {
                        ObjectListValueChangedArg e =
                            new ObjectList<T>.ObjectListValueChangedArg(key, index, this[index]);

                        ValueChanged(this, e);
                        break;
                    }
                }
            }
        }

        public class ObjectListValueChangedArg : EventArgs
        {
            public ObjectListValueChangedArg(string key, int index, T value)
            {
                this.Key = key;
                this.Value = value;
                this.Index = index;
            }
            [ReadOnly(true)]
            public string Key { get; set; }
            [ReadOnly(true)]
            public int Index { get; set; }
            [ReadOnly(true)]
            public T Value { get; set; }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }
        #region INotifyCollectionChanged 成员

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }

    public class EntityHelper
    {
        public ObservableCollection<FBEntity> DeleteList;
        private FBEntity masterFBEntity;
        public FBEntity MasterFBEntity
        {
            get
            {
                return masterFBEntity;
            }
            set
            {
                if ( object.Equals(masterFBEntity, value))
                {
                    return;
                }
                masterFBEntity = value;
                
                Init(masterFBEntity);
            }
        }

        public event EventHandler<EntityChangedArgs> EntityCharged;

        private Dictionary<EntityObject, FBEntity> dict;

        public EntityHelper()
        {
            DeleteList = new ObservableCollection<FBEntity>();
            dict = new Dictionary<EntityObject, FBEntity>();
        }
        public EntityHelper(FBEntity fbEntity) : this()
        {
            MasterFBEntity = fbEntity;
           
        }

        public FBEntity CreateEntity<T>() where T : EntityObject
        {
            T t = Activator.CreateInstance<T>();
            FBEntity fbEntity = new FBEntity();
            fbEntity.Entity = t;
            fbEntity.FBEntityState = FBEntityState.Added;
            Init(fbEntity);
            OnCreateEntity(typeof(T).Name, fbEntity);
            return fbEntity;
        }

        public bool DeleteFBEntity(FBEntity  fbEntity)
        {
            bool bDel = false;
            this.MasterFBEntity.CollectionEntity.ToList().ForEach(ce =>
                {
                    //FBEntity entityDel = ce.FBEntities.FirstOrDefault(entity =>
                    //    {
                    //        return object.Equals(entity, fbEntity);
                    //    });
                    //if (entityDel != null)
                    //{
                    //    ce.FBEntities.Remove(entityDel);
                    //}
                    bDel |= ce.FBEntities.Remove(fbEntity);
                });

            return bDel;
        }

        private void RegisterFBEntity(FBEntity fbEntity)
        {
            if (dict.ContainsKey(fbEntity.Entity))
            {
                return;
            }
            fbEntity.PropertyChanged += new PropertyChangedEventHandler(FBEntity_PropertyChanged);
            fbEntity.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Entity_PropertyChanged);
            dict.Add(fbEntity.Entity, fbEntity);
        }

        void FBEntity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnEntityPropertyChanged();
        }

        private void OnCreateEntity(string entityType, FBEntity fbEntity)
        {
            ObservableCollection < FBEntity > entityList = this.MasterFBEntity.GetRelationFBEntities(entityType);

            entityList.Add(fbEntity);
        }

        private void OnEntityPropertyChanged()
        {

            if (MasterFBEntity.FBEntityState == FBEntityState.Unchanged)
            {
                MasterFBEntity.FBEntityState = FBEntityState.Modified;
            }

           
        }

        private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FBEntity fbEntity = dict[(sender as EntityObject)];
            if (fbEntity.FBEntityState == FBEntityState.Unchanged)
            {
                fbEntity.FBEntityState = FBEntityState.Modified;
            }

            if (EntityCharged != null)
            {
                EntityCharged(sender, new EntityChangedArgs(e));
            }
            OnEntityPropertyChanged();
        }

        private void Init(FBEntity fbEntity)
        {
            
            if (fbEntity.CollectionEntity == null)
            {
                fbEntity.CollectionEntity = new ObservableCollection<RelationManyEntity>();
            }

            if (fbEntity.ReferencedEntity == null)
            {
                fbEntity.ReferencedEntity = new ObservableCollection<RelationOneEntity>();
                
            }
            if (fbEntity.FBEntityState == 0)
            {
                fbEntity.FBEntityState = FBEntityState.Added;
            }
            RegisterFBEntity(fbEntity);
            foreach (var rem in fbEntity.CollectionEntity)
            {
                if (rem.FBEntities == null)
                {
                    rem.FBEntities = new ObservableCollection<FBEntity>();
                }

                rem.FBEntities.CollectionChanged += new NotifyCollectionChangedEventHandler(FBEntities_CollectionChanged);
                foreach (FBEntity entity in rem.FBEntities)
                {
                    RegisterFBEntity(entity);   
                }
            }

            fbEntity.CollectionEntity.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionEntity_CollectionChanged);
        }

        /// <summary>
        /// FBEntity.CollectionEntity 的集合变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CollectionEntity_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    RelationManyEntity rme = e.NewItems[i] as RelationManyEntity;
                    rme.FBEntities.CollectionChanged +=new NotifyCollectionChangedEventHandler(FBEntities_CollectionChanged);

                    foreach (FBEntity entity in rme.FBEntities)
                    {
                        RegisterFBEntity(entity);
                    }
                }
            }
            this.OnEntityPropertyChanged();
        }

        /// <summary>
        /// FBEntity.CollectionEntity的每个子集里的Entities的集合事件。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FBEntities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 将删除的实体记录.等待传回service端处理
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    FBEntity fbEntity = e.OldItems[i] as FBEntity;
                    if (fbEntity.FBEntityState != FBEntityState.Added)
                    {
                        if ( !DeleteList.Contains(fbEntity))
                        {
                            fbEntity.FBEntityState = FBEntityState.Detached;
                            DeleteList.Add(fbEntity);
                            fbEntity.Entity.SetObjValue(this.masterFBEntity.Entity.GetType().Name, null);
                        }
                    }
                    if (EntityCharged != null)
                    {
                        EntityCharged(fbEntity.Entity, new EntityChangedArgs(Actions.Delete));
                    }
                }
                
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    FBEntity fbEntity = e.NewItems[i] as FBEntity;
                    RegisterFBEntity(fbEntity);
                    fbEntity.Entity.SetObjValue(this.masterFBEntity.Entity.GetType().Name, this.masterFBEntity.Entity);

                    if (EntityCharged != null)
                    {
                        EntityCharged(fbEntity.Entity, new EntityChangedArgs(Actions.Delete));
                    }
                }

            }
            this.OnEntityPropertyChanged();
        }


       
    }
    
    #endregion
}
