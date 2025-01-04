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
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.OpenXR;
using XrAction = Silk.NET.OpenXR.Action;

namespace nkast.LibOXR
{
    public class OxrSpace : IDisposable
    {
        private Space _space;
        private OxrAPI _oxrApi;

        public Space Space { get { return _space; } }

        internal OxrSpace(OxrAPI oxrAPI, Space space)
        {
            this._space = space;
            this._oxrApi = oxrAPI;

        }

        public override string ToString()
        {
            return String.Format("{{OxrSpace: {0} }}", _space.Handle.ToString("X"));
        }


        #region IDisposable
        ~OxrSpace()
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

            if (_space.Handle != 0)
            {
                _oxrApi.Api.DestroySpace(_space);
                _space.Handle = 0;
            }

            _oxrApi = null;
        }
        #endregion IDisposable
    }
}
