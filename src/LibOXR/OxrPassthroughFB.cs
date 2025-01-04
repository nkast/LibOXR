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
using Silk.NET.OpenXR;

namespace nkast.LibOXR
{
    public class OxrPassthroughFB : IDisposable
    {
        private PassthroughFB _passthroughFB;

        public PassthroughFB PassthroughFB { get { return _passthroughFB; } }

        private unsafe delegate Result xrDestroyPassthroughFB(PassthroughFB passthrough);
        public unsafe delegate Result xrPassthroughStartFB(PassthroughFB passthrough);
        public unsafe delegate Result xrPassthroughPauseFB(PassthroughFB passthrough);

        xrDestroyPassthroughFB Fun_xrDestroyPassthroughFB;
        xrPassthroughStartFB Fun_xrPassthroughStartFB;
        xrPassthroughPauseFB Fun_xrPassthroughPauseFB;

        public OxrPassthroughFB(PassthroughFB passthroughFB, OxrInstance oxrInstance)
        {
            this._passthroughFB = passthroughFB;

            Result xrResult;
            xrResult = oxrInstance.GetInstanceProcAddr("xrDestroyPassthroughFB",
                out Fun_xrDestroyPassthroughFB);
            xrResult = oxrInstance.GetInstanceProcAddr("xrPassthroughStartFB",
                out Fun_xrPassthroughStartFB);
            xrResult = oxrInstance.GetInstanceProcAddr("xrPassthroughPauseFB",
                out Fun_xrPassthroughPauseFB);
        }

        public Result Start()
        {
            return Fun_xrPassthroughStartFB(_passthroughFB);
        }

        public Result Pause()
        {
            return Fun_xrPassthroughPauseFB(_passthroughFB);
        }

        public override string ToString()
        {
            return String.Format("{{OxrPassthroughFB: {0} }}", _passthroughFB.Handle.ToString("X"));
        }


        #region IDisposable
        ~OxrPassthroughFB()
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

            if (_passthroughFB.Handle != 0)
            {
                Fun_xrDestroyPassthroughFB(_passthroughFB);
                _passthroughFB.Handle = 0;
            }
        }

        #endregion IDisposable
    }
}