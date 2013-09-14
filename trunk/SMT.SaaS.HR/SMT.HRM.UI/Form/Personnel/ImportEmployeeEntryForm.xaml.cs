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

using SMT.SaaS.FrameworkUI.ChildWidow;
using System.IO;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.AutoCompleteComboBox;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.HRM.UI.Form.Personnel
{
    /// <summary>
    /// 批量导入员工入职信息
    /// </summary>
    public partial class ImportEmployeeEntryForm : BaseForm, IEntityEditor, IClient
    {
        public OpenFileDialog OpenFileDialog = null;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        PersonnelServiceClient client;
        OrganizationServiceClient orgclient;
        //员工入职信息从excel中导出的数据集合
        ObservableCollection<V_EmployeeEntryInfo> listEmployeeInfo = new ObservableCollection<V_EmployeeEntryInfo>();
        /// <summary>
        /// 构造函数
        /// </summary>
        public ImportEmployeeEntryForm()
        {
            InitializeComponent();
          
            this.Loaded += (sender, args) =>
            {
                InitControlEvent();
            };
        }

        /// <summary>
        ///  注册事件
        /// </summary>
        void InitControlEvent()
        {
            orgclient = new OrganizationServiceClient();
            client = new PersonnelServiceClient();
            orgclient.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(client_GetCompanyActivedCompleted);
            client.ImportEmployeeEntryCompleted += new EventHandler<ImportEmployeeEntryCompletedEventArgs>(client_ImportEmployeeEntryCompleted);
            client.ValidUserNameIsExistCompleted += new EventHandler<ValidUserNameIsExistCompletedEventArgs>(client_ValidUserNameIsExistCompleted);
            client.AddBatchEmployeeEntryCompleted += new EventHandler<AddBatchEmployeeEntryCompletedEventArgs>(client_AddBatchEmployeeEntryCompleted);
            ImportEmployeeEntryForm_Load();
        }

        /// <summary>
        /// 设置各个控件显示为空
        /// </summary>
        private void Set()
        {
            listEmployeeInfo = null;
            this.tbFileName.Text = string.Empty;
            this.txtUploadResMsg.Text = string.Empty;
            DtGrid.ItemsSource = null;
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_AddBatchEmployeeEntryCompleted(object sender, AddBatchEmployeeEntryCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Set();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), e.Error.Message, Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                bool flag = e.Result;
                if (flag)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                Set();
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        /// <summary>
        /// 验证之后显示到页面预览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_ValidUserNameIsExistCompleted(object sender, ValidUserNameIsExistCompletedEventArgs e)
       {
          try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Set();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), e.Error.Message, Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                listEmployeeInfo = e.Result;
                DtGrid.ItemsSource = listEmployeeInfo;
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            catch (Exception ex)
            {
                Set();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
       }


        /// <summary>
        /// 导入完数据之后接着验证用户名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_ImportEmployeeEntryCompleted(object sender, ImportEmployeeEntryCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Set();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), e.Error.Message, Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                ObservableCollection<V_EmployeeEntryInfo> listEmp = e.Result;
                if (listEmp!=null)
                {
                    listEmp.ForEach(it =>
                    {
                        it.UserName = HanziZhuanPingYin.Convert(it.EmployeeName).ToLower();
                    });
                    client.ValidUserNameIsExistAsync(listEmp);
                }
                else
                {
                    Set();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "导入数据为空,请确认模板和数据是否正确", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            }
            catch (Exception ex)
            {
                Set();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        void ImportEmployeeEntryForm_Load()
        {
            orgclient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }
        /// <summary>
        /// 加载该员工可以看到的有效的公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetCompanyActivedCompleted(object sender, GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                acbCompanyName.ItemsSource = e.Result;
                acbCompanyName.ValueMemberPath = "CNAME";
            }
        }

        /// <summary>
        /// DataGrid序号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            V_EmployeeEntryInfo tmp = (V_EmployeeEntryInfo)e.Row.DataContext;
            int index = e.Row.GetIndex();
            var cell = DtGrid.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();
            var IdNum = DtGrid.Columns[3].GetCellContent(e.Row) as TextBlock;
            if (tmp.IdNumber.IndexOf("身份证不合法") > -1)
            {
                tmp.IdNumber = tmp.IdNumber.Replace("身份证不合法", string.Empty);
                IdNum.Foreground = new SolidColorBrush(Colors.Red);
                ToolTipService.SetToolTip(IdNum, tmp.IdNumber + "请参考提示信息进行修改");
                txtUploadResMsg.Text = "请注意，有员工的身份证号码有异常，请重新确认！";
                txtUploadResMsg.Foreground = new SolidColorBrush(Colors.Red);
            }
            if (!string.IsNullOrWhiteSpace(tmp.ErrorMsg))
            {
                txtUploadResMsg.Text = "导入数据存在异常，请根据提示信息进行修改";
                txtUploadResMsg.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
   
        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //清空提醒信息
                this.txtUploadResMsg.Text = "";
                OpenFileDialog = new OpenFileDialog();

                OpenFileDialog.Multiselect = false;
                //OpenFileDialog.Filter = "Excel 文件 (*.csv,*.xls)|*.csv,*.xls";
                OpenFileDialog.Filter = "Excel Files (*.xls)|*.xls;";
                if (OpenFileDialog.ShowDialog() != true)
                {
                    return;
                }
                tbFileName.Text = OpenFileDialog.File.Name;
                txtUploadResMsg.Text = string.Empty;
                ImportEmployeeEntry();
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        /// <summary>
        ///  读取员工入职信息的Excel文件，并导入数据库，返回导入后的结果
        /// </summary>
        private void ImportEmployeeEntry()
        {
            string strMsg = string.Empty;
            try
            {
                if (acbCompanyName.SelectedItem==null)
                {
                    tbFileName.Text = string.Empty;//不显示文件
                  ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),Utility.GetResourceStr("SELECTCOMPANY"),
                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                if (OpenFileDialog == null || OpenFileDialog.File == null)
                {
                    tbFileName.Text = string.Empty;//不显示文件
                    return;
                }
                Stream Stream = (System.IO.Stream)OpenFileDialog.File.OpenRead();

                byte[] Buffer = new byte[Stream.Length];
                Stream.Read(Buffer, 0, (int)Stream.Length);

                Stream.Dispose();
                Stream.Close();
                SMT.Saas.Tools.PersonnelWS.UploadFileModel UploadFile = new SMT.Saas.Tools.PersonnelWS.UploadFileModel();
                UploadFile.FileName = OpenFileDialog.File.Name;
                UploadFile.File = Buffer;
                string companyID = (acbCompanyName.SelectedItem as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID;
                Dictionary<string, string> empInfo = new Dictionary<string, string>();
                empInfo.Add("ownerID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                empInfo.Add("ownerPostID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                empInfo.Add("ownerDepartmentID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
                empInfo.Add("ownerCompanyID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.ImportEmployeeEntryAsync(UploadFile, companyID, empInfo);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        /// <summary>
        ///点击导入，进行保存
        /// </summary>
        private void Save()
        {
            if (listEmployeeInfo == null || listEmployeeInfo.Count() == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "没有信息可以导入",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            string strMsg = string.Empty, Result = string.Empty;
            string companyID = (acbCompanyName.SelectedItem as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID;
            ObservableCollection<V_EmployeeEntryInfo> listTemp = DtGrid.ItemsSource as ObservableCollection<V_EmployeeEntryInfo>;
            List<T_SYS_DICTIONARY> dic= Application.Current.Resources["SYS_DICTIONARY"] as  List<T_SYS_DICTIONARY>;
            bool flag = true;
            listTemp.ForEach(it =>
                {
                    decimal? plDic=dic.Where(s => s.DICTIONCATEGORY == "POSTLEVEL" && s.DICTIONARYNAME == it.PostLevel).FirstOrDefault().DICTIONARYVALUE;
                    it.PostLevel = Convert.ToString(plDic);
                    if (!string.IsNullOrWhiteSpace(it.ErrorMsg))
                    {
                        flag = false;
                    }
                });
            if (!flag)
            {
                 ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "导入数据存在错误，请参照表格后面的提示信息进行修改",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (objcom, result) =>
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.AddBatchEmployeeEntryAsync(listTemp, companyID, strMsg);
            };
            com.SelectionBox("导入确认", "是否确认要导入员工入职信息", ComfirmWindow.titlename, Result);

        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("IMPORT");
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
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    //  Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("IMPORT"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/area/18_import.png"
            };

            items.Add(item);

            return items;
            //return ToolbarItems;
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
            // throw new NotImplementedException();
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

        /// <summary>
        /// 选项公司事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void acbCompanyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
