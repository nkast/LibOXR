#region License
//   Copyright 2025 Kastellanos Nikolaos
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Core;
using Silk.NET.OpenXR;
using SilkXR = Silk.NET.OpenXR.XR;

namespace nkast.LibOXR
{
    public sealed class OxrAPI
    {
        private SilkXR _api;

        public SilkXR Api { get { return _api; } }

        private unsafe delegate Result xrInitializeLoaderKHRDelegate(LoaderInitInfoBaseHeaderKHR* input);

        private unsafe OxrAPI()
        {
            this._api = SilkXR.GetApi();
        }

        public unsafe static Result InitializeLoader(LoaderInitInfoBaseHeaderKHR* loaderInitInfo, out OxrAPI xrAPI)
        {
            xrAPI = null;

            OxrAPI api = new OxrAPI();
            Result xrResult = api.GetInstanceProcAddr("xrInitializeLoaderKHR", out xrInitializeLoaderKHRDelegate xrInitializeLoaderKHR);
            Debug.Assert(xrResult == Result.Success, "GetInstanceProcAddr");
            if (xrResult != Result.Success)
                return xrResult;

            xrResult = xrInitializeLoaderKHR(loaderInitInfo);
            if (xrResult != Result.Success)
                return xrResult;

            xrAPI = api;
            return xrResult;
        }

        private unsafe Result GetInstanceProcAddr<TDelegate>(string functionName, out TDelegate functionDelegate)
        {
            Result xrResult = default;
            functionDelegate = default;

            PfnVoidFunction function = default;
            xrResult = _api.GetInstanceProcAddr(new Instance(null), functionName, ref function);
            if (function.Handle == null)
                Console.WriteLine($"Failed to get xrInitializeLoaderKHR ProcAddr {xrResult}");
            if (xrResult != Result.Success)
                return xrResult;

            functionDelegate = Marshal.GetDelegateForFunctionPointer<TDelegate>((IntPtr)function);

            return xrResult;
        }

        public unsafe IList<string> GetLayers()
        {
            Result xrResult;

            uint numLayers = 0;
            xrResult = _api.EnumerateApiLayerProperties(0, &numLayers, null);
            Debug.Assert(xrResult == Result.Success, "EnumerateApiLayerProperties");

            List<string> layers = new List<string>((int)numLayers);

            ApiLayerProperties[] layerProperties = new ApiLayerProperties[numLayers];
            for (int i = 0; i < numLayers; i++)
            {
                layerProperties[i].Type = StructureType.ApiLayerProperties;
                layerProperties[i].Next = null;
            }

            xrResult = _api.EnumerateApiLayerProperties(numLayers, &numLayers, layerProperties);
            Debug.Assert(xrResult == Result.Success, "EnumerateApiLayerProperties");
            for (int i = 0; i < numLayers; i++)
            {
                fixed (ApiLayerProperties* p = &layerProperties[i])
                {
                    string layerName = Encoding.ASCII.GetString(p->LayerName, (int)SilkXR.MaxApiLayerNameSize); //256
                    layerName = layerName.TrimEnd('\0');
                    layers.Add(layerName);
                }
            }

            return layers;
        }

        public unsafe IList<string> GetExtensions()
        {
            Result xrResult;

            uint numExtensions = 0;
            xrResult = _api.EnumerateInstanceExtensionProperties(
                (byte*)null, 0, &numExtensions, null);
            Debug.Assert(xrResult == Result.Success, "EnumerateInstanceExtensionProperties");

            List<string> _extensions = new List<string>((int)numExtensions);

            ExtensionProperties[] extensionProperties = new ExtensionProperties[numExtensions];
            for (int i = 0; i < numExtensions; i++)
            {
                extensionProperties[i].Type = StructureType.ExtensionProperties;
                extensionProperties[i].Next = null;
            }

            xrResult = _api.EnumerateInstanceExtensionProperties(
                (byte*)null, numExtensions, &numExtensions, extensionProperties);
            Debug.Assert(xrResult == Result.Success, "EnumerateInstanceExtensionProperties");
            for (int i = 0; i < numExtensions; i++)
            {
                fixed (ExtensionProperties* p = &extensionProperties[i])
                {
                    string extensionName = Encoding.ASCII.GetString(p->ExtensionName, (int)SilkXR.MaxExtensionNameSize); //128                        
                    extensionName = extensionName.TrimEnd('\0');
                    _extensions.Add(extensionName);
                }
            }

            return _extensions;
        }

