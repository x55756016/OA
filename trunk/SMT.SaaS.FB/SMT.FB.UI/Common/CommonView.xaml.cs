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
using SMT.FB.UI.Form;

using SMT.FB.UI.FBCommonWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.Platform;
using SMT.FB.UI.Views;
using System.ComponentModel;
using SMT.FB.UI.Views.Query;
using SMT.SaaS.FrameworkUI.OrganizationControl;

namespace SMT.FB.UI.Common
{
    public partial class CommonView : FBBaseControl, IWebPart
    {
        private ComboBox cbbOrderStatus;
        private ComboBox cbbQueryList;
        public CommonView()
        {
            InitializeComponent();
        }


        public CommonView(OrderEntity orderEntity)
            : this()
        {
            this.DefaultEntity = orderEntity;
            InitForm();
        }
        public OrderEntity DefaultEntity { get; set; }

        public OrderEntityService OrderSource { get; set; }

        private IList<OrderEntity> OrderEntities { get; set; }

        public void InitForm()
        {

            OrderInfo order = DefaultEntity.OrderInfo;
            OrderSource = new OrderEntityService();
            OrderSource.ModelCode = this.DefaultEntity.OrderType.Name;
            this.FormTitleName.TextTitle.Text = order.Name;

            this.FormTitleName.Visibility = FBBasePage.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;
            InitQueryFilter();
            InitToolBar(order);
            InitData(order);
            InitControl(order);

        }

        private void InitQueryFilter()
        {
            MultiValuesItem<ExtOrgObj> item = new MultiValuesItem<ExtOrgObj>();
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ep = new Saas.Tools.PersonnelWS.T_HR_EMPLOYEE();
            ep.EMPLOYEECNAME = DataCore.CurrentUser.Text;
            ep.EMPLOYEEID = DataCore.CurrentUser.Value.ToString();

            List<ExtOrgObj> list = new List<ExtOrgObj>() { new ExtOrgObj { ObjectInstance = ep } };
            item.Values = list;
            item.Text = ep.EMPLOYEECNAME;
            lkObject.SelectItem = item;
            lkObject.DataContext = item;
            lkObject.DisplayMemberPath = "Text";

            DateTime dtSystem = DataCore.SystemSetting.UPDATEDATE.Value;
            dtSystem = dtSystem.AddDays(1 - dtSystem.Day);

            // 开始日期
            //  DatePicker dpStart = this.expander.FindChildControl<DatePicker>("dpStart");
            dpStart.SelectedDate = dtSystem;

            // 结束日期
            //  DatePicker dpEnd = this.expander.FindChildControl<DatePicker>("dpEnd");
            dpEnd.SelectedDate = dtSystem.AddMonths(1).AddSeconds(-1);
        }

