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
    public class V_OrgObjectVM
    {
        public V_OrgObjectVM()
        {
        }

        private List<V_OrgObject> _lstV_OrgObjects;
        public List<V_OrgObject> LstV_OrgObjects
        {
            get { return _lstV_OrgObjects; }
            set
            {
                _lstV_OrgObjects = value;
                OnPropertyChanged("LstV_OrgObjects");
            }
        }

        private V_OrgObject _currentV_OrgObject;
        public V_OrgObject CurrentV_OrgObject
        {
            get { return _currentV_OrgObject; }
            set
            {
                _currentV_OrgObject = value;
                OnPropertyChanged("CurrentV_OrgObject");
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

        public static void SaveV_OrgObject(string strEmployeeID, List<V_OrgObject> ents)
        {
            var tmps = (from o in SterlingService.Current.Database.Query<V_OrgObject, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_OrgObject(strEmployeeID);
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
            var ents = (from o in SterlingService.Current.Database.Query<V_OrgObject, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).Count();

            if (ents > 0)
            {
                bIsExists = true;
            }

            return bIsExists;
        }

        public static List<V_OrgObject> GetAllV_OrgObject(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_OrgObject, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteAllV_OrgObject(string strEmployeeID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_OrgObject, string>()
                        where o.LazyValue.Value.EmployeeID == strEmployeeID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }
    }
}
