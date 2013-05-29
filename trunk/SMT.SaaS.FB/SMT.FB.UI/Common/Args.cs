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
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SMT.FB.UI.FBCommonWS;
using System.Collections;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq;
using SMT.SaaS.FrameworkUI;
using System.Windows.Media.Imaging;

namespace SMT.FB.UI.Common
{
    public class Args
    {

        public const string DELETE_ENTITYTYPE = "DELETE_ENTITYTYPE";

       
    }
    public  class FieldDisplayName
    {
        public const string YearUsableMoney = "年度结余";
        public const string MonthUsableMoney = "月度结余";
        public const string UsableMoney = "可用额度";
        public const string BudgetMoney = "费用预算";

        public const string AssignMoney = "下拨金额";
        public const string BorrowMoney = "借款金额";
        public const string RepayMoney = "还款金额";
        public const string ChargeMoney = "报销金额";

        public const string TransMoney = "已分配金额";

        public const string PersonMoney = "个人预算";
        public const string DeptMoney = "部门预算";
    }

    public class ErrorMessage
    {
        public const string Err_ComCheck1 = "总预算资金必需等于总分配资金!";
        public const string MoneyZero = "科目：{0} 的{1}不能小于零!";
        public const string Err_ComCheck2 = "回收资金不能大于已分配资金!";

        public static string TransMoneyZero
        {
            get
            {
                return string.Format(MoneyZero, "{0}", FieldDisplayName.TransMoney);
            }
        }
        public static string BudgetMoneyZero
        {
            get
            {
                return string.Format(MoneyZero, "{0}", FieldDisplayName.BudgetMoney);
            }
        }
        public static string BorrowMoneyZero
        {
            get
            {
                return string.Format(MoneyZero, "{0}", FieldDisplayName.BorrowMoney);
            }
        }
        public static string RepayMoneyZero
        {
            get
            {
                return string.Format(MoneyZero, "{0}", FieldDisplayName.RepayMoney);
            }
        }
        public static string ChargeMoneyZero
        {
            get
            {
                return string.Format(MoneyZero, "{0}", FieldDisplayName.ChargeMoney);
            }
        }
        public static string AssignMoneyZero
        {
            get
            {
                return string.Format(MoneyZero, "{0}", FieldDisplayName.AssignMoney);
            }
        }

        public static string MoneyBigger 
        {
            get
            {
                return string.Format("科目：{0} 的{1}", "{2}", CommonFunction.GetString("DATEGREATERERROR"));
            }
        }
        public static string TransMoneyBigger
        {
            get
            {
                return string.Format(MoneyBigger, FieldDisplayName.TransMoney, FieldDisplayName.MonthUsableMoney, "{0}");
            }
        }
        public static string BudgetMoneyBigger
        {
            get
            {
                return string.Format(MoneyBigger, FieldDisplayName.BudgetMoney, FieldDisplayName.MonthUsableMoney, "{0}");
            }
        }
        public static string BudgetYearMoneyBigger
        {
            get
            {
                return string.Format(MoneyBigger, FieldDisplayName.BudgetMoney, FieldDisplayName.YearUsableMoney, "{0}");
            }
        }

        public static string PersonBudgetMoneyBigger
        {
            get
            {
                return string.Format(MoneyBigger, FieldDisplayName.PersonMoney, FieldDisplayName.DeptMoney, "{0}");
            }
        }

        public static string BorrowMoneyBigger
        {
            get
            {
                return string.Format(MoneyBigger, FieldDisplayName.BorrowMoney, FieldDisplayName.UsableMoney, "{0}");
            }
        }
        public static string RepayMoneyBigger
        {
            get
            {
                return string.Format(MoneyBigger, FieldDisplayName.RepayMoney, FieldDisplayName.BorrowMoney, "{0}");
            }
        }
        public static string ChargeMoneyBigger
        {
            get
            {
                return string.Format(MoneyBigger, FieldDisplayName.ChargeMoney, FieldDisplayName.UsableMoney, "{0}");
            }
        }

        public static string NoDetailInfo
        {
            get
            {
                return "保存失败,请先添加单据明细!";
            }
        }

