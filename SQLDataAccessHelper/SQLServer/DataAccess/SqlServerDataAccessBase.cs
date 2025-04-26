// "<copyright file="SqlServerDataAccessBase.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SQLDataAccessHelper.SQLServer.DataAccess
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using SQLDataAccessHelper.Common.Exceptions;
    using Common.Models;
    using Exceptions;

    /// <summary>
    /// Base class for Sql Server (TSQL) Opertions. Inherit this class with you implementation to use.
    /// </summary>
    public class SqlServerDataAccessBase : IDisposable
    {
        /// <summary>
        /// Holds the SQL connection object.
        /// </summary>
        private SqlConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataAccessBase"/> class.
        /// This Constructor Initializes an instance of class with the Default Connection String
        /// from the IConfiguration Instance with the given connection string path.
        /// </summary>
        /// <param name="configuration">The IConfiguration instance.</param>
        /// <param name="connectionStringPath">The Connection String Path.</param>
        public SqlServerDataAccessBase(IConfiguration configuration, string connectionStringPath)
        {
            this.ConnectionString = configuration[connectionStringPath];
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataAccessBase"/> class.
        /// </summary>
        /// <param name="sqlCredentials">The SQL Credentials.</param>
        public SqlServerDataAccessBase(SqlCredentials sqlCredentials)
        {
            this.ConnectionString = sqlCredentials.ConnectionString;
            this.ReadOnlyConnectionString = sqlCredentials.ReadOnlyConnectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataAccessBase"/> class.
        /// </summary>
        /// <param name="connectionString">The General SQL Purpose Connection String</param>
        public SqlServerDataAccessBase(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataAccessBase"/> class.
        /// </summary>
        /// <param name="connectionString">The General SQL Purpose Connection String</param>
        /// <param name="readOnlyConnectionString"></param>
        public SqlServerDataAccessBase(string connectionString, string readOnlyConnectionString)
        {
            this.ConnectionString = connectionString;
            this.ReadOnlyConnectionString = readOnlyConnectionString;
        }

        /// <summary>
        /// The General Purpose Connection String that can be used
        /// for both Read and Write Actions.
        /// </summary>
        /// <value>
        /// The Read and Write Database Connection string.
        /// </value>
        protected virtual string ConnectionString { get; }

        /// <summary>
        /// The Specilized Readonly Connection String that can be used
        /// for read actions specifically.
        /// </summary>
        /// <value>
        /// The Readonly Database Connection string.
        /// </value>
        protected virtual string? ReadOnlyConnectionString { get; }

        /// <summary>
        /// The Dispose Implementation that disposes of all managed and unmanged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Method which executes the SQL command to retrieve data from database.
        /// </summary>
        /// <param name="connection">The Connection to database.</param>
        /// <param name="commandType">The Command type.</param>
        /// <param name="commandText">The Command text.</param>
        /// <param name="parameters">The Parameters passed to function.</param>
        /// <returns>
        /// Returns a SQL Data Reader object.
        /// </returns>
        protected SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            SqlCommand command = null!;
            SqlDataReader reader = null;

            try
            {
                command = this.CreateSqlCommand(connection, commandType, commandText, parameters);
                reader = command.ExecuteReader();
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(), parameters, sqlException);
                }
                else
                {
                    throw new DataAccessException(ex.Message, ex);
                }
            }
            finally
            {
                this.DisposeCommand(command);
            }

            return reader;
        }

        /// <summary>
        /// Method which executes the SQL command to retrieve data from database.
        /// </summary>
        /// <param name="connection">The Connection to database.</param>
        /// <param name="commandType">The Command type.</param>
        /// <param name="commandText">The Command text.</param>
        /// <param name="parameters">The Parameters passed to function.</param>
        /// <returns>
        /// Returns a SQL Data Reader object.
        /// </returns>
        protected async Task<SqlDataReader> ExecuteReaderAsync(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            SqlCommand command = null!;
            SqlDataReader reader = null;

            try
            {
                command = this.CreateSqlCommand(connection, commandType, commandText, parameters);
                reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(), parameters, sqlException);
                }
                else
                {
                    throw new DataAccessException(ex.Message, ex);
                }
            }
            finally
            {
                await this.DisposeCommandAsync(command).ConfigureAwait(false);                
            }

            return reader;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="connection">The Connection to database.</param>
        /// <param name="commandType">The Command type.</param>
        /// <param name="cmdText">The Command text.</param>
        /// <param name="parameters">The Parameters passed to the query.</param>
        /// <returns>
        /// Number of rows affected as an Integer.
        /// </returns>
        protected int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            SqlCommand command = null!;

            try
            {
                command = this.CreateSqlCommand(connection, commandType, commandText, parameters);
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(), parameters, sqlException);
                }
                else
                {
                    throw new DataAccessException(ex.Message, ex);
                }
            }
            finally
            {
                this.DisposeCommand(command);
            }
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="connection">The Connection to database.</param>
        /// <param name="commandType">The Command type.</param>
        /// <param name="commandText">The Command text.</param>
        /// <param name="parameters">The Parameters passed to the query.</param>
        /// <returns>
        /// Number of rows affected as an Integer.
        /// </returns>
        protected async Task<int> ExecuteNonQueryAsync(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] parameters)
        {

            SqlCommand command = null!;

            try
            {
                command = this.CreateSqlCommand(connection, commandType, commandText, parameters);
                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(), parameters, sqlException);
                }
                else
                {
                    throw new DataAccessException(ex.Message, ex);
                }
            }
            finally
            {
                await this.DisposeCommandAsync(command).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a TSQL Statement against an Open Connection and returns a Scalar Result .
        /// </summary>
        /// <param name="connection">The Connection to database.</param>
        /// <param name="commandType">The Command type.</param>
        /// <param name="commandText">The Command text.</param>
        /// <param name="parameters">The Parameters passed to the query.</param>
        /// <returns>
        /// The Scalar Value as a generic.
        /// </returns>
        protected T? ExecuteScalar<T>(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            SqlCommand command = null!;

            try
            {
                command = this.CreateSqlCommand(connection, commandType, commandText, parameters);
                return (T)command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(), parameters, sqlException);
                }
                else
                {
                    throw new DataAccessException(ex.Message, ex);
                }
            }
            finally
            {
                this.DisposeCommand(command);
            }
        }

        /// <summary>
        /// Executes a TSQL Statement against an Open Connection and returns a Scalar Result Asynchronously.
        /// </summary>
        /// <param name="connection">The Database Connection.</param>
        /// <param name="commandType">The Command type.</param>
        /// <param name="commandText">The Command Text.</param>
        /// <param name="parameters">The Parameters to be added (Optional).</param>
        /// <returns>
        /// The Scalar Value as a generic.
        /// </returns>
        protected async Task<T?> ExecuteScalarAsync<T>(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] parameters)
        {
            SqlCommand command = null!;

            try
            {
                command = this.CreateSqlCommand(connection, commandType, commandText, parameters);
                return (T?)await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw new SqlServerDataAccessException(sqlException.Message, commandText, commandType.ToString(), parameters, sqlException);
                }
                else
                {
                    throw new DataAccessException(ex.Message, ex);
                }
            }
            finally
            {
                await this.DisposeCommandAsync(command).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Opens a Database Connection.
        /// </summary>
        /// <returns>
        /// The Database Connection Object.
        /// </returns>
        protected SqlConnection OpenConnection()
        {
            if (this.ConnectionString is null)
                throw new DataAccessException("The Connection String has not been specified.");

            return this.OpenConnection(this.ConnectionString);
        }
        
        /// <summary>
        /// Opens a Readonly database Connection.
        /// </summary>
        /// <returns>
        /// The Readonly Database Connection Object
        /// </returns>
        protected SqlConnection OpenReadonlyConnection()
        {
            if (this.ReadOnlyConnectionString is null)
                throw new DataAccessException("The Readonly Connection String has not been specified.");

            return this.OpenConnection(this.ReadOnlyConnectionString);
        }

        /// <summary>
        /// Opens a Database Connection with the given Connection String.
        /// </summary>
        /// <param name="connectionString">The Connection String.</param>
        /// <returns>
        /// The Database Connection Object.
        /// </returns>
        protected SqlConnection OpenConnection(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Opens a Database Connection Asynchronously.
        /// </summary>
        /// <returns>
        /// The Database Connection Object.
        /// </returns>
        protected async Task<SqlConnection> OpenConnectionAsync()
        {
            if (this.ConnectionString is null)
                throw new DataAccessException("The Connection String has not been specified.");

            return await this.OpenConnectionAsync(this.ConnectionString).ConfigureAwait(false);
        }

        /// <summary>
        /// Opens a Readonly Database Connection Asynchronously.
        /// </summary>
        /// <returns>
        /// The Database Connection Object.
        /// </returns>
        protected async Task<SqlConnection> OpenReadonlyConnectionAsync()
        {
            if (this.ReadOnlyConnectionString is null)
                throw new DataAccessException("The Readonly Connection String has not been specified.");
                
            return await this.OpenConnectionAsync(this.ReadOnlyConnectionString).ConfigureAwait(false);
        }

        /// <summary>
        /// Opens a Database Connection Asynchronously with the given Connection String.
        /// </summary>
        /// <param name="connectionString">The Connection String.</param>
        /// <returns>
        /// The Database Connection Object. 
        /// </returns>
        protected async Task<SqlConnection> OpenConnectionAsync(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        /// <summary>
        /// Releases unmanaged and optionally the managed resources.
        /// </summary>
        /// <param name="disposing">if true, releases both managed and unmanaged resources; otherwise releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Closes the connection if it is present and open.
                    if (this.connection != null && this.connection.State == ConnectionState.Open)
                    {
                        this.connection.Close();
                    }
                }
            }
            catch (SqlException)
            {
            }
            finally
            {
                this.connection = null;
            }
        }

        /// <summary>
        /// Creates a Sql command with the given parameters.
        /// </summary>
        /// <param name="connection">The Sql Connection.</param>
        /// <param name="commandType">The Command Type.</param>
        /// <param name="commandText">The Command Text.</param>
        /// <param name="sqlParameters">The Sql Parameters to add.</param>
        /// <returns></returns>
        private SqlCommand CreateSqlCommand(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] sqlParameters)
        {

            SqlCommand sqlCommand = new SqlCommand(commandText, connection);
            sqlCommand.CommandType = commandType;
            foreach (SqlParameter param in sqlParameters)
            {
                if ((param.Direction == (ParameterDirection)3 || param.Direction == (ParameterDirection)1) && param.Value == null)
                {
                    param.Value = DBNull.Value;
                }

                sqlCommand.Parameters.Add(param);
            }

            return sqlCommand;
        }

        /// <summary>
        /// Disposes the SqlCommand provided Command.
        /// </summary>
        /// <param name="sqlCommand">The Sql Command.</param>
        private void DisposeCommand(SqlCommand sqlCommand)
        {
            if (sqlCommand != null)
                sqlCommand?.Dispose();
        }

        /// <summary>
        /// Disposes the provided SqlCommand asynchronously.
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <returns></returns>
        private async Task DisposeCommandAsync(SqlCommand sqlCommand)
        {
            if (sqlCommand != null)
                await sqlCommand.DisposeAsync().ConfigureAwait(false);
        }
    }
}

