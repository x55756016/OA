using System.Collections.Generic;
using System.Workflow.Runtime.Tracking;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Implementation of <see cref="ActivityTrackingRecord" /> that provides 
    /// a serialisable representation of the event args and body properties.
    /// </summary>
    public class SerialisableActivityTrackingRecord : ActivityTrackingRecord
    {
        private readonly IList<SerialisableTrackingDataItem> body = new List<SerialisableTrackingDataItem>();
        /// <summary>
        /// Serialisable representation of the body of the <see cref="ActivityTrackingRecord" />.
        /// </summary>
        public new IList<SerialisableTrackingDataItem> Body
        {
            get { return body; }
        }

        private SerialisableData eventArgs;
        /// <summary>
        /// Serialisable representation of the event arguments of the <see cref="ActivityTrackingRecord" />.
        /// </summary>
        public new SerialisableData EventArgs
        {
            get { return eventArgs; }
            set { eventArgs = value; }
        }
    }
}
