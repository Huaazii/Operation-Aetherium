using UnityEngine;

namespace Aetherium.Core.Definitions
{
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Aetherium/Definitions/WeaponDefinition")]
    public class WeaponDefinition : ItemDefinition
    {
        [Header("Simulation")]
        public WeaponSimulationKind simulationKind;
        [Tooltip("True for full-auto weapons (AK, MX16A4); false for semi-auto/single-fire weapons (X18, L96X, Shotgun).")]
        public bool isAutomatic = true;
        public float damage = 20f;
        [Tooltip("Multiplier applied to base damage when hitting the head hitbox. 1.0 = no bonus, 1.5 = +50%, 2.4 = 2.4x")]
        public float headshotMultiplier = 1.5f;
        public float roundsPerMinute = 600f;
        public float range = 100f;
        public int pelletsPerShot = 1;
        [Header("Accuracy & Handling")]
        [Tooltip("Hip-fire cone half-angle in degrees. Legacy spreadAngle is retained as a migration fallback.")]
        public float hipSpreadAngle = 0f;
        [Tooltip("Fully aimed cone half-angle in degrees.")]
        public float adsSpreadAngle = 0f;
        [Tooltip("Additional cone angle while moving at the weapon's reference speed.")]
        public float movingSpreadAdd = 0f;
        [Tooltip("Additional cone angle while sprinting.")]
        public float sprintSpreadAdd = 0f;
        [Tooltip("Additional cone angle while airborne.")]
        public float airSpreadAdd = 0f;
        [Tooltip("Multiplier applied to the complete cone while crouching.")]
        public float crouchSpreadMultiplier = 0.85f;
        [Tooltip("Cone growth per accepted shot before recovery.")]
        public float bloomPerShot = 0f;
        [Tooltip("Maximum accumulated bloom angle.")]
        public float maxBloomAngle = 0f;
        [Tooltip("Bloom recovery speed in degrees per second.")]
        public float bloomRecoveryPerSecond = 0f;
        [Tooltip("Delay after the last accepted shot before accumulated bloom begins recovering.")]
        public float bloomRecoveryDelaySeconds = 0.15f;
        [Tooltip("Time in seconds to reach full ADS accuracy from hip accuracy.")]
        public float adsAccuracyEnterSeconds = 0.18f;
        [Tooltip("Time in seconds to return from ADS accuracy to hip accuracy.")]
        public float adsAccuracyExitSeconds = 0.12f;
        public float spreadAngle = 0f;

        [Header("First-Shots Accuracy")]
        [Tooltip("First N shots of a burst keep the spread cone multiplied by firstShotsSpreadMultiplier. 0 disables the mechanic.")]
        public int firstShotsAccurateCount = 0;
        [Tooltip("Spread multiplier applied to the first firstShotsAccurateCount shots of a burst (0..1; 1 = no change).")]
        [Range(0f, 1f)] public float firstShotsSpreadMultiplier = 0.5f;
        [Tooltip("Seconds without firing after which the first-shots counter resets to a new burst.")]
        public float burstResetSeconds = 0.35f;

        [Header("Aim Recoil (degrees per shot)")]
        [Tooltip("Upward aim kick range. Separate from cosmetic KINEMATION gun/camera motion. Zero disables aim recoil.")]
        public Vector2 aimRecoilPitch;
        [Tooltip("Signed horizontal aim kick range in degrees.")]
        public Vector2 aimRecoilYaw;
        [Range(0f, 1f)] public float aimRecoilAdsMultiplier = 1f;

        [Header("Ammo & Magazine")]
        public int maxClipAmmo = 30;
        public int maxReserveAmmo = 120;
        public float reloadDuration = 2.0f;
        [Tooltip("Authoritative empty reload duration in seconds. If 0 or negative, falls back to reloadDuration.")]
        public float emptyReloadDuration = 0f;
        public bool allowReloadCancel = true;

        [Header("Shell Reload (shotguns)")]
        [Tooltip("When true, the authoritative sim reloads one shell per cycle instead of a full magazine at once. Default false preserves batch behavior.")]
        public bool shellByShellReload = false;
        [Tooltip("Authoritative seconds for the shell-reload start segment (insert the first shell). 0 or negative skips the start wait.")]
        public float shellReloadStartSeconds = 0f;
        [Tooltip("Authoritative seconds per shell inserted during the reload loop (full loop iteration length).")]
        public float shellReloadIntervalSeconds = 0.633f;
        [Tooltip("Authoritative seconds into each loop iteration when the shell seats (KXG12 Loop frame 20 ≈ 0.633). Applied + HUD tick fire here; 0 applies at loop start.")]
        public float shellReloadApplySeconds = 0f;
        [Tooltip("Authoritative seconds for the shell-reload end segment. 0 or negative finishes immediately after the last shell.")]
        public float shellReloadEndSeconds = 0f;