        private void InitToolBar(OrderInfo order)
        {
            // PermissionHelper.DisplayGridToolBarButton(this.tooBarTop, order.Type, true);
            Utility.DisplayGridToolBarButton(tooBarTop, order.Type, true);
            tooBarTop.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            tooBarTop.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            tooBarTop.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            tooBarTop.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            tooBarTop.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            tooBarTop.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            tooBarTop.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            cbbOrderStatus = tooBarTop.cbxCheckState;
            cbbQueryList = tooBarTop.cbbQueryList;
            this.tooBarTop.ShowView(false);

            if (this.DefaultEntity.OrderType == typeof(T_FB_SUMSETTINGSMASTER))
            {
                this.tooBarTop.stpCheckState.Visibility = Visibility.Collapsed;
                this.tooBarTop.btnAudit.Visibility = Visibility.Collapsed;
            }

            //if (this.DefaultEntity.OrderType == typeof(T_FB_CHARGEAPPLYMASTER) ||
            //        this.DefaultEntity.OrderType == typeof(T_FB_BORROWAPPLYMASTER) ||
            //            this.DefaultEntity.OrderType == typeof(T_FB_REPAYAPPLYMASTER))
            //{
            //    if (tooBarTop.btnNew.Visibility == System.Windows.Visibility.Visible)
            //    {
            //        tooBarTop.btnNew.Visibility = Visibility.Collapsed;
            //        tooBarTop.retNew.Visibility = Visibility.Collapsed;
            //    }

            //    if (tooBarTop.btnEdit.Visibility == System.Windows.Visibility.Visible)
            //    {
            //        tooBarTop.btnEdit.Visibility = Visibility.Collapsed;
            //        tooBarTop.retEdit.Visibility = Visibility.Collapsed;
            //    }

            //    if (tooBarTop.btnAudit.Visibility == System.Windows.Visibility.Visible)
            //    {
            //        tooBarTop.btnAudit.Visibility = Visibility.Collapsed;
            //        tooBarTop.retAudit.Visibility = Visibility.Collapsed;
            //    }
            //}

            // cbbOrderStatus.Visibility = Visibility.Visible;

            #region Added By 安凯航 2011年5月30日 增加借款单的导入功能

            if (this.DefaultEntity.OrderType == typeof(T_FB_BORROWAPPLYMASTER))
            {
                tooBarTop.btnImport.Visibility = System.Windows.Visibility.Visible;
                tooBarTop.btnImport.Click += new RoutedEventHandler(btnImportBorrowData_Click);
            }

            #endregion

            if ((this.DefaultEntity.OrderType == typeof(T_FB_DEPTBUDGETSUMMASTER) ||
                    this.DefaultEntity.OrderType == typeof(T_FB_COMPANYBUDGETSUMMASTER)))
            {
                tooBarTop.btnNew.Visibility = System.Windows.Visibility.Collapsed;
                tooBarTop.retNew.Visibility = System.Windows.Visibility.Collapsed;
                tooBarTop.btnDelete.Visibility = System.Windows.Visibility.Collapsed;
                tooBarTop.retDelete.Visibility = System.Windows.Visibility.Collapsed;
            }            
        }

