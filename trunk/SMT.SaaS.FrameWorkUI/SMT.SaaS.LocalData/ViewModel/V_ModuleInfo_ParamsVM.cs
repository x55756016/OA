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
    public class V_ModuleInfo_ParamsVM: INotifyPropertyChanged
    {
        public V_ModuleInfo_ParamsVM()
        {
        }

        private List<V_ModuleInfo_Params> _lstV_ModuleInfo_Paramss;
        public List<V_ModuleInfo_Params> LstV_ModuleInfo_Paramss
        {
            get { return _lstV_ModuleInfo_Paramss; }
            set
            {
                _lstV_ModuleInfo_Paramss = value;
                OnPropertyChanged("LstV_ModuleInfo_Paramss");
            }
        }

        private V_ModuleInfo_Params _currentV_ModuleInfo_Params;
        public V_ModuleInfo_Params CurrentV_ModuleInfo_Params
        {
            get { return _currentV_ModuleInfo_Params; }
            set
            {
                _currentV_ModuleInfo_Params = value;
                OnPropertyChanged("CurrentV_ModuleInfo_Params");
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

        public static void SaveV_ModuleInfo_Params(string strUserID, List<V_ModuleInfo_Params> ents)
        {
            V_ModuleInfo_Params newV_ModuleInfo_Params = new V_ModuleInfo_Params();

            var tmps = (from o in SterlingService.Current.Database.Query<V_ModuleInfo_Params, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_ModuleInfo_Params(strUserID);
                }
            }

            foreach (var o in ents)
            {
                SterlingService.Current.Database.Save(o);
            }

            SterlingService.Current.Database.Flush();
        }

        public static void DeleteAllV_ModuleInfo_Params(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_ModuleInfo_Params, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }

        public static List<V_ModuleInfo_Params> GetAllV_ModuleInfo_Params(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_ModuleInfo_Params, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteSelectedV_ModuleInfo_Params(string strUserID, string strModuleID)
        {
            V_ModuleInfo_Params deleteV_ModuleInfo_Params = (from o in SterlingService.Current.Database.Query<V_ModuleInfo_Params, string>()
                                               where o.LazyValue.Value.ModuleID == strModuleID && o.LazyValue.Value.UserID == strUserID
                                               select o.LazyValue.Value).FirstOrDefault();
            if (deleteV_ModuleInfo_Params != null)
            {
                SterlingService.Current.Database.Delete(deleteV_ModuleInfo_Params);
                SterlingService.Current.Database.Flush();
            }

            GetAllV_ModuleInfo_Params(strUserID);
        }
    }
}
