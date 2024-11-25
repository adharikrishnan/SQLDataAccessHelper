// "<copyright file="SqlServerDataAccessException.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SQLDataAccess.SQLServer.Exceptions
{
    using System;
    using Microsoft.Data.SqlClient;
    using SQLDataAccess.Common.Exceptions;

    /// <summary>
    /// Exception Class to deal with SQL Server DataAccess Exceptions.
    /// </summary>
    [Serializable]
    public class SqlServerDataAccessException : DataAccessException
    {
        /// <summary>
        /// The Sql Parameters as a string.
        /// </summary>
        public string? SqlParameters { get; }
        
        /// <summary>
        /// The MS Sql Exception Instance.
        ///  Contains more exact information about the exception.
        /// </summary>
        public SqlException? sqlException { get; }

        /// <summary>
        /// Creates an instance of the SqlServerDataAccessException class 
        /// </summary>
        public SqlServerDataAccessException()
        {}

        /// <summary>
        /// Creates an instance of the SqlServerDataAccessException class 
        /// </summary>
        /// <param name="message">The Exception Message.</para>
        public SqlServerDataAccessException(string message)
            : base (message)
        {}

        /// <summary>
        /// Creates an instance of the SqlServerDataAccessException class 
        /// </summary>
        /// <param name="message">The Exception Message.</param>
        /// <param name="commandText">The Command Text.</param>
        /// <param name="commandType">The Command Type.</param>
        public SqlServerDataAccessException(string message, string commandText, string commandType)
            : base (message, commandText, commandType)
        {}

        /// <summary>
        /// Creates an instance of the SqlServerDataAccessException class 
        /// </summary>
        /// <param name="message">The Exception Message.</param>
        /// <param name="commandText">The Command Text.</param>
        /// <param name="commandType">The Command Type.</param>
        /// <param name="sqlParameters">The SQL Parameters.</param>
        /// <param name="sqlException">The MS SQL Exception.</param>
        public SqlServerDataAccessException(string message, string commandText, string commandType, SqlParameter[]? sqlParameters, SqlException sqlException)
            : base (message, commandText, commandType)
        {
            this.sqlException = sqlException;
            this.SqlParameters = ParseSqlParameters(sqlParameters);
        }

        /// <summary>
        /// Creates an instance of the SqlServerDataAccessException class 
        /// </summary>
        /// <param name="message">The Exception Message.</param>
        /// <param name="sqlException">The MS SQL Exception.</param>
        public SqlServerDataAccessException(string message, SqlException sqlException)
            : base (message)
        {
            this.sqlException = sqlException;
        }

        private string? ParseSqlParameters(SqlParameter[] sqlParameters)
        {
            if (sqlParameters is null || sqlParameters.Length is 0)
                return null;

            string parameters = "Parameters:\n";
            parameters += "Name : Value\n";
            
            foreach(SqlParameter parameter in sqlParameters)
            {
                parameters += $"{parameter.ParameterName} : {parameter.Value}\n";
            }

            return parameters;
        }

    }
}
