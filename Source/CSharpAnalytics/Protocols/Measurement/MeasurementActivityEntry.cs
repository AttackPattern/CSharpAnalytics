using System.Collections.Generic;

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// Records a measurement activity and its associated data for tracking.
    /// </summary>
    internal class MeasurementActivityEntry
    {
        public IMeasurementActivity Activity;
        public IEnumerable<KeyValuePair<int, string>> CustomDimensions;
        public IEnumerable<KeyValuePair<int, long?>> CustomMetrics;
        public bool EndSession;
    }
}