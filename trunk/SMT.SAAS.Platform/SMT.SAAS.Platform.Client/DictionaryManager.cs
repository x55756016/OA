using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.LocalData.ViewModel;
using SMT.SaaS.LocalData.Tables;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 字典检测与加载类。用于负责系统字典的检测与按类型加载。
// 完成日期：2011-05-27 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.ClientUtility
{
    /// <summary>
    /// 字典检测与加载类。
    /// 用于负责系统字典的检测与按类型加载。
    /// </summary>
    public class DictionaryManager
    {
        //缓存字典类型，用于检测所属类型的字典是否加载成功。
        //加载成功的字典类型将会被存储到集合中。
        private static List<string> _cacheCategory = new List<string>();

        private PermissionServiceClient _client;

        private List<string> _tempCategory = new List<string>();
        private const string RES_DIC_KEY = "SYS_DICTIONARY";



        /// <summary>
        /// 字典加载完成事件。
        /// </summary>
        public event EventHandler<OnDictionaryLoadArgs> OnDictionaryLoadCompleted;

        public static void ClearCacheDictonary()
        {
            DictionaryManager._cacheCategory.Clear();
        }

        /// <summary>
        /// 创建一个字典加载的实例
        /// </summary>
        public DictionaryManager()
        {
            _client = ClientServices.BuildClient.PermissionClient;
            _client.GetDictionaryByCategoryArrayCompleted += new EventHandler<GetDictionaryByCategoryArrayCompletedEventArgs>(_client_GetDictionaryByCategoryArrayCompleted);

            permClient = new PermissionServiceClient();
            permClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(permClient_GetSysDictionaryByCategoryCompleted);

        }

        /// <summary>
        /// 根据单个字典类型加载字典。
        /// </summary>
        /// <param name="categoryName">字典名称</param>
        public void LoadDictionary(string categoryName)
        {
            List<string> categoryNames = new List<string>();
            categoryNames.Add(categoryName);
            this.LoadDictionary(categoryNames);
        }

        /// <summary>
        /// 根据多个字典类型加载字典。
        /// </summary>
        /// <param name="categoryNames"></param>
        public void LoadDictionary(List<string> categoryNames)
        {
            #region 按需加载
            if (categoryNames == null)
                throw new ArgumentNullException("categoryNames");

            this.LoadDictionaryByCategory(categoryNames);
            #endregion

            //#region 加载所有字典

            //if (IsLoad)
            //{
            //    if (this.OnDictionaryLoadCompleted != null)
            //        OnDictionaryLoadCompleted(this, new OnDictionaryLoadArgs() { Result = IsLoad, Error = null });
            //}
            //else
            //{
            //    LoadAll();
            //}

            //#endregion
        }

        private void LoadDictionaryByCategory(List<string> categoryNames)
        {
            List<string> tempList = CheckCategory(categoryNames);
            if (tempList.Count > 0)
            {
                ObservableCollection<string> categorys = new ObservableCollection<string>();
                foreach (var item in tempList)
                {
                    categorys.Add(item);
                }

                if (V_DictionaryInfoVM.IsExists(categorys.ToList()) == false)
                {
                    _client.GetDictionaryByCategoryArrayAsync(categorys);
                }
                else
                {
                    GetDictionaryInfoByLocal();
                }
            }
            else
            {
                if (this.OnDictionaryLoadCompleted != null)
                    OnDictionaryLoadCompleted(this, new OnDictionaryLoadArgs() { Result = true, Error = null });
            }
        }

        /// <summary>
        /// 检测类型是否加载，如果加载则不将类型返回。
        /// </summary>
        /// <param name="categoryNames"></param>
        /// <returns></returns>
        private List<string> CheckCategory(List<string> categoryNames)
        {
            List<string> tempCategory = new List<string>();

            foreach (var item in categoryNames)
            {
                if (!_cacheCategory.Contains(item))
                    tempCategory.Add(item);
            }
            _tempCategory = tempCategory;
            return tempCategory;
        }

        void _client_GetDictionaryByCategoryArrayCompleted(object sender, GetDictionaryByCategoryArrayCompletedEventArgs e)
        {

            Exception ex = e.Error;
            bool isLoad = false;

            if (e.Result != null)
            {
                ConventDictionary(e.Result.ToList());
                _cacheCategory.AddRange(_tempCategory);

                isLoad = true;
            }
            else
            {
                ex = new Exception("根据对应字典类型获取到的字典集合为空！");
            }

            if (this.OnDictionaryLoadCompleted != null)
                OnDictionaryLoadCompleted(this, new OnDictionaryLoadArgs() { Result = isLoad, Error = ex });

        }

        private void ConventDictionary(List<V_Dictionary> source)
        {
            List<T_SYS_DICTIONARY> sourceDic = null;
            if (!Application.Current.Resources.Contains(RES_DIC_KEY))
            { sourceDic = new List<T_SYS_DICTIONARY>(); }
            else
            {
                sourceDic = Application.Current.Resources[RES_DIC_KEY] as List<T_SYS_DICTIONARY>;
                Application.Current.Resources.Remove(RES_DIC_KEY);
            }

            foreach (var item in source)
            {
                T_SYS_DICTIONARY tempItem = new T_SYS_DICTIONARY();
                tempItem.DICTIONARYID = item.DICTIONARYID;
                tempItem.DICTIONARYNAME = item.DICTIONARYNAME;
                tempItem.DICTIONARYVALUE = item.DICTIONARYVALUE;
                tempItem.DICTIONCATEGORY = item.DICTIONCATEGORY;
                if (!string.IsNullOrEmpty(item.FATHERID))
                {
                    tempItem.T_SYS_DICTIONARY2 = new T_SYS_DICTIONARY();
                    tempItem.T_SYS_DICTIONARY2.DICTIONARYID = item.FATHERID;
                }
                var ent = sourceDic.Where(s => s.DICTIONARYID == item.DICTIONARYID).FirstOrDefault();
                if (ent != null)
                {
                    sourceDic.Remove(ent);
                    sourceDic.Add(tempItem);
                }
                else
                {
                    sourceDic.Add(tempItem);
                }
            }

            Application.Current.Resources.Add(RES_DIC_KEY, sourceDic);
            SaveDictionaryInfoByLocal(false, sourceDic);
        }

        #region 加载所有字典，此代码正式版中需要删除
        private PermissionServiceClient permClient = null;
        public static bool IsLoad = false;

        public void LoadAll()
        {
            permClient.GetSysDictionaryByCategoryAsync("");
        }

        void permClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_SYS_DICTIONARY> dicts = e.Result == null ? null : e.Result.ToList();
                AddToResourceDictionary<List<T_SYS_DICTIONARY>>("SYS_DICTIONARY", dicts);
                SaveDictionaryInfoByLocal(true, dicts);
                IsLoad = true;
            }

            if (this.OnDictionaryLoadCompleted != null)
                OnDictionaryLoadCompleted(this, new OnDictionaryLoadArgs() { Result = IsLoad, Error = e.Error });
        }

        private static void AddToResourceDictionary<T>(string key, T value)
        {
            try
            {
                if (Application.Current.Resources[key] == null)
                {
                    if (value != null)
                    {
                        if (Application.Current.Resources.Contains(key))
                            Application.Current.Resources.Remove(key);

                        Application.Current.Resources.Add(key, value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void GetDictionaryInfoByLocal()
        {
            List<V_Dictionary> dicts = new List<V_Dictionary>();
            List<V_DictionaryInfo> LocalDictionaryInfos = V_DictionaryInfoVM.GetAllV_DictionaryInfo();
            foreach (var item in LocalDictionaryInfos)
            {
                V_Dictionary dict = SMT.SAAS.Main.CurrentContext.Common.CloneObject<V_DictionaryInfo, V_Dictionary>(item, new V_Dictionary());
                dicts.Add(dict);
            }

            ConventDictionary(dicts);
        }


        private void SaveDictionaryInfoByLocal(bool bIsSaveAll, List<T_SYS_DICTIONARY> dicts)
        {
            List<V_DictionaryInfo> LocalDictionaryInfos = new List<V_DictionaryInfo>();
            foreach (var item in dicts)
            {
                V_DictionaryInfo dict = SMT.SAAS.Main.CurrentContext.Common.CloneObject<T_SYS_DICTIONARY, V_DictionaryInfo>(item, new V_DictionaryInfo());
                if (item.T_SYS_DICTIONARY2 != null)
                {
                    dict.FATHERID = item.T_SYS_DICTIONARY2.DICTIONARYID;
                }
                LocalDictionaryInfos.Add(dict);
            }

            V_DictionaryInfoVM.SaveV_DictionaryInfo(bIsSaveAll, LocalDictionaryInfos);
        }

        #endregion
    }

    /// <summary>
    /// 字典加载事件参数
    /// </summary>
    public class OnDictionaryLoadArgs : EventArgs
    {
        /// <summary>
        /// 是否加载成功
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// 是否存在异常
        /// </summary>
        public Exception Error { get; set; }
        public OnDictionaryLoadArgs()
        { }
    }

}
