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
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.SalaryWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.OrganizationWS;
using System.Collections.ObjectModel;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Text;
using SMT.SaaS.MobileXml;
using System.Collections;
using System.Xml.Linq;


namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalaryRecordMassAudit : BaseForm, IEntityEditor, IAudit, IClient
    {
        public FormTypes FormType { get; set; }
        public string FormID { get; set; }
        public int ObjectType { get; set; }
        public string ObjectValue { get; set; }
        public string CheckState { get; set; }
        private string nuYear { get; set; }
        private string nuStartmounth { get; set; }
        private string monthlyBatchid;

        int recordCount = 0;
        Dictionary<string, decimal> dictSum;
        private System.Collections.ObjectModel.ObservableCollection<string> ids = new System.Collections.ObjectModel.ObservableCollection<string>();
        int iChooseCount = 0;
        decimal dChooseSum = 0;

        DataSetData dataSet;
        private List<ToolbarItem> toolbarItems = new List<ToolbarItem>();
        private SMT.Saas.Tools.SalaryWS.SalaryServiceClient client = new Saas.Tools.SalaryWS.SalaryServiceClient();
        private SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient orgClient = new Saas.Tools.OrganizationWS.OrganizationServiceClient();
        private string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

        List<string> paras = new List<string>();

        private DataGrid DtGriddy;

        private T_HR_SALARYRECORDBATCH salaryRecordBatch;

        public T_HR_SALARYRECORDBATCH SalaryRecordBatch
        {
            get { return salaryRecordBatch; }
            set
            {
                salaryRecordBatch = value;
                this.DataContext = salaryRecordBatch;
            }
        }
        private bool needsubmit = false;

        //和导出相关变量
        private byte[] byExport;
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;

        //关闭待办
        SMT.Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient engineclient = new Saas.Tools.EngineWS.EngineWcfGlobalFunctionClient();


        void SalaryRecordMassAudit_Loaded(object sender, RoutedEventArgs e)
        {
            if ((FormType == FormTypes.Audit || FormType == FormTypes.Resubmit) && !string.IsNullOrWhiteSpace(FormID))
            {
                InitParas();
                //  basePage.GetEntityLogo("T_HR_SALARYRECORDBATCH");
                tbAuditType.Visibility = Visibility.Collapsed;
                cbxAuditType.Visibility = Visibility.Collapsed;
                tbDpepart.Visibility = Visibility.Collapsed;
                lkdepart.Visibility = Visibility.Collapsed;

                client.GetSalaryRecordBatchByIDAsync(FormID, "AUDIT");

                return;
            }

            txtBalanceYear.Text = paras[2];
            nuYear = paras[2];
            nuStartmounth = paras[3];
            nudBalanceMonth.Value = Convert.ToDouble(paras[3]);
            CheckState = paras[4];
            cbxkAssignedObjectType.SelectedIndex = paras[0].ToInt32();
            ObjectType = paras[0].ToInt32();
            ObjectValue = paras[1];
            tbBalanceYearMonth.Text = paras[2] + Utility.GetResourceStr("YEAR") + paras[3] + Utility.GetResourceStr("MONTH");
            monthlyBatchid = paras[5];

            #region 初始化批量审核实体
            SalaryRecordBatch = new T_HR_SALARYRECORDBATCH();
            SalaryRecordBatch.MONTHLYBATCHID = Guid.NewGuid().ToString();
            SalaryRecordBatch.BALANCEOBJECTNAME = string.Empty;
            SalaryRecordBatch.BALANCEOBJECTID = ObjectValue;
            SalaryRecordBatch.BALANCEOBJECTTYPE = ObjectType.ToString();
            SalaryRecordBatch.BALANCEYEAR = Convert.ToDecimal(txtBalanceYear.Text);
            SalaryRecordBatch.BALANCEMONTH = Convert.ToDecimal(nudBalanceMonth.Value);
            SalaryRecordBatch.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
            SalaryRecordBatch.CREATEDATE = System.DateTime.Now;
            SalaryRecordBatch.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();

            SalaryRecordBatch.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            SalaryRecordBatch.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            SalaryRecordBatch.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            SalaryRecordBatch.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            string strOwnerCompanyId = string.Empty, strOwnerDepartmentId = string.Empty, strOwnerPostId = string.Empty;

            if (SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                SaaS.LocalData.UserPost curPost = null;

                if (ObjectType == 0)
                {
                    var temp = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.CompanyID == ObjectValue);
                    if (temp != null)
                    {
                        curPost = temp.FirstOrDefault();
                    }
                }
                else if (ObjectType == 1)
                {
                    var temp = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.DepartmentID == ObjectValue);
                    if (temp != null)
                    {
                        strOwnerCompanyId = temp.FirstOrDefault().CompanyID;
                        strOwnerDepartmentId = temp.FirstOrDefault().DepartmentID;
                        strOwnerPostId = temp.FirstOrDefault().PostID;
                    }
                }
                else if (ObjectType == 2)
                {
                    var temp = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.PostID == ObjectValue);
                    if (temp != null)
                    {
                        strOwnerCompanyId = temp.FirstOrDefault().CompanyID;
                        strOwnerDepartmentId = temp.FirstOrDefault().DepartmentID;
                        strOwnerPostId = temp.FirstOrDefault().PostID;
                    }
                }

                if (curPost == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARNING"), "请注意，您当前在此公司无在职岗位，提交操作被终止", Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    this.IsEnabled = false;
                    return;
                }


                strOwnerCompanyId = curPost.CompanyID;
                strOwnerDepartmentId = curPost.DepartmentID;
                strOwnerPostId = curPost.PostID;
            }
            else
            {
                strOwnerCompanyId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                strOwnerDepartmentId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                strOwnerPostId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            }

            if (!string.IsNullOrWhiteSpace(strOwnerCompanyId) && !string.IsNullOrWhiteSpace(strOwnerDepartmentId) && !string.IsNullOrWhiteSpace(strOwnerPostId))
            {
                SalaryRecordBatch.OWNERCOMPANYID = strOwnerCompanyId;
                SalaryRecordBatch.OWNERDEPARTMENTID = strOwnerDepartmentId;
                SalaryRecordBatch.OWNERPOSTID = strOwnerPostId;
                SalaryRecordBatch.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                salaryRecordBatch.BALANCEOBJECTNAME = "0";
                this.DataContext = SalaryRecordBatch;
                monthlyBatchid = SalaryRecordBatch.MONTHLYBATCHID;
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();

                BindAssignObjectLookup();
                InitParas();
                LoadData();
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }

            #endregion
        }
        #region  构造函数
        public SalaryRecordMassAudit()
        {
            InitializeComponent();
        }
        public SalaryRecordMassAudit(FormTypes formtype, List<string> paras)
        {
            InitializeComponent();
            FormType = formtype;
            this.paras = paras;
            this.Loaded += new RoutedEventHandler(SalaryRecordMassAudit_Loaded);

        }
        public SalaryRecordMassAudit(FormTypes formtype, string strFormID)
        {

            InitializeComponent();
            FormType = formtype;
            FormID = strFormID;
            this.Loaded += new RoutedEventHandler(SalaryRecordMassAudit_Loaded);
        }

        public SalaryRecordMassAudit(FormTypes formtype, int strType, string strValue, string year, string month, string strCheckState)
        {
            InitializeComponent();
            txtBalanceYear.Text = year;
            nudBalanceMonth.Value = Convert.ToDouble(month);
            FormType = formtype;
            CheckState = strCheckState;
            cbxkAssignedObjectType.SelectedIndex = strType;
            ObjectType = strType;
            ObjectValue = strValue;
            tbBalanceYearMonth.Text = year + Utility.GetResourceStr("YEAR") + month + Utility.GetResourceStr("MONTH");
            BindAssignObjectLookup();
            //   basePage.GetEntityLogo("T_HR_SALARYRECORDBATCH");
            InitParas();
            LoadData();
        }
        #endregion
        public void InitParas()
        {

            orgClient.GetCompanyByIdCompleted += new EventHandler<GetCompanyByIdCompletedEventArgs>(orgClient_GetCompanyByIdCompleted);
            orgClient.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(orgClient_GetDepartmentByIdCompleted);
            orgClient.GetPostByIdCompleted += new EventHandler<GetPostByIdCompletedEventArgs>(orgClient_GetPostByIdCompleted);

            client.UndoRepaymentMassCompleted += new EventHandler<UndoRepaymentMassCompletedEventArgs>(client_UndoRepaymentMassCompleted);
            client.SalaryRecordBatchUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryRecordBatchUpdateCompleted);
            client.GetSalaryRecordBatchByIDCompleted += new EventHandler<GetSalaryRecordBatchByIDCompletedEventArgs>(client_GetSalaryRecordBatchByIDCompleted);
            client.SalaryRecordBatchAddCompleted += new EventHandler<SalaryRecordBatchAddCompletedEventArgs>(client_SalaryRecordBatchAddCompleted);
            client.GetAuditSalaryRecordsCompleted += new EventHandler<GetAuditSalaryRecordsCompletedEventArgs>(client_GetAuditSalaryRecordsCompleted);
            //client.FBStatisticsMassCompleted += new EventHandler<FBStatisticsMassCompletedEventArgs>(client_FBStatisticsMassCompleted); //查询预算服务(旧)停用
            client.FBStatisticsMassByChooseCompleted += new EventHandler<FBStatisticsMassByChooseCompletedEventArgs>(client_FBStatisticsMassByChooseCompleted);
            client.ExportSalaryExcelCompleted += new EventHandler<ExportSalaryExcelCompletedEventArgs>(client_ExportSalaryExcelCompleted);

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
        }


        #region 加载批量审核数据
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        void client_GetAuditSalaryRecordsCompleted(object sender, GetAuditSalaryRecordsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                dataSet = e.Result;

                if (dataSet == null)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("NODATA") + ",请检查是否未获得员工月薪生成的权限，或提交数据已被更改。", Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    return;
                }
                IEnumerable list = GetDataListForSalary(e.Result);
                DtGriddy = new DataGrid();
                #region 设置当前加载DtGriddy样式
                DtGriddy.Style = Application.Current.Resources["DataGridStyle"] as Style;
                DtGriddy.CellStyle = Application.Current.Resources["DataGridCellStyle"] as Style;
                DtGriddy.RowHeaderStyle = Application.Current.Resources["DataGridRowHeaderStyle"] as Style;
                DtGriddy.ColumnHeaderStyle = Application.Current.Resources["DataGridColumnHeaderStyle"] as Style;
                DtGriddy.RowStyle = Application.Current.Resources["DataGridRowStyle"] as Style;


                DtGriddy.FrozenColumnCount = 8;

                if (recordCount >= 17) //数据少了  固定高度 空白太多
                {
                    DtGriddy.Height = 400;
                }
                //DtGriddy.AreRowDetailsFrozen = true;
                DtGriddy.IsReadOnly = true;
                DtGriddy.CanUserSortColumns = false;
                DtGriddy.CanUserReorderColumns = false;
                DtGriddy.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                DtGriddy.AutoGenerateColumns = false;
                DtGriddy.IsReadOnly = true;
                #endregion

                #region chexkbox列
                DataGridTemplateColumn colchb = new DataGridTemplateColumn();
                colchb.Header = Utility.GetResourceStr("SELECT");
                colchb.CellEditingTemplate = (DataTemplate)Resources["DataTemplates"];
                colchb.HeaderStyle = Resources["DataGridCheckBoxColumnHeaderStyleAudit"] as Style;
                if (dataSet.Tables.Count > 0 && SalaryRecordBatch.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    colchb.Visibility = Visibility.Collapsed;
                }

                DtGriddy.Columns.Add(colchb);
                #endregion
                #region 序号列
                DataGridTemplateColumn colNo = new DataGridTemplateColumn();
                colNo.Header = Utility.GetResourceStr("序号");
                colNo.CellEditingTemplate = (DataTemplate)Resources["DataTemplateTextBlock"];
                //colchb.HeaderStyle = Resources["DataGridCheckBoxColumnHeaderStyleAudit"] as Style;
                DtGriddy.Columns.Add(colNo);
                #endregion
                #region  初始列
                if (e.Result.Tables.Count > 0)
                {
                    foreach (DataColumnInfo column in e.Result.Tables[0].Columns)
                    {
                        #region 注释
                        //if (column.DisplayIndex != -1)
                        //{
                        //    DataGridColumn col;
                        //    DataTemplate dt;
                        //    if (column.DataTypeName == typeof(bool).FullName)
                        //    {
                        //        DataGridCheckBoxColumn checkBoxColumn = new DataGridCheckBoxColumn();
                        //        checkBoxColumn.Binding = new Binding(column.ColumnName);
                        //        col = checkBoxColumn;
                        //    }
                        //    else if (column.DataTypeName == typeof(DateTime).FullName)
                        //    {
                        //        DataGridTemplateColumn templateColumn = new DataGridTemplateColumn();
                        //        string temp = TemplateManager.DataTemplates["DateTimeCellTemplate"];
                        //        temp = temp.Replace("@HorizontalAlignment@", HorizontalAlignment.Left.ToString());
                        //        temp = temp.Replace("@Text@", column.ColumnName);
                        //        temp = temp.Replace("@DateTimeFormat@", "MM/dd/yyyy");

                        //        dt = XamlReader.Load(temp) as DataTemplate;
                        //        templateColumn.CellTemplate = dt;

                        //        DataTemplate t = new DataTemplate();

                        //        temp = TemplateManager.DataTemplates["DateTimeCellEditingTemplate"];
                        //        temp = temp.Replace("@HorizontalAlignment@", HorizontalAlignment.Left.ToString());
                        //        temp = temp.Replace("@SelectedDate@", column.ColumnName);

                        //        dt = XamlReader.Load(temp) as DataTemplate;

                        //        templateColumn.CellEditingTemplate = dt;
                        //        col = templateColumn;

                        //    }
                        //    else
                        //    {
                        //        DataGridTextColumn textColumn = new DataGridTextColumn();
                        //        textColumn.Binding = new Binding(column.ColumnName);
                        //        textColumn.Binding.ValidatesOnExceptions = true;
                        //        col = textColumn;
                        //    }
                        #endregion
                        DataGridColumn col;
                        DataGridTextColumn textColumn = new DataGridTextColumn();
                        textColumn.Binding = new Binding(column.ColumnName);
                        textColumn.Binding.ValidatesOnExceptions = true;
                        col = textColumn;
                        col.IsReadOnly = column.IsReadOnly;
                        col.Visibility = column.IsShow ? Visibility.Visible : Visibility.Collapsed;
                        col.Header = column.ColumnTitle;
                        col.SortMemberPath = column.ColumnName;
                        col.CanUserSort = false;
                        col.MinWidth = 100;
                        DtGriddy.Columns.Add(col);

                    }
                }
                #endregion

                DtGriddy.LoadingRow += new EventHandler<DataGridRowEventArgs>(DtGriddy_LoadingRow);
                DtGriddy.ItemsSource = list;

                SpSalaryRecord.Children.Add(DtGriddy);
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.HideProgressBar);
                #region

                //if (its != null)
                //{

                //    for (int i = 0; i < its.Count; i++)
                //    {
                //        V_salaryRecordDetailView itemTmp = new V_salaryRecordDetailView();
                //        itemTmp.ACTUALLYPAY = its[i].ACTUALLYPAY;
                //        itemTmp.EMPLOYEESALARYRECORDID = its[i].EMPLOYEESALARYRECORDID;
                //        itemTmp.EMPLOYEEID = its[i].EMPLOYEEID;
                //        itemTmp.EMPLOYEENAME = its[i].EMPLOYEENAME;
                //        itemTmp.DEPARTMENT = its[i].DEPARTMENT;
                //        itemTmp.POST = its[i].POST;
                //        its[i].ACTUALLYPAY = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(its[i].ACTUALLYPAY);
                //        decimal actPay = string.IsNullOrEmpty(its[i].ACTUALLYPAY) ? 0 : Convert.ToDecimal(its[i].ACTUALLYPAY);
                //        its[i].ACTUALLYPAY = actPay.ToString();
                //        if (SalarySumList.ContainsKey("实发工资"))
                //        {
                //            SalarySumList["实发工资"] += actPay;
                //        }
                //        else
                //        {
                //            SalarySumList.Add("实发工资", actPay);
                //        }
                //        if (its[i].SALARYLEVEL != null && its[i].POSTLEVEL != null)
                //        {
                //            its[i].POSTLEVELCODE = SMT.SaaS.FrameworkUI.Common.Utility.GetConvertResources("POSTLEVEL", its[i].POSTLEVEL.ToString());
                //            its[i].POSTLEVELCODE += "-" + its[i].SALARYLEVEL.ToString();
                //        }
                //        itemTmp.POSTLEVELCODE = its[i].POSTLEVELCODE;

                //        for (int j = 0; j < its[i].EMPLOYEESALARYRECORDITEMS.Count(); j++)
                //        {
                //            V_EMPLOYEESALARYRECORDITEM singleRecordItems = new V_EMPLOYEESALARYRECORDITEM();
                //            singleRecordItems.SALARYITEMNAME = its[i].EMPLOYEESALARYRECORDITEMS[j].SALARYITEMNAME;
                //            singleRecordItems.SUM = its[i].EMPLOYEESALARYRECORDITEMS[j].SUM;
                //            singleRecordItems.SALARYITEMID = its[i].EMPLOYEESALARYRECORDITEMS[j].SALARYITEMID;
                //            singleRecordItems.SALARYRECORDITEMID = its[i].EMPLOYEESALARYRECORDITEMS[j].SALARYRECORDITEMID;
                //            singleRecordItems.SALARYITEMCODE = its[i].EMPLOYEESALARYRECORDITEMS[j].SALARYITEMCODE;
                //            its[i].EMPLOYEESALARYRECORDITEMS[j].SUM = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(its[i].EMPLOYEESALARYRECORDITEMS[j].SUM);
                //            //计算每个薪资项合计值 不包括以下几项
                //            if (singleRecordItems.SALARYITEMNAME != "备注" && singleRecordItems.SALARYITEMNAME != "纳税系数" && singleRecordItems.SALARYITEMNAME != "扣税基数" && singleRecordItems.SALARYITEMNAME != "税率")
                //            {
                //                decimal sum = string.IsNullOrEmpty(its[i].EMPLOYEESALARYRECORDITEMS[j].SUM) ? 0 : Convert.ToDecimal(its[i].EMPLOYEESALARYRECORDITEMS[j].SUM);
                //                if (SalarySumList.ContainsKey(singleRecordItems.SALARYITEMNAME))
                //                {
                //                    SalarySumList[singleRecordItems.SALARYITEMNAME] += sum;
                //                }
                //                else
                //                {
                //                    SalarySumList.Add(singleRecordItems.SALARYITEMNAME, sum);
                //                }
                //            }
                //        }
                //    }

                //    dataPager.PageCount = e.pageCount;

                //    #region 合计
                //    V_salaryRecordDetailView sumRecorder = new V_salaryRecordDetailView();
                //    sumRecorder.ORDINAL = "合计";
                //    sumRecorder.EMPLOYEESALARYRECORDID = Guid.NewGuid().ToString();
                //    sumRecorder.DEPARTMENT = "--";
                //    sumRecorder.POST = "--";
                //    sumRecorder.POSTLEVELCODE = "--";
                //    sumRecorder.EMPLOYEENAME = "--";
                //    sumRecorder.EMPLOYEESALARYRECORDITEMS = new ObservableCollection<V_EMPLOYEESALARYRECORDITEM>();
                //    if (SalarySumList.ContainsKey("实发工资"))
                //    {
                //        sumRecorder.ACTUALLYPAY = SalarySumList["实发工资"].ToString();
                //    }
                //    var salaryRecordItems = its.FirstOrDefault().EMPLOYEESALARYRECORDITEMS;
                //    foreach (var ent in salaryRecordItems)
                //    {
                //        V_EMPLOYEESALARYRECORDITEM sumRecordItem = new V_EMPLOYEESALARYRECORDITEM();
                //        if (SalarySumList.ContainsKey(ent.SALARYITEMNAME))
                //        {
                //            sumRecordItem.SALARYITEMID = ent.SALARYITEMID;
                //            sumRecordItem.SALARYITEMNAME = ent.SALARYITEMNAME;
                //            sumRecordItem.SUM = SalarySumList[ent.SALARYITEMNAME].ToString();
                //        }
                //        sumRecorder.EMPLOYEESALARYRECORDITEMS.Add(sumRecordItem);
                //    }
                //    its.Add(sumRecorder);
                //    #endregion

                //    SpSalaryRecord.Children.Clear();

                //    DtGriddy = new DataGrid();


                //    #region  --- 初始化固定列 ---

                //    DataGridTemplateColumn colchb = new DataGridTemplateColumn();
                //    colchb.Header = Utility.GetResourceStr("SELECT");
                //    colchb.CellEditingTemplate = (DataTemplate)Resources["DataTemplates"];
                //    colchb.HeaderStyle = Resources["DataGridCheckBoxColumnHeaderStyleAudit"] as Style;
                //    if (its.Count > 0 && its[0].CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                //    {
                //        colchb.Visibility = Visibility.Collapsed;
                //    }
                //    DtGriddy.Columns.Add(colchb);

                //    DataGridTextColumn col = new DataGridTextColumn();
                //    col.Header = Utility.GetResourceStr("EMPLOYEESALARYRECORDID");
                //    col.Binding = new Binding("EMPLOYEESALARYRECORDID");
                //    col.Visibility = Visibility.Collapsed;
                //    col.CanUserSort = false;
                //    DtGriddy.Columns.Add(col);

                //    DataGridTextColumn colOrder = new DataGridTextColumn();
                //    colOrder.Header = Utility.GetResourceStr("序号");
                //    colOrder.Binding = new Binding("ORDINAL");
                //    colOrder.Visibility = Visibility.Visible;
                //    colOrder.CanUserSort = false;
                //    DtGriddy.Columns.Add(colOrder);

                //    DataGridTextColumn coldepartment = new DataGridTextColumn();
                //    coldepartment.Header = Utility.GetResourceStr("DEPARTMENT");
                //    coldepartment.Binding = new Binding("DEPARTMENT");
                //    coldepartment.CanUserSort = false;
                //    DtGriddy.Columns.Add(coldepartment);

                //    DataGridTextColumn colPost = new DataGridTextColumn();
                //    colPost.Header = Utility.GetResourceStr("POST");
                //    colPost.Binding = new Binding("POST");
                //    colPost.CanUserSort = false;
                //    DtGriddy.Columns.Add(colPost);

                //    DataGridTextColumn colEmployee = new DataGridTextColumn();
                //    colEmployee.Header = Utility.GetResourceStr("EMPLOYEENAME");
                //    colEmployee.Binding = new Binding("EMPLOYEENAME");
                //    colEmployee.CanUserSort = false;
                //    DtGriddy.Columns.Add(colEmployee);

                //    DataGridTextColumn col2 = new DataGridTextColumn();
                //    col2.Header = Utility.GetResourceStr("POSTLEVELCODE");
                //    col2.Binding = new Binding("POSTLEVELCODE");
                //    col2.CanUserSort = false;
                //    DtGriddy.Columns.Add(col2);

                //    DataGridTextColumn col8 = new DataGridTextColumn();
                //    col8.Header = Utility.GetResourceStr("ACTUALLYPAY");
                //    col8.Binding = new Binding("ACTUALLYPAY");
                //    col8.CanUserSort = false;
                //    DtGriddy.Columns.Add(col8);

                //    recordPoint = DtGriddy.Columns.Count;//统计固定列的个数
                //    #endregion

                //    #region 动态添加薪资项目
                //    var tmp = its.FirstOrDefault().EMPLOYEESALARYRECORDITEMS.OrderBy(s => s.ORDERNUMBER).ToList();//取一条薪资记录的薪资项，动态创建薪资项列
                //    foreach (var it in tmp)
                //    {
                //        DataGridTextColumn txtCol = new DataGridTextColumn();
                //        txtCol.Header = it.SALARYITEMNAME;
                //        txtCol.Binding = new Binding("SUM");
                //        txtCol.Width = DataGridLength.SizeToCells;
                //        txtCol.MinWidth = 100;
                //        DtGriddy.Columns.Add(txtCol);
                //    }
                //    #endregion

                //    for (int i = 0; i < its.Count; i++)
                //    {
                //        if (Number == 0)
                //            Number = dataPager.PageSize * (dataPager.PageIndex - 1) + 1;
                //        else
                //            Number++;
                //        if (its[i].ORDINAL != "合计")
                //        {
                //            its[i].ORDINAL = Number.ToString();
                //        }
                //    }
                //    DtGriddy.ItemsSource = its;

                //    SpSalaryRecord.Loaded += new RoutedEventHandler(SpSalaryRecord_Loaded);
                //    SpSalaryRecord.Children.Add(DtGriddy);

                //    try
                //    {
                //        var ent = e.Result.FirstOrDefault() as V_salaryRecordDetailView;
                //        tbTotalMoney.Text = ent.PAYTOTALMONEY + " " + Utility.GetResourceStr("MONETARYUNIT");
                //        tbAvgMoney.Text = ent.AVGMONEY + " " + Utility.GetResourceStr("MONETARYUNIT");
                //        SalaryRecord = ent;

                //    }
                //    catch (Exception exc)
                //    {
                //        exc.Message.ToString();
                //        loadbar.Stop();
                //    }
                #endregion
                //}
                //else
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NODATA"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //    loadbar.Stop();
                //}

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        /// <summary>
        /// 加载记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DtGriddy_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var ent = e.Row.DataContext as CustomerDataObject;
            CheckBox cbk = DtGriddy.Columns[0].GetCellContent(e.Row).FindName("CheckBoxs") as CheckBox;
            if (cbk != null && ent != null)
            {
                if (ids.Contains(ent.GetFieldValue("EMPLOYEESALARYRECORDID").ToString()))
                {
                    cbk.IsChecked = true;
                }
                else
                {
                    cbk.IsChecked = false;
                }
            }

            TextBlock tborder = DtGriddy.Columns[1].GetCellContent(e.Row).FindName("tbNO") as TextBlock;
            string strName = ent.GetFieldValue("COMPANY").ToString();
            if (tborder != null)
            {
                tborder.Text = (e.Row.GetIndex() + 1).ToString();
                if (strName == "合计")
                {
                    tborder.Text = string.Empty;
                }
            }
        }

        public IEnumerable GetDataListForSalary(DataSetData data)
        {
            if (data == null)
                return null;
            if (data.Tables.Count == 0)
                return null;

            DataTableInfo tableInfo = data.Tables[0];

            System.Type dataType = DynamicDataBuilder.BuildDataObjectType(tableInfo.Columns, "MyDataObject");

            var listType = typeof(ObservableCollection<>).MakeGenericType(new[] { dataType });
            var list = Activator.CreateInstance(listType);

            XDocument xd = XDocument.Parse(data.DataXML);
            var table = from row in xd.Descendants(tableInfo.TableName)
                        select row.Elements().ToDictionary(r => r.Name, r => r.Value);
            recordCount = table == null ? 0 : table.Count();
            #region 取出合计值和平均值
            var table1 = from row in xd.Descendants("Table2")
                         select row.Elements().ToDictionary(r => r.Name, r => r.Value);
            foreach (var r in table1)
            {
                tbTotalMoney.Text = r["sum"];
                tbAvgMoney.Text = r["avg"];
            }
            #endregion
            foreach (var r in table)
            {
                var rowData = Activator.CreateInstance(dataType) as CustomerDataObject;
                if (rowData != null)
                {
                    foreach (DataColumnInfo col in tableInfo.Columns)
                    {
                        if (r.ContainsKey(col.ColumnName) && col.DataTypeName != typeof(System.Byte[]).FullName && col.DataTypeName != typeof(System.Guid).FullName)
                        {
                            if (col.IsEncrypt)
                            {
                                rowData.SetFieldValue(col.ColumnName, SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(r[col.ColumnName]), true);
                            }
                            else
                            {
                                rowData.SetFieldValue(col.ColumnName, r[col.ColumnName], true);
                            }
                        }
                    }
                }
                listType.GetMethod("Add").Invoke(list, new[] { rowData });
            }
            ObservableCollection<CustomerDataObject> l = list as ObservableCollection<CustomerDataObject>;
            return list as IEnumerable;
        }

        public void LoadData()
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.ShowProgressBar);
            string filter = "";
            int pageCount = 0;
            DateTime? starttimes = new DateTime(Convert.ToInt32(txtBalanceYear.Text), Convert.ToInt32(nudBalanceMonth.Value.ToString()), 1);
            DateTime? endtimes = new DateTime(Convert.ToInt32(txtBalanceYear.Text), Convert.ToInt32(nudBalanceMonth.Value.ToString()), DateTime.DaysInMonth(Convert.ToInt32(txtBalanceYear.Text), Convert.ToInt32(nudBalanceMonth.Value.ToString())));
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            if (!string.IsNullOrEmpty(monthlyBatchid) && !(SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString()))
            {
                filter += " MONTHLYBATCHID==@" + paras.Count().ToString();
                paras.Add(monthlyBatchid);
            }
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
            if (ObjectType == 3)
            {
                filter += " EMPLOYEESALARYRECORDID==@" + paras.Count().ToString();
                paras.Add(ObjectValue);
            }
            else
            {
                filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                paras.Add(ObjectValue);
            }

            //client.GetAuditSalaryRecordsPagingsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), ObjectType, ObjectValue, CheckState, userID);
            //client.GetAuditSalaryRecordsAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEESALARYRECORDID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), ObjectType, ObjectValue, CheckState, userID);
            client.GetAuditSalaryRecordsAsync(dataPager.PageIndex, dataPager.PageSize, "COMPANY", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), ObjectType, ObjectValue, CheckState, userID);
        }
        #endregion

        #region 操作批量审核的完成事件

        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (SalaryRecordBatch.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SUBMITLIMITATION"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }

            if (SalaryRecordBatch == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("ERROR_VDUSEAPP"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }

            if (lkdepart.DataContext == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARNING"), "请选择预算扣除部门！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }

            if (ids.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARNING"), "请确保已勾选待提交的薪资人员名单！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.IsEnabled = false;
            needsubmit = true;

            string oldCheckState = salaryRecordBatch.CHECKSTATE;

            if (ids.Count > 0)
            {
                if (oldCheckState == Utility.GetCheckState(CheckStates.UnSubmit))
                {
                    client.SalaryRecordBatchAddAsync(SalaryRecordBatch, ids);//提交审核
                }
            }

            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_UndoRepaymentMassCompleted(object sender, UndoRepaymentMassCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void client_SalaryRecordBatchUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESS"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                RefreshUI(RefreshedTypes.All);

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 获取批量审核的信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetSalaryRecordBatchByIDCompleted(object sender, GetSalaryRecordBatchByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {

                    SalaryRecordBatch = e.Result as T_HR_SALARYRECORDBATCH;
                    if (!string.IsNullOrEmpty(salaryRecordBatch.BALANCEOBJECTNAME))
                        cbxAuditType.Visibility = Visibility.Collapsed;
                    txtBalanceYear.Text = SalaryRecordBatch.BALANCEYEAR.ToString();
                    nuYear = SalaryRecordBatch.BALANCEYEAR.ToString();
                    nuStartmounth = SalaryRecordBatch.BALANCEMONTH.ToString();
                    nudBalanceMonth.Value = SalaryRecordBatch.BALANCEMONTH.ToDouble();
                    CheckState = SalaryRecordBatch.CHECKSTATE;
                    cbxkAssignedObjectType.SelectedIndex = Convert.ToInt32(SalaryRecordBatch.BALANCEOBJECTTYPE);
                    ObjectType = Convert.ToInt32(SalaryRecordBatch.BALANCEOBJECTTYPE);
                    ObjectValue = SalaryRecordBatch.BALANCEOBJECTID;
                    tbBalanceYearMonth.Text = nuYear + Utility.GetResourceStr("YEAR") + nuStartmounth.ToString() + Utility.GetResourceStr("MONTH");
                    monthlyBatchid = SalaryRecordBatch.MONTHLYBATCHID;
                    BindAssignObjectLookup();
                    LoadData();
                    this.DataContext = SalaryRecordBatch;
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void client_SalaryRecordBatchAddCompleted(object sender, SalaryRecordBatchAddCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (!e.Result)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    if (needsubmit == true)
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.ManualSubmit();
                        needsubmit = false;
                    }
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

                    RefreshUI(RefreshedTypes.All);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        #endregion

        #region 根据ID获取公司部门岗位
        void orgClient_GetPostByIdCompleted(object sender, GetPostByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                OrganizationWS.T_HR_POST post = e.Result as OrganizationWS.T_HR_POST;
                lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                lkAssignObject.DataContext = post;

                if (SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    if (post.POSTID != SalaryRecordBatch.OWNERPOSTID)
                    {
                        var temp = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.PostID == post.POSTID);
                        if (temp != null)
                        {
                            SalaryRecordBatch.OWNERCOMPANYID = temp.FirstOrDefault().CompanyID;
                            SalaryRecordBatch.OWNERDEPARTMENTID = temp.FirstOrDefault().DepartmentID;
                            SalaryRecordBatch.OWNERPOSTID = temp.FirstOrDefault().PostID;
                        }
                    }
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void orgClient_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                OrganizationWS.T_HR_DEPARTMENT depart = e.Result as OrganizationWS.T_HR_DEPARTMENT;
                lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                lkAssignObject.DataContext = depart;

                if (SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    if (depart.DEPARTMENTID != SalaryRecordBatch.OWNERDEPARTMENTID)
                    {
                        var temp = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.DepartmentID == depart.DEPARTMENTID);
                        if (temp != null)
                        {
                            SalaryRecordBatch.OWNERCOMPANYID = temp.FirstOrDefault().CompanyID;
                            SalaryRecordBatch.OWNERDEPARTMENTID = temp.FirstOrDefault().DepartmentID;
                            SalaryRecordBatch.OWNERPOSTID = temp.FirstOrDefault().PostID;
                        }
                    }
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void orgClient_GetCompanyByIdCompleted(object sender, GetCompanyByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                OrganizationWS.T_HR_COMPANY company = e.Result as OrganizationWS.T_HR_COMPANY;
                lkAssignObject.DisplayMemberPath = "CNAME";
                lkAssignObject.DataContext = company;

                if (SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    if (company.COMPANYID != SalaryRecordBatch.OWNERCOMPANYID)
                    {
                        var temp = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.CompanyID == company.COMPANYID);
                        if (temp != null)
                        {
                            SalaryRecordBatch.OWNERCOMPANYID = temp.FirstOrDefault().CompanyID;
                            SalaryRecordBatch.OWNERDEPARTMENTID = temp.FirstOrDefault().DepartmentID;
                            SalaryRecordBatch.OWNERPOSTID = temp.FirstOrDefault().PostID;
                        }
                    }
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYMASSAUDIT");
        }

        public string GetStatus()
        {
            return string.Empty;
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "9":
                    ExportData();
                    break;
                case "10":
                    closeTask();
                    break;
            }
            return;
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);
            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return toolbarItems;
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

        private void SetToolBar()
        {
            if (FormType == FormTypes.Audit)
                toolbarItems = Utility.CreateFormEditButton("T_HR_SALARYRECORDBATCH", SalaryRecordBatch.OWNERID,
                               SalaryRecordBatch.OWNERPOSTID, SalaryRecordBatch.OWNERDEPARTMENTID, SalaryRecordBatch.OWNERCOMPANYID);
            if (SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
            {
                toolbarItems.Clear();
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "9",
                    Title = Utility.GetResourceStr("BANKPAY"),// "保存",
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png"
                };
                toolbarItems.Add(item);
                ToolbarItem item1 = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "10",
                    Title = Utility.GetResourceStr("关闭待办"),// "保存",
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png"
                };
                toolbarItems.Add(item1);
            }
            RefreshUI(RefreshedTypes.All);
        }
        #endregion

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_SALARYRECORDBATCH Info)
        {

            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_SALARYRECORDBATCH", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_SALARYRECORDBATCH", "PAYTOTALMONEY", tbTotalMoney.Text, ""));
            AutoList.Add(basedata("T_HR_SALARYRECORDBATCH", "PAYAVERAGE", tbAvgMoney.Text, ""));
            AutoList.Add(basedata("T_HR_SALARYRECORDBATCH", "BALANCEOBJECTID", Info.BALANCEOBJECTID, lkAssignObject.TxtLookUp.Text));
            AutoList.Add(basedata("T_HR_SALARYRECORDBATCH", "PAYDATE", Info.BALANCEYEAR + Utility.GetResourceStr("YEAR") + Info.BALANCEMONTH + Utility.GetResourceStr("MONTH"), ""));

            if (Info.T_HR_EMPLOYEESALARYRECORD != null)
            {
                Info.T_HR_EMPLOYEESALARYRECORD.Clear();
            }

            string a = mx.TableToXml(Info, null, StrSource, AutoList);

            return a;
        }

        private AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }

        private AutoDictionary basedataForChild(string TableName, string Name, string Value, string Text, string keyValue)
        {
            string[] strlist = new string[5];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            strlist[4] = keyValue;
            AutoDictionary ad = new AutoDictionary();
            //ad.AutoDictionaryChiledList(strlist);//这里需要传递5个参数过去，keyvalue就是该表的主键ID
            return ad;
        }
        #endregion

        #region IAudit 成员
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            if (SalaryRecordBatch == null || SalaryRecordBatch.BALANCEOBJECTNAME == null)
            {
                SalaryRecordBatch.BALANCEOBJECTNAME = "0";
            }

            string strXmlObjectSource = string.Empty;
            entity.SystemCode = "HR";
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
            {
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, SalaryRecordBatch);
                //string tmp = CreateXML();
                //strXmlObjectSource = strXmlObjectSource.Replace("<ObjectDETAIL />", tmp);
                //strXmlObjectSource = strXmlObjectSource.Replace("<sum />", createSumXML());

            }

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", SalaryRecordBatch.OWNERID);//CreateUserID=UserID
            paraIDs.Add("CreatePostID", SalaryRecordBatch.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", SalaryRecordBatch.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", SalaryRecordBatch.OWNERCOMPANYID);
            paraIDs.Add("BALANCEOBJECTNAME", SalaryRecordBatch.BALANCEOBJECTNAME);

            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);

            if (SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                Utility.SetAuditEntity(entity, "T_HR_SALARYRECORDBATCH", SalaryRecordBatch.MONTHLYBATCHID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_SALARYRECORDBATCH", SalaryRecordBatch.MONTHLYBATCHID, strXmlObjectSource);
            }
        }

        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (ids.Count <= 0 && SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("没有选中任何数据"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (lkdepart.DataContext == null && SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("请选择预算扣除部门"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

            }
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            //string UserState = "Audit";
            string strEditState = Convert.ToInt32(EditStates.UnActived).ToString();
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    strEditState = Convert.ToInt32(EditStates.Actived).ToString();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    strEditState = Convert.ToInt32(EditStates.Canceled).ToString();
                    break;
            }

            if (SalaryRecordBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }

            SalaryRecordBatch.EDITSTATE = strEditState;
            SalaryRecordBatch.CHECKSTATE = state;

            RefreshUI(RefreshedTypes.HideProgressBar);

        }

        public string GetAuditState()
        {
            string state = "-1";
            if (SalaryRecordBatch != null)
                state = SalaryRecordBatch.CHECKSTATE;
            return state;
        }

        #endregion

        #region  发薪对象相关信息
        /// <summary>
        /// 发薪公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkAssignObject_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.All;
            lookup.TitleContent = Utility.GetResourceStr("ORGAN");
            lookup.SelectedClick += (obj, ev) =>
            {
                lkAssignObject.DataContext = lookup.SelectedObj;
                if (lookup.SelectedObj is OrganizationWS.T_HR_COMPANY)
                {
                    lkAssignObject.DisplayMemberPath = "CNAME";
                }
                else if (lookup.SelectedObj is OrganizationWS.T_HR_DEPARTMENT)
                {
                    lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                }
                else if (lookup.SelectedObj is OrganizationWS.T_HR_POST)
                {
                    lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                }
                else if (lookup.SelectedObj is OrganizationWS.T_HR_EMPLOYEE)
                {
                    lkAssignObject.DisplayMemberPath = "EMPLOYEECNAME";
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void BindAuditType()
        {
            try
            {
                if (!string.IsNullOrEmpty(salaryRecordBatch.BALANCEOBJECTNAME))
                {
                    cbxAuditType.SelectedIndex = Convert.ToInt32(salaryRecordBatch.BALANCEOBJECTNAME);
                    cbxAuditType.IsEnabled = false;
                }
                else
                    cbxAuditType.SelectedIndex = 0;
            }
            catch { }
        }

        private void BindAssignObjectLookup()
        {
            if (ObjectValue == null || ObjectType == -1)
            {
                lkAssignObject.DataContext = null;
                return;
            }
            switch (ObjectType + 1)
            {
                case 1:
                    orgClient.GetCompanyByIdAsync(ObjectValue);
                    break;

                case 2:
                    orgClient.GetDepartmentByIdAsync(ObjectValue);
                    break;

                case 3:
                    orgClient.GetPostByIdAsync(ObjectValue);
                    break;

                case 4:
                    OrganizationWS.T_HR_EMPLOYEE employee = new OrganizationWS.T_HR_EMPLOYEE();
                    employee.EMPLOYEEID = ObjectValue;
                    //employee.EMPLOYEECNAME = "";
                    lkAssignObject.DisplayMemberPath = "EMPLOYEECNAME";
                    lkAssignObject.DataContext = employee;
                    break;

                default:
                    lkAssignObject.DataContext = null;
                    break;
            }

        }

        protected void CheckObject()
        {
            if (!SetAssignFromLookup())
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("选择的类型不匹配"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private bool SetAssignFromLookup()
        {
            object obj = lkAssignObject.DataContext;
            if (obj == null)
            {
                return false;
            }

            if (obj is OrganizationWS.T_HR_COMPANY)
            {
                if (cbxkAssignedObjectType.SelectedIndex + 1 == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (obj is OrganizationWS.T_HR_DEPARTMENT)
            {
                if (cbxkAssignedObjectType.SelectedIndex + 1 == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (obj is OrganizationWS.T_HR_POST)
            {
                if (cbxkAssignedObjectType.SelectedIndex + 1 == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;

        }
        #endregion

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
            engineclient.DoClose();
            orgClient.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        #region checkbox选择
        private void CheckBoxs_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            CustomerDataObject v = DtGriddy.SelectedItem as CustomerDataObject;
            if (string.IsNullOrEmpty(v.GetFieldValue("EMPLOYEESALARYRECORDID").ToString()))
            {
                cb.IsChecked = false;
                return;
            }
            if (cb.IsChecked == true)
            {
                ids.Add(v.GetFieldValue("EMPLOYEESALARYRECORDID").ToString());
            }
            else
            {
                ids.Remove(v.GetFieldValue("EMPLOYEESALARYRECORDID").ToString());
            }

            CalculateTotalActuallyPay();
        }

        private void SelectCheckBoxAudit_Click(object sender, RoutedEventArgs e)
        {
            CheckBox CBAll = sender as CheckBox;
            if (CBAll == null)
            {
                return;
            }

            if (CBAll.IsChecked == false)
            {
                ids.Clear(); //清空全选的集合
            }

            //全选或取消全选（checkbox由于有滚动条的原因 不是一次性全选，而是分页选中，会导致有些漏选，需要在 loadingRow事件里补充）
            foreach (var entItem in DtGriddy.ItemsSource)
            {
                var ent = entItem as CustomerDataObject;
                CheckBox cbk = null;
                if (DtGriddy.Columns[0].GetCellContent(entItem) != null)
                {
                    cbk = DtGriddy.Columns[0].GetCellContent(entItem).FindName("CheckBoxs") as CheckBox;
                }

                if (CBAll.IsChecked == true)
                {
                    if (ent != null)
                    {
                        string recordID = ent.GetFieldValue("EMPLOYEESALARYRECORDID").ToString();
                        if (!string.IsNullOrEmpty(recordID)) //合计这一行的recordId为空值   不能选择合计
                        {
                            ids.Add(recordID);
                            if (cbk != null)
                            {
                                cbk.IsChecked = true;
                            }
                        }
                    }

                }
                else
                {
                    if (cbk != null)
                    {
                        cbk.IsChecked = false;
                    }
                }
            }

            CalculateTotalActuallyPay();
        }

        /// <summary>
        /// 计算选中人员的薪资总额
        /// </summary>
        /// <param name="ids"></param>
        private void CalculateTotalActuallyPay()
        {
            if (DtGriddy.ItemsSource != null)
            {
                tbTotalMoney.Text = string.Empty;
                tbAvgMoney.Text = string.Empty;
                var enu = DtGriddy.ItemsSource.GetEnumerator();
                decimal dActSum = 0, dSum = 0;
                iChooseCount = ids.Count();
                while (enu.MoveNext())
                {
                    var ent = enu.Current as CustomerDataObject;
                    string recordID = ent.GetFieldValue("EMPLOYEESALARYRECORDID").ToString();
                    decimal dActCur = 0, dCur = 0;

                    if (!ids.Contains(recordID))
                    {
                        continue;
                    }

                    object objActCur = ent.GetFieldValue("ACTUALLYPAY");
                    object objCur = ent.GetFieldValue("SALARYITEM6");

                    if (objActCur == null)
                    {
                        continue;
                    }

                    decimal.TryParse(objActCur.ToString().Replace("\0", ""), out dActCur);
                    decimal.TryParse(objCur.ToString().Replace("\0", ""), out dCur);

                    dActSum += dActCur;
                    dSum += dCur;
                }

                dChooseSum = dActSum;
                tbTotalMoney.Text = dActSum.ToString();

                if (iChooseCount > 0)
                {
                    tbAvgMoney.Text = decimal.Round((dActSum / iChooseCount), 2).ToString();

                    var etu = DtGriddy.ItemsSource.GetEnumerator();
                    while (etu.MoveNext())
                    {
                        var ent = etu.Current as CustomerDataObject;
                        string recordID = ent.GetFieldValue("EMPLOYEESALARYRECORDID").ToString();

                        if (string.IsNullOrWhiteSpace(recordID))
                        {
                            string strName = ent.GetFieldValue("COMPANY").ToString();
                            if (strName == "合计")
                            {
                                ent.SetFieldValue("ACTUALLYPAY", dActSum.ToString());
                                ent.SetFieldValue("SALARYITEM6", dSum.ToString());
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 选择预算扣除部门
        /// <summary>
        /// 选择预算扣除部门
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkdepart_FindClick(object sender, EventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT ent = lookup.SelectedObj[0].ObjectInstance as OrganizationWS.T_HR_DEPARTMENT;

                if (ent != null && dChooseSum > 0)
                {
                    lkdepart.DataContext = ent;
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    //client.FBStatisticsMassAsync(ObjectType, ObjectValue, nuYear.ToInt32(), nuStartmounth.ToInt32(), userID, ent.DEPARTMENTID); //查询预算服务(旧)停用
                    client.FBStatisticsMassByChooseAsync(ObjectType, ObjectValue, dChooseSum, nuYear.ToInt32(), nuStartmounth.ToInt32(), userID, ent.DEPARTMENTID);

                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请勾选要提交的人员，再选择预算扣除部门", Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 查询预算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_FBStatisticsMassByChooseCompleted(object sender, FBStatisticsMassByChooseCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result[0] == "false")
                {
                    string recordFB = e.Result[1];
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OVERBUDGET", recordFB), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    lkdepart.DataContext = null;

                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                lkdepart.DataContext = null;

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        #region 查询预算服务(旧)停用
        /// <summary>
        /// 查询预算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void client_FBStatisticsMassCompleted(object sender, FBStatisticsMassCompletedEventArgs e)
        //{

        //    if (e.Error == null)
        //    {
        //        if (e.Result[0] == "false")
        //        {
        //            string recordFB = e.Result[1];
        //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OVERBUDGET", recordFB), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //            lkdepart.DataContext = null;

        //        }
        //        else
        //        {
        //            // string employeesalaryrecordid = e.UserState as string;
        //        }
        //    }
        //    else
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //        lkdepart.DataContext = null;

        //    }
        //    RefreshUI(RefreshedTypes.HideProgressBar);
        //}
        #endregion 查询预算服务停用

        #endregion

        #region 创建传给流程的xml格式月薪审核子表
        /// <summary>
        /// 创建传给流程的xml格式月薪审核子表
        /// </summary>
        /// <returns></returns>
        string CreateXML()
        {

            #region
            //SalarySumList.Clear();
            //StringBuilder sb = new StringBuilder();
            //if (ids.Count <= 0)
            //{
            //    return "";
            //}
            //foreach (var ent in its)
            //{
            //    if (!ids.Contains(ent.EMPLOYEESALARYRECORDID))
            //    {
            //        continue;
            //    }
            //    if (ent.EMPLOYEENAME == "合计")
            //    {
            //        continue;
            //    }
            //    sb.Append("<Object Name=\"SALARYRECORDBATCHDETAIL\" LableResourceID=\"SALARYRECORDBATCHDETAIL\" Description=\"月薪批量审核\" Key=\"EMPLOYEESALARYRECORDID\" id=\"" + ent.EMPLOYEESALARYRECORDID + "\">");
            //    sb.Append("\r\n");
            //    var EmployeeInfo = its.Where(s => s.EMPLOYEESALARYRECORDID == ent.EMPLOYEESALARYRECORDID).FirstOrDefault();
            //    if (EmployeeInfo != null)
            //    {
            //        string tmp = "SALARYITEM1";
            //        sb.Append("<Attribute Name=\"");
            //        sb.Append(tmp + "\"");
            //        sb.Append(" LableResourceID=\"" + tmp + "\"  Description=\"");
            //        sb.Append("部门" + "\"");
            //        sb.Append(" DataType=\"string\"  DataValue=\"");
            //        sb.Append(EmployeeInfo.DEPARTMENT + "\"  DataText=\"\" /> ");
            //        sb.Append("\r\n");

            //        tmp = "SALARYITEM2";
            //        sb.Append("<Attribute Name=\"");
            //        sb.Append(tmp + "\"");
            //        sb.Append(" LableResourceID=\"" + tmp + "\"  Description=\"");
            //        sb.Append("岗位" + "\"");
            //        sb.Append(" DataType=\"string\"  DataValue=\"");
            //        sb.Append(EmployeeInfo.POST + "\"  DataText=\"\" /> ");
            //        sb.Append("\r\n");

            //        tmp = "SALARYITEM3";
            //        sb.Append("<Attribute Name=\"");
            //        sb.Append(tmp + "\"");
            //        sb.Append(" LableResourceID=\"" + tmp + "\"  Description=\"");
            //        sb.Append("员工姓名" + "\"");
            //        sb.Append(" DataType=\"string\"  DataValue=\"");
            //        sb.Append(EmployeeInfo.EMPLOYEENAME + "\"  DataText=\"\" /> ");
            //        sb.Append("\r\n");

            //        tmp = "SALARYITEM4";
            //        sb.Append("<Attribute Name=\"");
            //        sb.Append(tmp + "\"");
            //        sb.Append(" LableResourceID=\"" + tmp + "\"  Description=\"");
            //        sb.Append("职级代码" + "\"");
            //        sb.Append(" DataType=\"string\"  DataValue=\"");
            //        sb.Append(EmployeeInfo.POSTLEVELCODE + "\"  DataText=\"\" /> ");
            //        sb.Append("\r\n");

            //        tmp = "SALARYITEM5";
            //        sb.Append("<Attribute Name=\"");
            //        sb.Append(tmp + "\"");
            //        sb.Append(" LableResourceID=\"" + tmp + "\"  Description=\"");
            //        sb.Append("实发工资" + "\"");
            //        sb.Append(" DataType=\"string\"  DataValue=\"");
            //        sb.Append(SMT.SaaS.FrameworkUI.Common.Utility.AESEncrypt(EmployeeInfo.ACTUALLYPAY) + "\"  IsEncryption=\"True\"  DataText=\"\" /> ");
            //        sb.Append("\r\n");
            //        if (SalarySumList.ContainsKey("实发工资"))
            //        {
            //            SalarySumList["实发工资"] += string.IsNullOrEmpty(EmployeeInfo.ACTUALLYPAY) ? 0 : Convert.ToDecimal(EmployeeInfo.ACTUALLYPAY);
            //        }
            //        else
            //        {
            //            SalarySumList.Add("实发工资", string.IsNullOrEmpty(EmployeeInfo.ACTUALLYPAY) ? 0 : Convert.ToDecimal(EmployeeInfo.ACTUALLYPAY));
            //        }
            //    }
            //    int j = 6;
            //    foreach (var item in ent.EMPLOYEESALARYRECORDITEMS)
            //    {
            //        string tmp = "SALARYITEM" + j.ToString();
            //        item.SALARYITEMCODE = tmp;
            //        sb.Append("<Attribute Name=\"");
            //        sb.Append(tmp + "\"");
            //        sb.Append(" LableResourceID=\"" + tmp + "\"  Description=\"");
            //        sb.Append(item.SALARYITEMNAME + "\"");
            //        sb.Append(" DataType=\"string\"  DataValue=\"");
            //        sb.Append(SMT.SaaS.FrameworkUI.Common.Utility.AESEncrypt(item.SUM) + "\"  IsEncryption=\"True\"  DataText=\"\" /> ");
            //        sb.Append("\r\n");
            //        if (item.SALARYITEMNAME != "备注")
            //        {
            //            if (SalarySumList.ContainsKey(item.SALARYITEMNAME))
            //            {
            //                SalarySumList[item.SALARYITEMNAME] += string.IsNullOrEmpty(item.SUM) ? 0 : Convert.ToDecimal(item.SUM);
            //            }
            //            else
            //            {
            //                SalarySumList.Add(item.SALARYITEMNAME, string.IsNullOrEmpty(item.SUM) ? 0 : Convert.ToDecimal(item.SUM));
            //            }
            //        }
            //        j++;
            //    }
            //    sb.Append("</Object>");
            //}
            //return sb.ToString();
            #endregion
            dictSum = new Dictionary<string, decimal>();
            XDocument xd = XDocument.Parse(dataSet.DataXML);
            DataTableInfo tableInfo = dataSet.Tables[0];
            var table = from row in xd.Descendants(tableInfo.TableName)
                        select row.Elements().ToDictionary(r => r.Name, r => r.Value);
            StringBuilder sb = new StringBuilder();
            bool flag = true;
            foreach (var r in table)
            {
                foreach (DataColumnInfo col in tableInfo.Columns)
                {
                    if (r.ContainsKey(col.ColumnName) && col.IsRequired && col.IsKey) //是主键 
                    {
                        if (!ids.Contains(r[col.ColumnName])) //主键不在选中集合中跳过
                        {
                            flag = false;
                            break;
                        }
                        sb.Append("<Object Name=\"SALARYRECORDBATCHDETAIL\" LableResourceID=\"SALARYRECORDBATCHDETAIL\" Description=\"月薪批量审核\" Key=\"EMPLOYEESALARYRECORDID\" id=\"" + r[col.ColumnName] + "\">");
                        sb.Append("\r\n");
                    }
                    else if (r.ContainsKey(col.ColumnName) && col.IsRequired)
                    {
                        string IsEncryption = col.IsEncrypt ? "True" : "False";
                        sb.Append("<Attribute Name=\"");
                        sb.Append(col.ColumnName + "\"");
                        sb.Append(" LableResourceID=\"" + col.ColumnName + "\"  Description=\"");
                        sb.Append(col.ColumnTitle + "\"");
                        sb.Append(" DataType=\"string\"  DataValue=\"");
                        sb.Append(r[col.ColumnName] + "\" IsEncryption=\"" + IsEncryption + "\"  DataText=\"\" /> ");
                        sb.Append("\r\n");

                        //合计值
                        if (col.IsNeedSum)
                        {
                            string itemValue = SMT.SaaS.FrameworkUI.Common.Utility.AESDecrypt(r[col.ColumnName]);
                            if (dictSum.ContainsKey(col.ColumnName))
                            {
                                dictSum[col.ColumnName] += string.IsNullOrEmpty(itemValue) ? 0 : Convert.ToDecimal(itemValue);
                            }
                            else
                            {
                                dictSum.Add(col.ColumnName, string.IsNullOrEmpty(itemValue) ? 0 : Convert.ToDecimal(itemValue));
                            }
                        }
                    }

                }
                if (flag)
                {
                    sb.Append("</Object>");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 月薪审核子表合计值
        /// </summary>
        /// <returns></returns>
        string createSumXML()
        {

            DataTableInfo tableInfo = dataSet.Tables[0];
            StringBuilder sb = new StringBuilder();
            foreach (DataColumnInfo col in tableInfo.Columns)
            {

                if (dictSum.ContainsKey(col.ColumnName) && col.IsNeedSum)
                {
                    string IsEncryption = "False";
                    sb.Append("<Attribute Name=\"");
                    sb.Append(col.ColumnName + "\"");
                    sb.Append(" LableResourceID=\"" + col.ColumnName + "\"  Description=\"");
                    sb.Append(col.ColumnTitle + "\"");
                    sb.Append(" DataType=\"string\"  DataValue=\"");
                    sb.Append(dictSum[col.ColumnName] + "\" IsEncryption=\"" + IsEncryption + "\"  DataText=\"\" /> ");
                    sb.Append("\r\n");
                }

            }
            return sb.ToString();
        }
        #endregion
        #region 导出数据

        private void ExportData()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> parass = new System.Collections.ObjectModel.ObservableCollection<string>();
            filter += " MONTHLYBATCHID==@" + parass.Count().ToString();
            parass.Add(SalaryRecordBatch.MONTHLYBATCHID);

            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;
            result = dialog.ShowDialog();
            client.ExportSalaryExcelAsync("EMPLOYEENAME", filter, parass, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        }

        void client_ExportSalaryExcelCompleted(object sender, ExportSalaryExcelCompletedEventArgs e)
        {
            #region
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    byExport = e.Result;
                    using (System.IO.Stream stream = dialog.OpenFile())
                    {
                        stream.Write(byExport, 0, byExport.Length);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("BANKDATAEXPORT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        byExport = null;
                    }
                }
                else
                {
                    byExport = null;
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ISNOT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                byExport = null;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            #endregion
        }

        #endregion

        #region 关闭待办
        private void closeTask()
        {
            engineclient.TaskMsgCloseCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(engineclient_TaskMsgCloseCompleted);
            engineclient.TaskMsgCloseAsync("HR", SalaryRecordBatch.MONTHLYBATCHID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void engineclient_TaskMsgCloseCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("关闭待办成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }
        #endregion
    }
}
