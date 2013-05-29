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
using System.Windows.Navigation;
using SMT.FB.UI.Common;
using System.ComponentModel;
using SMT.FB.UI.FBCommonWS;
using System.Windows.Data;
using SMT.FB.UI.Common.Controls;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.FB.UI.Views.SubjectManagement
{
    public partial class SubjectCompanySet : FBBasePage
    {
        PermissionServiceClient permClient = new PermissionServiceClient();
        private int IsFbAdmin = 0;
        public SubjectCompanySet()
        {
            InitializeComponent();
            //this.FBBasePageLoaded += new EventHandler(SubjectCompany_FBBasePageLoaded);
            permClient.getFbAdminByEmployeeIDCompleted += new EventHandler<getFbAdminByEmployeeIDCompletedEventArgs>(permClient_getFbAdminByEmployeeIDCompleted);
            permClient.getFbAdminByEmployeeIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void permClient_getFbAdminByEmployeeIDCompleted(object sender, getFbAdminByEmployeeIDCompletedEventArgs e)
        {
            
            if (e.Error == null)
            {
                InitForm();
                IsFbAdmin = e.Result;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs("提示", "系统发生错误请联系管理员。", Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                
            }
        }

        void SubjectCompany_FBBasePageLoaded(object sender, EventArgs e)
        {
            InitForm();
        }

        #region Fields
        private List<OrderEntity> EntityList;
        private Dictionary<SMT.FB.UI.FBCommonWS.EntityObject, OrderEntity> dictTreeViewItem;

        private OrderEntity currentOrderEntity = null;
        protected OrderEntity CurrentOrderEntity
        {
            get
            {
                return currentOrderEntity;
            }
            set
            {
                if (!object.Equals(currentOrderEntity, value))
                {
                    currentOrderEntity = value;
                    GetEditForm(currentOrderEntity);
                }
            }
        }
        private TreeViewItem currentTreeViewItem = null;

        public EditForm CurrentEditForm { get; set; }
        private ObjectList<EditForm> forms { get; set; }
        private OrderEntityService orderEntityService = null;

        #endregion

        #region 属性

        public List<IControlAction> Controls { get; set; }
        [DefaultValue(true)]
        public bool RefreshEntityWhenLoad { get; set; }
        #endregion



        #region 获取数据
        protected void InitData()
        {
            EntityList.Clear();

            dictTreeViewItem.Clear();

            QueryExpression qe = new QueryExpression();
            //qe.QueryType = typeof(T_FB_SUBJECTCOMPANY).Name;
            qe.QueryType = "T_FB_SUBJECTCOMPANYSET";
            //qe.VisitModuleCode = "T_FB_SUBJECTCOMPANYSET";
            
            orderEntityService.QueryFBEntities(qe);

            this.TreeView.Items.AddTempLoadingItem();
        }

        void orderEntityService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            this.TreeView.Items.Clear();

            List<OrderEntity> listCompany = e.Result.ToEntityAdapterList<OrderEntity>().ToList();
            
            List<TreeViewItem> items = TreeView.Items.AddObjectList(listCompany, "Entity.Name");
            EntityList = listCompany;

            TreeViewItem tvi = this.TreeView.Items.FirstOrDefault() as TreeViewItem;
            if (tvi != null)
            {
                tvi.IsSelected = true;

                RoutedPropertyChangedEventArgs<object> ea = new RoutedPropertyChangedEventArgs<object>(null, tvi);
                TreeView_SelectedItemChanged(tvi, ea);
            }
        }

       

        #endregion


        #region 初始化
        private void InitForm()
        {
            tooBarTop.InitToolBarItem(new List<ToolbarItem>() { ToolBarItems.Save });
            tooBarTop.ItemClicked += new EventHandler<ToolBar.ToolBarItemClickArgs>(tooBarTop_ItemClicked);
            editForm.RefreshEntityWhenLoad = false;
            editForm.LoadControlComplete += new EventHandler(editForm_LoadControlComplete);
            orderEntityService = new OrderEntityService();
            orderEntityService.ModelCode = typeof(T_FB_SUBJECTCOMPANY).Name;

            this.FormTitleName.TextTitle.Text = "公司科目设置";

            orderEntityService.SaveListCompleted += new EventHandler<ActionCompletedEventArgs<bool>>(orderEntityService_SaveListCompleted);
            orderEntityService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(orderEntityService_QueryFBEntitiesCompleted);
            EntityList = new List<OrderEntity>();
            forms = new ObjectList<EditForm>();
            dictTreeViewItem = new Dictionary<SMT.FB.UI.FBCommonWS.EntityObject, OrderEntity>();

            this.FormTitleName.Visibility = FBBasePage.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;
            InitData();
            

        }
        
               
        #endregion
        protected void GetEditForm(OrderEntity orderEntity)
        {
            editForm.OrderEntity = orderEntity;
            
            if (orderEntity != null)
            {
                Title = orderEntity.OrderTypeName;
            }
            editForm.InitForm();
        }

        void editForm_LoadControlComplete(object sender, EventArgs e)
        {
            DetailGrid grid = editForm.FindChildControl<DetailGrid>("OrderGrid");
            if (grid != null)
            {
                //如果是公司预算管理员：则自己的公司不可以修改
                if (IsFbAdmin == 1)
                {
                    OrderEntity entity = currentTreeViewItem.DataContext as OrderEntity;

                    string StrCompany = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    string SelectCompany = (entity.Entity as VirtualCompany).ID;
                    if (StrCompany == SelectCompany)
                    {
                        grid.ADGrid.Columns[2].IsReadOnly = true;
                    }
                    else
                    {
                        grid.ADGrid.Columns[2].IsReadOnly = false;
                    }
                }
                
                grid.ADGrid.Columns[3].Visibility = Visibility.Collapsed;
                grid.ADGrid.Columns[4].Visibility = Visibility.Collapsed;
                grid.ADGrid.Columns[5].Visibility = Visibility.Collapsed;
                grid.ADGrid.Columns[6].Visibility = Visibility.Collapsed;
                grid.ADGrid.Columns[7].Visibility = Visibility.Collapsed;
                List<string> groups = new List<string> { "Entity.T_FB_SUBJECT.T_FB_SUBJECTTYPE.SUBJECTTYPENAME" };
                grid.Groups = groups;

                //活动经费处理
                OrderEntity oe = grid.DataContext as OrderEntity;
                oe.FBEntity.CollectionEntity.ForEach(p =>
                {
                    p.FBEntities.ForEach(item =>
                    {
                        var v = item.Entity as T_FB_SUBJECTCOMPANY;
                        if (v != null && v.T_FB_SUBJECT.SUBJECTID == DataCore.SystemSetting.MONEYASSIGNSUBJECTID)
                        {
                            item.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(DeatilEntity_PropertyChanged);
                            DeatilEntity_PropertyChanged(item.Entity, null);
                        }
                    });
                });
            }
        }

        void DeatilEntity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {            
           T_FB_SUBJECTCOMPANY aa= sender as T_FB_SUBJECTCOMPANY;
           
           //sender.SetObjValue("ACTIVED", 1);
           sender.SetObjValue("ISYEARBUDGET", 1);
           sender.SetObjValue("ISMONTHLIMIT", 1);
           sender.SetObjValue("ISPERSON", 1);
           sender.SetObjValue("CONTROLTYPE", 3);
        }

        void ADGrid_LoadingRowGroup(object sender, DataGridRowGroupHeaderEventArgs e)
        {
            
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem treeViewItem = e.NewValue as TreeViewItem;
            if (treeViewItem != null)
            {
                this.currentTreeViewItem = treeViewItem;
                OrderEntity entity = currentTreeViewItem.DataContext as OrderEntity;
                this.CurrentOrderEntity = entity;// new OrderEntity(typeof(VirtualCompany));               
            }
            //PermissionHelper.GetPermissionValue(orderEntityService.ModelCode, Permissions.Edit);
            //tooBarTop.ShowItem("Save", 

        }

        #region 保存

        private void tooBarTop_ItemClicked(object sender, ToolBar.ToolBarItemClickArgs e)
        {
            
            try
            {
                ShowProcess();
                
                Save();
                
            }
            catch ( Exception ex)
            {
                CommonFunction.ShowErrorMessage(ex.Message);
                CloseProcess();
            }
        }

        private void Save()
        {
            ObservableCollection<FBEntity> listOfFBEntity = new ObservableCollection<FBEntity>();
            for (int i = 0; i < EntityList.Count; i++)
            {
                OrderEntity item = EntityList[i];
                FBEntity fbEntitySave = item.GetModifiedFBEntity();

                
                fbEntitySave.CollectionEntity[0].FBEntities.ToList().ForEach(fbEntity =>
                {
                    T_FB_SUBJECTCOMPANY sc = fbEntity.Entity as T_FB_SUBJECTCOMPANY;
                    if (fbEntity.Entity.EntityKey.EntityKeyValues == null)
                    {
                        
                        fbEntity.FBEntityState = FBEntityState.Added;
                        fbEntity.Entity.EntityKey = null;
                        
                        fbEntity.SetObjValue("Entity.SUBJECTCOMPANYID", Guid.NewGuid().ToString());
                        fbEntity.SetObjValue("Entity.CREATECOMPANYID", item.LoginUser.Company.Value);
                        
                        fbEntity.SetObjValue("Entity.CREATECOMPANYNAME", item.LoginUser.Company.Text);
                        fbEntity.SetObjValue("Entity.CREATEDEPARTMENTID", item.LoginUser.Department.Value);
                        fbEntity.SetObjValue("Entity.CREATEDEPARTMENTNAME", item.LoginUser.Department.Text);
                        fbEntity.SetObjValue("Entity.CREATEPOSTID", item.LoginUser.Post.Value);
                        fbEntity.SetObjValue("Entity.CREATEPOSTNAME", item.LoginUser.Post.Text);
                        fbEntity.SetObjValue("Entity.CREATEUSERID", item.LoginUser.Value);
                        fbEntity.SetObjValue("Entity.CREATEUSERNAME", item.LoginUser.Text);

                        fbEntity.SetObjValue("Entity.OWNERCOMPANYID", item.GetObjValue("Entity.OWNERCOMPANYID"));
                        fbEntity.SetObjValue("Entity.OWNERCOMPANYNAME", item.GetObjValue("Entity.OWNERCOMPANYNAME"));
                        fbEntity.SetObjValue("Entity.OWNERDEPARTMENTID", item.LoginUser.Department.Value);
                        fbEntity.SetObjValue("Entity.OWNERDEPARTMENTNAME", item.LoginUser.Department.Text);
                        fbEntity.SetObjValue("Entity.OWNERPOSTID", item.LoginUser.Post.Value);
                        fbEntity.SetObjValue("Entity.OWNERPOSTNAME", item.LoginUser.Post.Text);
                        fbEntity.SetObjValue("Entity.OWNERID", item.LoginUser.Value);
                        fbEntity.SetObjValue("Entity.OWNERNAME", item.LoginUser.Text);
                        fbEntity.SetObjValue("Entity.EDITSTATES", decimal.Parse("1"));
                    }
                    else
                    {
                        if (sc.EDITSTATES != 1)
                        {
                            fbEntity.FBEntityState = FBEntityState.Detached;
                        }
                    }
                    
                    fbEntity.SetObjValue("Entity.UPDATEUSERID", item.LoginUser.Value);
                    fbEntity.SetObjValue("Entity.UPDATEUSERNAME", item.LoginUser.Text);

                    sc.T_FB_SUBJECTReference.EntityKey = sc.T_FB_SUBJECT.EntityKey;
                    sc.T_FB_SUBJECT = null;

                    sc.T_FB_SUBJECTDEPTMENT = new ObservableCollection<T_FB_SUBJECTDEPTMENT>();
                    listOfFBEntity.Add(fbEntity);

                });
            }

            if (listOfFBEntity.Count > 0)
            {
                this.CurrentOrderEntity = new OrderEntity(typeof(VirtualCompany));
                this.orderEntityService.SaveList(listOfFBEntity);
            }
            else
            {
                CloseProcess();
            }
        }

        void orderEntityService_SaveListCompleted(object sender, ActionCompletedEventArgs<bool> e)
        {
            if (e.Result)
            {
                CommonFunction.NotifySelection("保存成功");
                InitData();
            }
            CloseProcess();
        }

        // 不要删除
        private void SetFouceBeforeAction(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }
        #endregion

        private void SelectTreeViewItem(TreeViewItem item)
        {
            if (item != null)
            {
                item.IsSelected = true;
                TreeView_SelectedItemChanged(this.TreeView, new RoutedPropertyChangedEventArgs<object>(null, item));
            }
        }

        public override bool CheckPermisstion()
        {
            int perm = PermissionHelper.GetPermissionValue("T_FB_SUBJECTCOMPANYSET", Permissions.Browse);
            //return true;
            //// 需要大于等公司的范围权限
            return !(perm > (int)PermissionRange.Company || perm < 0);
       
        }

    }
}
