using System;
using System.Threading;

namespace CSharpAnalytics.Sessions
{
    /// <summary>
    /// Represents a session of user activity.
    /// </summary>
    public class Session
    {
        private int hitCount;
        private readonly long hitId;
        private readonly int number;
        private readonly DateTimeOffset startedAt;

        /// <summary>
        /// When this session started.
        /// </summary>
        public DateTimeOffset StartedAt { get { return startedAt; } }

        /// <summary>
        /// Number of hits in this session so far.
        /// </summary>
        public int HitCount { get { return hitCount; } }

        /// <summary>
        /// Session number for this Visitor that counts up by one each time.
        /// </summary>
        public int Number { get { return number; } }

        /// <summary>
        /// Random Id used to ensure any web cache is bypassed.
        /// </summary>
        public long HitId { get { return hitId; } }

        /// <summary>
        /// Creates the first ever session for a Visitor.
        /// </summary>
        public Session()
            : this(DateTimeOffset.Now, 1)
        {
        }

        /// <summary>
        /// Create a new session given a specific start time and session number. Used to create the next session.
        /// </summary>
        /// <param name="startedAt">When this session started at.</param>
        /// <param name="number">Session number.</param>
        public Session(DateTimeOffset startedAt, int number)
            : this(startedAt, number, 0, new Random().Next())
        {
        }

        /// <summary>
        /// Create a new session given all the parameters possible. Used to restore sessions from state before the timeout occurs.
        /// </summary>
        /// <param name="startedAt">When this session started at.</param>
        /// <param name="number">Session number.</param>
        /// <param name="hitCount">Number of hits in this session so far.</param>
        /// <param name="hitId">Random Id used to ensure any web cache is bypassed.</param>
        public Session(DateTimeOffset startedAt, int number, int hitCount, long hitId)
        {
            this.startedAt = startedAt;
            this.number = number;
            this.hitCount = hitCount;
            this.hitId = hitId;
        }

        /// <summary>
        /// Increase the hit count for this session in a thread-safe way.
        /// </summary>
        public void IncreaseHitCount()
        {
            Interlocked.Increment(ref hitCount);
        }
    }
}