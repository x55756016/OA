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
using SMT.FB.UI.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using SMT.FB.UI.Common.Controls;
using SMT.FB.UI.Common.Controls.DataTemplates;
using System.Windows.Data;
using System.ComponentModel;


namespace SMT.FB.UI.Form.BudgetApply
{
    public class CompanyBudgetModForm : FBPage
    {
        FBEntityService fbService;
        private VirtualCompany _curCompany;
        private VirtualCompany curCompany
        {
            get
            {
                if (_curCompany == null)
                {
                    string companyID = Convert.ToString(OrderEntity.GetObjValue("Entity.OWNERCOMPANYID"));
                    _curCompany = DataCore.CompanyList.FirstOrDefault(company => { return company.ID == companyID; });
                }
                return _curCompany;
            }
            set
            {
                _curCompany = value;
            }
        }
        private Dictionary<T_FB_SUBJECT, FBEntity> dictDetail;
        private Dictionary<T_FB_SUBJECT, ObservableCollection<T_FB_COMPANYBUDGETMODDETAIL>> dictDetailDept;

        Dictionary<string, decimal?> dictComDetail = new Dictionary<string, decimal?>();
        public CompanyBudgetModForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            fbService = new FBEntityService();
            fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);

            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);
         //    this.EditForm.SaveCompleted += new EventHandler<SavingEventArgs>(EditForm_SaveCompleted);
        }

        //void EditForm_SaveCompleted(object sender, SavingEventArgs e)
        //{
        //    if (e.Action != Actions.Cancel && e.Action != Actions.NoAction)
        //    {
        //        InitData();
        //    }
            
        //}

        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            FBEntity modifiedEntity = e.SaveFBEntity;

            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETMODDETAIL).Name);
            if (details.Count == 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(ErrorMessage.NoDetailInfo);
                return;
            }

            List<string> msgs = new List<string>();
            decimal dTotalMoney = 0;
            if (((T_FB_COMPANYBUDGETMODMASTER)modifiedEntity.Entity).BUDGETMONEY != null)
            {
                dTotalMoney = ((T_FB_COMPANYBUDGETMODMASTER)modifiedEntity.Entity).BUDGETMONEY.Value;
            }

            var resultCheck = details.Where(item =>
                {
                    T_FB_COMPANYBUDGETMODDETAIL detail = item.Entity as T_FB_COMPANYBUDGETMODDETAIL;
                    if (detail.BUDGETMONEY < 0)
                    {
                        string errorMessage = string.Format(ErrorMessage.BudgetMoneyZero, detail.T_FB_SUBJECT.SUBJECTNAME);
                        msgs.Add(errorMessage);
                    }

                    return (item.Entity as T_FB_COMPANYBUDGETMODDETAIL).BUDGETMONEY == 0;
                });

            if (dTotalMoney <= 0)
            {
                string errorMessage = "申请的预算总额必须大于零!";
                msgs.Add(errorMessage);
            }

            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }

            details.ToList().ForEach(item =>
                {
                    if (item.IsNewEntity())
                    {
                        item.FBEntityState = FBEntityState.Added;
                        item.SetObjValue("Entity.CREATEUSERID", this.OrderEntity.GetObjValue("Entity.CREATEUSERID"));
                        item.SetObjValue("Entity.CREATEDATE", this.OrderEntity.GetObjValue("Entity.CREATEDATE"));
                    }
                });

            
        }

        protected override void OnLoadDataComplete()
        {
            
        }

        protected override void OnLoadControlComplete()
        {
            if (this.EditForm.OperationType != OperationTypes.Add)
            {
                ((SMT.FB.UI.Common.DataFieldItem)((SMT.FB.UI.FieldForm2)this.editForm.Controls[1]).RightForm.Items[2]).Requited = false;
            }
            InitData();
            //DetailGrid grid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            //if (grid != null)
            //{
            //    grid.Groups = new List<string>() { "ReferencedEntity[0].FBEntity.Entity.Name" };
            //}
            
        }

        private void GroupData()
        {
           // ObservableCollection<FBEntity> listDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETMODDETAIL).Name);

            DetailGrid grid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            if (grid != null)
            {
                grid.Groups = new List<string>() { "Entity.OwnerDepartmentName" };
            }
        }

        private void InitData()
        {
            this.OrderEntity.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Entity_PropertyChanged);
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                GetDetail();

                EmployeerData create = this.OrderEntity.GetCreateInfo();
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerID, create.Value);
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerName, create.Text);
                this.OrderEntity.SetObjValue(EntityFieldName.OwnerPostID, create.Post.Value);


                OrderEntity.SetObjValue("Entity.BUDGETYEAR", DataCore.SystemDateTime.Year);
            }
            else
            {
                UpdateData();
                dictComDetail.Clear();
                //ObservableCollection<FBEntity> detailList = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETAPPLYDETAIL).Name);
                ObservableCollection<FBEntity> detailList = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETMODDETAIL).Name);
                detailList.ToList().ForEach(item =>
                {
                    item.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(DeatilEntity_PropertyChanged);
                    T_FB_COMPANYBUDGETMODDETAIL ComDetail = (item.Entity as T_FB_COMPANYBUDGETMODDETAIL);
                    dictComDetail.Add(ComDetail.COMPANYBUDGETMODDETAILID, ComDetail.BUDGETMONEY);

                });
            }

        }

        void DeatilEntity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BUDGETMONEY")
            {
                T_FB_COMPANYBUDGETMODDETAIL ComDetail = sender as T_FB_COMPANYBUDGETMODDETAIL;
                decimal? budgetMoney = sender.GetObjValue(e.PropertyName) as decimal?;
                decimal? oldbudgetMoney = 0;
                if (dictComDetail.ContainsKey(ComDetail.COMPANYBUDGETMODDETAILID))
                {
                    oldbudgetMoney = dictComDetail[ComDetail.COMPANYBUDGETMODDETAILID];
                }

                decimal? SumbudgetMoney = this.OrderEntity.FBEntity.Entity.GetObjValue(e.PropertyName) as decimal?;
                if (SumbudgetMoney == null)
                {
                    SumbudgetMoney = 0;
                }
                SumbudgetMoney = SumbudgetMoney.Add(oldbudgetMoney * -1);
                this.OrderEntity.FBEntity.Entity.SetObjValue("BUDGETMONEY", SumbudgetMoney.Add(budgetMoney));

                dictComDetail[ComDetail.COMPANYBUDGETMODDETAILID] = budgetMoney;
            }


        }

        #region Update data when Init;
        protected void UpdateData()
        {
            //ObservableCollection<FBEntity> listDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETMODDETAIL).Name);
            
            //listDetail.ToList().ForEach ( item =>
            //    {
            //        string deptID = (item.Entity as T_FB_COMPANYBUDGETMODDETAIL).OWNERDEPARTMENTID;
            //        VirtualDepartment vd = this.curCompany.DepartmentCollection.FirstOrDefault(dept => {return dept.ID == deptID;});
            //        item.AddReferenceFBEntity<VirtualDepartment>(vd.ToFBEntity());
            //    });
            GroupData();
        }
        #endregion

        #region New Records when Init;
        void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OWNERDEPARTMENTID")
            {
                InitData();
                ChangeCreator();
            }
            
        }
        protected void GetDetail()
        {
            // 获取部门ID
            string deptID = Convert.ToString(OrderEntity.GetObjValue(EntityFieldName.OwnerDepartmentID));
            string companyID = Convert.ToString(OrderEntity.GetObjValue(EntityFieldName.OwnerCompanyID));
            curCompany = DataCore.CompanyList.FirstOrDefault(company => { return company.ID == companyID; });

            // 清除预算明细
            this.OrderEntity.CollectionEntity.ToList().ForEach(item => { item.FBEntities.Clear(); });


            QueryExpression qe = QueryExpressionHelper.Equal(FieldName.OwnerDepartmentID, deptID);
            qe.QueryType = typeof(T_FB_COMPANYBUDGETMODDETAIL).Name;
            fbService.QueryFBEntities(qe);

           
        }

        void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {


            var listDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETMODDETAIL).Name);
            listDetail.Clear();

            e.Result.ToList().ForEach(detail =>
            {

                T_FB_COMPANYBUDGETMODDETAIL deptDetail = detail.Entity as T_FB_COMPANYBUDGETMODDETAIL;
                deptDetail.T_FB_COMPANYBUDGETMODMASTER = this.OrderEntity.Entity as T_FB_COMPANYBUDGETMODMASTER;

                listDetail.Add(detail);
            });
            e.Result.ToList().ForEach(item =>
            {
                T_FB_COMPANYBUDGETMODDETAIL ComDetail = (item.Entity as T_FB_COMPANYBUDGETMODDETAIL);
                ComDetail.T_FB_COMPANYBUDGETMODMASTER = this.OrderEntity.Entity as T_FB_COMPANYBUDGETMODMASTER;
                ComDetail.CREATEDATE = DateTime.Now;
                ComDetail.CREATEUSERID = this.OrderEntity.LoginUser.Value.ToString();
                ComDetail.UPDATEDATE = DateTime.Now;
                ComDetail.UPDATEUSERID = this.OrderEntity.LoginUser.Value.ToString(); ;
                item.FBEntityState = FBEntityState.Added;
                item.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(DeatilEntity_PropertyChanged);
                dictComDetail.Add(ComDetail.COMPANYBUDGETMODDETAILID, ComDetail.BUDGETMONEY);

            });
            GroupData();
        }
        #endregion

        public class GroupSubject : FBEntity
        {
            FBEntity innerFBEntity;
            public GroupSubject(FBEntity fbEntity)
            {
                innerFBEntity = fbEntity;
                this.Entity = innerFBEntity.Entity;
                this.CollectionEntity = innerFBEntity.CollectionEntity;
                this.ReferencedEntity = innerFBEntity.ReferencedEntity;

                VirtualDepartment vd = innerFBEntity.ReferencedEntity[0].FBEntity.Entity as VirtualDepartment;
                this.DepartmentName = vd.Name;
            }
            
            public string DepartmentName { get; set; }
        }

        //public void InitData()
        //{
        //    //ObservableCollection<FBEntity> list = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_COMPANYBUDGETMODDETAIL).Name);

        //    //list.
        //}
    }
}
