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
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using SMT.FB.UI.FBCommonWS;
using System.Threading;
using System.Reflection;
using System.ComponentModel;
using SMT.FB.UI.Common.Controls;
using SMT.SaaS.FrameworkUI.Validator;

namespace SMT.FB.UI.Common
{

    public class OrderInitManager
    {
        public event EventHandler InitCompleted;
        public void Init()
        {
            OrderHelper.InitOrderInfoCompleted += new EventHandler<ActionCompletedEventArgs<string>>(OrderHelper_InitOrderInfoCompleted);
            OrderHelper.InitOrderInfo();
        }

        void OrderHelper_InitOrderInfoCompleted(object sender, ActionCompletedEventArgs<string> e)
        {
            OrderHelper.InitOrderInfoCompleted -= new EventHandler<ActionCompletedEventArgs<string>>(OrderHelper_InitOrderInfoCompleted);
            if (InitCompleted != null)
            {
                InitCompleted(this, null);
            }
        }        
    }

    public class SelectorHelper
    {
        public static List<SelectorInfo> Selectors;
        public static void InitSelectorInfo()
        {
            if (!DataCore.IsInit)
            {
                DataCore.InitDataCoreCompleted += (o, e) =>
                {
                    if (e != null)
                    {
                        if (InitSelectorInfoCompleted != null)
                        {
                            InitSelectorInfoCompleted(null, e);
                        }
                    }
                    InnerInitSelectorInfo();
                };
                DataCore.InitDataCore();
            }
            else
            {
                InnerInitSelectorInfo();
            }

        }

        ///// <summary>
        ///// 通过 queryType 返回对应的 SelectorInfo.
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public static SelectorInfo GetSelectorInfo(string type)
        //{
        //    var selectorInfo = Selectors.FirstOrDefault(item =>
        //    {
        //        return item.Type == type;
        //    });
        //    return selectorInfo;
        //}

        /// <summary>
        /// 通过 referenceType 返回对应的 SelectorInfo.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SelectorInfo GetSelectorInfo(string refType)
        {
            var selectorInfo = Selectors.FirstOrDefault(item =>
            {
                return item.ReferencedDataType == refType;
            });
            return selectorInfo;
        }


        private static void InnerInitSelectorInfo()
        {
            if (Selectors == null)
            {
                SelectorXml ox = new SelectorXml();
                ox.InitSelectorInfoCompleted += (o, e) =>
                {
                    Selectors = ox.SelectorInfos;

                    if (InitSelectorInfoCompleted != null)
                    {
                       
                        InitSelectorInfoCompleted(null, null);
                    }
                };
                ox.InitSelectorInfo();
            }
            else
            {
                if (InitSelectorInfoCompleted != null)
                {
                    InitSelectorInfoCompleted(null, null);
                }
            }
        }
        public static event EventHandler InitSelectorInfoCompleted;
    }
    public class OrderHelper
    {

        public static List<OrderInfo> OrderInfos
        {
            get;
            set;
        }

        public static void InitOrderInfo()
        {
            if (!DataCore.IsInit)
            {
                DataCore.InitDataCoreCompleted += (o, e) =>
                    {
                        if (e != null)
                        {
                            if (InitOrderInfoCompleted != null)
                            {
                                InitOrderInfoCompleted(null, e);
                            }
                        }

                        InnerInitOrderInfo();
                    };
                DataCore.InitDataCore();
            }
            else
            {
                InnerInitOrderInfo();
            }
           
        }

        private static void InnerInitOrderInfo()
        {
            if (OrderInfos == null)
            {
                OrderXml ox = new OrderXml();
                ox.InitOrderInfoCompleted += (o, e) =>
                {
                    OrderInfos = ox.OrderInfos;

                    if (InitOrderInfoCompleted != null)
                    {
                        InitOrderInfoCompleted(null, null);
                    }
                };
                ox.InitOrderInfo();
            }
            else
            {
                if (InitOrderInfoCompleted != null)
                {
                    InitOrderInfoCompleted(null, null);
                }
            }
        }

