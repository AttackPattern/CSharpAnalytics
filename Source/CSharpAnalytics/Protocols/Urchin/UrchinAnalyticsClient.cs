﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Linq;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Urchin.CustomVariables;
using CSharpAnalytics.Sessions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// UrchinAnalyticsClient should exist for the scope of your application and is the primary entry point for tracking via Urchin.
    /// </summary>
    public class UrchinAnalyticsClient
    {
        private readonly Queue<UrchinActivityEntry> queue = new Queue<UrchinActivityEntry>();

        private UrchinTracker tracker;

        /// <summary>
        /// Create a new AnalyticsClient with a given configuration, session, environment and URI sender.
        /// </summary>
        /// <param name="configuration">Configuration of this Google Analytics Urchin client.</param>
        /// <param name="sessionManager">Session manager with visitor and session information.</param>
        /// <param name="environment">Provider of environmental information such as screen resolution.</param>
        /// <param name="sender">Action to take prepared URIs for Google Analytics and send them on.</param>
        public void Configure(UrchinConfiguration configuration, SessionManager sessionManager, IEnvironment environment, Action<Uri> sender)
        {
            Debug.Assert(tracker == null);
            var newTracker = new UrchinTracker(configuration, sessionManager, environment, sender);
            while (queue.Count > 0)
                newTracker.Track(queue.Dequeue());
            tracker = newTracker;
        }

        /// <summary>
        /// Track this activity in analytics.
        /// </summary>
        /// <param name="activity">Activity to track in analytics.</param>
        /// <param name="activityCustomVariables">Activity scoped custom variable slots to record for this activity.</param>
        public void Track(IUrchinActivity activity, CustomVariableSlots activityCustomVariables = null)
        {
            if (activity is AutoTimedEventActivity)
                ((AutoTimedEventActivity)activity).End();

            var entry = new UrchinActivityEntry
            {
                Activity = activity,
                CustomVariables = GetFinalCustomVariables(activityCustomVariables)
            };

            if (tracker == null)
                queue.Enqueue(entry);
            else
                tracker.Track(entry);
        }

        /// <summary>
        /// Expands a set of scoped custom variable slots into a final array containing the custom variables and their scope.
        /// </summary>
        /// <param name="activityCustomVariables">Set of scoped custom variable slots to expand.</param>
        /// <returns>An array of custom variables with their scope indexed by slot.</returns>
        internal ScopedCustomVariableSlot[] GetFinalCustomVariables(CustomVariableSlots activityCustomVariables)
        {
            var finalSlots = new Dictionary<int, ScopedCustomVariableSlot>();

            if (activityCustomVariables != null)
                foreach (var slot in activityCustomVariables.AllSlots)
                    finalSlots[slot.Key] = new ScopedCustomVariableSlot(CustomVariableScope.Activity, slot.Value, slot.Key);

            foreach (var slot in SessionCustomVariables.AllSlots)
                finalSlots[slot.Key] = new ScopedCustomVariableSlot(CustomVariableScope.Session, slot.Value, slot.Key);

            foreach (var slot in VisitorCustomVariables.AllSlots)
                finalSlots[slot.Key] = new ScopedCustomVariableSlot(CustomVariableScope.Visitor, slot.Value, slot.Key);

            return finalSlots.Values.ToArray();
        }

        /// <summary>
        /// Custom variables currently declared for this visitor.
        /// </summary>
        public CustomVariableSlots VisitorCustomVariables = new CustomVariableSlots();

        /// <summary>
        /// Custom variables currently declared for this session.
        /// </summary>
        public CustomVariableSlots SessionCustomVariables = new CustomVariableSlots();
    }
}