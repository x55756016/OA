using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SMT.SaaS.LocalData;
using SterlingDemoProject.Tables;

namespace SMT.SaaS.LocalData.ViewModel
{
    public class V_DictionaryInfoVM
    {
        public V_DictionaryInfoVM()
        {
        }

        private List<V_DictionaryInfo> _lstV_DictionaryInfos;
        public List<V_DictionaryInfo> LstV_DictionaryInfos
        {
            get { return _lstV_DictionaryInfos; }
            set
            {
                _lstV_DictionaryInfos = value;
                OnPropertyChanged("LstV_DictionaryInfos");
            }
        }

        private V_DictionaryInfo _currentV_DictionaryInfo;
        public V_DictionaryInfo CurrentV_DictionaryInfo
        {
            get { return _currentV_DictionaryInfo; }
            set
            {
                _currentV_DictionaryInfo = value;
                OnPropertyChanged("CurrentV_DictionaryInfo");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static void SaveV_DictionaryInfo(bool bIsAll, List<V_DictionaryInfo> ents)
        {
            V_DictionaryInfo newV_DictionaryInfo = new V_DictionaryInfo();

            var tmps = (from o in SterlingService.Current.Database.Query<V_DictionaryInfo, string>()
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    if (bIsAll)
                    {
                        DeleteAllV_DictionaryInfo();
                    }
                    else
                    {
                        foreach (var ent in ents)
                        {
                            DeleteSelectedV_DictionaryInfo(ent.DICTIONARYID);
                        }
                    }
                }
            }

            foreach (var o in ents)
            {
                SterlingService.Current.Database.Save(o);
            }

            SterlingService.Current.Database.Flush();
        }

        public static bool IsExists()
        {
            bool bIsExists = false;
            var ents = (from o in SterlingService.Current.Database.Query<V_DictionaryInfo, string>()
                        select o.LazyValue.Value).Count();

            if (ents != null)
            {
                if (ents > 0)
                {
                    bIsExists = true;
                }
            }

            return bIsExists;
        }

        public static bool IsExists(List<string> categorys)
        {
            bool bIsExists = false;
            string strCategorys = string.Empty;
            foreach (var c in categorys)
            {
                strCategorys += c + ",";
            }

            var ents = (from o in SterlingService.Current.Database.Query<V_DictionaryInfo, string>()
                        where o.LazyValue.Value.DICTIONARYID.Contains(strCategorys)
                        select o.LazyValue.Value).Count();

            if (ents != null)
            {
                if (ents > 0)
                {
                    bIsExists = true;
                }
            }

            return bIsExists;
        }

        public static List<V_DictionaryInfo> GetAllV_DictionaryInfo()
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_DictionaryInfo, string>()
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteAllV_DictionaryInfo()
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_DictionaryInfo, string>()
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }

        public static void DeleteSelectedV_DictionaryInfo(string strDictionaryID)
        {
            V_DictionaryInfo deleteV_DictionaryInfo = (from o in SterlingService.Current.Database.Query<V_DictionaryInfo, string>()
                                                       where o.LazyValue.Value.DICTIONARYID == strDictionaryID
                                                       select o.LazyValue.Value).FirstOrDefault();
            if (deleteV_DictionaryInfo != null)
            {
                SterlingService.Current.Database.Delete(deleteV_DictionaryInfo);
                SterlingService.Current.Database.Flush();
            }
        }
    }
}
