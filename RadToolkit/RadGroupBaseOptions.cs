namespace Skyline.DataMiner.Utils.RadToolkit
{
    /// <summary>
    /// Provides base options for configuring anomaly detection groups.
    /// </summary>
    public class RadGroupBaseOptions
    {
        /// <summary>
        /// The default threshold above which an anomaly will be generated.
        /// </summary>
        public const double DefaultAnomalyThreshold = 3.0;

        /// <summary>
        /// The default minimal duration (in minutes) the anomaly score should be above the threshold before a suggestion event is generated.
        /// </summary>
        public const int DefaultMinimalDuration = 5;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadGroupBaseOptions"/> class.
        /// </summary>
        /// <param name="anomalyThreshold">Threshold above which an anomaly will be generated. If <c>null</c>, the default threshold is used.</param>
        /// <param name="minimalDuration">Minimal duration (in minutes) the anomaly score should be above the threshold before a suggestion event is generated. If <c>null</c>, the default duration is used.</param>
        protected RadGroupBaseOptions(double? anomalyThreshold = null, int? minimalDuration = null)
        {
            AnomalyThreshold = anomalyThreshold;
            MinimalDuration = minimalDuration;
        }

        /// <summary>
        /// Gets or sets the threshold above which an anomaly will be generated. Leave empty to use the default threshold.
        /// </summary>
        public double? AnomalyThreshold { get; set; }

        /// <summary>
        /// Gets or sets the minimal duration (in minutes) the anomaly score should be above the threshold before a suggestion event is generated. Leave empty to use the default duration.
        /// </summary>
        public int? MinimalDuration { get; set; }
    }
}
