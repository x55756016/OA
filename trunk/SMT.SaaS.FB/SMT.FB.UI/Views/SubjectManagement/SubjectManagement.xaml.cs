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
using System.Windows.Data;
using System.Xml.Linq;
using System.Xml;
using System.Windows.Markup;
using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.Common;
using System.Windows.Media.Imaging;

namespace SMT.FB.UI.Views.SubjectManagement
{
    public partial class SubjectManagement : FBBasePage
    {
        public SubjectManagement()
        {
            InitializeComponent();

            this.FBBasePageLoaded += new EventHandler(SubjectManagement_FBBasePageLoaded);
        }

        void SubjectManagement_FBBasePageLoaded(object sender, EventArgs e)
        {
            InitForm();
        }

       
        #region Fields
        private IList<OrderEntity> EntityList;
        private Dictionary<EntityObject, OrderEntity> dictTreeViewItem;

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

        private DataTemplate subjectTypeTemplate = null;
        private DataTemplate subjectTemplate = null;

        #endregion

        private void InitForm()
        {
            
            this.FormTitleName.TextTitle.Text = "公司科目维护管理";
            InitToolBar();
            orderEntityService = new OrderEntityService();
            orderEntityService.ModelCode = typeof(T_FB_SUBJECTTYPE).Name;
            orderEntityService.SaveListCompleted += new EventHandler<ActionCompletedEventArgs<bool>>(orderEntityService_SaveListCompleted);
            orderEntityService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(orderEntityService_QueryFBEntitiesCompleted);
            EntityList = new List<OrderEntity>();
            forms = new ObjectList<EditForm>();
            dictTreeViewItem = new Dictionary<EntityObject, OrderEntity>();

            this.FormTitleName.Visibility = FBBasePage.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;

            subjectTypeTemplate = Resources["tvItemSubjectTypeTemplate"] as DataTemplate;
            subjectTemplate = Resources["tvItemSubjectTemplate"] as DataTemplate;
            InitData();

            
        }

