using UnityEngine;
using Fusion;

namespace Aetherium.Core.Models
{
    public enum AetheriumInputButtons
    {
        Jump = 0,
        Sprint = 1,
        Crouch = 2,
        Fire = 3,
        Aim = 4,
        Reload = 5,
        Interact = 6,
        DropItem = 7,
        Ability = 8,
        Ability1 = 8, // Alias
        ThrowItem = 7 // Alias
    }

    public struct AetheriumInputData : INetworkInput
    {
        public Vector2 Movement;

        /// <summary>
        /// Absolute local view yaw/pitch (degrees, unscaled). Produced exactly
        /// once per rendered frame by FusionInputCollector and consumed by the
        /// authoritative motor (root yaw, WASD direction, ViewPitch) and by
        /// the local FPV driver with the same sensitivity formula, so the
        /// screen center, movement heading, interaction ray and authoritative
        /// gun line always share one view coordinate.
        /// </summary>
        public float ViewYawDegrees;
        public float ViewPitchDegrees;

        /// <summary>
        /// Canonical edge-aware input representation. All button state
        /// must go through NetworkButtons — no duplicate boolean fields.
        /// </summary>
        public NetworkButtons Buttons;

        /// <summary>
        /// Latched 0 -> 1 press edges accumulated within the rendering frame window
        /// before submission to this simulation tick.
        /// </summary>
        public NetworkButtons PressedEdges;

        /// <summary>
        /// Latched 1 -> 0 release edges accumulated within the rendering frame window
        /// before submission to this simulation tick.
        /// </summary>
        public NetworkButtons ReleasedEdges;

        /// <summary>
        /// Weapon slot selection. -1 = no change, 0-2 = slot index (Primary=0, Secondary=1, Special=2).
        /// </summary>
        public int SelectWeaponSlot;

        public bool IsPressed(AetheriumInputButtons button)
        {
            return Buttons.IsSet((int)button);
        }

        public bool WasPressed(AetheriumInputButtons button, AetheriumInputData previous)
        {
            return PressedEdges.IsSet((int)button) || (Buttons.IsSet((int)button) && !previous.Buttons.IsSet((int)button));
        }

        public bool WasReleased(AetheriumInputButtons button, AetheriumInputData previous)
        {
            return ReleasedEdges.IsSet((int)button) || (!Buttons.IsSet((int)button) && previous.Buttons.IsSet((int)button));
        }
    }
}
