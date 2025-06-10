using System;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    /// <summary>
    /// Provides logging functionality for error messages.
    /// </summary>
    public class Logger
    {
        private readonly Action<string> _errorLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="errorLogger">
        /// The delegate to handle error log messages.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="errorLogger"/> is <c>null</c>.
        /// </exception>
        public Logger(Action<string> errorLogger)
        {
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">
        /// The error message to log.
        /// </param>
        public void Error(string message)
        {
            _errorLogger(message);
        }
    }
}