        /// <summary>
        /// 通过 Entity 返回对应的 OrderInfo.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static OrderInfo GetOrderInfo(EntityObject entity)
        {
            var orderInfo = XMLHelper.Orders.FirstOrDefault(item =>
            {
                return item.OrderEntity.Entity.Type == entity.GetType().Name;
            });
            return orderInfo;
        }
        /// <summary>
        /// 通过 OrderType 返回对应的 OrderInfo.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static OrderInfo GetOrderInfo(string orderTypeName)
        {
            OrderInfo orderInfo = OrderInfos.FirstOrDefault(item =>
            {
                return item.Type == orderTypeName;
            });
            return orderInfo;
        }

        public static event EventHandler<ActionCompletedEventArgs<string>> InitOrderInfoCompleted;

        public static QueryExpression GetQueryExpression(QueryExpressionInfo queryExpressionInfo, OrderEntity orderEntity)
        {
            if (queryExpressionInfo == null)
            {
                return null;
            }
            QueryExpression queryExpression = queryExpressionInfo.GetQueryExpression();
            QueryExpression tempExp = queryExpression;
            while (tempExp != null)
            {

                string[] strs = tempExp.PropertyName.Split('.');
                if (strs[0] == "Entity")
                {
                    tempExp.PropertyName = strs[1];
                }

                string propertyValue = tempExp.PropertyValue;
                if (propertyValue.StartsWith("{") && propertyValue.EndsWith("}"))
                {
                    propertyValue = CommonFunction.TryConvertValue(orderEntity.GetObjValue(propertyValue), propertyValue);
                }

                tempExp.PropertyValue = propertyValue;
                tempExp = tempExp.RelatedExpression;
            }
            return queryExpression;
        }

    }

    public class OrderXml
    {
        public List<OrderInfo> OrderInfos { get; set; }
        public void InitOrderInfo()
        {
            //FBCommonWS.FBCommonServiceClient fc = new FBCommonServiceClient();

            //fc.GetXMLCompleted += new EventHandler<GetXMLCompletedEventArgs>(fc_GetXMLCompleted);
            //fc.GetXMLAsync();
            fc_GetXMLCompleted();
        }

        void fc_GetXMLCompleted()
        {
            XMLHelper.InitOrderInfo();
            OrderInfos = XMLHelper.Orders;

            if (InitOrderInfoCompleted != null)
            {
                InitOrderInfoCompleted(this, null);
            }
        }

        public event EventHandler InitOrderInfoCompleted;
    }

    public class SelectorXml
    {
        public List<SelectorInfo> SelectorInfos { get; set; }
        public void InitSelectorInfo()
        {
            FBCommonWS.FBCommonServiceClient fc = new FBCommonServiceClient();

            //fc.GetXMLCompleted += new EventHandler<GetXMLCompletedEventArgs>(fc_GetXMLCompleted);
            //fc.GetXMLAsync();
            fc_GetXMLCompleted();
        }

        void fc_GetXMLCompleted()
        {
            XMLHelper.InitSelectorInfo();
            SelectorInfos = XMLHelper.Selects;

            if (InitSelectorInfoCompleted != null)
            {
                InitSelectorInfoCompleted(this, null);
            }
        }
        public event EventHandler<ActionCompletedEventArgs<string>> InitSelectorInfoCompleted;
    }


    public class XMLHelper
    {
        static XMLHelper()
        {
            Orders = new List<OrderInfo>();
            // Refresh();
        }
        public static List<OrderInfo> Orders { get; set; }
        public static List<SelectorInfo> Selects { get; set; }
        public static event EventHandler RefreshCompleted;
        static OrderInfo RuntimeOrder = null;
        public static void InitOrderInfo()
        {
            XDocument xml = XDocument.Load("/SMT.FB.UI;component/Common/UIXml/UIConfig.xml");

            var orders = from p in xml.Element("FB").Element("Orders").Elements("Order")
                         select new
                         {
                             Name = p.Attribute("Name").Value,
                             Type = p.Attribute("Type").Value,

                             OrderEntity = p.Element("OrderEntity"),
                             View = p.Element("View"),
                             Form = p.Element("Form")
                         };
            foreach (var order in orders)
            {
                OrderInfo orderC = new OrderInfo();
                RuntimeOrder = orderC;
                orderC.Name = order.Name;
                orderC.Type = order.Type;

                orderC.OrderEntity = GetOrderEntityInfo(order.OrderEntity);
                orderC.View = GetViewInfo(order.View);
                orderC.Form = GetOrderForm(order.Form);

                XMLHelper.Orders.Add(orderC);
            }
            if (RefreshCompleted != null)
            {
                RefreshCompleted(null, null);
            }
           
        }
        public static void InitSelectorInfo()
        {
            XDocument xml = XDocument.Load("/SMT.FB.UI;component/Common/UIXml/UIConfig.xml");

            List<SelectorInfo> listResult = new List<SelectorInfo>();
            var selectors = from p in xml.Element("FB").Element("Selectors").Elements("Selector")
                            select p;
            foreach (XElement se in selectors)
            {
                SelectorInfo selectorInfo = new SelectorInfo();
                SetValue(selectorInfo, se);

                selectorInfo.Items = GetGridItem(se);
                listResult.Add(selectorInfo);
            }
            Selects = listResult;
        }


