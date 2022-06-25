using Sabresaurus.RemoteActions.Requests;

namespace Sabresaurus.RemoteActions
{
    public abstract class CustomDisplay
    {
        protected static APIManager APIManager => BridgingContext.Instance.container.APIManager;
        public abstract void OnResponseReceived(BaseResponse response);
        public abstract void OnGUI();
    }
}