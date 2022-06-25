using System;
using System.Collections.Generic;
using System.IO;

namespace Sabresaurus.RemoteActions
{
     public class ComponentScope
    {
        public WrappedVariable GetProperty(string variableName)
        {
            foreach (var item in Properties)
            {
                if (item.VariableName == variableName)
                {
                    return item;
                }
            }

            return null;
        }

        public object GetPropertyValue(string variableName)
        {
            foreach (var item in Properties)
            {
                if(item.VariableName == variableName)
                {
                    return item.Value;
                }
            }

            return null;
        }

        public ComponentScope(Type componentType)
        {
            this.TypeFullName = componentType.FullName;
            this.TypeShortName = componentType.Name;
        }

        public ComponentScope(BinaryReader br)
        {
            TypeFullName = br.ReadString();
            TypeShortName = br.ReadString();
            // Fields
            int fieldCount = br.ReadInt32();
            for (int i = 0; i < fieldCount; i++)
            {
                Fields.Add(new WrappedVariable(br));
            }
            // Properties
            int propertyCount = br.ReadInt32();
            for (int i = 0; i < propertyCount; i++)
            {
                Properties.Add(new WrappedVariable(br));
            }
            // Methods
            int methodCount = br.ReadInt32();
            for (int i = 0; i < methodCount; i++)
            {
                Methods.Add(new WrappedMethod(br));
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(TypeFullName);
            bw.Write(TypeShortName);
            bw.Write(Fields.Count);
            // Fields
            for (int i = 0; i < Fields.Count; i++)
            {
                Fields[i].Write(bw);
            }
            // Properties
            bw.Write(Properties.Count);
            for (int i = 0; i < Properties.Count; i++)
            {
                Properties[i].Write(bw);
            }
            // Methods
            bw.Write(Methods.Count);
            for (int i = 0; i < Methods.Count; i++)
            {
                Methods[i].Write(bw);
            }
        }

        public string TypeFullName { get; }

        public string TypeShortName { get; }

        public List<WrappedVariable> Fields { get; set; } = new List<WrappedVariable>();

        public List<WrappedVariable> Properties { get; set; } = new List<WrappedVariable>();

        public List<WrappedMethod> Methods { get; set; } = new List<WrappedMethod>();
    }
}