namespace Aetherium.Core.Definitions
{
    /// <summary>
    /// Global centralized Canvas sorting order layers.
    /// Eliminates magic numbers across HUDs, modals, settlement, and loading overlays.
    /// </summary>
    public static class UiSortingOrder
    {
        public const int Gameplay = 10;
        public const int Settlement = 100;
        public const int SystemModal = 200;
        public const int Loading = 32767;
    }
}
