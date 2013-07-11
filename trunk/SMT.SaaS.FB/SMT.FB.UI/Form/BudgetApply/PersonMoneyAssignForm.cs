using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.FB.UI.Common;
using SMT.FB.UI.Common.Controls;
using SMT.FB.UI.FBCommonWS;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI;
using System.Collections.Specialized;


namespace SMT.FB.UI.Form.BudgetApply
{
    public class PersonMoneyAssignForm : FBPage
    {
        FBEntityService fbService;
        SMTLoading loadbar = null;
        /// <summary>
        /// 个人经费下拨 构造函数
        /// </summary>
        /// <param name="orderEntity"></param>
        public PersonMoneyAssignForm(OrderEntity orderEntity)
            : base(orderEntity)
        {
            fbService = new FBEntityService();
            fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);
            this.EditForm.Saving += new EventHandler<SavingEventArgs>(EditForm_Saving);
        }

        /// <summary>
        /// 保存单据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditForm_Saving(object sender, SavingEventArgs e)
        {
            List<string> msgs = new List<string>();
            T_FB_PERSONMONEYASSIGNMASTER entCurr = this.OrderEntity.Entity as T_FB_PERSONMONEYASSIGNMASTER;
            if (string.IsNullOrWhiteSpace(entCurr.ASSIGNCOMPANYID) || string.IsNullOrWhiteSpace(entCurr.ASSIGNCOMPANYNAME))
            {
                msgs.Add("下拨公司不能为空");
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
                return;
            }

            ObservableCollection<FBEntity> details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);

            ObservableCollection<FBEntity> list0 = new ObservableCollection<FBEntity>();


            details.ToList().ForEach(item =>
            {
                T_FB_PERSONMONEYASSIGNDETAIL detail = item.Entity as T_FB_PERSONMONEYASSIGNDETAIL;

                if (detail.BUDGETMONEY <= 0 || detail.BUDGETMONEY == null)
                {
                    string errorMessage = detail.OWNERNAME + "的下拨金额为零请删除";
                    msgs.Add(errorMessage);
                }
            });

            //明细为为0的不能提交
            if (details.ToList().Count <= 0)
            {
                msgs.Add("下拨明细不能为空");
            }
            if (msgs.Count > 0)
            {
                e.Action = Actions.Cancel;
                CommonFunction.ShowErrorMessage(msgs);
            }

            //删除多余的关联
            if (entCurr == null)
            {
                return;
            }

            if (entCurr.T_FB_PERSONMONEYASSIGNDETAIL == null)
            {
                return;
            }

