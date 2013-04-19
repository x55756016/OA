using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;

namespace SMT.SaaS.OA.UI.Form.EmployeeSurveys
{
    public partial class JoinSurveyingForm : BaseForm, IEntityEditor//, IAudit
    {
        string requireID = string.Empty;
        FormTypes actionTypes = 0;
        SmtOAPersonOfficeClient client = null;//WCF服务
        string requiremasterID = "";
        ObservableCollection<T_OA_REQUIRERESULT> listResult = new ObservableCollection<T_OA_REQUIRERESULT>();
        ObservableCollection<T_OA_REQUIRERESULT> deleteResult = new ObservableCollection<T_OA_REQUIRERESULT>();
      
        V_EmployeeSurveysModel model = new V_EmployeeSurveysModel();
        public FormTypes actionType;//动作标记
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private SMTLoading loadbar = new SMTLoading();
        private ObservableCollection<string> IDs;

        public JoinSurveyingForm(FormTypes actionTypes, string requireID)
        {
            InitializeComponent();
            this.requireID = requireID;
            this.actionTypes = actionTypes;
            InitEvent();
        }


        /// <summary>
        /// 初始化事件
        /// </summary>
        private void InitEvent()
        {
            client = new SmtOAPersonOfficeClient();
            PARENT.Children.Add(loadbar);

            this.textTitle.IsEnabled = false;
            this.textContent.IsEnabled = false;

            if (actionType == FormTypes.New) // 新增
            {
                this.SetToolBar();
            }

            loadbar.Start();
            client.GetDataByRequireIDAsync(requireID);
            client.GetDataByRequireIDCompleted += new EventHandler<GetDataByRequireIDCompletedEventArgs>(client_GetDataByRequireIDCompleted);
            client.GetAnswerCompleted += new EventHandler<GetAnswerCompletedEventArgs>(client_GetAnswerCompleted);
            client.EditRequireresultCompleted += new EventHandler<EditRequireresultCompletedEventArgs>(client_EditRequireresultCompleted);
        }

        void client_EditRequireresultCompleted(object sender, EditRequireresultCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "成功", " 保存成功", MessageIcon.Error);
                        actionType = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;

                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "失败", "保存失败", MessageIcon.Error);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "失败", "保存失败", MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "失败", "保存失败", MessageIcon.Error);
            }
            loadbar.Stop();
        }

       

        void client_GetAnswerCompleted(object sender, GetAnswerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.dgResult.ItemsSource = e.Result;

                foreach (var detail in e.Result)
                {
                    deleteResult.Add(detail.T_OA_REQUIRERESULT);
                }
            }
        }

        void client_GetDataByRequireIDCompleted(object sender, GetDataByRequireIDCompletedEventArgs e)
        {
            model = e.Result;
            ObservableCollection<T_OA_REQUIREDETAIL2> detail2 = model.T_OA_REQUIREDETAIL2;
            if (model.T_OA_REQUIREDETAIL2 != null)
            {
                this.dgQuestion.ItemsSource = detail2;
            }
            else
            {
                this.dgQuestion.ItemsSource = null;
            }

            if(model.T_OA_REQUIRE!=null)
            {
                requiremasterID = model.T_OA_REQUIREMASTER.REQUIREMASTERID;
                this.textTitle.Text = model.T_OA_REQUIRE.APPTITLE;
                this.textContent.Text = model.T_OA_REQUIRE.CONTENT;
            }
            loadbar.Stop();
        }

        private void dgQuestion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_OA_REQUIREDETAIL2 detail2 = this.dgQuestion.SelectedItem as T_OA_REQUIREDETAIL2;
            decimal subjectID = detail2.SUBJECTID;

            client.GetAnswerAsync(requiremasterID, subjectID, Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void RowFilterButton_Click(object sender, RoutedEventArgs e)
        {
            V_REQUIRERESULTMODE detail = (V_REQUIRERESULTMODE)((CheckBox)sender).Tag;
            CheckBox RowFilterButton = dgResult.Columns[0].GetCellContent(detail).FindName("RowFilterButton") as CheckBox;

            T_OA_REQUIRERESULT result = new T_OA_REQUIRERESULT();
            result.REQUIRERESULTID = Guid.NewGuid().ToString();
            result.T_OA_REQUIREMASTER = new T_OA_REQUIREMASTER();
            result.T_OA_REQUIREMASTER = model.T_OA_REQUIREMASTER;
            result.T_OA_REQUIRE = new T_OA_REQUIRE();
            result.T_OA_REQUIRE = model.T_OA_REQUIRE;

            result.SUBJECTID =detail.T_OA_REQUIREDETAIL.SUBJECTID;
            result.CODE = detail.T_OA_REQUIREDETAIL.CODE;
            result.RESULT = "1"; // 结果
            result.CONTENT = this.textContent.Text; // 补充意见

            result.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            result.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            result.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            result.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            result.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            result.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            result.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            result.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            result.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            result.CREATEDATE = DateTime.Now;
            result.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            result.UPDATEDATE = DateTime.Now;
            result.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
          
            if(RowFilterButton.IsChecked==true) // 选中 添加
            {
               listResult.Add(result);
              // deleteResult.Add(result);
             }
           else // 删除
            {
                listResult.Remove(result);
                IDs = new ObservableCollection<string>();
                IDs.Add(detail.T_OA_REQUIRERESULT.REQUIRERESULTID);
            }
        }

        public void Save()
        {
            loadbar.Stop();
            
            client.EditRequireresultAsync(listResult,IDs);
        }

        public bool Check()
        {
            return true;
        }

        #region IEntityEditor

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Save();
                    if (Check())
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.Close();
                    }
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

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            switch (this.actionType)
            {
                case FormTypes.New:
                    return "添加员工参入调查";
                case FormTypes.Edit:
                    return "修改员工参入调查";
                default:
                    return "查看员工参入调查";
            }
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }

        /// <summary>
        /// 控制窗口显示的按钮
        /// </summary>
        private void SetToolBar()
        {
            if (actionType == FormTypes.Browse)
            {
                return;
            }
            switch (actionType)
            {
                case FormTypes.New:
                    ToolbarItems = Utility.CreateFormSaveButton();
                    break;
                case FormTypes.Edit:
                    ToolbarItems = Utility.CreateFormEditButton();
                    break;
            }
            RefreshUI(RefreshedTypes.All);
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

        //#region IAudit接口

        //public string GetAuditState()
        //{
        //    string state = "-1";
        //    if (master != null)
        //        state = master.AUDITSTATE;
        //    return state;
        //}

        //public void OnSubmitCompleted(SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        //{
        //    string state = "";
        //    switch (args)
        //    {
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
        //            state = Utility.GetCheckState(Class.CheckStates.Approving);
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
        //            state = Utility.GetCheckState(Class.CheckStates.Approved);
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
        //            state = Utility.GetCheckState(Class.CheckStates.UnApproved);
        //            break;
        //    }
        //    if (this.Check(true))
        //    {
        //        master.AUDITSTATE = state;
        //        client.UpdateInStockAsync(master, addDetail, updateDetail, detailIDs);
        //        client.AuditInStockAsync(master, listDetail);
        //    }
        //}

        //public void SetFlowRecordEntity(SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        //{
        //    string strXmlObjectSource = string.Empty;
        //    strXmlObjectSource = Utility.ObjListToXml<T_EDM_INSTOCKMASTER>(master, "EDM");
        //    Utility.SetAuditEntity(entity, "T_EDM_INSTOCKMASTER", master.INTOSTORAGEMASTERID, strXmlObjectSource);
        //}

        //#endregion

    }
}

