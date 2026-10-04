using UnityEngine;

namespace Aetherium.Core.Definitions
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Aetherium/Definitions/ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public ItemId itemId;
        public string displayName;
        public WeaponSlot defaultSlot;
        public Sprite icon;
    }
}
