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
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.Saas.Tools.PersonnelWS;
//using SMT.Saas.Tools.PersonnelWS;
using System.Collections.ObjectModel;
using System.Windows.Browser;
using System.Text;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Class;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MeetingDetailForm : BaseForm,IClient, IEntityEditor,IAudit
    {
        public MeetingDetailForm()
        {
            InitializeComponent();
        }

        private T_OA_MEETINGINFO tmpMeetingInfoT;
        private V_MeetingInfo tmpvmeeting = new V_MeetingInfo();
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID
        private List<T_HR_EMPLOYEE> MeetingStaffs = new List<T_HR_EMPLOYEE>();
        private RefreshedTypes refreshType = RefreshedTypes.All;
        List<T_OA_MEETINGSTAFF> tmpMeetingStaff = new List<T_OA_MEETINGSTAFF>();
        public MeetingDetailForm(V_MeetingInfo obj)
        {
            InitializeComponent();
            tmpvmeeting = obj;
            tmpMeetingInfoT = obj.meetinginfo;
            GetMeetingStaffInfo(obj.meetinginfo);
            ShowMeetingDetailInfos(obj.meetingtype);
            tblMeetingContent.HideControls();
            ShowMeetingChangeInfos(); //会议时间变更
            ShowMeetingRoomTimeChangeInfos(); //获取会议室使用时间变更
            MeetingClient.GetMeetingContentInfosByMeetngInfoIDCompleted += new EventHandler<GetMeetingContentInfosByMeetngInfoIDCompletedEventArgs>(ContentClient_GetMeetingContentInfosByMeetngInfoIDCompleted);
            MeetingClient.MeetingInfoUpdateCompleted += new EventHandler<MeetingInfoUpdateCompletedEventArgs>(MeetingInfoClient_MeetingInfoUpdateCompleted);
            MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDCompleted += new EventHandler<GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs>(MeetingClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted);
            this.Loaded += new RoutedEventHandler(MeetingDetailForm_Loaded);
        }

        void MeetingDetailForm_Loaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            RefreshUI(RefreshedTypes.AuditInfo);
        }

        void ShowMeetingDetailInfos(T_OA_MEETINGTYPE typeobj)
        {
            this.tblTitle.Text = tmpMeetingInfoT.MEETINGTITLE + "会议基本信息";
            this.tblMeetingTitle.Text = tmpMeetingInfoT.MEETINGTITLE;
            this.tblMeetingType.Text = tmpvmeeting.meetingtype.MEETINGTYPE;
            this.tblDepartment.Text = tmpMeetingInfoT.DEPARTNAME;
            this.tblMeetingRoom.Text = tmpvmeeting.meetingroom.MEETINGROOMNAME;
            this.tblStartTime.Text = Convert.ToDateTime(tmpMeetingInfoT.STARTTIME).ToLongDateString() + Convert.ToDateTime(tmpMeetingInfoT.STARTTIME).ToLongTimeString();
            this.tblEndTime.Text = Convert.ToDateTime(tmpMeetingInfoT.ENDTIME).ToLongDateString() + Convert.ToDateTime(tmpMeetingInfoT.ENDTIME).ToLongTimeString();
            tblMeetingContent.RichTextBoxContext = tmpMeetingInfoT.CONTENT;
            if (tmpMeetingInfoT.UPDATEDATE != null)
            {
                this.tblUpdateTime.Text = tmpMeetingInfoT.UPDATEDATE.ToString();
            }
            else
            {
                this.tblUpdateTime.Text = "";
            }
            if (!string.IsNullOrEmpty(tmpMeetingInfoT.UPDATEUSERNAME))
                this.tblEditer.Text = tmpMeetingInfoT.UPDATEUSERNAME;
            tblMemberCount.Text = tmpMeetingInfoT.COUNT.ToString();
            tblAddTime.Text = Convert.ToDateTime(tmpMeetingInfoT.CREATEDATE).ToLongDateString() + Convert.ToDateTime(tmpMeetingInfoT.CREATEDATE).ToLongTimeString();
            //RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
        }

        #region 显示与会人员上传的内容

        

        void ContentClient_GetMeetingContentInfosByMeetngInfoIDCompleted(object sender, GetMeetingContentInfosByMeetngInfoIDCompletedEventArgs e)
        {
            List<T_OA_MEETINGCONTENT> ListContent = new List<T_OA_MEETINGCONTENT>();
            ListContent = e.Result.ToList();
            StringBuilder sbInfos = new StringBuilder();
            foreach (T_OA_MEETINGCONTENT SingleInfo in ListContent)
            { 
                sbInfos.Append("与会人：");
                if (MeetingStaffs.Count > 0)
                { 
                    foreach(SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee in MeetingStaffs)
                    {
                        if (SingleInfo.MEETINGUSERID == employee.EMPLOYEEID)
                        {
                            sbInfos.Append(employee.EMPLOYEECNAME);
                            sbInfos.Append("\n");
                        }
                    }
                }
                
                sbInfos.Append(HttpUtility.HtmlDecode(SingleInfo.CONTENT));
                sbInfos.Append("\n");
                if (string.IsNullOrEmpty(SingleInfo.UPDATEDATE.ToString()))
                {
                    //sbInfos.Append("<font color='red'>还没上传内容</font>\n");
                    sbInfos.Append("还没上传内容\n");
                }
                sbInfos.Append("\n\n");
            }
            
            string StrTmp = "";
            StrTmp = HttpUtility.HtmlEncode(sbInfos.ToString());
            tblUploadContent.Text = HttpUtility.HtmlDecode(StrTmp);
        }

        #endregion

        #region 显示与会人员上传的附件
        

        void StaffClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted(object sender, GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs e)
        {
            StringBuilder sbInfos = new StringBuilder();
            if (e.Result != null)
            {
                List<T_OA_MEETINGSTAFF> ListContent = new List<T_OA_MEETINGSTAFF>();

                ListContent = e.Result.ToList();

                foreach (T_OA_MEETINGSTAFF SingleInfo in ListContent)
                {

                    sbInfos.Append("与会人：");                    
                    if (MeetingStaffs.Count > 0)
                    {
                        foreach (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee in MeetingStaffs)
                        {
                            if (SingleInfo.MEETINGUSERID == employee.EMPLOYEEID)
                            {
                                sbInfos.Append(employee.EMPLOYEECNAME);
                                sbInfos.Append("\n");
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(SingleInfo.FILENAME))
                    {
                        sbInfos.Append(SingleInfo.FILENAME);
                    }
                    else
                    {
                        sbInfos.Append("没有附件上传");
                    }

                    sbInfos.Append("\n\n");
                }
            }
            else
            {
                sbInfos.Append("暂无信息");
            }


            tblUploadAccessory.Text = sbInfos.ToString();
        }
        #endregion

        void ShowMeetingChangeInfos()
        { 

        }

        void ShowMeetingRoomTimeChangeInfos()
        { 

        }

        #region 获取参会人员信息
        /// <summary>
        /// 获取参会人员
        /// </summary>
        /// <param name="MeetingT"></param>
        private void GetMeetingStaffInfo(T_OA_MEETINGINFO MeetingT)
        {
            if (MeetingT != null)
            {
                
                MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDAsync(MeetingT.MEETINGINFOID);
            }
        }

        void MeetingClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted(object sender, GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<T_OA_MEETINGSTAFF> stafflist = new List<T_OA_MEETINGSTAFF>();
                stafflist = e.Result.ToList();
                tmpMeetingStaff = e.Result.ToList();
                foreach (T_OA_MEETINGSTAFF staff in stafflist)
                {
                    StrStaffList.Add(staff.MEETINGUSERID);
                }
                if (StrStaffList.Count > 0)
                {
                    GetMeetingStaff(StrStaffList);
                }
            }

        }

        /// <summary>
        /// 获取参会人员并填充
        /// </summary>
        private void GetMeetingStaff(ObservableCollection<string> staffs)
        {
            PersonnelServiceClient personclient = new PersonnelServiceClient();
            //personclient.getem
            personclient.GetEmployeeByIDsCompleted += new EventHandler<GetEmployeeByIDsCompletedEventArgs>(personclient_GetEmployeeByIDsCompleted);
            personclient.GetEmployeeByIDsAsync(staffs);

        }

        void personclient_GetEmployeeByIDsCompleted(object sender, GetEmployeeByIDsCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Result != null)
            {                
                MeetingStaffs = e.Result.ToList();
                MeetingClient.GetMeetingContentInfosByMeetngInfoIDAsync(tmpMeetingInfoT.MEETINGINFOID);
                StringBuilder sbInfos = new StringBuilder();
                if (e.Result != null)
                {
                    

                    foreach (T_OA_MEETINGSTAFF SingleInfo in tmpMeetingStaff)
                    {

                        sbInfos.Append("与会人：");
                        if (MeetingStaffs.Count > 0)
                        {
                            foreach (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee in MeetingStaffs)
                            {
                                if (SingleInfo.MEETINGUSERID == employee.EMPLOYEEID)
                                {
                                    sbInfos.Append(employee.EMPLOYEECNAME);
                                    sbInfos.Append("\n");
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(SingleInfo.FILENAME))
                        {
                            sbInfos.Append(SingleInfo.FILENAME);
                        }
                        else
                        {
                            sbInfos.Append("没有附件上传");
                        }

                        sbInfos.Append("\n\n");
                    }
                }
                else
                {
                    sbInfos.Append("暂无信息");
                }


                tblUploadAccessory.Text = sbInfos.ToString();
                //MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDAsync(tmpMeetingInfoT.MEETINGINFOID);
            }
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {

            return Utility.GetResourceStr("VIEWTITLE", "MEETINGADD");
            
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
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_close.png"
            };
            
            items.Add(item);

            return items;
        }

        private void Close()
        {
            RefreshUI(refreshType);
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
 
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            MeetingClient.DoClose();
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

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_MEETINGINFO>(tmpMeetingInfoT, "OA");

            Utility.SetAuditEntity(entity, "MeetingInfo", tmpMeetingInfoT.MEETINGINFOID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (tmpMeetingInfoT.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            tmpMeetingInfoT.CHECKSTATE = state;
            MeetingClient.MeetingInfoUpdateAsync(tmpMeetingInfoT, UserState);

        }

        public string GetAuditState()
        {

            string state = "-1";
            //if (tmpMeetingInfoT != null)
            //    state = tmpMeetingInfoT.CHECKSTATE;
            //if (ActionType == FormTypes.Browse)
            //{
            //    state = "-1";
            //}
            return state;
        }

        #endregion

        #region 更新操作
        /// <summary>
        /// 针对审核的动作  审核时 只修改会议主表 其它不操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MeetingInfoClient_MeetingInfoUpdateCompleted(object sender, MeetingInfoUpdateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (!e.Cancelled)
                {
                    if (e.Error == null)
                    {
                        if (e.Result > 0)
                        {

                            if (e.UserState.ToString() == "Edit")
                            {
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                                if (GlobalFunction.IsSaveAndClose(refreshType))
                                {
                                    RefreshUI(refreshType);
                                }
                                else
                                {
                                    //ActionType = FormTypes.Edit;
                                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                                    entBrowser.FormType = FormTypes.Edit;
                                    RefreshUI(RefreshedTypes.AuditInfo);
                                    RefreshUI(RefreshedTypes.All);
                                }

                                
                            }
                            else if (e.UserState.ToString() == "Audit")
                            {
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                            }
                            else if (e.UserState.ToString() == "Submit")
                            {
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                            }
                            RefreshUI(RefreshedTypes.All);

                            
                        }
                    }//需要将修改记录添加到会议修改表中
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), e.Error.Message.ToString());
                    }
                }
                else
                {
                    //MessageBox.Show(e.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.ToString());
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }

        }
        #endregion
    }
}
