// "<copyright file="DataAccessException.cs">
// Copyright (c) Advaith Harikrishnan. All rights reserved.
// </copyright>"


namespace SQLDataAccessHelper.Common.Exceptions
{
    public class DataAccessException : Exception
    {
        /// <summary>
        /// The Command Text.
        /// </summary>
        public string? CommandText { get; }

        /// <summary>
        /// The Command Type.
        /// </summary>
        public string? CommandType { get; }

        /// <summary>
        /// Creates an instance of the DataAccessException Class.
        /// </summary>
        public DataAccessException()
            : base()
        {
        }

        /// <summary>
        /// Creates an instance of the DataAccessException Class.
        /// </summary>
        /// <param name="message">The Exception Messsage.</param>
        public DataAccessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the DataAccessException Class.
        /// </summary>
        /// <param name="message">The Exception Messsage.</param>
        /// <param name="innerException">The Inner Exception.</param>
        public DataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new Instance of the DataAccessException Class. 
        /// </summary>
        /// <param name="message">The Exception Messsage.</param>
        /// <param name="commandText">The Command Text.</param>
        /// <param name="commandType">The Command Type.</param>
        public DataAccessException(string message, string commandText, string commandType)
            : base(message)
        {
            this.CommandText = commandText;
            this.CommandType = commandType;
        }

        /// <summary>
        /// Creates a new Instance of the DataAccessException Class. 
        /// </summary>
        /// <param name="message">The Exception Messsage.</param>
        /// <param name="innerException">The Inner Exception.</param>
        /// <param name="commandText">The Command Text.</param>
        /// <param name="commandType">The Command Type.</param>
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