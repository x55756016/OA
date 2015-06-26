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
using System.Windows.Media.Imaging;
namespace SMT.FileUpLoad
{
    public partial class FileUploadRowControl : UserControl
    {
        private bool imageSet;
        private SMT.Saas.Tools.NewFileUploadWS.UploadServiceClient _client;
        public FileUploadRowControl()
        {
            imageSet = false;
            InitializeComponent();
            _client = new SMT.Saas.Tools.NewFileUploadWS.UploadServiceClient();
            _client.DeleteFileCompleted += new EventHandler<SMT.Saas.Tools.NewFileUploadWS.DeleteFileCompletedEventArgs>(_client_DeleteFileCompleted);
            removeButton.Click += new RoutedEventHandler(removeButton_Click);
            Loaded += new RoutedEventHandler(FileUploadRowControl_Loaded);
        }
        //WCF删除
        void _client_DeleteFileCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.DeleteFileCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    string txt = e.Result;
                }
            }
        }

        void FileUploadRowControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            UserFile fu = DataContext as UserFile;
            if (fu.NotShowDeleteButton)
            {
                this.removeButton.Visibility = Visibility.Collapsed;
            }
            fu.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(fu_PropertyChanged);
            LoadImage(fu);
        }
        //点击上传，删除按钮发生
        void fu_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UserFile fu = sender as UserFile;
          
            if (e.PropertyName == "DisplayThumbnail")
            {                
                LoadImage(fu);
            }
            else if (e.PropertyName == "Status")
            {
                bool showtimeleft = false;
                switch (fu.State)
                {
                    case Constants.FileStates.Pending:
                        VisualStateManager.GoToState(this, "选中", true);
                        break;
                    case Constants.FileStates.Uploading:
                        VisualStateManager.GoToState(this, "上传中", true);
                        break;
                    case Constants.FileStates.Finished:
                        VisualStateManager.GoToState(this, "完成", true);
                        break;
                    case Constants.FileStates.Error:
                        VisualStateManager.GoToState(this, "错误", true);
                        break;
                    //case Constants.FileStates.Canceled:
                    //    VisualStateManager.GoToState(this, "选中", true);
                    //    break;
                    //case Constants.FileStates.Removed:
                    //    VisualStateManager.GoToState(this, "选中", true);
                    //    break;
                    //case Constants.FileStates.Resizing:
                    //    VisualStateManager.GoToState(this, "调整大小", true);
                    //    break;
                    default:
                        break;
                }
                TbName.Text = ConstantsCN.CN(fu.State);
            }
        }

        private void LoadImage(UserFile fu)
        {

            if (fu != null && fu.DisplayThumbnail && (fu.FileName.ToLower().EndsWith("jpg") || fu.FileName.ToLower().EndsWith("png")))
            {
                if (!imageSet)
                {
                    BitmapImage imageSource = new BitmapImage();
                    try
                    {
                        imageSource.SetSource(fu.FileStream);
                        imagePreview.Source = imageSource;
                        imageSet = true;
                        imagePreview.Visibility = Visibility.Visible;
                    }
                    catch (Exception e)
                    {
                        string message = e.Message;
                    }
                }
                else
                    imagePreview.Visibility = Visibility.Visible;
            }
            else
                imagePreview.Visibility = Visibility.Collapsed;
        }

        void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("你确定删除吗?", "删除文件：", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                UserFile fu = DataContext as UserFile;
                if (fu != null)
                {
                    fu.IsDeleted = true;
                    this.Visibility = Visibility.Collapsed;
                }
                _client.DeleteFileAsync(fu.SmtFileListId, fu.FileUrl);//WCF删除
            }

            
          //  fu.RemoveUpload();             
        }
    }
}
