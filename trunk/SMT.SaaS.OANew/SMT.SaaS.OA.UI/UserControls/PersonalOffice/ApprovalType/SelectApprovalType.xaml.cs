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

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SelectApprovalType : UserControl
    {
        public SelectApprovalType()
        {
            InitializeComponent();
            this.btnSelectApprovalType.Click += new RoutedEventHandler(btSelectApprovalType_Click);
            SelectItemProperty = DependencyProperty.Register("SelectItem", typeof(object), typeof(SelectApprovalType), null);
        }
        public event EventHandler SelectClick;

        public DependencyProperty SelectItemProperty;

        private object entity;

        public Button SearchButton
        {
            get { return this.btnSelectApprovalType; }
        }

        public TextBox TxtSelectedApprovalType
        {
            set { this.txtSelectedApprovalType = value; }
            get { return this.txtSelectedApprovalType; }
        }

        public object Entity
        {
            get { return entity; }
            set { entity = value; }
        }

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
            }
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
                txtSelectedApprovalType.SetBinding(TextBox.TextProperty, binding);
                txtSelectedApprovalType.GetBindingExpression(TextBox.DataContextProperty);
            }
        }

        private string searchCityType;

        /// <summary>
        /// SearchCityType
        /// </summary>
        public string SearchCityType
        {
            get { return searchCityType; }
            set { searchCityType = value; }
        }



        public void btSelectApprovalType_Click(object sender, RoutedEventArgs e)
        {
            if (SelectClick != null)
                SelectClick(this, e);
        }
    }
}
