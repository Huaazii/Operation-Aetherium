using UnityEngine;
using Aetherium.Core.Definitions;
using Aetherium.Core.Models;
using Fusion;

namespace Aetherium.Core.Interfaces
{
    public interface IFusionVisibilityTarget
    {
        Vector3 BodyCenterPosition { get; }
        Vector3 AuthorityEyePosition { get; }
    }

    public interface IFusionCombatTarget : IFusionVisibilityTarget
    {
        NetworkObject NetworkObject { get; }
        TeamIdentity Team { get; }
        bool IsAlive { get; }
        MobaEntityType EntityType { get; }
        Vector3 AuthorityHitPosition { get; }
        bool TryApplyDamage(DamageRequest request, out DamageResult result);
    }
}

