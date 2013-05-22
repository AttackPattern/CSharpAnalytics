﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Protocols.Urchin.CustomVariables;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// Captures a measurement activity and associated data for tracking.
    /// </summary>
    internal class UrchinActivityEntry
    {
        /// <summary>
        /// Activity being captured.
        /// </summary>
        public IUrchinActivity Activity;

        /// <summary>
        /// Custom variables captured for this activity.
        /// </summary>
        public ScopedCustomVariableSlot[] CustomVariables;
    }
}