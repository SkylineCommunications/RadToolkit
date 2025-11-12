namespace Skyline.DataMiner.Utils.RadToolkit
{
    using Skyline.DataMiner.Analytics.DataTypes;
    using System;

    /// <summary>
    /// An anomaly detected by the Relational Anomaly Detection (RAD) engine.
    /// </summary>
    public class RelationalAnomaly
    {
        /// <summary>
        /// Identifier of the anomaly. Each parameter involved in the anomaly gets its own record, but they all share the same AnomalyID.
        /// </summary>
        public Guid AnomalyID { get; set; }

        /// <summary>
        /// Parameter instance on which the relational anomaly was detected.
        /// </summary>
        public ParameterKey ParameterKey { get; set; }

        /// <summary>
        /// The time point where the first anomalous values were detected.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The last registered time point where the anomalous values were still confirmed
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Name of the Relational Anomaly Detection group that detected the anomaly.
        ///</summary>
        public string GroupName { get; set; }

        /// <summary>
        /// If specified (i.e. not null or empty), the name of the subgroup that detected the anomaly. 
        /// </summary>
        public string SubgroupName { get; set; }

        /// <summary>
        /// Score indicating the severity of the anomaly. The higher the score, the more severe the anomaly.
        /// </summary>
        public double AnomalyScore { get; set; }

        /// <summary>
        /// The unique identifier of the subgroup that detected the anomaly.
        /// </summary>
        public Guid SubgroupID { get; set; }

        /// <summary>
        /// Construct a new instance of the <see cref="RelationalAnomaly"/> class.
        /// </summary>
        /// <param name="anomalyID">The anomaly ID.</param>
        /// <param name="parameterKey">The parameter on which the anomaly was detected.</param>
        /// <param name="startTime">The start time of the anomaly.</param>
        /// <param name="endTime">The end time of the anomaly.</param>
        /// <param name="groupName">The name of the group on which the anomaly was detected.</param>
        /// <param name="subgroupName">The name of the subgroup on which the anomaly was detected (or null if the subgroup has no name).</param>
        /// <param name="subgroupID">The unique identifier of the subgroup that detected the anomaly.</param>
        /// <param name="anomalyScore">The anomaly score.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameterKey"/> or <paramref name="groupName"/> is null.</exception>
        public RelationalAnomaly(Guid anomalyID, ParameterKey parameterKey, DateTime startTime, DateTime endTime, string groupName, string subgroupName,
            Guid subgroupID, double anomalyScore)
        {
            AnomalyID = anomalyID;
            ParameterKey = parameterKey ?? throw new ArgumentNullException(nameof(parameterKey));
            StartTime = startTime;
            EndTime = endTime;
            GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
            SubgroupName = subgroupName; // Can be null or empty
            AnomalyScore = anomalyScore;
            SubgroupID = subgroupID;
        }
    }
}
