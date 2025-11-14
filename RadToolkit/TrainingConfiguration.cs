namespace Skyline.DataMiner.Utils.RadToolkit
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents configuration settings for training relational anomaly models.
    /// </summary>
    public class TrainingConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrainingConfiguration"/> class.
        /// </summary>
        /// <param name="timeRanges">The time ranges to be used for training the model.</param>
        /// <param name="excludedSubgroups">The subgroups whose data should be excluded from training.</param>
        public TrainingConfiguration(List<TimeRange> timeRanges, List<int> excludedSubgroups)
        {
            TimeRanges = timeRanges ?? new List<TimeRange>();
            ExcludedSubgroups = excludedSubgroups ?? new List<int>();
        }

        /// <summary>
        /// The time ranges to be used for training the model.
        /// </summary>
        public List<TimeRange> TimeRanges { get; set; }

        /// <summary>
        /// The subgroups whose data should be excluded from training, identified by their subgroup index in the <seealso cref="Skyline.DataMiner.Analytics.Rad.AddRADParameterGroupMessage"/>.
        /// </summary>
        public List<int> ExcludedSubgroups { get; set; }
    }
}
