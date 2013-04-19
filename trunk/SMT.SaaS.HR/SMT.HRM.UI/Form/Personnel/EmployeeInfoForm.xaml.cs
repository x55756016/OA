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

using SMT.Saas.Tools.PersonnelWS;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeInfoForm : BaseForm, IClient
    {
        public T_HR_EMPLOYEE Employee { get; set; }

        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient client = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient perclient = new Saas.Tools.PermissionWS.PermissionServiceClient();
        private string strName = "", sIDNumber = "";
        public bool IsEntryBefore { get; set; } //是否是离职再入职
        private string btnActionType = string.Empty; // 保存还是下一步
        public T_HR_RESUME resume;//简历
        //public List<T_HR_EDUCATEHISTORY> edulist;
        //public List<T_HR_EXPERIENCE> explist;
        public EmployeeInfoForm()
        {
        }
        public EmployeeInfoForm(string sNumberID, string sName)
        {
            InitializeComponent();
            InitPara(sNumberID);
            strName = sName;
            sIDNumber = sNumberID;
        }
        public EmployeeInfoForm(string employeeID)
        {
            InitializeComponent();
            client.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(client_GetEmployeeByIDCompleted);
            client.IsExistFingerPrintIDCompleted += new EventHandler<IsExistFingerPrintIDCompletedEventArgs>(client_IsExistFingerPrintIDCompleted);
            client.EmployeeAddOrUpdateCompleted += new EventHandler<EmployeeAddOrUpdateCompletedEventArgs>(client_EmployeeAddOrUpdateCompleted);
            perclient.GetUserNameIsExistNameAddOneCompleted += new EventHandler<Saas.Tools.PermissionWS.GetUserNameIsExistNameAddOneCompletedEventArgs>(perclient_GetUserNameIsExistNameAddOneCompleted);
            client.GetResumeByNumberCompleted += new EventHandler<GetResumeByNumberCompletedEventArgs>(client_GetResumeByNumberCompleted);
            client.GetEmployeeByIDAsync(employeeID);
        }

        /// <summary>
        /// 根据身份证获取简历
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetResumeByNumberCompleted(object sender, GetResumeByNumberCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                resume = e.Result;
                //LoadExp();
                //LoadEdu();
                experience.LoadData(FormTypes.Edit, resume.RESUMEID, resume);
                educateHistory.LoadData(FormTypes.Edit, resume.RESUMEID, resume);
            }
        }

        /// <summary>
        /// 新增或修改员工档案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeAddOrUpdateCompleted(object sender, EmployeeAddOrUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                             Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (string.IsNullOrEmpty(e.strMsg))
                {
                    saveResume();
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                              Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }

            }
        }
        public void saveResume()
        {
            //          ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
            //Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            string strMsg = string.Empty;

            if (resume == null)
            {
                resume = new T_HR_RESUME();
                resume.NAME = Employee.EMPLOYEECNAME;
                resume.RESUMEID = Guid.NewGuid().ToString();
                resume.IDCARDNUMBER = Employee.IDNUMBER;
                foreach (var ent in experience.ExperienceList)
                {
                    ent.T_HR_RESUME = new T_HR_RESUME();
                    ent.T_HR_RESUME.RESUMEID = resume.RESUMEID;
                }
                foreach (var tmp in educateHistory.EducateHistory)
                {
                    tmp.T_HR_RESUME = new T_HR_RESUME();
                    tmp.T_HR_RESUME.RESUMEID = resume.RESUMEID;
                }
                client.ResumeAddCompleted += (o, m) =>
                {
                    if (m.Error == null)
                    {
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        }
                        else
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(strMsg),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        }
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                };
                client.ResumeAddAsync(resume, experience.ExperienceList, educateHistory.EducateHistory, strMsg);
            }
            else
            {
                client.ResumeUpdateCompleted += (o, m) =>
                {
                    if (m.Error == null)
                    {
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        }
                        else
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(strMsg),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        }
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                };
                foreach (var ent in experience.ExperienceList)
                {
                    ent.T_HR_RESUME = new T_HR_RESUME();
                    ent.T_HR_RESUME.RESUMEID = resume.RESUMEID;
                }
                foreach (var tmp in educateHistory.EducateHistory)
                {
                    tmp.T_HR_RESUME = new T_HR_RESUME();
                    tmp.T_HR_RESUME.RESUMEID = resume.RESUMEID;
                }
                client.ResumeUpdateAsync(resume, experience.ExperienceList, educateHistory.EducateHistory, experience.DelableExp,
                    educateHistory.DelableEdu, strMsg);
            }
        }
        void perclient_GetUserNameIsExistNameAddOneCompleted(object sender, Saas.Tools.PermissionWS.GetUserNameIsExistNameAddOneCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                string strMsg = string.Empty;
                if (!string.IsNullOrEmpty(e.Result))
                {
                    string Result = "";
                    if (Employee.EMPLOYEEENAME != e.Result)
                    {
                        Employee.EMPLOYEEENAME = e.Result;
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            if (!string.IsNullOrEmpty(Employee.FINGERPRINTID))
                            {
                                client.IsExistFingerPrintIDAsync(Employee.FINGERPRINTID, Employee.EMPLOYEEID, "Save");
                            }
                            else
                            {
                                if (btnActionType == "Save")
                                {

                                    client.EmployeeAddOrUpdateAsync(Employee, strMsg);
                                }
                                else
                                {
                                    OnUIRefreshed();
                                }
                            }
                        };
                        com.SelectionBox(Utility.GetResourceStr("CONFIRM"), Utility.GetResourceStr("用户名重复，系统自动更名为" + e.Result), ComfirmWindow.titlename, Result);
                    }
                    else
                    {
                        if (btnActionType == "Save")
                        {
                            client.EmployeeAddOrUpdateAsync(Employee, strMsg);
                        }
                        else
                        {
                            OnUIRefreshed();
                        }
                    }
                }
                else
                {
                    if (btnActionType == "Save")
                    {
                        client.EmployeeAddOrUpdateAsync(Employee, strMsg);
                    }
                    else
                    {
                        OnUIRefreshed();
                    }
                }
            }
        }


        private void InitPara(string sNumberID)
        {
            client.GetEmployeeByNumberIDCompleted += new EventHandler<GetEmployeeByNumberIDCompletedEventArgs>(client_GetEmployeeByNumberIDCompleted);
            client.EmployeeAddCompleted += new EventHandler<EmployeeAddCompletedEventArgs>(client_EmployeeAddCompleted);
            client.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(client_GetEmployeeByIDCompleted);
            client.IsExistFingerPrintIDCompleted += new EventHandler<IsExistFingerPrintIDCompletedEventArgs>(client_IsExistFingerPrintIDCompleted);
            client.EmployeeAddOrUpdateCompleted += new EventHandler<EmployeeAddOrUpdateCompletedEventArgs>(client_EmployeeAddOrUpdateCompleted);
            perclient.GetUserNameIsExistNameAddOneCompleted += new EventHandler<Saas.Tools.PermissionWS.GetUserNameIsExistNameAddOneCompletedEventArgs>(perclient_GetUserNameIsExistNameAddOneCompleted);
            client.GetResumeByNumberCompleted += new EventHandler<GetResumeByNumberCompletedEventArgs>(client_GetResumeByNumberCompleted);
            this.Loaded += new RoutedEventHandler(EmployeeInfoForm_Loaded);
            client.GetEmployeeByNumberIDAsync(sNumberID);
        }

        /// <summary>
        /// 根据ID获取员工信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                Employee = e.Result;
                Employee.EMPLOYEESTATE = "4";//还未入职
                Employee.WORKINGAGE = 0;
                BindData();
                client.GetResumeByNumberAsync(Employee.IDNUMBER);
            }
        }
        /// <summary>
        /// 判断员工指纹编号是否重复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_IsExistFingerPrintIDCompleted(object sender, IsExistFingerPrintIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                             Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == true)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FINGERPRINTIDEXIST"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("FINGERPRINTIDEXIST"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                }
                else
                {
                    if (e.UserState.ToString() == "Save")
                    {
                        if (btnActionType == "Save")
                        {
                            string strMsg = string.Empty;
                            client.EmployeeAddOrUpdateAsync(Employee, strMsg);
                        }
                        else
                        {
                            OnUIRefreshed();
                        }
                    }
                }
            }
        }


        void EmployeeInfoForm_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxIDType.Items)
            {
                if (item.DICTIONARYVALUE == 0)
                {
                    cbxIDType.SelectedItem = item;
                    break;
                }
            }
        }

        void client_GetEmployeeByNumberIDCompleted(object sender, GetEmployeeByNumberIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                Employee = e.Result;
                if (string.IsNullOrEmpty(Employee.EMPLOYEECNAME))
                {
                    Employee.EMPLOYEECNAME = strName;
                }
                if (string.IsNullOrEmpty(Employee.IDNUMBER))
                {
                    Employee.IDNUMBER = sIDNumber;
                }
                Employee.EMPLOYEESTATE = "4";
                Employee.EMPLOYEEENAME = HanziZhuanPingYin.Convert(strName).ToLower();
                GetBirthDay(sIDNumber.Trim());
                BindData();
                client.GetResumeByNumberAsync(sIDNumber);
            }
        }
        private void GetBirthDay(string ID)
        {
            DateTime dt;
            int year = 0;
            int month = 0;
            int day = 0;
            try
            {
                if (ID.Length == 18)
                {
                    year = Convert.ToInt32(ID.Substring(6, 4));
                    month = Convert.ToInt32(ID.Substring(10, 2));
                    day = Convert.ToInt32(ID.Substring(12, 2));
                    dt = new DateTime(year, month, day);
                    Employee.BIRTHDAY = dt;
                }
                else if (ID.Length == 15)
                {
                    year = Convert.ToInt32("19" + ID.Substring(6, 2));
                    month = Convert.ToInt32(ID.Substring(8, 2));
                    day = Convert.ToInt32(ID.Substring(10, 2));
                    dt = new DateTime(year, month, day);
                    Employee.BIRTHDAY = dt;
                }
            }
            catch
            {

            }
        }
        private void BindData()
        {
            this.DataContext = Employee;
        }


        void client_EmployeeAddCompleted(object sender, EmployeeAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EMPLOYEE"));
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "EMPLOYEECODE,EMPLOYEECNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "EMPLOYEECODE,EMPLOYEECNAME"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                OnUIRefreshed();

            }
        }
        public delegate void refreshGridView();
        public event refreshGridView OnUIRefreshed;

        public void RefreshUI()
        {
            if (OnUIRefreshed != null)
            {
                OnUIRefreshed();
            }
        }
        #region 保存
        public void Save(string btnactionType)
        {

            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                //RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            if (cbxEmployeeSex.SelectedIndex < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SEX"));
                //  RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            if (cbxIDType.SelectedIndex < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "IDTYPE"));
                //  RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            if (experience.ExperienceList.Count <= 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("EXPERIENCEADD"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            if (educateHistory.EducateHistory.Count <= 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("EDUCATEHISTORYADD"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

            
            else
            {
                if (chkHas.IsChecked.HasValue)
                {
                    if (chkHas.IsChecked.Value == true)
                    {
                        Employee.HASCHILDREN = "1";
                    }
                    else
                    {
                        Employee.HASCHILDREN = "0";
                    }
                }
                else
                {
                    Employee.HASCHILDREN = "0";
                }
                if (Employee.PHOTO != null)
                {
                    if (Employee.PHOTO.Length > 51200)
                    {
                        string str = "员工头像图片不能大于50k，请压缩后再试";
                        Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(str));
                        return;
                    }
                }
                //所属
                Employee.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                Employee.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                Employee.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                Employee.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                //Employee.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                //Employee.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                //Employee.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                //Employee.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                Employee.CREATEDATE = DateTime.Now;
                Employee.IDTYPE = (cbxIDType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
                Employee.WORKINGAGE = 0;
                Employee.IDNUMBER = Employee.IDNUMBER.ToUpper();
                btnActionType = btnactionType;
                perclient.GetUserNameIsExistNameAddOneAsync(Employee.EMPLOYEEENAME, Employee.EMPLOYEEID);
                //client.EmployeeAddAsync(Employee);
                //if (!string.IsNullOrEmpty(Employee.FINGERPRINTID))
                //{
                //    client.IsExistFingerPrintIDAsync(Employee.FINGERPRINTID, Employee.EMPLOYEEID, "Save");
                //}
                //else
                //{
                //    OnUIRefreshed();
                //}

            }

        }
        #endregion

        private void cbxIDType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item = cbxIDType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;

            if (item.DICTIONARYVALUE != 0)
            {
                txtIDNumber.IsEnabled = true;
            }
            else
            {
                txtIDNumber.IsEnabled = false;
            }
        }

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



        private void txtFingerPrintId_LostFocus(object sender, RoutedEventArgs e)
        {
            string fid = (sender as TextBox).Text;
            if (!string.IsNullOrEmpty(fid))
            {
                client.IsExistFingerPrintIDAsync(fid, Employee.EMPLOYEEID, "Change");
            }
        }
        private void LoadExp()
        {
            if (resume != null)
            {
                client.GetExperienceAllCompleted += (o, e) =>
                    {
                        if (e.Error == null)
                        {
                            if (e.Result != null)
                            {
                                // explist = e.Result.ToList();
                                experience.ExperienceList = e.Result;
                                experience.DtGrid.ItemsSource = experience.ExperienceList;
                            }
                        }
                    };
                client.GetExperienceAllAsync(resume.RESUMEID);
            }
        }
        private void LoadEdu()
        {
            if (resume != null)
            {
                client.GetEducateHistoryAllCompleted += (o, e) =>
                    {
                        if (e.Error == null)
                        {
                            if (e.Result != null)
                            {
                                // edulist = e.Result.ToList();
                                educateHistory.EducateHistory = e.Result;
                                educateHistory.DtEduGrid.ItemsSource = educateHistory.EducateHistory;
                            }
                        }
                    };
                client.GetEducateHistoryAllAsync(resume.RESUMEID);
            }
        }
        private void tbcContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbcContainer != null)
            {
                switch (tbcContainer.SelectedIndex)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        //   LoadExp();
                        break;
                    case 3:
                        //    LoadEdu();
                        break;

                }
            }
        }
    }
}

