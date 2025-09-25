// "<copyright file="SqlServerDataAccess.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SQLDataAccessHelper.Helpers;
using SQLDataAccessHelper.Models;
using SQLDataAccessHelper.Exceptions;

namespace SQLDataAccessHelper.SQLServer;

/// <summary>
/// Managed Helper class that help streamline connections, read and write operations to
/// a SQL Server Database.
/// </summary>
public class SqlServerDataAccess : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The Private Sql Connection Instance.
    /// </summary>
    private SqlConnection? _connection;
    
    /// <summary>
    /// The Private Data Reader object to manage data reader lifetime.
    /// </summary>
    private SqlDataReader? _dataReader;
    
    /// <summary>
    /// Private Command Helper Instance
    /// </summary>
    private readonly CommandHelper<SqlConnection, SqlCommand, SqlParameter> _commandHelper = new();
    
    /// <summary>
    /// The general purpose connection string that can be used
    /// for both read and write Actions.
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    /// The readonly connection string that can be used
    /// for read actions specifically.
    /// </summary>
    private readonly string? _readOnlyConnectionString;
    
    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDataAccess"/> class.
    /// This Constructor Initializes an instance of class with the Default Connection String
    /// from the IConfiguration Instance with the given connection string path.
    /// </summary>
    /// <param name="configuration">The IConfiguration instance.</param>
    /// <param name="connectionStringPath">The Connection String Path.</param>
    public SqlServerDataAccess(IConfiguration configuration, string connectionStringPath)
    {
        _connectionString = configuration[connectionStringPath]
                                ?? throw new DataAccessException(
                                    $"Connection String could not be found from the specified path - {connectionStringPath}");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDataAccess "/> class.
    /// </summary>
    /// <param name="sqlCredentials">The SQL Credentials.</param>
    public SqlServerDataAccess(SqlCredentials sqlCredentials)
    {
        _connectionString = sqlCredentials.ConnectionString;
        _readOnlyConnectionString = sqlCredentials.ReadOnlyConnectionString;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDataAccess "/> class.
    /// </summary>
    /// <param name="connectionString">The General SQL Purpose Connection String</param>
    public SqlServerDataAccess(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDataAccess "/> class.
    /// </summary>
    /// <param name="connectionString">The General SQL Purpose Connection String</param>
    /// <param name="readOnlyConnectionString"></param>
    public SqlServerDataAccess(string connectionString, string readOnlyConnectionString)
    {
        _connectionString = connectionString;
        _readOnlyConnectionString = readOnlyConnectionString;
    }
    
    #endregion
    
    #region Private Methods

    /// <summary>
    /// Closes and disposes the private instance data reader object, if it has been set from a
    /// previous execute reader operation.
    /// </summary>
    private void DisposeExistingReader()
    {
        if (_dataReader is null) return;

        if (!_dataReader.IsClosed)
            _dataReader.Close();

        _dataReader.Dispose();
        _dataReader = null;
    }

    /// <summary>
    /// Closes and disposes the private instance data reader object asynchronously, if it has been set from a
    /// previous execute reader operation.
    /// </summary>
    private async Task DisposeExistingReaderAsync()
    {
        if (_dataReader is null) return;

        if (!_dataReader.IsClosed)
            await _dataReader.CloseAsync().ConfigureAwait(false);

        await _dataReader.DisposeAsync().ConfigureAwait(false);
        _dataReader = null;
    }

    #endregion
    
    #region Public Synchronous Operations

    /// <summary>
    /// Disposes all managed and unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        DisposeExistingReader();
        
        if (_connection is null) return;
        
        if (_connection.State is ConnectionState.Open)
            _connection.Close();

        _connection.Dispose();
        
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Opens a SQL Connection (Use this in a using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public SqlServerDataAccess OpenConnection()
    {
        _connection = new SqlConnection(_connectionString);
        _connection.Open();
        return this;
    }
    
    /// <summary>
    /// Opens a Readonly SQL Connection (Use this in a using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public SqlServerDataAccess OpenReadonlyConnection()
    {
        if (string.IsNullOrWhiteSpace(_readOnlyConnectionString))
            throw new DataAccessException("Readonly Connection String is not configured. Please provide a Read Only Connection through one of the Constructors or use the general use OpenConnection() method.");

        _connection = new SqlConnection(_readOnlyConnectionString); 
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
    public SqlDataReader ExecuteReader(CommandType commandType, string commandText, params SqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnection() or OpenReadonlyConnection() to initialize the connection first.");
        
        // In the case there is an open data reader object present on the same connection.
        DisposeExistingReader();
        
        SqlDataReader? reader;

        try
        {
            SqlCommand? command;
            using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                reader = command.ExecuteReader();
            }
        }
        catch (Exception ex)
        {
            if (ex is SqlException sqlException)
            {
                throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(),
                    parameters, sqlException);
            }

            throw new DataAccessException(ex.Message, ex);
        }

        _dataReader = reader;
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
    public int ExecuteNonQuery(CommandType commandType, string commandText, params SqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnection() or OpenReadonlyConnection() to initialize the connection first.");
        
        // In the case there is an open data reader object present on the same connection.
        DisposeExistingReader();
        
        try
        {
            SqlCommand command;
            using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            if (ex is SqlException sqlException)
            {
                throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(),
                    parameters, sqlException);
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
    public T? ExecuteScalar<T>(CommandType commandType, string commandText, params SqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnection() or OpenReadonlyConnection() to initialize the connection first.");

        // In the case there is an open data reader object present on the same connection.
        DisposeExistingReader();
        
        try
        {
            SqlCommand command;
            using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                return (T?)command.ExecuteScalar();
            }
        }
        catch (Exception ex)
        {
            if (ex is SqlException sqlException)
            {
                throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(),
                    parameters, sqlException);
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
            if(_connection.State == ConnectionState.Open)
                await _connection.CloseAsync().ConfigureAwait(false);

            await _connection.DisposeAsync().ConfigureAwait(false);            
        }

    }

    /// <summary>
    /// Opens a SQL Connection Asynchronously (Use this in an async using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public async Task<SqlServerDataAccess> OpenConnectionAsync()
    {
        _connection = new SqlConnection(_connectionString);
        await _connection.OpenAsync().ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Opens a Readonly SQL Connection Asynchronously (Use this in an async using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public async Task<SqlServerDataAccess> OpenReadonlyConnectionAsync()
    {
        if (string.IsNullOrWhiteSpace(_readOnlyConnectionString))
            throw new DataAccessException("Readonly Connection String is not configured. Please provide a Read Only Connection through one of the Constructors or use the general use OpenConnectionAsync() method.");

        _connection = new SqlConnection(_connectionString);
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
    public async Task<SqlDataReader> ExecuteReaderAsync(CommandType commandType, string commandText, params SqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnectionAsync() or OpenReadonlyConnectionAsync() to initialize the connection first.");
        
        // In the case there is an open data reader object present on the same connection.
        await DisposeExistingReaderAsync().ConfigureAwait(false);
        
        SqlDataReader? reader;

        try
        {
            SqlCommand command;
            await using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            if (ex is SqlException sqlException)
            {
                throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(),
                    parameters, sqlException);
            }

            throw new DataAccessException(ex.Message, ex);
        }

        _dataReader = reader;
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
    public async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, params SqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnectionAsync() or OpenReadonlyConnectionAsync() to initialize the connection first.");
        
        // In the case there is an open data reader object present on the same connection.
        await DisposeExistingReaderAsync().ConfigureAwait(false);
        
        try
        {
            SqlCommand command;
            await using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            if (ex is SqlException sqlException)
            {
                throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(),
                    parameters, sqlException);
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
    public async Task<T?> ExecuteScalarAsync<T>(CommandType commandType, string commandText, params SqlParameter[] parameters)
    {
        if (_connection is null)
            throw new DataAccessException("Connection is not initialized. Please use OpenConnectionAsync() or OpenReadonlyConnectionAsync() to initialize the connection first.");
        
        // In the case there is an open data reader object present on the same connection.
        await DisposeExistingReaderAsync().ConfigureAwait(false);
        
        try
        {
            SqlCommand command;
            await using (command = _commandHelper.CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                return (T?)await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            if (ex is SqlException sqlException)
            {
                throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(),
                    parameters, sqlException);
            }

            throw new DataAccessException(ex.Message, ex);
        }
    }
    #endregion
}