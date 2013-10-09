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
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class OrderMealDetailInfo : BaseForm,IClient, IEntityEditor
    {
        //public OrderMealDetailInfo()
        //{
        //    InitializeComponent();
        //}

        SmtOAPersonOfficeClient OrderMealClient = new SmtOAPersonOfficeClient();
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭  
        private Action action;
        public OrderMealDetailInfo(T_OA_ORDERMEAL OrderObj)
        {
            InitializeComponent();
            GetOrderMealDetailInfoByOrderID(OrderObj);
            
            GetDepartmentNameByDepartmentID(OrderObj.CREATEDEPARTMENTID);
        }


        private void GetOrderMealDetailInfoByOrderID(T_OA_ORDERMEAL OrderObj)
        {
            if (OrderObj != null)
            {
                this.tblOrderMealTitle.Text = OrderObj.ORDERMEALTITLE;
                //this.tblDepartment.Text = OrderObj.DEPARTNAME;
                this.tblContent.Text = HttpUtility.HtmlEncode(OrderObj.CONTENT);
                this.tblRemark.Text = OrderObj.REMARK;
                string StrState = "";
                switch (OrderObj.ORDERMEALFLAG)
                {
                    case "0":
                        StrState = "已取消";
                        break;
                    case "1":
                        StrState = "已完成";
                        break;
                    case "2":
                        StrState = "待订";
                        break;

                }
                this.tblState.Text = StrState;
                this.tblAddDate.Text = OrderObj.CREATEDATE.ToShortDateString() + " " + OrderObj.CREATEDATE.ToShortTimeString();
                if (OrderObj.UPDATEDATE != null || OrderObj.UPDATEDATE.ToString() != "")
                {
                    this.tblUpdateDate.Text = OrderObj.UPDATEDATE.ToString();

                }
                this.tblAdduser.Text = OrderObj.CREATEUSERNAME;
                this.tblUpdater.Text = OrderObj.UPDATEUSERNAME;
                this.tblWindowTitle.Text = "查看" + OrderObj.ORDERMEALTITLE + "信息";
                

            }
        }

       
        

        private void GetDepartmentNameByDepartmentID(string StrDepartmentID)
        {
            OrganizationServiceClient Organ = new OrganizationServiceClient();
            Organ.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(Organ_GetDepartmentByIdCompleted);
            Organ.GetDepartmentByIdAsync(StrDepartmentID);

        }
        void Organ_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    T_HR_DEPARTMENT department = new T_HR_DEPARTMENT();
                    department = e.Result;
                    this.tblDepartment.Text = department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    //StrCompanyId = department.DEPARTMENTID;
                    //PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    //PostsObject.DataContext = department;
                }
            }
        }

        #region IEntityEditor
        
        public string GetTitle()
        {
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "ORDERMEALINFO");
            }
            else if (action == Action.Edit)
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
           SaveAndClose();
            
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
                Key = "0",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            items.Add(item);
            
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
        

        private void SaveAndClose()
        {
            saveType = RefreshedTypes.CloseAndReloadData;

            RefreshUI(saveType);
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
