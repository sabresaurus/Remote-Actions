using Sabresaurus.RemoteActions.Responses;
using System.IO;

namespace Sabresaurus.RemoteActions.Requests
{
    public abstract class BaseRequest
    {
        public virtual void Write(BinaryWriter bw)
        {
            
        }

        public abstract BaseResponse GenerateResponse();
    }
}