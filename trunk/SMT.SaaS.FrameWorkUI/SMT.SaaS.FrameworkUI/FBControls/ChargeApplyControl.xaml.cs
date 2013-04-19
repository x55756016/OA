/*
 *  0. 在初始化数据之前，需要对 Order 赋值
 *  public T_FB_EXTENSIONALORDER Order { get; set; }
 *  
 *  1. 初始化数据
 *  public void InitData(bool readOnly)
 *  1.5 初始化数据后，返回原结果 是一个string 集合。如果为空，就是操作成功，否则就是出错的信息。
 *  public event EventHandler<InitDataCompletedArgs> InitDataComplete;
 *  ...
 *  2. 保存 stataes : 草稿，提交，审核通过，审核不通过
 *  public void Save(CheckStates statates)
 *  2.5 保存后，返回的结果 是一个string 集合。如果为空，就是操作成功，否则就是出错的信息。
 *  public event EventHandler<SaveCompletedArgs> SaveCompleted;
 *  
 */
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

using SMT.Saas.Tools.FBServiceWS;
using System.Windows.Data;
using System.Collections;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Browser;
using System.Collections.Specialized;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.Globalization;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.FrameworkUI.FBControls
{
    public partial class ChargeApplyControl : UserControl
    {
        FBServiceClient fbService = null;
        SelectedForm selectForm = null;

        public SelectedForm SelectForm
        {
            get { return selectForm; }
            set { selectForm = value; }
        }
        FBServiceClient FBServiceLoacal
        {
            get
            {
                if (fbService == null)
                {
                    bool registerResult = WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
                    bool httpsResult = WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);

                    fbService = new FBServiceClient();
                    fbService.SaveEntityCompleted += new EventHandler<SaveEntityCompletedEventArgs>(fbService_SaveEntityCompleted);
                    fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);
                }
                return fbService;
            }
        }


        public Grid GetPayType
        {
            get
            {
                return this.PayTypeinfo;
            }
            set
            {
                this.PayTypeinfo = value;
            }
        }

        #region Extensional Method

        /// <summary>
        /// 根据 entityType , 返回对应的有关系的 FBEntity的集合, 
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <param name="entityType">有关系的对象集合类型</param>
        /// <returns>如果不存在,将会创建相应的集合</returns>
        private ObservableCollection<FBEntity> GetRelationFBEntities(FBEntity fbEntity, string entityType)
        {
            ObservableCollection<FBEntity> listFBEntities = null;

            RelationManyEntity rmEntity = fbEntity.CollectionEntity.FirstOrDefault(item =>
            {
                return item.EntityType == entityType;
            });

            if (rmEntity == null)
            {
                rmEntity = new RelationManyEntity();
                rmEntity.EntityType = entityType;
                rmEntity.FBEntities = new ObservableCollection<FBEntity>();
                fbEntity.CollectionEntity.Add(rmEntity);
            }
            listFBEntities = rmEntity.FBEntities;
            return listFBEntities;
        }

        private FBEntity ToFBEntity(EntityObject t)
        {
            FBEntity fbEntity = new FBEntity();
            fbEntity.Entity = t;
            fbEntity.CollectionEntity = new ObservableCollection<RelationManyEntity>();
            fbEntity.ReferencedEntity = new ObservableCollection<RelationOneEntity>();
            return fbEntity;
        }


        private FBEntity GetModifiedFBEntity(FBEntity fbEntity)
        {
            FBEntity newEntity = ToFBEntity(fbEntity.Entity);
            newEntity.FBEntityState = fbEntity.FBEntityState;

            foreach (RelationManyEntity rme in fbEntity.CollectionEntity)
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
                            this.CurrentOrderEntity.CopyTo(item, "Entity.CREATEUSERID");
                        }
                        this.CurrentOrderEntity.CopyTo(item, "Entity.UPDATEUSERID");

                        rmeNew.FBEntities.Add(item);
                    }
                });

                newEntity.CollectionEntity.Add(rmeNew);
            }

            if (DeleteList.Count > 0)
            {
                RelationManyEntity rmeDel = new RelationManyEntity();
                rmeDel.EntityType = "DELETE_ENTITYTYPE";
                rmeDel.FBEntities = DeleteList;
                newEntity.CollectionEntity.Add(rmeDel);
            }
            return newEntity;
        }


        #endregion

        private bool isInit = true;

        private SelectedDataManager selectedDataManager;
        public SelectedDataManager SelectedDataManager
        {
            get
            {
                if (selectedDataManager == null)
                {
                    selectedDataManager = new SelectedDataManager();
                }
                return selectedDataManager;
            }
        }

        private FBDataGrid AGrid;
        private FBDataGrid BorrowGrid;
        private FBEntity CurrentOrderEntity { get; set; }
        private T_FB_CHARGEAPPLYMASTER master { get; set; }
        public ObservableCollection<FBEntity> ListDetail { get; set; }
        public ObservableCollection<FBEntity> ListExtOrderType { get; set; }

        public ObservableCollection<T_FB_CHARGEAPPLYREPAYDETAIL> ListBorrowDetail { get; set; }

        public ChargeApplyControl()
        {
            try
            {
                InitializeComponent();
                Order = new T_FB_EXTENSIONALORDER();
                Order.CHECKSTATES = 0;
                GetPayType.Opacity = 0;
                //InitAllExtOrderType();
                InitApplyType();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region 接口需求

        public void SetRemarkVisiblity(System.Windows.Visibility visibility)
        {
            this.gridRemark.Visibility = visibility;
        }
        
        public void SetApplyTypeVisiblity(System.Windows.Visibility visibility)
        {
            this.spApplyType.Visibility = visibility;
        }

        public ChargeParameter GetParamenter()
        {
            return new ChargeParameter();
        }

        /// <summary>
        /// 扩展表单对象
        ///	单据ID	ORDERID
        ///	单据编号	ORDERCODE
        ///	申请人	OWNERID
        ///	申请人岗位	OWNERPOSTID
        ///	申请人部门	OWNERDEPARTMENTID
        ///	申请人公司	OWNERCOMPANYID
        ///	创建人	CREATEUSERID
        ///	创建时间	CREATEDATE
        ///	公司ID	CREATECOMPANYID
        ///	部门ID	CREATEDEPARTMENTID
        ///	岗位ID	CREATEPOSTID
        ///	修改人	UPDATEUSERID
        ///	修改时间	UPDATEDATE
        ///	创建人名称	CREATEUSERNAME
        ///	修改人名称	UPDATEUSERNAME
        ///	申请人名称	OWNERNAME
        ///	部门名称	CREATEDEPARTMENTNAME
        ///	公司名称	CREATECOMPANYNAME
        ///	岗位名称	CREATEPOSTNAME
        ///	申请人部门名称	OWNERDEPARTMENTNAME
        ///	申请人公司名称	OWNERCOMPANYNAME
        ///	申请人岗位名称	OWNERPOSTNAME
        /// </summary>
        /// 
        public T_FB_EXTENSIONALORDER Order { get; set; }

        public ExtensionalOrderTypes ExtOrderType { get; set; }

        public string strExtOrderModelCode = string.Empty;
        public FormTypes submitFBFormTypes { get; set; }

        public enum ExtensionalOrderTypes
        {
            TravelApplication,
            TravelReimbursement,
            ApprovalForm
        }

        /// <summary>
        /// 初始化数据绑定
        /// </summary>
        public void InitData()
        {
            InitData(false);
        }

        public void InitData(bool readOnly)
        {
            
            deatilGridBar.Visibility = readOnly ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            
            selectedDataManager = null;

            if (ListDetail != null)
            {
                this.ListDetail.Clear();
            }
            if (this.ListDetail != null)
            {
                this.DeleteList.Clear();
            }

            QueryExpression qe = GetOrderQueryExp();
            FBServiceLoacal.QueryFBEntitiesAsync(qe);
            SelectedDataManager.QueryExpression = GetQueryDetail();
        }

        private QueryExpression GetOrderQueryExp()
        {
            string orderID = Order.ORDERID;
            QueryExpression qe = GetQE("ORDERID", orderID);


            qe.RelatedExpression = GetQueryE();

            if (this.TravelSubject != null)
            {
                var tempQe = qe;
                qe = GetQE("TravelSubject", "1");
                qe.RelatedExpression = tempQe;
            }

            QueryExpression qeExtType = GetExtOrderType();
            qeExtType.RelatedExpression = qe;
            qe = qeExtType;

            qe.QueryType = typeof(T_FB_EXTENSIONALORDER).Name;           

            return qe;
        }

        /// <summary>
        /// 对应上预算中的单据类型： 1.费用报销， 2：借款。
        /// 如果是Both，那么类型由用户操作时，决定。
        /// </summary>
        public enum ApplyTypes
        {
            Both,
            ChargeApply,
            BorrowApply

        }

        /// <summary>
        /// 用于出差申请，如果是出差申请，选Travel， 其他的选Charge
        /// 区别在于：可选科目中，Tranvel类型的可以选择差旅费的科目，Charge不可以选择。
        /// </summary>
        public enum OrderTypes
        {
            Travel,
            Charge
        }

        public ApplyTypes _ApplyType = ApplyTypes.Both;
        public ApplyTypes ApplyType
        {
            get
            {
                return _ApplyType;
            }
            set
            {
                _ApplyType = value;
            }


        }

        public OrderTypes _OrderType = OrderTypes.Charge;
        public OrderTypes OrderType
        {
            get
            {
                return _OrderType;
            }
            set
            {
                _OrderType = value;
            }


        }

        /// <summary>
        /// 如果是出差申请,可以对该对象赋值.注:需要在调用InitData方法前赋值
        /// e.g
        ///   c.TravelSubject  = new TravelSubject();
        ///   c.InitData()
        ///   ......
        ///   
        ///   c.ApplyMoney = 400
        ///   c.Save()
        /// </summary>
        public TravelSubject TravelSubject { get; set; }

        private void InitApplyType()
        {
            if (ApplyType == ApplyTypes.BorrowApply)
            {
                this.rbtnApplyTypeCharge.Visibility = System.Windows.Visibility.Collapsed;

            }
            else if (ApplyType == ApplyTypes.ChargeApply)
            {
                this.rbtnApplyTypeBorrow.Visibility = System.Windows.Visibility.Collapsed;

            }
            rbtnChargePerson_Click(null, null);
            rbtnApplyTypeCharge_Click(null, null);
        }


        /// <summary>
        /// 保存预算申请单
        /// </summary>
        /// <param name="statates">当前表单状态</param>
        public void Save(CheckStates statates)
        {
            List<string> listMsg = this.OnSave();

            if (listMsg.Count > 0)
            {
                OnSaveCompleted(listMsg);
                return;
            }
            this.GetValue();

            
            if (statates == CheckStates.Delete)
            {
                CurrentOrderEntity.FBEntityState = FBEntityState.Detached;
            }
            // else if如果是重新提交，如果当前单据是审核不通过的状态才可以重新提交，否则提示异常。
            // 需处理　this.Order.CHECKSTATES 为提交状态，并this.CurrentOrderEntity.FBEntityState 为重新提交
            else
            {
                if (submitFBFormTypes == FormTypes.Audit && statates == CheckStates.Approving)
                {
                    return;
                }


                this.Order.CHECKSTATES = Convert.ToDecimal((int)statates);

                if (submitFBFormTypes == FormTypes.Resubmit)
                {
                    this.CurrentOrderEntity.FBEntityState = FBEntityState.ReSubmit;

                }
                else
                {
                    if (this.CurrentOrderEntity.FBEntityState == FBEntityState.Unchanged)
                    {
                        this.CurrentOrderEntity.FBEntityState = FBEntityState.Modified;
                    }
                }

            }

            //if (this.Order != null && ExtOrderType != null)
            //{
            //    if (this.Order.T_FB_EXTENSIONALTYPE == null)
            //    {
            //        switch (ExtOrderType)
            //        {
            //            case ExtensionalOrderTypes.ApprovalForm:
            //                this.Order.T_FB_EXTENSIONALTYPE = GetExtOrderType("SXSP");
            //                break;
            //            case ExtensionalOrderTypes.TravelApplication:
            //                this.Order.T_FB_EXTENSIONALTYPE = GetExtOrderType("CCSQ");
            //                break;
            //            case ExtensionalOrderTypes.TravelReimbursement:
            //                this.Order.T_FB_EXTENSIONALTYPE = GetExtOrderType("CCPX");
            //                break;
            //        }
            //    }
            //}

            QueryExpression qeSave = this.GetOrderQueryExp();
            SaveEntity se = new SaveEntity();
            se.QueryExpression = qeSave;
            se.FBEntity = CurrentOrderEntity;
            FBServiceLoacal.SaveEntityAsync(se);
        }

        private QueryExpression GetExtOrderType()
        {
            QueryExpression qeRes = null;
            if (string.IsNullOrWhiteSpace(strExtOrderModelCode))
            {
                return qeRes;
            }

            qeRes = new QueryExpression();
            qeRes.Operation = QueryExpression.Operations.Equal;
            qeRes.PropertyName = "EXTENSIONALTYPECODE";
            qeRes.PropertyValue = strExtOrderModelCode;
            qeRes.QueryType = typeof(T_FB_EXTENSIONALTYPE).Name;            

            return qeRes;
        }

        string msgBiggerError = Utility.GetResourceStr("DATEGREATERERROR");
        private List<string> OnSave()
        {
            List<string> listReslut = new List<string>();
            if (this.ListDetail != null)
            {
                this.ListDetail.ForEach(item =>
                {
                    T_FB_EXTENSIONORDERDETAIL detailEntity = item.Entity as T_FB_EXTENSIONORDERDETAIL;

                    if (detailEntity.APPLIEDMONEY > detailEntity.USABLEMONEY.Value)
                    {
                        listReslut.Add(string.Format("科目:{2} 的" + msgBiggerError, "申请金额", "可用金额", detailEntity.T_FB_SUBJECT.SUBJECTNAME));

                    }
                    if (detailEntity.APPLIEDMONEY <= 0)
                    {
                        string errorMessage = string.Format("科目： {0} 的申请金额必须大于零!", detailEntity.T_FB_SUBJECT.SUBJECTNAME);
                        listReslut.Add(errorMessage);

                    }
                    if (string.IsNullOrWhiteSpace(detailEntity.REMARK))
                    {
                        string errorMessage = string.Format("科目： {0} 的摘要不能为空!", detailEntity.T_FB_SUBJECT.SUBJECTNAME);
                        listReslut.Add(errorMessage);
                    }

                });
            }

            var items = from item in ListDetail
                        group item by new
                        {

                            (item.Entity as T_FB_EXTENSIONORDERDETAIL).T_FB_SUBJECT.SUBJECTNAME,
                            (item.Entity as T_FB_EXTENSIONORDERDETAIL).CHARGETYPE,
                            (item.Entity as T_FB_EXTENSIONORDERDETAIL).USABLEMONEY
                        } into g
                        where g.Sum(item => (item.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY) > g.Key.USABLEMONEY
                        select new { g.Key, totalCharge = g.Sum(item => (item.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY) };

            if (this.TravelSubject != null)
            {
                if (this.TravelSubject.SpecialListDetail.Count == 0)
                {
                    listReslut.Add("当前申请人没有出差的预算额度");
                }
                else if (this.TravelSubject.ApplyMoney > this.TravelSubject.UsableMoney)
                {
                    listReslut.Add(string.Format("科目:{2} 的" + msgBiggerError, "申请金额", "可用金额", TravelSubject.subject.SUBJECTNAME));
                }

            }

            foreach (var item in items)
            {
                listReslut.Add(string.Format("科目:{2} 的" + msgBiggerError, "申请金额", "可用金额", item.Key.SUBJECTNAME));
            }

            bool isbError = false;
            
                if (ListBorrowDetail != null)
                {
                    this.ListBorrowDetail.ForEach(item =>
                        {
                            if (isbError)
                            {
                                return;
                            }
                            var dItem = this.BorrowGrid.GridItems[0].ReferenceTypes.FirstOrDefault(itemFind => itemFind.DICTIONARYVALUE == item.REPAYTYPE);
                            string typeName = "{0}-的";
                            if (dItem != null)
                            {
                                typeName = string.Format(typeName, dItem.DICTIONARYNAME);
                            }
                            else
                            {
                                typeName = "";
                            }

                            if (item.BORROWMONEY < item.REPAYMONEY)
                            {

                                listReslut.Add(string.Format("冲借款金额不能大于借款余额或报销金额"));
                                isbError = true;
                            }

                            if (item.REPAYMONEY > 0 && string.IsNullOrWhiteSpace(item.REMARK))
                            {
                                listReslut.Add("冲借款明细的摘要不能为空");
                                isbError = true;
                            }
                        });


                    var bcount = this.ListBorrowDetail.Sum(item =>
                        {
                            return item.REPAYMONEY;
                        });
                    if (bcount > Order.TOTALMONEY.Value)
                    {
                        listReslut.Add("冲借款金额不能大于借款余额或报销金额");
                    }
                    if (this.rbtnChargeBorrow.IsChecked == true)
                    {
                        if (bcount == 0)
                        {
                            listReslut.Add("冲借款时明细不能为空!");
                        }
                    }
                
            }


            return listReslut;
        }
        public event EventHandler<SaveCompletedArgs> SaveCompleted;

        public event EventHandler<InitDataCompletedArgs> InitDataComplete;
        public class SaveCompletedArgs : EventArgs
        {
            private List<string> _Message;
            public List<string> Message
            {
                get
                {
                    return _Message;
                }
            }
            public SaveCompletedArgs(List<string> messages)
            {
                _Message = messages;
            }
        }

        public class InitDataCompletedArgs : SaveCompletedArgs
        {
            public InitDataCompletedArgs(List<string> messages)
                : base(messages)
            {
            }
        }
        #endregion

        #region 初始化数据

        private void InitControl()
        {
            if (isInit)
            {
                this.PayP.Visibility = Visibility.Visible;
                this.ToolBarP.Visibility = Visibility.Visible;
                AGrid = new FBDataGrid();
                this.GridP.Children.Add(AGrid);
                AGrid.GridItems = GetItems();

                AGrid.InitControl(this.GetDelColumn());
                
                AGrid.MinHeight = 80;
                                                
                BorrowGrid = new FBDataGrid();
                this.spForBorrowInfo.Children.Add(BorrowGrid);
                BorrowGrid.GridItems = GetBorrowItems();
                BorrowGrid.InitControl();
                
                BorrowGrid.MinHeight = 60;

                InitToolBar();
                
            }
            AGrid.ItemsSource = this.ListDetail;
            BorrowGrid.ItemsSource = this.ListBorrowDetail;
            SetValue();
            isInit = false;
            
        }

        private DataGridColumn GetDelColumn()
        {
            DataGridTemplateColumn dgtc = new DataGridTemplateColumn();
            dgtc.CellTemplate = this.Resources["dtDelColumn"] as DataTemplate;
            return dgtc;
        }


        public List<FBDataGrid.FBGridItem> GetItems()
        {
            string xmlDoc = @"<?xml version=""1.0"" encoding=""utf-8"" ?><GridItems>
                <GridItem PropertyDisplayName=""科目名称"" PropertyName=""Entity.T_FB_SUBJECT.SUBJECTNAME"" Width=""150"" IsReadOnly=""true""/>
                <GridItem PropertyDisplayName=""可用结余"" PropertyName=""Entity.USABLEMONEY"" Width=""75"" IsReadOnly=""true""/>
                <GridItem PropertyDisplayName=""摘要"" PropertyName=""Entity.REMARK"" Width=""175"" MaxLength=""200""/>
                <GridItem PropertyDisplayName=""报销金额"" PropertyName=""Entity.APPLIEDMONEY"" Width=""75""/>

            </GridItems>";
            XElement xml = XElement.Parse(xmlDoc);

            XElement xElement = xml;
            List<FBDataGrid.FBGridItem> list = new List<FBDataGrid.FBGridItem>();
            foreach (XElement xeItem in xElement.Elements("GridItem"))
            {
                FBDataGrid.FBGridItem gridItem = new FBDataGrid.FBGridItem();
                Type type = typeof(FBDataGrid.FBGridItem);
                xeItem.Attributes().ForEach(item =>
                {
                    PropertyInfo p = type.GetProperty(item.Name.LocalName);
                    if (p != null)
                    {
                        object v = item.Value.ConvertOrNull(p.PropertyType, null, null, DateTimeStyles.None, null);
                        p.SetValue(gridItem, v, null);
                    }

                });
                list.Add(gridItem);
            }
            return list;

        }


        public List<FBDataGrid.FBGridItem> GetBorrowItems()
        {
            string xmlDoc = @"<?xml version=""1.0"" encoding=""utf-8"" ?><GridItems>
                <GridItem PropertyDisplayName=""借款项目"" PropertyName=""REPAYTYPE"" Width=""100"" IsReadOnly=""true"" ReferenceType=""RepayType""/>
                <GridItem PropertyDisplayName=""借款余额"" PropertyName=""BORROWMONEY"" Width=""75"" IsReadOnly=""true"" />
                <GridItem PropertyDisplayName=""摘要"" PropertyName=""REMARK"" Width=""200"" IsReadOnly=""flase""/>
                <GridItem PropertyDisplayName=""还款金额"" PropertyName=""REPAYMONEY"" Width=""75"" IsReadOnly=""flase""/>
            </GridItems>";
            XElement xml = XElement.Parse(xmlDoc);

            XElement xElement = xml;
            List<FBDataGrid.FBGridItem> list = new List<FBDataGrid.FBGridItem>();
            foreach (XElement xeItem in xElement.Elements("GridItem"))
            {
                FBDataGrid.FBGridItem gridItem = new FBDataGrid.FBGridItem();
                Type type = typeof(FBDataGrid.FBGridItem);
                xeItem.Attributes().ForEach(item =>
                {
                    PropertyInfo p = type.GetProperty(item.Name.LocalName);
                    if (p != null)
                    {
                        object v = item.Value.ConvertOrNull(p.PropertyType, null, null, DateTimeStyles.None, null);
                        p.SetValue(gridItem, v, null);
                    }

                });
                list.Add(gridItem);
            }
            return list;

        }
        private void InitToolBar()
        {
            List<ToolbarItem> listBar = new List<ToolbarItem>();
            ToolbarItem tbItem = ToolBarItems.New;
            tbItem.Title = "选择科目";
            listBar.Add(tbItem);
            // listBar.Add(ToolBarItems.Delete);
            deatilGridBar.InitToolBarItem(listBar);
            deatilGridBar.ShowItem(deatilGridBar.TitleImageName, false);
            deatilGridBar.ShowItem(deatilGridBar.HelpButtonName, false);
            deatilGridBar.ItemClicked += new EventHandler<ToolBar.ToolBarItemClickArgs>(deatilGridBar_ItemClicked);
        }


        #endregion

        #region 保存数据
        private void OnSaveCompleted(List<string> list)
        {
            if (SaveCompleted != null)
            {

                SaveCompletedArgs args = new SaveCompletedArgs(null);
                if (list.Count > 0)
                {

                    args = new SaveCompletedArgs(list);
                }

                SaveCompleted(this, args);
            }
        }

        void fbService_SaveEntityCompleted(object sender, SaveEntityCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result.FBEntity != null)
                {
                    OnQueryCompleted(e.Result.FBEntity);
                }
                List<string> listResult = new List<string>();
                if (!string.IsNullOrEmpty(e.Result.Exception))
                {
                    listResult.Add(e.Result.Exception);
                }
                OnSaveCompleted(listResult);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "系统错误，请联系管理员!");                
            }
        }

        /// <summary>
        /// 查询数据后对控件赋值
        /// </summary>
        private void SetValue()
        {
            T_FB_EXTENSIONALORDER eOrder = this.CurrentOrderEntity.Entity as T_FB_EXTENSIONALORDER;



            if (ApplyType == ApplyTypes.BorrowApply)
            {
                eOrder.APPLYTYPE = 2;
            }
            else if (ApplyType == ApplyTypes.ChargeApply)
            {
                eOrder.APPLYTYPE = 1;
            }

            if (master != null)
            {
                this.rbtnChargeBorrow.IsChecked = (master != null && master.PAYTYPE == decimal.Parse("2"));
                this.rbtnChargePerson.IsChecked = (master != null && master.PAYTYPE == decimal.Parse("1"));
            }
            this.rbtnApplyTypeBorrow.IsChecked = (eOrder.APPLYTYPE != null && eOrder.APPLYTYPE.Value == decimal.Parse("2"));
            this.rbtnApplyTypeCharge.IsChecked = (eOrder.APPLYTYPE != null && eOrder.APPLYTYPE.Value == decimal.Parse("1"));

            this.tbRemark.Text = string.IsNullOrEmpty(eOrder.REMARK) ? "" : eOrder.REMARK;

            rbtnChargePerson_Click(null, null);
            rbtnApplyTypeCharge_Click(null, null);

            // this.tbCount.SetBinding(TextBox.TextProperty, new Binding() { Path = new PropertyPath("TOTALMONEY"), Source = this.Order });
            SetLittleCount();

        }

        /// <summary>
        /// 保存数据时用
        /// </summary>
        private void GetValue()
        {
            T_FB_EXTENSIONALORDER eOrder = this.CurrentOrderEntity.Entity as T_FB_EXTENSIONALORDER;
            if (this.rbtnApplyTypeBorrow.IsChecked == true)
            {
                eOrder.APPLYTYPE = 2;
            }
            else
            {
                eOrder.APPLYTYPE = 1;
            }
            if (this.rbtnChargePerson.IsChecked == true)
            {
                master.PAYTYPE = 1;
            }
            else
            {
                master.PAYTYPE = 2;
            }

            eOrder.REMARK = this.tbRemark.Text;

            if (this.TravelSubject != null)
            {
                //var sd = this.TravelSubject.SpecialListDetail.FirstOrDefault();
                //(sd.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY = this.TravelSubject.ApplyMoney;

                TravelSubject.SpecialListDetail.ForEach(item =>
                {
                    item.Entity.SetObjValue("CREATEUSERID", eOrder.GetObjValue("CREATEUSERID"));
                    item.Entity.SetObjValue("CREATEDATE", System.DateTime.Now);

                    item.Entity.SetObjValue("UPDATEUSERID", eOrder.GetObjValue("UPDATEUSERID"));
                    item.Entity.SetObjValue("UPDATEDATE", System.DateTime.Now);

                    #region 去掉无关的关联
                    T_FB_EXTENSIONORDERDETAIL detail = item.Entity as T_FB_EXTENSIONORDERDETAIL;
                    detail.T_FB_SUBJECT.T_FB_EXTENSIONORDERDETAIL.Clear();
                    detail.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                    #endregion

                });
            }
            ListDetail.ForEach(item =>
            {
                item.Entity.SetObjValue("CREATEUSERID", eOrder.GetObjValue("CREATEUSERID"));
                item.Entity.SetObjValue("CREATEDATE", System.DateTime.Now);

                item.Entity.SetObjValue("UPDATEUSERID", eOrder.GetObjValue("UPDATEUSERID"));
                item.Entity.SetObjValue("UPDATEDATE", System.DateTime.Now);

                #region 去掉无关的关联
                T_FB_EXTENSIONORDERDETAIL detail = item.Entity as T_FB_EXTENSIONORDERDETAIL;
                detail.T_FB_SUBJECT.T_FB_EXTENSIONORDERDETAIL.Clear();
                detail.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                #endregion

            });

            if (this.gridForBorrowInfo.Visibility == System.Windows.Visibility.Collapsed)
            {
                var list = GetRelationFBEntities(this.CurrentOrderEntity, typeof(T_FB_CHARGEAPPLYMASTER).Name);
                list.Clear();
            }
        }
        #endregion

        #region 查询数据

        private void InitAllExtOrderType()
        {
            if (fbService == null)
            {
                fbService = new FBServiceClient(); 
            }
            QueryExpression qe = new QueryExpression();
            qe.QueryType = typeof(T_FB_EXTENSIONALTYPE).Name;
            fbService.GetFBEntitiesAsync(qe);
            fbService.GetFBEntitiesCompleted += new EventHandler<GetFBEntitiesCompletedEventArgs>(fbService_GetFBEntitiesCompleted);

        }

        /// <summary>
        /// 刷新当前可用预算科目
        /// </summary>
        public void RefreshData()
        {
            SelectedDataManager.QueryExpression = GetQueryDetail();
            this.Clear();
        }

        private QueryExpression GetQueryDetail()
        {
            var qeOwnerID = GetQueryE();
            // 查当前用户可用预算科目的条件
            qeOwnerID.QueryType = typeof(T_FB_EXTENSIONORDERDETAIL).Name;
            qeOwnerID.Include = new ObservableCollection<string>() { typeof(T_FB_SUBJECT).Name };
            return qeOwnerID;
        }
        private QueryExpression GetQueryE()
        {
            string ownerID = Order.OWNERID;
            string postID = Order.OWNERPOSTID;
            string deptID = Order.OWNERDEPARTMENTID;
            string companyID = Order.OWNERCOMPANYID;


            QueryExpression qePost = GetQE("OWNERPOSTID", postID);
            QueryExpression qeDept = GetQE("OWNERDEPARTMENTID", deptID);
            QueryExpression qeOwnerID = GetQE("OWNERID", ownerID);
            QueryExpression qeCom = GetQE("OWNERCOMPANYID", companyID);

            qeOwnerID.RelatedExpression = qePost;
            qePost.RelatedExpression = qeDept;
            qeDept.RelatedExpression = qeCom;

            if (this.OrderType == OrderTypes.Travel)
            {
                var tempQe = qeOwnerID;
                qeOwnerID = GetQE("OrderTypes", "Travel");
                qeOwnerID.RelatedExpression = tempQe;
            }
            return qeOwnerID;
        }
        private QueryExpression GetQE(string propertyName, string propertyValue)
        {
            QueryExpression qe = new QueryExpression();
            qe.Operation = QueryExpression.Operations.Equal;
            qe.PropertyName = propertyName;
            qe.PropertyValue = propertyValue;
            return qe;
        }

        void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

                if (e.Result.Count() == 0)
                {
                    return;
                }

                FBEntity queryEntity = e.Result.FirstOrDefault();
                OnQueryCompleted(queryEntity);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), e.Error.Message.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            
        }

        private void OnQueryCompleted(FBEntity queryEntity)
        {
            if (queryEntity == null) // 新增
            {
                this.Order.EXTENSIONALORDERID = Guid.NewGuid().ToString();
                this.CurrentOrderEntity = ToFBEntity(this.Order);
                this.CurrentOrderEntity.FBEntityState = FBEntityState.Added;

                this.Order.APPLYTYPE = 1;
                this.Order.PAYTARGET = 1;

            }
            else
            {
                this.CurrentOrderEntity = queryEntity;
                var curEntity = this.CurrentOrderEntity.Entity as T_FB_EXTENSIONALORDER;
                if (this.CurrentOrderEntity.FBEntityState == FBEntityState.Added)
                {
                    curEntity.CREATECOMPANYID = this.Order.CREATECOMPANYID;
                    curEntity.CREATECOMPANYNAME = this.Order.CREATECOMPANYNAME;
                    curEntity.CREATEDEPARTMENTID = this.Order.CREATEDEPARTMENTID;
                    curEntity.CREATEDEPARTMENTNAME = this.Order.CREATEDEPARTMENTNAME;
                    curEntity.CREATEPOSTID = this.Order.CREATEPOSTID;
                    curEntity.CREATEPOSTNAME = this.Order.CREATEPOSTNAME;
                    curEntity.CREATEUSERID = this.Order.CREATEUSERID;
                    curEntity.CREATEUSERNAME = this.Order.CREATEUSERNAME;


                    curEntity.OWNERCOMPANYID = this.Order.OWNERCOMPANYID;
                    curEntity.OWNERCOMPANYNAME = this.Order.OWNERCOMPANYNAME;
                    curEntity.OWNERDEPARTMENTID = this.Order.OWNERDEPARTMENTID;
                    curEntity.OWNERDEPARTMENTNAME = this.Order.OWNERDEPARTMENTNAME;
                    curEntity.OWNERPOSTID = this.Order.OWNERPOSTID;
                    curEntity.OWNERPOSTNAME = this.Order.OWNERPOSTNAME;
                    curEntity.OWNERID = this.Order.OWNERID;
                    curEntity.OWNERNAME = this.Order.OWNERNAME;

                    curEntity.UPDATEUSERID = this.Order.UPDATEUSERID;
                    curEntity.UPDATEUSERNAME = this.Order.UPDATEUSERNAME;


                }
                this.Order = curEntity;
            }

            this.DeleteList = GetRelationFBEntities(this.CurrentOrderEntity, "_Delete");
            this.ListExtOrderType = GetRelationFBEntities(this.CurrentOrderEntity, typeof(T_FB_EXTENSIONALTYPE).Name);
            this.ListDetail = GetRelationFBEntities(this.CurrentOrderEntity, typeof(T_FB_EXTENSIONORDERDETAIL).Name);
            var masterList = GetRelationFBEntities(this.CurrentOrderEntity, typeof(T_FB_CHARGEAPPLYMASTER).Name);
            var masterFBEntity = masterList.FirstOrDefault();
            if (masterFBEntity != null)
            {
                master = masterFBEntity.Entity as T_FB_CHARGEAPPLYMASTER;
                this.ListBorrowDetail = master.T_FB_CHARGEAPPLYREPAYDETAIL;

            }
            else
            {
                master = new T_FB_CHARGEAPPLYMASTER();
            }

            HandleSpecialSubject();
            this.CountTotalMoney();
            this.ListDetail.CollectionChanged += new NotifyCollectionChangedEventHandler(FBEntities_CollectionChanged);
            this.ListDetail.ForEach(detail =>
            {
                this.RegisterFBEntity(detail);
            });

            if (ListExtOrderType.Count() > 0)
            {
                (this.CurrentOrderEntity.Entity as T_FB_EXTENSIONALORDER).T_FB_EXTENSIONALTYPE = ListExtOrderType.FirstOrDefault().Entity as T_FB_EXTENSIONALTYPE;
            }

            InitControl();

            if (InitDataComplete != null)
            {
                List<string> messages = new List<string>();
                if (this.TravelSubject != null && this.TravelSubject.SpecialListDetail.Count == 0)
                {
                    messages.Add("当前申请人没有出差的预算额度");
                }
                InitDataComplete(this, new InitDataCompletedArgs(messages));
            }
        }

        void fbService_GetFBEntitiesCompleted(object sender, GetFBEntitiesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ListExtOrderType = e.Result;
            }
        }

        #endregion

        #region 明细 工具栏操作
        void deatilGridBar_ItemClicked(object sender, ToolBar.ToolBarItemClickArgs e)
        {
            switch (e.Key)
            {
                case "New":
                    Add();
                    break;
                case "Delete":
                    IList list = this.AGrid.SelectedItems as IList;
                    Delete(list);
                    break;
            }
        }

        public void Add()
        {
            SelectedForm sf = new SelectedForm();
            sf.SelectedDataManager = this.SelectedDataManager;
            sf.SelectedItems = this.ListDetail.ToList();
            bool isBigger5 = false;
            sf.SelectedCompleted += (o, e) =>
            {
                
                sf.SelectedItems.ForEach(item =>
                {
                    if (ListDetail.Count >= 5)
                    {
                        isBigger5 = true;
                        return;
                    }
                    item.FBEntityState = FBEntityState.Added;
                    (item.Entity as T_FB_EXTENSIONORDERDETAIL).T_FB_EXTENSIONALORDER = this.CurrentOrderEntity.Entity as T_FB_EXTENSIONALORDER;
                    this.ListDetail.Add(item);
                });
                AGrid.ItemsSource = this.ListDetail;

                if (isBigger5)
                {
                    MessageWindow.Show<string>("提示", "报销明细项不能超过5条!", MessageIcon.Information,
                    null, "Default", Localization.GetString("OKBUTTON"));
                }

            };

            sf.Show();
            SelectForm = sf;

           
        }

        public void Delete(IList list)
        {
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                FBEntity entity = list[i] as FBEntity;
                if (entity != null)
                {
                    entity.Actived = false;
                    ListDetail.Remove(entity);
                }
            }
        }

        //xiedx
        //2012-8-11
        //这个是在出差申请中用到的函数，但是值ListDetail一直是null，所以会出错，现在判断一下
        public void Clear()
        {
            if (this.ListDetail != null)
            {
                this.ListDetail.Clear();
            }   
        }
        #endregion

        #region 数据对象，属性值改变处理
        public ObservableCollection<FBEntity> DeleteList = new ObservableCollection<FBEntity>();
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
                        if (!DeleteList.Contains(fbEntity))
                        {
                            fbEntity.FBEntityState = FBEntityState.Detached;
                            DeleteList.Add(fbEntity);
                        }
                    }
                    UnRegisterFBEntity(fbEntity);
                }
                CountTotalMoney();
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    FBEntity fbEntity = e.NewItems[i] as FBEntity;
                    RegisterFBEntity(fbEntity);
                }
            }
            this.OnEntityPropertyChanged(this.CurrentOrderEntity);
        }

        private void RegisterFBEntity(FBEntity fbEntity)
        {
            fbEntity.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Entity_PropertyChanged);
        }
        private void UnRegisterFBEntity(FBEntity fbEntity)
        {
            fbEntity.Entity.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(Entity_PropertyChanged);
        }

        private void OnEntityPropertyChanged(FBEntity fbEntity)
        {
            if (fbEntity.FBEntityState == FBEntityState.Unchanged)
            {
                fbEntity.FBEntityState = FBEntityState.Modified;

            }

        }

        private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FBEntity fbEntity = this.ListDetail.FirstOrDefault(item =>
            {
                return object.Equals(item.Entity, sender);
            });
            OnEntityPropertyChanged(fbEntity);

            if (sender.GetType() == typeof(T_FB_EXTENSIONORDERDETAIL))
            {
                if (e.PropertyName == "APPLIEDMONEY")
                {
                    CountTotalMoney();
                }
            }
        }

        private void SpecialEntity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.TravelSubject != null)
            {
                FBEntity fbEntity = this.TravelSubject.SpecialListDetail.FirstOrDefault(item =>
                {
                    return object.Equals(item.Entity, sender);
                });
                OnEntityPropertyChanged(fbEntity);

                if (sender.GetType() == typeof(T_FB_EXTENSIONORDERDETAIL))
                {
                    if (e.PropertyName == "APPLIEDMONEY")
                    {
                        CountTotalMoney();
                    }
                }
            }

        }


        #endregion

        /// <summary>
        /// 统计费用总金额
        /// </summary>
        private void CountTotalMoney()
        {
            decimal totalMoney = LittleCount();

            if (this.TravelSubject != null)
            {
                totalMoney += this.TravelSubject.SpecialListDetail.Sum(item =>
                {
                    return (item.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY;
                });
            }

            (this.CurrentOrderEntity.Entity as T_FB_EXTENSIONALORDER).TOTALMONEY = totalMoney;

            SetLittleCount();
        }

        private decimal LittleCount()
        {
            decimal totalMoney = this.ListDetail.Sum(item =>
            {
                return (item.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY;
            });
            return totalMoney;
        }

        private void SetLittleCount()
        {
            this.tbCount.Text = Convert.ToString(LittleCount());
        }
        private void HandleSpecialSubject()
        {

            if (this.TravelSubject != null)
            {
                this.TravelSubject.SpecialListDetail = GetRelationFBEntities(this.CurrentOrderEntity, "T_FB_EXTENSIONORDERDETAIL_Travel");

                this.TravelSubject.SpecialListDetail.ForEach(item =>
                {
                    //TravelSubject.subject = (item.Entity as T_FB_EXTENSIONORDERDETAIL).T_FB_SUBJECT;
                    //TravelSubject.UsableMoney = (item.Entity as T_FB_EXTENSIONORDERDETAIL).USABLEMONEY.Value;
                    //TravelSubject.ApplyMoney = TravelSubject.ApplyMoney + (item.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY;

                    item.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SpecialEntity_PropertyChanged);
                });


            }


        }

        private void rbtnChargePerson_Click(object sender, RoutedEventArgs e)
        {
            if (rbtnChargePerson.IsChecked == true)
            {
                this.gridForBorrowInfo.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.gridForBorrowInfo.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void rbtnApplyTypeCharge_Click(object sender, RoutedEventArgs e)
        {
            if (rbtnApplyTypeCharge.IsChecked == true)
            {
                this.spChargeApplyType.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.spChargeApplyType.Visibility = System.Windows.Visibility.Collapsed;
                this.rbtnChargePerson.IsChecked = true;
                this.gridForBorrowInfo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            
            object obj = sender.GetObjValue("DataContext");
            this.Delete(new List<object>() { obj });
        }
    }

    public class ChargeParameter
    {
        /// <summary>
        /// 当前总费用
        /// </summary>
        public decimal ChargeMoney { get; set; }
        /// <summary>
        /// 当前总借款
        /// </summary>
        public decimal BorrowMoney { get; set; }
        /// <summary>
        /// 已借款
        /// </summary>
        public decimal BorrowedMoney { get; set; }
        /// <summary>
        /// 差旅费
        /// </summary>
        public decimal TravelMoney { get; set; }
        /// <summary>
        /// 招待费
        /// </summary>
        public decimal EntertainmentMoney { get; set; }

    }


    public class TravelSubject
    {

        public T_FB_SUBJECT subject
        {
            get
            {

                if (SpecialListDetail != null)
                {
                    var sd = SpecialListDetail.FirstOrDefault();
                    if (sd != null)
                    {
                        return (sd.Entity as T_FB_EXTENSIONORDERDETAIL).T_FB_SUBJECT;
                    }
                }
                return new T_FB_SUBJECT();
            }
        }

        public decimal ApplyMoney
        {
            get
            {

                if (SpecialListDetail != null)
                {
                    var sd = SpecialListDetail.FirstOrDefault();
                    if (sd != null)
                    {
                        return (sd.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY;
                    }
                }
                return 0;
            }
            set
            {
                if (SpecialListDetail != null)
                {
                    var sd = SpecialListDetail.FirstOrDefault();
                    if (sd != null)
                    {
                        (sd.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY = value;
                    }
                }
            }
        }

        public decimal UsableMoney
        {
            get
            {

                if (SpecialListDetail != null)
                {
                    var sd = SpecialListDetail.FirstOrDefault();
                    if (sd != null)
                    {
                        return (sd.Entity as T_FB_EXTENSIONORDERDETAIL).USABLEMONEY.Value;
                    }
                }
                return 0;
            }
            set
            {
                if (SpecialListDetail != null)
                {
                    var sd = SpecialListDetail.FirstOrDefault();
                    if (sd != null)
                    {
                        (sd.Entity as T_FB_EXTENSIONORDERDETAIL).USABLEMONEY = value;
                    }
                }
            }
        }

        private ObservableCollection<FBEntity> _SpecialListDetail = null;
        public ObservableCollection<FBEntity> SpecialListDetail
        {
            get
            {
                if (_SpecialListDetail == null)
                {
                    _SpecialListDetail = new ObservableCollection<FBEntity>();
                }
                return _SpecialListDetail;
            }
            set
            {
                _SpecialListDetail = value;
            }
        }

    }

}
