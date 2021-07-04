using UnityEngine;
using System.IO;
using Sabresaurus.RemoteActions.Requests;
using Sabresaurus.RemoteActions.Responses;
using System;

namespace Sabresaurus.RemoteActions
{
    public static class ResponseProcessor
    {
        public static BaseResponse Process(byte[] input)
        {
            using MemoryStream ms = new MemoryStream(input);
            using BinaryReader br = new BinaryReader(ms);
            
            // Read size here
            br.ReadInt32();

            int requestId = br.ReadInt32();

            if (requestId == -1) // Error?
            {
                throw new Exception(br.ReadString());
            }

            string requestType = br.ReadString();

#if DEBUG_RESPONSES
            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, requestType + "Response.bytes"), input);
#endif
            if (requestType.EndsWith("Request", StringComparison.InvariantCulture))
            {
                string responseType = requestType.Replace("Request", "Response");
                Type type = typeof(BaseResponse).Assembly.GetType("Sabresaurus.RemoteActions.Responses." + responseType);
                if (type != null && typeof(BaseResponse).IsAssignableFrom(type))
                {
                    return (BaseResponse)Activator.CreateInstance(type, br, requestId);
                }

                throw new NotImplementedException("Could not match a type to " + responseType + ". Does it have the correct namespace and assembly?");
            }

            throw new NotSupportedException("RequestType name must end in Request for automated substitution: " + requestType);
        }
    }
}
