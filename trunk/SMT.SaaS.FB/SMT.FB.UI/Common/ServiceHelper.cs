using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.FB.UI.FBCommonWS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq;
using SMT.SaaS.FrameworkUI.Common;
using System.ComponentModel;
namespace SMT.FB.UI.Common
{
    public class ServiceHelper
    {
        public static void QueryFBEntities(QueryExpression qe, Action<object, QueryFBEntitiesCompletedEventArgs> callBackMethod)
        {
            FBEntityService service = new FBEntityService();
            service.QueryFBEntitiesCompleted += (o, e) =>
                {
                    callBackMethod(o, e);
                };
            service.QueryFBEntities(qe);
        }

        static void service_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

    #region Service

    public class GetOrderCompletedEventArgsa : EventArgs
    {
        private IList<OrderEntity> result;
        public IList<OrderEntity> Result
        {
            get
            {
                return result;
            }
        }
        public GetOrderCompletedEventArgsa(IList<OrderEntity> list)
        {
            this.result = list;
        }
    }

    public class OrderEntityService : FBEntityService
    {
        public OrderEntityService()
            : base()
        {
            FBService.GetFBEntityCompleted += new EventHandler<GetFBEntityCompletedEventArgs>(fbService_GetFBEntityCompleted);
            FBService.SaveCompleted += new EventHandler<SaveCompletedEventArgs>(fbService_SaveCompleted);

            FBService.SaveListCompleted += new EventHandler<SaveListCompletedEventArgs>(fbService_SaveListCompleted);
        }



        void fbService_SaveListCompleted(object sender, SaveListCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CommonFunction.ShowErrorMessage("操作失败, " + e.Error.Message);
            }

            if (SaveListCompleted != null)
            {
                SaveListCompleted(this, new ActionCompletedEventArgs<bool>(e.Result));
            }
        }



        void fbService_SaveCompleted(object sender, SaveCompletedEventArgs e)
        {
            FBEntity fbEntity = null;
            if (e.Error != null)
            {
                CommonFunction.ShowErrorMessage("操作失败, " + e.Error.Message);
                
            }
            else if (e.Result.Exception != null)
            {
                CommonFunction.ShowErrorMessage(e.Result.Exception);    
            }

            fbEntity = e.Result.FBEntity;
            if (SaveCompleted != null)
            {
                OrderEntity entityAdapter = null;
                if (fbEntity != null)
                {
                    entityAdapter = new OrderEntity(e.Result.FBEntity);
                }

                ActionCompletedEventArgs<OrderEntity> se = new ActionCompletedEventArgs<OrderEntity>(entityAdapter);
                SaveCompleted(this, se);

            }
        }

        void fbService_GetFBEntityCompleted(object sender, GetFBEntityCompletedEventArgs e)
        {
            FBBaseControl fb = new FBBaseControl();
            if (e.Error != null)
            {
                CommonFunction.ShowErrorMessage("操作失败, " + e.Error.Message);
            }
            if (e.Result == null)
            {
                CommonFunction.ShowErrorMessage("无可用数据");
                fb.CloseProcess();
            }
            if (GetEntityCompleted != null)
            {
                OrderEntity entityAdapter = new OrderEntity(e.Result);
                ActionCompletedEventArgs<OrderEntity> se = new ActionCompletedEventArgs<OrderEntity>(entityAdapter);
                GetEntityCompleted(this, se);
            }
        }


        public void Delete(OrderEntity orderEntity)
        {
            orderEntity.FBEntityState = FBEntityState.Deleted;
            this.Save(orderEntity);
        }

        public void Delete(List<OrderEntity> list)
        {
            list.ForEach(item =>
            {
                item.FBEntityState = FBEntityState.Deleted;
            });
        }

        public void Remove(List<OrderEntity> list)
        {
            list.ForEach(item =>
            {
                item.FBEntityState = FBEntityState.Detached;
            });
        }


        public void GetEntities(QueryExpression qe)
        {
            SetVisitUser(qe);

        }

        public void GetEntity(QueryExpression qe)
        {
            SetVisitUser(qe);
            FBService.GetFBEntityAsync(qe);
        }

        public void Save(OrderEntity orderEntity)
        {
            FBEntity newEntity = orderEntity.GetModifiedFBEntity();

            Save(newEntity);
        }

