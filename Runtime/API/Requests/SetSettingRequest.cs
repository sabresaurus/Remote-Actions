using System.IO;
using System.Reflection;
using JetBrains.Annotations;

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
            var matchedObject = SettingsObjectTracker.GetObject(groupKey);

            if (matchedObject != null)
            {
                FieldInfo fieldInfo = matchedObject.GetType().GetField(wrappedVariable.VariableName);
                
                if(fieldInfo != null)
                {
                    fieldInfo.SetValue(matchedObject, wrappedVariable.Value);
                }
                
                PropertyInfo propertyInfo = matchedObject.GetType().GetProperty(wrappedVariable.VariableName);
                
                if(propertyInfo != null && propertyInfo.GetSetMethod() != null)
                {
                    propertyInfo.SetValue(matchedObject, wrappedVariable.Value);
                }
            }

            return new SetSettingResponse();
        }
    }
    public class SetSettingResponse : BaseResponse
    {
        public SetSettingResponse()
        {
        }

        [UsedImplicitly]
        public SetSettingResponse(BinaryReader br, int requestID)
            : base(br, requestID)
        {
        }

        public override void Write(BinaryWriter bw)
        {
        }
    }
}