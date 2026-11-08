using UnityEngine;

namespace PinballRoguelite
{
    /// <summary>
    /// Legacy compatibility component retained for existing GameplayUI prefab instances.
    /// Static UI layout now belongs exclusively to the prefab's RectTransforms.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleScreenLayout : MonoBehaviour
    {
    }
}
