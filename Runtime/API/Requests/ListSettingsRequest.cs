using System.Collections.Generic;
using Sabresaurus.RemoteActions.Responses;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace Sabresaurus.RemoteActions.Requests
{
    [HideInDefaultList]
    public class ListSettingsRequest : BaseRequest
    {
        public ListSettingsRequest()
        {
        }

        [UsedImplicitly]
        public ListSettingsRequest(BinaryReader binaryReader)
        {
        }

        public override BaseResponse GenerateResponse()
        {
            List<SettingsGroup> settingsGroups = new List<SettingsGroup>();

            foreach (KeyValuePair<string, object> trackedObject in ObjectTracker.TrackedObjects)
            {
                FieldInfo[] fields = trackedObject.Value.GetType().GetFields();

                settingsGroups.Add(new SettingsGroup(trackedObject.Key, fields, trackedObject.Value));
            }

            return new ListSettingsResponse(settingsGroups);
        }
    }
}