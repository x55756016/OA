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
//using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PersonnelWS;
using System.Windows.Browser;
using System.Windows.Data;
using System.Globalization;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmceeMeetingForm : BaseForm, IClient, IEntityEditor
    {
        private Action ActionType;//动作标记
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private T_OA_MEETINGINFO tmpMeetingInfoT;
        private V_MeetingInfo tmpvmeeting = new V_MeetingInfo();
        private T_OA_MEETINGROOM tmpRoom = new T_OA_MEETINGROOM();
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID
        private List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> MeetingStaffs = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE>();
        V_MyMeetingInfosManagement SelectMeeting = new V_MyMeetingInfosManagement();
        V_MyMeetingInfosManagement tmpmeeting = new V_MyMeetingInfosManagement();
        public EmceeMeetingForm(V_MyMeetingInfosManagement objV)
        {
            InitializeComponent();
            GetMeetingInfo(objV);
            //NewButton();
            tmpMeetingInfoT = new T_OA_MEETINGINFO();
            tmpMeetingInfoT = objV.OAMeetingInfoT;
            tmpRoom = objV.meetingroom;// 2010-08-03
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            tmpmeeting = objV;
            DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
            MeetingClient.GetMeetingStaffByMeetingInfoIdEmceeCompleted += new EventHandler<GetMeetingStaffByMeetingInfoIdEmceeCompletedEventArgs>(MeetingClient_GetMeetingStaffByMeetingInfoIdEmceeCompleted);
            LoadMeetingStaffInfos();
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {

        }



        private void LoadMeetingStaffInfos()
        {
            string StrFlag = "";
            string StrIsOk = "";
            if (this.cbxConfirm.SelectedIndex > 0)
            {
                switch (this.cbxConfirm.SelectedIndex)
                {
                    case 1:
                        StrFlag = "1";
                        break;
                    case 2:
                        StrFlag = "0";
                        break;

                }
            }
            if (this.cbxOK.SelectedIndex > 0)
            {
                switch (this.cbxOK.SelectedIndex)
                {
                    case 1:
                        StrIsOk = "1";
                        break;
                    case 2:
                        StrIsOk = "0";
                        break;
                }
            }
            MeetingClient.GetMeetingStaffByMeetingInfoIdEmceeAsync(StrFlag, StrIsOk, tmpmeeting.OAMeetingInfoT.MEETINGINFOID);
        }

        void MeetingClient_GetMeetingStaffByMeetingInfoIdEmceeCompleted(object sender, GetMeetingStaffByMeetingInfoIdEmceeCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    DaGr.ItemsSource = e.Result.ToList();
                }
                else
                {
                    DaGr.ItemsSource = null;
                }
            }
        }

        private void GetMeetingInfo(V_MyMeetingInfosManagement objV)
        {
            //this.tbldepartment.Text = objV.me
            this.tblend.Text = Convert.ToDateTime(objV.OAMeetingInfoT.ENDTIME).ToShortDateString() + " " + Convert.ToDateTime(objV.OAMeetingInfoT.ENDTIME).ToShortTimeString();
            this.tblstart.Text = Convert.ToDateTime(objV.OAMeetingInfoT.STARTTIME).ToShortDateString() + " " + Convert.ToDateTime(objV.OAMeetingInfoT.STARTTIME).ToShortTimeString();
            this.tbltel.Text = objV.OAMeetingInfoT.TEL;
            this.tbltitle.Text = objV.OAMeetingInfoT.MEETINGTITLE;
            this.tblroom.Text = objV.meetingroom.MEETINGROOMNAME;
            this.tbltype.Text = objV.meetingtype.MEETINGTYPE;
            this.tblRecord.Text = objV.OAMeetingInfoT.RECORDUSERNAME;
            this.tblhost.Text = objV.OAMeetingInfoT.HOSTNAME;
            this.tblContent.HideControls();//屏蔽富文本框的头部
            tblContent.RichTextBoxContext = objV.OAMeetingInfoT.CONTENT;
            this.tbldepartment.Text = objV.OAMeetingInfoT.DEPARTNAME;
        }




        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingStaffInfos();
        }

        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingStaffInfos();
        }

        private void PostsObject_FindClick(object sender, EventArgs e)
        {

        }

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            V_MyMeetingInfosManagement Employ = (V_MyMeetingInfosManagement)e.Row.DataContext;
            CheckBox mychkBox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            mychkBox.Tag = Employ;
        }


        #region IEntityEditor
        public string GetTitle()
        {
            return "";

        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            Save();
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
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/16_close.png"

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

            RefreshUI(RefreshedTypes.Close);
        }

        private void SaveAndClose()
        {
            Save();
            //RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        private void DaGr_Loaded(object sender, RoutedEventArgs e)
        {

        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            V_MyMeetingInfosManagement VMeetingStaff = new V_MyMeetingInfosManagement();
            if (DaGr.ItemsSource != null)
            {
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        if (cb1.IsChecked == true)
                        {
                            VMeetingStaff = cb1.Tag as V_MyMeetingInfosManagement;
                            break;
                        }
                    }
                }

            }
            if (VMeetingStaff.OAMeetingStaffT == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                if (SelectMeeting.OAMeetingStaffT.ISOK == "1" || SelectMeeting.OAMeetingStaffT.ISOK == "2")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ISOKNOTCONFIRMAGAIN"));
                    return;
                }
                else
                {
                    EmceeMemberMeetingForm AddWin = new EmceeMemberMeetingForm(tmpMeetingInfoT, VMeetingStaff, tmpRoom);
                    EntityBrowser browser = new EntityBrowser(AddWin);
                    browser.MinHeight = 650;
                    browser.MinWidth = 600;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

                    //T_OA_MEETINGSTAFF staff = new T_OA_MEETINGSTAFF();
                    //staff = SelectMeeting.OAMeetingStaffT;
                    //staff.ISOK = "2";////确认不通过
                    //MeetingClient.MeetingStaffUpdateInfosCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingClient_MeetingStaffUpdateInfosCompleted);
                    //MeetingClient.MeetingStaffUpdateInfosAsync(staff);

                }


            }
        }

        void AddWin_ReloadDataEvent()
        {
            LoadMeetingStaffInfos();
        }
        private void NoPassedBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectMeeting == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                if (SelectMeeting.OAMeetingStaffT.ISOK == "1" || SelectMeeting.OAMeetingStaffT.ISOK == "2")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ISOKNOTCONFIRMAGAIN"));
                    return;
                }
                else
                {
                    T_OA_MEETINGSTAFF staff = new T_OA_MEETINGSTAFF();
                    staff = SelectMeeting.OAMeetingStaffT;
                    staff.ISOK = "2";////确认不通过
                    MeetingClient.MeetingStaffUpdateInfosCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingClient_MeetingStaffUpdateInfosCompleted);
                    MeetingClient.MeetingStaffUpdateInfosAsync(staff);

                }


            }
        }

        void MeetingClient_MeetingStaffUpdateInfosCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CONFIRMSUCCESSED"));
                LoadMeetingStaffInfos();
            }
        }

        private void PassedBtn_Click(object sender, RoutedEventArgs e)
        {

            if (SelectMeeting == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                if (SelectMeeting.OAMeetingStaffT.ISOK == "1" || SelectMeeting.OAMeetingStaffT.ISOK == "2")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ISOKNOTCONFIRMAGAIN"));
                    return;
                }
                else
                {
                    T_OA_MEETINGSTAFF staff = new T_OA_MEETINGSTAFF();
                    staff = SelectMeeting.OAMeetingStaffT;
                    staff.ISOK = "1"; //确认通过
                    MeetingClient.MeetingStaffUpdateInfosCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingClient_MeetingStaffUpdateInfosCompleted);
                    MeetingClient.MeetingStaffUpdateInfosAsync(staff);
                }
            }
        }


        #region 添加报告Button
        public void NewButton()
        {

            ToolBar.stpOtherAction.Children.Clear();
            ImageButton ChangeMeetingBtn = new ImageButton();
            ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
            ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("PASSED");
            ChangeMeetingBtn.Image.Width = 16.0;
            ChangeMeetingBtn.Image.Height = 22.0;
            ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            ChangeMeetingBtn.Click += new RoutedEventHandler(PassedBtn_Click);
            ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);

            ImageButton MeetingCancelBtn = new ImageButton();
            MeetingCancelBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
            MeetingCancelBtn.TextBlock.Text = Utility.GetResourceStr("NOPASSED");
            MeetingCancelBtn.Image.Width = 16.0;
            MeetingCancelBtn.Image.Height = 22.0;
            MeetingCancelBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            MeetingCancelBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            MeetingCancelBtn.Click += new RoutedEventHandler(NoPassedBtn_Click);


            ToolBar.stpOtherAction.Children.Add(MeetingCancelBtn);
        }


        #endregion

        private void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DaGr.SelectedItems.Count == 0)
                return;

            SelectMeeting = DaGr.SelectedItems[0] as V_MyMeetingInfosManagement;
        }

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
    }
}
