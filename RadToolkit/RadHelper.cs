using Skyline.DataMiner.Analytics.Mad;
using Skyline.DataMiner.Analytics.Rad;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    /// <summary>
    /// A utility class for working with RAD parameter groups in DataMiner.
    /// </summary>
    public class RadHelper
    {
        /// <summary>
        /// The minimum DataMiner version that allows shared model groups.
        /// </summary>
        public const string AllowSharedModelGroupsVersion = "10.5.9.0-16057";
        /// <summary>
        /// The minimum DataMiner version that has fields for default anomaly threshold and default minimal anomaly duration.
        /// </summary>
        public const string DefaultGroupOptionsVersion = "10.5.9.0-16100";
        /// <summary>
        /// The minimum DataMiner version that allows sending SLAnalytics messages directly via GQI.
        /// </summary>
        public const string GQISendAnalyticsMessagesVersion = "10.5.9.0";
        /// <summary>
        /// The minimum DataMiner version that has a RadGroupInfoEvent cache.
        /// </summary>
        public const string RadGroupInfoEventCacheVersion = "10.5.11.0-16340";
        /// <summary>
        /// The minimum DataMiner version that allows fetching historical anomalies.
        /// </summary>
        public const string HistoricalAnomaliesVersion = "10.5.12.0-16429";
        /// <summary>
        /// The minimum DataMiner version that allows training configuration in the AddRADParameterGroupMessage.
        /// </summary>
        public const string TrainingConfigInAddGroupMessageVersion = "10.6.0.0-16548";

        private readonly IConnection _connection;
        private readonly Logger _logger;
        private readonly bool _allowSharedModelGroups;
        private readonly bool _defaultGroupOptionsAvailable;
        private readonly bool _allowGQISendAnalyticsMessages;
        private readonly bool _radGroupInfoEventCacheAvailable;
        private readonly bool _historicalAnomaliesAvailable;
        private readonly bool _trainingConfigInAddGroupMessageAvailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadHelper"/> class.
        /// </summary>
        /// <param name="connection">The DataMiner connection to use for message handling.</param>
        /// <param name="logger">The logger for error reporting.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> or <paramref name="logger"/> is <c>null</c>.</exception>
        public RadHelper(IConnection connection, Logger logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection), "Connection cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            var dataMinerVersion = RetrieveActiveDmsVersion();
            if (dataMinerVersion != string.Empty)
            {
                _allowSharedModelGroups = IsDmsHigherThanMinimum(dataMinerVersion, AllowSharedModelGroupsVersion);
                _defaultGroupOptionsAvailable = IsDmsHigherThanMinimum(dataMinerVersion, DefaultGroupOptionsVersion);
                _allowGQISendAnalyticsMessages = IsDmsHigherThanMinimum(dataMinerVersion, GQISendAnalyticsMessagesVersion);
                _radGroupInfoEventCacheAvailable = IsDmsHigherThanMinimum(dataMinerVersion, RadGroupInfoEventCacheVersion);
                _historicalAnomaliesAvailable = IsDmsHigherThanMinimum(dataMinerVersion, HistoricalAnomaliesVersion);
                _trainingConfigInAddGroupMessageAvailable = IsDmsHigherThanMinimum(dataMinerVersion, TrainingConfigInAddGroupMessageVersion);
            }
        }

        /// <summary>
        /// Gets a value indicating whether sending SLAnalytics messages directly via GQI is allowed on the connected DataMiner version.
        /// </summary>
        public bool AllowGQISendAnalyticsMessages => _allowGQISendAnalyticsMessages;

        /// <summary>
        /// Gets a value indicating whether shared model groups are allowed on the connected DataMiner version.
        /// </summary>
        public bool AllowSharedModelGroups => _allowSharedModelGroups;

        /// <summary>
        /// Gets a value indicating whether the RadGroupInfoEvent cache is available on the connected DataMiner version.
        /// </summary>
        public bool RadGroupInfoEventCacheAvailable => _radGroupInfoEventCacheAvailable;

        /// <summary>
        /// Gets a value indicating whether fetching historical anomalies is available on the connected DataMiner version.
        /// </summary>
        public bool HistoricalAnomaliesAvailable => _historicalAnomaliesAvailable;

        /// <summary>
        /// Gets a value indicating whether training configuration in the AddRADParameterGroupMessage is available on the connected DataMiner version.
        /// </summary>
        public bool TrainingConfigInAddGroupMessageAvailable => _trainingConfigInAddGroupMessageAvailable;

        /// <summary>
        /// Gets the default value for the threshold above which an anomaly will be generated.
        /// </summary>
        public double DefaultAnomalyThreshold
        {
            get
            {
                if (_defaultGroupOptionsAvailable)
                    return GetDefaultAnomalyThreshold();
                else
                    return 3.0;
            }
        }

        /// <summary>
        /// Gets the default value for the minimal duration (in minutes) the anomaly score should be above the threshold before a suggestion event is generated.
        /// </summary>
        public int DefaultMinimumAnomalyDuration
        {
            get
            {
                if (_defaultGroupOptionsAvailable)
                    return GetMinimumAnomalyDuration();
                else
                    return 5;
            }
        }

        /// <summary>
        /// Gets the connection used by this instance.
        /// </summary>
        public IConnection Connection => _connection;

        /// <summary>
        /// Fetches the names of all relational anomaly groups across all DataMiner agents.
        /// </summary>
        /// <returns>A list of parameter group names.</returns>
        public List<string> FetchParameterGroups()
        {
            if (!_radGroupInfoEventCacheAvailable)
            {
                return _connection.HandleMessage(new GetInfoMessage(InfoType.DataMinerInfo))
                    .OfType<GetDataMinerInfoResponseMessage>()
                    .Select(m => m.ID)
                    .Distinct()
                    .SelectMany(dmaID => InnerFetchParameterGroups(dmaID) ?? new List<string>())
                    .ToList();
            }
            else
            {
                return InnerFetchParameterGroups() ?? new List<string>();
            }
        }

        /// <summary>
        /// Fetches the details of all relational anomaly groups across all DataMiner agents.
        /// </summary>
        /// <returns>A list of parameter group infos.</returns>
        public List<RadGroupInfo> FetchParameterGroupInfos()
        {
            if (!_radGroupInfoEventCacheAvailable)
            {
                var dataMinerIDs = _connection.HandleMessage(new GetInfoMessage(InfoType.DataMinerInfo))
                    .OfType<GetDataMinerInfoResponseMessage>()
                    .Select(m => m.ID)
                    .Distinct();

                var result = new List<RadGroupInfo>();
                foreach (var dataMinerID in dataMinerIDs)
                {
                    var groupNames = InnerFetchParameterGroups(dataMinerID);
                    if (groupNames == null)
                        continue;

                    result.AddRange(groupNames.Select(groupName => FetchParameterGroupInfo(dataMinerID, groupName)));
                }

                return result;
            }
            else
            {
                return InnerFetchParameterGroupInfosFromCache();
            }
        }

        /// <summary>
        /// Fetch the group info for a specific relational anomaly group by name. Note that this only works on DataMiner versions later than <see cref="RadGroupInfoEventCacheVersion"/>, on older versions
        /// use the version with a dataMinerID parameter.
        /// </summary>
        /// <param name="groupName">The name of the relational anomaly group.</param>
        /// <returns>The <see cref="RadGroupInfo"/> for the group, or <c>null</c> if not found.</returns>
        /// <exception cref="NotSupportedException">Thrown if your DataMiner version is older than <see cref="RadGroupInfoEventCacheVersion"/>.</exception>
        public RadGroupInfo FetchParameterGroupInfo(string groupName)
        {
            if (!_radGroupInfoEventCacheAvailable)
                throw new NotSupportedException("Fetching parameter group info by name only is not supported on this DataMiner version.");

            return InnerFetchParameterGroupInfo(groupName);
        }

        /// <summary>
        /// Fetches the list of parameter group names for a given DataMiner agent. Note that on versions prior to <see cref="RadGroupInfoEventCacheVersion"/>, this will
        /// only return groups from the given agent, while on higher versions, it will return groups from all agents.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID.</param>
        /// <returns>List of parameter group names, or <c>null</c> if not available.</returns>
        [Obsolete("This method is obsolete since DataMiner 10.5.11. Use FetchParameterGroups() instead, which fetches groups from all agents.")]
        public List<string> FetchParameterGroups(int dataMinerID)
        {
            return InnerFetchParameterGroups(dataMinerID);
        }

