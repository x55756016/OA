﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.1
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This code was auto-generated by Microsoft.Silverlight.ServiceReference, version 4.0.50401.0
// 
namespace SMT.Saas.Tools.HrCommonServiceWS {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="", ConfigurationName="HrCommonServiceWS.HrCommonService")]
    public interface HrCommonService {
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:HrCommonService/GetAppConfigByName", ReplyAction="urn:HrCommonService/GetAppConfigByNameResponse")]
        System.IAsyncResult BeginGetAppConfigByName(string AppConfigName, System.AsyncCallback callback, object asyncState);
        
        string EndGetAppConfigByName(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:HrCommonService/GetEntityProNameByEnityName", ReplyAction="urn:HrCommonService/GetEntityProNameByEnityNameResponse")]
        System.IAsyncResult BeginGetEntityProNameByEnityName(string entityName, string masterIDName, System.AsyncCallback callback, object asyncState);
        
        System.Collections.ObjectModel.ObservableCollection<string> EndGetEntityProNameByEnityName(System.IAsyncResult result);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface HrCommonServiceChannel : SMT.Saas.Tools.HrCommonServiceWS.HrCommonService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetAppConfigByNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetAppConfigByNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public string Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetEntityProNameByEnityNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetEntityProNameByEnityNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public System.Collections.ObjectModel.ObservableCollection<string> Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((System.Collections.ObjectModel.ObservableCollection<string>)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class HrCommonServiceClient : System.ServiceModel.ClientBase<SMT.Saas.Tools.HrCommonServiceWS.HrCommonService>, SMT.Saas.Tools.HrCommonServiceWS.HrCommonService {
        
        private BeginOperationDelegate onBeginGetAppConfigByNameDelegate;
        
        private EndOperationDelegate onEndGetAppConfigByNameDelegate;
        
        private System.Threading.SendOrPostCallback onGetAppConfigByNameCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetEntityProNameByEnityNameDelegate;
        
        private EndOperationDelegate onEndGetEntityProNameByEnityNameDelegate;
        
        private System.Threading.SendOrPostCallback onGetEntityProNameByEnityNameCompletedDelegate;
        
        private BeginOperationDelegate onBeginOpenDelegate;
        
        private EndOperationDelegate onEndOpenDelegate;
        
        private System.Threading.SendOrPostCallback onOpenCompletedDelegate;
        
        private BeginOperationDelegate onBeginCloseDelegate;
        
        private EndOperationDelegate onEndCloseDelegate;
        
        private System.Threading.SendOrPostCallback onCloseCompletedDelegate;
        
        public HrCommonServiceClient() {
        }
        
        public HrCommonServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public HrCommonServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public HrCommonServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public HrCommonServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Net.CookieContainer CookieContainer {
            get {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    return httpCookieContainerManager.CookieContainer;
                }
                else {
                    return null;
                }
            }
            set {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    httpCookieContainerManager.CookieContainer = value;
                }
                else {
                    throw new System.InvalidOperationException("无法设置 CookieContainer。请确保绑定包含 HttpCookieContainerBindingElement。");
                }
            }
        }
        
        public event System.EventHandler<GetAppConfigByNameCompletedEventArgs> GetAppConfigByNameCompleted;
        
        public event System.EventHandler<GetEntityProNameByEnityNameCompletedEventArgs> GetEntityProNameByEnityNameCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult SMT.Saas.Tools.HrCommonServiceWS.HrCommonService.BeginGetAppConfigByName(string AppConfigName, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetAppConfigByName(AppConfigName, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        string SMT.Saas.Tools.HrCommonServiceWS.HrCommonService.EndGetAppConfigByName(System.IAsyncResult result) {
            return base.Channel.EndGetAppConfigByName(result);
        }
        
        private System.IAsyncResult OnBeginGetAppConfigByName(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string AppConfigName = ((string)(inValues[0]));
            return ((SMT.Saas.Tools.HrCommonServiceWS.HrCommonService)(this)).BeginGetAppConfigByName(AppConfigName, callback, asyncState);
        }
        
        private object[] OnEndGetAppConfigByName(System.IAsyncResult result) {
            string retVal = ((SMT.Saas.Tools.HrCommonServiceWS.HrCommonService)(this)).EndGetAppConfigByName(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetAppConfigByNameCompleted(object state) {
            if ((this.GetAppConfigByNameCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetAppConfigByNameCompleted(this, new GetAppConfigByNameCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetAppConfigByNameAsync(string AppConfigName) {
            this.GetAppConfigByNameAsync(AppConfigName, null);
        }
        
        public void GetAppConfigByNameAsync(string AppConfigName, object userState) {
            if ((this.onBeginGetAppConfigByNameDelegate == null)) {
                this.onBeginGetAppConfigByNameDelegate = new BeginOperationDelegate(this.OnBeginGetAppConfigByName);
            }
            if ((this.onEndGetAppConfigByNameDelegate == null)) {
                this.onEndGetAppConfigByNameDelegate = new EndOperationDelegate(this.OnEndGetAppConfigByName);
            }
            if ((this.onGetAppConfigByNameCompletedDelegate == null)) {
                this.onGetAppConfigByNameCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetAppConfigByNameCompleted);
            }
            base.InvokeAsync(this.onBeginGetAppConfigByNameDelegate, new object[] {
                        AppConfigName}, this.onEndGetAppConfigByNameDelegate, this.onGetAppConfigByNameCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult SMT.Saas.Tools.HrCommonServiceWS.HrCommonService.BeginGetEntityProNameByEnityName(string entityName, string masterIDName, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetEntityProNameByEnityName(entityName, masterIDName, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Collections.ObjectModel.ObservableCollection<string> SMT.Saas.Tools.HrCommonServiceWS.HrCommonService.EndGetEntityProNameByEnityName(System.IAsyncResult result) {
            return base.Channel.EndGetEntityProNameByEnityName(result);
        }
        
        private System.IAsyncResult OnBeginGetEntityProNameByEnityName(object[] inValues, System.AsyncCallback callback, object asyncState) {
            string entityName = ((string)(inValues[0]));
            string masterIDName = ((string)(inValues[1]));
            return ((SMT.Saas.Tools.HrCommonServiceWS.HrCommonService)(this)).BeginGetEntityProNameByEnityName(entityName, masterIDName, callback, asyncState);
        }
        
        private object[] OnEndGetEntityProNameByEnityName(System.IAsyncResult result) {
            System.Collections.ObjectModel.ObservableCollection<string> retVal = ((SMT.Saas.Tools.HrCommonServiceWS.HrCommonService)(this)).EndGetEntityProNameByEnityName(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetEntityProNameByEnityNameCompleted(object state) {
            if ((this.GetEntityProNameByEnityNameCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetEntityProNameByEnityNameCompleted(this, new GetEntityProNameByEnityNameCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetEntityProNameByEnityNameAsync(string entityName, string masterIDName) {
            this.GetEntityProNameByEnityNameAsync(entityName, masterIDName, null);
        }
        
        public void GetEntityProNameByEnityNameAsync(string entityName, string masterIDName, object userState) {
            if ((this.onBeginGetEntityProNameByEnityNameDelegate == null)) {
                this.onBeginGetEntityProNameByEnityNameDelegate = new BeginOperationDelegate(this.OnBeginGetEntityProNameByEnityName);
            }
            if ((this.onEndGetEntityProNameByEnityNameDelegate == null)) {
                this.onEndGetEntityProNameByEnityNameDelegate = new EndOperationDelegate(this.OnEndGetEntityProNameByEnityName);
            }
            if ((this.onGetEntityProNameByEnityNameCompletedDelegate == null)) {
                this.onGetEntityProNameByEnityNameCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetEntityProNameByEnityNameCompleted);
            }
            base.InvokeAsync(this.onBeginGetEntityProNameByEnityNameDelegate, new object[] {
                        entityName,
                        masterIDName}, this.onEndGetEntityProNameByEnityNameDelegate, this.onGetEntityProNameByEnityNameCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginOpen(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(callback, asyncState);
        }
        
        private object[] OnEndOpen(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndOpen(result);
            return null;
        }
        
        private void OnOpenCompleted(object state) {
            if ((this.OpenCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.OpenCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void OpenAsync() {
            this.OpenAsync(null);
        }
        
        public void OpenAsync(object userState) {
            if ((this.onBeginOpenDelegate == null)) {
                this.onBeginOpenDelegate = new BeginOperationDelegate(this.OnBeginOpen);
            }
            if ((this.onEndOpenDelegate == null)) {
                this.onEndOpenDelegate = new EndOperationDelegate(this.OnEndOpen);
            }
            if ((this.onOpenCompletedDelegate == null)) {
                this.onOpenCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnOpenCompleted);
            }
            base.InvokeAsync(this.onBeginOpenDelegate, null, this.onEndOpenDelegate, this.onOpenCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginClose(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginClose(callback, asyncState);
        }
        
        private object[] OnEndClose(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndClose(result);
            return null;
        }
        
        private void OnCloseCompleted(object state) {
            if ((this.CloseCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.CloseCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void CloseAsync() {
            this.CloseAsync(null);
        }
        
        public void CloseAsync(object userState) {
            if ((this.onBeginCloseDelegate == null)) {
                this.onBeginCloseDelegate = new BeginOperationDelegate(this.OnBeginClose);
            }
            if ((this.onEndCloseDelegate == null)) {
                this.onEndCloseDelegate = new EndOperationDelegate(this.OnEndClose);
            }
            if ((this.onCloseCompletedDelegate == null)) {
                this.onCloseCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnCloseCompleted);
            }
            base.InvokeAsync(this.onBeginCloseDelegate, null, this.onEndCloseDelegate, this.onCloseCompletedDelegate, userState);
        }
        
        protected override SMT.Saas.Tools.HrCommonServiceWS.HrCommonService CreateChannel() {
            return new HrCommonServiceClientChannel(this);
        }
        
        private class HrCommonServiceClientChannel : ChannelBase<SMT.Saas.Tools.HrCommonServiceWS.HrCommonService>, SMT.Saas.Tools.HrCommonServiceWS.HrCommonService {
            
            public HrCommonServiceClientChannel(System.ServiceModel.ClientBase<SMT.Saas.Tools.HrCommonServiceWS.HrCommonService> client) : 
                    base(client) {
            }
            
            public System.IAsyncResult BeginGetAppConfigByName(string AppConfigName, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[1];
                _args[0] = AppConfigName;
                System.IAsyncResult _result = base.BeginInvoke("GetAppConfigByName", _args, callback, asyncState);
                return _result;
            }
            
            public string EndGetAppConfigByName(System.IAsyncResult result) {
                object[] _args = new object[0];
                string _result = ((string)(base.EndInvoke("GetAppConfigByName", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetEntityProNameByEnityName(string entityName, string masterIDName, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[2];
                _args[0] = entityName;
                _args[1] = masterIDName;
                System.IAsyncResult _result = base.BeginInvoke("GetEntityProNameByEnityName", _args, callback, asyncState);
                return _result;
            }
            
            public System.Collections.ObjectModel.ObservableCollection<string> EndGetEntityProNameByEnityName(System.IAsyncResult result) {
                object[] _args = new object[0];
                System.Collections.ObjectModel.ObservableCollection<string> _result = ((System.Collections.ObjectModel.ObservableCollection<string>)(base.EndInvoke("GetEntityProNameByEnityName", _args, result)));
                return _result;
            }
        }
    }
}