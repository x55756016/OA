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
using SterlingDemoProject.Tables;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace SMT.SaaS.LocalData.ViewModel
{
    public class V_DepartmentInfoVM
    {
        public V_DepartmentInfoVM()
        {
        }

        private List<V_DepartmentInfo> _lstV_DepartmentInfos;
        public List<V_DepartmentInfo> LstV_DepartmentInfos
        {
            get { return _lstV_DepartmentInfos; }
            set
            {
                _lstV_DepartmentInfos = value;
                OnPropertyChanged("LstV_DepartmentInfos");
            }
        }

        private V_DepartmentInfo _currentV_DepartmentInfo;
        public V_DepartmentInfo CurrentV_DepartmentInfo
        {
            get { return _currentV_DepartmentInfo; }
            set
            {
                _currentV_DepartmentInfo = value;
                OnPropertyChanged("CurrentV_DepartmentInfo");
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

        public static void SaveV_DepartmentInfo(string strUserID, List<V_DepartmentInfo> ents)
        {
            V_DepartmentInfo newV_DepartmentInfo = new V_DepartmentInfo();

            var tmps = (from o in SterlingService.Current.Database.Query<V_DepartmentInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_DepartmentInfo(strUserID);
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
            var ents = (from o in SterlingService.Current.Database.Query<V_DepartmentInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
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

        public static List<V_DepartmentInfo> GetAllV_DepartmentInfo(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_DepartmentInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteAllV_DepartmentInfo(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_DepartmentInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }

        public static void DeleteSelectedV_DepartmentInfo(string strUserID, string strDepartmentID)
        {
            V_DepartmentInfo deleteV_DepartmentInfo = (from o in SterlingService.Current.Database.Query<V_DepartmentInfo, string>()
                                                       where o.LazyValue.Value.UserID == strUserID && o.LazyValue.Value.DEPARTMENTID == strDepartmentID
                                                 select o.LazyValue.Value).FirstOrDefault();
            if (deleteV_DepartmentInfo != null)
            {
                SterlingService.Current.Database.Delete(deleteV_DepartmentInfo);
                SterlingService.Current.Database.Flush();
            }

            GetAllV_DepartmentInfo(strUserID);
        }
    }
}
