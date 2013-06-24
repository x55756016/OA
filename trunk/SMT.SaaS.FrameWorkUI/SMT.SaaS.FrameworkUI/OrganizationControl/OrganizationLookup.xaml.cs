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


using SMT.SaaS.FrameworkUI.Helper;
using SMT.Saas.Tools.OrganizationWS;
using PersonnelWS = SMT.Saas.Tools.PersonnelWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Common;
using System.Collections.ObjectModel;

namespace SMT.SaaS.FrameworkUI.OrganizationControl
{
    public partial class OrganizationLookup : System.Windows.Controls.Window
    {

        public string ModelCode { get; set; }
        protected List<PersonnelWS.V_EMPLOYEEVIEW> SelectedEmployees = new List<PersonnelWS.V_EMPLOYEEVIEW>();
        protected ObservableCollection<PersonnelWS.T_HR_EMPLOYEEPOST> SelectedPersonnelEmloyeePostNew = new ObservableCollection<PersonnelWS.T_HR_EMPLOYEEPOST>();
        protected List<PersonnelWS.T_HR_DEPARTMENTDICTIONARY> SelectedDepartment = new List<PersonnelWS.T_HR_DEPARTMENTDICTIONARY>();
        protected List<PersonnelWS.T_HR_POSTDICTIONARY> SelectedPost = new List<PersonnelWS.T_HR_POSTDICTIONARY>();
        public List<ExtOrgObj> SelectedObj
        {
            get
            {
                List<ExtOrgObj> selObjs = new List<ExtOrgObj>();
                selObjs = orgTree.SelectedObj;
                if (SelectedEmployees.Count() > 0)
                {
                    foreach (PersonnelWS.V_EMPLOYEEVIEW item in SelectedEmployees)
                    {
                        ExtOrgObj obj = new ExtOrgObj();


                        /// <summary>
                        /// Modify Desc:下面的代码来之不易，由于存入数据类型问题，下面在认为比较简要的地方述说一下，
                        ///              而且也要注意传入值，这是目前发现用到的信息，以后有需要可继续加入
                        /// Modifier：xiedx
                        /// Modify Date：2012-8-8
                        /// </summary>
                        ExtOrgObj objCompany = new ExtOrgObj();
                        ExtOrgObj objDepartent = new ExtOrgObj();
                        ExtOrgObj objPost = new ExtOrgObj();

                        //公司信息
                        OrganizationWS.T_HR_COMPANY tempCompany = new T_HR_COMPANY();
                        tempCompany.COMPANYID = item.OWNERCOMPANYID;
                        tempCompany.CNAME = item.COMPANYNAME;

                        objCompany.ObjectInstance = tempCompany;
                        objCompany.ObjectID = item.OWNERCOMPANYID;
                        objCompany.ObjectName = item.COMPANYNAME;

                        //部门信息
                        OrganizationWS.T_HR_DEPARTMENT tempDepartent = new T_HR_DEPARTMENT();
                        OrganizationWS.T_HR_DEPARTMENTDICTIONARY listDepartent = new T_HR_DEPARTMENTDICTIONARY();
                        tempDepartent.DEPARTMENTID = item.OWNERDEPARTMENTID;
                        listDepartent.DEPARTMENTNAME = item.DEPARTMENTNAME;
                        tempDepartent.T_HR_COMPANY = tempCompany;

                        //这里简直就是个悲剧，
                        //ObjectType的ObjectInstance是根据不同数据表得到不同表对应的值，
                        //但是部门和岗位返回的是部门字典和岗位字典，所以要加上这句        
                        tempDepartent.T_HR_DEPARTMENTDICTIONARY = listDepartent;

                        objDepartent.ObjectInstance = tempDepartent;
                        objDepartent.ObjectID = item.OWNERDEPARTMENTID;
                        //把部门名字赋值给ObjectName
                        objDepartent.ObjectName = listDepartent.DEPARTMENTNAME;

                        //岗位信息
                        OrganizationWS.T_HR_POST tempPost = new T_HR_POST();
                        OrganizationWS.T_HR_POSTDICTIONARY listPost = new OrganizationWS.T_HR_POSTDICTIONARY();
                        listPost.POSTNAME = item.POSTNAME;
                        tempPost.T_HR_POSTDICTIONARY = listPost;
                        tempPost.POSTID = item.OWNERPOSTID;
                        tempPost.POSTLEVEL = item.POSTLEVEL;

                        objPost.ObjectInstance = tempPost;
                        objPost.ObjectID = item.OWNERPOSTID;
                        objPost.ObjectName = listPost.POSTNAME;

                        PersonnelWS.T_HR_POST tempPerWSPost = new PersonnelWS.T_HR_POST();
                        tempPerWSPost.POSTLEVEL = item.POSTLEVEL;

                        //员工信息
                        PersonnelWS.T_HR_EMPLOYEE temp = new PersonnelWS.T_HR_EMPLOYEE();
                        temp.EMPLOYEEID = item.EMPLOYEEID;
                        temp.EMPLOYEECNAME = item.EMPLOYEECNAME;
                        temp.EMPLOYEECODE = item.EMPLOYEECODE;
                        temp.SEX = item.SEX;
                        temp.IDNUMBER = item.IDNUMBER;
                        temp.OWNERCOMPANYID = item.OWNERCOMPANYID;
                        temp.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                        temp.OWNERPOSTID = item.OWNERPOSTID;
                        temp.MOBILE = item.MOBILE;
                        temp.OFFICEPHONE = item.OFFICEPHONE;

                        //这里用到ObservableCollection类，注意转换
                        temp.T_HR_EMPLOYEEPOST = SelectedPersonnelEmloyeePostNew;

                        obj.ObjectInstance = temp;
                        //这里的各种ParentObject是各个的ParentObject，例如员工的ParentObject是post，
                        //post的ParentObject又是objDepartent，是从公司，部门，岗位再到员工一层一层下来，
                        //所以这里ParentObject得注意
                        obj.ParentObject = objPost;
                        objPost.ParentObject = objDepartent;
                        objDepartent.ParentObject = objCompany;

                        selObjs.Add(obj);
                    }
                }
                return selObjs;
            }
        }
        SMTLoading loadbar = new SMTLoading();

