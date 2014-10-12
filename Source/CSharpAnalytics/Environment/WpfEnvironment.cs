// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace CSharpAnalytics.Environment
{
    /// <summary>
    ///     Implements the IEnvironment interface required by analytics to track details of the machine
    ///     in a Windows Forms application.
    /// </summary>
    internal class WpfEnvironment : IEnvironment
    {
        public string CharacterSet
        {
            get { return Encoding.Default.BodyName; }
        }

        public string LanguageCode
        {
            get { return CultureInfo.CurrentUICulture.Name; }
        }

        public string FlashVersion
        {
            get { return null; }
        }

        public bool? JavaEnabled
        {
            get { return null; }
        }

        public uint ScreenColorDepth
        {
            get
            {
                return (uint) GetScreenForMainWindow()
                    .BitsPerPixel;
            }
        }

        public uint ScreenHeight
        {
            get
            {
                return (uint) GetScreenForMainWindow()
                    .Bounds.Height;
            }
        }

        public uint ScreenWidth
        {
            get
            {
                return (uint) GetScreenForMainWindow()
                    .Bounds.Width;
            }
        }

        public uint ViewportHeight
        {
            get
            {
                return (uint) GetScreenForMainWindow()
                    .WorkingArea.Height;
            }
        }

        public uint ViewportWidth
        {
            get
            {
                return (uint) GetScreenForMainWindow()
                    .WorkingArea.Width;
            }
        }

        private static Screen GetScreenForMainWindow()
        {
            Window mainWindow = Application.Current.MainWindow;

            return mainWindow != null
                ? Screen.FromHandle(new WindowInteropHelper(mainWindow).Handle)
                : Screen.PrimaryScreen;
        }
    }
}