using System;
using System.Collections.Generic;
using System.Linq;
using Sabresaurus.RemoteActions.Requests;
using UnityEditor;
using UnityEngine;

namespace Sabresaurus.RemoteActions
{
    public class RemoteActionsWindow : EditorWindow
    {
        int cachedPlayerCount = 0;

        DateTime lastSentHeartbeat = DateTime.MinValue;
        private bool registered;

        private List<CustomDisplay> customDisplays;

        private APIManager APIManager => BridgingContext.Instance.container.APIManager;

        private RemoteActionsSettings Settings => BridgingContext.Instance.container.NetworkSettings;
        Vector2 scrollPosition = Vector2.zero;

        void OnEnable()
        {
            APIManager.ResponseReceived -= OnResponseReceived;
            APIManager.ResponseReceived += OnResponseReceived;

            this.titleContent.image = EditorGUIUtility.TrIconContent("PlayButton On").image;

            customDisplays = new List<CustomDisplay>();
            var customDisplayTypes = TypeCache.GetTypesDerivedFrom<CustomDisplay>();
            foreach (var customDisplayType in customDisplayTypes)
            {
                if (!customDisplayType.IsAbstract)
                {
                    customDisplays.Add((CustomDisplay) Activator.CreateInstance(customDisplayType));
                }
            }
        }

        [MenuItem("Window/Remote Actions")]
        public static void OpenWindow()
        {
            // Get existing open window or if none, make a new one:
            GetWindow<RemoteActionsWindow>("Remote Actions");
        }

        void OnDisable()
        {
            APIManager.ResponseReceived -= OnResponseReceived;
        }

        private void OnReceivedResponse(byte[] responseData)
        {
            BaseResponse response = ResponseProcessor.Process(responseData);
            APIManager.ResponseReceived(response);
        }

        void OnResponseReceived(BaseResponse response)
        {
            if (!(response is HeartbeatResponse))
            {
                foreach (var customDisplay in customDisplays)
                {
                    customDisplay.OnResponseReceived(response);
                }

#if REMOTEACTIONS_DEBUG
                Debug.Log("Received response: " + response);
#endif
            }

            Repaint();
        }

        // Called at 10 frames per second
        void OnInspectorUpdate()
        {
            if (!EditorMessaging.Started)
            {
                EditorMessaging.Start();
            }

            EditorMessaging.Tick();

            EditorMessaging.RegisterForResponses(OnReceivedResponse);
            if (EditorMessaging.KnownEndpoints.Count >= 1)
            {
                if (!registered)
                {
                    EditorMessaging.RegisterForResponses(OnReceivedResponse);
                    registered = true;
                }
            }
            else
            {
                if (registered)
                {
                    registered = false;
                    //EditorConnection.instance.Unregister(RuntimeSidekickBridge.SEND_PLAYER_TO_EDITOR, OnMessageEvent);
                }
            }


            if (EditorMessaging.KnownEndpoints.Count != cachedPlayerCount)
            {
                cachedPlayerCount = EditorMessaging.KnownEndpoints.Count;
                Repaint();
            }

            if (EditorMessaging.IsConnected)
            {
                // If there's a valid connection send a heartbeat every second so the device knows we're still here
                if (DateTime.UtcNow - lastSentHeartbeat > TimeSpan.FromSeconds(1))
                {
                    APIManager.SendToPlayers(new HeartbeatRequest());
                    lastSentHeartbeat = DateTime.UtcNow;
                }
            }
        }

        void OnGUI()
        {
            GUILayout.Space(9);

            if (EditorMessaging.HasValidConnection == false)
            {
                EditorGUILayout.HelpBox("No player found, make sure both the editor and player are on the same network", MessageType.Warning);
            }
            else
            {
                List<string> displayNames = new List<string>();
                int index = 0;
                int selectionIndex = 0;
                foreach (var pair in EditorMessaging.KnownEndpoints)
                {
                    displayNames.Add($"{pair.Key} - {pair.Value}");
                    if (pair.Key == EditorMessaging.ConnectedIP)
                    {
                        selectionIndex = index;
                    }

                    index++;
                }

                EditorGUILayout.Popup(selectionIndex, displayNames.ToArray());
            }

            Settings.AutoRefreshRemote = EditorGUILayout.Toggle("Auto Refresh Remote", Settings.AutoRefreshRemote);
#if REMOTEACTIONS_DEBUG
            Settings.LocalDevMode = EditorGUILayout.Toggle("Local Dev Mode", Settings.LocalDevMode);
#endif
            if (EditorMessaging.HasValidConnection)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                TypeCache.TypeCollection requestTypes = TypeCache.GetTypesDerivedFrom<BaseRequest>();

                GUILayout.Label("Actions", EditorStyles.boldLabel);

                foreach (var requestType in requestTypes)
                {
                    var customAttributes = requestType.GetCustomAttributes(true);
                    if (customAttributes.Any(item => item is HideInDefaultListAttribute))
                    {
                        continue;
                    }

                    string displayName = requestType.Name;

                    if (displayName.EndsWith("Request"))
                    {
                        displayName = displayName.Remove(displayName.LastIndexOf("Request"));
                    }

                    displayName = ObjectNames.NicifyVariableName(displayName);

                    if (GUILayout.Button(displayName))
                    {
                        APIManager.SendToPlayers((BaseRequest) Activator.CreateInstance(requestType));
                    }
                }

                foreach (var customDisplay in customDisplays)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    GUIStyle style = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold};
                    GUILayout.Label(customDisplay.GetType().Name, style);
                    EditorGUILayout.EndHorizontal();
                    customDisplay.OnGUI();
                    GUILayout.Space(20);
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
}