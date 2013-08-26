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
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.SalaryWS;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class EmployeeAddImportForm : BaseForm, IEntityEditor, IClient
    {
        public OpenFileDialog OpenFileDialog = null;
        byte[] byExport;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        PersonnelServiceClient client;
        private SalaryServiceClient salaryClient;
        //员工加扣款从excel中导出的数据集合
        ObservableCollection<T_HR_EMPLOYEEADDSUM> ListPensions = new ObservableCollection<T_HR_EMPLOYEEADDSUM>();
        public EmployeeAddImportForm()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                client = new PersonnelServiceClient();
                client.ImportClockInRdListFromExcelCompleted += new EventHandler<ImportClockInRdListFromExcelCompletedEventArgs>(client_ImportClockInRdListFromExcelCompleted);
                //client.ImportClockInRdListFromExcelForShowCompleted += new EventHandler<ImportClockInRdListFromExcelForShowCompletedEventArgs>(client_ImportClockInRdListFromExcelForShowCompleted);
                //client.BatchAddPensionDetailCompleted += new EventHandler<BatchAddPensionDetailCompletedEventArgs>(client_BatchAddPensionDetailCompleted);

                salaryClient = new SalaryServiceClient();
                salaryClient.EmployeeAddSumLotsofADDCompleted += new EventHandler<EmployeeAddSumLotsofADDCompletedEventArgs>(salaryClient_EmployeeAddSumLotsofADDCompleted);
                salaryClient.ImportEmployeeAddSumFromExcelForShowCompleted += new EventHandler<ImportEmployeeAddSumFromExcelForShowCompletedEventArgs>(salaryClient_ImportEmployeeAddSumFromExcelForShowCompleted);
            };
        }
        #region 导入员工员工加扣款完成事件-预览结果

      
        void salaryClient_ImportEmployeeAddSumFromExcelForShowCompleted(object sender, ImportEmployeeAddSumFromExcelForShowCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), e.strMsg,
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }

                if (e.Result != null)
                {
                    ListPensions = e.Result;
                    this.LoadData(ListPensions.ToList());
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
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "导入员工员工加扣款记录失败",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }
        #endregion

        #region 批量添加员工加扣款导入完成事件
        void salaryClient_EmployeeAddSumLotsofADDCompleted(object sender, EmployeeAddSumLotsofADDCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "员工加扣款导入成功",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //return;
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "员工加扣款导入失败",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //return;
                }

                //if (!string.IsNullOrEmpty(e.StrMsg))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), e.StrMsg,
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "员工加扣款导入失败，请联系管理员",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }
       
        #endregion



        private void BrowseFiles_Click(object sender, RoutedEventArgs e)
        {
            //清空提醒信息
            this.txtUploadResMsg.Text = "";
            ListPensions.Clear();
            OpenFileDialog = new OpenFileDialog();

            OpenFileDialog.Multiselect = false;
            OpenFileDialog.Filter = "Excel Files (*.xls)|*.xls;";

            if (OpenFileDialog.ShowDialog() != true)
            {
                return;
            }

            tbFileName.Text = OpenFileDialog.File.Name;
            txtUploadResMsg.Text = string.Empty;
            ImportEmployeeAddSum();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            //ImportClockInRd();
            ImportPension();

            //if (StrToolTip.IndexOf("有员工的身份证号码有异常") > -1)
            //{ 

            //}
        }

        private void ImportPension()
        {
            if (ListPensions.Count() == 0 || ListPensions == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "没有员工加扣款记录可以导入",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            string StrToolTip = "";//提示信息
            StrToolTip = txtUploadResMsg.Text;
            string Result = "";
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (objcom, result) =>
            {
                Save();
            };
            com.SelectionBox("员工加扣款导入确认", "是否确认要导入员工加扣款", ComfirmWindow.titlename, Result);
        }

        private void Save()
        {
              
            string strMsg = string.Empty;                
            Dictionary<string, string> paras = new Dictionary<string, string>();
            //paras.Add("CITY", (cbxCity.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE.ToString());
            paras.Add("OWNERID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            paras.Add("CREATEUSERID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            paras.Add("OWNERCOMPANYID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            paras.Add("OWNERDEPARTMENTID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            paras.Add("OWNERPOSTID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID);
            paras.Add("YEAR", (Nuyear.Value).ToString());
            paras.Add("MONTH", (NuStartmounth.Value).ToString());
            RefreshUI(RefreshedTypes.ShowProgressBar);


            salaryClient.EmployeeAddSumLotsofADDAsync(ListPensions);
            
        }

        /// <summary>
        ///  读取员工加扣款卡的Excel文件，并导入数据库，返回导入后的结果
        /// </summary>
        private void ImportEmployeeAddSum()
        {

            string strMsg = string.Empty;
            try
            {
                //if (cbxCity.SelectedItem == null)
                //{
                //    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CITY"));
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "CITY"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                if (string.IsNullOrEmpty(tbFileName.Text))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("IMPORTSELECT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("IMPORTSELECT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                if (OpenFileDialog == null)
                    return;

                if (OpenFileDialog.File == null)
                    return;

                RefreshUI(RefreshedTypes.ShowProgressBar);

                Stream Stream = (System.IO.Stream)OpenFileDialog.File.OpenRead();

                byte[] Buffer = new byte[Stream.Length];
                Stream.Read(Buffer, 0, (int)Stream.Length);

                Stream.Dispose();
                Stream.Close();

                SMT.Saas.Tools.SalaryWS.UploadFileModel UploadFile = new SMT.Saas.Tools.SalaryWS.UploadFileModel();
                UploadFile.FileName = OpenFileDialog.File.Name;
                UploadFile.File = Buffer;

                strMsg = string.Empty;
                Dictionary<string, string> paras = new Dictionary<string, string>();
                //paras.Add("CITY", (cbxCity.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE.ToString());
                paras.Add("OWNERID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                paras.Add("CREATEUSERID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                paras.Add("CREATECOMPANYID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
                paras.Add("CREATEDEPARTMENTID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
                paras.Add("CREATEPOSTID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                paras.Add("YEAR", (Nuyear.Value).ToString());
                paras.Add("MONTH", (NuStartmounth.Value).ToString());
                salaryClient.ImportEmployeeAddSumFromExcelForShowAsync(UploadFile, paras, strMsg, true);
                //client.ImportClockInRdListFromExcelAsync(UploadFile, paras, strMsg);
                //client.ImportClockInRdListFromExcelAsync(UploadFile, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strMsg);
                //clientAtt.ImportClockInRdListFromExcelAsync(UploadFile, dtStart, dtEnd, strMsg);
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        void client_ImportClockInRdListFromExcelCompleted(object sender, ImportClockInRdListFromExcelCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.strMsg == "true")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("IMPORTSUCCESSED", "PENSIONDETAIL"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("IMPORTSUCCESSED", "PENSIONDETAIL"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.strMsg == "false")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("导入不成功"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else if (e.strMsg == "NOTFOUND")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("未找到导入设置"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                tbFileName.Text = string.Empty;
                txtUploadResMsg.Text = "1";
                RefreshUI(RefreshedTypes.All);
            }
        }


        private void Nuyear_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = (NumericUpDown)sender;
            nuyear.Value = DateTime.Now.Year.ToDouble();
        }

        private void NuStartmounth_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown nuyear = (NumericUpDown)sender;
            nuyear.Value = DateTime.Now.Month.ToDouble();
        }


        #region 员工加扣款导入记录

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //SMT.HRM.UI.BasePage.SetRowLogo(DtGrid, e.Row, "T_HR_LEFTOFFICE");
            T_HR_EMPLOYEEADDSUM tmp = (T_HR_EMPLOYEEADDSUM)e.Row.DataContext;
            int index = e.Row.GetIndex();
            var cell = DtGrid.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();
            var IdNum = DtGrid.Columns[4].GetCellContent(e.Row) as TextBlock;
            //有提示信息“此身份证号无效或员工已离职”则标红，表示有问题。
            if(string.IsNullOrEmpty(tmp.PROJECTMONEY.ToString()))
            {
                //IdNum.BorderBrush = new SolidColorBrush(Colors.Red);
                IdNum.Foreground = new SolidColorBrush(Colors.Red);
                ToolTipService.SetToolTip(IdNum, tmp.PROJECTMONEY);
                txtUploadResMsg.Text = "请注意，导入的金额有误，请重新确认！";
                txtUploadResMsg.Foreground = new SolidColorBrush(Colors.Red);
            }

        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            //LoadData();
        }

        private void LoadData(List<T_HR_EMPLOYEEADDSUM> ListPensions)
        {            
            DtGrid.ItemsSource = ListPensions;               
        }

        #endregion



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
                    ImportPension();
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
        
    }
}
