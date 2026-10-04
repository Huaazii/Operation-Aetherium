namespace Aetherium.Core.Definitions
{
    /// <summary>
    /// Authoritative projectile trajectory. Linear keeps the legacy
    /// straight-line motion; Ballistic applies continuous gravity
    /// integration (velocity += gravity * multiplier * dt) with swept
    /// collision. Only definitions explicitly marked Ballistic (Serum) use
    /// gravity — legacy assets without the field default to Linear.
    /// </summary>
    public enum ProjectileTrajectoryKind
    {
        Linear = 0,
        Ballistic = 1
    }
}
