using System.IO;
using JetBrains.Annotations;

namespace Sabresaurus.RemoteActions.Requests
{
    /// <summary>
    /// Tells a connected client that we're still interested and they shouldn't
    /// disconnect us
    /// </summary>
    [HideInDefaultList]
    public class HeartbeatRequest : BaseRequest
    {
        public HeartbeatRequest()
        {
        }

        [UsedImplicitly]
        public HeartbeatRequest(BinaryReader binaryReader)
        {
            
        }

        public override BaseResponse GenerateResponse()
        {
            return new HeartbeatResponse();
        }
    }
    
    public class HeartbeatResponse : BaseResponse
    {
        public HeartbeatResponse()
        {

        }

        [UsedImplicitly]
        public HeartbeatResponse(BinaryReader br, int requestID)
            : base(br, requestID)
        {
            
        }

        public override void Write(BinaryWriter bw)
        {
            
        }
    }
}