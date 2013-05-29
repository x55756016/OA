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
using System.Collections.Generic;
using System.Windows.Data;
using SMT.FB.UI.FBCommonWS;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using SMT.FB.UI.Form.DailyManagement;
using SMT.FB.UI.Form;
using SMT.FB.UI.Form.BudgetApply;
using SMT.SaaS.Globalization;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace SMT.FB.UI.Common
{
    public static class CommonFunction
    {
        /// <summary>
        /// 克隆对像
        /// </summary>
        /// <param name="source">要被克隆的对像</param>
        /// <returns>克隆的新对像</returns>
        public static T CloneObject<T>(object source) where T : class
        {
            if (source == null)
            {
                return null;
            }

            DataContractSerializer dcSer = new DataContractSerializer(source.GetType());
            MemoryStream memoryStream = new MemoryStream();

            dcSer.WriteObject(memoryStream, source);
            memoryStream.Position = 0;

            T newObject = (T)dcSer.ReadObject(memoryStream);
            return newObject;
        }

        public static void FillComboBox(ComboBox cbb, IList<ITextValueItem> data)
        {
            cbb.DisplayMemberPath = "Text";
            cbb.ItemsSource = data;
            // cbb.SelectedIndex = 0;
        }

        public static void FillComboBox<T>(ComboBox cbb) where T : ITextValueItem
        {
            cbb.ItemsSource = DataCore.GetReferencedData<T>();
            cbb.DisplayMemberPath = "Text";
        }
        public static void FillQueryComboBox<T>(ComboBox cbb) where T : ITextValueItem
        {
            cbb.ItemsSource = DataCore.GetReferencedQuery<T>();
            cbb.DisplayMemberPath = "Text";
        }

        /// <summary>
        /// 根据PostId从岗位列表缓存内取出岗位名称
        /// </summary>
        /// <param name="strPostId"></param>
        /// <returns></returns>
        public static string GetPostNameByPostId(string strPostId)
        {
            string strRes = string.Empty;
            List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> entPostList = App.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;

            if (entPostList == null)
            {
                return strRes;
            }

            if (entPostList.Count() == 0)
            {
                return strRes;
            }

            SMT.Saas.Tools.OrganizationWS.T_HR_POST entPost = entPostList.Where(c => c.POSTID == strPostId).FirstOrDefault();

            if (entPost == null)
            {
                return strRes;
            }

            if (entPost.T_HR_POSTDICTIONARY == null)
            {
                return strRes;
            }

            strRes = entPost.T_HR_POSTDICTIONARY.POSTNAME;

            return strRes;
        }

        public static void GetTableCode(Type typeSend, ref string strCodeName)
        {
            if (typeSend == typeof(T_FB_BORROWAPPLYMASTER))                    //借款
            {
                strCodeName = "BORROWAPPLYMASTERCODE";
            }
            else if (typeSend == typeof(T_FB_CHARGEAPPLYMASTER))                    //费用报销                    
            {
                strCodeName = "CHARGEAPPLYMASTERCODE";
            }
            else if (typeSend == typeof(T_FB_TRAVELEXPAPPLYMASTER))                //差旅
            {
                strCodeName = "TRAVELEXPAPPLYMASTERCODE";

            }
            else if (typeSend == typeof(T_FB_REPAYAPPLYMASTER))                      //还款
            {
                strCodeName = "REPAYAPPLYCODE";
            }
            else if (typeSend == typeof(T_FB_COMPANYBUDGETAPPLYMASTER))              //年度预算申请
            {
                strCodeName = "COMPANYBUDGETAPPLYMASTERCODE";
            }
            else if (typeSend == typeof(T_FB_COMPANYBUDGETSUMMASTER))                //年度预算汇总
            {
                strCodeName = "COMPANYBUDGETSUMMASTERCODE";

            }
            else if (typeSend == typeof(T_FB_COMPANYBUDGETMODMASTER))                //年度预算变更
            {
                strCodeName = "COMPANYBUDGETMODMASTERCODE";
            }
            else if (typeSend == typeof(T_FB_COMPANYTRANSFERMASTER))                 //年度预算划拨申请
            {
                strCodeName = "COMPANYTRANSFERMASTERCODE";
            }
            else if (typeSend == typeof(T_FB_DEPTBUDGETAPPLYMASTER))                 //月度预算申请
            {
                strCodeName = "DEPTBUDGETAPPLYMASTERCODE";

            }
            else if (typeSend == typeof(T_FB_DEPTBUDGETSUMMASTER))                   //月度预算汇总申请
            {
                strCodeName = "DEPTBUDGETSUMMASTERCODE";

            }
            else if (typeSend == typeof(T_FB_DEPTBUDGETADDMASTER))                   //月度预算增补申请
            {
                strCodeName = "DEPTBUDGETADDMASTERCODE";
            }
            else if (typeSend == typeof(T_FB_DEPTTRANSFERMASTER))                    //月度预算调拨申请
            {
                strCodeName = "DEPTTRANSFERMASTERCODE";

            }
            else if (typeSend == typeof(T_FB_PERSONMONEYASSIGNMASTER))               //个人经费下拨申请
            {
                strCodeName = "PERSONMONEYASSIGNMASTERCODE";
            }
        }

        public static ITextValueItem GetComboBoxData(IList<ITextValueItem> data, object value)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Value.Equals(value))
                {
                    return data[i];
                }
            }
            return null;
        }


        //public static QueryExpression GetQueryExpression(ITextValueItem item)
        //{
        //    QueryExpression qe = new QueryExpression();
        //    qe.PropertyName = item.Text;
        //    qe.PropertyValue = item.Value;
        //    qe.Operation = QueryExpression.Operations.Equal;
        //    qe.RelatedType = QueryExpression.RelationType.And;
        //    return qe;
        //}


        #region MessageShow
        public enum MessageType { Delete, Question, Attention, Error }

        public static void AskDelete(string message, Action action)
        {
            string defaultMsg = "你确定要删除吗?";
            string defaultTitle = "确认删除";
            message = string.IsNullOrEmpty(message) ? defaultMsg : message;

            DialogOKCanel(defaultTitle, defaultMsg, action, null);
        }



        public static bool NotifySelection(string message)
        {
            string defaultMsg = "请先选择一条记录!";
            string defaultTitle = "选择提示";
            message = string.IsNullOrEmpty(message) ? defaultMsg : message;
            return ShowMessage(defaultTitle, message, MessageType.Attention);
        }

        public static bool NotifySuccessfulSaving(string message)
        {
            string defaultMsg = "保存成功!";
            string defaultTitle = "保存";
            message = string.IsNullOrEmpty(message) ? defaultMsg : message;
            return ShowMessage(defaultTitle, message, MessageType.Attention);
        }

        public static bool NotifySaveB4Close(string message, Action actionOK, Action actionCancel)
        {
            string defaultMsg = "数据已被修改是否保存!";
            string defaultTitle = "确认";
            message = string.IsNullOrEmpty(message) ? defaultMsg : message;
            return DialogOKCanel(defaultTitle, message, actionOK, actionCancel);

        }

        public static bool ShowErrorMessage(string message)
        {
            string defaultMsg = "出错了!";
            string defaultTitle = "出错提示";
            message = string.IsNullOrEmpty(message) ? defaultMsg : message;
            return ShowMessage(defaultTitle, message, MessageType.Error);
        }

        public static bool ShowErrorMessage(List<string> mgsList)
        {
            string innerMessage = "验证失败";
            mgsList.ForEach(msg => { innerMessage += "\r\n   " + msg; });
            return ShowErrorMessage(innerMessage);
        }

        public static bool ShowMessage(string title, string msg, MessageType messageType)
        {
            MessageIcon mi = MessageIcon.Exclamation;
            switch (messageType)
            {
                case MessageType.Attention:
                    mi = MessageIcon.Information;
                    break;
                case MessageType.Delete:
                    mi = MessageIcon.Question;
                    break;
                case MessageType.Error:
                    mi = MessageIcon.Error;
                    break;
                case MessageType.Question:
                    mi = MessageIcon.Question;
                    break;
            }


            string _text = "";
            MessageWindow.Show<string>(title, msg, mi,
               result => _text = result, "Default", GetString("OKBUTTON"));

            return true;
        }

        public static bool DialogOKCanel(string title, string msg, Action actionOK, Action actionCancel)
        {
            ComfirmWindow cw = new ComfirmWindow();
            cw.OnSelectionBoxClosed += (o, e) =>
            {
                if (e.Result == ComfirmWindow.titlename[0])
                {
                    if (actionOK != null)
                    {
                        actionOK();
                    }
                }
                else if (e.Result == ComfirmWindow.titlename[1])
                {
                    if (actionCancel != null)
                    {
                        actionCancel();
                    }
                }

            };
            cw.SelectionBox(title, msg, ComfirmWindow.titlename, string.Empty);
            return true;
        }
        public static bool ShowMessage(string msg)
        {
            string defaultTitle = "提示";
            return ShowMessage(defaultTitle, msg, MessageType.Attention);
        }

        #endregion

        public enum TypeCategory
        {
            Common, EntityObject, ReferencedData
        }
        public static Type GetType(string typeName, TypeCategory typeCategory)
        {
            string strFullName = "SMT.FB.UI.Common.";
            if (typeCategory == TypeCategory.EntityObject)
            {
                strFullName = "SMT.FB.UI.FBCommonWS.";
            }
            return Type.GetType(strFullName + typeName);
        }

        #region TryConvertValue
        public static T TryConvertValue<T>(object source)
        {
            Type t = typeof(T);
            if (t.IsValueType)
            {
                try
                {
                    return (T)CommonFunction.TryConvertValue(t, source);
                }
                catch
                {
                    return default(T);
                }
            }
            else
            {
                return (T)source;
            }
        }

        public static object TryConvertValue(Type t, object source)
        {

            object tryValue = default(object);
            if (source == null)
            {
                return tryValue;
            }
            else if (source.GetType() == t)
            {
                return source;
            }
            if (typeof(ITextValueItem).IsAssignableFrom(source.GetType()))
            {
                return TryConvertValue(t, (source as ITextValueItem).Value);
            }
            if (t.IsValueType)
            {
                if (t.BaseType == typeof(System.Enum))
                {
                    return System.Enum.Parse(t, source.ToString(), true);
                }
                if (t.Name.IndexOf("Nullable`") == 0)
                {
                    t = t.GetProperty("Value").PropertyType;
                }
                if (t == typeof(bool))
                {
                    return Convert.ToBoolean(source);
                }
                else if (source.GetType() == typeof(bool))
                {
                    source = Convert.ToInt32(source);
                }
                source = Convert.ToString(source);
                MethodInfo methodInfo = t.GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => { return m.Name == "Parse"; });
                tryValue = methodInfo.Invoke(null, new object[] { source });
            }
            else if (typeof(ITextValueItem).IsAssignableFrom(t))
            {

                MethodInfo myMethod = typeof(DataCore).GetMethods().First(m => m.Name.Equals("FindReferencedData") && m.IsGenericMethod);
                myMethod = myMethod.MakeGenericMethod(t);
                tryValue = myMethod.Invoke(null, new object[] { source });
            }
            else
            {
                tryValue = source;
            }
            return tryValue;

        }

        public static string TryConvertValue(object source, string target)
        {
            if (source == null)
            {
                return null;
            }
            return source.ToString();

        }

        public static int TryConvertValue(object source, int target)
        {
            int result = 0;
            if (source != null)
            {
                int.TryParse(source.ToString(), out result);
            }
            return result;
        }

        public static float TryConvertValue(object source, float target)
        {
            float result = 0f;
            if (source != null)
            {
                float.TryParse(source.ToString(), out result);
            }
            return result;
        }

        public static decimal TryConvertValue(object source, decimal target)
        {
            decimal result = 0;
            if (source != null)
            {
                decimal.TryParse(source.ToString(), out result);
            }
            return result;
        }

        public static DateTime TryConvertValue(object source, DateTime target)
        {
            DateTime result = DateTime.Now;
            if (source != null)
            {
                DateTime.TryParse(source.ToString(), out result);
            }
            return result;
        }

        #endregion

        #region CommonForm
        public static FBPage GetOrderForm(OrderEntity orderEntity)
        {
            switch (orderEntity.OrderType.Name.ToUpper())
            {
                case "T_FB_TRAVELEXPAPPLYMASTER":
                    return new TravelApplyForm(orderEntity);

                case "T_FB_COMPANYBUDGETAPPLYMASTER":
                    return new CompanyBudgetApplyForm(orderEntity);
                case "T_FB_COMPANYBUDGETMODMASTER":
                    return new CompanyBudgetModForm(orderEntity);
                case "T_FB_DEPTBUDGETAPPLYMASTER":
                    return new DeptBudgetApplyForm(orderEntity);
                case "T_FB_PERSONBUDGETAPPLYMASTER":
                    return new PersonBudgetApplyForm(orderEntity);
                case "T_FB_PERSONBUDGETADDMASTER":
                    return new PersonBudgetAddForm(orderEntity);

                case "COMPANYTRANSFERMASTER":
                    return new CompanyTransferAppForm(orderEntity);
                case "DEPARTMENTTRANSFERMASTER":
                    return new DepartmentTransferAppForm(orderEntity);
                default:
                    FBPage cForm = new FBPage(orderEntity);
                    cForm.InitForm();
                    return cForm;

            }

            throw new FBSystemException("没找到相应的Form");
        }
        #endregion

        #region 多语言
        public static string GetString(string key)
        {
            return Utility.GetResourceStr(key);
        }
        #endregion

        public static IValueConverter GetIValueConverter(string targetType)
        {
            return new EntityConvert(targetType);
        }

        public static FrameworkElement ParentLayoutRoot
        {
            get
            {
                //FrameworkElement grid = Application.Current.RootVisual as FrameworkElement;
                return SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot as FrameworkElement;
            }
        }

        //weirui 2012-11-20注释
        //public static LoginUser CurrentUserInfo
        //{
        //    get
        //    {
        //        return null; 
        //            //SMT.SAAS.Main.CurrentContext.Common.CurrentConfig.CurrentUser;
        //    }
        //}

        #region 弹出审核表单(现有方式)
        /// <summary>
        /// 显示审核界面
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="modelCode"></param>
        /// <param name="formType">BROWSE : 查看，　Audit:审核，　EDIT : 更新，　Add: 添加</param>
        public static void ShowAuditForm(string orderID, string modelCode, string formType)
        {
            ShowEditForm(orderID, modelCode, formType);
        }

        /// <summary>
        /// 门户创建待审批的单据（针对新平台） 2012-8-3
        /// </summary>
        /// <param name="FormID"></param>
        /// <param name="FormName"></param>
        public static void CreateFormFromMvcPlat(string orderID, string modelCode, string formType)
        {
            ShowEditFormForMvcPlat(orderID, modelCode, formType);        
        }


        /// <summary>
        /// 显示审核界面
        /// </summary>
        /// <param name="modelCode"></param>
        /// <param name="orderID"></param>
        public static void ShowEditFormForMvcPlat(string orderID, string modelCode, string formType)
        {
            try
            {
                OrderInitManager om = new OrderInitManager();
                om.InitCompleted += (o, e) =>
                {
                    EntityBrowser eb = GetEditPage(modelCode, orderID, formType) as EntityBrowser;
                    FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;
                    eb.MinHeight = 400;
                    eb.ShowMvcPlat<string>(DialogMode.Default, plRoot, "", (result) => { });
                };
                om.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Code: {0}, ID: {1} ,Exception :{2} ", modelCode, orderID, ex.ToString()));
            }
        }

        /// <summary>
        /// 显示审核界面
        /// </summary>
        /// <param name="modelCode"></param>
        /// <param name="orderID"></param>
        public static void ShowEditForm(string orderID, string modelCode, string formType)
        {
            try
            {
                OrderInitManager om = new OrderInitManager();
                om.InitCompleted += (o, e) =>
                {
                    EntityBrowser eb = GetEditPage(modelCode, orderID, formType) as EntityBrowser;
                    FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;
                    eb.MinHeight = 400;
                    eb.Show<string>(DialogMode.Default, plRoot, "", (result) => { }, true, orderID);
                };
                om.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Code: {0}, ID: {1} ,Exception :{2} ", modelCode, orderID, ex.ToString()));
            }
        }

        /// <summary>
        /// 返回审核usercontrol 
        /// </summary>
        /// <param name="modelCode"></param>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static UserControl GetEditPage(string modelCode, string orderID, string formType)
        {
            Type type = CommonFunction.GetType(modelCode, CommonFunction.TypeCategory.EntityObject);
            OrderEntity orderEntity = new OrderEntity(type);
            orderEntity.OrderID = orderID;
            orderEntity.FBEntityState = FBEntityState.Unchanged;
            FBPage page = FBPage.GetPage(orderEntity);
            OperationTypes oType = OperationTypes.Audit;
            switch (formType.ToUpper())
            {
                case "BROWSE":
                    oType = OperationTypes.Browse;
                    break;
                case "AUDIT":
                    oType = OperationTypes.Audit;
                    break;
                case "EDIT":
                    oType = OperationTypes.Edit;
                    break;
                case "ADD":
                    oType = OperationTypes.Add;
                    break;
                case "RESUBMIT":
                    oType = OperationTypes.ReSubmit;
                    break;
            }

            page.EditForm.OperationType = oType;
            EntityBrowser eb = new EntityBrowser(page);
            page.PageClosing += (o, e) =>
            {
                eb.Close();
            };
            return eb;
        }
        #endregion



        #region 弹出审核表单(新平台方式)

        /// <summary>
        /// 显示审核界面
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="modelCode"></param>
        /// <param name="formType">BROWSE : 查看，　Audit:审核，　EDIT : 更新，　Add: 添加</param>
        public static void ShowAuditForm(string orderID, string modelCode, string formType, Border parent)
        {
            ShowEditForm(orderID, modelCode, formType, parent);
        }

        /// <summary>
        /// 显示审核界面
        /// </summary>
        /// <param name="modelCode"></param>
        /// <param name="orderID"></param>
        public static void ShowEditForm(string orderID, string modelCode, string formType, Border parent)
        {
            try
            {
                OrderInitManager om = new OrderInitManager();
                om.InitCompleted += (o, e) =>
                {
                    EntityBrowser eb = GetEditPage(modelCode, orderID, formType) as EntityBrowser;
                    FrameworkElement plRoot = CommonFunction.ParentLayoutRoot;
                    eb.MinHeight = 400;
                    parent.Child = eb;
                };
                om.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Code: {0}, ID: {1} ,Exception :{2} ", modelCode, orderID, ex.ToString()));
            }
        }
        #endregion

        #region 显示外部单据
        /// <summary>
        /// 显示外部单据(即预算关联OA的单据)
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="modelCode"></param>
        /// <param name="formType"></param>
        public static void ShowExtendForm(object sourceobj)
        {
            if (sourceobj == null)
            {
                return;
            }

            T_FB_EXTENSIONALORDER entOrder = sourceobj as T_FB_EXTENSIONALORDER;

            if(entOrder == null)
            {
                return;
            }

            if(entOrder.T_FB_EXTENSIONALTYPE == null)
            {
                return;
            }

            if(string.IsNullOrWhiteSpace(entOrder.ORDERID) || string.IsNullOrWhiteSpace(entOrder.T_FB_EXTENSIONALTYPE.MODELCODE))
            {
                return;
            }

            SMT.SAAS.Controls.Toolkit.Windows.Program pg = new SAAS.Controls.Toolkit.Windows.Program();
            string AssemblyName = string.Empty, PublicClass = string.Empty, ProcessName = string.Empty, PageParameter = string.Empty;
            string ApplicationOrder = string.Empty, FormType = string.Empty;

            FormType = Convert.ToInt32(SMT.SaaS.FrameworkUI.FormTypes.Browse).ToString();
            switch(entOrder.T_FB_EXTENSIONALTYPE.EXTENSIONALTYPECODE)
            {
                case "CCSQ":
                    AssemblyName = "SMT.SaaS.OA.UI";
                    PublicClass = "SMT.SaaS.OA.UI.Utility";
                    ProcessName = "CreateFormFromEngine";
                    PageParameter = "SMT.SaaS.OA.UI.Views.Travelmanagement.TravelapplicationChildWindows";
                    ApplicationOrder = entOrder.ORDERID;                    
                    break;
                case "CCBX":
                    AssemblyName = "SMT.SaaS.OA.UI";
                    PublicClass = "SMT.SaaS.OA.UI.Utility";
                    ProcessName = "CreateFormFromEngine";
                    PageParameter = "SMT.SaaS.OA.UI.UserControls.TravelReimbursementControl";
                    ApplicationOrder = entOrder.ORDERID;                   
                    break;
                case "SXSP":
                    AssemblyName = "SMT.SaaS.OA.UI";
                    PublicClass = "SMT.SaaS.OA.UI.Utility";
                    ProcessName = "CreateFormFromEngine";
                    PageParameter = "SMT.SaaS.OA.UI.UserControls.ApprovalForm_aud";
                    ApplicationOrder = entOrder.ORDERID;
                    break;
            }

            Assembly assembly = SMT.SAAS.Utility.ApplicationHelper.GetAssembly(AssemblyName);
            if (assembly == null)
            {
                return;
            }

            Type type = assembly.GetType(PublicClass);

            Type[] types = new Type[] { typeof(string), typeof(string), typeof(string) };
            MethodInfo method = type.GetMethod(ProcessName, types);
            method.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, new object[] { ApplicationOrder, PageParameter, FormType }, null);
        }
        #endregion
        
    }

    public class FBSystemException : Exception
    {
        public FBSystemException(string msg)
            : base(msg)
        {
        }
    }



    public static class XmlHelper
    {

        public static XElement ToXml(this object obj)
        {
            if (obj == null)
            {
                return new XElement("Error", "Error:: 对象为nul! ");
            }

            List<object> listDone = new List<object>();

            return obj.ToXml(null, listDone);
        }

        /// <summary>
        /// 对单个节点的操作
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="listDone"></param>
        /// <returns></returns>
        private static XElement ToXml(this object obj, string xName, List<object> listDone)
        {
            if (obj == null)
            {
                if (xName != null)
                {
                    return new XElement(xName);
                }
                return null;
            }
            Type curType = obj.GetType();
            if (xName == null)
            {
                xName = curType.Name;
            }
            XElement result = null;


            // 值类型
            if (curType.IsValueType || curType.IsEnum || curType == typeof(string))
            {
                result = new XElement(xName);
                result.Add(obj);
            }
            else
            {
                IEnumerable ie = obj as IEnumerable;
                // 集合类型
                if (ie != null)
                {
                    result = ie.ListToXml(xName, listDone);
                }
                else // 单个对象类型
                {
                    result = obj.ObjectToXml(xName, listDone);
                }
            }
            return result;

        }

        /// <summary>
        /// 集合对象的输出
        /// </summary>
        /// <param name="list"></param>
        /// <param name="xName"></param>
        /// <param name="listDone"></param>
        /// <returns></returns>
        private static XElement ListToXml(this IEnumerable list, string xName, List<object> listDone)
        {
            var result = new XElement(xName);
            foreach (var item in list)
            {
                result.Add(item.ToXml(null, listDone));
            }
            return result;
        }
        /// <summary>
        /// 单个实例的输出
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="xName"></param>
        /// <param name="listDone"></param>
        /// <returns></returns>
        private static XElement ObjectToXml(this object obj, string xName, List<object> listDone)
        {
            if (listDone.CheckAndAddToExist(obj))
            {
                return new XElement(xName, "Warn:: 对象已处理过! ");
            }

            //特殊处理
            var exResult = ExpectObjectToXml(obj, xName, listDone);
            if (exResult != null)
            {
                return exResult;
            }

            var result = new XElement(xName);
            Type type = obj.GetType();
            var ps = type.GetProperties(System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.GetProperty |
                System.Reflection.BindingFlags.SetProperty);
            foreach (var item in ps)
            {
                var itemValue = item.GetValue(obj, null);
                result.Add(itemValue.ToXml(item.Name, listDone));
            }
            return result;
        }
        /// <summary>
        /// 检查是否obj已存在listDone中，存在返回true,否则添加到listDone列表中，并返回false.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="listDone"></param>
        /// <returns></returns>
        private static bool CheckAndAddToExist(this List<object> listDone, object obj)
        {
            bool result = false;
            if (listDone.Contains(obj)) // 是否存在
            {

                result = true;
            }
            listDone.Add(obj);          // 添加列表,表达已处理过 
            return result;
        }


        private static XElement ExpectObjectToXml(this object obj, string xName, List<object> listDone)
        {
            Type type = obj.GetType();
            if (type == typeof(EntityKey))
            {
                return new XElement(xName, "Type: EntityKey, 暂不实现xml! ");
            }
            else if (type == typeof(Type))
            {
                return new XElement(xName, "Type : Type, 暂不实现xml! ");
            }
            else if (typeof(EntityReference).IsAssignableFrom(type))
            {
                return new XElement(xName, "Type: EntityReference, 暂不实现xml! ");
            }

            return null;
        }
    }
}
