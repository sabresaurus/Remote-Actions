using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Sabresaurus.RemoteActions.Requests
{
    [HideInDefaultList]
    public class ListSettingsRequest : BaseRequest
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        
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

            foreach (KeyValuePair<string, object> trackedObject in SettingsObjectTracker.TrackedObjects)
            {
                List<WrappedVariable> variables = new List<WrappedVariable>();

                FieldInfo[] fields = trackedObject.Value.GetType().GetFields(BINDING_FLAGS);
                foreach (FieldInfo fieldInfo in fields)
                {
                    variables.Add(new WrappedVariable(fieldInfo, fieldInfo.GetValue(trackedObject.Value)));
                }

                PropertyInfo[] properties = trackedObject.Value.GetType().GetProperties(BINDING_FLAGS);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    bool isObsolete = propertyInfo.GetCustomAttributes(false).Any(item => item is ObsoleteAttribute);

                    if (isObsolete) continue;
                    variables.Add(new WrappedVariable(propertyInfo, propertyInfo.GetValue(trackedObject.Value)));
                }

                settingsGroups.Add(new SettingsGroup(trackedObject.Key, variables));
            }

            return new ListSettingsResponse(settingsGroups);
        }
    }

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

            foreach (SettingsGroup settingsGroup in settingsGroups)
            {
                settingsGroup.Write(bw);
            }
        }
    }
}