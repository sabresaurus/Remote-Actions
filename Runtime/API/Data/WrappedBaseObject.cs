using System;
using System.IO;

namespace Sabresaurus.RemoteActions
{
    [Flags]
    public enum VariableAttributes : byte
    {
        None = 0,
        ReadOnly = 1 << 0,
        IsStatic = 1 << 1,
        IsLiteral = 1 << 2, // e.g. const

        // IsArray = 1 << 3,
        // IsList = 1 << 4,
        IsValueType = 1 << 5,
        Obsolete = 1 << 6,
    }

    /// <summary>
    /// Base class for WrappedVariable and WrappedParameter
    /// </summary>
    public abstract class WrappedBaseObject
    {
        protected string variableName;
        protected VariableAttributes attributes = VariableAttributes.None;
        protected DataType dataType;

        // Meta data
        protected VariableMetadata metadata;

        #region Properties

        public string VariableName => variableName;

        public DataType DataType => dataType;

        public VariableAttributes Attributes => attributes;

        public VariableMetadata Metadata => metadata;

        #endregion

        public WrappedBaseObject(string variableName, Type type, VariableMetadata metadata)
            : this(variableName, type, false)
        {
            this.metadata = metadata;
        }

        public WrappedBaseObject(string variableName, Type type, bool generateMetadata)
        {
            this.variableName = variableName;
            this.dataType = DataTypeHelper.GetWrappedDataTypeFromSystemType(type);

            // bool isArray = type.IsArray;
            // bool isGenericList = TypeUtility.IsGenericList(type);

            this.attributes = VariableAttributes.None;

            Type elementType = type;

            // if (isArray || isGenericList)
            // {
            //     if (isArray)
            //     {
            //         this.attributes |= VariableAttributes.IsArray;
            //     }
            //     else if (isGenericList)
            //     {
            //         this.attributes |= VariableAttributes.IsList;
            //     }
            //
            //     elementType = TypeUtility.GetElementType(type);
            //     //Debug.Log(elementType);
            //     this.dataType = DataTypeHelper.GetWrappedDataTypeFromSystemType(elementType);
            // }

            if (generateMetadata)
            {
                metadata = VariableMetadata.Create(dataType, elementType);
            }
        }

        protected WrappedBaseObject(string variableName, DataType dataType, VariableAttributes attributes, VariableMetadata metadata)
        {
            this.variableName = variableName;
            this.dataType = dataType;
            this.attributes = attributes;
            this.metadata = metadata;
        }

        public WrappedBaseObject(BinaryReader br)
        {
            this.variableName = br.ReadString();
            this.attributes = (VariableAttributes) br.ReadByte();
            this.dataType = (DataType) br.ReadByte();

            bool hasMetaData = br.ReadBoolean();
            if (hasMetaData)
            {
                metadata = new VariableMetadata(br, dataType);
            }
        }

        public virtual void Write(BinaryWriter bw)
        {
            bw.Write(variableName);
            bw.Write((byte) attributes);
            bw.Write((byte) dataType);

            bw.Write(metadata != null);
            if (metadata != null)
            {
                metadata.Write(bw, dataType);
            }
        }
    }
}