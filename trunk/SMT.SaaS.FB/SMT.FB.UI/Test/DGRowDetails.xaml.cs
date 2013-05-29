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
using System.ComponentModel;

namespace SMT.FB.UI.Test
{
    public partial class DGRowDetails : UserControl
    {
        public DGRowDetails()
        {
            InitializeComponent();
            // Create a list to store task data.
            List<Task> taskList = new List<Task>();
            int itemsCount = 15;

            // Generate some task data and add it to the task list.
            for (int i = 1; i <= itemsCount; i++)
            {
                taskList.Add(new Task()
                {
                    Name = "Task " + i.ToString(),
                    DueDate = DateTime.Now.AddDays(i),
                    Complete = (i % 3 == 0),
                    Notes = "Task " + i.ToString() + " is due on "
                          + DateTime.Now.AddDays(i) + ". Lorum ipsum..."
                });
            }
            this.dataGrid1.ItemsSource = taskList;
        }

        // Set the row details visibility to the option selected in the combo box.
        private void cbRowDetailsVis_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;
            if (this.dataGrid1 != null)
            {
                if (cbi.Content.ToString() == "Selected Row (Default)")
                    dataGrid1.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
                else if (cbi.Content.ToString() == "None")
                    dataGrid1.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                else if (cbi.Content.ToString() == "All")
                    dataGrid1.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Visible;
            }
        }
        // Freeze the row details if the check box is checked.
        private void cbFreezeRowDetails_Changed(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (this.dataGrid1 != null)
                this.dataGrid1.AreRowDetailsFrozen = (bool)cb.IsChecked;
        }



        public class Task : System.ComponentModel.INotifyPropertyChanged
        {
            // The Task class implements INotifyPropertyChanged so that 
            // the datagrid row will be notified of changes to the data
            // that are made in the row details section.

            // Private task data.
            private string m_Name;
            private DateTime m_DueDate;
            private bool m_Complete;
            private string m_Notes;

            // Define the public properties.
            public string Name
            {
                get { return this.m_Name; }
                set
                {
                    if (value != this.m_Name)
                    {
                        this.m_Name = value;
                        NotifyPropertyChanged("Name");
                    }
                }
            }

            public DateTime DueDate
            {
                get { return this.m_DueDate; }
                set
                {
                    if (value != this.m_DueDate)
                    {
                        this.m_DueDate = value;
                        NotifyPropertyChanged("DueDate");
                    }
                }
            }

            public bool Complete
            {
                get { return this.m_Complete; }
                set
                {
                    if (value != this.m_Complete)
                    {
                        this.m_Complete = value;
                        NotifyPropertyChanged("Complete");
                    }
                }
            }

            public string Notes
            {
                get { return this.m_Notes; }
                set
                {
                    if (value != this.m_Notes)
                    {
                        this.m_Notes = value;
                        NotifyPropertyChanged("Notes");
                    }
                }
            }

            // Implement INotifyPropertyChanged interface.
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

    }
}
