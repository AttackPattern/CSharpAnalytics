using System;
using System.IO;
using CSharpAnalytics.Sessions;
#if WINDOWS_STORE
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
#endif

namespace CSharpAnalytics.Test.Sessions
{
    [TestClass]
    public class SessionStateTests
    {
        [TestMethod]
        public void SessionState_Properties_Can_Be_Set()
        {
            var visitorId = Guid.NewGuid();
            const int sessionNumber = 3;

            var lastActivityAt = new DateTimeOffset(2006, 3, 1, 8, 00, 00, TimeSpan.Zero);
            var previousSessionStartedAt = new DateTimeOffset(2003, 1, 1, 18, 22, 55, TimeSpan.Zero);
            var sessionStartedAt = new DateTimeOffset(2005, 2, 3, 4, 5, 6, TimeSpan.Zero);

            var referrer = new Uri("http://attackpattern.com");

            var state = new SessionState
            {
                VisitorId = visitorId,
                SessionNumber = sessionNumber,

                LastActivityAt = lastActivityAt,
                PreviousSessionStartedAt = previousSessionStartedAt,
                SessionStartedAt = sessionStartedAt,

                Referrer = referrer
            };

            Assert.AreEqual(visitorId, state.VisitorId);
            Assert.AreEqual(sessionNumber, state.SessionNumber);

            Assert.AreEqual(lastActivityAt, state.LastActivityAt);
            Assert.AreEqual(previousSessionStartedAt, state.PreviousSessionStartedAt);
            Assert.AreEqual(sessionStartedAt, state.SessionStartedAt);

            Assert.AreEqual(referrer, state.Referrer);
        }

        [TestMethod]
        public void SessionState_Serialized_And_Deserialized_Correctly()
        {
            var original = CreateSampleState();

            var deserialized = SerializeAndDeserialize(original);

            Assert.AreEqual(original.VisitorId, deserialized.VisitorId);
            Assert.AreEqual(original.SessionNumber, deserialized.SessionNumber);

            Assert.AreEqual(original.LastActivityAt, deserialized.LastActivityAt);
            Assert.AreEqual(original.PreviousSessionStartedAt, deserialized.PreviousSessionStartedAt);
            Assert.AreEqual(original.SessionStartedAt, deserialized.SessionStartedAt);
            
            Assert.AreEqual(original.Referrer, deserialized.Referrer);
        }

        private static SessionState CreateSampleState()
        {
            var original = new SessionState
            {
                VisitorId = Guid.NewGuid(),
                SessionNumber = 408,
                LastActivityAt = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 1)),
                PreviousSessionStartedAt = DateTime.Now.Subtract(new TimeSpan(0, 1, 10, 15)),
                SessionStartedAt = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 15)),
                Referrer = new Uri("http://damieng.com")
            };
            return original;
        }

        private static T SerializeAndDeserialize<T>(T objectToSerialize)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof (T));
                serializer.WriteObject(memoryStream, objectToSerialize);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T) serializer.ReadObject(memoryStream);
            }
        }
    }
}