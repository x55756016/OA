using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.ComponentModel.Compiler;
using System.Configuration;
using SMT.WFLib;
using WFTools.Services.Persistence.Ado;
using WFTools.Services.Tracking.Ado;
using WFTools.Services.Batching.Ado;
using System.Xml;
using System.IO;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel.Serialization;
using SMT.FlowWFService.PublicClass;
using SMT.Workflow.Common.DataAccess;
using System.Workflow.Runtime.Hosting;

namespace SMT.FlowWFService
{
    /// <summary>
    /// 流程运行操作类
    /// </summary>
    class SMTWorkFlowManage
    {

        /// <summary>
        /// 创建工作流运行时
        /// </summary>
        /// <param name="IsPer">是否使用持久化</param>
        /// <returns></returns>
        public static WorkflowRuntime CreateWorkFlowRuntime(bool IsPer)
        {
            try
            {
                WorkflowRuntime WfRuntime = new WorkflowRuntime();


                if (IsPer)
                {
                    String connStringPersistence = ConfigurationManager.ConnectionStrings["SqlPersistenceConnection"].ConnectionString;//Data Source=172.30.50.110;Initial Catalog=WorkflowPersistence;Persist Security Info=True;User ID=sa;Password=fbaz2012;MultipleActiveResultSets=True";
                    SqlWorkflowPersistenceService persistence = new SqlWorkflowPersistenceService(connStringPersistence, true, new TimeSpan(0, 0, 30), new TimeSpan(0, 1, 0));
                    WfRuntime.AddService(persistence);
                    WfRuntime.WorkflowPersisted += new EventHandler<WorkflowEventArgs>(WfRuntime_WorkflowPersisted);
                    WfRuntime.ServicesExceptionNotHandled += new EventHandler<ServicesExceptionNotHandledEventArgs>(WfRuntime_ServicesExceptionNotHandled);
                //    ConnectionStringSettings defaultConnectionString = ConfigurationManager.ConnectionStrings["OracleConnection"];
                //    WfRuntime.AddService(new AdoPersistenceService(defaultConnectionString, true, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)));
                //    WfRuntime.AddService(new AdoTrackingService(defaultConnectionString));
                //    WfRuntime.AddService(new AdoWorkBatchService());
                }

                FlowEvent ExternalEvent = new FlowEvent();
                ExternalDataExchangeService objService = new ExternalDataExchangeService();
                WfRuntime.AddService(objService);
                objService.AddService(ExternalEvent);
                TypeProvider typeProvider = new TypeProvider(null);
                WfRuntime.AddService(typeProvider);
                WfRuntime.StartRuntime();
                return WfRuntime;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("CreateWorkFlowRuntime异常信息 ：" + ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        static void WfRuntime_ServicesExceptionNotHandled(object sender, ServicesExceptionNotHandledEventArgs e)
        {
            LogHelper.WriteLog("WfRuntime_ServicesExceptionNotHandled异常信息 ：" + e.Exception.ToString ());
        }

        static void WfRuntime_WorkflowPersisted(object sender, WorkflowEventArgs e)
        {
            LogHelper.WriteLog("WfRuntime_WorkflowPersisted Persisted completed信息 ：" + e.WorkflowInstance.InstanceId.ToString());
        }

        /// <summary>
        /// 释放运行时
        /// </summary>
        /// <param name="WfRuntime"></param>
        public static void ColseWorkFlowRuntime(WorkflowRuntime WfRuntime)
        {
            //try
            //{
            //    if (WfRuntime != null && WfRuntime.IsStarted)
            //    {
            //        WfRuntime.Dispose();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.WriteLog("ColseWorkFlowRuntime异常信息 ：" + ex.ToString());
            //}
        }

        /// <summary>
        /// 根据模型文件创建工作流实例
        /// </summary>
        /// <param name="WfRuntime">运行时</param>
        /// <param name="Xoml">模型文件</param>
        /// <param name="Rules">规则文件</param>
        /// <returns></returns>
        public static WorkflowInstance CreateWorkflowInstance(WorkflowRuntime WfRuntime,  string Xoml, string Rules)
        {
            try
            {
                WorkflowInstance instance;
                XmlReader readerxoml, readerule;
                StringReader strXoml = new StringReader(Xoml);
                StringReader strRules = new StringReader(Rules == null ? "" : Rules);

                readerxoml = XmlReader.Create(strXoml);
                readerule = XmlReader.Create(strRules);

                //  WorkflowRuntime workflowRuntime = SMTWorkFlowManage.StarWorkFlowRuntime(true);
                if (Rules == null)
                    instance = WfRuntime.CreateWorkflow(readerxoml);
                else
                    instance = WfRuntime.CreateWorkflow(readerxoml, readerule, null);

                instance.Start();
                return instance;
            }
    
             catch (WorkflowValidationFailedException exp)
            {

                StringBuilder errors = new StringBuilder();

                foreach (ValidationError error in exp.Errors)
                {

                    errors.AppendLine(error.ToString());

                }

                throw new Exception(errors.ToString());

            }
          
           

        }
        /// <summary>
        /// 建立虚拟流程实例
        /// </summary>
        /// <param name="WfRuntime"></param>
        /// <param name="xmlFileName"></param>
        /// <returns></returns>
        public static WorkflowInstance CreateWorkflowInstance(WorkflowRuntime WfRuntime,string xmlFileName)
        {
            try
            {
                WorkflowInstance instance;
                string Xml = AppDomain.CurrentDomain.BaseDirectory +"\\"+ xmlFileName;
                XmlReader reader = XmlReader.Create(Xml);
                instance = WfRuntime.CreateWorkflow(reader);
                instance.Start();
                return instance;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("CreateWorkflowInstance异常信息 ：" + ex.ToString());
                throw new Exception(ex.Message);
            }

        }
        /// <summary>
        /// 从持久化库在恢复实例
        /// </summary>
        /// <param name="WfRuntime"></param>
        /// <param name="INSTANCEID"></param>
        /// <returns></returns>
        public static WorkflowInstance GetWorkflowInstance(WorkflowRuntime WfRuntime, string INSTANCEID)
        {
            try
            {
                WfRuntime = new WorkflowRuntime();

                if (WfRuntime.GetService(typeof(AdoPersistenceService)) == null)
                {
                    ConnectionStringSettings defaultConnectionString = ConfigurationManager.ConnectionStrings["OracleConnection"];
                    WfRuntime.AddService(new AdoPersistenceService(defaultConnectionString, true, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)));
                    WfRuntime.AddService(new AdoTrackingService(defaultConnectionString));
                    WfRuntime.AddService(new AdoWorkBatchService());

                }

                FlowEvent ExternalEvent = new FlowEvent();
                ExternalDataExchangeService objService = new ExternalDataExchangeService();
                WfRuntime.AddService(objService);
                objService.AddService(ExternalEvent);
                TypeProvider typeProvider = new TypeProvider(null);
                WfRuntime.AddService(typeProvider);
                WfRuntime.StartRuntime();

                WorkflowInstance instance = null;
                try
                {
                    instance = WfRuntime.GetWorkflow(new Guid(INSTANCEID));
                }
                catch
                {
                    instance = null;
                }
                if (instance == null) //try find instance in sql server persistence
                {
                    WfRuntime = new WorkflowRuntime();
                    ExternalEvent = new FlowEvent();
                    objService = new ExternalDataExchangeService();
                    WfRuntime.AddService(objService);
                    objService.AddService(ExternalEvent);
                    typeProvider = new TypeProvider(null);
                    WfRuntime.AddService(typeProvider);

                    String connStringPersistence =ConfigurationManager.ConnectionStrings["SqlPersistenceConnection"].ConnectionString ;//Data Source=172.30.50.110;Initial Catalog=WorkflowPersistence;Persist Security Info=True;User ID=sa;Password=fbaz2012;MultipleActiveResultSets=True";
                    SqlWorkflowPersistenceService persistence = new SqlWorkflowPersistenceService(connStringPersistence, true, new TimeSpan(0, 2, 0), new TimeSpan(0, 0, 5));
                    WfRuntime.AddService(persistence);
                    WfRuntime.StartRuntime();
                    instance = WfRuntime.GetWorkflow(new Guid(INSTANCEID));
                }
                //WfRuntime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e)
                //{
                //    instance = null;

                //};
                return instance;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("GetWorkflowInstance异常信息 ：" + ex.ToString());
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 克隆一个实例
        /// </summary>
        /// <param name="WfRuntimeClone"></param>
        /// <param name="instanceClone"></param>
        /// <param name="WfRuntime"></param>
        /// <returns></returns>
        public static WorkflowInstance CloneWorkflowInstance(WorkflowRuntime WfRuntimeClone,WorkflowInstance instanceClone, WorkflowRuntime WfRuntime)
        {
            try
            {
                if (!WfRuntimeClone.IsStarted)
                {
                    WfRuntimeClone.StartRuntime();
                }
                StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(WfRuntimeClone, instanceClone.InstanceId);

                System.Workflow.Activities.StateMachineWorkflowActivity smworkflow = new StateMachineWorkflowActivity();
                smworkflow = workflowinstance.StateMachineWorkflow;
                RuleDefinitions ruleDefinitions = smworkflow.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
                WorkflowMarkupSerializer markupSerializer = new WorkflowMarkupSerializer();

                StringBuilder xoml = new StringBuilder();
                StringBuilder rule = new StringBuilder();
                XmlWriter xmlWriter = XmlWriter.Create(xoml);
                XmlWriter ruleWriter = XmlWriter.Create(rule);
                markupSerializer.Serialize(xmlWriter, smworkflow);

                if (ruleDefinitions != null)
                    markupSerializer.Serialize(ruleWriter, ruleDefinitions);

                xmlWriter.Close();
                ruleWriter.Close();

                StringReader readxoml = new StringReader(xoml.ToString());
                XmlReader readerxoml = XmlReader.Create(readxoml);
                WorkflowInstance instance;
                if (ruleDefinitions == null)
                    instance = WfRuntime.CreateWorkflow(readerxoml);
                else
                {
                    StringReader readrule = new StringReader(rule.ToString());
                    XmlReader readerrule = XmlReader.Create(readrule);
                    instance = WfRuntime.CreateWorkflow(readerxoml, readerrule, null);
                }

                instance.Start();
                return instance;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("CloneWorkflowInstance异常信息 ：" + ex.ToString());
                throw new Exception(ex.Message);
            }

        }
       /// <summary>
       /// 获取当前实例的状态代码
       /// </summary>
       /// <param name="WfRuntime"></param>
       /// <param name="instance"></param>
       /// <param name="CurrentStateName"></param>
       /// <returns></returns>
        public static string GetNextState(WorkflowRuntime WfRuntime, WorkflowInstance instance, string CurrentStateName)
        {
            try
            {
                string StateName = CurrentStateName;
                LogHelper.WriteLog("循环获取当前实例的状态代码开始。。。。。。");
                while (StateName == CurrentStateName)
                {
                    if (instance == null)
                    {
                        StateName = "EndFlow";
                        return StateName;
                    }
                    StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(WfRuntime, instance.InstanceId);
                    StateName = workflowinstance.CurrentStateName;
                }               
                LogHelper.WriteLog("循环获取当前实例的状态代码结束");
                return StateName;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("GetNextState异常信息 ：" + ex.ToString());
                throw new Exception(ex.Message);
            }
        }
     /// <summary>
     /// 激发事件到一下状态，并获取状态代码
     /// </summary>
     /// <param name="WfRuntime"></param>
     /// <param name="instance"></param>
     /// <param name="CurrentStateName"></param>
     /// <param name="xml"></param>
     /// <returns></returns>
        public static string GetNextStateByEvent(WorkflowRuntime WfRuntime, WorkflowInstance instance, string CurrentStateName, string xml)
        {
            try
            {
                if (!WfRuntime.IsStarted)
                {
                    WfRuntime.StartRuntime();
                }
                WfRuntime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e)
                {
                    instance = null;

                };
                StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(WfRuntime, instance.InstanceId);
                
                workflowinstance.SetState(CurrentStateName);

                FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
                FlowData.xml = xml;

                WfRuntime.GetService<FlowEvent>().OnDoFlow(instance.InstanceId, FlowData);//激发流程引擎流转到下一状态
                //while (true)
                //{
                //    string stateName = workflowinstance.CurrentStateName;

                //    if (stateName != null && stateName.ToUpper().IndexOf("START") == -1)
                //    {
                //        break;
                //    }
                //}

                System.Threading.Thread.Sleep(1000);
               // StateMachineWorkflowInstance workflowinstance1 = new StateMachineWorkflowInstance(WfRuntime, instance.InstanceId);
                
                return GetNextState(WfRuntime, instance, CurrentStateName);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("GetNextStateByEvent异常信息 ：" + ex.ToString());
                throw new Exception(ex.Message);
            }
        }
    }
}
