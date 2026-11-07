using UnityEngine;

namespace PinballRoguelite
{
    [CreateAssetMenu(fileName = "BattleArtCatalog", menuName = "Pinball Roguelite/UI/Battle Art Catalog")]
    public sealed class BattleArtCatalog : ScriptableObject
    {
        [Header("Backgrounds")]
        [SerializeField] private Sprite boardTexture;
        [SerializeField] private Sprite boardFrame;

        [Header("Characters")]
        [SerializeField] private Sprite playerCharacter;
        [SerializeField] private Sprite playerWeapon;
        [SerializeField] private Sprite enemyCharacter;

        [Header("HUD")]
        [SerializeField] private Sprite playerInfoBack;
        [SerializeField] private Sprite playerAvatar;
        [SerializeField] private Sprite playerHealthBack;
        [SerializeField] private Sprite playerHealthFill;
        [SerializeField] private Sprite enemyInfoBack;
        [SerializeField] private Sprite enemyAvatar;
        [SerializeField] private Sprite enemyHealthBack;
        [SerializeField] private Sprite enemyHealthFill;
        [SerializeField] private Sprite damageBack;
        [SerializeField] private Sprite damageTitle;

        [Header("Board")]
        [SerializeField] private Sprite normalPeg;
        [SerializeField] private Sprite slotDivider;
        [SerializeField] private Sprite starSlot;
        [SerializeField] private Sprite launcherSpring;
        [SerializeField] private Sprite launcherSpringCap;
        [SerializeField] private Sprite launcherLoadedMarble;

        [Header("Controls")]
        [SerializeField] private Sprite normalButton;
        [SerializeField] private Sprite normalButtonIcon;
        [SerializeField] private Sprite splitButton;
        [SerializeField] private Sprite splitButtonIcon;
        [SerializeField] private Sprite launchButton;
        [SerializeField] private Sprite launchButtonIcon;

        public Sprite BoardTexture => boardTexture;
        public Sprite BoardFrame => boardFrame;
        public Sprite PlayerCharacter => playerCharacter;
        public Sprite PlayerWeapon => playerWeapon;
        public Sprite EnemyCharacter => enemyCharacter;
        public Sprite PlayerInfoBack => playerInfoBack;
        public Sprite PlayerAvatar => playerAvatar;
        public Sprite PlayerHealthBack => playerHealthBack;
        public Sprite PlayerHealthFill => playerHealthFill;
        public Sprite EnemyInfoBack => enemyInfoBack;
        public Sprite EnemyAvatar => enemyAvatar;
        public Sprite EnemyHealthBack => enemyHealthBack;
        public Sprite EnemyHealthFill => enemyHealthFill;
        public Sprite DamageBack => damageBack;
        public Sprite DamageTitle => damageTitle;
        public Sprite NormalPeg => normalPeg;
        public Sprite SlotDivider => slotDivider;
        public Sprite StarSlot => starSlot;
        public Sprite LauncherSpring => launcherSpring;
        public Sprite LauncherSpringCap => launcherSpringCap;
        public Sprite LauncherLoadedMarble => launcherLoadedMarble;
        public Sprite NormalButton => normalButton;
        public Sprite NormalButtonIcon => normalButtonIcon;
        public Sprite SplitButton => splitButton;
        public Sprite SplitButtonIcon => splitButtonIcon;
        public Sprite LaunchButton => launchButton;
        public Sprite LaunchButtonIcon => launchButtonIcon;
    }
}
