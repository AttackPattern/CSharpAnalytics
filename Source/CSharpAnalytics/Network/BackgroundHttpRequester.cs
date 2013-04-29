﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpAnalytics.Network
{
    /// <summary>
    /// Responsible for requesting a queue of URIs over HTTP or HTTPS in background.
    /// </summary>
    public class BackgroundHttpRequester : IDisposable
    {
        private const int MaxUriLength = 2000;

        private static readonly TimeSpan networkRetryWaitStep = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan networkRetryWaitMax = TimeSpan.FromMinutes(10);

        private readonly Queue<Uri> currentRequests = new Queue<Uri>();
        private readonly Action<HttpRequestMessage> preprocessor;

        private CancellationTokenSource cancellationTokenSource;
        private Queue<Uri> priorRequests = new Queue<Uri>();
        private Task backgroundSender;
        private TimeSpan currentUploadInterval;

        /// <summary>
        /// Determines whether the BackgroundHttpRequester is currently started.
        /// </summary>
        public bool IsStarted { get { return cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested; } }

        /// <summary>
        /// Create a new BackgroundHttpRequester.
        /// </summary>
        /// <param name="preprocessor">Optional preprocessor for setting user agents, debugging etc.</param>
        public BackgroundHttpRequester(Action<HttpRequestMessage> preprocessor = null)
        {
            this.preprocessor = preprocessor;
        }

        /// <summary>
        /// Add a URI to be requested to the queue.
        /// </summary>
        /// <param name="uri">URI to be requested.</param>
        public void Add(Uri uri)
        {
            currentRequests.Enqueue(uri);
        }

        /// <summary>
        /// Start the BackgroundHttpRequester with a given upload interval and a list of previously unrequested URIs.
        /// </summary>
        /// <param name="uploadInterval">How often to send the contents of the queue.</param>
        /// <param name="previouslyUnrequested">List of previously unrequested URIs obtained last time the requester was stopped.</param>
        public void Start(TimeSpan uploadInterval, IEnumerable<Uri> previouslyUnrequested = null)
        {
            if (IsStarted)
                throw new InvalidOperationException(String.Format("Cannot start a {0} when already started", typeof(BackgroundHttpRequester).Name));

            if (previouslyUnrequested != null)
                priorRequests = new Queue<Uri>(previouslyUnrequested);

            cancellationTokenSource = new CancellationTokenSource();
            currentUploadInterval = uploadInterval;
            backgroundSender = Task.Factory.StartNew(RequestLoop, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Stop the BackgroundHttpRequester and return a list of URIs that were not requested.
        /// </summary>
        /// <returns>List of unrequested URIs that should be passed into Start next time.</returns>
        public async Task<List<Uri>> StopAsync()
        {
            if (!IsStarted)
                throw new InvalidOperationException(String.Format("Cannot stop a {0} when already stopped", typeof(BackgroundHttpRequester).Name));

            cancellationTokenSource.Cancel();
            await backgroundSender;

            return priorRequests.Concat(currentRequests).ToList();
        }

        /// <summary>
        /// Loop that keeps requesting URIs in the queue until there are none left, then sleeps.
        /// </summary>
        private void RequestLoop()
        {
            using (var queueEmptyWait = new ManualResetEventSlim())
            {
                try
                {
                    while (IsStarted)
                    {
                        // Always empty the priorRequest queue first
                        var requestQueue = priorRequests.Count > 0 ? priorRequests : currentRequests;
                        // Send all the requests we currently have
                        while (requestQueue.Count > 0)
                        {
                            // Don't dequeue until successfully sent to avoid loss
                            RequestWithFailureRetry(requestQueue.Peek());
                            requestQueue.Dequeue();
                        }

                        queueEmptyWait.Wait(currentUploadInterval, cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        /// <summary>
        /// Request the URI with retry logic.
        /// </summary>
        /// <param name="uri">URI to request.</param>
        private void RequestWithFailureRetry(Uri uri)
        {
            var retryDelay = TimeSpan.Zero;
            var successfullySent = false;

            using (var requestFailureWait = new ManualResetEventSlim())
            {
                do
                {
                    var message = CreateRequestMessage(uri);
                    if (preprocessor != null)
                        preprocessor(message);

                    HttpResponseMessage response = null;
                    try
                    {
                        response = new HttpClient().SendAsync(message).Result;
                    }
                    catch (AggregateException e)
                    {
                        System.Diagnostics.Debug.WriteLine("BackgroundHttp failing with {0}", GetInnermostException(e).Message);
                    }
                    finally
                    {
                        if (response == null || !response.IsSuccessStatusCode)
                        {
                            retryDelay = retryDelay + networkRetryWaitStep;
                            if (retryDelay > networkRetryWaitMax)
                                retryDelay = networkRetryWaitMax;

                            requestFailureWait.Wait(retryDelay, cancellationTokenSource.Token);
                        }
                        else
                        {
                            successfullySent = true;
                        }
                    }
                } while (!successfullySent);
            }
        }

        /// <summary>
        /// Creates the HttpRequestMessage for a URI taking into consideration the length.
        /// For Uri's over 2000 bytes it will be a GET otherwise it will become a POST
        /// with the query payload moved to the POST body.
        /// </summary>
        /// <param name="uri">URI to request.</param>
        /// <returns>HttpRequestMessage for this URI.</returns>
        internal static HttpRequestMessage CreateRequestMessage(Uri uri)
        {
            if (uri.AbsoluteUri.Length <= MaxUriLength)
                return new HttpRequestMessage(HttpMethod.Get, uri);

            var uriWithoutQuery = new Uri(uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));
            return new HttpRequestMessage(HttpMethod.Post, uriWithoutQuery) { Content = new StringContent(uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped)) };
        }

        /// <summary>
        /// Total count of all remaining items in the queue to aid tests.
        /// </summary>
        internal int QueueCount
        {
            get { return priorRequests.Count + currentRequests.Count; }
        }

        /// <summary>
        /// Obtain the innermost Exception from within an Exception.
        /// </summary>
        /// <param name="ex">Exception to obtain the innermost exception from.</param>
        /// <returns>Innermost exception that could be obtained.</returns>
        private static Exception GetInnermostException(Exception ex)
        {
            var nextException = ex;
            while (nextException.InnerException != null)
                nextException = nextException.InnerException;
            return nextException;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            var safeCancellationTokenSource = cancellationTokenSource;
            if (isDisposing && safeCancellationTokenSource != null)
            {
                safeCancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
    }
}