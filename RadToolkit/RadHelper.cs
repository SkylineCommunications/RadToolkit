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
        public const string AllowSharedModelGroupsVersion = "10.5.6.0-15861"; //TODO: fill in the correct version number
        private readonly IConnection _connection;
        private readonly Logger _logger;
        private readonly string _dataMinerVersion;
        private readonly bool _allowSharedModelGroups;

        public RadHelper(IConnection connection, Logger logger)
        {
            _connection = Engine.SLNetRaw ?? throw new ArgumentNullException(nameof(connection), "Connection cannot be null.");
            _logger = logger;
            _dataMinerVersion = RetrieveActiveDmsVersion();
            if (_dataMinerVersion != string.Empty)
            {
                _allowSharedModelGroups = IsDmsHigherThanMinimum(_dataMinerVersion, AllowSharedModelGroupsVersion);
            }
        }

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

        public List<KeyValuePair<DateTime, double>> FetchAnomalyScoreData(int dataMinerID, string groupName, DateTime startTime, DateTime endTime)
        {
            GetMADDataMessage request = new GetMADDataMessage(groupName, startTime, endTime)
            {
                DataMinerID = dataMinerID,
            };
            var response = _connection.HandleSingleResponseMessage(request) as GetMADDataResponseMessage;
            return response?.Data.Select(p => new KeyValuePair<DateTime, double>(p.Timestamp.ToUniversalTime(), p.AnomalyScore)).ToList();
        }

        public void RemoveParameterGroup(int dataMinerID, string groupName)
        {
            var request = new RemoveMADParameterGroupMessage(groupName)
            {
                DataMinerID = dataMinerID,
            };
            _connection.HandleSingleResponseMessage(request);
        }

        public void AddParameterGroup(RadGroupSettings settings)
        {
            var groupInfo = new MADGroupInfo(settings.GroupName, settings.Parameters.ToList(), settings.Options.UpdateModel,
                settings.Options.AnomalyThreshold, settings.Options.MinimalDuration);
            var request = new AddMADParameterGroupMessage(groupInfo);
            _connection.HandleSingleResponseMessage(request);
        }

        public void RetrainParameterGroup(int dataMinerID, string groupName, IEnumerable<TimeRange> timeRanges)
        {
            var request = new RetrainMADModelMessage(groupName, timeRanges.Select(r => new Skyline.DataMiner.Analytics.Mad.TimeRange(r.Start, r.End)).ToList())
            {
                DataMinerID = dataMinerID,
            };
            _connection.HandleSingleResponseMessage(request);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void AddParameterGroup(RadSharedModelGroupSettings settings)
        {
            if (!_allowSharedModelGroups)
            {
                _logger.Log("Adding shared model group is not allowed on this DataMiner version.", LogType.Error, 0);
                return;
            }
            var subgroups = settings.Subgroups.Select(s => ToRADSubgroupInfo(s)).ToList();
            var groupInfo = new RADSharedModelGroupInfo(settings.GroupName, subgroups, settings.Options.UpdateModel, settings.Options.AnomalyThreshold,
                settings.Options.MinimalDuration);
            var request = new AddRADSharedModelGroupMessage(groupInfo);
            _connection.HandleSingleResponseMessage(request);
        }

        private RADSubgroupInfo ToRADSubgroupInfo(RadSubgroupSettings settings)
        {
            if (settings == null)
                return null;

            var parameters = settings.Parameters.Select(p => new RADParameter(p?.Key, p?.Label)).ToList();
            return new RADSubgroupInfo(settings.Name, parameters, settings.Options.AnomalyThreshold, settings.Options.MinimalDuration);
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
                _logger.Log($"Warning: Active DataMiner has a version with unknown format {dataminerVersion}. Expected 'A.B.C.D-Build'. Ignoring check and returning false.", LogType.Error, 0);
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
                _logger.Log($"Warning: Provided Minimum DataMiner Version has a version with unknown format {minimumDmaVersion}. Expected 'A.B.C.D-Build'. Ignoring check and returning false.", LogType.Error, 0);
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
                _logger.Log("Failed to retrieve DataMiner version information.", LogType.Error, 0);
                return string.Empty;
            }

            var agent = result.Agents.FirstOrDefault();
            return agent.RawVersion + "-" + agent.UpgradeBuildID;
        }
    }
}
