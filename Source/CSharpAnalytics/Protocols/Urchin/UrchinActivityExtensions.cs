using System;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Urchin;

namespace CSharpAnalytics
{
    public static class UrchinActivityExtensions
    {
        /// <summary>
        /// Capture the details of a campaign that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">UrchinAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="source">Source of the campaign.</param>
        /// <param name="name">Optional name of the campaign.</param>
        /// <param name="medium">Optional type of campaign.</param>
        /// <param name="term">Optional keyword terms for this campaign.</param>
        /// <param name="content">Optional content such as the specific link or content item for this campaign.</param>
        public static void TrackCampaign(this UrchinAnalyticsClient analyticsClient, string source, string name = null, string medium = null, string term = null, string content = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new CampaignActivity(source) { Name = name, Medium = medium, Term = term, Content = content });
        }

        /// <summary>
        /// Capture the details of an event that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">UrchinAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="action">Action name of the event to send.</param>
        /// <param name="category">Category of the event to send.</param>
        /// <param name="label">Optional label name of the event to send.</param>
        /// <param name="value">Optional numeric value of the event to send.</param>
        /// <param name="nonInteraction">Optional boolean value to be assigned to the NonInteraction property.</param>
        public static void TrackEvent(this UrchinAnalyticsClient analyticsClient, string action, string category, string label = null, int? value = null, bool nonInteraction = false)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new EventActivity(action, category, label, value, nonInteraction));
        }

        /// <summary>
        /// Track a new PageView for a given page and title.
        /// </summary>
        /// <param name="analyticsClient">UrchinAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="title">Title of the page.</param>
        /// <param name="page">Relative path of the page.</param>
        public static void TrackPageView(this UrchinAnalyticsClient analyticsClient, string title, string page)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new PageViewActivity(title, page));
        }

        /// <summary>
        /// Track a social activity being performed.
        /// </summary>
        /// <param name="analyticsClient">UrchinAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="action">Social action being performed.</param>
        /// <param name="network">Name of the social network being acted upon.</param>
        /// <param name="pagePath">Optional path of the page the action occured on.</param>
        /// <param name="target">Optional target resource being acted upon.</param>
        public static void TrackSocial(this UrchinAnalyticsClient analyticsClient, string action, string network, string target = null, string pagePath = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new SocialActivity(action, network, pagePath, target));
        }

        /// <summary>
        /// Capture the details of a timed event that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">UrchinAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="category">Category of the event to send.</param>
        /// <param name="variable">Variable name of the event to send.</param>
        /// <param name="time">Time of the event to send.</param>
        /// <param name="label">Optional label name of the event to send.</param>
        public static void TrackTimedEvent(this UrchinAnalyticsClient analyticsClient, string category, string variable, TimeSpan time, string label = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new TimedEventActivity(category, variable, time, label));
        }
    }
}