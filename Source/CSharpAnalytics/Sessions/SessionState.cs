using System;
using System.Runtime.Serialization;

namespace CSharpAnalytics.Sessions
{
    /// <summary>
    /// Memento to capture all SessionManager details for persisting between application runs.
    /// </summary>
    [DataContract(Namespace="CSharpAnalytics")]
    public class SessionState
    {
        // Visitor
        [DataMember] public int VisitorId { get; set; }
        [DataMember] public DateTimeOffset FirstVisitAt { get; set; }

        // Session
        [DataMember] public DateTimeOffset SessionStartedAt { get; set; }
        [DataMember] public int SessionHitCount { get; set; }
        [DataMember] public int SessionNumber { get; set; }
        [DataMember] public long HitId { get; set; }

        // Internal
        [DataMember] public DateTimeOffset PreviousSessionStartedAt { get; set; }
        [DataMember] public DateTimeOffset LastActivityAt { get; set; }
        [DataMember] public Uri Referrer { get; set; }
    }
}