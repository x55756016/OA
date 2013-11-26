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
using SMT.FB.UI.FBCommonWS;
using System.ComponentModel;
using System.Windows.Data;
using SMT.FB.UI.Common.Controls;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.FB.UI.Views.SubjectManagement
{
    public partial class SubjectDepartment : FBBasePage
    {
        static ObservableCollection<FBEntity> staticobpost = null;
        public SubjectDepartment()
        {
            InitializeComponent();
            this.FBBasePageLoaded += new EventHandler(SubjectDepartment_FBBasePageLoaded);
            staticobpost = new ObservableCollection<FBEntity>();
        }

        void SubjectDepartment_FBBasePageLoaded(object sender, EventArgs e)
        {
            InitForm();
        }

        #region Fields
        private IList<OrderEntity> EntityList;
        private Dictionary<EntityObject, OrderEntity> dictTreeViewItem;
        private Dictionary<VirtualDepartment, OrderEntity> dictDepartment;
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
        private FBCommonServiceClient SubjectService = null;
        public OrderInfo OrderInfo { get; set; }
        public List<IControlAction> Controls { get; set; }
        [DefaultValue(true)]
        public bool RefreshEntityWhenLoad { get; set; }

        #endregion

        #region 全局变量
        private string companyId = string.Empty;//保存公司ID，在查询是使用
        #endregion

        private void InitForm()
        {
            this.FormTitleName.TextTitle.Text = "部门科目维护管理";

            tooBarTop.InitToolBarItem(new List<ToolbarItem>() { ToolBarItems.Save });
            tooBarTop.ItemClicked += new EventHandler<ToolBar.ToolBarItemClickArgs>(tooBarTop_ItemClicked);


            orderEntityService = new OrderEntityService();
            orderEntityService.ModelCode = typeof(T_FB_SUBJECTDEPTMENT).Name;
            orderEntityService.SaveListCompleted += new EventHandler<ActionCompletedEventArgs<bool>>(orderEntityService_SaveListCompleted);
            orderEntityService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(orderEntityService_QueryFBEntitiesCompleted);
            EntityList = new List<OrderEntity>();
            forms = new ObjectList<EditForm>();
            dictTreeViewItem = new Dictionary<EntityObject, OrderEntity>();
            dictDepartment = new Dictionary<VirtualDepartment, OrderEntity>();

            this.FormTitleName.Visibility = FBBasePage.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;
            InitData();
        }


        bool isInit = false;
        void orderEntityService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            Dictionary<VirtualCompany, TreeViewItem> dictCompany = new Dictionary<VirtualCompany, TreeViewItem>();

            //  this.TreeView.Items.Clear();
            List<OrderEntity> listDepartment = e.Result.ToEntityAdapterList<OrderEntity>().ToList();

            if (currTreeItmes != null)
            {
                currTreeItmes.Items.Clear();
            }
            if (!isInit)
            {
                this.TreeView.Items.Clear();
            }
            listDepartment.ForEach(department =>
            {
                TreeViewItem tvItemCompany = null;
                VirtualDepartment virtualDepartment = department.Entity as VirtualDepartment;
                if (isInit)
                {
                    tvItemCompany = currTreeItmes.Items.AddObject<OrderEntity>(department, "Entity.Name");
                    tvItemCompany.Items.AddObjectList<VirtualPost>(virtualDepartment.PostCollection.ToList(), "Name");
                    dictDepartment.Add(virtualDepartment, department);
                    AttachEventToSubjectDepartment(department);
                    EntityList.Add(department);
                }
                else
                {
                    if (!dictCompany.ContainsKey(virtualDepartment.VirtualCompany))
                    {
                        tvItemCompany = this.TreeView.Items.AddObject<VirtualCompany>(virtualDepartment.VirtualCompany, "Name");
                        dictCompany.Add(virtualDepartment.VirtualCompany, tvItemCompany);
                    }
                    tvItemCompany = dictCompany[virtualDepartment.VirtualCompany];
                    TreeViewItem tvItemDepartment = tvItemCompany.Items.AddObject<OrderEntity>(department, "Entity.Name");
                    tvItemDepartment.Items.AddObjectList<VirtualPost>(virtualDepartment.PostCollection.ToList(), "Name");
                    //dictDepartment.Add(virtualDepartment, department);
                   // AttachEventToSubjectDepartment(department);
                   // EntityList.Add(department);
                }
                //  tvItemCompany = this.TreeView.Items.AddObject<VirtualCompany>(virtualDepartment.VirtualCompany, "Name");

            });
            //TreeViewItem tvi = this.TreeView.Items.FirstOrDefault() as TreeViewItem;
            //if (tvi != null)
            //{
            //    tvi.IsSelected = true;

            //    RoutedPropertyChangedEventArgs<object> ea = new RoutedPropertyChangedEventArgs<object>(null, tvi);
            //    TreeView_SelectedItemChanged(tvi, ea);
            //}
            isInit = true;
            this.CloseProcess();
        }

        void orderEntityService_SaveListCompleted(object sender, ActionCompletedEventArgs<bool> e)
        {
            if (e.Result)
            {
                CommonFunction.NotifySuccessfulSaving(string.Empty);
                InitData();
            }
            this.CloseProcess();

        }

        protected void InitData()
        {
            try
            {
                isInit = false;
                this.ShowProcess();
                EntityList.Clear();

                dictTreeViewItem.Clear();

                QueryExpression qe = new QueryExpression();

                qe.QueryType = typeof(T_FB_SUBJECTDEPTMENT).Name;
                orderEntityService.QueryFBEntities(qe);

                this.TreeView.Items.AddTempLoadingItem();

            }
            catch
            {
                this.CloseProcess();
            }
        }

        protected void GetEditForm(OrderEntity orderEntity)
        {

            forms.ToList().ForEach(
                form =>
                {
                    form.Visibility = Visibility.Collapsed;
                });
            if (orderEntity == null)
            {
                return;
            }
            string key = orderEntity.OrderType.Name;
            EditForm editForm = null;
            if (!forms.Dictionary.ContainsKey(key))
            {
                editForm = new EditForm(orderEntity);
                editForm.RefreshEntityWhenLoad = false;
                editForm.LoadControlComplete += new EventHandler(editForm_LoadControlComplete);
                forms.Add(key, editForm);
                editForm.InitForm();

                //editForm.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.MainGrid.Children.Add(editForm);
            }
            else
            {
                editForm = forms[key];
                editForm.OrderEntity = orderEntity;
                Title = orderEntity.OrderTypeName;
                editForm.InitForm();
                editForm.Visibility = Visibility.Visible;
            }
            CurrentEditForm = editForm;

        }

        void editForm_LoadControlComplete(object sender, EventArgs e)
        {
            EditForm editForm = sender as EditForm;

            DetailGrid grid = editForm.FindChildControl<DetailGrid>("OrderGrid");
            if (grid != null)
            {
                List<string> groups = new List<string> { "Entity.T_FB_SUBJECT.T_FB_SUBJECTTYPE.SUBJECTTYPENAME" };
                grid.Groups = groups;
            }

            //初始化活动经费
            BeginMoneyAssign(grid);
        }

        TreeViewItem currTreeItmes;
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender!=null)
            {
                var treeView = sender as TreeView;
                currTreeItmes = treeView.SelectedItem as TreeViewItem;
            }
            TreeViewItem treeViewItem = e.NewValue as TreeViewItem;
            if (treeViewItem != null)
            {
                QuerySubjectCompanyData(treeViewItem);
                this.currentTreeViewItem = treeViewItem;

                VirtualPost virtualPost = currentTreeViewItem.DataContext as VirtualPost;
                if (virtualPost != null)
                {
                    this.CurrentOrderEntity = CreatePostEntity(virtualPost, treeViewItem, string.Empty);
                }
                else
                {
                    OrderEntity entity = currentTreeViewItem.DataContext as OrderEntity;
                    this.CurrentOrderEntity = entity;
                }
            }
        }

        /// <summary>
        /// 点击公司右边显示科目
        /// </summary>
        /// <param name="item"></param>
        void QuerySubjectCompanyData(TreeViewItem item)
        {
            try
            {
                var com = item.DataContext as VirtualEntityObject;
                if (com != null && com.GetType().ToString() == "SMT.FB.UI.FBCommonWS.VirtualCompany")
                {
                    QueryExpression qe = new QueryExpression();
                    qe.QueryType = typeof(T_FB_SUBJECTDEPTMENT).Name;
                    QueryExpression company = new QueryExpression();
                    company.PropertyName = "COMPANYID";
                    company.PropertyValue = com.ID;
                    companyId = com.ID;
                    qe.RelatedExpression = company;
                    this.ShowProcess();
                    orderEntityService.QueryFBEntities(qe);
                }
            }
            catch (Exception ex)
            {
                CommonFunction.ShowErrorMessage(ex.Message);
                CloseProcess();
            }
        }


        #region ToolBar

        public void tooBarTop_ItemClicked(object sender, ToolBar.ToolBarItemClickArgs e)
        {
            try
            {
                ShowProcess();

                Save();

            }
            catch (Exception ex)
            {
                CommonFunction.ShowErrorMessage(ex.Message);
                CloseProcess();
            }
        }

        public void Save()
        {
            ObservableCollection<FBEntity> listOfFBEntity = new ObservableCollection<FBEntity>();
            for (int i = 0; i < EntityList.Count; i++)
            {
                OrderEntity item = EntityList[i];

                FBEntity fbEntitySave = item.GetModifiedFBEntity();
                ObservableCollection<FBEntity> listSave = null;
                if (fbEntitySave.Entity.GetType() == typeof(VirtualDepartment))
                {
                    listSave = fbEntitySave.GetRelationFBEntities(typeof(T_FB_SUBJECTDEPTMENT).Name);

                    //活动经费
                    // CheckMoneyAssign(listSave);
                }
                else
                {
                    listSave = fbEntitySave.GetRelationFBEntities(typeof(T_FB_SUBJECTPOST).Name);

                    // 删除 不是active的岗位项目记录
                    listSave.RemoveAll(itemRemove =>
                    {
                        bool isAdded = itemRemove.FBEntityState == FBEntityState.Added;
                      //  bool isNew = (itemRemove.Entity as T_FB_SUBJECTPOST).ACTIVED != 1;
                        //return isAdded && isNew;
                        return isAdded;
                    });
                }

                listSave.ToList().ForEach(fbEntity =>
                {
                    bool tf = true;//外键为空的不处理
                    #region 公共参数赋值
                    if (fbEntity.IsNewEntity())
                    {
                        fbEntity.FBEntityState = FBEntityState.Added;


                        fbEntity.SetObjValue("Entity.CREATECOMPANYID", item.LoginUser.Company.Value);
                        fbEntity.SetObjValue("Entity.CREATECOMPANYNAME", item.LoginUser.Company.Text);
                        fbEntity.SetObjValue("Entity.CREATEDEPARTMENTID", item.LoginUser.Department.Value);
                        fbEntity.SetObjValue("Entity.CREATEDEPARTMENTNAME", item.LoginUser.Department.Text);
                        fbEntity.SetObjValue("Entity.CREATEPOSTID", item.LoginUser.Post.Value);
                        fbEntity.SetObjValue("Entity.CREATEPOSTNAME", item.LoginUser.Post.Text);
                        fbEntity.SetObjValue("Entity.CREATEUSERID", item.LoginUser.Value);
                        fbEntity.SetObjValue("Entity.CREATEUSERNAME", item.LoginUser.Text);

                        fbEntity.SetObjValue("Entity.EDITSTATES", decimal.Parse("1"));

                    }
                    fbEntity.SetObjValue("Entity.UPDATEUSERID", item.LoginUser.Value);
                    fbEntity.SetObjValue("Entity.UPDATEUSERNAME", item.LoginUser.Text);
                    #endregion

                    T_FB_SUBJECTPOST sp = fbEntity.Entity as T_FB_SUBJECTPOST;

                    // 去除实体之间的关联,如有关联存在,上传服务端时,会有异常
                    if (sp == null)
                    {
                        T_FB_SUBJECTDEPTMENT sd = fbEntity.Entity as T_FB_SUBJECTDEPTMENT;

                        if (sd.T_FB_SUBJECTCOMPANY != null && sd.T_FB_SUBJECT != null)
                        {
                            sd.T_FB_SUBJECTCOMPANYReference.EntityKey = sd.T_FB_SUBJECTCOMPANY.EntityKey;
                            sd.T_FB_SUBJECTCOMPANY = null;

                            sd.T_FB_SUBJECTReference.EntityKey = sd.T_FB_SUBJECT.EntityKey;
                            sd.T_FB_SUBJECT = null;
                            sd.T_FB_SUBJECTPOST = null;
                            fbEntity.CollectionEntity.Clear();
                        }
                        else
                        {
                            tf = false;
                        }
                    }
                    else
                    {
                        if (sp.T_FB_SUBJECTDEPTMENT != null && sp.T_FB_SUBJECT != null)
                        {
                            EntityKey parentKey = sp.T_FB_SUBJECTDEPTMENT.EntityKey;
                            if (parentKey.EntityKeyValues == null)
                            {
                                EntityKeyMember em = new EntityKeyMember();
                                em.Key = "SUBJECTDEPTMENTID";
                                em.Value = sp.T_FB_SUBJECTDEPTMENT.SUBJECTDEPTMENTID;

                                EntityKey newKey = new EntityKey();
                                newKey.EntityContainerName = parentKey.EntityContainerName;
                                newKey.EntitySetName = parentKey.EntitySetName;
                                newKey.EntityKeyValues = new ObservableCollection<EntityKeyMember>();
                                newKey.EntityKeyValues.Add(em);
                                parentKey = newKey;
                            }
                            sp.T_FB_SUBJECTDEPTMENTReference = new EntityReferenceOfT_FB_SUBJECTDEPTMENTZ5CrhPbu();
                            sp.T_FB_SUBJECTDEPTMENTReference.EntityKey = parentKey;
                            sp.T_FB_SUBJECTDEPTMENT = null;

                            sp.T_FB_SUBJECTReference = new EntityReferenceOfT_FB_SUBJECTZ5CrhPbu();
                            sp.T_FB_SUBJECTReference.EntityKey = sp.T_FB_SUBJECT.EntityKey;
                            sp.T_FB_SUBJECT = null;

                            // 清除已删除实体,这些实体会在 T_FB_SUBJECTDEPTMENT 实体中处理 .
                            fbEntity.GetRelationFBEntities(Args.DELETE_ENTITYTYPE).Clear();
                        }
                        else
                        {
                            tf = false;
                        }
                    }
                    if (tf)
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
        // 不要删除
        private void SetFouceBeforeAction(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }
        #endregion

        /// <summary>
        /// 创建或获取相应岗位科目
        /// </summary>
        /// <param name="virtualPost"></param>
        /// <returns></returns>
        private OrderEntity CreatePostEntity(VirtualPost virtualPost, TreeViewItem treeViewItem, string strFlag)
        {
            ObservableCollection<FBEntity> listFBEntities = new ObservableCollection<FBEntity>();
            OrderEntity oeDepartment = dictDepartment[virtualPost.VirtualDepartment];

            // 已存在的岗位科目
            ObservableCollection<FBEntity> listOfSubjectDepartmentFB = oeDepartment.GetRelationFBEntities(typeof(T_FB_SUBJECTDEPTMENT).Name);

            //　1. 获取所有已启用的部门科目
            var itemsDepartmentActived = listOfSubjectDepartmentFB.Where(item =>
            {
                return (item.Entity as T_FB_SUBJECTDEPTMENT).ACTIVED == 1;
            });

            // 2. 遍历所有已启用的部门科目, 添加相应的岗位科目,已有科目的，则加上原有的，没有的，则新增

            foreach (FBEntity entityDepartment in itemsDepartmentActived)
            {
                T_FB_SUBJECTDEPTMENT sd = entityDepartment.Entity as T_FB_SUBJECTDEPTMENT;

                // 是否已有岗位科目的记录
                List<FBEntity> listPost = entityDepartment.GetRelationFBEntities(typeof(T_FB_SUBJECTPOST).Name, item =>
                {
                    return (item.Entity as T_FB_SUBJECTPOST).OWNERPOSTID == virtualPost.ID;
                });

                // 已有岗位科目，添加。没有，就新增         
                if (listPost.Count > 0)
                {
                    T_FB_SUBJECTPOST post = listPost[0].Entity as T_FB_SUBJECTPOST;
                    if (post.ACTIVED != sd.ACTIVED)
                    {
                        listPost[0].FBEntityState = FBEntityState.Modified;
                    }
                    //岗位默认处理
                    if (post != null)
                        post = SubjectPostChanged(sd, post, strFlag);
                    listPost[0].Entity = post;
                    listFBEntities.Add(listPost[0]);
                }
                else
                {
                    //临时添加防止重复数据
                    var a = staticobpost.FirstOrDefault(item => (item.Entity as T_FB_SUBJECTPOST).OWNERPOSTID == virtualPost.ID && (item.Entity as T_FB_SUBJECTPOST).T_FB_SUBJECT != null && (item.Entity as T_FB_SUBJECTPOST).T_FB_SUBJECT.SUBJECTID == sd.T_FB_SUBJECT.SUBJECTID);
                    if (a == null && sd.ACTIVED == 1)
                    {
                        FBEntity fbEntityPostNew = this.CreateSubjectPost(sd, virtualPost, strFlag);
                        listFBEntities.Add(fbEntityPostNew);

                        staticobpost.Add(fbEntityPostNew);
                    }
                    else
                    {
                        listFBEntities.Add(a);
                    }
                }
            }

            FBEntity postFBEntity = virtualPost.ToFBEntity();
            postFBEntity.AddFBEntities<T_FB_SUBJECTPOST>(listFBEntities);
            OrderEntity entityPost = new OrderEntity(postFBEntity);

            Binding binding = new Binding();
            binding.Path = new PropertyPath("Entity.Name");
            treeViewItem.SetBinding(TreeViewItem.HeaderProperty, binding);
            treeViewItem.DataContext = entityPost;
            EntityList.Add(entityPost);
            return entityPost;
        }

        /// <summary>
        /// 新增岗位科目
        /// </summary>
        /// <param name="sd"></param>
        /// <returns></returns>
        private FBEntity CreateSubjectPost(T_FB_SUBJECTDEPTMENT sd, VirtualPost virtualPost, string strFlag)
        {
            T_FB_SUBJECTPOST post = new T_FB_SUBJECTPOST();
            post.T_FB_SUBJECTDEPTMENT = sd;
            post.T_FB_SUBJECT = sd.T_FB_SUBJECT;
            post.LIMITBUDGEMONEY = 0;
            post.SUBJECTPOSTID = Guid.NewGuid().ToString();

            post.OWNERID = DataCore.SuperUser.Value.ToString();
            post.OWNERPOSTID = virtualPost.ID;
            post.OWNERDEPARTMENTID = virtualPost.OWNERDEPARTMENTID;
            post.OWNERCOMPANYID = virtualPost.OWNERCOMPANYID;

            //岗位默认处理
            post = SubjectPostChanged(sd, post, strFlag);

            FBEntity fbEntityPostNew = post.ToFBEntity();
            fbEntityPostNew.FBEntityState = FBEntityState.Added;

            return fbEntityPostNew;
        }

        private void CreateNonActionSubjectPost(T_FB_SUBJECTDEPTMENT sd, T_FB_SUBJECTPOST post, VirtualPost virtualPost)
        {
            ObservableCollection<FBEntity> listFBEntities = new ObservableCollection<FBEntity>();
            post.ACTIVED = 0;
            post.UPDATEDATE = DateTime.Now;
            FBEntity fbEntityPostNew = post.ToFBEntity();
            fbEntityPostNew.FBEntityState = FBEntityState.Modified;
            listFBEntities.Add(fbEntityPostNew);
            FBEntity postFBEntity = virtualPost.ToFBEntity();
            postFBEntity.AddFBEntities<T_FB_SUBJECTPOST>(listFBEntities);
            OrderEntity entityPost = new OrderEntity(postFBEntity);
            EntityList.Add(entityPost);
        }

        /// <summary>
        /// 添加 部门项目actived 改变时,触发事件
        /// </summary>
        /// <param name="departmentEntity"></param>
        private void AttachEventToSubjectDepartment(OrderEntity departmentEntity)
        {

            ObservableCollection<FBEntity> listOfSubjectDepartmentFB = departmentEntity.GetRelationFBEntities(typeof(T_FB_SUBJECTDEPTMENT).Name);

            listOfSubjectDepartmentFB.ToList().ForEach(item =>
            {
                item.Entity.PropertyChanged += new PropertyChangedEventHandler(SubjectDepartment_PropertyChanged);
            });
        }

        /// <summary>
        /// 处理 部门项目actived 改变时,相应的增减对应的岗位项目
        ///  
        /// </summary>
        /// <remarks>
        ///     1. 去除 SubjectPost的所有关联,如有跟其他实体有关联,再提交服务端时,可能会出错
        ///     2. 从 RelationCollection中去掉 SubjectPost
        ///     3. 根据 SubjectDepartment 是否启用，对SubjectPost进行相应处理
        ///        Y.新增SubjectPost　到集合中
        ///        N.从集合中去除SubjectPost
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SubjectDepartment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ACTIVED" || e.PropertyName == "ISPERSON")
            {
                T_FB_SUBJECTDEPTMENT subjectDepartment = sender as T_FB_SUBJECTDEPTMENT;

                VirtualDepartment vd = this.currentOrderEntity.Entity as VirtualDepartment;
                OrderEntity curOrderEntity = this.dictDepartment[vd];

                // 3.如果是不启用的部门科目，则从各个岗位项目对象中去除 SubjectPost，　否则，添加相应的subjectPost

                foreach (TreeViewItem item in this.currentTreeViewItem.Items)
                {
                    OrderEntity oe = item.DataContext as OrderEntity;
                    if (oe != null)
                    {
                        VirtualPost virtualPost = oe.Entity as VirtualPost;
                        // 启用科目，添加新subjectpost.
                        if (subjectDepartment.ACTIVED == 1)
                        {
                            if (e.PropertyName == "ACTIVED")//启用科目
                            {
                                //FBEntity fbEntityPostNew = CreateSubjectPost(subjectDepartment, virtualPost, "ACTIVED");
                                //oe.FBEntity.AddFBEntities<T_FB_SUBJECTPOST>(new List<FBEntity> { fbEntityPostNew });

                                CreatePostEntity(virtualPost, item, "ACTIVED");
                            }
                            else if (e.PropertyName == "ISPERSON")//修改科目
                            {
                                oe = IsPersonChanged(oe, subjectDepartment);
                            }
                        }
                        else // 从集合中去除相应的subjectPost
                        {
                            if (subjectDepartment.T_FB_SUBJECTPOST!=null)
                            {
                                foreach (var postEnt in subjectDepartment.T_FB_SUBJECTPOST)
                                {
                                    CreateNonActionSubjectPost(subjectDepartment, postEnt, virtualPost);
                                }
                            }
                            ObservableCollection<FBEntity> fbEntities = oe.GetRelationFBEntities(typeof(T_FB_SUBJECTPOST).Name);
                            fbEntities.RemoveAll(entity =>
                            {
                                return (entity.Entity as T_FB_SUBJECTPOST).T_FB_SUBJECTDEPTMENT.SUBJECTDEPTMENTID == subjectDepartment.SUBJECTDEPTMENTID;
                            });
                        }
                    }
                    else //处理修改部门数据后，默认处理岗位数据
                    {
                        VirtualPost virtualPost = item.DataContext as VirtualPost;
                        if (virtualPost == null)
                        {
                            continue;
                        }

                        // 启用科目，添加新subjectpost.
                        if (subjectDepartment.ACTIVED == 1)
                        {
                            if (e.PropertyName == "ACTIVED")//启用科目
                            {
                                //  FBEntity fbEntityPostNew = CreateSubjectPost(subjectDepartment, virtualPost, "ACTIVED");
                                //  curOrderEntity.FBEntity.AddFBEntities<T_FB_SUBJECTPOST>(new List<FBEntity> { fbEntityPostNew });

                                CreatePostEntity(virtualPost, item, "ACTIVED");
                            }
                            else if (e.PropertyName == "ISPERSON")//修改科目
                            {
                                curOrderEntity = IsPersonChanged(curOrderEntity, subjectDepartment);
                            }
                        }
                        else // 从集合中去除相应的subjectPost
                        {
                            if (subjectDepartment.T_FB_SUBJECTPOST != null)
                            {
                                foreach (var postEnt in subjectDepartment.T_FB_SUBJECTPOST)
                                {
                                    CreateNonActionSubjectPost(subjectDepartment, postEnt, virtualPost);
                                }
                            }
                            ObservableCollection<FBEntity> fbEntities = curOrderEntity.GetRelationFBEntities(typeof(T_FB_SUBJECTPOST).Name);
                            fbEntities.RemoveAll(entity =>
                            {
                                return (entity.Entity as T_FB_SUBJECTPOST).T_FB_SUBJECTDEPTMENT.SUBJECTDEPTMENTID == subjectDepartment.SUBJECTDEPTMENTID;
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加不生效岗位
        /// </summary>
        /// <param name="subjectDepartment"></param>
        /// <param name="virtualPost"></param>
        private void CreateNonActionPost(T_FB_SUBJECTDEPTMENT subjectDepartment, VirtualPost virtualPost)
        {
            ObservableCollection<FBEntity> listFBEntities = new ObservableCollection<FBEntity>();
            FBEntity fbEntityPostNew = this.CreateSubjectPost(subjectDepartment, virtualPost, "ACTIVED");
            listFBEntities.Add(fbEntityPostNew);
            FBEntity postFBEntity = virtualPost.ToFBEntity();
            postFBEntity.AddFBEntities<T_FB_SUBJECTPOST>(listFBEntities);
            OrderEntity entityPost = new OrderEntity(postFBEntity);
            EntityList.Add(entityPost);
        }
        public override bool CheckPermisstion()
        {
            int perm = PermissionHelper.GetPermissionValue(typeof(T_FB_SUBJECTDEPTMENT).Name, Permissions.Browse);

            // 需要大于等公司的范围权限
            return !(perm > (int)PermissionRange.Department || perm < 0);

        }

        //处理默认活动经费岗位的设置
        public void CheckMoneyAssign(ObservableCollection<FBEntity> listSave)
        {
            #region 处理默认活动经费岗位的设置
            listSave.ForEach(p =>
            {
                foreach (TreeViewItem tvi in this.currentTreeViewItem.Items)
                {
                    OrderEntity oe = tvi.DataContext as OrderEntity;
                    bool tf = true;
                    if (oe != null)
                    {
                        VirtualPost virtualPost = oe.Entity as VirtualPost;
                        //活动经费处理初始化                                  
                        if (virtualPost != null && tf)
                        {
                            tf = false;
                            CreatePostEntity(virtualPost, tvi, "MoneyAssign");
                        }
                    }
                    else
                    {
                        VirtualPost virtualPost = tvi.DataContext as VirtualPost;
                        //活动经费处理初始化                                  
                        if (virtualPost != null && tf)
                        {
                            CreatePostEntity(virtualPost, tvi, "MoneyAssign");
                        }
                    }
                }
            });
            #endregion
        }

        //处理默认活动经费岗位的设置
        public void BeginMoneyAssign(DetailGrid grid)
        {
            OrderEntity oe = grid.DataContext as OrderEntity;

            oe.FBEntity.CollectionEntity.ForEach(p =>
            {
                p.FBEntities.ForEach(item =>
                {
                    //活动经费处理初始化
                    var v = item.Entity as T_FB_SUBJECTDEPTMENT;
                    if (v != null && v.T_FB_SUBJECT != null && v.T_FB_SUBJECT.SUBJECTID == DataCore.SystemSetting.MONEYASSIGNSUBJECTID)
                    {
                        DeatilEntity_PropertyChanged(item.Entity, null);
                    }
                    var s = item.Entity as T_FB_SUBJECTPOST;
                    if (s != null && s.T_FB_SUBJECT != null && s.T_FB_SUBJECT.SUBJECTID == DataCore.SystemSetting.MONEYASSIGNSUBJECTID)
                    {
                        DeatilEntity_PropertyChanged(item.Entity, null);
                    }

                    item.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(DeatilEntity_PropertyChanged);
                });
            });
        }

        void DeatilEntity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //岗位科目
            T_FB_SUBJECTPOST subjectPost = sender as T_FB_SUBJECTPOST;
            if (subjectPost != null)
            {
                if (subjectPost.T_FB_SUBJECT != null && subjectPost.T_FB_SUBJECT.SUBJECTID == DataCore.SystemSetting.MONEYASSIGNSUBJECTID)
                {
                    if (subjectPost.ACTIVED != 1 || subjectPost.ISPERSON != 1)
                    {
                        ComfirmWindow.ConfirmationBoxs("提示", "活动经费在此只能是启用并勾选分配个人，并且修改无效。", Utility.GetResourceStr("CONFIRM"), MessageIcon.Question);
                    }
                    sender.SetObjValue("ACTIVED", 1);
                    sender.SetObjValue("ISPERSON", 1);
                }
            }
            //部门科目
            T_FB_SUBJECTDEPTMENT subjectDepartment = sender as T_FB_SUBJECTDEPTMENT;
            if (subjectDepartment != null)
            {
                if (subjectDepartment.T_FB_SUBJECT != null && subjectDepartment.T_FB_SUBJECT.SUBJECTID == DataCore.SystemSetting.MONEYASSIGNSUBJECTID)
                {
                    if (subjectDepartment.ACTIVED != 1 || subjectDepartment.ISPERSON != 1)
                    {
                        ComfirmWindow.ConfirmationBoxs("提示", "活动经费在此只能是启用并勾选分配个人，并且修改无效。", Utility.GetResourceStr("CONFIRM"), MessageIcon.Question);
                    }
                    sender.SetObjValue("ACTIVED", 1);
                    sender.SetObjValue("ISPERSON", 1);
                }
            }
        }

        //岗位默认处理
        public T_FB_SUBJECTPOST SubjectPostChanged(T_FB_SUBJECTDEPTMENT sd, T_FB_SUBJECTPOST post, string strFlag)
        {
            if (strFlag == "ACTIVED")  //ACTIVED 用于区别启用事件点击和树的点击
            {
                if (sd.ACTIVED == 1)
                    post.ACTIVED = 1;
                else
                    post.ACTIVED = 0;

                if (sd.ISPERSON == 1)
                    post.ISPERSON = 1;
                else
                    post.ISPERSON = 0;
            }
            else if (strFlag == "MoneyAssign")
            {
                post.ACTIVED = 1;
                post.ISPERSON = 1;
            }

            return post;
        }

        public OrderEntity IsPersonChanged(OrderEntity oe, T_FB_SUBJECTDEPTMENT subjectDepartment)
        {
            if (subjectDepartment.ISPERSON == 1)
            {
                oe.FBEntity.CollectionEntity.ForEach(p =>
                {
                    p.FBEntities.ForEach(f =>
                    {
                        T_FB_SUBJECTPOST post = f.Entity as T_FB_SUBJECTPOST;
                        if (post != null && subjectDepartment.T_FB_SUBJECT.SUBJECTID == post.T_FB_SUBJECT.SUBJECTID)
                            post.ISPERSON = 1;
                    });
                });
            }
            else
            {
                oe.FBEntity.CollectionEntity.ForEach(p =>
                {
                    p.FBEntities.ForEach(f =>
                    {
                        T_FB_SUBJECTPOST post = f.Entity as T_FB_SUBJECTPOST;
                        if (post != null && subjectDepartment.T_FB_SUBJECT.SUBJECTID == post.T_FB_SUBJECT.SUBJECTID)
                            post.ISPERSON = 0;
                    });
                });
            }

            return oe;
        }
    }
}
