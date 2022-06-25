using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Sabresaurus.RemoteActions
{
    public static class HierarchyHelper
    {
        static Scene dontDestroyOnLoadScene;

        public static Scene DontDestroyOnLoadScene
        {
            get
            {
                if (!dontDestroyOnLoadScene.IsValid())
                {
                    GameObject tempObject = new GameObject("[TEMP]DontDestroyOnLoadProxy");
                    Object.DontDestroyOnLoad(tempObject);
                    // Cache the scene ref
                    dontDestroyOnLoadScene = tempObject.scene;

                    Object.Destroy(tempObject);
                }
                return dontDestroyOnLoadScene;
            }
        }

        public static List<Scene> GetAllScenes()
        {
            List<Scene> scenes = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if(scene.IsValid())
                {
                    scenes.Add(scene);
                }
            }

            if(Application.isPlaying)
            {
                if(DontDestroyOnLoadScene.IsValid())
                {
                    scenes.Add(DontDestroyOnLoadScene);
                }
            }

            return scenes;
        }
    }
}