        public void Save(FBEntity fbEntity)
        {
            SetVisitUser(fbEntity);
            FBService.SaveAsync(fbEntity);

         
        }

        public void SaveList(List<OrderEntity> listOfEntity)
        {

            ObservableCollection<FBEntity> listSave = new ObservableCollection<FBEntity>();
            listOfEntity.ForEach(orderEntity =>
            {
                listSave.Add(orderEntity.GetModifiedFBEntity());
            });
            SaveList(listSave);
        }

        public void SaveList(ObservableCollection<FBEntity> listOfFBEntity)
        {
            //for (int i = 0; i < listOfFBEntity.Count; i++)
            //{
            //    fbService.SaveAsync(listOfFBEntity[i]);
            //}
            FBService.SaveListAsync(listOfFBEntity);
        }

        public event EventHandler<ActionCompletedEventArgs<List<OrderEntity>>> GetEntitiesCompleted;
        public event EventHandler<ActionCompletedEventArgs<OrderEntity>> SaveCompleted;
        public event EventHandler<ActionCompletedEventArgs<OrderEntity>> GetEntityCompleted;
        public event EventHandler<ActionCompletedEventArgs<bool>> SaveListCompleted;
    }

    public class FBEntityService
    {
        private FBCommonServiceClient fbService;
        protected FBCommonServiceClient FBService
        {
            get
            {
                return fbService;
            }
        }
        public FBEntityService()
        {

            fbService = new FBCommonServiceClient();
            fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);
            FBService.QueryCompleted += new EventHandler<QueryCompletedEventArgs>(FBService_QueryCompleted);
        }

        private void FBService_QueryCompleted(object sender, QueryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CommonFunction.ShowErrorMessage("操作失败, " + e.Error.Message);
            }

            OnQueryCompleted(e);
        }

        private void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CommonFunction.ShowErrorMessage(e.Error.ToString());
                return;
            }
            if (QueryFBEntitiesCompleted != null)
            {
                QueryFBEntitiesCompleted(this, e);
            }
        }

        protected virtual void OnQueryCompleted(QueryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CommonFunction.ShowErrorMessage(e.Error.ToString());
                return;
            }
            if (QueryCompleted != null)
            {
                QueryCompleted(this, e);
            }
        }

        public void Query(QueryExpression qe)
        {
            this.ModelCode = qe.QueryType;
            SetVisitUser(qe);
            FBService.QueryAsync(qe);
            
        }

        public void QueryFBEntities(QueryExpression qe)
        {
            this.ModelCode = qe.QueryType;
            SetVisitUser(qe);
            fbService.QueryFBEntitiesAsync(qe);
        }

        public event EventHandler<QueryFBEntitiesCompletedEventArgs> QueryFBEntitiesCompleted;

        public event EventHandler<QueryCompletedEventArgs> QueryCompleted;

        public void SetVisitUser(object param)
        {
            VisitUserBase v = param as VisitUserBase;
            if (v != null)
            {
                try
                {
                    v.VisitUserID = DataCore.CurrentUser.Value.ToString();
                    v.VisitModuleCode = ModelCode;
                    if (string.IsNullOrEmpty(v.VisitAction))
                    {
                        v.VisitAction = ((int)this.Perm).ToString();
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("null user");
                }
            }
        }

        public string ModelCode { get; set; }
        [DefaultValue(Permissions.Browse)]
        public Permissions Perm { get; set; }
    }

    public class AuditService
    {
        private FBCommonServiceClient fbService;
        protected FBCommonServiceClient FBService
        {
            get
            {
                return fbService;
            }
        }

        public AuditService()
        {
            fbService = new FBCommonServiceClient();
            fbService.AuditFBEntityCompleted += new EventHandler<AuditFBEntityCompletedEventArgs>(fbService_AuditFBEntityCompleted);
        }

        public event EventHandler<AuditFBEntityCompletedEventArgs> AuditFBEntityCompleted;
        void fbService_AuditFBEntityCompleted(object sender, AuditFBEntityCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CommonFunction.ShowErrorMessage("操作失败, " + e.Error.Message);
            }

            if (AuditFBEntityCompleted != null)
            {
                AuditFBEntityCompleted(this, e);
            }
        }
        public void AuditFBEntity(FBEntity fbEntity, CheckStates checkStates)
        {
            FBService.AuditFBEntityAsync(fbEntity, checkStates);
        }
    }
    #endregion
}
