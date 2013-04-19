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
using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI.Helper;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EntityPageForm : System.Windows.Controls.Window
    {
        Control ctr;

        EmployeeEntryAddForm entryAddForm;
        EmployeeUserForm userForm;
        EmployeeInfoForm infoForm;

        public EntityPageForm()
        {
            InitializeComponent();
            TitleContent = Utility.GetResourceStr("EMPLOYEEENTRY");
            userForm = new EmployeeUserForm();
            ctr = userForm as Control;
            ctr.Width = 400;
            ctr.Height = 200;
            ctr.VerticalAlignment = VerticalAlignment.Stretch;
            ctr.HorizontalAlignment = HorizontalAlignment.Stretch;

            pnlEntity.Children.Add(userForm);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateToolBar();
            GenerateEntityInfoCtr("0");
        }

        private void GenerateToolBar()
        {
            //
        }

        public delegate void refreshGridView();
        public event refreshGridView ReloadDataEvent;

        private void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        private void GenerateEntityInfoCtr(string sType)
        {
            FootBar.Children.Clear();
            ImageButton btn;
            string img;
            switch (sType)
            {
                case "0":
                    btn = new ImageButton();
                    btn.TextBlock.Text = UIHelper.GetResourceStr("NEXT");
                    //btn.TextBlock.Text = "下一步";
                    img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/mnu_actions.png";

                    btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                    btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                    btn.Click += new RoutedEventHandler(btnNext_Click);
                    FootBar.Children.Add(btn);
                    break;
                case "1":
                    btn = new ImageButton();
                    //btn.TextBlock.Text = UIHelper.GetResourceStr("MANUALAUDIT");
                    btn.TextBlock.Text = "上一步";
                    img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/mnu_actions.png";
                    btn.Visibility = Visibility.Collapsed;
                    btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                    btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                    btn.Click += new RoutedEventHandler(btnPre_Click);
                    FootBar.Children.Add(btn);

                    btn = new ImageButton();
                    btn.TextBlock.Text = UIHelper.GetResourceStr("SAVE");
                    //btn.TextBlock.Text = "下一步";
                    img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/mnu_actions.png";

                    btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                    btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                    btn.Click += new RoutedEventHandler(btnSave_Click);
                    FootBar.Children.Add(btn);

                    btn = new ImageButton();
                    btn.TextBlock.Text = UIHelper.GetResourceStr("NEXT");
                    //btn.TextBlock.Text = "下一步";
                    img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/mnu_actions.png";

                    btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                    btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                    btn.Click += new RoutedEventHandler(btnNext_Click);
                    FootBar.Children.Add(btn);
                    break;
                default:
                    btn = new ImageButton();
                    //btn.TextBlock.Text = UIHelper.GetResourceStr("MANUALAUDIT");
                    btn.TextBlock.Text = "上一步";
                    img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/mnu_actions.png";
                    // btn.Visibility = Visibility.Collapsed;
                    btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                    btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                    btn.Click += new RoutedEventHandler(btnPre_Click);
                    FootBar.Children.Add(btn);

                    btn = new ImageButton();
                    btn.TextBlock.Text = UIHelper.GetResourceStr("FINISH");
                    //btn.TextBlock.Text = "完成";
                    img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/mnu_actions.png";

                    btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                    btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                    btn.Click += new RoutedEventHandler(btnFinshed_Click);
                    FootBar.Children.Add(btn);
                    break;

            }

        }

        void btnPre_Click(object sender, RoutedEventArgs e)
        {
            if (ctr is EmployeeInfoForm)
            {
                ctr.Visibility = Visibility.Collapsed;
                if (userForm != null)
                    userForm.Visibility = Visibility.Visible;

                ctr = userForm;
                ctr.MinWidth = 400;
                ctr.MinHeight = 200;
                this.ParentWindow.Height = 290;
                this.ParentWindow.Width = 440;
                ctr.VerticalAlignment = VerticalAlignment.Stretch;
                ctr.HorizontalAlignment = HorizontalAlignment.Stretch;
                GenerateEntityInfoCtr("0");
            }
            else
            {
                ctr.Visibility = Visibility.Collapsed;
                if (infoForm != null)
                    infoForm.Visibility = Visibility.Visible;

                ctr = infoForm;
                //ctr.MinHeight = 500;
                ctr.Height = 500;
                ctr.MinWidth = 680;
                this.ParentWindow.Height = 605;
                this.ParentWindow.Width = 700;
                ctr.VerticalAlignment = VerticalAlignment.Stretch;
                ctr.HorizontalAlignment = HorizontalAlignment.Stretch;
                GenerateEntityInfoCtr("1");
            }
        }

        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (ctr is EmployeeUserForm)
            {
                EmployeeUserForm temp = ctr as EmployeeUserForm;
                temp.OnUIRefreshed += new EmployeeUserForm.refreshGridView(temp_OnUIRefreshed);
                temp.save();
            }
            else
            {
                EmployeeInfoForm tempForm = ctr as EmployeeInfoForm;
                tempForm.OnUIRefreshed += new EmployeeInfoForm.refreshGridView(tempForm_OnUIRefreshed);
                tempForm.Save("");
            }
        }
        void btnSave_Click(object sender, RoutedEventArgs e)
        {
            EmployeeInfoForm tempForm = ctr as EmployeeInfoForm;
            tempForm.Save("Save");

        }
        void tempForm_OnUIRefreshed()
        {
            if (ctr is EmployeeInfoForm)
            {
                EmployeeInfoForm temp = ctr as EmployeeInfoForm;
                //pnlEntity.Children.Clear();
                temp.Visibility = Visibility.Collapsed;

                if (entryAddForm == null)
                {
                    int employeeType = 0;//0 表示新增 1表示使用原有的员工信息
                    if (userForm.leaveMessage.Count > 0)
                    {
                        employeeType = 1;
                    }
                    entryAddForm = new EmployeeEntryAddForm(temp.Employee, employeeType);
                    entryAddForm.IsEntryBefore = temp.IsEntryBefore;
                    //EmployeeEntryAddForm form = new EmployeeEntryAddForm(FormTypes.New,"");
                    ctr = entryAddForm as Control;
                    ctr.HorizontalAlignment = HorizontalAlignment.Stretch;
                    ctr.VerticalAlignment = VerticalAlignment.Top;
                    //this.Width = 700;
                    //this.Height = 300;
                    pnlEntity.Children.Add(entryAddForm);

                }
                else
                {
                    entryAddForm.Visibility = Visibility.Visible;
                    ctr = entryAddForm as Control;
                    //entryAddForm.SysUser.USERNAME = temp.Employee.EMPLOYEEENAME;
                    entryAddForm.txtUser.Text = temp.Employee.EMPLOYEEENAME;
                    if (temp.Employee.IDNUMBER.Length > 6)
                    {
                        //entryAddForm.SysUser.PASSWORD = temp.Employee.IDNUMBER.Substring(temp.Employee.IDNUMBER.Length - 6);
                        entryAddForm.txtPwd.Password = "smt" + temp.Employee.IDNUMBER.Substring(temp.Employee.IDNUMBER.Length - 6);
                    }
                    else
                    {
                        entryAddForm.txtPwd.Password = temp.Employee.IDNUMBER;
                    }

                }
                entryAddForm.ComputerNo = temp.txtComputerNO.Text.Trim();
                entryAddForm.PensionCardID = temp.txtCardID.Text.Trim();
                entryAddForm.eminfo = temp;
                ctr.MinHeight = 300;
                ctr.MinWidth = 680;
                this.ParentWindow.Height = 340;
                this.ParentWindow.Width = 700;
                GenerateEntityInfoCtr("2");
            }
        }

        void temp_OnUIRefreshed()
        {
            if (ctr is EmployeeUserForm)
            {
                EmployeeUserForm temp = ctr as EmployeeUserForm;
                //pnlEntity.Children.Clear();
                temp.Visibility = Visibility.Collapsed;

                if (infoForm == null)
                {
                    //if (string.IsNullOrEmpty(temp.leaveMessage[0]))
                    if (temp.leaveMessage.Count <= 0)
                    {
                        infoForm = new EmployeeInfoForm(temp.sNumberID.Text.Trim().ToUpper(), temp.sName.Text.Trim());
                    }
                    else
                    {
                        infoForm = new EmployeeInfoForm(temp.leaveMessage[0]);
                        if (!string.IsNullOrEmpty(temp.leaveMessage[1]))
                        {
                            infoForm.IsEntryBefore = true;
                        }
                        else
                        {
                            infoForm.IsEntryBefore = false;
                        }
                    }

                    ctr = infoForm as Control;
                    ctr.HorizontalAlignment = HorizontalAlignment.Stretch;
                    ctr.VerticalAlignment = VerticalAlignment.Top;
                    pnlEntity.Children.Add(infoForm);
                }
                else
                {
                    if (temp.leaveMessage.Count > 0)
                    {
                        infoForm = new EmployeeInfoForm(temp.leaveMessage[0]);
                        if (!string.IsNullOrEmpty(temp.leaveMessage[1]))
                        {
                            infoForm.IsEntryBefore = true;
                        }
                        else
                        {
                            infoForm.IsEntryBefore = false;
                        }
                        ctr = infoForm as Control;
                        ctr.HorizontalAlignment = HorizontalAlignment.Stretch;
                        ctr.VerticalAlignment = VerticalAlignment.Top;
                        pnlEntity.Children.Add(infoForm);
                    }
                    else
                    {
                        infoForm.Employee.EMPLOYEECNAME = temp.sName.Text.Trim();
                        infoForm.Employee.IDNUMBER = temp.sNumberID.Text.Trim().ToUpper();
                        infoForm.Visibility = Visibility.Visible;

                        ctr = infoForm as Control;
                        ctr.HorizontalAlignment = HorizontalAlignment.Stretch;
                        ctr.VerticalAlignment = VerticalAlignment.Top;
                    }
                    //infoForm.Visibility = Visibility.Visible;

                    //ctr = infoForm as Control;
                    //ctr.HorizontalAlignment = HorizontalAlignment.Stretch;
                    //ctr.VerticalAlignment = VerticalAlignment.Top;
                }
                //ctr.MinHeight = 500;
                ctr.Height = 500;
                ctr.MinWidth = 450;
                this.ParentWindow.Width = 700;
                this.ParentWindow.Height = 605;
                GenerateEntityInfoCtr("1");
            }
        }

        void btnFinshed_Click(object sender, RoutedEventArgs e)
        {
            if (ctr is EmployeeEntryAddForm)
            {
                EmployeeEntryAddForm temp = ctr as EmployeeEntryAddForm;
                temp.OnUIRefreshed += new EmployeeEntryAddForm.refreshGridView(tempFinshed_OnUIRefreshed);
                temp.txtPwd.IsEnabled = false;
                if(temp.canSave) temp.Save();
            }
        }

        void tempFinshed_OnUIRefreshed()
        {
            ReloadData();
            this.Close();
        }
    }
}

