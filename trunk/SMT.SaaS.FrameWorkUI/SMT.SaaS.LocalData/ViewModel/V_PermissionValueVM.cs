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
using SMT.SaaS.LocalData.Tables;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SMT.SaaS.LocalData.ViewModel
{
    public class V_PermissionValueVM
    {
        public V_PermissionValueVM()
        {
        }

        private List<V_PermissionValue> _lstV_PermissionValues;
        public List<V_PermissionValue> LstV_PermissionValues
        {
            get { return _lstV_PermissionValues; }
            set
            {
                _lstV_PermissionValues = value;
                OnPropertyChanged("LstV_PermissionValues");
            }
        }

        private V_PermissionValue _currentV_PermissionValue;
        public V_PermissionValue CurrentV_PermissionValue
        {
            get { return _currentV_PermissionValue; }
            set
            {
                _currentV_PermissionValue = value;
                OnPropertyChanged("CurrentV_PermissionValue");
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

        public static void SaveV_PermissionValue(string strEmployeeID, List<V_PermissionValue> ents)
        {
            var tmps = (from o in SterlingService.Current.Database.Query<V_PermissionValue, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_PermissionValue(strEmployeeID);
                }
            }

            foreach (var o in ents)
            {
                SterlingService.Current.Database.Save(o);
            }

            SterlingService.Current.Database.Flush();
        }

        public static bool IsExists(string strEmployeeID)
        {
            bool bIsExists = false;
            var ents = (from o in SterlingService.Current.Database.Query<V_PermissionValue, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).Count();

            if (ents > 0)
            {
                bIsExists = true;
            }

            return bIsExists;
        }

        public static List<V_PermissionValue> GetAllV_PermissionValue(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_PermissionValue, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteAllV_PermissionValue(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_PermissionValue, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }
    }
}
