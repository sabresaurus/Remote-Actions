using UnityEngine;

namespace Sabresaurus.RemoteActions
{
    /// <summary>
    /// Required for RemoteActions to connect to a remote. This class is included in development builds and
    /// auto-instantiates when the game starts.
    /// </summary>
    public class RuntimeRemoteActionsBridge : MonoBehaviour
    {
        bool wasConnected;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            // Can't wrap the whole method - see https://stackoverflow.com/questions/44655667/
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
            Debug.Log($"Initializing Remote Actions by auto-instantiating {nameof(RuntimeRemoteActionsBridge)}");
            GameObject newGameObject = new GameObject(nameof(RuntimeRemoteActionsBridge), typeof(RuntimeRemoteActionsBridge));
            DontDestroyOnLoad(newGameObject);
#endif
        }

        private void Start()
        {
            PlayerMessaging.Start();
            PlayerMessaging.RegisterForRequests(OnRequestReceived);
            // HTTPServer.Start();
        }

        void Update()
        {
            PlayerMessaging.Tick();
        }

        byte[] OnRequestReceived(byte[] request)
        {
            return RequestProcessor.Process(request);
        }
    }
}