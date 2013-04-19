using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Data;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows.Media.Imaging;

namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MeetingManagementInfos : BasePage,IClient
    {     
        #region 装载数据
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();        
        private T_OA_MEETINGINFO tmpInfoT = new T_OA_MEETINGINFO();
        SMT.SaaS.FrameworkUI.AuditControl.AuditControl audit = new SMT.SaaS.FrameworkUI.AuditControl.AuditControl();
        private string tmpStrUserID = "1";
        private string EditState = ""; //修改状态 这里有审核的过程  故用此来区分
        //private string checkState = "0";
        private string StrDepartmentID = ""; //部门ID
        private string StrDepartmentName = "";// 部门名
        private string checkState = ((int)CheckStates.ALL).ToString(); //等待审核
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        CheckBox SelectBox = new CheckBox();
        private bool IsQueryBtn = false; //是否是查询
        private ObservableCollection<string> DelInfosList;

        private SMTLoading loadbar = new SMTLoading();

        private V_MeetingInfo meetinginfo;

        public V_MeetingInfo Meetinginfo
        {
            get { return meetinginfo; }
            set { meetinginfo = value; }
        }
        public MeetingManagementInfos()
        {            
            InitializeComponent();
            this.Loaded+=new RoutedEventHandler(MeetingManagementInfos_Loaded);
        }

        private void InitEvent()
        {

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(MeetingDetailBtn_Click);            
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "5");            
            MeetingClient.MeetingInfoUpdateCompleted += new EventHandler<MeetingInfoUpdateCompletedEventArgs>(MeetingClient_MeetingInfoUpdateCompleted);
            MeetingClient.GetMeetingInfoListByFlowCompleted += new EventHandler<GetMeetingInfoListByFlowCompletedEventArgs>(MeetingInfoClient_GetMeetingInfoListByFlowCompleted);
            MeetingClient.MeetingInfoBatchDelCompleted += new EventHandler<MeetingInfoBatchDelCompletedEventArgs>(MeetingClient_MeetingInfoBatchDelCompleted);
            
            //MeetingClient.meetinginfo
            SetButtonVisible();
            //LoadMeetingInfos();
            DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
            ToolBar.ShowRect();
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Meetinginfo != null)
            {
                if (Meetinginfo.meetinginfo.CHECKSTATE == "2" || meetinginfo.meetinginfo.CHECKSTATE == "3")
                {
                    MeetingForm form = new MeetingForm(FormTypes.Resubmit, Meetinginfo);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Resubmit;
                    browser.MinHeight = 650;
                    browser.MinWidth = 800;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("MEETINGINFONORESUBMIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "SUBMITAUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Meetinginfo = (V_MeetingInfo)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void MeetingManagementInfos_Loaded(object sender, RoutedEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_MEETINGINFO", true);
            PARENT.Children.Add(loadbar);
            combox_SelectSource();
            InitEvent();
            GetEntityLogo("T_OA_MEETINGINFO");
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingInfos();
        }

       

        void MeetingClient_MeetingInfoBatchDelCompleted(object sender, MeetingInfoBatchDelCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", ""));
                    LoadMeetingInfos();
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", ""));
                    return;
                }
            }
        }

        void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            V_MeetingInfo MeetingInfoT = new V_MeetingInfo();

            if (DaGr.ItemsSource != null)
            {
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        if (cb1.IsChecked == true)
                        {
                            //MeetingInfoId = cb1.Tag.ToString();
                            MeetingInfoT = cb1.Tag as V_MeetingInfo;
                            break;
                        }
                    }
                }

            }

            if (MeetingInfoT.meetinginfo != null)
            {
                if (MeetingInfoT.meetinginfo.CHECKSTATE == "2")
                {
                    //MessageBox.Show("信息已经审核通过，不能修改");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(""), Utility.GetResourceStr(""));
                    
                }
                else
                {
                    if (checkState == "0")  //未提交的审核 
                    {
                        //SumbitFlow(audit, tmpInfoT);
                    }
                    
                }
            }
            else
            {
                
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void MeetingClient_MeetingInfoUpdateCompleted(object sender, MeetingInfoUpdateCompletedEventArgs e)
        {
            
            if (!e.Cancelled)
            {
                //MessageBox.Show("会议已取消");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MEETINGCANCELEDSUCCESSED"));
                LoadMeetingInfos();
            }
        }

        

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();

            if (Meetinginfo != null)
            {
                if (Meetinginfo.meetinginfo.CHECKSTATE == "2")
                {                    
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("INOFSUBMITEDNOTEDIT"));
                    return;
                }
                else
                {
                    if (checkState == "0")  //未提交的审核 
                    {
                        //SumbitFlow(audit, tmpInfoT);                        
                    }
                    else
                    {
                        if (checkState == "1" || checkState == "4") //待审核 或正审核
                        {
                            MeetingForm form = new MeetingForm(FormTypes.Audit, Meetinginfo);
                            EntityBrowser browser = new EntityBrowser(form);
                            browser.FormType = FormTypes.Audit;
                            browser.MinWidth = 800;
                            browser.MinHeight = 580;
                            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });


                        }
                    }


                }

                
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            
        }

        void LoadMeetingInfos()
        {
            string StrTitle = "";
            string StrType = "";
            string StrDepartment = "";
            string StrContent = "";

            string StrStart = "";
            string StrEnd = "";
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (checkState == ((int)CheckStates.UnSubmit).ToString())
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "meetinginfo.OWNERID==@" + paras.Count().ToString();
                paras.Add(Common.CurrentLoginUserInfo.EmployeeID);
            }
            


            if (IsQueryBtn)
            {
                StrTitle = txtMeetingTitle.Text.Trim().ToString();                
                if (cbMeetingType.SelectedIndex >0) //if (cbMeetingType.SelectedItem.ToString() != "请选择")
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    StrType = ((cbMeetingType.SelectedItem) as T_OA_MEETINGTYPE).MEETINGTYPE.ToString();
                    filter += "meetingtype.meetingtype ==@" + paras.Count().ToString();
                    paras.Add(StrType);

                }
                if (!string.IsNullOrEmpty(StrTitle))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "meetinginfo.MEETINGTITLE ^@" + paras.Count().ToString();
                    paras.Add(StrTitle);
                }
                

                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    DtStart = System.Convert.ToDateTime(StrStart);
                    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                    if (DtStart > DtEnd)
                    {
                        //MessageBox.Show("开始时间不能大于结束时间");
                        Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("SEARCH"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                        return;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            if (!string.IsNullOrEmpty(filter))
                            {
                                filter += " and ";
                            }
                            filter += "meetinginfo.CREATEDATE >=@" + paras.Count().ToString();
                            paras.Add(DtStart);
                            if (!string.IsNullOrEmpty(filter))
                            {
                                filter += " and ";
                            }
                            filter += "meetinginfo.CREATEDATE <=@" + paras.Count().ToString();
                            paras.Add(DtEnd);
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))//选择了开始时间
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "meetinginfo.CREATEDATE >=@" + paras.Count().ToString();
                    paras.Add(System.Convert.ToDateTime(StrStart));
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))//选择了结束时间
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "meetinginfo.CREATEDATE <=@" + paras.Count().ToString();
                    paras.Add(System.Convert.ToDateTime(StrEnd));
                }
            }

            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loadbar.Start();
            MeetingClient.GetMeetingInfoListByFlowAsync(dataPager.PageIndex, dataPager.PageSize, "meetinginfo.CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
            
        }

        void MeetingInfoClient_GetMeetingInfoListByFlowCompleted(object sender, GetMeetingInfoListByFlowCompletedEventArgs e)
        {       
            if (e.Result != null)
            {
                SelectBox = Utility.FindChildControl<CheckBox>(DaGr, "SelectAll");
                BindDataGrid(e.Result.ToList(), e.pageCount);
            }
            else
            {
                BindDataGrid(null, 0);
            }
            loadbar.Stop();
            
        }

        #region  绑定DataGird
        private void BindDataGrid(List<V_MeetingInfo> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }
        #endregion 

        
        #endregion

        #region COMBOX 设置数据源

        private void combox_SelectSource()
        {
            //MeetingTypeManagementServiceClient typeClient = new MeetingTypeManagementServiceClient();
            MeetingClient.GetMeetingTypeNameInfosToComboxAsync();
            MeetingClient.GetMeetingTypeNameInfosToComboxCompleted += new EventHandler<GetMeetingTypeNameInfosToComboxCompletedEventArgs>(typeClient_GetMeetingTypeNameInfosToComboxCompleted);

        }

        void typeClient_GetMeetingTypeNameInfosToComboxCompleted(object send, GetMeetingTypeNameInfosToComboxCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.cbMeetingType.Items.Clear();
                //List<T_OA_MEETINGTYPE>  TypeNames= e.Result.ToList();
                //T_OA_MEETINGTYPE MeetingType = new T_OA_MEETINGTYPE();
                //MeetingType.MEETINGTYPEID = System.Guid.NewGuid().ToString();

                //MeetingType.MEETINGTYPE = "请选择";


                //string StrInsert = "请选择";
                //TypeNames.Insert(0, StrInsert);

                T_OA_MEETINGTYPE Meetingtype = new T_OA_MEETINGTYPE();
                Meetingtype.MEETINGTYPE = "请选择";
                e.Result.Insert(0, Meetingtype);
                this.cbMeetingType.ItemsSource = e.Result;
                this.cbMeetingType.DisplayMemberPath = "MEETINGTYPE";
                //this.cbMeetingType.SelectedIndex = 0;                

            }
        }
        #endregion

        #region 添加按钮
        

        void AddWin_ReloadDataEvent()
        {
            LoadMeetingInfos();
        }
        #endregion       

        #region 删除按钮控件事件
        //这里使用一个确认框，需要引入System.Windows.Browser;.
        
        private void GetMeetingInfoByID(string StrMeetingInfoID)
        {
            MeetingClient.MeetingInfoDelCompleted += new EventHandler<MeetingInfoDelCompletedEventArgs>(MeetingInfoClient_MeetingInfoDelCompleted);
            MeetingClient.MeetingInfoDelAsync(StrMeetingInfoID);

        }


        void MeetingInfoClient_MeetingInfoDelCompleted(object sender, MeetingInfoDelCompletedEventArgs e)
        {
            MainPage current = Application.Current.RootVisual as MainPage;
            current.HideWaitingControl();
        }

        #endregion

        #region DaGr LoadingRow事件


        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_MEETINGINFO");
        }

        #endregion

        #region 复选框按钮事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {

            


            //if (DaGr.ItemsSource != null)
            //{
            //    if (this.chkAll.IsChecked.Value)//全选
            //    {

            //        foreach (object obj in DaGr.ItemsSource)
            //        {
            //            if (DaGr.Columns[0].GetCellContent(obj) != null)
            //            {
            //                CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox; //cb为
            //                cb1.IsChecked = true;
            //            }
            //        }
            //        chkAll.Content = "全不选";
            //    }
            //    else//取消
            //    {
            //        foreach (object obj in DaGr.ItemsSource)
            //        {
            //            if (DaGr.Columns[0].GetCellContent(obj) != null)
            //            {
            //                CheckBox cb2 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox;
            //                cb2.IsChecked = false;
            //            }
            //        }
            //        chkAll.Content = "全选";
            //    }
            //}

        }
        #endregion

        #region 模板中CheckBox单击事件
        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.IsChecked == false)
            {
                if (SelectBox != null)
                {
                    if (SelectBox.IsChecked == true)
                    {
                        SelectBox.IsChecked = false;
                    }
                }
            }

        }
        #endregion       

        #region 查询按钮事件

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQueryBtn = true;
            LoadMeetingInfos();
            //string StrTitle = "";
            //string StrType = "";
            //string StrDepartment = "";
            //string StrContent = "";
            
            //string StrStart = "";
            //string StrEnd = "";
            
            
            //StrTitle = txtMeetingTitle.Text.Trim().ToString();
            //if (cbMeetingType.SelectedItem.ToString() != "请选择")
            //{
            //    StrType = cbMeetingType.SelectedItem.ToString();
            //}
            //StrContent = txtContent.Text.Trim().ToString();

            //StrStart = dpStart.Text.ToString();
            //StrEnd = dpEnd.Text.ToString();
            //DateTime DtStart = new DateTime();
            //DateTime DtEnd = new DateTime();
            //if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            //{
            //    DtStart = System.Convert.ToDateTime(StrStart);
            //    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
            //    if (DtStart > DtEnd)
            //    {
            //        MessageBox.Show("开始时间不能大于结束时间");
            //        return;
            //    }
            //}

            //MeetingClient.GetMeetintInfosListByTitleTimeSearchCompleted += new EventHandler<GetMeetintInfosListByTitleTimeSearchCompletedEventArgs>(MeetingInfoClient_GetMeetintInfosListByTitleTimeSearchCompleted);
            //MeetingClient.GetMeetintInfosListByTitleTimeSearchAsync(StrTitle, StrDepartment, StrType, StrContent, DtStart, DtEnd, checkState);
        }


        void MeetingInfoClient_GetMeetintInfosListByTitleTimeSearchCompleted(object sender, GetMeetintInfosListByTitleTimeSearchCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    List<T_OA_MEETINGINFO> infos = new List<T_OA_MEETINGINFO>(e.Result);
                    if (infos.Count() > 0)
                    {
                        //dpGrid.PageSize = 3;
                        PagedCollectionView pager = new PagedCollectionView(infos);
                        DaGr.ItemsSource = pager;
                    }
                }
                else
                {
                    MessageBox.Show("对不起，没找到您需要的数据！");
                }

            }
        }


        #endregion

        #region 变更会议时间

        private void ChangeMeetingBtn_Click(object sender, RoutedEventArgs e)
        {
            string MeetingInfoID = "";
            //V_MeetingInfo MeetingInfoT = new V_MeetingInfo();
            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为

            //            if (cb1.IsChecked == true)
            //            {
            //                MeetingInfoT = cb1.Tag as V_MeetingInfo;
            //                break;

            //            }
            //        }
            //    }

            //}
            
            if (Meetinginfo !=null)
            {
                string Result = "";
                SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    
                    try
                    {
                        ChangeMeetingTimeForm form = new ChangeMeetingTimeForm(FormTypes.New, Meetinginfo);
                        EntityBrowser browser = new EntityBrowser(form);
                        browser.MinHeight = 400;
                        browser.MinWidth = 500;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result1) => { }, true);

                    }
                    catch (Exception ex)
                    {
                        throw(ex);
                        //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                    }
                    
                };
                com.SelectionBox(Utility.GetResourceStr("CHANGECONFIRM"), Utility.GetResourceStr("CHANGEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);

            }
            else
            {
                //MessageBox.Show("请您选择需要删除的数据！");
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "DELETE"));
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            
        }
        #endregion

        #region 查看会议的详细信息
        private void MeetingDetailBtn_Click(object sender, RoutedEventArgs e)
        {

            if (Meetinginfo != null)
            {
                MeetingDetailForm form = new MeetingDetailForm(Meetinginfo);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 580;
                browser.MinWidth = 800;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            

            
        }
        #endregion

        #region 提交申请按钮

        private void MeetingAppBtn_Click(object sender, RoutedEventArgs e)
        {
            Button AppBtn = sender as Button;
            T_OA_MEETINGINFO InfoT = AppBtn.Tag as T_OA_MEETINGINFO;
            tmpInfoT = InfoT;
            InfoT.CHECKSTATE = "0";            
            MeetingClient.MeetingInfoUpdateCompleted += new EventHandler<MeetingInfoUpdateCompletedEventArgs>(MeetingInfoClient_MeetingInfoUpdateCompleted);
            MeetingClient.MeetingInfoUpdateAsync(InfoT);            

        }
        void MeetingInfoClient_MeetingInfoUpdateCompleted(object sender, MeetingInfoUpdateCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                if (EditState == "")
                {
                    
                }
                else
                {
                    //MessageBox.Show("取消申请成功");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CANCELSUCCESSED","MEETING"));
                    //添加1条取消会议通知
                    //T_OA_MEETINGMESSAGE Message = new T_OA_MEETINGMESSAGE();
                    //Message.MEETINGMESSAGEID = System.Guid.NewGuid().ToString();
                    //Message.TITLE = Utility.GetResourceStr("CANCEL")+tmpInfoT.MEETINGTITLE+Utility.GetResourceStr("NOTICES");

                    this.LoadMeetingInfos();
                }
                
            }
                        

        }

        void MeetingInfoClient_SubmitFlowCompleted(object sender,SubmitFlowCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                //MessageBox.Show("提交成功");
                //LoadMeetingInfos(tmpStrUserID, checkState);
                LoadMeetingInfos();
            }
        }

        void MeetingInfoClient_SubmitCommentCompleted(object sender, SubmitCommentCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                MessageBox.Show("提交成功");
                //LoadMeetingInfos(tmpStrUserID, checkState);
                LoadMeetingInfos();
            }
        }

        #endregion

        #region 取消会议
        
        

        private void MeetingCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            string MeetingInfoID = "";
             
            //V_MeetingInfo MeetingInfoT = new V_MeetingInfo();
            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为

            //            if (cb1.IsChecked == true)
            //            {
            //                MeetingInfoT = cb1.Tag as V_MeetingInfo;
            //                break;

            //            }
            //        }
            //    }

            //}
            

            if (Meetinginfo != null)
            {
                string Result = "";
                SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    
                    try
                    {
                        //tmpInfoT = MeetingInfoT.meetinginfo;
                        //tmpInfoT.CHECKSTATE = "3";                        
                        
                        //MeetingClient.MeetingInfoUpdateAsync(tmpInfoT);

                        CancelMeetingForm form = new CancelMeetingForm(FormTypes.New, Meetinginfo);
                        EntityBrowser browser = new EntityBrowser(form);
                        browser.MinHeight = 570;
                        browser.MinWidth = 580;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result1) => { }, true);


                    }
                    catch (Exception ex)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),ex.ToString());
                        return;
                        //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                    }
                    
                };
                com.SelectionBox(Utility.GetResourceStr("CANCELMEETINGCONFIRM"), Utility.GetResourceStr("CANCELMEETINGALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);

            }
            else
            {
                //MessageBox.Show("请您选择需要删除的数据！");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "CANCELMEETINGCONFIRM"));
            }
        }

        #endregion

        #region 按钮事件
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //T_OA_MEETINGINFO info = new T_OA_MEETINGINFO();
            V_MeetingInfo MeetingInfoT = new V_MeetingInfo();
            MeetingForm form = new MeetingForm(FormTypes.New, MeetingInfoT);
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinHeight = 580;            
            browser.MinWidth = 800;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { },true);

            
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Meetinginfo != null)
            {
                if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Meetinginfo.meetinginfo, "T_OA_MEETINGINFO", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
                {
                    if (Meetinginfo.meetinginfo.CHECKSTATE == "0" || meetinginfo.meetinginfo.CHECKSTATE == "3")
                    {
                        MeetingForm form = new MeetingForm(FormTypes.Edit, Meetinginfo);
                        EntityBrowser browser = new EntityBrowser(form);
                        browser.FormType = FormTypes.Edit;
                        browser.MinHeight = 580;
                        browser.MinWidth = 800;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                    }
                    else
                    {
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("MEETINGINFONOTEDIT"));
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("MEETINGINFONOTEDIT"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            
        }
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            //client.DeleteOrganAsync(organDelID);
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (DaGr.SelectedItems.Count > 0)
            {

                string Result = "";
                DelInfosList = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                    {
                        string SenddocID = "";
                        T_OA_MEETINGINFO tmpMeeting = (DaGr.SelectedItems[i] as V_MeetingInfo).meetinginfo;
                        SenddocID = tmpMeeting.MEETINGINFOID;
                        if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmpMeeting, "T_OA_MEETINGINFO", OperationType.Delete, Common.CurrentLoginUserInfo.EmployeeID))
                        {
                            if ((DaGr.SelectedItems[i] as V_MeetingInfo).meetinginfo.CHECKSTATE == "0" || (DaGr.SelectedItems[i] as V_MeetingInfo).meetinginfo.CHECKSTATE == "3")
                            {
                                if (!(DelInfosList.IndexOf(SenddocID) > -1))
                                {
                                    DelInfosList.Add(SenddocID);
                                }
                            }
                        }
                        else
                        {
                           string  StrTip = "您不能删除您选中的第" + (i + 1).ToString() + "条，标题为" + tmpMeeting.MEETINGTITLE + "的会议信息";
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), StrTip, Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                    }
                    if (DelInfosList.Count > 0)
                    {
                        MeetingClient.MeetingInfoBatchDelAsync(DelInfosList);
                    }
                    else
                    {
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTITEMSNOTDELETE"));
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTITEMSNOTDELETE"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }





        }
        


        //模板列取消选中
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (!chkbox.IsChecked.Value)
            //{
            //    organObj = chkbox.DataContext as V_OrganRegister;
            //    organDelID.Remove(organObj.organ.ORGANIZATIONID);
            //    organObj = null;
            //}
        }

        //模板列选中
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (chkbox.IsChecked.Value)
            //{
            //    organObj = chkbox.DataContext as V_OrganRegister;
            //    organDelID.Add(organObj.organ.ORGANIZATIONID);
            //}
        }

        

        //刷新
        private void browser_ReloadDataEvent()
        {
            Meetinginfo = null;
            LoadMeetingInfos();
        }

        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {

                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_MEETINGINFO");
                checkState = dict.DICTIONARYVALUE.ToString();
                SetButtonVisible();
                LoadMeetingInfos();
            }  
            
        }

        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //草稿                    
                    ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核                    
                    //NewDetailButton();
                    ToolBar.stpOtherAction.Children.Clear();
                    break;
                case "1":  //审批中

                    ToolBar.btnAudit.Visibility = Visibility.Collapsed;       //审核
                    //提交审核
                    ToolBar.stpOtherAction.Children.Clear();

                    break;
                case "2":  //审批通过

                    NewButton();
                    break;
                case "3":  //审批未通过

                    ToolBar.stpOtherAction.Children.Clear();
                    break;
                case "4":  //待审核

                    ToolBar.stpOtherAction.Children.Clear();
                    break;
                case "5":
                    ToolBar.stpOtherAction.Children.Clear();
                    break;

            }
                        
        }

        //private void orgFrm_ReloadDataEvent()
        //{
        //    LoadData();
        //}

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            //GridHelper.SetUnCheckAll(dgOrgan);
            //LoadData();
        }

        #endregion

        #region 选择部门

        private void PostsObject_FindClick(object sender, EventArgs e)
        {
            //OrganizationLookupForm lookup = new OrganizationLookupForm();
            //lookup.SelectedObjType = OrgTreeItemTypes.Department;


            //lookup.SelectedClick += (obj, ev) =>
            //{
            //    PostsObject.DataContext = lookup.SelectedObj;
            //    if (lookup.SelectedObj is T_HR_DEPARTMENT)
            //    {
            //        T_HR_DEPARTMENT tmp = lookup.SelectedObj as T_HR_DEPARTMENT;
                    
            //        StrDepartmentID = tmp.DEPARTMENTID;
            //        StrDepartmentName = tmp.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

            //        PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
            //    }

            //};
            
            //lookup.Show();
        }


        
        #endregion

        #region 流程完成动作
        

        private void Cancel()
        {
            //LoadMeetingInfos(tmpStrUserID, checkState);  
            LoadMeetingInfos();  
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            //LoadMeetingInfos(tmpStrUserID, checkState);
            LoadMeetingInfos();
        }

        #endregion

        #region 添加报告Button
        public void NewButton()
        {
            
            ToolBar.stpOtherAction.Children.Clear();
            ImageButton ChangeMeetingBtn = new ImageButton();
            ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
            ChangeMeetingBtn.TextBlock.Text = "变更时间";
            ChangeMeetingBtn.Image.Width = 16.0;
            ChangeMeetingBtn.Image.Height = 22.0;
            ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            ChangeMeetingBtn.Click += new RoutedEventHandler(ChangeMeetingBtn_Click);
            ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);

            ImageButton MeetingCancelBtn = new ImageButton();
            MeetingCancelBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
            MeetingCancelBtn.TextBlock.Text = "取消会议";
            MeetingCancelBtn.Image.Width = 16.0;
            MeetingCancelBtn.Image.Height = 22.0;
            MeetingCancelBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            MeetingCancelBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            MeetingCancelBtn.Click += new RoutedEventHandler(MeetingCancelBtn_Click);


            ToolBar.stpOtherAction.Children.Add(MeetingCancelBtn);
        }

        
        #endregion

        #region IClient 成员

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

