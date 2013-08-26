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
using System.Collections.ObjectModel;

namespace SMT.Workflow.Platform.Designer.Common
{
    public class TreeItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _id;
        private string _name;
        private int _levelId;
        private string _parentId;
        private string _modelCode;
        private string _modelName;
        private ObservableCollection<TreeItem> _children;

        public TreeItem()
        {
            _children = new ObservableCollection<TreeItem>();
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public string ID
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        public int LevelID
        {
            get { return _levelId; }
            set
            {
                if (value != _levelId)
                {
                    _levelId = value;
                    NotifyPropertyChanged("LevelID");
                }
            }
        }

        public string ParentID
        {
            get { return _parentId; }
            set
            {
                if (value != _parentId)
                {
                    _parentId = value;
                    NotifyPropertyChanged("ParentID");
                }
            }
        }

        public ObservableCollection<TreeItem> Children
        {
            get { return _children; }
            set
            {
                if (value != _children)
                {
                    _children = value;
                    NotifyPropertyChanged("Children");
                }
            }
        }
        public string ModelCode
        {
            get { return _modelCode; }
            set
            {
                if (value != _modelCode)
                {
                    _modelCode = value;
                    NotifyPropertyChanged("ModelCode");
                }
            }
        }
        public string ModelName
        {
            get { return _modelName; }
            set
            {
                if (value != _modelName)
                {
                    _modelName = value;
                    NotifyPropertyChanged("ModelName");
                }
            }
        }
        public void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
