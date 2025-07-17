namespace Skyline.DataMiner.Utils.RadToolkit
{
    /// <summary>
    /// Provides options for configuring anomaly detection groups, including model update behavior.
    /// </summary>
    public class RadGroupOptions : RadGroupBaseOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadGroupOptions"/> class.
        /// </summary>
        /// <param name="updateModel">Indicates whether to update the model on new data.</param>
        /// <param name="anomalyThreshold">Threshold above which an anomaly will be generated. If <c>null</c>, the default threshold is used.</param>
        /// <param name="minimalDuration">Minimal duration (in minutes) the anomaly score should be above the threshold before a suggestion event is generated. If <c>null</c>, the default duration is used.</param>
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
        /// <param name="helper">The RadHelper instance.</param>
        /// <returns>The anomaly threshold.</returns>
        public double GetAnomalyThresholdOrDefault(RadHelper helper)
        {
            if (helper == null)
            {
                throw new System.ArgumentNullException(nameof(helper), "RadHelper cannot be null.");
            }
            return AnomalyThreshold ?? helper.DefaultAnomalyThreshold;
        }

        /// <summary>
        /// Gets the minimal anomaly duration set in the options, or the default value if none was set.
        /// </summary>
        /// <param name="helper">The RadHelper instance.</param>
        /// <returns>The minimal duration.</returns>
        public int GetMinimalDurationOrDefault(RadHelper helper)
        {
            if (helper == null)
            {
                throw new System.ArgumentNullException(nameof(helper), "RadHelper cannot be null.");
            }
            return MinimalDuration ?? helper.DefaultMinimumAnomalyDuration;
        }
    }
}
