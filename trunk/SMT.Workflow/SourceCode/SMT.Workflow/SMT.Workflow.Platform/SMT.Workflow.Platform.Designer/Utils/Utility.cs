using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;
using System.Windows.Data;
using SMT.SaaS.LocalData;
namespace SMT.Workflow.Platform.Designer.Utils
{
    public class BaseConverter : IValueConverter
    {

        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            switch (value.ToString())
            {
                case "0":
                    return "否";
                case "1":
                    return "是";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
    public sealed class Utility
    {
        public static T_SYS_USER CurrentUser;

        //added by jason, 02/21/2012
        /// <summary>
        /// 获取当前用户能够操作的公司ID，以“,”分割
        /// </summary>
        /// <returns></returns>
        public static string GetAllOwnerCompanyId()
        {
            string strAllOwnerCompanyId = "";

            foreach (UserPost usr in SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts)
            {
                strAllOwnerCompanyId += "'" + usr.CompanyID + "',";
            }

            if (strAllOwnerCompanyId != "") strAllOwnerCompanyId = strAllOwnerCompanyId.Substring(0, strAllOwnerCompanyId.Length - 1);

            return strAllOwnerCompanyId;            
        }
        /// <summary>
        /// 获取当前用户能够操作的公司ID，以“,”分割
        /// </summary>
        /// <returns></returns>
        public static string GetOwnerCompanyId()
        {
           string  allOwnerCompanyId = "";
            bool bol=true;
            #region 加载所有公司
            try
            {
                string path = "silverlightcache\\" + Utility.CurrentUser.EMPLOYEEID + "ID.txt";
                var company = SLCache.GetCache<string>(path, 5);
                if (company != null)
                {
                    allOwnerCompanyId = company;
                    return company;
                }
                else
                {
                    SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient osc = new Saas.Tools.OrganizationWS.OrganizationServiceClient();
                    osc.GetCompanyViewAsync(Utility.CurrentUser.EMPLOYEEID, "3", "");
                    osc.GetCompanyViewCompleted += (obj, args) =>
                    {
                        if (args.Result != null)
                        {
                            string companylist = "";
                            foreach (var ent in args.Result)
                            {
                                companylist += "'" + ent.COMPANYID + "',";
                            }
                            allOwnerCompanyId = companylist.TrimEnd(',');                      
                            SLCache.SaveData<string>(companylist.TrimEnd(','), path);
                        }
                        bol=false;
                    };
                   //等异步完成
                    while (bol)
                    {
                        //等异步完成
                    };
                    return allOwnerCompanyId;
                }
            }
            catch (Exception ee)
            {
                SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.ConfirmationBox("错误信息", ee.Message, "确定");
                return null;
            }
            #endregion
        }

        #region 提示框信息
        /// <summary>
        /// 显示错误提示框信息
        /// </summary>
        /// <param name="messageType">信息类型</param>
        /// <param name="title">标题</param>
        /// <param name="message">错误内容</param>
        public static void ShowCustomMessage(Common.MessageTypes messageType, string title, string message)
        {
            string _text = "";
            if (messageType == Common.MessageTypes.Error)
            {
                MessageWindow.Show<string>(title, message, MessageIcon.Error, result => _text = result, "Default",
                                           Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else if (messageType == Common.MessageTypes.Caution)
            {
                MessageWindow.Show<string>(title, message, MessageIcon.Exclamation, result => _text = result, "Default",
                                           Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                MessageWindow.Show<string>(title, message, MessageIcon.Information, result => _text = result, "Default",
                                           Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        public static string GetResourceStr(string message)
        {
            string rslt = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString(message,
                                                                                    SMT.SaaS.Globalization.Localization.
                                                                                        UiCulture);
            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }

        public static string GetResourceStr(string message, string parameter)
        {
            string rslt = SMT.SaaS.Globalization.Localization.GetString(message, parameter);

            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }
        #endregion

        #region 分页方法
        public static void HandleDataPageDisplay(GridPager gridpager, int pageCout)
        {
            if (pageCout > 0)
            {
                gridpager.Visibility = Visibility.Visible;
            }
            else
            {
                gridpager.Visibility = Visibility.Collapsed;
            }
            gridpager.PageCount = pageCout;
        }
        #endregion
    }
    #region ComboBox扩展类
    /// <summary>
    ///  ComboBox扩展类
    /// </summary>
    public static class ComboBoxExtensions
    {
        /// <summary>
        /// 绑定ComboBox
        /// </summary>
        /// <param name="cBox"></param>
        /// <param name="listObj">数据源</param>
        /// <param name="displayMemberPath">显示的字段</param>
        /// <param name="selectedValuePath">选中的字段</param>
        internal static void BindData(this ComboBox cBox, IList listObj, string displayMemberPath, string selectedValuePath)
        {
            if (listObj != null)
            {
                cBox.ItemsSource = listObj;
                cBox.DisplayMemberPath = displayMemberPath;
                cBox.SelectedValuePath = selectedValuePath;
            }
        }
        /// <summary>
        /// 绑定ComboBox
        /// </summary>
        /// <param name="cBox">ComboBox</param>
        /// <param name="listObj">数据源</param>
        internal static void BindData(this ComboBox cBox, IList listObj)
        {
            if (listObj != null)
            {
                cBox.ItemsSource = listObj;
            }
        }
        /// <summary>
        /// 设置ComboBox选中项
        /// </summary>
        /// <param name="cBox"></param>
        /// <param name="text">选中的文字</param>
        internal static void SelectedByText(this ComboBox cBox, string text)
        {

            for (int i = 0; i < cBox.Items.Count; i++)
            {
                //((System.Windows.Controls.ContentControl)(cBox.Items[i])).Content
                ComboBoxItem item = cBox.Items[i] as ComboBoxItem;
                if (item != null)
                {
                    if (item.Content.ToString() == text)
                    {
                        cBox.SelectedIndex = i;
                    }
                }
            }
        }
        /// <summary>
        /// 设置ComboBox选中项
        /// </summary>
        /// <typeparam name="T">对像</typeparam>
        /// <param name="cBox">ComboBox</param>
        /// <param name="propertyName">对象属性名称(判断依据)</param>
        /// <param name="value">判断依据的值</param>
        internal static void SelectedByObject<T>(this ComboBox cBox, string propertyName, string value)
        {
            if (cBox.ItemsSource != null)
            {
                ObservableCollection<T> list = new ObservableCollection<T>();
                // ObservableCollection<T> list = (ObservableCollection<T>) cBox.ItemsSource.AsQueryable();//.Cast<T>();
                foreach (T t in cBox.ItemsSource)
                {
                    list.Add(t);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i];
                    if (obj != null && obj.GetType().GetProperty(propertyName) != null)
                    {
                        string nameproperty = obj.GetType().GetProperty(propertyName).GetValue(obj, null).ToString();
                        if (nameproperty == value)
                        {
                            cBox.SelectedIndex = i;
                        }
                    }
                    //list[1].GetType().GetProperty("propertyName").GetValue(list[1],null)
                }
            }
        }
        /// <summary>
        /// 设置ComboBox选中项
        /// </summary>
        /// <typeparam name="T">对像</typeparam>
        /// <param name="cBox">ComboBox</param>
        /// <param name="propertyName">对象属性名称(判断依据)</param>
        /// <param name="value">判断依据的值</param>
        internal static void Selected<T>(this ComboBox cBox, string propertyName, string value)
        {
            if (cBox.ItemsSource != null)
            {
                ObservableCollection<T> list = new ObservableCollection<T>();
                // ObservableCollection<T> list = (ObservableCollection<T>) cBox.ItemsSource.AsQueryable();//.Cast<T>();
                foreach (T t in cBox.ItemsSource)
                {
                    list.Add(t);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    var obj = list[i];
                    if (obj != null && obj.GetType().GetProperty(propertyName) != null)
                    {
                        string nameproperty = obj.GetType().GetProperty(propertyName).GetValue(obj, null).ToString();
                        if (nameproperty == value)
                        {
                            cBox.SelectedIndex = i;
                        }
                    }
                    //list[1].GetType().GetProperty("propertyName").GetValue(list[1],null)
                }
            }
        }
    }
    #endregion

    #region 缓存类，龙康才新增
    public class SLCache
    {
        /// <summary>
        /// 申请独立存储空间
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool ApplayStrogeSpace(double size)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    Int64 quotSize = Convert.ToInt64(size * 1024);
                    Int64 curSize = store.AvailableFreeSpace;
                    if (curSize < quotSize)
                    {
                        if (store.IncreaseQuotaTo(quotSize))
                        { return true; }
                        else
                        { return false; }
                    }
                    else
                    { return true; }
                }
            }
            catch (IsolatedStorageException ex)
            { throw new IsolatedStorageException("申请独立存储空间失败！" + ex.Message); }
        }

        /// <summary>
        /// 保存字符串到文件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        public static void SaveString(string data, string fileName)
        {
            string dir = System.IO.Path.GetDirectoryName(fileName);

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.DirectoryExists(dir))
                {
                    store.CreateDirectory(dir);
                }
                if (store.FileExists(fileName))
                { store.DeleteFile(fileName); }

                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(fileName, FileMode.Create, store))
                {
                    using (var sw = new StreamWriter(isfs))
                    {
                        sw.Write(data);
                        sw.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 泛类型支持存储文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Tdata"></param>
        /// <param name="fileName"></param>
        public static void SaveData<T>(T Tdata, string fileName)
        {
            string dir = System.IO.Path.GetDirectoryName(fileName);
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.DirectoryExists(dir))
                {
                    store.CreateDirectory(dir);
                }
                if (store.FileExists(fileName))
                {
                    store.DeleteFile(fileName);
                }
                IsolatedStorageFileStream isfs = store.CreateFile(fileName);
                DataContractSerializer ser = new DataContractSerializer(typeof(T));
                ser.WriteObject(isfs, Tdata);
                isfs.Close();
            }
        }

        /// <summary>
        /// 返回字符串
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetCache(string fileName)
        {
            string data = string.Empty;
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fileName))
                {
                    using (var isfs = new IsolatedStorageFileStream(fileName, FileMode.Open, isf))
                    {
                        using (var sr = new StreamReader(isfs))
                        {
                            string lineData;
                            while ((lineData = sr.ReadLine()) != null) { data += lineData; }
                        }
                    }
                }

            }
            return data;
        }


