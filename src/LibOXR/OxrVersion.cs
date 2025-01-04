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
using System.Runtime.InteropServices;

namespace nkast.LibOXR
{
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public struct OxrVersion
    {
        // The major version number is a 16-bit integer packed into bits 63-48.
        // The minor version number is a 16 - bit integer packed into bits 47 - 32.
        // The patch version number is a 32 - bit integer packed into bits 31 - 0.

        [System.Runtime.InteropServices.FieldOffset(0)]
        public Int32 Patch;
        [System.Runtime.InteropServices.FieldOffset(4)]
        public Int16 Minor;
        [System.Runtime.InteropServices.FieldOffset(6)]
        public Int16 Major;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public UInt64 Packed;

        public OxrVersion(Int16 major, Int16 minor, Int32 patch) : this()
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        public OxrVersion(UInt64 packed) : this()
        {
            this.Packed = packed;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
        }
    }
}
