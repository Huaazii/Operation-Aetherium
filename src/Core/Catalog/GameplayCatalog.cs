using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Aetherium.Core.Models;
using Aetherium.Core.Definitions;

namespace Aetherium.Core.Catalog
{
    /// <summary>
    /// Explicit catalog mapping TeamIdentity, PlayerClassId, ItemId, NetworkContentId, and AbilityId
    /// to their authoritative ScriptableObjects and NetworkObject prefabs.
    /// Replaces legacy string path formatting.
    /// </summary>
    [CreateAssetMenu(fileName = "GameplayCatalog", menuName = "Aetherium/Catalog/GameplayCatalog")]
    public sealed class GameplayCatalog : ScriptableObject
    {
        [Serializable]
        public struct PlayerClassPrefabMapping
        {
            public TeamIdentity team;
            public PlayerClassId classId;
            public NetworkObject prefab;
        }

        [Serializable]
        public struct WeaponMapping
        {
            public ItemId itemId;
            public WeaponDefinition definition;
        }

        [Serializable]
        public struct ContentMapping
        {
            public NetworkContentId contentId;
            public NetworkObject prefab;
        }

        [Serializable]
        public struct ProjectileMapping
        {
            public NetworkContentId contentId;
            public ProjectileDefinition definition;
        }

        [Serializable]
        public struct AbilityMapping
        {
            public AbilityId abilityId;
            public AbilityDefinition definition;
        }

        [SerializeField] private List<PlayerClassPrefabMapping> playerClassPrefabs = new List<PlayerClassPrefabMapping>();
        [SerializeField] private List<WeaponMapping> weaponDefinitions = new List<WeaponMapping>();
        [SerializeField] private List<ContentMapping> contentPrefabs = new List<ContentMapping>();
        [SerializeField] private List<ProjectileMapping> projectileDefinitions = new List<ProjectileMapping>();
        [SerializeField] private List<AbilityMapping> abilityDefinitions = new List<AbilityMapping>();

        [Header("Bullet Impact Effects")]
        [SerializeField] private GameObject metalImpactPrefab;
        [SerializeField] private GameObject woodImpactPrefab;
        [SerializeField] private GameObject stoneImpactPrefab;
        [SerializeField] private GameObject bloodImpactPrefab;

        public GameObject MetalImpactPrefab => metalImpactPrefab;
        public GameObject WoodImpactPrefab => woodImpactPrefab;
        public GameObject StoneImpactPrefab => stoneImpactPrefab;
        public GameObject BloodImpactPrefab => bloodImpactPrefab;

        private readonly Dictionary<(TeamIdentity team, PlayerClassId classId), NetworkObject> playerLookup =
            new Dictionary<(TeamIdentity team, PlayerClassId classId), NetworkObject>();
        private readonly Dictionary<ItemId, WeaponDefinition> weaponLookup = new Dictionary<ItemId, WeaponDefinition>();
        private readonly Dictionary<NetworkContentId, NetworkObject> contentLookup = new Dictionary<NetworkContentId, NetworkObject>();
        private readonly Dictionary<NetworkContentId, ProjectileDefinition> projectileLookup = new Dictionary<NetworkContentId, ProjectileDefinition>();
        private readonly Dictionary<AbilityId, AbilityDefinition> abilityLookup = new Dictionary<AbilityId, AbilityDefinition>();

        private void OnEnable()
        {
            BuildLookup();
        }

        public void BuildLookup()
        {
            playerLookup.Clear();
            foreach (var mapping in playerClassPrefabs)
            {
                if (mapping.prefab != null)
                {
                    playerLookup[(mapping.team, mapping.classId)] = mapping.prefab;
                }
            }

            weaponLookup.Clear();
            foreach (var mapping in weaponDefinitions)
            {
                if (mapping.definition != null)
                {
                    weaponLookup[mapping.itemId] = mapping.definition;
                }
            }

            contentLookup.Clear();
            foreach (var mapping in contentPrefabs)
            {
                if (mapping.prefab != null)
                {
                    contentLookup[mapping.contentId] = mapping.prefab;
                }
            }

            projectileLookup.Clear();
            foreach (var mapping in projectileDefinitions)
            {
                if (mapping.definition != null)
                {
                    projectileLookup[mapping.contentId] = mapping.definition;
                }
            }

            abilityLookup.Clear();
            foreach (var mapping in abilityDefinitions)
            {
                if (mapping.definition != null)
                {
                    abilityLookup[mapping.abilityId] = mapping.definition;
                }
            }
        }

        public bool TryGetPlayerPrefab(TeamIdentity team, PlayerClassId classId, out NetworkObject prefab)
        {
            if (playerLookup.Count == 0 && playerClassPrefabs.Count > 0) BuildLookup();
            return playerLookup.TryGetValue((team, classId), out prefab);
        }

        public bool TryGetWeaponDefinition(ItemId itemId, out WeaponDefinition definition)
        {
            if (weaponLookup.Count == 0 && weaponDefinitions.Count > 0) BuildLookup();
            return weaponLookup.TryGetValue(itemId, out definition);
        }

        public bool TryGetContentPrefab(NetworkContentId contentId, out NetworkObject prefab)
        {
            if (contentLookup.Count == 0 && contentPrefabs.Count > 0) BuildLookup();
            return contentLookup.TryGetValue(contentId, out prefab);
        }

        public bool TryGetProjectileDefinition(NetworkContentId contentId, out ProjectileDefinition definition)
        {
            if (projectileLookup.Count == 0 && projectileDefinitions.Count > 0) BuildLookup();
            return projectileLookup.TryGetValue(contentId, out definition);
        }

        public bool TryGetAbilityDefinition(AbilityId abilityId, out AbilityDefinition definition)
        {
            if (abilityLookup.Count == 0 && abilityDefinitions.Count > 0) BuildLookup();
            return abilityLookup.TryGetValue(abilityId, out definition);
        }

        public void RegisterMapping(TeamIdentity team, PlayerClassId classId, NetworkObject prefab)
        {
            playerClassPrefabs.Add(new PlayerClassPrefabMapping
            {
                team = team,
                classId = classId,
                prefab = prefab
            });
            playerLookup[(team, classId)] = prefab;
        }

        public void RegisterWeapon(ItemId itemId, WeaponDefinition definition)
        {
            weaponDefinitions.Add(new WeaponMapping { itemId = itemId, definition = definition });
            weaponLookup[itemId] = definition;
        }

        public void RegisterContent(NetworkContentId contentId, NetworkObject prefab)
        {
            contentPrefabs.Add(new ContentMapping { contentId = contentId, prefab = prefab });
            contentLookup[contentId] = prefab;
        }

        public void RegisterProjectile(NetworkContentId contentId, ProjectileDefinition definition)
        {
            projectileDefinitions.Add(new ProjectileMapping { contentId = contentId, definition = definition });
            projectileLookup[contentId] = definition;
        }

        public void RegisterAbility(AbilityId abilityId, AbilityDefinition definition)
        {
            abilityDefinitions.Add(new AbilityMapping { abilityId = abilityId, definition = definition });
            abilityLookup[abilityId] = definition;
        }

        /// <summary>
        /// Clears all editor-managed mappings before an idempotent catalog rebuild.
        /// Runtime systems should only consume the resulting asset.
        /// </summary>
        public void ClearMappings()
        {
            playerClassPrefabs.Clear();
            weaponDefinitions.Clear();
            contentPrefabs.Clear();
            projectileDefinitions.Clear();
            abilityDefinitions.Clear();
            BuildLookup();
        }
    }
}
