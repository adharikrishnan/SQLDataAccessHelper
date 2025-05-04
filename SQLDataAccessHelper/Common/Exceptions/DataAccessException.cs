// "<copyright file="DataAccessException.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SQLDataAccessHelper.Common.Exceptions;

using System.Data.Common;

public class DataAccessException : Exception
{
    /// <summary>
    /// The Command Text.
    /// </summary>
    public string? CommandText { get; }

    /// <summary>
    /// The Command Type.
    /// </summary>
    public string? CommandType { get; }

    /// <summary>
    /// Creates an instance of the DataAccessException Class.
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    public DataAccessException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates an instance of the DataAccessException Class.
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    /// <param name="innerException">The Inner Exception.</param>
    public DataAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a new Instance of the DataAccessException Class. 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="commandType">The Command Type.</param>
    protected DataAccessException(string message, string commandText, string commandType)
        : base(message)
    {
        this.CommandText = commandText;
        this.CommandType = commandType;
    }

    /// <summary>
    /// Creates a new Instance of the DataAccessException Class. 
    /// </summary>
    /// <param name="message">The Exception Message.</param>
    /// <param name="innerException">The Inner Exception.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="commandType">The Command Type.</param>
    protected DataAccessException(
        string message,
        Exception innerException,
        string commandText,
        string commandType)
        : base
        (
            message, innerException
        )
    {
        this.CommandText = commandText;
        this.CommandType = commandType;
    }

    /// <summary>
    /// Parses the Sql Parameter Array to a single string.
    /// </summary>
    /// <param name="sqlParameters">The Sql Parameters</param>
    /// <returns>The Sql Parameters as a string (Null if no parameters are there).</returns>
    protected string? ParseSqlParameters<TParameter>(TParameter[]? sqlParameters)
        where TParameter : DbParameter
    {
        if (sqlParameters == null || sqlParameters.Length == 0)
            return null;

        string parameters = "Parameters:\n";
        parameters += "Name : Value\n";

        foreach (TParameter parameter in sqlParameters)
        {
            parameters += $"{parameter.ParameterName} : {parameter.Value}\n";
        }

        return parameters;
    }
}