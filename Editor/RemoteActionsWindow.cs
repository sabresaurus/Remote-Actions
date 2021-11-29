using System;
using System.Collections.Generic;
using System.Linq;
using Sabresaurus.RemoteActions.Requests;
using Sabresaurus.RemoteActions.Responses;
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
        
        private Texture2D lastTexture = null;
        
        private APIManager APIManager => BridgingContext.Instance.container.APIManager;

        private RemoteActionsSettings Settings => BridgingContext.Instance.container.NetworkSettings;

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
        static void Init()
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
                if (response is ScreenshotResponse screenshotResponse)
                {
                    lastTexture = new Texture2D(2, 2);
                    lastTexture.LoadImage(screenshotResponse.PNGBytes);
                }

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
            GUIStyle centerMessageStyle = new GUIStyle(GUI.skin.label);
            centerMessageStyle.alignment = TextAnchor.MiddleCenter;
            centerMessageStyle.wordWrap = true;

            GUILayout.Space(9);

            bool validConnection = (EditorMessaging.KnownEndpoints.Count >= 1);

#if REMOTEACTIONS_DEBUG
            validConnection |= Settings.LocalDevMode;
#endif

            if (validConnection == false)
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

#if REMOTEACTIONS_DEBUG
            Settings.LocalDevMode = EditorGUILayout.Toggle("Local Dev Mode", Settings.LocalDevMode);
#endif
            if (validConnection)
            {
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
                    bool foldout = EditorGUILayout.BeginFoldoutHeaderGroup(true, customDisplay.GetType().Name);
                    customDisplay.OnGUI();
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                if (lastTexture != null && lastTexture.width != 2)
                {
                    var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.currentViewWidth / (lastTexture.width / (float) lastTexture.height));
                    GUI.DrawTexture(rect, lastTexture, ScaleMode.ScaleToFit);
                }
            }
        }
    }
}