        public unsafe Result CreateInstance(string applicationName, string engineName, IReadOnlyList<string> requiredExtensionNames, out OxrInstance oxrInstance)
        {
            Result xrResult;

            ApplicationInfo appInfo = new ApplicationInfo();
            appInfo.ApplicationVersion = 0;
            appInfo.EngineVersion = 0;
            appInfo.ApiVersion = 0x0001_0000_00000000; // XR_MAKE_VERSION(1, 0, 34);
            OxrVersion ver1 = new OxrVersion(1, 0, 0);
            OxrVersion ver2 = new OxrVersion(1, 0, 34);
            appInfo.ApiVersion = ver2.Packed;

            byte[] applicationNameBytes = Encoding.UTF8.GetBytes(applicationName);
            for (int i = 0; i < (int)XR.MaxApplicationNameSize && i < applicationName.Length; i++)
                appInfo.ApplicationName[i] = applicationNameBytes[i];

            byte[] engineNameBytes = Encoding.UTF8.GetBytes(engineName);
            for (int i = 0; i < (int)XR.MaxEngineNameSize && i < engineName.Length; i++)
                appInfo.EngineName[i] = engineNameBytes[i];

            IntPtr prequiredExtensionNames = AllocNativeStringArray(requiredExtensionNames);

            InstanceCreateInfo instanceCreateInfo = new InstanceCreateInfo(StructureType.InstanceCreateInfo);
            instanceCreateInfo.CreateFlags = 0;
            instanceCreateInfo.ApplicationInfo = appInfo;
            instanceCreateInfo.EnabledApiLayerCount = 0;
            instanceCreateInfo.EnabledApiLayerNames = null;
            instanceCreateInfo.EnabledExtensionCount = (uint)requiredExtensionNames.Count;
            instanceCreateInfo.EnabledExtensionNames = (byte**)prequiredExtensionNames;

            Instance instance;
            xrResult = Api.CreateInstance(&instanceCreateInfo, &instance);
            if (xrResult != Result.Success)
            {
                oxrInstance = null;
                return xrResult;
            }
            oxrInstance = new OxrInstance(this, instance);

            FreeAllocNativeStringArray(prequiredExtensionNames, requiredExtensionNames.Count);

            xrResult = oxrInstance.GetSystem();



            return xrResult;
        }


        private unsafe static IntPtr AllocNativeStringArray(IReadOnlyList<string> strings)
        {
            IntPtr pArray = Marshal.AllocHGlobal(strings.Count * IntPtr.Size);

            for (int i = 0; i < strings.Count; i++)
            {
                int num = Encoding.UTF8.GetMaxByteCount(strings[i].Length) + 1;
                IntPtr pStr = Marshal.AllocHGlobal(num);
                fixed (char* chars = strings[i])
                {
                    int bytes = Encoding.UTF8.GetBytes(chars, strings[i].Length, (byte*)pStr, num);
                    Marshal.WriteByte(pStr, bytes, 0);
                }

                Marshal.WriteIntPtr(pArray, i * IntPtr.Size, pStr);
            }

            return pArray;
        }

        private static void FreeAllocNativeStringArray(IntPtr pArray, int length)
        {
            for (int i = 0; i < length; i++)
            {
                IntPtr pStr = Marshal.ReadIntPtr(pArray, i * IntPtr.Size);
                Marshal.FreeHGlobal(pStr);
            }
            Marshal.FreeHGlobal(pArray);
        }

    }
}
