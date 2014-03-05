using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in Windows Forms applications.
    /// Execute <see cref="Start"/> method when you applications starts and <see cref="End"/> method while it is being closed.
    /// </summary>
    public static class AutoMeasurement
    {
        private const string ApplicationLifecycleEvent = "ApplicationLifecycle";
        private const string RequestQueueFileName = "CSharpAnalytics-MeasurementQueue";
        private const string SessionStateFileName = "CSharpAnalytics-MeasurementSession";
        private const int MaximumRequestsToPersist = 60;

        private static readonly ProtocolDebugger protocolDebugger = new ProtocolDebugger(MeasurementParameterDefinitions.All);
        private static readonly MeasurementAnalyticsClient client = new MeasurementAnalyticsClient();
        private static readonly ProductInfoHeaderValue clientUserAgent = new ProductInfoHeaderValue("CSharpAnalytics", "0.2");

        private static bool? delayedOptOut;
        private static TimeSpan lastUploadInterval;
        private static BackgroundHttpRequester requester;
        private static SessionManager sessionManager;
        private static string systemUserAgent;
        private static bool isStarted;

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connDescription, int reservedValue);

        /// <summary>
        /// Access to the MeasurementAnalyticsClient necessary to send additional events.
        /// </summary>
        public static MeasurementAnalyticsClient Client { get { return client; } }

        /// <summary>
        /// Action to receive protocol debug output. 
        /// </summary>
        public static Action<string> DebugWriter { get; set; }

        /// <summary>
        /// Initialize CSharpAnalytics by restoring the session state and starting the background sender.
        /// </summary>
        /// <param name="configuration">Configuration to use, must at a minimum specify your Google Analytics ID and app name.</param>
        /// <param name="uploadInterval">How often to upload to the server. Lower times = more traffic but realtime. Defaults to 5 seconds.</param>
        /// <example>var analyticsTask = AutoMeasurement.Start(new MeasurementConfiguration("UA-123123123-1", "MyApp", "1.0.0.0"));</example>
        public static async Task Start(MeasurementConfiguration configuration, TimeSpan? uploadInterval = null)
        {
            if (!isStarted)
            {
                isStarted = true;
                lastUploadInterval = uploadInterval ?? TimeSpan.FromSeconds(5);
                systemUserAgent = WinFormsSystemInformation.GetSystemUserAgent();

                var sessionState = await LoadSessionState();
                sessionManager = new SessionManager(sessionState, configuration.SampleRate);
                await StartRequesterAsync();

                if (delayedOptOut != null) SetOptOut(delayedOptOut.Value);

                Client.Configure(configuration, sessionManager, new WinFormsEnvironment(), Add);

                // Sometimes apps crash so preserve at least session number and visitor id on launch
                await SaveSessionState(sessionManager.GetState());
            }

            client.TrackEvent("Start", ApplicationLifecycleEvent, "Launch");
        }

        /// <summary>
        /// Ends analytics tracking by persisting any pending requests and session state to persistent storage.
        /// </summary>
        public static async Task End()
        {
            List<Uri> recentRequestsToPersist;
            if (requester.IsStarted)
            {
                var pendingRequests = await requester.StopAsync();
                recentRequestsToPersist = pendingRequests.Skip(pendingRequests.Count - MaximumRequestsToPersist).ToList();
            }
            else
            {
                recentRequestsToPersist = new List<Uri>();
            }

            await AppDataContractSerializer.Save(recentRequestsToPersist, RequestQueueFileName);
            await SaveSessionState(sessionManager.GetState());
        }

        /// <summary>
        /// Opt the user in or out of analytics for this application install.
        /// </summary>
        /// <param name="optOut">True if the user is opting out, false if they are opting back in.</param>
        /// <remarks>
        /// This option persists automatically.
        /// You should call this only when the user changes their decision.
        /// </remarks>
        public static async void SetOptOut(bool optOut)
        {
            if (sessionManager == null)
            {
                delayedOptOut = optOut;
                return;
            }
            delayedOptOut = null;

            if (sessionManager.VisitorStatus == VisitorStatus.SampledOut) return;

            var newVisitorStatus = optOut ? VisitorStatus.OptedOut : VisitorStatus.Active;
            if (newVisitorStatus != sessionManager.VisitorStatus)
            {
                Debug.WriteLine("Switching VisitorStatus from {0} to {1}", sessionManager.VisitorStatus, newVisitorStatus);
                sessionManager.VisitorStatus = newVisitorStatus;
                await SaveSessionState(sessionManager.GetState());
            }
        }

        /// <summary>
        /// Internal status of this visitor.
        /// </summary>
        public static VisitorStatus VisitorStatus
        {
            get
            {
                // Allow AnalyticsUserOption to function at design time.
                if (sessionManager == null)
                    return delayedOptOut == true ? VisitorStatus.OptedOut : VisitorStatus.Active;

                return sessionManager.VisitorStatus;
            }
        }

        /// <summary>
        /// Start the requester with any unsent URIs from the last application run.
        /// </summary>
        /// <returns>Task that completes when the requester is ready.</returns>
        private static async Task StartRequesterAsync()
        {
            requester = new BackgroundHttpClientRequester(PreprocessHttpRequest, IsInternetAvailable);
            var previousRequests = await AppDataContractSerializer.Restore<List<Uri>>(RequestQueueFileName);
            requester.Start(lastUploadInterval, previousRequests);
        }

        /// <summary>
        /// Determine if the Internet is available at this point in time.
        /// </summary>
        /// <returns>True if the Internet is available, false otherwise.</returns>
        private static bool IsInternetAvailable()
        {
            int connDesc;
            return InternetGetConnectedState(out connDesc, 0);
        }

        /// <summary>
        /// Pre-process the HttpRequestMessage before it is sent. This includes adding the user agent for tracking
        /// and for debug builds writing out the debug information to the console log.
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to modify or inspect before it is sent.</param>
        /// <remarks>
        /// Because user agent is not persisted unsent URIs that are saved and then sent after an upgrade
        /// will have the new user agent string not the actual one that generated them.
        /// </remarks>
        private static void PreprocessHttpRequest(HttpRequestMessage requestMessage)
        {
            if (sessionManager.VisitorStatus != VisitorStatus.Active)
            {
                requestMessage.RequestUri = null;
                return;
            }

            requestMessage.RequestUri = client.AdjustUriBeforeRequest(requestMessage.RequestUri);
            AddUserAgent(requestMessage.Headers.UserAgent);
            DebugRequest(requestMessage);
        }

        /// <summary>
        /// Figure out the user agent and add it to the header collection.
        /// </summary>
        /// <param name="userAgents">User agent header collection.</param>
        private static void AddUserAgent(ICollection<ProductInfoHeaderValue> userAgents)
        {
            userAgents.Add(clientUserAgent);

            if (!String.IsNullOrEmpty(systemUserAgent))
                userAgents.Add(new ProductInfoHeaderValue(systemUserAgent));
        }

        /// <summary>
        /// Send the HttpRequestMessage with the protocol debugger for examination.
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to examine with the protocol debugger.</param>
        private async static void DebugRequest(HttpRequestMessage requestMessage)
        {
            var payloadUri = await RejoinPayload(requestMessage);
            protocolDebugger.Dump(payloadUri, DebugWriter);
        }

        /// <summary>
        /// Rejoin the POST body payload with the Uri parameter if necessary so it can be sent to the
        /// protocol debugger.
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to obtain complete payload for.</param>
        /// <returns>Uri with final payload to be sent.</returns>
        private async static Task<Uri> RejoinPayload(HttpRequestMessage requestMessage)
        {
            if (requestMessage.Content == null)
                return requestMessage.RequestUri;

            var bodyPayload = await requestMessage.Content.ReadAsStringAsync();
            return new UriBuilder(requestMessage.RequestUri) { Query = bodyPayload }.Uri;
        }

        /// <summary>
        /// Load the session state from storage if it exists, null if it does not.
        /// </summary>
        /// <returns>Task that completes when the SessionState is available.</returns>
        private static async Task<SessionState> LoadSessionState()
        {
            return await AppDataContractSerializer.Restore<SessionState>(SessionStateFileName);
        }

        /// <summary>
        /// Save the session state to preserve state between application launches.
        /// </summary>
        /// <returns>Task that completes when the session state has been saved.</returns>
        private static async Task SaveSessionState(SessionState sessionState)
        {
            await AppDataContractSerializer.Save(sessionState, SessionStateFileName);
        }

        /// <summary>
        /// Send the Uri request to the current background requester safely.
        /// </summary>
        private static void Add(Uri uri)
        {
            var safeRequester = requester;
            if (safeRequester != null)
                safeRequester.Add(uri);
        }
    }
}