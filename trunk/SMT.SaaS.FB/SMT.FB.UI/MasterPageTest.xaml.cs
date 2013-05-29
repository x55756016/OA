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
using SMT.FB.UI.Common;
using System.Windows.Controls.Primitives;

namespace SMT.FB.UI
{
    public partial class MasterPageTest : UserControl
    {
        public MasterPageTest()
        {
            InitializeComponent();
            Test(); 
        }

        public void Test()
        {
   
            List<MonthItem> list = new List<MonthItem>()
             {
                 new MonthItem()
                 {
                     Text = "2011-11", Value = "11"
                 },
                 new MonthItem()
                 {
                     Text = "2011-12", Value = "12"
                 }
             };

            myTest.ItemsSource = list;
            myTest.DisplayMemberPath = "Text";
            myTest.SelectedItem = new MonthItem()
                 {
                     Text = "2011-08",
                     Value = "08"
                 };

            myCbb.ItemsSource = list;
            myCbb.SelectionChanged += new SelectionChangedEventHandler(myCbb_SelectionChanged);
            //list.ForEach( item =>
            //    {
            //        myCbb.Items.AddObject<MonthItem>(item, "Text");

            
                    
            //    });
            //ComboBoxItem cbi = new ComboBoxItem();
            //cbi.Visibility = System.Windows.Visibility.Collapsed;
            //cbi.
            //myCbb.Items.Add(
            
            
            
            myCbb.SelectedItem = myTest.SelectedItem;
            myCbb.DisplayMemberPath = "Text";
            myCbb.MouseLeftButtonDown += new MouseButtonEventHandler(myCbb_MouseLeftButtonDown);
        }

        void myCbb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.OriginalSource.GetType().Name);
        }

        void myCbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            var a = e.AddedItems;
        }
        

       
    }
}
