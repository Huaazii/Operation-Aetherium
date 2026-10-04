namespace Aetherium.Core.Models
{
    /// <summary>
    /// Team-independent player class identity. The selected team is stored
    /// separately so duplicate classes remain valid within a team.
    /// </summary>
    public enum PlayerClassId : byte
    {
        None = 0,
        Heavy = 1,
        Medic = 2,
        Rocket = 3
    }
}
