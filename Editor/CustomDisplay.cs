using Sabresaurus.RemoteActions.Responses;

namespace Sabresaurus.RemoteActions
{
    public abstract class CustomDisplay
    {
        public APIManager APIManager => BridgingContext.Instance.container.APIManager;
        public abstract void OnResponseReceived(BaseResponse response);
        public abstract void OnGUI();
    }
}