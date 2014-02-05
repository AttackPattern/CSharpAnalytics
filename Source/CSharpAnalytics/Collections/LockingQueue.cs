// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.Generic;

namespace CSharpAnalytics.Collections
{
    internal class LockingQueue<T>
    {
        private readonly object locking = new object();
        private readonly Queue<T> queue;

        public LockingQueue()
        {
        }

        public LockingQueue(IEnumerable<T> collection)
        {
            queue = new Queue<T>(collection);
        }

        internal bool TryDequeue(out T entry)
        {
            lock (locking)
            {
                if (queue.Count == 0)
                {
                    entry = default(T);
                    return false;
                }

                entry = queue.Dequeue();
                return true;
            }
        }

        public void Enqueue(T item)
        {
            lock (locking)
            {
                queue.Enqueue(item);
            }
        }

        public int Count
        {
            get { return queue.Count; }
        }

        public List<T> ToList()
        {
            lock (locking)
                return new List<T>(queue);
        }
    }
}