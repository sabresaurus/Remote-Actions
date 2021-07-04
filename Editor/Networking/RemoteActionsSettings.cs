using System;

namespace Sabresaurus.RemoteActions
{
    [Serializable]
    public class RemoteActionsSettings
    {
#if SIDEKICK_DEBUG
        public bool LocalDevMode = false;
#endif
        public bool AutoRefreshRemote = false;

        public InspectionConnection InspectionConnection = InspectionConnection.LocalEditor;
    }
}