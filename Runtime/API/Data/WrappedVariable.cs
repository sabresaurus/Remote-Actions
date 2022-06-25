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
    public class WrappedVariable : WrappedBaseObject
    {
        object value;

        // Meta data

        #region Properties

        public object Value
        {
            get => value;
            set => this.value = value;
        }

        public object ValueNative
        {
            get
            {
                // if (dataType == DataType.UnityObjectReference)
                // {
                //     // TODO support array of Unity Objects
                //
                //     if (value is UnityEngine.Object || value == null)
                //     {
                //         UnityEngine.Object unityObject = value as UnityEngine.Object;
                //         return unityObject;
                //     }
                //     else
                //     {
                //         return ObjectMap.GetObjectFromGUID((Guid)value);
                //     }
                // }

                // if (attributes.HasFlagByte(VariableAttributes.IsArray)
                //     || attributes.HasFlagByte(VariableAttributes.IsList))
                // {
                //     return CollectionUtility.ConvertArrayOrListToNative(this, DataTypeHelper.GetSystemTypeFromWrappedDataType(dataType, metaData, attributes));
                // }
                // else
                {
                    return Value;
                }
            }
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
            : base(variableName, type, generateMetadata)
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
            : base(br)
        {
            // if (this.attributes.HasFlagByte(VariableAttributes.IsArray))
            // {
            //     int count = br.ReadInt32();
            //     object[] array = new object[count];
            //     for (int i = 0; i < count; i++)
            //     {
            //         array[i] = DataTypeHelper.ReadFromBinary(dataType, br, metaData);
            //     }
            //     this.value = array;
            // }
            // else if (this.attributes.HasFlagByte(VariableAttributes.IsList))
            // {
            //     int count = br.ReadInt32();
            //
            //     List<object> list = new List<object>(count);
            //     for (int i = 0; i < count; i++)
            //     {
            //         list.Add(DataTypeHelper.ReadFromBinary(dataType, br, metaData));
            //     }
            //     this.value = list;
            // }
            // else
            {
                this.value = DataTypeHelper.ReadFromBinary(dataType, br, metadata);
            }

            // if (dataType == DataType.UnityObjectReference)
            // {
            //     valueDisplayNames = new string[br.ReadInt32()];
            //     for (int i = 0; i < valueDisplayNames.Length; i++)
            //     {
            //         valueDisplayNames[i] = br.ReadString();
            //     }
            // }
        }

        public void Write(BinaryWriter bw)
        {
            base.Write(bw);
            // if (this.attributes.HasFlagByte(VariableAttributes.IsArray)
            //     || this.attributes.HasFlagByte(VariableAttributes.IsList))
            // {
            //     if (value is IList)
            //     {
            //         IList list = (IList)value;
            //         int count = list.Count;
            //         bw.Write(count);
            //         for (int i = 0; i < count; i++)
            //         {
            //             DataTypeHelper.WriteToBinary(dataType, list[i], bw);
            //         }
            //     }
            //     else
            //     {
            //         throw new NotImplementedException("Array serialisation has not been implemented for this array type: " + value.GetType());
            //     }
            // }
            // else
            {                
                DataTypeHelper.WriteToBinary(dataType, value, bw);
            }

            // if (dataType == DataType.UnityObjectReference)
            // {
            //     bw.Write(valueDisplayNames.Length);
            //     for (int i = 0; i < valueDisplayNames.Length; i++)
            //     {
            //         bw.Write(valueDisplayNames[i]);
            //     }
            // }
        }
    }
}