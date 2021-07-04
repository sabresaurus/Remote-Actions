using Sabresaurus.RemoteActions.Responses;
using System.IO;
using UnityEngine;

namespace Sabresaurus.RemoteActions.Requests
{
    public class ScreenshotRequest : BaseRequest
    {
        public ScreenshotRequest()
        {
        }

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