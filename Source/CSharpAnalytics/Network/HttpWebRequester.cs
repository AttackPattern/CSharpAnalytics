﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpAnalytics.Network
{
    /// <summary>
    /// Responsible for requesting a queue of URIs over HTTP or HTTPS in background using HttpWebRequest.
    /// </summary>
    public class HttpWebRequester
    {
        private readonly string userAgent;

        /// <summary>
        /// Create a new HttpWebRequester.
        /// </summary>
        /// <param name="userAgent">User agent string.</param>
        public HttpWebRequester(string userAgent)
        {
            this.userAgent = userAgent;
        }

        /// <summary>
        /// Request the URI with retry logic using HttpWebRequest.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <param name="cancellationToken">CancellationToken to indicate if the request should be cancelled.</param>
#if WINDOWS_PHONE_APP
        public async Task<bool> Request(Uri requestUri, CancellationToken cancellationToken)
#else
        public bool Request(Uri requestUri, CancellationToken cancellationToken)
#endif
        {
#if WINDOWS_PHONE_APP
            var request = await CreateRequest(requestUri);
            request.Headers[HttpRequestHeader.UserAgent] = userAgent;
            var response = await request.GetResponseAsync();
#else
            var request = CreateRequest(requestUri);
            request.Headers.Add(HttpRequestHeader.UserAgent, userAgent);
            var response = (HttpWebResponse)request.GetResponse();
#endif
            var httpResponse = (HttpWebResponse)response;
            return httpResponse.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Creates the HttpWebRequest for a URI taking into consideration the length.
        /// For URIs over 2000 bytes it will be a GET otherwise it will become a POST
        /// with the query payload moved to the POST body.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <param name="writePostBody">Whether to open the http request and write the POST body.</param>
        /// <returns>HttpWebRequest for this URI.</returns>
#if WINDOWS_PHONE_APP
        internal static async Task<HttpWebRequest> CreateRequest(Uri requestUri, bool writePostBody = true)
        {
            return await CreatePostRequest(requestUri, writePostBody);
        }
#else
        internal static HttpWebRequest CreateRequest(Uri requestUri, bool writePostBody = true)
        {
            return CreatePostRequest(requestUri, writePostBody);
        }
#endif

        /// <summary>
        /// Create a HttpWebRequest using the HTTP POST method.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <param name="writeBody">Whether it should write the contents of the body.</param>
        /// <returns>HttpWebRequest for this URI.</returns>
#if WINDOWS_PHONE_APP
        private static async Task<HttpWebRequest> CreatePostRequest(Uri requestUri, bool writeBody)
#else
        private static HttpWebRequest CreatePostRequest(Uri requestUri, bool writeBody)
#endif
        {
            var uriWithoutQuery = new Uri(requestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));
            var postRequest = WebRequest.CreateHttp(uriWithoutQuery);
            postRequest.Method = "POST";
            
            var bodyWithQuery = requestUri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);
            var bodyBytes = Encoding.UTF8.GetBytes(bodyWithQuery);
#if !WINDOWS_PHONE_APP
            postRequest.ContentLength = bodyBytes.Length;
#endif

            if (writeBody)
            {
#if WINDOWS_PHONE_APP
                var stream = await postRequest.GetRequestStreamAsync();
#else
                var stream = postRequest.GetRequestStream();
#endif
                stream.Write(bodyBytes, 0, bodyBytes.Length);

#if WINDOWS_PHONE_APP
                stream.Flush();
                stream.Dispose();
#else
                stream.Close();      
#endif
            }

            return postRequest;
        }
    }

#if WINDOWS_PHONE

    internal static class HttpWebRequestExtensions
    {
        internal static void Add(this WebHeaderCollection collection, HttpRequestHeader header, string value)
        {
            collection[header] = value;
        }

        internal static HttpWebResponse GetResponse(this HttpWebRequest request)
        {
            return request.GetResponseAsync().GetAwaiter().GetResult();
        }

        internal static Stream GetRequestStream(this HttpWebRequest request)
        {
            return request.GetRequestStreamAsync().GetAwaiter().GetResult();
        }

        internal static Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request)
        {
            var tcs = new TaskCompletionSource<HttpWebResponse>();
            request.BeginGetResponse(asyncResponse =>
            {
                try
                {
                    var asyncState = (HttpWebRequest)asyncResponse.AsyncState;
                    var response = (HttpWebResponse)asyncState.EndGetResponse(asyncResponse);
                    tcs.TrySetResult(response);
                }
                catch (WebException ex)
                {
                    var failedResponse = (HttpWebResponse)ex.Response;
                    tcs.TrySetResult(failedResponse);
                }
            }, request);
            return tcs.Task;
        }

        internal static Task<Stream> GetRequestStreamAsync(this HttpWebRequest request)
        {
            var tcs = new TaskCompletionSource<Stream>();
            request.BeginGetRequestStream(asyncResponse =>
            {
                try
                {
                    var asyncState = (HttpWebRequest)asyncResponse.AsyncState;
                    var stream = asyncState.EndGetRequestStream(asyncResponse);
                    tcs.TrySetResult(stream);
                }
                catch (WebException ex)
                {
                    tcs.SetException(ex);
                }
            }, request);
            return tcs.Task;
        }
        
    }

#endif
}