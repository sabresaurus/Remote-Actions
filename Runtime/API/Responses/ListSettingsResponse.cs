using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace Sabresaurus.RemoteActions.Responses
{
    public class ListSettingsResponse : BaseResponse
    {
        private readonly List<SettingsGroup> settingsGroups;
        public List<SettingsGroup> SettingsGroups => settingsGroups;

        public ListSettingsResponse(List<SettingsGroup> settingsGroups)
        {
            this.settingsGroups = settingsGroups;
        }

        [UsedImplicitly]
        public ListSettingsResponse(BinaryReader br, int requestID)
            : base(br, requestID)
        {
            int groupCount = br.ReadInt32();

            settingsGroups = new List<SettingsGroup>(groupCount);

            for (int i = 0; i < groupCount; i++)
            {
                settingsGroups.Add(new SettingsGroup(br));
            }
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(settingsGroups.Count);

            foreach (var settingsGroup in settingsGroups)
            {
                settingsGroup.Write(bw);
            }
        }
    }
}