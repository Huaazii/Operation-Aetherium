using System.Collections.Generic;
using Aetherium.Core.Models;
using UnityEngine;

namespace Battlefield.Runtime
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider))]
    public sealed class BattlefieldClassChangeZone : MonoBehaviour
    {
        [SerializeField] private TeamIdentity team;
        [SerializeField] private BoxCollider trigger;
        private static readonly HashSet<BattlefieldClassChangeZone> zones = new HashSet<BattlefieldClassChangeZone>();

        public TeamIdentity Team => team;

        public bool Contains(Vector3 worldPosition)
        {
            if (!isActiveAndEnabled || trigger == null || !trigger.enabled || !trigger.isTrigger) return false;
            Vector3 point = trigger.transform.InverseTransformPoint(worldPosition) - trigger.center;
            Vector3 halfSize = trigger.size * 0.5f;
            return Mathf.Abs(point.x) <= halfSize.x && Mathf.Abs(point.y) <= halfSize.y &&
                   Mathf.Abs(point.z) <= halfSize.z;
        }

        public static bool Contains(TeamIdentity team, Vector3 position)
        {
            foreach (BattlefieldClassChangeZone zone in zones)
                if (zone != null && zone.team == team && zone.Contains(position)) return true;
            return false;
        }

        private void OnEnable() => zones.Add(this);
        private void OnDisable() => zones.Remove(this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => zones.Clear();
    }
}
