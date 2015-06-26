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
using System.IO;
namespace SMT.FileUpLoad
{

    public partial class FileRowControl : UserControl
    {
        delegate void dSaveDlg(byte[] b);
        dSaveDlg ds;
        
        private UserFile _UserFile
        {
            get {
                if (this.DataContext != null)
                    return ((UserFile)this.DataContext);
                else return null;
            }
        }
      
        public FileRowControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(FileRowControl_Loaded);
        }
        void FileRowControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (this._UserFile.State == Constants.FileStates.DataLoaded)
            //{
            //    FileSizeTextBlock.Visibility = Visibility.Collapsed;
            //    ShowValidIcon();
            //}
            ////定制UserFile的PropertyChanged 属性，如BytesUploaded，Percentage，IsDeleted 控制 界面的先 字体颜色的变动
            //_UserFile.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(FileRowControl_PropertyChanged);
        }
        void FileRowControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        { 
            if (e.PropertyName == "State")
            {
                ////当前文件上传完毕后显示灰字
                //if (this._UserFile.State == Constants.FileStates.Finished )
                //    ShowValidIcon();                
                //if (this._UserFile.State == Constants.FileStates.Deleteing)
                //    this.Visibility = Visibility.Collapsed;
                ////如上传失败显示错误信息
                //if (this._UserFile.State == Constants.FileStates.Error)
                //    ErrorMsgTextBlock.Visibility = Visibility.Visible;
            }
        }
      
       
        /// <summary>
        ///  移除单个上传文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            //UserFile file = (UserFile)((Button)e.OriginalSource).DataContext;
            //if (file.State == Constants.FileStates.Pending)
            //    file.State = Constants.FileStates.Remove;
            //else
            //    // 逻辑删除 true 表示是 btnupload隐藏
            //        file.State = Constants.FileStates.Deleteing;
        }
        //下载
        private void hbtn_url_Click(object sender, RoutedEventArgs e)
        {
            //_UserFile.Ev_Download += new EventHandler<FileDownloadEventArgs>(Ev_Download);
            //_UserFile.Download();
            //dsave();

        }
        bool b = true;
        private void dsave()
        {
            SaveFileDialog sDlg;
            sDlg = new SaveFileDialog();
            int i = this._UserFile.FileName.LastIndexOf('.');
            string sufix = "*" + this._UserFile.FileName.Substring(i);
            sDlg.Filter = "(" + sufix + ")|" + sufix + "|(.txt)|*.txt|All Files|*.*";
            sDlg.FilterIndex = 1;
            sDlg.DefaultExt = sufix;

            bool? ret = sDlg.ShowDialog();
            
            if (ret == true)
            {
                while (b){
                    if (_by != null)
                    {                        
                        using (Stream fs = (Stream)sDlg.OpenFile())
                        {
                            byte[] info = _by;
                            fs.Write(info, 0, info.Length);
                            fs.Close();
                        }
                        b = false;
                    }
                    System.Threading.Thread.Sleep(100);
                }                
            }
        }
        byte[] _by;
        //private void Ev_Download(object sender, FileDownloadEventArgs e)
        //{
        //    if (e.byFile == null)
        //        b = false;
        //    else
        //    _by = e.byFile;
        //}
    }
   
}