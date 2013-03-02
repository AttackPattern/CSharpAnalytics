// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
﻿using CSharpAnalytics.Activities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace CSharpAnalytics.Protocols.Measurement
{
    internal class MeasurementTrackerActivities
    {
        private string lastTransactionId;

        internal IEnumerable<KeyValuePair<string, string>> GetActivityParameters(IMeasurementActivity activity)
        {
            if (activity is ContentViewActivity)
                return GetParameters((ContentViewActivity)activity);
            if (activity is CampaignActivity)
                return GetParameters((CampaignActivity)activity);
            if (activity is ExceptionActivity)
                return GetParameters((ExceptionActivity)activity);
            if (activity is EventActivity)
                return GetParameters((EventActivity)activity);
            if (activity is TimedEventActivity)
                return GetParameters((TimedEventActivity)activity);
            if (activity is SocialActivity)
                return GetParameters((SocialActivity)activity);
            if (activity is TransactionActivity)
                return GetParameters((TransactionActivity)activity);

            Debug.Assert(false, "Unknown Activity type");
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(ContentViewActivity content)
        {
            yield return KeyValuePair.Create("t", "appview");

            if (content.DocumentLocation != null)
                yield return KeyValuePair.Create("dl", content.DocumentLocation.OriginalString);

            if (!String.IsNullOrEmpty(content.DocumentHostName))
                yield return KeyValuePair.Create("dh", content.DocumentHostName);

            if (!String.IsNullOrEmpty(content.DocumentPath))
                yield return KeyValuePair.Create("dp", content.DocumentPath);

            if (!String.IsNullOrEmpty(content.DocumentTitle))
                yield return KeyValuePair.Create("dt", content.DocumentTitle);

            if (!String.IsNullOrEmpty(content.ContentDescription))
                yield return KeyValuePair.Create("cd", content.ContentDescription);
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(ExceptionActivity exception)
        {
            yield return KeyValuePair.Create("exd", exception.Description);
            if (!exception.IsFatal)
                yield return KeyValuePair.Create("exf", "0");
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(CampaignActivity campaign)
        {
            if (!String.IsNullOrEmpty(campaign.Name))
                yield return KeyValuePair.Create("cn", campaign.Name);

            if (!String.IsNullOrEmpty(campaign.Source))
                yield return KeyValuePair.Create("cs", campaign.Source);

            if (!String.IsNullOrEmpty(campaign.Medium))
                yield return KeyValuePair.Create("cm", campaign.Medium);

            if (!String.IsNullOrEmpty(campaign.Term))
                yield return KeyValuePair.Create("ck", campaign.Term);

            if (!String.IsNullOrEmpty(campaign.Content))
                yield return KeyValuePair.Create("ct", campaign.Content);
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(EventActivity @event)
        {
            yield return KeyValuePair.Create("t", "event");

            if (!String.IsNullOrWhiteSpace(@event.Category))
                yield return KeyValuePair.Create("ec", @event.Category);

            if (!String.IsNullOrWhiteSpace(@event.Action))
                yield return KeyValuePair.Create("ea", @event.Action);

            if (!String.IsNullOrWhiteSpace(@event.Label))
                yield return KeyValuePair.Create("el", @event.Label);

            if (@event.Value.HasValue)
                yield return KeyValuePair.Create("ev", @event.Value.Value.ToString(CultureInfo.InvariantCulture));

            if (@event.NonInteraction)
                yield return KeyValuePair.Create("ni", "1");
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(TimedEventActivity timedEvent)
        {
            yield return KeyValuePair.Create("t", "timing");

            if (!String.IsNullOrWhiteSpace(timedEvent.Category))
                yield return KeyValuePair.Create("utc", timedEvent.Category);

            if (!String.IsNullOrWhiteSpace(timedEvent.Variable))
                yield return KeyValuePair.Create("utv", timedEvent.Variable);

            if (!String.IsNullOrWhiteSpace(timedEvent.Label))
                yield return KeyValuePair.Create("utl", timedEvent.Label);

            yield return KeyValuePair.Create("utt", timedEvent.Time.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(SocialActivity social)
        {
            yield return KeyValuePair.Create("t", "social");

            yield return KeyValuePair.Create("sn", social.Network);            
            yield return KeyValuePair.Create("sa", social.Action);
            yield return KeyValuePair.Create("st", social.Target);
        }

        internal IEnumerable<KeyValuePair<string, string>> GetParameters(TransactionActivity transaction)
        {
            yield return KeyValuePair.Create("t", "transaction");

            lastTransactionId = transaction.OrderId;
            yield return KeyValuePair.Create("ti", transaction.OrderId);

            if (!String.IsNullOrWhiteSpace(transaction.StoreName))
                yield return KeyValuePair.Create("ta", transaction.StoreName);

            if (transaction.OrderTotal != Decimal.Zero)
                yield return KeyValuePair.Create("tr", transaction.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture));

            if (transaction.ShippingCost != Decimal.Zero)
                yield return KeyValuePair.Create("ts", transaction.TaxCost.ToString("0.00", CultureInfo.InvariantCulture));

            if (transaction.TaxCost != Decimal.Zero)
                yield return KeyValuePair.Create("tt", transaction.ShippingCost.ToString("0.00", CultureInfo.InvariantCulture));
        }

        internal IEnumerable<KeyValuePair<string, string>> GetParameters(TransactionItemActivity item)
        {
            yield return KeyValuePair.Create("t", "item");

            yield return KeyValuePair.Create("ti", lastTransactionId);

            if (item.Price != Decimal.Zero)
                yield return KeyValuePair.Create("ip", item.Price.ToString("0.00", CultureInfo.InvariantCulture));

            if (item.Quantity != 0)
                yield return KeyValuePair.Create("iq", item.Quantity.ToString(CultureInfo.InvariantCulture));

            if (!String.IsNullOrWhiteSpace(item.Code))
                yield return KeyValuePair.Create("ic", item.Code);

            if (!String.IsNullOrWhiteSpace(item.Name))
                yield return KeyValuePair.Create("in", item.Name);

            if (!String.IsNullOrEmpty(item.Variation))
                yield return KeyValuePair.Create("iv", item.Variation);
        }
    }
}