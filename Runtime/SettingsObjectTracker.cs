using System;
using System.Collections.Generic;
using System.Linq;

namespace Sabresaurus.RemoteActions
{
    public static class SettingsObjectTracker
    {
        private static readonly Dictionary<string, object> trackedObjects = new Dictionary<string, object>();

        public static Dictionary<string, object> TrackedObjects => trackedObjects;

        public static void RegisterObject(object trackedObject)
        {
            if (trackedObject is UnityEngine.Object unityObject)
            {
                trackedObjects.Add(unityObject.name + " " + Guid.NewGuid(), trackedObject);
            }
            else
            {
                trackedObjects.Add(trackedObject.GetType().Name + " " + Guid.NewGuid(), trackedObject);
            }
        }

        public static void UnregisterObject(object trackedObject)
        {
            var key = trackedObjects.FirstOrDefault(item => item.Value == trackedObject).Key;
            if (!string.IsNullOrEmpty(key))
            {
                trackedObjects.Remove(key);
            }
        }

        public static object GetObject(string objectKey)
        {
            if (trackedObjects.TryGetValue(objectKey, out object value))
            {
                return value;
            }

            return null;
        }
    }
}