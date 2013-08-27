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
using SMT.SAAS.Platform.Client;
using System.Collections.ObjectModel;

namespace SMT.SAAS.Platform.Model.Services
{
    public class ShortCutServices
    {
        private Platform.Client.PlatformWS.PlatformServicesClient _client = new Client.PlatformWS.PlatformServicesClient();

        public event EventHandler<GetEntityListEventArgs<Model.ShortCut>> OnGetShortCutCompleted;

        public event EventHandler<ExecuteNoQueryEventArgs> OnRemoveShortCutCompleted;

        private CommonServices commonSv;

        public ShortCutServices()
        {
            commonSv = new CommonServices();
            commonSv.OnGetUserCustomerPermissionCompleted += new EventHandler<ExecuteNoQueryEventArgs>(commonSv_OnGetUserCustomerPermissionCompleted);
            _client.GetShortCutByUserCompleted += new EventHandler<Client.PlatformWS.GetShortCutByUserCompletedEventArgs>(_client_GetShortCutByUserCompleted);
            _client.RemoveShortCutByUserCompleted += new EventHandler<Client.PlatformWS.RemoveShortCutByUserCompletedEventArgs>(_client_RemoveShortCutByUserCompleted);
            _client.AddShortCutByUserCompleted += new EventHandler<Client.PlatformWS.AddShortCutByUserCompletedEventArgs>(_client_AddShortCutByUserCompleted);
        }

        void _client_AddShortCutByUserCompleted(object sender, Client.PlatformWS.AddShortCutByUserCompletedEventArgs e)
        {

            var error = e.Error;
            var result = e.Result;
        }

        void _client_RemoveShortCutByUserCompleted(object sender, Client.PlatformWS.RemoveShortCutByUserCompletedEventArgs e)
        {
            if (OnRemoveShortCutCompleted != null)
                OnRemoveShortCutCompleted(this, new ExecuteNoQueryEventArgs(e.Result, e.Error));
        }

        void _client_GetShortCutByUserCompleted(object sender, Client.PlatformWS.GetShortCutByUserCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    ObservableCollection<Model.ShortCut> result = new ObservableCollection<ShortCut>();


                    result.Add(new ShortCut
                    {
                        IconPath = "/SMT.SAAS.Platform;Component/Images/icons/config.png",
                        ShortCutID = "a2274a93-70e6-49cf-869f-6db192f806e8",
                        Titel = "系统日志",
                        AssemplyName = "SMT.SAAS.Platform",
                        FullName = "SMT.SAAS.Platform.Xamls.SystemLogger, SMT.SAAS.Platform, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                        IsSysNeed = "1",
                        UserState = "1",
                        ModuleID = "SystemLog"
                    });

                    if (CommonServices.HasNewsPublish)
                    {
                        result.Add(new ShortCut
                        {
                            IconPath = "/SMT.SaaS.FrameworkUI;Component/Images/icon/News.png",
                            ShortCutID = "a2274a93-70e6-49cf-869f-6db192f806e9",
                            Titel = "新闻管理",
                            AssemplyName = "SMT.SAAS.Platform.WebParts",
                            FullName = "SMT.SAAS.Platform.WebParts.Views.NewsManager, SMT.SAAS.Platform.WebParts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                            IsSysNeed = "1",
                            UserState = "1",
                            ModuleID = "NewsManager"
                        });
                    }


                    foreach (var item in e.Result)
                    {
                        Model.ShortCut v = item.CloneObject<Model.ShortCut>(new Model.ShortCut());

                        if (v.ModuleID != "NewsManager")
                        {
                            if (v.IconPath != "none")
                            {
                                result.Add(v);
                            }
                        }
                    }
                    if (OnGetShortCutCompleted != null)
                        OnGetShortCutCompleted(this, new GetEntityListEventArgs<Model.ShortCut>(result, e.Error));
                }

            }
        }

        public void GetShortCutByUser(string userid)
        {

            commonSv.GetCustomPermission(userid, "NEWSPUBLISH");

        }


        void commonSv_OnGetUserCustomerPermissionCompleted(object sender, ExecuteNoQueryEventArgs e)
        {
            _client.GetShortCutByUserAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
        }

        public void AddShortCutByUser(ObservableCollection<ShortCut> items, string userid)
        {
            ObservableCollection<Client.PlatformWS.ShortCut> result = new ObservableCollection<Client.PlatformWS.ShortCut>();
            foreach (var item in items)
            {
                if (item.ModuleID == "SystemLog" || item.ModuleID == "NewsManager")
                {
                    continue;
                }
                else
                {
                    Client.PlatformWS.ShortCut v = item.CloneObject<Client.PlatformWS.ShortCut>(new Client.PlatformWS.ShortCut());
                    result.Add(v);
                }
            }
            _client.AddShortCutByUserAsync(result, userid);

        }

        public void RemoveShortCutByUser(string shortCutID, string userID)
        {
            _client.RemoveShortCutByUserAsync(shortCutID, userID);
        }
    }
}
