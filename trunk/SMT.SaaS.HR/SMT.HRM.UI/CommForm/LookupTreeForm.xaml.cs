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
using System.Reflection;
using System.Collections;
using SMT.Saas.Tools.OrganizationWS;

namespace SMT.HRM.UI
{
    public partial class LookupTreeForm : System.Windows.Controls.Window
    {
        private EntityObject entity;

        public EntityObject Entity
        {
            get { return entity; }
            set { entity = value; }
        }

        public LookupTreeForm()
        {
            InitializeComponent();
            initiaTreeItem(entity);
        }


        private void initiaTreeItem(EntityObject entity)
        {
            if (entity == null) return;
                        
            Type type = entity.GetType();
            PropertyInfo[] infos = type.GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                PropertyInfo pinfo = infos[i];
                if (pinfo.GetValue(entity, null) != null)
                {
                    string strPinfoValue = pinfo.GetValue(entity, null).ToString();
                    TreeViewItem item = new TreeViewItem();
                    item.DataContext = "itemValue:"+strPinfoValue;
                    item.Header = strPinfoValue;
                    lookUpTree.Items.Add(item);
                }
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item  = (TreeViewItem)lookUpTree.SelectedItem;
            if (item != null)
            {
                MessageBox.Show(item.DataContext.ToString());
            }
            //this.DialogResult = true;
            //this.Result = true; 2010.4.24更新测试
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            //this.Result = false; 2010.4.24更新测试
        }

        private void lookUpTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (lookUpTree.SelectedItem.GetType() != typeof(TreeViewItem)) return;
            TreeViewItem item  = (TreeViewItem)lookUpTree.SelectedItem;
            if (item == null || item.DataContext == null) return;
            txtTreeSelect.Text = item.DataContext.ToString();
        }
    }
}