        private static OrderEntityInfo GetOrderEntityInfo(XElement xe)
        {

            OrderEntityInfo orderEntityInfo = new OrderEntityInfo();
            if (xe != null)
            {
                orderEntityInfo.Entity = GetEntityInfo(xe.Element("Entity"));
                orderEntityInfo.OrderDetailEntities = GetOrderDetailEntities(xe.Element("OrderDetailEntities"));
                orderEntityInfo.ReferenceDatas = new ObjectList<ReferencedDataInfo>();
            }
            return orderEntityInfo;
        }

        private static EntityInfo GetEntityInfo(XElement xe)
        {
            EntityInfo entityInfo = new EntityInfo();
            SetValue(entityInfo, xe);

            return entityInfo;
        }

        private static ObjectList<OrderDetailEntityInfo> GetOrderDetailEntities(XElement xe)
        {
            ObjectList<OrderDetailEntityInfo> list = new ObjectList<OrderDetailEntityInfo>();
            if (xe == null)
            {
                return list;
            }
            
            if (xe.Elements("OrderDetailEntity") != null)
            {
                foreach (XElement item in xe.Elements("OrderDetailEntity"))
                {
                    OrderDetailEntityInfo orderDetailEntityInfo = GetOrderDetailEntityInfo(item);
                    list.Add(orderDetailEntityInfo.Name, orderDetailEntityInfo);
                }
            }
            return list;


        }

        private static OrderDetailEntityInfo GetOrderDetailEntityInfo(XElement xe)
        {
            OrderDetailEntityInfo orderDetailEntityInfo = new OrderDetailEntityInfo();
            SetValue(orderDetailEntityInfo, xe);
            orderDetailEntityInfo.Entity = GetEntityInfo(xe.Element("Entity"));
            orderDetailEntityInfo.QueryExpression = GetQueryExpressionInfo(xe.Element("QueryExpression"));
            return orderDetailEntityInfo;
        }

        private static QueryExpressionInfo GetQueryExpressionInfo(XElement xe)
        {
            if (xe == null)
            {
                return null;
            }
            QueryExpressionInfo q = new QueryExpressionInfo();
            SetValue(q, xe);
            QueryExpressionInfo currentQ = q;
            XElement currentXe = xe.Element("RelatedExpression");
            while (currentXe != null)
            {
                currentQ.RelatedExpression = GetQueryExpressionInfo(currentXe);
                currentXe = currentXe.Element("RelatedExpression");
            }
            return q;

        }

        private static ViewInfo GetViewInfo(XElement xe)
        {
            
            ViewInfo view = new ViewInfo();

            if (xe != null)
            {

                XElement queryList = xe.Element("QueryList");

                view.GridItems = GetGridItem(xe);

                if (queryList != null)
                {
                    foreach (XElement xquery in queryList.Elements("Query"))
                    {
                        QueryInfo qi = new QueryInfo();

                        SetValue(qi, xquery);
                        qi.QueryExpression = GetQueryExpressionInfo(xquery.Element("QueryExpression"));

                        view.QueryList.Add(qi);

                    }
                }
               
            }

           
            return view;
        }
        /// <summary>
        /// 根据xml获取控件
        /// </summary>
        /// <param name="xe"></param>
        /// <returns></returns>
        private static FormInfo GetOrderForm(XElement xe)
        {
            FormInfo from = new FormInfo();
            if (xe != null)
            {

                foreach (XElement dp in xe.Elements("DataPanel"))
                {
                    from.Panels.Add(GetIDataPanel(dp));

                }
            }
            return from;
        }

