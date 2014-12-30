// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Threading.Tasks;
using CSharpAnalytics.Environment;
using CSharpAnalytics.Network;
using CSharpAnalytics.Portable45.Serializers;

namespace CSharpAnalytics
{
    public class ManualMeasurement : BaseAutoMeasurement
    {
        private readonly Func<bool> isInternetAvailableFunc;
        private readonly IEnvironment environment;
        private readonly IEventSerializer eventSerializer;

        public ManualMeasurement(Func<bool> isInternetAvailableFunc, IEnvironment environment = null, IEventSerializer eventSerializer = null)
        {
            if (isInternetAvailableFunc == null) throw new ArgumentNullException("isInternetAvailableFunc");

            this.isInternetAvailableFunc = isInternetAvailableFunc;
            this.environment = environment ?? new ManualEnvironment();
            this.eventSerializer = eventSerializer;
        }

        protected override void HookEvents()
        {
        }

        protected override void UnhookEvents()
        {
        }

        protected override IEnvironment GetEnvironment()
        {
            return environment;
        }

        protected override Task<T> Load<T>(string name)
        {
            return eventSerializer != null ? eventSerializer.Load<T>(name) : Task.FromResult(default(T));
        }

        protected override Task Save<T>(T data, string name)
        {
            return eventSerializer != null ? eventSerializer.Save(data, name) : Task.FromResult(0);
        }

        protected override Task SetupRequesterAsync()
        {
            var httpClientRequester = new HttpClientRequester();
            httpClientRequester.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ClientUserAgent);

            Requester = httpClientRequester.Request;

            return Task.FromResult(true);
        }

        protected override bool IsInternetAvailable()
        {
            return isInternetAvailableFunc();
        }
    }
}