#pragma warning disable CS0618 // Type or member is obsolete: messages are obsolete since 10.5.5, but replacements were only added in that version
        /// <summary>
        /// Fetches detailed information about a specific parameter group.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID.</param>
        /// <param name="groupName">The name of the parameter group.</param>
        /// <returns>The <see cref="RadGroupInfo"/> for the group, or <c>null</c> if not found.</returns>
        public RadGroupInfo FetchParameterGroupInfo(int dataMinerID, string groupName)
        {
            GetMADParameterGroupInfoMessage request = new GetMADParameterGroupInfoMessage(groupName)
            {
                DataMinerID = dataMinerID,
            };

            var response = _connection.HandleSingleResponseMessage(request);
            if (_allowSharedModelGroups)
                return ParseParameterGroupInfoResponse(dataMinerID, response);
            else if (response is GetMADParameterGroupInfoResponseMessage madResponse)
                return ParseMADParameterGroupInfoResponse(dataMinerID, madResponse);
            else
                return null;
        }

        /// <summary>
        /// Removes a parameter group.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID, or -1 to let resolve the DataMiner agent automatically.</param>
        /// <param name="groupName">The name of the parameter group to remove.</param>
        public void RemoveParameterGroup(int dataMinerID, string groupName)
        {
            var request = new RemoveMADParameterGroupMessage(groupName);
            if (dataMinerID != -1)
                request.DataMinerID = dataMinerID;

            _connection.HandleSingleResponseMessage(request);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Adds a new parameter group using the specified settings.
        /// </summary>
        /// <param name="settings">The group settings.</param>
        /// <param name="trainingConfiguration">The training configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> or its subgroups are <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if no subgroups are specified.</exception>
        /// <exception cref="NotSupportedException">Thrown if the group is a shared model group (i.e. it has two or more subgroups) and DataMiner does not yet support shared model groups, or
        /// if training configuration is provided and DataMiner does not yet support passing training configuration while adding parameter groups.</exception>
        public void AddParameterGroup(RadGroupSettings settings, TrainingConfiguration trainingConfiguration = null)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "Settings cannot be null.");
            if (settings.Subgroups == null)
                throw new ArgumentNullException(nameof(settings), "Settings must contain subgroups.");
            if (settings.Subgroups.Count == 0)
                throw new ArgumentException("Settings must contain at least one subgroup.", nameof(settings));
            if (trainingConfiguration != null && !_trainingConfigInAddGroupMessageAvailable)
                throw new NotSupportedException("Passing training configuration while adding parameter groups is not supported on this DataMiner version.");

            if (!_allowSharedModelGroups)
            {
                if (settings.Subgroups.Count >= 2)
                    throw new NotSupportedException("Adding parameter groups with multiple subgroups is not supported on this DataMiner version.");
                if (settings.Subgroups.First()?.Options?.AnomalyThreshold != null)
                    throw new NotSupportedException("Setting subgroup-specific anomaly thresholds is not supported on this DataMiner version.");
                if (settings.Subgroups.First()?.Options?.MinimalDuration != null)
                    throw new NotSupportedException("Setting subgroup-specific minimal durations is not supported on this DataMiner version.");

                InnerAddMadParameterGroup(settings);
            }
            else
            {
                if (_trainingConfigInAddGroupMessageAvailable)
                    InnerAddRadParameterGroup(settings, trainingConfiguration);
                else
                    InnerAddRadParameterGroup(settings);
            }
        }

        /// <summary>
        /// Retrains a parameter group using the specified time ranges and optionally excluded subgroups.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID, or -1 to let resolve the DataMiner agent automatically.</param>
        /// <param name="groupName">The name of the parameter group.</param>
        /// <param name="timeRanges">The time ranges to use for retraining.</param>
        /// <param name="excludedSubgroupIDs">Optional list of subgroup IDs whose data should be excluded while retraining.</param>
        /// <exception cref="NotSupportedException">Thrown if excluding subgroups is not supported on the current DataMiner version.</exception>
        public void RetrainParameterGroup(int dataMinerID, string groupName, IEnumerable<TimeRange> timeRanges, IEnumerable<Guid> excludedSubgroupIDs = null)
        {
            if (_allowSharedModelGroups)
                InnerRetrainParameterGroup(dataMinerID, groupName, timeRanges, excludedSubgroupIDs);
            else if (excludedSubgroupIDs?.Any() == true)
                throw new NotSupportedException("Excluding subgroups is not supported on this DataMiner version.");
            else
                InnerRetrainParameterGroup(dataMinerID, groupName, timeRanges);
        }

