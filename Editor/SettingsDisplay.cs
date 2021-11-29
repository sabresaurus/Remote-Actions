using System.Collections.Generic;
using Sabresaurus.RemoteActions.Requests;
using Sabresaurus.RemoteActions.Responses;
using UnityEditor;
using UnityEngine;

namespace Sabresaurus.RemoteActions
{
    public class SettingsDisplay : CustomDisplay
    {
        List<SettingsGroup> settingsGroups = new List<SettingsGroup>();

        public override void OnResponseReceived(BaseResponse response)
        {
            if (response is ListSettingsResponse listSettingsResponse)
            {
                settingsGroups = listSettingsResponse.SettingsGroups;
            }
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("Refresh"))
            {
                APIManager.SendToPlayers(new ListSettingsRequest());
            }

            if (settingsGroups != null)
            {
                foreach (var objectGroup in settingsGroups)
                {
                    GUILayout.Label(objectGroup.GroupKey, EditorStyles.boldLabel);

                    foreach (var setting in objectGroup.Fields)
                    {
                        EditorGUI.BeginChangeCheck();
                        object newValue = GUIHelper.Draw(setting);

                        if (EditorGUI.EndChangeCheck())
                        {
                            setting.Value = newValue;
                            SetSettingRequest request = new SetSettingRequest(objectGroup.GroupKey, setting);
                            APIManager.SendToPlayers(request);
                        }
                    }
                }
            }
        }
    }
}