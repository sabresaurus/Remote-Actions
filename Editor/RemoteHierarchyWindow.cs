using System;
using System.Collections.Generic;
using Sabresaurus.RemoteActions.Requests;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Sabresaurus.RemoteActions
{
    public class RemoteHierarchyWindow : EditorWindow
    {
        const float AUTO_REFRESH_FREQUENCY = 2f;
        
        RemoteActionsWindow parentWindow;

        TreeViewState treeViewState;
        HierarchyTreeView treeView;
        SearchField hierarchySearchField;
        private Dictionary<int, Guid> treeIndexesToTransformGuids = new Dictionary<int, Guid>();

        double timeLastRefreshed = 0;

        private static APIManager APIManager => BridgingContext.Instance.container.APIManager;

        private static SelectionManager SelectionManager => BridgingContext.Instance.container.SelectionManager;

        private static RemoteActionsSettings Settings => BridgingContext.Instance.container.NetworkSettings;

        void OnEnable()
        {
            // Check if we already had a serialized view state (state 
            // that survived assembly reloading)
            treeViewState ??= new TreeViewState();

            treeView = new HierarchyTreeView(treeViewState);
            treeView.OnSelectionChanged += OnHierarchySelectionChanged;

            hierarchySearchField = new SearchField();
            hierarchySearchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;

            //NetworkWrapper_Editor.RegisterConnection(OnPlayerConnected);
            //NetworkWrapper_Editor.RegisterDisconnection(OnPlayerDisconnected);

            APIManager.ResponseReceived -= OnResponseReceived;
            APIManager.ResponseReceived += OnResponseReceived;

            
            this.titleContent.image = EditorGUIUtility.TrIconContent("UnityEditor.SceneHierarchyWindow").image;
        }

        private void OnPlayerConnected(int playerID)
        {
            Repaint();

            APIManager.SendToPlayers(new GetHierarchyRequest());
        }

        private void OnPlayerDisconnected(int playerID)
        {
            Repaint();
        }

        void OnDisable()
        {
            APIManager.ResponseReceived -= OnResponseReceived;
        }

        void OnHierarchySelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count >= 1)
            {
                IList<TreeViewItem> items = treeView.GetRows();
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].id == selectedIds[0])
                    {
                        // Get the path of the selection
                        string path = GetPathForTreeViewItem(items[i]);
                        SelectionManager.SetSelectedPath(path);
                        Guid transformGuid = treeIndexesToTransformGuids[selectedIds[0]];
                        APIManager.SendToPlayers(new GetObjectRequest(transformGuid));

                        break;
                    }
                }
            }
        }

        void OnResponseReceived(BaseResponse response)
        {
            Repaint();
            //Debug.Log("Hierarchy OnResponseReceived");

            if (response is GetHierarchyResponse)
            {
                treeIndexesToTransformGuids.Clear();
                GetHierarchyResponse hierarchyResponse = (GetHierarchyResponse) response;
                List<TreeViewItem> displays = new List<TreeViewItem>();
                List<HierarchyNode> allNodes = new List<HierarchyNode>();

                int index = 0;
                foreach (var scene in hierarchyResponse.Scenes)
                {
                    displays.Add(new TreeViewItem {id = index, depth = 0, displayName = scene.SceneName});
                    allNodes.Add(null);
                    treeView.SetExpanded(index, true);
                    index++;
                    foreach (var node in scene.HierarchyNodes)
                    {
                        displays.Add(new TreeViewItem {id = index, depth = node.Depth + 1, displayName = node.ObjectName});
                        treeIndexesToTransformGuids[index] = node.TransformGUID;
                        allNodes.Add(node);
                        index++;
                    }
                }

                treeView.SetDisplays(displays, allNodes);
            }
        }

        bool AcquireParentWindowIfPossible()
        {
            RemoteActionsWindow[] windows = Resources.FindObjectsOfTypeAll<RemoteActionsWindow>();
            if (windows.Length > 0)
            {
                parentWindow = windows[0];

                return true;
            }
            else
            {
                return false;
            }
        }

        // Called at 10 frames per second
        void OnInspectorUpdate()
        {
            if (Settings.AutoRefreshRemote)
            {
                if (EditorApplication.timeSinceStartup > timeLastRefreshed + AUTO_REFRESH_FREQUENCY)
                {
                    timeLastRefreshed = EditorApplication.timeSinceStartup;
                    APIManager.SendToPlayers(new GetHierarchyRequest());
                    if (!string.IsNullOrEmpty(SelectionManager.SelectedPath)) // Valid path?
                    {
                        //APIManager.SendToPlayers(new GetObjectRequest(SelectionManager.SelectedPath, Settings.GetGameObjectFlags));
                    }
                }
            }
        }

        void OnGUI()
        {
            GUIStyle centerMessageStyle = new GUIStyle(GUI.skin.label);
            centerMessageStyle.alignment = TextAnchor.MiddleCenter;
            centerMessageStyle.wordWrap = true;

            if (parentWindow == null)
            {
                AcquireParentWindowIfPossible();
            }

            if (parentWindow == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Remote Actions window must be open to use remote hierarchy", centerMessageStyle);
                if (GUILayout.Button("Open Remote Actions"))
                {
                    RemoteActionsWindow.OpenWindow();
                    AcquireParentWindowIfPossible();
                }

                GUILayout.FlexibleSpace();


                return;
            }

            if (EditorMessaging.HasValidConnection != false)
            {
                if (GUILayout.Button("Refresh Hierarchy"))
                {
                    APIManager.SendToPlayers(new GetHierarchyRequest());
                }

                DoToolbar();
                DoTreeView();
            }
        }

        static string GetPathForTreeViewItem(TreeViewItem item)
        {
            string path = item.displayName;

            item = item.parent;
            while (item != null && item.depth >= 0)
            {
                if (item.depth > 0)
                    path = item.displayName + "/" + path;
                else
                    path = item.displayName + "//" + path;
                item = item.parent;
            }

            return path;
        }

        void DoToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            treeView.searchString = hierarchySearchField.OnToolbarGUI(treeView.searchString);
            GUILayout.EndHorizontal();
        }

        void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            treeView.OnGUI(rect);
        }
    }
}