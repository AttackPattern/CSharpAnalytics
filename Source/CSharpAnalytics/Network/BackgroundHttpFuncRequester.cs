﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;

namespace CSharpAnalytics.Network
{
    /// <summary>
    /// Responsible for requesting a queue of URIs over HTTP or HTTPS in background using a given Func.
    /// </summary>
    public class BackgroundHttpFuncRequester : BackgroundHttpRequester
    {
        private readonly Func<Uri, bool> requester;

        /// <summary>
        /// Create a new BackgroundHttpFuncRequester.
        /// </summary>
        /// <param name="requester">Func to perform the request that will return true if successful.</param>
        public BackgroundHttpFuncRequester(Func<Uri, bool> requester)
        {
            this.requester = requester;
        }

        /// <summary>
        /// Request the URI with retry logic using HttpClient.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        protected override void RequestWithFailureRetry(Uri requestUri)
        {
            var retryDelay = TimeSpan.Zero;
            var successfullySent = false;

            do
            {
                try
                {
                    successfullySent = requester(requestUri);
                }
                catch (AggregateException e)
                {
                    Debug.WriteLine("{0} failing with {1}", GetType().Name, GetInnermostException(e).Message);
                }
                finally
                {
                    WaitBetweenFailedRequests(ref retryDelay);
                }
            } while (!successfullySent);
        }
    }
}