        /// <summary>
        /// 泛类型返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T GetCache<T>(string fileName)
        {
            T data = default(T);
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fileName))
                {                  
                    IsolatedStorageFileStream isfs = isf.OpenFile(fileName, FileMode.Open);
                    var ser = new DataContractSerializer(typeof(T));
                    data = (T)ser.ReadObject(isfs);
                    isfs.Close();
                }
            }
            return data;
        }
    /// <summary>
        /// 泛类型返回
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <param name="hours">过期时间,小时</param>
    /// <returns></returns>
        public static T GetCache<T>(string fileName,double hours)
        {
            T data = default(T);
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fileName))
                {
                    if ((DateTime.Now - isf.GetLastWriteTime(fileName)).TotalHours > hours)
                    {
                        isf.DeleteFile(fileName);                      
                        return data;
                    }
                    IsolatedStorageFileStream isfs = isf.OpenFile(fileName, FileMode.Open);
                    var ser = new DataContractSerializer(typeof(T));
                    data = (T)ser.ReadObject(isfs);
                    isfs.Close();
                }
            }
            return data;
        }
        /// <summary>
        /// 清空独立存储
        /// </summary>
        /// <param name="fileName"></param>
        public static void ReMove(string fileName)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fileName))
                { isf.DeleteFile(fileName); }
            }
        }
        public static void CreateDirectory(string dirPath)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                String directoryName = dirPath;

                if (!store.DirectoryExists(directoryName))
                {
                    store.CreateDirectory(directoryName);
                }
            }

        }
        public static void CreateFile(string filePath, object content)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream fileStream = store.CreateFile(filePath);
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    sw.WriteLine(content);
                }
                fileStream.Close();

            }
        }
        static SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient osc = new Saas.Tools.OrganizationWS.OrganizationServiceClient();
       /// <summary>
        ///  ComboBox 绑定所有公司T_HR_COMPANY，显示CNAME
       /// </summary>
        /// <param name="cboCompany">ComboBox</param>
       /// <param name="companyidSelect">被选中的公司ID,如果是空，则默认为登录人员所属公司ID</param>
        public static void ComboBoxBindAllCompany(ComboBox cboCompany,string companyidSelect)
       {
            #region 加载所有公司
           try
           {
               //string path = "silverlightcache\\t_hr_company.txt";
               //实际位置：C:\Users\longkc\AppData\LocalLow\Microsoft\Silverlight\is\5qkvd2vt.kdf\ngv1qcai.xbm\1\s\4nhtt330ofuzh0pwkcqvwnecnu2bejhgref3w4wo5cz02n5f0haaaaga\f\silverlightcache\t_hr_company.txt
             //  var company = SLCache.GetCache<ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>>(path);
               string path = "silverlightcache\\"+Utility.CurrentUser.EMPLOYEEID+".txt";
               var company = SLCache.GetCache<ObservableCollection<SMT.Saas.Tools.OrganizationWS.V_COMPANY>>(path, 2);
               if (company == null)
               {
                   osc.GetCompanyViewAsync(Utility.CurrentUser.EMPLOYEEID, "3", "");
                   osc.GetCompanyViewCompleted += (obj, args) =>
                   {

                       if (args.Result != null)
                       {
                           if (company != null)
                           {
                               company.Clear();
                           }
                           var result = args.Result;
                           company = result;
                           var selobj = company.Where(c => c.CNAME == "=请选择=").FirstOrDefault();
                           if (selobj == null)
                           {
                               company.Insert(0, new Saas.Tools.OrganizationWS.V_COMPANY { COMPANYID = "", CNAME = "=请选择=" });
                               //if (selobj.CNAME.IndexOf("=请选择=") < 0)
                               //{
                               //    company.Insert(0, new Saas.Tools.OrganizationWS.V_COMPANY { COMPANYID = "", CNAME = "=请选择=" });
                               //}
                           }
                          // company.Insert(0, new Saas.Tools.OrganizationWS.V_COMPANY { COMPANYID = "", CNAME = "=请选择=" });
                           cboCompany.ItemsSource = result.OrderBy(c => c.CNAME);
                           cboCompany.DisplayMemberPath = "CNAME";

                           if (string.IsNullOrEmpty(companyidSelect))
                           {
                               string companyid = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                               cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", companyid);
                           }
                           else
                           {
                               cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", companyidSelect);
                           }
                           SLCache.SaveData<ObservableCollection<SMT.Saas.Tools.OrganizationWS.V_COMPANY>>(result, path);
                       }
                   };
                   #region 没有权限
                   //osc.GetCompanyAllAsync("");                
                   //osc.GetCompanyAllCompleted += (obj, args) =>
                   //{                      
                      
                   //    if (args.Result != null)
                   //    {
                   //        var result = args.Result;
                   //        company = result;
                   //        company.Insert(0, new Saas.Tools.OrganizationWS.T_HR_COMPANY { COMPANYID = "", CNAME = "=请选择=" });
                   //        cboCompany.ItemsSource = result.OrderBy(c => c.CNAME);
                   //        cboCompany.DisplayMemberPath = "CNAME";

                   //        if (string.IsNullOrEmpty(companyidSelect))
                   //        {
                   //            string companyid = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                   //            cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>("COMPANYID", companyid);
                   //        }
                   //        else
                   //        {
                   //            cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>("COMPANYID", companyidSelect);
                   //        }
                   //        SLCache.SaveData<ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>>(result, path);
                   //    }
                   //};
                   #endregion
               }
               else
               {
                   //var selobj = company.Where(c => c.CNAME == "=请选择=").FirstOrDefault();
                   //if (selobj != null)
                   //{
                   //    company.Remove(selobj); 
                   //}
                   //if (selobj.CNAME.IndexOf("=请选择=") < 0)
                   //{
                   //    company.Insert(0, new Saas.Tools.OrganizationWS.V_COMPANY { COMPANYID = "", CNAME = "=请选择=" });
                   //}
                   cboCompany.ItemsSource = company.OrderBy(c => c.CNAME);
                   cboCompany.DisplayMemberPath = "CNAME";
                   if (string.IsNullOrEmpty(companyidSelect))
                   {
                       string companyid = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                       cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", companyid);
                   }
                   else
                   {
                       cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", companyidSelect);
                   }
               }
           }
           catch
           {
               if (cboCompany.Items.Count < 2)//小于1是因为=请选择= 有一个
               {
                   osc.GetCompanyViewAsync(Utility.CurrentUser.EMPLOYEEID, "3", "");
                   osc.GetCompanyViewCompleted += (obj, args) =>
                   {
                       if (args.Result != null)
                       {
                           cboCompany.ItemsSource = null;
                           var result = args.Result;
                           //var sel = new Saas.Tools.OrganizationWS.T_HR_COMPANY { COMPANYID = "", CNAME = "=请选择=" };
                           var s = from c in result where c.COMPANYID == "" select c;
                           if (s.FirstOrDefault() == null)
                           {
                               result.Insert(0, new Saas.Tools.OrganizationWS.V_COMPANY { COMPANYID = "", CNAME = "=请选择=" });
                           }
                           //var  company = result;
                           //company.Insert(0, new Saas.Tools.OrganizationWS.T_HR_COMPANY { COMPANYID = "", CNAME = "=请选择=" });

                           cboCompany.ItemsSource = result.OrderBy(c => c.CNAME);
                           cboCompany.DisplayMemberPath = "CNAME";

                           if (string.IsNullOrEmpty(companyidSelect))
                           {
                               string companyid = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                               cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", companyid);
                           }
                           else
                           {
                               cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", companyidSelect);
                           }

                       }
                   };
                   #region 重新绑定

                   //osc.GetCompanyAllAsync("");
                   //osc.GetCompanyAllCompleted += (obj, args) =>
                   //{
                   //    if (args.Result != null)
                   //    {
                   //        cboCompany.ItemsSource = null;
                   //        var result = args.Result;
                   //        //var sel = new Saas.Tools.OrganizationWS.T_HR_COMPANY { COMPANYID = "", CNAME = "=请选择=" };
                   //        var s = from c in result where c.COMPANYID == "" select c;
                   //        if (s.FirstOrDefault() == null)
                   //        {
                   //            result.Insert(0, new Saas.Tools.OrganizationWS.T_HR_COMPANY { COMPANYID = "", CNAME = "=请选择=" });
                   //        }
                   //        //var  company = result;
                   //        //company.Insert(0, new Saas.Tools.OrganizationWS.T_HR_COMPANY { COMPANYID = "", CNAME = "=请选择=" });

                   //        cboCompany.ItemsSource = result.OrderBy(c => c.CNAME);
                   //        cboCompany.DisplayMemberPath = "CNAME";

                   //        if (string.IsNullOrEmpty(companyidSelect))
                   //        {
                   //            string companyid = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                   //            cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>("COMPANYID", companyid);
                   //        }
                   //        else
                   //        {
                   //            cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>("COMPANYID", companyidSelect);
                   //        }

                   //    }
                   //};
                   #endregion
               }
               else
               {
                   if (string.IsNullOrEmpty(companyidSelect))
                   {
                       string companyid = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                       cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", companyid);
                   }
                   else
                   {
                       cboCompany.SelectedByObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>("COMPANYID", companyidSelect);
                   } 
               }
           }
            #endregion
        }
    }

    #endregion
}
