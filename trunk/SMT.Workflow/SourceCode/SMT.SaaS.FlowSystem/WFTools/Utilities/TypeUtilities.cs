using System;
using System.Collections;

namespace WFTools.Utilities
{
    public static class TypeUtilities
    {
        private static Hashtable cachedTypes = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Retrieve a type using the specified name, utilising the cache
        /// where possible.
        /// </summary>
        public static Type GetType(string typeName)
        {
            return GetType(typeName, true);
        }

        /// <summary>
        /// Retrieve a type using the specified type and assembly name, utilising the cache
        /// where possible.
        /// </summary>
        public static Type GetType(string typeName, string assemblyName)
        {
            return GetType(typeName, assemblyName, true);
        }

        /// <summary>
        /// Retrieve a type using the specified name, utilising the cache
        /// if useCache is true.
        /// </summary>
        public static Type GetType(string typeName, bool useCache)
        {
            Type type;

            if (!useCache || !cachedTypes.ContainsKey(typeName))
            {
                type = Type.GetType(typeName);
                if (type == null)
                {
                    throw new TypeLoadException(
                        string.Format("Unable to resolve type {0}.", typeName));
                }

                if (useCache)
                    cachedTypes.Add(typeName, type);
            }
            else
                type = (Type) cachedTypes[typeName];

            return type;
        }

        /// <summary>
        /// Retrieve a type using the specified type and assembly name, utilising the cache
        /// if useCache is true.
        /// </summary>
        public static Type GetType(string typeName, string assemblyName, bool useCache)
        {
            return GetType(string.Format("{0}, {1}", typeName, assemblyName), useCache);
        }

        /// <summary>
        /// Given a type name, create a new instance of the type, casting
        /// it to the specified generic type parameter.
        /// </summary>
        /// <typeparam name="T">
        /// Type to cast the instance to.
        /// </typeparam>
        /// <param name="typeName">
        /// Name of the type to create an instance of.
        /// </param>
        /// <returns>
        /// Instance of the specified type, cast to the specified generic parameter.
        /// </returns>
        public static T CreateInstance<T>(string typeName)
        {
            Type type = GetType(typeName);
            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new InvalidCastException(
                    string.Format("Unable to cast {0} to {1}.", typeName,
                        typeof(T).FullName));
            }

            return (T)Activator.CreateInstance(type);
        }

        /// <summary>
        /// Given a type name, create a new instance of the type, casting
        /// it to the specified generic type parameter.
        /// </summary>
        /// <typeparam name="T">
        /// Type to cast the instance to.
        /// </typeparam>
        /// <param name="typeName">
        /// Name of the type to create an instance of.
        /// </param>
        /// <param name="parameters">
        /// Parameters to pass to the constructor.
        /// </param>
        /// <returns>
        /// Instance of the specified type, cast to the specified generic parameter.
        /// </returns>
        public static T CreateInstance<T>(string typeName, params object [] parameters)
        {
            Type type = GetType(typeName);
            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new InvalidCastException(
                    string.Format("Unable to cast {0} to {1}.", typeName,
                        typeof (T).FullName));
            }

            return (T)Activator.CreateInstance(type, parameters);
        }
    }
}
