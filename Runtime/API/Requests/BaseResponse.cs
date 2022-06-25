using System.IO;
using UnityEngine.Scripting;

namespace Sabresaurus.RemoteActions.Requests
{
    [Preserve, RequireDerived] // Prevent code stripping of this and derived types
    public abstract class BaseResponse
    {
        int requestID;

        public int RequestID
        {
            get { return requestID; }
        }

        public BaseResponse()
        {
        }

        public BaseResponse(BinaryReader br, int requestID)
        {
            this.requestID = requestID;
        }

        public abstract void Write(BinaryWriter bw);
    }
}