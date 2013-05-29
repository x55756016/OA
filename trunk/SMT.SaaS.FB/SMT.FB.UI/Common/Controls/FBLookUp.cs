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
using SMT.SaaS.FrameworkUI.OrganizationControl;
using CurrentContext = SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.Common;
using System.Collections.Generic;
using SMT.FB.UI.FBCommonWS;

using System.Linq;
using SMT.SaaS.FrameworkUI;
namespace SMT.FB.UI
{
    public class FBLookUp : SMT.SaaS.FrameworkUI.LookUp
    {
        public FBLookUp()
        {
            this.FindClick += new EventHandler(FBLookUp_FindClick);
        }

        void FBLookUp_FindClick(object sender, EventArgs e)
        {
            Init();
        }
        public ReferencedDataInfo ReferencedDataInfo { get; set; }

        public OperationTypes OperationType { get; set; }

        public void Init()
        {
            if (ReferencedDataInfo == null)
            {
                return;
            }

            switch (ReferencedDataInfo.Type)
            {
                case "EmployeerData":
                case "DepartmentData":
                case "CompanyData":
                case "PostData":
                    InitOrganization();
                    break;
                case "BorrowOrderData":
                    InitBorrowOrder();
                    break;
                case "MyOrgObjectData":
                    InitOrgObjectData();
                    break;
            }
        }

        public void InitOrganization()
        {

            string perm = "3";
            string entity = ReferencedDataInfo.OrderInfo.Type;
            if (OperationType == OperationTypes.Edit)
            {
                perm = ((int)Permissions.Edit).ToString();
            }
            else if (OperationType == OperationTypes.Add)
            {
                perm = ((int)Permissions.Add).ToString();
            }
            else
            {
                perm = ((int)Permissions.Browse).ToString();
            }

            string userID = DataCore.CurrentUser.Value.ToString();
            //             BF06E969-1B2C-4a89-B0AE-A91CA1244053
            OrganizationLookup ogzLookup = new OrganizationLookup(userID, perm, entity);

            if (ReferencedDataInfo.Type == typeof(CompanyData).Name)
            {
                ogzLookup.SelectedObjType = OrgTreeItemTypes.Company;
            }
            else if (ReferencedDataInfo.Type == typeof(EmployeerData).Name)
            {
                ogzLookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            }
            else if (ReferencedDataInfo.Type == typeof(DepartmentData).Name)
            {
                ogzLookup.SelectedObjType = OrgTreeItemTypes.Department;
            }
            else if (ReferencedDataInfo.Type == typeof(OrgObjectData).Name)
            {
                ogzLookup.SelectedObjType = OrgTreeItemTypes.All;
            }

            FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;

            ogzLookup.SelectedClick += (o, e) =>
            {
                if (ogzLookup.SelectedObj.Count > 0)
                {
                    ITextValueItem item = null;
                    string id = ogzLookup.SelectedObj[0].ObjectID;
                    if (ReferencedDataInfo.Type == typeof(EmployeerData).Name)
                    {
                        EmployeerData eData = new EmployeerData();
                        eData.Value = ogzLookup.SelectedObj[0].ObjectID;
                        eData.Text = ogzLookup.SelectedObj[0].ObjectName;
                        ExtOrgObj post = ogzLookup.SelectedObj[0].ParentObject as ExtOrgObj;
                        ExtOrgObj dept = post.ParentObject as ExtOrgObj;

                        // ExtOrgObj com = dept.ParentObject as ExtOrgObj;
                        ITextValueItem pdata = DataCore.FindReferencedData<PostData>(post.ObjectID);
                        ITextValueItem ddata = DataCore.FindReferencedData<DepartmentData>(dept.ObjectID);
                        ITextValueItem cdata = (ddata as DepartmentData).Company;

                        eData.Company = cdata as CompanyData;
                        eData.Department = ddata as DepartmentData;
                        eData.Post = pdata as PostData;
                        item = eData;
                    }
                    else
                    {
                        item = DataCore.FindRefData(ReferencedDataInfo.Type, id);
                    }
                    this.SelectItem = item;
                }
            };
            ogzLookup.Show<string>(DialogMode.ApplicationModal, plRoot, "", (result) => { });

        }

