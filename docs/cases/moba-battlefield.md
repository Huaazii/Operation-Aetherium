# MOBA battlefield: connecting combat to objective progression

## Requirement

Players, minions and structures share targeting and damage concepts, while navigation, defensive fire and objective unlocks each need their own rules. Local combat needs to produce visible progress toward the enemy core.

## Implementation

[IFusionCombatTarget](../../src/Core/Interfaces/IFusionCombatTarget.cs) exposes team, life state, entity type, authority-space target positions and a damage entry point. The contract allows weapons and AI to interact with different entities while each target applies its own health and protection rules.

`FusionMinion` selects targets on state authority and uses a NavMeshAgent for lane movement and pursuit. It replicates position, rotation and speed for remote presentation. Target selection prioritizes minions and structures, with player retaliation handled by its combat conditions. Limited-frequency scans and reusable physics buffers bound candidate-query work.

`FusionStructure` combines objective prerequisites with combat state and retaliation. Objective identity and prerequisite relationships determine progression. Defensive attack behaviour and protection state are expressed separately, allowing a protected structure to continue defending. The core is the final objective and has its own behaviour.

| Data / component | Role |
|---|---|
| [MinionDefinition](../../src/Core/Definitions/MinionDefinition.cs) | Unit statistics and movement / combat settings |
| [MinionWaveDefinition](../../src/Core/Definitions/MinionWaveDefinition.cs) | Wave timing and count configuration |
| [MobaStructureDefinition](../../src/Core/Definitions/MobaStructureDefinition.cs) | Structure durability, targeting, attack and protection parameters |
| [ObjectiveId](../../src/Core/Definitions/ObjectiveId.cs) | Stable identity for the progression graph |
| FusionMinion / FusionStructure | Authority-side decisions, timers and state replication |

## Engineering benefit

The common target contract keeps interactions consistent. Separate navigation and target decisions allow pathing and combat priorities to be inspected independently. Objective prerequisites and defensive state compose the rules that turn a won firefight into lane and base pressure.
