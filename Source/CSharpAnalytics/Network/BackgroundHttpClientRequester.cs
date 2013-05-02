﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;
using System.Net.Http;

namespace CSharpAnalytics.Network
{
    /// <summary>
    /// Responsible for requesting a queue of URIs over HTTP or HTTPS in background using HttpClient.
    /// </summary>
    public class BackgroundHttpClientRequester : BackgroundHttpRequester
    {
        private readonly Action<HttpRequestMessage> preprocessor;

        /// <summary>
        /// Create a new BackgroundHttpRequester.
        /// </summary>
        /// <param name="preprocessor">Optional preprocessor for setting user agents, debugging etc.</param>
        public BackgroundHttpClientRequester(Action<HttpRequestMessage> preprocessor = null)
        {
            this.preprocessor = preprocessor;
        }

        /// <summary>
        /// Request the URI with retry logic using HttpClient.
        /// </summary>
        /// <param name="uri">URI to request.</param>
        protected override void RequestWithFailureRetry(Uri uri)
        {
            var retryDelay = TimeSpan.Zero;
            var successfullySent = false;

            using (var httpClient = new HttpClient())
            {
                do
                {
                    var message = CreateRequestMessage(uri);
                    if (preprocessor != null)
                        preprocessor(message);

                    HttpResponseMessage response = null;
                    try
                    {
                        response = httpClient.SendAsync(message).Result;
                    }
                    catch (AggregateException e)
                    {
                        Debug.WriteLine("{0} failing with {1}", GetType().Name, GetInnermostException(e).Message);
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
        internal static HttpRequestMessage CreateRequestMessage(Uri uri)
        {
            if (uri.AbsoluteUri.Length <= MaxUriLength)
                return new HttpRequestMessage(HttpMethod.Get, uri);

            var uriWithoutQuery = new Uri(uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));
            return new HttpRequestMessage(HttpMethod.Post, uriWithoutQuery) { Content = new StringContent(uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped)) };
        }
    }
}