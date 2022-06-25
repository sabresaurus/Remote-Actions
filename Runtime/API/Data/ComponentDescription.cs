using System;
using System.Collections.Generic;
using System.IO;

namespace Sabresaurus.RemoteActions
{
    public class ComponentDescription
    {
        Guid guid;

        public ComponentScope BehaviourScope
        {
            get
            {
                foreach (ComponentScope scope in Scopes)
                {
                    if (scope.TypeShortName == "Behaviour")
                    {
                        return scope;
                    }
                }

                return null;
            }
        }
        
        public ComponentScope RendererScope
        {
            get
            {
                foreach (ComponentScope scope in Scopes)
                {
                    if (scope.TypeShortName == "Renderer")
                    {
                        return scope;
                    }
                }

                return null;
            }
        }

        public ComponentDescription(object component)
        {
            Type componentType = component.GetType();
            this.TypeFullName = componentType.FullName;
            this.TypeShortName = componentType.Name;
            this.guid = ObjectMap.AddOrGetObject(component);
        }

        public ComponentDescription(BinaryReader br)
        {
            TypeFullName = br.ReadString();
            TypeShortName = br.ReadString();
            guid = new Guid(br.ReadString());
            int scopeCount = br.ReadInt32();
            for (int i = 0; i < scopeCount; i++)
            {
                Scopes.Add(new ComponentScope(br));
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(TypeFullName);
            bw.Write(TypeShortName);
            bw.Write(guid.ToString());
            bw.Write(Scopes.Count);
            for (int i = 0; i < Scopes.Count; i++)
            {
                Scopes[i].Write(bw);
            }
        }

        public string TypeFullName { get; }

        public string TypeShortName { get; }

        public Guid Guid => guid;

        public List<ComponentScope> Scopes { get; set; } = new List<ComponentScope>();
    }
}