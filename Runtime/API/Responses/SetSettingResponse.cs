using System.IO;
using JetBrains.Annotations;

namespace Sabresaurus.RemoteActions.Responses
{
    public class SetSettingResponse : BaseResponse
    {
        public SetSettingResponse()
        {
        }

        [UsedImplicitly]
        public SetSettingResponse(BinaryReader br, int requestID)
            : base(br, requestID)
        {
        }

        public override void Write(BinaryWriter bw)
        {
        }
    }
}