        public static string OverDetailInfo
        {
            get
            {
                return "保存失败,单据明细最多只能5条!";
            }
        }
    }
    ///// <summary>
    ///// 审批状态
    ///// </summary>
    //public enum CheckStates
    //{
    //    /// <summary>
    //    /// 删除
    //    /// </summary>
    //    Delete = -1,
    //    /// <summary>
    //    /// 待审批
    //    /// </summary>
    //    UnSubmit = 0,
    //    /// <summary>
    //    /// 审核中
    //    /// </summary>
    //    Approving,
    //    /// <summary>
    //    /// 审核通过
    //    /// </summary>
    //    Approved,
    //    /// <summary>
    //    /// 审核不通过
    //    /// </summary>
    //    UnApproved,

    //    /// <summary>
    //    /// 保存
    //    /// </summary>
    //    Saved
    //}

    
    public struct EntityFieldName
    {

        public const string OwnerID = "Entity.OWNERID";
        public const string OwnerPostID = "Entity.OWNERPOSTID";
        public const string OwnerDepartmentID = "Entity.OWNERDEPARTMENTID";
        public const string OwnerCompanyID = "Entity.OWNERCOMPANYID";

        public const string CreateUserID = "Entity.CREATEUSERID";
        public const string CreatePostID = "Entity.CREATEPOSTID";
        public const string CreateDepartmentID = "Entity.CREATEDEPARTMENTID";
        public const string CreateCompanyID = "Entity.CREATECOMPANYID";

        public const string UpdateUserID = "Entity.UPDATEUSERID";
        public const string UpdateDate = "Entity.UPDATEDATE";
        public const string CreateDate = "Entity.CREATEDATE";

        public const string EditStates = "Entity.EDITSTATES";
        public const string CheckStates = "Entity.CHECKSTATES";

        public const string OwnerName = "Entity.OWNERNAME";
        public const string OwnerPostName = "Entity.OWNERPOSTNAME";
        public const string OwnerDepartmentName = "Entity.OWNERDEPARTMENTNAME";
        public const string OwnerCompanyName = "Entity.OWNERCOMPANYNAME";

    }

    public struct FieldName
    {

        public const string OwnerID = "OWNERID";
        public const string OwnerPostID = "OWNERPOSTID";
        public const string OwnerDepartmentID = "OWNERDEPARTMENTID";
        public const string OwnerCompanyID = "OWNERCOMPANYID";

        public const string CreateUserID = "CREATEUSERID";
        public const string CreatePostID = "CREATEPOSTID";
        public const string CreateDepartmentID = "CREATEDEPARTMENTID";
        public const string CreateCompanyID = "CREATECOMPANYID";

        public const string UpdateUserID = "UPDATEUSERID";
        public const string UpdateDate = "UPDATEDATE";
        public const string CreateDate = "CREATEDATE";

        public const string EditStates = "EDITSTATES";
        public const string CheckStates = "CHECKSTATES";

    }
    public enum ControlType
    {
        Label, TextBox, Combobox = 2,
        LookUp, Remark, DatePicker = 5,
        CheckBox, DateTimePicker = 7, TreeViewItem,
        // Add By LVCHAO 2011.01.30 14:46
        HyperlinkButton = 8
    }

   

    public enum CompareResult
    {
        Equal, Bigger, Less, None
    }

    public enum OperationTypes
    {
        Add,
        Edit,
        Delete,
        Browse,
        Export,
        Report,
        Audit,
        Import,
        ReSubmit
    }
    public enum Actions
    {
        Save, SaveAndClose, Delete, Cancel, Add, Update, NoAction
    }


    public class EntityChangedArgs : EventArgs
    {
        public PropertyChangedEventArgs ChangedEventArgs { get; set; }
        public Actions Action { get; set; }

        public EntityChangedArgs(Actions action)
        {
            Action = action;
            ChangedEventArgs = new PropertyChangedEventArgs("");

        }
        public EntityChangedArgs(PropertyChangedEventArgs args)
        {
            Action = Actions.Update;
            ChangedEventArgs = args;
        }
    }
    public class SavingEventArgs : EventArgs
    {
        [DefaultValue(false)]
        public Actions Action { get; set; }
        public FBEntity SaveFBEntity { get; set; }
        public SavingEventArgs()
        {

        }

