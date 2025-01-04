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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.OpenXR;
using XrAction = Silk.NET.OpenXR.Action;

namespace nkast.LibOXR
{
    public class OxrSession : IDisposable
    {
        private Session _session;
        private OxrAPI _oxrApi;

        public Session Session { get { return _session; } }

        internal OxrSession(OxrAPI oxrAPI, Session session)
        {
            this._session = session;
            this._oxrApi = oxrAPI;

        }


        public unsafe Result WaitFrame(out FrameWaitInfo waitFrameInfo, out FrameState frameState)
        {
            Result xrResult;

            waitFrameInfo = new FrameWaitInfo(StructureType.FrameWaitInfo);
            frameState = new FrameState(StructureType.FrameState);
            xrResult = _oxrApi.Api.WaitFrame(Session, ref waitFrameInfo, ref frameState);

            return xrResult;
        }

        public unsafe Result BeginFrame(out FrameBeginInfo beginFrameDesc)
        {
            Result xrResult;

            beginFrameDesc = new FrameBeginInfo(StructureType.FrameBeginInfo);
            xrResult = _oxrApi.Api.BeginFrame(Session, ref beginFrameDesc);

            return xrResult;
        }

        public unsafe Result EndFrame(ref FrameEndInfo endFrameInfo)
        {
            Result xrResult;

            xrResult = _oxrApi.Api.EndFrame(Session, ref endFrameInfo);

            return xrResult;
        }

        public unsafe Result BeginSession(ViewConfigurationType viewConfigurationType)
        {
            Result xrResult;

            SessionBeginInfo sessionBeginInfo = new SessionBeginInfo(StructureType.SessionBeginInfo);
            sessionBeginInfo.PrimaryViewConfigurationType = viewConfigurationType;
            xrResult = _oxrApi.Api.BeginSession(Session, ref sessionBeginInfo);

            return xrResult;
        }

        public unsafe Result EndSession()
        {
            Result xrResult;

            xrResult = _oxrApi.Api.EndSession(Session);

            return xrResult;
        }


        public unsafe Result EnumerateReferenceSpaces(out ReferenceSpaceType[] referenceSpaces)
        {
            Result xrResult;

            uint numOutputSpaces = 0;
            xrResult = _oxrApi.Api.EnumerateReferenceSpaces(Session, 0, &numOutputSpaces, null);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "EnumerateReferenceSpaces");

            referenceSpaces = new ReferenceSpaceType[numOutputSpaces];

            xrResult = _oxrApi.Api.EnumerateReferenceSpaces(Session, numOutputSpaces, &numOutputSpaces, referenceSpaces);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "EnumerateReferenceSpaces");

