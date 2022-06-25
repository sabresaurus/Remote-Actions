using System.IO;
using UnityEngine.Scripting;

namespace Sabresaurus.RemoteActions.Requests
{
    [Preserve, RequireDerived] // Prevent code stripping of this and derived types
    public abstract class BaseRequest
    {
        public virtual void Write(BinaryWriter bw)
        {
            
        }

        public abstract BaseResponse GenerateResponse();
    }
}