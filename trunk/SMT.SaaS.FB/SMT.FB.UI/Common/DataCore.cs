using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SMT.FB.UI.FBCommonWS;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using SMT.SaaS.FrameworkUI.OrganizationControl;

using CurrentContext = SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContext;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;


using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;

namespace SMT.FB.UI.Common
{
    #region DataCore
    public static class DataCore
    {
        public const decimal Max_Charge = 999999999999;
        public const decimal Max_Budget = 9999999999999;

        private const string Default_DataFormat = "yyyy年MM月dd日 HH:mm";
        private static string _DataFormat = Default_DataFormat;

        static DictionaryManager DictManager = new DictionaryManager();
        static List<string> ListDict = new List<string>(); //字典列表

        public static string DataFormat
        {
            get
            {
                if (_DataFormat == null)
                {
                    _DataFormat = Default_DataFormat;
                }
                return _DataFormat;
            }
            set
            {
                _DataFormat = value;
            }
        }
        public static string DataFormat_Month = "yyyy年MM月";
        public static string DataFormat_Year = "yyyy年";
        public static bool _IsInit = false;
        public static bool IsInit
        {
            get
            {
                if (DataCore.CurrentUser != null)
                {
                    var er = CurrentContext.Common.CurrentLoginUserInfo;
                    if (DataCore.CurrentUser.Value.ToString() != er.EmployeeID)
                    {
                        _IsInit = false;
                    }
                }
                return _IsInit;
            }
            set
            {
                _IsInit = value;
            }
        }
        static DataCore()
        {
            //    GetOrganization();
            //test
            //List<T_SYS_DICTIONARY> listDict = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            //var result = listDict.FindAll(item =>
            //{
            //    return item.DICTIONARYNAME == "生效";
            //});
            //List<T_SYS_DICTIONARY> l=result.ToList();
            ListDict = InitDictValue();
            DictManager.OnDictionaryLoadCompleted += new EventHandler<OnDictionaryLoadArgs>(DictManager_OnDictionaryLoadCompleted);
        }

        private static List<string> InitDictValue()
        {
            List<string> strList = new List<string>();

            strList.Add("CHECKSTATE");
            strList.Add("PayType");
            strList.Add("BudgetAccountType");
            strList.Add("BudgetType");
            strList.Add("BudgetChargeType");
            strList.Add("BorrowType");
            strList.Add("RepayType");
            strList.Add("ControlType");
            strList.Add("BudgetSumStates");

            return strList;
        }

        public static List<T_SYS_DICTIONARY> GetDictionary(string dictionCategory)
        {
            List<T_SYS_DICTIONARY> listDict = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;

            var result = listDict.FindAll(item =>
                {
                    return item.DICTIONCATEGORY == dictionCategory;
                });
            return result.ToList();
        }
        public static List<VirtualCompany> CompanyList { get; set; }
        public static T_FB_SYSTEMSETTINGS SystemSetting { get; set; }
        public static DateTime SystemDateTime { get; set; }
        #region 初始化数据

        public static event EventHandler<ActionCompletedEventArgs<string>> InitDataCoreCompleted;
        public static void InitDataCore()
        {
            if (SMT.SAAS.Main.CurrentContext.Common.CurrentConfig == null)
            {
                throw new Exception("没有登录信息");
            }
            CompanyList = new List<VirtualCompany>();
            QueryExpression qe = new QueryExpression();
            qe.QueryType = "InitDataCore";
            //InitHR(); --2011年6月21日注释，原因是：新平台组织架构缓存是按需加载，因此不能直接使用了
            FBEntityService service = new FBEntityService();
            service.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(service_QueryFBEntitiesCompleted);
            service.QueryFBEntities(qe);

            SuperUser = new EmployeerData();
            SuperUser.Company = new CompanyData();
            SuperUser.Department = new DepartmentData();
            SuperUser.Post = new PostData();
            SuperUser.Company.Value = "001";
            SuperUser.Company.Text = "001";
            SuperUser.Department.Value = "001";
            SuperUser.Department.Text = "001";
            SuperUser.Post.Value = "001";
            SuperUser.Post.Text = "001";
            SuperUser.Value = "001";
            SuperUser.Text = "001";

        }

