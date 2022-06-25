using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace Sabresaurus.RemoteActions.Requests
{
    [HideInDefaultList]
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
    
    public class ScreenshotResponse : BaseResponse
    {
        private byte[] pngBytes;

        public byte[] PNGBytes => pngBytes;

        public ScreenshotResponse(byte[] pngBytes)
        {
            this.pngBytes = pngBytes;
        }

        [UsedImplicitly]
        public ScreenshotResponse(BinaryReader br, int requestID)
            : base(br, requestID)
        {
            int length = br.ReadInt32();
            pngBytes = br.ReadBytes(length);
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(pngBytes.Length);
            bw.Write(pngBytes);
        }
    }
}