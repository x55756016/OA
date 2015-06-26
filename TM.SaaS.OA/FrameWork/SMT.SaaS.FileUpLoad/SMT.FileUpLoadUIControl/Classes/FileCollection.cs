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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Browser;
using System.Collections.Generic;



namespace SMT.FileUpLoad
{
    [ScriptableType]
    public class FileCollection : ObservableCollection<UserFile>
    {
        private double _bytesUploaded = 0;
        private int _percentage = 0;
        /// <summary>
        /// 当前上传文件的数量
        /// </summary>
        private int _currentUpload = 0;
        private string _customParams;
        private int _maxUpload;
        private int _totalUploadedFiles = 0;

        

        public double BytesUploaded
        {
            get { return _bytesUploaded; }
            set
            {
                _bytesUploaded = value;

                this.OnPropertyChanged(new PropertyChangedEventArgs("BytesUploaded"));
            }
        }

        [ScriptableMember()]
        public string CustomParams
        {
            get { return _customParams; }
            set
            {
                _customParams = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("CustomParams"));
            }
        }


        [ScriptableMember()]
        public int TotalFilesSelected
        {
            get { return this.Items.Count; }           
        }

        [ScriptableMember()]
        public int Percentage
        {
            get { return _percentage; }
            set
            {
                _percentage = value;

                this.OnPropertyChanged(new PropertyChangedEventArgs("Percentage"));   
            
                //Notify Javascript of percentage change
                if (TotalPercentageChanged != null)
                    TotalPercentageChanged(this, null);
            }
        }

        [ScriptableMember()]
        public int TotalUploadedFiles
        {
            get { return _totalUploadedFiles; }
            set
            {
                _totalUploadedFiles = value;

                this.OnPropertyChanged(new PropertyChangedEventArgs("TotalUploadedFiles"));
            }
        }

        [ScriptableMember()]
        public IList<UserFile> FileList
        {
            get { return this.Items; }
        }

        [ScriptableMember()]
        public event EventHandler SingleFileUploadFinished;

        [ScriptableMember()]
        public event EventHandler AllFilesFinished;

        [ScriptableMember()]
        public event EventHandler ErrorOccurred;

        [ScriptableMember()]
        public event EventHandler FileAdded;

        [ScriptableMember()]
        public event EventHandler FileRemoved;

        [ScriptableMember()]
        public event EventHandler StateChanged;

        [ScriptableMember()]
        public event EventHandler TotalPercentageChanged;

        /// <summary>
        /// FileCollection constructor
        /// </summary>
        /// <param name="customParams"></param>
        /// <param name="maxUploads">最大允许上传个数</param>
        public FileCollection(string customParams, int maxUploads)
        {
            _customParams = customParams;
            _maxUpload = maxUploads;

            this.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(FileCollection_CollectionChanged);
            
        }

       
        /// <summary>
        /// Add a new file to the file collection
        /// </summary>
        /// <param name="item"></param>
        public new void Add(UserFile item)
        {
            //Listen to the property changed for each added item
            item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);

            base.Add(item);

            if (FileAdded != null)
                FileAdded(this, null);
        }

        /// <summary>
        /// Removed an existing user file to the collection
        /// </summary>
        /// <param name="item"></param>
        public new void Remove(UserFile item)
        {
            base.Remove(item);

            if (FileRemoved != null)
                FileRemoved(this, null);
        }

        /// <summary>
        /// Clears the complete list
        /// </summary>
        public new void Clear()
        {
            base.Clear();

            if (FileRemoved != null)
                FileRemoved(this, null);
        }

        /// <summary>
        ///开始上传所有文件 Start uploading files
        /// </summary>
        public void UploadFiles()
        {
            lock (this)
            {
                foreach (UserFile file in this)
                {
                    if (!file.IsDeleted && file.State == Constants.FileStates.Pending && _currentUpload < _maxUpload)
                    //if (!file.IsDeleted && (file.State == Constants.FileStates.Pending ||file.State == Constants.FileStates.Removed)&& _currentUpload < _maxUpload)
                    {
                        file.Upload(_customParams);
                        _currentUpload++;
                    }
                }
            }


        }
        /// <summary>
        /// 取消所有上传文件(龙康才加)
        /// </summary>
        public void CancelUpload()
        {
            lock (this)
            {
                _currentUpload = 0;
                foreach (UserFile file in this)
                {
                    file.CancelUpload();                   
                }
            }            
        }

        

        /// <summary>
        /// Recount statistics
        /// </summary>
        private void RecountTotal()
        {
            //Recount total
            double totalSize = 0;
            double totalSizeDone = 0;

            foreach (UserFile file in this)
            {
                totalSize += file.FileSize;
                totalSizeDone += file.BytesUploaded;
            }

            double percentage = 0;

            if (totalSize > 0)
                percentage = 100 * totalSizeDone / totalSize;

            BytesUploaded = totalSizeDone;

            Percentage = (int)percentage;
        }

        /// <summary>
        /// Check if all files are finished uploading
        /// </summary>
        private void AreAllFilesFinished()
        {
            if (Percentage == 100)
            {        
                if (AllFilesFinished != null)
                    AllFilesFinished(this, null);
            }
        }

        /// <summary>
        ///文件集合数的改变发 The collection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Recount total when the collection changed (items added or deleted)
            RecountTotal();
        }

        /// <summary>
        /// Property of individual item changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Check if deleted property is changed
            if (e.PropertyName == "IsDeleted")
            {
                UserFile file = (UserFile)sender;

                if (file.IsDeleted)
                {
                    if (file.State == Constants.FileStates.Uploading)
                    {
                        _currentUpload--;
                        UploadFiles();
                    }

                    this.Remove(file);

                    file = null;
                }
            }
            else if (e.PropertyName == "State")
            {
                UserFile file = (UserFile)sender;
                if (file.State == Constants.FileStates.Finished)
                {
                    _currentUpload--;
                    TotalUploadedFiles++;

                    UploadFiles();

                    if (SingleFileUploadFinished != null)
                        SingleFileUploadFinished(this, null);                    
                   
                }
                else if (file.State == Constants.FileStates.Error)
                {
                    _currentUpload--;

                    UploadFiles();

                    if (ErrorOccurred != null)
                        ErrorOccurred(this, null);
                }

                if (StateChanged != null)
                    StateChanged(this, null);

                AreAllFilesFinished();


            }
            else if (e.PropertyName == "BytesUploaded")
            {
                RecountTotal();
            }
        }
     
    }
}
