using System;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace WFTools.Utilities.Patterns
{
    /// <summary>
    /// GenericSingleton provides a generic means of defining a singleton.
    /// </summary>
    public static class GenericSingleton<T> where T : class
    {
        private static int instanceCount;

        private static T instance;
        /// <summary>
        /// Instance of the singleton.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instanceCount == 0)
                {
                    T newInstance = createInstance();
                    object reference = Interlocked.CompareExchange(
                        ref instance, newInstance, null);

                    if (reference == null)
                        Interlocked.Increment(ref instanceCount);
                }

                return instance;
            }
        }

        /// <summary>
        /// Create an instance of the specified type.
        /// </summary>
        private static T createInstance()
        {
            return (T)Activator.CreateInstance(typeof(T),
                BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance, null, new object[0],
                CultureInfo.InvariantCulture);
        }
    }
}
