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
    public class OxrSwapChain : IDisposable
    {
        private Swapchain _swapchain;
        private OxrAPI _oxrApi;

        private uint _width;
        private uint _height;
            
        public Swapchain Swapchain { get { return _swapchain; } }
        public uint Width { get { return _width; } }
        public uint Height { get { return _height; } }

        internal OxrSwapChain(OxrAPI oxrAPI, Swapchain swapchain, uint width, uint height)
        {
            this._swapchain = swapchain;
            this._oxrApi = oxrAPI;

            this._width = width;
            this._height = height;
        }

        public unsafe uint GetImagesCount()
        {
            Result xrResult;

            uint imageCountOutput = default;
            xrResult = _oxrApi.Api.EnumerateSwapchainImages(
                this.Swapchain, 0, ref imageCountOutput, null);
            Debug.Assert(xrResult == Result.Success, "EnumerateSwapchainImages");

            return imageCountOutput;
        }

        public unsafe Result AcquireSwapchainImage(ref uint index)
        {
            Result xrResult;

            SwapchainImageAcquireInfo acquireInfo = new SwapchainImageAcquireInfo(StructureType.SwapchainImageAcquireInfo);
            xrResult = _oxrApi.Api.AcquireSwapchainImage(Swapchain, ref acquireInfo, ref index);

            return xrResult;
        }

        public unsafe Result ReleaseSwapchainImage()
        {
            Result xrResult;

            SwapchainImageReleaseInfo releaseInfo = new SwapchainImageReleaseInfo(StructureType.SwapchainImageReleaseInfo);
            xrResult = _oxrApi.Api.ReleaseSwapchainImage(Swapchain, ref releaseInfo);

            return xrResult;
        }

        public Result WaitSwapchainImage(ref SwapchainImageWaitInfo waitInfo)
        {
            Result xrResult;

            xrResult = _oxrApi.Api.WaitSwapchainImage(Swapchain, ref waitInfo);

            return xrResult;
        }

        public override string ToString()
        {
            return String.Format("{{OxrSwapChain: {0} }}", _swapchain.Handle.ToString("X"));
        }


        #region IDisposable
        ~OxrSwapChain()
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

            if (_swapchain.Handle != 0)
            {
                _oxrApi.Api.DestroySwapchain(_swapchain);
                _swapchain.Handle = 0;
            }

            _oxrApi = null;
        }
        #endregion IDisposable
    }
}
