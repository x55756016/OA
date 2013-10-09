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

using SMT.Saas.Tools.PermissionWS;
using System.Linq;

using System.Collections.Generic;
using System.Threading;
using SMT.Saas.Tools.HrCommonServiceWS;

namespace SMT.SaaS.OA.UI.Class
{
    public class DictionaryComboBox : ComboBox
    {

        public DependencyProperty SelectedValueProperty;
        public DependencyProperty CategoryProperty;
        public DependencyProperty IsShowNullProperty;
        public HrCommonServiceClient Configclient;
        private PermissionServiceClient DictionNaryclinet;

        private AutoResetEvent EventAttention = null;
        private AutoResetEvent EventFunction = null;
        private AutoResetEvent[] EventArray = new AutoResetEvent[2];


        public string SelectedValue
        {
            get
            {
                return GetValue(SelectedValueProperty) as string;

            }
            set
            {
                SetValue(SelectedValueProperty, value);

            }
        }
        public bool IsShowNull
        {
            get
            {
                return (bool)GetValue(IsShowNullProperty);

            }
            set
            {
                SetValue(IsShowNullProperty, value);

            }
        }
        public string Category
        {
            get
            {
                return GetValue(CategoryProperty) as string;
            }
            set
            {
                SetValue(CategoryProperty, value);
            }
        }
        public DictionaryComboBox()
        {

            this.Style = (Style)Application.Current.Resources["ComboBoxStyle"];

            IsShowNullProperty = DependencyProperty.Register("IsShowNull", typeof(bool), typeof(DictionaryComboBox)
                , new PropertyMetadata(true, new PropertyChangedCallback(DictionaryComboBox.OnIsShowNullChanged)));

            SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(string), typeof(DictionaryComboBox)
              , new PropertyMetadata("", new PropertyChangedCallback(DictionaryComboBox.OnSelectedValuePropertyChanged)));

            CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(DictionaryComboBox)
   , new PropertyMetadata("", new PropertyChangedCallback(DictionaryComboBox.OnCategoryPropertyChanged)));

            EventAttention = new AutoResetEvent(false);
            EventFunction = new AutoResetEvent(false);
            EventArray[0] = EventAttention;
            EventArray[1] = EventFunction;
            Configclient = new HrCommonServiceClient();
            Configclient.GetAppConfigByNameCompleted += new EventHandler<GetAppConfigByNameCompletedEventArgs>(Configclient_GetAppConfigByNameCompleted);
            DictionNaryclinet = new PermissionServiceClient();
            DictionNaryclinet.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(DictionNaryclinet_GetSysDictionaryByCategoryCompleted);
        }

        public static void OnSelectedValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DictionaryComboBox obj = sender as DictionaryComboBox;
            if (obj != null)
            {
                obj.OnSelectedValuePropertyChanged(e);
            }
        }
        public static void OnCategoryPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DictionaryComboBox obj = sender as DictionaryComboBox;
            if (obj != null)
            {
                obj.OnCategoryPropertyChanged(e);
            }
        }
        public static void OnIsShowNullChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DictionaryComboBox obj = sender as DictionaryComboBox;
            if (obj != null)
            {
                obj.OnIsShowNullChanged(e);
            }
        }
        private string category = string.Empty;
        private void OnCategoryPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            category = e.NewValue.ToString();
            BindItems(e.NewValue == null ? "" : e.NewValue.ToString());
        }
        private void OnIsShowNullChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                IsShowNull = (bool)e.NewValue;
            }
        }
        private void BindItems(string cate)
        {
            ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                Configclient.GetAppConfigByNameAsync("GetDicitionnaryFromCache");
                //WaitHandle.WaitAll(EventArray);
                EventAttention.WaitOne();
            });
        }

        void Configclient_GetAppConfigByNameCompleted(object sender, GetAppConfigByNameCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "true")
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        List<T_SYS_DICTIONARY> dictss = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
                        BindComboBox(dictss, category, SelectedValue);
                    });

                }
                else
                {
                    DictionNaryclinet.GetSysDictionaryByCategoryAsync(category);
                }
            }
        }

        void DictionNaryclinet_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    List<T_SYS_DICTIONARY> dicts = e.Result.ToList();
                    BindComboBox(dicts, category, SelectedValue);
                });
            }
        }

        /// <summary>
        /// 可用于动态加载字典
        /// </summary>
        /// <param name="dicts"></param>
        /// <param name="category"></param>
        /// <param name="defaultValue"></param>
        public void BindComboBox(List<T_SYS_DICTIONARY> dicts, string category, string defaultValue)
        {
            //Deployment.Current.Dispatcher.BeginInvoke(() => this.DataContext = dicts);
            var objs = from d in dicts
                       where d.DICTIONCATEGORY == category
                       orderby d.DICTIONARYVALUE
                       select d;
            List<T_SYS_DICTIONARY> tmpDicts = objs.ToList();

            if (IsShowNull)
            {
                T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
                string dictname = Utility.GetResourceStr("PLEASESELECTED");

                nuldict.DICTIONARYNAME = dictname;
                nuldict.DICTIONARYVALUE = -1;
                tmpDicts.Insert(0, nuldict);
            }

            this.ItemsSource = tmpDicts;
            DisplayMemberPath = "DICTIONARYNAME";
            SetSelectedItem(defaultValue);
            EventAttention.Set();
        }
        private void OnSelectedValuePropertyChanged(DependencyPropertyChangedEventArgs e)
        {

            SetSelectedItem(e.NewValue == null ? "" : e.NewValue.ToString());
        }
        private void SetSelectedItem(string value)
        {
            foreach (var item in Items)
            {
                T_SYS_DICTIONARY obj = item as T_SYS_DICTIONARY;
                if (obj != null)
                {
                    if (obj.DICTIONARYVALUE.ToString() == value && obj.DICTIONARYNAME != "空")
                    {
                        SelectedItem = item;
                        break;
                    }
                }
            }
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            T_SYS_DICTIONARY dict = this.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SelectedValue = dict.DICTIONARYVALUE.ToString();
            }

        }
    }
}
