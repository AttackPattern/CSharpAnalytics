// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Threading.Tasks;

namespace CSharpAnalytics.Portable45.Serializers
{
    public interface IEventSerializer
    {
        Task<T> Load<T>(string name);
        Task Save<T>(T data, string name);
    }
}