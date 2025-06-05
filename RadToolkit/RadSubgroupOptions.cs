namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class RadSubgroupOptions : RadGroupBaseOptions
    {
        public RadSubgroupOptions(double? anomalyThreshold = null, int? minimalDuration = null)
            : base(anomalyThreshold, minimalDuration)
        {
        }

        /// <summary>
        /// Gets the anomaly threshold set in the options, or the parent anomaly threshold if none was set, or the default value if also that one was not set.
        /// </summary>
        /// <param name="parentAnomalyThreshold">The parent anomaly threshold.</param>
        /// <returns>The anomaly threshold.</returns>
        public double GetAnomalyThresholdOrDefault(double? parentAnomalyThreshold)
        {
            return AnomalyThreshold ?? parentAnomalyThreshold ?? DefaultAnomalyThreshold;
        }

        /// <summary>
        /// Gets the minimal anomaly duration set in the options, or the parent minimal duration if none was set,
        /// or the default value if also that one was not set.
        /// </summary>
        /// <param name="parentMinimalDuration">The parent minimal anomaly duration.</param>
        /// <returns>The minimal anomaly duration.</returns>
        public int GetMinimalDurationOrDefault(int? parentMinimalDuration)
        {
            return MinimalDuration ?? parentMinimalDuration ?? DefaultMinimalDuration;
        }
    }
}
