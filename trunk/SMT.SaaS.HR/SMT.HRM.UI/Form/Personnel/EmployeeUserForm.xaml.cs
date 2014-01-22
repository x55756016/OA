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
using System.Collections.ObjectModel;
using SMT.HRM.UI.CommForm;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeUserForm : BaseForm, IClient
    {
        PersonnelServiceClient client;
        public ObservableCollection<string> leaveMessage;
        public EmployeeUserForm()
        {
            InitializeComponent();
            InitEvent();
        }
        public EmployeeUserForm(FormTypes type, string strID)
        {
            InitializeComponent();
            InitEvent();
            client.GetEmployeeEntryByIDAsync(strID);
        }
        private void InitEvent()
        {
            client = new PersonnelServiceClient();
            client.EmployeeIsEntryCompleted += new EventHandler<EmployeeIsEntryCompletedEventArgs>(client_EmployeeIsEntryCompleted);
            client.GetEmployeeEntryByIDCompleted += new EventHandler<GetEmployeeEntryByIDCompletedEventArgs>(client_GetEmployeeEntryByIDCompleted);
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
        public void save()
        {
            //验证      
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                return;
            }
            //身份证号不能为空
            if (string.IsNullOrEmpty(sNumberID.Text.Trim()))
            {

                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "IDCARDNUMBER"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "IDCARDNUMBER"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                sNumberID.Focus();
                return;
            }
            //检查身份证
            string blackMessage = "";
            //string leaveMessage = "";
            leaveMessage = new ObservableCollection<string>();
            client.EmployeeIsEntryAsync(sNumberID.Text.Trim().ToUpper(), blackMessage, leaveMessage);

        }
        #endregion
        #region 完成事件
        void client_GetEmployeeEntryByIDCompleted(object sender, GetEmployeeEntryByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                sName.Text = e.Result.T_HR_EMPLOYEE.EMPLOYEECNAME;
                sNumberID.Text = e.Result.T_HR_EMPLOYEE.IDNUMBER;
            }
        }
        void client_EmployeeIsEntryCompleted(object sender, EmployeeIsEntryCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //// 黑名单存在
                //if (e.Error.Message == "BLACKLISTEXIST")
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("BLACKLISTEXIST"));
                ////离职
                //else if (e.Error.Message == "EMPLOYEELEAVE")
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EMPLOYEELEAVE"));
                //else
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.blackMessage))
                {
                    if (e.blackMessage != "0")
                    {
                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("BLACKLISTEXIST"));
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("BLACKLISTEXIST"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }

                }
                if (e.Result == false)
                {

                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("已经在职"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("已经在职"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                if (e.leaveMessage.Count > 0)
                {
                    leaveMessage.Clear();
                    leaveMessage.Add(e.leaveMessage[0]);
                    leaveMessage.Add(e.leaveMessage[1]);
                    if (!string.IsNullOrEmpty(e.leaveMessage[1]))
                    {
                        if (e.leaveMessage[1] == "LessThan")
                        {
                            string Result = "";
                            SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                            com.OnSelectionBoxClosed += (obj, result) =>
                            {
                                RefreshUI();
                            };
                            com.SelectionBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("离职未满6个月,确定要入职吗？"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                        }
                        else if (e.leaveMessage[1] == "LimitEntry")
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), 
                                Utility.GetResourceStr("该身份证号已存在未提交或审核中的入职记录，请处理完后再入职."),
                                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                        else if (e.leaveMessage[1] == "UnApproved")
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                               Utility.GetResourceStr("该身份证号已存在审核未通过的入职记录，请直接进行重新提交操作."),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                    }
                    else
                    {
                        RefreshUI();
                    }
                }
                else
                {
                    RefreshUI();
                }

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
    }
}
