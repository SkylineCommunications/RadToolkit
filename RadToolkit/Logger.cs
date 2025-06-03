using Skyline.DataMiner.Automation;
using System;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class Logger
    {
        private readonly IEngine _engine;
        public Logger(IEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine), "Engine cannot be null.");
            }

            _engine = engine;
        }

        public void Log(string message, LogType type, int logLevel)
        {
            _engine.Log(message, type, logLevel);
        }
    }
}
