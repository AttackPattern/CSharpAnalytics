// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics
{
    /// <summary>
    ///     AutoMeasurement static wrapper to make it easier to use across a WinForms application.
    /// </summary>
    public static class AutoMeasurement
    {
        private static BaseAutoMeasurement _instance;

        /// <summary>
        /// Gets a value indicating whether this <see cref="AutoMeasurement"/> is initialized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initialized; otherwise, <c>false</c>.
        /// </value>
        public static bool Initialized
        {
            get { return _instance != null; }
        }

        /// <summary>
        ///     The implementation of <see cref="BaseAutoMeasurement" /> to use when calling methods and properties
        ///     on <see cref="AutoMeasurement" />. Built-in types are <see cref="WinFormAutoMeasurement" /> and
        ///     <see cref="WpfAutoMeasurement" />.
        ///     Optionally you can use the implementations directly.
        /// </summary>
        public static BaseAutoMeasurement Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new NullReferenceException(
                        "Assign an implementation to this property first, such as WinFormsAutoMeasurement or WpfAutoMeasurement. Normally you do this once on application startup.");
                }

                return _instance;
            }
            set { _instance = value; }
        }

        public static VisitorStatus VisitorStatus
        {
            get { return Instance.VisitorStatus; }
        }

        public static MeasurementAnalyticsClient Client
        {
            get { return Instance.Client; }
        }

        public static Action<string> DebugWriter
        {
            set { Instance.DebugWriter = value; }
        }

        public static void SetOptOut(bool optOut)
        {
            Instance.SetOptOut(optOut);
        }

        public static void Start(MeasurementConfiguration measurementConfiguration, string launchKind = "",
            TimeSpan? uploadInterval = null)
        {
            Instance.Start(measurementConfiguration, launchKind, uploadInterval);
        }
    }
}