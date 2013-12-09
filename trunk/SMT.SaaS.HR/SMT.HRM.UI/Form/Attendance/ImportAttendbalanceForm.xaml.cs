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

using SMT.Saas.Tools.AttendanceWS;      //考勤接口
using SMT.Saas.Tools.OrganizationWS;    //公司组织接口
using System.ServiceModel;
using System.IO;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using System.Net.Browser;
using System.Threading;
using System.Windows.Browser;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;

namespace SMT.HRM.UI.Form.Attendance
{
    public delegate void SaveFileHandler(int bytesRead, byte[] buffer);
    public delegate void UploadFileHandler(int bytesRead);

    public partial class ImportAttendbalanceForm : BaseForm, IEntityEditor
    {
        public OpenFileDialog OpenFileDialog = null;
        AttendanceServiceClient clientAtt;
        byte[] byExport;

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        ObservableCollection<T_HR_ATTENDMONTHLYBALANCE> ListBalance = new ObservableCollection<T_HR_ATTENDMONTHLYBALANCE>();

        public ImportAttendbalanceForm()
        {
            InitializeComponent();
            RegisterEvents();
        }


        private void RegisterEvents()
        {
            clientAtt = new AttendanceServiceClient();
            clientAtt.ImportAttendMonthlyBalanceFromCSVCompleted += new EventHandler<ImportAttendMonthlyBalanceFromCSVCompletedEventArgs>(clientAtt_ImportAttendMonthlyBalanceFromCSVCompleted);
            clientAtt.ImportAttendMonthlyBalanceForShowCompleted += new EventHandler<ImportAttendMonthlyBalanceForShowCompletedEventArgs>(clientAtt_ImportAttendMonthlyBalanceForShowCompleted);
            this.Loaded += new RoutedEventHandler(ImportAttendbalanceForm_Loaded);
        }

