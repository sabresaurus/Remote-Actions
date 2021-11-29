using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Sabresaurus.RemoteActions
{
    /// <summary>
    /// Wraps a field or property so that it can be sent over the network
    /// </summary>
    public class WrappedVariable
    {
        object value;

        private readonly string variableName;
        private readonly DataType dataType;

        // Meta data
        private readonly VariableMetadata metadata;

        #region Properties

        public string VariableName => variableName;

        public DataType DataType => dataType;

        public VariableMetadata Metadata => metadata;

        public object Value
        {
            get => value;
            set => this.value = value;
        }

        #endregion

        public WrappedVariable(FieldInfo fieldInfo, object objectValue)
            : this(fieldInfo.Name, objectValue, fieldInfo.FieldType, true)
        {
        }

        public WrappedVariable(PropertyInfo propertyInfo, object objectValue)
            : this(propertyInfo.Name, objectValue, propertyInfo.PropertyType, true)
        {
        }

        public WrappedVariable(string variableName, object value, Type type, bool generateMetadata)
        {
            this.variableName = variableName;
            this.dataType = DataTypeHelper.GetWrappedDataTypeFromSystemType(type);

            Type elementType = type;

            if (generateMetadata)
            {
                metadata = VariableMetadata.Create(dataType, elementType);
            }

            this.value = value;

            // Root data type or element type of collection is unknown
            if (this.dataType == DataType.Unknown)
            {
                // Let's just use the type of the value to help us debug
                this.value = type.Name;
            }
            else if (this.dataType == DataType.Enum)
            {
                // Convert enums to underlying type
                if (type.IsEnum)
                {
                    Type underlyingType = Enum.GetUnderlyingType(type);
                    this.Value = Convert.ChangeType(this.Value, underlyingType);
                }
            }
        }

        public WrappedVariable(BinaryReader br)
        {
            this.variableName = br.ReadString();
            this.dataType = (DataType) br.ReadByte();

            bool hasMetaData = br.ReadBoolean();
            if (hasMetaData)
            {
                metadata = new VariableMetadata(br, dataType);
            }

            this.value = DataTypeHelper.ReadFromBinary(dataType, br, metadata);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(variableName);
            bw.Write((byte) dataType);

            bw.Write(metadata != null);
            metadata?.Write(bw, dataType);

            DataTypeHelper.WriteToBinary(dataType, value, bw);
        }
    }
}