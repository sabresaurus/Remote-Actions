using System;

namespace Sabresaurus.RemoteActions
{
    [Serializable]
    public class RemoteActionsSettings
    {
        public bool AutoRefreshRemote = false;
#if REMOTEACTIONS_DEBUG
        public bool LocalDevMode = false;
#endif
    }
}