using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.FB.UI.Common;
using System.Reflection;
using SMT.FB.UI.FBCommonWS;
using SMT.SaaS.FrameworkUI;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI.Validator;
using System.Net.Browser;

namespace SMT.FB.UI.Common
{
    public partial class EditForm : UserControl
    {
        
        ValidatorManager vmGroup;

        public EditForm()
        {
            InitializeComponent();
            this.Controls = new List<IControlAction>();
            RefreshEntityWhenLoad = true;

            vmGroup = new ValidatorManager();
            vmGroup.Name = "Group1";
            this.MainGrid.Children.Add(vmGroup);            
        }            

        private OperationTypes operationType = OperationTypes.Add;
        public OperationTypes OperationType
        {
            get
            {
                return operationType;
            }
            set
            {
                operationType = value;
            }
        }
        private bool isInit = true;

        public EditForm(OrderEntity orderEntity)
            : this()
        {
            this.OrderEntity = orderEntity;

        }
        
        #region 属性
        public OrderEntityService OrderService { set; get; }
        private OrderEntity orderEntity = null;
        public OrderEntity OrderEntity
        {
            get
            {
                return orderEntity;
            }
            set
            {
                if (object.Equals(orderEntity, value) || (value == null))
                {
                    return;
                }

                orderEntity = value;

                this.DataContext = orderEntity;
                
                if (OrderEntityChanged != null)
                {
                    OrderEntityChanged(this, null);
                }
            }
        }

       
        public OrderInfo OrderInfo { get; set; }
        public List<IControlAction> Controls { get; set; }
        [DefaultValue(true)]
        public bool RefreshEntityWhenLoad { get; set; }

        private bool _IsReInitForm = false;
        public bool IsReInitForm
        {
            get
            {
                return _IsReInitForm;
            }
        }

        private bool _IsAduitRefresh = false;
        #endregion

