﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;
using Windows.System;

namespace CSharpAnalytics.WindowsStore
{
    /// <summary>
    /// Obtain system information not conveniently exposed by WinRT APIs.
    /// </summary>
    /// <remarks>
    /// Microsoft doesn't really want you getting this information and makes it difficult.
    /// The techniques used here are not bullet proof but are good enough for analytics.
    /// Do not use these methods or techniques for anything more important than that.
    /// </remarks>
    public class SystemInformation
    {
        private const string ItemNameKey = "System.ItemNameDisplay";
        private const string ModelNameKey = "System.Devices.ModelName";
        private const string ManufacturerKey = "System.Devices.Manufacturer";
        private const string DeviceClassKey = "{A45C254E-DF1C-4EFD-8020-67D146A850E0},10";
        private const string DisplayPrimaryCategoryKey = "{78C34FC8-104A-4ACA-9EA4-524D52996E57},97";
        private const string DeviceDriverVersionKey = "{A8B865DD-2E3D-4094-AD97-E593A70C75D6},3";

        private const string RootContainer = "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}";

        private const string ProcessorQuery = "System.Devices.InterfaceClassGuid:=\"{97FADB10-4E33-40AE-359C-8BEF029DBDD0}\"";
        private const string RootContainerQuery = "System.Devices.ContainerId:=\"" + RootContainer + "\"";

        private const string HalDeviceClass = "4d36e966-e325-11ce-bfc1-08002be10318";

        /// <summary>
        /// Get the likely processor architecture of this computer.
        /// </summary>
        /// <returns>The likely processor architecture of this computer.</returns>
        public static async Task<ProcessorArchitecture> GetProcessorArchitectureAsync()
        {
            var halDevice = await GetHalDevice(ItemNameKey);
            if (halDevice != null && halDevice.Properties[ItemNameKey] != null)
            {
                var halName = halDevice.Properties[ItemNameKey].ToString();
                if (halName.Contains("x64")) return ProcessorArchitecture.X64;
                if (halName.Contains("ARM")) return ProcessorArchitecture.Arm;
                return ProcessorArchitecture.X86;
            }

            return ProcessorArchitecture.Unknown;
        }

        /// <summary>
        /// Get the display name of the processor in this computer.
        /// </summary>
        /// <remarks>
        /// Quite possibly culture-specific.
        /// </remarks>
        /// <returns>The display name of the processor in this computer.</returns>
        public static async Task<string> GetProcessorDisplayNameAsync()
        {
            var processors = await DeviceInformation.FindAllAsync(ProcessorQuery);
            return FindStartsWith(processors.Select(p => p.Name));
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
            if (halDevice != null && halDevice.Properties[DeviceDriverVersionKey] != null)
            {
                var versionParts = halDevice.Properties[DeviceDriverVersionKey].ToString().Split('.');
                return string.Join(".", versionParts.Take(2).ToArray());
            }

            return null;
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
                if (lastProperty.Value != null)
                    if (lastProperty.Value.ToString().Equals(HalDeviceClass))
                        return rootDevice;
            }
            return null;
        }

        /// <summary>
        /// Finds the string that an enumeration of strings starts with.
        /// </summary>
        /// <param name="values">Enumeration of strings to examine.</param>
        /// <returns>String that all values start with.</returns>
        private static string FindStartsWith(IEnumerable<string> values)
        {
            string result = null;
            foreach (var value in values)
            {
                result = result ?? value;
                for (int i = 0; i < result.Length; i++)
                    if (result[i] != value[i])
                    {
                        result = result.Substring(0, i);
                        break;
                    }
            }
            return result;
        }
    }
}