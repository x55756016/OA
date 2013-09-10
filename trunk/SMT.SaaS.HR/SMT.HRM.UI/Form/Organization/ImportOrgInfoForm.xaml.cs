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
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.AutoCompleteComboBox;

namespace SMT.HRM.UI.Form
{
    /// <summary>
    /// 批量导入部门和岗位信息
    /// </summary>
    public partial class ImportOrgInfoForm : BaseForm, IEntityEditor, IClient
    {
        public OpenFileDialog OpenFileDialog = null;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        OrganizationServiceClient client;
        //部门岗位从excel中导出的数据集合
        ObservableCollection<V_ORGANIZATIONINFO> listOrgInfo = new ObservableCollection<V_ORGANIZATIONINFO>();
        /// <summary>
        /// 构造函数
        /// </summary>
        public ImportOrgInfoForm()
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
            client = new OrganizationServiceClient();
            client.AddBatchOrgInfoCompleted += new EventHandler<AddBatchOrgInfoCompletedEventArgs>(client_AddBatchOrgInfoCompleted);
            client.ImportOrgInfoCompleted += new EventHandler<ImportOrgInfoCompletedEventArgs>(client_ImportOrgInfoCompleted);
            client.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(client_GetCompanyActivedCompleted);
            ImportOrgInfoForm_Load();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        void ImportOrgInfoForm_Load()
        {
            client.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
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
        /// 保存数据的完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_AddBatchOrgInfoCompleted(object sender, AddBatchOrgInfoCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), e.Error.Message, Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Result)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("导入失败"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        /// <summary>
        /// 加载完Excel数据，显示到页面进行预览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_ImportOrgInfoCompleted(object sender, ImportOrgInfoCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), e.Error.Message, Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                listOrgInfo = e.Result;
                DtGrid.ItemsSource = listOrgInfo;
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }
        /// <summary>
        /// DataGrid序号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            V_ORGANIZATIONINFO tmp = (V_ORGANIZATIONINFO)e.Row.DataContext;
            int index = e.Row.GetIndex();
            var cell = DtGrid.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();
            var IdNum = DtGrid.Columns[4].GetCellContent(e.Row) as TextBlock;
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
                ImportOrgInfo();
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        /// <summary>
        ///点击导入，进行保存
        /// </summary>
        private void Save()
        {
            if (listOrgInfo == null || listOrgInfo.Count() == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "没有信息可以导入",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            bool flag = true;
            listOrgInfo.ForEach(it =>
            {
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
            string strMsg = string.Empty, Result = string.Empty;
            string companyID = (acbCompanyName.SelectedItem as T_HR_COMPANY).COMPANYID;
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (objcom, result) =>
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.AddBatchOrgInfoAsync(listOrgInfo, companyID, strMsg);
            };
            com.SelectionBox("导入确认", "是否确认要导入部门岗位信息", ComfirmWindow.titlename, Result);
        }

        /// <summary>
        ///  读取部门岗位的Excel文件，并导入数据库，返回导入后的结果
        /// </summary>
        private void ImportOrgInfo()
        {
            string strMsg = string.Empty;
            try
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                if (acbCompanyName.SelectedItem==null)
                {
                  ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),Utility.GetResourceStr("SELECTCOMPANY"),
                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                  RefreshUI(RefreshedTypes.HideProgressBar);
                  return;
                }
                if (OpenFileDialog == null)
                    return;

                if (OpenFileDialog.File == null)
                    return;
                Stream Stream = (System.IO.Stream)OpenFileDialog.File.OpenRead();

                byte[] Buffer = new byte[Stream.Length];
                Stream.Read(Buffer, 0, (int)Stream.Length);

                Stream.Dispose();
                Stream.Close();
                UploadFileModel UploadFile = new UploadFileModel();
                UploadFile.FileName = OpenFileDialog.File.Name;
                UploadFile.File = Buffer;
                string companyID = (acbCompanyName.SelectedItem as T_HR_COMPANY).COMPANYID;
                Dictionary<string, string> empInfo = new Dictionary<string, string>();
                empInfo.Add("ownerID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                empInfo.Add("ownerPostID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                empInfo.Add("ownerDepartmentID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
                empInfo.Add("ownerCompanyID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
                client.ImportOrgInfoAsync(UploadFile, companyID, empInfo);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
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