        /// <summary>
        /// True for magazine-fed weapons that can reload (KXG12 included).
        /// False only for single-use / consumable items (RPG, Serum, Grenade, Melee).
        /// </summary>
        public bool SupportsReload()
        {
            return itemId != ItemId.RPG
                && itemId != ItemId.Serum
                && itemId != ItemId.Grenade
                && itemId != ItemId.Melee;
        }

        /// <summary>
        /// Whether an emptied weapon should auto-reload: reloadable, clip spent, reserve left.
        /// </summary>
        public static bool ShouldAutoReload(bool supportsReload, int clipAfterShot, int reserveAmmo)
        {
            return supportsReload && clipAfterShot <= 0 && reserveAmmo > 0;
        }

         public float GetReloadDuration(bool isEmpty)
         {
             return (isEmpty && emptyReloadDuration > 0f) ? emptyReloadDuration : reloadDuration;
         }

        [Header("Prefabs & Visuals")]
        public GameObject fpvPrefab;
        public GameObject tpvPrefab;
        public GameObject projectileOrThrowablePrefab;
        public GameObject pickableWorldPrefab;
        public ProjectileDefinition projectileDefinition;

        [Header("Presentation")]
        [Tooltip("Native Equip clip duration baked for slot switches and class changes. Fire unlocks at Equip Fire Ready Normalized Time of this duration.")]
        [Min(0f), InspectorName("Equip Animation Seconds")] public float classChangeEquipLockSeconds = 0.5f;
        [Tooltip("Fraction of the native Equip clip after which firearms may fire. Does not change weapons configured to reload on equip.")]
        [Range(0f, 1f)] public float equipFireReadyNormalizedTime = 0.8f;
        [Tooltip("True for weapons that play their native reload on equip and require an equip lock until the animation event/duration completes.")]
        public bool playReloadOnEquip;

        public bool PlayReloadOnEquip => playReloadOnEquip;

        public float EquipFireLockSeconds => PlayReloadOnEquip
            ? Mathf.Max(0f, GetReloadDuration(true))
            : simulationKind == WeaponSimulationKind.Hitscan || simulationKind == WeaponSimulationKind.Ballistic
                ? Mathf.Max(0f, classChangeEquipLockSeconds) * Mathf.Clamp01(equipFireReadyNormalizedTime)
                : 0f;

        [Tooltip("Seconds the FPV model keeps presenting after a one-shot weapon is consumed on its final shot, so its fire clip plays out before the fallback item swaps in. 0 for regular weapons.")]
        public float fpvPostShotHoldSeconds;

        [Header("Throwable")]
        [Tooltip("Authoritative delay (seconds) between the Fire release and the projectile spawn for handheld throwables (Serum). Locked direction is captured at release; the timer guarantees a single handoff even when animation events are missing.")]
        public float throwReleaseDelaySeconds = 0.35f;

        [Header("Authoritative Muzzle Pose")]
        [Tooltip("Server-side muzzle local position relative to the weapon display root, extracted from the weapon's FpvPresentationBindings.muzzle. Deterministic simulation uses this instead of any local FPV transform.")]
        public Vector3 muzzleLocalPosition;
        [Tooltip("Server-side muzzle local rotation relative to the weapon display root.")]
        public Quaternion muzzleLocalRotation = Quaternion.identity;
        [Tooltip("True once the weapon has a validated muzzle pose baked from its prefab bindings.")]
        public bool hasMuzzlePose;

        [Tooltip("Whether the weapon shows the ammo HUD (name/clip/reserve). False for Grenade/Serum/Melee/empty slots.")]
        public bool showAmmoHud = true;

        [Tooltip("Crosshair prefab instantiated for this weapon. Null hides the crosshair.")]
        public GameObject crosshairPrefab;

        public float SecondsPerShot => roundsPerMinute > 0f ? 60f / roundsPerMinute : 0.1f;
    }
}
