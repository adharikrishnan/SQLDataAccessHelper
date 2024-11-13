namespace SQLDataAccess.SQLServer.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Class for Custom SQL Exception.
    /// </summary>
    [Serializable]
    public class SqlServerDataAccessException : Exception
    {
    }
}
