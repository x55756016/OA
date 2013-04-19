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

namespace SMT.SaaS.FrameworkUI
{
    public partial class LookUp : UserControl
    {
        public static readonly DependencyProperty SelectItemProperty = DependencyProperty.RegisterAttached("SelectItem", typeof(object), typeof(LookUp), null);
        public object SelectItem
        {
            get
            {
                return base.GetValue(SelectItemProperty);
            }
            set
            {
                base.SetValue(SelectItemProperty, value);
                this.Entity = value;
                // this.TxtLookUp.DataContext = this.Entity;
            }
        }

        private object entity;

        public object Entity
        {
            get { return entity; }
            set { entity = value; }
        }

        public Button SearchButton
        {
            get { return this.btnFind_LookUp; }
        }

        public TextBox TxtLookUp
        {
            get { return this.txtLookUpValue_LookUp; }
        }

        private string displayMemberPath;

        public string DisplayMemberPath
        {
            get { return displayMemberPath; }
            set
            {
                displayMemberPath = value;
                System.Windows.Data.Binding binding = new System.Windows.Data.Binding();
                binding.Path = new PropertyPath(value);
                //binding.Source = this.con
                txtLookUpValue_LookUp.SetBinding(TextBox.TextProperty, binding);
                txtLookUpValue_LookUp.GetBindingExpression(TextBox.DataContextProperty);
            }
        }

        private string lookUpType;
        /// <summary>
        /// 树形控件Tree，网格为Grid
        /// </summary>
        public string LookUpType
        {
            get { return lookUpType; }
            set { lookUpType = value; }
        }

        public LookUp()
        {
            InitializeComponent();
            this.btnFind_LookUp.Click += new RoutedEventHandler(btnFind_Click);
        }

        public event EventHandler FindClick;

        public void btnFind_Click(object sender, RoutedEventArgs e)
        {
            if (FindClick != null)
                FindClick(this, e);
        }
        public void BindEmployeeName(string postID, string departmentID, string companyID)
        {

            string postName = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == postID).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
            string departName = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == departmentID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            string companyName = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == companyID).FirstOrDefault().CNAME;
            TxtLookUp.Text += "-"+postName + "-" + departName + "-" + companyName;
        }
    }
}