        #region 方法
        public Control FindControl(string name)
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                Control c = Controls[i].FindControl(name);
                if (c != null)
                {
                    return c;
                }
            }
            return null;
        }
        public FrameworkElement this[string name]
        {
            get
            {
                return this.Controls.FirstOrDefault(item => item.Name == name) as FrameworkElement;
            }
            set
            {
                throw new Exception("不可以设置");
            }
        }
        public bool CheckBeforeSave(FBEntity saveFBEntity)
        {
            List<ValidatorBase> listValidation = vmGroup.ValidateAll();
            if (listValidation.Count > 0)
            {
                List<string> listMgs = listValidation.CreateList(validator =>
                {
                    return validator.ErrorMessage;
                });
                CommonFunction.ShowErrorMessage(listMgs);
                return false;
            }
            return OnSaving(saveFBEntity);
        }
        public bool Save()
        {
            FBEntity saveFBEntity = this.OrderEntity.GetModifiedFBEntity();
            if (!CheckBeforeSave(saveFBEntity))
            {
                OnSavedCompleted(Actions.Cancel);
                return false;       
            }

            bool isNeedToSave = this.OrderEntity.FBEntityState != FBEntityState.Unchanged;
            if (!isNeedToSave)
            {
                OnSavedCompleted(Actions.NoAction);
                return false;
            }

            this.SaveData();
            this.OrderService.Save(saveFBEntity);
            return true;
        }

        #endregion

        #region 事件
        
        public event EventHandler<SavingEventArgs> Saving;
        public event EventHandler<SavingEventArgs> SaveCompleted;
        public event EventHandler LoadDataComplete;
        public event EventHandler LoadControlComplete;
        //public event EventHandler<PropertyChangedEventArgs> OrderEntityChanged;
        public event EventHandler OrderEntityChanged;
        #endregion

        #region 初始化方法

        public void AuditinitForm(bool isRefreshForm)
        {
            _IsAduitRefresh = true;
            InitForm();
            _IsAduitRefresh = false;
        }
        public void InitForm()
        {
            if (OrderService == null)
            {
                OrderService = new OrderEntityService();
                OrderService.SaveCompleted += new EventHandler<ActionCompletedEventArgs<OrderEntity>>(OrderSource_SaveCompleted);
                OrderService.GetEntityCompleted += new EventHandler<ActionCompletedEventArgs<OrderEntity>>(OrderService_GetEntityCompleted);
                OrderInfo order = OrderEntity.OrderInfo;
                this.OrderInfo = order;
            }

            InitData();
        }        

        void OrderService_GetEntityCompleted(object sender, ActionCompletedEventArgs<OrderEntity> e)
        {
            OrderEntity oe = e.Result;
            if (oe == null)
            {
                CommonFunction.ShowErrorMessage("未找到相应记录!");
            }
            if (this.OperationType == OperationTypes.ReSubmit)
            {
                this.orderEntity.IsReSubmit = true;
                this.OperationType = OperationTypes.Edit;
            }
            oe.IsReSubmit = this.orderEntity.IsReSubmit;
            this.OrderEntity = oe;
           
            OnLoadDataCompleted();
            InitControl();
        }

        
        private void InitData()
        {
            _IsReInitForm = !isInit;
            OrderInfo order = this.OrderInfo;
            //if (this.OperationType == OperationTypes.ReSubmit)
            //{
                
            //    this.OrderEntity.FBEntityState = FBEntityState.ReSubmit;
            //    this.OrderService.Save(this.OrderEntity);
            //    this.OperationType = OperationTypes.Add;
            //    return;
            //}

            //if (this.OrderEntity.FBEntityState == FBEntityState.ReSubmit)
            //{
            //    this.OperationType = OperationTypes.Edit;
            //    OnLoadDataCompleted();
            //    InitControl();
            //}
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                OnLoadDataCompleted();
                InitControl();
            }
            else 
            {
                if ((RefreshEntityWhenLoad && isInit) || _IsAduitRefresh)
                {
                    QueryExpression q = new QueryExpression();
                    q.QueryType = this.OrderEntity.Entity.GetType().Name;
                    q.PropertyName = OrderEntity.KeyName;
                    q.PropertyValue = OrderEntity.OrderID;
                    q.Operation = QueryExpression.Operations.Equal;
                    q.VisitAction = ((int)this.OperationType).ToString();

                    q.Include = this.OrderInfo.OrderEntity.Entity.Include;
                    OrderService.GetEntity(q);
                }
                else
                {
                    OnLoadDataCompleted();
                    InitControl();
                }   
            }
        }

        private void InitControl()
        {
            try
            {
                if (isInit)
                {
                    OrderInfo order = this.OrderInfo;
                    List<IControlAction> listC = new List<IControlAction>();

                    FormInfo f = order.Form;

                    for (int i = 0; i < f.Panels.Count; i++)
                    {
                        listC.Add(f.Panels[i].GetUIElement());
                    }

                    for (int i = 0; i < listC.Count; i++)
                    {
                        RowDefinition r = new RowDefinition();
                        r.Height = new GridLength(30, GridUnitType.Auto);
                        if (i == listC.Count - 1)
                        {
                            r.Height = new GridLength(1, GridUnitType.Star);
                        }
                        this.MainGrid.RowDefinitions.Add(r);

                        UIElement ue = listC[i] as UIElement;
                        ue.SetValue(Grid.RowProperty, i);

                        this.MainGrid.Children.Add(ue);

                        listC[i].ValidatorManager = this.vmGroup;
                        listC[i].InitControl(OperationType);
                        listC[i].DataContext = this.OrderEntity;
                    }
                    Controls = listC;
                    isInit = false;
                }
                BindingData();
                OnLoadControlCompleted();
            }
            catch (Exception ex)
            {
                throw new FBSystemException(ex.ToString());
            }
        }

        public bool SaveData()
        {
            //var listC = Controls;
            //var tempList = listC.ToList();

            //Action<IControlAction> actionSaveData = (ca) =>
            //    {
            //        lock (tempList)
            //        {
            //            tempList.Remove(ca);
            //        }

            //    };

            //bool result = true;
            
            //for (int i = 0; i < listC.Count; i++)
            //{
            //    result &= listC[i].SaveData(actionSaveData);
            //}
            //return result;
            Controls.ForEach(item =>
                {
                    item.SaveData(null);
                });
            return true;
        }

        private void BindingData()
        {
            if (Controls == null)
            {
                FBBaseControl fb = new FBBaseControl();
                CommonFunction.ShowErrorMessage("无可用数据");
                fb.CloseProcess();
            }
            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].DataContext = this.OrderEntity;
            }
        }
        #endregion

        #region CUID

        void OrderSource_SaveCompleted(object sender, ActionCompletedEventArgs<OrderEntity> e)
        {
            if (e.Result != null)
            {
                this.OrderEntity = e.Result;
                this.InitData();
            }
            if (this.OrderEntity.FBEntityState != FBEntityState.ReSubmit)
            {
                OnSavedCompleted(Actions.Save);
            }
            else
            {
                this.OrderEntity.FBEntityState = FBEntityState.Added;
            }
            
        }

        private void OnSavedCompleted(Actions action)
        {
            if (SaveCompleted != null)
            {

                SavingEventArgs args = new SavingEventArgs(action);
                SaveCompleted(this, args);
            }
        }
        void OrderSerivce_AuditCompleted(object sender, ActionCompletedEventArgs<bool> e)
        {

            this.InitData();
            if (SaveCompleted != null)
            {
                SaveCompleted(this, null);
            }
        }

        #endregion

       
        protected virtual void OnLoadDataCompleted()
        {

            ObjectList<ReferencedDataInfo> listRef = this.OrderInfo.OrderEntity.ReferenceDatas;
            ObjectList<OrderDetailEntityInfo> listDetail = this.OrderInfo.OrderEntity.OrderDetailEntities;

            this.OrderEntity.OrderDetailData.Dictionary = listDetail.Dictionary;

            CheckOrderAction();
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                this.OrderEntity.ReferencedData.Dictionary = listRef.Dictionary;
                this.OrderEntity.SetObjValue(EntityFieldName.CreateDate, DateTime.Now);
                this.OrderEntity.SetObjValue(EntityFieldName.EditStates, decimal.Parse(((int)(EditStates.Actived)).ToString()));
                this.OrderEntity.SetObjValue(EntityFieldName.CheckStates, decimal.Parse(((int)(SMT.FB.UI.FBCommonWS.CheckStates.UnSubmit)).ToString()));
                
                
                EmployeerData CreateUser = this.OrderEntity.LoginUser;
                this.OrderEntity.SetObjValue(EntityFieldName.CreateUserID, CreateUser.Value);
                this.OrderEntity.SetObjValue("Entity.CREATEUSERNAME", CreateUser.Text);
                this.OrderEntity.SetObjValue("Entity.CREATECOMPANYID", CreateUser.Company.Value);
                this.OrderEntity.SetObjValue("Entity.CREATECOMPANYNAME", CreateUser.Company.Text);

                this.OrderEntity.SetObjValue("Entity.CREATEDEPARTMENTID", CreateUser.Department.Value);
                this.OrderEntity.SetObjValue("Entity.CREATEDEPARTMENTNAME", CreateUser.Department.Text);

                this.OrderEntity.SetObjValue("Entity.CREATEPOSTID", CreateUser.Post.Value);
                this.OrderEntity.SetObjValue("Entity.CREATEPOSTNAME", CreateUser.Post.Text);
               
            }
            
            // 初始化 ReferenceData
            for (int i = 0; i < listRef.Count; i++)
            {
                object value = listRef[i].DefaultValue;
                if (this.OrderEntity.FBEntityState != FBEntityState.Added)
                {
                    value = this.OrderEntity.GetObjValue(listRef[i].ReferencedName);
                }
                ITextValueItem item = GetReference(listRef[i].Type, value);
                this.OrderEntity.ReferencedData.Add(listRef[i].ReferencedName, item);
            }

            // 初始化 OrderDetail
            for (int i = 0; i < listDetail.Count; i++)
            {
                RelationManyEntity relationManyEntity = this.OrderEntity.CollectionEntity.FirstOrDefault(item =>
                    {
                        return item.EntityType == listDetail[i].Entity.Type;
                    });

                ObservableCollection<FBEntity> listOfEntity = null;
                if (relationManyEntity == null)
                {
                    listOfEntity = new ObservableCollection<FBEntity>();
                    relationManyEntity = new RelationManyEntity();
                    relationManyEntity.EntityType = listDetail[i].Entity.Type;
                    relationManyEntity.FBEntities = listOfEntity;
                    this.OrderEntity.CollectionEntity.Add(relationManyEntity);
                }
                else
                {
                    listOfEntity = relationManyEntity.FBEntities;
                }
                this.OrderEntity.OrderDetailData[listDetail[i].Name] = listOfEntity;
            }

            if (LoadDataComplete != null)
            {
                LoadDataComplete(this, null);
            }
        }

        private void CheckOrderAction()
        {
            object value = this.OrderEntity.GetObjValue("Entity.CHECKSTATES");
            SMT.FB.UI.FBCommonWS.CheckStates checkState =  (SMT.FB.UI.FBCommonWS.CheckStates)Convert.ToInt32(value);
            
            // 草稿状态
            bool isNotDraft = checkState != SMT.FB.UI.FBCommonWS.CheckStates.UnSubmit;
            bool isWritable = (this.OperationType == OperationTypes.Add || this.OperationType == OperationTypes.Edit);

            
            // 表单不是草稿状态，而且以添加或修改进入的，则与只读浏览的操作进行操作。
            if (isNotDraft && isWritable)
            {
                if (!this.orderEntity.IsReSubmit)
                {
                    this.OperationType = OperationTypes.Browse;
                }
            }

            //控制上传控件在审核时保持只读状态
            if (!isWritable || isNotDraft)
            {
                SMT.FB.UI.Common.Controls.FBUploadControl fbUpload = null;
                foreach (IControlAction item in this.Controls)
                {
                    if (item.GetType() == typeof(SMT.FB.UI.Common.Controls.FBUploadControl))
                    {
                        fbUpload = (SMT.FB.UI.Common.Controls.FBUploadControl)item;
                        break;
                    }
                }

                if (fbUpload != null)
                {
                    fbUpload.InitControl(OperationTypes.Browse);
                }
            }
        }

        private ITextValueItem GetReference(string refType, object value)
        {
            ITextValueItem refData = null;
            if ( value == null)
            {
                return refData;
            }
            if ( (value.GetType() == typeof(string)) && (value.ToString().StartsWith("{")) && (value.ToString().EndsWith("}")) )
            {
                refData = this.OrderEntity.GetObjValue(value.ToString()) as ITextValueItem;
            }
            else
            {
                refData = DataCore.FindRefData(refType, value);
            }
            return refData;
        }

        
        //void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    switch (e.PropertyName)
        //    {
        //        case "OWNERID" :
                    
        //            EmployeerData emplyeerData = this.OrderEntity.ReferencedData["Entity.OWNERID"] as EmployeerData;
        //            if (emplyeerData == null)
        //            {
        //                return;
        //            }
        //            this.OrderEntity.SetValue("Entity.OWNERNAME", emplyeerData.Text);
        //            this.OrderEntity.SetValue("Entity.OWNERPOSTID", emplyeerData.Post.Value);
 
        //            break; 
        //        case "OWNERCOMPANYID":
        //            SetTextFromValue<CompanyData>("Entity.OWNERCOMPANYID", "Entity.OWNERCOMPANYNAME");
        //            break;
        //        case "OWNERDEPARTMENTID" :
        //            DepartmentData dData = SetTextFromValue<DepartmentData>("Entity.OWNERDEPARTMENTID", "Entity.OWNERDEPARTMENTNAME");

        //            break;
        //        case "OWNERPOSTID" :
        //            PostData pData = SetTextFromValue<PostData>(EntityFieldName.OwnerPostID, "Entity.OWNERPOSTNAME");
        //            this.OrderEntity.SetValue(EntityFieldName.OwnerCompanyID, pData.Company.Value);
        //            this.OrderEntity.SetValue(EntityFieldName.OwnerDepartmentID, pData.Department.Value);
        //            break;
        //    }
        //    //if (OrderEntityChanged != null)
        //    //{
        //    //    OrderEntityChanged(this, e);
        //    //}
        //}

        private T SetTextFromValue<T>(string valuePropertyName, string NamePropertyName) where T : ITextValueItem
        {
            object value = this.OrderEntity.GetObjValue(valuePropertyName);
            string name = string.Empty;
            ITextValueItem item = DataCore.FindReferencedData<T>(value);
            if (item != null)
            {
                name = item.Text;
            }

            this.OrderEntity.SetObjValue(NamePropertyName, name);
            return (T)item;
            
        }
        protected virtual void OnLoadControlCompleted()
        {
            if (this.OrderEntity.FBEntityState != FBEntityState.Added)
            {
                ComboBox cbb = this.FindControl(FieldName.CreateUserID) as ComboBox;
                if (cbb != null)
                {
                    cbb.IsEnabled = false;
                }
            }

            if (LoadControlComplete != null)
            {
                LoadControlComplete(this, null);
            }
        }

        protected virtual bool OnSaving(FBEntity saveFBEntity)
        {
            if (Saving != null)
            {
                SavingEventArgs savingEventArgs = new SavingEventArgs(saveFBEntity);
                Saving(this, savingEventArgs);
                return !(savingEventArgs.Action == Actions.Cancel);
            }
            return true;
        }

    }
    
}
