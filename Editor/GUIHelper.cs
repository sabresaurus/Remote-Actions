using System;
using UnityEditor;
using UnityEngine;

namespace Sabresaurus.RemoteActions
{
    public static class GUIHelper
    {
        public static object Draw(WrappedVariable variable)
        {
            string name = variable.VariableName;

            object newValue = null;

            if (variable.DataType != DataType.Unknown)
            {
                newValue = DrawIndividualVariable(variable, name, variable.Value);
            }
            else
            {
                EditorGUILayout.LabelField(name, "Unknown <" + variable.Value.ToString() + "> ");
            }

            return newValue;
        }

        public static object DrawIndividualVariable(WrappedVariable variable, string fieldName, object fieldValue, int index = 0)
        {
            object newValue;
            if (variable.DataType == DataType.Enum)
            {
                //Type underlyingType = variable.MetaData.GetTypeFromMetaData();
                int enumValueIndex = Array.IndexOf(variable.Metadata.EnumValues, fieldValue);
                enumValueIndex = EditorGUILayout.Popup(fieldName, enumValueIndex, variable.Metadata.EnumNames);

                newValue = variable.Metadata.EnumValues[enumValueIndex];
            }
            else if (variable.DataType == DataType.Integer)
            {
                newValue = EditorGUILayout.IntField(fieldName, (int) fieldValue);
            }
            else if (variable.DataType == DataType.Long)
            {
                newValue = EditorGUILayout.LongField(fieldName, (long) fieldValue);
            }
            else if (variable.DataType == DataType.String)
            {
                newValue = EditorGUILayout.TextField(fieldName, (string) fieldValue);
            }
            else if (variable.DataType == DataType.Char)
            {
                string newString = EditorGUILayout.TextField(fieldName, new string((char) fieldValue, 1));
                if (newString.Length == 1)
                {
                    newValue = newString[0];
                }
                else
                {
                    newValue = fieldValue;
                }
            }
            else if (variable.DataType == DataType.Float)
            {
                newValue = EditorGUILayout.FloatField(fieldName, (float) fieldValue);
            }
            else if (variable.DataType == DataType.Double)
            {
                newValue = EditorGUILayout.DoubleField(fieldName, (double) fieldValue);
            }
            else if (variable.DataType == DataType.Boolean)
            {
                newValue = EditorGUILayout.Toggle(fieldName, (bool) fieldValue);
            }
            else if (variable.DataType == DataType.Vector2)
            {
                newValue = EditorGUILayout.Vector2Field(fieldName, (Vector2) fieldValue);
            }
            else if (variable.DataType == DataType.Vector3)
            {
                newValue = EditorGUILayout.Vector3Field(fieldName, (Vector3) fieldValue);
            }
            else if (variable.DataType == DataType.Vector4)
            {
                newValue = EditorGUILayout.Vector4Field(fieldName, (Vector4) fieldValue);
            }
#if UNITY_2017_2_OR_NEWER
            else if (variable.DataType == DataType.Vector2Int)
            {
                newValue = EditorGUILayout.Vector2IntField(fieldName, (Vector2Int) fieldValue);
            }
            else if (variable.DataType == DataType.Vector3Int)
            {
                newValue = EditorGUILayout.Vector3IntField(fieldName, (Vector3Int) fieldValue);
            }
#endif
            else if (variable.DataType == DataType.Quaternion)
            {
                //if(InspectorSidekick.Current.Settings.RotationsAsEuler)
                //{
                //  Quaternion quaternion = (Quaternion)fieldValue;
                //  Vector3 eulerAngles = quaternion.eulerAngles;
                //  eulerAngles = EditorGUILayout.Vector3Field(fieldName, eulerAngles);
                //  newValue = Quaternion.Euler(eulerAngles);
                //}
                //else
                {
                    Quaternion quaternion = (Quaternion) fieldValue;
                    Vector4 vector = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                    vector = EditorGUILayout.Vector4Field(fieldName, vector);
                    newValue = new Quaternion(vector.x, vector.y, vector.z, vector.z);
                }
            }
            else if (variable.DataType == DataType.Bounds)
            {
                newValue = EditorGUILayout.BoundsField(fieldName, (Bounds) fieldValue);
            }
#if UNITY_2017_2_OR_NEWER
            else if (variable.DataType == DataType.BoundsInt)
            {
                newValue = EditorGUILayout.BoundsIntField(fieldName, (BoundsInt) fieldValue);
            }
#endif
            else if (variable.DataType == DataType.Color)
            {
                newValue = EditorGUILayout.ColorField(fieldName, (Color) fieldValue);
            }
            else if (variable.DataType == DataType.Color32)
            {
                newValue = (Color32) EditorGUILayout.ColorField(fieldName, (Color32) fieldValue);
            }
            else if (variable.DataType == DataType.Gradient)
            {
                newValue = EditorGUILayout.GradientField(new GUIContent(fieldName), (Gradient) fieldValue);
            }
            else if (variable.DataType == DataType.AnimationCurve)
            {
                newValue = EditorGUILayout.CurveField(fieldName, (AnimationCurve) fieldValue);
            }
            else if (variable.DataType == DataType.Rect)
            {
                newValue = EditorGUILayout.RectField(fieldName, (Rect) fieldValue);
            }
#if UNITY_2017_2_OR_NEWER
            else if (variable.DataType == DataType.RectInt)
            {
                newValue = EditorGUILayout.RectIntField(fieldName, (RectInt) fieldValue);
            }
#endif
            else
            {
                GUILayout.Label(fieldName);
                newValue = fieldValue;
            }

            return newValue;
        }
        
        public static bool DetectClickInRect(Rect rect, int mouseButton = 0)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == mouseButton && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                return true;
            }

            return false;
        }
    }
}