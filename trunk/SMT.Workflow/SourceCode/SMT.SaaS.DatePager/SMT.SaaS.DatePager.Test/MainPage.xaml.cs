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
using System.Collections.ObjectModel;

namespace SMT.SaaS.DatePager.Test
{
    public partial class MainPage : UserControl
    {
        private ObservableCollection<Product> products;
        private int count = 2;
        public MainPage()
        {
            InitializeComponent();
           
            this.Pager.Click += new SmtPager.PagerButtonClick(Pager_Click);
            this.Pager.PageCount = count;
            InitData(1, this.productList);
        }

        void Pager_Click(object sender, RoutedEventArgs e)
        {          
            if (tcPersonalRd == null)
            {
                return;
            }

            string strHeaderText = ((TabItem)this.tcPersonalRd.SelectedItem).Header.ToString();
            switch (strHeaderText)
            {
                case "未提交":  
                    InitData(this.Pager.PageIndex, this.productList);
                    break;
                case "审核中":                  
                    InitData(this.Pager.PageIndex, this.productList1);
                    break;
                case "审核通过":                  
                    InitData(this.Pager.PageIndex, this.productList2);
                    break;
                case "审核未通过":
                    InitData(this.Pager.PageIndex, this.productList3);
                    break;
                case "转发":
                    InitData(this.Pager.PageIndex, this.productList4);
                    break;
                default:               
                    InitData(this.Pager.PageIndex, this.productList);
                    break;
            }
        }

        private void InitData(int index,DataGrid dg)
        {           
            products = new ObservableCollection<Product>();
            products.Clear();
            for (int i = 0; i < 15; i++)
            {
                products.Add(new Product()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = "第" + index + "页",
                    Price = i,
                    Category = "神州通",
                    Info = "在线"
                });
            }
           
            dg.ItemsSource = null;
            dg.ItemsSource = products;
        }
        private void tcPersonalRd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {       
            if (tcPersonalRd == null)
            {
                return;
            }

            string strHeaderText = ((TabItem)this.tcPersonalRd.SelectedItem).Header.ToString();
            switch (strHeaderText)
            {
                case "未提交":
                    count = 2;
                    this.Pager.PageCount = count;
                    InitData(1, this.productList);
                    break;
                case "审核中":
                    count = 6;
                    this.Pager.PageCount = count;
                    InitData(1, this.productList1);
                    break;
                case "审核通过":
                    count = 11;
                    this.Pager.PageCount = count;
                    InitData(1, this.productList2);
                    break;
                case "审核未通过":
                    count = 5;
                    this.Pager.PageCount = count;
                    InitData(1, this.productList3);
                    break;
                case "转发":
                    count = 1;
                    this.Pager.PageCount = count;
                    InitData(1, this.productList4);
                    break;
                default:
                    count = 2;
                    this.Pager.PageCount = count;
                    InitData(1, this.productList);
                    break;
            }
        }
    }
}
