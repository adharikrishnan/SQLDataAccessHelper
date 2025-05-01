using System.Data;
using Microsoft.Data.SqlClient;

namespace SQLDataAccessHelper.SQLServer.Common;

/// <summary>
/// Helper Class for Common Methods
/// </summary>
public static class SqlServerHelpers
{
    /// <summary>
    /// Creates a Sql command with the given parameters.
    /// </summary>
    /// <param name="connection">The Sql Connection.</param>
    /// <param name="commandType">The Command Type.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="sqlParameters">The Sql Parameters to add.</param>
    /// <returns></returns>
    public static SqlCommand CreateSqlCommand(SqlConnection connection, CommandType commandType, string commandText,
        params SqlParameter[] sqlParameters)
    {
        SqlCommand sqlCommand = new SqlCommand(commandText, connection);
        sqlCommand.CommandType = commandType;
        foreach (SqlParameter param in sqlParameters)
        {
            if (param.Direction is (ParameterDirection)3 or (ParameterDirection)1 &&
                param.Value is null)
            {
                param.Value = DBNull.Value;
            }

            sqlCommand.Parameters.Add(param);
        }

        return sqlCommand;
    }
}