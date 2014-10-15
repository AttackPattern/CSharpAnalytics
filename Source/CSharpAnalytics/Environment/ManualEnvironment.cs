// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics.Environment
{
    public class ManualEnvironment : IEnvironment
    {
        public ManualEnvironment()
        {
        }

        public ManualEnvironment(string characterSet, string languageCode, string flashVersion, bool? javaEnabled,
            uint screenColorDepth, uint screenHeight, uint screenWidth, uint viewportHeight, uint viewportWidth)
        {
            CharacterSet = characterSet;
            LanguageCode = languageCode;
            FlashVersion = flashVersion;
            JavaEnabled = javaEnabled;
            ScreenColorDepth = screenColorDepth;
            ScreenHeight = screenHeight;
            ScreenWidth = screenWidth;
            ViewportHeight = viewportHeight;
            ViewportWidth = viewportWidth;
        }

        public string CharacterSet { get; set; }
        public string LanguageCode { get; set; }
        public string FlashVersion { get; set; }
        public bool? JavaEnabled { get; set; }
        public uint ScreenColorDepth { get; set; }
        public uint ScreenHeight { get; set; }
        public uint ScreenWidth { get; set; }
        public uint ViewportHeight { get; set; }
        public uint ViewportWidth { get; set; }
    }
}