using System;

namespace BananaMpq.Geometry
{
    [Flags]
    public enum MaterialFlags
    {
        None = 0, // terrain, walls, everything unspectacular
        Liquid = 1,
        DamageOverTime = 2,
    }
}