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

namespace SMT.SaaS.FrameworkUI
{
    public partial class SelectPost : UserControl
    {
        
        public event EventHandler SelectClick;

        public DependencyProperty SelectItemProperty;  

        private object entity;

        public Button SearchButton
        {
            get { return this.btnSelectedPost; }
        }

        public TextBox TxtSelectedPost
        {
            set { this.txtSelectedPost = value; }
            get { return this.txtSelectedPost; }
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
                // this.TxtSelectedPost.DataContext = this.Entity;
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
                txtSelectedPost.SetBinding(TextBox.TextProperty, binding);
                txtSelectedPost.GetBindingExpression(TextBox.DataContextProperty);
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

        public SelectPost()
        {
            InitializeComponent();
            this.btnSelectedPost.Click += new RoutedEventHandler(btSelectedPost_Click);
            SelectItemProperty = DependencyProperty.Register("SelectItem", typeof(object), typeof(SelectPost), null);
        }

        public void btSelectedPost_Click(object sender, RoutedEventArgs e)
        {
            if (SelectClick != null)
                SelectClick(this, e);
        }
    }
}