        public SavingEventArgs(FBEntity fbEntity)
        {
            this.SaveFBEntity = fbEntity;
        }

        public SavingEventArgs(Actions action)
        {
            Action = action;
            
        }
    }

    public class OrderPropertyChangedArgs : ActionCompletedEventArgs<List<string>>
    {
        public OrderPropertyChangedArgs(List<string> list)
            : base(list)
        {
        }
    }
   
    public class ActionCompletedEventArgs<T> : EventArgs
    {
        private T result;
        public T Result
        {
            get
            {
                return result;
            }
        }
        public ActionCompletedEventArgs(T t)
        {
            this.result = t;
        }
    }

    public class ToolBarItemClickEventArgs : EventArgs
    {

        public Actions Action { get; set; }
        public ToolBarItemClickEventArgs(Actions action)
        {
            Action = action;
        }

    }


    public static class StringExtension
    {
        public static string ToEntityString(this string fieldName)
        {
            if (!string.IsNullOrEmpty(fieldName))
            {
                return "Entity." + fieldName;
            }
            return fieldName;
        }
    }
    public static class TreeViewExtension
    {
        public static TreeViewItem AddObject<T>(this PresentationFrameworkCollection<object> items, T t, string namePath)
        {
            TreeViewItem treeViewItem = new TreeViewItem();

            Binding binding = new Binding();

            binding.Path = new PropertyPath(namePath);
            treeViewItem.SetBinding(TreeViewItem.HeaderProperty, binding);

            treeViewItem.DataContext = t;
            items.Add(treeViewItem);
            return treeViewItem;
        }
        public static List<TreeViewItem> AddObjectList<T>(this PresentationFrameworkCollection<object> items, List<T> listofT, string namePath)
        {
            List<TreeViewItem> treeViewItemList = new List<TreeViewItem>();
            listofT.ForEach(t =>
            {
                TreeViewItem treeViewItem = items.AddObject<T>(t, namePath);
                treeViewItemList.Add(treeViewItem);

            });
            return treeViewItemList;
        }

        public static TreeViewItem AddObject<T>(this PresentationFrameworkCollection<object> items, T t, DataTemplate template)
        {
            TreeViewItem treeViewItem = new TreeViewItem();

            treeViewItem.HeaderTemplate = template;
            treeViewItem.Header = t;
            treeViewItem.DataContext = t;
            treeViewItem.Loaded += new RoutedEventHandler(treeViewItem_Loaded);
            items.Add(treeViewItem);
            return treeViewItem;
        }

        static void treeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            object a = e;
        }
        public static List<TreeViewItem> AddObjectList<T>(this PresentationFrameworkCollection<object> items, List<T> listofT, DataTemplate template)
        {
            List<TreeViewItem> treeViewItemList = new List<TreeViewItem>();
            listofT.ForEach(t =>
            {
                TreeViewItem treeViewItem = items.AddObject<T>(t, template);
                treeViewItemList.Add(treeViewItem);

            });
            return treeViewItemList;
        }
       

