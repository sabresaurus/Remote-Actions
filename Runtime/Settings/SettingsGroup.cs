using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Sabresaurus.RemoteActions
{
    public class SettingsGroup
    {
        private string groupKey;
        List<WrappedVariable> fields = new List<WrappedVariable>();

        public string GroupKey => groupKey;

        public List<WrappedVariable> Fields => fields;

        public SettingsGroup()
        {
        }

        public SettingsGroup(string groupKey, FieldInfo[] fields, object trackedObject)
        {
            this.groupKey = groupKey;
            this.fields = new List<WrappedVariable>(this.fields.Count);
            foreach (var fieldInfo in fields)
            {
                this.fields.Add(new WrappedVariable(fieldInfo, fieldInfo.GetValue(trackedObject)));
            }
        }

        public SettingsGroup(BinaryReader br)
        {
            groupKey = br.ReadString();

            int fieldCount = br.ReadInt32();
            for (int i = 0; i < fieldCount; i++)
            {
                fields.Add(new WrappedVariable(br));
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(groupKey);

            bw.Write(fields.Count);
            for (int i = 0; i < fields.Count; i++)
            {
                fields[i].Write(bw);
            }
        }
    }
}