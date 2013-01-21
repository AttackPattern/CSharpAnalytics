﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CSharpAnalytics.Activities;
using System.Diagnostics;
using CSharpAnalytics.Protocols;

namespace CSharpAnalytics.Protocols.Urchin
{
    internal static class UrchinTrackerActivities
    {
        internal static IEnumerable<KeyValuePair<string, string>> GetActivityParameters(IActivity activity)
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

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(EventActivity @event)
        {
            yield return KeyValuePair.Create("utmt", "event");
            yield return KeyValuePair.Create("utme", ToEventParameter(@event));
            if (@event.NonInteraction)
                yield return KeyValuePair.Create("utmi", "1");
        }

        private static string ToEventParameter(EventActivity @event)
        {
            var queryValue = UtmeEncoder.Encode("5", @event.Category, @event.Action, @event.Label);
            if (@event.Value.HasValue)
                queryValue += "(" + UtmeEncoder.EscapeValue(@event.Value.Value.ToString(CultureInfo.InvariantCulture)) + ")";

            return queryValue;
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(PageViewActivity pageView)
        {
            yield return KeyValuePair.Create("utmp", pageView.Page);
            yield return KeyValuePair.Create("utmdt", pageView.Title);
        }

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

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(TimedEventActivity timedEvent)
        {
            yield return KeyValuePair.Create("utmt", "event");
            yield return KeyValuePair.Create("utme", UtmeEncoder.Encode(timedEvent));
        }

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

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(TransactionItemActivity itemActivity)
        {
            yield return KeyValuePair.Create("utmt", "item");
            yield return KeyValuePair.Create("utmipc", itemActivity.Code);
            yield return KeyValuePair.Create("utmipn", itemActivity.Name);
            yield return KeyValuePair.Create("utmipr", itemActivity.Price.ToString("0.00", CultureInfo.InvariantCulture));

            if (itemActivity.Quantity != 0)
                yield return KeyValuePair.Create("utmiqt", itemActivity.Quantity.ToString(CultureInfo.InvariantCulture));

            if (!String.IsNullOrEmpty(itemActivity.Variation))
                yield return KeyValuePair.Create("utmiva", itemActivity.Variation);
        }
    }
}