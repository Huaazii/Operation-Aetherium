# Weapon presentation: phases, mounts and confirmed actions

[Combat](networked-combat.md) · [Architecture](../architecture.md)

## Requirement

FPV, TPV, audio and weapon simulation advance on different schedules. Equipping, firing, reloading, consuming a special item and replacing a player can overlap. Each feedback action needs to remain associated with its weapon and mounting lifetime.

## Implementation

FusionWeaponController owns gameplay gates. FusionWeaponPresentation consumes confirmed events and local prediction. FusionLocalFpvDriver and FusionFpvKinemationBridge translate those inputs into the integrated first-person framework.

| Boundary | Implementation | Effect |
|---|---|---|
| Equipment resource vs firing readiness | FPV exposes separate readiness; WeaponDefinition supplies equip duration and firing threshold | A loaded rig still follows its equip timing |
| Shot identity | [Shot event](../../src/Battlefield/Player/WeaponPresentationEvent.cs) retains item, sequence, tick and ammo | Historical fire belongs to the weapon that fired |
| Reload outcome | Started / Applied / Complete / Cancel phases | Presentation receives the actual phase, including interruption |
| Predicted feedback ownership | Local predicted sequences reconcile with confirmed events | Same action has one local fire response |
| Mount lifetime | Pending action state clears on weapon / ownership / player lifecycle changes | Waiting work has an explicit owner |

Authority retains the longer of outgoing fire recovery and incoming equip lock. Automatic held fire can resume when the gate clears; an early semi-automatic click is not queued as a later shot.

When a legal confirmed shot arrives during FPV equip, ammo and unsuppressed sound are handled immediately. If the mount is not yet fire-ready, it retains the latest applicable confirmed motion and plays that motion when ready, without replaying sound. Already predicted motions are not cached again.

RPG uses its configured equip and loaded pose. A confirmed FireOut action retains the post-shot hold before the consumed special slot switches back. That hold starts from actual motion playback when equip delayed it.

Serum has preparation, release and follow-through ownership. Switching is locked in that interval; launch state is captured at release. Pending throw routines and waiting actions clear on death, authority correction or teardown.

## Lifecycle policy

Switching / empty equipment, death and respawn, component disable, input-authority loss and session exit clear pending mount work. Confirmed shot playback uses its own item ID rather than whichever weapon happens to be active at that frame. Remote TPV and audio consume confirmation timing; local hitscan reconciles immediate feedback with that confirmation.

## 工程要点

动作阶段、可开火门控、事件物品身份和挂载生命周期共同连接仿真与动画。动画资源负责表现，权威计时负责规则。明确 Started、Applied、Complete、Cancel 能表达换弹中的真实变化，挂载归属则限制等待动作的有效范围。
