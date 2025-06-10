using System;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class Logger
    {
        private readonly Action<string> _errorLogger;

        public Logger(Action<string> errorLogger)
        {
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
        }

        public void Error(string message)
        {
            _errorLogger(message);
        }
    }
}
