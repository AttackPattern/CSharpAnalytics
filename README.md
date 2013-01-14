CSharpAnalytics
===============

CSharpAnalytics is a fully-managed C# library for adding Google Analytics metrics to your applications.

By using this library you can see application usage activity and statistics in real-time to help you decide where to spend your development effort and best of all it's free.

Platforms
---------
Our goal is to support all major C# platforms however right now we have project files for:

* Windows 8 Store applications (Visual Studio 2012)
* .NET 4.5 applications (Visual Studio 2012)

Only Windows 8 "WindowsStore" applications are propertly supported right now.

Getting started
---------------
You will need:

* Google Analytics account - Head to http://analytics.google.com and sign-up if you don't have one
* Analytics property set-up - See http://support.google.com/analytics/bin/answer.py?hl=en&answer=2614741 for details

Download or clone the source and add a reference to CSharpAnalytics.WindowsStore from your application.

AutoAnalytics for Windows 8
---------------------------
The easiest way to start is to use the AutoAnalytics class. It hooks into a few events and will automatically give you:

* Application launch/suspend events
* Visitor, session counts, time-spent
* Social sharing events
* Basic page navigation activity

To use it simply add the following two lines to your app.xaml.cs:

**Start analytics** with this line in App.OnLaunched directly before  Window.Current.Activate()

`await AutoAnalytics.StartAsync(new Configuration("UA-XXX-YYY", "analytics app name"), "user agent name");`

**Stop analytics** with this line in App.OnSuspending directly before deferral.Complete()

`await AutoAnalytics.StopAsync();`

Check out the CSharpAnalytics.Sample.WindowsStore application if still unsure of usage.

Going further
-------------
AutoAnalytics is a start but you'll certainly want to go further.

**For pages that display content from a data source**

Add ITrackPageView to your page to stop AutoAnalytics from tracking it and instead track it yourself once the content is loaded - we recommend the end of the LoadState method with something like:

`AutoAnalytics.Client.TrackPageView(item.Title, "/news/" + item.Id);`

**For additional user events**

Say you want to track when the video "Today's News" is played back:

`AutoAnalytics.Client.TrackEvent("Play", "Video", "Today's News");`


Important notes
---------------
Privacy is very important to Google and Microsoft and it should be to you too. Here's a few things you should know:

1. [Google's Measurement Protocol / SDK Policy](https://developers.google.com/analytics/devguides/collection/protocol/policy) (do not track personally identifyable information)
1. [Microsoft's Windows 8 app certification requirements](http://msdn.microsoft.com/en-us/library/windows/apps/hh694083.aspx) (include a privacy policy with your app)
1. AnonymizeIp is set to true by default. We recommend you leave this on as it scrubs the last IP octet at Google's end.
1. Do not share the names of user-generated content. e.g. Page titles for mail apps or photo album apps.
 
**In summary do not share personally identifyable information.**

Limitations
-----------
* No throttling of requests to adhere to the limits of Google Analytics
* No tracking of operating system type or version
* Campaign tracking is of limited use as the Windows Store doesn't pass through parameters (iOS has same limitation).

Still to do
-----------
1. Support for other platforms (Windows Phone 7.x & 8, Silverlight, Mono variants)
1. Support new Google Measurement SDK
1. E-commerce tracking (Transactions and Items are partially implemented)
1. Timed events needs testing
1. Facilitate total opt-out with session state switch and null receiver
1. More unit tests & documentation

Licence
-------
Copyright 2012-2013 Attack Pattern LLC

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

`http://www.apache.org/licenses/LICENSE-2.0`

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