        /// <summary>
        /// 导入借款单
        /// Creator:安凯航
        /// 日期:2011年5月30日
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnImportBorrowData_Click(object sender, RoutedEventArgs e)
        {
            //导入时间
            DateTime createTime = DateTime.Now;
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Excel 2007-2010 File(*.xlsx)|*.xlsx"
            };
            bool? userClickedOK = dialog.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK != true)
            {
                return;
            }
            try
            {

                //待保存数据列表
                List<OrderEntity> orders = new List<OrderEntity>();
                #region 读取Excel文件

                XLSXReader reader = new XLSXReader(dialog.File);
                List<string> subItems = reader.GetListSubItems();
                var maseters = reader.GetData(subItems[0]);
                var listm = maseters.ToDataSource();
                var details = reader.GetData(subItems[1]);
                var listd = details.ToDataSource();
                List<dynamic> masterList = new List<dynamic>();
                foreach (var item in listm)
                {
                    dynamic d = item;
                    masterList.Add(d);
                }
                //排除第一行信息
                masterList.Remove(masterList.First());

                List<dynamic> detailList = new List<dynamic>();
                foreach (var item in listd)
                {
                    dynamic d = item;
                    detailList.Add(d);
                }
                //排除第一行信息
                detailList.Remove(detailList.First());

                #endregion

                #region 添加主表
                List<T_FB_BORROWAPPLYMASTER> import = new List<T_FB_BORROWAPPLYMASTER>();
                foreach (var item in masterList)
                {
                    //构造OrderEntity数据
                    OrderEntity oe = new OrderEntity(typeof(T_FB_BORROWAPPLYMASTER));
                    orders.Add(oe);

                    T_FB_BORROWAPPLYMASTER t = oe.FBEntity.Entity as T_FB_BORROWAPPLYMASTER;
                    t.BORROWAPPLYMASTERID = Guid.NewGuid().ToString();
                    t.BORROWAPPLYMASTERCODE = item.单据号;
                    if (item.借款类型 == "普通借款")
                    {
                        t.REPAYTYPE = 1;
                    }
                    else if (item.借款类型 == "备用金借款")
                    {
                        t.REPAYTYPE = 2;
                    }
                    else if (item.借款类型 == "专项借款")
                    {
                        t.REPAYTYPE = 3;
                    }
                    else
                    {
                        continue;
                    }
                    t.REMARK = item.备注;
                    import.Add(t);
                }

                #endregion

                #region 添加子表

                foreach (var item in detailList)
                {
                    OrderEntity oe = orders.FirstOrDefault(oen =>
                    {
                        T_FB_BORROWAPPLYMASTER bm = oen.FBEntity.Entity as T_FB_BORROWAPPLYMASTER;
                        if (bm.BORROWAPPLYMASTERCODE == item.单据号)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                    if (oe == null)
                    {
                        string message = "导入的借款单明细借款单号为\"" + item.单据号 + "\"的明细,没有相应的借款单对应.请检查借款Excel中是否有此借款单的数据";
                        throw new InvalidOperationException(message);
                    }
                    FBEntity fbdetail = oe.CreateFBEntity<T_FB_BORROWAPPLYDETAIL>();
                    T_FB_BORROWAPPLYDETAIL d = fbdetail.Entity as T_FB_BORROWAPPLYDETAIL;
                    d.BORROWAPPLYDETAILID = Guid.NewGuid().ToString();
                    d.REMARK = item.摘要;
                    d.BORROWMONEY = Convert.ToDecimal(item.借款金额);
                    d.UNREPAYMONEY = Convert.ToDecimal(item.未还款金额);
                    d.T_FB_SUBJECT = d.T_FB_SUBJECT ?? new T_FB_SUBJECT();
                    d.T_FB_SUBJECT.SUBJECTID = item.借款科目;
                    d.CREATEDATE = createTime;
                    d.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    d.UPDATEDATE = createTime;
                    d.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                }

                #endregion

                //服务
                OrderEntityService service = new OrderEntityService();

                #region 得到科目信息

                QueryExpression reExp = QueryExpressionHelper.Equal("OWNERID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                QueryExpression exp = QueryExpressionHelper.Equal("OWNERPOSTID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID);
                exp.QueryType = "T_FB_BORROWAPPLYDETAIL";
                exp.RelatedExpression = reExp;
                exp.IsUnCheckRight = true;

                #endregion

                #region 获取申请人信息

                ObservableCollection<string> numbers = new ObservableCollection<string>();
                masterList.ForEach(item => numbers.Add(item.申请人身份证号));//拿到所有身份证号
                SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient client = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
                client.GetEmployeesByIdnumberAsync(numbers);

                #endregion

                #region 先获取申请人信息,待申请人信息填充完毕之后进行下一步操作

                client.GetEmployeesByIdnumberCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeesByIdnumberCompletedEventArgs>((oo, ee) =>
                {
                    if (ee.Error == null)
                    {
                        string message = null;
                        try
                        {
                            masterList.ForEach(m =>
                            {
                                //添加owner和creator
                                SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW userview = null;
                                try
                                {
                                    userview = ee.Result.First(t => t.IDNUMBER == m.申请人身份证号);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    message = ex.Message + "\n" + "在系统中找不到身份证号为\"" + m.申请人身份证号 + "\"的,员工信息.也可能是您没有相应的权限.";
                                    throw new InvalidOperationException(message);
                                }
                                T_FB_BORROWAPPLYMASTER master = null;
                                try
                                {
                                    master = import.First(t => t.BORROWAPPLYMASTERCODE == m.单据号);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    message = ex.Message + "\n" + "导入的借款单明细借款单号为\"" + m.单据号 + "\"的明细,没有相应的借款单对应.请检查借款Excel中是否有此借款单的数据";
                                    throw new InvalidOperationException(message);
                                }
                                master.OWNERCOMPANYID = userview.OWNERCOMPANYID;
                                master.OWNERCOMPANYNAME = userview.OWNERCOMPANYID;
                                master.OWNERDEPARTMENTID = userview.OWNERDEPARTMENTID;
                                master.OWNERID = userview.EMPLOYEEID;
                                master.OWNERPOSTID = userview.OWNERPOSTID;
                                master.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                                master.CREATECOMPANYNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
                                master.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                                master.CREATEDEPARTMENTNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                                master.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                                master.CREATEPOSTNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName;
                                master.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                                master.CREATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                                master.CREATEDATE = createTime;
                                master.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                                master.UPDATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                                master.UPDATEDATE = createTime;
                            });
                            //填充完申请人信息之后开始填充科目信息
                            service.QueryFBEntities(exp);
                        }
                        catch (Exception ex)
                        {
                            SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.ConfirmationBoxs("导入出错", ex.Message, "确定", MessageIcon.Exclamation);
                        }
                    }
                });

                #endregion

                #region 填充完申请人信息之后开始填充科目信息

                service.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>((o, args) =>
                {
                    if (args.Error == null)
                    {
                        //构造明细科目及账务信息
                        string message = null;
                        try
                        {
                            import.ForEach(m =>
                            {
                                OrderEntity oe = orders.FirstOrDefault(oen =>
                                {
                                    T_FB_BORROWAPPLYMASTER bm = oen.FBEntity.Entity as T_FB_BORROWAPPLYMASTER;
                                    if (bm.BORROWAPPLYMASTERCODE == m.BORROWAPPLYMASTERCODE)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                });
                                var dlist = oe.GetRelationFBEntities("T_FB_BORROWAPPLYDETAIL");
                                List<T_FB_BORROWAPPLYDETAIL> detailslist = new List<T_FB_BORROWAPPLYDETAIL>();
                                dlist.ForEach(ddd =>
                                {
                                    detailslist.Add(ddd.Entity as T_FB_BORROWAPPLYDETAIL);
                                });
                                detailslist.ForEach(d =>
                                {
                                    FBEntity en = null;
                                    try
                                    {
                                        en = args.Result.First(a => ((T_FB_BORROWAPPLYDETAIL)a.Entity).T_FB_SUBJECT.SUBJECTCODE == d.T_FB_SUBJECT.SUBJECTID);
                                    }
                                    catch (InvalidOperationException ex)
                                    {
                                        message = ex.Message + "\n" + "导入的借款单明细中,在系统中找不到ID为\"" + d.T_FB_SUBJECT.SUBJECTID + "\"的科目,也可能是您没有相应的权限";
                                        throw new InvalidOperationException(message);
                                    }
                                    T_FB_BORROWAPPLYDETAIL t = en.Entity as T_FB_BORROWAPPLYDETAIL;
                                    if (t != null)
                                    {
                                        t.T_FB_SUBJECT.T_FB_BORROWAPPLYDETAIL.Clear();
                                        d.T_FB_SUBJECT = t.T_FB_SUBJECT;
                                        d.CHARGETYPE = t.CHARGETYPE;
                                        d.USABLEMONEY = t.USABLEMONEY;
                                        #region 清理科目信息中的无用数据

                                        t.T_FB_SUBJECT.T_FB_BORROWAPPLYDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                                        t.T_FB_SUBJECT.T_FB_BUDGETCHECK.Clear();
                                        t.T_FB_SUBJECT.T_FB_CHARGEAPPLYDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_COMPANYBUDGETAPPLYDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_COMPANYBUDGETMODDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_COMPANYBUDGETMODDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_COMPANYTRANSFERDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_DEPTBUDGETADDDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_DEPTBUDGETAPPLYDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_DEPTTRANSFERDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_EXTENSIONORDERDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_PERSONBUDGETADDDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_PERSONBUDGETAPPLYDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_PERSONMONEYASSIGNDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_REPAYAPPLYDETAIL.Clear();
                                        t.T_FB_SUBJECT.T_FB_SUBJECTCOMPANY.Clear();
                                        t.T_FB_SUBJECT.T_FB_SUBJECTDEPTMENT.Clear();
                                        t.T_FB_SUBJECT.T_FB_SUBJECTPOST.Clear();
                                        t.T_FB_SUBJECT.T_FB_TRAVELEXPAPPLYDETAIL.Clear();

                                        #endregion
                                    }
                                });
                            });

                            //保存
                            service.SaveList(orders);
                        }
                        catch (Exception ex)
                        {
                            SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.ConfirmationBoxs("导入出错", ex.Message, "确定", MessageIcon.Exclamation);
                        }
                    }
                });

                #endregion

                #region 所有信息构造完成之后保存数据

                service.SaveListCompleted += new EventHandler<ActionCompletedEventArgs<bool>>((s, evgs) =>
                {
                    if (evgs.Result)
                    {
                        SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.ConfirmationBox("导入成功", "导入成功", "确定");
                    }
                    else
                    {
                        SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.ConfirmationBox("导入失败", "导入失败", "确定");
                    }
                });

                #endregion

            }
            catch (Exception ex)
            {
                SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.ConfirmationBoxs("导入出错", ex.Message, "确定", MessageIcon.Exclamation);
            }
        }

        private void InitData(OrderInfo order)
        {
            CommonFunction.FillQueryComboBox<OrderStatusData>(this.cbbOrderStatus);
            ITextValueItem itemStatus = DataCore.AllSelectdData;// DataCore.FindReferencedData<OrderStatusData>("0");
            cbbOrderStatus.SelectedItem = itemStatus;
            this.ADtGrid.Queryer.QueryCompleted += new EventHandler<QueryCompletedEventArgs>(Queryer_QueryCompleted);
            GetOrders();
        }



        private void InitControl(OrderInfo order)
        {
            this.ADtGrid.Grid.GridItems = order.View.GridItems;
            this.ADtGrid.Grid.InitControl(OperationTypes.Browse);
            cbbOrderStatus.SelectionChanged += new SelectionChangedEventHandler(cbbOrderStatus_SelectionChanged);
            cbbQueryList.SelectionChanged += new SelectionChangedEventHandler(cbbQueryList_SelectionChanged);
            OrderSource.SaveListCompleted += new EventHandler<ActionCompletedEventArgs<bool>>(OrderSource_SaveListCompleted);
            this.ADtGrid.Queryer.Querying += new EventHandler(Queryer_Querying);

            this.ADtGrid.Queryer.QueryCompleted += new EventHandler<QueryCompletedEventArgs>(Queryer_QueryCompleted);
        }

        /// <summary>
        /// DataGrid分页时启动等待图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Queryer_Querying(object sender, EventArgs e)
        {
            this.ShowProcess();
        }

        /// <summary>
        /// DataGrid分页完成隐藏等待图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Queryer_QueryCompleted(object sender, QueryCompletedEventArgs e)
        {
            this.CloseProcess();
        }

        void OrderSource_SaveListCompleted(object sender, ActionCompletedEventArgs<bool> e)
        {
            GetOrders();
        }


        private void GetOrders()
        {
            ShowProcess();
            ITextValueItem dataOrderStatus = cbbOrderStatus.SelectedItem as ITextValueItem;
            QueryData dataQueryList = cbbQueryList.SelectedItem as QueryData;
            QueryExpression qeTop = new QueryExpression();
            qeTop.QueryType = this.DefaultEntity.OrderType.Name;
            QueryExpression qe = qeTop;
            qe.RelatedType = QueryExpression.RelationType.And;

            QueryExpression qe1 = new QueryExpression();
            qe1.QueryType = qe.QueryType;
            qe1.PropertyName = FieldName.CheckStates;
            qe1.PropertyValue = dataOrderStatus.Value.ToString();
            qe1.Operation = QueryExpression.Operations.Equal;
            qe1.RelatedType = QueryExpression.RelationType.And;
            qe.RelatedExpression = qe1;
            qe = qe.RelatedExpression;

            string strCodeName = string.Empty;
            CommonFunction.GetTableCode(this.DefaultEntity.OrderType, ref strCodeName);
            //if (this.DefaultEntity.OrderType == typeof(T_FB_CHARGEAPPLYMASTER))
            //{
            //    strCodeName = "CHARGEAPPLYMASTERCODE";
            //}
            //else if (this.DefaultEntity.OrderType == typeof(T_FB_TRAVELEXPAPPLYMASTER))
            //{
            //    strCodeName = "TRAVELEXPAPPLYMASTERCODE";
            //}
            //else if (this.DefaultEntity.OrderType == typeof(T_FB_BORROWAPPLYMASTER))
            //{
            //    strCodeName = "BORROWAPPLYMASTERCODE";
            //}
            //else if (this.DefaultEntity.OrderType == typeof(T_FB_REPAYAPPLYMASTER))
            //{
            //    strCodeName = "REPAYAPPLYMASTERCODE";
            //}

            if (!string.IsNullOrWhiteSpace(txtCode.Text) && !string.IsNullOrWhiteSpace(strCodeName))
            {

                QueryExpression qeCode = new QueryExpression();
                qeCode.QueryType = qe.QueryType;
                qeCode.PropertyName = strCodeName;
                qeCode.PropertyValue = txtCode.Text;
                qeCode.Operation = QueryExpression.Operations.Like;
                qeCode.RelatedType = QueryExpression.RelationType.And;
                qe.RelatedExpression = qeCode;
                qe = qe.RelatedExpression;
            }

            // 开始日期
            if (dpStart.SelectedDate != null)
            {
                QueryExpression qeStartDate = new QueryExpression();
                qeStartDate.QueryType = qe.QueryType;
                qeStartDate.PropertyName = FieldName.UpdateDate;
                qeStartDate.PropertyValue = dpStart.SelectedDate.Value.ToString();
                qeStartDate.Operation = QueryExpression.Operations.GreaterThanOrEqual;
                qeStartDate.RelatedType = QueryExpression.RelationType.And;
                qe.RelatedExpression = qeStartDate;
                qe = qe.RelatedExpression;
            }

            // 截止日期
            if (dpEnd.SelectedDate != null)
            {
                QueryExpression qeEndDate = new QueryExpression();
                qeEndDate.QueryType = qe.QueryType;
                qeEndDate.PropertyName = FieldName.UpdateDate;
                qeEndDate.PropertyValue = dpEnd.SelectedDate.Value.ToString();
                qeEndDate.Operation = QueryExpression.Operations.LessThanOrEqual;
                qeEndDate.RelatedType = QueryExpression.RelationType.And;
                qe.RelatedExpression = qeEndDate;
                qe = qe.RelatedExpression;
            }

            // 查询对象选择范围
            QueryExpression qeTemp = null;
            if (lkObject.DataContext != null)
            {
                MultiValuesItem<ExtOrgObj> mutilValues = lkObject.SelectItem as MultiValuesItem<ExtOrgObj>;
                if (mutilValues != null)
                {
                    Dictionary<OrgTreeItemTypes, string> dictTypes = new Dictionary<OrgTreeItemTypes, string>();
                    dictTypes.Add(OrgTreeItemTypes.Company, FieldName.OwnerCompanyID);
                    dictTypes.Add(OrgTreeItemTypes.Department, FieldName.OwnerDepartmentID);
                    dictTypes.Add(OrgTreeItemTypes.Personnel, FieldName.OwnerID);
                    dictTypes.Add(OrgTreeItemTypes.Post, FieldName.OwnerPostID);

                    mutilValues.Values.ForEach(item =>
                    {

                        string propertyName = dictTypes[item.ObjectType];
                        QueryExpression qeItem = QueryExpressionHelper.Equal(propertyName, item.ObjectID);
                        qeItem.RelatedType = QueryExpression.RelationType.Or;
                        qeItem.RelatedExpression = qeTemp;
                        qeTemp = qeItem;
                    });
                }
            }

            if (qeTemp != null)
            {
                qe.RelatedExpression = qeTemp;
                qe = qeTemp;
            }

            if (qeTop.RelatedExpression != null)
            {
                qeTop = qeTop.RelatedExpression;
            }
            qeTop.OrderByExpression = new OrderByExpression();
            qeTop.OrderByExpression.PropertyName = FieldName.UpdateDate;
            qeTop.OrderByExpression.PropertyType = typeof(DateTime).Name;
            qeTop.OrderByExpression.OrderByType = OrderByType.Dsc;
            this.ADtGrid.Queryer.QueryExpression = qeTop;
            this.ADtGrid.Queryer.Query();
        }

        private void cbbOrderStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ITextValueItem dataOrderStatus = cbbOrderStatus.SelectedItem as ITextValueItem;
            int icheckState = 0;
            if (dataOrderStatus != null)
            {
                icheckState = CommonFunction.TryConvertValue<int>(dataOrderStatus.Value.ToString());
                Utility.SetToolBarButtonByCheckState(icheckState, this.tooBarTop, this.DefaultEntity.OrderInfo.Type);
            }

            if ((this.DefaultEntity.OrderType == typeof(T_FB_DEPTBUDGETSUMMASTER) ||
                    this.DefaultEntity.OrderType == typeof(T_FB_COMPANYBUDGETSUMMASTER)))
            {
                tooBarTop.btnNew.Visibility = System.Windows.Visibility.Collapsed;
                tooBarTop.retNew.Visibility = System.Windows.Visibility.Collapsed;
                tooBarTop.btnDelete.Visibility = System.Windows.Visibility.Collapsed;
                tooBarTop.retDelete.Visibility = System.Windows.Visibility.Collapsed;
            }
            // 注释掉打印
            //if (icheckState == 2)
            //{
            //    tooBarTop.btnPrint.Visibility = System.Windows.Visibility.Visible;
            //}
            //else
            //{
            //    tooBarTop.btnPrint.Visibility = System.Windows.Visibility.Collapsed;
            //}

            GetOrders();
        }

