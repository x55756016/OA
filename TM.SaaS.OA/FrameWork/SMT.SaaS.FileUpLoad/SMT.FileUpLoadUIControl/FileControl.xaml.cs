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
using SMT.Saas.Tools.NewFileUploadWS;
using System.Windows.Data;
using SMT.FileUpLoad.Classes;
using System.Collections.ObjectModel;
using System.Windows.Browser;
using SMT.Saas.Tools.NewFileUploadWS;
using System.ComponentModel;
namespace SMT.FileUpLoad
{
    public partial class FileControl : UserControl, INotifyPropertyChanged
    {
        public event EventHandler GetFileComplete;

        ObservableCollection<SMT_FILELIST> itemSource;
        private string systemCode;
        /// <summary>
        /// 系统代号
        /// </summary>
        [Category("SystemCode")]
        [Description("系统编码")]
        [Bindable(true)]
        public string SystemCode
        {
            get { return systemCode; }
            set { systemCode = value; }
        }
        private string modeCode;
        /// <summary>
        /// 模块代号
        /// </summary>
        [Category("ModelCode")]
        [Description("模块编码")]
        [Bindable(true)]
        public string ModelCode
        {
            get { return modeCode; }
            set { modeCode = value; }
        }
        private string applicationid;
        /// <summary>
        /// 业务系统ID
        /// </summary>
        [Category("ApplicationID")]
        [Description("上传控件对应的表单Id")]
        [Bindable(true)]
        public string ApplicationID
        {
            get{return applicationid;}
            set { applicationid=value;

            if (PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("applicationid"));
            }
            }
        }

        private bool autoInit;
        /// <summary>
        /// 业务系统ID
        /// </summary>
        [Category("AutoInit")]
        [Description("自动初始化")]
        [Bindable(true)]
        public bool AutoInitReadOnlyControl
        {
            get { return autoInit; }
            set { autoInit = value;}
        }

       

        //Create the binding description
        public static readonly DependencyProperty ApplictionIDProperty =
           DependencyProperty.Register("applicationid", typeof(string), typeof(FileControl),null);
        public string FormId
        {
            get { return (string)GetValue(ApplictionIDProperty); }
            set { SetValue(ApplictionIDProperty, value); }
        }
        /// <summary>
        /// 用户ID
        /// </summary>
        public Binding ApplicationIdBinding
        {
            get { return null; }
            set
            {             
                //txtAppliction = value;

                //Binding bind = new Binding();
                //bind.Source = scroll;
                //bind.Path = new PropertyPath(ScrollBar.ValueProperty);
                SetBinding(ApplictionIDProperty, value);
                //txtAppliction.Source = this.applicationid;
                //ApplicationID = txtAppliction.Text;
            }
        }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID
        {
            get;
            set;
        }
        /// <summary>
        /// 是否有附件
        /// </summary>
        public bool HasAccessory
        {
            get;
            set;
        }

        //private bool isfileReadOnly = false;
        //[Category("ReadOnly")]
        //[Description("设置上传控件只读模式并初始化")]
        //[Bindable(true)]
        //public bool isFileReadOnly
        //{
        //    get { return isfileReadOnly; }
        //    set
        //    {
        //        isfileReadOnly = value;
        //    }
        //}

