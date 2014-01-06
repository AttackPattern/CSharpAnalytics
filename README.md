# CSharpAnalytics

CSharpAnalytics is a free C# library for tracking application metrics via Google Analytics.

## The pitch

### Why add metrics to your app?

Metrics let you see what your app is actually doing in the real world. Crash rates, performance data and feature usage content popularity let you find out what is important to your users and where to spend your effort for the next version.

### Why use Google Analytics?

Google Analytics is capable, well-supported, easy to use and free. While originally designed for web analytics the Measurement Protocol expands into more app-specific metrics with features including screen views, device breakout and crash data.

### Why the CSharpAnalytics project?

This is the best solution for C# apps wanting to talk to Google Analytics. Why?

1. Pure C# - easy to debug, extend or port (no JavaScript, web views or DLLs)
1. Feature rich - offline, configurable, OS stats
1. Ease of use - add *two lines* to your WindowsStore app

Still not convinced? Check out [how we compare to the alternatives](https://github.com/AttackPattern/CSharpAnalytics/wiki/Comparison)

## Platforms

Our goal is to support all major C# platforms. Right now we have project files for:

* Windows 8 Store applications (Visual Studio 2012, Measurement Protocol, AutoMeasurement)
* Windows 8.1 Store applications (Visual Studio 2013, Measurement Protocol, AutoMeasurement)
* .NET 4.5 applications (Visual Studio 2012, Measurement Protocol)

Windows 8 and 8.1 Store support are the most complete as they each includes a sample app and an AutoMeasurement implementation that provides a plug-in-and-go solution.

## Important notes

This project:

* does not support e-commerce or exception track yet as both are covered by Windows Store
* does not support campaign tracking as there is no way to do so via the Windows Store
* does not honor Windows 8 network metering modes

In short **it is up to you to ensure the suitability of this project for your purposes**. Good advice when choosing any piece of software.

## Features

* Online and offline support with timestamping
* Configurable upload interval, over HTTPS/SSL if desired
* Support for page/screen views, events, timed events, social events, custom variables/dimensions/metrics
* Manages visitor and session state
* Built-in debug output window support (ga_debug.js style)

Additionally on Windows 8/8.1 there is an automatic mode that wires up a lot of things for you, see Automatic analytics for Windows 8/8.1 below.

## Getting started

You will need:

* Google Analytics account - [Sign up](http://analytics.google.com) if you don't have one
* An analytics property set-up as an app ("Track interactions within Android and iOS apps")

Download or clone the source and add a reference to CSharpAnalytics.WindowsStore from your application.

### Automatic analytics for Windows 8/8.1 Store apps

The easiest way to start is to use the AutoMeasurement helper class. It hooks into a few events and will automatically give you:

* Application launch, suspend, resume events
* Visitor, session activity, time-spent
* Social sharing events
* Screen navigation activity
* Operating system, window resolution, CPU type identification
* Save/persist last 60 hits for offline/online support

Simply add two lines to your App.xaml.cs.

At the start of the OnLaunched method add (replacing UA-319000-8 with your own Google Analytics property ID):

`CSharpAnalytics.AutoMeasurement.StartAsync(new CSharpAnalytics.MeasurementConfiguration("UA-319000-8"), e);`

(where e is the name of your OnLaunched event argument method signature. It is 'args' by default in the Windows 8 templates and 'e' in Windows 8.1 templates).

At the end of the OnLaunched method add:

`CSharpAnalytics.AutoMeasurement.Attach(rootFrame);`

If your app only has a single page this line is not necessary as it only attaches to the page navigation events.

Check out the CSharpAnalytics.Sample.WindowsStore application if still unsure of usage.

NOTE: There is no need to await for the analyticsTask to complete. In fact doing so will slow down your app start-up!

### Going further

See [going further with CSharpAnalytics](https://github.com/AttackPattern/CSharpAnalytics/wiki/Going-further)

## Privacy

Privacy is very important to Google and Microsoft and it should be to you too. Here's a few things you should know:

1. [Google's Measurement Protocol / SDK Policy](https://developers.google.com/analytics/devguides/collection/protocol/policy) (do not track personally identifyable information)
1. [Microsoft's Windows 8 app certification requirements](http://msdn.microsoft.com/en-us/library/windows/apps/hh694083.aspx) (include a privacy policy with your app)
1. AnonymizeIp is set to true by default. We recommend you leave this on as it scrubs the last IP octet at Google's end.
1. Do not track the names of user-generated content. e.g. Page titles for mail apps or photo album apps.
 
In summary: **Do not share personally identifyable information**

## Future enhancements

1. Support for Windows 8 network metering modes
1. Additional platforms (Windows Phone 7/8, Silverlight, WPF)
1. Throttling & replenishing of hits as per official SDKs
1. Configurable session management modes
1. In-app purchase tracking integration

If you want to contribute please consider the CSharpAnalytics.sln which will load all platforms and unit tests (if you get any project load failures you're probably missing an SDK)

## Licence

Copyright 2012-2014 Attack Pattern LLC

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
