using Sabresaurus.RemoteActions.Responses;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace Sabresaurus.RemoteActions.Requests
{
    [UsedImplicitly]
    public class ScreenshotRequest : BaseRequest
    {
        public ScreenshotRequest()
        {
        }

        [UsedImplicitly]
        public ScreenshotRequest(BinaryReader br)
        {
        }

        public override BaseResponse GenerateResponse()
        {
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] pngBytes = texture.EncodeToPNG();
            return new ScreenshotResponse(pngBytes);
        }
    }
}