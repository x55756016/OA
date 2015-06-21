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
using System.Collections.Generic;
using System.Linq;
using SMT.SAAS.ClientUtility;

namespace SMT.SaaS.Permission.UI.AppControl
{
    public class DictionaryComboBox : ComboBox
    {
        public DependencyProperty SelectedValueProperty;
        public DependencyProperty CategoryProperty;
        public DependencyProperty IsShowNullProperty;
        private static DictionaryManager DictionNaryclinet;

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
            if (DictionNaryclinet == null)
            {
                DictionNaryclinet = new DictionaryManager();
            }
            IsShowNullProperty = DependencyProperty.Register("IsShowNull", typeof(bool), typeof(DictionaryComboBox)
                , new PropertyMetadata(true, new PropertyChangedCallback(DictionaryComboBox.OnIsShowNullChanged)));

            SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(string), typeof(DictionaryComboBox)
              , new PropertyMetadata("", new PropertyChangedCallback(DictionaryComboBox.OnSelectedValuePropertyChanged)));

            CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(DictionaryComboBox)
   , new PropertyMetadata("", new PropertyChangedCallback(DictionaryComboBox.OnCategoryPropertyChanged)));
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
        private void OnCategoryPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            BindItems(e.NewValue == null ? "" : e.NewValue.ToString());
        }
        private void OnIsShowNullChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                IsShowNull = (bool)e.NewValue;
            }
        }
        private void BindItems(string category)
        {
            List<T_SYS_DICTIONARY> dictss = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var q = from ent in dictss
                    where ent.DICTIONCATEGORY == category
                    select ent;
            if (q.Count() > 0)
            {
                List<T_SYS_DICTIONARY> dicts = q.ToList();
                BindComboBox(dicts, category, SelectedValue);
            }
            else
            {
                //ThreadPool.QueueUserWorkItem(delegate(object o)
                //{
                DictionNaryclinet.OnDictionaryLoadCompleted += (obj, args) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        var qBind = from ent in dictss
                                    where ent.DICTIONCATEGORY == category
                                    select ent;
                        if (qBind.Count() > 0)
                        {
                            List<T_SYS_DICTIONARY> dicts = qBind.ToList();
                            BindComboBox(dictss, category, SelectedValue);
                        }
                    });

                };
                DictionNaryclinet.LoadDictionary(category);
                //EventAttention.WaitOne();
                //});
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
            var objs = from d in dicts
                       where d.DICTIONCATEGORY == category
                       orderby d.DICTIONARYVALUE
                       select d;
            List<T_SYS_DICTIONARY> tmpDicts = objs.ToList();

            if (IsShowNull)
            {
                T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
                nuldict.DICTIONARYNAME = "NULL";
                nuldict.DICTIONARYVALUE = 0;
                tmpDicts.Insert(0, nuldict);
            }

            ItemsSource = tmpDicts;
            DisplayMemberPath = "DICTIONARYNAME";


            SetValue(SelectedValueProperty, defaultValue);

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
                    if (obj.DICTIONARYVALUE.ToString() == value)
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
