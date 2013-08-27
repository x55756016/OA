using System;
using System.Collections.ObjectModel;
using SMT.SAAS.Platform.Client;
using System.Diagnostics;

namespace SMT.SAAS.Platform.Model.Services
{
    public class ModuleServices
    {
        private Platform.Client.PlatformWS.PlatformServicesClient _client = new Client.PlatformWS.PlatformServicesClient();

        public event EventHandler<GetEntityListEventArgs<Model.ModuleInfo>> OnGetModulesCompleted;

        public ModuleServices()
        {
            _client.GetModuleByCodesCompleted += new EventHandler<Client.PlatformWS.GetModuleByCodesCompletedEventArgs>(_client_GetModuleByCodesCompleted);
            _client.GetModuleCatalogByUserCompleted += new EventHandler<Client.PlatformWS.GetModuleCatalogByUserCompletedEventArgs>(_client_GetModuleCatalogByUserCompleted);
        }

        public void GetModuleByCodes(string[] codes)
        {
            ObservableCollection<string> codelist = new ObservableCollection<string>();
            foreach (var item in codes)
            {
                codelist.Add(item);
            }
            _client.GetModuleByCodesAsync(codelist);

        }

        public void GetModuleCatalogByUser(string userSysid)
        {
            _client.GetModuleCatalogByUserAsync(userSysid);
        }


        void _client_GetModuleCatalogByUserCompleted(object sender, Client.PlatformWS.GetModuleCatalogByUserCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {

                    ObservableCollection<Model.ModuleInfo> result = new ObservableCollection<ModuleInfo>();
                    foreach (var item in e.Result)
                    {
                        Model.ModuleInfo v = item.CloneObject<Model.ModuleInfo>(new Model.ModuleInfo());
                        if (item.DependsOn.Count > 0)
                        {
                            foreach (var dependsitem in item.DependsOn)
                            {
                                v.DependsOn.Add(dependsitem);
                            }

                        }
                        result.Add(v);
                    }

                    if (OnGetModulesCompleted != null)
                        OnGetModulesCompleted(this, new GetEntityListEventArgs<Model.ModuleInfo>(result, e.Error));

                }

            }
        }

        void _client_GetModuleByCodesCompleted(object sender, Client.PlatformWS.GetModuleByCodesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {

                    ObservableCollection<Model.ModuleInfo> result = new ObservableCollection<ModuleInfo>();
                    foreach (var item in e.Result)
                    {
                        Model.ModuleInfo v = item.CloneObject<Model.ModuleInfo>(new Model.ModuleInfo());
                        result.Add(v);
                    }

                    if (OnGetModulesCompleted != null)
                        OnGetModulesCompleted(this, new GetEntityListEventArgs<Model.ModuleInfo>(result, e.Error));

                }

            }
        }


    }
}
