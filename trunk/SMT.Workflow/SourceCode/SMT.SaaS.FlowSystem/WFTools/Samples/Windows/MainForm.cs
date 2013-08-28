using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime.Tracking;
using WFTools.Samples.WorkFlow;
using WFTools.Services.Tracking;
using WFTools.Services.Tracking.Ado;
using WFTools.Utilities.Diagnostics;

namespace WFTools.Samples.Windows
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private delegate void TraceHandler();

        private void MainForm_Load(Object sender, EventArgs e)
        {
            // bind connection Strings to the dropdown lists
            cboPersistenceService.DisplayMember = "Name";
            cboTrackingService.DisplayMember = "Name";
            foreach (ConnectionStringSettings connectionStringSetting in ConfigurationManager.ConnectionStrings)
            {
                cboPersistenceService.Items.Add(connectionStringSetting);
                cboTrackingService.Items.Add(connectionStringSetting);
            }

            cboTrackingService.Items.Insert(0, "None");

            if (cboPersistenceService.Items.Count > 0)
                cboPersistenceService.SelectedIndex = 0;

            if (cboTrackingService.Items.Count > 0)
                cboTrackingService.SelectedIndex = 0;

            // configure delegate trace listener
            DelegateTraceListener traceListener = new DelegateTraceListener();
            traceListener.WriteDelegate += traceListener_WriteDelegate;
            traceListener.WriteLineDelegate += traceListener_WriteDelegate;
            Trace.Listeners.Add(traceListener);

            // and the timer to flush messages to the message box
            traceDelegate = traceToMessageBox;
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(Object sender, EventArgs e)
        {
            Invoke(traceDelegate);
        }

        private TraceHandler traceDelegate;
        private readonly Object traceMessagesLock = new Object();
        private readonly Stack<String> traceMessages = new Stack<String>();
        private Timer timer;

        private void traceListener_WriteDelegate(String message)
        {
            lock (traceMessagesLock)
            {
                traceMessages.Push(message);
            }
        }

        private void traceToMessageBox()
        {
            lock (traceMessagesLock)
            {
                StringBuilder stringBuilder = new StringBuilder();
                while (traceMessages.Count > 0)
                    stringBuilder.AppendFormat("{0}\r\n", traceMessages.Pop());

                stringBuilder.Append(txtTraceOutput.Text);
                // truncate text after 10000 characters, keeps performance good
                if (stringBuilder.Length > 10000)
                    stringBuilder.Remove(10000, stringBuilder.Length - 10000);

                txtTraceOutput.Text = stringBuilder.ToString();
            }
        }

        private readonly Dictionary<String, SampleWorkFlowRuntime> loadedWorkflowRuntimes = new Dictionary<String, SampleWorkFlowRuntime>();

        private void btnCreateSequentialWorkflow_Click(Object sender, EventArgs e)
        {
            ConnectionStringSettings persistenceConnectionString = cboPersistenceService.SelectedItem as ConnectionStringSettings;
            ConnectionStringSettings trackingConnectionString = cboTrackingService.SelectedItem as ConnectionStringSettings;
            if (persistenceConnectionString == null)
            {
                MessageBox.Show("No connection string selected for persistence service.", "WFTools Samples",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            String workflowRuntimeKey = String.Format("{0}_{1}_{2}",
                persistenceConnectionString.Name, 
                trackingConnectionString == null ? "None" : trackingConnectionString.Name,
                chkUseLocalTransactions.Checked);

            SampleWorkFlowRuntime workflowRuntime;
            if (!this.loadedWorkflowRuntimes.TryGetValue(workflowRuntimeKey, out workflowRuntime))
            {
                workflowRuntime = new SampleWorkFlowRuntime(persistenceConnectionString,
                    trackingConnectionString, chkUseLocalTransactions.Checked);

                workflowRuntime.WorkflowTerminated += workflowRuntime_WorkflowTerminated;
                workflowRuntime.ServicesExceptionNotHandled += workflowRuntime_ServicesExceptionNotHandled;

                this.loadedWorkflowRuntimes.Add(workflowRuntimeKey, workflowRuntime);
            }

            // create a new sequential workflow
            WorkflowInstance workflowInstance = workflowRuntime.CreateSequentialWorkflow();

            if (chkModifyWorkflow.Checked)
            {
                Activity rootActivity = workflowInstance.GetWorkflowDefinition();
                WorkflowChanges workflowChanges = new WorkflowChanges(rootActivity);

                // modify the workflow
                Activity activityToRemove = workflowChanges.TransientWorkflow.GetActivityByName("codeActivity3");
                CompositeActivity parentActivity = activityToRemove.Parent;

                parentActivity.Activities.Remove(activityToRemove);

                CodeActivity codeActivity = new CodeActivity("TestChangeActivity");
                codeActivity.ExecuteCode +=
                    delegate { Trace.WriteLine("Test Change Activity executed..."); };

                parentActivity.Activities.Add(codeActivity);

                workflowInstance.ApplyWorkflowChanges(workflowChanges);
            }

            workflowInstance.Start();

            ManualWorkflowSchedulerService schedulerService = workflowRuntime.GetService<ManualWorkflowSchedulerService>();
            schedulerService.RunWorkflow(workflowInstance.InstanceId);
        }

        private void workflowRuntime_WorkflowTerminated(Object sender, WorkflowTerminatedEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show(String.Format("Workflow {0}:\r\n\r\n{1}",
                    e.WorkflowInstance.InstanceId, e.Exception));
            }
        }

        private void workflowRuntime_ServicesExceptionNotHandled(Object sender, ServicesExceptionNotHandledEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show(String.Format("Workflow {0}:\r\n\r\n{1}",
                    e.WorkflowInstanceId, e.Exception));
            }
        }

        private void cmdUpdateTrackingProfile_Click(Object sender, EventArgs e)
        {
            ConnectionStringSettings trackingConnectionString = cboTrackingService.SelectedItem as ConnectionStringSettings;

            // update the default tracking profile
            GenericTrackingProfileManager profileManager = new AdoTrackingProfileManager(trackingConnectionString);
            TrackingProfile trackingProfile = profileManager.GetDefaultTrackingProfile();
            foreach (ActivityTrackPoint trackPoint in trackingProfile.ActivityTrackPoints)
            {
                trackPoint.Annotations.Add("Annotation added on " + DateTime.UtcNow);
                TrackingExtract trackingExtract = new ActivityDataTrackingExtract("Name");
                trackingExtract.Annotations.Add("Tracking Data Item added on " + DateTime.UtcNow);
                trackPoint.Extracts.Add(trackingExtract);
            }

            trackingProfile.Version = new Version(
                trackingProfile.Version.Major,
                trackingProfile.Version.Minor + 1, 0, 0);

            if (profileManager.GetTrackingProfile(typeof(SequentialWorkFlow)) != null)
                profileManager.UpdateTrackingProfile(typeof(SequentialWorkFlow), trackingProfile);

            profileManager.UpdateDefaultTrackingProfile(trackingProfile);
        }
    }
}