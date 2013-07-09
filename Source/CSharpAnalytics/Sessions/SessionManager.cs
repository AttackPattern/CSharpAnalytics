﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics.Sessions
{
    public enum SessionStatus { Starting, Active, Ending };
    public enum VisitorStatus { Active, OptedOut };

    /// <summary>
    /// Manages visitors and sessions to ensure they are correctly saved, restored and time-out as appropriate.
    /// </summary>
    public class SessionManager
    {
        private readonly Visitor visitor;

        protected DateTimeOffset lastActivityAt = DateTimeOffset.Now;
        protected readonly object newSessionLock = new object();

        /// <summary>
        /// Recreate a SessionManager from state.
        /// </summary>
        /// <param name="timeout">How long before a session will expire if no activity is seen.</param>
        /// <param name="sessionState">SessionState containing details captured from a previous SessionManager or null if no previous SessionManager.</param>
        /// <returns>Recreated SessionManager.</returns>
        public SessionManager(SessionState sessionState)
        {
            if (sessionState != null)
            {
                visitor = new Visitor(sessionState.VisitorId, sessionState.FirstVisitAt);
                Session = new Session(sessionState.SessionStartedAt, sessionState.SessionNumber, sessionState.SessionHitCount, sessionState.HitId);
                Referrer = sessionState.Referrer;
                PreviousSessionStartedAt = sessionState.PreviousSessionStartedAt;
                lastActivityAt = sessionState.LastActivityAt;
                SessionStatus = sessionState.SessionStatus;
                VisitorStatus = sessionState.VisitorStatus;
            }
            else
            {
                visitor = new Visitor();
                Session = new Session();
                PreviousSessionStartedAt = Session.StartedAt;
                SessionStatus = SessionStatus.Starting;
                VisitorStatus = VisitorStatus.Active;
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
        /// Current status of this session.
        /// </summary>
        public SessionStatus SessionStatus { get; private set; }

        /// <summary>
        /// Current status of this visitor.
        /// </summary>
        public VisitorStatus VisitorStatus { get; internal set; }

        /// <summary>
        /// Current session.
        /// </summary>
        public Session Session { get; private set; }

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
            return new SessionState
            {
                FirstVisitAt = Visitor.FirstVisitAt,
                VisitorId = Visitor.ClientId,
                SessionStartedAt = Session.StartedAt,
                SessionHitCount = Session.HitCount,
                HitId = Session.HitId,
                SessionNumber = Session.Number,
                PreviousSessionStartedAt = PreviousSessionStartedAt,
                LastActivityAt = lastActivityAt,
                Referrer = Referrer,
                SessionStatus = SessionStatus,
                VisitorStatus = VisitorStatus
            };
        }

        /// <summary>
        /// Record a hit to this session to ensure counts and timeouts are honoured.
        /// </summary>
        internal virtual void Hit()
        {
            lastActivityAt = DateTimeOffset.Now;

            MoveToNextSessionStatus();

            Session.IncreaseHitCount();
        }

        /// <summary>
        /// Move to the next session status, e.g. Ending to Starting, Starting to Active.
        /// </summary>
        private void MoveToNextSessionStatus()
        {
            switch (SessionStatus)
            {
                case SessionStatus.Ending:
                    SessionStatus = SessionStatus.Starting;
                    break;
                case SessionStatus.Starting:
                    SessionStatus = SessionStatus.Active;
                    break;
            }
        }

        /// <summary>
        /// End the current session.
        /// </summary>
        internal void End()
        {
            SessionStatus = SessionStatus.Ending;
        }

        /// <summary>
        /// Start a new session.
        /// </summary>
        /// <param name="startedAt">When this session started.</param>
        protected void StartNewSession(DateTimeOffset startedAt)
        {
            lock (newSessionLock)
            {
                PreviousSessionStartedAt = Session.StartedAt;
                Session = new Session(startedAt, Session.Number + 1);
                SessionStatus = SessionStatus.Starting;
            }
        }
    }
}