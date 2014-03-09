using System;

namespace BananaMpq.Geometry
{
    [Flags]
    public enum MaterialFlags : ushort
    {
        None = 0,
        Liquid = 1,
        DamageOverTime = 2,
        Traversable = 32768,
    }
}