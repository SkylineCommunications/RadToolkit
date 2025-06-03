using Skyline.DataMiner.Automation;
using System;
using System.Runtime.CompilerServices;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    public static class IEngineExtensions
    {
        public static RadHelper GetRadHelper(this IEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine), "Engine cannot be null.");
            }

            return new RadHelper(Engine.SLNetRaw, new Logger(engine));
        }
    }
}