using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

/// <summary>
    /// Resource manager.
    /// </summary>
public static class RM
{
    private static readonly ResourceManager resourceManager = new ResourceManager("WFTools.Resources.Strings", Assembly.GetExecutingAssembly());

    public static string Get_Error_OracleParameterExpected()
    {
        return getString("Error_OracleParameterExpected");
    }

    public static string Get_Error_MySqlParameterExpected()
    {
        return getString("Error_MySqlParameterExpected");
    }

    public static string Get_Error_CommandNamesNotSupported()
    {
        return getString("Error_CommandNamesNotSupported");
    }

    public static string Get_Error_CommandNameParameterNameNotSupported()
    {
        return getString("Error_CommandNameParameterNameNotSupported");
    }

    public static string Get_Error_ColumnNotFound(string columnName)
    {
        return getString("Error_ColumnNotFound", columnName);
    }

    public static string Get_Error_CannotConvertDBNullToEnum()
    {
        return getString("Error_CannotConvertDBNullToEnum");
    }

    public static string Get_Error_CannotConvertColumnToEnum()
    {
        return getString("Error_CannotConvertColumnToEnum");
    }

    public static string Get_Error_CannotConvertParameterToEnum()
    {
        return getString("Error_CannotConvertParameterToEnum");
    }

    public static string Get_Error_CannotConvertDBNullToInt16()
    {
        return getString("Error_CannotConvertDBNullToInt16");
    }

    public static string Get_Error_CannotConvertDBNullToInt32()
    {
        return getString("Error_CannotConvertDBNullToInt32");
    }

    public static string Get_Error_CannotConvertDBNullToInt64()
    {
        return getString("Error_CannotConvertDBNullToInt64");
    }

    public static string Get_Error_CannotConvertDBNullToByte()
    {
        return getString("Error_CannotConvertDBNullToByte");
    }

    public static string Get_Error_CannotConvertDBNullToSingle()
    {
        return getString("Error_CannotConvertDBNullToSingle");
    }

    public static string Get_Error_CannotConvertDBNullToDouble()
    {
        return getString("Error_CannotConvertDBNullToDouble");
    }

    public static string Get_Error_CannotConvertDBNullToGuid()
    {
        return getString("Error_CannotConvertDBNullToGuid");
    }

    public static string Get_Error_CannotConvertDBNullToDecimal()
    {
        return getString("Error_CannotConvertDBNullToDecimal");
    }

    public static string Get_Error_CannotConvertDBNullToBoolean()
    {
        return getString("Error_CannotConvertDBNullToBoolean");
    }

    public static string Get_Error_CannotConvertDBNullToDateTime()
    {
        return getString("Error_CannotConvertDBNullToDateTime");
    }

    public static string Get_Error_NotIAdoResourceProvider()
    {
        return getString("Error_NotIAdoResourceProvider");
    }

    public static string Get_Error_TrackingChannelException(string message)
    {
        return getString("Error_TrackingChannelException", message);
    }

    public static string Get_Error_TrackingServiceException(string message)
    {
        return getString("Error_TrackingServiceException", message);
    }

    public static string Get_Error_PersistenceServiceException(string message)
    {
        return getString("Error_PersistenceServiceException", message);
    }

    public static string Get_Error_ScopeCouldNotBeLoaded(Guid scopeId)
    {
        return getString("Error_ScopeCouldNotBeLoaded", scopeId);
    }

    public static string Get_Error_InstanceCouldNotBeLoaded(Guid instanceId)
    {
        return getString("Error_InstanceCouldNotBeLoaded", instanceId);
    }

    public static string Get_Error_InvalidTransactionSpecified()
    {
        return getString("Error_InvalidTransactionSpecified");
    }

    public static string Get_Error_InvalidConnectionStringSpecified()
    {
        return getString("Error_InvalidConnectionStringSpecified");
    }

    public static string Get_Error_ResourceProviderNotInitialised()
    {
        return getString("Error_ResourceProviderNotInitialised");
    }

    public static string Get_Error_KeyAlreadyExists(string key)
    {
        return getString("Error_KeyAlreadyExists", key);
    }

    public static string Get_Error_KeyNotFound(string key)
    {
        return getString("Error_KeyNotFound", key);
    }

    public static string Get_Error_PromotionNotSupported()
    {
        return getString("Error_PromotionNotSupported");
    }

    public static string Get_Error_CannotEnlistInNonTransactionalContext()
    {
        return getString("Error_CannotEnlistInNonTransactionalContext");
    }

    public static string Get_Error_CannotEnlistInDistributedTransaction()
    {
        return getString("Error_CannotEnlistInDistributedTransaction");
    }

    public static string Get_Error_AdapterAlreadyEnlisted()
    {
        return getString("Error_AdapterAlreadyEnlisted");
    }

    private static string getString(string key)
    {
        return resourceManager.GetString(key, CultureInfo.CurrentCulture);
    }

    private static string getString(string key, params object[] parameters)
    {
        return string.Format(resourceManager.GetString(key, CultureInfo.CurrentCulture), parameters);
    }
}