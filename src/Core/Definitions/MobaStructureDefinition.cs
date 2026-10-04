using UnityEngine;

namespace Aetherium.Core.Definitions
{
    [CreateAssetMenu(fileName = "MobaStructureDefinition", menuName = "Aetherium/MOBA/Structure Definition")]
    public class MobaStructureDefinition : ScriptableObject
    {
        [Header("Health & Armor (生命与护甲)")]
        [Tooltip("建筑的最大生命值（例如：T1/T2/T3 防御塔默认 4000，Base 基地核心默认 6000）。")]
        public float maxHealth = 4000f;

        [Tooltip("基础护甲值。护甲减伤公式：实际伤害 = 原始伤害 * (100 / (100 + armor))。")]
        public float armor = 80f;

        [Header("Attack Combat (攻击性能)")]
        [Tooltip("防御塔单次炮击的基础伤害值。")]
        public float attackDamage = 80f;

        [Tooltip("防御塔的索敌与开火射程（米）。例如 T1 塔默认 12~13m，T2/T3 塔默认 15m。")]
        public float attackRange = 12f;

        [Tooltip("攻击攻速（次/秒）。例如 0.5 表示每 2 秒开火一次。")]
        public float attacksPerSecond = 0.5f;

        [Tooltip("开火微蓄力前摇时间（秒），决定从锁定目标到炮弹射出的间隔。")]
        public float attackWindup = 0.2f;

        [Tooltip("防御塔发射炮弹的飞行初速度（米/秒）。")]
        public float projectileSpeed = 40f;

        [Header("Targeting & Line of Sight (索敌与视线)")]
        [System.Obsolete("Deprecated. Line of Sight tolerance now governs target retention.")]
        [Tooltip("【已废弃】早期锁定维持时长，当前系统已由 lineOfSightTolerance 统一管理。")]
        public float targetLockMinimumDuration = 0.5f;

        [Tooltip("攻击是否需要无遮挡的视线。开启后，若目标处于掩体后方将无法攻击。")]
        public bool requireLineOfSight = true;

        [Tooltip("视线丢失容忍时间（秒）。当目标暂时脱离视线时，在此缓冲时间内仍维持锁定，超出则丢失目标。")]
        public float lineOfSightTolerance = 0.5f;

        [Tooltip("掩体检测的球体投射半径（米），防止射线过于纤细穿透缝隙导致误判。")]
        public float coverProbeRadius = 0.15f;

        [Tooltip("静态障碍物检测图层掩码（墙体、掩体、建筑等）。")]
        public LayerMask obstructionLayers = ~0;

        [Header("Behavior & Animation (行为与动画)")]
        [Tooltip("首次锁定目标后的开火启动延迟（秒），匹配防御塔展开（Open）动画时长（默认 0.85s）。")]
        public float attackStartupDelay = 0.85f;

        [Tooltip("【防偷塔机制】当防御塔攻击范围内没有敌方小兵时，是否强制激活护盾进入偷塔防御状态。")]
        public bool invulnerableWhenNoEligibleTarget = true;

        [Header("Backdoor Protection (防偷塔机制)")]
        [Tooltip("偷塔保护检测范围（米）。在防御塔/基地此半径内没有敌方小兵时触发护盾减伤。基地核心默认 18m。")]
        public float backdoorProtectionRadius = 15f;

        [Tooltip("无小兵偷塔时，玩家对防御塔的基础伤害折减倍率（例如 0.5 表示仅造成 50% 伤害）。")]
        public float playerStructureDamageMultiplier = 0.5f;

        [Header("Shield Multipliers (护盾减伤倍率)")]
        [Tooltip("护盾激活状态下，敌方玩家普通枪械对建筑的伤害倍率（默认 0.5 即减免 50%）。")]
        public float shieldedPlayerWeaponMultiplier = 0.5f;

        [Tooltip("护盾激活状态下，敌方玩家技能对建筑的伤害倍率（默认 0.5 即减免 50%）。")]
        public float shieldedPlayerAbilityMultiplier = 0.5f;

        [Tooltip("护盾激活状态下，爆炸物（火箭筒/手雷）对建筑的伤害倍率（默认 0.5 即减免 50%）。")]
        public float shieldedStructureExplosiveMultiplier = 0.5f;

        [Tooltip("非护盾（正常）状态下的标准伤害倍率（通常为 1.0）。")]
        public float normalDamageMultiplier = 1f;

        [Header("Friendly Healing Aura (友方回血光环)")]
        [Tooltip("是否向范围内的友方玩家提供缓慢回血（外层防御塔开启，基地核心关闭）。")]
        public bool enableFriendlyHeal = true;

        [Tooltip("以防御塔为中心的回血半径（米），默认 2 米。")]
        public float healRadius = 2.0f;

        [Tooltip("回血脉冲触发间隔（秒），默认 1 秒。")]
        public float healInterval = 1.0f;

        [Tooltip("单次脉冲回复的生命值量，默认 10 点。")]
        public float healAmount = 10.0f;
    }
}