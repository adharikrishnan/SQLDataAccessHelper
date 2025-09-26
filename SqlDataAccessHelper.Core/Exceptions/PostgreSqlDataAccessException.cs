// "<copyright file="PostgreSqlDataAccessException.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SqlDataAccessHelper.Core.Exceptions;

using Npgsql;

/// <summary>
/// Exception Class to deal with PostgreSql DataAccess Exceptions.
/// </summary>
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
    private static readonly string ErrorMessageTemplate = "A PostgreSQL Data Access Exception Occured: {0}";

    /// <summary>
    /// Creates an instance of the PostgreSqlDataAccessException class 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    public PostgreSqlDataAccessException(string message)
        : base(string.Format(ErrorMessageTemplate, message))
    {
    }

    /// <summary>
    /// Creates an instance of the PostgreSqlDataAccessException class 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="commandType">The Command Type.</param>
    public PostgreSqlDataAccessException(string message, string commandText, string commandType)
        : base(string.Format(ErrorMessageTemplate, message), commandText, commandType)
    {
    }

    /// <summary>
    /// Creates an instance of the PostgreSqlDataAccessException class 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="commandType">The Command Type.</param>
    /// <param name="sqlParameters">The SQL Parameters.</param>
    /// <param name="npgSqlException">The NpgsqlException.</param>
    public PostgreSqlDataAccessException(string message, string commandText, string commandType,
        NpgsqlParameter[]? sqlParameters, NpgsqlException npgSqlException)
        : base(string.Format(ErrorMessageTemplate, message), commandText, commandType)
    {
        this.NpgsqlException = npgSqlException;
        this.SqlParameters = ParseSqlParameters(sqlParameters);
    }

    /// <summary>
    /// Creates an instance of the PostgreSqlDataAccessException class 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    /// <param name="npgSqlException">The NpgsqlException.</param>
    public PostgreSqlDataAccessException(string message, NpgsqlException npgSqlException)
        : base(string.Format(ErrorMessageTemplate, message))
    {
        this.NpgsqlException = npgSqlException;
    }
}