        private void InitToolBar()
        {
            List<ToolbarItem> list = new List<ToolbarItem>();


            ToolbarItem item = ToolBarItems.Save;
            list.Add(item);

             item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "AddType",
                    Title = "添加科目类型",
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
                };
            list.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "AddSubject",
                Title = "添加同级科目",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
            };
            list.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "AddSubSubject",
                Title = "添加子级科目",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
            };
            list.Add(item);
            
            item = ToolBarItems.Delete;
            list.Add(item);

            tooBarTop.InitToolBarItem(list);
            tooBarTop.ItemClicked += new EventHandler<ToolBar.ToolBarItemClickArgs>(tooBarTop_ItemClicked);
            
        }

        

        void orderEntityService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            this.TreeView.Items.Clear();
            List<OrderEntity> list = e.Result.ToEntityAdapterList<OrderEntity>().ToList();
            list.ForEach(order => { EntityList.Add(order); });
            List<TreeViewItem> items = TreeView.Items.AddObjectList(list, subjectTypeTemplate);
            
            items.ForEach(treeViewItem =>
            {
                treeViewItem.Expanded += new RoutedEventHandler(treeViewItem_Expanded);
                treeViewItem.Items.AddTempLoadingItem();
            });

            TreeViewItem tvi = this.TreeView.Items.FirstOrDefault() as TreeViewItem;
            if (tvi != null)
            {
                tvi.IsSelected = true;

                RoutedPropertyChangedEventArgs<object> ea = new RoutedPropertyChangedEventArgs<object>(null, tvi);
                TreeView_SelectedItemChanged(tvi, ea);
            }
        }

        void orderEntityService_SaveListCompleted(object sender, ActionCompletedEventArgs<bool> e)
        {
            if (e.Result)
            {

                InitData();
                CommonFunction.NotifySelection("保存成功");
                CloseProcess();
            }
        }

        protected void InitData()
        {
            EntityList.Clear();
            
            dictTreeViewItem.Clear();

            QueryExpression qe = new QueryExpression();
            qe.QueryType = typeof(T_FB_SUBJECTTYPE).Name;
            orderEntityService.QueryFBEntities(qe);
            this.TreeView.Items.AddTempLoadingItem();
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

        private DataTemplate GetDataTemplete(string name)
        {
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XElement xDataTemplate =
            new XElement(ns + "DataTemplate", new XAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"),
            new XElement(ns + "TextBlock", new XAttribute("Text", "{Binding Mode=TwoWay, Path=" + name + "}"))
            );

            //将内存中的XAML实例化成为DataTemplate对象，并赋值给
            //ListBox的ItemTemplate属性，完成数据绑定
            XmlReader xr = xDataTemplate.CreateReader();
            DataTemplate dataTemplate = XamlReader.Load(xDataTemplate.ToString()) as DataTemplate;
            return dataTemplate;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem treeViewItem = e.NewValue as TreeViewItem;
            if (treeViewItem != null)
            {

                this.currentTreeViewItem = treeViewItem;
                this.CurrentOrderEntity = currentTreeViewItem.DataContext as OrderEntity;

                if (this.CurrentOrderEntity.OrderType == typeof(T_FB_SUBJECTTYPE))
                {
                    this.treeViewItem_Expanded(this.currentTreeViewItem, null);
                }
            }

        }

        private void treeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem curItem = sender as TreeViewItem;
            OrderEntity orderEntity = curItem.DataContext as OrderEntity;

            if (!dictTreeViewItem.ContainsKey(orderEntity.Entity))
            {
                curItem.Items.Clear();
                List<OrderEntity> list = orderEntity.CollectionEntity[0].FBEntities.ToEntityAdapterList<OrderEntity>().ToList();
                dictTreeViewItem.Add(orderEntity.Entity, orderEntity);
                AttachSubject(list, curItem);
               

            }
        }

        private void AttachSubject(List<OrderEntity> list, TreeViewItem treeViewItem)
        {
            AttachSubject(list, null, treeViewItem);
        }

        private void AttachSubject(List<OrderEntity> list, OrderEntity orderEntity, TreeViewItem treeViewItem)
        {
            List<OrderEntity> result = null;
            if (orderEntity != null)
            {
                result = list.Where(item =>
                {
                    T_FB_SUBJECT subjectSource = item.Entity as T_FB_SUBJECT;
                    T_FB_SUBJECT subjectTarget = orderEntity.Entity as T_FB_SUBJECT;
                    if (subjectSource.T_FB_SUBJECT2 != null)
                    {
                        return subjectSource.T_FB_SUBJECT2.SUBJECTID == subjectTarget.SUBJECTID;
                    }
                    return false;
                }).ToList<OrderEntity>();
            }
            else
            {
                result = (from item in list
                          where (item.Entity as T_FB_SUBJECT).T_FB_SUBJECT2 == null
                          select item).ToList<OrderEntity>();
            }


            if (result.Count > 0)
            {
                foreach (OrderEntity oEntity in result)
                {
                    
                    TreeViewItem objTreeNode = treeViewItem.Items.AddObject<OrderEntity>(oEntity, subjectTemplate);
                    
                    AttachSubject(list, oEntity, objTreeNode);
                }
            }
        }


        #region ToolBar

        void tooBarTop_ItemClicked(object sender, ToolBar.ToolBarItemClickArgs e)
        {
            switch (e.Key)
            {
                case "AddType" :
                    AddType();
                    break;
                case "AddSubject":
                    AddSubject();
                    break;
                case "AddSubSubject":
                    AddSubSubject();
                    break;
                case "Save":
                    try
                    {
                        ShowProcess();
                        bool isCheck = Save();

                        if ( !isCheck)
                        {
                            CloseProcess();
                        }
                    }
                    catch ( Exception ex)
                    {
                        CommonFunction.ShowErrorMessage(ex.Message);
                        CloseProcess();
                    }
                    break;
                case "Delete":
                    Delete();
                    break;
            }

        }

        private void AddType()
        {
            OrderEntity orderEntityNew = new OrderEntity(typeof(T_FB_SUBJECTTYPE));
            EntityList.Add(orderEntityNew);
            this.dictTreeViewItem.Add(orderEntityNew.Entity, orderEntityNew);
            orderEntityNew.SetObjValue("Entity.SUBJECTTYPENAME", GetName(orderEntityNew.Entity, "类别"));
            orderEntityNew.SetObjValue("Entity.EDITSTATES", "1");
            this.currentTreeViewItem = this.TreeView.Items.AddObject<OrderEntity>(orderEntityNew, subjectTypeTemplate);
            currentTreeViewItem.IsSelected = true;
            this.CurrentOrderEntity = orderEntityNew;
            
        }

        private void AddSubject()
        {

            AddSubject(false);

        }

        private void AddSubSubject()
        {
            AddSubject(true);
        }

        private void AddSubject(bool isSubSubject)
        {
            TreeViewItem parentTreeViewItem = this.currentTreeViewItem;
            object parentOrderEntity = CurrentOrderEntity.Entity;
            object subType = CurrentOrderEntity.Entity;
            
            if (CurrentOrderEntity.OrderType == typeof(T_FB_SUBJECT))
            {
                subType = CurrentOrderEntity.GetObjValue("Entity.T_FB_SUBJECTTYPE");

                if (!isSubSubject)
                {
                    parentOrderEntity = CurrentOrderEntity.GetObjValue("Entity.T_FB_SUBJECT2");
                    parentTreeViewItem = currentTreeViewItem.Parent as TreeViewItem;
                }
            }
            else
            {
                parentOrderEntity = null;
            }
            OrderEntity oeSubjectType = dictTreeViewItem[subType as EntityObject];
            FBEntity subjectNew = oeSubjectType.CreateFBEntity<T_FB_SUBJECT>();
            OrderEntity orderEntityNew = new OrderEntity(subjectNew);

            orderEntityNew.SetObjValue("Entity.SUBJECTID", Guid.NewGuid().ToString());
            orderEntityNew.SetObjValue("Entity.T_FB_SUBJECTTYPE", subType);
            orderEntityNew.SetObjValue("Entity.T_FB_SUBJECT2", parentOrderEntity);
            orderEntityNew.SetObjValue("Entity.SUBJECTNAME", GetName(orderEntityNew.Entity, "科目"));
            orderEntityNew.SetObjValue("Entity.EDITSTATES", "1");


            this.currentTreeViewItem = parentTreeViewItem.Items.AddObject<OrderEntity>(orderEntityNew, subjectTemplate);

            parentTreeViewItem.IsExpanded = true;
            currentTreeViewItem.IsSelected = true;
            this.CurrentOrderEntity = orderEntityNew;
        }

        private void Delete()
        {
            if (currentTreeViewItem.HasItems)
            {
                CommonFunction.NotifySelection("当前记录有子结点,不能被删除");
                return;
            }

            Action action = () =>
            {
                if (this.CurrentOrderEntity.FBEntityState == FBEntityState.Added)
                {
                    OrderEntity orderEntity = this.EntityList.FirstOrDefault(item => { return object.Equals(item, this.currentOrderEntity); });
                    if (orderEntity != null)
                    {
                        this.EntityList.Remove(orderEntity);
                    }
                    else
                    {
                        FBEntity fbEntity = this.CurrentOrderEntity.FBEntity;
                        T_FB_SUBJECT subject = fbEntity.Entity as T_FB_SUBJECT;
                        this.dictTreeViewItem[subject.T_FB_SUBJECTTYPE].DeleteFBEntity(fbEntity);
                    }
                    this.CurrentOrderEntity = null;
                }
                else
                {
                    this.CurrentOrderEntity.FBEntityState = FBEntityState.Deleted;
                }

                Type type = this.currentTreeViewItem.Parent.GetType();
                PropertyInfo p = type.GetProperty("Items");
                ItemCollection items = (ItemCollection)p.GetValue(this.currentTreeViewItem.Parent, null);

                //beyond
                TreeViewItem tempcurrentTreeViewItem=currentTreeViewItem.Parent as TreeViewItem;
               
                items.Remove(this.currentTreeViewItem);

                //beyond this.currentTreeViewItem
                if (items.Count > 0)
                {
                    this.currentTreeViewItem = items[0] as TreeViewItem;
                }
                else
                {
                    this.currentTreeViewItem = tempcurrentTreeViewItem;    
                }

                this.CurrentOrderEntity = currentTreeViewItem.DataContext as OrderEntity;

                //  this.CurrentEditForm.Save();
            };
            CommonFunction.AskDelete(string.Empty, action);
        }

        private bool Save()
        {
            
            List<OrderEntity> listSave = new List<OrderEntity>();
            for (int i = 0; i < EntityList.Count; i++)
            {
                OrderEntity orderEntity = EntityList[i];
                if (orderEntity.FBEntity.FBEntityState != FBEntityState.Unchanged)
                {
                    orderEntity.SetObjValue("Entity.UPDATEDATE", DateTime.Now);
                    orderEntity.SetObjValue("Entity.UPDATEUSERID", DataCore.CurrentUser.Value);
                    listSave.Add(orderEntity);
                }
            }
            if (listSave.Count > 0)
            {
                ObservableCollection<FBEntity> listSaveNew = new ObservableCollection<FBEntity>();
                listSave.ForEach(orderEntity =>
                {
                    listSaveNew.Add(orderEntity.GetModifiedFBEntity());
                });
                if (CheckData(listSaveNew))
                {
                    orderEntityService.SaveList(listSaveNew);
                    return true;
                }
                else
                {
                    CommonFunction.ShowMessage("保存失败", "数据验证不通过，请将鼠标移到左侧科目，在对应科目的感叹号上查看具体错误原因", CommonFunction.MessageType.Attention);
                    return false;
                }
            }
            else
            {
                return false;
            }
            
        }

        #endregion


        public override bool CheckPermisstion()
        {
            int perm = PermissionHelper.GetPermissionValue(typeof(T_FB_SUBJECTTYPE).Name, Permissions.Browse);

            // 需要大于等公司的范围权限
            return !(perm > (int)PermissionRange.Employee || perm < 0);

        }

        #region 检查 名称，编码是否为空，重复
        //beyond
        string errFirst = "";

        public bool CheckData(ObservableCollection<FBEntity> listSaveNew)
        {
            bool pass = true;
            #region old
            //listSaveNew.ToList().ForEach(item =>
            //    {
            //        // 要删除的实体
            //        if (item.FBEntityState == EntityState.Deleted)
            //        {
            //            return;
            //        }
            //        T_FB_SUBJECTTYPE entitySubjectType = item.Entity as T_FB_SUBJECTTYPE;

            //        pass &= SetErrorFlag(entitySubjectType, CheckSubjectType(entitySubjectType));
                    
            //        var subjects = item.GetRelationFBEntities(typeof(T_FB_SUBJECT).Name);
            //        subjects.ToList().ForEach(fbEntitySubject =>
            //            {
            //                // 要删除的实体
            //                if (fbEntitySubject.FBEntityState == EntityState.Deleted)
            //                {
            //                    return;
            //                }
            //                T_FB_SUBJECT entitySubject = fbEntitySubject.Entity as T_FB_SUBJECT;
            //                pass &= SetErrorFlag(entitySubject, CheckSubject(entitySubject));
            //            });
            //    });
            #endregion 

            #region beyond
            listSaveNew.ToList().ForEach(item =>
            {
                // 要删除的实体
                if (item.FBEntityState == FBEntityState.Deleted)
                {
                    return;
                }
                T_FB_SUBJECTTYPE entitySubjectType = item.Entity as T_FB_SUBJECTTYPE;

                pass &= SetErrorFlag(entitySubjectType, CheckSubjectType(entitySubjectType));
                if (!pass)
                {
                    return;
                }
                var subjects = item.GetRelationFBEntities(typeof(T_FB_SUBJECT).Name);
                subjects.ToList().ForEach(fbEntitySubject =>
                {
                    // 要删除的实体
                    if (fbEntitySubject.FBEntityState == FBEntityState.Deleted)
                    {
                        return;
                    }
                    T_FB_SUBJECT entitySubject = fbEntitySubject.Entity as T_FB_SUBJECT;
                    pass &= SetErrorFlag(entitySubject, CheckSubject(entitySubject));
                    if (!pass)
                    {
                        return;
                    }
                });
            });

            #endregion 
            return pass;
        }

        private bool SetErrorFlag(EntityObject entity, List<string> errMsgs)
        {

            TreeViewItem treeViewItem = GetItem(entity);
            OrderEntity oe = treeViewItem.DataContext as OrderEntity;
            oe.IsErrorData = errMsgs.Count > 0;
            oe.ErrorMessage = errMsgs;
            
            //　展开所有的父节点
            if (oe.IsErrorData)
            {
                TreeViewItem parentItem = treeViewItem.GetParentTreeViewItem();
                while (parentItem != null)
                {
                    parentItem.IsExpanded = true;
                    parentItem = parentItem.GetParentTreeViewItem();
                }
            }

            return !oe.IsErrorData;
        }

        private List<string> CheckSubjectType(T_FB_SUBJECTTYPE entitySubjectType)
        {
            
            #region 处理 SubjectType
            List<string> errMsg = new List<string>();

            string name = entitySubjectType.SUBJECTTYPENAME;
            if (string.IsNullOrEmpty(name))
            {
                errMsg.Add("科目类别名称不能为空");

                //beyond
                errFirst = "科目类别名称不能为空";
            }
            
            var otherEntity = this.EntityList.FirstOrDefault(orderEntity =>
            {
                EntityObject entity = orderEntity.Entity;
                bool isRightType = entity.GetType().Name == "T_FB_SUBJECTTYPE";
                bool isNotItselft = !object.Equals(entity, entitySubjectType);
                bool isSameName = name == (entity as T_FB_SUBJECTTYPE).SUBJECTTYPENAME;

                return isRightType && isNotItselft && isSameName;
            });
            if (otherEntity != null)
            {
                errMsg.Add("科目类别名称已存在，不可重复");
                errFirst = "科目类别名称已存在，不可重复";
            }
            #endregion
            return errMsg;
        }

        private List<string> CheckSubject(T_FB_SUBJECT entitySubject)
        {
            #region 处理 Subject
            List<string> errMsg = new List<string>();

            string code = entitySubject.SUBJECTCODE;
            if (string.IsNullOrEmpty(code))
            {
                errMsg.Add("科目编号不能为空");
            }

            string name = entitySubject.SUBJECTNAME;
            if (string.IsNullOrEmpty(name))
            {
                errMsg.Add("科目类别不能为空");
            }

             foreach (OrderEntity orderEntity in this.EntityList)
             {
                 if (errMsg.Count > 0)
                 {
                     break;
                 }
                var subjects = orderEntity.GetRelationFBEntities(typeof(T_FB_SUBJECT).Name);

                #region 科目名称
                var subjectFind = subjects.FirstOrDefault(fbEntity =>
                {
                    T_FB_SUBJECT entity = fbEntity.Entity as T_FB_SUBJECT;
                    bool isRightType = entity.GetType().Name == "T_FB_SUBJECT";
                    bool isNotItselft = !object.Equals(entity, entitySubject);
                    bool isSameName = name == entity.SUBJECTNAME;

                    return isRightType && isNotItselft && isSameName;
                });

                if ( subjectFind != null)
                {
                    errMsg.Add("科目名称已存在，不可重复");
                }
                #endregion

                #region 科目编号
                var subjectFind2 = subjects.FirstOrDefault(fbEntity =>
                {
                    T_FB_SUBJECT entity = fbEntity.Entity as T_FB_SUBJECT;
                    bool isRightType = entity.GetType().Name == "T_FB_SUBJECT";
                    bool isNotItselft = !object.Equals(entity, entitySubject);
                    bool isSameCode = code == entity.SUBJECTCODE;

                    return isRightType && isNotItselft && isSameCode;
                });

                if ( subjectFind2 != null)
                {
                    errMsg.Add("科目编号已存在，不可重复");
                }
                #endregion
            }
            #endregion

            return errMsg;
        }

        private TreeViewItem GetItem(EntityObject entity)
        {
            
            ItemCollection cs = this.TreeView.Items;
            List<ItemCollection> listCS = new List<ItemCollection>();
            return GetItem(entity, cs);
        }

        private TreeViewItem GetItem(EntityObject entity, ItemCollection cs)
        {
             foreach (object item in cs)
            {
                TreeViewItem result = null;
                TreeViewItem itemTree = (item as TreeViewItem);
                EntityObject entityTemp = (itemTree.DataContext as OrderEntity).Entity;

                if (object.Equals(entityTemp, entity))
                {
                    result = (item as TreeViewItem);
                    
                }
                else
                {
                    result = GetItem(entity, itemTree.Items);
                }

                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        #endregion

        #region 获取生动生成的名称
        private string GetName ( EntityObject entity, string preName)
        {
           
            int i = 1;
            string tempName = preName + " " + i.ToString();
            
            Func<OrderEntity, bool> funFindType = item =>
                {
                    T_FB_SUBJECTTYPE sType = item.Entity as T_FB_SUBJECTTYPE;
                    return sType.SUBJECTTYPENAME == tempName && entity != item.Entity;
                };

            Func<OrderEntity, bool> funFindSubject = item =>
                {
                    T_FB_SUBJECTTYPE sType = item.Entity as T_FB_SUBJECTTYPE;

                    var subjects = item.GetRelationFBEntities(typeof(T_FB_SUBJECT).Name);
                    var subjectFind =subjects.FirstOrDefault( subject =>
                        {
                            return (subject.Entity as T_FB_SUBJECT).SUBJECTNAME == tempName 
                                && entity != subject;
                        });
                    return subjectFind != null;
                };

            Func<OrderEntity, bool> funFind = null;

             if ( entity.GetType() == typeof(T_FB_SUBJECTTYPE))
             {
                funFind = funFindType;
             }
             else
             {
                 funFind = funFindSubject;
             }
            bool isNotOK = true;
            do 
            {
                var entityFind = EntityList.FirstOrDefault(funFind);
                if ( entityFind != null)
                {
                    isNotOK = true;
                    i++;
                    tempName = preName + " " + i.ToString();
                }
                else
                {
                    isNotOK = false;
                }
            }
            while (isNotOK);
            return tempName;
        }
        #endregion
    }

  
}
