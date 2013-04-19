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
    public partial class SearchCityNew : UserControl
    {
        

        public event EventHandler SelectClick;

        public DependencyProperty SelectItemProperty;  

        public int RowNum { get; set; }

        public int ColunmNum { get; set; }

        private object entity;

        public Button SearchButton
        {
            get { return this.btSelectedCity; }
        }

        public TextBox TxtSelectedCity
        {
            set { this.txtSelectedCity = value; }
            get { return this.txtSelectedCity; }
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
                // this.TxtSelectedCity.DataContext = this.Entity;
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
                txtSelectedCity.SetBinding(TextBox.TextProperty, binding);
                txtSelectedCity.GetBindingExpression(TextBox.DataContextProperty);
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

        public SearchCityNew()
        {
            InitializeComponent();
            this.btSelectedCity.Click += new RoutedEventHandler(btSelectedCity_Click);
            SelectItemProperty = DependencyProperty.Register("SelectItem", typeof(object), typeof(SearchCity) , null);
        }

        public void btSelectedCity_Click(object sender, RoutedEventArgs e)
        {
            if (SelectClick != null)
            {

                SelectClick(this, e);
            }
        }
    }
}
