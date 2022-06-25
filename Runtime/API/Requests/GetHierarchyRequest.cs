using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sabresaurus.RemoteActions.Requests
{
    /// <summary>
    /// Gets a complete hierarchy of all loaded scenes, only including named paths
    /// </summary>
    [HideInDefaultList]
    public class GetHierarchyRequest : BaseRequest
    {
        public GetHierarchyRequest()
        {
        }

        public GetHierarchyRequest(BinaryReader br)
        {
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
        }

        public override BaseResponse GenerateResponse()
        {
            GetHierarchyResponse response = new GetHierarchyResponse();

            List<Scene> scenes = HierarchyHelper.GetAllScenes();
            foreach (Scene scene in scenes)
            {
                SceneHierarchyDescription sceneHierarchyDescription = new SceneHierarchyDescription();

                sceneHierarchyDescription.SceneName = scene.name;
                GameObject[] rootObjects = scene.GetRootGameObjects();

                foreach (GameObject rootObject in rootObjects)
                {
                    RecurseHierarchy(sceneHierarchyDescription.HierarchyNodes, rootObject.transform, 0);
                }

                response.Scenes.Add(sceneHierarchyDescription);
            }

            return response;
        }

        private static void RecurseHierarchy(List<HierarchyNode> nodes, Transform transform, int depth)
        {
            Guid transformGUID = ObjectMap.AddOrGetObject(transform);
            ObjectMap.AddOrGetObject(transform.gameObject);

            nodes.Add(new HierarchyNode()
            {
                ObjectName = transform.name,
                Depth = depth,
                ActiveInHierarchy = transform.gameObject.activeInHierarchy,
                TransformGUID = transformGUID
            });

            foreach (Transform childTransform in transform)
            {
                RecurseHierarchy(nodes, childTransform, depth + 1);
            }
        }
    }

    public class GetHierarchyResponse : BaseResponse
    {
        public GetHierarchyResponse()
        {
        }

        public GetHierarchyResponse(BinaryReader br, int requestID)
            : base(br, requestID)
        {
            int sceneCount = br.ReadInt32();
            for (int i = 0; i < sceneCount; i++)
            {
                Scenes.Add(new SceneHierarchyDescription(br));
            }
        }

        public List<SceneHierarchyDescription> Scenes { get; set; } = new List<SceneHierarchyDescription>();

        public override void Write(BinaryWriter bw)
        {
            bw.Write(Scenes.Count);
            for (int i = 0; i < Scenes.Count; i++)
            {
                Scenes[i].Write(bw);
            }
        }
    }
}