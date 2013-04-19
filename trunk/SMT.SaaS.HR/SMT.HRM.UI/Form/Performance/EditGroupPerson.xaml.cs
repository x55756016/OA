/// <summary>
/// Log No.： 1
/// Modify Desc： 组织结构样式
/// Modifier： 冉龙军
/// Modify Date： 2010-08-31
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.Saas.Tools.PersonnelWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PerformanceWS;
using System.Windows.Media;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Performance
{
    public partial class EditGroupPerson : BaseForm, IEntityEditor
    {
        private PersonnelServiceClient client; //人员服务
        //   private OrganizationServiceClient orgClient;//机构服务
        private PerformanceServiceClient kpiClient;//绩效考核服务
        public FormTypes FormType { get; set; }//窗口状态
        public T_HR_RANDOMGROUP RandomGroup { get; set; }//当前抽查组信息

        public ObservableCollection<T_HR_RAMDONGROUPPERSON> groupPersonList = new ObservableCollection<T_HR_RAMDONGROUPPERSON>();//抽查组中的原始人员列表
        public ObservableCollection<T_HR_RAMDONGROUPPERSON> addList = new ObservableCollection<T_HR_RAMDONGROUPPERSON>();//需要添加到抽查组人员的添加列表
        public ObservableCollection<T_HR_RAMDONGROUPPERSON> delList = new ObservableCollection<T_HR_RAMDONGROUPPERSON>();//需要从抽查组人员中删除的删除列表

        public List<string> personIDList { get; set; }//抽查组中的原始人员ID列表
        public T_HR_RAMDONGROUPPERSON SelectedPerson { get; set; }//当前选择的抽查组人员的关联表实体

        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;//所有公司
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;//所有部门
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions; //所有岗位

        private bool isClose = false;
        SMTLoading loadbar = new SMTLoading();
        /// <summary>
        /// 构造EditGroupPerson
        /// </summary>
        /// <param name="type">窗口状态</param>
        /// <param name="randomGroup">抽查组实体</param>
        public EditGroupPerson(FormTypes type, T_HR_RANDOMGROUP randomGroup)
        {
            FormType = type;
            this.RandomGroup = randomGroup;
            InitializeComponent();
            InitPara();
        }

        #region 所有的方法

        /// <summary>
        /// 获取页面数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Employee_Loaded(object sender, RoutedEventArgs e)
        {
            kpiClient.GetRandomGroupPersonAllAsync(RandomGroup.RANDOMGROUPID);
            personIDList = new List<string>();
            //  LoadData();
            //  BindTree();

            //DtGrid_CurrentCellChanged(DtGrid, null);
        }

        /// <summary>
        /// 构造页面触发事件
        /// </summary>
        public void InitPara()
        {
            try
            {
                Parent.Children.Add(loadbar);
                loadbar.Stop();
                client = new PersonnelServiceClient();
                client.GetEmployeePagingCompleted += new EventHandler<GetEmployeePagingCompletedEventArgs>(client_GetEmployeePagingCompleted);

                kpiClient = new PerformanceServiceClient();
                kpiClient.GetRandomGroupPersonAllCompleted += new EventHandler<GetRandomGroupPersonAllCompletedEventArgs>(client_GetRandomGroupPersonAllCompleted);
                kpiClient.GetRandomGroupPersonByIDCompleted += new EventHandler<GetRandomGroupPersonByIDCompletedEventArgs>(client_GetRandomGroupPersonByIDCompleted);
                kpiClient.AddRandomGroupPersonListCompleted += new EventHandler<AddRandomGroupPersonListCompletedEventArgs>(client_AddRandomGroupPersonListCompleted);
                kpiClient.DeleteRandomGroupPersonsCompleted += new EventHandler<DeleteRandomGroupPersonsCompletedEventArgs>(client_DeleteRandomGroupPersonsCompleted);
                kpiClient.UpdateRandomGroupPersonListCompleted += new EventHandler<UpdateRandomGroupPersonListCompletedEventArgs>(client_UpdateRandomGroupPersonListCompleted);

                treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
                //orgClient = new OrganizationServiceClient();
                //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);

                DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);

                this.Loaded += new RoutedEventHandler(Employee_Loaded);
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 读取列表信息
        /// </summary>
        void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            TextBox txtEmpCode = Utility.FindChildControl<TextBox>(expander, "txtEmpCode");
            if (!string.IsNullOrEmpty(txtEmpCode.Text.Trim()))
            {
                // filter += "EMPLOYEECODE==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECODE)";
                paras.Add(txtEmpCode.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                //  filter += "EMPLOYEECNAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECNAME)";
                paras.Add(txtEmpName.Text.Trim());
            }

            string sType = "", sValue = "";
            sType = treeOrganization.sType;
            sValue = treeOrganization.sValue;
            client.GetEmployeePagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEECNAME",
                filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        /// <summary>
        /// 在内存中增加抽查组人员
        /// </summary>
        private void AddRandomGroupPerson()
        {
            //获取当前人员信息
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = DtGrid.SelectedItem as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
            //如果在删除列表中，则从删除列表中删除该记录
            if (delList.IndexOf(SelectedPerson) != -1)
            {
                delList.Remove(SelectedPerson);
            }
            //如果在抽查组人员列表中不存在，而且在添加列表中也不存在，则添加到内存中的添加列表中
            else if ((groupPersonList == null || groupPersonList.IndexOf(SelectedPerson) == -1) && addList.IndexOf(SelectedPerson) == -1)
            {
                addList.Add(SelectedPerson);
                //添加到已关联的ID列表中
                personIDList.Add(ent.EMPLOYEEID);
            }
        }

        /// <summary>
        /// 删除内存中的抽查组人员
        /// </summary>
        private void DeleteRandomGroupPerson()
        {
            //获取当前人员信息
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = DtGrid.SelectedItem as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
            //如果在添加列表中，则从添加列表中删除该记录
            if (addList.IndexOf(SelectedPerson) != -1)
            {
                addList.Remove(SelectedPerson);
                //从已关联的ID列表中删除
                personIDList.Remove(ent.EMPLOYEEID);
            }
            //如果在抽查组人员列表中存在，而且在删除列表中不存在，则添加到内存中的删除列表中
            else if (groupPersonList != null && groupPersonList.IndexOf(SelectedPerson) != -1 && delList.IndexOf(SelectedPerson) == -1)
            {
                delList.Add(SelectedPerson);
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
            ObservableCollection<string> delListId = new ObservableCollection<string>();
            foreach (T_HR_RAMDONGROUPPERSON ent in delList)
            {
                delListId.Add(ent.GROUPPERSONID);
            }
            if (addList.Count > 0 || delListId.Count > 0)
                kpiClient.UpdateRandomGroupPersonListAsync(addList, delListId);
        }

        /// <summary>
        /// 在三个T_HR_RAMDONGROUPPERSON的列表中，找出PERSONID为employeeId的实体
        /// </summary>
        /// <param name="p">personID</param>
        /// <returns></returns>
        private T_HR_RAMDONGROUPPERSON GetPersonFromList(string employeeId)
        {
            //总的List
            List<T_HR_RAMDONGROUPPERSON> list = new List<T_HR_RAMDONGROUPPERSON>();
            //三合一
            if (groupPersonList != null && groupPersonList.Count != 0)
                list.AddRange(groupPersonList);
            if (addList != null && addList.Count != 0)
                list.AddRange(addList);
            if (delList != null && delList.Count != 0)
                list.AddRange(delList);
            foreach (T_HR_RAMDONGROUPPERSON person in list)
            {
                if (person.PERSONID.Equals(employeeId))
                    return person;
            }
            //没有找到就新建
            T_HR_RAMDONGROUPPERSON ent = new T_HR_RAMDONGROUPPERSON();
            ent.GROUPPERSONID = Guid.NewGuid().ToString();
            ent.T_HR_RANDOMGROUP = this.RandomGroup;
            ent.PERSONID = employeeId;
            return ent;
        }

        #endregion 所有的方法

        #region 所有的事件
        /// <summary>
        /// 获取人员后触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeePagingCompleted(object sender, GetEmployeePagingCompletedEventArgs e)
        {
            List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> list = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE>();
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
                DtGrid.SelectedIndex = 0;
                DtGrid.SelectedItem = DtGrid.SelectedItem;
                //DtGrid.CurrentColumn = DtGrid.Columns[1]; 
            }
            loadbar.Stop();
        }

        /// <summary>
        /// 获取所有抽查组人员后触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetRandomGroupPersonAllCompleted(object sender, GetRandomGroupPersonAllCompletedEventArgs e)
        {
            groupPersonList = e.Result == null ? null : e.Result;
            personIDList.Clear();
            if (groupPersonList != null)
                foreach (T_HR_RAMDONGROUPPERSON groupPerson in groupPersonList)
                {
                    personIDList.Add(groupPerson.PERSONID);
                }
        }

        /// <summary>
        /// 获取抽查组人员后触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetRandomGroupPersonByIDCompleted(object sender, GetRandomGroupPersonByIDCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                SelectedPerson = new T_HR_RAMDONGROUPPERSON();
                SelectedPerson.GROUPPERSONID = Guid.NewGuid().ToString();
                SelectedPerson.T_HR_RANDOMGROUP = this.RandomGroup;
            }
            else
                SelectedPerson = e.Result;
        }

        /// <summary>
        /// 查询事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 点击列表的单元格改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DtGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee = (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)grid.SelectedItems[0];
                SelectedPerson = GetPersonFromList(employee.EMPLOYEEID);
                //kpiClient.GetRandomGroupPersonByIDAsync(employee.EMPLOYEEID);
            }
        }

        /// <summary>
        /// 读取每行的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //获取该行人员信息
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = e.Row.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
            //获取该行的CheckBox
            CheckBox ckh = DtGrid.Columns[0].GetCellContent(e.Row).FindName("chkMyChkBox") as CheckBox;
            //是否已经是抽查组人员
            if (personIDList != null && personIDList.IndexOf(ent.EMPLOYEEID) != -1)
            {
                ckh.IsChecked = true;
            }
            //增加CheckBox事件
            ckh.Click += new RoutedEventHandler(chkMyChkBox_Click);
        }

        /// <summary>
        /// 翻页条事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 每行的CheckBox事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkMyChkBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (DtGrid.ItemsSource != null)
            {
                //判断是在内存中增加还是删除
                if (chk.IsChecked.Value)
                    AddRandomGroupPerson();
                else
                    DeleteRandomGroupPerson();
            }

        }

        /// <summary>
        /// 添加完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_AddRandomGroupPersonListCompleted(object sender, AddRandomGroupPersonListCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.Error.Message == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                //将添加列表合并到抽查组列表
                foreach (T_HR_RAMDONGROUPPERSON ent in addList)
                {
                    groupPersonList.Add(ent);
                }
                //清楚添加列表
                addList.Clear();
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                else
                    RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 删除完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DeleteRandomGroupPersonsCompleted(object sender, DeleteRandomGroupPersonsCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //if (e.Error.Message == "Repetition")
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"));
                //}
                //else
                //{
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "POSTCODE,POSTNAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                // }
            }
            else
            {
                //从抽查组列表中删除删除列表的集合
                foreach (T_HR_RAMDONGROUPPERSON ent in delList)
                {
                    groupPersonList.Remove(ent);
                }
                //清楚删除列表
                delList.Clear();
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                else
                    RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 更新完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_UpdateRandomGroupPersonListCompleted(object sender, UpdateRandomGroupPersonListCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.Error.Message == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                //将添加列表合并到抽查组列表
                foreach (T_HR_RAMDONGROUPPERSON ent in addList)
                {
                    // 1s 冉龙军
                    // 暂不处理错误
                    // 1e
                    try
                    {
                        groupPersonList.Add(ent);
                    }
                    catch { }

                }
                //清楚添加列表
                addList.Clear();

                //从抽查组列表中删除删除列表的集合
                foreach (T_HR_RAMDONGROUPPERSON ent in delList)
                {
                    // 1s 冉龙军
                    // 暂不处理错误
                    // 1e
                    try
                    {
                        groupPersonList.Remove(ent);
                    }
                    catch { }

                }
                //清楚删除列表
                delList.Clear();
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED", "RANDOMGROUPPERSON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED", "RANDOMGROUPPERSON"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                else
                    RefreshUI(RefreshedTypes.All);
            }
        }

        #endregion 所有的事件



        #region IEntityEditor
        public string GetTitle()
        {
            // 1s 冉龙军
            //return Utility.GetResourceStr("RANDOMGROUP");
            return Utility.GetResourceStr("RANDOMGROUPPERSON");
            // 1s
        }
        public string GetStatus()
        {
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    isClose = false;
                    break;
                case "1":
                    isClose = true;
                    break;
            }
            Save();
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("RANDOMGROUPINFO"),
                Tooltip = Utility.GetResourceStr("RANDOMGROUPINFO")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType != FormTypes.Browse)
            {
                items = Utility.CreateFormSaveButton();
            }

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
    }
}
