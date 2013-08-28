using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Timers;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime.Tracking;
using WFTools.Services.Tracking.Entity;
using WFTools.Utilities.Diagnostics;

namespace WFTools.Services.Tracking
{
    /// <summary>
    /// Abstract implementation of <see cref="TrackingService" /> that provides the
    /// framework necessary for a very generic tracking service. 
    /// 
    /// A resource provider and accessor work hand-in-hand to actually
    /// read and persist workflow tracking information to the persistence store.
    /// </summary>
    public abstract class GenericTrackingService : TrackingService, IProfileNotification, IDisposable
    {
        /// <summary>
        /// Construct a new <see cref="GenericTrackingService" /> using a default value
        /// for the interval in which to poll for profile changes.
        /// </summary>
        protected GenericTrackingService() : this(defaultProfileChangeInterval) { }
        /// <summary>
        /// Construct a new <see cref="GenericTrackingService" /> specifying the
        /// interval in which to poll for profile changes.
        /// </summary>
        /// <param name="profileChangeInterval">
        /// <see cref="TimeSpan" /> representing the interval to poll for profile
        /// changes.
        /// </param>
        protected GenericTrackingService(TimeSpan profileChangeInterval)
        {
            this.profileChangeInterval = profileChangeInterval;
        }

        /// <summary>
        /// Default timespan of 60 seconds to poll for profile changes.
        /// </summary>
        private static readonly TimeSpan defaultProfileChangeInterval = TimeSpan.FromSeconds(60);

        /// <summary>
        /// The last time that profile changes were checked for.
        /// </summary>
        private DateTime lastProfileChangeCheck = DateTime.UtcNow;
        /// <summary>
        /// Interval in which to poll for profile changes.
        /// </summary>
        private TimeSpan profileChangeInterval;

        /// <summary>
        /// Timer used to poll for profile changes.
        /// </summary>
        private Timer profileChangeTimer;

        private IResourceProvider resourceProvider;
        /// <summary>
        /// The active <see cref="IResourceProvider" /> that provides 
        /// resources necessary for manipulating the underlying tracking store.
        /// </summary>
        protected IResourceProvider ResourceProvider
        {
            get { return resourceProvider; }
        }

        /// <summary>
        /// Create an <see cref="IResourceProvider" /> that provides 
        /// resources necessary for manipulating the underlying tracking store.
        /// </summary>
        /// <remarks>
        /// The resource provider is created once upon Start of the 
        /// tracking service.
        /// </remarks>
        protected abstract IResourceProvider CreateResourceProvider();

        /// <summary>
        /// Create an <see cref="ITrackingServiceResourceAccessor" /> that is responsible
        /// for manipulating the underlying tracking store.
        /// </summary>
        /// <param name="resourceProvider">
        /// The active <see cref="IResourceProvider" />.
        /// </param>
        protected abstract ITrackingServiceResourceAccessor CreateAccessor(IResourceProvider resourceProvider);

        /// <summary>
        /// Perform startup duties associated with this tracking service.
        /// <remarks>
        /// This implementation calls a virtual method to create a single 
        /// resource provider for this tracking service and initialises
        /// a timer to poll for tracking profile changes.
        /// </remarks>
        /// </summary>
        protected override void Start()
        {
            TraceHelper.Trace();

            try
            {
                // retrieve the active resource provider
                this.resourceProvider = CreateResourceProvider();

                // configure the profile change poll timer
                this.profileChangeTimer = new Timer(profileChangeInterval.TotalMilliseconds);
                this.profileChangeTimer.AutoReset = false;
                this.profileChangeTimer.Elapsed += profileChangeTimer_Elapsed;
                this.profileChangeTimer.Start();

                base.Start();
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_TrackingServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                TrackingException trackingException = 
                    new TrackingException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    trackingException, Guid.Empty);

                throw trackingException;
            }
        }

