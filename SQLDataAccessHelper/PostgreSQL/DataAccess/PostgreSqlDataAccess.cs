// "<copyright file="PostgreSqlDataAccess.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SQLDataAccessHelper.PostgreSQL.DataAccess;

using System.Data;
using Npgsql;
using SQLDataAccessHelper.Common.Exceptions;
using Common.Helpers;
using Exceptions;

public class PostgreSqlDataAccess
{
    /// <summary>
    /// The Private Sql Connection Instance.
    /// </summary>
    private NpgsqlConnection _connection;
    
    /// <summary>
    /// Private Command Helper Instance
    /// </summary>
    private CommandHelper<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter> _commandHelper = new();

    /// <summary>
    /// Creates an Instance, initializing a Sql Connection with the provided Database Connection String.
    /// </summary>
    /// <param name="connectionString">The Connection String.</param>
    public PostgreSqlDataAccess(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
    }
    
    #region Public Synchronous Operations
    
    /// <summary>
    /// Disposes all managed and unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (this._connection.State == ConnectionState.Open)
            _connection.Close();

        _connection?.Dispose();
    }

    /// <summary>
    /// Opens the SQL Connection (Use this in a using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public PostgreSqlDataAccess Open()
    {
        this._connection.Open();
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
        if (_connection.State == ConnectionState.Open)
            await _connection.CloseAsync().ConfigureAwait(false);

        await _connection.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Opens the SQL Connection Asynchronously (Use this in an async using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public async Task<PostgreSqlDataAccess> OpenAsync()
    {
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
        NpgsqlDataReader reader;

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