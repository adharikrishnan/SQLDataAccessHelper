// "<copyright file="SqlServerDataAccessException.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

using Npgsql;

namespace SQLDataAccessHelper.PostgreSQL.Exceptions
{
    using System;
    using Microsoft.Data.SqlClient;
    using SQLDataAccessHelper.Common.Exceptions;

    /// <summary>
    /// Exception Class to deal with SQL Server DataAccess Exceptions.
    /// </summary>
    [Serializable]
    public class PostgreSqlDataAccessException : DataAccessException
    {
        /// <summary>
        /// The Sql Parameters as a string.
        /// </summary>
        public string? SqlParameters { get; }
        
        /// <summary>
        /// The MS Sql Exception Instance.
        ///  Contains more exact information about the exception.
        /// </summary>
        public NpgsqlException? NpgsqlException { get; }

        /// <summary>
        /// The Default Error Message Template
        /// </summary>
        private static string _errorMessageTemplate = "A Postgre SQL Data Access Exception Occured: {0}";

        /// <summary>
        /// Creates an instance of the PostgreSqlDataAccessException class 
        /// </summary>
        /// <param name="message">The Exception Message.</param>
        public PostgreSqlDataAccessException(string message)
            : base (string.Format(_errorMessageTemplate, message))
        {}

        /// <summary>
        /// Creates an instance of the PostgreSqlDataAccessException class 
        /// </summary>
        /// <param name="message">The Exception Message.</param>
        /// <param name="commandText">The Command Text.</param>
        /// <param name="commandType">The Command Type.</param>
        public PostgreSqlDataAccessException(string message, string commandText, string commandType)
            : base (string.Format(_errorMessageTemplate, message), commandText, commandType)
        {}

        /// <summary>
        /// Creates an instance of the PostgreSqlDataAccessException class 
        /// </summary>
        /// <param name="message">The Exception Message.</param>
        /// <param name="commandText">The Command Text.</param>
        /// <param name="commandType">The Command Type.</param>
        /// <param name="sqlParameters">The SQL Parameters.</param>
        /// <param name="npgSqlException">The NpgsqlException.</param>
        public PostgreSqlDataAccessException(string message, string commandText, string commandType, NpgsqlParameter[]? sqlParameters, NpgsqlException npgSqlException)
            : base (string.Format(_errorMessageTemplate, message), commandText, commandType)
        {
            this.NpgsqlException = npgSqlException;
            this.SqlParameters = ParseSqlParameters(sqlParameters);
        }

        /// <summary>
        /// Creates an instance of the PostgreSqlDataAccessException class 
        /// </summary>
        /// <param name="message">The Exception Message.</param>
        /// <param name="npgSqlException">The MS SQL Exception.</param>
        public PostgreSqlDataAccessException(string message, NpgsqlException npgSqlException)
            : base (string.Format(_errorMessageTemplate, message))
        {
            this.NpgsqlException = npgSqlException;
        }

    }
}
