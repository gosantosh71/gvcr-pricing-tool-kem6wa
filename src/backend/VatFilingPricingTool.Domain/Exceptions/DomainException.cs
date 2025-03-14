using System;
using System.Runtime.Serialization;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Domain.Exceptions
{
    /// <summary>
    /// Base exception class for all domain-specific exceptions in the VAT Filing Pricing Tool application.
    /// Provides standardized error codes and consistent error handling across the application.
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        /// <summary>
        /// Gets the standardized error code associated with this exception.
        /// </summary>
        public string ErrorCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with a default error message and error code.
        /// </summary>
        public DomainException() 
            : base("Domain exception occurred")
        {
            ErrorCode = ErrorCodes.General.ServerError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        public DomainException(string message) 
            : base(message)
        {
            ErrorCode = ErrorCodes.General.ServerError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message and error code.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        /// <param name="errorCode">The standardized error code for this exception.</param>
        public DomainException(string message, string errorCode) 
            : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DomainException(string message, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = ErrorCodes.General.ServerError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message,
        /// error code, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        /// <param name="errorCode">The standardized error code for this exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DomainException(string message, string errorCode, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected DomainException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            // Retrieve the ErrorCode from the serialization info if available
            if (info != null)
            {
                try
                {
                    ErrorCode = info.GetString(nameof(ErrorCode));
                }
                catch
                {
                    ErrorCode = ErrorCodes.General.ServerError;
                }
            }
            else
            {
                ErrorCode = ErrorCodes.General.ServerError;
            }
        }

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The object that holds the serialized data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            // Add the ErrorCode to the serialization info
            info.AddValue(nameof(ErrorCode), ErrorCode);
            
            // Call the base implementation to handle standard exception serialization
            base.GetObjectData(info, context);
        }
    }
}