using System.Collections.Generic;
using Aetherium.Core.Definitions;
using UnityEngine;

namespace Aetherium.Core.Models
{
    /// <summary>Gameplay aim kick, independent of cosmetic animation and frame rate.</summary>
    public static class WeaponAimRecoil
    {
        public static Vector2 Sample(WeaponDefinition weapon, int shotSequence, bool aiming)
        {
            if (weapon == null || weapon.simulationKind != WeaponSimulationKind.Hitscan) return Vector2.zero;
            uint seed = unchecked((uint)(shotSequence * 7919 ^ (int)weapon.itemId * 104729));
            float scale = aiming ? Mathf.Clamp01(weapon.aimRecoilAdsMultiplier) : 1f;
            float pitch = Mathf.Lerp(Mathf.Min(weapon.aimRecoilPitch.x, weapon.aimRecoilPitch.y),
                Mathf.Max(weapon.aimRecoilPitch.x, weapon.aimRecoilPitch.y), Next(ref seed));
            float yaw = Mathf.Lerp(Mathf.Min(weapon.aimRecoilYaw.x, weapon.aimRecoilYaw.y),
                Mathf.Max(weapon.aimRecoilYaw.x, weapon.aimRecoilYaw.y), Next(ref seed));
            return new Vector2(yaw, -Mathf.Max(0f, pitch)) * scale;
        }

        public static Vector2 ClampKick(Vector2 kick, float viewPitch, float minPitch, float maxPitch)
        {
            kick.y = Mathf.Clamp(viewPitch + kick.y, minPitch, maxPitch) - viewPitch;
            return kick;
        }

        private static float Next(ref uint seed)
        {
            if (seed == 0) seed = 0x9E3779B9u;
            seed ^= seed << 13;
            seed ^= seed >> 17;
            seed ^= seed << 5;
            return (seed & 0x00FFFFFFu) / 16777216f;
        }
    }

    /// <summary>Bounded local kicks awaiting authority. Snapshots replace, never add to, these kicks.</summary>
    public sealed class PredictedAimRecoil
    {
        private readonly List<Entry> pending = new List<Entry>(64);
        private int predictedSequence;
        private int generation;
        public int PendingCount => pending.Count;

        private readonly struct Entry
        {
            public readonly int Sequence;
            public readonly Vector2 Kick;
            public Entry(int sequence, Vector2 kick) { Sequence = sequence; Kick = kick; }
        }

        public void Reset(int authoritativeSequence, int authoritativeGeneration)
        {
            pending.Clear();
            predictedSequence = authoritativeSequence;
            generation = authoritativeGeneration;
        }

        public void Reconcile(int authoritativeSequence, int authoritativeGeneration, int generationStartSequence = -1)
        {
            if (generation != authoritativeGeneration)
                Reset(generationStartSequence >= 0 ? generationStartSequence : authoritativeSequence, authoritativeGeneration);
            for (int i = pending.Count - 1; i >= 0; i--)
                if (pending[i].Sequence <= authoritativeSequence) pending.RemoveAt(i);
        }

        public void DiscardPending(int authoritativeSequence)
        {
            pending.Clear();
            // Preserve a render producer that is behind the host simulation:
            // its next trigger may already be included in this snapshot.
            predictedSequence = Mathf.Min(predictedSequence, authoritativeSequence);
        }

        public Vector2 Resolve(Vector2 authoritativeOffset)
        {
            foreach (Entry entry in pending) authoritativeOffset += entry.Kick;
            return authoritativeOffset;
        }

        public int NextSequence(int authoritativeSequence) => Mathf.Max(predictedSequence + 1, authoritativeSequence);

        public bool Add(int sequence, int authoritativeSequence, Vector2 kick)
        {
            if (pending.Count >= 64) return false;
            predictedSequence = sequence;
            if (sequence > authoritativeSequence) pending.Add(new Entry(sequence, kick));
            return true;
        }
    }
}