        //
        //static void service_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        //{

        //    // List<FBEntity> listVC = GetCompany();// e.Result[0].GetRelationFBEntities(typeof(VirtualCompany).Name).ToList();

        //    SystemSetting = e.Result[0].GetRelationFBEntities(typeof(T_FB_SYSTEMSETTINGS).Name)[0].Entity as T_FB_SYSTEMSETTINGS;
        //    SystemDateTime = SystemSetting.UPDATEDATE.Value;

        //    RefreshData();

        //}

        static void service_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IList<CompanyData> listCompany = new List<CompanyData>();
                IList<DepartmentData> listDepartment = new List<DepartmentData>();
                IList<PostData> listPost = new List<PostData>();


                List<FBEntity> listVC = e.Result[0].GetRelationFBEntities(typeof(VirtualCompany).Name).ToList();
                SystemSetting = e.Result[0].GetRelationFBEntities(typeof(T_FB_SYSTEMSETTINGS).Name)[0].Entity as T_FB_SYSTEMSETTINGS;
                SystemDateTime = SystemSetting.UPDATEDATE.Value;
                listVC.ForEach(companyFB =>
                {
                    VirtualCompany vc = companyFB.Entity as VirtualCompany;
                    CompanyData cData = CreateItem<CompanyData>(vc);
                    List<DepartmentData> listDepartmentData = new List<DepartmentData>();

                    vc.DepartmentCollection.ToList().ForEach(departmentFB =>
                    {
                        DepartmentData dData = CreateItem<DepartmentData>(departmentFB);
                        List<PostData> listPostData = new List<PostData>();
                        departmentFB.PostCollection.ToList().ForEach(postFB =>
                        {
                            PostData pData = CreateItem<PostData>(postFB);
                            pData.Company = cData;
                            pData.Department = dData;
                            listPostData.Add(pData);
                            listPost.Add(pData);
                        });

                        dData.Company = cData;
                        dData.PostCollection = listPostData;
                        listDepartmentData.Add(dData);
                        listDepartment.Add(dData);

                    });

                    cData.DepartmentCollection = listDepartmentData;
                    listCompany.Add(cData);
                    CompanyList.Add(vc);

                });
                ReferencedData<CompanyData>.RefData = listCompany;
                ReferencedData<DepartmentData>.RefData = listDepartment;
                ReferencedData<PostData>.RefData = listPost;

