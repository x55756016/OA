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
using System.Collections.ObjectModel;
using SMT.FB.UI.Common;
using System.ComponentModel;

namespace SMT.FB.UI.FBCommonWS
{
    public interface IDetails
    {
        bool HideDetails { get; set; }
        bool ReadOnly { get; set; }
    }
    public partial class FBEntity : IDetails
    {
        public T GetEntity<T>() where T : EntityObject
        {
            return this.Entity as T;
        }

        /// <summary>
        /// 0 :无，　1:有
        /// </summary>
        private bool _HaveDetails = false;
        public bool HideDetails
        {
            get
            {
                return _HaveDetails;
            }
            set
            {
                if (_HaveDetails != value)
                {
                    _HaveDetails = value;
                    this.RaisePropertyChanged("HideDetails");
                }
            }
        }

        private bool _ReadOnly = false;
        public bool ReadOnly
        {
            get
            {
                return _ReadOnly;
            }
            set
            {
                if (_ReadOnly != value)
                {
                    _ReadOnly = value;
                    this.RaisePropertyChanged("ReadOnly");
                }
            }
        }
        
    }
    #region 预算

    public partial class T_FB_BUDGETACCOUNT
    {
        public string PostInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.OWNERPOSTID))
                {
                    return this.OWNERCOMPANYID + "-" + this.OWNERDEPARTMENTID; 
                }

                if (string.IsNullOrWhiteSpace(this.OWNERDEPARTMENTID))
                {
                    return this.OWNERCOMPANYID;
                }

                return this.OWNERCOMPANYID + "-" + this.OWNERDEPARTMENTID + "-" + this.OWNERPOSTID;
            }
        }

    }


    #region 预算汇总
    
    public class V_SubjectDepartmentSum : EntityObject
    {
        public T_FB_SUBJECT T_FB_SUBJECT { get; set; }
        public decimal BUDGETMONEY { get; set; }
        public List<T_FB_DEPTBUDGETAPPLYDETAIL> T_FB_DEPTBUDGETAPPLYDETAIL { get; set; }
    }

    public partial class T_FB_DEPTBUDGETSUMDETAIL
    {

        public decimal? BUDGETMONEY
        {
            get
            {
                return this.T_FB_DEPTBUDGETAPPLYMASTER.BUDGETMONEY;
            }
        }

        public ObservableCollection<T_FB_DEPTBUDGETAPPLYDETAIL> T_FB_DEPTBUDGETAPPLYDETAIL
        {
            get
            {
                return this.T_FB_DEPTBUDGETAPPLYMASTER.T_FB_DEPTBUDGETAPPLYDETAIL;
            }
            set
            {
            }
        }
    }
    #endregion

    #region 年度预算
    public class V_SubjectCompanySum : EntityObject
    {
        public T_FB_SUBJECT T_FB_SUBJECT { get; set; }
        public decimal BUDGETMONEY { get; set; }
        public List<T_FB_COMPANYBUDGETAPPLYDETAIL> T_FB_COMPANYBUDGETAPPLYDETAIL { get; set; }
    }

    public partial class T_FB_COMPANYBUDGETSUMDETAIL
    {
        public decimal? BUDGETMONEY
        {
            get
            {
                return this.T_FB_COMPANYBUDGETAPPLYMASTER.BUDGETMONEY;
            }
        }
        public ObservableCollection<T_FB_COMPANYBUDGETAPPLYDETAIL> T_FB_COMPANYBUDGETAPPLYDETAIL
        {
            get
            {
                return this.T_FB_COMPANYBUDGETAPPLYMASTER.T_FB_COMPANYBUDGETAPPLYDETAIL;
            }
            set
            {
            }
        }


    }
    public partial class T_FB_COMPANYBUDGETMODDETAIL
    {
        public string _OwnerDepartmentName = null;
        public string OwnerDepartmentName
        {
            get
            {
                if (_OwnerDepartmentName == null)
                {

                    DepartmentData d = ReferencedData<DepartmentData>.Find(this.OWNERDEPARTMENTID);
                    if (d != null)
                    {
                        _OwnerDepartmentName = d.Text;
                    }
                    else
                    {
                        _OwnerDepartmentName = "";
                    }
                }
                return _OwnerDepartmentName;
            }
        }
    }
    #endregion

    #region 部门预算

    #region 自定义对象
    public class V_DepartmentSum : EntityObject
    {
        public V_DepartmentSum(EntityObject entity)
        {
            entity.PropertyChanged +=(sender,e) =>
                {
                    if ( e.PropertyName == "BUDGETMONEY")
                    {
                        SetValue(sender);
                    }
                };

            SetValue(entity);
        }

        private void SetValue(object sender)
        {
            var newValue = sender.GetObjValue("BUDGETMONEY");
            this.BUDGETMONEY = Convert.ToDecimal(newValue);
        }

        private string _OWNERNAME = "公共";
        public string OWNERNAME
        {
            get
            {
                return _OWNERNAME;
            }
            set
            {
                if (! object.Equals(_OWNERNAME, value))
                {
                    _OWNERNAME = value;
                    this.RaisePropertyChanged("OWNERNAME");
                }
            }
        }

        private string _OWNERPOSTNAME = "";
        public string OWNERPOSTNAME
        {
            get
            {
                return _OWNERPOSTNAME;
            }
            set
            {
                if (!object.Equals(_OWNERPOSTNAME, value))
                {
                    _OWNERPOSTNAME = value;
                    this.RaisePropertyChanged("OWNERPOSTNAME");
                }
            }
        }


        private decimal? _LIMITBUDGETMONEY = 0;
        public decimal? LIMITBUDGETMONEY
        {
            get
            {
                return _LIMITBUDGETMONEY;
            }
            set
            {
                if (!object.Equals(_LIMITBUDGETMONEY, value))
                {
                    _LIMITBUDGETMONEY = value;
                    this.RaisePropertyChanged("LIMITBUDGETMONEY");
                }
            }
        }

        private decimal? _BUDGETMONEY = 0;
        public decimal? BUDGETMONEY
        {
            get
            {
                return _BUDGETMONEY;
            }
            set
            {
                if (!_BUDGETMONEY.Equal(value))
                {
                    _BUDGETMONEY = value;
                    this.RaisePropertyChanged("BUDGETMONEY");
                }
            }
        }
    }
    #endregion
    public partial class T_FB_DEPTBUDGETADDMASTER
    {
        public string BudgetMonthName
        {
            get
            {
                return this.BUDGETARYMONTH.ToString(DataCore.DataFormat_Month);
            }
        }
    }

    public partial class T_FB_DEPTBUDGETAPPLYMASTER
    {
        public string BudgetMonthName
        {
            get
            {
                return this.BUDGETARYMONTH.ToString(DataCore.DataFormat_Month);
            }
        }
    }

    public partial class T_FB_DEPTTRANSFERMASTER
    {
        public string BudgetMonthName
        {
            get
            {
                return this.BUDGETARYMONTH.ToString(DataCore.DataFormat_Month);
            }
        }
    }

    public partial class T_FB_DEPTBUDGETSUMMASTER
    {
        public string BudgetMonthName
        {
            get
            {
                return this.BUDGETARYMONTH.ToString(DataCore.DataFormat_Month);
            }
        }
    }


    public partial class T_FB_DEPTBUDGETAPPLYDETAIL
    {
        /// <summary>
        /// 可用额度的中文名称
        /// </summary>
        public string UableMoneyName
        {
            get
            {

                if (this.USABLEMONEY.Equal(DataCore.Max_Budget) || this.USABLEMONEY.Equal(99999999))
                {
                    return "不受年度预算限制";
                }
                else
                {
                    return this.USABLEMONEY.ToString();
                }
            }
        }
    }

    public partial class T_FB_DEPTBUDGETADDDETAIL
    {
        /// <summary>
        /// 可用额度的中文名称
        /// </summary>
        public string UableMoneyName
        {
            get
            {

                if (this.USABLEMONEY.Equal(DataCore.Max_Budget) || this.USABLEMONEY.Equal(99999999))
                {
                    return "不受年度预算限制";
                }
                else
                {
                    return this.USABLEMONEY.ToString();
                }
            }
        }
    }

    public partial class T_FB_PERSONBUDGETAPPLYDETAIL
    {
        /// <summary>
        /// 可用额度的中文名称
        /// </summary>
        public string UableMoneyName
        {
            get
            {

                if (this.USABLEMONEY.Equal(DataCore.Max_Charge) || this.USABLEMONEY.Equal(999999))
                {
                    return "无预算额度限制";
                }
                else
                {
                    return this.USABLEMONEY.ToString();
                }
            }
        }
    }

    public partial class T_FB_PERSONBUDGETADDDETAIL
    {
        /// <summary>
        /// 可用额度的中文名称
        /// </summary>
        public string UableMoneyName
        {
            get
            {

                if (this.USABLEMONEY.Equal(DataCore.Max_Charge) || this.USABLEMONEY.Equal(999999))
                {
                    return "无预算额度限制";
                }
                else
                {
                    return this.USABLEMONEY.ToString();
                }
            }
        }
    }

    public partial class T_FB_DEPTTRANSFERMASTER
    {
        private OrgObjectData _ToObject = null;

        /// <summary>
        /// 调拨来源
        /// </summary>
        public OrgObjectData ToObject
        {
            get
            {
                if (_ToObject == null)
                {
                    if (this.TRANSFERTOTYPE.Equal(3))
                    {
                        _ToObject = new EmployeerData
                        {
                            Post = new PostData
                            {
                                Text = this.TRANSFERTOPOSTNAME,
                                Value = this.TRANSFERTOPOSTID
                            }
                        };

                    }
                    else if (this.TRANSFERTOTYPE.Equal(2))
                    {
                        _ToObject = new DepartmentData();
                    }
                    else
                    {
                        _ToObject = null;
                        return _ToObject;
                    }
                    _ToObject.Value = this.TRANSFERTO;
                    _ToObject.Text = this.TRANSFERTONAME;
                }
                return _ToObject;
            }
            set
            {
                if (!object.Equals(_ToObject, value))
                {
                    _ToObject = value;
                    if (_ToObject == null)
                    {
                        this.TRANSFERTO = null;
                        this.TRANSFERTONAME = null;
                        this.TRANSFERTOTYPE = 0;

                        this.TRANSFERTOPOSTID = null;
                        this.TRANSFERTOPOSTNAME = null;

                        this.TRANSFERTODEPARTMENTID = null;
                        this.TRANSFERTODEPARTMENTNAME = null;

                        this.TRANSFERTOCOMPANYID = null;
                        this.TRANSFERTOCOMPANYNAME = null;
                    }
                    else
                    {
                        Type type = _ToObject.GetType();
                        if (typeof(EmployeerData).IsAssignableFrom(type))
                        {
                            this.TRANSFERTOTYPE = 3;
                            this.TRANSFERTOPOSTID = (_ToObject as EmployeerData).Post.Value.ToString();
                            this.TRANSFERTOPOSTNAME = (_ToObject as EmployeerData).Post.Text.ToString();
                            this.TRANSFERTODEPARTMENTID = (_ToObject as EmployeerData).Department.Value.ToString();
                            this.TRANSFERTODEPARTMENTNAME = (_ToObject as EmployeerData).Department.Text.ToString();
                            this.TRANSFERTOCOMPANYID = (_ToObject as EmployeerData).Company.Value.ToString();
                            this.TRANSFERTOCOMPANYNAME = (_ToObject as EmployeerData).Company.Text.ToString();
                        }
                        else if (typeof(DepartmentData).IsAssignableFrom(type))
                        {
                            this.TRANSFERTOTYPE = 2;

                            this.TRANSFERTODEPARTMENTID = (_ToObject as DepartmentData).Value.ToString();
                            this.TRANSFERTODEPARTMENTNAME = (_ToObject as DepartmentData).Text.ToString();
                            this.TRANSFERTOCOMPANYID = (_ToObject as DepartmentData).Company.Value.ToString();
                            this.TRANSFERTOCOMPANYNAME = (_ToObject as DepartmentData).Company.Text.ToString();
                        }
                        else
                        {
                            ToObject = null;
                            return;
                        }
                        this.TRANSFERTO = _ToObject.Value.ToString();
                        this.TRANSFERTONAME = _ToObject.Text;
                    }
                    this.RaisePropertyChanged("ToObject");
                }
            }
        }

        /// <summary>
        /// 调拨目标
        /// </summary>
        private OrgObjectData _FromObject = null;
        public OrgObjectData FromObject
        {
            get
            {
                if (_FromObject == null)
                {
                    if (this.TRANSFERFROMTYPE.Equal(3))
                    {
                        _FromObject = new EmployeerData
                        {
                            Post = new PostData
                            {
                                Text = this.TRANSFERFROMPOSTNAME,
                                Value = this.TRANSFERFROMPOSTID
                            }
                        };

                    }
                    else if (this.TRANSFERFROMTYPE.Equal(2))
                    {
                        _FromObject = new DepartmentData();

                    }
                    else
                    {
                        _FromObject = null;
                        return _FromObject;
                    }
                    _FromObject.Value = this.TRANSFERFROM;
                    _FromObject.Text = this.TRANSFERFROMNAME;
                }
                return _FromObject;
            }
            set
            {
                if (!object.Equals(_FromObject, value))
                {
                    _FromObject = value;
                    if (_FromObject == null)
                    {
                        this.TRANSFERFROM = null;
                        this.TRANSFERFROMNAME = null;
                        this.TRANSFERFROMTYPE = 0;

                        this.TRANSFERFROMPOSTID = null;
                        this.TRANSFERFROMPOSTNAME = null;

                        this.TRANSFERFROMDEPARTMENTID = null;
                        this.TRANSFERFROMDEPARTMENTNAME = null;

                        this.TRANSFERFROMCOMPANYID = null;
                        this.TRANSFERFROMCOMPANYNAME = null;
                    }
                    else
                    {
                        Type type = _FromObject.GetType();
                        if (typeof(EmployeerData).IsAssignableFrom(type))
                        {
                            this.TRANSFERFROMTYPE = 3;
                            this.TRANSFERFROMPOSTID = (_FromObject as EmployeerData).Post.Value.ToString();
                            this.TRANSFERFROMPOSTNAME = (_FromObject as EmployeerData).Post.Text.ToString();
                            this.TRANSFERFROMDEPARTMENTID = (_FromObject as EmployeerData).Department.Value.ToString();
                            this.TRANSFERFROMDEPARTMENTNAME = (_FromObject as EmployeerData).Department.Text.ToString();
                            this.TRANSFERFROMCOMPANYID = (_FromObject as EmployeerData).Company.Value.ToString();
                            this.TRANSFERFROMCOMPANYNAME = (_FromObject as EmployeerData).Company.Text.ToString();
                        }
                        else if (typeof(DepartmentData).IsAssignableFrom(type))
                        {
                            this.TRANSFERFROMTYPE = 2;

                            this.TRANSFERFROMDEPARTMENTID = (_FromObject as DepartmentData).Value.ToString();
                            this.TRANSFERFROMDEPARTMENTNAME = (_FromObject as DepartmentData).Text.ToString();
                            this.TRANSFERFROMCOMPANYID = (_FromObject as DepartmentData).Company.Value.ToString();
                            this.TRANSFERFROMCOMPANYNAME = (_FromObject as DepartmentData).Company.Text.ToString();
                        }
                        else
                        {
                            FromObject = null;
                            return;
                        }
                        this.TRANSFERFROM = _FromObject.Value.ToString();
                        this.TRANSFERFROMNAME = _FromObject.Text;
                    }
                    this.RaisePropertyChanged("FromObject");
                }
            }
        }
    }
    #endregion

    #region 个人经费下拨
    public partial class T_FB_PERSONMONEYASSIGNMASTER
    {

        public string BudgetMonthName
        {
            get
            {
                return this.BUDGETARYMONTH.ToString(DataCore.DataFormat_Month);
            }
        }

        private OrgObjectData _AssignCompany = null;
        public OrgObjectData AssignCompany
        {
            get
            {
                if (_AssignCompany == null)
                {
                    _AssignCompany = new CompanyData();
                    if (!string.IsNullOrWhiteSpace(this.ASSIGNCOMPANYID) && !string.IsNullOrWhiteSpace(this.ASSIGNCOMPANYNAME))
                    {
                        _AssignCompany.Value = this.ASSIGNCOMPANYID;
                        _AssignCompany.Text = this.ASSIGNCOMPANYNAME;
                    }
                    else
                    {
                        _AssignCompany.Value = this.OWNERCOMPANYID;
                        _AssignCompany.Text = this.OWNERCOMPANYNAME;
                    }
                }
                return _AssignCompany;
            }
            set
            {
                if (!object.Equals(_AssignCompany, value))
                {
                    _AssignCompany = value;
                    if (_AssignCompany == null)
                    {
                        this.ASSIGNCOMPANYID = null;
                        this.ASSIGNCOMPANYNAME = null;
                    }
                    else
                    {
                        this.ASSIGNCOMPANYID = _AssignCompany.Value.ToString();
                        this.ASSIGNCOMPANYNAME = _AssignCompany.Text;
                    }
                    this.RaisePropertyChanged("AssignCompany");
                }
            }
        }
    }


    public partial class T_FB_PERSONMONEYASSIGNDETAIL
    {
        private int rowIndex;
        public int RowIndex
        {
            get
            {
                return this.rowIndex;
            }
            set
            {
                rowIndex = value;
            }
        }

        public decimal LastBudgetmoney
        {
            get
            {
                if (this.BUDGETMONEY == null)
                {
                    return 0;
                }

                return this.BUDGETMONEY.Value;
            }
        }
    }
    #endregion

    #endregion

    #region 日常

    public partial class T_FB_CHARGEAPPLYMASTER
    {
        /// <summary>
        /// 扩展单据名称
        /// </summary>
        public string ExOrder
        {
            get
            {
                if (this.T_FB_EXTENSIONALORDER != null)
                {
                    // 
                    return this.T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPE.EXTENSIONALTYPENAME + " : " + this.T_FB_EXTENSIONALORDER.ORDERCODE;
                }
                return "";
            }
        }

        /// <summary>
        /// 月份的中文名称
        /// </summary>
        public string BudgetMonthName
        {
            get
            {

                return this.BUDGETARYMONTH.ToString(DataCore.DataFormat_Month);
            }
        }


    }

    public partial class T_FB_CHARGEAPPLYDETAIL
    {

        /// <summary>
        /// 可用额度的中文名称
        /// </summary>
        public string UableMoneyName
        {
            get
            {
                if (this.USABLEMONEY.Equal(999999) || this.USABLEMONEY.Equal(99999999))
                {
                    return "不受月度预算限制";
                }
                else
                {
                    return this.USABLEMONEY.ToString();
                }
            }
        }
    }

    public partial class T_FB_BORROWAPPLYDETAIL
    {
        /// <summary>
        /// 可用额度的中文名称
        /// </summary>
        public string UableMoneyName
        {
            get
            {

                if (this.USABLEMONEY.Equal(999999) || this.USABLEMONEY.Equal(99999999))
                {
                    return "不受月度预算限制";
                }
                else
                {
                    return this.USABLEMONEY.ToString();
                }
            }
        }
    }


    public partial class T_FB_TRAVELEXPAPPLYDETAIL
    {
        public string UableMoneyName
        {
            get
            {
                return string.Format("{0}({1} + {2})", this.USABLEMONEY, this.DeptUsableMoney, this.PersonUsableMoney);
            }
        }

    }

    public partial class T_FB_TRAVELEXPAPPLYMASTER
    {

        public string BudgetMonthName
        {
            get
            {
                return this.BUDGETARYMONTH.ToString(DataCore.DataFormat_Month);
            }
        }
    }



    #endregion

    #region 科目维护
    public partial class T_FB_SUBJECT
    {
        private bool isInitDisplay = false;
        public void InitDisplay()
        {
            this.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(T_FB_SUBJECT_PropertyChanged);
        }

        void T_FB_SUBJECT_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SUBJECTNAME"
               || e.PropertyName == "SUBJECTCODE"
               )
            {
                this.RaisePropertyChanged("DisplayName");
            }
        }
        

        public string DisplayName
        {
            get
            {
                if (!isInitDisplay)
                {
                    isInitDisplay = true;
                    InitDisplay();

                }
                return string.Format("({0}) {1}", SUBJECTCODE, SUBJECTNAME);
            }
        }
       
    }
    public partial class T_FB_SUBJECTDEPTMENT
    {

        public string LimitString
        {
            get
            {
                if (this.LIMITBUDGEMONEY.Equal(0))
                {
                    return "无预算额度限制";
                }
                else
                {
                    return this.LIMITBUDGEMONEY.ToString();
                }
            }
            set
            {
                this.LIMITBUDGEMONEY = Convert.ToDecimal(value);
                this.RaisePropertyChanged("LimitString");
            }
        }
    }

    public partial class T_FB_SUBJECTPOST
    {

        public string LimitString
        {
            get
            {
                if (this.LIMITBUDGEMONEY.Equal(0))
                {
                    return "无预算额度限制";
                }
                else
                {
                    return this.LIMITBUDGEMONEY.ToString();
                }
            }
            set
            {
                this.LIMITBUDGEMONEY = Convert.ToDecimal(value);
                this.RaisePropertyChanged("LimitString");
            }
        }
    }
    #endregion

}