        /// <summary>
        /// Perform shutdown duties associated with this tracking service.
        /// </summary>
        protected override void Stop()
        {
            TraceHelper.Trace();

            try
            {
                if (this.profileChangeTimer != null)
                {
                    this.profileChangeTimer.Dispose();
                    this.profileChangeTimer = null;
                }

                base.Stop();
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_TrackingServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                TrackingException trackingException = 
                    new TrackingException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    trackingException, Guid.Empty);

                throw trackingException;
            }
        }

        /// <summary>
        /// Check for changes to tracking profiles since we last checked.
        /// </summary>
        private void profileChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (base.State == WorkflowRuntimeServiceState.Started)
                {
                    IList<TrackingProfileChange> changes = GetTrackingProfileChanges(ref lastProfileChangeCheck);
                    foreach (TrackingProfileChange change in changes)
                    {
                        if (change.TrackingProfile == null)
                            OnProfileRemoved(new ProfileRemovedEventArgs(change.WorkflowType));
                        else
                            OnProfileUpdated(new ProfileUpdatedEventArgs(change.WorkflowType, change.TrackingProfile));
                    }
                }

                this.profileChangeTimer.Start();
            }
            catch (Exception ex)
            {
                string errorMessage = RM.Get_Error_TrackingServiceException(ex.ToString());

                TraceHelper.Trace(errorMessage);

                base.RaiseServicesExceptionNotHandledEvent(
                    new TrackingException(errorMessage, ex),
                    Guid.Empty);
            }
        }

        /// <summary>
        /// Retrieve a list of all tracking profile changes.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception is raised using base.RaiseServicesExceptionNotHandledEvent where the workflow runtime deals with it.")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference",
            Justification = "Return value is used for changes and the last check is always input/output.")]
        protected virtual IList<TrackingProfileChange> GetTrackingProfileChanges(ref DateTime lastCheck)
        {
            TraceHelper.Trace();

            IList<TrackingProfileChange> changes;
            try
            {
                using (ITrackingServiceResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
                {
                    changes = resourceAccessor.GetTrackingProfileChanges(ref lastCheck);
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_TrackingServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                base.RaiseServicesExceptionNotHandledEvent(
                    new TrackingException(errorMessage, e),
                    Guid.Empty);

                changes = new List<TrackingProfileChange>();
            }

            return changes;
        }

        ///<summary>
        ///Must be overridden in the derived class, and when implemented, retrieves the tracking profile for the specified workflow type if one is available.
        ///</summary>
        ///
        ///<returns>
        ///true if a <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see> for the specified workflow <see cref="T:System.Type"></see> is available; otherwise, false. If true, the <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see> is returned in profile.
        ///</returns>
        ///
        ///<param name="workflowType">The <see cref="T:System.Type"></see> of the workflow for which to get the tracking profile.</param>
        ///<param name="profile">When this method returns, contains the <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see> to load. This parameter is passed un-initialized.</param>
        protected override bool TryGetProfile(Type workflowType, out TrackingProfile profile)
        {
            TraceHelper.Trace();

            try
            {
                using (ITrackingServiceResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
                {
                    return resourceAccessor.TryGetTrackingProfile(workflowType, out profile);
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_TrackingServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                TrackingException trackingException =
                    new TrackingException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    trackingException, Guid.Empty);

                throw trackingException;
            }
        }

        ///<summary>
        ///Must be overridden in the derived class, and when implemented, returns the tracking profile, qualified by version, for the specified workflow <see cref="T:System.Type"></see>. 
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        ///
        ///<param name="workflowType">The <see cref="T:System.Type"></see> of the workflow.</param>
        ///<param name="profileVersionId">The <see cref="T:System.Version"></see> of the tracking profile.</param>
        protected override TrackingProfile GetProfile(Type workflowType, Version profileVersionId)
        {
            TraceHelper.Trace();

            TrackingProfile trackingProfile;
            try
            {
                using (ITrackingServiceResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
                {
                    trackingProfile = resourceAccessor.GetTrackingProfile(workflowType, profileVersionId);
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_TrackingServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                TrackingException trackingException = 
                    new TrackingException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    trackingException, Guid.Empty);

                throw trackingException;
            }

            return trackingProfile;
        }

        ///<summary>
        ///Must be overridden in the derived class, and when implemented, returns the tracking profile for the specified workflow instance.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        ///
        ///<param name="workflowInstanceId">The <see cref="T:System.Guid"></see> of the workflow instance.</param>
        protected override TrackingProfile GetProfile(Guid workflowInstanceId)
        {
            TraceHelper.Trace();

            TrackingProfile trackingProfile;

            try
            {
                using (ITrackingServiceResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
                {
                    trackingProfile = resourceAccessor.GetTrackingProfile(workflowInstanceId);
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_TrackingServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                TrackingException trackingException = 
                    new TrackingException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    trackingException, workflowInstanceId);

                throw trackingException;
            }

            return trackingProfile;
        }

        ///<summary>
        ///Must be overridden in the derived class, and when implemented, retrieves a new tracking profile for the specified workflow instance if the tracking profile has changed since it was last loaded.
        ///</summary>
        ///
        ///<returns>
        ///true if a new <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see> should be loaded; otherwise, false. If true, the <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see> is returned in profile.
        ///</returns>
        ///
        ///<param name="workflowType">The <see cref="T:System.Type"></see> of the workflow instance.</param>
        ///<param name="profile">When this method returns, contains the <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see> to load. This parameter is passed un-initialized.</param>
        ///<param name="workflowInstanceId">The <see cref="T:System.Guid"></see> of the workflow instance.</param>
        protected override bool TryReloadProfile(Type workflowType, Guid workflowInstanceId, out TrackingProfile profile)
        {
            TraceHelper.Trace();

            try
            {
                using (ITrackingServiceResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
                {
                    return resourceAccessor.TryReloadTrackingProfile(workflowType, workflowInstanceId, out profile);
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_TrackingServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                TrackingException trackingException = 
                    new TrackingException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    trackingException, Guid.Empty);

                throw trackingException;
            }
        }

        /// <summary>
        /// Raise a profile updated event.
        /// </summary>
        protected void OnProfileUpdated(ProfileUpdatedEventArgs e)
        {
            TraceHelper.Trace();

            if (profileUpdated != null)
                profileUpdated(this, e);
        }

        private EventHandler<ProfileUpdatedEventArgs> profileUpdated;
        ///<summary>
        ///Occurs when a <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see> for a specific workflow <see cref="T:System.Type"></see> is updated.
        ///</summary>
        ///
        public event EventHandler<ProfileUpdatedEventArgs> ProfileUpdated
        {
            add { profileUpdated += value; }
            remove { profileUpdated -= value; }
        }

        /// <summary>
        /// Raise a profile removed event.
        /// </summary>
        protected void OnProfileRemoved(ProfileRemovedEventArgs e)
        {
            TraceHelper.Trace();

            if (profileRemoved != null)
                profileRemoved(this, e);
        }

        private EventHandler<ProfileRemovedEventArgs> profileRemoved; 
        ///<summary>
        ///Occurs when a <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see> for a specific workflow Type is removed.
        ///</summary>
        ///
        public event EventHandler<ProfileRemovedEventArgs> ProfileRemoved
        {
            add { profileRemoved += value; }
            remove { profileRemoved -= value; }
        }

        ///<summary>
        /// Clean-up the timers used by the persistence service.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///<summary>
        /// Clean-up the timers used by the persistence service.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.profileChangeTimer != null)
                {
                    this.profileChangeTimer.Dispose();
                    this.profileChangeTimer = null;
                }
            }
        }
    }
}