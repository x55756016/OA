﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.1
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SMT.SaaS.BLLCommonServices.TKServicesWS {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="BaseObject", Namespace="http://schemas.datacontract.org/2004/07/SMT.Portal.Communication")]
    [System.SerializableAttribute()]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(SMT.SaaS.BLLCommonServices.TKServicesWS.ResultObject))]
    public partial class BaseObject : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string CommandKeyField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte[] DataContentField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string[] ParamsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string TokenField;
        
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
        public string CommandKey {
            get {
                return this.CommandKeyField;
            }
            set {
                if ((object.ReferenceEquals(this.CommandKeyField, value) != true)) {
                    this.CommandKeyField = value;
                    this.RaisePropertyChanged("CommandKey");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] DataContent {
            get {
                return this.DataContentField;
            }
            set {
                if ((object.ReferenceEquals(this.DataContentField, value) != true)) {
                    this.DataContentField = value;
                    this.RaisePropertyChanged("DataContent");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string[] Params {
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
        public string Token {
            get {
                return this.TokenField;
            }
            set {
                if ((object.ReferenceEquals(this.TokenField, value) != true)) {
                    this.TokenField = value;
                    this.RaisePropertyChanged("Token");
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
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ResultObject", Namespace="http://schemas.datacontract.org/2004/07/SMT.Portal.Communication")]
    [System.SerializableAttribute()]
    public partial class ResultObject : SMT.SaaS.BLLCommonServices.TKServicesWS.BaseObject {
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ErrorMessageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool StateField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ErrorMessage {
            get {
                return this.ErrorMessageField;
            }
            set {
                if ((object.ReferenceEquals(this.ErrorMessageField, value) != true)) {
                    this.ErrorMessageField = value;
                    this.RaisePropertyChanged("ErrorMessage");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool State {
            get {
                return this.StateField;
            }
            set {
                if ((this.StateField.Equals(value) != true)) {
                    this.StateField = value;
                    this.RaisePropertyChanged("State");
                }
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="TKServicesWS.ITKServices")]
    public interface ITKServices {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITKServices/Execute", ReplyAction="http://tempuri.org/ITKServices/ExecuteResponse")]
        SMT.SaaS.BLLCommonServices.TKServicesWS.ResultObject Execute(SMT.SaaS.BLLCommonServices.TKServicesWS.BaseObject baseObject);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITKServices/UpdateCheckState", ReplyAction="http://tempuri.org/ITKServices/UpdateCheckStateResponse")]
        int UpdateCheckState(string EntityType, string EntityKey, string EntityId, int CheckState, string strXmlParams);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ITKServicesChannel : SMT.SaaS.BLLCommonServices.TKServicesWS.ITKServices, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class TKServicesClient : System.ServiceModel.ClientBase<SMT.SaaS.BLLCommonServices.TKServicesWS.ITKServices>, SMT.SaaS.BLLCommonServices.TKServicesWS.ITKServices {
        
        public TKServicesClient() {
        }
        
        public TKServicesClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public TKServicesClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TKServicesClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TKServicesClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public SMT.SaaS.BLLCommonServices.TKServicesWS.ResultObject Execute(SMT.SaaS.BLLCommonServices.TKServicesWS.BaseObject baseObject) {
            return base.Channel.Execute(baseObject);
        }
        
        public int UpdateCheckState(string EntityType, string EntityKey, string EntityId, int CheckState, string strXmlParams) {
            return base.Channel.UpdateCheckState(EntityType, EntityKey, EntityId, CheckState, strXmlParams);
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="TKServicesWS.IApplicationService")]
    public interface IApplicationService {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IApplicationService/CallApplicationService")]
        void CallApplicationService(string strXml);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IApplicationService/CallWaitAppService", ReplyAction="http://tempuri.org/IApplicationService/CallWaitAppServiceResponse")]
        string CallWaitAppService(string strXml);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IApplicationServiceChannel : SMT.SaaS.BLLCommonServices.TKServicesWS.IApplicationService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ApplicationServiceClient : System.ServiceModel.ClientBase<SMT.SaaS.BLLCommonServices.TKServicesWS.IApplicationService>, SMT.SaaS.BLLCommonServices.TKServicesWS.IApplicationService {
        
        public ApplicationServiceClient() {
        }
        
        public ApplicationServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ApplicationServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ApplicationServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ApplicationServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void CallApplicationService(string strXml) {
            base.Channel.CallApplicationService(strXml);
        }
        
        public string CallWaitAppService(string strXml) {
            return base.Channel.CallWaitAppService(strXml);
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="TKServicesWS.IEventTriggerProcess")]
    public interface IEventTriggerProcess {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IEventTriggerProcess/EventTriggerProcess")]
        void EventTriggerProcess(string param);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IEventTriggerProcessChannel : SMT.SaaS.BLLCommonServices.TKServicesWS.IEventTriggerProcess, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class EventTriggerProcessClient : System.ServiceModel.ClientBase<SMT.SaaS.BLLCommonServices.TKServicesWS.IEventTriggerProcess>, SMT.SaaS.BLLCommonServices.TKServicesWS.IEventTriggerProcess {
        
        public EventTriggerProcessClient() {
        }
        
        public EventTriggerProcessClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public EventTriggerProcessClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public EventTriggerProcessClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public EventTriggerProcessClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void EventTriggerProcess(string param) {
            base.Channel.EventTriggerProcess(param);
        }
    }
}
