using System;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    /// <summary>
    /// Options for RAD subgroups.
    /// </summary>
    public class RadSubgroupOptions : RadGroupBaseOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadSubgroupOptions"/> class.
        /// </summary>
        /// <param name="anomalyThreshold">
        /// Threshold above which an anomaly will be generated for this subgroup.
        /// If <c>null</c>, the parent or default threshold will be used.
        /// </param>
        /// <param name="minimalDuration">
        /// Minimal duration (in minutes) the anomaly score should be above the threshold before a suggestion event is generated for this subgroup.
        /// If <c>null</c>, the parent or default duration will be used.
        /// </param>
        public RadSubgroupOptions(double? anomalyThreshold = null, int? minimalDuration = null)
            : base(anomalyThreshold, minimalDuration)
        {
        }

        /// <summary>
        /// Gets the anomaly threshold set in the options, or the parent anomaly threshold if none was set, or the default value if also that one was not set.
        /// </summary>
        /// <param name="helper">The RadHelper instance.</param>
        /// <param name="parentAnomalyThreshold">The parent anomaly threshold.</param>
        /// <returns>The anomaly threshold.</returns>
        public double GetAnomalyThresholdOrDefault(RadHelper helper, double? parentAnomalyThreshold)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper), "RadHelper cannot be null.");
            }
            return AnomalyThreshold ?? parentAnomalyThreshold ?? helper.DefaultAnomalyThreshold;
        }

        /// <summary>
        /// Gets the minimal anomaly duration set in the options, or the parent minimal duration if none was set,
        /// or the default value if also that one was not set.
        /// </summary>
        /// <param name="helper">The RadHelper instance.</param>
        /// <param name="parentMinimalDuration">The parent minimal anomaly duration.</param>
        /// <returns>The minimal anomaly duration.</returns>
        public int GetMinimalDurationOrDefault(RadHelper helper, int? parentMinimalDuration)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper), "RadHelper cannot be null.");
            }
            return MinimalDuration ?? parentMinimalDuration ?? helper.DefaultMinimumAnomalyDuration;
        }
    }
}
