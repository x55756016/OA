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
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SatisfactionsChildWindow :BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量定义
        private SmtOAPersonOfficeClient client = null;
        private FormTypes actionType;
        private string primaryKey=string.Empty;
        private T_OA_SATISFACTIONMASTER masterEntity;
        private ObservableCollection<T_OA_SATISFACTIONDETAIL> detailList;
        #endregion

        #region
        public SatisfactionsChildWindow(FormTypes actionType, string primaryKey)
        {
            InitializeComponent();
            this.actionType = actionType;
            this.primaryKey = primaryKey;
            detailList = new ObservableCollection<T_OA_SATISFACTIONDETAIL>();
        }
        #endregion

        #region 事件注册
        private void EventRegister()
        {
            client = new SmtOAPersonOfficeClient();
            this.Loaded += new RoutedEventHandler(SatisfactionsChildWindow_Loaded);
            client.AddSatisfactionsMasterCompleted += new EventHandler<AddSatisfactionsMasterCompletedEventArgs>(client_AddSatisfactionsMasterCompleted);
            
            client.GetStaffSurveyInfosCompleted += new EventHandler<GetStaffSurveyInfosCompletedEventArgs>(client_GetStaffSurveyInfosCompleted);
        }

       

        
        #endregion

        #region 事件完成程序
        void SatisfactionsChildWindow_Loaded(object sender, RoutedEventArgs e)//页面加载
        {
            throw new NotImplementedException();
        }
        void client_AddSatisfactionsMasterCompleted(object sender, AddSatisfactionsMasterCompletedEventArgs e)//WCF新增
        {
            throw new NotImplementedException();
        }
        void client_GetStaffSurveyInfosCompleted(object sender, GetStaffSurveyInfosCompletedEventArgs e)//查询
        {
            throw new NotImplementedException();
        }
        #endregion

        #region XAML事件完成程序
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)//点击回车新加载一行
        {
            T_OA_SATISFACTIONDETAIL temp = (T_OA_SATISFACTIONDETAIL)e.Row.DataContext;
            ImageButton MyButton_Delbaodao = dg.Columns[3].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            MyButton_Delbaodao.Tag = temp;
        }
        private void txtSub_KeyDown(object sender, KeyEventArgs e)//按下回车后响应
        {

        }
        private void Delete_Click(object sender, RoutedEventArgs e)//行删除按钮
        {

        }
        #endregion

        #region 其他函数
        private void SetCommonField()
        {
            detailList[0].CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            detailList[0].CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            detailList[0].CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            detailList[0].CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            detailList[0].CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            detailList[0].OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            detailList[0].OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            detailList[0].OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            detailList[0].OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            detailList[0].OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            detailList[0].CREATEDATE = DateTime.Now;
            detailList[0].SATISFACTIONDETAILID = Guid.NewGuid().ToString();
        }
        private bool Check()//UI数据验证
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var info in validators)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(info.ErrorMessage),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    return false;
                }
               
            }
            return true;
        }

        private void SaveEntity()
        {
            //if (Check())
            //{
            //    if (actionType == FormTypes.New)
            //    {
            //        foreach(var x in 
                   

               
            //}
        }

       
        #endregion

        #region 接口实现

        #region IClient资源回收
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

        #region IEntityEditor窗体控制
        public string GetTitle()
        {
            throw new NotImplementedException();
        }

        public string GetStatus()
        {
            throw new NotImplementedException();
        }

        public void DoAction(string actionType)
        {
            throw new NotImplementedException();
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            throw new NotImplementedException();
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            throw new NotImplementedException();
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        #region IAudit审核控制
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            throw new NotImplementedException();
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            throw new NotImplementedException();
        }

        public string GetAuditState()
        {
            throw new NotImplementedException();
        }
        #endregion

       

        #endregion

      
       





        
    }
}