        private static IDataPanel GetIDataPanel(XElement xe)
        {
            switch (xe.Attribute("Type").Value)
            {
                case "DataPanel":
                    return GetDataPanel(xe);

                case "DataPanelTwoSide":
                    return GetDataPanelTwoSide(xe);

                case "OrderGrid":
                    return GetOrderGrid(xe);
                case "DataPanelSplit":
                    DataPanelSplit p = new DataPanelSplit();
                    p.Height = float.Parse(xe.Attribute("Height").Value);
                    return p;
                case "DataPanelGrid":
                    DataPanelGrid grid = new DataPanelGrid();
                    SetValue(grid, xe);
                    return grid;
                case "DataPanelUpload" :
                    return GetDataPanelUpload(xe);
                default:
                    throw new Exception("异常Type");
            }
        }

        private static OrderDetailGridInfo GetOrderGrid(XElement xe)
        {
            OrderDetailGridInfo og = new OrderDetailGridInfo();
            XMLHelper.SetValue(og, xe);
            if (xe.Attribute("OrderDetailEntity") != null)
            {
                string name = xe.Attribute("OrderDetailEntity").Value;
                og.OrderDetailEntityInfo = XMLHelper.RuntimeOrder.RegisterOrderDetailEntity(name);
            }
            XElement xeRowDetail = xe.Element("RowDetailTemplate");
            if (xeRowDetail != null)
            {
                XElement xpanel = xeRowDetail.Element("DataPanel");
                IDataPanel panel = GetIDataPanel(xpanel);
                og.RowDetailDataPanel = panel;
            }
            og.GridItems = GetGridItem(xe);
            og.Parameters = GetParameters(xe);
            
            
            
            return og;
        }

        private static List<GridItem> GetGridItem(XElement xElement)
        {
            xElement = xElement.Element("GridItems");
            List<GridItem> list = new List<GridItem>();
            foreach (XElement xeItem in xElement.Elements("GridItem"))
            {
                GridItem gridItem = new GridItem();
                XMLHelper.SetValue(gridItem, xeItem);
                list.Add(gridItem);

                XElement xeISource = xeItem.Element("ReferencedData");

                if (xeISource != null)
                {
                    string rType = xeISource.Attribute("Type").Value;
                    string rDefaultValue = GetValue(xeISource, "DefaultValue");
                    ReferencedDataInfo r = new ReferencedDataInfo();
                    r.Type = rType;
                    r.DefaultValue = rDefaultValue;
                    r.ReferencedMember = gridItem.PropertyName;
                    gridItem.ReferenceDataInfo = r;
                }

            }
            return list;
        }

        private static DataPanelTwoSideInfo GetDataPanelTwoSide(XElement xe)
        {
            DataPanelTwoSideInfo dpt = new DataPanelTwoSideInfo();
            XElement xeLeft = xe.Element("LeftPanel");
            XElement xeRight = xe.Element("RightPanel");
            dpt.LeftPanel = GetDataPanel(xeLeft);
            dpt.RightPanel = GetDataPanel(xeRight);
            return dpt;
        }

        private static DataPanelUploadInfo GetDataPanelUpload(XElement xe)
        {
            DataPanelUploadInfo dpt = new DataPanelUploadInfo();
            SetValue(dpt, xe);
            return dpt;
        }

        private static DataPanelInfo GetDataPanel(XElement xe)
        {
            DataPanelInfo p = new DataPanelInfo();
            if (xe.Elements("DataFieldItem") == null)
            {
                return p;
            }
            foreach (XElement item in xe.Elements("DataFieldItem"))
            {
                DataFieldItem dfItem = new DataFieldItem();
                SetValue(dfItem, item);
                dfItem.Order = XMLHelper.RuntimeOrder;
                //dfItem.CType = (ControlType)int.Parse(item.Attribute("CType").Value);
                //dfItem.Name = item.Attribute("Name").Value;
                //dfItem.PropertyDisplayName = item.Attribute("PropertyDisplayName").Value;
                //dfItem.Requited = Convert.ToBoolean(int.Parse(item.Attribute("Requited").Value));
                //dfItem.PropertyName = item.Attribute("PropertyName").Value;

                XElement xeISource = item.Element("ReferencedData");

                if (xeISource != null)
                {
                    ReferencedDataInfo r = new ReferencedDataInfo();
                    SetValue(r, xeISource);

                    string rType = xeISource.Attribute("Type").Value;
                    string rDefaultValue = GetValue(xeISource, "DefaultValue");
                    r.ReferencedName = dfItem.PropertyName;
                    XMLHelper.RuntimeOrder.RegisterReferenceData(r);
                    dfItem.ReferenceDataInfo = r;
                    r.Parameters = GetParameters(xeISource);
                }

                
                p.Items.Add(dfItem);

            }
            return p;
        }

