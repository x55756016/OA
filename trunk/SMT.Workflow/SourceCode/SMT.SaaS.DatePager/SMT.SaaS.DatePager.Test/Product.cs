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
using System.ComponentModel;

namespace SMT.SaaS.DatePager.Test
{
    public class Product : INotifyPropertyChanged
    {

        private string id;
        public string ID
        {
            get { return id; }
            set { id = value; NotifyPropertyChanged("ID"); }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged("Name"); }
        }

        private int price;
        public int Price
        {
            get { return price; }
            set { price = value; NotifyPropertyChanged("Price"); }
        }

        private string category;
        public string Category
        {
            get { return category; }
            set { category = value; NotifyPropertyChanged("Category"); }
        }

        private string info;
        public string Info
        {
            get { return info; }
            set { info = value; NotifyPropertyChanged("Info"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


    }
}
