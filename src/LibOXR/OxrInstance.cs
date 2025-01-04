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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.OpenXR;
using XrAction = Silk.NET.OpenXR.Action;

namespace nkast.LibOXR
{
    public class OxrInstance : IDisposable
    {
        private Instance _instance;
        private OxrAPI _oxrApi;
        private ulong _systemId;

        public OxrAPI OXRAPI { get { return _oxrApi; } }

        public Instance Instance { get { return _instance; } }
        public ulong SystemId { get { return _systemId; } }

        public string RuntimeName { get; private set; }
        public ulong RuntimeVersion { get; private set; }


        internal unsafe OxrInstance(OxrAPI oxrAPI, Instance xrInstance)
        {
            this._instance = xrInstance;
            this._oxrApi = oxrAPI;

            InstanceProperties instanceInfo = new InstanceProperties(StructureType.InstanceProperties);
            Result xrResult = _oxrApi.Api.GetInstanceProperties(Instance, ref instanceInfo);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "GetInstanceProperties");

            string runtimeName = Encoding.ASCII.GetString(instanceInfo.RuntimeName, (int)XR.MaxRuntimeNameSize);
            this.RuntimeName = runtimeName.TrimEnd('\0');
            this.RuntimeVersion = instanceInfo.RuntimeVersion;
        }


        public unsafe Result CreateSession(void* next, out OxrSession oxrSession)
        {
            Result xrResult;

            SessionCreateInfo sessionCreateInfo = new SessionCreateInfo(StructureType.SessionCreateInfo);
            sessionCreateInfo.Next = next;
            sessionCreateInfo.CreateFlags = 0;
            sessionCreateInfo.SystemId = SystemId;

            Session session = default;
            xrResult = _oxrApi.Api.CreateSession(Instance, ref sessionCreateInfo, ref session);
            oxrSession = null;
            if (xrResult == Result.Success)
                oxrSession = new OxrSession(_oxrApi, session);

            return xrResult;
        }

        public Result GetInstanceProcAddr<TDelegate>(string name, out TDelegate functionDelegate)
        {
            Result xrResult;

            PfnVoidFunction function = default;
            xrResult = _oxrApi.Api.GetInstanceProcAddr(Instance, name, ref function);
            functionDelegate = Marshal.GetDelegateForFunctionPointer<TDelegate>((IntPtr)function);

            return xrResult;
        }

        public unsafe Result GetSystem()
        {
            Result xrResult;

            SystemGetInfo systemGetInfo = new SystemGetInfo(StructureType.SystemGetInfo);
            systemGetInfo.FormFactor = FormFactor.HeadMountedDisplay;

            ulong systemId = 0;
            xrResult = _oxrApi.Api.GetSystem(Instance, ref systemGetInfo, ref systemId);
            _systemId = systemId;

            return xrResult;
        }

        public unsafe Result GetSystemProperties(void* next, out SystemProperties systemProperties)
        {
            Result xrResult;

            systemProperties = new SystemProperties(StructureType.SystemProperties);
            systemProperties.Next = next;

            xrResult = _oxrApi.Api.GetSystemProperties(Instance, SystemId, ref systemProperties);

            return xrResult;
        }

        public unsafe Result EnumerateViewConfiguration(out ViewConfigurationType[] viewportConfigurationTypes)
        {
            Result xrResult;

            uint viewportConfigTypeCount = 0;
            xrResult = _oxrApi.Api.EnumerateViewConfiguration(Instance, SystemId, 0, ref viewportConfigTypeCount, null);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "EnumerateViewConfiguration");

