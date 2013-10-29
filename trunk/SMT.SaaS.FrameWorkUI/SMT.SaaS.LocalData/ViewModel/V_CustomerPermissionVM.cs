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
    public class V_CustomerPermissionVM
    {
        public V_CustomerPermissionVM()
        {
        }

        private List<V_CustomerPermission> _lstV_CustomerPermissions;
        public List<V_CustomerPermission> LstV_CustomerPermissions
        {
            get { return _lstV_CustomerPermissions; }
            set
            {
                _lstV_CustomerPermissions = value;
                OnPropertyChanged("LstV_CustomerPermissions");
            }
        }

        private V_CustomerPermission _currentV_CustomerPermission;
        public V_CustomerPermission CurrentV_CustomerPermission
        {
            get { return _currentV_CustomerPermission; }
            set
            {
                _currentV_CustomerPermission = value;
                OnPropertyChanged("CurrentV_CustomerPermission");
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

        public static void SaveV_CustomerPermission(string strEmployeeID, List<V_CustomerPermission> ents)
        {
            var tmps = (from o in SterlingService.Current.Database.Query<V_CustomerPermission, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_CustomerPermission(strEmployeeID);
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
            var ents = (from o in SterlingService.Current.Database.Query<V_CustomerPermission, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).Count();

            if (ents > 0)
            {
                bIsExists = true;
            }

            return bIsExists;
        }

        public static List<V_CustomerPermission> GetAllV_CustomerPermission(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_CustomerPermission, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteAllV_CustomerPermission(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_CustomerPermission, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }
    }
}