#pragma warning disable CS0618 // Type or member is obsolete: messages are obsolete since 10.5.5, but replacements were only added in that version
        /// <summary>
        /// Fetches anomaly score data for a parameter group within a specified time range.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID, or -1 to let resolve the DataMiner agent automatically.</param>
        /// <param name="groupName">The name of the parameter group.</param>
        /// <param name="startTime">The start time of the range.</param>
        /// <param name="endTime">The end time of the range.</param>
        /// <returns>List of timestamp and anomaly score pairs.</returns>
        public List<KeyValuePair<DateTime, double>> FetchAnomalyScoreData(int dataMinerID, string groupName, DateTime startTime, DateTime endTime)
        {
            GetMADDataMessage request = new GetMADDataMessage(groupName, startTime, endTime);
            if (dataMinerID != -1)
                request.DataMinerID = dataMinerID;

            var response = _connection.HandleSingleResponseMessage(request) as GetMADDataResponseMessage;
            return response?.Data?.Where(p => p != null).Select(p => new KeyValuePair<DateTime, double>(p.Timestamp.ToUniversalTime(), p.AnomalyScore)).ToList();
        }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Fetches anomaly score data for a specific subgroup by name within a parameter group and time range.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID, or -1 to let resolve the DataMiner agent automatically.</param>
        /// <param name="groupName">The name of the parameter group.</param>
        /// <param name="subGroupName">The name of the subgroup.</param>
        /// <param name="startTime">The start time of the range.</param>
        /// <param name="endTime">The end time of the range.</param>
        /// <returns>List of timestamp and anomaly score pairs.</returns>
        /// <exception cref="NotSupportedException">Thrown if the operation is not supported on the current DataMiner version.</exception>
        public List<KeyValuePair<DateTime, double>> FetchAnomalyScoreData(int dataMinerID, string groupName, string subGroupName,
            DateTime startTime, DateTime endTime)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Fetching subgroup anomaly score data for a subgroup is not allowed on this DataMiner version.");

            return InnerFetchAnomalyScoreData(dataMinerID, groupName, subGroupName, startTime, endTime);
        }

        /// <summary>
        /// Fetches anomaly score data for a specific subgroup by ID within a parameter group and time range.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID, or -1 to let resolve the DataMiner agent automatically.</param>
        /// <param name="groupName">The name of the parameter group.</param>
        /// <param name="subGroupID">The ID of the subgroup.</param>
        /// <param name="startTime">The start time of the range.</param>
        /// <param name="endTime">The end time of the range.</param>
        /// <returns>List of timestamp and anomaly score pairs.</returns>
        /// <exception cref="NotSupportedException">Thrown if the operation is not supported on the current DataMiner version.</exception>
        public List<KeyValuePair<DateTime, double>> FetchAnomalyScoreData(int dataMinerID, string groupName, Guid subGroupID,
            DateTime startTime, DateTime endTime)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Fetching subgroup anomaly score data for a subgroup is not allowed on this DataMiner version.");

            return InnerFetchAnomalyScoreData(dataMinerID, groupName, subGroupID, startTime, endTime);
        }

        /// <summary>
        /// Renames a parameter group.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID, or -1 to let resolve the DataMiner agent automatically.</param>
        /// <param name="oldGroupName">The current name of the group.</param>
        /// <param name="newGroupName">The new name for the group.</param>
        /// <exception cref="NotSupportedException">Thrown if the operation is not supported on the current DataMiner version.</exception>
        public void RenameParameterGroup(int dataMinerID, string oldGroupName, string newGroupName)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Renaming parameter group is not allowed on this DataMiner version.");

            InnerRenameParameterGroup(dataMinerID, oldGroupName, newGroupName);
        }

        /// <summary>
        /// Adds a new subgroup to an existing parameter group.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID, or -1 to let resolve the DataMiner agent automatically.</param>
        /// <param name="groupName">The name of the parameter group.</param>
        /// <param name="settings">The subgroup settings.</param>
        /// <exception cref="NotSupportedException">Thrown if the operation is not supported on the current DataMiner version.</exception>
        public void AddSubgroup(int dataMinerID, string groupName, RadSubgroupSettings settings)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Adding subgroups is not allowed on this DataMiner version.");

            InnerAddSubgroup(dataMinerID, groupName, settings);
        }

        /// <summary>
        /// Removes a subgroup from a parameter group.
        /// </summary>
        /// <param name="dataMinerID">The DataMiner agent ID, or -1 to let resolve the DataMiner agent automatically.</param>
        /// <param name="groupName">The name of the parameter group.</param>
        /// <param name="subgroupID">The ID of the subgroup to remove.</param>
        /// <exception cref="NotSupportedException">Thrown if the operation is not supported on the current DataMiner version.</exception>
        public void RemoveSubgroup(int dataMinerID, string groupName, Guid subgroupID)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Removing subgroups is not allowed on this DataMiner version.");
            
            InnerRemoveSubgroup(dataMinerID, groupName, subgroupID);
        }

        /// <summary>
        /// Fetches historical anomalies that start within the specified time range.
        /// </summary>
        /// <param name="startTime">The start time of the range in which to fetch historical anomalies.</param>
        /// <param name="endTime">The end time of the range in which to fetch historical anomalies.</param>
        /// <returns>A list of historical anomalies</returns>
        /// <exception cref="NotSupportedException">Thrown if the operation is not supported on the current DataMiner version.</exception>
        public List<RelationalAnomaly> FetchRelationalAnomalies(DateTime startTime, DateTime endTime)
        {
            if (!_historicalAnomaliesAvailable)
                throw new NotSupportedException("Fetching historical anomalies is not supported on this DataMiner version.");

            return InnerFetchRelationalAnomalies(startTime, endTime);
        }

        /// <summary>
        /// Only call this when <see cref="_radGroupInfoEventCacheAvailable"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<RadGroupInfo> InnerFetchParameterGroupInfosFromCache()
        {
            var request = new GetEventsFromCacheMessage(new SubscriptionFilter(typeof(RadGroupInfoEvent)));
            return _connection.HandleMessage(request)
                .OfType<RadGroupInfoEvent>()
                .Where(evt => evt.Info != null)
                .Select(evt => ParseRADGroupInfo(evt.DataMinerID, evt.Info))
                .ToList();
        }

        /// <summary>
        /// Only use this call when <see cref="_radGroupInfoEventCacheAvailable"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<string> InnerFetchParameterGroups()
        {
            var request = new GetRADParameterGroupsMessage();
            var response = _connection.HandleSingleResponseMessage(request) as GetRADParameterGroupsResponseMessage;
            return response?.GroupNames;
        }

