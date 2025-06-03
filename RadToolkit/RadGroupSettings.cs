using System.Collections.Generic;
using Skyline.DataMiner.Analytics.DataTypes;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class RadGroupInfo : RadGroupSettings, IRadGroupBaseInfo
    {
        public bool IsMonitored { get; set; } = true;
    }

    /// <summary>
    /// Represents the settings for a (non-shared model) RAD group.
    /// </summary>
    public class RadGroupSettings : RadGroupBaseSettings
    {
        /// <summary>
        /// Gets or sets the parameters in the RAD group.
        /// </summary>
        public IEnumerable<ParameterKey> Parameters { get; set; }
    }
}
