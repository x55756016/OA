﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Services.Test.PlatformSiWS {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ShortCut", Namespace="http://schemas.datacontract.org/2004/07/SMT.SAAS.Platform.Model")]
    [System.SerializableAttribute()]
    public partial class ShortCut : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AssemplyNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FullNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IconPathField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IsSysNeedField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ModuleIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ParamsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ShortCutIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string TitelField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UserStateField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AssemplyName {
            get {
                return this.AssemplyNameField;
            }
            set {
                if ((object.ReferenceEquals(this.AssemplyNameField, value) != true)) {
                    this.AssemplyNameField = value;
                    this.RaisePropertyChanged("AssemplyName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FullName {
            get {
                return this.FullNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FullNameField, value) != true)) {
                    this.FullNameField = value;
                    this.RaisePropertyChanged("FullName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string IconPath {
            get {
                return this.IconPathField;
            }
            set {
                if ((object.ReferenceEquals(this.IconPathField, value) != true)) {
                    this.IconPathField = value;
                    this.RaisePropertyChanged("IconPath");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string IsSysNeed {
            get {
                return this.IsSysNeedField;
            }
            set {
                if ((object.ReferenceEquals(this.IsSysNeedField, value) != true)) {
                    this.IsSysNeedField = value;
                    this.RaisePropertyChanged("IsSysNeed");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ModuleID {
            get {
                return this.ModuleIDField;
            }
            set {
                if ((object.ReferenceEquals(this.ModuleIDField, value) != true)) {
                    this.ModuleIDField = value;
                    this.RaisePropertyChanged("ModuleID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Params {
            get {
                return this.ParamsField;
            }
            set {
                if ((object.ReferenceEquals(this.ParamsField, value) != true)) {
                    this.ParamsField = value;
                    this.RaisePropertyChanged("Params");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ShortCutID {
            get {
                return this.ShortCutIDField;
            }
            set {
                if ((object.ReferenceEquals(this.ShortCutIDField, value) != true)) {
                    this.ShortCutIDField = value;
                    this.RaisePropertyChanged("ShortCutID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Titel {
            get {
                return this.TitelField;
            }
            set {
                if ((object.ReferenceEquals(this.TitelField, value) != true)) {
                    this.TitelField = value;
                    this.RaisePropertyChanged("Titel");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string UserState {
            get {
                return this.UserStateField;
            }
            set {
                if ((object.ReferenceEquals(this.UserStateField, value) != true)) {
                    this.UserStateField = value;
                    this.RaisePropertyChanged("UserState");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="PlatformSiWS.IPlatformSiServices")]
    public interface IPlatformSiServices {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPlatformSiServices/AddShortCut", ReplyAction="http://tempuri.org/IPlatformSiServices/AddShortCutResponse")]
        bool AddShortCut(SMT.SAAS.Platform.Services.Test.PlatformSiWS.ShortCut model);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPlatformSiServices/AddShortCutByList", ReplyAction="http://tempuri.org/IPlatformSiServices/AddShortCutByListResponse")]
        bool AddShortCutByList(SMT.SAAS.Platform.Services.Test.PlatformSiWS.ShortCut[] models);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPlatformSiServices/AddShortCutByUser", ReplyAction="http://tempuri.org/IPlatformSiServices/AddShortCutByUserResponse")]
        bool AddShortCutByUser(SMT.SAAS.Platform.Services.Test.PlatformSiWS.ShortCut[] models, string userID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPlatformSiServices/GetShortCutByUser", ReplyAction="http://tempuri.org/IPlatformSiServices/GetShortCutByUserResponse")]
        SMT.SAAS.Platform.Services.Test.PlatformSiWS.ShortCut[] GetShortCutByUser(string userSysID);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IPlatformSiServicesChannel : SMT.SAAS.Platform.Services.Test.PlatformSiWS.IPlatformSiServices, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class PlatformSiServicesClient : System.ServiceModel.ClientBase<SMT.SAAS.Platform.Services.Test.PlatformSiWS.IPlatformSiServices>, SMT.SAAS.Platform.Services.Test.PlatformSiWS.IPlatformSiServices {
        
        public PlatformSiServicesClient() {
        }
        
        public PlatformSiServicesClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public PlatformSiServicesClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PlatformSiServicesClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PlatformSiServicesClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool AddShortCut(SMT.SAAS.Platform.Services.Test.PlatformSiWS.ShortCut model) {
            return base.Channel.AddShortCut(model);
        }
        
        public bool AddShortCutByList(SMT.SAAS.Platform.Services.Test.PlatformSiWS.ShortCut[] models) {
            return base.Channel.AddShortCutByList(models);
        }
        
        public bool AddShortCutByUser(SMT.SAAS.Platform.Services.Test.PlatformSiWS.ShortCut[] models, string userID) {
            return base.Channel.AddShortCutByUser(models, userID);
        }
        
        public SMT.SAAS.Platform.Services.Test.PlatformSiWS.ShortCut[] GetShortCutByUser(string userSysID) {
            return base.Channel.GetShortCutByUser(userSysID);
        }
    }
}
