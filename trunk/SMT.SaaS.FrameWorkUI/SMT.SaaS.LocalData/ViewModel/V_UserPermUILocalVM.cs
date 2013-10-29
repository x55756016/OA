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
    public class V_UserPermUILocalVM
    {
        public V_UserPermUILocalVM()
        {
        }

        private List<V_UserPermUILocal> _lstV_UserPermUILocals;
        public List<V_UserPermUILocal> LstV_UserPermUILocals
        {
            get { return _lstV_UserPermUILocals; }
            set
            {
                _lstV_UserPermUILocals = value;
                OnPropertyChanged("LstV_UserPermUILocals");
            }
        }

        private V_UserPermUILocal _currentV_UserPermUILocal;
        public V_UserPermUILocal CurrentV_UserPermUILocal
        {
            get { return _currentV_UserPermUILocal; }
            set
            {
                _currentV_UserPermUILocal = value;
                OnPropertyChanged("CurrentV_UserPermUILocal");
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

        public static void SaveV_UserPermUILocal(string strEmployeeID, List<V_UserPermUILocal> ents)
        {
            var tmps = (from o in SterlingService.Current.Database.Query<V_UserPermUILocal, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_UserPermUILocal(strEmployeeID);
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
            var ents = (from o in SterlingService.Current.Database.Query<V_UserPermUILocal, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).Count();

            if (ents > 0)
            {
                bIsExists = true;
            }

            return bIsExists;
        }

        public static bool IsExists(string strEmployeeID, string strMenuID)
        {
            bool bIsExists = false;
            var ents = (from o in SterlingService.Current.Database.Query<V_UserPermUILocal, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID && o.LazyValue.Value.EntityMenuID == strMenuID
                        select o.LazyValue.Value).Count();

            if (ents > 0)
            {
                bIsExists = true;
            }

            return bIsExists;
        }

        public static List<V_UserPermUILocal> GetAllV_UserPermUILocal(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_UserPermUILocal, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteAllV_UserPermUILocal(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_UserPermUILocal, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }
    }
}
