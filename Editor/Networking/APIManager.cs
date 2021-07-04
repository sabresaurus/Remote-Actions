using System;
using System.IO;
using Sabresaurus.RemoteActions.Requests;
using Sabresaurus.RemoteActions.Responses;

namespace Sabresaurus.RemoteActions
{
    [System.Serializable]
    public class APIManager
    {
        int lastRequestID = 0;

        public Action<BaseResponse> ResponseReceived;


        public int SendToPlayers(BaseRequest request)
        {
            RemoteActionsSettings networkSettings = BridgingContext.Instance.container.NetworkSettings;
            lastRequestID++;
            
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(lastRequestID);

                    bw.Write(request.GetType().Name);
                    request.Write(bw);
                }
                bytes = ms.ToArray();
            }
#if REMOTEACTIONS_DEBUG
            if (settings.LocalDevMode)
            {
                byte[] testResponse = SidekickRequestProcessor.Process(bytes);

                BaseResponse response = SidekickResponseProcessor.Process(testResponse);
                if (ResponseReceived != null)
                    ResponseReceived(response);
            }
            else
#endif
            {
                EditorMessaging.SendRequest(bytes);
            }
            return lastRequestID;
        }
    }
}