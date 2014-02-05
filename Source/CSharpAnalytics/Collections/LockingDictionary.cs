// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.Generic;
using System.Linq;

namespace CSharpAnalytics.Collections
{
    internal class LockingDictionary<TKey, TValue>
    {
        private readonly object locking = new object();
        private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public void Clear()
        {
            lock (locking)
                dictionary.Clear();
        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            lock (locking)
                return dictionary.ToArray();
        }

        public TValue this[TKey key]
        {
            get { return dictionary[key]; }
            set
            {
                lock (locking)
                    dictionary[key] = value;
            }
        }
    }
}