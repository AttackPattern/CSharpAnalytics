﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace CSharpAnalytics.Network
{
    /// <summary>
    /// Responsible for requesting a queue of URIs over HTTP or HTTPS in background using HttpClient.
    /// </summary>
    public class BackgroundHttpClientRequester : BackgroundHttpRequester
    {
        private readonly Action<HttpRequestMessage> preprocessor;
        private readonly Func<bool> checkInternetAvailable; 

        /// <summary>
        /// Create a new BackgroundHttpClientRequester.
        /// </summary>
        /// <param name="preprocessor">Optional preprocessor for setting user agents, debugging etc.</param>
        /// <param name="checkInternetAvailable">Optional func to determine if the Internet is available.</param>
        public BackgroundHttpClientRequester(Action<HttpRequestMessage> preprocessor = null, Func<bool> checkInternetAvailable = null)
        {
            this.preprocessor = preprocessor;
            this.checkInternetAvailable = checkInternetAvailable;
        }

        /// <summary>
        /// Request the URI with retry logic using HttpClient.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <param name="cancellationToken">Token to indicate if request should be abandoned.</param>
        protected override void RequestWithFailureRetry(Uri requestUri, CancellationToken cancellationToken)
        {
            var retryDelay = TimeSpan.Zero;
            var successfullySent = false;

            using (var httpClient = new HttpClient())
            {
                do
                {
                    var message = CreateRequest(requestUri);
                    if (preprocessor != null)
                        preprocessor(message);

                    HttpResponseMessage response = null;
                    try
                    {
                        response = httpClient.SendAsync(message, cancellationToken).Result;
                    }
                    catch (Exception ex)
                    {
                        if (ex is AggregateException)
                            ex = GetInnermostException(ex);

                        Debug.WriteLine("{0} failing with {1}", GetType().Name, ex.Message);
                    }
                    finally
                    {
                        if (response == null || !response.IsSuccessStatusCode)
                        {
                            WaitBetweenFailedRequests(ref retryDelay);
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
        /// For URIs over 2000 bytes it will be a GET otherwise it will become a POST
        /// with the query payload moved to the POST body.
        /// </summary>
        /// <param name="uri">URI to request.</param>
        /// <returns>HttpRequestMessage for this URI.</returns>
        internal static HttpRequestMessage CreateRequest(Uri uri)
        {
            if (!ShouldUsePostForRequest(uri))
                return new HttpRequestMessage(HttpMethod.Get, uri);

            var uriWithoutQuery = new Uri(uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));
            return new HttpRequestMessage(HttpMethod.Post, uriWithoutQuery) { Content = new StringContent(uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped)) };
        }

        /// <summary>
        /// Determines whether a connection to the Internet is available.
        /// </summary>
        protected override bool IsInternetAvailable
        {
            get
            {
                return checkInternetAvailable == null ? base.IsInternetAvailable : checkInternetAvailable();
            }
        }
    }
}