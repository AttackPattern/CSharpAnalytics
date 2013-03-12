﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Activities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// Converts UrchinActivities into key/value pairs that will form the Urchin-style URIs generated.
    /// </summary>
    internal static class UrchinActivityTracker
    {
        /// <summary>
        /// Turn an IUrchinActivity into the key/value pairs necessary for building
        /// the URI to track with Urchin.
        /// </summary>
        /// <param name="activity">Activity to turn into key/value pairs.</param>
        /// <returns>Enumerable of key/value pairs representing the activity.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(IUrchinActivity activity)
        {
            if (activity is CampaignActivity)
                return GetParameters((CampaignActivity)activity);
            if (activity is EventActivity)
                return GetParameters((EventActivity)activity);
            if (activity is PageViewActivity)
                return GetParameters((PageViewActivity)activity);
            if (activity is SocialActivity)
                return GetParameters((SocialActivity)activity);
            if (activity is TimedEventActivity)
                return GetParameters((TimedEventActivity)activity);
            if (activity is TransactionActivity)
                return GetParameters((TransactionActivity)activity);

            Debug.Assert(false, "Unknown Activity type");
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Obtain the key/value pairs for a CampaignActivity.
        /// </summary>
        /// <param name="campaign">CampaignActivity to turn into key/value pairs.</param>
        /// <returns>Key/value pairs representing this CampaignActivity.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(CampaignActivity campaign)
        {
            yield return KeyValuePair.Create("utmcsr", campaign.Source);

            if (!String.IsNullOrEmpty(campaign.Name))
                yield return KeyValuePair.Create("utmccn", campaign.Name);

            if (!String.IsNullOrEmpty(campaign.Medium))
                yield return KeyValuePair.Create("utmcmd", campaign.Medium);

            if (!String.IsNullOrEmpty(campaign.Term))
                yield return KeyValuePair.Create("utmctr", campaign.Term);

            if (!String.IsNullOrEmpty(campaign.Content))
                yield return KeyValuePair.Create("utmcct", campaign.Content);

            yield return KeyValuePair.Create(campaign.IsNewVisit ? "utmcn" : "utmcr", "1");
        }

        /// <summary>
        /// Obtain the key/value pairs for an EventActivity.
        /// </summary>
        /// <param name="event">EventActivity to turn into key/value pairs.</param>
        /// <returns>Key/value pairs representing this EventActivity.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(EventActivity @event)
        {
            yield return KeyValuePair.Create("utmt", "event");
            yield return KeyValuePair.Create("utme", ToEventParameter(@event));
            if (@event.NonInteraction)
                yield return KeyValuePair.Create("utmi", "1");
        }

        /// <summary>
        /// Create a Utme-encoded parameter string containing the details of a given EventActivity.
        /// </summary>
        /// <param name="event">Event to encode.</param>
        /// <returns>Utme-encoded parameter string representing this EventActivity.</returns>
        private static string ToEventParameter(EventActivity @event)
        {
            var queryValue = UtmeEncoder.Encode("5", @event.Category, @event.Action, @event.Label);
            if (@event.Value.HasValue)
                queryValue += "(" + UtmeEncoder.EscapeValue(@event.Value.Value.ToString(CultureInfo.InvariantCulture)) + ")";

            return queryValue;
        }

        /// <summary>
        /// Obtain the key/value pairs for a PageViewActivity.
        /// </summary>
        /// <param name="pageView">PageviewActivity to turn into key/value pairs.</param>
        /// <returns>Key/value pairs representing this PageViewActivity.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(PageViewActivity pageView)
        {
            yield return KeyValuePair.Create("utmp", pageView.Page);
            yield return KeyValuePair.Create("utmdt", pageView.Title);
        }

        /// <summary>
        /// Obtain the key/value pairs for a SocialActivity.
        /// </summary>
        /// <param name="social">SocialActivity to turn into key/value pairs.</param>
        /// <returns>Key/value pairs representing this SocialActivity.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(SocialActivity social)
        {
            yield return KeyValuePair.Create("utmt", "social");
            yield return KeyValuePair.Create("utmsa", social.Action);
            yield return KeyValuePair.Create("utmsn", social.Network);

            if (!String.IsNullOrWhiteSpace(social.Target))
                yield return KeyValuePair.Create("utmsid", social.Target);

            if (!String.IsNullOrWhiteSpace(social.PagePath))
                yield return KeyValuePair.Create("utmp", social.PagePath);
        }

        /// <summary>
        /// Obtain the key/value pairs for a TimedEventActivity.
        /// </summary>
        /// <param name="timedEvent">TimedEventActivity to turn into key/value pairs.</param>
        /// <returns>Key/value pairs representing this TimedEventActivity.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(TimedEventActivity timedEvent)
        {
            yield return KeyValuePair.Create("utmt", "event");
            yield return KeyValuePair.Create("utme", UtmeEncoder.Encode(timedEvent));
        }

        /// <summary>
        /// Obtain the key/value pairs for a TransactionActivity.
        /// </summary>
        /// <param name="transaction">TransactionActivity to turn into key/value pairs.</param>
        /// <returns>Key/value pairs representing this TransactionActivity.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(TransactionActivity transaction)
        {
            yield return KeyValuePair.Create("utmt", "tran");
            yield return KeyValuePair.Create("utmtid", transaction.OrderId);
            yield return KeyValuePair.Create("utmtst", transaction.StoreName);

            yield return KeyValuePair.Create("utmtto", transaction.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture));
            yield return KeyValuePair.Create("utmttx", transaction.TaxCost.ToString("0.00", CultureInfo.InvariantCulture));
            yield return KeyValuePair.Create("utmtsp", transaction.ShippingCost.ToString("0.00", CultureInfo.InvariantCulture));

            yield return KeyValuePair.Create("utmtci", transaction.BillingCity ?? "");
            yield return KeyValuePair.Create("utmtco", transaction.BillingCountry ?? "");
            yield return KeyValuePair.Create("utmtrg", transaction.BillingRegion ?? "");
        }

        /// <summary>
        /// Obtain the key/value pairs for a TransactionItemActivity.
        /// </summary>
        /// <param name="item">TransactionItemActivity to turn into key/value pairs.</param>
        /// <returns>Key/value pairs representing this TransactionItemActivity.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(TransactionItemActivity item)
        {
            yield return KeyValuePair.Create("utmt", "item");
            yield return KeyValuePair.Create("utmipc", item.Code);
            yield return KeyValuePair.Create("utmipn", item.Name);
            yield return KeyValuePair.Create("utmipr", item.Price.ToString("0.00", CultureInfo.InvariantCulture));

            if (item.Quantity != 0)
                yield return KeyValuePair.Create("utmiqt", item.Quantity.ToString(CultureInfo.InvariantCulture));

            if (!String.IsNullOrEmpty(item.Variation))
                yield return KeyValuePair.Create("utmiva", item.Variation);
        }
    }
}