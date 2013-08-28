﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration.Pnp;

namespace CSharpAnalytics
{
    /// <summary>
    /// Obtain system information not conveniently exposed by WinRT APIs.
    /// </summary>
    /// <remarks>
    /// Microsoft doesn't really want you getting this information and makes it difficult.
    /// The techniques used here are not bullet proof but are good enough for analytics.
    /// Do not use these methods or techniques for anything more important than that.
    /// </remarks>
    public class WindowsStoreSystemInformation
    {
        private const string ModelNameKey = "System.Devices.ModelName";
        private const string ManufacturerKey = "System.Devices.Manufacturer";
        private const string DeviceClassKey = "{A45C254E-DF1C-4EFD-8020-67D146A850E0},10";
        private const string DisplayPrimaryCategoryKey = "{78C34FC8-104A-4ACA-9EA4-524D52996E57},97";
        private const string DeviceDriverVersionKey = "{A8B865DD-2E3D-4094-AD97-E593A70C75D6},3";
        private const string RootContainer = "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}";
        private const string RootContainerQuery = "System.Devices.ContainerId:=\"" + RootContainer + "\"";
        private const string HalDeviceClass = "4d36e966-e325-11ce-bfc1-08002be10318";

        /// <summary>
        /// Get the processor architecture of this computer.
        /// </summary>
        /// <returns>The processor architecture of this computer.</returns>
        public static ProcessorArchitecture GetProcessorArchitecture()
        {
            try
            {
                var sysInfo = new _SYSTEM_INFO();
                GetNativeSystemInfo(ref sysInfo);

                return Enum.IsDefined(typeof(ProcessorArchitecture), sysInfo.wProcessorArchitecture)
                    ? (ProcessorArchitecture)sysInfo.wProcessorArchitecture
                    : ProcessorArchitecture.UNKNOWN;
            }
            catch
            {
            }

            return ProcessorArchitecture.UNKNOWN;
        }

        /// <summary>
        /// Get the name of the manufacturer of this computer.
        /// </summary>
        /// <returns>The name of the manufacturer of this computer.</returns>
        public static async Task<string> GetDeviceManufacturerAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { ManufacturerKey });
            return (string)rootContainer.Properties[ManufacturerKey];
        }

        /// <summary>
        /// Get the name of the model of this computer.
        /// </summary>
        /// <returns>The name of the model of this computer.</returns>
        public static async Task<string> GetDeviceModelAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { ModelNameKey });
            return (string)rootContainer.Properties[ModelNameKey];
        }

        /// <summary>
        /// Get the device category this computer belongs to.
        /// </summary>
        /// <returns>The category of this device.</returns>
        public static async Task<string> GetDeviceCategoryAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { DisplayPrimaryCategoryKey });
            return (string)rootContainer.Properties[DisplayPrimaryCategoryKey];
        }

        /// <summary>
        /// Get the version of Windows for this computer.
        /// </summary>
        /// <example>5.2</example>
        /// <returns>Version number of Windows running on this computer.</returns>
        public static async Task<string> GetWindowsVersionAsync()
        {
            // There is no good place to get this. The HAL driver version number will work
            // unless you're using a custom HAL... We could try three different places in the
            // future (e.g. USB drivers, System timer) and let it tie-break.
            var halDevice = await GetHalDevice(DeviceDriverVersionKey);
            if (halDevice == null || halDevice.Properties[DeviceDriverVersionKey] == null) return null;

            var versionParts = halDevice.Properties[DeviceDriverVersionKey].ToString().Split('.');
            return string.Join(".", versionParts.Take(2).ToArray());
        }

        /// <summary>
        /// Attempt to find the HAL (Hardware Abstraction Layer) device for this computer.
        /// </summary>
        /// <param name="properties">Additional property names to obtain for the HAL.</param>
        /// <returns>PnpObject of the HAL with the additional properties populated.</returns>
        private static async Task<PnpObject> GetHalDevice(params string[] properties)
        {
            var actualProperties = properties.Concat(new[] { DeviceClassKey });
            var rootDevices = (await PnpObject.FindAllAsync(PnpObjectType.Device, actualProperties, RootContainerQuery));
            foreach (var rootDevice in rootDevices.Where(d => d.Properties != null && d.Properties.Any()))
            {
                var lastProperty = rootDevice.Properties.Last();
                if (lastProperty.Value != null && lastProperty.Value.ToString().Equals(HalDeviceClass))
                    return rootDevice;
            }
            return null;
        }

        [DllImport("kernel32.dll")]
        static extern void GetNativeSystemInfo(ref _SYSTEM_INFO lpSystemInfo);

        [StructLayout(LayoutKind.Sequential)]
        struct _SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };
    }

    public enum ProcessorArchitecture : ushort
    {
        INTEL = 0,
        MIPS = 1,
        ALPHA = 2,
        PPC = 3,
        SHX = 4,
        ARM = 5,
        IA64 = 6,
        ALPHA64 = 7,
        MSIL = 8,
        AMD64 = 9,
        IA32_ON_WIN64 = 10,
        UNKNOWN = 0xFFFF
    }
}