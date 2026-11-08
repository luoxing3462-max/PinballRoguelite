using UnityEngine;

namespace PinballRoguelite
{
    public sealed class MarbleSelectionUiPrefabRegistry : MonoBehaviour
    {
        [SerializeField] private MarbleSelectionUIController _marbleSelectionUiPrefab;

        public MarbleSelectionUIController MarbleSelectionUiPrefab => _marbleSelectionUiPrefab;
    }
}
