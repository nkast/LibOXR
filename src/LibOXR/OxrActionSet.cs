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
    public class OxrActionSet : IDisposable
    {
        private ActionSet _actionSet;
        private OxrAPI _oxrApi;

        public ActionSet ActionSet { get { return _actionSet; } }

        internal OxrActionSet(OxrAPI oxrAPI, ActionSet actionSet)
        {
            _actionSet = actionSet;
            this._oxrApi = oxrAPI;

        }


        public unsafe XrAction CreateAction(
            ActionType type,
            string actionName,
            string localizedName,
            int countSubactionPaths,
            ActionPath[] subactionPaths /*xrPath*/
            )
        {
            Console.WriteLine("CreateAction {0}, {1}", actionName, countSubactionPaths);

            Result xrResult;

            fixed (ActionPath* psubactionPaths = subactionPaths)
            {
                ActionCreateInfo aci = new ActionCreateInfo(StructureType.ActionCreateInfo);
                aci.ActionType = type;
                if (countSubactionPaths > 0)
                {
                    aci.CountSubactionPaths = (uint)countSubactionPaths;
                    aci.SubactionPaths = (ulong*)psubactionPaths;
                }

                byte[] actionNameData = Encoding.ASCII.GetBytes(actionName);
                for (int i = 0; i < (int)XR.MaxActionNameSize && i < actionName.Length; i++)
                    aci.ActionName[i] = actionNameData[i];

                localizedName = (localizedName != null) ? localizedName : actionName;
                byte[] localizedNameData = Encoding.ASCII.GetBytes(localizedName);
                for (int i = 0; i < (int)XR.MaxLocalizedActionNameSize && i < localizedName.Length; i++)
                    aci.LocalizedActionName[i] = localizedNameData[i];

                XrAction action = new XrAction(null);
                xrResult = _oxrApi.Api.CreateAction(_actionSet, &aci, &action);
                System.Diagnostics.Debug.Assert(xrResult == Result.Success, "CreateAction");
                return action;
            }
        }

        public override string ToString()
        {
            return String.Format("{{OxrActionSet: {0} }}", _actionSet.Handle.ToString("X"));
        }


        #region IDisposable
        ~OxrActionSet()
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

            if (_actionSet.Handle != 0)
            {
                _oxrApi.Api.DestroyActionSet(_actionSet);
                _actionSet.Handle = 0;
            }

            _oxrApi = null;
        }

        #endregion IDisposable
    }
}
