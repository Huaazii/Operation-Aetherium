using UnityEngine;

namespace Aetherium.Core.Definitions
{
    [CreateAssetMenu(fileName = "ProjectileDefinition", menuName = "Aetherium/Definitions/ProjectileDefinition")]
    public class ProjectileDefinition : ScriptableObject
    {
        public NetworkContentId contentId;
        public float initialSpeed = 80f;
        public float lifetime = 10f;
        public float gravityMultiplier = 1f;
        public ProjectileTrajectoryKind trajectory = ProjectileTrajectoryKind.Linear;
        public float fuseSeconds = 0f;
        public float detonationRadius = 5f;
        public float maxDamage = 100f;
        public float minDamage = 20f;

        [Header("Area Effect (e.g. Serum)")]
        [Tooltip("Area effect radius in meters (default: 5m).")]
        public float areaRadius = 5f;
        [Tooltip("Area effect lifetime in seconds (default: 6s).")]
        public float areaDuration = 6f;
        [Tooltip("Area pulse interval in seconds (default: 0.5s).")]
        public float areaTickInterval = 0.5f;
        [Tooltip("Healing applied per tick to friendly targets.")]
        public float areaHealPerTick = 18f;
        [Tooltip("Damage applied per tick to hostile targets.")]
        public float areaDamagePerTick = 18f;
        [Tooltip("Linger duration of HOT / DOT on targets affected by the area (default: 6s).")]
        public float areaBuffDuration = 6f;
        [Tooltip("Immunity cooldown between reapplying area buff (default: 8s).")]
        public float areaBuffCooldown = 8f;

        [Header("Presentation (direct references; no Resources.Load)")]
        [Tooltip("In-flight projectile visual prefab (e.g. RPG trail, Serum bottle).")]
        public GameObject visualPrefab;
        [Tooltip("Impact/explosion presentation prefab.")]
        public GameObject impactPrefab;
        [Tooltip("Impact/explosion one-shot audio clip. Played through the scene SpatialOneShotAudioPool on every peer receiving the impact RPC.")]
        public AudioClip impactAudioClip;
        [Range(0f, 1f)] public float impactAudioVolume = 1f;
        [Tooltip("3D audibility range for the impact one-shot.")]
        public float impactAudioMinDistance = 10f;
        public float impactAudioMaxDistance = 120f;
    }
}