        public static void AddTempLoadingItem(this PresentationFrameworkCollection<Object> items)
        {
            items.Clear();
            TreeViewItem tvILoading = new TreeViewItem();
            tvILoading.Header = "加载中...";
            tvILoading.IsSelected = false;
            items.Add(tvILoading);
        }
    }

    public static class ListExtension
    {
        public static List<FBEntity> ToFBEntityList<T>(this IList<T> list) where T : EntityObject
        {
            List<FBEntity> listResult = new List<FBEntity>();
            if (list == null)
            {
                return listResult;
            }

            for (int i = 0; i < list.Count; i++)
            {
                T t = (T)list[i];
                if (t != null)
                {
                    listResult.Add(t.ToFBEntity());
                }
            }
            return listResult;
        }

        public static List<T> ToObjectList<T>(this IList list)
        {
            List<T> listResult = new List<T>();
            if (list == null)
            {
                return listResult;
            }

            for (int i = 0; i < list.Count; i++)
            {
                T t = (T)list[i];
                if (t != null)
                {
                    listResult.Add(t);
                }
            }
            return listResult;
        }

        public static bool RemoveAll<T>(this IList<T> list, Func<T, bool> predicate)
        {
            bool result = true;

            T[] itemsRemove = list.FindAll(predicate).ToArray();
            for (int i = 0; i < itemsRemove.Length; i++)
            {
                result &= list.Remove(itemsRemove[i]);
            }
            return result;
        }

        public static List<T> FindAll<T>(this IList<T> list, Func<T, bool> predicate)
        {
            var itemsWhere = list.Where(predicate);
            return itemsWhere.ToList();
        }

        public static List<TResult> CreateList<T, TResult>(this List<T> listSource, Func<T, TResult> func)
        {
            List<TResult> listResult = new List<TResult>();
            listSource.ForEach(item =>
            {
                TResult tr = func(item);
                listResult.Add(tr);
            });
            return listResult;
        }

        public static ObservableCollection<T> ToEntityAdapterList<T>(this IList list) where T : EntityAdapter
        {

            ObservableCollection<T> listOE = new ObservableCollection<T>();

            for (int i = 0; i < list.Count; i++)
            {
                T t = default(T);
                if (list[i].GetType() == typeof(OrderEntity))
                {
                    t = (T)list[i];
                }
                else
                {
                    t = (T)Activator.CreateInstance(typeof(T), list[i]);
                }
                listOE.Add(t);
            }
            return listOE;

        }
    }

    public static class EntityExtension
    {

        public static FBEntity ToFBEntity<T>(this T t) where T : EntityObject
        {
            FBEntity fbEntity = new FBEntity();
            fbEntity.Entity = t;
            fbEntity.CollectionEntity = new ObservableCollection<RelationManyEntity>();
            fbEntity.ReferencedEntity = new ObservableCollection<RelationOneEntity>();
            // fbEntity.FBEntityState = EntityState.Unchanged;
            return fbEntity;
        }
        

        /// <summary>
        /// 根据 entityType , 返回对应的有关系的 FBEntity的集合, 
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <param name="entityType">有关系的对象集合类型</param>
        /// <returns>如果不存在,将会创建相应的集合</returns>
        public static ObservableCollection<FBEntity> GetRelationFBEntities(this FBEntity fbEntity, string entityType)
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

        /// <summary>
        /// 根据 entityType , 返回对应的有关系的 FBEntity的集合, 
        /// </summary>
        /// <param name="orderEntity"></param>
        /// <param name="entityType">有关系的对象集合类型</param>
        /// <returns>如果不存在,将会创建相应的集合</returns>
        public static ObservableCollection<FBEntity> GetRelationFBEntities(this OrderEntity orderEntity, string entityType)
        {
            return orderEntity.FBEntity.GetRelationFBEntities(entityType);
        }

        public static List<FBEntity> GetRelationFBEntities(this FBEntity fbEntity, string entityType, Func<FBEntity, bool> predicate)
        {
            ObservableCollection<FBEntity> list = fbEntity.GetRelationFBEntities(entityType);

            List<FBEntity> listResult = list.FindAll(predicate);
            return listResult;
            
        }
        /// <summary>
        /// 添加 有关系的集合FBEntity
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <param name="listNew"></param>
        public static void AddFBEntities<T>(this FBEntity fbEntity, IList<FBEntity> listNew)
        {
            string entityName = typeof(T).Name;
            
            ObservableCollection<FBEntity> list = fbEntity.GetRelationFBEntities(entityName);

            foreach (FBEntity item in listNew)
            {
                list.Add(item);
            }
        }

        public static void AddReferenceFBEntity<T>(this FBEntity fbEntity, FBEntity referenceFBEntity)
        {
            string entityName = typeof(T).Name;
            RelationOneEntity roe = new RelationOneEntity();
            roe.EntityType = entityName;
            roe.FBEntity = referenceFBEntity;
            fbEntity.ReferencedEntity.Add(roe);
        }

        public static bool IsNewEntity(this FBEntity fbEntity)
        {
            if (fbEntity.Entity.EntityKey == null)
            {
                return true;
            }
            else if (fbEntity.Entity.EntityKey.EntityKeyValues == null)
            {
                return true;
            }
            else if (fbEntity.FBEntityState == FBEntityState.Added)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<T> ToEntityList<T>(this IList<FBEntity> list) where T : EntityObject
        {
            List<T> listResult = new List<T>();
            foreach (var item in list)
            {
                T t = item.Entity as T;
                if (t != null)
                {
                    listResult.Add(t); 
                }
                
            }
            return listResult;
        }


        public static TEntity CopyEntity<TEntity>(this TEntity source)
           where TEntity : EntityObject, new()
        {
            TEntity target = new TEntity();
            List<PropertyInfo> listP = typeof(TEntity).GetProperties().ToList();
            listP.ForEach(property =>
            {

                bool isCopyProperty = property.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), true).Count() > 0;
                bool isList = typeof(IEnumerable).IsAssignableFrom(property.PropertyType);
                bool isEntityReference = typeof(SMT.FB.UI.FBCommonWS.EntityReference).IsAssignableFrom(property.PropertyType);
                if (isCopyProperty && !isList && !isEntityReference)
                {
                    source.CopyTo<TEntity>(target, property.Name);
                }
            });
            return target;
        }

        public static void OrderDetailBy<T>(this FBEntity fbEntity, Func<T, object> keySelector) where T : EntityObject
        {
            var details = fbEntity.GetRelationFBEntities(typeof(T).Name);

            var result = details.OrderBy(item =>
            {
                return keySelector(item.Entity as T);
            }).ToList();

            details.Clear();
            result.ForEach(item =>
            {
                details.Add(item);
            });
        }

    }

    public static class NullableExtension
    {

        public static CompareResult Compare(this Nullable<decimal> a, Nullable<decimal> b)
        {
            if (a == null)
            {
                a = new Nullable<Decimal>(0);
            }
            if (b == null)
            {
                b = new Nullable<Decimal>(0);
            }
            if (a.Value > b.Value)
            {
                return CompareResult.Bigger;
            }
            else if (a.Value == b.Value)
            {
                return CompareResult.Equal;
            }
            else
            {
                return CompareResult.Less;
            }
        }

        public static bool Equal(this Nullable<decimal> a, Nullable<decimal> b)
        {
            return a.Compare(b) == CompareResult.Equal;
        }

        public static bool Equal(this decimal a, Nullable<decimal> b)
        {
            return b.Equal(a);
        }
        public static bool BiggerThan(this Nullable<decimal> a, Nullable<decimal> b)
        {
            return a.Compare(b) == CompareResult.Bigger;
        }

        public static bool LessThan(this Nullable<decimal> a, Nullable<decimal> b)
        {
            return a.Compare(b) == CompareResult.Less;
        }

        public static Nullable<decimal> Add(this Nullable<decimal> a, Nullable<decimal> b)
        {
            if (a == null)
            {
                a = new Nullable<decimal>(0);
            }
            if (b == null)
            {
                b = new Nullable<decimal>(0);
            }
            return new Nullable<decimal>(a.Value + b.Value);
        }
        public static Nullable<decimal> Subtract(this Nullable<decimal> a, Nullable<decimal> b)
        {
            if (a == null)
            {
                a = new Nullable<decimal>(0);
            }
            if (b == null)
            {
                b = new Nullable<decimal>(0);
            }
            return new Nullable<decimal>(a.Value - b.Value);
        }
    }

    public static class StyleExtenstion
    {
        public static Style Copy(this Style style)
        {
            var styleNew = new System.Windows.Style(style.TargetType);
            
            style.Setters.ForEach(item =>
                {
                    Setter setter = item as Setter;
                    if (setter != null)
                    {
                        styleNew.Setters.Add(new Setter() { Property = setter.Property, Value = setter.Value });
                    }
                });

            return styleNew;
        }

        public static void SetStyle(this Style style, DependencyProperty property, object value)
        {
            var setterFind = style.Setters.FirstOrDefault(item =>
                {
                    Setter setter = item as Setter;
                    if (setter != null)
                    {
                        return setter.Property == property;
                    }
                    return false;
                });
            if (setterFind == null)
            {
                style.Setters.Remove(setterFind);
            }
            
            style.Setters.Add( new Setter() { Property = property, Value = value });
        }
    }


    public static class QueryExpressionHelper
    {
        public static QueryExpression Equal( string propertyName, string propertyValue)
        {
            QueryExpression qe = new QueryExpression();
            qe.PropertyName = propertyName;
            qe.PropertyValue = propertyValue;
            qe.Operation = QueryExpression.Operations.Equal;
            qe.RelatedType = QueryExpression.RelationType.And;
            return qe;
        }

        public static QueryExpression GetQueryExpression(this OrderEntity orderEntity, string propertyName)
        {

            object value = orderEntity.GetObjValue("Entity." + propertyName);
            if (value != null)
            {
                return QueryExpressionHelper.Equal(propertyName, Convert.ToString(value));
            }
            return null;
        }
        
    }

    #region IValueConverter
    public class ITextValueItemConvert : IValueConverter
    {

        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            MethodInfo myMethod = typeof(DataCore).GetMethods().First(m => m.Name.Equals("FindReferencedData") && m.IsGenericMethod);
            myMethod = myMethod.MakeGenericMethod(targetType);
            object result = myMethod.Invoke(value, null);
            
            //if ( targetType.IsAssignableFrom(ITextValueItem))
            //{
            //    IList<ITextValueItem> list = DataCore.GetRefData(targetType.Name);
            //    list.ToList().FirstOrDefault(item =>
            //        {
            //            return object.Equals( item.Value, 
            //        });
            //}
            //ITextValueItem result = DataCore.FindReferencedData<T>(value);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ITextValueItem item = value as ITextValueItem;
            if (item != null)
            {
                return item.Value;
            }
            else
            {
                return value;
            }
        }

        #endregion
    }

    public class CommonConvert : IValueConverter
    {
        public CommonConvert(PropertyItemInfo propertyItemInfo)
        {
            this.PropertyItemInfo = propertyItemInfo;
        }
        public PropertyItemInfo PropertyItemInfo { get; set; }
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(DateTime))
                {
                    return ((DateTime)value).ToString(PropertyItemInfo.DataFormat);
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
    public class EntityConvert : IValueConverter
    {
        public Type TargetType { get; set; }

        public EntityConvert(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                type = CommonFunction.GetType(typeName, CommonFunction.TypeCategory.Common);
            }
           
            TargetType = type;
        }
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = CommonFunction.TryConvertValue(TargetType, value);

           
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = CommonFunction.TryConvertValue(targetType, value);
            return result;
        }

        #endregion
    }

    public class TypeImageConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            Type type = value as Type;
            if (type == typeof(T_FB_SUBJECTTYPE))
            {
                ImageSource imageSource = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;component/Images/company.png"));
                return imageSource;
            }
            else
            {
                ImageSource imageSource = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;component/Images/entity.png"));
                return imageSource;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    public class VisibleBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class CurrencyConverter : IValueConverter
    {
        public string Currency { get; set; }

        public int Decimals { get; set; }

        public CurrencyConverter()
        {
            this.Decimals = 2;
        }
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            {
                try
                {
                    // 目前只能清为0。
                    decimal newValue = CommonFunction.TryConvertValue<decimal>(value); 

                    return Math.Round(newValue, Decimals);
                }
                catch
                {
                    // throw new Exception("格式为数值类型");
                }
                
            }
            return value;
        }

        #endregion
    }

    /// <summary>
    /// 用于分组
    /// </summary>
    public class GroupConverter : IValueConverter
    {
        public GroupConverter(string propertyName)
        {
            this.PropertyName = propertyName;
        }
        #region IValueConverter Members
        public string PropertyName { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
           
            var va = value.GetObjValue(PropertyName);
            if (va == null)
            {
                return value;
            }
            return va;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    #endregion

    public static class DependancyObjectExtension
    {
        public static T FindChildControl<T>(this DependencyObject obj, string ctrName)
         where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {

                    object value = child.GetValue(Control.NameProperty);
                    if (value != null && value.ToString() == ctrName)
                    {
                        return child as T;
                    }
                    else
                    {
                        T childOfChild = FindChildControl<T>(child, ctrName);
                        if (childOfChild != null)
                            return childOfChild;
                    }
                }
                else
                {
                    T childOfChild = FindChildControl<T>(child, ctrName);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
