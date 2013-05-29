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
using System.Collections.Generic;
using SMT.FB.UI.FBCommonWS;
using System.ComponentModel;

namespace SMT.FB.UI.Common
{
    #region ReferencedData

    public interface ITextValueItem : INotifyPropertyChanged
    {
        string Text { get; set; }
        object Value { get; set; }
    }
  
    public class TextValueItemBase : StructuralObject, ITextValueItem
    {
        public TextValueItemBase()
        {
            ID = Guid.NewGuid().ToString();
        }
        public string ID { get; set; }
        private string _Text;
        public string Text 
        {
            get
            {
                return _Text;
            }
            set
            {
                if ((object.ReferenceEquals(this._Text, value) != true))
                {
                    _Text = value;
                    RaisePropertyChanged("Text");
                }
                
            }
        }

        private object _Value;
        public object Value 
        {
            get
            {
                return _Value;
            }
            set
            {
                if ((object.ReferenceEquals(this._Value, value) != true))
                {
                    _Value = value;
                    RaisePropertyChanged("Value");
                }
            }
        }

        public void CopyData(ITextValueItem item)
        {
            item.Text = Text;
            item.Value = Value;
        }
    }

    public class MonthItem : TextValueItemBase
    {

    }

    public class YearItem : TextValueItemBase
    {

    }

    public class OrgObjectData : TextValueItemBase
    {
    }
    public class EmployeerData : OrgObjectData
    {
        public PostData Post { get; set; }
        public CompanyData Company { get; set; }
        public DepartmentData Department { get; set; }
        public EmployeerData Copy()
        {
            EmployeerData newItem = new EmployeerData();
            base.CopyData(newItem);
            newItem.Department = this.Department;
            newItem.Post = this.Post;
            newItem.Company = this.Company;
            return newItem;
        }
    }

    public class LoginUserData : EmployeerData
    {
        public List<LoginUserData> PostInfos { get; set; }
    }


    public class CompanyData : OrgObjectData
    {
        public List<DepartmentData> DepartmentCollection { get; set; }
    }
    public class DepartmentData : OrgObjectData
    {
        public CompanyData Company { get; set; }

        public List<PostData> PostCollection { get; set; }
    }
    public class PostData : OrgObjectData
    {
        public DepartmentData Department { get; set; }
        public CompanyData Company { get; set; }

    }

    public class QueryData : TextValueItemBase
    {
        public QueryExpression QueryExpression { get; set; }
    }
    public class BudgetAccountTypeData : TextValueItemBase
    {
    }
    public class OrderStatusData : TextValueItemBase
    {

    }
    public class PayTypeData : TextValueItemBase
    {
    }
    public class RepayTypeData : TextValueItemBase
    {
    }
    public class BorrowTypeData : TextValueItemBase
    {
    }
    public class ControlTypeData : TextValueItemBase
    {
    }
    public class BudgetChargeTypeData : TextValueItemBase
    {
    }
    public class BudgetTypeData : TextValueItemBase
    {
    }
    //beyond
    public class BudgetSumStatesData : TextValueItemBase
    {
    }
    public class ReferencedData<T> where T : ITextValueItem
    {
        public static IList<T> RefData { set; get; }
        public static T Find(object value)
        {
            if (value == null)
            {
                return default(T);
            }
            if (RefData == null)
            {
                T item = Activator.CreateInstance<T>();
                item.Value = value;
                return item;

            }
            for (int i = 0; i < RefData.Count; i++)
            {
                object newValue = CommonFunction.TryConvertValue(RefData[i].Value.GetType(), value);
                if (RefData[i].Value.Equals(newValue))
                {
                    return RefData[i];
                }
            }
            return default(T);
        }
    }

    public class DateTimeData : StructuralObject, ITextValueItem
    {
        public string DateFormat = "yyyy-MM";
        #region ITextValueItem 成员

        public string Text
        {
            get
            {

                DateTime dtValue = Convert.ToDateTime(Value);
                return dtValue.ToString(DateFormat);
            }
            set
            {
            }

        }

        public object Value
        {
            get;
            set;
        }

        #endregion
    }

    public class BorrowOrderData : ITextValueItem
    {
        public T_FB_BORROWAPPLYMASTER BorrowApplyMaster { get; set; }

        #region ITextValueItem 成员

        public string Text
        {
            get
            {
                return BorrowApplyMaster.BORROWAPPLYMASTERCODE;
            }
            set
            {
                
            }
        }

        public object Value
        {
            get
            {
                return BorrowApplyMaster;
            }
            set
            {
                BorrowApplyMaster = value as T_FB_BORROWAPPLYMASTER;
            }
        }

        #endregion

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class ExtensionalOrderData : ITextValueItem
    {
        public T_FB_EXTENSIONALORDER ExtensionalOrder { get; set; }

        #region ITextValueItem 成员

        public string Text
        {
            get
            {
                return ExtensionalOrder.T_FB_EXTENSIONALTYPE.EXTENSIONALTYPENAME;
            }
            set
            {

            }
        }

        public object Value
        {
            get
            {
                return ExtensionalOrder;
            }
            set
            {
                ExtensionalOrder = value as T_FB_EXTENSIONALORDER;
            }
        }

        #endregion

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    /// <summary>
    /// 组织架构对象
    /// </summary>
    public class MyOrgObjectData : ITextValueItem
    {
        public OrgObjectData OrgObject { get; set; }

        public string Text
        {
            get
            {
                return OrgObject.Text;
            }
            set
            {

            }
        }

        public object Value
        {
            get
            {
                return OrgObject;
            }
            set
            {
                OrgObject = value as OrgObjectData;
            }
        }
        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
   
    #endregion
}