        public static Dictionary<string, string> GetParameters(XElement xe)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            XElement xeIParams = xe.Element("Parameters");
            if (xeIParams != null)
            {
                xeIParams.Elements("Parameter").ToList().ForEach(element =>
                {
                    string name = element.Attribute("Name").Value;
                    string value = element.Value;
                    result.Add(name, value);
                });
            }
            return result;
        }

        public static string GetValue(XElement xe, string attrName)
        {
            XAttribute attr = xe.Attribute(attrName);
            if (attr != null)
            {
                return attr.Value;
            }
            else
            {
                return null;
            }
        }

        public static void SetValue(InfoBase obj, XElement xe)
        {
            foreach (XAttribute xa in xe.Attributes())
            {
                string propertyName = xa.Name.LocalName;
                try
                {
                    obj.SetObjValue(propertyName, xa.Value);
                }
                catch (Exception ex)
                {
                 //   MessageBox.Show("Error: " + propertyName + ":" + xa.Value + ":" + obj.GetType().Name + ex.ToString());
                }

            }
        }

    }

    #region UIMapping

    public class InfoBase
    {
        public string Name { get; set; }
        private string typeName = null;
        public string Type
        {
            get
            {
                return typeName;
            }
            set
            {
                typeName = value;
                if ( string.IsNullOrEmpty(Name))
                {
                    Name = typeName;
                }
            }
        }
    }
    public class OrderInfo : InfoBase
    {
        public OrderInfo()
        {
            // OrderEntity = new OrderEntityInfo();
        }
        public ViewInfo View { get; set; }
        public FormInfo Form { get; set; }
        public OrderEntityInfo OrderEntity { get; set; }

        public void RegisterReferenceData(ReferencedDataInfo r)
        {

            r.OrderInfo = this;
            string propertyName = r.ReferencedName;

            string referencedType = r.Type;
            
            r.Index = this.OrderEntity.ReferenceDatas.Count;
            r.ReferencedMember = string.Format("ReferencedData[{0}]", r.Index);

            this.OrderEntity.ReferenceDatas.Add(propertyName, r);

            //if (referencedType == typeof(DateTimeData).Name)
            //{
            //    r.ReferencedMember += ".Text";
            //}
        }

        public OrderDetailEntityInfo RegisterOrderDetailEntity(string name)
        {
            int index = this.OrderEntity.OrderDetailEntities.Dictionary[name];
            OrderDetailEntityInfo odei = this.OrderEntity.OrderDetailEntities[index];
            odei.Index = index;
            odei.ReferencedMember = string.Format("OrderDetailData[{0}]", index);
            
            if ( !string.IsNullOrEmpty(odei.PropertyName ))
            {
                string t = odei.PropertyName;
                if (t.IndexOf("Entity.") == 0)
                {
                    t = t.Substring("Entity.".Length);
                }
                this.OrderEntity.Entity.Include.Add(odei.PropertyName, t);
            }
            return odei;
        }
    }

    public class ViewInfo : InfoBase
    {
        public ViewInfo()
        {
            GridItems = new List<GridItem>();
            QueryList = new List<QueryInfo>();
        }
        public IList<GridItem> GridItems { get; set; }
        public List<QueryInfo> QueryList { get; set; }
    }

    public class FormInfo : InfoBase
    {
        public FormInfo()
        {
            Panels = new List<IDataPanel>();
        }
        public IList<IDataPanel> Panels;
    }

 

    public interface IDataPanel
    {
        IControlAction GetUIElement();
    }

    public class DataPanelInfo : InfoBase, IDataPanel
    {
        public DataPanelInfo()
        {
            Items = new List<DataFieldItem>();
        }
        public IList<DataFieldItem> Items { set; get; }

        #region IDataPanel 成员

        IControlAction IDataPanel.GetUIElement()
        {
            FieldForm f = new FieldForm();
            f.Items = this.Items;
            return f;
        }

        #endregion
    }

    public class DataPanelTwoSideInfo : InfoBase, IDataPanel
    {
        public DataPanelInfo LeftPanel { get; set; }
        public DataPanelInfo RightPanel { get; set; }

        #region IDataPanel 成员

        IControlAction IDataPanel.GetUIElement()
        {
            FieldForm2 f = new FieldForm2();
            f.LeftFForm.Items = LeftPanel.Items;
            f.RightFForm.Items = RightPanel.Items;

            return f;
        }

        #endregion
    }

    public class DataPanelUploadInfo : InfoBase, IDataPanel
    {
        public string IDPropertyName { get; set; }
        public string ModelName { get; set; }

        public IControlAction GetUIElement()
        {
            FBUploadControl uc = new FBUploadControl(this);
            return uc;
        }
    }

    public class OrderDetailGridInfo : InfoBase, IDataPanel
    {
        public OrderDetailEntityInfo OrderDetailEntityInfo { get; set; }
        public OrderDetailGridInfo()
        {
            GridItems = new List<GridItem>();
            Parameters = new Dictionary<string, string>();
        }
        public List<GridItem> GridItems { set; get; }
        public string ShowToolBar { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public IDataPanel RowDetailDataPanel { get; set; }
        #region IDataPanel 成员

        public IControlAction GetUIElement()
        {
            DetailGrid adg = new DetailGrid();
            // adg.GridItems = this.GridItems;
            if (ShowToolBar == "false")
            {
                adg.ShowToolBar = false;
            }
            adg.OrderDetailGridInfo = this;
            return adg;
        }
        #endregion
    }

    public class DataPanelSplit : InfoBase, IDataPanel
    {
        public float Height { get; set; }
        #region IDataPanel 成员

        public IControlAction GetUIElement()
        {
            SplitPanel p = new SplitPanel();
            p.Height = this.Height;
            return p;
        }

        #endregion
    }

    public class DataPanelGrid : InfoBase, IDataPanel
    {
        
        #region IDataPanel 成员

        public IControlAction GetUIElement()
        {
            GridPanel grid = new GridPanel() { Name = this.Name };
            return grid;
        }

        #endregion
    }

    public class PropertyItemInfo : InfoBase
    {

        public PropertyItemInfo()
        {
            MaxLength = 50;
            DataFormat = DataCore.DataFormat;
            DataType = "string";
        }
        public string PropertyName { get; set; }
        public string PropertyDisplayName { set; get; }
        public ReferencedDataInfo ReferenceDataInfo { get; set; }
        public ControlType CType { set; get; }
        public bool IsReadOnly { set; get; }
        public int MaxLength { get; set; }
        public string DataFormat { get; set; }
        public string DataType { get; set; }
        public string TipText { get; set; }
    }

    public class DataFieldItem : PropertyItemInfo
    {
        
        public OrderInfo Order { set; get; }
        public bool Requited { set; get; }
        
    }

    public class GridItem : PropertyItemInfo
    {
        public float Width { set; get; }
        
    }

    public class OrderEntityInfo : InfoBase
    {
        public OrderEntityInfo()
        {
            Entity = new EntityInfo();
            ReferenceDatas = new ObjectList<ReferencedDataInfo>();
            OrderDetailEntities = new ObjectList<OrderDetailEntityInfo>();
        }
        public EntityInfo Entity { get; set; }
        public ObjectList<ReferencedDataInfo> ReferenceDatas { get; set; }
        public ObjectList<OrderDetailEntityInfo> OrderDetailEntities { get; set; }
    }

    public class OrderDetailEntityInfo : InfoBase
    {
        public EntityInfo Entity { get; set; }
        public QueryExpressionInfo QueryExpression { get; set; }
        public string PropertyName { get; set; }
        public string ReferencedMember { get; set; }
        public int Index { get; set; }
    }

    public class EntityInfo : InfoBase
    {
        public EntityInfo()
            : base()
        {
            Include = new ObjectList<string>();
        }
        public ObjectList<string> Include { get; set; }
        public string KeyName { get; set; }
        public string CodeName { get; set; }
    }

    public class ReferencedDataInfo : InfoBase
    {
        public ReferencedDataInfo()
        {
            Parameters = new Dictionary<string, string>();
        }

        public int Index { get; set; }
        public string ReferencedName { get; set; }
        public string ReferencedMember { get; set; }
        public string DefaultValue { get; set; }
        public string TextPath { get; set; }
        public string ValuePath { get; set; }
        public OrderInfo OrderInfo { get; set; }
        
        public Dictionary<string, string> Parameters { get; set; }
    }

    public class QueryInfo : InfoBase
    {
        public string Code { get; set; }
        public QueryExpressionInfo QueryExpression { get; set; }
    }

    public class QueryExpressionInfo : InfoBase
    {
        public string QueryType { get; set; }
        public string PropertyName { set; get; }
        public string PropertyValue { set; get; }

        [DefaultValue("And")]
        public string Operation { set; get; }

        [DefaultValue("And")]
        public string RelatedType { get; set; }
        public QueryExpressionInfo RelatedExpression { get; set; }

        public QueryExpression GetQueryExpression()
        {
            return this.GetQueryExpression(this);
        }

        private QueryExpression GetQueryExpression(QueryExpressionInfo queryExpressionInfo)
        {
            if (queryExpressionInfo == null)
            {
                return null;
            }
            QueryExpression qe = new QueryExpression();
            qe.PropertyName = queryExpressionInfo.PropertyName;
            qe.QueryType = queryExpressionInfo.QueryType;
            qe.RelatedType = queryExpressionInfo.RelatedType == "And" ? QueryExpression.RelationType.And : QueryExpression.RelationType.Or;
            QueryExpression.Operations op = QueryExpression.Operations.Equal;
            if (queryExpressionInfo.PropertyName == QueryExpression.Operations.GreaterThan.ToString())
            {
                op = QueryExpression.Operations.GreaterThan;
            }
            else if (queryExpressionInfo.PropertyName == QueryExpression.Operations.GreaterThanOrEqual.ToString())
            {
                op = QueryExpression.Operations.GreaterThanOrEqual;
            }
            else if (queryExpressionInfo.PropertyName == QueryExpression.Operations.LessThan.ToString())
            {
                op = QueryExpression.Operations.LessThan;
            }
            else if (queryExpressionInfo.PropertyName == QueryExpression.Operations.LessThanOrEqual.ToString())
            {
                op = QueryExpression.Operations.LessThanOrEqual;
            }
            else if (queryExpressionInfo.PropertyName == QueryExpression.Operations.Like.ToString())
            {
                op = QueryExpression.Operations.Like;
            }
            qe.Operation = op;
            qe.PropertyValue = queryExpressionInfo.PropertyValue;


            qe.RelatedExpression = GetQueryExpression(queryExpressionInfo.RelatedExpression);

            return qe;
        }

    }

    public class SelectorInfo : InfoBase
    {
        public List<GridItem> Items { set; get; }
        public string ReferencedDataType { get; set; }
    }

    #endregion

    #region Control

    public class SplitPanel : StackPanel, IControlAction
    {

        #region IControlAction 成员

        public void InitControl(OperationTypes operationType)
        {
        }

        public Control FindControl(string name)
        {
            return null;
        }
        public bool Validate()
        {
            return true;
        }

        #endregion

        #region IControlAction 成员


        public ValidatorManager ValidatorManager
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                
            }
        }

        #endregion


        public void InitData(OrderEntity entity)
        {
            
        }

        public bool SaveData(Action<IControlAction> SavedCompletedAction)
        {
            return true;
        }
    }

    public class GridPanel : Grid, IControlAction
    {
        #region IControlAction 成员

        public void InitControl(OperationTypes operationType)
        {
        }

        public Control FindControl(string name)
        {
            return null;
        }
        public bool Validate()
        {
            return true;
        }

        #endregion

        #region IControlAction 成员


        public ValidatorManager ValidatorManager
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {

            }
        }

        #endregion


        public void InitData(OrderEntity entity)
        {

        }

        public bool SaveData(Action<IControlAction> SavedCompletedAction)
        {
            return true;
        }
    }
    #endregion

    public interface IControlAction
    {
        string Name { get; set; }
        object DataContext { get; set; }
        ValidatorManager ValidatorManager { get; set; }
        void InitControl(OperationTypes operationType);
        Control FindControl(string name);

        void InitData(OrderEntity entity);
        bool SaveData(Action<IControlAction> SavedCompletedAction);

    }
}
