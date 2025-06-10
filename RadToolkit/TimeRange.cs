using System;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    /// <summary>
    /// Represents a range of time with a start and end <see cref="DateTime"/>.
    /// </summary>
    public class TimeRange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeRange"/> class with the specified start and end times.
        /// </summary>
        /// <param name="start">The start <see cref="DateTime"/> of the range.</param>
        /// <param name="end">The end <see cref="DateTime"/> of the range.</param>
        public TimeRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets or sets the start <see cref="DateTime"/> of the range.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the end <see cref="DateTime"/> of the range.
        /// </summary>
        public DateTime End { get; set; }
    }
}
