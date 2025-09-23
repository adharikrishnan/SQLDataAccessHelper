// "<copyright file="PostgreSqlDataAccess.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SQLDataAccessHelper.Common.Helpers;
using SQLDataAccessHelper.Models;
using SQLDataAccessHelper.Exceptions;

namespace SQLDataAccessHelper.PostgreSQL;

/// <summary>
/// Managed Helper class that help streamline connections, read and write operations to
/// a PostgreSQL Database with the provided Connection String
/// </summary>
public class PostgreSqlDataAccess
{
    /// <summary>
    /// The Private Npgsql Connection Instance.
    /// </summary>
    private NpgsqlConnection? _connection;

    /// <summary>
    /// Private Command Helper Instance
    /// </summary>
    private CommandHelper<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter> _commandHelper = new();

    /// <summary>
    /// The general purpose connection string that can be used
    /// for both read and write Actions.
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    /// The read only connection string that can be used
    /// for read actions specifically.
    /// </summary>
    private readonly string? _readOnlyConnectionString;

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataAccess"/> class.
    /// This Constructor Initializes an instance of class with the Default Connection String
    /// from the IConfiguration Instance with the given connection string path.
    /// </summary>
    /// <param name="configuration">The IConfiguration instance.</param>
    /// <param name="connectionStringPath">The Connection String Path.</param>
    public PostgreSqlDataAccess(IConfiguration configuration, string connectionStringPath)
    {
        _connectionString = configuration[connectionStringPath]
                                ?? throw new DataAccessException(
                                    $"Connection String could not be found from the specified path - {connectionStringPath}");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataAccess "/> class.
    /// </summary>
    /// <param name="sqlCredentials">The SQL Credentials.</param>
    public PostgreSqlDataAccess(SqlCredentials sqlCredentials)
    {
        _connectionString = sqlCredentials.ConnectionString;
        _readOnlyConnectionString = sqlCredentials.ReadOnlyConnectionString;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataAccess "/> class.
    /// </summary>
    /// <param name="connectionString">The General SQL Purpose Connection String</param>
    public PostgreSqlDataAccess(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataAccess "/> class.
    /// </summary>
    /// <param name="connectionString">The General SQL Purpose Connection String</param>
    /// <param name="readOnlyConnectionString"></param>
    public PostgreSqlDataAccess(string connectionString, string readOnlyConnectionString)
    {
        _connectionString = connectionString;
        _readOnlyConnectionString = readOnlyConnectionString;
    }

    #endregion


    #region Public Synchronous Operations

    /// <summary>
    /// Disposes all managed and unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (_connection is not null)
        {
            if (_connection.State is ConnectionState.Open)
                _connection.Close();

            _connection.Dispose();
        }
    }

    /// <summary>
    /// Opens a SQL Connection (Use this in a using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public PostgreSqlDataAccess OpenConnection()
    {
        _connection = new NpgsqlConnection(_connectionString);
        _connection.Open();
        return this;
    }

    /// <summary>
    /// Opens a Readonly SQL Connection (Use this in a using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public PostgreSqlDataAccess OpenReadonlyConnection<T>()
    {
        if (string.IsNullOrWhiteSpace(_readOnlyConnectionString))
            throw new DataAccessException("Readonly Connection String is not configured. Please provide a Read Only Connection through one of the Constructors or use the general use OpenConnection() method.");

        _connection = new NpgsqlConnection(_readOnlyConnectionString);
        _connection.Open();
        return this;
    }

    /// <summary>
    /// Executes a psql Command against the given connection and returns a data reader object.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to function.</param>
    /// <returns>
    /// Returns a Npgsql Data Reader object.
    /// </returns>
    public NpgsqlDataReader ExecuteReader(CommandType commandType, string commandText,
        params NpgsqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnection() or OpenReadonlyConnection() to initialize the connection first.");

        NpgsqlDataReader? reader = null;

        try
        {
            NpgsqlCommand? command;
            using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                reader = command.ExecuteReader();
            }
        }
        catch (Exception ex)
        {
            if (ex is NpgsqlException npgSqlException)
            {
                throw new PostgreSqlDataAccessException(npgSqlException.Message, commandText, commandType.ToString(),
                    parameters, npgSqlException);
            }

            throw new DataAccessException(ex.Message, ex);
        }

        return reader;
    }


    /// <summary>
    /// Executes a psql statement against the connection and returns the number of rows affected.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to the query.</param>
    /// <returns>
    /// Number of rows affected as an Integer.
    /// </returns>
    public int ExecuteNonQuery(CommandType commandType, string commandText,
        params NpgsqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnection() or OpenReadonlyConnection() to initialize the connection first.");

        try
        {
            NpgsqlCommand command;
            using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            if (ex is NpgsqlException npgSqlException)
            {
                throw new PostgreSqlDataAccessException(npgSqlException.Message, commandText, commandType.ToString(),
                    parameters, npgSqlException);
            }

            throw new DataAccessException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Executes a psql Statement against an Open Connection and returns a Scalar Result .
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to the query.</param>
    /// <returns>
    /// The Scalar Value as a generic.
    /// </returns>
    public T? ExecuteScalar<T>(CommandType commandType, string commandText,
        params NpgsqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnection() or OpenReadonlyConnection() to initialize the connection first.");

        try
        {
            NpgsqlCommand command;
            using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                return (T?)command.ExecuteScalar();
            }
        }
        catch (Exception ex)
        {
            if (ex is NpgsqlException npgSqlException)
            {
                throw new PostgreSqlDataAccessException(npgSqlException.Message, commandText, commandType.ToString(),
                    parameters, npgSqlException);
            }

            throw new DataAccessException(ex.Message, ex);
        }
    }
    #endregion

    #region Public Async Operations

    /// <summary>
    /// Disposes all managed and unmanaged resources asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            if (_connection.State == ConnectionState.Open)
                await _connection.CloseAsync().ConfigureAwait(false);

            await _connection.DisposeAsync().ConfigureAwait(false);
        }

    }

