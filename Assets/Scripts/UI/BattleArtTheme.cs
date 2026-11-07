using UnityEngine;

namespace PinballRoguelite
{
    /// <summary>
    /// Holds the battle art reference on GameplayUI. Gameplay systems can fall back to their
    /// existing geometric visuals when this catalog is intentionally absent.
    /// </summary>
    public sealed class BattleArtTheme : MonoBehaviour
    {
        [SerializeField] private BattleArtCatalog catalog;

        public BattleArtCatalog Catalog => catalog;

        public void Configure(BattleArtCatalog value)
        {
            catalog = value;
        }
    }
}
