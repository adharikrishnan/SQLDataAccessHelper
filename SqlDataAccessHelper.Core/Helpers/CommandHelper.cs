// "<copyright file="CommandHelper.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"

namespace SqlDataAccessHelper.Core.Helpers;

using System.Data;
using System.Data.Common;

/// <summary>
/// Helper class to Standardise Sql Command creation generically.
/// </summary>
public class CommandHelper<TConnection, TCommand, TParameter>
    where TConnection : DbConnection
    where TCommand : DbCommand, new()
    where TParameter : DbParameter
{
    /// <summary>
    /// Creates a Sql command with the given parameters.
    /// </summary>
    /// <param name="connection">The Sql Connection.</param>
    /// <param name="commandType">The Command Type.</param>
    /// <param name="commandText">The Command Text.</param>
    /// <param name="sqlParameters">The Sql Parameters to add.</param>
    /// <returns></returns>
    public TCommand CreateSqlCommand(TConnection connection,
        CommandType commandType, string commandText, params TParameter[] sqlParameters)
    {
        TCommand sqlCommand = new TCommand()
        {
            CommandText = commandText,
            CommandType = commandType,
            Connection = connection
        };
        foreach (TParameter param in sqlParameters)
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