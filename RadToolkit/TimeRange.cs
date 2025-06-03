using System;

namespace Skyline.DataMiner.Utils.RadToolkit
{
    public class TimeRange
    {
        public TimeRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}
