using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

namespace SMT.SaaS.Permission.UI.UserControls
{
    /// <summary>
    /// Client Storage class for storing objects in IsolatedStorage
    /// </summary>
    public class ClientStorage
    {
        #region Constants

        const string ISOLATED_KEY_FILE_NAME = "KeyNames.txt";
        const string KEY_OBJECT_FILE = "object.xml";

        #endregion

        #region Private Static Fields

        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        Dictionary<string, TypeAndValue> keysNTypes;

        #endregion

        #region CTOR
        /// <summary>
        /// private CTOR to initialize the class to make it singleton
        /// </summary>
        private ClientStorage()
        {
            keysNTypes = new Dictionary<string, TypeAndValue>();
            if (FileExists(ISOLATED_KEY_FILE_NAME))
            {
                ReadKeys(isoStore);
            }
        }

        /// <summary>
        /// Nested class for lazy initialization.
        /// </summary>
        class NestedClientStorage
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static NestedClientStorage()
            {
            }
            internal static readonly ClientStorage Instance = new ClientStorage();
        }
        #endregion

        #region Private Helper Methods

        private void ReadKeys(IsolatedStorageFile isoStore)
        {
            IsolatedStorageFileStream iStream = new IsolatedStorageFileStream(ISOLATED_KEY_FILE_NAME,
                                                                    FileMode.Open, isoStore);
            DataContractSerializer serializer = new DataContractSerializer(keysNTypes.GetType());
            keysNTypes = serializer.ReadObject(iStream) as Dictionary<string, TypeAndValue>;
            iStream.Close();
        }

        private void AddKey(string key, object value)
        {
            if (!keysNTypes.ContainsKey(key))
                keysNTypes.Add(key, new TypeAndValue());
            keysNTypes[key].TypeofObject = value.GetType();
            keysNTypes[key].StoredObject = value;
            WriteKeyFile();
        }

        private void WriteKeyFile()
        {
            using (IsolatedStorageFileStream oStream = new IsolatedStorageFileStream(ISOLATED_KEY_FILE_NAME,
                                                        FileMode.Create, isoStore))
            {
                DataContractSerializer serializer = new DataContractSerializer(keysNTypes.GetType());

                serializer.WriteObject(oStream, keysNTypes);
                oStream.Close();
            }
        }

        private object Retreive(string key)
        {
            object value = null;

            if (FileExists(key + KEY_OBJECT_FILE) && keysNTypes.ContainsKey(key))
            {
                if (keysNTypes[key].StoredObject == null)
                {
                    try
                    {
                        using (IsolatedStorageFileStream iStream = new IsolatedStorageFileStream(key + KEY_OBJECT_FILE, FileMode.OpenOrCreate, isoStore))
                        {
                            if (iStream != null)
                            {
                                try
                                {
                                    DataContractSerializer serializer = new DataContractSerializer(keysNTypes[key].TypeofObject);
                                    value = serializer.ReadObject(iStream);
                                }
                                catch (Exception)
                                {
                                    // Do nothing simply retrun null
                                }
                                keysNTypes[key].StoredObject = value;
                                iStream.Close();
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        throw new KeyNotFoundException();
                    }
                }
                else
                {
                    value = keysNTypes[key].StoredObject;
                }
            }
            return value;
        }

        private void AddOrUpdate(string key, object value)
        {
            try
            {
                IsolatedStorageFileStream oStream = new IsolatedStorageFileStream(key + KEY_OBJECT_FILE,
                                                    FileMode.Create, isoStore);
                DataContractSerializer serializer = new DataContractSerializer(value.GetType());

                serializer.WriteObject(oStream, value);

                oStream.Close();
            }
            catch (IsolatedStorageException)
            {
                if (ApplyStorageSpace(1024 * 1024 * 1024))
                {
                    AddOrUpdate(key, value);
                }
            }
            
        }

        private void Add(string key, object value, bool throwErrorOnDuplicate)
        {
            if (keysNTypes.ContainsKey(key) && throwErrorOnDuplicate)
            {
                //throw new System.Exception("Duplicate key provided.");
            }
            else
            {
                AddKey(key, value);
                AddOrUpdate(key, value);
            }
        }

        private bool FileExists(string fileName)
        {
            return isoStore.FileExists(fileName);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Public static property to get the instance of ClientStorage which is a singleton class
        /// </summary>
        public static ClientStorage Instance
        {
            get
            {
                return NestedClientStorage.Instance;
            }
        }

        /// <summary> 
        /// Adds a key/value to the storage device. 
        /// </summary> 
        /// <param name="key">Key to identify the object</param> 
        /// <param name="versionNumber">Version Number</param>
        /// <param name="value">Value as object</param> 
        public void Add(string key, object value)
        {
            Add(key, value, true);
        }

        /// <summary>
        /// Remove a element from the Isolated Storage
        /// </summary>
        /// <param name="key">key</param>
        public void Remove(string key)
        {
            keysNTypes.Remove(key);
            WriteKeyFile();
            if (FileExists(key + KEY_OBJECT_FILE))
            {
                isoStore.DeleteFile(key + KEY_OBJECT_FILE);
            }

        }

        /// <summary>
        /// Indexer for CLientStorage
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="versionNumber"> Version Number</param>
        /// <returns>returns the object on the basis of key</returns>
        public object this[string key]
        {
            get
            {
                return Retreive(key);
            }
            set
            {
                Add(key, value, false);
            }
        }

        /// <summary>
        /// 在客户端申请存储空间
        /// </summary>
        /// <param name="size">空间大小（KB）</param>
        /// <returns></returns>
        public static bool ApplyStorageSpace(double size)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    Int64 newQuotaSize = Convert.ToInt64(size * 1024); //换算为字节
                    Int64 curAvail = store.AvailableFreeSpace;

                    if (curAvail < newQuotaSize)
                    {
                        if (!store.IncreaseQuotaTo(newQuotaSize))
                            return false;
                        else
                            return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (IsolatedStorageException ex)
            {
                throw ex;
            }
        }
        #endregion

        #region TypeAndValue

        [DataContract]
        public class TypeAndValue
        {
            public TypeAndValue()
            {
            }

            public Type TypeofObject { get; set; }
            public object StoredObject { get; set; }

            private string fullyQualifiedTypeName;

            [DataMember]
            public string FullyQualifiedTypeName
            {
                get
                {
                    if (fullyQualifiedTypeName == null)
                    {
                        fullyQualifiedTypeName = TypeofObject.AssemblyQualifiedName;
                    }
                    return fullyQualifiedTypeName;
                }
                set
                {
                    fullyQualifiedTypeName = value;
                    TypeofObject = Type.GetType(fullyQualifiedTypeName);
                }
            }
        }
        #endregion
    }
}