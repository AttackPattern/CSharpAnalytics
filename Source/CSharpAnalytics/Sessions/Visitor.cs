﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics.Sessions
{
    /// <summary>
    /// Represents a Visitor or user of the application.
    /// </summary>
    public class Visitor
    {
        private readonly Guid id;
        private readonly DateTimeOffset firstVisitAt;

        /// <summary>
        /// Create a brand-new Visitor.
        /// </summary>
        internal Visitor()
            : this(Guid.NewGuid(), DateTimeOffset.Now)
        {
        }

        /// <summary>
        /// Create an existing Visitor.
        /// </summary>
        /// <param name="id">Unique Id of the existing Visitor.</param>
        /// <param name="firstVisitAt">When the first visit occured.</param>
        internal Visitor(Guid id, DateTimeOffset firstVisitAt)
        {
            this.id = id;
            this.firstVisitAt = firstVisitAt;
        }

        /// <summary>
        /// Unique Id of this user.
        /// </summary>
        public Guid Id { get { return id; } }

        /// <summary>
        /// Earliest recorded visit for this Visitor.
        /// </summary>
        public DateTimeOffset FirstVisitAt { get { return firstVisitAt; } }
    }
}