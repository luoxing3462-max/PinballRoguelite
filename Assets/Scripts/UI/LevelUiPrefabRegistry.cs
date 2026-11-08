using UnityEngine;

namespace PinballRoguelite
{
    public sealed class LevelUiPrefabRegistry : MonoBehaviour
    {
        [SerializeField] private LevelUiView _levelUiPrefab;
        [SerializeField] private LevelMapUIController _levelMapUiPrefab;

        public LevelUiView LevelUiPrefab => _levelUiPrefab;
        public LevelMapUIController LevelMapUiPrefab => _levelMapUiPrefab;
    }
}