        public void InitBorrowOrder()
        {
            string entity = ReferencedDataInfo.OrderInfo.Type;

            OrderEntity orderEntity = this.DataContext as OrderEntity;
            QueryExpression qeTop = null;

            string strEascapeDepIds = "c1dc9c03-31a2-4f0c-be0b-459fdb06b851,4ede37fa-72b5-4939-b087-10bef4520d49,815daaee-b439-4695-874d-ad79ba361add,"
                + "0027e9e6-c645-48ec-9333-a2ef507faf57,0abaf119-a470-44ee-8a9e-2e42783b4c86,45995e7d-6062-49e6-b08f-1da59729f9f1";

            string strCurDepId = string.Empty;

            foreach (KeyValuePair<string, string> dict in Parameters)
            {
                object objvalue = orderEntity.GetEntityValue(dict.Value);
                if (dict.Key == "OWNERDEPARTMENTID" && objvalue != null)
                {
                    strCurDepId = objvalue.ToString();
                    continue;
                }

                if (dict.Key == "OWNERPOSTID")
                {
                    if (strEascapeDepIds.Contains(strCurDepId))
                    {
                        continue;
                    }
                }
                QueryExpression qe = QueryExpressionHelper.Equal(dict.Key, objvalue.ToString());
                qe.RelatedExpression = qeTop;
                qeTop = qe;
            }

            // 未偿还
            QueryExpression qeIsRepaied = QueryExpressionHelper.Equal("ISREPAIED", "1");
            qeIsRepaied.Operation = QueryExpression.Operations.NotEqual;
            qeIsRepaied.RelatedExpression = qeTop;
            qeTop = qeIsRepaied;

            if (qeTop != null)
            {

                qeTop.VisitAction = ((int)this.OperationType).ToString();
                qeTop.QueryType = entity;
                qeTop.VisitModuleCode = entity;
                qeTop.Include = new System.Collections.ObjectModel.ObservableCollection<string>();
                qeTop.Include.Add("T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPE");
                qeTop.Include.Add("T_FB_BORROWAPPLYDETAIL.T_FB_SUBJECT");
            }

            SelectedDataManager.QueryExpression = qeTop;
            SelectedForm sf = new SelectedForm();
            sf.TitleContent = "借款单";
            sf.SelectedDataManager = this.SelectedDataManager;
            sf.ReferenceDataType = this.ReferencedDataInfo.Type;
            sf.SelectionMode = DataGridSelectionMode.Single;
            sf.SelectedCompleted += (o, e) =>
                {
                    FBEntity fbEntity = sf.SelectedItems.FirstOrDefault();
                    Type type = CommonFunction.GetType(this.ReferencedDataInfo.Type, CommonFunction.TypeCategory.ReferencedData);
                    ITextValueItem item = Activator.CreateInstance(type) as ITextValueItem;
                    item.Value = fbEntity.Entity;
                    this.SelectItem = item;
                };
            sf.Show();

        }

        public void InitOrgObjectData()
        {

            string perm = "3";
            string entity = ReferencedDataInfo.OrderInfo.Type;
            if (OperationType == OperationTypes.Edit)
            {
                perm = ((int)Permissions.Edit).ToString();
            }
            else if (OperationType == OperationTypes.Add)
            {
                perm = ((int)Permissions.Add).ToString();
            }
            else
            {
                perm = ((int)Permissions.Browse).ToString();
            }

            string userID = DataCore.CurrentUser.Value.ToString();
            //             BF06E969-1B2C-4a89-B0AE-A91CA1244053
            OrganizationLookup ogzLookup = new OrganizationLookup(userID, perm, entity);

            ogzLookup.SelectedObjType = OrgTreeItemTypes.Company;

            FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;

            ogzLookup.SelectedClick += (o, e) =>
            {
                if (ogzLookup.SelectedObj.Count > 0)
                {
                    ITextValueItem item = null;

                    var sItem = ogzLookup.SelectedObj[0];

                    //下拨公司只能是公司，其他机构类型都不可以
                    if (sItem.ObjectType == OrgTreeItemTypes.Company)
                    {
                        string id = sItem.ObjectID;
                        ITextValueItem cdata = DataCore.FindReferencedData<CompanyData>(id);
                        item = cdata;
                    }

                    #region 废弃代码
                    //if (sItem.ObjectType == OrgTreeItemTypes.Personnel)
                    //{
                    //    EmployeerData eData = new EmployeerData();
                    //    eData.Value = sItem.ObjectID;
                    //    eData.Text = sItem.ObjectName;
                    //    ExtOrgObj post = sItem.ParentObject as ExtOrgObj;
                    //    ExtOrgObj dept = post.ParentObject as ExtOrgObj;

                    //    // ExtOrgObj com = dept.ParentObject as ExtOrgObj;
                    //    ITextValueItem pdata = DataCore.FindReferencedData<PostData>(post.ObjectID);
                    //    ITextValueItem ddata = DataCore.FindReferencedData<DepartmentData>(dept.ObjectID);
                    //    ITextValueItem cdata = (ddata as DepartmentData).Company;

                    //    eData.Company = cdata as CompanyData;
                    //    eData.Department = ddata as DepartmentData;
                    //    eData.Post = pdata as PostData;
                    //    item = eData;
                    //}
                    //else if (sItem.ObjectType == OrgTreeItemTypes.Department)
                    //{
                    //    string id = sItem.ObjectID;
                    //    item = DataCore.FindReferencedData<DepartmentData>(id);
                    //}
                    #endregion 废弃代码


                    MyOrgObjectData sValue = null;
                    if (item != null)
                    {
                        sValue = new MyOrgObjectData { OrgObject = item as OrgObjectData };
                    }
                    this.SelectItem = sValue;
                }
            };
            ogzLookup.Show<string>(DialogMode.ApplicationModal, plRoot, "", (result) => { });
        }

        public Dictionary<string, string> Parameters { get; set; }

        private SelectedDataManager selectedDataManager;
        public SelectedDataManager SelectedDataManager
        {
            get
            {
                if (selectedDataManager == null)
                {
                    selectedDataManager = new SelectedDataManager();
                }
                return selectedDataManager;
            }
        }

        public bool _IsReadOnly = false;
        public bool IsReadOnly
        {
            get
            {
                return _IsReadOnly;
            }
            set
            {
                if (_IsReadOnly != value)
                {
                    _IsReadOnly = value;
                    this.TxtLookUp.IsReadOnly = _IsReadOnly;
                    this.SearchButton.IsEnabled = !_IsReadOnly;
                }
            }
        }
    }


    
}
