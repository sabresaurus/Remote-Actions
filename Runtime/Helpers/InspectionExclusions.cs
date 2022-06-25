using System;
using System.Collections.Generic;

namespace Sabresaurus.RemoteActions
{
    public static class InspectionExclusions
    {
        public static List<Type> GetExcludedTypes()
        {
            return new List<Type>()
            {
                // Built in exclusions
                typeof(System.Object),
                typeof(UnityEngine.Object),
                typeof(UnityEngine.Component),
                // Add any custom exclusions here
            };
        }
    }
}