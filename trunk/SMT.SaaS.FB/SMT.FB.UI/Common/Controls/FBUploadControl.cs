using System;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;

namespace SMT.FB.UI.Common.Controls
{
    public class FBUploadControl : StackPanel, IControlAction
    {
        public DataPanelUploadInfo uploadInfo = null;
        public SMT.FileUpLoad.FileControl NewUploadControl { get; set; }
        public string FormID { get; set; } 
        public FBUploadControl(DataPanelUploadInfo uploadInfo)
        {
            this.uploadInfo = uploadInfo;
            InitControl();
        }
        
        private void InitControl()
        {
            //UploadControl = new CtrlFileUploadM();
            //UploadControl.SystemName = "FB";
            //UploadControl.ModelName = uploadInfo.ModelName;

            //UploadControl = new CtrlFileUploadM();
            NewUploadControl = new SMT.FileUpLoad.FileControl();
            //operationType == FormTypes.New;
            //表单ID
            //string id = "";
            NewUploadControl.MaxWidth = 600;
            //NewUploadControl.Width = 770;
            Binding binding = new Binding(uploadInfo.IDPropertyName);
            this.SetBinding(FBUploadControl.UploadIDProperty, binding);

            TextBlock tb = new TextBlock();
            tb.Text = uploadInfo.Name;
            tb.Width = 83;
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.VerticalAlignment = VerticalAlignment.Top;
            tb.Height = 22;
            this.Orientation = System.Windows.Controls.Orientation.Horizontal;
            this.Children.Add(tb);
            this.Children.Add(NewUploadControl);
        }
        OperationTypes OperationType;
        //public CtrlFileUploadM UploadControl { get; set; }


        public void FileLoadedCompleted()
        {
            //if (!UploadControl._files.HasAccessory)
            //{
            //    if (OperationType == OperationTypes.Audit || OperationType == OperationTypes.Browse)
            //    {
            //        // SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayGrid, 4);
            //        //this.Visibility = System.Windows.Visibility.Collapsed;
            //    }
            //}
        }

