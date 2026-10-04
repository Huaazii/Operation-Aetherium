using System.Collections;
using System.Collections.Generic;
using Aetherium.Core.Models;
using Battlefield.Match;
using Battlefield.Player;
using Fusion;
using UnityEngine;

namespace Battlefield.Runtime
{
    public sealed partial class BattlefieldRuntimeBootstrap
    {
        private readonly Dictionary<int, NetworkObject> classChangePrefabs = new Dictionary<int, NetworkObject>();
        private Coroutine classChangePreload;

        private void EnsureClassChangePrefabsLoaded(NetworkRunner runner)
        {
            if (!initialized || runner == null || !runner.IsRunning || !runner.IsServer ||
                sceneRoot?.GameplayCatalog == null || classChangePreload != null || classChangePrefabs.Count == 6) return;
            classChangePreload = StartCoroutine(PreloadClassChangePrefabs(runner));
        }

        private IEnumerator PreloadClassChangePrefabs(NetworkRunner runner)
        {
            // Always yield once: StartCoroutine can otherwise finish before its handle is assigned.
            yield return null;
            for (int team = 0; team < 2; team++)
            for (int classId = 1; classId <= 3; classId++)
            {
                if (!initialized || runner == null || !runner.IsRunning) { classChangePreload = null; yield break; }
                if (!sceneRoot.GameplayCatalog.TryGetPlayerPrefab((TeamIdentity)team, (PlayerClassId)classId,
                        out NetworkObject prefab) || prefab == null) continue;
                NetworkObjectPrefabData prefabData = prefab.GetComponent<NetworkObjectPrefabData>();
                if (prefabData == null || !prefabData.Guid.IsValid) continue;
                NetworkPrefabId id = runner.Prefabs.GetId(prefabData.Guid);
                NetworkObject loaded = null;
                for (int frame = 0; frame < 300 && loaded == null; frame++)
                {
                    if (!initialized || runner == null || !runner.IsRunning) { classChangePreload = null; yield break; }
                    loaded = runner.Prefabs.Load(id, isSynchronous: false);
                    if (loaded == null) yield return null;
                }
                if (loaded != null) classChangePrefabs[team * 10 + classId] = loaded;
            }
            classChangePreload = null;
        }

        public ClassChangeResult TryReplacePlayerClass(BattlefieldPlayer source, PlayerClassId target,
            int version, TickTimer cooldown, out BattlefieldPlayer replacement)
        {
            replacement = null;
            NetworkRunner runner = source != null ? source.Runner : null;
            if (!initialized || runner == null || !runner.IsRunning || !runner.IsServer || sceneRoot == null)
                return ClassChangeResult.ResourcesNotReady;
            EnsureClassChangePrefabsLoaded(runner);
            if (!classChangePrefabs.TryGetValue(source.TeamId * 10 + (int)target, out NetworkObject prefab))
                return ClassChangeResult.ResourcesNotReady;
            if (prefab.GetComponent<BattlefieldPlayer>() == null || prefab.GetComponent<FusionHealth>() == null ||
                prefab.GetComponent<FusionInventory>() == null || prefab.GetComponent<FusionAbilityController>() == null ||
                prefab.GetComponent<FusionWeaponController>() == null || prefab.GetComponent<FusionPlayerMotor>() == null)
                return ClassChangeResult.Failed;

            NetworkObject spawned = null;
            try
            {
                spawned = runner.Spawn(prefab, source.transform.position, source.transform.rotation,
                    source.Object.InputAuthority, onBeforeSpawned: (r, o) =>
                    {
                        BattlefieldPlayer player = o.GetComponent<BattlefieldPlayer>();
                        player.InitializePlayer(source.PlayerRefId, source.TeamId, (int)target, source.ConnectionTokenId);
                        player.TeamSlot = source.TeamSlot;
                        player.ClassChangeVersion = version;
                        player.IsClassChangeStaged = true;
                    });
                if (spawned == null) return ClassChangeResult.Failed;
                replacement = spawned.GetComponent<BattlefieldPlayer>();
                replacement.GetComponent<FusionInventory>().RestoreForClassChange(source.GetComponent<FusionInventory>());
                replacement.GetComponent<FusionWeaponController>().RestoreForClassChange(source.GetComponent<FusionWeaponController>());
                replacement.HealthModule.RestoreForClassChange(source.HealthModule);
                replacement.Motor.RestoreForClassChange(source.Motor);
                replacement.GetComponent<FusionAbilityController>().RestoreForClassChange(cooldown);
                FusionCombatStats oldStats = source.CombatStats;
                FusionCombatStats newStats = replacement.CombatStats;
                newStats.Kills = oldStats.Kills;
                newStats.Deaths = oldStats.Deaths;
                newStats.Assists = oldStats.Assists;
                newStats.MinionKills = oldStats.MinionKills;
                newStats.Score = oldStats.Score;
                newStats.LastKillerToken = oldStats.LastKillerToken;
                return ClassChangeResult.Success;
            }
            catch (System.Exception exception)
            {
                if (spawned != null && spawned.IsValid) runner.Despawn(spawned);
                replacement = null;
                Debug.LogError($"[ClassChange] Replacement failed; original player retained: {exception.Message}", this);
                return ClassChangeResult.Failed;
            }
        }
    }
}
