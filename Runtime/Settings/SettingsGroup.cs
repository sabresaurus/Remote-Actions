using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Sabresaurus.RemoteActions
{
    public class SettingsGroup
    {
        public string GroupKey { get; }

        public List<WrappedVariable> Variables { get; } = new List<WrappedVariable>();

        public SettingsGroup()
        {
        }

        public SettingsGroup(string groupKey, List<WrappedVariable> variables)
        {
            GroupKey = groupKey;
            Variables = variables;
        }

        public SettingsGroup(BinaryReader br)
        {
            GroupKey = br.ReadString();

            int fieldCount = br.ReadInt32();
            for (int i = 0; i < fieldCount; i++)
            {
                Variables.Add(new WrappedVariable(br));
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(GroupKey);

            bw.Write(Variables.Count);
            for (int i = 0; i < Variables.Count; i++)
            {
                Variables[i].Write(bw);
            }
        }
    }
}