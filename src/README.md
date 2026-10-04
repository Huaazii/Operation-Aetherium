# Selected project source

[Architecture](../docs/architecture.md) · [Knowledge map](../docs/knowledge-map.md) · [System contracts](../docs/system-contracts.md)

These **32 project-specific C# files** expose Desert Storm's input and combat contracts, typed content configuration, connection ownership and player replacement logic.

| Area | Files | Responsibility |
|---|---|---|
| Input | [AetheriumInputData](Core/Models/AetheriumInputData.cs) | Movement, absolute view, held buttons, accumulated edges and slot selection |
| Combat | [DamageRequest / DamageResult](Core/Models/DamageRequest.cs), [IFusionCombatTarget](Core/Interfaces/IFusionCombatTarget.cs) | Attack context, target semantics and applied results |
| Recoil | [WeaponAimRecoil / PredictedAimRecoil](Core/Models/WeaponAimRecoil.cs) | Deterministic kick, pitch limits and bounded pending-entry reconciliation |
| Presentation | [WeaponPresentationEvent](Battlefield/Player/WeaponPresentationEvent.cs) | Confirmed shot data, local read models and explicit reload phases |
| Connection | [ConnectionRequest / ConnectionProgress](Core/Interfaces/ConnectionRequest.cs) | Monotonic budget, operation identity, cancellation and terminal outcome |
| Server launch | [ServerLaunchOptions](Core/Network/ServerLaunchOptions.cs) | Room / session normalization and public IPv4 validation |
| Base class change | [Match request](Battlefield/Match/BattlefieldMatchState.ClassChange.cs), [replacement bootstrap](Battlefield/Runtime/BattlefieldRuntimeBootstrap.ClassChange.cs), [team zone](Battlefield/Runtime/BattlefieldClassChangeZone.cs) | Server validation, staged state transfer and base geometry |
| Content lookup | [GameplayCatalog](Core/Catalog/GameplayCatalog.cs) | Typed serialized mappings and runtime caches |
| Weapons and abilities | [WeaponDefinition](Core/Definitions/WeaponDefinition.cs), [AbilityDefinition](Core/Definitions/AbilityDefinition.cs), [ProjectileDefinition](Core/Definitions/ProjectileDefinition.cs) | Simulation, equipment and presentation parameters |
| Battlefield | [MinionDefinition](Core/Definitions/MinionDefinition.cs), [MinionWaveDefinition](Core/Definitions/MinionWaveDefinition.cs), [MobaStructureDefinition](Core/Definitions/MobaStructureDefinition.cs) | AI, wave and structure configuration |

## Runtime integration

The project uses Unity **2022.3.62f3** and Photon Fusion 2. UnityEngine and Fusion provide the engine and networking APIs; project components connect the contracts to player, match and presentation lifetimes. Partial classes organize request validation and replacement logic around their owning runtime components.

Serialized fields define configuration schemas. Authored assets supply gameplay values through definitions and catalog mappings.

[Project contribution and credits](../CREDITS.md) · [Copyright](../COPYRIGHT.md)
