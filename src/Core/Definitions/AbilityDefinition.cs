using UnityEngine;
using Aetherium.Core.Models;

namespace Aetherium.Core.Definitions
{
    [CreateAssetMenu(fileName = "AbilityDefinition", menuName = "Aetherium/Definitions/AbilityDefinition")]
    public class AbilityDefinition : ScriptableObject
    {
        public AbilityId abilityId;
        public PlayerClassId classId;
        public float cooldownSeconds = 20f;
        public float activeDurationSeconds = 8f;
        public float damageReductionFraction = 0.40f; // Heavy shield 40% reduction
        public WeaponDefinition grantedSpecialWeapon;
        public ProjectileDefinition grantedProjectile;

        [Header("Presentation")]
        [Tooltip("Skill icon shown on the player card.")]
        public Sprite icon;
    }
}
