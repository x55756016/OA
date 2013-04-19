using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
//using SMT.SaaS.OA.UI.BumfManagementWS;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.Class;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class DocTypeForm : BaseForm, IEntityEditor,IClient
    {
        
        #region 初始化变量
        private T_OA_SENDDOCTYPE tmpDocTypeT = new T_OA_SENDDOCTYPE();
        private T_OA_SENDDOCTYPE OldtmpDocTypeT = new T_OA_SENDDOCTYPE();
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭        
        //private BumfManagementServiceClient TypeClient = new BumfManagementServiceClient();
        private SmtOACommonOfficeClient TypeClient = new SmtOACommonOfficeClient();
        private Action action;
        //private SMTLoading loadbar = new SMTLoading(); 
        public delegate void refreshGridView();       
        
        public event refreshGridView ReloadDataEvent;

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }
        #endregion

        #region 构造函数

        public DocTypeForm(Action actionenum, T_OA_SENDDOCTYPE TypeObj)
        {
            InitializeComponent();
            //PARENT.Children.Add(loadbar);
            TypeClient.DocTypeAddCompleted += new EventHandler<DocTypeAddCompletedEventArgs>(TypeClient_DocTypeAddCompleted);
            TypeClient.DocTypeInfoUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(TypeClient_DocTypeInfoUpdateCompleted);
            action = actionenum;
            tmpDocTypeT = TypeObj;
            this.Loaded += new RoutedEventHandler(DocTypeForm_Loaded);
            
            
            
        }

        void DocTypeForm_Loaded(object sender, RoutedEventArgs e)
        {
            //RefreshUI(RefreshedTypes.ShowProgressBar);
            InitDocType(action, tmpDocTypeT);
        }

        private void InitDocType(Action actionenum, T_OA_SENDDOCTYPE TypeObj)
        {
            //loadbar.Start();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (actionenum == Action.Add)
            {

                RefreshUI(RefreshedTypes.HideProgressBar);
                //loadbar.Stop();
            }
            if (actionenum == Action.Edit)
            {

                this.txtDocType.IsEnabled = false;
                GetDocTypeDetailInfo(TypeObj);
            }
            if (actionenum == Action.Read)
            {

                this.txtDocType.IsEnabled = false;
                this.txtDemo.IsEnabled = false;
                this.RbtNo.IsEnabled = false;
                this.rbtYes.IsEnabled = false;
                GetDocTypeDetailInfo(TypeObj);
            }
        }

        
         private void GetDocTypeDetailInfo(T_OA_SENDDOCTYPE TypeObj)
         {
             this.txtDocType.Text = TypeObj.SENDDOCTYPE;
             this.txtDemo.Text = TypeObj.REMARK;
             switch (TypeObj.OPTFLAG)
             { 
                 case "0":
                     this.RbtNo.IsChecked = true;
                     this.rbtYes.IsChecked = false;
                     break;
                 case "1":
                     this.RbtNo.IsChecked = false;
                     this.rbtYes.IsChecked = true;
                     break;
             }
             tmpDocTypeT = TypeObj;
             OldtmpDocTypeT = TypeObj;
             //loadbar.Stop();
             RefreshUI(RefreshedTypes.HideProgressBar);
         }
        #endregion        

        #region 添加按钮

        private void SaveDocType()
        {
            try
            {
                
                string StrDocType = "";
                string StrFlag = "";
                string StrRemark = "";
                if (this.rbtYes.IsChecked == true)
                {
                    StrFlag = "1";
                }
                if (this.RbtNo.IsChecked == true)
                {
                    StrFlag = "0";
                }

                StrDocType = this.txtDocType.Text.Trim().ToString();
                StrRemark = this.txtDemo.Text.Trim().ToString();

                if (CheckDocType())
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    if (!string.IsNullOrEmpty(StrDocType))
                    {
                        if (action == Action.Add)
                        {
                            tmpDocTypeT.SENDDOCTYPEID = System.Guid.NewGuid().ToString();

                            tmpDocTypeT.OPTFLAG = StrFlag;
                            tmpDocTypeT.SENDDOCTYPE = StrDocType;
                            tmpDocTypeT.CREATEDATE = System.DateTime.Now;
                            tmpDocTypeT.REMARK = StrRemark;

                            tmpDocTypeT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            tmpDocTypeT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            tmpDocTypeT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                            tmpDocTypeT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            tmpDocTypeT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            tmpDocTypeT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            tmpDocTypeT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            tmpDocTypeT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            tmpDocTypeT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            tmpDocTypeT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

                            tmpDocTypeT.UPDATEUSERNAME = "";
                            tmpDocTypeT.UPDATEDATE = null;
                            tmpDocTypeT.UPDATEUSERID = "";

                            //loadbar.Start();
                            
                            TypeClient.DocTypeAddAsync(tmpDocTypeT);
                        }
                        else
                        {
                            tmpDocTypeT.OPTFLAG = StrFlag;
                            tmpDocTypeT.SENDDOCTYPE = StrDocType;


                            tmpDocTypeT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            tmpDocTypeT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            tmpDocTypeT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                            tmpDocTypeT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            tmpDocTypeT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            tmpDocTypeT.REMARK = StrRemark;
                            tmpDocTypeT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            tmpDocTypeT.UPDATEDATE = System.DateTime.Now;
                            tmpDocTypeT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                            
                            TypeClient.DocTypeInfoUpdateAsync(tmpDocTypeT);

                            //TypeClient.DocTypeAddAsync(tmpDocTypeT);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        void TypeClient_DocTypeAddCompleted(object sender, DocTypeAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {                    
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "DOCUMENTTYPE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "DOCUMENTTYPE"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    this.txtDocType.IsEnabled = false;
                                      
                    //RefreshUI(saveType);
                    if (GlobalFunction.IsSaveAndClose(saveType))
                    {
                        RefreshUI(saveType);
                    }
                    else
                    {
                        action = Action.Edit;                          
                        RefreshUI(RefreshedTypes.All);
                    }
                    
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString(),"DOCUMENTTYPE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString(), "DOCUMENTTYPE"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }


            }
        }

        #endregion

        #region 修改按钮        

        void TypeClient_DocTypeInfoUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {

                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "DOCUMENTTYPE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "DOCUMENTTYPE"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //loadbar.Stop();
                RefreshUI(saveType);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
        }
        #endregion

        #region 归档处理
        private void rbtYes_Click(object sender, RoutedEventArgs e)
        {
            this.rbtYes.IsChecked = true;
            this.RbtNo.IsChecked = false;
        }
        #endregion

        #region 不归档
        private void RbtNo_Click(object sender, RoutedEventArgs e)
        {
            this.RbtNo.IsChecked = true;
            this.rbtYes.IsChecked = false;

        }
        #endregion


        #region IEntityEditor
        private bool CheckDocType()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {

                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return false;
                }
            }
            return true;
        }
        public string GetTitle()
        {
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "OASENDDICTITLE");
            }
            else if (action == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "OASENDDICTITLE");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "OASENDDICTITLE");
            }
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    saveType = RefreshedTypes.LeftMenu;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    SaveAndClose();
                    break;

            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (action != Action.Read)
            {

                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1",
                    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                };
                items.Add(item);
                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("SAVE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"

                };
                items.Add(item);
            }

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region 确定、取消
        private void Save()
        {
            saveType = RefreshedTypes.LeftMenu;
            SaveDocType();          
        }

        private void SaveAndClose()
        {
            saveType = RefreshedTypes.CloseAndReloadData;
            SaveDocType();
            
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            TypeClient.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            //throw new NotImplementedException();
            return true;
        }

        public void SetOldEntity(object entity)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
