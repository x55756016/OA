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
using System.Collections.Generic;

namespace SMT.SaaS.FrameworkUI.RichNotepad
{
    public class Customer
    {
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Address { get; set; }
        public Boolean IsNew { get; set; }

        // A null value for IsSubscribed can indicate 
        // "no preference" or "no response".
        public Boolean? IsSubscribed { get; set; }

        public Customer() { }

        public Customer(String firstName, String lastName,
            String address, Boolean isNew, Boolean? isSubscribed)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Address = address;
            this.IsNew = isNew;
            this.IsSubscribed = isSubscribed;
        }

        public static List<Customer> GetSampleCustomerList()
        {
            return new List<Customer>(new Customer[4] {
                new Customer("A.", "Zero", 
                    "12 North Third Street, Apartment 45", 
                    false, true), 
                new Customer("B.", "One", 
                    "34 West Fifth Street, Apartment 67", 
                    false, false),
                new Customer("C.", "Two", 
                    "56 East Seventh Street, Apartment 89", 
                    true, null),
                new Customer("D.", "Three", 
                    "78 South Ninth Street, Apartment 10", 
                    true, true)
            });
        }
    }

}