using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;
using System;

namespace CSharpAnalytics.Test
{
    public static class TestHelpers
    {
        private static readonly Random random = new Random();
        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(50);

        public static readonly MeasurementConfiguration MeasurementConfiguration = new MeasurementConfiguration("UA-319000-10", "AppName", "1.2.3.4");
        public static readonly Func<SessionManager> CreateSessionManager = () => new SessionManager(TimeSpan.FromSeconds(20), new SessionState());
        public const string Utm = "http://www.google-analytics.com/__utm.gif";

        public static string RandomChars(int length)
        {
            var chars = new char[length];
            for (var i = 0; i < length; i++)
                chars[i] = (char)random.Next('A', 'Z');

            return new string(chars);
        }

        public static void WaitForQueueToEmpty(BackgroundHttpRequester requester)
        {
            var time = new Stopwatch();
            time.Start();

            while (requester.QueueCount != 0 && time.Elapsed < timeout)
                Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
        }

        public static List<Uri> CreateRequestList(int count)
        {
            return new List<Uri>(Enumerable.Range(1, count).Select(i => new Uri(Utm + "?" + i)));
        }
    }
}