        /// <summary>
        /// 是否把公司代号作为限制查询条件
        /// </summary>
        public bool IsLimitCompanyCode = true;
        public UserConfig usc;
        private SMT.Saas.Tools.NewFileUploadWS.UploadServiceClient Client;
       public FileControl()
        {
            InitializeComponent();

            try
            {
                btnUp.Click += new RoutedEventHandler(btnUp_Click);
                Client = new SMT.Saas.Tools.NewFileUploadWS.UploadServiceClient();
                //Client.ListUpLoadFileCompleted += new EventHandler<SMT.Saas.Tools.NewFileUploadWS.ListUpLoadFileCompletedEventArgs>(Client_ListUpLoadFileCompleted);
                #region 获取文件列表 完成事件
                Client.GetFileListByApplicationIDCompleted += new EventHandler<GetFileListByApplicationIDCompletedEventArgs>(Client_GetFileListByApplicationIDCompleted);
                Client.GetFileListByCompanyCodeCompleted += new EventHandler<GetFileListByCompanyCodeCompletedEventArgs>(Client_GetFileListByCompanyCodeCompleted);
                Client.GetFileListByModelCodeCompleted += new EventHandler<GetFileListByModelCodeCompletedEventArgs>(Client_GetFileListByModelCodeCompleted);
                Client.GetFileListBySystemCodeCompleted += new EventHandler<GetFileListBySystemCodeCompletedEventArgs>(Client_GetFileListBySystemCodeCompleted);
                Client.GetFileListByOnlyApplicationIDCompleted += new EventHandler<GetFileListByOnlyApplicationIDCompletedEventArgs>(Client_GetFileListByOnlyApplicationIDCompleted);
                //Client.GetFileListByOnlyApplicationIDAsync("9847d30f-0272-4274-b1b7-b0b5ddb41349");
                #endregion
                #region 注册删除文件 完成事件
                Client.DeleteFileByUrlCompleted += new EventHandler<DeleteFileByUrlCompletedEventArgs>(Client_DeleteFileByUrlCompleted);
                Client.DeleteFileCompleted += new EventHandler<DeleteFileCompletedEventArgs>(Client_DeleteFileCompleted);
                Client.DeleteFileByApplicationIDCompleted += new EventHandler<DeleteFileByApplicationIDCompletedEventArgs>(Client_DeleteFileByApplicationIDCompleted);
                Client.DeleteFileByOnlyApplicationIDCompleted += new EventHandler<DeleteFileByOnlyApplicationIDCompletedEventArgs>(Client_DeleteFileByOnlyApplicationIDCompleted);
                Client.DeleteFileByCompanycodeCompleted += new EventHandler<DeleteFileByCompanycodeCompletedEventArgs>(Client_DeleteFileByCompanycodeCompleted);
                Client.DeleteFileByModelCodeCompleted += new EventHandler<DeleteFileByModelCodeCompletedEventArgs>(Client_DeleteFileByModelCodeCompleted);
                Client.DeleteFileBySystemCodeCompleted += new EventHandler<DeleteFileBySystemCodeCompletedEventArgs>(Client_DeleteFileBySystemCodeCompleted);
                Client.DeleteFileByApplicationIDAndFileNameCompleted += new EventHandler<DeleteFileByApplicationIDAndFileNameCompletedEventArgs>(Client_DeleteFileByApplicationIDAndFileNameCompleted);
            }
            catch (Exception ex)
            {
                string a = ex.ToString();
            }
            #endregion          
        }

