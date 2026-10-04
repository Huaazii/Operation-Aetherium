using Fusion;
using UnityEngine;
using Aetherium.Core.Definitions;

namespace Battlefield.Player
{
    /// <summary>
    /// Reload lifecycle phase published by the authoritative weapon controller.
    /// The presentation layer drives start/complete/cancel explicitly so a
    /// IsReloading true→false transition can never be misread as a completion.
    /// </summary>
    public enum WeaponReloadPhase : byte
    {
        None = 0,
        Started = 1,
        Complete = 2,
        Cancel = 3,
        Applied = 4
    }

    /// <summary>
    /// Networked ring-buffer element for confirmed shot events. Plain mutable
    /// struct required by Fusion's NetworkArray weaving; consumed only through
    /// the presentation-side read model. Carries the authoritative ray so
    /// every peer renders the same pure-presentation tracer from the FPV/TPV
    /// muzzle to the exact authoritative end point.
    /// </summary>
    public struct WeaponShotPresentationEventData : INetworkStruct
    {
        public int Sequence;
        public int ShotTick;
        public int ItemId;
        public int ClipAmmoAfterShot;
        public int ConsumedAfterShot;
        public Vector3 ShotOrigin;
        public Vector3 ShotDirection;
        public Vector3 AimPoint;
        public Vector3 ShotEndPoint;
        public int DidHit;
    }

    /// <summary>
    /// Authoritative damage confirmation consumed by the attacker's local
    /// HUD. It is deliberately separate from shot/ammo presentation because
    /// projectile damage may resolve several ticks after the shot.
    /// </summary>
    public struct DamageFeedbackEventData : INetworkStruct
    {
        public int Sequence;
        public int SourceSequence;
        public int WasFatal;
        public int IsHeadshot;
    }

    /// <summary>
    /// Read model for one authoritative, ammo-confirmed shot. ItemId is the
    /// weapon that actually fired — captured before any special-slot
    /// consumption — so presentation never maps a historical shot onto the
    /// current active item.
    /// </summary>
    public readonly struct WeaponShotPresentationEvent
    {
        public readonly int Sequence;
        public readonly int ShotTick;
        // Local read-model flags only; never added to the networked event payload.
        public readonly bool SuppressFireAudio;
        public readonly bool SuppressFireMotion;
        public readonly ItemId ItemId;
        public readonly int ClipAmmoAfterShot;
        public readonly bool ConsumedAfterShot;
        public readonly Vector3 ShotOrigin;
        public readonly Vector3 ShotDirection;
        public readonly Vector3 AimPoint;
        public readonly Vector3 ShotEndPoint;
        public readonly bool DidHit;

        public WeaponShotPresentationEvent(int sequence, ItemId itemId, int clipAmmoAfterShot, bool consumedAfterShot)
        {
            Sequence = sequence;
            ShotTick = 0;
            SuppressFireAudio = false;
            SuppressFireMotion = false;
            ItemId = itemId;
            ClipAmmoAfterShot = clipAmmoAfterShot;
            ConsumedAfterShot = consumedAfterShot;
            ShotOrigin = Vector3.zero;
            ShotDirection = Vector3.forward;
            AimPoint = Vector3.zero;
            ShotEndPoint = Vector3.zero;
            DidHit = false;
        }

        public WeaponShotPresentationEvent(int sequence, ItemId itemId, int clipAmmoAfterShot, bool consumedAfterShot,
            Vector3 origin, Vector3 direction, Vector3 endPoint, bool didHit)
        {
            Sequence = sequence;
            ShotTick = 0;
            SuppressFireAudio = false;
            SuppressFireMotion = false;
            ItemId = itemId;
            ClipAmmoAfterShot = clipAmmoAfterShot;
            ConsumedAfterShot = consumedAfterShot;
            ShotOrigin = origin;
            ShotDirection = direction;
            AimPoint = endPoint;
            ShotEndPoint = endPoint;
            DidHit = didHit;
        }

        public WeaponShotPresentationEvent(int sequence, ItemId itemId, int clipAmmoAfterShot, bool consumedAfterShot,
            Vector3 origin, Vector3 direction, Vector3 aimPoint, Vector3 endPoint, bool didHit,
            int shotTick = 0, bool suppressFireAudio = false, bool suppressFireMotion = false)
        {
            Sequence = sequence;
            ShotTick = shotTick;
            SuppressFireAudio = suppressFireAudio;
            SuppressFireMotion = suppressFireMotion;
            ItemId = itemId;
            ClipAmmoAfterShot = clipAmmoAfterShot;
            ConsumedAfterShot = consumedAfterShot;
            ShotOrigin = origin;
            ShotDirection = direction;
            AimPoint = aimPoint;
            ShotEndPoint = endPoint;
            DidHit = didHit;
        }
    }

    /// <summary>
    /// Read model for one authoritative reload lifecycle transition.
    /// </summary>
    public readonly struct WeaponReloadPresentationState
    {
        public readonly int Sequence;
        public readonly ItemId ItemId;
        public readonly WeaponReloadPhase Phase;
        public readonly int ClipAmmo;
        public readonly float DurationSeconds;

        public WeaponReloadPresentationState(int sequence, ItemId itemId, WeaponReloadPhase phase, int clipAmmo, float durationSeconds)
        {
            Sequence = sequence;
            ItemId = itemId;
            Phase = phase;
            ClipAmmo = clipAmmo;
            DurationSeconds = durationSeconds;
        }
    }
}
