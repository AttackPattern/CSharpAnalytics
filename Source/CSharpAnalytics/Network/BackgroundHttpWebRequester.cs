﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace CSharpAnalytics.Network
{
    /// <summary>
    /// Responsible for requesting a queue of URIs over HTTP or HTTPS in background using HttpWebRequest.
    /// </summary>
    public class BackgroundHttpWebRequester : BackgroundHttpRequester
    {
        private readonly Action<HttpWebRequest> preprocessor;

        /// <summary>
        /// Create a new BackgroundHttpWebRequester.
        /// </summary>
        /// <param name="preprocessor">Optional preprocessor for setting user agents, debugging etc.</param>
        public BackgroundHttpWebRequester(Action<HttpWebRequest> preprocessor = null)
        {
            this.preprocessor = preprocessor;
        }

        /// <summary>
        /// Request the URI with retry logic using HttpWebRequest.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        protected override void RequestWithFailureRetry(Uri requestUri)
        {
            var retryDelay = TimeSpan.Zero;
            var successfullySent = false;

            do
            {
                var request = CreateRequest(requestUri);
                if (preprocessor != null)
                    preprocessor(request);

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse) request.GetResponse();
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                        ex = GetInnermostException(ex);

                    Debug.WriteLine("{0} failing with {1}", GetType().Name, ex.Message);
                }
                finally
                {
                    if (response == null || response.StatusCode != HttpStatusCode.OK)
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

        /// <summary>
        /// Creates the HttpWebRequest for a URI taking into consideration the length.
        /// For URIs over 2000 bytes it will be a GET otherwise it will become a POST
        /// with the query payload moved to the POST body.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <returns>HttpWebRequest for this URI.</returns>
        internal static HttpWebRequest CreateRequest(Uri requestUri)
        {
            return requestUri.AbsoluteUri.Length <= MaxUriLength
                       ? CreateGetRequest(requestUri)
                       : CreatePostRequest(requestUri);
        }

        /// <summary>
        /// Create a HttpWebRequest using the HTTP GET method.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <returns>HttpWebRequest for this URI.</returns>
        private static HttpWebRequest CreateGetRequest(Uri requestUri)
        {
            var getRequest = WebRequest.CreateHttp(requestUri);
            getRequest.Method = "GET";
            return getRequest;
        }

        /// <summary>
        /// Create a HttpWebRequest using the HTTP POST method.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <returns>HttpWebRequest for this URI.</returns>
        private static HttpWebRequest CreatePostRequest(Uri requestUri)
        {
            var uriWithoutQuery = new Uri(requestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));
            var bodyWithQuery = requestUri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);
            var bodyBytes = Encoding.UTF8.GetBytes(bodyWithQuery);

            var postRequest = WebRequest.CreateHttp(uriWithoutQuery);
            postRequest.Method = "POST";
            postRequest.ContentLength = bodyBytes.Length;
            var stream = postRequest.GetRequestStream();
            stream.Write(bodyBytes, 0, bodyBytes.Length);
            stream.Close();

            return postRequest;
        }
    }
}