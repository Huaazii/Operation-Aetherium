# Content architecture: explicit IDs and configurable behaviour

[Architecture](../architecture.md) · [Source](../../src/README.md)

## Requirement

Classes share spawning, health, equipment and presentation infrastructure. Weapons share simulation patterns while differing in damage, handling, reload timing and assets. The project gives identity, parameters, resources and runtime behaviour explicit integration points.

## Implementation

[TeamIdentity](../../src/Core/Models/TeamIdentity.cs), [PlayerClassId](../../src/Core/Models/PlayerClassId.cs) and the match seat model represent separate concepts. [GameplayCatalog](../../src/Core/Catalog/GameplayCatalog.cs) maps typed IDs to definitions and prefabs:

| Key | Value |
|---|---|
| Team + class | Network player prefab |
| Item ID | Weapon definition |
| Network content ID | Network prefab or projectile definition |
| Ability ID | Ability definition |

Serialized lists make authoring visible in the Inspector; dictionaries provide runtime lookup. BuildLookup rebuilds caches from those mappings. Editor-managed registration methods support authored content integration.

[WeaponDefinition](../../src/Core/Definitions/WeaponDefinition.cs) groups simulation kind, damage, accuracy, ammo, reload, projectile, recoil and presentation timing. Timing helpers expose equip firing readiness and reload policy to authority and presentation consumers. [AbilityDefinition](../../src/Core/Definitions/AbilityDefinition.cs) and [ProjectileDefinition](../../src/Core/Definitions/ProjectileDefinition.cs) configure the corresponding behaviours.

## Class composition

| Layer | Current integration |
|---|---|
| Identity | Heavy, Medic and Rocket have team-independent class IDs |
| Resources | Catalog maps both Alpha and Omega class variants to network player prefabs |
| Shared player state | Movement, inventory, health and combat statistics use common components |
| Ability behaviour | Class selection resolves ability configuration and the corresponding equipment / state flow |
| Local presentation | Prefab bindings and selection resources connect the class to FPV, TPV and UI |
| Runtime replacement | Base class changes resolve target prefabs and transfer shared state |

Identity and seat capacity are independent. The current playable-class checks and replacement preload loop explicitly cover the three implemented classes. Data mappings and behaviour integration have separate responsibilities.

## Engineering benefit

A weapon parameter, prefab reference and damage rule have identifiable places to change. Shared state contracts allow the roster to reuse player infrastructure, while authored mappings make resource relationships inspectable. Equip timing and recoil configuration are consumed through the same definition boundary.
