// "<copyright file="SqlServerDataAccessException.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SqlDataAccessHelper.Core.Exceptions;

using Microsoft.Data.SqlClient;

/// <summary>
/// Exception Class to deal with SQL Server DataAccess Exceptions.
/// </summary>
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
    public SqlException? SqlException { get; }

    /// <summary>
    /// The Default Error Message Template
    /// </summary>
    private static readonly string ErrorMessageTemplate = "A SQL Server Data Access Exception Occured: {0}";

    /// <summary>
    /// Creates an instance of the SqlServerDataAccessException class 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    public SqlServerDataAccessException(string message)
        : base(string.Format(ErrorMessageTemplate, message))
    {
    }

    /// <summary>
    /// Creates an instance of the SqlServerDataAccessException class 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="commandType">The Command Type.</param>
    public SqlServerDataAccessException(string message, string commandText, string commandType)
        : base(string.Format(ErrorMessageTemplate, message), commandText, commandType)
    {
    }

    /// <summary>
    /// Creates an instance of the SqlServerDataAccessException class 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="commandType">The Command Type.</param>
    /// <param name="sqlParameters">The SQL Parameters.</param>
    /// <param name="sqlException">The MS SQL Exception.</param>
    public SqlServerDataAccessException(string message, string commandText, string commandType,
        SqlParameter[]? sqlParameters, SqlException sqlException)
        : base(string.Format(ErrorMessageTemplate, message), commandText, commandType)
    {
        this.SqlException = sqlException;
        this.SqlParameters = ParseSqlParameters(sqlParameters);
    }

    /// <summary>
    /// Creates an instance of the SqlServerDataAccessException class 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    /// <param name="sqlException">The MS SQL Exception.</param>
    public SqlServerDataAccessException(string message, SqlException sqlException)
        : base(string.Format(ErrorMessageTemplate, message))
    {
        this.SqlException = sqlException;
    }
}