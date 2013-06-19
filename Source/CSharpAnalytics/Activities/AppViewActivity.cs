﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of a view displayed within an application.
    /// </summary>
    public class AppViewActivity : ContentViewActivity
    {
        /// <summary>
        /// Create a new AppViewActivity to capture details of this application view.
        /// </summary>
        /// <param name="screenName">Name of the screen being viewed.</param>
        public AppViewActivity(string screenName)
            : base(null, null, screenName)
        {
        }

        /// <summary>
        /// Name of the screen being viewed.
        /// </summary>
        public string ScreenName { get { return ContentDescription; } }
    }
}