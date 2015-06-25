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
using SMT.Saas.Tools.PlatformWS;
using PlatformWS=SMT.Saas.Tools.PlatformWS;
using System.Collections.ObjectModel;

namespace SMT.SAAS.Platform.Model.Services
{
    public class ShortCutServices
    {
        private PlatformServicesClient _client = new PlatformServicesClient();

        public event EventHandler<CommonEventArgs<Model.ShortCut>> OnGetShortCutCompleted;

        public event EventHandler<ExecuteNoQueryEventArgs> OnRemoveShortCutCompleted;

        private CustomPermissionServices customPermissionServices;

        public ShortCutServices()
        {
            customPermissionServices = new CustomPermissionServices();
            customPermissionServices.OnGetUserCustomerPermissionCompleted += new EventHandler<ExecuteNoQueryEventArgs>(commonSv_OnGetUserCustomerPermissionCompleted);
            _client.GetShortCutByUserCompleted += new EventHandler<GetShortCutByUserCompletedEventArgs>(_client_GetShortCutByUserCompleted);
            _client.RemoveShortCutByUserCompleted += new EventHandler<RemoveShortCutByUserCompletedEventArgs>(_client_RemoveShortCutByUserCompleted);
            _client.AddShortCutByUserCompleted += new EventHandler<AddShortCutByUserCompletedEventArgs>(_client_AddShortCutByUserCompleted);
        }

        void _client_AddShortCutByUserCompleted(object sender, AddShortCutByUserCompletedEventArgs e)
        {

            var error = e.Error;
            var result = e.Result;
        }

        void _client_RemoveShortCutByUserCompleted(object sender, RemoveShortCutByUserCompletedEventArgs e)
        {
            if (OnRemoveShortCutCompleted != null)
                OnRemoveShortCutCompleted(this, new ExecuteNoQueryEventArgs(e.Result, e.Error));
        }

        void _client_GetShortCutByUserCompleted(object sender, GetShortCutByUserCompletedEventArgs e)
        {
            ObservableCollection<Model.ShortCut> result = new ObservableCollection<ShortCut>();
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {


                        //result.Add(new ShortCut
                        //{
                        //    IconPath = "/SMT.SAAS.Platform;Component/Images/icons/config.png",
                        //    ShortCutID = "a2274a93-70e6-49cf-869f-6db192f806e8",
                        //    Titel = "系统日志",
                        //    AssemplyName = "SMT.SAAS.Platform",
                        //    FullName = "SMT.SAAS.Platform.Xamls.SystemLogger, SMT.SAAS.Platform, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                        //    IsSysNeed = "1",
                        //    UserState = "1",
                        //    ModuleID = "SystemLog"
                        //});

                        if (CustomPermissionServices.HasNewsPublish)
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
                    }

                }
            }catch(Exception ex)
            {

            }finally
            {
                if (OnGetShortCutCompleted != null)
                    OnGetShortCutCompleted(this, new CommonEventArgs<Model.ShortCut>(result, e.Error));

            }
        }

        public void GetNewsPublishMenuByUser(string userid)
        {
            //ken 暂时直接加载新闻，不做权限判断
            ObservableCollection<Model.ShortCut> result = new ObservableCollection<ShortCut>();
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
            if (OnGetShortCutCompleted != null)
                OnGetShortCutCompleted(this, new CommonEventArgs<Model.ShortCut>(result,null));
           
            //customPermissionServices.GetCustomPermission(userid, "NEWSPUBLISH");

        }


        void commonSv_OnGetUserCustomerPermissionCompleted(object sender, ExecuteNoQueryEventArgs e)
        {
            _client.GetShortCutByUserAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
        }

        public void AddShortCutByUser(ObservableCollection<ShortCut> items, string userid)
        {
            ObservableCollection<PlatformWS.ShortCut> result = new ObservableCollection<PlatformWS.ShortCut>();
            foreach (var item in items)
            {
                if (item.ModuleID == "SystemLog" || item.ModuleID == "NewsManager")
                {
                    continue;
                }
                else
                {
                    PlatformWS.ShortCut v = item.CloneObject<PlatformWS.ShortCut>(new PlatformWS.ShortCut());
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
