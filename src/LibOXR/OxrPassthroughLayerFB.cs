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
    public class OxrPassthroughLayerFB : IDisposable
    {
        private PassthroughLayerFB _passthroughLayerFB;

        public PassthroughLayerFB PassthroughLayerFB { get { return _passthroughLayerFB; } }

        private unsafe delegate Result xrDestroyPassthroughLayerFB(PassthroughLayerFB passthroughLayer);
        private unsafe delegate Result xrPassthroughLayerResumeFB(PassthroughLayerFB passthroughLayer);
        private unsafe delegate Result xrPassthroughLayerPauseFB(PassthroughLayerFB passthroughLayer);
        private unsafe delegate Result xrPassthroughLayerSetStyleFB(PassthroughLayerFB passthroughLayer, PassthroughStyleFB* style);

        xrDestroyPassthroughLayerFB Fun_xrDestroyPassthroughLayerFB;
        xrPassthroughLayerResumeFB Fun_xrPassthroughLayerResumeFB;
        xrPassthroughLayerPauseFB Fun_xrPassthroughLayerPauseFB;
        xrPassthroughLayerSetStyleFB Fun_xrPassthroughLayerSetStyleFB;

        public OxrPassthroughLayerFB(PassthroughLayerFB passthroughLayerFB, OxrInstance oxrInstance)
        {
            this._passthroughLayerFB = passthroughLayerFB;

            Result xrResult;

            xrResult = oxrInstance.GetInstanceProcAddr("xrDestroyPassthroughLayerFB",
                out Fun_xrDestroyPassthroughLayerFB);
            xrResult = oxrInstance.GetInstanceProcAddr("xrPassthroughLayerResumeFB",
                    out Fun_xrPassthroughLayerResumeFB);
            xrResult = oxrInstance.GetInstanceProcAddr("xrPassthroughLayerPauseFB",
                out Fun_xrPassthroughLayerPauseFB);
            xrResult = oxrInstance.GetInstanceProcAddr("xrPassthroughLayerSetStyleFB",
                out Fun_xrPassthroughLayerSetStyleFB);
        }

        public Result Resume()
        {
            return Fun_xrPassthroughLayerResumeFB(_passthroughLayerFB);
        }

        public Result Pause()
        {
            return Fun_xrPassthroughLayerPauseFB(_passthroughLayerFB);
        }

        public unsafe Result SetStyle(float textureOpacityFactor, Color4f edgeColor)
        {
            PassthroughStyleFB style = default;
            style.Type = StructureType.PassthroughStyleFB;
            style.TextureOpacityFactor = textureOpacityFactor;
            style.EdgeColor = edgeColor;

            return Fun_xrPassthroughLayerSetStyleFB(_passthroughLayerFB, &style);
        }

        public override string ToString()
        {
            return String.Format("{{PassthroughLayerFB: {0} }}", _passthroughLayerFB.Handle.ToString("X"));
        }


        #region IDisposable
        ~OxrPassthroughLayerFB()
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

            if (_passthroughLayerFB.Handle != 0)
            {
                 Fun_xrDestroyPassthroughLayerFB(_passthroughLayerFB);
                _passthroughLayerFB.Handle = 0;
            }
        }

        #endregion IDisposable
    }
}