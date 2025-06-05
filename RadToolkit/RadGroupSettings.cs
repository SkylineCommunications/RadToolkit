using System.Collections.Generic;
using Skyline.DataMiner.Analytics.DataTypes;
using Skyline.DataMiner.Net.Database;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class RadGroupInfo : ARadGroupSettings<RadSubgroupInfo>
    {
        public RadGroupInfo(string groupName, RadGroupOptions options, List<RadSubgroupInfo> subgroups)
            : base(groupName, options, subgroups)
        {
        }
    }

    public class RadGroupSettings : ARadGroupSettings<RadSubgroupSettings>
    {
        public RadGroupSettings(string groupName, RadGroupOptions options, List<RadSubgroupSettings> subgroups)
            : base(groupName, options, subgroups)
        {
        }
    }

    /// <summary>
    /// Represents the settings for a (non-shared model) RAD group.
    /// </summary>
    public abstract class ARadGroupSettings<T> where T: RadSubgroupSettings
    {
        protected ARadGroupSettings(string groupName, RadGroupOptions options, List<T> subgroups)
        {
            GroupName = groupName;
            Options = options;
            Subgroups = subgroups;
        }

        /// <summary>
        /// Gets or sets the name of the RAD group.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the options for the RAD group.
        /// </summary>
        public RadGroupOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the parameter subgroups in the RAD group.
        /// </summary>
        public List<T> Subgroups { get; set; }
    }
}
