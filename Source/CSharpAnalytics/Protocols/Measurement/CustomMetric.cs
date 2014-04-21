// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics.Protocols.Measurement
{
    internal class CustomMetric
    {
        private readonly int index;
        private readonly object metric;

        public CustomMetric(int index, string metric)
        {
            this.index = index;
            this.metric = metric;
        }

        public int Index { get { return index; } }
        public object Metric { get { return metric; } }
    }
}