            viewportConfigurationTypes = new ViewConfigurationType[viewportConfigTypeCount];
            xrResult = _oxrApi.Api.EnumerateViewConfiguration(Instance, SystemId, viewportConfigTypeCount, &viewportConfigTypeCount, viewportConfigurationTypes);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "EnumerateViewConfiguration");

            return xrResult;
        }

        public unsafe Result GetViewConfigurationProperties(ViewConfigurationType viewportConfigType, out ViewConfigurationProperties viewportConfig)
        {
            Result xrResult;

            viewportConfig = new ViewConfigurationProperties(StructureType.ViewConfigurationProperties);
            xrResult = _oxrApi.Api.GetViewConfigurationProperties(Instance, SystemId, viewportConfigType, ref viewportConfig);

            return xrResult;
        }

        public unsafe Result EnumerateViewConfigurationView(ViewConfigurationType viewportConfigType, out ViewConfigurationView[] elements)
        {
            Result xrResult;

            uint viewCount;
            xrResult = _oxrApi.Api.EnumerateViewConfigurationView(Instance, SystemId, viewportConfigType, 0, &viewCount, null);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "EnumerateViewConfigurationView");

            elements = new ViewConfigurationView[viewCount];
            for (uint e = 0; e < elements.Length; e++)
            {
                elements[e].Type = StructureType.ViewConfigurationView;
                elements[e].Next = null;
            }

            xrResult = _oxrApi.Api.EnumerateViewConfigurationView(Instance, SystemId, viewportConfigType, viewCount, &viewCount, elements);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "EnumerateViewConfigurationView");

            return xrResult;
        }

        public unsafe Result LocateSpace(OxrSpace space, OxrSpace baseSpace, long time,
            out Posef pose,
            out SpaceLocationFlags locationFlags
            )
        {
            Result xrResult;

            SpaceLocation location = new SpaceLocation(StructureType.SpaceLocation);

            xrResult = _oxrApi.Api.LocateSpace(space.Space, baseSpace.Space, time, ref location);
            pose = location.Pose;
            locationFlags = location.LocationFlags;

            return xrResult;
        }

        public unsafe Result LocateSpace(OxrSpace space, OxrSpace baseSpace, long time, void* next,
            out Posef pose,
            out SpaceLocationFlags locationFlags
            )
        {
            Result xrResult;

            SpaceLocation location = new SpaceLocation(StructureType.SpaceLocation);
            location.Next = next;

            xrResult = _oxrApi.Api.LocateSpace(space.Space, baseSpace.Space, time, ref location);
            pose = location.Pose;
            locationFlags = location.LocationFlags;

            return xrResult;
        }

        public unsafe Result StringToPath(string pathString, out ActionPath path)
        {
            Result xrResult;

            path = default;
            xrResult = _oxrApi.Api.StringToPath(Instance, pathString, ref path.Handle);

            return xrResult;
        }

        public unsafe OxrActionSet CreateActionSet(int priority, string name, string localizedName)
        {
            Result xrResult;

            ActionSetCreateInfo asci = new ActionSetCreateInfo(StructureType.ActionSetCreateInfo);
            asci.Priority = (uint)priority;

            byte[] nameData = Encoding.ASCII.GetBytes(name);
            for (int i = 0; i < (int)XR.MaxActionSetNameSize && i < name.Length; i++)
                asci.ActionSetName[i] = nameData[i];

            byte[] localizedNameData = Encoding.ASCII.GetBytes(localizedName);
            for (int i = 0; i < (int)XR.MaxLocalizedActionSetNameSize && i < localizedName.Length; i++)
                asci.LocalizedActionSetName[i] = localizedNameData[i];

            ActionSet actionSet = new ActionSet(null);
            xrResult = _oxrApi.Api.CreateActionSet(Instance, ref asci, ref actionSet);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "CreateActionSet");
            return new OxrActionSet(_oxrApi, actionSet);
        }

        public unsafe ActionSuggestedBinding ActionSuggestedBinding(XrAction action, string bindingString)
        {
            Result xrResult;

            ActionSuggestedBinding asb = new ActionSuggestedBinding();
            asb.Action = action;

            xrResult = this.StringToPath(bindingString, out ActionPath bindingPath);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "StringToPath");

            asb.Binding = bindingPath.Handle;
            return asb;
        }

        public unsafe Result SuggestInteractionProfileBinding(ActionPath interactionProfilePath, ActionSuggestedBinding[] bindings)
        {
            Result xrResult;

            fixed (ActionSuggestedBinding* pbindings = bindings)
            {
                InteractionProfileSuggestedBinding suggestedBindings = default;
                suggestedBindings.Type = StructureType.InteractionProfileSuggestedBinding;
                suggestedBindings.InteractionProfile = interactionProfilePath.Handle;
                suggestedBindings.SuggestedBindings = pbindings;
                suggestedBindings.CountSuggestedBindings = (uint)bindings.Length;

                xrResult = _oxrApi.Api.SuggestInteractionProfileBinding(Instance, ref suggestedBindings);
            }

            return xrResult;
        }

        public Result PollEvent(ref EventDataBuffer eventDataBuffer)
        {
            Result xrResult;

            xrResult = _oxrApi.Api.PollEvent(Instance, ref eventDataBuffer);

            return xrResult;
        }

        public override string ToString()
        {
            return String.Format("{{OxrInstance: {0} }}", _instance.Handle.ToString("X"));
        }

        #region IDisposable
        ~OxrInstance()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_instance.Handle != 0)
            {
                _oxrApi.Api.DestroyInstance(_instance);
                _instance.Handle = 0;
            }

            _oxrApi = null;

        }
        #endregion IDisposable
    }
}
