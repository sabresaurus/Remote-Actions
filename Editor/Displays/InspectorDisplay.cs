using System.Collections.Generic;
using JetBrains.Annotations;
using Sabresaurus.RemoteActions.Requests;
using UnityEditor;
using UnityEngine;

namespace Sabresaurus.RemoteActions
{
    [UsedImplicitly]
    public class InspectorDisplay : CustomDisplay
    {
        List<ComponentDescription> components = new List<ComponentDescription>();

        public override void OnResponseReceived(BaseResponse response)
        {
            if (response is GetObjectResponse getObjectResponse)
            {
                components = getObjectResponse.Components;
            }
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("Open Remote Hierarchy"))
            {
                EditorWindow.GetWindow<RemoteHierarchyWindow>("Remote").Show();
            }

            if (components != null)
            {
                foreach (var component in components)
                {
                    Rect foldoutRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.foldoutHeader);
                
                    Rect toggleRect = foldoutRect;
                    toggleRect.xMin += 15;
                    toggleRect.width = 20;

                    GUIContent content = new GUIContent("     " + component.TypeShortName, component.TypeFullName);
                    bool? activeOrEnabled = null;
                    if (component.TypeShortName == "GameObject")
                    {
                        activeOrEnabled = (bool)component.Scopes[0].GetPropertyValue("activeSelf");
                    }
                    else
                    {
                        ComponentScope behaviourScope = component.BehaviourScope;
                        if (behaviourScope != null)
                        {
                            activeOrEnabled = (bool)behaviourScope.GetPropertyValue("enabled");
                        }
                        else
                        {
                            ComponentScope rendererScope = component.RendererScope;
                            if (rendererScope != null)
                            {
                                activeOrEnabled = (bool)rendererScope.GetPropertyValue("enabled");
                            }
                        }
                    }
                    
                    // Have to do this before BeginFoldoutHeaderGroup otherwise it'll consume the mouse down event
                    if (activeOrEnabled.HasValue && GUIHelper.DetectClickInRect(toggleRect))
                    {
                        activeOrEnabled = !activeOrEnabled.Value;
                        if (component.TypeShortName == "GameObject")
                        {
                            // Update local cache (requires method call)
                            var property = component.Scopes[0].GetProperty("activeSelf");
                            property.Value = activeOrEnabled.Value;

                            // Update via method call
                            APIManager.SendToPlayers(new InvokeMethodRequest(component.Guid, "SetActive", new WrappedVariable[] { new WrappedVariable("", activeOrEnabled.Value, typeof(bool), false) }));
                        }
                        else if (component.BehaviourScope != null)
                        {
                            // Update local cache, then ship via SetVariable
                            var property = component.BehaviourScope.GetProperty("enabled");
                            property.Value = activeOrEnabled.Value;

                            APIManager.SendToPlayers(new SetVariableRequest(component.Guid, property));
                        }
                        else if (component.RendererScope != null)
                        {
                            // Update local cache, then ship via SetVariable
                            var property = component.RendererScope.GetProperty("enabled");
                            property.Value = activeOrEnabled.Value;

                            APIManager.SendToPlayers(new SetVariableRequest(component.Guid, property));
                        }
                    }

                    bool foldout = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, true, content, EditorStyles.foldoutHeader);
                    
                    if (activeOrEnabled.HasValue)
                    {
                        EditorGUI.Toggle(toggleRect, activeOrEnabled.Value);
                    }
                    
                    foreach (var componentDescriptionScope in component.Scopes)
                    {
                        foreach (var variable in componentDescriptionScope.Fields)
                        {
                            EditorGUI.BeginChangeCheck();
                            object newValue = GUIHelper.Draw(variable);

                            if (EditorGUI.EndChangeCheck())
                            {
                                variable.Value = newValue;
                                SetVariableRequest request = new SetVariableRequest(component.Guid, variable);
                                APIManager.SendToPlayers(request);
                            }
                        }
                    }

                    foreach (var componentDescriptionScope in component.Scopes)
                    {
                        foreach (var variable in componentDescriptionScope.Properties)
                        {
                            EditorGUI.BeginChangeCheck();
                            object newValue = GUIHelper.Draw(variable);

                            if (EditorGUI.EndChangeCheck())
                            {
                                variable.Value = newValue;
                                SetVariableRequest request = new SetVariableRequest(component.Guid, variable);
                                APIManager.SendToPlayers(request);
                            }
                        }
                    }

                    // foreach (var componentDescriptionScope in componentDescription.Scopes)
                    // {
                    //     foreach (var wrappedVariable in componentDescriptionScope.Methods)
                    //     {
                    //         GUILayout.Label(wrappedVariable.MethodName);
                    //     }
                    // }
                    EditorGUI.EndFoldoutHeaderGroup();
                }

                // foreach (var objectGroup in components)
                // {
                //     GUILayout.Label(objectGroup.GroupKey, EditorStyles.boldLabel);
                //
                //     foreach (var setting in objectGroup.Variables)
                //     {
                //         EditorGUI.BeginChangeCheck();
                //         object newValue = GUIHelper.Draw(setting);
                //
                //         if (EditorGUI.EndChangeCheck())
                //         {
                //             setting.Value = newValue;
                //             SetSettingRequest request = new SetSettingRequest(objectGroup.GroupKey, setting);
                //             APIManager.SendToPlayers(request);
                //         }
                //     }
                // }
            }
        }
    }
}