        void clientAtt_ImportAttendMonthlyBalanceForShowCompleted(object sender, ImportAttendMonthlyBalanceForShowCompletedEventArgs e)
        {
            if (e.Error == null)
            {

                if (e.Result != null)
                {
                    ListBalance = e.Result;
                    this.LoadData(ListBalance.ToList());
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "没有导入的数据",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "导入月度考勤结算记录失败",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        private void LoadData(List<T_HR_ATTENDMONTHLYBALANCE> ListPensions)
        {
            ListPensions.RemoveAt(0);
            DtGrid.ItemsSource = ListPensions;
        }

        void ImportAttendbalanceForm_Loaded(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        /// <summary>
        /// 表单重置
        /// </summary>
        private void ResetForm()
        {
            DateTime dtCur = DateTime.Now;
            nuYear.Value = dtCur.Year;
            nuMonth.Value = dtCur.Month;
            tbFileName.Text = string.Empty;
            lkBalanceUnit.DataContext = null;

            if (cbxkBalanceUnitType.Items.Count > 0)
            {
                cbxkBalanceUnitType.SelectedIndex = 0;
            }
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("IMPORTATTENDBALANCEFORM");
        }

        public string GetStatus()
        {
            if (string.IsNullOrEmpty(txtUploadResMsg.Text))
                return "上传中";
            else
                return "上传完毕";
        }


        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            return items;
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    ImportBalance();
                    break;
                case "2":
                    DownloadFile();
                    break;
                case "1":
                    //  Cancel();
                    break;
            }
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item2 = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "2",
                Title = "下载模版",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/area/18_workrules.png"
            };
            items.Add(item2);

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("IMPORT"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/area/18_import.png"
            };

            items.Add(item);
            return items;
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

        /// <summary>
        /// 返回考勤导入结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ImportAttendMonthlyBalanceFromCSVCompleted(object sender, ImportAttendMonthlyBalanceFromCSVCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (e.Error == null)
            {
                if (e.strMsg != "{IMPORTSUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg.Replace("{", "").Replace("}", "")));
                    return;
                }

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("IMPORTSUCCESSED", "ATTENDMONTHLYBALANCE"));
                ResetForm();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        private void lkBalanceUnit_FindClick(object sender, EventArgs e)
        {
            if (cbxkBalanceUnitType.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkBalanceUnitType.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
            {
                return;
            }

            OrganizationLookupForm lookup = new OrganizationLookupForm();
            if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Company;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Department;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Post;
            }

            lookup.SelectedClick += (obj, ev) =>
            {
                lkBalanceUnit.DataContext = lookup.SelectedObj;

                if (lookup.SelectedObj is T_HR_COMPANY)
                {
                    lkBalanceUnit.DisplayMemberPath = "CNAME";
                }
                else if (lookup.SelectedObj is T_HR_DEPARTMENT)
                {
                    lkBalanceUnit.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                }
                else if (lookup.SelectedObj is T_HR_POST)
                {
                    lkBalanceUnit.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 选择上传文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog = new OpenFileDialog();

            OpenFileDialog.Multiselect = false;
            OpenFileDialog.Filter = "csv Files (*.csv)|*.csv;";

            if (OpenFileDialog.ShowDialog() != true)
            {
                return;
            }

            tbFileName.Text = OpenFileDialog.File.Name;
            txtUploadResMsg.Text = string.Empty;
            ShowDTGrid();
        }

        private void ShowDTGrid()
        {
            Stream Stream = (System.IO.Stream)OpenFileDialog.File.OpenRead();

            byte[] Buffer = new byte[Stream.Length];
            Stream.Read(Buffer, 0, (int)Stream.Length);

            Stream.Dispose();
            Stream.Close();
            SMT.Saas.Tools.AttendanceWS.UploadFileModel UploadFile = new Saas.Tools.AttendanceWS.UploadFileModel();
            UploadFile.FileName = OpenFileDialog.File.Name;
            UploadFile.File = Buffer;
            clientAtt.ImportAttendMonthlyBalanceForShowAsync(UploadFile);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            ImportBalance();
        }

        private void ImportBalance()
        {
            string strMsg = string.Empty;
            try
            {
                string strUnitType = string.Empty, strUnitObjectId = string.Empty;
                int dYear = 0, dMonth = 0;
                DateTime dtImportDate = new DateTime(), dtCurDate = new DateTime();


                int.TryParse(nuYear.Value.ToString(), out dYear);
                int.TryParse(nuMonth.Value.ToString(), out dMonth);
                DateTime.TryParse(dYear.ToString() + "-" + dMonth.ToString() + "-1", out dtImportDate);
                DateTime.TryParse(DateTime.Now.ToString("yyyy-MM") + "-1", out dtCurDate);

                if (dtImportDate > dtCurDate)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("IMPORTBALANCEDATE"), Utility.GetResourceStr("DATECOMPARECURENTTIME", "BALANCEYEAR"));
                    return;
                }

                if (cbxkBalanceUnitType.SelectedItem == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CLOCKINRDUNITTYPE"), Utility.GetResourceStr("REQUIRED", "CLOCKINRDUNITTYPE"));
                    return;
                }

                if (lkBalanceUnit.DataContext == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CLOCKINRDUNIT"), Utility.GetResourceStr("REQUIRED", "CLOCKINRDUNIT"));
                    return;
                }

                T_SYS_DICTIONARY entUnitType = cbxkBalanceUnitType.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entUnitType.DICTIONARYID) || string.IsNullOrEmpty(entUnitType.DICTIONCATEGORY) || string.IsNullOrEmpty(entUnitType.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CLOCKINRDUNITTYPE"), Utility.GetResourceStr("REQUIRED", "CLOCKINRDUNITTYPE"));
                    return;
                }

                strUnitType = entUnitType.DICTIONARYVALUE.ToString();
                if (strUnitType == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
                {
                    T_HR_COMPANY entCompany = lkBalanceUnit.DataContext as T_HR_COMPANY;
                    strUnitObjectId = entCompany.COMPANYID;
                }
                else if (strUnitType == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
                {
                    T_HR_DEPARTMENT entDepartment = lkBalanceUnit.DataContext as T_HR_DEPARTMENT;
                    strUnitObjectId = entDepartment.DEPARTMENTID;
                }
                else if (strUnitType == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
                {
                    T_HR_POST entPost = lkBalanceUnit.DataContext as T_HR_POST;
                    strUnitObjectId = entPost.POSTID;
                }


                if (OpenFileDialog == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIREDIMPORTFILE"));
                    return;
                }

                if (OpenFileDialog.File == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIREDIMPORTFILE"));
                    return;
                }

                RefreshUI(RefreshedTypes.ProgressBar);

                Stream Stream = (System.IO.Stream)OpenFileDialog.File.OpenRead();

                byte[] Buffer = new byte[Stream.Length];
                Stream.Read(Buffer, 0, (int)Stream.Length);

                Stream.Dispose();
                Stream.Close();

                SMT.Saas.Tools.AttendanceWS.UploadFileModel UploadFile = new Saas.Tools.AttendanceWS.UploadFileModel();
                UploadFile.FileName = OpenFileDialog.File.Name;
                UploadFile.File = Buffer;

                clientAtt.ImportAttendMonthlyBalanceFromCSVAsync(UploadFile, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strUnitType, strUnitObjectId, dYear, dMonth, strMsg);

                strMsg = string.Empty;
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        private void DownloadFile()
        {
            if (HtmlPage.IsPopupWindowAllowed)
            {
                //一定要有三个参数
                HtmlPage.PopupWindow(new System.Uri("http://newportal.smt-online.net/download/月度考勤结算批量导入模板.csv"), "_blank", null);
            }
            else
            {
                HtmlPage.Window.Navigate(new System.Uri("http://newportal.smt-online.net/download/月度考勤结算批量导入模板.csv"), "_blank");
            }
        }

    }
}