        public event EventHandler SelectedClick;

        public OrgTreeItemTypes SelectedObjType
        {
            get
            {
                return orgTree.SelectedObjType;
            }
            set
            {
                orgTree.SelectedObjType = value;
            }

        }
        public bool SelectSameGradeOnly
        {
            get
            {
                return orgTree.SelectSameGradeOnly;
            }
            set
            {
                orgTree.SelectSameGradeOnly = value;
            }

        }
        public bool MultiSelected;

        public string CurrentUserID
        {
            get
            {
                return orgTree.CurrentUserID;
            }
            set
            {
                orgTree.CurrentUserID = value;
            }

        }

        public string Perm
        {
            get
            {
                return orgTree.Perm;
            }
            set
            {
                orgTree.Perm = value;
            }

        }
        public string Entity
        {
            get
            {
                return orgTree.Entity;
            }
            set
            {
                orgTree.Entity = value;
            }

        }

        public OrganizationLookup()
        {
            InitializeComponent();
            this.organizationHistory.Content = "组织架构历史";
        }

        public OrganizationLookup(string userID, string perm, string entity)
            : this()
        {

            this.CurrentUserID = userID;
            this.Perm = perm;
            this.Entity = entity;
        }

        public OrganizationLookup(string perm, string entity)
            : this()
        {
            this.Perm = perm;
            this.Entity = entity;
        }

