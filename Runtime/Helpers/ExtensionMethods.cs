using System;

namespace Sabresaurus.RemoteActions
{
    public static class ExtensionMethods
    {
        public static bool HasFlagByte(this Enum mask, Enum flags) // Same behavior than Enum.HasFlag in .NET 4
        {
            return ((byte)(IConvertible)mask & (byte)(IConvertible)flags) == (byte)(IConvertible)flags;
        }
    }
}