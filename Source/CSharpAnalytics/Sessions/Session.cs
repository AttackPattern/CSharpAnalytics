﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics.Sessions
{
    /// <summary>
    /// Represents a session of user activity.
    /// </summary>
    public class Session
    {
        private readonly int number;
        private readonly DateTimeOffset startedAt;

        /// <summary>
        /// When this session started.
        /// </summary>
        public DateTimeOffset StartedAt { get { return startedAt; } }

        /// <summary>
        /// Session number for this Visitor that counts up by one each time.
        /// </summary>
        public int Number { get { return number; } }

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
        {
            this.startedAt = startedAt;
            this.number = number;
        }
    }
}