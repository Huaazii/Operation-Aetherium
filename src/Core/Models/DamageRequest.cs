using UnityEngine;
using Aetherium.Core.Definitions;

namespace Aetherium.Core.Models
{
    public enum HitZone
    {
        Body = 0,
        Head = 1,
        Limbs = 2
    }

    public struct DamageRequest
    {
        public GameObject Instigator;
        public long AttackerToken;
        public TeamIdentity AttackerTeam;
        public ItemId ItemId;
        public DamageType DamageType;
        public HitZone HitZone;
        public float BaseDamage;
        public float HeadshotMultiplier;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public int ShotSequence;
    }

    public struct DamageResult
    {
        public bool Applied;
        public float ActualDamage;
        public bool WasFatal;
        public bool IsHeadshot;
    }
}
