// "<copyright file="DataAccessException.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"


namespace SQLDataAccess.Common.Exceptions
{
    public class DataAccessException : Exception
    {
        /// <summary>
        /// The Command Text.
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// The Command Type.
        /// </summary>
        public string CommandType { get; }

        public DataAccessException(string message, string commandText, string commandType)
            : base(message)
        {
            this.CommandText = commandText;
            this.CommandType = commandType;
        }

        public DataAccessException(
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

    }
}