// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics.Protocols.Measurement
{
    internal class CustomDimension
    {
        private readonly int index;
        private readonly string dimension;

        public CustomDimension(int index, string dimension)
        {
            this.index = index;
            this.dimension = dimension;
        }

        public int Index { get { return index; } }
        public string Dimension { get { return dimension; } }
    }
}