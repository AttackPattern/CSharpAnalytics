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
1. Ease of use - add *two lines* to your app for basic screen tracking

Still not convinced? Check out [how we compare to the alternatives](https://github.com/AttackPattern/CSharpAnalytics/wiki/Comparison)

### Isn't there an official Google client?

Google produce a library for *reading* analytics data in .NET apps but not recording them. The is a project that tries to look official on CodePlex but it is not, does not buffer up analytics while offline and is incapable of reporting accurate OS version information.

## Platforms

Our goal is to support all major C# platforms. Right now we have project files for:

* WinForms .NET 4.5 applications
* WinForms .NET 4.6.1 applications
* WPF .NET Core 3 applications

All of these platforms include the AutoMeasurement class that let you get up and running with only a few lines of code.

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

You can either:

1. Get a more stable release via NuGet
2. Clone the source code and add a reference to CSharpAnalytics.Windows81.csproj (or Windows8, .NET45 depending on your environment)

### Going further

See [going further with CSharpAnalytics](https://github.com/AttackPattern/CSharpAnalytics/wiki/Going-further)

## Seeing data

Once you have launched your app with your Google Analytics 'UA' property ID set correctly and signed into Google Analytics you should be able to start seeing metrics.

### Real-Time
The real-time overview displays information as soon as it is received by Google. CSharpAnalytics buffers requests and sends them every 5 seconds so it should appear quickly. You can specify a different delay by passing the optional uploadInterval parameter on the Start method. Higher values increase the amount of time it takes an event to show in the real-time views but lets tablets, phones and laptops save power by not using the networking radio as often. Setting this value higher than your session timeout will likely cause issues.

### Reports
There is an unspecified delay between events being sent to Google Analytics and appearing in the regular (non-real-time) reports. You should also remember to set the date range at the top right to include today's date. By default it only includes data up to yesterday.

## Privacy

Privacy is very important to Google and Microsoft and it should be to you too. Here's a few things you should know:

1. [Google's Measurement Protocol / SDK Policy](https://developers.google.com/analytics/devguides/collection/protocol/policy) (do not track personally identifyable information)
1. [Microsoft's Windows 8 app certification requirements](http://msdn.microsoft.com/en-us/library/windows/apps/hh694083.aspx) (include a privacy policy with your app)
1. AnonymizeIp is set to true by default. We recommend you leave this on as it scrubs the last IP octet at Google's end.
1. Do not track the names of user-generated content. e.g. Page titles for mail apps or photo album apps.
 
In summary: **Do not share personally identifyable information**

## Future enhancements

1. Support for Windows Store Universal and Xamarin projects
1. User ID, override IP and other new April 2014 Measurement Protocol features
1. In-app purchase tracking integration and campaign support
1. Throttling & replenishing of hits as per official SDKs
1. Configurable session management modes

If you want to contribute please consider the CSharpAnalytics.sln which will load all platforms and unit tests (if you get any project load failures you're probably missing an SDK). Please ignore any messages about upgrading or retargeting to Windows 8.1 - the solution contains both 8.0 and 8.1 projects as we want to support 8.0 for a while.

## Licence

Copyright 2012-2014 Attack Pattern LLC

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

## Packing 

### dotnet pack

More info at:

https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack

Example:
```
dotnet pack -c release --include-symbols --version-suffix ""
```

