using System.IO;
using JetBrains.Annotations;
using Sabresaurus.RemoteActions.Responses;

namespace Sabresaurus.RemoteActions.Requests
{
    [HideInDefaultList]
    public class SetSettingRequest : BaseRequest
    {
        private readonly string groupKey;
        private readonly WrappedVariable wrappedVariable;

        [UsedImplicitly]
        public SetSettingRequest(BinaryReader br)
        {
            groupKey = br.ReadString();
            wrappedVariable = new WrappedVariable(br);
        }

        public SetSettingRequest(string groupKey, WrappedVariable wrappedVariable)
        {
            this.groupKey = groupKey;
            this.wrappedVariable = wrappedVariable;
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);

            bw.Write(groupKey);
            wrappedVariable.Write(bw);
        }

        public override BaseResponse GenerateResponse()
        {
            var matchedObject = ObjectTracker.GetObject(groupKey);

            if (matchedObject != null)
            {
                var fieldInfo = matchedObject.GetType().GetField(wrappedVariable.VariableName);

                fieldInfo.SetValue(matchedObject, wrappedVariable.Value);
            }

            return new SetSettingResponse();
        }
    }
}