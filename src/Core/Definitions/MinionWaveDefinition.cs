using UnityEngine;

namespace Aetherium.Core.Definitions
{
    [CreateAssetMenu(fileName = "MinionWaveDefinition", menuName = "Aetherium/MOBA/Minion Wave Definition")]
    public class MinionWaveDefinition : ScriptableObject
    {
        [Header("Wave Timing")]
        public float firstWaveDelay = 20f;
        public float waveInterval = 25f;

        [Header("Per-Wave Spawning")]
        public int minionsPerTeamPerWave = 3;
        public float spawnIntervalPerMinion = 0.5f;

        [Header("Population Cap")]
        public int maxAlivePerTeam = 12;
    }
}
