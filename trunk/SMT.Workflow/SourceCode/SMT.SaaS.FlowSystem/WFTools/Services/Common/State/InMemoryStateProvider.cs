using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WFTools.Services.Common.State
{
    /// <summary>
    /// Implementation of <see cref="IStateProvider" /> that uses an in-memory
    /// dictionary indexed by key.
    /// </summary>
    public class InMemoryStateProvider : IStateProvider
    {
        private readonly Dictionary<string, object> stateStorage = new Dictionary<string, object>();

        /// <summary>
        /// Retrieve an item from storage.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of an item in storage.
        /// </param>
        /// <returns>
        /// An <see cref="object" /> representing the item in storage, 
        /// or <c>null</c> if the item was not found.
        /// </returns>
        public object Get(string key)
        {
            if (stateStorage.ContainsKey(key))
                return stateStorage[key];
            else
                return null;
        }

        /// <summary>
        /// Retrieve an item from storage, casting it to the specified type.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of an item in storage.
        /// </param>
        /// <typeparam name="T">
        /// Type to cast the returned item to.
        /// </typeparam>
        /// <returns>
        /// The item from storage, or <c>null</c> if the item was not found.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        public T Get<T>(string key)
        {
            if (stateStorage.ContainsKey(key))
                return (T) stateStorage[key];
            else
                return default(T);
        }

        /// <summary>
        /// Add an item to storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item.
        /// </param>
        /// <param name="value">
        /// The item to add.
        /// </param>
        public void Add(string key, object value)
        {
            if (stateStorage.ContainsKey(key))
                throw new ArgumentException(RM.Get_Error_KeyAlreadyExists(key));

            stateStorage.Add(key, value);
        }

        /// <summary>
        /// Add an item to storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item.
        /// </param>
        /// <param name="value">
        /// The item to add.
        /// </param>
        public void Add<T>(string key, T value)
        {
            if (stateStorage.ContainsKey(key))
                throw new ArgumentException(RM.Get_Error_KeyAlreadyExists(key));

            stateStorage.Add(key, value);
        }

        /// <summary>
        /// Remove an item to storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item.
        /// </param>
        public void Remove(string key)
        {
            if (!stateStorage.ContainsKey(key))
                throw new KeyNotFoundException(RM.Get_Error_KeyNotFound(key));

            stateStorage.Remove(key);
        }

        /// <summary>
        /// Update an item in storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item to update.
        /// </param>
        /// <param name="value">
        /// The item to update.
        /// </param>
        public void Update(string key, object value)
        {
            if (!stateStorage.ContainsKey(key))
                throw new KeyNotFoundException(RM.Get_Error_KeyNotFound(key));
            
            stateStorage[key] = value;
        }

        /// <summary>
        /// Update an item in storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item to update.
        /// </param>
        /// <param name="value">
        /// The item to update.
        /// </param>
        public void Update<T>(string key, T value)
        {
            if (!stateStorage.ContainsKey(key))
                throw new KeyNotFoundException(RM.Get_Error_KeyNotFound(key));

            stateStorage[key] = value;
        }

        /// <summary>
        /// Indicates whether an item exists in storage.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item.
        /// </param>
        public bool Contains(string key)
        {
            return stateStorage.ContainsKey(key);
        }
    }
}