       void Client_DeleteFileByApplicationIDAndFileNameCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.DeleteFileByApplicationIDAndFileNameCompletedEventArgs e)
       {
           if (e.Error == null)
           {
               if (e.Result != null)
               {
                   //1:删除成功；－1：文件不存在；其他：失败

                   if (e.Result == "1")
                   {
                       MessageBox.Show("删除成功！", "信息提示：", MessageBoxButton.OK);
                       BindDataGrid(itemSource);
                       //List.ItemsSource = itemSource;
                   }
                   else if (e.Result == "-1")
                   {
                       MessageBox.Show("文件不存在！", "信息提示：", MessageBoxButton.OK);
                       //List.ItemsSource = itemSource;
                   }
                   else
                   {
                       MessageBox.Show(e.Result, "信息提示：", MessageBoxButton.OK);
                   }
               }
           }
       }
       
       public void Init(string strSystemCode,string strModelCode,string strUserID,string strApplicationID)
       {
           SystemCode = strSystemCode;
           ModelCode = strModelCode;
           UserID = strUserID;
           ApplicationID = strApplicationID;
           //Client.ListUpLoadFileAsync(ApplicationID);
       }
       public void Init(UserConfig uc)
       {
           usc = uc;
           if (uc.NotShowUploadButton)
           {
               btnUp.Visibility = Visibility.Collapsed;
           }
           SystemCode = uc.SystemCode;
           ModelCode = uc.ModelCode;
           UserID = uc.UserID;
           ApplicationID = uc.ApplicationID;
           IsLimitCompanyCode = uc.IsLimitCompanyCode;
           Client.GetFileListByOnlyApplicationIDAsync(ApplicationID);
           //if (IsLimitCompanyCode)
           //{

           //    Client.GetFileListByApplicationIDAsync(uc.CompanyCode, uc.SystemCode, uc.ModelCode, uc.ApplicationID, uc.OnlyShowMySelf == true ? uc.CreateName : null);  //获取文件列表
           //}
           //else
           //{
           //    Client.GetFileListByApplicationIDAsync(string.Empty, uc.SystemCode, uc.ModelCode, uc.ApplicationID, uc.OnlyShowMySelf == true ? uc.CreateName : null);  //获取文件列表
           //}
       }
       #region 获取文件列表
       void Client_GetFileListByCompanyCodeCompleted(object sender, GetFileListByCompanyCodeCompletedEventArgs e)
       {
           if (e.Error == null)
           {
               if (e.Result != null)
               {

                   if (e.Result.FileList != null)
                   {
                       HasAccessory = true;
                       foreach (SMT_FILELIST file in e.Result.FileList)
                       {
                           string path = file.FILEURL;
                           string filename = path.Substring(path.LastIndexOf('\\') + 1);
                           string filepath = HttpUtility.UrlEncode(file.FILEURL + "\\" + file.FILENAME);
                           file.FILEURL = e.Result.DownloadUrl + "?filename=" + filepath;//文件地址
                       }
                       itemSource = e.Result.FileList;
                       BindDataGrid(itemSource);
                   }
               }
           }
           else
           {
               MessageBox.Show("网络出现错误请联系管理员");
           }
       }
       void Client_GetFileListByModelCodeCompleted(object sender, GetFileListByModelCodeCompletedEventArgs e)
       {
           if (e.Error == null)
           {
               if (e.Result != null)
               {

                   if (e.Result.FileList != null)
                   {
                       HasAccessory = true;
                       foreach (SMT_FILELIST file in e.Result.FileList)
                       {
                           string path = file.FILEURL;
                           string filename = path.Substring(path.LastIndexOf('\\') + 1);
                           string filepath = HttpUtility.UrlEncode(file.FILEURL + "\\" + file.FILENAME);
                           file.FILEURL = e.Result.DownloadUrl + "?filename=" + filepath;//文件地址
                       }
                       itemSource = e.Result.FileList;
                       BindDataGrid(itemSource);
                   }
                   else
                   {
                       HasAccessory = false;
                   }
               }
           }
           else
           {
               MessageBox.Show("网络出现错误请联系管理员");
           }
       }
       void Client_GetFileListBySystemCodeCompleted(object sender, GetFileListBySystemCodeCompletedEventArgs e)
       {
           if (e.Error == null)
           {
               if (e.Result != null)
               {

                   if (e.Result.FileList != null)
                   {
                       HasAccessory = true;
                       foreach (SMT_FILELIST file in e.Result.FileList)
                       {
                           string path = file.FILEURL;
                           string filename = path.Substring(path.LastIndexOf('\\') + 1);
                           string filepath = HttpUtility.UrlEncode(file.FILEURL + "\\" + file.FILENAME);
                           file.FILEURL = e.Result.DownloadUrl + "?filename=" + filepath;//文件地址
                       }
                       itemSource = e.Result.FileList;
                       BindDataGrid(itemSource);
                   }
                   else
                   {
                       HasAccessory = false;
                   }
               }
           }
           else
           {
               MessageBox.Show("网络出现错误请联系管理员");
           }
       }
       void Client_GetFileListByApplicationIDCompleted(object sender, GetFileListByApplicationIDCompletedEventArgs e)
       {
           if (e.Error == null)
           {
               if (e.Result != null)
               {

                   if (e.Result.FileList != null)
                   {
                       HasAccessory = true;
                       foreach (SMT_FILELIST file in e.Result.FileList)
                       {
                           string path = file.FILEURL;
                           string filename = path.Substring(path.LastIndexOf('\\') + 1);

                           string filepath = HttpUtility.UrlEncode(file.FILEURL + "\\" + file.FILENAME);
                           file.FILEURL = e.Result.DownloadUrl + "?filename=" + filepath;//文件地址
                           //string filepath = HttpUtility.UrlEncode(file.THUMBNAILURL + "\\" + file.FILENAME);
                           //file.FILEURL = e.Result.DownloadUrl + "?flag=1&filename=" + filepath;
                           //formid暂时没用处，用来存储fileurl信息
                           file.FORMID = path;
                       }
                       itemSource = e.Result.FileList;
                       BindDataGrid(itemSource);
                   }
                   else
                   {
                       HasAccessory = false;
                   }
               }
           }
           else
           {
               MessageBox.Show("网络出现错误请联系管理员");
           }
       }
       void Client_GetFileListByOnlyApplicationIDCompleted(object sender, GetFileListByOnlyApplicationIDCompletedEventArgs e)
       {
           try
           {
               if (e.Error == null)
               {
                   if (e.Result != null)
                   {

                       if (e.Result.FileList != null)
                       {
                           HasAccessory = true;
                           foreach (SMT_FILELIST file in e.Result.FileList)
                           {
                               string path = file.FILEURL;
                               string filename = path.Substring(path.LastIndexOf('\\') + 1);
                               string filepath = HttpUtility.UrlEncode(file.FILEURL + "\\" + file.FILENAME);
                               file.FILEURL = e.Result.DownloadUrl + "?filename=" + filepath;//文件地址
                               //string filepath = HttpUtility.UrlEncode(file.THUMBNAILURL + "\\" + file.FILENAME);
                               //file.FILEURL = e.Result.DownloadUrl + "?flag=1&filename=" + filepath;
                               file.FORMID = path;
                           }
                           itemSource = e.Result.FileList;
                           BindDataGrid(itemSource);
                       }
                       else
                       {
                           HasAccessory = false;
                       }
                   }
               }
               else
               {
                   MessageBox.Show("网络出现错误请联系管理员");
               }
           }
           catch (Exception ex)
           {
               SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(ex.ToString());
               SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
           }
           finally
           {
               if (GetFileComplete != null)
               {
                   this.GetFileComplete(this, EventArgs.Empty);
               }
           }
       }
       /// <summary>
       /// 获取文件列表(暂时不公司此接口)
       /// </summary>
       /// <param name="companycode">公司代号</param>
        private void GetFileListByCompanyCode(string companycode)
       {
           Client.GetFileListByCompanyCodeAsync(companycode);
       }
       /// <summary>
        /// 获取文件列表(暂时不公司此接口)
       /// </summary>
       /// <param name="companycode">公司代号</param>
       /// <param name="systemcode">系统代号</param>
        private void GetFileListBySystemCode(string companycode, string systemcode)
       {
           Client.GetFileListBySystemCodeAsync( companycode,  systemcode);
       }
       /// <summary>
        /// 获取文件列表(暂时不公司此接口)
       /// </summary>
       /// <param name="companycode">公司代号</param>
       /// <param name="systemcode">系统代号</param>
       /// <param name="modelcode">模块代号</param>
        private void GetFileListByModelCode(string companycode, string systemcode, string modelcode)
       {
           Client.GetFileListByModelCodeAsync(companycode, systemcode,modelcode);
       }
       /// <summary>
        /// 获取文件列表(暂时不公司此接口)
       /// </summary>
       /// <param name="companycode">公司代号</param>
       /// <param name="systemcode">系统代号</param>
       /// <param name="modelcode">模块代号</param>
       /// <param name="applicationid">业务ID</param>
        private void GetFileListByApplicationID(string companycode, string systemcode, string modelcode, string applicationid)
       {
           Client.GetFileListByApplicationIDAsync( companycode,  systemcode,  modelcode, applicationid,null); 
       }
       /// <summary>
       /// 获取文件列表(暂时不公司此接口)
       /// </summary>
       /// <param name="applicationid">业务ID</param>
       private void GetFileListByApplicationID(string applicationid)
       {
           Client.GetFileListByOnlyApplicationIDAsync(applicationid);
       }
       #endregion
       #region 删除文件
       #region 删除文件 删除文件 完成事件

       void Client_DeleteFileBySystemCodeCompleted(object sender, DeleteFileBySystemCodeCompletedEventArgs e)
       {
           if (e.Error != null)
           {
               MessageBox.Show("网络出现错误请联系管理员");
           }
       }
       void Client_DeleteFileByModelCodeCompleted(object sender, DeleteFileByModelCodeCompletedEventArgs e)
       {
           if (e.Error != null)
           {
               MessageBox.Show("网络出现错误请联系管理员");
           }
       }
       void Client_DeleteFileByCompanycodeCompleted(object sender, DeleteFileByCompanycodeCompletedEventArgs e)
       {
           if (e.Error != null)
           {
               MessageBox.Show("网络出现错误请联系管理员");
           }
       }

       void Client_DeleteFileByApplicationIDCompleted(object sender, DeleteFileByApplicationIDCompletedEventArgs e)
       {
       }
       void Client_DeleteFileCompleted(object sender, DeleteFileCompletedEventArgs e)
       {
           if (e.Error != null)
           {
               MessageBox.Show("网络出现错误请联系管理员");
           }
       }
       void Client_DeleteFileByOnlyApplicationIDCompleted(object sender, DeleteFileByOnlyApplicationIDCompletedEventArgs e)
       {
           if (e.Error != null)
           {
               MessageBox.Show("网络出现错误请联系管理员");
           }
       }
       void Client_DeleteFileByUrlCompleted(object sender, DeleteFileByUrlCompletedEventArgs e)
       {
           if (e.Error == null)
           {
               if (e.Result != null)
               {
                   //1:删除成功；－1：文件不存在；其他：失败

                   if (e.Result == "1")
                   {
                       MessageBox.Show("删除成功！", "信息提示：", MessageBoxButton.OK);
                       BindDataGrid(itemSource);
                       //List.ItemsSource = itemSource;
                   }
                   else if (e.Result == "-1")
                   {
                       MessageBox.Show("文件不存在！", "信息提示：", MessageBoxButton.OK);
                       //List.ItemsSource = itemSource;
                   }
                   else
                   {
                       MessageBox.Show(e.Result, "信息提示：", MessageBoxButton.OK);
                   }
               }
           }
       }
       #endregion
       #region 删除文件      
       /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="companycode">公司代号</param>
        /// <param name="systemcode">系统代号</param>
        /// <param name="modelcode">模块代号</param>
        /// <param name="applicationid">业务ID</param>
       public void DeleteFileByApplicationID(string companycode, string systemcode, string modelcode, string applicationid)
       {
           Client.DeleteFileByApplicationIDAsync(companycode, systemcode, modelcode, applicationid);           
       }
       /// <summary>
       /// 删除文件
       /// </summary>     
       /// <param name="applicationid">业务ID(一定要是唯一的)</param>
       public void DeleteFileByApplicationID(string applicationid)
       {
           Client.DeleteFileByOnlyApplicationIDAsync( applicationid);
       }
       /// <summary>
       /// 删除文件
       /// </summary>
       /// <param name="companycode">公司代号</param>
       /// <param name="systemcode">系统代号</param>
       /// <param name="modelcode">模块代号</param>
       public void DeleteFileByModelCode(string companycode, string systemcode, string modelcode)
       {
           Client.DeleteFileByModelCodeAsync(companycode, systemcode, modelcode); 
       }
       /// <summary>
       /// 删除文件
       /// </summary>
       /// <param name="companycode">公司代号</param>
       /// <param name="systemcode">系统代号</param>
       public void DeleteFileBySystemCode(string companycode, string systemcode)
       {
           Client.DeleteFileBySystemCodeAsync(companycode, systemcode); 
       }
       /// <summary>
       /// 删除文件
       /// </summary>
       /// <param name="companycode">公司代号</param>
       public void DeleteFileByCompanycode(string companycode)
       {
           Client.DeleteFileByCompanycodeAsync(companycode); 
       }
       #endregion
       #endregion
       //void Client_ListUpLoadFileCompleted(object sender, SMT.Saas.Tools.NewFileUploadWS.ListUpLoadFileCompletedEventArgs e)
       // {
       //     if (e.Result != null)
       //     {
       //         BindDataGrid(e.Result.ToList());
                
       //     }
       // }

        /// <summary>
        /// DataGrid数据绑定与分页
        /// </summary>
        /// <param name="prefixList"></param>
       private void BindDataGrid(ObservableCollection<SMT_FILELIST> fileList)
       {
          
           if (usc.NotAllowDownload)
           { 
               int n=fileList.Count;
               for (int i = 0; i < n; i++)
               {
                   fileList[i].FILEURL = "";
               }
           }
           PagedCollectionView pcv = new PagedCollectionView(fileList);
          
           //Pager.Source = pcv;
           //Pager.PageSize = usc.PageSize ==0? 10 : usc.PageSize;
           List.ItemsSource = pcv;
           if (usc.NotAllowDelete)
           {
               List.Columns[1].Visibility = Visibility.Collapsed;
           }
       }       
        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnUp_Click(object sender, RoutedEventArgs e)
        {        
             FileUploadWindow2 window = new FileUploadWindow2(usc);
            window.SystemCode = SystemCode;
            window.ModelCode = ModelCode;
            window.UserID = UserID;
            window.ApplicationID = ApplicationID;
            window.FileCompleted += new FileUploadWindow2.DelegateFileCompleted(window_FileCompleted);
            window.Closed += new EventHandler(window_Closed);
            window.Show();
        }
        //新窗口关闭时发生
        void window_Closed(object sender, EventArgs e)
        {
            FileCollection files = (sender as FileUploadWindow2)._files;
            var q2 = files.Where(f => f.State == Constants.FileStates.Cancel || f.State == Constants.FileStates.Error || f.State == Constants.FileStates.Removed || f.State == Constants.FileStates.Uploading);
            foreach (UserFile fu in q2)
            {//对于未上传完成的文件，删除服务器中的文件       
                fu.CancelUpload();
                //Client.DeleteFileAsync(fu.SmtFileListId, fu.FileUrl);
                if (MessageBox.Show(fu.FileName + "还没有上传完成,是否保留下一次续传?", "提示信息!", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    Client.DeleteFileAsync(fu.SmtFileListId, fu.FileUrl);
                }
            }
            if (IsLimitCompanyCode)
            {
                Client.GetFileListByApplicationIDAsync(usc.CompanyCode, usc.SystemCode, usc.ModelCode, usc.ApplicationID, usc.OnlyShowMySelf == true ? usc.CreateName : null);  //获取文件列表
            }
            else
            {
                Client.GetFileListByApplicationIDAsync(string.Empty, usc.SystemCode, usc.ModelCode, usc.ApplicationID, usc.OnlyShowMySelf == true ? usc.CreateName : null);  //获取文件列表
            }
        }

        void window_FileCompleted()
        {
            //Client.ListUpLoadFileAsync(ApplicationID);
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            string path = ((Button)sender).Tag.ToString();
            if (MessageBox.Show("你确定删除吗?", "删除文件：", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var item = this.itemSource.SingleOrDefault(c=>c.FILEURL==path);                
                itemSource.Remove(item);
                //Client.DeleteFileByUrlAsync(path);
                //formid 被重新赋值了
                Client.DeleteFileByApplicationIDAndFileNameAsync(item.APPLICATIONID,item.FORMID);
                //Client.GetFileListAsync();  //获取文件列表
            }
        }


        ////全选
        //private void checkAll_Click(object sender, RoutedEventArgs e)
        //{
        //    CheckBox cb = (CheckBox)sender;
        //    CheckAllRecord(cb);
        //}
        //public void CheckAllRecord(CheckBox cb)
        //{
        //    if (List.ItemsSource != null)
        //    {
        //        //CheckBox cb = (CheckBox)sender;
        //        if (cb.IsChecked == true)//全选
        //        {
        //            foreach (object ovj in List.ItemsSource)
        //            {
        //                CheckBox cb1 = List.Columns[5].GetCellContent(ovj).FindName("singnCheck") as CheckBox; //cb为
        //                cb1.IsChecked = true;
        //            }
        //        }
        //        else//取消
        //        {
        //            foreach (object obj in List.ItemsSource)
        //            {
        //                CheckBox cb2 = List.Columns[5].GetCellContent(obj).FindName("singnCheck") as CheckBox;

        //                cb2.IsChecked = false;
        //            }
        //        }
        //    }
        //}

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (AutoInitReadOnlyControl)
            {
                UserConfig uc = new SMT.FileUpLoad.Classes.UserConfig();
                uc.CompanyCode = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                uc.SystemCode = SystemCode;
                uc.ModelCode = ModelCode;
                uc.UserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                uc.ApplicationID = FormId;
                uc.NotShowThumbailChckBox = true;

                uc.NotShowUploadButton = true;
                uc.NotShowDeleteButton = true;
                uc.NotAllowDelete = true;
                uc.NotShowDeleteButton = true;

                uc.Multiselect = true;
                uc.Filter = "所有文件 (*.*)|*.*";
                //uc.Filter = "图片文件(*.jpg,*.gif,*.bmp)|*.jpg;*.gif;*.bmp";
                uc.MaxConcurrentUploads = 5;
                uc.MaxSize = "20.MB";
                uc.CreateName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                uc.PageSize = 20;
                Init(uc);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
