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
using System.Windows.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SMT.FB.UI.Test
{
    public partial class DataGridGrouping : UserControl
    {
        public DataGridGrouping()
        {
            InitializeComponent();
            // Create a collection to store task data.
            ObservableCollection<Task> taskList = new ObservableCollection<Task>();
            // Generate some task data and add it to the task list.
            for (int i = 1; i <= 14; i++)
            {
                taskList.Add(new Task()
                {
                    ProjectName = "Project " + ((i % 3) + 1).ToString(),
                    TaskName = "Task " + i.ToString(),
                    DueDate = DateTime.Now.AddDays(i),
                    Complete = (i % 2 == 0),
                    Notes = "Task " + i.ToString() + " is due on "
                          + DateTime.Now.AddDays(i) + ". Lorum ipsum..."
                });
            }
            
            PagedCollectionView taskListView = new PagedCollectionView(taskList);
            this.dataGrid1.ItemsSource = taskListView;
            if (taskListView.CanGroup == true)
            {
                // Group tasks by ProjectName...
                taskListView.GroupDescriptions.Add(new PropertyGroupDescription("ProjectName"));
                // Then group by Complete status.
                taskListView.GroupDescriptions.Add(new PropertyGroupDescription("Complete"));
            }
            if (taskListView.CanSort == true)
            {
                // By default, sort by ProjectName.
                taskListView.SortDescriptions.Add(new SortDescription("ProjectName", ListSortDirection.Ascending));
            }
            
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PagedCollectionView pcv = this.dataGrid1.ItemsSource as PagedCollectionView;
            if (pcv != null && pcv.CanFilter == true)
            {
                // Apply the filter.
                pcv.Filter = new Predicate<object>(FilterCompletedTasks);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PagedCollectionView pcv = this.dataGrid1.ItemsSource as PagedCollectionView;
            if (pcv != null)
            {
                // Remove the filter.
                pcv.Filter = null;
            }
        }

        public bool FilterCompletedTasks(object t)
        {
            Task task = t as Task;
            return (task.Complete == false);
        }
        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            PagedCollectionView pcv = dataGrid1.ItemsSource as PagedCollectionView;
            try
            {
                foreach (CollectionViewGroup group in pcv.Groups)
                {
                    dataGrid1.ExpandRowGroup(group, true);
                }
            }
            catch (Exception ex)
            {
                // Could not expand group.
                MessageBox.Show(ex.Message);
            }
        }

        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            PagedCollectionView pcv = dataGrid1.ItemsSource as PagedCollectionView;
            try
            {
                foreach (CollectionViewGroup group in pcv.Groups)
                {
                    dataGrid1.ScrollIntoView(group, null);
                    dataGrid1.CollapseRowGroup(group, true);
                }
            }
            catch (Exception ex)
            {
                // Could not collapse group.
                MessageBox.Show(ex.Message);
            }
        }

        private void MainPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			List<DataTest> list = new List<DataTest>();
			
			DataTest t1 = new DataTest();
			t1.aa = "aa1";
			t1.bb = "bb1";				
		}


        public class DataTest
        {
            public string aa { get; set; }
            public string bb { get; set; }
        }
 
    }

    public class Task : System.ComponentModel.INotifyPropertyChanged, IEditableObject
    {
        // The Task class implements INotifyPropertyChanged and IEditableObject 
        // so that the datagrid can properly respond to changes to the 
        // data collection and edits made in the DataGrid.
        
        // Private task data.
        private string m_ProjectName = string.Empty;
        private string m_TaskName = string.Empty;
        private DateTime m_DueDate = DateTime.Now;
        private bool m_Complete = false;
        private string m_Notes = string.Empty;

        // Data for undoing canceled edits.
        private Task temp_Task = null;
        private bool m_Editing = false;

        // Public properties.
        [Display(Name = "Project")]
        public string ProjectName
        {
            get { return this.m_ProjectName; }
            set
            {
                if (value != this.m_ProjectName)
                {
                    this.m_ProjectName = value;
                    NotifyPropertyChanged("ProjectName");
                }
            }
        }

        [Display(Name = "Task")]
        public string TaskName
        {
            get { return this.m_TaskName; }
            set
            {
                if (value != this.m_TaskName)
                {
                    this.m_TaskName = value;
                    NotifyPropertyChanged("TaskName");
                }
            }
        }

        [Display(Name = "Due Date")]
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

        // Implement IEditableObject interface.
        public void BeginEdit()
        {
            if (m_Editing == false)
            {
                temp_Task = this.MemberwiseClone() as Task;
                m_Editing = true;
            }
        }

        public void CancelEdit()
        {
            if (m_Editing == true)
            {
                this.ProjectName = temp_Task.ProjectName;
                this.TaskName = temp_Task.TaskName;
                this.DueDate = temp_Task.DueDate;
                this.Complete = temp_Task.Complete;
                this.Notes = temp_Task.Notes;
                m_Editing = false;
            }
        }

        public void EndEdit()
        {
            if (m_Editing == true)
            {
                temp_Task = null;
                m_Editing = false;
            }
        }

    }


}