                RefreshData();
            }
        }

        private static T CreateItem<T>(VirtualEntityObject veo) where T : ITextValueItem
        {
            T t = Activator.CreateInstance<T>();
            t.Text = veo.Name;
            t.Value = veo.ID;
            return t;
        }

        static void DictManager_OnDictionaryLoadCompleted(object sender, OnDictionaryLoadArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    ReferencedData<OrderStatusData>.RefData = GetOrderStatus();
                    ReferencedData<PayTypeData>.RefData = GetPayType();
                    ReferencedData<RepayTypeData>.RefData = GetRepayTypeData();
                    ReferencedData<BorrowTypeData>.RefData = GetBorrowTypeData();
                    //ReferencedData<BudgetAccountTypeData>.RefData = GetBudgetAccountTypeData();

                    ReferencedData<QueryData>.RefData = GetQueryList();

                    ReferencedData<LoginUserData>.RefData = new List<LoginUserData> { DataCore.CurrentUser };

                    ReferencedData<ControlTypeData>.RefData = GetControlTypeData();
                    ReferencedData<BudgetChargeTypeData>.RefData = GetBudgetChargeTypeData();
                    ReferencedData<BudgetTypeData>.RefData = GetBudgetTypeData();
                    //beyond
                    ReferencedData<BudgetSumStatesData>.RefData = GetBudgetSumStates();

                    ReferencedData<MonthItem>.RefData = GetMonthItems();

                    ReferencedData<YearItem>.RefData = GetYearItems();

                    IsInit = true;
                    if (InitDataCoreCompleted != null)
                    {
                        InitDataCoreCompleted(null, null);
                    }
                }
                catch
                {
                    if (InitDataCoreCompleted != null)
                    {
                        InitDataCoreCompleted(null, new ActionCompletedEventArgs<string>("读取个人信息异常，无法加载当前页面"));
                    }
                    IsInit = false;

                }
            }
        }


        public static void RefreshData()
        {
            string strMsg = string.Empty;
            try
            {
                GetLoginUserInfo(ref strMsg);

                if (!string.IsNullOrWhiteSpace(strMsg))
                {
                    InitDataCoreCompleted(null, new ActionCompletedEventArgs<string>("读取个人信息异常，无法加载当前页面,错误信息为：" + strMsg));
                    IsInit = false;
                    return;
                }

                DictManager.LoadDictionary(ListDict);
            }
            catch
            {
                if (InitDataCoreCompleted != null)
                {
                    InitDataCoreCompleted(null, new ActionCompletedEventArgs<string>("读取个人信息异常，无法加载当前页面"));
                }
                IsInit = false;

            }
        }

        private static IList<PayTypeData> GetPayType()
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary("PayType");

            var result = listDict.CreateList(item =>
                {
                    PayTypeData cbbD = new PayTypeData();
                    cbbD.Value = item.DICTIONARYVALUE;
                    cbbD.Text = item.DICTIONARYNAME;
                    return cbbD;
                });
            return result;
        }


        private static IList<BudgetAccountTypeData> GetBudgetAccountTypeData()
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary("BudgetAccountType");

            var result = listDict.CreateList(item =>
            {
                BudgetAccountTypeData cbbD = new BudgetAccountTypeData();
                cbbD.Value = item.DICTIONARYVALUE;
                cbbD.Text = item.DICTIONARYNAME;
                return cbbD;
            });
            return result;
        }
        private static IList<BudgetTypeData> GetBudgetTypeData()
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary("BudgetType");

            var result = listDict.CreateList(item =>
            {
                BudgetTypeData cbbD = new BudgetTypeData();
                cbbD.Value = item.DICTIONARYVALUE;
                cbbD.Text = item.DICTIONARYNAME;
                return cbbD;
            });
            return result;
        }
        private static IList<BudgetChargeTypeData> GetBudgetChargeTypeData()
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary("BudgetChargeType");

            var result = listDict.CreateList(item =>
            {
                BudgetChargeTypeData cbbD = new BudgetChargeTypeData();
                cbbD.Value = item.DICTIONARYVALUE;
                cbbD.Text = item.DICTIONARYNAME;
                return cbbD;
            });
            return result;
        }
        private static IList<BorrowTypeData> GetBorrowTypeData()
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary("BorrowType");

            var result = listDict.CreateList(item =>
            {
                BorrowTypeData cbbD = new BorrowTypeData();
                cbbD.Value = item.DICTIONARYVALUE;
                cbbD.Text = item.DICTIONARYNAME;
                return cbbD;
            });
            return result;
        }

        private static IList<RepayTypeData> GetRepayTypeData()
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary("RepayType");

            var result = listDict.CreateList(item =>
            {
                RepayTypeData cbbD = new RepayTypeData();
                cbbD.Value = item.DICTIONARYVALUE;
                cbbD.Text = item.DICTIONARYNAME;
                return cbbD;
            });
            return result;
        }

        private static IList<ControlTypeData> GetControlTypeData()
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary("ControlType");

            var result = listDict.CreateList(item =>
            {
                ControlTypeData cbbD = new ControlTypeData();
                cbbD.Value = item.DICTIONARYVALUE;
                cbbD.Text = item.DICTIONARYNAME;
                return cbbD;
            });
            return result;
        }

        public static IList<OrderStatusData> GetOrderStatus()
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary("CHECKSTATE");

            var result = listDict.CreateList(item =>
            {
                OrderStatusData cbbD = new OrderStatusData();
                cbbD.Value = item.DICTIONARYVALUE;
                cbbD.Text = item.DICTIONARYNAME;
                return cbbD;
            });
            return result;
        }

        public static IList<MonthItem> GetMonthItems()
        {
            DateTime dtSystem = SystemSetting.UPDATEDATE.Value;
            dtSystem = dtSystem.AddDays(1 - dtSystem.Day);
            List<MonthItem> listDict = new List<MonthItem>();
            MonthItem item = new MonthItem();
            item.Text = dtSystem.ToString("yyyy年MM月");
            item.Value = dtSystem;

            MonthItem item2 = new MonthItem();
            item2.Text = dtSystem.AddMonths(1).ToString("yyyy年MM月");
            item2.Value = dtSystem.AddMonths(1);
            listDict.Add(item);
            listDict.Add(item2);
            return listDict;
        }

        public static IList<YearItem> GetYearItems()
        {
            DateTime dtSystem = SystemSetting.UPDATEDATE.Value;
            List<YearItem> listDict = new List<YearItem>();
            YearItem item = new YearItem();
            item.Text = dtSystem.Year.ToString() + "年";
            item.Value = dtSystem.Year;

            YearItem item2 = new YearItem();
            item2.Text = dtSystem.AddYears(1).Year.ToString() + "年";
            item2.Value = dtSystem.AddYears(1).Year;
            listDict.Add(item);
            listDict.Add(item2);
            return listDict;
        }



        #region beyond
        public static IList<BudgetSumStatesData> GetBudgetSumStates()
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary("BudgetSumStates");

            var result = listDict.CreateList(item =>
            {
                BudgetSumStatesData cbbD = new BudgetSumStatesData();
                cbbD.Value = item.DICTIONARYVALUE;
                cbbD.Text = item.DICTIONARYNAME;
                return cbbD;
            });
            return result;
        }
        #endregion
        public static IList<QueryData> GetQueryList()
        {
            List<QueryData> list = new List<QueryData>();

            QueryData q1 = new QueryData();
            q1.Text = "我的单据";
            q1.Value = "MyOrder";
            list.Add(q1);

            QueryExpression qe1 = new QueryExpression();
            qe1.PropertyName = "CreateUserID";
            qe1.PropertyValue = DataCore.CurrentUser.Value.ToString();
            qe1.Operation = QueryExpression.Operations.Equal;

            QueryExpression qe2 = new QueryExpression();
            qe2.PropertyName = "OwnerID";
            qe2.PropertyValue = DataCore.CurrentUser.Value.ToString();
            qe2.Operation = QueryExpression.Operations.Equal;
            qe2.RelatedType = QueryExpression.RelationType.And;
            qe2.RelatedExpression = qe1;

            q1.QueryExpression = qe2;


            QueryData q2 = new QueryData();
            q2.Text = "所有单据";
            q2.Value = "AllOrder";
            list.Add(q2);

            return list;
        }

        public static void GetLoginUserInfo(ref string strMsg)
        {

            DataCore.CurrentUser = new LoginUserData();
            List<LoginUserData> listLogin = new List<LoginUserData>();

            string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            string userName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            try
            {
                listLogin = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.CreateList(userPost =>
                     {
                         LoginUserData eData = new LoginUserData();

                         eData.Value = userID;
                         eData.Text = userName;

                         eData.Post = ReferencedData<PostData>.Find(userPost.PostID);

                         // 处理缓存不存在登录人的岗位
                         if (eData.Post == null)
                         {
                             PostData newPostData = new PostData();
                             //公司
                             CompanyData newComData = new CompanyData();
                             newComData.Text = userPost.CompanyName;
                             newComData.Value = userPost.CompanyID;

                             newPostData.Company = newComData;
                             // 判断和添加 部门
                             DepartmentData newDepData = new DepartmentData();
                             newDepData.Text = userPost.DepartmentName;
                             newDepData.Value = userPost.DepartmentID;

                             newPostData.Department = newDepData;

                             // 添加岗位
                             newPostData.Text = userPost.PostName;
                             newPostData.Value = userPost.PostID;

                             ReferencedData<PostData>.RefData.Add(newPostData);
                             eData.Post = newPostData;
                         }
                         eData.Company = eData.Post.Company;
                         eData.Department = eData.Post.Department;
                         return eData;
                     });
                listLogin.ForEach(item =>
                    {
                        item.PostInfos = listLogin;
                    });

                DataCore.CurrentUser = listLogin.FirstOrDefault();

                ReferencedData<LoginUserData>.RefData = listLogin;
            }
            catch (Exception ex)
            {
                strMsg = ex.ToString();
            }
        }

        public static IList<ITextValueItem> GetTextValueItem(string dictType)
        {
            List<T_SYS_DICTIONARY> listDict = GetDictionary(dictType);

            var result = listDict.CreateList(item =>
            {
                ITextValueItem cbbD = new TextValueItemBase();
                cbbD.Value = item.DICTIONARYVALUE;
                cbbD.Text = item.DICTIONARYNAME;
                return cbbD;
            });
            return result;
        }
        #endregion

        #region RefenecedData
        public static ITextValueItem FindReferencedData<T>(object value) where T : ITextValueItem
        {
            T t = ReferencedData<T>.Find(value);
            return t as ITextValueItem;
        }

        public static IList<ITextValueItem> GetReferencedData<T>() where T : ITextValueItem
        {
            IList<ITextValueItem> listR = new List<ITextValueItem>();

            //if (typeof(T) == typeof(LoginUserData))
            //{
            //    IList<EmployeerData> listTa = DataCore.CurrentUser.PostInfo;
            //    if (listTa != null)
            //    {
            //        for (int i = 0; i < listTa.Count; i++)
            //        {
            //            listR.Add(listTa[i]);
            //        }
            //    }
            //    return listR;

            //}

            IList<T> listT = ReferencedData<T>.RefData;
            if (listT != null)
            {
                for (int i = 0; i < listT.Count; i++)
                {
                    listR.Add(listT[i]);
                }
            }
            return listR;
        }

        public static IList<ITextValueItem> GetReferencedQuery<T>() where T : ITextValueItem
        {
            Type type = typeof(T);

            IList<ITextValueItem> list = GetReferencedData<T>();
            list.Insert(0, DataCore.AllSelectdData);
            return list;
        }

        public static IList<ITextValueItem> GetRefData(string referencedDataType)
        {
            Type t = CommonFunction.GetType(referencedDataType, CommonFunction.TypeCategory.ReferencedData);
            MethodInfo myMethod = typeof(DataCore).GetMethods().First(m => m.Name.Equals("GetReferencedData") && m.IsGenericMethod);
            myMethod = myMethod.MakeGenericMethod(t);
            object result = myMethod.Invoke(null, null);
            return result as IList<ITextValueItem>;
        }

        public static ITextValueItem FindRefData(string referencedDataType, object value)
        {
            if (referencedDataType == typeof(DateTimeData).Name)
            {
                DateTimeData dtData = new DateTimeData();
                dtData.Value = value;
                return dtData;
            }
            Type t = CommonFunction.GetType(referencedDataType, CommonFunction.TypeCategory.ReferencedData);
            MethodInfo myMethod = typeof(DataCore).GetMethods().First(m => m.Name.Equals("FindReferencedData") && m.IsGenericMethod);
            myMethod = myMethod.MakeGenericMethod(t);
            object result = myMethod.Invoke(null, new object[] { value });
            return result as ITextValueItem;
        }



        private static ITextValueItem allSelectdData = null;
        public static ITextValueItem AllSelectdData
        {
            get
            {
                if (allSelectdData == null)
                {
                    allSelectdData = new TextValueItemBase();
                    allSelectdData.Text = "所有";
                    allSelectdData.Value = "";

                }
                return allSelectdData;
            }
        }
        #endregion

        public static LoginUserData CurrentUser { get; set; }

        public static EmployeerData SuperUser { get; set; }

        #region HR数据 ---方法作废：2011年6月21日注释，原因是：新平台组织架构缓存是按需加载，因此不能直接使用了

        //public static void InitHR()
        //{
        //    try
        //    {
        //        IList<CompanyData> listCData = new List<CompanyData>();
        //        IList<DepartmentData> ListDData = new List<DepartmentData>();
        //        IList<PostData> ListPData = new List<PostData>();

        //        List<OrganizationWS.T_HR_COMPANY> comList = (List<OrganizationWS.T_HR_COMPANY>)App.Current.Resources["SYS_CompanyInfo"];
        //        List<OrganizationWS.T_HR_DEPARTMENT> deptList = (List<OrganizationWS.T_HR_DEPARTMENT>)App.Current.Resources["SYS_DepartmentInfo"];
        //        List<OrganizationWS.T_HR_POST> postList = (List<OrganizationWS.T_HR_POST>)App.Current.Resources["SYS_PostInfo"];


        //        comList.ForEach(comHR =>
        //        {
        //            ObservableCollection<VirtualDepartment> listDepartment = new ObservableCollection<VirtualDepartment>();

        //            VirtualCompany vc = new VirtualCompany();
        //            vc.ID = comHR.COMPANYID;
        //            vc.Name = comHR.CNAME;
        //            vc.DepartmentCollection = listDepartment;


        //            CompanyData cData = CreateItem<CompanyData>(vc);
        //            List<DepartmentData> listDepartmentData = new List<DepartmentData>();

        //            List<OrganizationWS.T_HR_DEPARTMENT> deptListPart = deptList.FindAll(item =>
        //            {
        //                if (item.T_HR_COMPANY == null)
        //                {
        //                    return false;
        //                }
        //                return item.T_HR_COMPANY.COMPANYID == comHR.COMPANYID;

        //            });

        //            deptListPart.ForEach(deptHR =>
        //            {
        //                ObservableCollection<VirtualPost> listPost = new ObservableCollection<VirtualPost>();

        //                VirtualDepartment vd = new VirtualDepartment();
        //                vd.ID = deptHR.DEPARTMENTID;
        //                vd.Name = deptHR.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //                vd.VirtualCompany = vc;
        //                vd.PostCollection = listPost;
        //                listDepartment.Add(vd);


        //                DepartmentData dData = CreateItem<DepartmentData>(vd);
        //                List<PostData> listPostData = new List<PostData>();

        //                List<OrganizationWS.T_HR_POST> postListPart = postList.FindAll(item =>
        //                {
        //                    if (item.T_HR_DEPARTMENT == null)
        //                    {
        //                        return false;
        //                    }
        //                    return item.T_HR_DEPARTMENT.DEPARTMENTID == deptHR.DEPARTMENTID;
        //                });

        //                postListPart.ForEach(postHR =>
        //                {
        //                    VirtualPost vp = new VirtualPost();
        //                    vp.ID = postHR.POSTID;
        //                    vp.Name = postHR.T_HR_POSTDICTIONARY.POSTNAME;
        //                    vp.VirtualCompany = vc;
        //                    vp.VirtualDepartment = vd;
        //                    listPost.Add(vp);


        //                    PostData pData = CreateItem<PostData>(vp);
        //                    pData.Company = cData;
        //                    pData.Department = dData;
        //                    listPostData.Add(pData);
        //                    ListPData.Add(pData);

        //                });

        //                dData.Company = cData;
        //                dData.PostCollection = listPostData;
        //                listDepartmentData.Add(dData);
        //                ListDData.Add(dData);
        //            });

        //            cData.DepartmentCollection = listDepartmentData;
        //            listCData.Add(cData);
        //            CompanyList.Add(vc);
        //        });

        //        ReferencedData<CompanyData>.RefData = listCData;
        //        ReferencedData<DepartmentData>.RefData = ListDData;
        //        ReferencedData<PostData>.RefData = ListPData;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex);
        //        throw new Exception("调用HR服务异常", ex);
        //    }
        //}

        #endregion

    }
    #endregion

    #region DataQueryer

    public class PageQueryer : FBEntityService
    {

        public PageQueryer()
        {
        }

        private PageExpression _Pager;
        public PageExpression Pager
        {
            get
            {
                if (_Pager == null)
                {
                    _Pager = new PageExpression();
                    _Pager.PageIndex = 1;
                    _Pager.PageSize = 25;
                    _Pager.PreRowCount = 25;
                }
                return _Pager;
            }
            set
            {
                _Pager = value;
            }
        }
        public QueryExpression QueryExpression { get; set; }

        protected override void OnQueryCompleted(QueryCompletedEventArgs e)
        {
            this.Pager = e.Result.Pager;
            base.OnQueryCompleted(e);
        }


        public void Query()
        {
            if (this.QueryExpression == null)
            {
                throw new ArgumentException("参数不能为空", "QueryExpression");
            }
            this.QueryExpression.Pager = this.Pager;

            if (Querying != null)
            {
                Querying(this, null);
            }
            this.Query(this.QueryExpression);
        }

        public event EventHandler Querying;


    }
    #endregion


}