#pragma warning disable CS0618 // Type or member is obsolete: messages are obsolete since 10.5.5, but replacements were only added in that version
        /// <summary>
        /// Only use this call when <see cref="_radGroupInfoEventCacheAvailable"/> is false.
        /// </summary>
        private List<string> InnerFetchParameterGroups(int dataMinerID)
        {
            GetMADParameterGroupsMessage request = new GetMADParameterGroupsMessage()
            {
                DataMinerID = dataMinerID,
            };

            var response = _connection.HandleSingleResponseMessage(request) as GetMADParameterGroupsResponseMessage;
            return response?.GroupNames;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Only call this when <see cref="_radGroupInfoEventCacheAvailable"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private RadGroupInfo InnerFetchParameterGroupInfo(string groupName)
        {
            var request = new GetRADParameterGroupInfoMessage(groupName);

            var response = _connection.HandleSingleResponseMessage(request) as GetRADParameterGroupInfoResponseMessage;
            if (response == null)
                return null;

            return ParseRADGroupInfo(response.DataMinerID, response.ParameterGroupInfo);
        }

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerAddRadParameterGroup(RadGroupSettings settings)
        {
            var subgroups = settings.Subgroups.Select(s => ToRADSubgroupInfo(s)).ToList();
            var groupInfo = new RADGroupInfo(settings.GroupName, subgroups, settings.Options.UpdateModel, settings.Options.AnomalyThreshold,
                settings.Options.MinimalDuration);
            var request = new AddRADParameterGroupMessage(groupInfo);
            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <see cref="_trainingConfigInAddGroupMessageAvailable"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerAddRadParameterGroup(RadGroupSettings settings, TrainingConfiguration trainingConfiguration)
        {
            var subgroups = settings.Subgroups.Select(s => ToRADSubgroupInfo(s)).ToList();
            var groupInfo = new RADGroupInfo(settings.GroupName, subgroups, settings.Options.UpdateModel, settings.Options.AnomalyThreshold,
                settings.Options.MinimalDuration);

            var request = new AddRADParameterGroupMessage(groupInfo);
            if (trainingConfiguration != null)
            {
                var trainingTimeRange = trainingConfiguration.TimeRanges.Select(tr => new Analytics.Rad.TimeRange(tr.Start, tr.End)).ToList();
                request.TrainingConfiguration = new Analytics.Rad.TrainingConfiguration(trainingTimeRange, trainingConfiguration.ExcludedSubgroups);
            }
            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private RadGroupInfo ParseRADGroupInfo(int dataMinerID, RADGroupInfo groupInfo)
        {
            if (groupInfo == null)
                return null;

            var options = new RadGroupOptions(groupInfo.UpdateModel, groupInfo.AnomalyThreshold, groupInfo.MinimumAnomalyDuration);
            var subgroups = new List<RadSubgroupInfo>();
            foreach (var subgroup in groupInfo.Subgroups)
            {
                if (subgroup == null)
                    continue;

                var subgroupOptions = new RadSubgroupOptions(subgroup.AnomalyThreshold, subgroup.MinimumAnomalyDuration);
                var subgroupParameters = subgroup.Parameters?.Where(p => p != null).Select(p => new RadParameter(p.Key, p.Label)).ToList() ?? new List<RadParameter>();
                subgroups.Add(new RadSubgroupInfo(subgroup.Name, subgroup.ID, subgroupParameters, subgroupOptions, subgroup.IsMonitored));
            }
            return new RadGroupInfo(dataMinerID, groupInfo.Name, options, subgroups);
        }

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private RadGroupInfo ParseParameterGroupInfoResponse(int dataMinerID, DMSMessage response)
        {
            if (response is GetRADParameterGroupInfoResponseMessage parameterGroupInfoResponse)
                return ParseRADGroupInfo(dataMinerID, parameterGroupInfoResponse?.ParameterGroupInfo);
            else if (response is GetMADParameterGroupInfoResponseMessage madParameterGroupInfoResponse)
                return ParseMADParameterGroupInfoResponse(dataMinerID, madParameterGroupInfoResponse);
            else
                return null;
        }

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<KeyValuePair<DateTime, double>> InnerFetchAnomalyScoreData(int dataMinerID, string groupName, string subGroupName,
            DateTime startTime, DateTime endTime)
        {
            GetRADDataMessage request = new GetRADDataMessage(groupName, subGroupName, startTime, endTime);
            if (dataMinerID != -1)
                request.DataMinerID = dataMinerID;

            var response = _connection.HandleSingleResponseMessage(request) as GetRADDataResponseMessage;
            return response?.DataPoints?.Where(p => p != null).Select(p => new KeyValuePair<DateTime, double>(p.Timestamp.ToUniversalTime(), p.AnomalyScore)).ToList();
        }

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<KeyValuePair<DateTime, double>> InnerFetchAnomalyScoreData(int dataMinerID, string groupName, Guid subGroupID,
            DateTime startTime, DateTime endTime)
        {
            GetRADDataMessage request = new GetRADDataMessage(groupName, subGroupID, startTime, endTime);
            if (dataMinerID != -1)
                request.DataMinerID = dataMinerID;
            
            var response = _connection.HandleSingleResponseMessage(request) as GetRADDataResponseMessage;
            return response?.DataPoints?.Where(p => p != null).Select(p => new KeyValuePair<DateTime, double>(p.Timestamp.ToUniversalTime(), p.AnomalyScore)).ToList();
        }

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerRenameParameterGroup(int dataMinerID, string oldGroupName, string newGroupName)
        {
            var request = new RenameRADParameterGroupMessage(oldGroupName, newGroupName);
            if (dataMinerID != -1)
                request.DataMinerID = dataMinerID;

            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerAddSubgroup(int dataMinerID, string groupName, RadSubgroupSettings settings)
        {
            var request = new AddRADSubgroupMessage(groupName, ToRADSubgroupInfo(settings));
            if (dataMinerID != -1)
                request.DataMinerID = dataMinerID;

            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerRemoveSubgroup(int dataMinerID, string groupName, Guid subgroupID)
        {
            var request = new RemoveRADSubgroupMessage(groupName, subgroupID);
            if (dataMinerID != -1)
                request.DataMinerID = dataMinerID;

            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerRetrainParameterGroup(int dataMinerID, string groupName, IEnumerable<TimeRange> timeRanges, IEnumerable<Guid> excludedSubgroupIDs)
        {
            var request = new RetrainRADModelMessage(groupName, timeRanges.Select(r => new Skyline.DataMiner.Analytics.Rad.TimeRange(r.Start, r.End)).ToList())
            {
                ExcludedSubgroupIDs = excludedSubgroupIDs?.ToList() ?? new List<Guid>(),
            };
            if (dataMinerID != -1)
                request.DataMinerID = dataMinerID;

            _connection.HandleSingleResponseMessage(request);
        }

#pragma warning disable CS0618 // Type or member is obsolete: messages are obsolete since 10.5.5, but replacements were only added in that version
        private void InnerRetrainParameterGroup(int dataMinerID, string groupName, IEnumerable<TimeRange> timeRanges)
        {
            var request = new RetrainMADModelMessage(groupName, timeRanges.Select(r => new Skyline.DataMiner.Analytics.Mad.TimeRange(r.Start, r.End)).ToList());
            if (dataMinerID != -1)
                request.DataMinerID = dataMinerID;

            _connection.HandleSingleResponseMessage(request);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Only call this when <see cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        private RADSubgroupInfo ToRADSubgroupInfo(RadSubgroupSettings settings)
        {
            if (settings == null)
                return null;

            var parameters = settings.Parameters.Select(p => new RADParameter(p?.Key, p?.Label)).ToList();
            return new RADSubgroupInfo(settings.Name, parameters, settings.Options.AnomalyThreshold, settings.Options.MinimalDuration);
        }

#pragma warning disable CS0618 // Type or member is obsolete: messages are obsolete since 10.5.5, but replacements were only added in that version
        private void InnerAddMadParameterGroup(RadGroupSettings settings)
        {
            var groupInfo = new MADGroupInfo(settings.GroupName, settings.Subgroups.First().Parameters?.ConvertAll(p => p?.Key), settings.Options.UpdateModel,
                settings.Options.AnomalyThreshold, settings.Options.MinimalDuration);
            var request = new AddMADParameterGroupMessage(groupInfo);
            _connection.HandleSingleResponseMessage(request);
        }
        
        private RadGroupInfo ParseMADParameterGroupInfoResponse(int dataMinerID, GetMADParameterGroupInfoResponseMessage response)
        {
            if (response?.GroupInfo == null)
                return null;

            var options = new RadGroupOptions(response.GroupInfo.UpdateModel, response.GroupInfo.AnomalyThreshold, response.GroupInfo.MinimumAnomalyDuration);
            var subgroups = new List<RadSubgroupInfo>()
            {
                new RadSubgroupInfo(string.Empty, Guid.NewGuid(), 
                    response.GroupInfo.Parameters?.ConvertAll(p => new RadParameter(p, string.Empty)) ?? new List<RadParameter>(), 
                    new RadSubgroupOptions(), true),
            };
            return new RadGroupInfo(dataMinerID, response.GroupInfo.Name, options, subgroups);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Only call this when <see cref="_defaultGroupOptionsAvailable"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private double GetDefaultAnomalyThreshold()
        {
            return RADGroupInfo.DefaultAnomalyThreshold;
        }

        /// <summary>
        /// Only call this when <see cref="_defaultGroupOptionsAvailable"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private int GetMinimumAnomalyDuration()
        {
            return RADGroupInfo.DefaultMinimumAnomalyDuration;
        }

        /// <summary>
        /// Only call this when <see cref="_historicalAnomaliesAvailable"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<RelationalAnomaly> InnerFetchRelationalAnomalies(DateTime startTime, DateTime endTime)
        {
            var request = new GetAllRelationalAnomaliesMessage(startTime, endTime);
            var response = _connection.HandleSingleResponseMessage(request) as GetRelationalAnomaliesResponseMessage;
            if (response?.Anomalies == null)
                return new List<RelationalAnomaly>();

            return response.Anomalies.Select(a => new RelationalAnomaly(a.AnomalyID, a.ParameterKey, a.StartTime, a.EndTime, a.GroupName, a.SubgroupName,
                a.SubgroupID, a.AnomalyScore)).ToList();
        }

        /// <summary>
        /// Determines whether the specified DataMiner version is higher than or equal to a given minimum version.
        /// </summary>
        /// <param name="dataminerVersion">
        /// The current DataMiner version, in the format "A.B.C.D-Build". The build part is optional. Example: "10.3.0.0-12345".
        /// </param>
        /// <param name="minimumDmaVersion">
        /// The minimum required DataMiner version, in the same format as above.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current DataMiner version is higher than or equal to the specified minimum; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method compares both the version number and the build number (if present).
        /// If either version string is invalid, a warning is logged and the method returns <c>false</c>.
        /// </remarks>
        private bool IsDmsHigherThanMinimum(string dataminerVersion, string minimumDmaVersion)
        {
            // decided to avoid needing extra references or using existing classes. Easier to allow readable string inputs.

            var currentBuildSplit = dataminerVersion.Split('-');
            var currentRawVersion = currentBuildSplit[0];
            var currentRawSplit = currentRawVersion.Split('.');

            if (currentRawSplit.Length != 4)
            {
                _logger.Error($"Warning: Active DataMiner has a version with unknown format {dataminerVersion}. Expected 'A.B.C.D-Build'. Ignoring check and returning false.");
                return false;
            }

            var currentBuild = (currentBuildSplit.Length > 1) ? Convert.ToInt32(currentBuildSplit[1].Trim()) : 0;
            int currentA = Convert.ToInt32(currentRawSplit[0].Trim());
            int currentB = Convert.ToInt32(currentRawSplit[1].Trim());
            int currentC = Convert.ToInt32(currentRawSplit[2].Trim());
            int currentD = Convert.ToInt32(currentRawSplit[3].Trim());

            var minBuildSplit = minimumDmaVersion.Split('-');
            var minRawVersion = minBuildSplit[0];
            var minRawSplit = minRawVersion.Split('.');

            if (minRawSplit.Length != 4)
            {
                _logger.Error($"Warning: Provided Minimum DataMiner Version has a version with unknown format {minimumDmaVersion}. Expected 'A.B.C.D-Build'. Ignoring check and returning false.");
                return false;
            }

            var minBuild = (minBuildSplit.Length > 1) ? Convert.ToInt32(minBuildSplit[1].Trim()) : 0;
            int minA = Convert.ToInt32(minRawSplit[0].Trim());
            int minB = Convert.ToInt32(minRawSplit[1].Trim());
            int minC = Convert.ToInt32(minRawSplit[2].Trim());
            int minD = Convert.ToInt32(minRawSplit[3].Trim());


            if (currentA > minA) return true;
            if (currentA < minA) return false;

            if (currentB > minB) return true;
            if (currentB < minB) return false;

            if (currentC > minC) return true;
            if (currentC < minC) return false;

            if (currentD > minD) return true;
            if (currentD < minD) return false;

            if (currentBuild >= minBuild) return true;
            if (currentBuild < minBuild) return false;

            return true;
        }

        private string RetrieveActiveDmsVersion()
        {
            GetAgentBuildInfo msg = new GetAgentBuildInfo();
            var result = _connection.HandleSingleResponseMessage(msg) as BuildInfoResponse;
            var agent = result?.Agents?.FirstOrDefault();
            if (agent == null)
            {
                _logger.Error("Failed to retrieve DataMiner version information.");
                return string.Empty;
            }

            return agent.RawVersion + "-" + agent.UpgradeBuildID;
        }
    }
}
