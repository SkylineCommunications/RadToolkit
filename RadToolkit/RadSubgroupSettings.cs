using Skyline.DataMiner.Analytics.DataTypes;
using System;
using System.Collections.Generic;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class RadParameter
    {
        public RadParameter(ParameterKey key, string label)
        {
            Key = key;
            Label = label;
        }

        public ParameterKey Key { get; set; }

        public string Label { get; set; }
    }

    public class RadSubgroupInfo : RadSubgroupSettings
    {
        public RadSubgroupInfo(string name, Guid id, List<RadParameter> parameters, RadSubgroupOptions options, bool isMonitored)
            : base(name, id, parameters, options)
        {
            IsMonitored = isMonitored;
        }

        public bool IsMonitored { get; set; }
    }

    public class RadSubgroupSettings
    {
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

        public string GetName(string parentName)
        {
            return string.IsNullOrEmpty(Name) ? parentName : Name;
        }
    }
}