            entCurr.T_FB_PERSONMONEYASSIGNDETAIL.Clear();
        }

        private void InitData()
        {
            this.StartProcess();
            if (this.OrderEntity.FBEntityState == FBEntityState.Added)
            {
                OrderEntity.SetObjValue("Entity.BUDGETARYMONTH", new DateTime(DataCore.SystemDateTime.Year, DataCore.SystemDateTime.Month, 1));

                EmployeerData create = this.OrderEntity.GetCreateInfo();
                this.OrderEntity.ReferencedData["Entity.AssignCompany"] = new MyOrgObjectData { Value = create.Company };
                OrderEntity.SetObjValue("Entity.ASSIGNCOMPANYID", create.Company.Value);
                OrderEntity.SetObjValue("Entity.ASSIGNCOMPANYNAME", create.Company.Text);

                // 1. 获取上个月当前操作者审核通过的单据
                GetOrderDetail();
            }
            else
            {
                SetSortDetails();
                //if (isFirst)
                //{
                //    isFirst = false;
                SortDetails();
                //  }
            }

            this.OrderEntity.OrderPropertyChanged += new EventHandler<OrderPropertyChangedArgs>(OrderEntity_OrderPropertyChanged);
            this.OrderEntity.CollectionEntityChanged += new EventHandler<EntityChangedArgs>(OrderEntity_CollectionEntityChanged);

            var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);
            details.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(details_CollectionChanged);
        }

        /// <summary>
        /// 为了明细安排岗位级别和薪资级别排序
        /// </summary>
        void SortDetails()
        {
            var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);
            ObservableCollection<string> employeeIDs = new ObservableCollection<string>();
            details.ForEach(item =>
                employeeIDs.Add((item.Entity as T_FB_PERSONMONEYASSIGNDETAIL).OWNERID)
                );


            //为了排序，后台排好序以后到前台还是乱的
            SMT.Saas.Tools.SalaryWS.SalaryServiceClient pe = new SMT.Saas.Tools.SalaryWS.SalaryServiceClient();
            pe.GetSalaryArchiveByEmployeeIDsCompleted += new EventHandler<Saas.Tools.SalaryWS.GetSalaryArchiveByEmployeeIDsCompletedEventArgs>(pe_GetSalaryArchiveByEmployeeIDsCompleted);
            int dYear = 0, dMonth = 0;
            dYear = DateTime.Now.AddMonths(-1).Year;     //取年份
            dMonth = DateTime.Now.AddMonths(-1).Month;   //取月份
            pe.GetSalaryArchiveByEmployeeIDsAsync(employeeIDs, dYear, dMonth);
        }
        void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<string> msgs = new List<string>();
                try
                {
                    this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name).Clear();

                    if (e.Result.Count == 0)
                    {
                        this.OrderEntity.SetObjValue("Entity.BUDGETMONEY", 0);
                        this.StopProcess();
                        CommonFunction.ShowErrorMessage("根据公司加载人员信息失败，请手动选择下拨人员");
                        return;
                    }

                    T_FB_PERSONMONEYASSIGNMASTER entlastest = e.Result[0].Entity as T_FB_PERSONMONEYASSIGNMASTER;
                    // 2. 复制上个月的单据数据到当前操作的新单据上
                    CopyData(entlastest);
                    SortDetails();
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.ToString());
                }

                if (msgs.Count > 0)
                {
                    CommonFunction.ShowErrorMessage(msgs);
                }
            }
        }

        private string GetAssignCompanyName(string strAssignCompanyID)
        {
            string strRes = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(strAssignCompanyID) || App.Current.Resources["SYS_CompanyInfo"] == null)
                {
                    return strRes;
                }

                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> entlist = App.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

                if (entlist.Count() == 0)
                {
                    return strRes;
                }

                var qv = from n in entlist
                         where n.COMPANYID == strAssignCompanyID
                         select n.CNAME;

                if (qv == null)
                {
                    return strRes;
                }

                if (qv.Count() == 0)
                {
                    return strRes;
                }

                strRes = qv.FirstOrDefault();
            }
            catch
            {

            }
            return strRes;
        }

        void OrderEntity_OrderPropertyChanged(object sender, OrderPropertyChangedArgs e)
        {
            
            T_FB_PERSONMONEYASSIGNMASTER entCurr = this.OrderEntity.Entity as T_FB_PERSONMONEYASSIGNMASTER;
            if (e.Result.Contains("Entity.AssignCompany"))
            {
                this.StartProcess();
                CompanyData comData = entCurr.AssignCompany as CompanyData;

                entCurr.ASSIGNCOMPANYNAME = comData.Text.ToString();
                entCurr.ASSIGNCOMPANYID = comData.Value.ToString();
                GetOrderDetail();
            }
        }

        void OrderEntity_CollectionEntityChanged(object sender, EntityChangedArgs e)
        {
            if (sender.GetType() == typeof(T_FB_PERSONMONEYASSIGNDETAIL))
            {
                if (e.ChangedEventArgs.PropertyName == "BUDGETMONEY" || e.Action == Actions.Delete)
                {
                    var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);
                    decimal? sumMoney = details.Sum(item =>
                    {
                        return (item.Entity as T_FB_PERSONMONEYASSIGNDETAIL).BUDGETMONEY;
                    });
                    this.OrderEntity.SetObjValue("Entity.BUDGETMONEY", sumMoney);


                }
            }
        }

        void details_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                SetSortDetails();
            }
        }

        private void SetSortDetails()
        {
            var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);
            for (int i = 0; i < details.Count; i++)
            {
                (details[i].Entity as T_FB_PERSONMONEYASSIGNDETAIL).RowIndex = i + 1;
            }

            DetailGrid dgrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
            if (dgrid != null)
            {
                dgrid.ClearValue(DetailGrid.ItemsSourceProperty);
                dgrid.ItemsSource = details;
            }
        }

        /// <summary>
        /// 根据员工ID获取员工薪资档案信息（为了根据岗位级别和薪资级别排序）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pe_GetSalaryArchiveByEmployeeIDsCompleted(object sender, Saas.Tools.SalaryWS.GetSalaryArchiveByEmployeeIDsCompletedEventArgs e)
        {
            try
            {
                if (e.Result == null || e.Result.Count == 0 || e.Error != null)
                {
                    CommonFunction.ShowErrorMessage("调用HR服务返回异常信息（为空表示没有数据）：" + e.Error);
                    return;
                }
                var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);
                List<SMT.Saas.Tools.SalaryWS.T_HR_SALARYARCHIVE> listSalay = e.Result.ToList();
                List<T_FB_PERSONMONEYASSIGNDETAIL> perLIst = new List<T_FB_PERSONMONEYASSIGNDETAIL>();
                foreach (var item in details)
                {
                    var itemFBEntity = item.Entity as T_FB_PERSONMONEYASSIGNDETAIL;
                    listSalay.ForEach(it =>
                    {
                        if (itemFBEntity.OWNERID == it.OWNERID)
                        {
                            itemFBEntity.CREATEUSERNAME = Convert.ToString(it.POSTLEVEL);//目前把岗位级别存为创建人字段
                            itemFBEntity.UPDATEUSERNAME = Convert.ToString(it.SALARYLEVEL);//把薪资级别存在更新热字段，目前表里面没有相应字段
                        }
                    });
                    perLIst.Add(itemFBEntity);
                }

                perLIst = perLIst.OrderBy(t => Convert.ToDecimal(t.CREATEUSERNAME)).ThenBy(t => Convert.ToDecimal(t.UPDATEUSERNAME)).ToList();//排序
                var fbEntity = perLIst.ToFBEntityList();
                DetailGrid dgrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;
                if (dgrid != null)
                {
                    dgrid.ClearValue(DetailGrid.ItemsSourceProperty);
                    dgrid.ItemsSource = fbEntity;
                }
                this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name).Clear();
                fbEntity.ForEach(item =>
                {
                    item.FBEntityState = FBEntityState.Modified;//因为清除了数据，所以得要加上状态
                    T_FB_PERSONMONEYASSIGNDETAIL perDetail = item.Entity as T_FB_PERSONMONEYASSIGNDETAIL;
                    perDetail.T_FB_PERSONMONEYASSIGNMASTER = this.OrderEntity.Entity as T_FB_PERSONMONEYASSIGNMASTER;//设置主表关联
                });
                this.OrderEntity.FBEntity.AddFBEntities<T_FB_PERSONMONEYASSIGNDETAIL>(fbEntity);//加载数据
                var detailss = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);//获取数据
                for (int i = 0; i < details.Count; i++)
                {
                    (details[i].Entity as T_FB_PERSONMONEYASSIGNDETAIL).RowIndex = i + 1;//序号
                }
                this.StopProcess();
            }
            catch (Exception ex)
            {
                this.StopProcess();
                CommonFunction.ShowErrorMessage("调用HR服务返回异常信息：" + ex.ToString());
            }
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        private void GetOrderDetail()
        {
            QueryExpression qe = QueryExpressionHelper.Equal(FieldName.OwnerID, DataCore.CurrentUser.Value.ToString());
            qe.QueryType = typeof(T_FB_PERSONMONEYASSIGNMASTER).Name + "FormHR";//Latest

            QueryExpression qeAssign = QueryExpressionHelper.Equal("ASSIGNCOMPANYID", this.OrderEntity.GetObjValue("Entity.ASSIGNCOMPANYID").ToString());
            qeAssign.QueryType = typeof(T_FB_PERSONMONEYASSIGNMASTER).Name + "FormHR";//Latest
            qeAssign.IsUnCheckRight = true;
            qeAssign.RelatedExpression = qe;
            qeAssign.IsNoTracking = false;

            fbService.QueryFBEntities(qeAssign);
        }

        protected override void OnLoadDataComplete()
        {
            this.InitProcess();
            InitData();
            //SortDetails();
        }

        protected override void OnLoadControlComplete()
        {
            DetailGrid dGrid = this.EditForm.FindControl("OrderGrid") as DetailGrid;

            if (dGrid != null && !this.EditForm.IsReInitForm)
            {
                dGrid.ToolBars[0].Title = "选择下拨人员";
                dGrid.AddToolBarItems(dGrid.ToolBars);
                double width = dGrid.ADGrid.Columns[dGrid.ADGrid.Columns.Count - 1].Width.Value;
                dGrid.ToolBarItemClick += new EventHandler<ToolBarItemClickEventArgs>(dGrid_ToolBarItemClick);

                //if (this.OrderEntity.FBEntityState != FBEntityState.Added)
                //{
                //    dGrid.ADGrid.Columns[5].Visibility = Visibility.Collapsed;
                //}
                //else
                //{
                //    dGrid.ADGrid.Columns[5].Visibility = Visibility.Visible;
                //}
            }
            if (this.EditForm.OperationType != OperationTypes.Add)
            {
                LookUp lu = this.EditForm.FindControl("AssignCompanyID") as LookUp;

                if (lu != null)
                    lu.IsEnabled = false;
            }
        }

        void dGrid_ToolBarItemClick(object sender, ToolBarItemClickEventArgs e)
        {
            if (e.Action != Actions.Add)
            {
                return;
            }
            e.Action = Actions.Cancel;
            string perm = "3";
            string entity = typeof(T_FB_PERSONMONEYASSIGNMASTER).Name;
            if (this.EditForm.OperationType == OperationTypes.Edit)
            {
                perm = ((int)Permissions.Edit).ToString();
            }
            else if (this.EditForm.OperationType == OperationTypes.Add)
            {
                perm = ((int)Permissions.Add + 1).ToString();
            }
            else
            {
                perm = ((int)Permissions.Browse).ToString();
            }

            string userID = DataCore.CurrentUser.Value.ToString();
            //             BF06E969-1B2C-4a89-B0AE-A91CA1244053
            OrganizationLookup ogzLookup = new OrganizationLookup();

            ogzLookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            ogzLookup.MultiSelected = true;

            FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;

            try
            {
                ogzLookup.SelectedClick += (o, ea) =>
                {
                    if (ogzLookup.SelectedObj.Count > 0)
                    {
                        //处理岗位及下拨
                        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient pe = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
                        ObservableCollection<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEFUNDS> vlistpostinfo = new ObservableCollection<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEFUNDS>();

                        var assignDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);

                        var selectedObjects = ogzLookup.SelectedObj;
                        selectedObjects.ForEach(obj =>
                        {
                            ExtOrgObj post = obj.ParentObject as ExtOrgObj;
                            ExtOrgObj dept = post.ParentObject as ExtOrgObj;

                            // ExtOrgObj com = dept.ParentObject as ExtOrgObj;
                            ITextValueItem pdata = DataCore.FindReferencedData<PostData>(post.ObjectID);
                            ITextValueItem ddata = DataCore.FindReferencedData<DepartmentData>(dept.ObjectID);
                            ITextValueItem cdata = (ddata as DepartmentData).Company;

                            var existDetail = assignDetail.FirstOrDefault(item =>
                            {
                                T_FB_PERSONMONEYASSIGNDETAIL cd = item.Entity as T_FB_PERSONMONEYASSIGNDETAIL;
                                return cd.OWNERID == obj.ObjectID && cd.OWNERPOSTID == pdata.Value.ToString();
                            });

                            T_FB_PERSONMONEYASSIGNDETAIL detail = new T_FB_PERSONMONEYASSIGNDETAIL();
                            if (existDetail != null)
                            {
                                detail.PERSONBUDGETAPPLYDETAILID = (existDetail.Entity as T_FB_PERSONMONEYASSIGNDETAIL).PERSONBUDGETAPPLYDETAILID;
                                detail.T_FB_PERSONMONEYASSIGNMASTER = this.OrderEntity.Entity as T_FB_PERSONMONEYASSIGNMASTER;
                            }
                            else
                            {
                                detail.PERSONBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                                detail.T_FB_PERSONMONEYASSIGNMASTER = this.OrderEntity.Entity as T_FB_PERSONMONEYASSIGNMASTER;
                                detail.BUDGETMONEY = 0;
                            }

                            detail.OWNERID = obj.ObjectID;
                            detail.OWNERNAME = obj.ObjectName;
                            detail.OWNERPOSTID = pdata.Value.ToString();
                            detail.OWNERPOSTNAME = pdata.Text;
                            detail.OWNERDEPARTMENTID = ddata.Value.ToString();
                            detail.OWNERDEPARTMENTNAME = ddata.Text;
                            detail.OWNERCOMPANYID = cdata.Value.ToString();
                            detail.OWNERCOMPANYNAME = cdata.Text;

                            //SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB vpostinfo = new SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB();
                            //vpostinfo.PERSONBUDGETAPPLYDETAILID = detail.PERSONBUDGETAPPLYDETAILID;
                            //vpostinfo.OWNERID = detail.OWNERID;
                            //vpostinfo.OWNERPOSTID = detail.OWNERPOSTID;
                            //vlistpostinfo.Add(vpostinfo);

                            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEFUNDS vpostinfo = new SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEFUNDS();

                            vpostinfo.EMPLOYEEID = detail.OWNERID;
                            vpostinfo.POSTID = detail.OWNERPOSTID;
                            vpostinfo.COMPANYID = detail.OWNERCOMPANYID;
                            vlistpostinfo.Add(vpostinfo);

                            if (existDetail != null)
                            {
                                return;
                            }
                            else
                            {
                                FBEntity fbEntity = detail.ToFBEntity();
                                fbEntity.FBEntityState = FBEntityState.Added;
                                assignDetail.Add(fbEntity);
                            }
                        });
                        if (vlistpostinfo != null && vlistpostinfo.Count > 0)
                        {
                            pe.GetEmployeeFundsListCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeeFundsListCompletedEventArgs>(pe_GetEmployeeFundsListCompleted);
                            pe.GetEmployeeFundsListAsync(vlistpostinfo);
                        }
                    }
                };
                ogzLookup.Show<string>(DialogMode.ApplicationModal, plRoot, "", (result) => { });
            }
            catch (Exception ex)
            {
                this.StopProcess();
                CommonFunction.ShowErrorMessage("调用HR服务返回异常信息：" + ex.ToString());
            }
        }

        void pe_GetEmployeeFundsListCompleted(object sender, Saas.Tools.PersonnelWS.GetEmployeeFundsListCompletedEventArgs e)
        {
            try
            {
                if (e.Result == null || e.Result.Count == 0 || e.Error != null)
                {
                    CommonFunction.ShowErrorMessage("调用HR服务返回异常信息（为空表示没有数据）：" + e.Error);
                    return;
                }
                var assignDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);
                ObservableCollection<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEFUNDS> vlistpostinfo = e.Result;
                vlistpostinfo.ForEach(person =>
                {
                    var detail = assignDetail.FirstOrDefault(p =>
                    {
                        T_FB_PERSONMONEYASSIGNDETAIL cd = p.Entity as T_FB_PERSONMONEYASSIGNDETAIL;
                        return cd.OWNERID == person.EMPLOYEEID && cd.OWNERPOSTID == person.POSTID;
                    });
                    if (detail != null)
                    {
                        T_FB_PERSONMONEYASSIGNDETAIL item = detail.Entity as T_FB_PERSONMONEYASSIGNDETAIL;
                        decimal dRealsum = 0, dNeedsum = 0;
                        if (person.REALSUM != null)
                        {
                            dRealsum = person.REALSUM.Value;

                        }

                        if (person.NEEDSUM != null)
                        {
                            dNeedsum = person.NEEDSUM.Value;

                        }

                        item.BUDGETMONEY = dRealsum;
                        item.SUGGESTBUDGETMONEY = dNeedsum;
                        item.POSTINFO = person.ATTENDREMARK;
                    }
                });
            }
            catch (Exception ex)
            {
                this.StopProcess();
                CommonFunction.ShowErrorMessage("调用HR服务返回异常信息：" + ex.ToString());
            }
        }

        void pe_GetEmployeeListForFBCompleted(object sender, Saas.Tools.PersonnelWS.GetEmployeeListForFBCompletedEventArgs e)
        {
            try
            {
                if (e.Result == null || e.Result.Count == 0 || e.Error != null)
                {
                    CommonFunction.ShowErrorMessage("调用HR服务返回异常信息（为空表示没有数据）：" + e.Error);
                    return;
                }
                var assignDetail = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);
                ObservableCollection<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOSTFORFB> vlistpostinfo = e.Result;

                vlistpostinfo.ForEach(person =>
                {
                    var detail = assignDetail.FirstOrDefault(p =>
                    {
                        T_FB_PERSONMONEYASSIGNDETAIL cd = p.Entity as T_FB_PERSONMONEYASSIGNDETAIL;
                        return cd.PERSONBUDGETAPPLYDETAILID == person.PERSONBUDGETAPPLYDETAILID;
                    });
                    if (detail != null)
                    {
                        T_FB_PERSONMONEYASSIGNDETAIL item = detail.Entity as T_FB_PERSONMONEYASSIGNDETAIL;
                        item.SUGGESTBUDGETMONEY = person.SUM;
                        switch (person.EMPLOYEESTATE)
                        {
                            case "0"://试用期
                                item.POSTINFO = "试用期，请注意";
                                break;
                            case "1"://在职
                                item.POSTINFO = string.Empty;
                                break;
                            case "2"://已离职
                                item.POSTINFO = "已离职，请删除";
                                break;
                            case "3"://离职中
                                item.POSTINFO = "离职中，请注意";
                                break;
                            case "4"://未入职
                                item.POSTINFO = "未入职，请删除";
                                break;
                            case "10"://异动中
                                item.POSTINFO = "异动中，请异动后再处理";
                                break;
                            case "11"://异动过
                                item.POSTINFO = "异动过，已转换成新岗位";

                                item.OWNERPOSTID = person.NEWPOSTID;
                                item.OWNERPOSTNAME = person.NEWPOSTNAME;
                                item.OWNERDEPARTMENTID = person.NEWDEPARTMENTID;
                                item.OWNERDEPARTMENTNAME = person.NEWDEPARTMENTNAME;
                                item.OWNERCOMPANYID = person.NEWCOMPANYID;
                                item.OWNERCOMPANYNAME = person.NEWCOMPANYNAME;
                                break;
                            case "12"://岗位异常
                                item.POSTINFO = "岗位异常，请删除后再选择";
                                break;
                            default:
                                item.POSTINFO = string.Empty;
                                break;
                        }

                        switch (person.ISAGENCY)
                        {
                            case "0"://主岗位
                                // item.POSTINFO += string.Empty;
                                break;
                            case "1"://赚职
                                if (item.POSTINFO != string.Empty && person.EMPLOYEESTATE != "1")
                                    item.POSTINFO = item.POSTINFO.Insert(0, "兼职，");
                                else
                                    item.POSTINFO = "兼职";
                                break;
                            default:
                                // item.POSTINFO += string.Empty;
                                break;
                        }
                    }
                });


            }
            catch (Exception ex)
            {
                this.StopProcess();
                CommonFunction.ShowErrorMessage("调用HR服务返回异常信息：" + ex.ToString());
            }
        }

        private void CopyData(T_FB_PERSONMONEYASSIGNMASTER masterOld)
        {
            // 1复制主表
            var master = CopyMaster();

            var details = this.OrderEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);

            // 复制每一个子表
            masterOld.T_FB_PERSONMONEYASSIGNDETAIL.OrderBy(item => item.OWNERNAME).ForEach(item =>
            {
                item.EntityKey = null;
                item.T_FB_PERSONMONEYASSIGNMASTER = master;
                item.T_FB_PERSONMONEYASSIGNMASTERReference = null;
                item.PERSONBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                item.REMARK = string.Empty;
                item.T_FB_SUBJECT.T_FB_PERSONMONEYASSIGNDETAIL.Clear();
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                var fbEntityDetail = item.ToFBEntity();
                fbEntityDetail.FBEntityState = FBEntityState.Added;
                details.Add(fbEntityDetail);

                master.T_FB_PERSONMONEYASSIGNDETAIL.Add(item);
            });

            decimal? sumMoney = details.Sum(item =>
            {
                return (item.Entity as T_FB_PERSONMONEYASSIGNDETAIL).BUDGETMONEY;
            });
            master.BUDGETMONEY = sumMoney;
        }

        private T_FB_PERSONMONEYASSIGNMASTER CopyMaster()
        {
            T_FB_PERSONMONEYASSIGNMASTER entityNewly = this.OrderEntity.Entity as T_FB_PERSONMONEYASSIGNMASTER;
            if (entityNewly.T_FB_PERSONMONEYASSIGNDETAIL == null)
            {
                entityNewly.T_FB_PERSONMONEYASSIGNDETAIL = new ObservableCollection<T_FB_PERSONMONEYASSIGNDETAIL>();
            }

            entityNewly.T_FB_PERSONMONEYASSIGNDETAIL.Clear();
            entityNewly.PERSONMONEYASSIGNMASTERID = Guid.NewGuid().ToString();

            return entityNewly;
        }

        #region 进度条（转圈那个东西）
        private  void InitProcess()
        {
            Panel parent = this.Content as Panel;
            if (parent != null)
            {
                Grid g = new Grid();
                this.Content = g;
                g.Children.Add(parent);
                loadbar = new SMTLoading(); //全局变量
                g.Children.Add(loadbar);
            }
        }
        private void StartProcess()
        {
            if (loadbar != null)
            {
                loadbar.Start();//调用服务时写
            }
        }
        private void StopProcess()
        {
            if (loadbar != null)
            {
                loadbar.Stop();
            }

        }
        #endregion 
    }
}
