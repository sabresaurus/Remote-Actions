using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Reflection;

namespace Sabresaurus.RemoteActions
{
    [Serializable]
    public class VariableMetadata
    {
        // Enum
        string enumUnderlyingType; // an enum is backed by an int by default, but can be changed
        string[] enumNames;
        object[] enumValues;

        public string EnumUnderlyingType => enumUnderlyingType;

        public string[] EnumNames => enumNames;

        public object[] EnumValues => enumValues;

        public VariableMetadata()
        {
        }

        public VariableMetadata(BinaryReader br, DataType dataType)
        {
            if (dataType == DataType.Enum)
            {
                enumUnderlyingType = br.ReadString();
                int enumNameCount = br.ReadInt32();
                enumNames = new string[enumNameCount];
                enumValues = new object[enumNameCount];
                for (int i = 0; i < enumNameCount; i++)
                {
                    enumNames[i] = br.ReadString();
                }

                Type enumBackingType = GetTypeFromMetaData();
                for (int i = 0; i < enumNameCount; i++)
                {
                    enumValues[i] = DataTypeHelper.ReadIntegerFromBinary(enumBackingType, br);
                }
            }
        }

        public void Write(BinaryWriter bw, DataType dataType)
        {
            if (dataType == DataType.Enum)
            {
                bw.Write(enumUnderlyingType);
                bw.Write(enumNames.Length);
                for (int i = 0; i < enumNames.Length; i++)
                {
                    bw.Write(enumNames[i]);
                }

                for (int i = 0; i < enumNames.Length; i++)
                {
                    DataTypeHelper.WriteIntegerToBinary(enumValues[i], bw);
                }
            }
        }

        public Type GetTypeFromMetaData()
        {
            if (!string.IsNullOrEmpty(EnumUnderlyingType))
            {
                return typeof(int).Assembly.GetType(EnumUnderlyingType);
            }

            return null;
        }

        public static VariableMetadata Create(DataType dataType, Type elementType)
        {
            if (dataType == DataType.Enum)
            {
                VariableMetadata metadata = new VariableMetadata();

                Type underlyingType = Enum.GetUnderlyingType(elementType);

                metadata.enumUnderlyingType = underlyingType.FullName;
                metadata.enumNames = Enum.GetNames(elementType);
                metadata.enumValues = new object[metadata.enumNames.Length];
                Array enumValuesArray = Enum.GetValues(elementType);

                for (int i = 0; i < metadata.enumNames.Length; i++)
                {
                    metadata.enumValues[i] = Convert.ChangeType(enumValuesArray.GetValue(i), underlyingType);
                }

                return metadata;
            }

            return null;
        }
    }
}