        public void ShowMessageForSelectOrganization()
        {
            tbSelectFlag.Text = "机构只可选择一个公司或一个部门或一个岗位";
            if (MultiSelected)
            {
                tbSelectFlag.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 确认组织架构选择，并关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedObjType != OrgTreeItemTypes.All && SelectedObjType != OrgTreeItemTypes.Personnel)
            {
                string errMsg = orgTree.ValidSelection();

                if (!string.IsNullOrEmpty(errMsg))
                {
                    //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), errMsg, Utility.GetResourceStr("CONFIRMBUTTON"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), errMsg,
                     Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
            }

            /// <summary>
            /// Modify Desc：判断组织架构选人的时候出现单选模式多选人，或是没有选人的情况
            ///              就这两句代码基本搞了半天多,写的不好，希望有时间可以改正
            ///              而且加了个try-catch，阻止了一些错误
            /// Modify Date：2012-8-2
            /// </summary>
            try
            {
                if (this.SelectedClick != null)
                {


                    if (this.MultiSelected == false)
                    {
                        if (this.SelectedObj.Count >= 2)
                        {

                            Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "单选模式只支持单选");
                            return;
                        }
                        if (this.SelectedObj.Count <= 0)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "请确保选择一位员工");
                            return;
                        }
                        this.SelectedClick(this, null);
                        this.Close();
                    }
                    if (this.MultiSelected == true)
                    {
                        if (this.SelectedObj.Count <= 0)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "请确保选择一位员工");
                            return;
                        }
                        this.SelectedClick(this, null);
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "数据存入缓存错误，可关掉重新选择："+ex.ToString());
            }
        }

        /// <summary>
        /// 取消组织架构选择，并关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 刷新，重新加载组织架构
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.Remove("ORGTREESYSCompanyInfo");
            Application.Current.Resources.Remove("ORGTREESYSDepartmentInfo");
            Application.Current.Resources.Remove("ORGTREESYSPostInfo");

            Application.Current.Resources.Remove("SYS_CompanyInfo");
            Application.Current.Resources.Remove("SYS_DepartmentInfo");
            Application.Current.Resources.Remove("SYS_PostInfo");

            orgTree.postIDsCach.Clear();
            orgTree.depIDsCach.Clear();
            orgTree.BindTree();
        }

        /// <summary>
        /// 模糊查询员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            if(!PARENT.Children.Contains(loadbar))
            {
                PARENT.Children.Add(loadbar);
            }
            loadbar.Start();
            //loadbar.Stop();
            //清空
           // dgEmployeeList.ItemsSource = null;
            SelectedEmployees.Clear();
            //输入为空则不能查询
            if (string.IsNullOrEmpty(txtEmpName.Text.Trim()))
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "请输入查询员工姓名");
                return;
            }
            if (SelectedObjType != OrgTreeItemTypes.All && SelectedObjType != OrgTreeItemTypes.Personnel)
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "当前模式只支持选择机构，不能查询员工");
                return;
            }

            BindEmployeeList();
        }

        /// <summary>
        /// 绑定员工列表
        /// </summary>
        private void BindEmployeeList()
        {
            int pageSize = 0, pageIndex = 0, pageCount = 0;
            string filter = string.Empty, strMsg = string.Empty;
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECNAME)";
                paras.Add(txtEmpName.Text.Trim());
            }

            string sType = "", sValue = "";
            //2012-9-13
            //不需要选择机构就可进行查询，于是注释
          //  GetOrgInfoByChecked(ref sType, ref sValue, ref strMsg);

            if (!string.IsNullOrWhiteSpace(strMsg))
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), strMsg);
                return;
            }

            //不分页
            pageIndex = 1;
            pageSize = 999999;

            PersonnelWS.PersonnelServiceClient clientPers = new PersonnelWS.PersonnelServiceClient();
            Employeestate statetmp = cbxEmployeeState.SelectedItem as Employeestate;
            if (statetmp != null)
            {
                if (statetmp.Value == "1")
                {
                    clientPers.GetLeaveEmployeeViewsPagingCompleted += new EventHandler<PersonnelWS.GetLeaveEmployeeViewsPagingCompletedEventArgs>(clientPers_GetLeaveEmployeeViewsPagingCompleted);
                    
                    clientPers.GetLeaveEmployeeViewsPagingAsync(pageIndex, pageSize, "EMPLOYEECNAME",
                filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                }
                else
                {
                    clientPers.GetEmployeeViewsPagingCompleted += new EventHandler<PersonnelWS.GetEmployeeViewsPagingCompletedEventArgs>(clientPers_GetEmployeeViewsPagingCompleted);
                    clientPers.GetEmployeeViewsPagingAsync(pageIndex, pageSize, "EMPLOYEECNAME",
                        filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                }
            }
            else
            {
                clientPers.GetEmployeeViewsPagingCompleted += new EventHandler<PersonnelWS.GetEmployeeViewsPagingCompletedEventArgs>(clientPers_GetEmployeeViewsPagingCompleted);
                clientPers.GetEmployeeViewsPagingAsync(pageIndex, pageSize, "EMPLOYEECNAME",
                    filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        private void GetOrgInfoByChecked(ref string sType, ref string sValue, ref string strMsg)
        {
            List<ExtOrgObj> selObjs = new List<ExtOrgObj>();
            foreach (TreeViewItem item in orgTree.treeOrganization.Items)
            {

                TreeViewItem myItem =
                    (TreeViewItem)(orgTree.treeOrganization.ItemContainerGenerator.ContainerFromItem(item));
                myItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

                CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);
                if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
                {
                    OrgTreeItemTypes type = (OrgTreeItemTypes)(item.Tag);

                    selObjs.Add(item.DataContext as ExtOrgObj);
                }

                GetChildSelectedOrgObj(item, ref selObjs);
            }

            if (selObjs.Count() == 0)
            {
                strMsg = "请选择员工所在的公司再进行查询";
                return;
            }

            if (selObjs.Count() > 1)
            {
                strMsg = "查询员工，只能选择单个机构做为范围查询";
                return;
            }

            ExtOrgObj selobj = selObjs.FirstOrDefault();
            sType = selobj.ObjectType.ToString();
            sValue = selobj.ObjectID;
        }

        private void GetChildSelectedOrgObj(TreeViewItem item, ref List<ExtOrgObj> selObjs)
        {
            foreach (TreeViewItem childItem in item.Items)
            {
                TreeViewItem myItem =
                (TreeViewItem)(item.ItemContainerGenerator.ContainerFromItem(childItem));

                CheckBox cbx = Helper.UIHelper.FindChildControl<CheckBox>(myItem);

                if (cbx != null && cbx.IsChecked.GetValueOrDefault(false))
                {
                    OrgTreeItemTypes type = (OrgTreeItemTypes)(childItem.Tag);


                    if (type == OrgTreeItemTypes.Post)
                    {
                        ExtOrgObj obj = childItem.DataContext as ExtOrgObj;
                        ExtOrgObj pobj = item.DataContext as ExtOrgObj;
                        if (obj != null && pobj != null)
                        {
                            //附父节点的值
                            ((T_HR_POST)obj.ObjectInstance).T_HR_DEPARTMENT = pobj.ObjectInstance as T_HR_DEPARTMENT;
                        }
                    }

                    selObjs.Add(childItem.DataContext as ExtOrgObj);
                }

                GetChildSelectedOrgObj(childItem, ref selObjs);
            }
        }

        /// <summary>
        /// 获取在职员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientPers_GetEmployeeViewsPagingCompleted(object sender, PersonnelWS.GetEmployeeViewsPagingCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error == null)
            {
                dgEmployeeList.ItemsSource = e.Result;
            }
        }

        /// <summary>
        /// 获取离职员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientPers_GetLeaveEmployeeViewsPagingCompleted(object sender, PersonnelWS.GetLeaveEmployeeViewsPagingCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error == null)
            {
                dgEmployeeList.ItemsSource = e.Result;
            }
        }

        private void cbxEmployeeState_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            cbx.DisplayMemberPath = "Name";
            cbx.ItemsSource = new List<Employeestate>()
            {       
                new Employeestate(){ Name="在职", Value="0"},        
                new Employeestate(){ Name="离职", Value="1"}         
            };
            cbx.SelectedIndex = 0;
        }

        public class Employeestate
        {
            private string name;
            private string value;
            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            public string Value
            {
                get { return this.value; }
                set { this.value = value; }
            }
        }

        private void dgEmployeeList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //获取该行人员信息
            var ent = e.Row.DataContext as PersonnelWS.V_EMPLOYEEVIEW;
            //获取该行的CheckBox
            CheckBox ckh = dgEmployeeList.Columns[0].GetCellContent(e.Row).FindName("chkMyChkBox") as CheckBox;

            if (SelectedEmployees.Count > 0)
            {
                var tmps = from n in SelectedEmployees
                           where n.EMPLOYEEID == ent.EMPLOYEEID
                           select n;

                if (tmps != null)
                {
                    if (tmps.Count() > 0)
                    {
                        ckh.IsChecked = true;
                    }
                }
            }
            //增加CheckBox事件
            ckh.Click += new RoutedEventHandler(ckh_Click);
        }

        void ckh_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            var ent = dgEmployeeList.SelectedItem as PersonnelWS.V_EMPLOYEEVIEW;

            var tmps = from n in SelectedEmployees
                       where n.EMPLOYEEID == ent.EMPLOYEEID
                       select n;

            //这里判断了，但是可能会导致存入不了相同EMPLOYEEID的数据，例如一个员工有两个岗位，
            //那么后面就会只有一条数据，所以注释掉
            //bool bIsNotExists = false;
            //if (tmps != null)
            //{
            //    if (tmps.Count() > 0)
            //    {
            //        bIsNotExists = true;
            //    }
            //}

            //PersonnelWS.T_HR_EMPLOYEE temp = new PersonnelWS.T_HR_EMPLOYEE();
            //temp.EMPLOYEEID = ent.EMPLOYEEID;
            //temp.EMPLOYEECNAME = ent.EMPLOYEECNAME;
            //temp.EMPLOYEECODE = ent.EMPLOYEECODE;
            //temp.SEX = ent.SEX;
            //temp.IDNUMBER = ent.IDNUMBER;
            //temp.OWNERCOMPANYID = ent.OWNERCOMPANYID;
            //temp.OWNERDEPARTMENTID = ent.OWNERDEPARTMENTID;
            //temp.OWNERPOSTID = ent.OWNERPOSTID;


            /// <summary>
            /// Modify Desc:在信息的处理中，在初始值中不能加入列表选择信息，所以在checkbox选择中
            ///             进行数据加入和移除，然后传入到初始值中进行初始化，而且也要注意不同表单
            ///             要传入的值可能不一样，这是目前发现用到的信息，以后有需要可继续加入
            /// Modify Date：2012-8-8

            //信息存入视图
            PersonnelWS.V_EMPLOYEEVIEW temp = new PersonnelWS.V_EMPLOYEEVIEW();
            temp.EMPLOYEEID = ent.EMPLOYEEID;
            temp.EMPLOYEECNAME = ent.EMPLOYEECNAME;
            temp.EMPLOYEECODE = ent.EMPLOYEECODE;
            temp.SEX = ent.SEX;
            temp.IDNUMBER = ent.IDNUMBER;
            temp.OWNERCOMPANYID = ent.OWNERCOMPANYID;
            temp.OWNERDEPARTMENTID = ent.OWNERDEPARTMENTID;
            temp.OWNERPOSTID = ent.OWNERPOSTID;
            temp.ISAGENCY = ent.ISAGENCY;
            temp.POSTLEVEL = ent.POSTLEVEL;
            temp.EMPLOYEEPOSTID = ent.EMPLOYEEPOSTID;
            temp.COMPANYNAME = ent.COMPANYNAME;
            temp.DEPARTMENTNAME = ent.DEPARTMENTNAME;
            temp.POSTNAME = ent.POSTNAME;
            temp.MOBILE = ent.MOBILE;
            temp.OFFICEPHONE = ent.OFFICEPHONE;

            //存入数据到V_EMPLOYEEVIEW视图和部门,岗位表
            PersonnelWS.V_EMPLOYEEVIEW vTemp = new PersonnelWS.V_EMPLOYEEVIEW();
            vTemp.EMPLOYEEID = ent.EMPLOYEEID;
            vTemp.EMPLOYEECNAME = ent.EMPLOYEECNAME;
            vTemp.EMPLOYEECODE = ent.EMPLOYEECODE;
            vTemp.SEX = ent.SEX;
            vTemp.IDNUMBER = ent.IDNUMBER;
            vTemp.OWNERCOMPANYID = ent.OWNERCOMPANYID;
            vTemp.OWNERDEPARTMENTID = ent.OWNERDEPARTMENTID;
            vTemp.OWNERPOSTID = ent.OWNERPOSTID;
            vTemp.DEPARTMENTNAME = ent.DEPARTMENTNAME;
            vTemp.POSTNAME = ent.POSTNAME;
            vTemp.POSTLEVEL = ent.POSTLEVEL;
            vTemp.ISAGENCY = ent.ISAGENCY;
            vTemp.EMPLOYEEPOSTID = ent.EMPLOYEEPOSTID;
            vTemp.MOBILE = ent.MOBILE;
            vTemp.OFFICEPHONE = ent.OFFICEPHONE;

            //信息存入PersonnelWS.T_HR_POSTDICTIONARY里
            PersonnelWS.T_HR_POSTDICTIONARY postDicTemp = new PersonnelWS.T_HR_POSTDICTIONARY();
            postDicTemp.POSTNAME = ent.POSTNAME;
            postDicTemp.POSTLEVEL = ent.POSTLEVEL;

            //信息存入PersonnelWS.T_HR_DEPARTMENTDICTIONARY里
            PersonnelWS.T_HR_DEPARTMENTDICTIONARY departmentTemp = new PersonnelWS.T_HR_DEPARTMENTDICTIONARY();
            departmentTemp.DEPARTMENTNAME = ent.DEPARTMENTNAME;


            //把信息存入PersonnelWS.T_HR_POST，然后再把PersonnelWS.T_HR_POST数据存入PersonnelWS.T_HR_EMPLOYEEPOST
            //最后把数据存入PersonnelWS.T_HR_EMPLOYEEPOST
            //注意他们之间数据类型转换
            PersonnelWS.T_HR_POST tempPerWSPost = new PersonnelWS.T_HR_POST();
            tempPerWSPost.POSTLEVEL = ent.POSTLEVEL;
            tempPerWSPost.POSTID = ent.OWNERPOSTID;


            PersonnelWS.T_HR_EMPLOYEE tempEmployee = new PersonnelWS.T_HR_EMPLOYEE();
            tempEmployee.EMPLOYEECNAME = ent.EMPLOYEECNAME;
            tempEmployee.EMPLOYEEID = ent.EMPLOYEEID;
            tempEmployee.EMPLOYEEENAME = ent.EMPLOYEEENAME;
            tempEmployee.EMPLOYEECODE = ent.EMPLOYEECODE;

            //把PersonnelWS.T_HR_EMPLOYEEPOST信息存入，有些表单可能会用到
            PersonnelWS.T_HR_EMPLOYEEPOST tempPerWSEmployeePost = new PersonnelWS.T_HR_EMPLOYEEPOST();
            tempPerWSEmployeePost.EMPLOYEEPOSTID = ent.EMPLOYEEPOSTID;
            tempPerWSEmployeePost.ISAGENCY = ent.ISAGENCY;
            tempPerWSEmployeePost.POSTLEVEL = ent.POSTLEVEL;
            tempPerWSEmployeePost.T_HR_POST = tempPerWSPost;
            tempPerWSEmployeePost.T_HR_EMPLOYEE = tempEmployee;



            try
            {
                if (chk.IsChecked.Value)
                {
                    //if (!bIsNotExists)
                    //{
                    //单多选进行判断，这段代码目前是为了解决员工考勤方案分配时，
                    //同一员工的考勤分配，一个人有两个岗位情况，那么只算一个考勤分配
                    //到底分配什么考勤方案，那就看情况，目前还不懂考勤分配
                    if (this.MultiSelected == true)
                    {
                        if (SelectedEmployees.Count() == 0)
                        {
                            SelectedDepartment.Add(departmentTemp);
                            SelectedPost.Add(postDicTemp);
                            SelectedEmployees.Add(temp);
                            SelectedPersonnelEmloyeePostNew.Add(tempPerWSEmployeePost);
                        }
                        else
                        {

                            for (int i = 0; i < SelectedEmployees.Count(); i++)
                            {
                                //岗位一样则只算一个，这个地方想了好久的逻辑
                                if (SelectedEmployees[i].EMPLOYEEID == temp.EMPLOYEEID && SelectedEmployees[i].EMPLOYEECNAME==temp.EMPLOYEECNAME)
                                {
                                    break;
                                }
                                if (i == SelectedEmployees.Count() - 1)
                                {
                                    //加入字典信息这两个没有用到，写着为了测试盒调试使用，以后可能有用
                                    SelectedDepartment.Add(departmentTemp);
                                    SelectedPost.Add(postDicTemp);
                                    SelectedEmployees.Add(temp);
                                    SelectedPersonnelEmloyeePostNew.Add(tempPerWSEmployeePost);
                                }
                               
                            }
                        }
                    }
                    if (this.MultiSelected == false)
                    {
                        SelectedDepartment.Add(departmentTemp);
                        SelectedPost.Add(postDicTemp);
                        SelectedEmployees.Add(temp);
                        SelectedPersonnelEmloyeePostNew.Add(tempPerWSEmployeePost);
                    }
                    //}
                }
                else
                {
                    //这里显然是个悲剧，原始的移除根本不起作用，所以在这加个判断，
                    //如果信息一致就移除相应序号的信息，考虑到一个人可能有几个岗位
                    //所以这里不仅要判断员工ID还要判断部门ID和岗位ID，我就不信这人
                    //在一个部门里面还有两个岗位，如果有那他可能就是程序员兼客服兼测试……
                    //那就没办法了
                    for (int i = 0; i < SelectedEmployees.Count(); i++)
                    {
                        if ((SelectedEmployees[i].EMPLOYEEID == temp.EMPLOYEEID) && (SelectedEmployees[i].OWNERDEPARTMENTID == temp.OWNERDEPARTMENTID) && (SelectedEmployees[i].OWNERPOSTID == temp.OWNERPOSTID))
                        {
                            SelectedDepartment.Remove(departmentTemp);
                            SelectedPost.Remove(postDicTemp);
                            SelectedEmployees.RemoveAt(i);
                            SelectedPersonnelEmloyeePostNew.RemoveAt(i);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "数据存入缓存错误，可关掉重新选择");
            }
            //if (chk.IsChecked.Value)
            //{
            //    if (!bIsNotExists)
            //    {
            //        SelectedEmployees.Add(temp);
            //    }
            //}
            //else
            //{
            //    if (bIsNotExists)
            //    {
            //        SelectedEmployees.Remove(temp);
            //    }
            //}
        }

        private void organizationHistory_Checked(object sender, RoutedEventArgs e)
        {
            OrganizationTree.IsOrganizationHistory = true;
            orgTree.BindTree();
        }

        private void organizationHistory_Unchecked(object sender, RoutedEventArgs e)
        {
            OrganizationTree.IsOrganizationHistory = false;
            orgTree.BindTree();
        }

    }
}

