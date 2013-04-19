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

namespace SMT.SaaS.FrameworkUI
{
    public partial class AppForm: ChildWindow
    {
        private object entity;

        #region "属性及构造"
        /// <summary>
        /// 获取增加按钮
        /// </summary>
        public Button ButtonAdd
        {
            get { return btnAdd; }
        }
        /// <summary>
        /// 获取更新按钮
        /// </summary>
        public Button ButtonUpdate
        {
            get { return this.btnUpdate; }
        }

        public AppForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="obj">实体对象</param>
        /// <param name="Add">是否为新增窗口</param>
        public AppForm(ref object obj, bool AddState)
        {
            InitializeComponent();
            entity = obj;
            initEntityForm(obj, AddState);
        }
        #endregion
        
        public virtual void initEntityForm( object obj,bool AddState)
        {
            if (AddState == true)
            {
                btnAdd.Visibility = Visibility.Visible;
                btnUpdate.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnAdd.Visibility = Visibility.Collapsed;
                btnUpdate.Visibility = Visibility.Visible;
            }

            Type type = obj.GetType();
            //设置Title
            txtEntityTitle.Text = "详情";
            this.Title = type.Name;

            EntityGrid.SetValue(Grid.ShowGridLinesProperty, true); //显示网格
            //设置实体属性
            PropertyInfo[] infos = type.GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                RowDefinition row=new RowDefinition();
                EntityGrid.RowDefinitions.Add(row);
                PropertyInfo pinfo = infos[i];
                if (pinfo.Name == "EntityKey") continue;
                Label lable = new Label();
                lable.Content=pinfo.Name+":";
                lable.Margin = new Thickness(0, 10, 0, 0);
                EntityGrid.Children.Add(lable);                
                lable.SetValue(Grid.ColumnProperty, 0);
                lable.SetValue(Grid.RowProperty, i);

                //EntityGrid.
                //LeftLabelArea.Children.Add(lable);
                //pinfo.Name;
                //pinfo.GetValue(obj, null);
                TextBox txtBox = new TextBox();
                txtBox.Name = pinfo.Name.Trim();
                txtBox.Margin = new Thickness(0, 10, 0, 0);                
                string strPinfoValue = string.Empty;
                if (pinfo.GetValue(obj, null) != null)
                {
                    strPinfoValue = pinfo.GetValue(obj, null).ToString();
                }
                txtBox.Text = strPinfoValue;
                //RightDataArea.Children.Add(txtBox);               
                EntityGrid.Children.Add(txtBox);
                txtBox.SetValue(Grid.ColumnProperty, 1);
                txtBox.SetValue(Grid.RowProperty, i);   
            }
        }

        public virtual void initAddEntityForm(ref object obj)
        {
            //StackPanel.FindName("Type") as TextBox;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();            
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Type type = entity.GetType();
            //设置实体属性
            PropertyInfo[] infos = type.GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                PropertyInfo pinfo = infos[i];
                TextBox tempbox=this.FindName(pinfo.Name) as TextBox;
                if ( tempbox!= null)
                {
                    
                    pinfo.SetValue(entity, tempbox.Text,null);
                }
            }
        }
    }
}

