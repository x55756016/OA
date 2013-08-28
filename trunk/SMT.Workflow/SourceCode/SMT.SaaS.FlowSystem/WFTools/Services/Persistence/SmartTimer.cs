using System;
using System.Threading;

namespace WFTools.Services.Persistence
{
    /// <summary>
    /// Wraps a <see cref="Timer" /> by automatically updating its timeout based
    /// upon information derived during the callback.
    /// </summary>
    public sealed class SmartTimer : IDisposable
    {
        /// <summary>
        /// Construct a new instance of the SmartTimer using the specified
        /// callback, state, due time and interval.
        /// </summary>
        /// <param name="callback">
        /// A <see cref="TimerCallback" /> delegate representing a method to be executed. 
        /// </param>
        /// <param name="state">
        /// An object containing information to be used by the callback method, 
        /// or a null reference.
        /// </param>
        /// <param name="dueTime">
        /// The <see cref="TimeSpan" /> representing the amount of time to delay 
        /// before the callback parameter invokes its methods. 
        /// Specify negative one (-1) milliseconds to prevent the timer from starting. 
        /// Specify zero (0) to start the timer immediately. 
        /// </param>
        /// <param name="period">
        /// The time interval between invocations of the methods referenced by 
        /// callback. Specify negative one (-1) milliseconds to disable 
        /// periodic signaling. 
        /// </param>
        public SmartTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            this.timerLock = new object();
            this.period = period;
            this.callback = callback;
            this.next = DateTime.UtcNow + dueTime;
            this.innerTimer = new Timer(handleCallback, state, dueTime, infiniteInterval);
        }

        /// <summary>
        /// <see cref="TimeSpan" /> representing an infinite interval.
        /// </summary>
        private static readonly TimeSpan infiniteInterval = new TimeSpan(-1);

        /// <summary>
        /// <see cref="TimeSpan" /> representing minimum allowable 
        /// time between intervals.
        /// </summary>
        private static readonly TimeSpan minInterval = new TimeSpan(0, 0, 5);

        /// <summary>
        /// Delegate to be executed.
        /// </summary>
        private readonly TimerCallback callback;

        /// <summary>
        /// Lock used when modifying the timer.
        /// </summary>
        private readonly object timerLock;

        /// <summary>
        /// The inner timer wrapped by this class.
        /// </summary>
        private Timer innerTimer;

        /// <summary>
        /// <see cref="DateTime" /> representing the next timeout for 
        /// the timer.
        /// </summary>
        private DateTime next;

        /// <summary>
        /// Indicates the next timeout has changed.
        /// </summary>
        private bool nextChanged;

        /// <summary>
        /// <see cref="TimeSpan" /> representing the interval between
        /// invocations of the callback.
        /// </summary>
        private readonly TimeSpan period;

        /// <summary>
        /// Dispose the underlying timer.
        /// </summary>
        public void Dispose()
        {
            lock (timerLock)
            {
                if (innerTimer != null)
                {
                    innerTimer.Dispose();
                    innerTimer = null;
                }
            }
        }

        /// <summary>
        /// Handle a callback, update the inner timer with the next timeout value.
        /// </summary>
        private void handleCallback(object state)
        {
            try
            {
                callback(state);
            }
            finally
            {
                lock (timerLock)
                {
                    if (innerTimer != null)
                    {
                        if (!nextChanged)
                            next = DateTime.UtcNow + period;
                        else
                            nextChanged = false;

                        TimeSpan newDueTime = next - DateTime.UtcNow;
                        if (newDueTime < TimeSpan.Zero)
                            newDueTime = TimeSpan.Zero;

                        innerTimer.Change(newDueTime, infiniteInterval);
                    }
                }
            }
        }

        /// <summary>
        /// Update the inner timer with the next timeout period, but only
        /// if it is greater than the minimum interval.
        /// </summary>
        /// <param name="newNext"></param>
        public void Update(DateTime newNext)
        {
            if ((newNext < next) && ((next - DateTime.UtcNow) > minInterval))
            {
                lock (timerLock)
                {
                    if (((newNext < next) && ((next - DateTime.UtcNow) > minInterval)) && (innerTimer != null))
                    {
                        next = newNext;
                        nextChanged = true;
                        TimeSpan newDueTime = next - DateTime.UtcNow;
                        if (newDueTime < TimeSpan.Zero)
                            newDueTime = TimeSpan.Zero;

                        innerTimer.Change(newDueTime, infiniteInterval);
                    }
                }
            }
        }
    }
}