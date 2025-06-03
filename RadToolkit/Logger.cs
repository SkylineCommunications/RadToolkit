using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Automation;
using System;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class Logger
    {
        private readonly IEngine _engine;
        private readonly IGQILogger _gqiLogger;

        public Logger(IEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine), "Engine cannot be null.");
            }

            _engine = engine;
        }

        public Logger(IGQILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            }
            _gqiLogger = logger;
        }

        public void Error(string message)
        {
            if (_engine != null)
                _engine.Log(message, LogType.Error, 0);
            else if (_gqiLogger != null)
                _gqiLogger.Error(message);
        }
    }
}
