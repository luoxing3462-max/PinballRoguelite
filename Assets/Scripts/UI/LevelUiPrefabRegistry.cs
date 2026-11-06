using UnityEngine;

namespace PinballRoguelite
{
    public sealed class LevelUiPrefabRegistry : MonoBehaviour
    {
        [SerializeField] private LevelUiView _levelUiPrefab;

        public LevelUiView LevelUiPrefab => _levelUiPrefab;
    }
}
