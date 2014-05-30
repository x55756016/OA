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
using System.Windows.Printing;
using System.Windows.Data;
using System.Windows.Controls.Data;
using System.Windows.Browser;

namespace SMT.SaaS.CommonPrint
{
    public partial class MainPage : UserControl
    {        
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private void OK_buton(object sender, System.Windows.RoutedEventArgs e)
        {
            dataGrid1.PrintForm();
        }
  
        void MainPage_Loaded(object sender, RoutedEventArgs e) 
        {
            IList<orderEntity> list = this.GetDataSource();
            PagedCollectionView pcv = new PagedCollectionView(list);
            pcv.PageSize = 10;
            this.dataGrid1.ItemsSource = pcv;
            this.dataPager1.DataContext = pcv;                                   
        }

        private IList<orderEntity> GetDataSource()
        {
            IList<orderEntity> entitys = new List<orderEntity>();
            for (int i = 0; i < 60; i++)
            {
                orderEntity entity = new orderEntity();
                entity.Name = "余德森";
                entity.Phone = "13751031413";
                entity.Sex = "男";
                entity.Email = "sam_man@163.com";
                entitys.Add(entity);
            }
            return entitys;
        }

        public class orderEntity
        {
            public string Name { set;get; }

            public string Phone { set; get; }

            public string Sex { set; get; }

            public string Email { set; get; }
        }

        private void OK1_buton(object sender, RoutedEventArgs e)
        {
            IList<orderEntity> list = this.GetDataSource();
            PagedCollectionView pcv = new PagedCollectionView(list);
            dataGrid1.PrintGrid(pcv);
        }

        private void OK2_buton(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test");
        }
    }
}
