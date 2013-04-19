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
using SMT.Saas.Tools.PersonnelWS;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class CheckGrade : BaseForm, IEntityEditor,IClient
    {
        public FormTypes FormType { get; set; }
        private PersonnelServiceClient client = new PersonnelServiceClient();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private T_HR_ASSESSMENTFORMMASTER formMaster;
        private string CheckType { get; set; }

        private string ObjectID { get; set; }
        private string EmployeeID { set; get; }
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public T_HR_ASSESSMENTFORMMASTER FormMaster
        {
            get { return formMaster; }
            set
            {
                formMaster = value;
                this.DataContext = value;
            }
        }
        private ObservableCollection<T_HR_ASSESSMENTFORMDETAIL> DetailList = new ObservableCollection<T_HR_ASSESSMENTFORMDETAIL>();
        private ObservableCollection<V_PROJECTPOINT> ProjectSet = new ObservableCollection<V_PROJECTPOINT>();
        /// <summary>
        /// 通过转正或异动调用的构造函数
        /// </summary>
        /// <param name="objectID">对象ID（转正ID或异动ID）</param>
        /// <param name="employeeID">员工ID</param>
        /// <param name="checkType">考核类型（0表示转正审核，1表示异动晋升）</param>
        public CheckGrade(string objectID, string employeeID, string checkType)
        {
            InitializeComponent();
            EmployeeID = employeeID;
            CheckType = checkType;
            ObjectID = objectID;
            client.GetAssessMentFormMasterByObjectIDCompleted += new EventHandler<GetAssessMentFormMasterByObjectIDCompletedEventArgs>(client_GetAssessMentFormMasterByObjectIDCompleted);
            client.GetAssessMentFormMasterByObjectIDAsync(objectID);
        }

        void client_GetAssessMentFormMasterByObjectIDCompleted(object sender, GetAssessMentFormMasterByObjectIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                if (e.Result == null)
                {
                    FormType = FormTypes.New;
                    InitPara("", "0");
                }
                else
                {
                    FormMaster = e.Result;
                    FormType = FormTypes.Edit;
                    InitPara(FormMaster.ASSESSMENTFORMMASTERID, FormMaster.EMPLOYEELEVEL);
                }
                client.GetEmployeeDetailByIDAsync(EmployeeID);
            }
        }

        void client_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                if (e.Result != null)
                {
                    V_EMPLOYEEPOST tempEnt = e.Result;
                    FormMaster.EMPLOYEEID = tempEnt.T_HR_EMPLOYEE.EMPLOYEEID;
                    FormMaster.EMPLOYEECODE = tempEnt.T_HR_EMPLOYEE.EMPLOYEECODE;
                    FormMaster.EMPLOYEENAME = tempEnt.T_HR_EMPLOYEE.EMPLOYEECNAME;
                    txtDepartment.Text = tempEnt.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    txtPost.Text = tempEnt.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                }
            }
        }

        public CheckGrade(FormTypes type, string strID, string strType)
        {
            InitializeComponent();
            FormType = type;
            InitPara(strID, strType);

        }

        private void InitPara(string strID, string strType)
        {
            client.GetCheckProjectSetByTypeCompleted += new EventHandler<GetCheckProjectSetByTypeCompletedEventArgs>(client_GetCheckProjectSetByTypeCompleted);
            client.AssessmentFormMasterAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AssessmentFormMasterAddCompleted);
            client.AssessmentFormMasterUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AssessmentFormMasterUpdateCompleted);
            client.GetAssessmentFormDetailByMasterIDCompleted += new EventHandler<GetAssessmentFormDetailByMasterIDCompletedEventArgs>(client_GetAssessmentFormDetailByMasterIDCompleted);
            client.GetAssessMentFormMasterByIDCompleted += new EventHandler<GetAssessMentFormMasterByIDCompletedEventArgs>(client_GetAssessMentFormMasterByIDCompleted);
            client.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(client_GetEmployeeDetailByIDCompleted);
            client.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(client_GetEmployeeByIDCompleted);

            if (FormType == FormTypes.New)
            {
                FormMaster = new T_HR_ASSESSMENTFORMMASTER();
                FormMaster.ASSESSMENTFORMMASTERID = Guid.NewGuid().ToString();
                FormMaster.CHECKREASON = CheckType;
                FormMaster.CHECKTYPE = CheckType;
                FormMaster.EMPLOYEELEVEL = strType;
                //赋对象值
                if (CheckType == "0")
                {
                    T_HR_EMPLOYEECHECK ent = new T_HR_EMPLOYEECHECK();
                    ent.BEREGULARID = ObjectID;
                    FormMaster.T_HR_EMPLOYEECHECK = ent;
                }
                else
                {
                    T_HR_EMPLOYEEPOSTCHANGE ent = new T_HR_EMPLOYEEPOSTCHANGE();
                    ent.POSTCHANGEID = ObjectID;
                    FormMaster.T_HR_EMPLOYEEPOSTCHANGE = ent;
                }
                //client.GetCheckProjectSetByTypeAsync(strType);
                SetToolBar();
            }
            else if (FormType == FormTypes.Browse)
            {
                client.GetCheckProjectSetByTypeAsync(strType);
            }
            else
            {
                //client.GetCheckProjectSetByTypeAsync(strType);
                client.GetAssessmentFormDetailByMasterIDAsync(strID);
                client.GetAssessMentFormMasterByIDAsync(strID);
            }
        }

        void client_GetAssessMentFormMasterByIDCompleted(object sender, GetAssessMentFormMasterByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                FormMaster = e.Result;
                client.GetEmployeeByIDAsync(FormMaster.CHECKPERSON);
                SetToolBar();
            }
        }

        void client_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                lkEmployeeName.DataContext = e.Result;
            }
        }

        void client_GetAssessmentFormDetailByMasterIDCompleted(object sender, GetAssessmentFormDetailByMasterIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                DetailList = e.Result;
            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_ASSESSMENTFORMMASTER", FormMaster.OWNERID,
                    FormMaster.OWNERPOSTID, FormMaster.OWNERDEPARTMENTID, FormMaster.OWNERCOMPANYID);

            if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_ASSESSMENTFORMMASTER", FormMaster.OWNERID,
                    FormMaster.OWNERPOSTID, FormMaster.OWNERDEPARTMENTID, FormMaster.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }

        void client_GetCheckProjectSetByTypeCompleted(object sender, GetCheckProjectSetByTypeCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                ProjectSet = e.Result;
                lbxCheckProject.DataContext = ProjectSet;
            }
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("CHECKPROJECTSET");
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
                    Save();
                    break;
                case "1":
                    closeFormFlag = true;
                    Save();
                    // Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
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
        #region IClient
        public void ClosedWCFClient()
        {
            //  throw new NotImplementedException();
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
        private bool Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            SetEntityValue();
            if (FormType == FormTypes.New)
            {
                //所属
                FormMaster.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                FormMaster.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                FormMaster.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                FormMaster.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                FormMaster.CREATEDATE = DateTime.Now;
                FormMaster.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.AssessmentFormMasterAddAsync(FormMaster, DetailList);
            }
            else
            {
                FormMaster.UPDATEDATE = DateTime.Now;
                FormMaster.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.AssessmentFormMasterUpdateAsync(FormMaster, DetailList);
            }
            return true;
        }

        private void SetEntityValue()
        {
            foreach (var ent in ProjectSet)
            {
                foreach (var tempEnt in ent.PointList)
                {
                    if (FormType == FormTypes.New)
                    {
                        T_HR_ASSESSMENTFORMDETAIL temp = new T_HR_ASSESSMENTFORMDETAIL();
                        temp.ASSESSMENTFORMDETAILID = Guid.NewGuid().ToString();
                        T_HR_CHECKPOINTSET ents = new T_HR_CHECKPOINTSET();
                        ents.CHECKPOINTSETID = tempEnt.CheckPointSetID;
                        temp.T_HR_CHECKPOINTSET = ents;
                        temp.T_HR_ASSESSMENTFORMMASTER = FormMaster;
                        temp.CREATEDATE = DateTime.Now;
                        temp.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        temp.SECONDSCORE = tempEnt.SecondScore;
                        temp.FIRSTSCORE = tempEnt.FirstScore;
                        DetailList.Add(temp);
                    }
                    else
                    {
                        foreach (var temp in DetailList)
                        {
                            if (tempEnt.CheckPointSetID == temp.T_HR_CHECKPOINTSET.CHECKPOINTSETID)
                            {
                                temp.SECONDSCORE = tempEnt.SecondScore;
                                temp.FIRSTSCORE = tempEnt.FirstScore;
                                temp.UPDATEDATE = DateTime.Now;
                                temp.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                            }
                        }
                    }
                }
            }
        }

        void client_AssessmentFormMasterAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_AssessmentFormMasterUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        private void _ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TotalBinder();
        }

        private void lbxCheckProject_LostFocus(object sender, RoutedEventArgs e)
        {
            decimal delFirst = 0, delSecond = 0;
            //获取所有评分记录
            foreach (var ent in ProjectSet)
            {
                foreach (var tempEnt in ent.PointList)
                {
                    if (tempEnt.FirstScore != 0)
                    {
                        delFirst += tempEnt.FirstScore;
                    }
                    if (tempEnt.SecondScore != 0)
                    {
                        delSecond += tempEnt.SecondScore;
                    }
                }
            }
            txtFirst.Text = delFirst.ToString();
            txtSecond.Text = delSecond.ToString();
            //txtTotal.Text = (delFirst + delSecond + (decimal)nudAwards.Value - (decimal)nudPunishment.Value).ToString();
        }

        private void txtFirst_TextChanged(object sender, TextChangedEventArgs e)
        {
            TotalBinder();
        }

        private void TotalBinder()
        {
            double delFirst = 0, delSecond = 0;
            if (txtFirst.Text != "")
            {
                delFirst = double.Parse(txtFirst.Text);
            }
            if (txtSecond.Text != "")
            {
                delSecond = double.Parse(txtSecond.Text);
            }
            txtTotal.Text = (delFirst + delSecond + nudAwards.Value - nudPunishment.Value).ToString();
        }

        private void LookUp_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            cols.Add("EMPLOYEEENAME", "T_HR_EMPLOYEE.EMPLOYEEENAME");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Employee,
                typeof(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST; ;

                if (ent != null)
                {
                    lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
                    FormMaster.CHECKPERSON = ent.T_HR_EMPLOYEE.EMPLOYEEID;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtFirstCom.Text != "")
            {
                txtFirstDate.Text = DateTime.Now.Date.ToString("yyyy-MM-dd");
            }
            else
            {
                txtFirstDate.Text = "";
            }
            if (txtSecondCom.Text != "")
            {
                txtSecondDate.Text = DateTime.Now.Date.ToString("yyyy-MM-dd");
            }
            else
            {
                txtSecondDate.Text = "";
            }
            if (txtHrCom.Text != "")
            {
                txtHrCommentDate.Text = DateTime.Now.Date.ToString("yyyy-MM-dd");
            }
            else
            {
                txtHrCommentDate.Text = "";
            }
        }

        private void DictionaryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = cbxLevel.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                client.GetCheckProjectSetByTypeAsync(dict.DICTIONARYVALUE.ToString());
            }
        }
    }
}
