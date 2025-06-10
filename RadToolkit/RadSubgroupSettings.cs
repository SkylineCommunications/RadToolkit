using Skyline.DataMiner.Analytics.DataTypes;
using System;
using System.Collections.Generic;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    /// <summary>
    /// Represents a parameter in a RAD subgroup.
    /// </summary>
    public class RadParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadParameter"/> class.
        /// </summary>
        /// <param name="key">The key of the parameter.</param>
        /// <param name="label">The label of the parameter.</param>
        public RadParameter(ParameterKey key, string label)
        {
            Key = key;
            Label = label;
        }

        /// <summary>
        /// Gets or sets the key of the parameter.
        /// </summary>
        public ParameterKey Key { get; set; }

        /// <summary>
        /// Gets or sets the label of the parameter.
        /// </summary>
        public string Label { get; set; }
    }

    /// <summary>
    /// Contains information about a RAD subgroup, including monitoring state.
    /// </summary>
    public class RadSubgroupInfo : RadSubgroupSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadSubgroupInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the subgroup.</param>
        /// <param name="id">The unique identifier of the subgroup.</param>
        /// <param name="parameters">The parameters in the subgroup.</param>
        /// <param name="options">The options for the subgroup.</param>
        /// <param name="isMonitored">Indicates whether the subgroup is monitored.</param>
        public RadSubgroupInfo(string name, Guid id, List<RadParameter> parameters, RadSubgroupOptions options, bool isMonitored)
            : base(name, id, parameters, options)
        {
            IsMonitored = isMonitored;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the subgroup is monitored.
        /// </summary>
        public bool IsMonitored { get; set; }
    }

    /// <summary>
    /// Represents the settings for a RAD subgroup.
    /// </summary>
    public class RadSubgroupSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadSubgroupSettings"/> class.
        /// </summary>
        /// <param name="name">The name of the subgroup.</param>
        /// <param name="id">The unique identifier of the subgroup.</param>
        /// <param name="parameters">The parameters in the subgroup.</param>
        /// <param name="options">The options for the subgroup.</param>
        public RadSubgroupSettings(string name, Guid id, List<RadParameter> parameters, RadSubgroupOptions options)
        {
            Name = name;
            ID = id;
            Parameters = parameters;
            Options = options;
        }

        /// <summary>
        /// Gets or sets the name of the subgroup.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ID of the subgroup.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Gets or sets the parameters in the subgroup.
        /// </summary>
        public List<RadParameter> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the options for the subgroup.
        /// </summary>
        public RadSubgroupOptions Options { get; set; }

        /// <summary>
        /// Gets the name of the subgroup, or the parent name if the subgroup name is not set.
        /// </summary>
        /// <param name="parentName">The name of the parent group.</param>
        /// <returns>The name of the subgroup, or the parent name if the subgroup name is null or empty.</returns>
        public string GetName(string parentName)
        {
            return string.IsNullOrEmpty(Name) ? parentName : Name;
        }
    }
}
