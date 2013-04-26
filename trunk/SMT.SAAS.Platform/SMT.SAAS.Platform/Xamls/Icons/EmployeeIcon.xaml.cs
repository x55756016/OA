//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 登录界面中员工信息icon
// 完成日期：2011-05-16 
// 版    本：V1.0 
// 作    者：王玲 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Media;


namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class EmployeeIcon : UserControl
    {
        private static bool isShow = true;
        private bool isClose = false;
        public DispatcherTimer _CloseTimer;
        private Saas.Tools.PersonnelWS.PersonnelServiceClient pClient;

        public EmployeeIcon()
        {
            InitializeComponent();
            this.Loaded += new System.Windows.RoutedEventHandler(EmployeeIcon_Loaded);
        }

        void EmployeeIcon_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _CloseTimer = new DispatcherTimer();
            _CloseTimer.Interval = new TimeSpan(0, 0, 2);
            _CloseTimer.Tick += new System.EventHandler(_CloseTimer_Tick);
            IconPath.MouseEnter += new MouseEventHandler(IconPath_MouseEnter);
            IconPath.MouseLeave += new System.Windows.Input.MouseEventHandler(IconPath_MouseLeave);
            ShowUserInfo.Completed += new EventHandler(ShowUserInfo_Completed);
            HideUserInfo.Completed += new EventHandler(HideUserInfo_Completed);
            pClient = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
            ShowUserinfo();
        }

        void IconPath_MouseEnter(object sender, MouseEventArgs e)
        {
            isClose = false;
            if (isShow)
            {
                isShow = false;
                this.ShowUserInfo.Begin();
            }
        }

        void HideUserInfo_Completed(object sender, EventArgs e)
        {
            isShow = true;
        }

        void ShowUserInfo_Completed(object sender, EventArgs e)
        {
            isShow = false;
        }

        void _CloseTimer_Tick(object sender, System.EventArgs e)
        {
            if (isClose)
            {
                isShow = false;
                this.HideUserInfo.Begin();
                _CloseTimer.Stop();
            }
        }

        void IconPath_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            isClose = true;
            if (_CloseTimer!=null)
                _CloseTimer.Start();
        }

        private void ShowUsers_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            isClose = true;
            if (_CloseTimer!=null)
                _CloseTimer.Start();
        }

        private void ShowUsers_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            isClose = false;
        }


        #region 私有方法
        /// <summary>
        /// 显示登录用户信息
        /// </summary>
        public void ShowUserinfo()
        {
            IconName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            txbUserName.Text = string.Format("姓 名 : {0}", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName);
            txbCpyName.Text = string.Format("公 司 : {0}", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName);
            txDdeptName.Text = string.Format("部 门 : {0}", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName);
            txbPostName.Text = string.Format("岗 位 : {0}", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName);

            ToolTipService.SetToolTip(txbUserName, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName);
            ToolTipService.SetToolTip(txbCpyName, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName);
            ToolTipService.SetToolTip(txDdeptName, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName);
            ToolTipService.SetToolTip(txbPostName, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName);

            BindImageField();
            LoadUserLevel();
        }

        private void BindImageField()
        {
            pClient.GetEmployeePhotoByIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            pClient.GetEmployeePhotoByIDCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.Photo = e.Result;
                        byte[] ImageField = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.Photo;
                        if (ImageField != null && ImageField.Length > 0)
                        {
                            try
                            {
                                BitmapImage img = new BitmapImage();

                                System.IO.MemoryStream stream = new System.IO.MemoryStream(ImageField);

                                img.SetSource(stream);
                                stream.Close();

                                headImage.Source = img;                                
                                IconPath.Source = img;
                                //Image imgControl = new Image();
                                //imgControl.Width = 28;
                                //imgControl.Height = 42;
                                //imgControl.Cursor = Cursors.Hand;
                                //imgControl.Stretch = Stretch.Fill;
                                //imgControl.Source = img;
                                //var imgFather = VisualTreeHelper.GetParent(imgControl) as Grid;
                                //if (imgFather != null && IconPath != null)
                                //{
                                //    imgFather.Children.Clear();
                                //    imgFather.Children.Add(imgControl);
                                //}

                            }
                            catch (Exception ex)
                            {
                                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(" 绑定员工头像出错：" + ex.ToString());
                                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
                            }
                        }
                        else
                        {
                            headImage.Source = new BitmapImage(new Uri("/Images/User_1.png", UriKind.Relative));
                        }
                    }
                }
            };
          
        }

        private void LoadUserLevel()
        {
            pClient.GetEmployeeWorkAgeByIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            pClient.GetEmployeeWorkAgeByIDCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.WorkingAge = e.Result;
                        userLevel.Children.Clear();
                        string tooltipstr = string.Empty;
                        int workingAge = 0;//SMT.SAAS.Main.CurrentContext.Common.loginUserInfo.WorkingAge;
                        if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.WorkingAge > 12)
                        {
                            workingAge = Int32.Parse(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.WorkingAge.ToString());
                        }

                        int sun = 60;
                        int moon = 24;//2年
                        int star = 12;//1年

                        var sunCount = workingAge / sun;
                        var moonCount = (workingAge - sunCount * sun) / moon;
                        var starCount = (workingAge - (sunCount * sun + moonCount * moon)) / star;

                        if (workingAge <= 12)
                            starCount = 1;
                        else if (workingAge - (sunCount * 60 + moonCount * 24 + star * 12) < 12)
                            starCount += 1;


                        #region MyRegion
                        for (int i = 0; i < sunCount; i++)
                        {
                            Image img = new Image() { Height = 25, Width = 15, Margin = new Thickness(0, 0, 2, 0) };
                            img.Source = new BitmapImage(new Uri("/Images/icons/level_sun.png", UriKind.Relative));
                            userLevel.Children.Add(img);
                        }
                        for (int i = 0; i < moonCount; i++)
                        {
                            Image img = new Image() { Height = 25, Width = 15, Margin = new Thickness(0, 0, 2, 0) };
                            img.Source = new BitmapImage(new Uri("/Images/icons/level_moon.png", UriKind.Relative));
                            userLevel.Children.Add(img);
                        }
                        for (int i = 0; i < starCount; i++)
                        {
                            Image img = new Image() { Height = 25, Width = 15, Margin = new Thickness(0, 0, 2, 0) };
                            img.Source = new BitmapImage(new Uri("/Images/icons/level_star.png", UriKind.Relative));
                            userLevel.Children.Add(img);
                        }

                        var Y = workingAge / 12;
                        var M = workingAge - Y * 12;
                        #endregion
                        tooltipstr = "感谢您的辛苦工作！\n您已入职" + Y + "年零" + M + "个月.";

                        if (workingAge == 0)
                            tooltipstr = "感谢您的辛苦工作！\n您已入职" + Y + "年零" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.WorkingAge + "个月.";

                        userLevel.SetValue(ToolTipService.ToolTipProperty, tooltipstr);
                    }
                }
                else
                {

                }
            };
        }
        #endregion

        private void ShowUsers_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowUserinfo();
        }
    }
}
