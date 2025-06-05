using Skyline.DataMiner.Analytics.Mad;
using Skyline.DataMiner.Analytics.Rad;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class RadHelper
    {
        public const string AllowSharedModelGroupsVersion = "10.5.6.0-15711"; //TODO: fill in the correct version number
        private readonly IConnection _connection;
        private readonly Logger _logger;
        private readonly string _dataMinerVersion;
        private readonly bool _allowSharedModelGroups;

        public RadHelper(IConnection connection, Logger logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection), "Connection cannot be null.");
            _logger = logger;
            _dataMinerVersion = RetrieveActiveDmsVersion();
            if (_dataMinerVersion != string.Empty)
            {
                _allowSharedModelGroups = IsDmsHigherThanMinimum(_dataMinerVersion, AllowSharedModelGroupsVersion);
            }
        }

        public bool AllowSharedModelGroups => _allowSharedModelGroups;

#pragma warning disable CS0618 // Type or member is obsolete: messages are obsolete since 10.5.5, but replacements were only added in that version
        public List<string> FetchParameterGroups(int dataMinerID)
        {
            GetMADParameterGroupsMessage request = new GetMADParameterGroupsMessage()
            {
                DataMinerID = dataMinerID,
            };

            var response = _connection.HandleSingleResponseMessage(request) as GetMADParameterGroupsResponseMessage;
            return response?.GroupNames;
        }

        public RadGroupInfo FetchParameterGroupInfo(int dataMinerID, string groupName)
        {
            GetMADParameterGroupInfoMessage request = new GetMADParameterGroupInfoMessage(groupName)
            {
                DataMinerID = dataMinerID,
            };
            var response = _connection.HandleSingleResponseMessage(request);
            if (_allowSharedModelGroups)
                return ParseParameterGroupInfoResponse(response);
            else if (response is GetMADParameterGroupInfoResponseMessage madResponse)
                return ParseMADParameterGroupInfoResponse(madResponse);
            else
                return null;
        }

        public List<KeyValuePair<DateTime, double>> FetchAnomalyScoreData(int dataMinerID, string groupName, DateTime startTime, DateTime endTime)
        {
            GetMADDataMessage request = new GetMADDataMessage(groupName, startTime, endTime)
            {
                DataMinerID = dataMinerID,
            };
            var response = _connection.HandleSingleResponseMessage(request) as GetMADDataResponseMessage;
            return response?.Data?.Where(p => p != null).Select(p => new KeyValuePair<DateTime, double>(p.Timestamp.ToUniversalTime(), p.AnomalyScore)).ToList();
        }

        public void RemoveParameterGroup(int dataMinerID, string groupName)
        {
            var request = new RemoveMADParameterGroupMessage(groupName)
            {
                DataMinerID = dataMinerID,
            };
            _connection.HandleSingleResponseMessage(request);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public void AddParameterGroup(RadGroupSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "Settings cannot be null.");
            if (settings.Subgroups?.Count() > 0)
                throw new ArgumentException(nameof(settings.Subgroups), "Settings must contain at least one subgroup.");

            if (settings.Subgroups.Count() >= 2)
            {
                if (!_allowSharedModelGroups)
                    throw new NotSupportedException("Adding parameter groups with multiple subgroups is not supported on this DataMiner version.");

                InnerAddSharedModelGroup(settings);
            }
            else
            {
                InnerAddParameterGroup(settings);
            }
        }

        public void RetrainParameterGroup(int dataMinerID, string groupName, IEnumerable<TimeRange> timeRanges, IEnumerable<Guid> excludedSubgroupIDs = null)
        {
            if (_allowSharedModelGroups)
                InnerRetrainParameterGroup(dataMinerID, groupName, timeRanges, excludedSubgroupIDs);
            else if (excludedSubgroupIDs?.Count() > 0)
                throw new NotSupportedException("Excluding subgroups is not supported on this DataMiner version.");
            else
                InnerRetrainParameterGroup(dataMinerID, groupName, timeRanges);
        }

        public List<KeyValuePair<DateTime, double>> FetchAnomalyScoreData(int dataMinerID, string groupName, string subGroupName,
            DateTime startTime, DateTime endTime)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Fetching subgroup anomaly score data for a subgroup is not allowed on this DataMiner version.");

            return InnerFetchAnomalyScoreData(dataMinerID, groupName, subGroupName, startTime, endTime);
        }

        public List<KeyValuePair<DateTime, double>> FetchAnomalyScoreData(int dataMinerID, string groupName, Guid subGroupID,
            DateTime startTime, DateTime endTime)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Fetching subgroup anomaly score data for a subgroup is not allowed on this DataMiner version.");

            return InnerFetchAnomalyScoreData(dataMinerID, groupName, subGroupID, startTime, endTime);
        }

        public void RenameParameterGroup(int dataMinerID, string oldGroupName, string newGroupName)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Renaming parameter group is not allowed on this DataMiner version.");

            InnerRenameParameterGroup(dataMinerID, oldGroupName, newGroupName);
        }

        public void AddSubgroup(int dataMinerID, string groupName, RadSubgroupSettings settings)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Adding subgroups is not allowed on this DataMiner version.");

            InnerAddSubgroup(dataMinerID, groupName, settings);
        }

        public void RemoveSubgroup(int dataMinerID, string groupName, Guid subgroupID)
        {
            if (!_allowSharedModelGroups)
                throw new NotSupportedException("Removing subgroups is not allowed on this DataMiner version.");
            
            InnerRemoveSubgroup(dataMinerID, groupName, subgroupID);
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerAddSharedModelGroup(RadGroupSettings settings)
        {
            var subgroups = settings.Subgroups.Select(s => ToRADSubgroupInfo(s)).ToList();
            var groupInfo = new RADGroupInfo(settings.GroupName, subgroups, settings.Options.UpdateModel, settings.Options.AnomalyThreshold,
                settings.Options.MinimalDuration);
            var request = new AddRADParameterGroupMessage(groupInfo);
            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private RadGroupInfo ParseRADParameterGroupInfoResponse(GetRADParameterGroupInfoResponseMessage response)
        {
            var groupInfo = response?.ParameterGroupInfo;
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
            return new RadGroupInfo(groupInfo.Name, options, subgroups);
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private RadGroupInfo ParseParameterGroupInfoResponse(DMSMessage response)
        {
            if (response is GetRADParameterGroupInfoResponseMessage parameterGroupInfoResponse)
                return ParseRADParameterGroupInfoResponse(parameterGroupInfoResponse);
            else if (response is GetMADParameterGroupInfoResponseMessage madParameterGroupInfoResponse)
                return ParseMADParameterGroupInfoResponse(madParameterGroupInfoResponse);
            else
                return null;
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<KeyValuePair<DateTime, double>> InnerFetchAnomalyScoreData(int dataMinerID, string groupName, string subGroupName,
            DateTime startTime, DateTime endTime)
        {
            GetRADDataMessage request = new GetRADDataMessage(groupName, subGroupName, startTime, endTime)
            {
                DataMinerID = dataMinerID,
            };
            var response = _connection.HandleSingleResponseMessage(request) as GetRADDataResponseMessage;
            return response?.DataPoints?.Where(p => p != null).Select(p => new KeyValuePair<DateTime, double>(p.Timestamp.ToUniversalTime(), p.AnomalyScore)).ToList();
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<KeyValuePair<DateTime, double>> InnerFetchAnomalyScoreData(int dataMinerID, string groupName, Guid subGroupID,
            DateTime startTime, DateTime endTime)
        {
            GetRADDataMessage request = new GetRADDataMessage(groupName, subGroupID, startTime, endTime)
            {
                DataMinerID = dataMinerID,
            };
            var response = _connection.HandleSingleResponseMessage(request) as GetRADDataResponseMessage;
            return response?.DataPoints?.Where(p => p != null).Select(p => new KeyValuePair<DateTime, double>(p.Timestamp.ToUniversalTime(), p.AnomalyScore)).ToList();
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerRenameParameterGroup(int dataMinerID, string oldGroupName, string newGroupName)
        {
            var request = new RenameRADParameterGroupMessage(oldGroupName, newGroupName)
            {
                DataMinerID = dataMinerID,
            };
            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerAddSubgroup(int dataMinerID, string groupName, RadSubgroupSettings settings)
        {
            var request = new AddRADSubgroupMessage(groupName, ToRADSubgroupInfo(settings))
            {
                DataMinerID = dataMinerID,
            };
            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerRemoveSubgroup(int dataMinerID, string groupName, Guid subgroupID)
        {
            var request = new RemoveRADSubgroupMessage(groupName, subgroupID)
            {
                DataMinerID = dataMinerID,
            };
            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InnerRetrainParameterGroup(int dataMinerID, string groupName, IEnumerable<TimeRange> timeRanges, IEnumerable<Guid> excludedSubgroupIDs)
        {
            var request = new RetrainRADModelMessage(groupName, timeRanges.Select(r => new Skyline.DataMiner.Analytics.Rad.TimeRange(r.Start, r.End)).ToList())
            {
                DataMinerID = dataMinerID,
                ExcludedSubgroupIDs = excludedSubgroupIDs?.ToList() ?? new List<Guid>(),
            };
            _connection.HandleSingleResponseMessage(request);
        }

        /// <summary>
        /// Only call this when <seealso cref="_allowSharedModelGroups"/> is true.
        /// </summary>
        private RADSubgroupInfo ToRADSubgroupInfo(RadSubgroupSettings settings)
        {
            if (settings == null)
                return null;

            var parameters = settings.Parameters.Select(p => new RADParameter(p?.Key, p?.Label)).ToList();
            return new RADSubgroupInfo(settings.Name, parameters, settings.Options.AnomalyThreshold, settings.Options.MinimalDuration);
        }

#pragma warning disable CS0618 // Type or member is obsolete: messages are obsolete since 10.5.5, but replacements were only added in that version
        private void InnerAddParameterGroup(RadGroupSettings settings)
        {
            var groupInfo = new MADGroupInfo(settings.GroupName, settings.Subgroups.First().Parameters?.ConvertAll(p => p?.Key), settings.Options.UpdateModel,
                settings.Options.AnomalyThreshold, settings.Options.MinimalDuration);
            var request = new AddMADParameterGroupMessage(groupInfo);
            _connection.HandleSingleResponseMessage(request);
        }
        
        private RadGroupInfo ParseMADParameterGroupInfoResponse(GetMADParameterGroupInfoResponseMessage response)
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
            return new RadGroupInfo(response.GroupInfo.Name, options, subgroups);
        }
        
        private void InnerRetrainParameterGroup(int dataMinerID, string groupName, IEnumerable<TimeRange> timeRanges)
        {
            var request = new RetrainMADModelMessage(groupName, timeRanges.Select(r => new Skyline.DataMiner.Analytics.Mad.TimeRange(r.Start, r.End)).ToList())
            {
                DataMinerID = dataMinerID,
            };
            _connection.HandleSingleResponseMessage(request);
        }
#pragma warning restore CS0618 // Type or member is obsolete

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

            if (currentBuild > minBuild) return true;
            if (currentBuild < minBuild) return false;

            return true;
        }

        private string RetrieveActiveDmsVersion()
        {
            GetAgentBuildInfo msg = new GetAgentBuildInfo();
            var result = _connection.HandleSingleResponseMessage(msg) as BuildInfoResponse;
            if (result?.Agents?.FirstOrDefault() == null)
            {
                _logger.Error("Failed to retrieve DataMiner version information.");
                return string.Empty;
            }

            var agent = result.Agents.FirstOrDefault();
            return agent.RawVersion + "-" + agent.UpgradeBuildID;
        }
    }
}
