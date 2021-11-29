﻿using UnityEngine;
using System.Collections;
using System.IO;
using JetBrains.Annotations;

namespace Sabresaurus.RemoteActions.Responses
{
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