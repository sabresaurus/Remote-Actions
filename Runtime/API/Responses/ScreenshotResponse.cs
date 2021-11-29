using UnityEngine;
using System.IO;
using JetBrains.Annotations;

namespace Sabresaurus.RemoteActions.Responses
{
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