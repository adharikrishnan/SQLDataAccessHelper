// "<copyright file="SqlServerDataAccessBase.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SQLDataAccess.SQLServer
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Base class for the data access classes which initiates the Database connections.
    /// </summary>
    public class SqlServerDataAccessBase : IDisposable
    {
        /// <summary>
        /// Holds the SQL connection object.
        /// </summary>
        private SqlConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataAccessBase"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        public SqlServerDataAccessBase(IConfiguration configuration)
        {
            
        }

        /// <summary>
        /// Gets the Read and Write  Database Connection string.
        /// </summary>
        /// <value>
        /// The Read and Write Database Connection string.
        /// </value>
        protected virtual string ReadAndWriteConnectionString { get; }

        /// <summary>
        /// Gets the Readonly Database Connection string.
        /// </summary>
        /// <value>
        /// The Readonly Database Connection string.
        /// </value>
        protected virtual string ReadOnlyConnectionString { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Method which executes the SQL command to retrieve data from database.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <param name="parameters">Parameters passed to function.</param>
        /// <returns>
        /// return the SQL Data Reader object.
        /// </returns>
        protected static SqlDataReader ExecuteReader(SqlConnection conn, CommandType type, string cmdText, params SqlParameter[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                command = new SqlCommand(cmdText, conn);
                command.CommandType = type;
                foreach (SqlParameter param in parameters)
                {
                    if ((param.Direction == (ParameterDirection)3 || param.Direction == (ParameterDirection)1) && param.Value == null)
                    {
                        param.Value = DBNull.Value;
                    }

                    command.Parameters.Add(param);
                }

                reader = command.ExecuteReader();
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, command.CommandText, parameters);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return reader;
        }

        /// <summary>
        /// Method which executes the SQL command to retrieve data from database.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <param name="parameters">Parameters passed to function.</param>
        /// <returns>
        /// return the SQL Data Reader object.
        /// </returns>
        protected static async Task<SqlDataReader> ExecuteReaderAsync(SqlConnection conn, CommandType type, string cmdText, params SqlParameter[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                command = new SqlCommand(cmdText, conn);
                command.CommandType = type;
                foreach (SqlParameter param in parameters)
                {
                    if ((param.Direction == (ParameterDirection)3 || param.Direction == (ParameterDirection)1) && param.Value == null)
                    {
                        param.Value = DBNull.Value;
                    }

                    command.Parameters.Add(param);
                }

                reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, command.CommandText, parameters);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return reader;
        }

        /// <summary>
        /// Method which executes the SQL command to retrieve data from database.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <returns>
        /// return the SQL Data Reader object.
        /// </returns>
        protected static SqlDataReader ExecuteReader(SqlConnection conn, CommandType type, string cmdText)
        {
            SqlCommand command = new SqlCommand(cmdText, conn);
            command.CommandType = type;
            SqlDataReader reader = null;

            try
            {
                reader = command.ExecuteReader();
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, null);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return reader;
        }

        /// <summary>
        /// Method which executes the SQL command to retrieve data from database.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <returns>
        /// return the SQL Data Reader object.
        /// </returns>
        protected static async Task<SqlDataReader> ExecuteReaderAsync(SqlConnection conn, CommandType type, string cmdText)
        {
            SqlCommand command = new SqlCommand(cmdText, conn);
            command.CommandType = type;
            SqlDataReader reader = null;

            try
            {
                reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, null);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return reader;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <param name="parameters">Parameters passed to the query.</param>
        /// <returns>
        /// Number of rows affected.
        /// </returns>
        protected static int ExecuteNonQuery(SqlConnection conn, CommandType type, string cmdText, params SqlParameter[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            SqlCommand command = null;
            int rowsAffected = 0;

            try
            {
                command = new SqlCommand(cmdText, conn);
                command.CommandType = type;
                foreach (SqlParameter param in parameters)
                {
                    command.Parameters.Add(param);
                }

                rowsAffected = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, command.CommandText, parameters);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <param name="parameters">Parameters passed to the query.</param>
        /// <returns>
        /// Number of rows affected.
        /// </returns>
        protected static async Task<int> ExecuteNonQueryAsync(SqlConnection conn, CommandType type, string cmdText, params SqlParameter[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            SqlCommand command = null;
            int rowsAffected = 0;

            try
            {
                command = new SqlCommand(cmdText, conn);
                command.CommandType = type;
                foreach (SqlParameter param in parameters)
                {
                    command.Parameters.Add(param);
                }

                rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, command.CommandText, parameters);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the scalar value.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <returns>
        /// The scalar value.
        /// </returns>
        protected static object ExecuteScalar(SqlConnection conn, CommandType type, string cmdText)
        {
            SqlCommand command = null;
            object returnValue = null;

            try
            {
                command = new SqlCommand(cmdText, conn);
                command.CommandType = type;

                returnValue = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, command.CommandText);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return returnValue;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the scalar value.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <returns>
        /// The scalar value.
        /// </returns>
        protected static async Task<object> ExecuteScalarAsync(SqlConnection conn, CommandType type, string cmdText)
        {
            SqlCommand command = null;
            object returnValue = null;

            try
            {
                command = new SqlCommand(cmdText, conn);
                command.CommandType = type;

                returnValue = await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, command.CommandText);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return returnValue;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the scalar value.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <param name="parameters">Parameters passed to the query.</param>
        /// <returns>
        /// The scalar value.
        /// </returns>
        protected static object ExecuteScalar(SqlConnection conn, CommandType type, string cmdText, params SqlParameter[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            SqlCommand command = null;
            object returnValue = null;

            try
            {
                command = new SqlCommand(cmdText, conn);
                command.CommandType = type;
                foreach (SqlParameter param in parameters)
                {
                    command.Parameters.Add(param);
                }

                returnValue = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, command.CommandText, parameters);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return returnValue;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the scalar value.
        /// </summary>
        /// <param name="conn">Connection to database.</param>
        /// <param name="type">Command type.</param>
        /// <param name="cmdText">Command text.</param>
        /// <param name="parameters">Parameters passed to the query.</param>
        /// <returns>
        /// The scalar value.
        /// </returns>
        protected static async Task<object> ExecuteScalarAsync(SqlConnection conn, CommandType type, string cmdText, params SqlParameter[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            SqlCommand command = null;
            object returnValue = null;

            try
            {
                command = new SqlCommand(cmdText, conn);
                command.CommandType = type;
                foreach (SqlParameter param in parameters)
                {
                    command.Parameters.Add(param);
                }

                returnValue = await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlException)
                {
                    throw CreateCustomSqlException(sqlException, command.CommandText, parameters);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
            }

            return returnValue;
        }

        /// <summary>
        /// Creates a database connection.
        /// </summary>
        /// <returns>
        /// Connection to database.
        /// </returns>
        protected SqlConnection OpenConnection()
        {
            return this.OpenConnection(this.ConnectionString);
        }

        /// <summary>
        /// Creates a database connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>
        /// Connection to database.
        /// </returns>
        protected SqlConnection OpenConnection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Creates a database connection.
        /// </summary>
        /// <returns>
        /// Connection to database.
        /// </returns>
        protected async Task<SqlConnection> OpenConnectionAsync()
        {
            return await this.OpenConnectionAsync(this.ConnectionString).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a database connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>
        /// Connection to database.
        /// </returns>
        protected async Task<SqlConnection> OpenConnectionAsync(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">if true, releases both managed and unmanaged resources; otherwise releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // dispose managed resources
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
        /// Creating custom exception for SQL Exceptions.
        /// </summary>
        /// <param name="sqlException">SQL exception.</param>
        /// <param name="commandText">Command text.</param>
        /// <param name="parameters">parameters of stored procedure.</param>
        /// <returns>Custom SQL exception.</returns>
        private static SqlServerDataAccessException CreateCustomSqlException(SqlException sqlException, string commandText, params SqlParameter[] parameters)
        {
            SqlServerDataAccessException customSqlException = null;
            customSqlException = new SqlServerDataAccessException(sqlException.Message)
            {
                Errors = sqlException.Errors,
                InnerException = sqlException.InnerException,
                StackTrace = sqlException.StackTrace,
                Number = sqlException.Number,
                SqlParams = parameters,
                CommandText = commandText,
            };
            return customSqlException;
        }
    }
}

