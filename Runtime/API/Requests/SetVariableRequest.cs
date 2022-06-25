using System;
using System.IO;
using System.Reflection;

namespace Sabresaurus.RemoteActions.Requests
{
    [HideInDefaultList]
    public class SetVariableRequest : BaseRequest
    {
        Guid guid;
        WrappedVariable wrappedVariable;

        public SetVariableRequest(Guid guid, WrappedVariable wrappedVariable)
        {
            this.guid = guid;
            this.wrappedVariable = wrappedVariable;
        }

        public SetVariableRequest(BinaryReader br)
        {
            this.guid = new Guid(br.ReadString());
            this.wrappedVariable = new WrappedVariable(br);
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);

            bw.Write(guid.ToString());
            wrappedVariable.Write(bw);
        }

        public override BaseResponse GenerateResponse()
        {
            object targetObject = ObjectMap.GetObjectFromGUID(guid);

            if (targetObject != null)
            {
                FieldInfo fieldInfo = targetObject.GetType().GetFieldAll(wrappedVariable.VariableName);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(targetObject, wrappedVariable.ValueNative);
                }
                else
                {
                    PropertyInfo propertyInfo = targetObject.GetType().GetPropertyAll(wrappedVariable.VariableName);
                    MethodInfo setMethod = propertyInfo.GetSetMethod();

                    setMethod.Invoke(targetObject, new object[] {wrappedVariable.ValueNative});
                }
            }
            else
            {
                throw new System.NullReferenceException();
            }

            return new SetVariableResponse();
        }
    }

    public class SetVariableResponse : BaseResponse
    {
        public SetVariableResponse()
        {
        }

        public SetVariableResponse(BinaryReader br, int requestID)
            : base(br, requestID)
        {
        }

        public override void Write(BinaryWriter bw)
        {
        }
    }
}