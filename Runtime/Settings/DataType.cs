namespace Sabresaurus.RemoteActions
{
    /// <summary>
    /// Enum matching types that Sidekick can understand
    /// </summary>
    public enum DataType : byte
    {
        // Array
        String,
        Char,
        Boolean,
        Integer, // Signed 32 bit
        Long, // Signed 64 bit
        Float, // 32 bit Single
        Double, // 64 bit Double
        Vector2,
        Vector3,
        Vector4,
        Vector2Int,
        Vector3Int,
        Bounds,
        BoundsInt,
        Quaternion,
        Rect,
        RectInt,
        Color,
        Color32,
        Enum,
        AnimationCurve,
        Gradient,

        Unknown = 255
    }
}