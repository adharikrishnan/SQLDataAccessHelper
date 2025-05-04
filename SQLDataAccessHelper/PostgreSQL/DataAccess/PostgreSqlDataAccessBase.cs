using Npgsql;
using SQLDataAccessHelper.PostgreSQL.Exceptions;

namespace SQLDataAccessHelper.PostgreSQL.DataAccess;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Common.Exceptions;
using Common.Helpers;
using Common.Models;
using SQLServer.Exceptions;

public class PostgreSqlDataAccessBase
{
    /// <summary>
    /// Private Command Helper Instance
    /// </summary>
    private CommandHelper<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter> _commandHelper = new();

    /// <summary>
    /// The General Purpose Connection String that can be used
    /// for both Read and Write Actions.
    /// </summary>
    /// <value>
    /// The Read and Write Database Connection string.
    /// </value>
    protected virtual string ConnectionString { get; } = null!;

    /// <summary>
    /// The Specialized Readonly Connection String that can be used
    /// for read actions specifically.
    /// </summary>
    /// <value>
    /// The Readonly Database Connection string.
    /// </value>
    protected virtual string? ReadOnlyConnectionString { get; }

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataAccessBase"/> class.
    /// This Constructor Initializes an instance of class with the Default Connection String
    /// from the IConfiguration Instance with the given connection string path.
    /// </summary>
    /// <param name="configuration">The IConfiguration instance.</param>
    /// <param name="connectionStringPath">The Connection String Path.</param>
    public PostgreSqlDataAccessBase(IConfiguration configuration, string connectionStringPath)
    {
        this.ConnectionString = configuration[connectionStringPath]
                                ?? throw new DataAccessException(
                                    $"Connection String could not be found from the specified path - {connectionStringPath}");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataAccessBase "/> class.
    /// </summary>
    /// <param name="sqlCredentials">The SQL Credentials.</param>
    public PostgreSqlDataAccessBase(SqlCredentials sqlCredentials)
    {
        this.ConnectionString = sqlCredentials.ConnectionString;
        this.ReadOnlyConnectionString = sqlCredentials.ReadOnlyConnectionString;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataAccessBase "/> class.
    /// </summary>
    /// <param name="connectionString">The General SQL Purpose Connection String</param>
    public PostgreSqlDataAccessBase(string connectionString)
    {
        this.ConnectionString = connectionString;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataAccessBase "/> class.
    /// </summary>
    /// <param name="connectionString">The General SQL Purpose Connection String</param>
    /// <param name="readOnlyConnectionString"></param>
    public PostgreSqlDataAccessBase(string connectionString, string readOnlyConnectionString)
    {
        this.ConnectionString = connectionString;
        this.ReadOnlyConnectionString = readOnlyConnectionString;
    }

    #endregion

    #region Synchronous Operations

    /// <summary>
    /// Executes a psql Command against the given connection and returns a data reader object.
    /// </summary>
    /// <param name="connection">The Connection to database.</param>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to function.</param>
    /// <returns>
    /// Returns a Npgsql Data Reader object.
    /// </returns>
    protected NpgsqlDataReader ExecuteReader(NpgsqlConnection connection, CommandType commandType, string commandText,
        params NpgsqlParameter[] parameters)
    {
        NpgsqlDataReader? reader = null;

        try
        {
            NpgsqlCommand? command;
            using (command = _commandHelper.CreateSqlCommand(connection, commandType, commandText, parameters))
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
    /// <param name="connection">The Connection to database.</param>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to the query.</param>
    /// <returns>
    /// Number of rows affected as an Integer.
    /// </returns>
    protected int ExecuteNonQuery(NpgsqlConnection connection, CommandType commandType, string commandText,
        params NpgsqlParameter[] parameters)
    {
        try
        {
            NpgsqlCommand command;
            using (command = _commandHelper.CreateSqlCommand(connection, commandType, commandText, parameters))
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
    /// <param name="connection">The Connection to database.</param>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to the query.</param>
    /// <returns>
    /// The Scalar Value as a generic.
    /// </returns>
    protected T? ExecuteScalar<T>(NpgsqlConnection connection, CommandType commandType, string commandText,
        params NpgsqlParameter[] parameters)
    {
        try
        {
            NpgsqlCommand command;
            using (command = _commandHelper.CreateSqlCommand(connection, commandType, commandText, parameters))
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

    /// <summary>
    /// Opens a PostgreSql Database Connection.
    /// </summary>
    /// <returns>
    /// The Database Connection Object.
    /// </returns>
    protected NpgsqlConnection OpenConnection()
    {
        if (this.ConnectionString is null)
            throw new DataAccessException("The Connection String has not been specified.");

        return this.OpenConnection(this.ConnectionString);
    }

    /// <summary>
    /// Opens a PostgreSql Readonly database Connection.
    /// </summary>
    /// <returns>
    /// The Readonly Database Connection Object
    /// </returns>
    protected NpgsqlConnection OpenReadonlyConnection()
    {
        if (this.ReadOnlyConnectionString is null)
            throw new DataAccessException("The Readonly Connection String has not been specified.");

        return this.OpenConnection(this.ReadOnlyConnectionString);
    }

    /// <summary>
    /// Opens a PostgreSql Database Connection with the given Connection String.
    /// </summary>
    /// <param name="connectionString">The Connection String.</param>
    /// <returns>
    /// The Database Connection Object.
    /// </returns>
    private NpgsqlConnection OpenConnection(string connectionString)
    {
        NpgsqlConnection connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    #endregion

    #region Async Operations

    /// <summary>
    /// Executes a PostgreSql Command against the given connection and returns a data reader object.
    /// </summary>
    /// <param name="connection">The Database Connection.</param>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to function.</param>
    /// <returns>
    /// Returns a SQL Data Reader object.
    /// </returns>
    protected async Task<NpgsqlDataReader> ExecuteReaderAsync(NpgsqlConnection connection, CommandType commandType,
        string commandText, params NpgsqlParameter[] parameters)
    {
        NpgsqlDataReader reader;

        try
        {
            NpgsqlCommand command;
            await using (command = _commandHelper.CreateSqlCommand(connection, commandType, commandText, parameters))
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
    /// <param name="connection">The Database Connection.</param>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to the query.</param>
    /// <returns>
    /// Number of rows affected as an Integer.
    /// </returns>
    protected async Task<int> ExecuteNonQueryAsync(NpgsqlConnection connection, CommandType commandType,
        string commandText, params NpgsqlParameter[] parameters)
    {
        try
        {
            NpgsqlCommand command;
            await using (command = _commandHelper.CreateSqlCommand(connection, commandType, commandText, parameters))
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
    /// <param name="connection">The Database Connection.</param>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="parameters">The Parameters to be added (Optional).</param>
    /// <returns>
    /// The Scalar Value as a generic.
    /// </returns>
    protected async Task<T?> ExecuteScalarAsync<T>(NpgsqlConnection connection, CommandType commandType,
        string commandText, params NpgsqlParameter[] parameters)
    {
        try
        {
            NpgsqlCommand command;
            await using (command = _commandHelper.CreateSqlCommand(connection, commandType, commandText, parameters))
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
    
    /// <summary>
    /// Opens a PostgreSql Database Connection Asynchronously.
    /// </summary>
    /// <returns>
    /// The Database Connection Object.
    /// </returns>
    protected async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        if (this.ConnectionString is null)
            throw new DataAccessException("The Connection String has not been specified.");

        return await this.OpenConnectionAsync(this.ConnectionString).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a PostgreSql Readonly Database Connection Asynchronously.
    /// </summary>
    /// <returns>
    /// The Database Connection Object.
    /// </returns>
    protected async Task<NpgsqlConnection> OpenReadonlyConnectionAsync()
    {
        if (this.ReadOnlyConnectionString is null)
            throw new DataAccessException("The Readonly Connection String has not been specified.");

        return await this.OpenConnectionAsync(this.ReadOnlyConnectionString).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a PostgreSql Database Connection Asynchronously with the given Connection String.
    /// </summary>
    /// <param name="connectionString">The Connection String.</param>
    /// <returns>
    /// The Database Connection Object. 
    /// </returns>
    private async Task<NpgsqlConnection> OpenConnectionAsync(string connectionString)
    {
        NpgsqlConnection connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync().ConfigureAwait(false);
        return connection;
    }

    #endregion
}