    /// <summary>
    /// Opens a SQL Connection Asynchronously (Use this in an async using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public async Task<PostgreSqlDataAccess> OpenConnectionAsync()
    {
        _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync().ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Opens a Readonly SQL Connection Asynchronously (Use this in an async using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public async Task<PostgreSqlDataAccess> OpenReadonlyConnectionAsync()
    {
        if (string.IsNullOrWhiteSpace(_readOnlyConnectionString))
            throw new DataAccessException("Readonly Connection String is not configured. Please provide a Read Only Connection through one of the Constructors or use the general use OpenConnectionAsync() method.");

        _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync().ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Executes a PostgreSql Command against the given connection and returns a data reader object.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to function.</param>
    /// <returns>
    /// Returns a SQL Data Reader object.
    /// </returns>
    public async Task<NpgsqlDataReader> ExecuteReaderAsync(CommandType commandType,
        string commandText, params NpgsqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnectionAsync() or OpenReadonlyConnectionAsync() to initialize the connection first.");

        NpgsqlDataReader? reader;

        try
        {
            NpgsqlCommand command;
            await using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            if (ex is NpgsqlException npgSqlException)
            {
                throw new PostgreSqlDataAccessException(npgSqlException.Message, commandText, commandType.ToString(),
                    parameters, npgSqlException);
            }

            throw new DataAccessException(ex.Message, ex);
        }

        return reader;
    }

    /// <summary>
    /// Executes a psql statement against the connection and returns the number of rows affected.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to the query.</param>
    /// <returns>
    /// Number of rows affected as an Integer.
    /// </returns>
    public async Task<int> ExecuteNonQueryAsync(CommandType commandType,
        string commandText, params NpgsqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnectionAsync() or OpenReadonlyConnectionAsync() to initialize the connection first.");

        try
        {
            NpgsqlCommand command;
            await using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            if (ex is NpgsqlException npgSqlException)
            {
                throw new PostgreSqlDataAccessException(npgSqlException.Message, commandText, commandType.ToString(),
                    parameters, npgSqlException);
            }

            throw new DataAccessException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Executes a psql Statement against an Open Connection and returns a Scalar Result Asynchronously.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="parameters">The Parameters to be added (Optional).</param>
    /// <returns>
    /// The Scalar Value as a generic.
    /// </returns>
    public async Task<T?> ExecuteScalarAsync<T>(CommandType commandType,
        string commandText, params NpgsqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnectionAsync() or OpenReadonlyConnectionAsync() to initialize the connection first.");

        try
        {
            NpgsqlCommand command;
            await using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                return (T?)await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            if (ex is NpgsqlException npgSqlException)
            {
                throw new PostgreSqlDataAccessException(npgSqlException.Message, commandText, commandType.ToString(),
                    parameters, npgSqlException);
            }

            throw new DataAccessException(ex.Message, ex);
        }
    }
    #endregion
}