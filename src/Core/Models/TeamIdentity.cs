namespace Aetherium.Core.Models
{
    public enum TeamIdentity
    {
        None = -1,
        Alpha = 0,  // Blue Team
        Omega = 1,  // Red Team
        Neutral = 2 // Neutral / Spectator
    }

    public static class TeamIdentityExtensions
    {
        public static TeamIdentity GetOppositeTeam(this TeamIdentity team)
        {
            if (team == TeamIdentity.Alpha) return TeamIdentity.Omega;
            if (team == TeamIdentity.Omega) return TeamIdentity.Alpha;
            return TeamIdentity.None;
        }
    }
}
