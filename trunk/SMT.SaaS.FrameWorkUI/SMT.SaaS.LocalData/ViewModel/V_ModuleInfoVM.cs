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
using System.ComponentModel;
using System.Linq;
using SMT.SaaS.LocalData;
using SMT.SaaS.LocalData.Tables;

namespace SMT.SaaS.LocalData.ViewModel
{
    public class V_ModuleInfoVM : INotifyPropertyChanged
    {
        public V_ModuleInfoVM()
        {
        }

        private List<V_ModuleInfo> _lstV_ModuleInfos;
        public List<V_ModuleInfo> LstV_ModuleInfos
        {
            get { return _lstV_ModuleInfos; }
            set
            {
                _lstV_ModuleInfos = value;
                OnPropertyChanged("LstV_ModuleInfos");
            }
        }

        private V_ModuleInfo _currentV_ModuleInfo;
        public V_ModuleInfo CurrentV_ModuleInfo
        {
            get { return _currentV_ModuleInfo; }
            set
            {
                _currentV_ModuleInfo = value;
                OnPropertyChanged("CurrentV_ModuleInfo");
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

        public static void SaveV_ModuleInfo(string strUserID, List<V_ModuleInfo> ents)
        {
            V_ModuleInfo newV_ModuleInfo = new V_ModuleInfo();

            var tmps = (from o in SterlingService.Current.Database.Query<V_ModuleInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_ModuleInfo(strUserID);
                }
            }

            foreach (var o in ents)
            {
                SterlingService.Current.Database.Save(o);
            }

            SterlingService.Current.Database.Flush();
        }

        public static bool IsExists(string strUserID)
        {
            bool bIsExists = false;
            var ents = (from o in SterlingService.Current.Database.Query<V_ModuleInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).Count();

            if (ents > 0)
            {
                bIsExists = true;
            }

            return bIsExists;
        }

        public static List<V_ModuleInfo> GetAllV_ModuleInfo(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_ModuleInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteAllV_ModuleInfo(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_ModuleInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }

        public static void DeleteSelectedV_ModuleInfo(string strUserID, string strModuleID)
        {
            V_ModuleInfo deleteV_ModuleInfo = (from o in SterlingService.Current.Database.Query<V_ModuleInfo, string>()
                                               where o.LazyValue.Value.ModuleID == strModuleID && o.LazyValue.Value.UserID == strUserID
                                               select o.LazyValue.Value).FirstOrDefault();
            if (deleteV_ModuleInfo != null)
            {
                SterlingService.Current.Database.Delete(deleteV_ModuleInfo);
                SterlingService.Current.Database.Flush();
            }

            GetAllV_ModuleInfo(strUserID);
        }
    }
}
