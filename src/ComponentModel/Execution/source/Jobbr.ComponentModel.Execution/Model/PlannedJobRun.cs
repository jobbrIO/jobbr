using System;

namespace Jobbr.ComponentModel.Execution.Model
{
    public class PlannedJobRun
    {
        public long Id { get; set; }

        public DateTime PlannedStartDateTimeUtc { get; set; }
    }
}
