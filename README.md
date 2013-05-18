CSharpAnalytics
===============

CSharpAnalytics is a C# library for tracking your application metrics via Google Analytics.

The pitch
---------

**Why add metrics to your app?**

Metrics let you see what's actually happening with your app in the real world. Crash rates, performance data, feature usage content popularity let you find out what is important to your users and where to spend your effort.

**Why use Google Analytics?**

Google Analytics is capable, well-supported, easy to use and free. While originally designed for web analytics it is gaining more and more app-specific metrics. The forthcoming Measurement Protocol adds even more with screen views, device breakout and crash data.

**Why the CSharpAnalytics project?**

This is the best solution for C# apps wanting to talk to Google Analytics. Why?

1. Ease of use - add *two lines* to your Windows 8 Store app
1. Pure C# - easy to debug, extend or port (no JavaScript, web views or DLLs)
1. Feature rich - offline, configurable, OS stats

Still not convinced? Check out [how we compare to the alternatives](https://github.com/AttackPattern/CSharpAnalytics/wiki/Comparison)

Platforms
---------
Our goal is to support all major C# platforms. Right now we have project files for:

* Windows 8 Store applications (Visual Studio 2012)
* .NET 4.5 applications (Visual Studio 2012)

Windows 8 Store support also includes a sample app and AutoAnalytics to add basic analytics to your app with just two lines of code.

Features
--------
* Offline and online support
* Configurable upload interval, over HTTPS/SSL if required
* Support for page views, events, timed events, social events, custom variables
* Manages visitor and session state
* Can auto-hook into a number of interesting events
* Built-in debug output window support (ga_debug.js style)
* Tracks operating system and version
* Helpers for device model, processor architecture

Important notes
---------------
* This project is new and under active development, ensure suitability of code for your purposes
* Windows 8 network metering is not honored by analytics at this time

Getting started
---------------
You will need:

* Google Analytics account - [Sign up](http://analytics.google.com) if you don't have one
* An analytics property

Download or clone the source and add a reference to CSharpAnalytics.WindowsStore from your application.

Automatic analytics for Windows 8
---------------------------------
The easiest way to start is to use one of the automatic helper classes. They hook into a few events and will automatically give you:

* Application launch/suspend events
* Visitor, session counts, time-spent
* Social sharing events
* Basic page navigation activity

You must choose between the two classes:

* AutoMeasurement - if you set-up your analytics property as an app (recommended)
* AutoAnalytics - if you set-up your analytics property as a web site

Simply add two lines to your App.xaml.cs OnLaunched method. At the start of the method add:

`AutoMeasurement.StartAsync(new MeasurementConfiguration("UA-319000-10"));`

Replacing UA-319000-10 with your own Analytics property ID. At the end of the method add:

`AutoMeasurement.Attach(rootFrame);`

Check out the CSharpAnalytics.Sample.WindowsStore application if still unsure of usage or to see the equivalent AutoAnalytics methods for web site style tracking.

Going further
-------------
AutoAnalytics & AutoMeasurement are a start but you'll certainly want to go further.

**For pages that display content from a data source**

Add ITrackOwnView to your page class to stop AutoAnalytics from tracking it and instead track it yourself once the content is loaded - we recommend the end of the LoadState method with either of:

`AutoMeasurement.Client.TrackAppView(item.Title);`

**For additional user events**

Say you want to track when the video "Today's News" is played back:

`AutoMeasurement.Client.TrackEvent("Play", "Video", "Today's News");`

**For timing**

If you want to track how long something takes:

```
var timedActivity = new AutoTimedEventActivity("Loading", "Pictures");
// do something that takes time
AutoMeasurement.Client.Track(timedActivity);
```

Privacy
-------
Privacy is very important to Google and Microsoft and it should be to you too. Here's a few things you should know:

1. [Google's Measurement Protocol / SDK Policy](https://developers.google.com/analytics/devguides/collection/protocol/policy) (do not track personally identifyable information)
1. [Microsoft's Windows 8 app certification requirements](http://msdn.microsoft.com/en-us/library/windows/apps/hh694083.aspx) (include a privacy policy with your app)
1. AnonymizeIp is set to true by default. We recommend you leave this on as it scrubs the last IP octet at Google's end.
1. Do not track the names of user-generated content. e.g. Page titles for mail apps or photo album apps.
 
In summary: **Do not share personally identifyable information**

Untested
----------------
1. E-commerce tracking (Windows Store already has its own)
1. Campaign tracking (limited use as Windows Store doesn't pass through parameters)

Future enhancements
-------------------
1. Add support for Windows 8 network metering modes
1. Additional platforms (Windows Phone 7/8, Silverlight, Mono)
1. Opt-out support via null receiver
1. Throttling of hits as per official SDKs

If you want to contribute please consider the CSharpAnalytics.sln which will load all platforms and unit tests (if you get any project load failures you're probably missing an SDK)

Licence
-------
Copyright 2012-2013 Attack Pattern LLC

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
