using UnityEngine;

namespace Aetherium.Core.Definitions
{
    [CreateAssetMenu(fileName = "MinionDefinition", menuName = "Aetherium/MOBA/Minion Definition")]
    public class MinionDefinition : ScriptableObject
    {
        [Header("Health")]
        public float maxHealth = 150f;
        public float armor = 0f;

        [Header("Movement")]
        public float moveSpeed = 2f;
        public float rotationSpeed = 360f;

        [Header("Detection")]
        public float detectionRadius = 10f;
        public float leashRadius = 11f;
        public float targetRefreshInterval = 0.5f;

        [Header("Attack")]
        public float attackDamage = 10f;
        public float attackRange = 8f;
        public float attackInterval = 1.4286f;
        public float attackWindup = 0.2f;

        [Header("Structure Damage")]
        public float structureDamageMultiplier = 1f;

        [Header("Death")]
        public float deathDisplayTime = 2f;
    }
}