            return xrResult;
        }

        public unsafe Result EnumerateSwapchainFormats(out long[] supportedFormats)
        {
            Result xrResult;

            uint numOutputFormats = 0;
            xrResult = _oxrApi.Api.EnumerateSwapchainFormats(Session, 0, &numOutputFormats, null);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "EnumerateSwapchainFormats");

            // Allocate an array large enough to contain the supported formats.
            supportedFormats = new long[numOutputFormats];

            xrResult = _oxrApi.Api.EnumerateSwapchainFormats(Session, numOutputFormats, &numOutputFormats, supportedFormats);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "EnumerateSwapchainFormats");
            
            return xrResult;
        }

        public Result CreateReferenceSpace(ref ReferenceSpaceCreateInfo spaceCreateInfo, out OxrSpace space)
        {
            Result xrResult;

            Space xrspace = default;
            xrResult = _oxrApi.Api.CreateReferenceSpace(Session, ref spaceCreateInfo, ref xrspace);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "CreateReferenceSpace");

            space = new OxrSpace(_oxrApi, xrspace);

            return xrResult;
        }

        public unsafe Result CreateReferenceSpace(ReferenceSpaceType referenceSpaceType, Posef poseInReferenceSpace, out OxrSpace space)
        {
            ReferenceSpaceCreateInfo spaceCreateInfo = new ReferenceSpaceCreateInfo(StructureType.ReferenceSpaceCreateInfo);
            spaceCreateInfo.ReferenceSpaceType = referenceSpaceType;
            spaceCreateInfo.PoseInReferenceSpace = poseInReferenceSpace;

            Result xrResult;

            Space xrspace = default;
            xrResult = _oxrApi.Api.CreateReferenceSpace(Session, ref spaceCreateInfo, ref xrspace);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "CreateReferenceSpace");

            space = new OxrSpace(_oxrApi, xrspace);

            return xrResult;
        }

        public Result GetReferenceSpaceBoundsRect(ReferenceSpaceType referenceSpaceType, out Extent2Df bounds)
        {
            Result xrResult;

            bounds = default;
            xrResult = _oxrApi.Api.GetReferenceSpaceBoundsRect(this.Session, referenceSpaceType, ref bounds);

            return xrResult;
        }

        public unsafe Result GetActionStatePose(XrAction action, ActionPath subactionPath, out ActionStatePose state)
        {
            Result xrResult;

            ActionStateGetInfo getInfo = new ActionStateGetInfo(StructureType.ActionStateGetInfo);
            getInfo.Action = action;
            getInfo.SubactionPath = subactionPath.Handle;

            state = new ActionStatePose(StructureType.ActionStatePose);
            xrResult = _oxrApi.Api.GetActionStatePose(Session, ref getInfo, ref state);

            return xrResult;
        }

        public unsafe ActionStateBoolean GetActionStateBoolean(XrAction action)
        {
            Result xrResult;

            ActionStateGetInfo getInfo = new ActionStateGetInfo(StructureType.ActionStateGetInfo);
            getInfo.Action = action;

            ActionStateBoolean state = new ActionStateBoolean(StructureType.ActionStateBoolean);

            xrResult = _oxrApi.Api.GetActionStateBoolean(Session, ref getInfo, ref state);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "GetActionStateBoolean");

            return state;
        }

        public unsafe ActionStateFloat GetActionStateFloat(XrAction action)
        {
            Result xrResult;

            ActionStateGetInfo getInfo = new ActionStateGetInfo( StructureType.ActionStateGetInfo);
            getInfo.Action = action;

            ActionStateFloat state = new ActionStateFloat(StructureType.ActionStateFloat);

            xrResult = _oxrApi.Api.GetActionStateFloat(Session, ref getInfo, ref state);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "GetActionStateFloat");

            return state;
        }

        public unsafe ActionStateVector2f GetActionStateVector2(XrAction action)
        {
            Result xrResult;

            ActionStateGetInfo getInfo = new ActionStateGetInfo(StructureType.ActionStateGetInfo);
            getInfo.Action = action;

            ActionStateVector2f state = new ActionStateVector2f(StructureType.ActionStateVector2f);

            xrResult = _oxrApi.Api.GetActionStateVector2(Session, &getInfo, &state);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "GetActionStateVector2");

            return state;
        }

        public unsafe void AttachSessionActionSets(OxrActionSet[] actionSets)
        {
            throw new NotImplementedException();
        }

        public unsafe Result AttachSessionActionSets(OxrActionSet actionSet)
        {
            Result xrResult;

            ActionSet localActionSet = actionSet.ActionSet;

            SessionActionSetsAttachInfo attachInfo = new SessionActionSetsAttachInfo(StructureType.SessionActionSetsAttachInfo);
            attachInfo.CountActionSets = 1;
            attachInfo.ActionSets = &localActionSet;
            xrResult = _oxrApi.Api.AttachSessionActionSets(Session, ref attachInfo);

            return xrResult;
        }

        public unsafe Result SyncActions(ActiveActionSet[] activeActionSets)
        {
            throw new NotImplementedException();
        }

        public unsafe Result SyncActions(OxrActionSet actionSet, ulong SubactionPath)
        {
            Result xrResult;

            ActiveActionSet activeActionSet = new ActiveActionSet();
            activeActionSet.ActionSet = actionSet.ActionSet;
            activeActionSet.SubactionPath = SubactionPath;

            ActionsSyncInfo syncInfo = new ActionsSyncInfo(StructureType.ActionsSyncInfo);
            syncInfo.CountActiveActionSets = 1;
            syncInfo.ActiveActionSets = &activeActionSet;
            xrResult = _oxrApi.Api.SyncAction(Session, ref syncInfo);

            return xrResult;
        }

        public Result CreateSwapchain(ref SwapchainCreateInfo createInfo, out OxrSwapChain swapchain)
        {
            Result xrResult;

            swapchain = null;
            Swapchain localSwapchain = default;
            xrResult = _oxrApi.Api.CreateSwapchain(Session, ref createInfo, ref localSwapchain);
            if (xrResult == Result.Success)
            {
                swapchain = new OxrSwapChain(_oxrApi, localSwapchain,
                                             createInfo.Width, createInfo.Height);
            }

            return xrResult;
        }

        public Result CreateActionSpace(ref ActionSpaceCreateInfo createInfo, ref Space space)
        {
            Result xrResult;

            xrResult = _oxrApi.Api.CreateActionSpace(Session, ref createInfo, ref space);

            return xrResult;
        }

        public unsafe OxrSpace CreateActionSpace(XrAction poseAction, ActionPath subactionPath)
        {
            Result xrResult;

            ActionSpaceCreateInfo createInfo = new ActionSpaceCreateInfo(StructureType.ActionSpaceCreateInfo);
            createInfo.Action = poseAction;
            createInfo.PoseInActionSpace.Orientation.W = 1.0f;
            createInfo.SubactionPath = subactionPath.Handle;
            Space space = default;

            xrResult = _oxrApi.Api.CreateActionSpace(Session, ref createInfo, ref space);
            System.Diagnostics.Debug.Assert(xrResult == Result.Success, "CreateActionSpace");

            return new OxrSpace(_oxrApi, space);
        }

        private static ulong[] EmptyBoundSources = new ulong[0];

        public unsafe ulong[] EnumerateBoundSourcesForAction(XrAction action)
        {
            Result xrResult;

            BoundSourcesForActionEnumerateInfo enumerateInfo = new BoundSourcesForActionEnumerateInfo(StructureType.BoundSourcesForActionEnumerateInfo);
            enumerateInfo.Action = action;

            // Get Count
            uint countOutput = 0;
            xrResult = _oxrApi.Api.EnumerateBoundSourcesForAction(this.Session, &enumerateInfo, 0 /* request size */, &countOutput, null);
            Debug.Assert(xrResult == Result.Success, "EnumerateBoundSourcesForAction");

            if (countOutput == 0)
                return EmptyBoundSources;

            ulong[] actionPathsBuffer = new ulong[countOutput];
            xrResult = _oxrApi.Api.EnumerateBoundSourcesForAction(this.Session, &enumerateInfo, countOutput, &countOutput, actionPathsBuffer);
            Debug.Assert(xrResult == Result.Success, "EnumerateBoundSourcesForAction");

            return actionPathsBuffer;
        }

        private unsafe delegate Result xrCreatePassthroughFB(Session xrSession, PassthroughCreateInfoFB ptci, PassthroughFB* passthrough);
        private unsafe delegate Result xrCreatePassthroughLayerFB(Session xrSession, PassthroughLayerCreateInfoFB plci, PassthroughLayerFB* passthroughLayer);

        public unsafe OxrPassthroughFB CreatePassthroughFB(PassthroughFlagsFB flags
            , OxrInstance oxrInstance)
        {
            Result xrResult;

            xrResult = oxrInstance.GetInstanceProcAddr("xrCreatePassthroughFB",
                out xrCreatePassthroughFB Fun_xrCreatePassthroughFB);
           
            PassthroughFB passthroughFB;
            PassthroughCreateInfoFB ptci = default;
            ptci.Type = StructureType.PassthroughCreateInfoFB;
            ptci.Flags = flags;
            xrResult = Fun_xrCreatePassthroughFB(this.Session, ptci, &passthroughFB);

            return new OxrPassthroughFB(passthroughFB, oxrInstance);
        }

        public unsafe OxrPassthroughLayerFB CreatePassthroughLayerFB(OxrPassthroughFB passthroughFB, PassthroughLayerPurposeFB purpose, PassthroughFlagsFB passthroughLayerFlags, OxrInstance oxrInstance)
        {
            Result xrResult;

            xrResult = oxrInstance.GetInstanceProcAddr("xrCreatePassthroughLayerFB",
                out xrCreatePassthroughLayerFB Fun_xrCreatePassthroughLayerFB);

            PassthroughLayerFB passthroughLayerFB = default;

            PassthroughLayerCreateInfoFB layerCreateInfo = default;
            layerCreateInfo.Type = StructureType.PassthroughLayerCreateInfoFB;
            layerCreateInfo.Passthrough = passthroughFB.PassthroughFB;
            layerCreateInfo.Purpose = purpose;
            layerCreateInfo.Flags = passthroughLayerFlags;
            xrResult = Fun_xrCreatePassthroughLayerFB(this.Session, layerCreateInfo, &passthroughLayerFB);

            return new OxrPassthroughLayerFB(passthroughLayerFB, oxrInstance);
        }

        public unsafe Result LocateView(
            ViewConfigurationType viewConfigurationType,
            long displayTime,
            OxrSpace _HeadSpace,
            out ViewState viewState, 
            uint viewCapacityInput, 
            out uint viewCountOutput,
            View[] views)
        {
            Result xrResult;

            ViewLocateInfo viewLocateInfo = new ViewLocateInfo(StructureType.ViewLocateInfo);
            viewLocateInfo.ViewConfigurationType = viewConfigurationType;
            viewLocateInfo.DisplayTime = displayTime;
            viewLocateInfo.Space = _HeadSpace.Space;

            viewState = new ViewState(StructureType.ViewState);
            viewCountOutput = default;

            fixed (View* pviews = views)
            {
                xrResult = _oxrApi.Api.LocateView(Session,
                in viewLocateInfo,
                ref viewState,
                viewCapacityInput,
                ref viewCountOutput,
                pviews);
            }

            return xrResult;
        }

        public unsafe Result StopHapticFeedback(XrAction action)
        {
            Result xrResult;

            HapticActionInfo hapticActionInfo = new HapticActionInfo(StructureType.HapticActionInfo);
            hapticActionInfo.Action = action;

            xrResult = _oxrApi.Api.StopHapticFeedback(Session, ref hapticActionInfo);

            return xrResult;
        }

        public unsafe Result ApplyHapticFeedback(XrAction action, HapticBaseHeader* hapticFeedback)
        {
            Result xrResult;

            HapticActionInfo hapticActionInfo = new HapticActionInfo(StructureType.HapticActionInfo);
            hapticActionInfo.Action = action;

            xrResult = _oxrApi.Api.ApplyHapticFeedback(Session, ref hapticActionInfo, (HapticBaseHeader*)&hapticFeedback);

            return xrResult;
        }

        public unsafe Result ApplyHapticFeedback(XrAction action, float amplitude, float frequency, long duration)
        {
            Result xrResult;

            HapticVibration vibration = new HapticVibration(StructureType.HapticVibration);
            vibration.Amplitude = amplitude;
            vibration.Frequency = frequency;
            vibration.Duration = duration;

            HapticActionInfo hapticActionInfo = new HapticActionInfo(StructureType.HapticActionInfo);
            hapticActionInfo.Action = action;

            xrResult = _oxrApi.Api.ApplyHapticFeedback(Session, ref hapticActionInfo, (HapticBaseHeader*)&vibration);

            return xrResult;
        }

        public unsafe Result GetCurrentInteractionProfile(ActionPath actionPath, out InteractionProfileState interactionProfileState)
        {
            Result xrResult;

            interactionProfileState = new InteractionProfileState(StructureType.InteractionProfileState);
            xrResult = _oxrApi.Api.GetCurrentInteractionProfile(Session, actionPath.Handle, ref interactionProfileState);

            return xrResult;
        }

        public override string ToString()
        {
            return String.Format("{{OxrSession: {0} }}", _session.Handle.ToString("X"));
        }


        #region IDisposable
        ~OxrSession()
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

            if (_session.Handle != 0)
            {
                _oxrApi.Api.DestroySession(_session);
                _session.Handle = 0;
            }

            _oxrApi = null;
        }
        #endregion IDisposable
    }
}
