using System;
using System.Collections.Generic;
using System.IO;

namespace Sabresaurus.RemoteActions
{
    [Serializable]
    public class HierarchyNode
    {
        public string ObjectName { get; set; }

        public int Depth { get; set; }

        public bool ActiveInHierarchy { get; set; }

        public Guid TransformGUID { get; set; }
    }

    public class SceneHierarchyDescription : APIData
    {
        public string SceneName { get; set; }

        public List<HierarchyNode> HierarchyNodes { get; set; } = new List<HierarchyNode>();

        public SceneHierarchyDescription()
        {
        }

        public SceneHierarchyDescription(BinaryReader br)
            : base(br)
        {
            SceneName = br.ReadString();

            int nodeCount = br.ReadInt32();
            for (int i = 0; i < nodeCount; i++)
            {
                HierarchyNodes.Add(new HierarchyNode()
                {
                    ObjectName = br.ReadString(),
                    Depth = br.ReadInt32(),
                    ActiveInHierarchy = br.ReadBoolean(),
                    TransformGUID = new Guid(br.ReadString())
                });
            }
        }

        public override void Write(BinaryWriter bw)
        {
            bw.Write(SceneName);

            bw.Write(HierarchyNodes.Count);
            for (int i = 0; i < HierarchyNodes.Count; i++)
            {
                bw.Write(HierarchyNodes[i].ObjectName);
                bw.Write(HierarchyNodes[i].Depth);
                bw.Write(HierarchyNodes[i].ActiveInHierarchy);
                bw.Write(HierarchyNodes[i].TransformGUID.ToString());
            }
        }
    }
}