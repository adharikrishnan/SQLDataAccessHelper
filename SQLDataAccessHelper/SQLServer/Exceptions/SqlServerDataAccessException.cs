// "<copyright file="SqlServerDataAccessException.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SQLDataAccessHelper.SQLServer.Exceptions
{
    using System;
    using Microsoft.Data.SqlClient;
    using SQLDataAccessHelper.Common.Exceptions;

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
        /// The Default Error Message Template
        /// </summary>
        private static string ErrorMessageTemplate = "A Sql Server Data Access Exception Occured: {0}";

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
            : base (string.Format(ErrorMessageTemplate, message))
        {}

        /// <summary>
        /// Creates an instance of the SqlServerDataAccessException class 
        /// </summary>
        /// <param name="message">The Exception Message.</param>
        /// <param name="commandText">The Command Text.</param>
        /// <param name="commandType">The Command Type.</param>
        public SqlServerDataAccessException(string message, string commandText, string commandType)
            : base (string.Format(ErrorMessageTemplate, message), commandText, commandType)
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
            : base (string.Format(ErrorMessageTemplate, message), commandText, commandType)
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
            : base (string.Format(ErrorMessageTemplate, message))
        {
            this.sqlException = sqlException;
        }

        /// <summary>
        /// Parses the Sql Parameter Array to a single string.
        /// </summary>
        /// <param name="sqlParameters">The Sql Parameters</param>
        /// <returns>The Sql Parameters as a string (Null if no parameters are there).</returns>
        private string? ParseSqlParameters(SqlParameter[] sqlParameters)
        {
            if (sqlParameters == null || sqlParameters.Length == 0)
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
