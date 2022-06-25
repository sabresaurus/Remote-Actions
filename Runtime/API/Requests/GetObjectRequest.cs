using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sabresaurus.RemoteActions.Requests
{
    [HideInDefaultList]
    public class GetObjectRequest : BaseRequest
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        string objectPath; // Path in scene or assets
        Guid guid; // GUID directly to any System.Object known by the ObjectMap

        public GetObjectRequest(Guid objectGuid)
        {
            this.guid = objectGuid;
        }

        public GetObjectRequest(BinaryReader br)
        {
            this.guid = new Guid(br.ReadString());
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(this.guid.ToString());
        }

        public override BaseResponse GenerateResponse()
        {
            GetObjectResponse response = new GetObjectResponse();
            List<object> components = new List<object>();

            object foundObject = ObjectMap.GetObjectFromGUID(guid);
            if (foundObject is Transform foundTransform)
            {
                // Not technically a component, but include the GameObject
                components.Add(foundTransform.gameObject);
                components.AddRange(foundTransform.GetComponents<Component>());
            }
            else
            {
                components.Add(foundObject);
            }

            Object foundUnityObject = foundObject as Object;
            if (foundUnityObject != null)
            {
                response.ObjectName = foundUnityObject.name;
            }

            response.Components = new List<ComponentDescription>(components.Count);
            foreach (object component in components)
            {
                //Guid guid = ObjectMap.AddOrGetObject(component);
                ObjectMap.AddOrGetObject(component);

                ComponentDescription description = new ComponentDescription(component);
                Type componentType = component.GetType();

                while (componentType != null)
                {
                    if (!InspectionExclusions.GetExcludedTypes().Contains(componentType))
                    {
                        ComponentScope componentScope = new ComponentScope(componentType);
                        FieldInfo[] fieldInfos = componentType.GetFields(BINDING_FLAGS);
                        foreach (FieldInfo fieldInfo in fieldInfos)
                        {
                            if (TypeUtility.IsBackingField(fieldInfo, componentType))
                            {
                                // Skip backing fields for auto-implemented properties
                                continue;
                            }

                            object objectValue = fieldInfo.GetValue(component);

                            WrappedVariable wrappedVariable = new WrappedVariable(fieldInfo, objectValue);
                            componentScope.Fields.Add(wrappedVariable);
                        }

                        if (componentType == typeof(GameObject)) // Special handling for GameObject.name to always be included
                        {
                            PropertyInfo nameProperty = componentType.GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                            WrappedVariable wrappedName = new WrappedVariable(nameProperty, nameProperty.GetValue(component, null));
                            componentScope.Properties.Add(wrappedName);
                        }

                        PropertyInfo[] properties = componentType.GetProperties(BINDING_FLAGS);
                        foreach (PropertyInfo property in properties)
                        {
                            Type declaringType = property.DeclaringType;
                            if (declaringType == typeof(Component)
                                || declaringType == typeof(UnityEngine.Object))
                            {
                                continue;
                            }

                            object[] attributes = property.GetCustomAttributes(false);
                            bool isObsoleteWithError = AttributeHelper.IsObsoleteWithError(attributes);
                            if (isObsoleteWithError)
                            {
                                continue;
                            }

                            // Skip properties that cause exceptions at edit time
                            if (Application.isPlaying == false)
                            {
                                if (typeof(MeshFilter).IsAssignableFrom(declaringType))
                                {
                                    if (property.Name == "mesh")
                                    {
                                        continue;
                                    }
                                }

                                if (typeof(Renderer).IsAssignableFrom(declaringType))
                                {
                                    if (property.Name == "material" || property.Name == "materials")
                                    {
                                        continue;
                                    }
                                }
                            }


                            string propertyName = property.Name;

                            MethodInfo getMethod = property.GetGetMethod(true);
                            if (getMethod != null)
                            {
                                object objectValue = getMethod.Invoke(component, null);

                                WrappedVariable wrappedVariable = new WrappedVariable(property, objectValue);
                                componentScope.Properties.Add(wrappedVariable);
                            }
                        }

                        MethodInfo[] methodInfos = componentType.GetMethods(BINDING_FLAGS);
                        foreach (var methodInfo in methodInfos)
                        {
                            if (TypeUtility.IsPropertyMethod(methodInfo, componentType))
                            {
                                // Skip automatically generated getter/setter methods
                                continue;
                            }

                            MethodImplAttributes methodImplAttributes = methodInfo.GetMethodImplementationFlags();
                            if ((methodImplAttributes & MethodImplAttributes.InternalCall) != 0 && methodInfo.Name.StartsWith("INTERNAL_"))
                            {
                                // Skip any internal method if it also begins with INTERNAL_
                                continue;
                            }

                            WrappedMethod wrappedMethod = new WrappedMethod(methodInfo);
                            componentScope.Methods.Add(wrappedMethod);
                        }

                        description.Scopes.Add(componentScope);
                    }
                    componentType = componentType.BaseType;
                }

                response.Components.Add(description);
            }

            return response;
        }
    }
    
    public class GetObjectResponse : BaseResponse
    {
        string objectName = "";
        List<ComponentDescription> components = new List<ComponentDescription>();

        public GetObjectResponse(BinaryReader br, int requestID)
            : base(br, requestID)
        {
            objectName = br.ReadString();

            int componentCount = br.ReadInt32();
            for (int i = 0; i < componentCount; i++)
            {
                components.Add(new ComponentDescription(br));
            }
        }

        public GetObjectResponse()
        {

        }


        public override void Write(BinaryWriter bw)
        {
            bw.Write(objectName);
            bw.Write(components.Count);

            foreach (ComponentDescription item in components)
            {
                item.Write(bw);
            }
        }

        public string ObjectName
        {
            get
            {
                return objectName;
            }

            set
            {
                objectName = value;
            }
        }

        public List<ComponentDescription> Components
        {
            get
            {
                return components;
            }

            set
            {
                components = value;
            }
        }
    }
}