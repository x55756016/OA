using SMT.SaaS.LocalData.Tables;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SMT.SaaS.LocalData.ViewModel
{
    public class V_UserLoginVM
    {
        public V_UserLoginVM()
        {
        }

        private List<V_UserLogin> _lstV_PostInfos;
        public List<V_UserLogin> LstV_PostInfos
        {
            get { return _lstV_PostInfos; }
            set
            {
                _lstV_PostInfos = value;
                OnPropertyChanged("LstV_PostInfos");
            }
        }

        private V_UserLogin _currentV_UserLogin;
        public V_UserLogin CurrentV_UserLogin
        {
            get { return _currentV_UserLogin; }
            set
            {
                _currentV_UserLogin = value;
                OnPropertyChanged("CurrentV_UserLogin");
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

        public static void SaveV_UserLogin(string UserName, V_UserLogin ents)
        {
            var tmps = (from o in SterlingService.Current.Database.Query<V_UserLogin, string>()
                        where o.LazyValue.Value.UserName == UserName
                        select o.LazyValue.Value).ToList();

            if (tmps != null)
            {
                if (tmps.Count() > 0)
                {
                    DeleteAllV_PostInfo(UserName);
                }
            }
            SterlingService.Current.Database.Save(ents);
            SterlingService.Current.Database.Flush();
        }

        public static bool IsExists(string UserName)
        {
            bool bIsExists = false;
            var ents = (from o in SterlingService.Current.Database.Query<V_UserLogin, string>()
                        where o.LazyValue.Value.EMPLOYEEID == UserName
                        select o.LazyValue.Value).Count();

            if (ents > 0)
            {
                bIsExists = true;
            }

            return bIsExists;
        }

        public static V_UserLogin Get_V_UserLogin(string UserName)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_UserLogin, string>()
                        where o.LazyValue.Value.UserName == UserName
                        select o.LazyValue.Value).FirstOrDefault();
            return ents;
        }

        public static void DeleteAllV_PostInfo(string UserName)
        {
            var ents = (from o in SterlingService.Current.Database.Query<V_UserLogin, string>()
                        where o.LazyValue.Value.EMPLOYEEID == UserName
                        select o.LazyValue.Value).ToList();
            foreach (var o in ents)
            {
                SterlingService.Current.Database.Delete(o);
            }
        }
    }
}