        public void GetUploadFileList()
        {
            try
            {
                //模块编码，一般为表名
                string ModelName = uploadInfo.ModelName;
                //动作
                //OperationType formTypes = ;\
                //FormTypes formTypes = new FormTypes();
                //string ide = UploadID;
                //switch (OperationType)
                //{
                //    case OperationTypes.Add:
                //        formTypes = FormTypes.New;
                //        break;
                //    case OperationTypes.Edit:
                //        formTypes = FormTypes.Edit;
                //        break;
                //    case OperationTypes.ReSubmit:
                //        formTypes = FormTypes.Resubmit;
                //        break;
                //    case OperationTypes.Browse:
                //        formTypes = FormTypes.Browse;
                //        break;

                //    case OperationTypes.Audit:
                //        formTypes = FormTypes.Audit;
                //        break;
                //}

                //if (!string.IsNullOrEmpty(UploadID))
                //{
                //    //UploadControl.Load_fileData(, this);//获取相应的上传信息
                //    InitFileLoad(ModelName, UploadID, formTypes, NewUploadControl, true);
                //}
                ////this.UploadControl.FormID = UploadID;
                //FormID = UploadID;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region UploadID
        private static void OnUploadIDChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FBUploadControl uc = d as FBUploadControl;
            uc.GetUploadFileList();
        }
        public static readonly DependencyProperty UploadIDProperty = DependencyProperty.RegisterAttached("UploadID", typeof(string), typeof(FBUploadControl), new PropertyMetadata(OnUploadIDChanged));

        public string UploadID
        {
            get
            {
                return Convert.ToString(this.GetValue(UploadIDProperty));
            }
            set
            {
                this.SetValue(UploadIDProperty, value);
            }
        }
        #endregion

        #region IControlAction

        public SaaS.FrameworkUI.Validator.ValidatorManager ValidatorManager
        {
            get;
            set;
        }

        public void InitControl(OperationTypes operationType)
        {
            this.OperationType = operationType;
            //if (operationType == OperationTypes.Audit || operationType == OperationTypes.Browse)
            //{
                //UploadControl.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
                //UploadControl.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
                //模块编码，一般为表名
                string ModelName = uploadInfo.ModelName;
                //动作
                //OperationType formTypes = ;\
                FormTypes formTypes = new FormTypes();
                string ide = UploadID;
                switch (OperationType)
                {
                    case OperationTypes.Add:
                        formTypes = FormTypes.New;
                        break;
                    case OperationTypes.Edit:
                        formTypes = FormTypes.Edit;
                        break;
                    case OperationTypes.ReSubmit:
                        formTypes = FormTypes.Resubmit;
                        break;
                    case OperationTypes.Browse:
                        formTypes = FormTypes.Browse;
                        break;

                    case OperationTypes.Audit:
                        formTypes = FormTypes.Audit;
                        break;
                }
                if (!string.IsNullOrEmpty(UploadID))
                {
                    //UploadControl.Load_fileData(, this);//获取相应的上传信息
                    InitFileLoad(ModelName, UploadID, formTypes, NewUploadControl, true);
                }
            //}
            else
            {
                //UploadControl.InitBtn(Visibility.Visible, Visibility.Collapsed);//隐藏删除、添加按钮
                //UploadControl.EntityEditor = this;//查看、审核时初始化用来解决异步显示控件的问题
            }
        }

        public Control FindControl(string name)
        {
            return null;
        }

        public void InitData(OrderEntity entity)
        {
            
        }

 
        #endregion


        public bool SaveData(Action<IControlAction> SavedCompletedAction)
        {
            //UploadControl.Save();
            return true;
        }



        #region  新上传控件调用
        // <summary>
        /// 新上传控件调用
        /// </summary>
        /// <param name="strModelCode">模块编码，一般为表名</param>
        /// <param name="strApplicationID">表单ID</param>
        /// <param name="action">动作</param>
        /// <param name="control">上传控件</param>
        public static void InitFileLoad(string strModelCode, string strApplicationID, FormTypes action, FileUpLoad.FileControl control,bool rel)
        {
            InitFileLoadSet(strModelCode, strApplicationID, action, control, rel);
        }

        /// <summary>
        /// 新上传控件调用
        /// </summary>
        /// <param name="strModelCode">模块编码，一般为表名</param>
        /// <param name="strApplicationID">表单ID</param>
        /// <param name="action">动作</param>
        /// <param name="control">上传控件</param>
        /// <param name="AllowDelete">是否允许删除</param>
        public static void InitFileLoadSet(string strModelCode, string strApplicationID, FormTypes action, FileUpLoad.FileControl control, bool AllowDelete)
        {
            SMT.FileUpLoad.Classes.UserConfig uc = new SMT.FileUpLoad.Classes.UserConfig();
            uc.CompanyCode = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            uc.SystemCode = "FB";
            uc.ModelCode = strModelCode;
            uc.UserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            uc.ApplicationID = strApplicationID;
            uc.NotShowThumbailChckBox = true;
            if (action == FormTypes.Browse || action == FormTypes.Audit)
            {
                uc.NotShowUploadButton = true;
                uc.NotShowDeleteButton = true;
                uc.NotAllowDelete = true;
            }
            if (!AllowDelete)
            {
                uc.NotShowDeleteButton = true;
            }
            uc.Multiselect = true;
            uc.Filter = "所有文件 (*.*)|*.*";
            //uc.Filter = "图片文件(*.jpg,*.gif,*.bmp)|*.jpg;*.gif;*.bmp";
            uc.MaxConcurrentUploads = 5;
            uc.MaxSize = "20.MB";
            uc.CreateName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            uc.PageSize = 20;
            control.Init(uc);


            if (action == FormTypes.Audit)
            {
                InitFileLoadOrt(FormTypes.Audit, control, strApplicationID, false);
            }
        }


        public static void InitFileLoadOrt(FormTypes action, FileUpLoad.FileControl control, string StrApplicationId, bool AllowDelete)
        {
            SMT.FileUpLoad.Classes.UserConfig uc = new SMT.FileUpLoad.Classes.UserConfig();
            uc.CompanyCode = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            uc.SystemCode = "FB";
            uc.IsLimitCompanyCode = false;
            uc.UserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            uc.ApplicationID = StrApplicationId;

            uc.NotShowThumbailChckBox = true;
            if (action == FormTypes.Browse || action == FormTypes.Audit)
            {
                uc.NotShowUploadButton = true;
                uc.NotShowDeleteButton = true;
                uc.NotAllowDelete = true;
            }
            if (!AllowDelete)
            {
                uc.NotShowDeleteButton = true;
            }
            uc.Multiselect = true;
            uc.Filter = "所有文件 (*.*)|*.*";
            //uc.Filter = "图片文件(*.jpg,*.gif,*.bmp)|*.jpg;*.gif;*.bmp";
            uc.MaxConcurrentUploads = 5;
            uc.MaxSize = "20.MB";
            //uc.CreateName = Common.CurrentLoginUserInfo.EmployeeName;
            uc.PageSize = 20;
            control.Init(uc);
        }
        #endregion
    }
}
