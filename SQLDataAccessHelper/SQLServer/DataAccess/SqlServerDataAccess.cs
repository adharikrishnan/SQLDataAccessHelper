using System.Data;
using Microsoft.Data.SqlClient;
using SQLDataAccessHelper.Common.Exceptions;
using SQLDataAccessHelper.SQLServer.Exceptions;
using static SQLDataAccessHelper.SQLServer.Common.SqlServerHelpers;

namespace SQLDataAccessHelper.SQLServer.DataAccess;

/// <summary>
/// Managed Helper class to help connections to
/// a Sql Server Database with the provided Connection String
/// </summary>
public class SqlServerDataAccess : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The Private Sql Connection Instance.
    /// </summary>
    private SqlConnection _connection;

    /// <summary>
    /// Creates an Instance, initializing a Sql Connection with the provided Database Connection String.
    /// </summary>
    /// <param name="connectionString">The Connection String.</param>
    public SqlServerDataAccess(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
    }

    #region Public Syncronous Operations

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
    public SqlServerDataAccess Open()
    {
        this._connection.Open();
        return this;
    }

    /// <summary>
    /// Method which executes the SQL command to retrieve data from database.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to function.</param>
    /// <returns>
    /// Returns a SQL Data Reader object.
    /// </returns>
    public SqlDataReader ExecuteReader(CommandType commandType, string commandText,
        params SqlParameter[] parameters)
    {
        SqlDataReader? reader = null;

        try
        {
            SqlCommand? command;
            using (command = CreateSqlCommand(_connection, commandType, commandText, parameters))
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

        return reader;
    }


    /// <summary>
    /// Executes a T-SQL statement against the connection and returns the number of rows affected.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to the query.</param>
    /// <returns>
    /// Number of rows affected as an Integer.
    /// </returns>
    public int ExecuteNonQuery(CommandType commandType, string commandText,
        params SqlParameter[] parameters)
    {
        try
        {
            SqlCommand command;
            using (command = CreateSqlCommand(_connection, commandType, commandText, parameters))
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
    /// Executes a T-SQL Statement against an Open Connection and returns a Scalar Result.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to the query.</param>
    /// <returns>
    /// The Scalar Value as a generic.
    /// </returns>
    public T? ExecuteScalar<T>(CommandType commandType, string commandText,
        params SqlParameter[] parameters)
    {
        try
        {
            SqlCommand command;
            using (command = CreateSqlCommand(_connection, commandType, commandText, parameters))
            {
                return (T)command.ExecuteScalar();
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
        if (_connection.State == ConnectionState.Open)
            await _connection.CloseAsync().ConfigureAwait(false);

        await _connection.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Opens the SQL Connection Asynchronously (Use this in an async using Scope)
    /// </summary>
    /// <returns>This Instance of the class.</returns>
    public async Task<SqlServerDataAccess> OpenAsync()
    {
        await _connection.OpenAsync().ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Method which executes the SQL command to retrieve data from database.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to function.</param>
    /// <returns>
    /// Returns a SQL Data Reader object.
    /// </returns>
    public async Task<SqlDataReader> ExecuteReaderAsync(CommandType commandType,
        string commandText, params SqlParameter[] parameters)
    {
        SqlDataReader reader;

        try
        {
            SqlCommand command;
            await using (command = CreateSqlCommand(_connection, commandType, commandText, parameters))
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

        return reader;
    }

    /// <summary>
    /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command text.</param>
    /// <param name="parameters">The Parameters passed to the query.</param>
    /// <returns>
    /// Number of rows affected as an Integer.
    /// </returns>
    public async Task<int> ExecuteNonQueryAsync(CommandType commandType,
        string commandText, params SqlParameter[] parameters)
    {
        try
        {
            SqlCommand command;
            await using (command = CreateSqlCommand(_connection, commandType, commandText, parameters))
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
    /// Executes a TSQL Statement against an Open Connection and returns a Scalar Result Asynchronously.
    /// </summary>
    /// <param name="commandType">The Command type.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="parameters">The Parameters to be added (Optional).</param>
    /// <returns>
    /// The Scalar Value as a generic.
    /// </returns>
    public async Task<T?> ExecuteScalarAsync<T>(CommandType commandType,
        string commandText, params SqlParameter[] parameters)
    {
        try
        {
            SqlCommand command;
            await using (command = CreateSqlCommand(_connection, commandType, commandText, parameters))
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