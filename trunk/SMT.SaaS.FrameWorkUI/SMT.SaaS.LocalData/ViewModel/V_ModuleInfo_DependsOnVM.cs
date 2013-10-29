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
    public class V_ModuleInfo_DependsOnVM: INotifyPropertyChanged
    {
        public V_ModuleInfo_DependsOnVM()
        {
        }

        private List<V_ModuleInfo_DependsOn> _lstV_ModuleInfo_DependsOns;
        public List<V_ModuleInfo_DependsOn> LstV_ModuleInfo_DependsOns
        {
            get { return _lstV_ModuleInfo_DependsOns; }
            set
            {
                _lstV_ModuleInfo_DependsOns = value;
                OnPropertyChanged("LstV_ModuleInfo_DependsOns");
            }
        }

        private V_ModuleInfo_DependsOn _currentV_ModuleInfo_DependsOn;
        public V_ModuleInfo_DependsOn CurrentV_ModuleInfo_DependsOn
        {
            get { return _currentV_ModuleInfo_DependsOn; }
            set
            {
                _currentV_ModuleInfo_DependsOn = value;
                OnPropertyChanged("CurrentV_ModuleInfo_DependsOn");
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

        public static void SaveV_ModuleInfo_DependsOn(string strUserID, List<V_ModuleInfo_DependsOn> ents)
        {
            V_ModuleInfo_DependsOn newV_ModuleInfo_DependsOn = new V_ModuleInfo_DependsOn();

            var tmps = (from o in SterlingService.Current.Database.Query<V_ModuleInfo_DependsOn, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_ModuleInfo_DependsOn(strUserID);
                }
            }

            foreach (var o in ents)
            {
                SterlingService.Current.Database.Save(o);
            }

            SterlingService.Current.Database.Flush();
        }

        public static void DeleteAllV_ModuleInfo_DependsOn(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_ModuleInfo_DependsOn, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }

        public static List<V_ModuleInfo_DependsOn> GetAllV_ModuleInfo_DependsOn(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_ModuleInfo_DependsOn, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteSelectedV_ModuleInfo_DependsOn(string strUserID, string strModuleID)
        {
            V_ModuleInfo_DependsOn deleteV_ModuleInfo_DependsOn = (from o in SterlingService.Current.Database.Query<V_ModuleInfo_DependsOn, string>()
                                               where o.LazyValue.Value.ModuleID == strModuleID && o.LazyValue.Value.UserID == strUserID
                                               select o.LazyValue.Value).FirstOrDefault();
            if (deleteV_ModuleInfo_DependsOn != null)
            {
                SterlingService.Current.Database.Delete(deleteV_ModuleInfo_DependsOn);
                SterlingService.Current.Database.Flush();
            }

            GetAllV_ModuleInfo_DependsOn(strUserID);
        }
    }
}
