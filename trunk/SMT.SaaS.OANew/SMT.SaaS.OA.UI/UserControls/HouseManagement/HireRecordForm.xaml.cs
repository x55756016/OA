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
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class HireRecordForm : BaseForm,IClient,IEntityEditor
    {
        
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        
        private string checkState = "";
        private Action action;
        private SmtOACommonAdminClient client;
        private string hireAppID = "";
        private MemberHireApp addFrm;
        private T_OA_HOUSEINFO houseInfo;
        //private V_HouseHireList houseList;
        private V_HireRecord hireRecordList;
        private T_OA_HIRERECORD hireRecord;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private string StrRentType = "0";//合租类型  0 整租  1 合租
        private string StrSettlementType = "0"; //结算方式 0 工资 1 现金

        private T_OA_HIREAPP app = new T_OA_HIREAPP();
        
        public V_HireRecord HireRecordObj
        {
            get { return hireRecordList; }
            set
            {
                hireRecordList = value;
                this.DataContext = value;
            }
        }


        #region 初始化
        /// <summary>
        /// 2010-5-20 by liujx
        /// </summary>
        /// <param name="action">动作 增、删、改、查、审核</param>
        /// <param name="hireAppID">申请ID</param>
        /// <param name="checkState">状态</param>
        /// <param name="FromFlag">来源标记</param>
        public HireRecordForm(Action action, V_HireRecord VRecord)
        {
            InitializeComponent();
            
            houseInfo = new T_OA_HOUSEINFO();
            hireRecordList = new V_HireRecord();
            this.action = action;
            hireRecordList = VRecord;
            
            hireRecord = new T_OA_HIRERECORD();
            hireRecord = VRecord.HouseRecordObj;
            InitEvent();
            SetToolBar();
            if (action == Action.Return)
            {
                SetReturnBar();
            }
            
            GetHireRecord(VRecord);
        }

        private void GetHireRecord(V_HireRecord Vrecord)
        {
            this.txelectricitypay.Text = Vrecord.HouseRecordObj.ELECTRICITY.ToString();
            this.txtelectricitynum.Text = Vrecord.HouseRecordObj.ELECTRICITYNUM.ToString();
            this.txtFloor.Text = Vrecord.houseInfoObj.FLOOR.ToString();
            this.txtHouseName.Text = Vrecord.houseInfoObj.HOUSENAME;
            this.txtManageCost.Text = Vrecord.HouseRecordObj.MANAGECOST.ToString();
            this.txtUptown.Text = Vrecord.houseInfoObj.UPTOWN;
            this.txtwater.Text = Vrecord.HouseRecordObj.WATER.ToString();
            this.txtwaternum.Text = Vrecord.HouseRecordObj.WATERNUM.ToString();
            this.txtother.Text = Vrecord.HouseRecordObj.OTHERCOST.ToString();
            this.SharedRentCost.Text = Vrecord.HouseRecordObj.RENTCOST.ToString();
            this.sDate.SelectedDate = Vrecord.houseAppObj.STARTDATE;
            this.eDate.SelectedDate = Vrecord.houseAppObj.ENDDATE;
            this.payDate.SelectedDate = Vrecord.HouseRecordObj.SETTLEMENTDATE;
            this.txtNum.Text = Vrecord.houseInfoObj.ROOMCODE;
            if (Vrecord.HouseRecordObj.SETTLEMENTTYPE == "0")
            {
                this.txtpaytype.Text = Utility.GetResourceStr("WAGE");
            }
            else
            {
                this.txtpaytype.Text = Utility.GetResourceStr("CASH");
            }
            if (Vrecord.houseAppObj.RENTTYPE == "0")
            {
                this.txtRentType.Text = Utility.GetResourceStr("SHAREDHIRE");
            }
            else
            {
                this.txtRentType.Text = Utility.GetResourceStr("WHOLEHIRE");
            }
            

        }

        private void InitEvent()
        {
            client = new SmtOACommonAdminClient();
            
            client.UpdateHireRecordCompleted += new EventHandler<UpdateHireRecordCompletedEventArgs>(client_UpdateHireRecordCompleted);
            //client.AddHireRecordCompleted += new EventHandler<AddHireRecordCompletedEventArgs>(client_AddHireRecordCompleted);
            //client.GetHireRecordByIDCompleted += new EventHandler<GetHireRecordByIDCompletedEventArgs>(client_GetHireRecordByIDCompleted);
        }

        
        void client_UpdateHireRecordCompleted(object sender, UpdateHireRecordCompletedEventArgs e)
        {
            
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        //HtmlPage.Window.Alert("房源记录修改成功！");
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "HOUSEINFO"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(RefreshedTypes.CloseAndReloadData);
                        }
                        else
                        {
                            RefreshUI(refreshType);
                        }
                    }
                }
                else
                {
                    
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_AddHireRecordCompleted(object sender, AddHireRecordCompletedEventArgs e)
        {
            //入住成功
        }

         

        #endregion

        

        #region 添加入住记录
        private void AddHireRecord()
        {
            
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("COMPANY");
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "HOUSEHIRERECORD");
            }
            else
            {
                return Utility.GetResourceStr("EDITTITLE", "HOUSEHIRERECORD");
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
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
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
            //item = new NavigateItem
            //{
            //    Title = "员工资料",
            //    Tooltip = "员工详细",
            //    Url = "/Personnel/Employee"
            //};
            //items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }

        private List<ToolbarItem> CreateFormNewButton()
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

        private List<ToolbarItem> CreateFormReturnButton()
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

        #region 自定义函数

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void SetReadOnly()
        {
            this.sDate.IsEnabled = false;
            this.eDate.IsEnabled = false;
            this.bDate.IsEnabled = false;
        }

        private void SetToolBar()
        {          
            ToolbarItems = CreateFormNewButton();
            if (checkState == ((int)CheckStates.UnSubmit).ToString() )
            {
                //scvAudit.Visibility = Visibility.Collapsed;
            }
        }
        private void SetReturnBar()
        {
            ToolbarItems = CreateFormReturnButton();
            this.sDate.IsEnabled = false;
            this.eDate.IsEnabled = false;
            this.bDate.IsEnabled = true; 
        }
       
        private void Save()
        {
            try
            {
                if(Check())
                {
                    hireRecord.RENTCOST = System.Convert.ToInt32(this.SharedRentCost.Text.ToString());
                    hireRecord.MANAGECOST = System.Convert.ToInt32(this.txtManageCost.Text.ToString());
                    hireRecord.WATER = System.Convert.ToInt32(this.txtwater.Text.ToString());
                    hireRecord.WATERNUM = System.Convert.ToInt32(this.txtwaternum.Text.ToString());
                    hireRecord.ELECTRICITY = System.Convert.ToInt32(this.txelectricitypay.Text.ToString());
                    hireRecord.ELECTRICITYNUM = System.Convert.ToInt32(this.txtelectricitynum.Text.ToString());
                    hireRecord.OTHERCOST = System.Convert.ToInt32(this.txtother.Text.ToString());
                    hireRecord.UPDATEDATE = System.DateTime.Now;
                    hireRecord.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    hireRecord.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    RefreshUI(RefreshedTypes.ProgressBar);
                    client.UpdateHireRecordAsync(hireRecord);
                }
                
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message);
            }
        }

        private void Close()
        {
            RefreshUI(refreshType);
        }

        private bool Check()
        {
            
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    //HtmlPage.Window.Alert(h.ErrorMessage);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            if (Convert.ToDateTime(sDate.SelectedDate) > Convert.ToDateTime(eDate.SelectedDate))
            {                
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEERROR", "HIREDATE,EXPECTEDBACKDATE"));
                return false;
            }
            if (action == Action.Return)
            {
                try
                {
                    Convert.ToDateTime(bDate.SelectedDate);
                }
                catch (Exception ex)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "HIREDATE,EXPECTEDBACKDATE"));
                    return false;
                }
                if (Convert.ToDateTime(sDate.SelectedDate) > Convert.ToDateTime(bDate.SelectedDate))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEERROR", "HIREDATE,BACKDATE"));
                    return false;
                }
            }
            return true;
        }
        private void Cancel()
        {
            RefreshUI(refreshType);
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            RefreshUI(refreshType);
        }
        

        #endregion





        #region IForm 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
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
