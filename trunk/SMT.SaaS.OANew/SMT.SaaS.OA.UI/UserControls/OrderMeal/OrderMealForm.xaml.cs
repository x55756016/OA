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
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Windows.Browser;
//using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.OA.UI.CommForm;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class OrderMealForm : BaseForm,IClient, IEntityEditor
    {
        public OrderMealForm()
        {
            InitializeComponent();
        }

        #region 页面初始化
        SmtOAPersonOfficeClient OrderMealClient = new SmtOAPersonOfficeClient();
        private string StrCompanyId = "";
        private T_OA_ORDERMEAL tmpOrderMeal=new T_OA_ORDERMEAL();
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        public delegate void refreshGridView();
        Action orderact ;
        private string StrDepartmentId = "";
        public event refreshGridView ReloadDataEvent;
        private SMTLoading loadbar = new SMTLoading();
        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }
       

        

        public OrderMealForm(Action actionenum, T_OA_ORDERMEAL OrderObj)
        {
            
            InitializeComponent();
            PARENT.Children.Add(loadbar);
            orderact = actionenum;
            OrderMealClient.OrderMealInfoAddCompleted += new EventHandler<OrderMealInfoAddCompletedEventArgs>(OrderMealClient_OrderMealInfoAddCompleted);
            OrderMealClient.OrderMealInfoUpdateCompleted += new EventHandler<OrderMealInfoUpdateCompletedEventArgs>(OrderMealClient_OrderMealInfoUpdateCompleted);
            loadbar.Start();
            switch(actionenum)
            {
                case Action.Add:
                    this.tblTitle.Text = "添加订餐信息";
                    loadbar.Stop();
                    //GetDepartmentNameByDepartmentID(Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
                    break;
                case Action.Edit:
                    this.tblTitle.Text = "修改订餐信息";                    
                    tmpOrderMeal = OrderObj;
                    GetOrderMealInfoByOrderMealID(OrderObj);
                    break;
                case Action.Read:
                    break;


            }
            
            //GetDepartmentNameByDepartmentID(Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
        }

        #endregion        

        #region 修改构造函数
        

        void GetOrderMealInfoByOrderMealID(T_OA_ORDERMEAL OrderObj)
        {
            if (OrderObj != null)
            {                
                tmpOrderMeal = OrderObj;
                //this.GetDepartmentNameByDepartmentID(OrderObj.CREATEDEPARTMENTID);
                this.txtContent.Text = HttpUtility.HtmlDecode(OrderObj.CONTENT);
                this.txtTitle.Text = OrderObj.ORDERMEALTITLE;
                this.txtTel.Text = OrderObj.TEL;
                loadbar.Stop();
            }
        }

        #endregion

        #region 添加按钮事件
        

        private void SaveOrderMeal()
        {
            string StrTitle = "";
            string StrContent = "";
            string StrTel = "";
            StrTitle = this.txtTitle.Text.Trim().ToString();
            StrContent = this.txtContent.Text.Trim().ToString();
            StrTel = this.txtTel.Text.Trim().ToString();
            
            //if (string.IsNullOrEmpty(tmpOrderMeal.CREATEDEPARTMENTID))
            //{

            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTDEPART"));
            //    return;
            //}
            if (string.IsNullOrEmpty(StrTel))
            {

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("PLEASESELECT", "TELL"));
                return;
            }
            if (string.IsNullOrEmpty(StrTitle))
            {

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("PLEASESELECT", "ORDERMEALTITLE"));
                return;
            }
            if (string.IsNullOrEmpty(StrContent))
            {

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("PLEASESELECT", "ORDERMEALCONTENT"));
                return;
            }
            loadbar.Start();
            if (CheckOrderMeal())
            {
                if (orderact == Action.Add)
                {
                    tmpOrderMeal.ORDERMEALID = System.Guid.NewGuid().ToString();
                    tmpOrderMeal.ORDERMEALTITLE = StrTitle;
                    tmpOrderMeal.REMARK = "";
                    tmpOrderMeal.CONTENT = HttpUtility.HtmlDecode(StrContent);
                    tmpOrderMeal.CREATEDATE = System.DateTime.Now;
                    tmpOrderMeal.TEL = StrTel;

                    tmpOrderMeal.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    tmpOrderMeal.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    tmpOrderMeal.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    tmpOrderMeal.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    tmpOrderMeal.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    tmpOrderMeal.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    tmpOrderMeal.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    tmpOrderMeal.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    tmpOrderMeal.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    tmpOrderMeal.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

                    tmpOrderMeal.UPDATEUSERNAME = "";
                    tmpOrderMeal.ORDERMEALFLAG = "2";
                    tmpOrderMeal.UPDATEDATE = null;
                    tmpOrderMeal.UPDATEUSERID = "";


                    OrderMealClient.OrderMealInfoAddAsync(tmpOrderMeal);
                }
                if (orderact == Action.Edit)
                {
                    tmpOrderMeal.ORDERMEALTITLE = StrTitle;
                    tmpOrderMeal.CONTENT = HttpUtility.HtmlEncode(StrContent);

                    tmpOrderMeal.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    tmpOrderMeal.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    tmpOrderMeal.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    tmpOrderMeal.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    tmpOrderMeal.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    tmpOrderMeal.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    tmpOrderMeal.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    tmpOrderMeal.TEL = StrTel;
                    tmpOrderMeal.UPDATEUSERNAME = "";

                    tmpOrderMeal.UPDATEDATE = System.DateTime.Now;
                    tmpOrderMeal.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;


                    OrderMealClient.OrderMealInfoUpdateAsync(tmpOrderMeal);
                }
            }
            
        }

        void OrderMealClient_OrderMealInfoAddCompleted(object sender, OrderMealInfoAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    orderact = Action.Edit;
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "ORDERMEALINFO"));
                    RefreshUI(saveType);
                    
                }
                else
                {
                    MessageBox.Show(e.Result.ToString());
                    return;
                }
            }
            loadbar.Stop();
        }
        #endregion

        #region 修改按钮事件
        
        void OrderMealClient_OrderMealInfoUpdateCompleted(object sender, OrderMealInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "ORDERMEALINFO"));
                RefreshUI(saveType);
            }
            loadbar.Stop();
        }


        #endregion

        

        //#region 选择部门

        //private void PostsObject_FindClick(object sender, EventArgs e)
        //{
        //    OrganizationLookupForm lookup = new OrganizationLookupForm();
        //    lookup.SelectedObjType = OrgTreeItemTypes.Department;


        //    lookup.SelectedClick += (obj, ev) =>
        //    {
        //        PostsObject.DataContext = lookup.SelectedObj;
        //        if (lookup.SelectedObj is T_HR_DEPARTMENT)
        //        {
        //            T_HR_DEPARTMENT tmp = lookup.SelectedObj as T_HR_DEPARTMENT;
        //            //tmpOrderMeal.POSTID = tmp.T_HR_POSTDICTIONARY.POSTLEVEL;
        //            tmpOrderMeal.CREATEDEPARTMENTID = tmp.DEPARTMENTID; 
                    
        //            PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
        //        }

        //    };
        //    //lookup.SelectedClick += new EventHandler(lookup_SelectedClick);
        //    lookup.Show();
        //}


        //private void GetDepartmentNameByDepartmentID(string StrDepartmentID)
        //{
        //    OrganizationServiceClient Organ = new OrganizationServiceClient();
        //    Organ.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(Organ_GetDepartmentByIdCompleted);
        //    Organ.GetDepartmentByIdAsync(StrDepartmentID);
            
        //}
        //void Organ_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        //{
        //    if (!e.Cancelled)
        //    {
        //        if (e.Result != null)
        //        {
        //            T_HR_DEPARTMENT department = new T_HR_DEPARTMENT();
        //            department = e.Result;
        //            StrCompanyId = department.DEPARTMENTID;
        //            PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
        //            PostsObject.DataContext = department;
        //        }
        //    }
        //}

        //#endregion



        #region IEntityEditor
        public string GetTitle()
        {

            if (orderact == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "ORDERMEALINFO");
            }
            else if (orderact == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "ORDERMEALINFO");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "ORDERMEALINFO");
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
                    SaveOrderMeal();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    SaveOrderMeal();
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

            return items;
        }

        private void Close()
        {
            RefreshUI(saveType);
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

        private bool CheckOrderMeal()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 确定、取消
       

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            OrderMealClient.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
