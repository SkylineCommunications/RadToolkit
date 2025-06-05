namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class RadGroupOptions : RadGroupBaseOptions
    {
        public RadGroupOptions(bool updateModel, double? anomalyThreshold = null, int? minimalDuration = null)
            : base(anomalyThreshold, minimalDuration)
        {
            UpdateModel = updateModel;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to update the model on new data.
        /// </summary>
        public bool UpdateModel { get; set; }

        /// <summary>
        /// Gets the anomaly threshold set in the options, or the default value if none was set.
        /// </summary>
        /// <returns>The anomaly threshold.</returns>
        public double GetAnomalyThresholdOrDefault()
        {
            return AnomalyThreshold ?? DefaultAnomalyThreshold;
        }

        /// <summary>
        /// Gets the minimal anomaly duration set in the options, or the default value if none was set.
        /// </summary>
        /// <returns>The minimal duration.</returns>
        public int GetMinimalDurationOrDefault()
        {
            return MinimalDuration ?? DefaultMinimalDuration;
        }
    }
}
