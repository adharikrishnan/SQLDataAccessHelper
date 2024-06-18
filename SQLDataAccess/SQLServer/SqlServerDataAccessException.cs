

namespace SQLDataAccess.SQLServer
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Class for Custom SQL Exception.
    /// </summary>
    [Serializable]
    public class SqlServerDataAccessException : Exception
    {
        /// <summary>
        /// SP name.
        /// </summary>
        private SqlErrorCollection errors;

        /// <summary>
        /// SQL parameters.
        /// </summary>
        private SqlParameter[] sqlparams;

        /// <summary>
        /// Command text string.
        /// </summary>
        private string commandText;


        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataAccessException"/> class.
        /// </summary>
        public SqlServerDataAccessException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataAccessException"/> class.
        /// </summary>
        /// <param name="message">This is the description of the exception</param>
        public SqlServerDataAccessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataAccessException"/> class.
        /// </summary>
        /// <param name="message">This is the description of the exception</param>
        /// <param name="innerException">Inner exception</param>
        public SqlServerDataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataAccessException"/> class.
        /// </summary>
        /// <param name="message">This is the description of the exception</param>
        /// <param name="innerException">Inner exception</param>
        public SqlServerDataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets or sets Errors.
        /// </summary>
        /// <value>
        /// SQL error collection
        /// </value>
        public SqlErrorCollection Errors
        {
            get
            {
                return this.errors;
            }

            set
            {
                this.errors = value;
            }
        }

        /// <summary>
        /// Gets or sets SQL parameters
        /// </summary>
        /// <value>
        /// SQL parameters
        /// </value>
        public SqlParameter[] SqlParams
        {
            get
            {
                return this.sqlparams;
            }

            set
            {
                this.sqlparams = value;
            }
        }

        /// <summary>
        /// Gets sQL parameter list
        /// </summary>
        /// <value>
        /// SQL parameter list as string
        /// </value>
        public string ParameterList
        {
            get
            {
                string parameters = string.Empty;
                foreach (SqlParameter parameter in this.SqlParams)
                {
                    parameters += string.Join(parameter.ParameterName, parameter.Value.ToString());
                }

                return parameters;
            }
        }

        /// <summary>
        /// Gets or sets Command text
        /// </summary>
        /// <value>
        /// Command text
        /// </value>
        public string CommandText
        {
            get
            {
                return this.commandText;
            }

            set
            {
                this.commandText = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether data conflict exists or not.
        /// </summary>
        /// <value> Message. </value>
        public bool DataConflict
        {
            get { return Message == "Message"; }
        }

        /// <summary>
        /// sets the System.Runtime.Serialization.SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">Instance of SerializationInfo</param>
        /// <param name="context">Instance of StreamingContext</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.GetObjectData(info, context);
        }
    }
}
