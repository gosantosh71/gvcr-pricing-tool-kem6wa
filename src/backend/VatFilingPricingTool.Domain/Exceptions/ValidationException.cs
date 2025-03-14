using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when domain validation rules are violated.
    /// </summary>
    [Serializable]
    public class ValidationException : DomainException
    {
        /// <summary>
        /// Gets the list of validation errors that describe why validation failed.
        /// </summary>
        public List<string> ValidationErrors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a default error message.
        /// </summary>
        public ValidationException() 
            : base("Validation failed", ErrorCodes.General.ValidationError)
        {
            ValidationErrors = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        public ValidationException(string message) 
            : base(message, ErrorCodes.General.ValidationError)
        {
            ValidationErrors = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with validation errors.
        /// </summary>
        /// <param name="validationErrors">The list of validation errors that describe why validation failed.</param>
        public ValidationException(List<string> validationErrors) 
            : base("Validation failed", ErrorCodes.General.ValidationError)
        {
            ValidationErrors = validationErrors ?? new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and validation errors.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        /// <param name="validationErrors">The list of validation errors that describe why validation failed.</param>
        public ValidationException(string message, List<string> validationErrors) 
            : base(message, ErrorCodes.General.ValidationError)
        {
            ValidationErrors = validationErrors ?? new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ValidationException(string message, Exception innerException) 
            : base(message, ErrorCodes.General.ValidationError, innerException)
        {
            ValidationErrors = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message,
        /// validation errors, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that describes the exception.</param>
        /// <param name="validationErrors">The list of validation errors that describe why validation failed.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ValidationException(string message, List<string> validationErrors, Exception innerException) 
            : base(message, ErrorCodes.General.ValidationError, innerException)
        {
            ValidationErrors = validationErrors ?? new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ValidationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            try
            {
                ValidationErrors = (List<string>)info.GetValue(nameof(ValidationErrors), typeof(List<string>));
            }
            catch
            {
                ValidationErrors = new List<string>();
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

            // Add the ValidationErrors to the serialization info
            info.AddValue(nameof(ValidationErrors), ValidationErrors);
            
            // Call the base implementation to handle standard exception serialization
            base.GetObjectData(info, context);
        }
    }
}