using SterlingDemoProject.Tables;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SMT.SaaS.LocalData.ViewModel
{
    public class V_PostInfoVM
    {
        public V_PostInfoVM()
        {
        }

        private List<V_PostInfo> _lstV_PostInfos;
        public List<V_PostInfo> LstV_PostInfos
        {
            get { return _lstV_PostInfos; }
            set
            {
                _lstV_PostInfos = value;
                OnPropertyChanged("LstV_PostInfos");
            }
        }

        private V_PostInfo _currentV_PostInfo;
        public V_PostInfo CurrentV_PostInfo
        {
            get { return _currentV_PostInfo; }
            set
            {
                _currentV_PostInfo = value;
                OnPropertyChanged("CurrentV_PostInfo");
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

        public static void SaveV_PostInfo(string strUserID, List<V_PostInfo> ents)
        {
            V_PostInfo newV_PostInfo = new V_PostInfo();

            var tmps = (from o in SterlingService.Current.Database.Query<V_PostInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_PostInfo(strUserID);
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
            var ents = (from o in SterlingService.Current.Database.Query<V_PostInfo, string>()
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

        public static List<V_PostInfo> GetAllV_PostInfo(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_PostInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            return ents;
        }

        public static void DeleteAllV_PostInfo(string strUserID)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_PostInfo, string>()
                        where o.LazyValue.Value.UserID == strUserID
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }

        public static void DeleteSelectedV_PostInfo(string strUserID, string strPostID)
        {
            V_PostInfo deleteV_PostInfo = (from o in SterlingService.Current.Database.Query<V_PostInfo, string>()
                                           where o.LazyValue.Value.POSTID == strPostID
                                               select o.LazyValue.Value).FirstOrDefault();
            if (deleteV_PostInfo != null)
            {
                SterlingService.Current.Database.Delete(deleteV_PostInfo);
                SterlingService.Current.Database.Flush();
            }

            GetAllV_PostInfo(strUserID);
        }
    }
}
