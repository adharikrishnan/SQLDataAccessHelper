// "<copyright file="SqlCredentials.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SQLDataAccess.Common.Models
{
    /// <summary>
    /// The SQL Credentials model to pass essential Credentials to connect to a database.
    /// </summary>
    public class SqlCredentials
    {
        /// <summary>
        /// The Default, General Purpose Connection String.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The Optional Read Only Connection String that can be
        /// specified for readonly connections.
        /// </summary>
        public string? ReadOnlyConnectionString { get; set; }
    }
}
