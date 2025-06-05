namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class RadGroupBaseOptions
    {
        public const double DefaultAnomalyThreshold = 3.0;
        public const int DefaultMinimalDuration = 5;

        protected RadGroupBaseOptions(double? anomalyThreshold = null, int? minimalDuration = null)
        {
            AnomalyThreshold = anomalyThreshold;
            MinimalDuration = minimalDuration;
        }

        /// <summary>
        /// Gets or sets threshold above which an anomaly will be generated. Leave empty to use the default threshold.
        /// </summary>
        public double? AnomalyThreshold { get; set; }

        /// <summary>
        /// Gets or sets the minimal duration (in minutes) the anomaly score should be above the threshold before a suggestion event is generated. Leave empty to use the default duration.
        /// </summary>
        public int? MinimalDuration { get; set; }
    }
}
