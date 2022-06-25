using System;
using System.Collections.Generic;

namespace Sabresaurus.RemoteActions
{
    public static class ObjectMap
    {
        static Dictionary<Guid, object> guidToObject = new Dictionary<Guid, object>();
        static Dictionary<object, Guid> objectToGuid = new Dictionary<object, Guid>();

        public static Guid AddOrGetObject(object targetObject)
        {
            if (!objectToGuid.ContainsKey(targetObject))
            {
                // Not contained, make it with a new Guid
                Guid guid = Guid.NewGuid();

                guidToObject[guid] = targetObject;
                objectToGuid[targetObject] = guid;
                return guid;
            }

            return objectToGuid[targetObject];
        }

        public static void AddObjects(List<object> objects)
        {
            foreach (object targetObject in objects)
            {
                AddOrGetObject(targetObject);
            }
        }

        public static object GetObjectFromGUID(Guid guid)
        {
            if (guidToObject.ContainsKey(guid))
            {
                return guidToObject[guid];
            }
            else
            {
                return null;
            }
        }
    }
}