using System;

namespace DEXR.UnitTest
{
    public class StressTestOptions
    {
        public string ApplicationFolder { get; set; }
        public int SeedNodePort { get; set; }
        public int NumberOfClones { get; set; }
        public int CloneStartPort { get; set; }
        public int TxPerInterval { get; set; }
        public int IntervalInSeconds { get; set; }
    }
}