        private void cbbQueryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            GetOrders();
        }

        private void FormToolPrint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //    P2.MaxHeight = this.LayoutRoot.ActualHeight - P1.ActualHeight - P3.ActualHeight;
        }

        #region ToolBar ButtonEvent

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSelectedRow(false))
            {
                return;
            }

            OrderEntity orderEntity = this.ADtGrid.SelectedItems[0] as OrderEntity;

            ShowEditForm(orderEntity, OperationTypes.Audit);
        }


        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSelectedRow(false))
            {
                return;
            }

            OrderEntity orderEntity = this.ADtGrid.SelectedItems[0] as OrderEntity;

            ShowEditForm(orderEntity, OperationTypes.ReSubmit);

        }



        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSelectedRow(false))
            {
                return;
            }

            OrderEntity orderEntity = this.ADtGrid.SelectedItems[0] as OrderEntity;

            ShowEditForm(orderEntity, OperationTypes.Browse);
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.GetOrders();
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSelectedRow(true))
            {
                return;
            }
            for (int i = 0; i < this.ADtGrid.SelectedItems.Count; i++)
            {
                OrderEntity order = this.ADtGrid.SelectedItems[i] as OrderEntity;
                object states = order.GetObjValue(EntityFieldName.CheckStates);
                if (states.ToString() != ((int)SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString())
                {
                    CommonFunction.NotifySelection(Utility.GetResourceStr("Msg_NoDeleteOrder"));
                    return;
                }

            }

            Action action = () =>
                {
                    List<OrderEntity> list = new List<OrderEntity>();
                    for (int i = 0; i < this.ADtGrid.SelectedItems.Count; i++)
                    {
                        OrderEntity order = this.ADtGrid.SelectedItems[i] as OrderEntity;

                        order.FBEntityState = FBEntityState.Detached;

                        list.Add(order);
                    }
                    OrderSource.SaveList(list);
                };
            CommonFunction.AskDelete(string.Empty, action);
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (!CheckSelectedRow(true))
            {
                return;
            }

            OrderEntity orderEntity = this.ADtGrid.SelectedItems[0] as OrderEntity;
            ShowEditForm(orderEntity, OperationTypes.Edit);
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            OrderEntity orderEntity = new OrderEntity(this.DefaultEntity.OrderType);
            ShowEditForm(orderEntity, OperationTypes.Add);
        }
        #endregion
        private void ShowEditForm(OrderEntity orderEntity, OperationTypes operationType)
        {
            //OrderForm caForm = CommonFunction.GetOrderForm(orderEntity);
            //caForm.Width = this.ActualWidth;
            //caForm.Height = this.ActualHeight;
            //caForm.Closed += (o, e) =>
            //{
            //    if (caForm.IsNeedToRefresh)
            //    {
            //        this.GetOrders();
            //    }
            //};
            //caForm.Show();
            FBPage page = FBPage.GetPage(orderEntity);
            page.EditForm.OperationType = operationType;
            FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;
            EntityBrowser eb = new EntityBrowser(page);
            eb.Show<string>(DialogMode.Default, plRoot, "", (result) => { });


            page.RefreshData += (o, e) =>
            {
                this.GetOrders();
            };
            page.PageClosing += (o, e) =>
            {
                eb.Close();
            };
        }

        private bool isWebPart = false;

        private bool CheckSelectedRow(bool canMultiItems)
        {
            if (this.ADtGrid.SelectedItems.Count == 0)
            {
                CommonFunction.NotifySelection(null);
                return false;
            }
            //else if (!canMultiItems && this.ADtGrid.SelectedItems.Count > 1)
            //{
            //    CommonFunction.NotifySelection("只能选择一条记录");
            //    return false;
            //}
            return true;

        }
        #region IWebPart 成员

        [DefaultValue(10)]
        public int RowCount
        {
            get;
            set;
        }

        public void ShowMaxiWebPart()
        {
            isWebPart = false;
            tooBarTop.btnNew.Visibility = Visibility.Visible;
            tooBarTop.retNew.Visibility = Visibility.Visible;

            tooBarTop.btnEdit.Visibility = Visibility.Visible;
            tooBarTop.retEdit.Visibility = Visibility.Visible;

            tooBarTop.btnDelete.Visibility = Visibility.Visible;
            tooBarTop.retDelete.Visibility = Visibility.Visible;
        }

        public void ShowMiniWebPart()
        {
            isWebPart = true;

            tooBarTop.btnNew.Visibility = Visibility.Collapsed;
            tooBarTop.retNew.Visibility = Visibility.Collapsed;

            tooBarTop.btnEdit.Visibility = Visibility.Collapsed;
            tooBarTop.retEdit.Visibility = Visibility.Collapsed;

            tooBarTop.btnDelete.Visibility = Visibility.Collapsed;
            tooBarTop.retDelete.Visibility = Visibility.Collapsed;
        }

        public void RefreshData()
        {
            this.GetOrders();
        }

        public event EventHandler OnMoreChanged;
        #endregion



        #region IWebPart Members


        public int RefreshTime
        {
            get;
            set;
        }

        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetOrders();
        }

        private void lkObject_FindClick(object sender, EventArgs e)
        {
            string userID = DataCore.CurrentUser.Value.ToString();
            string perm = ((int)Permissions.Browse).ToString();
            string entity = this.DefaultEntity.OrderType.Name;
            OrganizationLookup ogzLookup = new OrganizationLookup(userID, perm, entity);
            ogzLookup.MultiSelected = true;
            ogzLookup.SelectedObjType = OrgTreeItemTypes.All;


            FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;

            ogzLookup.SelectedClick += (o, ev) =>
            {
                if (ogzLookup.SelectedObj.Count > 0)
                {
                    List<ExtOrgObj> list = new List<ExtOrgObj>();
                    string text = " ";

                    foreach (ExtOrgObj obj in ogzLookup.SelectedObj)
                    {
                        list.Add(obj);

                        text = text.Trim() + ";" + obj.ObjectName;

                    }
                    MultiValuesItem<ExtOrgObj> item = new MultiValuesItem<ExtOrgObj>();
                    item.Values = list;
                    item.Text = text.Substring(1);
                    lkObject.SelectItem = item;
                    lkObject.DataContext = item;
                    lkObject.DisplayMemberPath = "Text";

                    //lkObject.TxtLookUp.Text = text.Substring(1);
                }
            };
            ogzLookup.Show<string>(DialogMode.ApplicationModal, plRoot, "", (result) => { });
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            lkObject.DataContext = null;
            txtCode.Text = string.Empty;
            dpStart.Text = string.Empty;
            dpEnd.Text = string.Empty;
        }
    }

}
