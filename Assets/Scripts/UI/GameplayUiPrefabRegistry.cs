using UnityEngine;

namespace PinballRoguelite
{
    public sealed class GameplayUiPrefabRegistry : MonoBehaviour
    {
        [SerializeField] private GameplayUiView _gameplayUiPrefab;

        public GameplayUiView GameplayUiPrefab => _gameplayUiPrefab;
    }
}
