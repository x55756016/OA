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
using SMT.SaaS.LocalData.Tables;

namespace SMT.SaaS.LocalData.ViewModel
{
    public class V_PermissionCheckVM
    {
        public V_PermissionCheckVM()
        {
        }

        private List<V_PermissionCheck> _lstV_PermissionChecks;
        public List<V_PermissionCheck> LstV_PermissionChecks
        {
            get { return _lstV_PermissionChecks; }
            set
            {
                _lstV_PermissionChecks = value;
                OnPropertyChanged("LstV_PermissionChecks");
            }
        }

        private V_PermissionCheck _currentV_PermissionCheck;
        public V_PermissionCheck CurrentV_PermissionCheck
        {
            get { return _currentV_PermissionCheck; }
            set
            {
                _currentV_PermissionCheck = value;
                OnPropertyChanged("CurrentV_PermissionCheck");
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

        public static void SaveV_PermissionCheck(string strEmployeeID, V_PermissionCheck ents)
        {
            var tmps = (from o in SterlingService.Current.Database.Query<V_PermissionCheck, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_PermissionCheck(strEmployeeID);
                }
            }
            SterlingService.Current.Database.Save(ents);
            SterlingService.Current.Database.Flush();
        }

        public static bool IsExists(string strEmployeeID)
        {
            bool bIsExists = false;
            var ents = (from o in SterlingService.Current.Database.Query<V_PermissionCheck, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).Count();

            if (ents > 0)
            {
                bIsExists = true;
            }

            return bIsExists;
        }

        public static V_PermissionCheck Get_V_PermissionCheck(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_PermissionCheck, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).FirstOrDefault();
            return ents;
        }

        public static void DeleteAllV_PermissionCheck(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_PermissionCheck, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }
    }
}
