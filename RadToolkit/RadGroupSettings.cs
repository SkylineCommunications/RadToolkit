using System.Collections.Generic;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    /// <summary>
    /// Represents setting for a RAD group and its subgroups, including whether the group is currently actively monitored. See also <seealso cref="RadGroupSettings"/>.
    /// </summary>
    public class RadGroupInfo : ARadGroupSettings<RadSubgroupInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadGroupInfo"/> class.
        /// </summary>
        /// <param name="groupName">The name of the RAD group.</param>
        /// <param name="options">The options for the RAD group.</param>
        /// <param name="subgroups">The list of subgroups in the RAD group.</param>
        public RadGroupInfo(string groupName, RadGroupOptions options, List<RadSubgroupInfo> subgroups)
            : base(groupName, options, subgroups)
        {
        }
    }

    /// <summary>
    /// Represents the settings for a RAD group and its subgroups. See also <seealso cref="RadGroupInfo"/> if information about the current state of the group should be included.
    /// </summary>
    public class RadGroupSettings : ARadGroupSettings<RadSubgroupSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadGroupSettings"/> class.
        /// </summary>
        /// <param name="groupName">The name of the RAD group.</param>
        /// <param name="options">The options for the RAD group.</param>
        /// <param name="subgroups">The list of subgroups in the RAD group.</param>
        public RadGroupSettings(string groupName, RadGroupOptions options, List<RadSubgroupSettings> subgroups)
            : base(groupName, options, subgroups)
        {
        }
    }

    /// <summary>
    /// An abstract base class for RAD group settings. This is the base class for both <see cref="RadGroupSettings"/> and <see cref="RadGroupInfo"/>.
    /// </summary>
    /// <typeparam name="T">The type of the subgroup settings.</typeparam>
    public abstract class ARadGroupSettings<T> where T : RadSubgroupSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ARadGroupSettings{T}"/> class.
        /// </summary>
        /// <param name="groupName">The name of the RAD group.</param>
        /// <param name="options">The options for the RAD group.</param>
        /// <param name="subgroups">The list of subgroups in the RAD group.</param>
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
