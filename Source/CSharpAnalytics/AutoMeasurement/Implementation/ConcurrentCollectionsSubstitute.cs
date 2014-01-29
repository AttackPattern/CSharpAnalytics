﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

#if WINDOWS_PHONE
using System.Collections.Generic;

namespace System.Collections.Concurrent
{
    internal class ConcurrentDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
    }

    internal class ConcurrentQueue<T> : Queue<T>
    {
        private readonly object locking = new object();

        public ConcurrentQueue()
        {            
        }

        public ConcurrentQueue(IEnumerable<T> collection)
            : base(collection)
        {            
        }

        internal bool TryDequeue(out T entry)
        {
            lock (locking)
            {
                if (Count == 0)
                {
                    entry = default(T);
                    return false;
                }
                
                entry = Dequeue();
                return true;
            }
        }
    }
}
#endif