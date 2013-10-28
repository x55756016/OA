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
    public class V_CompanyInfoVM
    {
        public V_CompanyInfoVM()
        {
        }

        private List<V_CompanyInfo> _lstV_CompanyInfos;
        public List<V_CompanyInfo> LstV_CompanyInfos
        {
            get { return _lstV_CompanyInfos; }
            set
            {
                _lstV_CompanyInfos = value;
                OnPropertyChanged("LstV_CompanyInfos");
            }
        }

        private V_CompanyInfo _currentV_CompanyInfo;
        public V_CompanyInfo CurrentV_CompanyInfo
        {
            get { return _currentV_CompanyInfo; }
            set
            {
                _currentV_CompanyInfo = value;
                OnPropertyChanged("CurrentV_CompanyInfo");
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

        public static void SaveV_CompanyInfo(string strUserID, List<V_CompanyInfo> ents)
        {
            V_CompanyInfo newV_CompanyInfo = new V_CompanyInfo();

            var tmps = (from o in SterlingService.Current.Database.Query<V_CompanyInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_CompanyInfo(strUserID);
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
            var ents = (from o in SterlingService.Current.Database.Query<V_CompanyInfo, string>()
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

        public static List<V_CompanyInfo> GetAllV_CompanyInfo(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_CompanyInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteAllV_CompanyInfo(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_CompanyInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }

        public static void DeleteSelectedV_CompanyInfo(string strUserID, string strCompanyID)
        {
            V_CompanyInfo deleteV_CompanyInfo = (from o in SterlingService.Current.Database.Query<V_CompanyInfo, string>()
                                                 where o.LazyValue.Value.UserID == strUserID && o.LazyValue.Value.COMPANYID == strCompanyID
                                                 select o.LazyValue.Value).FirstOrDefault();
            if (deleteV_CompanyInfo != null)
            {
                SterlingService.Current.Database.Delete(deleteV_CompanyInfo);
                SterlingService.Current.Database.Flush();
            }

            GetAllV_CompanyInfo(strUserID);
        }
    }
}
