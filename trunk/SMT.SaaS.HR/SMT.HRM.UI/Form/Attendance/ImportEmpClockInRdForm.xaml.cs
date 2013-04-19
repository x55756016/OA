using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using SMT.Saas.Tools.AttendanceWS;      //考勤接口
using SMT.Saas.Tools.OrganizationWS;    //公司组织接口
using System.ServiceModel;
using System.IO;
using SMT.SaaS.FrameworkUI;
using System.Collections.Generic;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.HRM.UI
{
    public partial class ImportEmpClockInRdForm : BaseForm, IEntityEditor
    {
        public OpenFileDialog OpenFileDialog = null;
        AttendanceServiceClient clientAtt;
        byte[] byExport;
        public DateTime dtStart { get; set; }
        public DateTime dtEnd { get; set; }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        public ImportEmpClockInRdForm(ref DateTime dtPunchDateFrom, ref DateTime dtPunchDateTo)
        {
            // 为初始化变量所必需
            InitializeComponent();
            dtStart = dtPunchDateFrom;
            dtEnd = dtPunchDateTo;
            clientAtt = new AttendanceServiceClient();

            clientAtt.ImportClockInRdListFromFileCompleted += new EventHandler<ImportClockInRdListFromFileCompletedEventArgs>(clientAtt_ImportClockInRdListFromFileCompleted);
            clientAtt.ImportClockInRdListFromLoginDataCompleted += new EventHandler<ImportClockInRdListFromLoginDataCompletedEventArgs>(clientAtt_ImportClockInRdListFromLoginDataCompleted);
            clientAtt.ImportClockInRdListFromFileAndLoginDataCompleted += new EventHandler<ImportClockInRdListFromFileAndLoginDataCompletedEventArgs>(clientAtt_ImportClockInRdListFromFileAndLoginDataCompleted);

            this.Loaded += new RoutedEventHandler(ImportEmpClockInRdForm_Loaded);
        }

        void ImportEmpClockInRdForm_Loaded(object sender, RoutedEventArgs e)
        {
            if (cbxkClockInRdUnitType.Items.Count > 0)
            {
                cbxkClockInRdUnitType.SelectedIndex = 0;
            }
        }

        #region IEntityEditor

        public string GetTitle()
        {
            return Utility.GetResourceStr("CLOCKINRD");
        }
        public string GetStatus()
        {
            if (string.IsNullOrEmpty(txtUploadResMsg.Text))
                return "上传中";
            else
                return "上传完毕";
        }
        public void DoAction(string actionType)
        {

        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
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

        /// <summary>
        /// 上传文件并加载处理结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ImportClockInRdListFromFileCompleted(object sender, ImportClockInRdListFromFileCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.strMsg.IndexOf("ERROR") > -1)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg.Replace("{", "").Replace("}", "")));
                    return;
                }

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("IMPORTSUCCESSED", "EMPLOYEECLOCKINRECORD"));
                tbFileName.Text = string.Empty;
                txtUploadResMsg.Text = "1";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void clientAtt_ImportClockInRdListFromLoginDataCompleted(object sender, ImportClockInRdListFromLoginDataCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.strMsg.IndexOf("ERROR") > -1)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg.Replace("{", "").Replace("}", "")));
                    return;
                }

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("IMPORTSUCCESSED", "EMPLOYEECLOCKINRECORD"));
                tbFileName.Text = string.Empty;
                txtUploadResMsg.Text = "1";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void clientAtt_ImportClockInRdListFromFileAndLoginDataCompleted(object sender, ImportClockInRdListFromFileAndLoginDataCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.strMsg.IndexOf("ERROR") > -1)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg.Replace("{", "").Replace("}", "")));
                    return;
                }

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("IMPORTSUCCESSED", "EMPLOYEECLOCKINRECORD"));
                tbFileName.Text = string.Empty;
                txtUploadResMsg.Text = "1";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkClockInRdUnit_FindClick(object sender, EventArgs e)
        {
            if (cbxkClockInRdUnitType.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkClockInRdUnitType.SelectedItem as T_SYS_DICTIONARY;
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
                lkClockInRdUnit.DataContext = lookup.SelectedObj;

                if (lookup.SelectedObj is T_HR_COMPANY)
                {
                    lkClockInRdUnit.DisplayMemberPath = "CNAME";
                }
                else if (lookup.SelectedObj is T_HR_DEPARTMENT)
                {
                    lkClockInRdUnit.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                }
                else if (lookup.SelectedObj is T_HR_POST)
                {
                    lkClockInRdUnit.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 导入方式选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxkUploadFileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxkUploadFileType.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkUploadFileType.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
            {
                return;
            }

            if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(ClockInRdUploadFileType.File) + 1).ToString())
            {
                tbFileNameTitle.Visibility = Visibility.Visible;
                tbFileName.Visibility = Visibility.Visible;
                BrowseFiles.Visibility = Visibility.Visible;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(ClockInRdUploadFileType.Login) + 1).ToString())
            {
                tbFileNameTitle.Visibility = Visibility.Collapsed;
                tbFileName.Visibility = Visibility.Collapsed;
                BrowseFiles.Visibility = Visibility.Collapsed;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(ClockInRdUploadFileType.FileAndLogin) + 1).ToString())
            {
                tbFileNameTitle.Visibility = Visibility.Visible;
                tbFileName.Visibility = Visibility.Visible;
                BrowseFiles.Visibility = Visibility.Visible;
            }
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
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            ImportClockInRd();
        }

        /// <summary>
        ///  读取打卡的Excel文件，并导入数据库，返回导入后的结果
        /// </summary>
        private void ImportClockInRd()
        {
            string strMsg = string.Empty;
            try
            {
                string strUnitType = string.Empty, strUnitObjectId = string.Empty;
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                bool flag = false;

                flag = DateTime.TryParse(dpStartDate.Text, out dtStart);
                if (!flag)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("IMPORTCLOCKINRDSTARTDATE"), Utility.GetResourceStr("REQUIRED", "IMPORTCLOCKINRDSTARTDATE"));
                    return;
                }

                flag = DateTime.TryParse(dpEndDate.Text, out dtEnd);
                if (!flag)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("IMPORTCLOCKINRDENDDATE"), Utility.GetResourceStr("REQUIRED", "IMPORTCLOCKINRDENDDATE"));
                    return;
                }

                if (dtStart > dtEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("IMPORTCLOCKINRDENDDATE"), Utility.GetResourceStr("DATECOMPARE", "IMPORTCLOCKINRDENDDATE,IMPORTCLOCKINRDSTARTDATE"));
                    return;
                }                

                if (cbxkClockInRdUnitType.SelectedItem == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CLOCKINRDUNITTYPE"), Utility.GetResourceStr("REQUIRED", "CLOCKINRDUNITTYPE"));
                    return;
                }

                if (lkClockInRdUnit.DataContext == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CLOCKINRDUNIT"), Utility.GetResourceStr("REQUIRED", "CLOCKINRDUNIT"));
                    return;
                }

                if (cbxkUploadFileType.SelectedItem == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CLOCKINRDUPLOADTYPE"), Utility.GetResourceStr("REQUIRED", "CLOCKINRDUPLOADTYPE"));
                    return;
                }

                T_SYS_DICTIONARY entUnitType = cbxkClockInRdUnitType.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entUnitType.DICTIONARYID) || string.IsNullOrEmpty(entUnitType.DICTIONCATEGORY) || string.IsNullOrEmpty(entUnitType.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CLOCKINRDUNITTYPE"), Utility.GetResourceStr("REQUIRED", "CLOCKINRDUNITTYPE"));
                    return;
                }

                T_SYS_DICTIONARY entUploadType = cbxkUploadFileType.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entUploadType.DICTIONARYID) || string.IsNullOrEmpty(entUploadType.DICTIONCATEGORY) || string.IsNullOrEmpty(entUploadType.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("CLOCKINRDUPLOADTYPE"), Utility.GetResourceStr("REQUIRED", "CLOCKINRDUPLOADTYPE"));
                    return;
                }

                strUnitType = entUnitType.DICTIONARYVALUE.ToString();
                if (strUnitType == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
                {
                    T_HR_COMPANY entCompany = lkClockInRdUnit.DataContext as T_HR_COMPANY;
                    strUnitObjectId = entCompany.COMPANYID;                    
                }
                else if (strUnitType == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
                {
                    T_HR_DEPARTMENT entDepartment = lkClockInRdUnit.DataContext as T_HR_DEPARTMENT;
                    strUnitObjectId = entDepartment.DEPARTMENTID;
                }
                else if (strUnitType == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
                {
                    T_HR_POST entPost = lkClockInRdUnit.DataContext as T_HR_POST;
                    strUnitObjectId = entPost.POSTID;
                }

                if (entUploadType.DICTIONARYVALUE.ToString() == (Convert.ToInt32(ClockInRdUploadFileType.File) + 1).ToString())
                {
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

                    RefreshUI(RefreshedTypes.ShowProgressBar);

                    Stream Stream = (System.IO.Stream)OpenFileDialog.File.OpenRead();

                    byte[] Buffer = new byte[Stream.Length];
                    Stream.Read(Buffer, 0, (int)Stream.Length);

                    Stream.Dispose();
                    Stream.Close();

                    UploadFileModel UploadFile = new UploadFileModel();
                    UploadFile.FileName = OpenFileDialog.File.Name;
                    UploadFile.File = Buffer;

                    string strFileType = "csv";
                    clientAtt.ImportClockInRdListFromFileAsync(UploadFile, strFileType, strUnitType, strUnitObjectId, dtStart, dtEnd, strMsg);

                }
                else if (entUploadType.DICTIONARYVALUE.ToString() == (Convert.ToInt32(ClockInRdUploadFileType.Login) + 1).ToString())
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    clientAtt.ImportClockInRdListFromLoginDataAsync(strUnitType, strUnitObjectId, dtStart, dtEnd, strMsg);
                    
                }
                else if (entUploadType.DICTIONARYVALUE.ToString() == (Convert.ToInt32(ClockInRdUploadFileType.FileAndLogin) + 1).ToString())
                {
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

                    RefreshUI(RefreshedTypes.ShowProgressBar);

                    Stream Stream = (System.IO.Stream)OpenFileDialog.File.OpenRead();

                    byte[] Buffer = new byte[Stream.Length];
                    Stream.Read(Buffer, 0, (int)Stream.Length);

                    Stream.Dispose();
                    Stream.Close();

                    UploadFileModel UploadFile = new UploadFileModel();
                    UploadFile.FileName = OpenFileDialog.File.Name;
                    UploadFile.File = Buffer;

                    string strFileType = "csv";
                    clientAtt.ImportClockInRdListFromFileAndLoginDataAsync(UploadFile, strFileType, strUnitType, strUnitObjectId, dtStart, dtEnd, strMsg);
                }                               

                strMsg = string.Empty;                
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }


    }
}