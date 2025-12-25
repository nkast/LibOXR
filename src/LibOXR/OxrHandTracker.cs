#region License
//   Copyright 2026 Kastellanos Nikolaos
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
using Silk.NET.OpenXR;

namespace nkast.LibOXR
{
    public class OxrHandTracker : IDisposable
    {
        private HandTrackerEXT _handTracker;

        public HandTrackerEXT HandTracker { get { return _handTracker; } }

        private unsafe delegate Result xrDestroyHandTrackerEXT(HandTrackerEXT handTracker);
        private unsafe delegate Result xrLocateHandJointsEXT(HandTrackerEXT handTracker, HandJointsLocateInfoEXT* locateInfo, HandJointLocationsEXT* locations);
        
        xrDestroyHandTrackerEXT Fun_xrDestroyHandTrackerEXT;
        xrLocateHandJointsEXT Fun_xrLocateHandJointsEXT;

        internal OxrHandTracker(HandTrackerEXT handTracker, OxrInstance oxrInstance)
        {
            this._handTracker = handTracker;

            Result xrResult;
            xrResult = oxrInstance.GetInstanceProcAddr("xrLocateHandJointsEXT",
                out Fun_xrLocateHandJointsEXT);
            xrResult = oxrInstance.GetInstanceProcAddr("xrDestroyHandTrackerEXT",
                out Fun_xrDestroyHandTrackerEXT);
        }

        public unsafe bool TryLocateJoints(OxrSpace baseSpace, long time, HandJointLocationEXT[] jointLocations)
        {
            return this.TryLocateJoints(baseSpace, time, jointLocations.AsSpan());
        }

        public unsafe bool TryLocateJoints(OxrSpace baseSpace, long time, Span<HandJointLocationEXT> jointLocations)
        {
            if (jointLocations == null || jointLocations.Length != XR.HandJointCountExt)
                throw new ArgumentException("jointLocations array must be of length XR.HandJointCountEXT");

            fixed (HandJointLocationEXT* pJointLocations = jointLocations)
            {
                HandJointsLocateInfoEXT locateInfo = default;
                locateInfo.Type = StructureType.HandJointsLocateInfoExt;
                locateInfo.BaseSpace = baseSpace.Space;
                locateInfo.Time = time;

                HandJointLocationsEXT locations = default;
                locations.Type = StructureType.HandJointLocationsExt;
                locations.JointCount = XR.HandJointCountExt;
                locations.JointLocations = pJointLocations;

                Result result = Fun_xrLocateHandJointsEXT(_handTracker, &locateInfo, &locations);
                if (result != Result.Success)
                    return false;

                return (locations.IsActive != 0);
            }
        }

        public override string ToString()
        {
            return String.Format("{{OxrHandTracker: {0} }}", _handTracker.Handle.ToString("X"));
        }

        #region IDisposable
        ~OxrHandTracker()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_handTracker.Handle != 0)
            {
                Fun_xrDestroyHandTrackerEXT(_handTracker);
                _handTracker.Handle = 0;
            }

        }
        #endregion IDisposable
    }
}