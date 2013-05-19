﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics.Sessions
{
    public enum SessionStatus { Starting, Active, Ending };

    /// <summary>
    /// Manages visitors and sessions to ensure they are correctly saved, restored and time-out as appropriate.
    /// </summary>
    public class SessionManager
    {
        private readonly object newSessionLock = new object();
        private readonly TimeSpan timeout;
        private readonly Visitor visitor;
        private DateTimeOffset lastActivityAt = DateTimeOffset.Now;

        public SessionStatus SessionStatus { get; private set; }

        /// <summary>
        /// Recreate a SessionManager from state.
        /// </summary>
        /// <param name="timeout">How long before a session will expire if no activity is seen.</param>
        /// <param name="sessionState">SessionState containing details captured from a previous SessionManager.</param>
        /// <returns>Recreated SessionManager.</returns>
        public SessionManager(TimeSpan timeout, SessionState sessionState)
        {
            this.timeout = timeout;
            SessionStatus = SessionStatus.Starting;

            if (sessionState != null)
            {
                visitor = new Visitor(sessionState.VisitorId, sessionState.FirstVisitAt);
                Session = new Session(sessionState.SessionStartedAt, sessionState.SessionNumber, sessionState.SessionHitCount, sessionState.HitId);
                Referrer = sessionState.Referrer;
                PreviousSessionStartedAt = sessionState.PreviousSessionStartedAt;
                lastActivityAt = sessionState.LastActivityAt;
            }
            else
            {
                visitor = new Visitor();
                Session = new Session();
                PreviousSessionStartedAt = Session.StartedAt;
            }
        }

        /// <summary>
        /// Manually start a new session. Useful for scoping out session custom variables, e.g. if an anonymous user
        /// becomes known.
        /// </summary>
        public void StartNewSession()
        {
            StartNewSession(DateTimeOffset.Now);   
        }

        /// <summary>
        /// Current session.
        /// </summary>
        public Session Session { get; private set; }

        /// <summary>
        /// How long before a session will expire if no activity is seen.
        /// </summary>
        public TimeSpan Timeout { get { return timeout; } }

        /// <summary>
        /// Visitor.
        /// </summary>
        public Visitor Visitor { get { return visitor; } }

        /// <summary>
        /// When the previous session started at.
        /// </summary>
        public DateTimeOffset PreviousSessionStartedAt { get; private set; }

        /// <summary>
        /// Last page or URI visited to act as referrer for subsequent ones.
        /// </summary>
        public Uri Referrer { get; internal set; }

        /// <summary>
        /// Capture details of the SessionManager into a SessionState that can be safely stored and restored.
        /// </summary>
        /// <returns>SessionState representing the current state of the SessionManager.</returns>
        public SessionState GetState()
        {
            return new SessionState {
                FirstVisitAt = Visitor.FirstVisitAt,
                VisitorId = Visitor.ClientId,
                SessionStartedAt = Session.StartedAt,
                SessionHitCount = Session.HitCount,
                HitId = Session.HitId,
                SessionNumber = Session.Number,
                PreviousSessionStartedAt = PreviousSessionStartedAt,
                LastActivityAt = lastActivityAt,
                Referrer = Referrer
            };
        }

        /// <summary>
        /// Record a hit to this session to ensure counts and timeouts are honoured.
        /// </summary>
        internal void Hit()
        {
            var now = DateTimeOffset.Now;

            switch (SessionStatus)
            {
                case SessionStatus.Ending:
                    SessionStatus = SessionStatus.Starting;
                    break;
                case SessionStatus.Starting:
                    SessionStatus = SessionStatus.Active;
                    break;
            }

            StartNewSessionIfTimedOut(now);

            if (now > lastActivityAt)
                lastActivityAt = now;

            Session.IncreaseHitCount();
        }

        /// <summary>
        /// Tell the session manager it is ending.
        /// </summary>
        internal void End()
        {
            SessionStatus = SessionStatus.Ending;
        }

        /// <summary>
        /// Starts are new session if the previous one has expired.
        /// </summary>
        /// <param name="activityStartedAt">When this hit activity started.</param>
        private void StartNewSessionIfTimedOut(DateTimeOffset activityStartedAt)
        {
            // Two threads could trigger activities back to back after a session ends, e.g. restarting the app
            // after some time spent suspended.  Only let one of them cause a new session to be started.
            while (TimeSinceLastActivity(activityStartedAt) > timeout)
            {
                lock (newSessionLock)
                {
                    if (TimeSinceLastActivity(activityStartedAt) > timeout)
                        StartNewSession(activityStartedAt);
                    lastActivityAt = activityStartedAt;
                }
            }
        }

        /// <summary>
        /// Calculate the elapsed time since the last activity.
        /// </summary>
        /// <param name="nextActivityTime">Next activity start time.</param>
        /// <returns>Elapsed time since the last activity.</returns>
        private TimeSpan TimeSinceLastActivity(DateTimeOffset nextActivityTime)
        {
            return nextActivityTime - lastActivityAt;
        }

        /// <summary>
        /// Start a new session for this Visitor.
        /// </summary>
        /// <param name="startedAt">When this session started at.</param>
        private void StartNewSession(DateTimeOffset startedAt)
        {
            lock (newSessionLock) {
                PreviousSessionStartedAt = Session.StartedAt;
                Session = new Session(startedAt, Session.Number + 1);
                SessionStatus = SessionStatus.Starting;
            }
        }
    }
}