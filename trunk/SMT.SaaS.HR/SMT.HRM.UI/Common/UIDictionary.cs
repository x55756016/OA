using System;
using System.Collections.Generic;
using System.Windows;
using SMT.SAAS.ClientUtility;

namespace SMT.HRM.UI
{
    public partial class BaseForm
    {
        public BaseForm()
        {
            base.Loaded += new RoutedEventHandler(BaseForm_Loaded);
        }

        void BaseForm_Loaded(object sender, RoutedEventArgs e)
        {
            DictionaryManager dm = new DictionaryManager();
            if (!UIDictionary.DictOfDict.ContainsKey(this.GetType()))
            {
                if (this.Loaded != null)
                {
                    this.Loaded(this, new RoutedEventArgs());
                }
                return;
            }
            dm.OnDictionaryLoadCompleted += (o, args) =>
            {
                if (this.Loaded != null)
                {
                    this.Loaded(o, new RoutedEventArgs());
                }
            };
            dm.LoadDictionary(UIDictionary.DictOfDict[this.GetType()]);
        }

        public new event RoutedEventHandler Loaded;
    }

    public partial class BasePage
    {
        public BasePage()
        {
            SMT.SaaS.FrameworkUI.Validator.ValidatorService.ResourceMgr = SMT.SaaS.Globalization.Localization.ResourceMgr;
            SMT.HRM.UI.Utility.CheckResourceConverter();
            base.Loaded += new RoutedEventHandler(BaseForm_Loaded);
        }

        void BaseForm_Loaded(object sender, RoutedEventArgs e)
        {
            DictionaryManager dm = new DictionaryManager();
            if (!UIDictionary.DictOfDict.ContainsKey(this.GetType()))
            {
                if (this.Loaded != null)
                {
                    this.Loaded(this, new RoutedEventArgs());
                }
                return;
            }
            dm.OnDictionaryLoadCompleted += (o, args) =>
            {
                if (this.Loaded != null)
                {
                    this.Loaded(o, new RoutedEventArgs());
                }
            };
            dm.LoadDictionary(UIDictionary.DictOfDict[this.GetType()]);
        }

        public new event RoutedEventHandler Loaded;
    }

    public partial class UIDictionary
    {
        public static Dictionary<Type, List<string>> DictOfDict
        {
            get;
            set;
        }
        private static List<KeyValuePair<Type, List<string>>> dicts = new List<KeyValuePair<Type, List<string>>>();
        static UIDictionary()
        {
            try
            {
                GetUIDictionary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("1!!" + ex.Message);
            }
        }

        private static void GetUIDictionary()
        {
            try
            {
                GetUIDictionary_AKH();
                GetUIDictionary_WD();
                GetUIDictionary_WRY();
                GetUIDictionary_YDS();

                DictOfDict = new Dictionary<Type, List<string>>();

                foreach (var item in dicts)
                {
                    if (!DictOfDict.ContainsKey(item.Key))
                    {
                        DictOfDict.Add(item.Key, item.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("1!!" + ex.Message);
            }
        }
        /// <summary>
        /// 在此处写个子页面需要的字典.
        /// </summary>
        //private static void GetUIDictionary()
        //{
        //自己写分配到自己的页面的字典就好.此处是一个Add的例子.
        //出差申请
        //dicts.Add(new KeyValuePair<Type, List<string>>(typeof(BusinessApplicationsForm), new List<string> { "CHECKSTATE", "CITY", "VICHILESTANDARD", "VICHILELEVEL" }));
        //}
    }
}
